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

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Group Type Map" )]
    [Category( "Groups" )]
    [Description( "Displays groups on a map." )]

    [GroupTypeField( "Group Type", "The type of group to map.", false, "", "", 0 )]
    //[GroupRoleField("", "Display Group Role", "")]
    [IntegerField( "Map Height", "Height of the map in pixels (default value is 600px)", false, 600, "", 2 )]
    [LinkedPage( "Group Detail Page", "Page to use as a link to the group details (optional).", false, "", "", 3 )]
    [LinkedPage( "Person Profile Page", "Page to use as a link to the person profile page (optional).", false, "", "", 4 )]
    [BooleanField( "Show Map Info Window", "Control whether a info window should be displayed when clicking on a map point.", true, "", 5 )]
    [BooleanField( "Include Inactive Groups", "Determines if inactive groups should be included on the map.", false, "", 6 )]
    [TextField( "Attributes", "Comma delimited list of attribute keys to include values for in the map info window (e.g. 'StudyTopic,MeetingTime').", false, "", "", 7 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.MAP_STYLES, "Map Style", "The map theme that should be used for styling the map.", true, false, Rock.SystemGuid.DefinedValue.MAP_STYLE_GOOGLE, "", 8 )]
    [CodeEditorField( "Info Window Contents", "Liquid template for the info window. To suppress the window provide a blank template.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 600, false, @"
<div class='clearfix'>
    <h4 class='pull-left' style='margin-top: 0;'>{{GroupName}}</h4> 
    <span class='label label-campus pull-right'>{{GroupCampus}}</span>
</div>

<div class='clearfix'>
    <div class='pull-left' style='padding-right: 24px'>
        <strong>{{GroupLocation.Name}}</strong><br>
        {{GroupLocation.Street1}}
        <br>{{GroupLocation.City}}, {{GroupLocation.State}} {{GroupLocation.PostalCode}}
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
    [BooleanField( "Enable Debug", "Enabling debug will display the fields of the first 5 groups to help show you wants available for your liquid.", false, "", 10 )]
    public partial class GroupTypeMap : Rock.Web.UI.RockBlock
    {
        #region Fields

        //// used for private variables

        #endregion

        #region Properties

        //// used for public / protected properties

        #endregion

        #region Base Control Methods

        ////  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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

            string settingGroupTypeId = GetAttributeValue( "GroupType" );
            string queryStringGroupTypeId = PageParameter( "GroupTypeId" );

            if ( ( string.IsNullOrWhiteSpace(settingGroupTypeId) && string.IsNullOrWhiteSpace(queryStringGroupTypeId) )  )
            {
                pnlMap.Visible = false;
                lMessages.Text = "<div class='alert alert-warning'><strong>Group Mapper</strong> Please configure a group type to display as a block setting or pass a GroupTypeId as a query parameter.</div>";
            } 
            else 
            {
                var rockContext = new RockContext();

                pnlMap.Visible = true;

                int groupsMapped = 0;
                int groupsWithNoGeo = 0;

                StringBuilder sbGroupJson = new StringBuilder();
                StringBuilder sbGroupsWithNoGeo = new StringBuilder();

                Guid? groupType = null;
                int groupTypeId = -1;

                if ( !string.IsNullOrWhiteSpace( settingGroupTypeId ) )
                {
                    groupType = new Guid( settingGroupTypeId );
                }
                else
                {
                    if ( !string.IsNullOrWhiteSpace( queryStringGroupTypeId ) && Int32.TryParse( queryStringGroupTypeId, out groupTypeId ) )
                    {
                        groupType = new GroupTypeService( rockContext ).Get( groupTypeId ).Guid;
                    }
                }

                if ( groupType != null )
                {

                    Template template = null;
                    if ( GetAttributeValue( "ShowMapInfoWindow" ).AsBoolean() )
                    {
                        template = Template.Parse( GetAttributeValue( "InfoWindowContents" ).Trim() );
                    }
                    else
                    {
                        template = Template.Parse( string.Empty );
                    }

                    var groupPageRef = new PageReference( GetAttributeValue( "GroupDetailPage" ) );

                    // create group detail link for use in map's info window
                    var personPageParams = new Dictionary<string, string>();
                    personPageParams.Add( "PersonId", string.Empty );
                    var personProfilePage = LinkedPageUrl( "PersonProfilePage", personPageParams );

                    var groupEntityType = EntityTypeCache.Read( typeof( Group ) );
                    var dynamicGroups = new List<dynamic>();


                    // Create query to get attribute values for selected attribute keys.
                    var attributeKeys = GetAttributeValue( "Attributes" ).SplitDelimitedValues().ToList();
                    var attributeValues = new AttributeValueService( rockContext ).Queryable( "Attribute" )
                        .Where( v =>
                            v.Attribute.EntityTypeId == groupEntityType.Id &&
                            attributeKeys.Contains( v.Attribute.Key ) );

                    GroupService groupService = new GroupService( rockContext );
                    var groups = groupService.Queryable()
                        .Where( g => g.GroupType.Guid == groupType )
                        .Select( g => new
                        {
                            Group = g,
                            GroupId = g.Id,
                            GroupName = g.Name,
                            GroupGuid = g.Guid,
                            GroupMemberTerm = g.GroupType.GroupMemberTerm,
                            GroupCampus = g.Campus.Name,
                            IsActive = g.IsActive,
                            GroupLocation = g.GroupLocations
                                                .Where( l => l.Location.GeoPoint != null )
                                                .Select( l => new
                                                {
                                                    l.Location.Street1,
                                                    l.Location.Street2,
                                                    l.Location.City,
                                                    l.Location.State,
                                                    PostalCode = l.Location.PostalCode,
                                                    Latitude = l.Location.GeoPoint.Latitude,
                                                    Longitude = l.Location.GeoPoint.Longitude,
                                                    Name = l.GroupLocationTypeValue.Value
                                                } ).FirstOrDefault(),
                            GroupMembers = g.Members,
                            AttributeValues = attributeValues
                                                .Where( v => v.EntityId == g.Id )
                        } );


                    if ( GetAttributeValue( "IncludeInactiveGroups" ).AsBoolean() == false )
                    {
                        groups = groups.Where( g => g.IsActive == true );
                    }

                    // Create dynamic object to include attribute values
                    foreach ( var group in groups )
                    {
                        dynamic dynGroup = new ExpandoObject();
                        dynGroup.GroupId = group.GroupId;
                        dynGroup.GroupName = group.GroupName;

                        // create group detail link for use in map's info window
                        if ( groupPageRef.PageId > 0 )
                        {
                            var groupPageParams = new Dictionary<string, string>();
                            groupPageParams.Add( "GroupId", group.GroupId.ToString() );
                            groupPageRef.Parameters = groupPageParams;
                            dynGroup.GroupDetailPage = groupPageRef.BuildUrl();
                        }
                        else
                        {
                            dynGroup.GroupDetailPage = string.Empty;
                        }

                        dynGroup.PersonProfilePage = personProfilePage;
                        dynGroup.GroupMemberTerm = group.GroupMemberTerm;
                        dynGroup.GroupCampus = group.GroupCampus;
                        dynGroup.GroupLocation = group.GroupLocation;

                        var groupAttributes = new List<dynamic>();
                        foreach ( AttributeValue value in group.AttributeValues )
                        {
                            var attrCache = AttributeCache.Read( value.AttributeId );
                            var dictAttribute = new Dictionary<string, object>();
                            dictAttribute.Add( "Key", attrCache.Key );
                            dictAttribute.Add( "Name", attrCache.Name );

                            if ( attrCache != null )
                            {
                                dictAttribute.Add( "Value", attrCache.FieldType.Field.FormatValueAsHtml( null, value.Value, attrCache.QualifierValues, false ) );
                            }
                            else
                            {
                                dictAttribute.Add( "Value", value.Value );
                            }

                            groupAttributes.Add( dictAttribute );
                        }

                        dynGroup.Attributes = groupAttributes;

                        var groupMembers = new List<dynamic>();
                        foreach ( GroupMember member in group.GroupMembers )
                        {
                            var dictMember = new Dictionary<string, object>();
                            dictMember.Add( "Id", member.Person.Id );
                            dictMember.Add( "GuidP", member.Person.Guid );
                            dictMember.Add( "NickName", member.Person.NickName );
                            dictMember.Add( "LastName", member.Person.LastName );
                            dictMember.Add( "RoleName", member.GroupRole.Name );
                            dictMember.Add( "Email", member.Person.Email );
                            dictMember.Add( "PhotoGuid", member.Person.Photo != null ? member.Person.Photo.Guid : Guid.Empty );

                            var phoneTypes = new List<dynamic>();
                            foreach ( PhoneNumber p in member.Person.PhoneNumbers )
                            {
                                var dictPhoneNumber = new Dictionary<string, object>();
                                dictPhoneNumber.Add( "Name", p.NumberTypeValue.Value );
                                dictPhoneNumber.Add( "Number", p.ToString() );
                                phoneTypes.Add( dictPhoneNumber );
                            }

                            dictMember.Add( "PhoneTypes", phoneTypes );

                            groupMembers.Add( dictMember );
                        }

                        dynGroup.GroupMembers = groupMembers;

                        dynamicGroups.Add( dynGroup );
                    }

                    // enable showing debug info
                    if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
                    {
                        lDebug.Visible = true;
                        lDebug.Text = dynamicGroups.Take( 5 ).lavaDebugInfo();
                    }
                    else
                    {
                        lDebug.Visible = false;
                        lDebug.Text = string.Empty;
                    }

                    foreach ( var group in dynamicGroups )
                    {
                        if ( group.GroupLocation != null && group.GroupLocation.Latitude != null )
                        {
                            groupsMapped++;
                            var groupDict = group as IDictionary<string, object>;
                            string infoWindow = template.Render( Hash.FromDictionary( groupDict ) ).Replace( "\n", string.Empty );
                            sbGroupJson.Append( string.Format(
                                                    @"{{ ""name"":""{0}"" , ""latitude"":""{1}"", ""longitude"":""{2}"", ""infowindow"":""{3}"" }},",
                                                    HttpUtility.HtmlEncode( group.GroupName ),
                                                    group.GroupLocation.Latitude,
                                                    group.GroupLocation.Longitude,
                                                    HttpUtility.HtmlEncode( infoWindow ) ) );
                        }
                        else
                        {
                            groupsWithNoGeo++;

                            if ( !string.IsNullOrWhiteSpace( group.GroupDetailPage ) )
                            {
                                sbGroupsWithNoGeo.Append( string.Format( @"<li><a href='{0}'>{1}</a></li>", group.GroupDetailPage, group.GroupName ) );
                            }
                            else
                            {
                                sbGroupsWithNoGeo.Append( string.Format( @"<li>{0}</li>", group.GroupName ) );
                            }
                        }
                    }

                    string groupJson = sbGroupJson.ToString();

                    // remove last comma
                    if ( groupJson.Length > 0 )
                    {
                        groupJson = groupJson.Substring( 0, groupJson.Length - 1 );
                    }

                    // add styling to map
                    string styleCode = "null";
                    string markerColor = "FE7569";

                    DefinedValueCache dvcMapStyle = DefinedValueCache.Read( GetAttributeValue( "MapStyle" ).AsGuid() );
                    if ( dvcMapStyle != null )
                    {
                        styleCode = dvcMapStyle.GetAttributeValue( "DynamicMapStyle" );
                        var colors = dvcMapStyle.GetAttributeValue( "Colors" ).Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                        if ( colors.Any() )
                        {
                            markerColor = colors.First().Replace( "#", "" );
                        }
                    }

                    // write script to page
                    string mapScriptFormat = @" <script> 
                                                Sys.Application.add_load(function () {{
                                                    var groupData = JSON.parse('{{ ""groups"" : [ {0} ]}}'); 
                                                    var showInfoWindow = {1}; 
                                                    var mapStyle = {2};
                                                    var pinColor = '{3}';
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
                                                        console.log(mapStyle);
                                                        var map;
                                                        var bounds = new google.maps.LatLngBounds();
                                                        var mapOptions = {{
                                                            mapTypeId: 'roadmap',
                                                            styles: mapStyle
                                                        }};

                                                        // Display a map on the page
                                                        map = new google.maps.Map(document.getElementById('map_canvas'), mapOptions);
                                                        map.setTilt(45);

                                                        // Display multiple markers on a map
                                                        if (showInfoWindow) {{
                                                            var infoWindow = new google.maps.InfoWindow(), marker, i;
                                                        }}

                                                        // Loop through our array of markers & place each one on the map
                                                        $.each(groupData.groups, function (i, group) {{

                                                            var position = new google.maps.LatLng(group.latitude, group.longitude);
                                                            bounds.extend(position);

                                                            marker = new google.maps.Marker({{
                                                                position: position,
                                                                map: map,
                                                                title: htmlDecode(group.name),
                                                                icon: pinImage,
                                                                shadow: pinShadow
                                                            }});

                                                            // Allow each marker to have an info window    
                                                            if (showInfoWindow) {{
                                                                google.maps.event.addListener(marker, 'click', (function (marker, i) {{
                                                                    return function () {{
                                                                        infoWindow.setContent(htmlDecode(groupData.groups[i].infowindow));
                                                                        infoWindow.open(map, marker);
                                                                    }}
                                                                }})(marker, i));
                                                            }}

                                                            map.fitBounds(bounds);
                       
                                                        }});

                                                        // Override our map zoom level once our fitBounds function runs (Make sure it only runs once)
                                                        var boundsListener = google.maps.event.addListener((map), 'bounds_changed', function (event) {{
                                                            google.maps.event.removeListener(boundsListener);
                                                        }});
                                                    }}

                                                    function htmlDecode(input) {{
                                                        var e = document.createElement('div');
                                                        e.innerHTML = input;
                                                        return e.childNodes.length === 0 ? """" : e.childNodes[0].nodeValue;
                                                    }}
                                                }});
                                            </script>";

                    string mapScript = string.Format(
                                            mapScriptFormat,
                                            groupJson,
                                            GetAttributeValue( "ShowMapInfoWindow" ).AsBoolean().ToString().ToLower(),
                                            styleCode,
                                            markerColor );

                    ScriptManager.RegisterStartupScript( pnlMap, pnlMap.GetType(), "group-mapper-script", mapScript, false );

                    if ( groupsMapped == 0 )
                    {
                        pnlMap.Visible = false;
                        lMessages.Text = @" <p>  
                                                <div class='alert alert-warning fade in'>No groups were able to be mapped. You may want to check your configuration.</div>
                                        </p>";
                    }
                    else
                    {
                        // output any warnings
                        if ( groupsWithNoGeo > 0 )
                        {
                            string messagesFormat = @" <p>  
                                                <div class='alert alert-warning fade in'>Some groups could not be mapped.
                                                    <button type='button' class='close' data-dismiss='alert' aria-hidden='true'><i class='fa fa-times'></i></button>
                                                    <small><a data-toggle='collapse' data-parent='#accordion' href='#map-error-details'>Show Details</a></small>
                                                    <div id='map-error-details' class='collapse'>
                                                        <p class='margin-t-sm'>
                                                            <strong>Groups That Could Not Be Mapped</strong>
                                                            <ul>
                                                                {0}
                                                            </ul>
                                                        </p>
                                                    </div>
                                                </div> 
                                            </p>";
                            lMessages.Text = string.Format( messagesFormat, sbGroupsWithNoGeo.ToString() );
                        }
                    }
                }
                else
                {
                    pnlMap.Visible = false;
                    lMessages.Text = "<div class='alert alert-warning'><strong>Group Mapper</strong> Please configure a group type to display and a location type to use.</div>";
                }
            }
        }

        #endregion
    }
}