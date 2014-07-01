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
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Block that can be used to set the default group context for the site
    /// </summary>
    [DisplayName( "Groups Context Setter" )]
    [Category( "Core" )]
    [Description( "Block that can be used to set the default groups context for the site." )]

    [GroupTypeGroupField( "Group Filter", "Select group type and root group filter groups by root group. Leave root group blank to filter by group type." )]
    public partial class GroupContextSetter : RockBlock
    {
        #region Base Control Methods

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

            if ( !Page.IsPostBack )
            {
                LoadDropDowns();
            }
        }

        /// <summary>
        /// Loads the groups.
        /// </summary>
        private void LoadDropDowns()
        {
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

            var groupService = new GroupService( new RockContext() );

            string defaultGroupPublicKey = string.Empty;
            var contextCookie = Request.Cookies["Rock:context"];
            if ( contextCookie != null )
            {
                var cookieValue = contextCookie.Values[typeof( Rock.Model.Group ).FullName];

                string contextItem = Rock.Security.Encryption.DecryptString( cookieValue );
                string[] contextItemParts = contextItem.Split( '|' );
                if ( contextItemParts.Length == 2 )
                {
                    defaultGroupPublicKey = contextItemParts[1];
                }
            }

            var defaultGroup = groupService.GetByPublicKey( defaultGroupPublicKey );

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
                qryGroups = groupService.Queryable().Where( a => a.GroupType.Guid == groupTypeGuid.Value );
            }

            if ( qryGroups == null )
            {
                nbSelectGroupTypeWarning.Visible = true;
                ddlGroup.Visible = false;
            }
            else
            {
                nbSelectGroupTypeWarning.Visible = false;
                ddlGroup.Visible = true;
                ddlGroup.Items.Clear();
                var groups = qryGroups.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
                foreach ( var group in groups )
                {
                    var listItem = new ListItem( group.Name, HttpUtility.UrlDecode( group.ContextKey ) );
                    if ( defaultGroup != null )
                    {
                        listItem.Selected = group.Guid == defaultGroup.Guid;
                    }

                    ddlGroup.Items.Add( listItem );
                }
            }
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
        /// Handles the SelectedIndexChanged event of the ddlGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroup_SelectedIndexChanged( object sender, EventArgs e )
        {
            var contextCookie = Request.Cookies["Rock:context"];
            if ( contextCookie == null )
            {
                contextCookie = new HttpCookie( "Rock:context" );
            }

            contextCookie.Values[typeof( Rock.Model.Group ).FullName] = ddlGroup.SelectedValue;
            contextCookie.Expires = RockDateTime.Now.AddYears( 1 );

            Response.Cookies.Add( contextCookie );
        }

        #endregion
    }
}