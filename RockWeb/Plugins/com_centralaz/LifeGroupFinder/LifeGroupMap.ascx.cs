// <copyright>
// Copyright by Central Christian Church
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
using System.ComponentModel;
using System.Data.Entity.Spatial;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Text;
using System.Collections.Generic;
using DotLiquid;
using System.Dynamic;
using Rock.Web;
using Rock.Security;

namespace RockWeb.Plugins.com_centralaz.LifeGroupFinder
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( " Life Group Map" )]
    [Category( "com_centralaz > Groups" )]
    [Description( "Displays a group (and any child groups) on a map." )]

    [LinkedPage( "Group Detail Page", "The page to display group details.", true, "", "", 0 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.MAP_STYLES, "Map Style", "The map theme that should be used for styling the map.", true, false, Rock.SystemGuid.DefinedValue.MAP_STYLE_GOOGLE, "", 3 )]
    [IntegerField( "Map Height", "Height of the map in pixels (default value is 600px)", false, 600, "", 4 )]
    [TextField( "Polygon Colors", "Comma-Delimited list of colors to use when displaying multiple polygons (e.g. #f37833,#446f7a,#afd074,#649dac,#f8eba2,#92d0df,#eaf7fc).", true, "#f37833,#446f7a,#afd074,#649dac,#f8eba2,#92d0df,#eaf7fc", "", 5 )]
    [CodeEditorField( "Info Window Contents", "Liquid template for the info window. To suppress the window provide a blank template.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 600, false, @"
<h4 class='margin-t-none'>{{ Group.Name }}</h4> 

<div class='margin-v-sm'>
{% if Location.FormattedHtmlAddress && Location.FormattedHtmlAddress != '' %}
	{{ Location.FormattedHtmlAddress }}
{% endif %}
</div>

{% if LinkedPages.GroupDetailPage != '' %}
    <a class='btn btn-xs btn-action margin-r-sm' href='{{ LinkedPages.GroupDetailPage }}?GroupId={{ Group.Id }}'>View {{ Group.GroupType.GroupTerm }}</a>
{% endif %}
", "", 6 )]
    public partial class LifeGroupMap : Rock.Web.UI.RockBlock
    {
        #region Fields

        protected string _groupColor = string.Empty;
        protected string _childGroupColor = string.Empty;
        protected string _memberColor = string.Empty;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the parameter state.
        /// </summary>
        /// <value>
        /// The state of the parameter.
        /// </value>
        public Dictionary<string, string> ParameterState
        {
            get
            {
                var parameterState = Session["ParameterState"] as Dictionary<string, string>;
                if ( parameterState == null )
                {
                    parameterState = new Dictionary<string, string>();

                    Session["ParameterState"] = parameterState;
                }
                return parameterState;
            }

            set
            {
                Session["ParameterState"] = value;
            }
        }

        #endregion

        #region Base Control Methods

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

            this.LoadGoogleMapsApi();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            lMessages.Text = string.Empty;
            pnlMap.Visible = true;

            if ( !Page.IsPostBack )
            {
                ShowDetails();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the GroupMapper control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            pnlMap.Visible = true;
            ShowDetails();
        }

        protected void ddlCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "CampusId", ddlCampus.SelectedValue );
            ParameterState.AddOrReplace( "Campus", ddlCampus.SelectedValue );
            NavigateToPage( RockPage.Guid, qryParams );
        }

        #endregion

        #region Methods

        private void ShowDetails()
        {
            List<MapItem> groupMapItems = new List<MapItem>();
            var rockContext = new RockContext();
            ParameterState.AddOrReplace( "DetailSource", RockPage.Guid.ToString() );
            var groupTypeGuid = "7F76AE15-C5C4-490E-BF3A-50FB0591A60F".AsGuid();

            ddlCampus.Items.Clear();
            foreach ( var campus in CampusCache.All() )
            {
                ddlCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString().ToUpper() ) );
            }

            int? campusId = PageParameter( "CampusId" ).AsIntegerOrNull();
            if ( !campusId.HasValue )
            {
                if ( ParameterState.Keys.Contains( "Campus" ) && !String.IsNullOrWhiteSpace( ParameterState["Campus"] ) )
                {
                    var qryParams = new Dictionary<string, string>();
                    qryParams.Add( "CampusId", ParameterState["Campus"] );
                    NavigateToPage( RockPage.Guid, qryParams );
                    return;
                }
                else
                {
                    if ( ddlCampus.SelectedValue.AsIntegerOrNull() != null )
                    {
                        var qryParams = new Dictionary<string, string>();
                        qryParams.Add( "CampusId", ddlCampus.SelectedValue );
                        ParameterState.AddOrReplace( "Campus", ddlCampus.SelectedValue );
                        NavigateToPage( RockPage.Guid, qryParams );
                        return;
                    }
                    else
                    {
                        pnlMap.Visible = false;
                        lMessages.Text = "<div class='alert alert-warning'><strong>Group Map</strong> A Campus ID is required to display the map.</div>";
                        return;
                    }
                }
            }

            var selectedCampus = new CampusService( rockContext ).Get( campusId.Value );
            ddlCampus.SelectedValue = campusId.ToString().ToUpper();

            var qry = new GroupService( rockContext ).Queryable( "GroupType.DefaultGroupRole" ).Where( g => g.CampusId == campusId && g.GroupType.Guid == groupTypeGuid && g.Members.FirstOrDefault( m => m.GroupRole.IsLeader == true ) != null && g.IsPublic );
            foreach ( Group group in qry )
            {
                var groupLocation = group.GroupLocations.FirstOrDefault();
                if ( groupLocation != null )
                {
                    // Resolve info window lava template
                    var linkedPageParams = new Dictionary<string, string> { { "GroupId", group.Id.ToString() } };
                    var mergeFields = new Dictionary<string, object>();
                    mergeFields.Add( "Group", group );
                    mergeFields.Add( "Location", groupLocation.Location );

                    Dictionary<string, object> linkedPages = new Dictionary<string, object>();
                    linkedPages.Add( "GroupDetailPage", LinkedPageUrl( "GroupDetailPage", null ) );
                    mergeFields.Add( "LinkedPages", linkedPages );

                    // add collection of allowed security actions
                    Dictionary<string, object> securityActions = new Dictionary<string, object>();
                    securityActions.Add( "View", group.IsAuthorized( Authorization.VIEW, CurrentPerson ) );
                    securityActions.Add( "Edit", group.IsAuthorized( Authorization.EDIT, CurrentPerson ) );
                    securityActions.Add( "Administrate", group.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) );
                    mergeFields.Add( "AllowedActions", securityActions );

                    Template template = Template.Parse( GetAttributeValue( "InfoWindowContents" ) );
                    string infoWindow = template.Render( Hash.FromDictionary( mergeFields ) );

                    // Add a map item for group
                    var mapItem = new FinderMapItem( groupLocation.Location );
                    mapItem.EntityTypeId = EntityTypeCache.Read( "Rock.Model.Group" ).Id;
                    mapItem.EntityId = group.Id;
                    mapItem.Name = group.Name;
                    mapItem.InfoWindow = HttpUtility.HtmlEncode( infoWindow.Replace( Environment.NewLine, string.Empty ).Replace( "\n", string.Empty ).Replace( "\t", string.Empty ) );
                    groupMapItems.Add( mapItem );
                }
            }

            pnlMap.Visible = true;

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
            var markerColors = new List<string>();

            DefinedValueCache dvcMapStyle = DefinedValueCache.Read( GetAttributeValue( "MapStyle" ).AsGuid() );
            if ( dvcMapStyle != null )
            {
                styleCode = dvcMapStyle.GetAttributeValue( "DynamicMapStyle" );
                markerColors = dvcMapStyle.GetAttributeValue( "Colors" )
                    .Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries )
                    .ToList();
                markerColors.ForEach( c => c = c.Replace( "#", string.Empty ) );
            }
            if ( !markerColors.Any() )
            {
                markerColors.Add( "FE7569" );
            }

            _groupColor = markerColors[0].Replace( "#", string.Empty );
            _childGroupColor = ( markerColors.Count > 1 ? markerColors[1] : markerColors[0] ).Replace( "#", string.Empty );
            _memberColor = ( markerColors.Count > 2 ? markerColors[2] : markerColors[0] ).Replace( "#", string.Empty );

            var polygonColorList = GetAttributeValue( "PolygonColors" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            string polygonColors = "\"" + polygonColorList.AsDelimited( "\", \"" ) + "\"";

            string latitude = "39.8282";
            string longitude = "-98.5795";
            string zoom = "4";
            var campusLocation = selectedCampus.Location;
            if ( campusLocation != null && campusLocation.GeoPoint != null )
            {
                latitude = campusLocation.GeoPoint.Latitude.ToString();
                longitude = campusLocation.GeoPoint.Longitude.ToString();
                zoom = "12";
            }

            // write script to page
            string mapScriptFormat = @"
<script> 

    Sys.Application.add_load(function () {{

        var groupData = {0}; 
        var allMarkers = [];
        var groupItems = [];
        var childGroupItems = [];
        var familyItems = {{}};

        var map;
        var bounds = new google.maps.LatLngBounds();
        var infoWindow = new google.maps.InfoWindow();

        var mapStyle = {1};

        var pinShadow = new google.maps.MarkerImage('//chart.googleapis.com/chart?chst=d_map_pin_shadow',
            new google.maps.Size(40, 37),
            new google.maps.Point(0, 0),
            new google.maps.Point(12, 35));

        var polygonColorIndex = 0;
        var polygonColors = [{2}];

        var min = .999999;
        var max = 1.000001;

        initializeMap();

        function initializeMap() {{

            // Set default map options
            var mapOptions = {{
                 mapTypeId: 'roadmap'
                ,styles: mapStyle
                ,center: new google.maps.LatLng({6}, {7})
                ,zoom: {8}
            }};

            // Display a map on the page
            map = new google.maps.Map(document.getElementById('map_canvas'), mapOptions);
            map.setTilt(45);
          
            if ( groupData != null ) {{
                for (var i = 0; i < groupData.length; i++) {{
                    var items = addMapItem(i, groupData[i], '{3}');
                    for (var j = 0; j < items.length; j++) {{
                        items[j].setMap(map);
                    }}
                }}
            }}
    
            // adjust any markers that may overlap
            adjustOverlappedMarkers();

            // When all three requests are done, set the map bounds
            if (!bounds.isEmpty()) {{
                map.fitBounds(bounds);
            }}                   
        }}

        function addMapItem( i, mapItem, color ) {{

            var items = [];

            if (mapItem.Point) {{ 

                var position = new google.maps.LatLng(mapItem.Point.Latitude, mapItem.Point.Longitude);
                bounds.extend(position);

                if (!color) {{
                    color = 'FE7569'
                }}

                var pinImage = new google.maps.MarkerImage('//chart.googleapis.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|' + color,
                    new google.maps.Size(21, 34),
                    new google.maps.Point(0,0),
                    new google.maps.Point(10, 34));

                marker = new google.maps.Marker({{
                    position: position,
                    map: map,
                    title: htmlDecode(mapItem.Name),
                    icon: pinImage,
                    shadow: pinShadow
                }});
    
                items.push(marker);
                allMarkers.push(marker);

                if ( mapItem.InfoWindow != null ) {{ 
                    google.maps.event.addListener(marker, 'click', (function (marker, i) {{
                        return function () {{
                            infoWindow.setContent( $('<div/>').html(mapItem.InfoWindow).text() );
                            infoWindow.open(map, marker);
                        }}
                    }})(marker, i));
                }}

                if ( mapItem.EntityId && mapItem.EntityId > 0 ) {{ 
                    google.maps.event.addListener(marker, 'mouseover', (function (marker, i) {{
                        return function () {{
                            $(""tr[datakey='"" + mapItem.EntityId + ""']"").addClass('row-highlight');
                        }}
                    }})(marker, i));

                    google.maps.event.addListener(marker, 'mouseout', (function (marker, i) {{
                        return function () {{
                            $(""tr[datakey='"" + mapItem.EntityId + ""']"").removeClass('row-highlight');
                        }}
                    }})(marker, i));

                }}
            }}

            if (typeof mapItem.PolygonPoints !== 'undefined' && mapItem.PolygonPoints.length > 0) {{

                var polygon;
                var polygonPoints = [];

                $.each(mapItem.PolygonPoints, function(j, point) {{
                    var position = new google.maps.LatLng(point.Latitude, point.Longitude);
                    bounds.extend(position);
                    polygonPoints.push(position);
                }});

                var polygonColor = getNextPolygonColor();

                polygon = new google.maps.Polygon({{
                    paths: polygonPoints,
                    map: map,
                    strokeColor: polygonColor,
                    fillColor: polygonColor
                }});

                items.push(polygon);

                // Get Center
                var polyBounds = new google.maps.LatLngBounds();
                for ( j = 0; j < polygonPoints.length; j++) {{
                    polyBounds.extend(polygonPoints[j]);
                }}

                if ( mapItem.InfoWindow != null ) {{ 
                    google.maps.event.addListener(polygon, 'click', (function (polygon, i) {{
                        return function () {{
                            infoWindow.setContent( mapItem.InfoWindow );
                            infoWindow.setPosition(polyBounds.getCenter());
                            infoWindow.open(map);
                        }}
                    }})(polygon, i));
                }}
        
            }}

            return items;

        }}

        function setAllMap(markers, map) {{
            for (var i = 0; i < markers.length; i++) {{
                markers[i].setMap(map);
            }}
        }}

        function htmlDecode(input) {{
            var e = document.createElement('div');
            e.innerHTML = input;
            return e.childNodes.length === 0 ? """" : e.childNodes[0].nodeValue;
        }}

        function getNextPolygonColor() {{
            var color = 'FE7569';
            if ( polygonColors.length > polygonColorIndex ) {{
                color = polygonColors[polygonColorIndex];
                polygonColorIndex++;
            }} else {{
                color = polygonColors[0];
                polygonColorIndex = 1;
            }}
            return color;
        }}

        function adjustOverlappedMarkers() {{
            
            if (allMarkers.length > 1) {{
                for(i=0; i < allMarkers.length-1; i++) {{
                    var marker1 = allMarkers[i];
                    var pos1 = marker1.getPosition();
                    for(j=i+1; j < allMarkers.length; j++) {{
                        var marker2 = allMarkers[j];
                        var pos2 = marker2.getPosition();
                        if (pos1.equals(pos2)) {{
                            var newLat = pos1.lat() * (Math.random() * (max - min) + min);
                            var newLng = pos1.lng() * (Math.random() * (max - min) + min);
                            marker1.setPosition( new google.maps.LatLng(newLat,newLng) );
                        }}
                    }}
                }}
            }}

        }}

    }});
</script>";
            var groupsJson = groupMapItems != null && groupMapItems.Any() ?
                string.Format( "JSON.parse('{0}')", groupMapItems.ToJson().Replace( Environment.NewLine, "" ).EscapeQuotes().Replace( "\x0A", "" ) ) : "null";

            string mapScript = string.Format( mapScriptFormat,
                groupsJson, styleCode, polygonColors, _groupColor, _childGroupColor, _memberColor,
                latitude, longitude, zoom );

            ScriptManager.RegisterStartupScript( pnlMap, pnlMap.GetType(), "group-map-script", mapScript, false );

        }

        #endregion

        /// <summary>
        /// A map item class specific to group finder
        /// </summary>
        class FinderMapItem : MapItem
        {
            /// <summary>
            /// Gets or sets the information window.
            /// </summary>
            /// <value>
            /// The information window.
            /// </value>
            public string InfoWindow { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="FinderMapItem"/> class.
            /// </summary>
            /// <param name="location">The location.</param>
            public FinderMapItem( Location location )
                : base( location )
            {

            }
        }
    }
}