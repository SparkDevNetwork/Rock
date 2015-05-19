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
    /// Writes entity audits 
    /// </summary>
    public class GroupMemberChangeTransaction : ITransaction
    {
        private EntityState State;
        private Guid? GroupMemberGuid;
        private int GroupId;
        private int PersonId;
        private GroupMemberStatus GroupMemberStatus;
        private int GroupMemberRoleId;
        private GroupMemberStatus PreviousGroupMemberStatus;
        private int PreviousGroupMemberRoleId;

        public GroupMemberChangeTransaction ( DbEntityEntry entry )
        {
            var groupMember = entry.Entity as GroupMember;
            if ( groupMember != null )
            {
                State = entry.State;
                GroupId = groupMember.GroupId;
                PersonId = groupMember.PersonId;
                GroupMemberStatus = groupMember.GroupMemberStatus;
                GroupMemberRoleId = groupMember.GroupRoleId;

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

                if ( State != EntityState.Deleted )
                {
                    GroupMemberGuid = groupMember.Guid;
                }
            }
        }

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new GroupMemberWorkflowTriggerService( rockContext );

                var groupWorkflows = service.Queryable().AsNoTracking()
                    .Where( w => w.GroupId == GroupId )
                    .OrderBy( w => w.Order );

                var groupTypeWorkflows = service.Queryable().AsNoTracking()
                    .Where( w => w.GroupType.Groups.Any( g => g.Id == GroupId ) )
                    .OrderBy( w => w.Order );

                foreach ( var workflowTrigger in groupWorkflows.Union( groupTypeWorkflows ) )
                {
                    switch ( workflowTrigger.TriggerType )
                    {
                        case GroupMemberWorkflowTriggerType.MemberAddedToGroup:
                            {
                                if ( State == EntityState.Added && QualifiersMatch( rockContext, workflowTrigger, GroupMemberStatus, GroupMemberRoleId ) )
                                {
                                    LaunchWorkflow( rockContext, workflowTrigger.WorkflowTypeId );
                                }
                                break;
                            }
                        case GroupMemberWorkflowTriggerType.MemberRemovedFromGroup:
                            {
                                if ( State == EntityState.Deleted && QualifiersMatch( rockContext, workflowTrigger, PreviousGroupMemberStatus, PreviousGroupMemberRoleId ) )
                                {
                                    LaunchWorkflow( rockContext, workflowTrigger.WorkflowTypeId );
                                }
                                break;
                            }
                        case GroupMemberWorkflowTriggerType.MemberRoleChanged:
                            {
                                if ( State == EntityState.Modified && QualifiersMatch( rockContext, workflowTrigger, GroupMemberRoleId ) )
                                {
                                    LaunchWorkflow( rockContext, workflowTrigger.WorkflowTypeId );
                                }
                                break;
                            }
                        case GroupMemberWorkflowTriggerType.MemberStatusChanged:
                            {
                                if ( State == EntityState.Modified && QualifiersMatch( rockContext, workflowTrigger, GroupMemberStatus ) )
                                {
                                    LaunchWorkflow( rockContext, workflowTrigger.WorkflowTypeId );
                                }
                                break;
                            }
                    }
                }
            }
        }

        private bool QualifiersMatch( RockContext rockContext, GroupMemberWorkflowTrigger workflowTrigger, GroupMemberStatus status, int roleId )
        {
            return QualifiersMatch( rockContext, workflowTrigger, status ) && QualifiersMatch( rockContext, workflowTrigger, roleId );
        }

        private bool QualifiersMatch( RockContext rockContext, GroupMemberWorkflowTrigger workflowTrigger, GroupMemberStatus status )
        {
            var qualifierParts = ( workflowTrigger.TypeQualifier ?? "" ).Split( new char[] { '|' } );
            if ( qualifierParts.Length > 0 && !string.IsNullOrWhiteSpace( qualifierParts[0] ) )
            {
                return qualifierParts[0].AsInteger() == status.ConvertToInt();
            }
            return true;
        }

        private bool QualifiersMatch( RockContext rockContext, GroupMemberWorkflowTrigger workflowTrigger, int roleId )
        {
            var qualifierParts = ( workflowTrigger.TypeQualifier ?? "" ).Split( new char[] { '|' } );
            if ( qualifierParts.Length > 1 && !string.IsNullOrWhiteSpace( qualifierParts[1] ) )
            {
                var guid = qualifierParts[1].AsGuidOrNull();
                if ( guid.HasValue )
                {
                    var qualifierRoleId = new GroupTypeRoleService( rockContext ).Queryable().AsNoTracking()
                        .Where( r => r.Guid.Equals( guid.Value ) )
                        .Select( r => r.Id )
                        .FirstOrDefault();
                    return qualifierRoleId != 0 && qualifierRoleId == roleId;
                }
                return false;
            }
            return true;
        }

        private void LaunchWorkflow( RockContext rockContext, int workflowTypeId )
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
                var workflow = Rock.Model.Workflow.Activate( workflowType, workflowType.Name );

                if ( workflow.AttributeValues != null )
                {
                    if ( workflow.AttributeValues.ContainsKey( "Group" ) )
                    {
                        var group = new GroupService( rockContext ).Get( GroupId );
                        if ( group != null )
                        {
                            workflow.AttributeValues["Group"].Value = group.Guid.ToString();
                        }
                    }

                    if ( workflow.AttributeValues.ContainsKey( "Person" ) )
                    {
                        var person = new PersonService( rockContext ).Get( PersonId );
                        if ( person != null )
                        {
                            workflow.AttributeValues["Person"].Value = person.PrimaryAlias.Guid.ToString();
                        }
                    }
                }

                List<string> workflowErrors;
                if ( workflow.Process( rockContext, groupMember, out workflowErrors ) )
                {
                    if ( workflow.IsPersisted || workflowType.IsPersisted )
                    {
                        var workflowService = new Rock.Model.WorkflowService( rockContext );
                        workflowService.Add( workflow );

                        rockContext.WrapTransaction( () =>
                        {
                            rockContext.SaveChanges();
                            workflow.SaveAttributeValues( rockContext );
                            foreach ( var activity in workflow.Activities )
                            {
                                activity.SaveAttributeValues( rockContext );
                            }
                        } );
                    }
                }
            }
        }
    }
}