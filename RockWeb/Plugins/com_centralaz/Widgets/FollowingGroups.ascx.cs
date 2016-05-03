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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Entity;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Text;
using Rock.Security;
using System.Reflection;

namespace RockWeb.Plugins.com_centralaz.Widgets
{
    /// <summary>
    /// Takes a defined type and returns all defined values and merges them with a liquid template
    /// </summary>
    [DisplayName( "Following Groups" )]
    [Category( "com_centralaz > Widgets" )]
    [Description( "Displays groups that the user is following." )]
    [CustomRadioListField( "Sort Property", "How to sort ", "1^GroupType then Campus, 2^Campus then GroupType", true, "1" )]
    [TextField( "Link URL", "The address to use for the link. The text '[Id]' will be replaced with the Id of the entity '[Guid]' will be replaced with the Guid.", false, "/samplepage/[Id]", "", 1, "LinkUrl" )]
    [CodeEditorField( "Lava Template", "Lava template to use to display content", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~/Plugins/com_centralaz/Widgets/Lava/FollowingGroups.lava' %}", "", 2, "LavaTemplate" )]
    [BooleanField( "Enable Debug", "Show merge data to help you see what's available to you.", order: 3 )]
    [IntegerField( "Max Results", "The maximum number of results to display.", true, 100, order: 4 )]
    public partial class FollowingGroups : Rock.Web.UI.RockBlock
    {
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

            if ( !Page.IsPostBack )
            {
                LoadContent();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadContent();
        }

        #endregion

        #region Methods

        protected void LoadContent()
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );

            var entityType = EntityTypeCache.Read( Rock.SystemGuid.EntityType.GROUP.AsGuid() );

            if ( entityType != null )
            {
                string sortProperty = GetAttributeValue( "SortProperty" );
                RockContext rockContext = new RockContext();

                int entityTypeId = EntityTypeCache.GetId<Group>().Value;
                int personId = this.CurrentPersonId.Value;

                var followingService = new FollowingService( rockContext );
                IQueryable<IEntity> qryFollowedItems = followingService.GetFollowedItems( entityTypeId, personId );
                List<Group> groupList = new List<Group>();
                foreach ( var item in qryFollowedItems )
                {
                    groupList.Add( item as Group );
                }

                if ( sortProperty == "1" )
                {
                    groupList.OrderBy( g => g.GroupTypeId ).ThenByDescending( g => g.CampusId.HasValue ).ThenBy(g=> g.CampusId);
                }
                else if ( sortProperty == "2" )
                {
                    groupList.OrderByDescending( g => g.CampusId.HasValue ).ThenBy( g => g.CampusId ).ThenBy( g => g.GroupTypeId );
                }

                mergeFields.Add( "FollowingItems", qryFollowedItems );
                mergeFields.Add( "SortProperty", sortProperty );
                mergeFields.Add( "EntityType", entityType.FriendlyName );
                mergeFields.Add( "LinkUrl", GetAttributeValue( "LinkUrl" ) );

                string template = GetAttributeValue( "LavaTemplate" );
                lContent.Text = template.ResolveMergeFields( mergeFields );

                // show debug info
                if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
                {
                    lDebug.Visible = true;
                    lDebug.Text = mergeFields.lavaDebugInfo();
                }
            }
            else
            {
                lContent.Text = string.Format( "<div class='alert alert-warning'>Please configure an entity in the block settings." );

            }
        }

        #endregion
    }
}