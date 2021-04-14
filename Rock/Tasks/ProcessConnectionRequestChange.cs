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
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tasks
{
    /// <summary>
    /// Launches a connection request change workflow
    /// </summary>
    public sealed class ProcessConnectionRequestChange : BusStartedTask<ProcessConnectionRequestChange.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            if ( !( message.ConnectionOpportunityId.HasValue && message.PersonId.HasValue ) )
            {
                return;
            }

            // Get all the workflows from cache
            var cachedWorkflows = ConnectionWorkflowService.GetCachedTriggers();

            if ( cachedWorkflows == null || !cachedWorkflows.Any() )
            {
                return;
            }

            var workflows = cachedWorkflows
                .Where( w =>
                    w.TriggerType != ConnectionWorkflowTriggerType.ActivityAdded &&
                    w.TriggerType != ConnectionWorkflowTriggerType.Manual &&
                    (
                        ( w.ConnectionOpportunityId.HasValue && w.ConnectionOpportunityId.Value == message.ConnectionOpportunityId.Value ) ||
                        w.ConnectionTypeId.HasValue
                    ) )
                .ToList();

            if ( workflows.Any() )
            {
                using ( var rockContext = new RockContext() )
                {
                    // Get the current txn's connection type id
                    if ( !message.ConnectionTypeId.HasValue )
                    {
                        message.ConnectionTypeId = new ConnectionOpportunityService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( o => o.Id == message.ConnectionOpportunityId.Value )
                            .Select( o => o.ConnectionTypeId )
                            .FirstOrDefault();
                    }

                    // Further filter the connection type workflows by the connection type id
                    workflows = workflows
                        .Where( w =>
                            ( w.ConnectionOpportunityId.HasValue && w.ConnectionOpportunityId.Value == message.ConnectionOpportunityId.Value ) ||
                            ( message.ConnectionTypeId.HasValue && w.ConnectionTypeId.HasValue && w.ConnectionTypeId.Value == message.ConnectionTypeId.Value ) )
                        .ToList();

                    // Loop through connectionWorkflows and launch appropriate workflow
                    foreach ( var connectionWorkflow in workflows )
                    {
                        switch ( connectionWorkflow.TriggerType )
                        {
                            case ConnectionWorkflowTriggerType.RequestStarted:
                                {
                                    if ( message.State == EntityState.Added )
                                    {
                                        LaunchWorkflow( rockContext, connectionWorkflow, "Request Started", message );
                                    }

                                    break;
                                }

                            case ConnectionWorkflowTriggerType.RequestAssigned:
                                {
                                    if ( message.ConnectorPersonAliasId.HasValue &&
                                        !message.ConnectorPersonAliasId.Equals( message.PreviousConnectorPersonAliasId ) )
                                    {
                                        LaunchWorkflow( rockContext, connectionWorkflow, "Request Assigned", message );
                                    }

                                    break;
                                }

                            case ConnectionWorkflowTriggerType.RequestConnected:
                                {
                                    if ( message.State == EntityState.Modified &&
                                        message.PreviousConnectionState != ConnectionState.Connected &&
                                        message.ConnectionState == ConnectionState.Connected )
                                    {
                                        LaunchWorkflow( rockContext, connectionWorkflow, "Request Completed", message );
                                    }

                                    break;
                                }

                            case ConnectionWorkflowTriggerType.RequestTransferred:
                                {
                                    if ( message.State == EntityState.Modified &&
                                        !message.PreviousConnectionOpportunityId.Equals( message.ConnectionOpportunityId ) )
                                    {
                                        LaunchWorkflow( rockContext, connectionWorkflow, "Request Transferred", message );
                                    }

                                    break;
                                }

                            case ConnectionWorkflowTriggerType.PlacementGroupAssigned:
                                {
                                    if ( message.State == EntityState.Modified &&
                                        !message.PreviousAssignedGroupId.HasValue &&
                                        message.AssignedGroupId.HasValue )
                                    {
                                        LaunchWorkflow( rockContext, connectionWorkflow, "Group Assigned", message );
                                    }

                                    break;
                                }

                            case ConnectionWorkflowTriggerType.StatusChanged:
                                {
                                    if ( message.State == EntityState.Modified && QualifiersMatch( rockContext, connectionWorkflow, message.PreviousConnectionStatusId, message.ConnectionStatusId ) )
                                    {
                                        LaunchWorkflow( rockContext, connectionWorkflow, "Status Changed", message );
                                    }

                                    break;
                                }

                            case ConnectionWorkflowTriggerType.StateChanged:
                                {
                                    if ( message.State == EntityState.Modified && QualifiersMatch( rockContext, connectionWorkflow, message.PreviousConnectionState, message.ConnectionState ) )
                                    {
                                        LaunchWorkflow( rockContext, connectionWorkflow, "State Changed", message );
                                    }

                                    break;
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

            var qualifierParts = ( workflowTrigger.QualifierValue ?? string.Empty ).Split( new char[] { '|' } );

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

            var qualifierParts = ( workflowTrigger.QualifierValue ?? string.Empty ).Split( new char[] { '|' } );

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

        private void LaunchWorkflow( RockContext rockContext, ConnectionWorkflow connectionWorkflow, string name, Message message )
        {
            var workflowType = WorkflowTypeCache.Get( connectionWorkflow.WorkflowTypeId.Value );
            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                ConnectionRequest connectionRequest = null;
                if ( message.ConnectionRequestGuid.HasValue )
                {
                    connectionRequest = new ConnectionRequestService( rockContext ).Get( message.ConnectionRequestGuid.Value );

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

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the state.
            /// </summary>
            public EntityState State { get; set; }

            /// <summary>
            /// Gets or sets the connection request identifier.
            /// </summary>
            public Guid? ConnectionRequestGuid { get; set; }

            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            public int? PersonId { get; set; }

            /// <summary>
            /// Gets or sets the connection type identifier.
            /// </summary>
            public int? ConnectionTypeId { get; set; }

            /// <summary>
            /// Gets or sets the connection opportunity identifier.
            /// </summary>
            public int? ConnectionOpportunityId { get; set; }

            /// <summary>
            /// Gets or sets the previous connection opportunity identifier.
            /// </summary>
            public int? PreviousConnectionOpportunityId { get; set; }

            /// <summary>
            /// Gets or sets the connector person alias identifier.
            /// </summary>
            public int? ConnectorPersonAliasId { get; set; }

            /// <summary>
            /// Gets or sets the previous connector person alias identifier.
            /// </summary>
            public int? PreviousConnectorPersonAliasId { get; set; }

            /// <summary>
            /// Gets or sets the connection state.
            /// </summary>
            public ConnectionState ConnectionState { get; set; }

            /// <summary>
            /// Gets or sets the previous connection state.
            /// </summary>
            public ConnectionState PreviousConnectionState { get; set; }

            /// <summary>
            /// Gets or sets the connection status identifier.
            /// </summary>
            public int ConnectionStatusId { get; set; }

            /// <summary>
            /// Gets or sets the previous connection status identifier.
            /// </summary>
            public int PreviousConnectionStatusId { get; set; }

            /// <summary>
            /// Gets or sets the assigned group identifier.
            /// </summary>
            public int? AssignedGroupId { get; set; }

            /// <summary>
            /// Gets or sets the previous assigned group identifier.
            /// </summary>
            public int? PreviousAssignedGroupId { get; set; }
        }
    }
}