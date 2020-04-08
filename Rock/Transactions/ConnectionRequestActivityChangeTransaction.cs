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
    public class ConnectionRequestActivityChangeTransaction : ITransaction
    {
        private Guid? ConnectionRequestActivityGuid;
        private int? ConnectionOpportunityId;
        private int? ConnectionRequestId;
        private int ConnectionActivityTypeId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionRequestActivityChangeTransaction"/> class.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public ConnectionRequestActivityChangeTransaction( DbEntityEntry entry )
        {
            // If entity was a connection request activity and state was added, save the values
            var connectionRequestActivity = entry.Entity as ConnectionRequestActivity;
            if ( connectionRequestActivity != null )
            {
                ConnectionRequestActivityGuid = connectionRequestActivity.Guid;
                ConnectionRequestId = connectionRequestActivity.ConnectionRequestId;
                ConnectionOpportunityId = connectionRequestActivity.ConnectionOpportunityId;
                ConnectionActivityTypeId = connectionRequestActivity.ConnectionActivityTypeId;
            }
        }

        /// <summary>
        /// Execute method to check for any workflows to launch.
        /// </summary>
        public void Execute()
        {
            // Verify that valid ids were saved
            if ( ConnectionRequestId.HasValue )
            {
                // Get all the connectionWorkflows from cache
                var cachedWorkflows = ConnectionWorkflowService.GetCachedTriggers();

                // If any connectionWorkflows exist
                if ( cachedWorkflows != null && cachedWorkflows.Any() )
                {
                    // Get the connectionWorkflows associated to the connection
                    var workflows = cachedWorkflows
                        .Where( w =>
                            w.TriggerType == ConnectionWorkflowTriggerType.ActivityAdded &&
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
                            var ConnectionTypeId = new ConnectionOpportunityService( rockContext )
                                .Queryable().AsNoTracking()
                                .Where( o => o.Id == ConnectionOpportunityId.Value )
                                .Select( o => o.ConnectionTypeId )
                                .FirstOrDefault();

                            // Further filter the connection type connectionWorkflows by the connection type id
                            workflows = workflows
                                .Where( w =>
                                    ( w.ConnectionOpportunityId.HasValue && w.ConnectionOpportunityId.Value == ConnectionOpportunityId.Value ) ||
                                    ( w.ConnectionTypeId.HasValue && w.ConnectionTypeId.Value == ConnectionTypeId ) )
                                .ToList();

                            // Loop through connectionWorkflows and launch appropriate workflow
                            foreach ( var connectionWorkflow in workflows )
                            {
                                if ( QualifiersMatch( rockContext, connectionWorkflow, ConnectionActivityTypeId ) )
                                {
                                    LaunchWorkflow( rockContext, connectionWorkflow, "Activity Added" );
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool QualifiersMatch( RockContext rockContext, ConnectionWorkflow workflowTrigger, int connectionActivityTypeId )
        {
            var qualifierParts = ( workflowTrigger.QualifierValue ?? "" ).Split( new char[] { '|' } );

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

        private void LaunchWorkflow( RockContext rockContext, ConnectionWorkflow connectionWorkflow, string name )
        {
            var workflowType = WorkflowTypeCache.Get( connectionWorkflow.WorkflowTypeId.Value );
            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                ConnectionRequestActivity connectionRequestActivity = null;
                if ( ConnectionRequestActivityGuid.HasValue )
                {
                    connectionRequestActivity = new ConnectionRequestActivityService( rockContext ).Get( ConnectionRequestActivityGuid.Value );
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
        }
    }
}