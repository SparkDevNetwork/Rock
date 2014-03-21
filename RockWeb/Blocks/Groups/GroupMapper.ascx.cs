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
    [IntegerField( "Map Height", "Height of the map in pixels (default value is 600px)", false, 600, "", 1 )]
    [LinkedPage("Group Detail Page", "Page to use as a link to the person details (optional).", false, "", "", 3)]
    [BooleanField("Show Map Info Window", "Control whether a info window should be displayed when clicking on a map point.", true, "", 4)]
    [CodeEditorField( "Info Window Contents", "Liquid template for the info window. To suppress the window provide a blank template.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 600, false, @"<h4>{{GroupName}}</h4>

<strong>{{GroupLocation.Name}}</strong><br>
{{GroupLocation.Street1}}
<br>{{GroupLocation.City}}, {{GroupLocation.State}} {{GroupLocation.Zip}} 

{% if GroupDetailPage != '' %}
    <br>
    <a href=""{{GroupDetailPage}}{{GroupId}} "">View Group</a>
{% endif %}
", "", 5  )]
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

            string groupDetailPage = GetAttributeValue( "GroupDetailPage" );
                                
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

                // create group detail link for use in map's info window
                string groupPageUrl = string.Empty;
                if ( groupDetailPage != null )
                {
                    var pageParamsInfoWindow = new Dictionary<string, string>();
                    pageParamsInfoWindow.Add( "groupId", "" );
                    var pageReferenceInfoWindow = new Rock.Web.PageReference( groupDetailPage, pageParamsInfoWindow );
                    groupPageUrl = pageReferenceInfoWindow.BuildUrl();
                }

                GroupService groupService = new GroupService();
                var groups = groupService.Queryable()
                    .Where( g => g.GroupType.Guid == groupType )
                    .Select( g => new
                    {
                        GroupId = g.Id,
                        GroupName = g.Name,
                        GroupGuid = g.Guid,
                        GroupDetailPage = groupPageUrl,
                        GroupLocation = g.GroupLocations
                                            .Where( l => l.GroupLocationTypeValue.Guid == locationType )
                                            .Select( l => new 
                                            { 
                                                l.Location.Street1,
                                                l.Location.Street2,
                                                l.Location.City,
                                                l.Location.State,
                                                l.Location.Zip,
                                                l.Location.GeoPoint,
                                                l.GroupLocationTypeValue.Name
                                            } ).FirstOrDefault()
                        //GroupMembers = g.Members.Where(m => m.GroupRoleId == 1).Select();
                    } );


                foreach ( var group in groups )
                {
                    if ( group.GroupLocation != null && group.GroupLocation.GeoPoint != null )
                    {
                        groupsMapped++;
                        string infoWindow = template.Render( Hash.FromAnonymousObject( group ) ).Replace("\n", "");
                        string test = group.ToJson();
                        sbGroupJson.Append( String.Format( @"{{ ""name"":""{0}"" , ""latitude"":""{1}"", ""longitude"":""{2}"", ""infowindow"":""{3}"" }}," 
                                                , HttpUtility.HtmlEncode( group.GroupName )
                                                , group.GroupLocation.GeoPoint.Latitude
                                                , group.GroupLocation.GeoPoint.Longitude
                                                , HttpUtility.HtmlEncode( infoWindow ) ) );
                    }
                    else
                    {
                        groupsWithNoGeo++;

                        if ( !string.IsNullOrWhiteSpace( groupDetailPage ) )
                        {
                            var pageParams = new Dictionary<string, string>();
                            pageParams.Add( "groupId", group.GroupId.ToString() );
                            var pageReference = new Rock.Web.PageReference( groupDetailPage, pageParams );
                            sbGroupsWithNoGeo.Append( String.Format( @"<li><a href='{0}'>{1}</a></li>", pageReference.BuildUrl(), group.GroupName ) );
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

                lGroupJson.Text = String.Format( @"<script>var groupData = JSON.parse('{{ ""groups"" : [ {0} ]}}'); var showInfoWindow = {1};</script>", groupJson, GetAttributeValue( "ShowMapInfoWindow" ).AsBoolean().ToString().ToLower() );

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