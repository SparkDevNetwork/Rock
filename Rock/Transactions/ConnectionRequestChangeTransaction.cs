﻿// <copyright>
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
using System.Data.Entity.Infrastructure;
using System.Linq;
using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Launches a connection request change workflow
    /// </summary>
    public class ConnectionRequestChangeTransaction : ITransaction
    {
        private EntityState State;
        private Guid? ConnectionRequestGuid;
        private int? ConnectionTypeId;
        private int? ConnectionOpportunityId;
        private int? PersonId;
        private ConnectionState ConnectionState;
        private ConnectionState PreviousConnectionState;
        private int ConnectionStatusId;
        private int PreviousConnectionStatusId;
        private int? AssignedGroupId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionRequestChangeTransaction"/> class.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public ConnectionRequestChangeTransaction( DbEntityEntry entry )
        {
            // If entity was a connection request, save the values
            var connectionRequest = entry.Entity as ConnectionRequest;
            if ( connectionRequest != null )
            {
                State = entry.State;
                ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId;
                PersonId = connectionRequest.PersonAlias != null ? connectionRequest.PersonAlias.PersonId : (int?)null;
                ConnectionState = connectionRequest.ConnectionState;
                ConnectionStatusId = connectionRequest.ConnectionStatusId;
                AssignedGroupId = connectionRequest.AssignedGroupId;

                if ( connectionRequest.ConnectionOpportunity != null )
                {
                    ConnectionTypeId = connectionRequest.ConnectionOpportunity.ConnectionTypeId;
                }

                // If this isn't a new connection request, get the previous state and role values
                if ( State != EntityState.Added )
                {
                    var dbStateProperty = entry.Property( "ConnectionState" );
                    if ( dbStateProperty != null )
                    {
                        PreviousConnectionState = (ConnectionState)dbStateProperty.OriginalValue;
                    }
                    var dbStatusProperty = entry.Property( "ConnectionStatusId" );
                    if ( dbStatusProperty != null )
                    {
                        PreviousConnectionStatusId = (int)dbStatusProperty.OriginalValue;
                    }
                }

                // If this isn't a deleted connection request, get the connection request guid
                if ( State != EntityState.Deleted )
                {
                    ConnectionRequestGuid = connectionRequest.Guid;
                }
            }
        }

        /// <summary>
        /// Execute method to check for any workflows to launch.
        /// </summary>
        public void Execute()
        {
            // Verify that valid ids were saved
            if ( ConnectionOpportunityId.HasValue && PersonId.HasValue )
            {
                // Get all the workflows from cache
                var cachedWorkflows = ConnectionWorkflowService.GetCachedTriggers();

                // If any workflows exist
                if ( cachedWorkflows != null && cachedWorkflows.Any() )
                {
                    // Get the workflows associated to the connection
                    var connectionOpportunityWorkflows = cachedWorkflows
                        .Where( w =>
                            w.TriggerType != ConnectionWorkflowTriggerType.ActivityAdded &&
                            w.ConnectionOpportunityId.HasValue &&
                            w.ConnectionOpportunityId.Value == ConnectionOpportunityId.Value )
                        .ToList();

                    // Get any workflows associated to a connection type ( if any are found, will then filter by connection type )
                    var connectionTypeWorkflows = cachedWorkflows
                        .Where( w =>
                            w.TriggerType != ConnectionWorkflowTriggerType.ActivityAdded &&
                            w.ConnectionTypeId.HasValue )
                        .ToList();

                    if ( connectionOpportunityWorkflows.Any() || connectionTypeWorkflows.Any() )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            // If there were any connection type workflows, will now need to read the opportunity's connection type id
                            // and then further filter these workflows by the current txn's connection type
                            if ( connectionTypeWorkflows.Any() )
                            {
                                // Get the current txn's connection type id
                                if ( !ConnectionTypeId.HasValue )
                                {
                                    ConnectionTypeId = new ConnectionOpportunityService( rockContext )
                                        .Queryable().AsNoTracking()
                                        .Where( o => o.Id == ConnectionOpportunityId.Value )
                                        .Select( o => o.ConnectionTypeId )
                                        .FirstOrDefault();
                                }

                                // Further filter the connection type workflows by the connection type id
                                connectionTypeWorkflows = connectionTypeWorkflows
                                    .Where( t =>
                                        t.ConnectionTypeId.HasValue &&
                                        t.ConnectionTypeId.Equals( ConnectionTypeId ) )
                                    .ToList();
                            }

                            // Combine connection opportunity and connection type trigers
                            var connectionWorkflows = connectionOpportunityWorkflows.Union( connectionTypeWorkflows ).ToList();

                            // If any connectionWorkflows were found
                            if ( connectionWorkflows.Any() )
                            {
                                // Loop through connectionWorkflows and lauch appropriate workflow
                                foreach ( var connectionWorkflow in connectionWorkflows )
                                {
                                    switch ( connectionWorkflow.TriggerType )
                                    {
                                        case ConnectionWorkflowTriggerType.RequestStarted:
                                            {
                                                if ( State == EntityState.Added && QualifiersMatch( rockContext, connectionWorkflow, ConnectionState, ConnectionState, ConnectionStatusId, ConnectionStatusId, AssignedGroupId ) )
                                                {
                                                    LaunchWorkflow( rockContext, connectionWorkflow, "Request Started" );
                                                }
                                                break;
                                            }
                                        case ConnectionWorkflowTriggerType.RequestConnected:
                                            {
                                                if ( State == EntityState.Modified && ConnectionState == global::ConnectionState.Connected )
                                                {
                                                    LaunchWorkflow( rockContext, connectionWorkflow, "Request Completed" );
                                                }
                                                break;
                                            }
                                        case ConnectionWorkflowTriggerType.StatusChanged:
                                            {
                                                if ( State == EntityState.Modified && QualifiersMatch( rockContext, connectionWorkflow, PreviousConnectionStatusId, ConnectionStatusId ) )
                                                {
                                                    LaunchWorkflow( rockContext, connectionWorkflow, "Status Changed" );
                                                }
                                                break;
                                            }
                                        case ConnectionWorkflowTriggerType.StateChanged:
                                            {
                                                if ( State == EntityState.Modified && QualifiersMatch( rockContext, connectionWorkflow, PreviousConnectionState, ConnectionState ) )
                                                {
                                                    LaunchWorkflow( rockContext, connectionWorkflow, "State Changed" );
                                                }
                                                break;
                                            }
                                        case ConnectionWorkflowTriggerType.PlacementGroupAssigned:
                                            {
                                                if ( State == EntityState.Modified && QualifiersMatch( rockContext, connectionWorkflow, AssignedGroupId ) )
                                                {
                                                    LaunchWorkflow( rockContext, connectionWorkflow, "Group Assigned" );
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

        private bool QualifiersMatch( RockContext rockContext, ConnectionWorkflow workflowTrigger, ConnectionState prevState, ConnectionState state, int prevStatusId, int statusId, int? groupId )
        {
            return QualifiersMatch( rockContext, workflowTrigger, prevState, state ) && QualifiersMatch( rockContext, workflowTrigger, prevStatusId, statusId ) && QualifiersMatch( rockContext, workflowTrigger, groupId );
        }

        private bool QualifiersMatch( RockContext rockContext, ConnectionWorkflow workflowTrigger, int? groupId )
        {
            var qualifierParts = ( workflowTrigger.QualifierValue ?? "" ).Split( new char[] { '|' } );

            bool matches = true;

            if ( matches && qualifierParts.Length > 1 && !string.IsNullOrWhiteSpace( qualifierParts[1] ) )
            {
                var qualifierGroupId = qualifierParts[1].AsIntegerOrNull();
                if ( qualifierGroupId.HasValue )
                {
                    matches = qualifierGroupId != 0 && qualifierGroupId == groupId;
                }
                else
                {
                    matches = false;
                }
            }
            return matches;
        }

        private bool QualifiersMatch( RockContext rockContext, ConnectionWorkflow workflowTrigger, ConnectionState prevState, ConnectionState state )
        {
            var qualifierParts = ( workflowTrigger.QualifierValue ?? "" ).Split( new char[] { '|' } );

            bool matches = true;

            if ( matches && qualifierParts.Length > 0 && !string.IsNullOrWhiteSpace( qualifierParts[0] ) )
            {
                matches = qualifierParts[0].AsInteger() == state.ConvertToInt();
            }

            if ( matches && qualifierParts.Length > 2 && !string.IsNullOrWhiteSpace( qualifierParts[2] ) )
            {
                matches = qualifierParts[2].AsInteger() == prevState.ConvertToInt();
            }

            return matches;
        }

        private bool QualifiersMatch( RockContext rockContext, ConnectionWorkflow workflowTrigger, int prevStatusId, int statusId )
        {
            var qualifierParts = ( workflowTrigger.QualifierValue ?? "" ).Split( new char[] { '|' } );

            bool matches = true;

            if ( matches && qualifierParts.Length > 1 && !string.IsNullOrWhiteSpace( qualifierParts[1] ) )
            {
                var qualifierRoleId = qualifierParts[1].AsIntegerOrNull();
                if ( qualifierRoleId.HasValue )
                {
                    matches = qualifierRoleId != 0 && qualifierRoleId == statusId;
                }
                else
                {
                    matches = false;
                }
            }

            if ( matches && qualifierParts.Length > 3 && !string.IsNullOrWhiteSpace( qualifierParts[3] ) )
            {
                var qualifierRoleId = qualifierParts[3].AsIntegerOrNull();
                if ( qualifierRoleId.HasValue )
                {
                    matches = qualifierRoleId != 0 && qualifierRoleId == prevStatusId;
                }
                else
                {
                    matches = false;
                }
            }

            return matches;
        }

        private void LaunchWorkflow( RockContext rockContext, ConnectionWorkflow connectionWorkflow, string name )
        {
            var workflowTypeService = new WorkflowTypeService( rockContext );
            var workflowType = workflowTypeService.Get( connectionWorkflow.WorkflowTypeId.Value );
            if ( workflowType != null )
            {
                ConnectionRequest connectionRequest = null;
                if ( ConnectionRequestGuid.HasValue )
                {
                    connectionRequest = new ConnectionRequestService( rockContext ).Get( ConnectionRequestGuid.Value );

                    var workflow = Rock.Model.Workflow.Activate( workflowType, name );

                    List<string> workflowErrors;
                    new WorkflowService( rockContext ).Process( workflow, connectionRequest, out workflowErrors );
                    if ( workflow.Id != 0 )
                    {
                        ConnectionRequestWorkflow connectionRequestWorkflow = new ConnectionRequestWorkflow();
                        connectionRequestWorkflow.ConnectionRequestId = connectionRequest.Id;
                        connectionRequestWorkflow.WorkflowId = workflow.Id;
                        connectionRequestWorkflow.ConnectionWorkflowId = connectionWorkflow.Id;
                        connectionRequestWorkflow.TriggerType = connectionWorkflow.TriggerType;
                        connectionRequestWorkflow.TriggerQualifier = connectionWorkflow.QualifierValue;
                        new ConnectionRequestWorkflowService( rockContext ).Add( connectionRequestWorkflow );
                        rockContext.SaveChanges();
                    }
                }
            }
        }
    }
}