﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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

    [CodeEditorField( "Map HTML", "The HTML to use for displaying group location maps. Lava syntax is used to render data from the following data structure: points[type, latitude, longitude], polygons[type, polygon_wkt, google_encoded_polygon]", CodeEditorMode.Lava, CodeEditorTheme.Rock, 300, false, @"
    {% if point or polygon %}
        <div class='group-location-map'>
            <img class='img-thumbnail' src='//maps.googleapis.com/maps/api/staticmap?sensor=false&size=350x200&format=png&style=feature:all|saturation:0|hue:0xe7ecf0&style=feature:road|saturation:-70&style=feature:transit|visibility:off&style=feature:poi|visibility:off&style=feature:water|visibility:simplified|saturation:-60{% if point %}&markers=color:0x779cb1|{{ point.latitude }},{{ point.longitude }}{% endif %}{% if polygon %}&path=fillcolor:0x779cb155|color:0xFFFFFF00|enc:{{ polygon.google_encoded_polygon }}{% endif %}&visual_refresh=true'/>
        </div>
    {% endif %}
" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.MAP_STYLES, "Map Style", "The map theme that should be used for styling the GeoPicker map.", true, false, Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK )]

    public partial class LocationDetail : RockBlock, IDetailBlock
    {
        private int? _personId = null;

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
            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Location ) ).Id;

            ddlPrinter.Items.Clear();
            ddlPrinter.DataSource = new DeviceService( new RockContext() )
                .GetByDeviceTypeGuid( new Guid( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_PRINTER ) )
                .OrderBy( d => d.Name )
                .ToList();
            ddlPrinter.DataBind();
            ddlPrinter.Items.Insert( 0, new ListItem( None.Text, None.IdValue ) );

            RockPage.AddCSSLink( "~/Styles/fluidbox.css" );
            RockPage.AddScriptLink( "~/Scripts/imagesloaded.min.js" );
            RockPage.AddScriptLink( "~/Scripts/jquery.fluidbox.min.js" );
            ScriptManager.RegisterStartupScript( lImage, lImage.GetType(), "image-fluidbox", "$('.photo a').fluidbox();", true );

            BuildAttributeEdits( GetLocation(), false );
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var location = GetLocation();

            //Changing the LocationTypeValue will change which attributes appear.
            //But we just loaded whatever was in the database which may not match what was selected.
            if ( LocationTypeValueId != location.LocationTypeValueId )
            {
                location.LocationTypeValueId = LocationTypeValueId;
                location.LoadAttributes();
                BuildAttributeEdits( location, false );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            _personId = PageParameter( "PersonId" ).AsIntegerOrNull();

            if ( !Page.IsPostBack )
            {
                Location location = GetLocation();
                ShowDetail( location.Id );
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
            ShowEditDetails();
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            LocationService locationService = new LocationService( rockContext );
            Location location = GetLocation( locationService );

            int? parentLocationId = null;

            if ( location != null )
            {
                parentLocationId = location.ParentLocationId;
                string errorMessage;
                if ( ! locationService.CanDelete( location, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                int locationId = location.Id;

                locationService.Delete( location );
                rockContext.SaveChanges();

                Rock.CheckIn.KioskDevice.Clear();
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
            RockContext rockContext = new RockContext();
            LocationService locationService = new LocationService( rockContext );
            Location location = GetLocation( locationService );

            AttributeService attributeService = new AttributeService( rockContext );
            AttributeQualifierService attributeQualifierService = new AttributeQualifierService( rockContext );

            string previousName = location.Name;

            int? orphanedImageId = null;
            if ( location.ImageId != imgImage.BinaryFileId )
            {
                orphanedImageId = location.ImageId;
                location.ImageId = imgImage.BinaryFileId;
            }

            location.Name = tbName.Text;
            location.IsActive = cbIsActive.Checked;
            location.LocationTypeValueId = dvpLocationType.SelectedValueAsId();
            if ( gpParentLocation != null && gpParentLocation.Location != null )
            {
                location.ParentLocationId = gpParentLocation.Location.Id;
            }
            else
            {
                location.ParentLocationId = null;
            }

            location.PrinterDeviceId = ddlPrinter.SelectedValueAsInt();

            acAddress.GetValues( location );

            location.GeoPoint = geopPoint.SelectedValue;
            if ( geopPoint.SelectedValue != null )
            {
                location.IsGeoPointLocked = true;
            }
            location.GeoFence = geopFence.SelectedValue;

            location.IsGeoPointLocked = cbGeoPointLocked.Checked;

            location.SoftRoomThreshold = nbSoftThreshold.Text.AsIntegerOrNull();
            location.FirmRoomThreshold = nbFirmThreshold.Text.AsIntegerOrNull();

            if ( !Page.IsValid )
            {
                return;
            }

            // if the location IsValid is false, and the UI controls didn't report any errors, it is probably because the custom rules of location didn't pass.
            // So, make sure a message is displayed in the validation summary
            cvLocation.IsValid = location.IsValid;

            if ( !cvLocation.IsValid )
            {
                cvLocation.ErrorMessage = location.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                return;
            }

            rockContext.WrapTransaction( () =>
            {
                if ( location.Id.Equals( 0 ) )
                {
                    locationService.Add( location );
                }
                rockContext.SaveChanges();

                if ( orphanedImageId.HasValue )
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

                location.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributeEdits, location );
                location.SaveAttributeValues( rockContext );
            } );

            // If this is a names location (or was previously)
            if ( !string.IsNullOrWhiteSpace( location.Name ) || ( previousName ?? string.Empty ) != ( location.Name ?? string.Empty ) )
            {
                // flush the checkin config
                Rock.CheckIn.KioskDevice.Clear();
            }

            if ( _personId.HasValue )
            {
                NavigateToParentPage( new Dictionary<string, string> { { "PersonId", _personId.Value.ToString() } } );
            }
            else
            {
                Rock.CheckIn.KioskDevice.Clear();

                var qryParams = new Dictionary<string, string>();
                qryParams["LocationId"] = location.Id.ToString();
                qryParams["ExpandedIds"] = PageParameter( "ExpandedIds" );

                NavigateToPage( RockPage.Guid, qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( _personId.HasValue )
            {
                NavigateToParentPage( new Dictionary<string, string> { { "PersonId", _personId.Value.ToString() } } );
            }
            else
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
                    ShowReadonlyDetails( GetLocation() );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnStandardize control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnStandardize_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            LocationService locationService = new LocationService( rockContext );
            Location location = GetLocation( locationService );

            acAddress.GetValues( location );

            locationService.Verify( location, true );

            rockContext.SaveChanges();

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
            Location location = GetLocation();
            location.LocationTypeValueId = dvpLocationType.SelectedValueAsId();
            LocationTypeValueId = location.LocationTypeValueId;
            location.LoadAttributes();
            Rock.Attribute.Helper.GetEditValues( phAttributeEdits, location );
            BuildAttributeEdits( location, true );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="parentLocationId">The parent location identifier.</param>
        public void ShowDetail( int locationId )
        {
            Location location = GetLocation();
            pnlDetails.Visible = false;

            if ( !locationId.Equals( 0 ) )
            {
                pdAuditDetails.SetEntity( location, ResolveRockUrl( "~" ) );
            }
            else
            {
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;

            }

            bool editAllowed = location.IsAuthorized( Authorization.EDIT, CurrentPerson );

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
                if ( location.Id > 0 && !_personId.HasValue )
                {
                    ShowReadonlyDetails( location );
                }
                else
                {
                    ShowEditDetails();
                }
            }

        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="location">The location.</param>
        private void ShowEditDetails()
        {
            RockContext rockContext = new RockContext();
            LocationService locationService = new LocationService( rockContext );
            Location location = GetLocation( locationService );

            divAdvSettings.Visible = !_personId.HasValue;
            cbIsActive.Visible = !_personId.HasValue;
            geopFence.Visible = !_personId.HasValue;
            nbSoftThreshold.Visible = !_personId.HasValue;
            nbFirmThreshold.Visible = !_personId.HasValue;

            if ( location.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( Location.FriendlyTypeName ).FormatAsHtmlTitle();
                hlInactive.Visible = false;
            }
            else
            {
                if ( _personId.HasValue )
                {
                    hlInactive.Visible = false;
                }

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

            nbSoftThreshold.Text = location.SoftRoomThreshold.HasValue ? location.SoftRoomThreshold.Value.ToString() : "";
            nbFirmThreshold.Text = location.FirmRoomThreshold.HasValue ? location.FirmRoomThreshold.Value.ToString() : "";

            Guid mapStyleValueGuid = GetAttributeValue( "MapStyle" ).AsGuid();
            geopPoint.MapStyleValueGuid = mapStyleValueGuid;
            geopFence.MapStyleValueGuid = mapStyleValueGuid;

            var attributeService = new AttributeService( rockContext );

            dvpLocationType.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.LOCATION_TYPE.AsGuid() ).Id;

            gpParentLocation.Location = location.ParentLocation ?? locationService.Get( location.ParentLocationId ?? 0 );

            // LocationType depends on Selected ParentLocation
            if ( location.Id == 0 && dvpLocationType.Items.Count > 1 )
            {
                // if this is a new location
                dvpLocationType.SelectedIndex = 0;
            }
            else
            {
                dvpLocationType.SetValue( location.LocationTypeValueId );
            }

            location.LoadAttributes( rockContext );
            BuildAttributeEdits( location, true );
        }

        private void BuildAttributeEdits( Location location, bool setValues )
        {
            phAttributeEdits.Controls.Clear();
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

            if ( location.SoftRoomThreshold.HasValue )
            {
                descriptionList.Add( "Threshold", location.SoftRoomThreshold.Value.ToString( "N0" ) );
            }

            if ( location.FirmRoomThreshold.HasValue )
            {
                descriptionList.Add( "Threshold (Absolute)", location.FirmRoomThreshold.Value.ToString( "N0" ) );
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
            var mapStyleValue = DefinedValueCache.Get( GetAttributeValue( "MapStyle" ) );
            var googleAPIKey = GlobalAttributesCache.Get().GetValue( "GoogleAPIKey" );

            if ( mapStyleValue == null )
            {
                mapStyleValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK );
            }

            if ( mapStyleValue != null && !string.IsNullOrWhiteSpace( googleAPIKey ) )
            {
                string mapStyle = mapStyleValue.GetAttributeValue( "StaticMapStyle" );

                if ( !string.IsNullOrWhiteSpace( mapStyle ) )
                {
                    if ( location.GeoPoint != null )
                    {
                        string markerPoints = string.Format( "{0},{1}", location.GeoPoint.Latitude, location.GeoPoint.Longitude );
                        string mapLink = System.Text.RegularExpressions.Regex.Replace( mapStyle, @"\{\s*MarkerPoints\s*\}", markerPoints );
                        mapLink = System.Text.RegularExpressions.Regex.Replace( mapLink, @"\{\s*PolygonPoints\s*\}", string.Empty );
                        mapLink += "&sensor=false&size=350x200&zoom=13&format=png&key=" + googleAPIKey;
                        phMaps.Controls.Add( new LiteralControl( string.Format( "<div class='group-location-map'><img class='img-thumbnail' src='{0}'/></div>", mapLink ) ) );
                    }

                    if ( location.GeoFence != null )
                    {
                        string polygonPoints = "enc:" + location.EncodeGooglePolygon();
                        string mapLink = System.Text.RegularExpressions.Regex.Replace( mapStyle, @"\{\s*MarkerPoints\s*\}", string.Empty );
                        mapLink = System.Text.RegularExpressions.Regex.Replace( mapLink, @"\{\s*PolygonPoints\s*\}", polygonPoints );
                        mapLink += "&sensor=false&size=350x200&format=png&key=" + googleAPIKey;
                        phMaps.Controls.Add( new LiteralControl( string.Format( "<div class='group-location-map'><img class='img-thumbnail' src='{0}'/></div>", mapLink ) ) );
                    }
                }
            }

            btnSecurity.Visible = location.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            btnSecurity.Title = location.Name;
            btnSecurity.EntityId = location.Id;

        }

        /// <summary>Gets the location.</summary>
        /// <returns></returns>
        private Location GetLocation()
        {
            return GetLocation( new LocationService( new RockContext() ) );
        }

        /// <summary>Gets the location.</summary>
        /// <param name="locationService">The location service.</param>
        /// <returns></returns>
        private Location GetLocation( LocationService locationService )
        {
            var location = locationService.Get( PageParameter( "LocationId" ).AsInteger() );
            if ( location == null )
            {
                location = new Location
                {
                    Id = 0,
                    IsActive = true,
                    ParentLocationId = PageParameter( "ParentLocationId" ).AsIntegerOrNull(),
                    State = acAddress.GetDefaultState(),
                    Country = acAddress.GetDefaultCountry()
                };
            }

            if ( LocationTypeValueId.HasValue )
            {
                location.LocationTypeValueId = LocationTypeValueId.Value;
            }

            location.LoadAttributes();
            return location;
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

        #endregion
    }
}
