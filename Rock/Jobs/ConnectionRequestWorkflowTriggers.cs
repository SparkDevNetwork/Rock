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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// This job triggers connection request workflows.
    /// </summary>
    [DisplayName( "Connection Request Workflow Triggers" )]
    [Description( "This job triggers connection request workflows." )]

    [DisallowConcurrentExecution]
    public class ConnectionRequestWorkflowTriggers : IJob
    {
        private const string SOURCE_OF_CHANGE = "Connection Request Workflow Triggers";
        private HttpContext _httpContext = null;

        #region Keys

        /// <summary>
        /// Keys to use for the attributes
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The number of days to look back
            /// </summary>
            public const string NumberOfDaysToLookBack = "NumberOfDaysToLookBack";
        }

        #endregion Keys
        #region Constructor

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public ConnectionRequestWorkflowTriggers()
        {
        }

        #endregion Constructor

        #region Methods

        /// <summary>
        /// Job that will run quick SQL queries on a schedule.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            _httpContext = HttpContext.Current;

            // Get all the workflows from cache
            var cachedWorkflows = ConnectionWorkflowService.GetCachedTriggers();

            var futureFollowupDateWorkflows = new List<ConnectionWorkflow>();
            if ( cachedWorkflows != null && cachedWorkflows.Any() )
            {
                futureFollowupDateWorkflows = cachedWorkflows
                   .Where( w => w.TriggerType == ConnectionWorkflowTriggerType.FutureFollowupDateReached )
                   .ToList();
            }

            var futureFollowupWorkflowResult = TriggerFutureFollowupWorkFlow( context, futureFollowupDateWorkflows );

            context.UpdateLastStatusMessage( $@"Future follow-up Workflow Triggered: {futureFollowupWorkflowResult}" );
        }

        /// <summary>
        /// Trigger Future Followup Workflow
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="futureFollowupDateWorkflows">The future follow-up date workflows.</param>
        /// <returns></returns>
        private string TriggerFutureFollowupWorkFlow( IJobExecutionContext context, List<ConnectionWorkflow> futureFollowupDateWorkflows )
        {
            try
            {
                JobDataMap dataMap = context.JobDetail.JobDataMap;

                context.UpdateLastStatusMessage( $"Processing future follow-up workFlows." );

                int recordsUpdated = 0;
                int triggerWorkflow = 0;
                int recordsWithError = 0;

                var rockContext = new RockContext();

                DateTime midnightToday = RockDateTime.Today.AddDays( 1 );

                var connectionRequestService = new ConnectionRequestService( rockContext );
                var eligibleConnectionRequests = connectionRequestService
                            .Queryable( "ConnectionRequestWorkflows" )
                            .AsNoTracking()
                            .Where( c => c.ConnectionState == ConnectionState.FutureFollowUp &&
                                        c.FollowupDate.HasValue &&
                                        c.FollowupDate < midnightToday )
                            .ToList();

                foreach ( var connectionRequest in eligibleConnectionRequests )
                {
                    try
                    {
                        using ( var updateRockContext = new RockContext() )
                        {
                            // increase the timeout just in case.
                            updateRockContext.Database.CommandTimeout = 180;
                            updateRockContext.SourceOfChange = SOURCE_OF_CHANGE;
                            var connectionOpportunity = connectionRequest.ConnectionOpportunity;
                            if ( connectionOpportunity != null )
                            {
                                var opportunityWorkflows = futureFollowupDateWorkflows
                                                        .Where( w =>
                                                        ( w.ConnectionOpportunityId.HasValue && w.ConnectionOpportunityId.Value == connectionOpportunity.Id ) ||
                                                        ( w.ConnectionTypeId.HasValue && w.ConnectionTypeId.Value == connectionOpportunity.ConnectionTypeId ) );

                                foreach ( var connectionWorkflow in opportunityWorkflows )
                                {
                                    LaunchWorkflow( updateRockContext, connectionRequest, connectionWorkflow, ConnectionWorkflowTriggerType.FutureFollowupDateReached.ConvertToString() );
                                    triggerWorkflow += 1;
                                }

                                updateRockContext.ConnectionRequests.Attach( connectionRequest );
                                connectionRequest.ConnectionState = ConnectionState.Active;

                                var guid = Rock.SystemGuid.ConnectionActivityType.FUTURE_FOLLOWUP_COMPLETE.AsGuid();
                                var futureFollowupCompleteActivityId = new ConnectionActivityTypeService( rockContext )
                                    .Queryable()
                                    .Where( t => t.Guid == guid )
                                    .Select( t => t.Id )
                                    .FirstOrDefault();

                                ConnectionRequestActivity connectionRequestActivity = new ConnectionRequestActivity();
                                connectionRequestActivity.ConnectionRequestId = connectionRequest.Id;
                                connectionRequestActivity.ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId;
                                connectionRequestActivity.ConnectionActivityTypeId = futureFollowupCompleteActivityId;
                                new ConnectionRequestActivityService( updateRockContext ).Add( connectionRequestActivity );
                                updateRockContext.SaveChanges();
                                recordsUpdated += 1;
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        // Log exception and keep on trucking.
                        ExceptionLogService.LogException( new Exception( $"Exception occurred trying to trigger future followup workFlow:{connectionRequest.Id}.", ex ), _httpContext );
                        recordsWithError += 1;
                    }
                }

                // Format the result message
                string result = $"{recordsUpdated:N0} connection request records triggered {triggerWorkflow} workflows.";
                if ( recordsWithError > 0 )
                {
                    result += $"{recordsWithError:N0} records logged an exception.";
                }

                return result;
            }
            catch ( Exception ex )
            {
                // Log exception and return the exception messages.
                ExceptionLogService.LogException( ex, _httpContext );

                return ex.Messages().AsDelimited( "; " );
            }
        }

        /// <summary>
        /// Launches the workflow.
        /// </summary>
        private void LaunchWorkflow( RockContext rockContext, ConnectionRequest connectionRequest, ConnectionWorkflow connectionWorkflow, string name )
        {
            var workflowType = WorkflowTypeCache.Get( connectionWorkflow.WorkflowTypeId.Value );
            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
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

        #endregion Methods
    }
}
