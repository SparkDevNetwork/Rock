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
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tasks
{
    /// <summary>
    /// Launches a group member change workflow. Will also recalculate group member requirements when a member is added
    /// </summary>
    public sealed class UpdateGroupMember : BusStartedTask<UpdateGroupMember.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            // if a GroupMember is getting added, call CalculateRequirements to make sure that group member requirements are calculated (if the group has requirements)
            if ( message.State == EntityContextState.Added || ( message.PreviousIsArchived && message.IsArchived != message.PreviousIsArchived ) )
            {
                if ( message.GroupMemberGuid.HasValue )
                {
                    // Allow an extended timeout for database access, to cater for situations where there is a large queue
                    // of these tasks to process and we may have to wait.
                    var rockContext = new RockContext();
                    rockContext.Database.CommandTimeout = 180;

                    var groupMember = new GroupMemberService( rockContext ).Get( message.GroupMemberGuid.Value );
                    if ( groupMember != null )
                    {
                        groupMember.CalculateRequirements( rockContext, true );
                    }
                }
            }

            GroupMemberWorkflowTriggerType[] groupMemberWorkflowChangeTriggers = new GroupMemberWorkflowTriggerType[] { GroupMemberWorkflowTriggerType.MemberAddedToGroup, GroupMemberWorkflowTriggerType.MemberRemovedFromGroup, GroupMemberWorkflowTriggerType.MemberStatusChanged, GroupMemberWorkflowTriggerType.MemberRoleChanged };

            // Verify that valid ids were saved
            if ( message.GroupId.HasValue && message.PersonId.HasValue )
            {
                // Get all the triggers from cache
                var cachedTriggers = GroupMemberWorkflowTriggerService.GetCachedTriggers();

                // If any triggers exist
                if ( cachedTriggers != null && cachedTriggers.Any() )
                {
                    // Get the ACTIVE group member triggers associated to the group 
                    var groupTriggers = cachedTriggers
                        .Where( w =>
                            groupMemberWorkflowChangeTriggers.Contains( w.TriggerType ) &&
                            w.IsActive &&
                            w.GroupId.HasValue &&
                            w.GroupId.Value == message.GroupId.Value )
                        .OrderBy( w => w.Order )
                        .ToList();

                    // Get any ACTIVE triggers associated to a group type ( if any are found, will then filter by group type )
                    var groupTypeTriggers = cachedTriggers
                        .Where( w =>
                            groupMemberWorkflowChangeTriggers.Contains( w.TriggerType ) &&
                            w.IsActive &&
                            w.GroupTypeId.HasValue )
                        .OrderBy( w => w.Order )
                        .ToList();

                    if ( groupTriggers.Any() || groupTypeTriggers.Any() )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            // If there were any group type triggers, will now need to read the group's group type id
                            // and then further filter these triggers by the current transaction's group type
                            if ( groupTypeTriggers.Any() )
                            {
                                // Get the current transaction's group type id
                                if ( !message.GroupTypeId.HasValue )
                                {
                                    message.GroupTypeId = new GroupService( rockContext )
                                        .Queryable().AsNoTracking()
                                        .Where( g => g.Id == message.GroupId.Value )
                                        .Select( g => g.GroupTypeId )
                                        .FirstOrDefault();
                                }

                                // Further filter the group type triggers by the group type id
                                groupTypeTriggers = groupTypeTriggers
                                    .Where( t =>
                                        t.GroupTypeId.HasValue &&
                                        t.GroupTypeId.Equals( message.GroupTypeId ) )
                                    .OrderBy( t => t.Order )
                                    .ToList();
                            }

                            // Combine group and group type triggers
                            var triggers = groupTriggers.Union( groupTypeTriggers ).ToList();

                            // If any triggers were found
                            if ( triggers.Any() )
                            {
                                // Loop through triggers and launch appropriate workflow
                                foreach ( var trigger in triggers )
                                {
                                    switch ( trigger.TriggerType )
                                    {
                                        case GroupMemberWorkflowTriggerType.MemberAddedToGroup:
                                            {
                                                if ( message.State == EntityContextState.Added && QualifiersMatch( rockContext, trigger, message.GroupMemberStatus, message.GroupMemberStatus, message.GroupMemberRoleId, message.GroupMemberRoleId ) )
                                                {
                                                    LaunchWorkflow( rockContext, trigger, message );
                                                }

                                                break;
                                            }

                                        case GroupMemberWorkflowTriggerType.MemberRemovedFromGroup:
                                            {
                                                if ( message.State == EntityContextState.Deleted && QualifiersMatch( rockContext, trigger, message.GroupMemberStatus, message.GroupMemberStatus, message.GroupMemberRoleId, message.GroupMemberRoleId ) )
                                                {
                                                    LaunchWorkflow( rockContext, trigger, message );
                                                }

                                                break;
                                            }

                                        case GroupMemberWorkflowTriggerType.MemberRoleChanged:
                                            {
                                                if ( message.State == EntityContextState.Modified && message.PreviousGroupMemberRoleId != message.GroupMemberRoleId && QualifiersMatch( rockContext, trigger, message.PreviousGroupMemberRoleId, message.GroupMemberRoleId ) )
                                                {
                                                    LaunchWorkflow( rockContext, trigger, message );
                                                }

                                                break;
                                            }

                                        case GroupMemberWorkflowTriggerType.MemberStatusChanged:
                                            {
                                                if ( message.State == EntityContextState.Modified && message.PreviousGroupMemberStatus != message.GroupMemberStatus && QualifiersMatch( rockContext, trigger, message.PreviousGroupMemberStatus, message.GroupMemberStatus ) )
                                                {
                                                    LaunchWorkflow( rockContext, trigger, message );
                                                }

                                                break;
                                            }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool QualifiersMatch( RockContext rockContext, GroupMemberWorkflowTrigger workflowTrigger, GroupMemberStatus prevStatus, GroupMemberStatus status, int prevRoleId, int roleId )
        {
            return QualifiersMatch( rockContext, workflowTrigger, prevStatus, status ) && QualifiersMatch( rockContext, workflowTrigger, prevRoleId, roleId );
        }

        private bool QualifiersMatch( RockContext rockContext, GroupMemberWorkflowTrigger workflowTrigger, GroupMemberStatus prevStatus, GroupMemberStatus status )
        {
            var qualifierParts = ( workflowTrigger.TypeQualifier ?? string.Empty ).Split( new char[] { '|' } );

            bool matches = true;

            if ( matches && qualifierParts.Length > 0 && !string.IsNullOrWhiteSpace( qualifierParts[0] ) )
            {
                matches = qualifierParts[0].AsInteger() == status.ConvertToInt();
            }

            if ( matches && qualifierParts.Length > 2 && !string.IsNullOrWhiteSpace( qualifierParts[2] ) )
            {
                matches = qualifierParts[2].AsInteger() == prevStatus.ConvertToInt();
            }

            return matches;
        }

        private bool QualifiersMatch( RockContext rockContext, GroupMemberWorkflowTrigger workflowTrigger, int prevRoleId, int roleId )
        {
            var qualifierParts = ( workflowTrigger.TypeQualifier ?? string.Empty ).Split( new char[] { '|' } );

            bool matches = true;

            if ( matches && qualifierParts.Length > 1 && !string.IsNullOrWhiteSpace( qualifierParts[1] ) )
            {
                var guid = qualifierParts[1].AsGuidOrNull();
                if ( guid.HasValue )
                {
                    var qualifierRoleId = new GroupTypeRoleService( rockContext ).Queryable().AsNoTracking()
                        .Where( r => r.Guid.Equals( guid.Value ) )
                        .Select( r => r.Id )
                        .FirstOrDefault();
                    matches = qualifierRoleId != 0 && qualifierRoleId == roleId;
                }
                else
                {
                    matches = false;
                }
            }

            if ( matches && qualifierParts.Length > 3 && !string.IsNullOrWhiteSpace( qualifierParts[3] ) )
            {
                var guid = qualifierParts[3].AsGuidOrNull();
                if ( guid.HasValue )
                {
                    var qualifierRoleId = new GroupTypeRoleService( rockContext ).Queryable().AsNoTracking()
                        .Where( r => r.Guid.Equals( guid.Value ) )
                        .Select( r => r.Id )
                        .FirstOrDefault();
                    matches = qualifierRoleId != 0 && qualifierRoleId == prevRoleId;
                }
                else
                {
                    matches = false;
                }
            }

            return matches;
        }

        private void LaunchWorkflow( RockContext rockContext, GroupMemberWorkflowTrigger groupMemberWorkflowTrigger, Message message )
        {
            GroupMember groupMember = null;
            if ( message.GroupMemberGuid.HasValue )
            {
                groupMember = new GroupMemberService( rockContext ).Get( message.GroupMemberGuid.Value );
            }

            var workflowType = WorkflowTypeCache.Get( groupMemberWorkflowTrigger.WorkflowTypeId );
            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                var workflow = Rock.Model.Workflow.Activate( workflowType, groupMemberWorkflowTrigger.Name );

                if ( workflow.AttributeValues != null )
                {
                    if ( workflow.AttributeValues.ContainsKey( "Group" ) )
                    {
                        var group = new GroupService( rockContext ).Get( message.GroupId.Value );
                        if ( group != null )
                        {
                            workflow.AttributeValues["Group"].Value = group.Guid.ToString();
                        }
                    }

                    if ( workflow.AttributeValues.ContainsKey( "Person" ) )
                    {
                        var person = new PersonService( rockContext ).Get( message.PersonId.Value );
                        if ( person != null )
                        {
                            workflow.AttributeValues["Person"].Value = person.PrimaryAlias.Guid.ToString();
                        }
                    }
                }

                List<string> workflowErrors;
                new Rock.Model.WorkflowService( rockContext ).Process( workflow, groupMember, out workflowErrors );
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the entity state.
            /// </summary>
            /// <value>
            /// The entity state.
            /// </value>
            public EntityContextState State { get; set; }

            /// <summary>
            /// Gets or sets the group member unique identifier.
            /// </summary>
            /// <value>
            /// The group member unique identifier.
            /// </value>
            public Guid? GroupMemberGuid { get; set; }

            /// <summary>
            /// Gets or sets the group type identifier.
            /// </summary>
            /// <value>
            /// The group type identifier.
            /// </value>
            public int? GroupTypeId { get; set; }

            /// <summary>
            /// Gets or sets the group identifier.
            /// </summary>
            /// <value>
            /// The group identifier.
            /// </value>
            public int? GroupId { get; set; }

            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int? PersonId { get; set; }

            /// <summary>
            /// Gets or sets the group member status.
            /// </summary>
            /// <value>
            /// The group member status.
            /// </value>
            public GroupMemberStatus GroupMemberStatus { get; set; }

            /// <summary>
            /// Gets or sets the group member role identifier.
            /// </summary>
            /// <value>
            /// The group member role identifier.
            /// </value>
            public int GroupMemberRoleId { get; set; }

            /// <summary>
            /// Gets or sets the previous group member status.
            /// </summary>
            /// <value>
            /// The previous group member status.
            /// </value>
            public GroupMemberStatus PreviousGroupMemberStatus { get; set; }

            /// <summary>
            /// Gets or sets the previous group member role identifier.
            /// </summary>
            /// <value>
            /// The previous group member role identifier.
            /// </value>
            public int PreviousGroupMemberRoleId { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this group member is archived (soft deleted)
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is archived; otherwise, <c>false</c>.
            /// </value>
            public bool IsArchived { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this group member is previous archived (soft deleted)
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is previous archived; otherwise, <c>false</c>.
            /// </value>
            public bool PreviousIsArchived { get; set; }
        }
    }
}