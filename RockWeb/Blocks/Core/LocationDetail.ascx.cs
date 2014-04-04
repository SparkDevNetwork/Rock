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
            <img src='http://maps.googleapis.com/maps/api/staticmap?sensor=false&size=350x200&format=png&style=feature:all|saturation:0|hue:0xe7ecf0&style=feature:road|saturation:-70&style=feature:transit|visibility:off&style=feature:poi|visibility:off&style=feature:water|visibility:simplified|saturation:-60{% if point %}&markers=color:0x779cb1|{{ point.latitude }},{{ point.longitude }}{% endif %}{% if polygon %}&path=fillcolor:0x779cb155|color:0xFFFFFF00|enc:{{ polygon.google_encoded_polygon }}{% endif %}&visual_refresh=true'/>
        </div>
    {% endif %}
")]
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
                string itemId = PageParameter( "locationId" );
                string parentLocationId = PageParameter( "parentLocationId" );

                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    if ( string.IsNullOrWhiteSpace( parentLocationId ) )
                    {
                        ShowDetail( "locationId", int.Parse( itemId ) );
                    }
                    else
                    {
                        ShowDetail( "locationId", int.Parse( itemId ), int.Parse( parentLocationId ) );
                    }
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

        #region Edit Events

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
            Location location = locationService.Get( int.Parse( hfLocationId.Value ) );

            if ( location != null )
            {
                parentLocationId = location.ParentLocationId;
                string errorMessage;
                if ( !locationService.CanDelete( location, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                locationService.Delete( location );
                rockContext.SaveChanges();
            }

            // reload page, selecting the deleted location's parent
            var qryParams = new Dictionary<string, string>();
            if ( parentLocationId != null )
            {
                qryParams["locationId"] = parentLocationId.ToString();
            }

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
            }

            location.Name = tbName.Text;
            location.IsActive = cbIsActive.Checked;
            location.LocationTypeValueId = ddlLocationType.SelectedValueAsId();
            location.ParentLocation = gpParentLocation.Location; ;

            var addrLocation = locapAddress.Location;
            if ( addrLocation != null )
            {
                location.Street1 = addrLocation.Street1;
                location.Street2 = addrLocation.Street2;
                location.City = addrLocation.City;
                location.State = addrLocation.State;
                location.Zip = addrLocation.Zip;
            }

            location.GeoPoint = geopPoint.SelectedValue;
            if ( geopPoint.SelectedValue != null )
            {
                location.IsGeoPointLocked = true;
            }
            location.GeoFence = geopFence.SelectedValue;

            location.LoadAttributes();
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

            RockTransactionScope.WrapTransaction( () =>
            {
                if ( location.Id.Equals( 0 ) )
                {
                    locationService.Add( location );
                }
                rockContext.SaveChanges();

                location.SaveAttributeValues( rockContext );

            } );



            var qryParams = new Dictionary<string, string>();
            qryParams["locationId"] = location.Id.ToString();

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
                if ( RockPage.Layout.FileName.Equals( "TwoColumnLeft" ) )
                {
                    // Cancelling on Add.  Return to tree view with parent category selected
                    var qryParams = new Dictionary<string, string>();

                    string parentLocationId = PageParameter( "parentLocationId" );
                    if ( !string.IsNullOrWhiteSpace( parentLocationId ) )
                    {
                        qryParams["locationId"] = parentLocationId;
                    }

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
        /// Handles the SelectedIndexChanged event of the ddlLocationType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlLocationType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var location = new LocationService( new RockContext() ).Get( hfLocationId.Value.AsInteger() ?? 0 );
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
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            ShowDetail( itemKey, itemKeyValue, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The location id.</param>
        public void ShowDetail( string itemKey, int itemKeyValue, int? parentLocationId )
        {
            pnlDetails.Visible = false;

            if ( !itemKey.Equals( "locationId" ) )
            {
                return;
            }

            bool editAllowed = true;

            Location location = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                location = new LocationService( new RockContext() ).Get( itemKeyValue );
                if ( location != null )
                {
                    editAllowed = location.IsAuthorized( Authorization.EDIT, CurrentPerson );
                }
            }
            else
            {
                location = new Location { Id = 0, IsActive = true, ParentLocationId = parentLocationId };
            }

            if ( location == null )
            {
                return;
            }

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
                lReadOnlyTitle.Text = location.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            tbName.Text = location.Name;
            cbIsActive.Checked = location.IsActive;
            locapAddress.SetValue( location );
            geopPoint.SetValue( location.GeoPoint );
            geopFence.SetValue( location.GeoFence );

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

        private void BuildAttributeEdits(Location location, bool setValues)
        {
            Rock.Attribute.Helper.AddEditControls( location, phAttributeEdits, setValues );
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="location">The location.</param>
        private void ShowReadonlyDetails( Location location )
        {
            SetEditMode( false );

            hfLocationId.SetValue( location.Id );
            lReadOnlyTitle.Text = location.Name.FormatAsHtmlTitle();

            hlInactive.Visible = !location.IsActive;
            if ( location.LocationTypeValue != null )
            {
                hlType.Text = location.LocationTypeValue.Name;
                hlType.Visible = true;
            }
            else
            {
                hlType.Visible = false;
            }

            DescriptionList descriptionList = new DescriptionList();

            string fullAddress = location.GetFullStreetAddress();
            if ( !string.IsNullOrWhiteSpace( fullAddress ) )
            {
                descriptionList.Add( "Address", fullAddress );
            }

            if ( location.ParentLocation != null )
            {
                descriptionList.Add( "Parent Location", location.ParentLocation.Name );
            }

            lblMainDetails.Text = descriptionList.Html;

            location.LoadAttributes();
            Rock.Attribute.Helper.AddDisplayControls( location, phAttributes );

            // Get all the location locations and location all those that have a geo-location into either points or polygons
            var dict = new Dictionary<string, object>();
            if ( location.GeoPoint != null )
            {
                var pointsDict = new Dictionary<string, object>();
                pointsDict.Add( "latitude", location.GeoPoint.Latitude );
                pointsDict.Add( "longitude", location.GeoPoint.Longitude );
                dict.Add( "point", pointsDict );
            }

            if ( location.GeoFence != null )
            {
                var polygonDict = new Dictionary<string, object>();
                polygonDict.Add( "polygon_wkt", location.GeoFence.AsText());
                polygonDict.Add( "google_encoded_polygon", location.EncodeGooglePolygon() );
                dict.Add( "polygon", polygonDict );
            }

            phMaps.Controls.Clear();
            phMaps.Controls.Add( new LiteralControl( GetAttributeValue( "MapHTML" ).ResolveMergeFields( dict ) ) );

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

        #endregion

}
}