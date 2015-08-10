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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Launches a group member change workflow. Will also recalculate group member requirements when a member is added
    /// </summary>
    public class GroupMemberChangeTransaction : ITransaction
    {
        private EntityState State;
        private Guid? GroupMemberGuid;
        private int? GroupTypeId;
        private int? GroupId;
        private int? PersonId;
        private GroupMemberStatus GroupMemberStatus;
        private int GroupMemberRoleId;
        private GroupMemberStatus PreviousGroupMemberStatus;
        private int PreviousGroupMemberRoleId;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberChangeTransaction"/> class.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public GroupMemberChangeTransaction( DbEntityEntry entry )
        {
            // If entity was a group member, save the values
            var groupMember = entry.Entity as GroupMember;
            if ( groupMember != null )
            {
                State = entry.State;
                GroupId = groupMember.GroupId;
                PersonId = groupMember.PersonId;
                GroupMemberStatus = groupMember.GroupMemberStatus;
                GroupMemberRoleId = groupMember.GroupRoleId;

                if ( groupMember.Group != null )
                {
                    GroupTypeId = groupMember.Group.GroupTypeId;
                }

                // If this isn't a new group member, get the previous status and role values
                if ( State != EntityState.Added )
                {
                    var dbStatusProperty = entry.Property( "GroupMemberStatus" );
                    if ( dbStatusProperty != null )
                    {
                        PreviousGroupMemberStatus = (GroupMemberStatus)dbStatusProperty.OriginalValue;
                    }
                    var dbRoleProperty = entry.Property( "GroupRoleId" );
                    if ( dbRoleProperty != null )
                    {
                        PreviousGroupMemberRoleId = (int)dbRoleProperty.OriginalValue;
                    }
                }

                // If this isn't a deleted group member, get the group member guid
                if ( State != EntityState.Deleted )
                {
                    GroupMemberGuid = groupMember.Guid;
                }
            }
        }

        /// <summary>
        /// Execute method to check for any workflows to launch and will also recalculate group member requirements when a member is added
        /// </summary>
        public void Execute()
        {
            // if a GroupMember is getting added, call CalculateRequirements to make sure that group member requirements are calculated (if the group has requirements)
            if ( State == EntityState.Added )
            {
                if ( GroupMemberGuid.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var groupMember = new GroupMemberService( rockContext ).Get( GroupMemberGuid.Value );
                        groupMember.CalculateRequirements( rockContext, true );
                    }
                }
            }

            GroupMemberWorkflowTriggerType[] groupMemberWorkflowChangeTriggers = new GroupMemberWorkflowTriggerType[] { GroupMemberWorkflowTriggerType.MemberAddedToGroup, GroupMemberWorkflowTriggerType.MemberRemovedFromGroup, GroupMemberWorkflowTriggerType.MemberStatusChanged, GroupMemberWorkflowTriggerType.MemberRoleChanged };

            // Verify that valid ids were saved
            if ( GroupId.HasValue && PersonId.HasValue )
            {
                // Get all the triggers from cache
                var cachedTriggers = GroupMemberWorkflowTriggerService.GetCachedTriggers();

                // If any triggers exist
                if ( cachedTriggers != null && cachedTriggers.Any() )
                {
                    // Get the triggers associated to the group 
                    var groupTriggers = cachedTriggers
                        .Where( w =>
                            groupMemberWorkflowChangeTriggers.Contains( w.TriggerType ) &&
                            w.GroupId.HasValue &&
                            w.GroupId.Value == GroupId.Value )
                        .OrderBy( w => w.Order )
                        .ToList();

                    // Get any triggers associated to a group type ( if any are found, will then filter by group type )
                    var groupTypeTriggers = cachedTriggers
                        .Where( w =>
                            groupMemberWorkflowChangeTriggers.Contains( w.TriggerType ) &&
                            w.GroupTypeId.HasValue )
                        .OrderBy( w => w.Order )
                        .ToList();

                    if ( groupTriggers.Any() || groupTypeTriggers.Any() )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            // If there were any group type triggers, will now need to read the group's group type id
                            // and then further filter these triggers by the current txn's group type
                            if ( groupTypeTriggers.Any() )
                            {
                                // Get the current txn's group type id
                                if ( !GroupTypeId.HasValue )
                                {
                                    GroupTypeId = new GroupService( rockContext )
                                        .Queryable().AsNoTracking()
                                        .Where( g => g.Id == GroupId.Value )
                                        .Select( g => g.GroupTypeId )
                                        .FirstOrDefault();
                                }

                                // Further filter the group type triggers by the group type id
                                groupTypeTriggers = groupTypeTriggers
                                    .Where( t =>
                                        t.GroupTypeId.HasValue &&
                                        t.GroupTypeId.Equals( GroupTypeId ) )
                                    .OrderBy( t => t.Order )
                                    .ToList();
                            }

                            // Combine group and grouptype trigers
                            var triggers = groupTriggers.Union( groupTypeTriggers ).ToList();

                            // If any triggers were found
                            if ( triggers.Any() )
                            {
                                // Loop through triggers and lauch appropriate workflow
                                foreach ( var trigger in triggers )
                                {
                                    switch ( trigger.TriggerType )
                                    {
                                        case GroupMemberWorkflowTriggerType.MemberAddedToGroup:
                                            {
                                                if ( State == EntityState.Added && QualifiersMatch( rockContext, trigger, GroupMemberStatus, GroupMemberStatus, GroupMemberRoleId, GroupMemberRoleId ) )
                                                {
                                                    LaunchWorkflow( rockContext, trigger.WorkflowTypeId, trigger.Name );
                                                }
                                                break;
                                            }
                                        case GroupMemberWorkflowTriggerType.MemberRemovedFromGroup:
                                            {
                                                if ( State == EntityState.Deleted && QualifiersMatch( rockContext, trigger, PreviousGroupMemberStatus, PreviousGroupMemberStatus, PreviousGroupMemberRoleId, PreviousGroupMemberRoleId ) )
                                                {
                                                    LaunchWorkflow( rockContext, trigger.WorkflowTypeId, trigger.Name );
                                                }
                                                break;
                                            }
                                        case GroupMemberWorkflowTriggerType.MemberRoleChanged:
                                            {
                                                if ( State == EntityState.Modified && QualifiersMatch( rockContext, trigger, PreviousGroupMemberRoleId, GroupMemberRoleId ) )
                                                {
                                                    LaunchWorkflow( rockContext, trigger.WorkflowTypeId, trigger.Name );
                                                }
                                                break;
                                            }
                                        case GroupMemberWorkflowTriggerType.MemberStatusChanged:
                                            {
                                                if ( State == EntityState.Modified && QualifiersMatch( rockContext, trigger, PreviousGroupMemberStatus, GroupMemberStatus ) )
                                                {
                                                    LaunchWorkflow( rockContext, trigger.WorkflowTypeId, trigger.Name );
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
            var qualifierParts = ( workflowTrigger.TypeQualifier ?? "" ).Split( new char[] { '|' } );

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
            var qualifierParts = ( workflowTrigger.TypeQualifier ?? "" ).Split( new char[] { '|' } );

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

        private void LaunchWorkflow( RockContext rockContext, int workflowTypeId, string name )
        {
            GroupMember groupMember = null;
            if ( GroupMemberGuid.HasValue )
            {
                groupMember = new GroupMemberService( rockContext ).Get( GroupMemberGuid.Value );
            }

            var workflowTypeService = new WorkflowTypeService( rockContext );
            var workflowType = workflowTypeService.Get( workflowTypeId );
            if ( workflowType != null )
            {
                var workflow = Rock.Model.Workflow.Activate( workflowType, name );

                if ( workflow.AttributeValues != null )
                {
                    if ( workflow.AttributeValues.ContainsKey( "Group" ) )
                    {
                        var group = new GroupService( rockContext ).Get( GroupId.Value );
                        if ( group != null )
                        {
                            workflow.AttributeValues["Group"].Value = group.Guid.ToString();
                        }
                    }

                    if ( workflow.AttributeValues.ContainsKey( "Person" ) )
                    {
                        var person = new PersonService( rockContext ).Get( PersonId.Value );
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
    }
}