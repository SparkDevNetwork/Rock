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

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Group Mapper" )]
    [Category( "Groups" )]
    [Description( "Displays groups on a map." )]
    [GroupTypeField( "Group Type", "The type of group to map.", true )]
    [DefinedValueField( "2E68D37C-FB7B-4AA5-9E09-3785D52156CB", "Location Type", "The location type to use for the map." )]
    [LinkedPage("Group Detail Page", "Page to use as a link to the person details (optional).", false)]
    [IntegerField("Map Height", "Height of the map in pixels (default value is 600px)", false, 600)]
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
                int groupsMapped = 0;
                int groupsWithNoGeo = 0;

                StringBuilder sbGroupJson = new StringBuilder();
                StringBuilder sbGroupsWithNoGeo = new StringBuilder();

                Guid groupType = new Guid( GetAttributeValue( "GroupType" ) );
                Guid locationType = new Guid( GetAttributeValue( "LocationType" ) );

                GroupService groupService = new GroupService();
                var groups = groupService.Queryable()
                    .Where( g => g.GroupType.Guid == groupType )
                    .Select( g => new
                    {
                        g.Id,
                        g.Name,
                        GroupLocation = g.GroupLocations.Where( l => l.GroupLocationTypeValue.Guid == locationType ).FirstOrDefault()
                    } );


                foreach ( var group in groups )
                {
                    if ( group.GroupLocation != null && group.GroupLocation.Location.GeoPoint != null )
                    {
                        groupsMapped++;
                        sbGroupJson.Append( String.Format( @"{{ ""name"":""{0}"" , ""latitude"":""{1}"", ""longitude"":""{2}"" }},", HttpUtility.HtmlEncode( group.Name ), group.GroupLocation.Location.GeoPoint.Latitude, group.GroupLocation.Location.GeoPoint.Longitude ) );
                    }
                    else
                    {
                        groupsWithNoGeo++;

                        if ( !string.IsNullOrWhiteSpace( groupDetailPage ) )
                        {
                            var pageParams = new Dictionary<string, string>();
                            pageParams.Add( "groupId", group.Id.ToString() );
                            var pageReference = new Rock.Web.PageReference( groupDetailPage, pageParams );
                            sbGroupsWithNoGeo.Append( String.Format( @"<li><a href='{0}'>{1}</a></li>", pageReference.BuildUrl(), group.Name ) );
                        }
                        else
                        {
                            sbGroupsWithNoGeo.Append( String.Format( @"<li>{0}</li>", group.Name ) );
                        }
                    }
                }

                string groupJson = sbGroupJson.ToString();

                // remove last comma
                if ( groupJson.Length > 0 )
                {
                    groupJson = groupJson.Substring( 0, groupJson.Length - 1 );
                }

                lGroupJson.Text = String.Format( @"<script>var groupData = JSON.parse('{{ ""groups"" : [ {0} ]}}');</script>", groupJson );

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
               
                lMessages.Text = "<div class='alert alert-warning'><strong>Group Mapper</strong> Please configure a group type to display and a location type to use.</div>";
               
            }
        }

        #endregion
    }
}