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
    /// Launches a connection request change workflow
    /// </summary>
    [Obsolete( "Use ConnectionRequestActivityChangeTransaction Transaction instead." )]
    [RockObsolete( "1.13" )]
    public sealed class ProcessConnectionRequestActivityChange : BusStartedTask<ProcessConnectionRequestActivityChange.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        public override void Execute( Message message )
        {
            var rockContext = new RockContext();
            var connectionRequestActivity = new ConnectionRequestActivityService( rockContext ).Get( message.ConnectionRequestActivityGuid );
            if ( connectionRequestActivity == null )
            {
                return;
            }

            // Get all the connectionWorkflows from cache
            var cachedWorkflows = ConnectionWorkflowService.GetCachedTriggers();

            // If any connectionWorkflows exist
            if ( cachedWorkflows != null && cachedWorkflows.Any() )
            {
                // Get the connectionWorkflows associated to the connection
                var workflows = cachedWorkflows
                    .Where( w =>
                        w.TriggerType == ConnectionWorkflowTriggerType.ActivityAdded &&
                        ( ( w.ConnectionOpportunityId.HasValue && w.ConnectionOpportunityId.Value == message.ConnectionOpportunityId ) ||
                            w.ConnectionTypeId.HasValue ) )
                    .ToList();

                if ( workflows.Any() )
                {
                    // Get the current txn's connection type id
                    var connectionTypeId = new ConnectionOpportunityService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( o => o.Id == message.ConnectionOpportunityId )
                        .Select( o => o.ConnectionTypeId )
                        .FirstOrDefault();

                    // Further filter the connection type connectionWorkflows by the connection type id
                    workflows = workflows
                        .Where( w =>
                            ( w.ConnectionOpportunityId.HasValue && w.ConnectionOpportunityId.Value == message.ConnectionOpportunityId ) ||
                            ( w.ConnectionTypeId.HasValue && w.ConnectionTypeId.Value == connectionTypeId ) )
                        .ToList();

                    // Loop through connectionWorkflows and launch appropriate workflow
                    foreach ( var connectionWorkflow in workflows )
                    {
                        if ( QualifiersMatch( rockContext, connectionWorkflow, message.ConnectionActivityTypeId ) )
                        {
                            LaunchWorkflow( rockContext, connectionRequestActivity, connectionWorkflow, "Activity Added" );
                        }
                    }
                }
            }
        }

        private bool QualifiersMatch( RockContext rockContext, ConnectionWorkflow workflowTrigger, int connectionActivityTypeId )
        {
            var qualifierParts = ( workflowTrigger.QualifierValue ?? string.Empty ).Split( new char[] { '|' } );

            bool matches = true;

            if ( matches && qualifierParts.Length > 1 && !string.IsNullOrWhiteSpace( qualifierParts[1] ) )
            {
                var qualifierGroupId = qualifierParts[1].AsIntegerOrNull();
                if ( qualifierGroupId.HasValue )
                {
                    matches = qualifierGroupId != 0 && qualifierGroupId == connectionActivityTypeId;
                }
                else
                {
                    matches = false;
                }
            }

            return matches;
        }

        private void LaunchWorkflow( RockContext rockContext, ConnectionRequestActivity connectionRequestActivity, ConnectionWorkflow connectionWorkflow, string name )
        {
            var workflowType = WorkflowTypeCache.Get( connectionWorkflow.WorkflowTypeId.Value );
            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                var workflow = Rock.Model.Workflow.Activate( workflowType, name );

                List<string> workflowErrors;
                new WorkflowService( rockContext ).Process( workflow, connectionRequestActivity, out workflowErrors );
                if ( workflow.Id != 0 )
                {
                    ConnectionRequestWorkflow connectionRequestWorkflow = new ConnectionRequestWorkflow();
                    connectionRequestWorkflow.ConnectionRequestId = connectionRequestActivity.ConnectionRequestId;
                    connectionRequestWorkflow.WorkflowId = workflow.Id;
                    connectionRequestWorkflow.ConnectionWorkflowId = connectionWorkflow.Id;
                    connectionRequestWorkflow.TriggerType = connectionWorkflow.TriggerType;
                    connectionRequestWorkflow.TriggerQualifier = connectionWorkflow.QualifierValue;
                    new ConnectionRequestWorkflowService( rockContext ).Add( connectionRequestWorkflow );
                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the connection request activity unique identifier.
            /// </summary>
            /// <value>
            /// The connection request activity unique identifier.
            /// </value>
            public Guid ConnectionRequestActivityGuid { get; set; }

            /// <summary>
            /// Gets or sets the connection opportunity identifier.
            /// </summary>
            /// <value>
            /// The connection opportunity identifier.
            /// </value>
            public int? ConnectionOpportunityId { get; set; }

            /// <summary>
            /// Gets or sets the connection activity type identifier.
            /// </summary>
            /// <value>
            /// The connection activity type unique identifier.
            /// </value>
            public int ConnectionActivityTypeId { get; set; }
        }
    }
}