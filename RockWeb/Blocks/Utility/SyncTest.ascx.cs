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

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Sync Test" )]
    [Category( "Utility" )]
    public partial class SyncTest : Rock.Web.UI.RockBlock
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

            if ( !Page.IsPostBack )
            {
                // added for your convenience
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

        }

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)

        #endregion
        protected void btnGo_Click( object sender, EventArgs e )
        {
            int targetIndex = 0;

            try
            {
                // get groups set to sync
                RockContext rockContext = new RockContext();
                var groupsThatSync = new GroupService( rockContext ).Queryable("Members").Where( g => g.SyncDataViewId != null ).ToList();

                GroupMemberService groupMemberService = new GroupMemberService( rockContext );

                foreach ( var syncGroup in groupsThatSync )
                {
                    var syncSource = new DataViewService( rockContext ).Get( syncGroup.SyncDataViewId.Value );

                    // ensure this is a person dataview
                    bool isPersonDataSet = syncSource.EntityTypeId == EntityTypeCache.Read( typeof( Rock.Model.Person ) ).Id;

                    if ( isPersonDataSet )
                    {
                        SortProperty sortById = new SortProperty();
                        sortById.Property = "Id";
                        sortById.Direction = System.Web.UI.WebControls.SortDirection.Ascending;
                        List<string> errorMessages = new List<string>();

                        var sourceItems = syncSource.GetQuery( sortById, 180, out errorMessages ).Select(q =>q.Id).ToList();
                        var targetItems = syncGroup.Members.OrderBy( g => g.Id ).Select(g => g.PersonId).ToList();
                        var targetGroupMembers = syncGroup.Members;
                        bool targetDone = false;

                        // start sync'ing items
                        foreach ( var sourceItem in sourceItems )
                        {
                            if ( targetItems.Count <= targetIndex )
                            {
                                targetDone = true;
                            }

                            if ( (! targetDone)  && (sourceItem > targetItems[targetIndex]) )
                            {
                                // remove target item
                                groupMemberService.Delete( targetGroupMembers.Where( g => g.PersonId == targetItems[targetIndex] ).First() );
                                targetIndex++;
                            }
                            else if ( targetDone || sourceItem < targetItems[targetIndex] )
                            {
                                // add source to target
                                var newGroupMember = new GroupMember { Id = 0 };
                                newGroupMember.PersonId = sourceItem;
                                newGroupMember.Group = syncGroup;
                                newGroupMember.GroupMemberStatus = GroupMemberStatus.Active;
                                newGroupMember.GroupRoleId = syncGroup.GroupType.DefaultGroupRoleId ?? syncGroup.GroupType.Roles.FirstOrDefault().Id;
                                groupMemberService.Add( newGroupMember );
                                
                            }
                            else
                            {
                                // already in source and target 
                                targetIndex++;
                            }
                        }

                        // remove any remaining items from the target
                        for ( int i = targetIndex; i < targetItems.Count; i++ )
                        {
                            groupMemberService.Delete( targetGroupMembers.Where( g => g.PersonId == targetItems[i] ).First() );
                        }

                        rockContext.SaveChanges();
                    }
                }
            }
            catch ( Exception ex )
            {
                string test = "";
            }
        }
    }
}