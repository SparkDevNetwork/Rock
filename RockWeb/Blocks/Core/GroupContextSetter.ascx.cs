﻿// <copyright>
// Copyright by the Spark Development Network
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
using System.Web;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Block that can be used to set the default group context for the site or page
    /// </summary>
    [DisplayName( "Group Context Setter" )]
    [Category( "Core" )]
    [Description( "Block that can be used to set the default group context for the site or page." )]
    [GroupTypeGroupField( "Group Filter", "Select group type and root group to filter groups by root group. Leave root group blank to filter by group type.", "Root Group", order: 0 )]
    [CustomRadioListField( "Context Scope", "The scope of context to set", "Site,Page", true, "Site", order: 1 )]
    [TextField( "No Group Text", "The text to show when there is no group in the context.", true, "Select Group", order: 2 )]
    [TextField( "Clear Selection Text", "The text displayed when a group can be unselected. This will not display when the text is empty.", true, "", order: 3 )]
    [BooleanField( "Display Query Strings", "Select to always display query strings. Default behavior will only display the query string when it's passed to the page.", false, "", order: 4 )]
    [BooleanField( "Include GroupType Children", "Include all children of the grouptype selected", false, "", order: 5 )]
    public partial class GroupContextSetter : RockBlock
    {
        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // repaint the screen after block settings are updated
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

            LoadDropDowns();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadDropDowns();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the groups.
        /// </summary>
        private void LoadDropDowns()
        {
            var groupEntityType = EntityTypeCache.Read( typeof( Group ) );
            var currentGroup = RockPage.GetCurrentContext( groupEntityType ) as Group;

            var groupIdString = Request.QueryString["groupId"];
            if ( groupIdString != null )
            {
                var groupId = groupIdString.AsInteger();

                if ( currentGroup == null || currentGroup.Id != groupId )
                {
                    currentGroup = SetGroupContext( groupId, false );
                }
            }

            var parts = ( GetAttributeValue( "GroupFilter" ) ?? string.Empty ).Split( '|' );
            Guid? groupTypeGuid = null;
            Guid? rootGroupGuid = null;

            if ( parts.Length >= 1 )
            {
                groupTypeGuid = parts[0].AsGuidOrNull();
                if ( parts.Length >= 2 )
                {
                    rootGroupGuid = parts[1].AsGuidOrNull();
                }
            }

            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var groupTypeService = new GroupTypeService( rockContext );
            IQueryable<Group> qryGroups = null;

            // if rootGroup is set, use that as the filter.  Otherwise, use GroupType as the filter
            if ( rootGroupGuid.HasValue )
            {
                var rootGroup = groupService.Get( rootGroupGuid.Value );
                if ( rootGroup != null )
                {
                    qryGroups = groupService.GetAllDescendents( rootGroup.Id ).AsQueryable();
                }
            }
            else if ( groupTypeGuid.HasValue )
            {
                SetGroupTypeContext( groupTypeGuid );

                if ( GetAttributeValue( "IncludeGroupTypeChildren" ).AsBoolean() )
                {
                    var childGroupTypeGuids = groupTypeService.Queryable().Where( t => t.ParentGroupTypes.Select( p => p.Guid ).Contains( groupTypeGuid.Value ) )
                        .Select( t => t.Guid ).ToList();

                    qryGroups = groupService.Queryable().Where( a => childGroupTypeGuids.Contains( a.GroupType.Guid ) );
                }
                else
                {
                    qryGroups = groupService.Queryable().Where( a => a.GroupType.Guid == groupTypeGuid.Value );
                }
            }

            // no results
            if ( qryGroups == null )
            {
                nbSelectGroupTypeWarning.Visible = true;
                lCurrentSelection.Text = string.Empty;
                rptGroups.Visible = false;
            }
            else
            {
                nbSelectGroupTypeWarning.Visible = false;
                rptGroups.Visible = true;

                lCurrentSelection.Text = currentGroup != null ? currentGroup.ToString() : GetAttributeValue( "NoGroupText" );

                var groupList = qryGroups.OrderBy( a => a.Name ).ToList()
                    .Select( a => new GroupItem() { Name = a.Name, Id = a.Id } )
                    .ToList();

                // check if the group can be unselected
                if ( !string.IsNullOrEmpty( GetAttributeValue( "ClearSelectionText" ) ) )
                {
                    var blankGroup = new GroupItem
                    {
                        Name = GetAttributeValue( "ClearSelectionText" ),
                        Id = Rock.Constants.All.Id
                    };

                    groupList.Insert( 0, blankGroup );
                }

                rptGroups.DataSource = groupList;
                rptGroups.DataBind();
            }
        }

        /// <summary>
        /// Sets the group context.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="refreshPage">if set to <c>true</c> [refresh page].</param>
        /// <returns></returns>
        protected Group SetGroupContext( int groupId, bool refreshPage = false )
        {
            bool pageScope = GetAttributeValue( "ContextScope" ) == "Page";
            var group = new GroupService( new RockContext() ).Get( groupId );
            if ( group == null )
            {
                // clear the current group context
                group = new Group()
                {
                    Name = GetAttributeValue( "NoGroupText" ),
                    Guid = Guid.Empty
                };
            }

            // set context and refresh below with the correct query string if needed
            RockPage.SetContextCookie( group, pageScope, false );

            if ( refreshPage )
            {
                // Only redirect if refreshPage is true
                if ( !string.IsNullOrWhiteSpace( PageParameter( "groupId" ) ) || GetAttributeValue( "DisplayQueryStrings" ).AsBoolean() )
                {
                    var queryString = HttpUtility.ParseQueryString( Request.QueryString.ToStringSafe() );
                    queryString.Set( "groupId", groupId.ToString() );
                    Response.Redirect( string.Format( "{0}?{1}", Request.Url.AbsolutePath, queryString ), false );
                }
                else
                {
                    Response.Redirect( Request.RawUrl, false );
                }

                Context.ApplicationInstance.CompleteRequest();
            }

            return group;
        }

        /// <summary>
        /// Sets the group type context.
        /// </summary>
        /// <param name="groupTypeGuid">The group type unique identifier.</param>
        /// <returns></returns>
        protected GroupType SetGroupTypeContext( Guid? groupTypeGuid )
        {
            bool pageScope = GetAttributeValue( "ContextScope" ) == "Page";
            var groupTypeService = new GroupTypeService( new RockContext() );

            // check if a grouptype parameter exists to set
            var groupType = groupTypeService.Get( (Guid)groupTypeGuid );

            if ( groupType == null )
            {
                groupType = new GroupType()
                {
                    Name = GetAttributeValue( "NoGroupText" ),
                    Guid = Guid.Empty
                };
            }

            RockPage.SetContextCookie( groupType, pageScope, false );

            return groupType;
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptGroups control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptGroups_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var groupId = e.CommandArgument.ToString();

            if ( groupId != null )
            {
                SetGroupContext( groupId.AsInteger(), true );
            }
        }

        #endregion

        /// <summary>
        /// Schedule Item
        /// </summary>
        public class GroupItem
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }
        }
    }
}