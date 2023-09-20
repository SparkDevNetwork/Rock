// <copyright>
// Copyright by BEMA Software Services
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_lakepointe.Groups
{
    [DisplayName( "Group Role Changer" )]
    [Category( "LPC > Groups" )]
    [Description( "Tool to change group members from one role to another." )]
    [IntegerField(
        "Database Timeout",
        Key = AttributeKeys.DatabaseTimeoutSeconds,
        Description = "The number of seconds to wait before reporting a database timeout.",
        IsRequired = false,
        DefaultIntegerValue = 180,
        Order = 0 )]
    public partial class GroupRoleChanger : RockBlock
    {
        public string SOURCE_OF_CHANGE = "Group Role Changer";

        private static class AttributeKeys
        {
            public const string DatabaseTimeoutSeconds = "DatabaseTimeoutSeconds";
        }

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            //// Set postback timeout and request-timeout to whatever the DatabaseTimeout is plus an extra 5 seconds so that page doesn't timeout before the database does
            int databaseTimeout = GetAttributeValue( AttributeKeys.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180;
            var sm = ScriptManager.GetCurrent( this.Page );
            if ( sm.AsyncPostBackTimeout < databaseTimeout + 5 )
            {
                sm.AsyncPostBackTimeout = databaseTimeout + 5;
                Server.ScriptTimeout = databaseTimeout + 5;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            nbSuccess.Visible = false;

            if ( !Page.IsPostBack )
            {
                BindGroupTypeDropDown();
            }
        }

        /// <summary>
        /// Binds the group type drop down.
        /// </summary>
        private void BindGroupTypeDropDown()
        {
            var groupTypes = new GroupTypeService( new RockContext() ).Queryable()
                .Select( gt => new
                {
                    Id = gt.Id,
                    Name = gt.Name
                } )
                .OrderBy( gt => gt.Name )
                .ToList();

            groupTypes.Insert( 0, new { Id = 0, Name = "" } );

            ddlGroupTypes.DataSource = groupTypes;
            ddlGroupTypes.DataBind();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupTypes_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindRoles();
        }

        /// <summary>
        /// Binds the roles.
        /// </summary>
        private void BindRoles()
        {
            var groupTypeId = ddlGroupTypes.SelectedValueAsId();
            if ( groupTypeId.HasValue )
            {
                var groupType = new GroupTypeService( new RockContext() ).Get( groupTypeId.Value );
                if ( groupType != null )
                {
                    var groupTypeRoles = groupType.Roles.Select( gr => new
                    {
                        Id = gr.Id,
                        Name = gr.Name
                    } )
                        .OrderBy( r => r.Name )
                        .ToList();

                    groupTypeRoles.Insert( 0, new { Id = 0, Name = "" } );

                    ddlOriginalRole.DataSource = groupTypeRoles;
                    ddlNewRole.DataSource = groupTypeRoles;

                    ddlOriginalRole.DataBind();
                    ddlNewRole.DataBind();

                }

            }
        }

        /// <summary>
        /// Checks the roles.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool CheckRoles()
        {
            nbWarning.Visible = false;
            nbError.Visible = false;

            List<string> errorMessages = new List<string>();

            var oldRoleId = ddlOriginalRole.SelectedValueAsId();

            if ( !oldRoleId.HasValue )
            {
                return true;
            }



            var rockContext = new RockContext();
            var groupTypeService = new GroupTypeService( rockContext );
            var groupSyncService = new GroupSyncService( rockContext );
            var connectionOpportunityGroupConfigService = new ConnectionOpportunityGroupConfigService( rockContext );

            // Check for roles on connection requests
            var relatedOpportunityConfigs = connectionOpportunityGroupConfigService.Queryable().AsNoTracking().Where( cogc => cogc.GroupMemberRoleId == oldRoleId.Value ).ToList();
            foreach ( var relatedConfig in relatedOpportunityConfigs )
            {
                var errorMessage = String.Format( "The selected role is used in a connection opportunity group for <a href='/people/connections/types/{0}/opportunity/{1}'>{2}</a>", relatedConfig.ConnectionOpportunity.ConnectionTypeId, relatedConfig.ConnectionOpportunityId, relatedConfig.ConnectionOpportunity.Name );
                errorMessages.Add( errorMessage );
            }

            // Check for roles on group syncs
            var relatedGroupSyncs = groupSyncService.Queryable().AsNoTracking().Where( gs => gs.GroupTypeRoleId == oldRoleId.Value ).ToList();
            foreach ( var groupSync in relatedGroupSyncs )
            {
                var errorMessage = String.Format( "The selected role is used in a group sync for <a href='/people/groups?GroupId={0}'>{1}</a>", groupSync.GroupId, groupSync.Group.Name );
                errorMessages.Add( errorMessage );
            }

            var relatedGroupTypes = groupTypeService.Queryable().AsNoTracking().Where( a => a.DefaultGroupRoleId == oldRoleId.Value ).ToList();
            foreach ( var relatedGroupType in relatedGroupTypes )
            {
                var errorMessage = String.Format( "The selected role is the default role for the group type <a href='/admin/general/group-types/{0}'>{1}</a>", relatedGroupType.Id, relatedGroupType.Name );
                errorMessages.Add( errorMessage );
            }


            // Display any uses of the role
            if ( errorMessages.Any() )
            {
                StringBuilder sb = new StringBuilder();

                sb.Append( "<ul>" );
                foreach ( var errorMessage in errorMessages )
                {
                    sb.AppendFormat( "<li>{0}</li>", errorMessage );
                }
                sb.Append( "</ul>" );

                nbWarning.Title = "The following issues must be resolved before removing the specified role:";
                nbWarning.Text = sb.ToString();
                nbWarning.Visible = true;

                btnSave.Enabled = false;
                return false;
            }
            else
            {
                btnSave.Enabled = true;
                return true;
            }
        }
        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlOriginalRole control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlOriginalRole_SelectedIndexChanged( object sender, EventArgs e )
        {
            CheckRoles();

            var oldRoleId = ddlOriginalRole.SelectedValueAsId();
            if ( oldRoleId.HasValue )
            {
                var rockContext = new RockContext();
                var gmids = GetGroupMemberIds( oldRoleId, rockContext );
                var groupMemberCount = gmids.Count();
                var historicalGroupMemberCount = GetHistoricalIds( gmids, oldRoleId, rockContext ).Count();

                lMemberSummary.Text = String.Format( "{0} historical record(s)</br>{1} group member(s)", historicalGroupMemberCount, groupMemberCount );
            }
            else
            {
                lMemberSummary.Text = String.Empty;
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlNewRole control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlNewRole_SelectedIndexChanged( object sender, EventArgs e )
        {
            CheckRoles();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            nbError.Visible = false;

            var oldRoleId = ddlOriginalRole.SelectedValueAsId();
            var newRoleId = ddlNewRole.SelectedValueAsId();

            // Check Roles
            var areRolesValid = CheckRoles();
            if ( !areRolesValid )
            {
                return;
            }

            if ( !oldRoleId.HasValue )
            {
                nbError.Title = "Please select a role to remove.";
                nbError.Visible = true;
                return;
            }

            if ( !newRoleId.HasValue )
            {
                nbError.Title = "Please select a role to replace the one you're deleting.";
                nbError.Visible = true;
                return;
            }


            if ( oldRoleId == newRoleId )
            {
                nbError.Title = "Please select a different role to replace the one you're deleting.";
                nbError.Visible = true;
                return;
            }

            var databaseTimeoutSeconds = GetAttributeValue( AttributeKeys.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180;

            var readContext = new RockContext();
            var newRole = new GroupTypeRoleService( readContext ).Get( newRoleId.Value );
            List<int> groupMemberIds = GetGroupMemberIds( oldRoleId, readContext );
            List<int> historicalGroupMemberIds = GetHistoricalIds( groupMemberIds, oldRoleId, readContext );
            var existingGroupMembers = new GroupMemberService( readContext ).Queryable().AsNoTracking().Where( gm =>
                                gm.GroupRoleId == newRoleId.Value
                               ).ToList();

            try
            {
                // create new rock context for each record (https://weblog.west-wind.com/posts/2014/Dec/21/Gotcha-Entity-Framework-gets-slow-in-long-Iteration-Loops)

                // Update historical group members
                UpdateHistoricalMembers( newRole, databaseTimeoutSeconds, historicalGroupMemberIds, existingGroupMembers );

                // Get list of affected group members
                UpdateGroupMembers( newRoleId, databaseTimeoutSeconds, groupMemberIds, existingGroupMembers );

                // Delete Role
                DeleteRole( oldRoleId, databaseTimeoutSeconds );

                nbSuccess.Visible = true;
            }
            catch ( Exception ex )
            {
                nbError.Title = "An error has occurred updating roles";
                nbError.Text = ex.Message;
                nbError.Visible = true;
            }
        }

        private static List<int> GetGroupMemberIds( int? oldRoleId, RockContext readContext )
        {
            return new GroupMemberService( readContext ).Queryable( true, true ).AsNoTracking().Where( gm => gm.GroupRoleId == oldRoleId ).Select( gm => gm.Id ).ToList();
        }

        private static List<int> GetHistoricalIds( List<int> oldGroupMemberIds, int? oldRoleId, RockContext readContext )
        {
            var gmhs = new GroupMemberHistoricalService( readContext ).Queryable().AsNoTracking();
            var result = gmhs.Where( gmh => gmh.GroupRoleId == oldRoleId ).Select( gmh => gmh.Id ).ToList();
            result.AddRange( gmhs.Where( gmh => oldGroupMemberIds.Contains( gmh.GroupMemberId ) ).Select( gmh => gmh.Id ).ToList() );
            return result;
        }

        private void UpdateHistoricalMembers( GroupTypeRole newRole, int databaseTimeoutSeconds, List<int> historicalGroupMemberIds, List<GroupMember> existingGroupMembers )
        {
            foreach ( var historicalGroupMemberId in historicalGroupMemberIds )
            {
                RockContext updateContext = GetNewContext( databaseTimeoutSeconds );
                var groupMemberHistoricalService = new GroupMemberHistoricalService( updateContext );
                var groupMemberService = new GroupMemberService( updateContext );

                var historicalGroupMember = groupMemberHistoricalService.GetInclude( historicalGroupMemberId, hgm => hgm.GroupMember );
                if ( historicalGroupMember != null )
                {
                    var existingGroupMember = existingGroupMembers.Where( gm =>
                            gm.GroupId == historicalGroupMember.GroupMember.GroupId &&
                            gm.PersonId == historicalGroupMember.GroupMember.PersonId
                           )
                           .FirstOrDefault();

                    if ( existingGroupMember != null )
                    {
                        historicalGroupMember.GroupMemberId = existingGroupMember.Id;
                        historicalGroupMember.ForeignKey = String.Format( "GroupRoleChange:{0}", historicalGroupMember.GroupRoleName );
                    }

                    historicalGroupMember.GroupRoleId = newRole.Id;
                    historicalGroupMember.GroupRoleName = newRole.Name;
                    historicalGroupMember.CurrentRowIndicator = false;

                    updateContext.SaveChanges();
                }
            }
        }

        private void UpdateGroupMembers( int? newRoleId, int databaseTimeoutSeconds, List<int> groupMemberIds, List<GroupMember> existingGroupMembers )
        {
            foreach ( var groupMemberId in groupMemberIds )
            {
                RockContext updateContext = GetNewContext( databaseTimeoutSeconds );
                var groupMemberService = new GroupMemberService( updateContext );
                var groupMember = groupMemberService.Get( groupMemberId );
                if ( groupMember != null )
                {
                    var existingGroupMember = existingGroupMembers.Where( gm =>
                         gm.GroupId == groupMember.GroupId &&
                         gm.PersonId == groupMember.PersonId
                        )
                        .FirstOrDefault();

                    if ( existingGroupMember != null )
                    {
                        groupMemberService.Delete( groupMember );
                    }
                    else
                    {
                        groupMember.GroupRoleId = newRoleId.Value;
                    }
                }
                updateContext.SaveChanges();
            }
        }

        private void DeleteRole( int? oldRoleId, int databaseTimeoutSeconds )
        {
            RockContext updateContext = GetNewContext( databaseTimeoutSeconds );
            var groupTypeRoleService = new GroupTypeRoleService( updateContext );
            var role = groupTypeRoleService.Get( oldRoleId.Value );
            if ( role != null )
            {
                var errorMessage = "";
                if ( !groupTypeRoleService.CanDelete( role, out errorMessage ) )
                {
                    throw new Exception( errorMessage );
                }
                else
                {
                    groupTypeRoleService.Delete( role );
                }
            }
            updateContext.SaveChanges();
        }

        private RockContext GetNewContext( int databaseTimeoutSeconds )
        {
            RockContext updateContext = new RockContext();
            updateContext.SourceOfChange = SOURCE_OF_CHANGE;
            updateContext.Database.CommandTimeout = databaseTimeoutSeconds;
            return updateContext;
        }
    }
}