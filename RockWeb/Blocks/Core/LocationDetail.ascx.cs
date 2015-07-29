// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Location Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of the given location." )]

    [CodeEditorField( "Map HTML", "The HTML to use for displaying group location maps. Liquid syntax is used to render data from the following data structure: points[type, latitude, longitude], polygons[type, polygon_wkt, google_encoded_polygon]", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 300, false, @"
    {% if point or polygon %}
        <div class='group-location-map'>
            <img src='//maps.googleapis.com/maps/api/staticmap?sensor=false&size=350x200&format=png&style=feature:all|saturation:0|hue:0xe7ecf0&style=feature:road|saturation:-70&style=feature:transit|visibility:off&style=feature:poi|visibility:off&style=feature:water|visibility:simplified|saturation:-60{% if point %}&markers=color:0x779cb1|{{ point.latitude }},{{ point.longitude }}{% endif %}{% if polygon %}&path=fillcolor:0x779cb155|color:0xFFFFFF00|enc:{{ polygon.google_encoded_polygon }}{% endif %}&visual_refresh=true'/>
        </div>
    {% endif %}
" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.MAP_STYLES, "Map Style", "The map theme that should be used for styling the GeoPicker map.", true, false, Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK )]

    public partial class LocationDetail : RockBlock, IDetailBlock
    {
        private int? LocationTypeValueId
        {
            get { return ViewState["LocationTypeValueId"] as int?; }
            set { ViewState["LocationTypeValueId"] = value; }
        }

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Location.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Location ) ).Id;

            ddlPrinter.Items.Clear();
            ddlPrinter.DataSource = new DeviceService( new RockContext() )
                .GetByDeviceTypeGuid( new Guid( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_PRINTER ) )
                .ToList();
            ddlPrinter.DataBind();
            ddlPrinter.Items.Insert( 0, new ListItem( None.Text, None.IdValue ) );

            RockPage.AddCSSLink( ResolveRockUrl( "~/Styles/fluidbox.css" ) );
            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/imagesloaded.min.js" ) );
            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/jquery.fluidbox.min.js" ) );
            ScriptManager.RegisterStartupScript( lImage, lImage.GetType(), "image-fluidbox", "$('.photo a').fluidbox();", true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string locationId = PageParameter( "LocationId" );
                if ( !string.IsNullOrWhiteSpace( locationId ) )
                {
                    ShowDetail( locationId.AsInteger(), PageParameter( "ParentLocationId" ).AsIntegerOrNull() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
            else
            {
                // Rebuild the attribute controls on postback based on group type
                if ( pnlDetails.Visible )
                {
                    var location = new Location { LocationTypeValueId = LocationTypeValueId ?? 0 };
                    BuildAttributeEdits( location, false );
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            LocationService locationService = new LocationService( new RockContext() );
            Location location = locationService.Get( int.Parse( hfLocationId.Value ) );
            ShowEditDetails( location );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            int? parentLocationId = null;

            var rockContext = new RockContext();
            LocationService locationService = new LocationService( rockContext );
            Location location = locationService.Get( hfLocationId.Value.AsInteger() );

            if ( location != null )
            {
                parentLocationId = location.ParentLocationId;
                string errorMessage;
                if ( !locationService.CanDelete( location, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                FlushCampus( location.Id );

                locationService.Delete( location );
                rockContext.SaveChanges();
            }

            // reload page, selecting the deleted location's parent
            var qryParams = new Dictionary<string, string>();
            if ( parentLocationId != null )
            {
                qryParams["LocationId"] = parentLocationId.ToString();
            }

            qryParams["ExpandedIds"] = PageParameter( "ExpandedIds" );

            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            Location location;

            var rockContext = new RockContext();
            LocationService locationService = new LocationService( rockContext );
            AttributeService attributeService = new AttributeService( rockContext );
            AttributeQualifierService attributeQualifierService = new AttributeQualifierService( rockContext );

            int locationId = int.Parse( hfLocationId.Value );

            if ( locationId == 0 )
            {
                location = new Location();
                location.Name = string.Empty;
            }
            else
            {
                location = locationService.Get( locationId );
                FlushCampus( locationId );
            }

            int? orphanedImageId = null;
            if ( location.ImageId != imgImage.BinaryFileId )
            {
                orphanedImageId = location.ImageId;
                location.ImageId = imgImage.BinaryFileId;
            }

            location.Name = tbName.Text;
            location.IsActive = cbIsActive.Checked;
            location.LocationTypeValueId = ddlLocationType.SelectedValueAsId();
            if ( gpParentLocation != null && gpParentLocation.Location != null )
            {
                location.ParentLocationId = gpParentLocation.Location.Id;
            }
            else
            {
                location.ParentLocationId = null;
            }

            location.PrinterDeviceId = ddlPrinter.SelectedValueAsInt();

            acAddress.GetValues(location);

            location.GeoPoint = geopPoint.SelectedValue;
            if ( geopPoint.SelectedValue != null )
            {
                location.IsGeoPointLocked = true;
            }
            location.GeoFence = geopFence.SelectedValue;

            location.IsGeoPointLocked = cbGeoPointLocked.Checked;

            location.LoadAttributes( rockContext );
            Rock.Attribute.Helper.GetEditValues( phAttributeEdits, location );

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !location.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.WrapTransaction( () =>
            {
                if ( location.Id.Equals( 0 ) )
                {
                    locationService.Add( location );
                }
                rockContext.SaveChanges();

                if (orphanedImageId.HasValue)
                {
                    BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                    var binaryFile = binaryFileService.Get( orphanedImageId.Value );
                    if ( binaryFile != null )
                    {
                        // marked the old images as IsTemporary so they will get cleaned up later
                        binaryFile.IsTemporary = true;
                        rockContext.SaveChanges();
                    }
                }

                location.SaveAttributeValues( rockContext );

            } );



            var qryParams = new Dictionary<string, string>();
            qryParams["LocationId"] = location.Id.ToString();
            qryParams["ExpandedIds"] = PageParameter( "ExpandedIds" );

            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfLocationId.Value.Equals( "0" ) )
            {
                int? parentLocationId = PageParameter( "ParentLocationId" ).AsIntegerOrNull();
                if ( parentLocationId.HasValue )
                {
                    // Cancelling on Add, and we know the parentLocationId, so we are probably in treeview mode, so navigate to the current page
                    var qryParams = new Dictionary<string, string>();
                    qryParams["LocationId"] = parentLocationId.ToString();
                    qryParams["ExpandedIds"] = PageParameter( "ExpandedIds" );
                    NavigateToPage( RockPage.Guid, qryParams );
                }
                else
                {
                    // Cancelling on Add.  Return to Grid
                    NavigateToParentPage();
                }
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                LocationService locationService = new LocationService( new RockContext() );
                Location location = locationService.Get( int.Parse( hfLocationId.Value ) );
                ShowReadonlyDetails( location );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnStandardize control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnStandardize_Click( object sender, EventArgs e )
        {
            int locationId = hfLocationId.Value.AsInteger();

            var rockContext = new RockContext();
            var service = new LocationService( rockContext );
            var location = service.Get( locationId );
            if (location == null)
            {
                // if they are adding a new named location, there won't be a location record yet, so just make a new one for the verification
                location = new Location();
            }

            acAddress.GetValues( location );

            service.Verify( location, true );

            acAddress.SetValues( location );
            geopPoint.SetValue( location.GeoPoint );

            lStandardizationUpdate.Text = String.Format( "<div class='alert alert-info'>Standardization Result: {0}<br/>Geocoding Result: {1}</div>",
                location.StandardizeAttemptedResult.IfEmpty( "No Result" ),
                location.GeocodeAttemptedResult.IfEmpty( "No Result" ) );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlLocationType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlLocationType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var location = new LocationService( new RockContext() ).Get( hfLocationId.Value.AsInteger() );
            if ( location == null )
            {
                location = new Location();
            }
            location.LocationTypeValueId = ddlLocationType.SelectedValueAsId();

            location.LoadAttributes();
            BuildAttributeEdits( location, true );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        public void ShowDetail( int locationId )
        {
            ShowDetail( locationId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="parentLocationId">The parent location identifier.</param>
        public void ShowDetail( int locationId, int? parentLocationId )
        {
            pnlDetails.Visible = false;

            bool editAllowed = true;

            Location location = null;

            if ( !locationId.Equals( 0 ) )
            {
                location = new LocationService( new RockContext() ).Get( locationId );
            }

            if ( location == null )
            {
                location = new Location { Id = 0, IsActive = true, ParentLocationId = parentLocationId };
            }

            editAllowed = location.IsAuthorized( Authorization.EDIT, CurrentPerson );

            pnlDetails.Visible = true;
            hfLocationId.Value = location.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed || !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Location.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                ShowReadonlyDetails( location );
            }
            else
            {
                btnEdit.Visible = true;
                btnDelete.Visible = true;
                if ( location.Id > 0 )
                {
                    ShowReadonlyDetails( location );
                }
                else
                {
                    ShowEditDetails( location );
                }
            }

        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="location">The location.</param>
        private void ShowEditDetails( Location location )
        {
            if ( location.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( Location.FriendlyTypeName ).FormatAsHtmlTitle();
                hlInactive.Visible = false;
            }
            else
            {
                if ( string.IsNullOrWhiteSpace( location.Name ) )
                {
                    lReadOnlyTitle.Text = location.ToString().FormatAsHtmlTitle();
                }
                else
                {
                    lReadOnlyTitle.Text = location.Name.FormatAsHtmlTitle();
                }
            }

            SetEditMode( true );

            imgImage.BinaryFileId = location.ImageId;
            imgImage.NoPictureUrl = System.Web.VirtualPathUtility.ToAbsolute( "~/Assets/Images/no-picture.svg?" );

            tbName.Text = location.Name;
            cbIsActive.Checked = location.IsActive;
            acAddress.SetValues( location );
            ddlPrinter.SetValue( location.PrinterDeviceId );
            geopPoint.SetValue( location.GeoPoint );
            geopFence.SetValue( location.GeoFence );

            cbGeoPointLocked.Checked = location.IsGeoPointLocked ?? false;

            Guid mapStyleValueGuid = GetAttributeValue( "MapStyle" ).AsGuid();
            geopPoint.MapStyleValueGuid = mapStyleValueGuid;
            geopFence.MapStyleValueGuid = mapStyleValueGuid;

            var rockContext = new RockContext();
            var locationService = new LocationService( rockContext );
            var attributeService = new AttributeService( rockContext );

            ddlLocationType.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.LOCATION_TYPE.AsGuid() ), true );

            gpParentLocation.Location = location.ParentLocation ?? locationService.Get( location.ParentLocationId ?? 0 );

            // LocationType depends on Selected ParentLocation
            if ( location.Id == 0 && ddlLocationType.Items.Count > 1 )
            {
                // if this is a new location 
                ddlLocationType.SelectedIndex = 0;
            }
            else
            {
                ddlLocationType.SetValue( location.LocationTypeValueId );
            }

            location.LoadAttributes( rockContext );
            BuildAttributeEdits( location, true );
        }

        private void BuildAttributeEdits( Location location, bool setValues )
        {
            Rock.Attribute.Helper.AddEditControls( location, phAttributeEdits, setValues, BlockValidationGroup );
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="location">The location.</param>
        private void ShowReadonlyDetails( Location location )
        {
            SetEditMode( false );

            hfLocationId.SetValue( location.Id );

            if ( string.IsNullOrWhiteSpace( location.Name ) )
            {
                lReadOnlyTitle.Text = location.ToString().FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = location.Name.FormatAsHtmlTitle();
            }

            hlInactive.Visible = !location.IsActive;
            if ( location.LocationTypeValue != null )
            {
                hlType.Text = location.LocationTypeValue.Value;
                hlType.Visible = true;
            }
            else
            {
                hlType.Visible = false;
            }

            string imgTag = GetImageTag( location.ImageId, 150, 150 );
            if ( location.ImageId.HasValue )
            {
                string imageUrl = ResolveRockUrl( String.Format( "~/GetImage.ashx?id={0}", location.ImageId.Value ) );
                lImage.Text = string.Format( "<a href='{0}'>{1}</a>", imageUrl, imgTag );
            }
            else
            {
                lImage.Text = imgTag;
            }

            DescriptionList descriptionList = new DescriptionList();

            if ( location.ParentLocation != null )
            {
                descriptionList.Add( "Parent Location", location.ParentLocation.Name );
            }

            if ( location.LocationTypeValue != null )
            {
                descriptionList.Add( "Location Type", location.LocationTypeValue.Value );
            }

            if ( location.PrinterDevice != null )
            {
                descriptionList.Add( "Printer", location.PrinterDevice.Name );
            }

            string fullAddress = location.GetFullStreetAddress().ConvertCrLfToHtmlBr();
            if ( !string.IsNullOrWhiteSpace( fullAddress ) )
            {
                descriptionList.Add( "Address", fullAddress );
            }

            lblMainDetails.Text = descriptionList.Html;

            location.LoadAttributes();
            Rock.Attribute.Helper.AddDisplayControls( location, phAttributes );

            phMaps.Controls.Clear();
            var mapStyleValue = DefinedValueCache.Read( GetAttributeValue( "MapStyle" ) );
            if ( mapStyleValue == null )
            {
                mapStyleValue = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK );
            }

            if ( mapStyleValue != null )
            {
                string mapStyle = mapStyleValue.GetAttributeValue( "StaticMapStyle" );

                if ( !string.IsNullOrWhiteSpace( mapStyle ) )
                {
                    if ( location.GeoPoint != null )
                    {
                        string markerPoints = string.Format( "{0},{1}", location.GeoPoint.Latitude, location.GeoPoint.Longitude );
                        string mapLink = System.Text.RegularExpressions.Regex.Replace( mapStyle, @"\{\s*MarkerPoints\s*\}", markerPoints );
                        mapLink = System.Text.RegularExpressions.Regex.Replace( mapLink, @"\{\s*PolygonPoints\s*\}", string.Empty );
                        mapLink += "&sensor=false&size=350x200&zoom=13&format=png";
                        phMaps.Controls.Add( new LiteralControl ( string.Format( "<div class='group-location-map'><img src='{0}'/></div>", mapLink ) ) );
                    }

                    if ( location.GeoFence != null )
                    {
                        string polygonPoints = "enc:" + location.EncodeGooglePolygon();
                        string mapLink = System.Text.RegularExpressions.Regex.Replace( mapStyle, @"\{\s*MarkerPoints\s*\}", string.Empty );
                        mapLink = System.Text.RegularExpressions.Regex.Replace( mapLink, @"\{\s*PolygonPoints\s*\}", polygonPoints );
                        mapLink += "&sensor=false&size=350x200&format=png";
                        phMaps.Controls.Add( new LiteralControl( string.Format( "<div class='group-location-map'><img src='{0}'/></div>", mapLink ) ) );
                    }
                }
            }

            btnSecurity.Visible = location.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            btnSecurity.Title = location.Name;
            btnSecurity.EntityId = location.Id;

        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        // Flush any cached campus that uses location
        private void FlushCampus( int locationId )
        {
            foreach ( var campus in CampusCache.All()
                .Where( c => c.LocationId == locationId ) )
            {
                CampusCache.Flush( campus.Id );
            }
        }

        #endregion
    }
}