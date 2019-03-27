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
using System.Data.Entity.Infrastructure;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Launches a connection request change workflow
    /// </summary>
    public class ConnectionRequestChangeTransaction : ITransaction
    {
        private EntityState State;
        private Guid? ConnectionRequestGuid;
        private int? PersonId;

        private int? ConnectionTypeId;

        private int? ConnectionOpportunityId;
        private int? PreviousConnectionOpportunityId;

        private int? ConnectorPersonAliasId;
        private int? PreviousConnectorPersonAliasId;

        private ConnectionState ConnectionState;
        private ConnectionState PreviousConnectionState;

        private int ConnectionStatusId;
        private int PreviousConnectionStatusId;

        private int? AssignedGroupId;
        private int? PreviousAssignedGroupId;

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

                // If this isn't a deleted connection request, get the connection request guid
                if ( State != EntityState.Deleted )
                {
                    ConnectionRequestGuid = connectionRequest.Guid;
                    PersonId = connectionRequest.PersonAlias != null ? connectionRequest.PersonAlias.PersonId : (int?)null;
                    if ( connectionRequest.ConnectionOpportunity != null )
                    {
                        ConnectionTypeId = connectionRequest.ConnectionOpportunity.ConnectionTypeId;
                    }
                    ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId;
                    ConnectorPersonAliasId = connectionRequest.ConnectorPersonAliasId;
                    ConnectionState = connectionRequest.ConnectionState;
                    ConnectionStatusId = connectionRequest.ConnectionStatusId;
                    AssignedGroupId = connectionRequest.AssignedGroupId;

                    if ( State == EntityState.Modified )
                    {
                        var dbOpportunityIdProperty = entry.Property( "ConnectionOpportunityId" );
                        if ( dbOpportunityIdProperty != null )
                        {
                            PreviousConnectionOpportunityId = dbOpportunityIdProperty.OriginalValue as int?;
                        }

                        var dbConnectorPersonAliasIdProperty = entry.Property( "ConnectorPersonAliasId" );
                        if ( dbConnectorPersonAliasIdProperty != null )
                        {
                            PreviousConnectorPersonAliasId = dbConnectorPersonAliasIdProperty.OriginalValue as int?;
                        }

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

                        var dbAssignedGroupIdProperty = entry.Property( "AssignedGroupId" );
                        if ( dbAssignedGroupIdProperty != null )
                        {
                            PreviousAssignedGroupId = dbAssignedGroupIdProperty.OriginalValue as int?;
                        }
                    }
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
                    var workflows = cachedWorkflows
                        .Where( w =>
                            w.TriggerType != ConnectionWorkflowTriggerType.ActivityAdded &&
                            w.TriggerType != ConnectionWorkflowTriggerType.Manual &&
                            (
                                ( w.ConnectionOpportunityId.HasValue && w.ConnectionOpportunityId.Value == ConnectionOpportunityId.Value ) ||
                                ( w.ConnectionTypeId.HasValue )
                            )
                        )
                        .ToList();

                    if ( workflows.Any() )
                    {
                        using ( var rockContext = new RockContext() )
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
                            workflows = workflows
                                .Where( w =>
                                    ( w.ConnectionOpportunityId.HasValue && w.ConnectionOpportunityId.Value == ConnectionOpportunityId.Value ) ||
                                    ( ConnectionTypeId.HasValue && w.ConnectionTypeId.HasValue && w.ConnectionTypeId.Value == ConnectionTypeId.Value ) )
                                .ToList();

                            // Loop through connectionWorkflows and launch appropriate workflow
                            foreach ( var connectionWorkflow in workflows )
                            {
                                switch ( connectionWorkflow.TriggerType )
                                {
                                    case ConnectionWorkflowTriggerType.RequestStarted:
                                        {
                                            if ( State == EntityState.Added )
                                            {
                                                LaunchWorkflow( rockContext, connectionWorkflow, "Request Started" );
                                            }
                                            break;
                                        }

                                    case ConnectionWorkflowTriggerType.RequestAssigned:
                                        {
                                            if ( ConnectorPersonAliasId.HasValue &&
                                                !ConnectorPersonAliasId.Equals( PreviousConnectorPersonAliasId ) )
                                            {
                                                LaunchWorkflow( rockContext, connectionWorkflow, "Request Assigned" );
                                            }
                                            break;
                                        }

                                    case ConnectionWorkflowTriggerType.RequestConnected:
                                        {
                                            if ( State == EntityState.Modified &&
                                                PreviousConnectionState != ConnectionState.Connected &&
                                                ConnectionState == ConnectionState.Connected )
                                            {
                                                LaunchWorkflow( rockContext, connectionWorkflow, "Request Completed" );
                                            }
                                            break;
                                        }

                                    case ConnectionWorkflowTriggerType.RequestTransferred:
                                        {
                                            if ( State == EntityState.Modified &&
                                                !PreviousConnectionOpportunityId.Equals( ConnectionOpportunityId ) )
                                            {
                                                LaunchWorkflow( rockContext, connectionWorkflow, "Request Transferred" );
                                            }
                                            break;
                                        }

                                    case ConnectionWorkflowTriggerType.PlacementGroupAssigned:
                                        {
                                            if ( State == EntityState.Modified &&
                                                !PreviousAssignedGroupId.HasValue &&
                                                AssignedGroupId.HasValue )
                                            {
                                                LaunchWorkflow( rockContext, connectionWorkflow, "Group Assigned" );
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
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool QualifiersMatch( RockContext rockContext, ConnectionWorkflow workflowTrigger, ConnectionState prevState, ConnectionState state )
        {
            if ( prevState == state )
            {
                return false;
            }

            var qualifierParts = ( workflowTrigger.QualifierValue ?? "" ).Split( new char[] { '|' } );

            bool matches = true;

            if ( matches && qualifierParts.Length > 1 && !string.IsNullOrWhiteSpace( qualifierParts[1] ) )
            {
                matches = qualifierParts[0].AsInteger() == prevState.ConvertToInt();
            }

            if ( matches && qualifierParts.Length > 2 && !string.IsNullOrWhiteSpace( qualifierParts[2] ) )
            {
                matches = qualifierParts[2].AsInteger() == state.ConvertToInt();
            }

            return matches;
        }

        private bool QualifiersMatch( RockContext rockContext, ConnectionWorkflow workflowTrigger, int prevStatusId, int statusId )
        {
            if ( prevStatusId == statusId )
            {
                return false;
            }

            var qualifierParts = ( workflowTrigger.QualifierValue ?? "" ).Split( new char[] { '|' } );

            bool matches = true;

            if ( matches && qualifierParts.Length > 1 && !string.IsNullOrWhiteSpace( qualifierParts[1] ) )
            {
                var qualifierStatusId = qualifierParts[1].AsIntegerOrNull();
                if ( qualifierStatusId.HasValue )
                {
                    matches = qualifierStatusId != 0 && qualifierStatusId == prevStatusId;
                }
                else
                {
                    matches = false;
                }
            }

            if ( matches && qualifierParts.Length > 2 && !string.IsNullOrWhiteSpace( qualifierParts[2] ) )
            {
                var qualifierStatusId = qualifierParts[2].AsIntegerOrNull();
                if ( qualifierStatusId.HasValue )
                {
                    matches = qualifierStatusId != 0 && qualifierStatusId == statusId;
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
            var workflowType = WorkflowTypeCache.Get( connectionWorkflow.WorkflowTypeId.Value );
            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
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