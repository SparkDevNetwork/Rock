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
    /// Launches a connection request change workflow
    /// </summary>
    public class ConnectionRequestActivityChangeTransaction : ITransaction
    {
        private EntityState State;
        private Guid? ConnectionRequestActivityGuid;
        private int? ConnectionTypeId;
        private int? ConnectionOpportunityId;
        private int? ConnectionRequestId;
        private int ConnectionActivityTypeId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionRequestActivityChangeTransaction"/> class.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public ConnectionRequestActivityChangeTransaction( DbEntityEntry entry )
        {
            // If entity was a connection request, save the values
            var connectionRequestActivity = entry.Entity as ConnectionRequestActivity;
            if ( connectionRequestActivity != null )
            {
                State = entry.State;
                ConnectionOpportunityId = connectionRequestActivity.ConnectionOpportunityId;
                ConnectionRequestId = connectionRequestActivity.ConnectionRequestId;
                ConnectionActivityTypeId = connectionRequestActivity.ConnectionActivityTypeId;

                if ( connectionRequestActivity.ConnectionOpportunity != null )
                {
                    ConnectionTypeId = connectionRequestActivity.ConnectionOpportunity.ConnectionTypeId;
                }

                // If this isn't a deleted connection request, get the connection request guid
                if ( State != EntityState.Deleted )
                {
                    ConnectionRequestActivityGuid = connectionRequestActivity.Guid;
                }
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
                    var connectionOpportunityWorkflows = cachedWorkflows
                        .Where( w =>
                            w.TriggerType != ConnectionWorkflowTriggerType.ActivityAdded &&
                            w.ConnectionOpportunityId.HasValue &&
                            w.ConnectionOpportunityId.Value == ConnectionOpportunityId.Value )
                        .ToList();

                    // Get any connectionWorkflows associated to a connection type ( if any are found, will then filter by connection type )
                    var connectionTypeWorkflows = cachedWorkflows
                        .Where( w =>
                            w.TriggerType != ConnectionWorkflowTriggerType.ActivityAdded &&
                            w.ConnectionTypeId.HasValue )
                        .ToList();

                    if ( connectionOpportunityWorkflows.Any() || connectionTypeWorkflows.Any() )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            // If there were any connection type connectionWorkflows, will now need to read the opportunity's connection type id
                            // and then further filter these connectionWorkflows by the current txn's connection type
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

                                // Further filter the connection type connectionWorkflows by the connection type id
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
                                    if ( connectionWorkflow.TriggerType == ConnectionWorkflowTriggerType.ActivityAdded )
                                    {
                                        if ( State == EntityState.Added && QualifiersMatch( rockContext, connectionWorkflow, ConnectionActivityTypeId ) )
                                        {
                                            LaunchWorkflow( rockContext, connectionWorkflow, "Activity Added" );
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
            ConnectionRequestActivity connectionRequestActivity = null;
            if ( ConnectionRequestActivityGuid.HasValue )
            {
                connectionRequestActivity = new ConnectionRequestActivityService( rockContext ).Get( ConnectionRequestActivityGuid.Value );
            }

            var workflowTypeService = new WorkflowTypeService( rockContext );
            var workflowType = workflowTypeService.Get( connectionWorkflow.WorkflowTypeId.Value );
            if ( workflowType != null )
            {
                var workflow = Rock.Model.Workflow.Activate( workflowType, name );

                if ( workflow.AttributeValues != null )
                {
                    if ( workflow.AttributeValues.ContainsKey( "ConnectionOpportunity" ) )
                    {
                        var connectionOpportunity = new ConnectionOpportunityService( rockContext ).Get( ConnectionOpportunityId.Value );
                        if ( connectionOpportunity != null )
                        {
                            workflow.AttributeValues["ConnectionOpportunity"].Value = connectionOpportunity.Guid.ToString();
                        }
                    }

                    if ( workflow.AttributeValues.ContainsKey( "ConnectionType" ) )
                    {
                        var connectionType = new ConnectionTypeService( rockContext ).Get( ConnectionTypeId.Value );
                        if ( connectionType != null )
                        {
                            workflow.AttributeValues["ConnectionType"].Value = connectionType.Guid.ToString();
                        }
                    }

                    if ( workflow.AttributeValues.ContainsKey( "ConnectionRequestActivity" ) )
                    {
                        if ( connectionRequestActivity != null )
                        {
                            workflow.AttributeValues["ConnectionRequestActivity"].Value = connectionRequestActivity.Guid.ToString();
                        }
                    }

                    if ( workflow.AttributeValues.ContainsKey( "ConnectionActivityType" ) )
                    {
                        var connectionActivityType = new ConnectionActivityTypeService( rockContext ).Get( ConnectionActivityTypeId );
                        if ( connectionActivityType != null )
                        {
                            workflow.AttributeValues["ConnectionActivityType"].Value = connectionActivityType.Guid.ToString();
                        }
                    }
                }

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