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
    [CodeEditorField( "Group Window Contents", "Liquid template for the group window. To suppress the window provide a blank template.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 600, false, @"
<div class='clearfix'>
    <h4 class='pull-left' style='margin-top: 0;'>{{GroupName}}</h4> 
    <span class='label label-campus pull-right'>{{GroupCampus}}</span>
</div>

<div class='clearfix'>
    <div class='pull-left' style='padding-right: 24px'>
        <strong>{{GroupLocation.Name}}</strong><br>
        {{GroupLocation.Street1}}
        <br>{{GroupLocation.City}}, {{GroupLocation.State}} {{GroupLocation.Zip}}
        {% for attribute in Attributes %}
            {% if forloop.first %}<br/>{% endif %}
            <br/><strong>{{attribute.Name}}:</strong> {{ attribute.Value }}
        {% endfor %}
    </div>
    <div class='pull-left'>
        <strong>{{GroupMemberTerm}}s</strong><br>
        {% for GroupMember in GroupMembers -%}
            {% if PersonProfilePage != '' %}
                <a href='{{PersonProfilePage}}{{GroupMember.Id}}'>{{GroupMember.NickName}} {{GroupMember.LastName}}</a>
            {% else %}
                {{GroupMember.NickName}} {{GroupMember.LastName}}
            {% endif %}
            - {{GroupMember.Email}}
            {% for PhoneType in GroupMember.PhoneTypes %}
                <br>{{PhoneType.Name}}: {{PhoneType.Number}}
            {% endfor %}
            <br>
        {% endfor -%}
    </div>
</div>

{% if GroupDetailPage != '' %}
    <br>
    <a class='btn btn-xs btn-action' href='{{GroupDetailPage}}'>View Group</a>
{% endif %}

", "", 9 )]
    [CodeEditorField( "Group Member Contents", "   Liquid template for the group window. To suppress the window provide a blank template.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 600, false, @"
<div class='clearfix'>
    <h4 class='pull-left' style='margin-top: 0;'>{{GroupName}}</h4> 
    <span class='label label-campus pull-right'>{{GroupCampus}}</span>
</div>

<div class='clearfix'>
    <div class='pull-left' style='padding-right: 24px'>
        <strong>{{GroupLocation.Name}}</strong><br>
        {{GroupLocation.Street1}}
        <br>{{GroupLocation.City}}, {{GroupLocation.State}} {{GroupLocation.Zip}}
        {% for attribute in Attributes %}
            {% if forloop.first %}<br/>{% endif %}
            <br/><strong>{{attribute.Name}}:</strong> {{ attribute.Value }}
        {% endfor %}
    </div>
    <div class='pull-left'>
        <strong>{{GroupMemberTerm}}s</strong><br>
        {% for GroupMember in GroupMembers -%}
            {% if PersonProfilePage != '' %}
                <a href='{{PersonProfilePage}}{{GroupMember.Id}}'>{{GroupMember.NickName}} {{GroupMember.LastName}}</a>
            {% else %}
                {{GroupMember.NickName}} {{GroupMember.LastName}}
            {% endif %}
            - {{GroupMember.Email}}
            {% for PhoneType in GroupMember.PhoneTypes %}
                <br>{{PhoneType.Name}}: {{PhoneType.Number}}
            {% endfor %}
            <br>
        {% endfor -%}
    </div>
</div>

{% if GroupDetailPage != '' %}
    <br>
    <a class='btn btn-xs btn-action' href='{{GroupDetailPage}}'>View Group</a>
{% endif %}

", "", 9 )]
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

        var map;
        var markers = [];
        var polygons = [];
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

        initializeMap();

        function initializeMap() {{

            var mapOptions = {{
                 mapTypeId: 'roadmap'
                ,styles: mapStyle
                ,center: new google.maps.LatLng(39.8282, -98.5795)
                ,zoom: 4
            }};

            // Display a map on the page
            map = new google.maps.Map(document.getElementById('map_canvas'), mapOptions);
            map.setTilt(45);

            addMarker('test', 39.8282, -98.5795); 

            getGroup();

        }}

        function getGroup() {{

            $.get( Rock.settings.get('baseUrl') + 'api/Groups/GetMapInfo/{0}', function( mapItems ) {{

                // Loop through array of map items
                $.each(mapItems, function (i, item) {{

                    if (item.Point) {{ 
                        addMarker(item.Name, item.Point.latitude, item.Point.longitude); 
                    }}

                    if (typeof item.PolygonPoints !== 'undefined' && item.PolygonPoints.length > 0) {{

                        var polygon;
                        var polygonPoints = [];

                        $.each(item.PolygonPoints, function(j, point) {{
                            var position = new google.maps.LatLng(point.latitude, point.longitude);
                            bounds.extend(position);
                            polygonPoints.push(position);
                        }});

                        polygon = new google.maps.Polygon({{
                            paths: polygonPoints,
                            map: map,
                            strokeColor: pinColor,
                            fillColor: pinColor
                        }});

                    }}

                }});

                // map.fitBounds(bounds);
                       
                // var boundsListener = google.maps.event.addListener((map), 'bounds_changed', addMapItems );

            }});        

        }}

        function addMarker( name, latitude, longitude ) {{

            var position = new google.maps.LatLng(latitude, longitude);
            bounds.extend(position);

            marker = new google.maps.Marker({{
                position: position,
                map: map,
                title: htmlDecode(name),
                icon: pinImage,
                shadow: pinShadow
            }});

            markers.push(marker);
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

            string mapScript = string.Format( mapScriptFormat, groupId.Value, styleCode, markerColor );

            ScriptManager.RegisterStartupScript( pnlMap, pnlMap.GetType(), "group-map-script", mapScript, false );

        }


        #endregion
    }
}