// <copyright>
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

using System.Dynamic;
using Rock.Web;
using Nest;

namespace RockWeb.Blocks.Groups
{
    [DisplayName( "Group Map" )]
    [Category( "Groups" )]
    [Description( "Displays a group (and any child groups) on a map." )]

    [LinkedPage(  "Group Page",
        Description = "The page to display group details.",
        IsRequired = true,
        DefaultValue = "",
        Category = "",
        Order = 0,
        Key = AttributeKey.GroupPage )]

    [LinkedPage( "Person Profile Page",
        Description = "The page to display person details.",
        IsRequired = true,
        DefaultValue = "",
        Category = "",
        Order = 1,
        Key = AttributeKey.PersonProfilePage )]

    [LinkedPage( "Map Page",
        Description = "The page to display group map (typically this page).",
        IsRequired = true,
        DefaultValue = "",
        Category = "",
        Order = 2,
        Key = AttributeKey.MapPage )]

    [DefinedValueField( "Map Style",
        Description = "The map theme that should be used for styling the map.",
        IsRequired = true,
        AllowMultiple = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.MAP_STYLES,
        DefaultValue = Rock.SystemGuid.DefinedValue.MAP_STYLE_GOOGLE,
        Category = "",
        Order = 3,
        Key = AttributeKey.MapStyle )]

    [IntegerField( "Map Height",
        Description = "Height of the map in pixels (default value is 600px)",
        IsRequired = false,
        DefaultIntegerValue = 600,
        Category = "",
        Order = 4,
        Key = AttributeKey.MapHeight )]

    [TextField( "Polygon Colors",
        Description = "Comma-Delimited list of colors to use when displaying multiple polygons (e.g. #f37833,#446f7a,#afd074,#649dac,#f8eba2,#92d0df,#eaf7fc).",
        IsRequired = true,
        DefaultValue = "#f37833,#446f7a,#afd074,#649dac,#f8eba2,#92d0df,#eaf7fc",
        Category = "",
        Order = 5,
        Key = AttributeKey.PolygonColors )]

    [BooleanField( "Show Campuses Filter",
        Description = "",
        DefaultBooleanValue = false,
        Order = 6,
        Key = AttributeKey.ShowCampusesFilter )]

    [DefinedValueField(
        "Campus Types",
        Description = "This setting filters the list of campuses by type that are displayed in the campus drop-down.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        Order = 7,
        Key = AttributeKey.CampusTypes )]

    [DefinedValueField(
        "Campus Statuses",
        Description = "This setting filters the list of campuses by statuses that are displayed in the campus drop-down.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        Order = 8,
        Key = AttributeKey.CampusStatuses )]

    [BooleanField("Show Child Groups as Default",
        Description = "Defaults to showing all child groups if no user preference is set",
        DefaultBooleanValue = false,
        Order = 9,
        Key = AttributeKey.ShowChildGroupsAsDefault )]

    [CodeEditorField( "Info Window Contents",
        Description = "Lava template for the info window. To suppress the window provide a blank template.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 600,
        IsRequired = false,
        DefaultValue = DEFAULT_LAVA_TEMPLATE,
        Category ="",
        Order = 10,
        Key = AttributeKey.InfoWindowContents )]

    [Rock.SystemGuid.BlockTypeGuid( "967F0D2B-DB76-486A-B034-D22B9D9240D3" )]
    public partial class GroupMap : Rock.Web.UI.RockBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string GroupPage = "GroupPage";
            public const string PersonProfilePage = "PersonProfilePage";
            public const string MapPage = "MapPage";
            public const string MapStyle = "MapStyle";
            public const string MapHeight = "MapHeight";
            public const string PolygonColors = "PolygonColors";
            public const string ShowCampusesFilter = "ShowCampusesFilter";
            public const string ShowChildGroupsAsDefault = "ShowChildGroupsAsDefault";
            public const string InfoWindowContents = "InfoWindowContents";
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatuses = "CampusStatuses";
        }

        private static class PreferenceKey
        {
            public const string ShowChildGroups = "ShowChildGroups";
            public const string GroupTypeIds = "GroupTypeIds";
        }

        #endregion

        #region Constants

        private const string DEFAULT_LAVA_TEMPLATE = @"
<div style='width:250px'>

    <div class='clearfix'>
        <h4 class='pull-left' style='margin-top: 0;'>{{ GroupName }}</h4>
        <span class='label label-campus pull-right'>{{ Campus.Name }}</span>
    </div>

    <div class='clearfix'>
		{% if Location.Address and Location.Address != '' %}
			<strong>{{ Location.Type }}</strong>
			<br>{{ Location.Address }}
		{% endif %}
		{% if Members.size > 0 %}
			<br>
			<br><strong>{{ GroupType.GroupMemberTerm }}s</strong><br>
			{% for GroupMember in Members -%}
				<div class='clearfix'>
					{% if GroupMember.PhotoUrl != '' %}
						<div class='pull-left' style='padding: 0 5px 2px 0'>
							<img src='{{ GroupMember.PhotoUrl }}&maxheight=50&maxwidth=50'>
						</div>
					{% endif %}
					<a href='{{ GroupMember.ProfilePageUrl }}'>{{ GroupMember.NickName }} {{ GroupMember.LastName }}</a> - {{ GroupMember.Role }}
                    {% if groupTypeGuid != '790E3215-3B10-442B-AF69-616C0DCB998E' and GroupMember.ConnectionStatus != '' %}
				        <br>{{ GroupMember.ConnectionStatus }}
					{% endif %}
					{% if GroupMember.Email != '' %}
						<br>{{ GroupMember.Email }}
					{% endif %}
					{% for Phone in GroupMember.Person.PhoneNumbers %}
						<br>{{ Phone.NumberTypeValue.Value }}: {{ Phone.NumberFormatted }}
					{% endfor %}
				</div>
				<br>
			{% endfor -%}
		{% endif %}
    </div>

    {% if GroupType.Guid != '790E3215-3B10-442B-AF69-616C0DCB998E' %}
		<br>
		<a class='btn btn-xs btn-action' href='{{ DetailPageUrl }}'>View {{ GroupType.GroupTerm }}</a>
		<a class='btn btn-xs btn-action' href='{{ MapPageUrl }}'>View Map</a>
	{% endif %}

</div>
";
        private const string MAP_STYLING_FORMAT = @"
                        <style>
                            #map_wrapper {{
                                height: {0}px;
                            }}

                            #map_canvas {{
                                width: 100%;
                                height: 100%;
                                border-radius: var(--border-radius-base);
                            }}
                        </style>";

        private const string MAP_SCRIPT_FORMAT_NAME = @"
<script>

    Sys.Application.add_load(function () {{

        var groupId = {0};
        var allMarkers = [];
        var groupItems = [];
        var childGroupItems = [];
        var groupMemberItems = [];
        var familyItems = {{}};
        var fetchFamiliesPromise = {{}};

        var map;
        var bounds = new google.maps.LatLngBounds();
        var infoWindow = new google.maps.InfoWindow();

        var mapStyle = {1};

        var polygonColorIndex = 0;
        var polygonColors = [{2}];

        var infoWindowRequest = {6};

        var min = .999999;
        var max = 1.000001;

        initializeMap();

        // the campuses picker only applies to the connection status checkboxes
        $('.js-campuses-picker').hide();

        function initializeMap() {{

            // Set default map options
            var mapOptions = {{
                 mapTypeId: 'roadmap'
                ,styles: mapStyle
                ,center: new google.maps.LatLng({7}, {8})
                ,zoom: {9}
            }};

            // Display a map on the page
            map = new google.maps.Map(document.getElementById('map_canvas'), mapOptions);
            map.setTilt(45);

            var getChildMapInfoUrl =  Rock.settings.get('baseUrl') + 'api/Groups/GetMapInfo/{0}/Children';
            if ('{10}' != '') {{
                getChildMapInfoUrl += '?includeDescendants={10}';
                if ('{11}' != '') {{
                    getChildMapInfoUrl += '&groupTypeIds={11}';
                }}
            }}

            // Query for group, child group, and group member locations asyncronously
            $.when (

                // Get group
                $.get( Rock.settings.get('baseUrl') + 'api/Groups/GetMapInfo/{0}', function( mapItems ) {{
                    $.each(mapItems, function (i, mapItem) {{
                        $('#lGroupName').text(mapItem.Name);
                        var items = addMapItem(i, mapItem, '{3}');
                        for (var i = 0; i < items.length; i++) {{
                            groupItems.push(items[i]);
                        }}
                    }});
                    if (groupItems.length == 0) {{
                        $('.js-show-group').hide();
                    }} else {{
                        setAllMap(groupItems, null);
                    }}
                }}),

                // Get Child Groups
                $.get(getChildMapInfoUrl, function( mapItems ) {{
                    $.each(mapItems, function (i, mapItem) {{
                        var items = addMapItem(i, mapItem, '{4}');
                        for (var i = 0; i < items.length; i++) {{
                            childGroupItems.push(items[i]);
                        }}
                    }});
                    if (childGroupItems.length == 0) {{
                        $('.js-show-child-groups').hide();
                    }} else {{
                        setAllMap(childGroupItems, null);
                    }}
                }}),

                // Get Group Members
                $.get( Rock.settings.get('baseUrl') + 'api/Groups/GetMapInfo/{0}/Members/{12}', function( mapItems ) {{
                    $.each(mapItems, function (i, mapItem) {{
                        var items = addMapItem(i, mapItem, '{5}');
                        for (var i = 0; i < items.length; i++) {{
                            groupMemberItems.push(items[i]);
                        }}
                    }});
                    if (groupMemberItems.length == 0) {{
                        $('.js-show-group-members').hide();
                    }} else {{
                        setAllMap(groupMemberItems, null);
                    }}
                }})

            ).done( function() {{

                // adjust any markers that may overlap
                adjustOverlappedMarkers();

                // When all three requests are done, set the map bounds
                if (!bounds.isEmpty()) {{
                    map.fitBounds(bounds);
                }}

                // If a group map marker exists, check the group option and show marker
                if ( groupItems.length > 0) {{
                    $('#cbShowGroup').prop('checked', true);
                    setAllMap(groupItems, map);

                // Else check for group member locations, if they exists, select the members option and show member markers
                }} else if ( groupMemberItems.length > 0) {{
                    $('#cbShowGroupMembers').prop('checked', true);
                    setAllMap(groupMemberItems, map);

                // otherwise look for child groups and display their markers
                }} else if ( childGroupItems.length > 0) {{
                    $('#cbShowChildGroups').prop('checked', true);
                    setAllMap(childGroupItems, map);
                }}

            }});

        }}

        function addMapItem( i, mapItem, color ) {{

            var items = [];

            if (mapItem.Point) {{

                var position = new google.maps.LatLng(mapItem.Point.Latitude, mapItem.Point.Longitude);
                bounds.extend(position);

                if (!color) {{
                    color = 'FE7569'
                }}

                var pinImage = {{
                    path: 'M 0,0 C -2,-20 -10,-22 -10,-30 A 10,10 0 1,1 10,-30 C 10,-22 2,-20 0,0 z',
                    fillColor: '#' + color,
                    fillOpacity: 1,
                    strokeColor: '#000',
                    strokeWeight: 1,
                    scale: 1,
                    labelOrigin: new google.maps.Point(0,-28)
                }};

                marker = new google.maps.Marker({{
                    position: position,
                    map: map,
                    title: htmlDecode(mapItem.Name),
                    icon: pinImage,
                    label: String.fromCharCode(9679)
                }});

                items.push(marker);
                allMarkers.push(marker);

                google.maps.event.addListener(marker, 'click', (function (marker, i) {{
                    return function () {{
                        $.post( Rock.settings.get('baseUrl') + 'api/Groups/GetMapInfoWindow/' + mapItem.EntityId + '/' + mapItem.LocationId, infoWindowRequest, function( data ) {{
                            infoWindow.setContent( data.Result );
                            infoWindow.open(map, marker);
                        }});
                    }}
                }})(marker, i));

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

                google.maps.event.addListener(polygon, 'click', (function (polygon, i) {{
                    return function () {{
                        $.post( Rock.settings.get('baseUrl') + 'api/Groups/GetMapInfoWindow/' + mapItem.EntityId + '/' + mapItem.LocationId, infoWindowRequest, function( data ) {{
                            infoWindow.setContent( data.Result );
                            infoWindow.setPosition(polyBounds.getCenter());
                            infoWindow.open(map);
                        }});
                    }}
                }})(polygon, i));

                if ( mapItem.EntityId == {0} ) {{
                    $('.js-connection-status').show();

                    // the campuses picker only applies to the connection status checkboxes
                    $('.js-campuses-picker').show();
                }}

            }}

            return items;

        }}

        // Show/Hide group
        $('#cbShowGroup').on('click', function() {{
            if ($(this).prop('checked')) {{
                setAllMap(groupItems, map);
            }} else {{
                setAllMap(groupItems, null);
            }}
        }});

        // Show/Hide child groups
        $('#cbShowChildGroups').on('click', function() {{
            if ($(this).prop('checked')) {{
                setAllMap(childGroupItems, map);
            }} else {{
                setAllMap(childGroupItems, null);
            }}
        }});

        // Show/Hide group members
        $('#cbShowGroupMembers').on('click', function() {{
            if ($(this).prop('checked')) {{
                setAllMap(groupMemberItems, map);
            }} else {{
                setAllMap(groupMemberItems, null);
            }}
        }});

        $('.js-campuses-picker input').on('click', function() {{
            // clear out all the family markers since we have a new set of Campuses
            $('.js-connection-status-cb').each( function(i) {{
                var statusId = $(this).attr('data-item');
                if (familyItems[statusId] !== undefined) {{
                    setAllMap(familyItems[statusId], null);
                }}
            }});
            familyItems = {{}};

            // re-select and fetch the familyitems for each selected connection status using the new set of campusids
            $('.js-connection-status-cb:checked').each( function(i) {{
                $(this).attr('checked', false);
                $(this).trigger('click');
            }});
        }});

        // Show/Hide families
        $('.js-connection-status-cb').on('click', function() {{
            var statusId = $(this).attr('data-item');

            var campusIds = '';
            $('.js-campuses-picker input:checked').each(function(i) {{
                campusIds += $(this).val() + ',';
            }});

            if ($(this).prop('checked')) {{
                if (typeof familyItems[statusId] !== 'undefined') {{
                    setAllMap(familyItems[statusId], map);
                }} else {{
                    familyItems[statusId] = [];
                    var color = $(this).attr('data-color');
                    var getMapInfoUrl = Rock.settings.get('baseUrl') + 'api/Groups/GetMapInfo/{0}/Families/' + statusId;
                    if (campusIds != '') {{
                        getMapInfoUrl += '?CampusIds=' + campusIds;
                    }}

                    // if we are already in the process of fetching families for this status, abort and start over again
                    if (fetchFamiliesPromise[statusId] !== undefined) {{
                        fetchFamiliesPromise[statusId].abort();
                    }}

                    fetchFamiliesPromise[statusId] = $.get( getMapInfoUrl, function( mapItems ) {{
                        $.each(mapItems, function (i, mapItem) {{
                            var items = addMapItem(i, mapItem, color);
                            for (var i = 0; i < items.length; i++) {{
                                familyItems[statusId].push(items[i]);
                            }}
                        }});
                    }});
                }}
            }} else {{
                if (typeof familyItems[statusId] !== 'undefined') {{
                    setAllMap(familyItems[statusId], null);
                }}
            }}
        }});

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

        #endregion

        #region Fields

        protected string _groupColor = string.Empty;
        protected string _childGroupColor = string.Empty;
        protected string _memberColor = string.Empty;

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

            this.LoadGoogleMapsApi();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            lMessages.Text = string.Empty;
            pnlMap.Visible = true;

            if ( !Page.IsPostBack )
            {
                // only list GroupTypes that could have a location (and have ShowInNavigation and ShowInGrouplist)
                gtpGroupType.GroupTypes = new GroupTypeService( new RockContext() ).Queryable().Where(
                    a => a.ShowInNavigation
                        && a.ShowInGroupList
                        && a.LocationSelectionMode != GroupLocationPickerMode.None).OrderBy( a => a.Name ).ToList();

                var preferences = GetBlockPersonPreferences();
                var selectedGroupTypeIds = preferences.GetValue( PreferenceKey.GroupTypeIds );
                if ( !string.IsNullOrWhiteSpace( selectedGroupTypeIds ) )
                {
                    var selectedGroupTypeIdList = selectedGroupTypeIds.Split( ',' ).AsIntegerList();
                    gtpGroupType.SelectedGroupTypeIds = selectedGroupTypeIdList;
                }

                var showChildGroups = preferences.GetValue( PreferenceKey.ShowChildGroups ).AsBooleanOrNull() ?? GetAttributeValue( AttributeKey.ShowChildGroupsAsDefault ).AsBoolean();
                cbShowAllGroups.Checked = showChildGroups;

                var statuses = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ).DefinedValues
                    .OrderBy( v => v.Order )
                    .ThenBy( v => v.Value )
                    .Select( v => new
                    {
                        v.Id,
                        Name = v.Value.Pluralize(),
                        Color = ( v.GetAttributeValue( "Color" ) ?? "" ).Replace( "#", "" )
                    } )
                    .ToList();

                rptStatus.DataSource = statuses.Where( s => s.Color != "" ).ToList();
                rptStatus.DataBind();

                cpCampuses.Campuses = GetCampuses();
                cpCampuses.Visible = this.GetAttributeValue( AttributeKey.ShowCampusesFilter ).AsBoolean();

                Map();
            }

            base.OnLoad( e );
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

            string mapStylingFormat = MAP_STYLING_FORMAT;
            lMapStyling.Text = string.Format( mapStylingFormat, GetAttributeValue( AttributeKey.MapHeight ) );

            // add styling to map
            string styleCode = "null";
            var markerColors = new List<string>();

            DefinedValueCache dvcMapStyle = DefinedValueCache.Get( GetAttributeValue( AttributeKey.MapStyle ).AsGuid() );
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

            var polygonColorList = GetAttributeValue( AttributeKey.PolygonColors ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            string polygonColors = "\"" + polygonColorList.AsDelimited( "\", \"" ) + "\"";

            string template = HttpUtility.HtmlEncode( GetAttributeValue( AttributeKey.InfoWindowContents ).Replace( Environment.NewLine, string.Empty ).Replace( "\n", string.Empty ) );
            string groupPage = GetAttributeValue( AttributeKey.GroupPage );
            string personProfilePage = GetAttributeValue( AttributeKey.PersonProfilePage );
            string mapPage = GetAttributeValue( AttributeKey.MapPage );
            string infoWindowJson = string.Format( @"{{ ""GroupPage"":""{0}"", ""PersonProfilePage"":""{1}"", ""MapPage"":""{2}"", ""Template"":""{3}"" }}",
                groupPage, personProfilePage, mapPage, template );

            string latitude = "39.8282";
            string longitude = "-98.5795";
            string zoom = "4";
            var orgLocation = GlobalAttributesCache.Get().OrganizationLocation;
            if (orgLocation != null && orgLocation.GeoPoint != null)
            {
                latitude = orgLocation.GeoPoint.Latitude.ToString();
                longitude = orgLocation.GeoPoint.Longitude.ToString();
                zoom = "12";
            }

            // write script to page
            string mapScriptFormat = MAP_SCRIPT_FORMAT_NAME;

            string mapScript = string.Format( mapScriptFormat,
                    groupId.Value, // {0}
                    styleCode, // {1}
                    polygonColors, // {2}
                    _groupColor, // {3}
                    _childGroupColor, // {4}
                    _memberColor, // {5}
                    infoWindowJson, // {6}
                    latitude, // {7}
                    longitude, // {8}
                    zoom, // {9}
                    cbShowAllGroups.Checked.ToTrueFalse(), // {10}
                    gtpGroupType.SelectedGroupTypeIds.AsDelimited(","), // {11}
                    GroupMemberStatus.Active // {12}
                );

            ScriptManager.RegisterStartupScript( pnlMap, pnlMap.GetType(), "group-map-script", mapScript, false );
        }

        /// <summary>
        /// Handles the Click event of the btnApplyOptions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnApplyOptions_Click( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( PreferenceKey.GroupTypeIds, gtpGroupType.SelectedGroupTypeIds.AsDelimited( "," ) );
            preferences.SetValue( PreferenceKey.ShowChildGroups, cbShowAllGroups.Checked.ToTrueFalse() );
            preferences.Save();

            Map();
        }

        /// <summary>
        /// Gets the list of available campuses after filtering by any selected statuses or types.
        /// </summary>
        /// <returns></returns>
        private List<CampusCache> GetCampuses()
        {
            var campusTypeIds = GetAttributeValues( AttributeKey.CampusTypes )
                .AsGuidOrNullList()
                .Where( g => g.HasValue )
                .Select( g => DefinedValueCache.GetId( g.Value ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            var campusStatusIds = GetAttributeValues( AttributeKey.CampusStatuses )
                .AsGuidOrNullList()
                .Where( g => g.HasValue )
                .Select( g => DefinedValueCache.GetId( g.Value ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            return CampusCache.All()
                .Where( c => ( !campusTypeIds.Any() || ( c.CampusTypeValueId.HasValue && campusTypeIds.Contains( c.CampusTypeValueId.Value ) ) )
                    && ( !campusStatusIds.Any() || ( c.CampusStatusValueId.HasValue && campusStatusIds.Contains( c.CampusStatusValueId.Value ) ) ) )
                .ToList();
        }

        #endregion
    }
}