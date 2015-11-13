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
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Dynamic Heat Map" )]
    [Category( "Reporting" )]
    [Description( "Block to a map of the locations of people" )]

    // CustomSetting Dialog
    [TextField( "DataView", "The dataview to filter the people shown on the map. Leave blank to have it determined by the user or by page param", false, "", "CustomSetting" )]

    // Regular attributes
    [DefinedValueField( Rock.SystemGuid.DefinedType.MAP_STYLES, "Map Style", "The map theme that should be used for styling the map.", true, false, Rock.SystemGuid.DefinedValue.MAP_STYLE_GOOGLE, "", 3 )]
    [IntegerField( "Map Height", "Height of the map in pixels (default value is 600px)", false, 600, "", 4 )]
    [TextField( "Polygon Colors", "Comma-Delimited list of colors to use when displaying multiple polygons (e.g. #f37833,#446f7a,#afd074,#649dac,#f8eba2,#92d0df,#eaf7fc).", true, "#f37833,#446f7a,#afd074,#649dac,#f8eba2,#92d0df,#eaf7fc", "", 5 )]
    [DecimalField( "Point Grouping", "The number of miles per to use to group points that are close together. For example, enter 0.25 to group points in 1/4 mile blocks. Increase this if the heatmap has lots of points and is slow", order: 6 )]
    [BooleanField( "Show Filter", defaultValue: true )]
    public partial class DynamicHeatMap : RockBlockCustomSettings
    {
        /// <summary>
        /// Gets the settings tool tip.
        /// </summary>
        /// <value>
        /// The settings tool tip.
        /// </value>
        public override string SettingsToolTip
        {
            get
            {
                return "Configure";
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            ddlUserDataView.Visible = !( this.GetAttributeValue( "DataView" ).AsGuidOrNull().HasValue || this.PageParameter( "DataViewId" ).AsIntegerOrNull().HasValue );
            if ( ddlUserDataView.Visible )
            {
                LoadDropDowns();
            }

            cpCampuses.Campuses = CampusCache.All();

            this.LoadGoogleMapsApi();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // reload the full page to ensure the updated HeatMapData is rendered correctly
            NavigateToPage( this.CurrentPageReference );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !this.IsPostBack )
            {
                var campusIds = this.GetUserPreference( GetUserPreferenceKey( "Campuses" ) ).SplitDelimitedValues().AsIntegerList();
                var dataViewGuid = this.GetUserPreference( GetUserPreferenceKey( "DataView" ) ).AsGuidOrNull();
                cpCampuses.SetValues( campusIds );
                ddlUserDataView.SetValue( dataViewGuid );
                lMessages.Text = string.Empty;
                pnlMap.Visible = true;
                pnlFilter.Visible = this.GetAttributeValue( "ShowFilter" ).AsBooleanOrNull() ?? true;
            }
            
            ShowMap();
        }

        /// <summary>
        /// Gets or sets the heat map data.
        /// </summary>
        /// <value>
        /// The heat map data.
        /// </value>
        public string HeatMapData { get; set; }

        /// <summary>
        /// Gets or sets the campus markers data.
        /// </summary>
        /// <value>
        /// The campus markers data.
        /// </value>
        public string CampusMarkersData { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public class LatLongWeighted
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LatLongWeighted"/> class.
            /// </summary>
            /// <param name="lat">The lat.</param>
            /// <param name="lng">The LNG.</param>
            /// <param name="weight">The weight.</param>
            public LatLongWeighted( double lat, double lng, int weight )
            {
                Lat = lat;
                Long = lng;
                Weight = weight;
            }

            /// <summary>
            /// Gets or sets the lat.
            /// </summary>
            /// <value>
            /// The lat.
            /// </value>
            public double Lat { get; set; }

            /// <summary>
            /// Gets or sets the long.
            /// </summary>
            /// <value>
            /// The long.
            /// </value>
            public double Long { get; set; }

            /// <summary>
            /// Gets or sets the weight.
            /// </summary>
            /// <value>
            /// The weight.
            /// </value>
            public int Weight { get; set; }
        }

        /// <summary>
        /// Shows the report.
        /// </summary>
        private void ShowMap()
        {
            string mapStylingFormat = @"
                        <style>
                            #map_wrapper {{
                                height: {0}px;
                            }}

                            #map_canvas {{
                                width: 100%;
                                height: 100%;
                                border-radius: 8px;
                            }}
                        </style>";

            lMapStyling.Text = string.Format( mapStylingFormat, GetAttributeValue( "MapHeight" ) );

            // add styling to map
            string styleCode = "null";

            DefinedValueCache dvcMapStyle = DefinedValueCache.Read( GetAttributeValue( "MapStyle" ).AsGuid() );
            if ( dvcMapStyle != null )
            {
                styleCode = dvcMapStyle.GetAttributeValue( "DynamicMapStyle" );
            }

            var polygonColorList = GetAttributeValue( "PolygonColors" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            string polygonColors = polygonColorList.AsDelimited( "," );

            string latitude = "39.8282";
            string longitude = "-98.5795";
            string zoom = "4";
            var orgLocation = GlobalAttributesCache.Read().OrganizationLocation;
            if ( orgLocation != null && orgLocation.GeoPoint != null )
            {
                latitude = orgLocation.GeoPoint.Latitude.Value.ToString( System.Globalization.CultureInfo.GetCultureInfo( "en-US" ) );
                longitude = orgLocation.GeoPoint.Longitude.Value.ToString( System.Globalization.CultureInfo.GetCultureInfo( "en-US" ) );
                zoom = "12";
            }

            var rockContext = new RockContext();
            var campuses = CampusCache.All();
            var locationService = new LocationService( rockContext );
            CampusMarkersData = string.Empty;
            foreach ( var campus in campuses )
            {
                if ( campus.LocationId.HasValue )
                {
                    var location = locationService.Get( campus.LocationId.Value );
                    if ( location != null && location.GeoPoint != null )
                    {
                        CampusMarkersData += string.Format( "{{ location: new google.maps.LatLng({0},{1}), campusName:'{2}' }},", location.GeoPoint.Latitude, location.GeoPoint.Longitude, campus.Name );
                    }
                }
            }

            CampusMarkersData.TrimEnd( new char[] { ',' } );

            var groupMemberService = new GroupMemberService( rockContext );
            var groupTypeFamily = GroupTypeCache.GetFamilyGroupType();
            int groupTypeFamilyId = groupTypeFamily.Id;
            var groupRoleAdultId = groupTypeFamily.Roles.FirstOrDefault( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
            var groupLocationTypeHomeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() ).Id;
            int[] connectionStatusValueIds = new[] { 
                DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_ATTENDEE.AsGuid()).Id,
                DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_MEMBER.AsGuid()).Id };

            int recordStatusActiveId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;

            int? dataViewId = this.PageParameter( "DataViewId" ).AsIntegerOrNull();
            Guid? dataViewGuid = null;
            if ( !dataViewId.HasValue )
            {
                dataViewGuid = this.GetAttributeValue( "DataView" ).AsGuidOrNull();
            }

            if ( ddlUserDataView.Visible )
            {
                dataViewGuid = ddlUserDataView.SelectedValue.AsGuidOrNull();
            }

            IQueryable<int> qryPersonIds = null;

            if ( dataViewId.HasValue || dataViewGuid.HasValue )
            {
                DataView dataView = null;
                if ( dataViewId.HasValue )
                {
                    dataView = new DataViewService( rockContext ).Get( dataViewId.Value );
                }
                else
                {
                    dataView = new DataViewService( rockContext ).Get( dataViewGuid.Value );
                }

                if ( dataView != null )
                {
                    List<string> errorMessages;
                    qryPersonIds = dataView.GetQuery( null, rockContext, null, out errorMessages ).OfType<Person>().Select( a => a.Id );
                }
            }

            if ( qryPersonIds == null )
            {
                // if no dataview was specified, show nothing
                qryPersonIds = new PersonService( rockContext ).Queryable().Where( a => false ).Select( a => a.Id );
            }

            var qryGroupMembers = groupMemberService.Queryable();
            
            if ( pnlFilter.Visible )
            {
                var campusIds = cpCampuses.SelectedCampusIds;
                if ( campusIds.Any() )
                {
                    qryGroupMembers = qryGroupMembers.Where( a => a.Group.CampusId.HasValue && campusIds.Contains( a.Group.CampusId.Value ) );
                }
            }

            var qryLocationGroupMembers = qryGroupMembers
                .Where( a => a.Group.GroupTypeId == groupTypeFamilyId )
                .Where( a => a.Group.IsActive )
                .Select( a => new
                {
                    GroupGeoPoint = a.Group.GroupLocations.Where( gl => gl.IsMappedLocation && gl.GroupLocationTypeValueId == groupLocationTypeHomeId && gl.Location.IsActive && gl.Location.GeoPoint != null ).Select( x => x.Location.GeoPoint ).FirstOrDefault(),
                    a.Group.CampusId,
                    a.PersonId
                } )
                .Where( a => ( a.GroupGeoPoint != null ) && qryPersonIds.Contains( a.PersonId ) )
                .GroupBy( a => new { a.GroupGeoPoint.Latitude, a.GroupGeoPoint.Longitude } )
                .Select( s => new
                {
                    s.Key.Longitude,
                    s.Key.Latitude,
                    MemberCount = s.Count()
                } );

            var locationList = qryLocationGroupMembers.ToList()
                .Select( a => new LatLongWeighted( a.Latitude.Value, a.Longitude.Value, a.MemberCount ) )
                .ToList();

            List<LatLongWeighted> points = locationList;

            // cluster points that are close together
            double? milesPerGrouping = this.GetAttributeValue( "PointGrouping" ).AsDoubleOrNull();
            if ( milesPerGrouping.HasValue && milesPerGrouping > 0 )
            {
                var metersPerLatitudePHX = 110886.79;
                var metersPerLongitudePHX = 94493.11;

                double metersPerMile = 1609.34;

                var squareLengthHeightMeters = metersPerMile * milesPerGrouping.Value;

                var longitudeRoundFactor = metersPerLongitudePHX / squareLengthHeightMeters;
                var latitudeRoundFactor = metersPerLatitudePHX / squareLengthHeightMeters;

                points = points.GroupBy( a => new
                {
                    rLat = Math.Round( a.Lat * latitudeRoundFactor ),
                    rLong = Math.Round( a.Lat * longitudeRoundFactor ),
                } ).Select( a => new LatLongWeighted( a.Average( x => x.Lat ), a.Average( x => x.Long ), a.Sum( x => x.Weight ) ) ).ToList();
            }

            this.HeatMapData = points.Select( a => a.Weight > 1
                ? string.Format( "{{location: new google.maps.LatLng({0}, {1}), weight: {2}}}", a.Lat, a.Long, a.Weight )
                : string.Format( "new google.maps.LatLng({0}, {1})", a.Lat, a.Long ) ).ToList().AsDelimited( ",\n" );

            StyleCode = styleCode;
            hfPolygonColors.Value = polygonColors;
            hfCenterLatitude.Value = latitude.ToString();
            hfCenterLongitude.Value = longitude.ToString();
            hfZoom.Value = zoom.ToString();
        }

        /// <summary>
        /// Gets or sets the style code.
        /// </summary>
        /// <value>
        /// The style code.
        /// </value>
        public string StyleCode { get; set; }

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            pnlConfigure.Visible = true;
            LoadDropDowns();
            ddlBlockConfigDataView.SetValue( this.GetAttributeValue( "DataView" ).AsGuidOrNull() );
            mdConfigure.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdConfigure control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdConfigure_SaveClick( object sender, EventArgs e )
        {
            mdConfigure.Hide();
            pnlConfigure.Visible = false;

            this.SetAttributeValue( "DataView", ddlBlockConfigDataView.SelectedValue.AsGuidOrNull().ToString() );
            SaveAttributeValues();

            this.Block_BlockUpdated( sender, e );
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        protected void LoadDropDowns()
        {
            var rockContext = new RockContext();
            var dataViewService = new DataViewService( rockContext );
            var entityTypeIdPerson = EntityTypeCache.GetId<Rock.Model.Person>() ?? 0;
            var dataViewQry = dataViewService.Queryable().Where( a => a.EntityTypeId == entityTypeIdPerson ).OrderBy( a => a.Name ).Select( a => new { a.Name, a.Guid } );

            ddlBlockConfigDataView.Items.Clear();
            ddlBlockConfigDataView.Items.Add( new ListItem() );

            ddlUserDataView.Items.Clear();
            ddlUserDataView.Items.Add( new ListItem() );

            foreach ( var dataView in dataViewQry.ToList() )
            {
                ddlBlockConfigDataView.Items.Add( new ListItem( dataView.Name, dataView.Guid.ToString() ) );
                ddlUserDataView.Items.Add( new ListItem( dataView.Name, dataView.Guid.ToString() ) );
            }
        }

        /// <summary>
        /// Gets the user preference key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string GetUserPreferenceKey( string key )
        {
            return string.Format( "{0}_{1}", this.BlockId, key );
        }

        /// <summary>
        /// Handles the Click event of the btnFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFilter_Click( object sender, EventArgs e )
        {
            // reload the full page to ensure the updated HeatMapData is rendered correctly
            this.SetUserPreference( GetUserPreferenceKey( "Campuses" ), cpCampuses.SelectedCampusIds.AsDelimited( "," ) );
            this.SetUserPreference( GetUserPreferenceKey( "DataView" ), ddlUserDataView.SelectedValue );
            NavigateToPage( this.CurrentPageReference );
        }
}
}
