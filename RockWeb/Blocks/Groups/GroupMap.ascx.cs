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

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Group Map" )]
    [Category( "Groups" )]
    [Description( "Displays a group (and any child groups) on a map." )]

    [IntegerField( "Map Height", "Height of the map in pixels (default value is 600px)", false, 600, "", 2 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.MAP_STYLES, "Map Style", "The map theme that should be used for styling the map.", true, false, Rock.SystemGuid.DefinedValue.MAP_STYLE_GOOGLE, "", 8 )]
    public partial class GroupMap : Rock.Web.UI.RockBlock
    {
        #region Fields

        #endregion

        #region Properties

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
                Map();
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
            Map();
        }

        #endregion

        #region Methods

        private void Map()
        {

            int? groupId = PageParameter( "GroupId" ).AsIntegerOrNull();
            if ( !groupId.HasValue )
            {
                pnlMap.Visible = false;
                lMessages.Text = "<div class='alert alert-warning'><strong>Group Map</strong> A Group ID is required to display the map.</div>";
                return;
            }

            var rockContext = new RockContext();
            var group = new GroupService( rockContext ).Queryable( "GroupLocations.Location" )
                .Where( g => g.Id == groupId.Value )
                .FirstOrDefault();

            if ( group == null )
            {
                pnlMap.Visible = false;
                lMessages.Text = "<div class='alert alert-warning'><strong>Group Map</strong> The requested does not exists.</div>";
                return;
            }

            var points = new List<string>();
            foreach ( var point in group.GroupLocations
                .Where( g => g.Location.GeoPoint != null )
                .Select( g => g.Location.GeoPoint ) )
            {
                if ( point.Latitude.HasValue && point.Longitude.HasValue )
                {
                    points.Add( string.Format( @"{{ ""latitude"":""{0}"", ""longitude"":""{1}"" }}", point.Latitude, point.Longitude ) );
                }
            }

            var polygons = new List<string>();
            foreach ( var polygon in group.GroupLocations
                .Where( g => g.Location.GeoFence != null )
                .Select( g => g.Location.GeoFence ) )
            {
                string coordinates = polygon.AsText().Replace( "POLYGON ((", "" ).Replace( "))", "" );
                string[] longSpaceLat = coordinates.Split( ',' );

                var polygonPoints = new List<string>();
                for ( int i = 0; i < longSpaceLat.Length; i++ )
                {
                    string[] longLat = longSpaceLat[i].Trim().Split( ' ' );
                    if ( longLat.Length == 2 )
                    {
                        polygonPoints.Add( string.Format( @"{{ ""latitude"":""{0}"", ""longitude"":""{1}"" }}", longLat[1], longLat[0] ) );
                    }
                }

                polygons.Add( string.Format( "[ {0} ]", polygonPoints.AsDelimited( ", " ) ) );
            }

            string groupJson = string.Format( @"{{ ""name"":""{0}"", ""points"": [ {1} ], ""polygons"": [ {2} ] }}",
                group.Name, points.AsDelimited(", "), polygons.AsDelimited(", ") );

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
            string markerColor = "FE7569";

            DefinedValueCache dvcMapStyle = DefinedValueCache.Read( GetAttributeValue( "MapStyle" ).AsGuid() );
            if ( dvcMapStyle != null )
            {
                styleCode = dvcMapStyle.GetAttributeValue( "DynamicMapStyle" );
                markerColor = dvcMapStyle.GetAttributeValue( "MarkerColor" ).Replace( "#", string.Empty );
            }

            // write script to page
            string mapScriptFormat = @"
<script> 

    Sys.Application.add_load(function () {{

        var groupId = {0};

        var map;
        var bounds = new google.maps.LatLngBounds();
        var mapStyle = {1};
        var pinColor = '{2}';
        var pinImage = new google.maps.MarkerImage('http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|' + pinColor,
            new google.maps.Size(21, 34),
            new google.maps.Point(0,0),
            new google.maps.Point(10, 34));
        var pinShadow = new google.maps.MarkerImage('http://chart.apis.google.com/chart?chst=d_map_pin_shadow',
            new google.maps.Size(40, 37),
            new google.maps.Point(0, 0),
            new google.maps.Point(12, 35));

        var groupData = JSON.parse('{3}'); 

        initializeMap();

        function initializeMap() {{

            var mapOptions = {{
                 mapTypeId: 'roadmap'
                ,styles: mapStyle
            }};

            // Display a map on the page
            map = new google.maps.Map(document.getElementById('map_canvas'), mapOptions);
            map.setTilt(45);

            // Loop through our array of markers & place each one on the map
            $.each(groupData.points, function (i, point) {{

                var position = new google.maps.LatLng(point.latitude, point.longitude);
                bounds.extend(position);

                marker = new google.maps.Marker({{
                    position: position,
                    map: map,
                    title: htmlDecode(groupData.name),
                    icon: pinImage,
                    shadow: pinShadow
                }});

            }});

            $.each(groupData.polygons, function (i, polygon) {{

                var polygon;

                var polyPoints = [];

                $.each(polygon, function(j, point) {{
                    var position = new google.maps.LatLng(point.latitude, point.longitude);
                    bounds.extend(position);
                    polyPoints.push(position);
                }});

                polygon = new google.maps.Polygon({{
                    paths: polyPoints,
                    map: map,
                    strokeColor: pinColor,
                    fillColor: pinColor
                }});

            }});

            map.fitBounds(bounds);
                       
            var boundsListener = google.maps.event.addListener((map), 'bounds_changed', addMapItems );

        }}

        function htmlDecode(input) {{
            var e = document.createElement('div');
            e.innerHTML = input;
            return e.childNodes.length === 0 ? """" : e.childNodes[0].nodeValue;
        }}

        function addMapItems() {{
        }}
    }});
</script>";

            string mapScript = string.Format( mapScriptFormat, groupId.Value, styleCode, markerColor, groupJson );

            ScriptManager.RegisterStartupScript( pnlMap, pnlMap.GetType(), "group-map-script", mapScript, false );

        }


        #endregion
    }
}