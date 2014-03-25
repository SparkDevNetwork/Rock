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

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Group Mapper" )]
    [Category( "Groups" )]
    [Description( "Displays groups on a map." )]

    [GroupTypeField( "Group Type", "The type of group to map.", true, "", "", 0 )]
    [DefinedValueField( "2E68D37C-FB7B-4AA5-9E09-3785D52156CB", "Location Type", "The location type to use for the map.", true, false, "", "", 1 )]
    //[GroupRoleField("", "Display Group Role", "")]
    [IntegerField( "Map Height", "Height of the map in pixels (default value is 600px)", false, 600, "", 2 )]
    [LinkedPage("Group Detail Page", "Page to use as a link to the group details (optional).", false, "", "", 3)]
    [LinkedPage( "Person Profile Page", "Page to use as a link to the person profile page (optional).", false, "", "", 4 )]
    [BooleanField("Show Map Info Window", "Control whether a info window should be displayed when clicking on a map point.", true, "", 5)]
    [TextField( "Attributes", "Comma delimited list of attribute keys to include values for in the map info window (e.g. 'StudyTopic,MeetingTime').", false, "", "", 6 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.MAP_STYLES, "Map Style", "The map theme that should be used for styling the map.", true, false, Rock.SystemGuid.DefinedValue.MAP_STYLE_GOOGLE, "", 7 )]
    [CodeEditorField( "Info Window Contents", "Liquid template for the info window. To suppress the window provide a blank template.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 600, false, @"<div class='clearfix'>
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
            <br>
        {% endfor -%}
    </div>
</div>

{% if GroupDetailPage != '' %}
    <br>
    <a class='btn btn-xs btn-action' href='{{GroupDetailPage}}'>View Group</a>
{% endif %}

", "", 8 )]
    [BooleanField( "Enable Debug", "Enabling debug will display the fields of the first 5 groups to help show you wants available for your liquid.", false, "", 9 )]
    public partial class GroupMapper : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods
        
        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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
        /// Handles the BlockUpdated event of the PageLiquid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated(object sender, EventArgs e)
        {
            pnlMap.Visible = true;
            Map();
        }

        #endregion

        #region Methods

        private void Map()
        {
            lMapStyling.Text = String.Format( @"
                        <style>
                            #map_wrapper {{
                                height: {0}px;
                            }}

                            #map_canvas {{
                                width: 100%;
                                height: 100%;
                                border-radius: 8px;
                            }}
                        </style>", GetAttributeValue( "MapHeight" ) );
                                
            if ( !string.IsNullOrEmpty( GetAttributeValue( "GroupType" ) ) && !string.IsNullOrEmpty( GetAttributeValue( "LocationType" ) ) )
            {
                pnlMap.Visible = true;

                int groupsMapped = 0;
                int groupsWithNoGeo = 0;

                StringBuilder sbGroupJson = new StringBuilder();
                StringBuilder sbGroupsWithNoGeo = new StringBuilder();

                Guid groupType = new Guid( GetAttributeValue( "GroupType" ) );
                Guid locationType = new Guid( GetAttributeValue( "LocationType" ) );

                Template template = null;
                if ( GetAttributeValue( "ShowMapInfoWindow" ).AsBoolean() )
                {
                    template = Template.Parse( GetAttributeValue( "InfoWindowContents" ).Trim() );
                }
                else
                {
                    template = Template.Parse( "" );
                }

                var groupPageRef = new PageReference( GetAttributeValue( "GroupDetailPage" ) );

                // create group detail link for use in map's info window
                var personPageParams = new Dictionary<string, string>();
                personPageParams.Add( "PersonId", "" );
                var personProfilePage = LinkedPageUrl("PersonProfilePage", personPageParams );


                var groupEntityType = EntityTypeCache.Read(typeof(Group));
                var dynamicGroups = new List<dynamic>();

                using ( new UnitOfWorkScope() )
                {
                    // Create query to get attribute values for selected attribute keys.
                    var attributeKeys = GetAttributeValue( "Attributes" ).SplitDelimitedValues().ToList();
                    var attributeValues = new AttributeValueService().Queryable( "Attribute" )
                        .Where( v =>
                            v.Attribute.EntityTypeId == groupEntityType.Id &&
                            attributeKeys.Contains( v.Attribute.Key ) );

                    GroupService groupService = new GroupService();
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
                            GroupLocation = g.GroupLocations
                                                .Where( l => l.GroupLocationTypeValue.Guid == locationType )
                                                .Select( l => new
                                                {
                                                    l.Location.Street1,
                                                    l.Location.Street2,
                                                    l.Location.City,
                                                    l.Location.State,
                                                    l.Location.Zip,
                                                    Latitude = l.Location.GeoPoint.Latitude,
                                                    Longitude = l.Location.GeoPoint.Longitude,
                                                    l.GroupLocationTypeValue.Name
                                                } ).FirstOrDefault(),
                            GroupMembers = g.Members
//                                                .Where( m => m.GroupRoleId == 25 )
                                                .Select( m => new
                                                {
                                                    m.Person.Id,
                                                    GuidP = m.Person.Guid,
                                                    m.Person.NickName,
                                                    m.Person.LastName,
                                                    RoleName = m.GroupRole.Name,
                                                    m.Person.Email,
                                                    PhotoGuid = m.Person.Photo != null ? m.Person.Photo.Guid : Guid.Empty,
                                                    PhoneNumbers = m.Person.PhoneNumbers.Select( p => new { p.IsUnlisted, p.Number, PhoneType = p.NumberTypeValue.Name } )
                                                } ),
                            AttributeValues = attributeValues
                                                .Where( v => v.EntityId == g.Id )

                        } );

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
                        dynGroup.GroupLocation = group.GroupLocation;
                        dynGroup.GroupMembers = group.GroupMembers;

                        var groupAttributes = new List<dynamic>();
                        foreach ( AttributeValue value in group.AttributeValues )
                        {
                            var attrCache = AttributeCache.Read( value.AttributeId );
                            var dictAttribute = new Dictionary<string, object>();
                            dictAttribute.Add( "Key", attrCache.Key );
                            dictAttribute.Add( "Name", attrCache.Name );

                            if (attrCache != null)
                            {
                                dictAttribute.Add( "Value", attrCache.FieldType.Field.FormatValue( null, value.Value, attrCache.QualifierValues, false ) );
                            }
                            else
                            {
                                dictAttribute.Add( "Value", value.Value );
                            }

                            groupAttributes.Add( dictAttribute );
                        }

                        dynGroup.Attributes = groupAttributes;

                        dynamicGroups.Add( dynGroup );
                    }
                }

                // enable showing debug info
                if ( GetAttributeValue( "EnableDebug" ).AsBoolean() )
                {
                    lDebug.Visible = true;
                    StringBuilder debugInfo = new StringBuilder();
                    debugInfo.Append( "<div class='alert alert-info'><h4>Debug Info</h4>" );
                    debugInfo.Append( "<p><em>Showing first 5 groups.</em></p>" );
                    debugInfo.Append( "<pre>" + dynamicGroups.Take( 5 ).ToJson() + "</pre>" );
                    debugInfo.Append( "</div" );
                    lDebug.Text = debugInfo.ToString();
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
                        string infoWindow = template.Render( Hash.FromDictionary( groupDict ) ).Replace( "\n", "" );
                        sbGroupJson.Append( String.Format( @"{{ ""name"":""{0}"" , ""latitude"":""{1}"", ""longitude"":""{2}"", ""infowindow"":""{3}"" }}," 
                                                , HttpUtility.HtmlEncode( group.GroupName )
                                                , group.GroupLocation.Latitude
                                                , group.GroupLocation.Longitude
                                                , HttpUtility.HtmlEncode( infoWindow ) ) );
                    }
                    else
                    {
                        groupsWithNoGeo++;

                        if ( !string.IsNullOrWhiteSpace( group.GroupDetailPage ) )
                        {
                            sbGroupsWithNoGeo.Append( String.Format( @"<li><a href='{0}'>{1}</a></li>", group.GroupDetailPage, group.GroupName ) );
                        }
                        else
                        {
                            sbGroupsWithNoGeo.Append( String.Format( @"<li>{0}</li>", group.GroupName ) );
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
                    markerColor = dvcMapStyle.GetAttributeValue( "MarkerColor" ).Replace( "#", "" );
                }

                // write script to page
                lMapScript.Text = String.Format( @" <script> 
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
                                                </script>",
                                        groupJson,
                                        GetAttributeValue( "ShowMapInfoWindow" ).AsBoolean().ToString().ToLower(),
                                        styleCode,
                                        markerColor );

                if ( groupsMapped == 0 ) {
                    pnlMap.Visible = false;
                    lMessages.Text = @" <p>  
                                                <div class='alert alert-warning fade in'>No groups were able to be mapped. You may want to check your configuration.</div>
                                        </p>";
                }
                else { 
                    // output any warnings
                    if (groupsWithNoGeo > 0 ) {
                        lMessages.Text = String.Format(@" <p>  
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
                                            </p>", sbGroupsWithNoGeo.ToString());
                    }
                }
            }
            else
            {
                pnlMap.Visible = false;
                lMessages.Text = "<div class='alert alert-warning'><strong>Group Mapper</strong> Please configure a group type to display and a location type to use.</div>";
               
            }
        }

        #endregion
    }
}