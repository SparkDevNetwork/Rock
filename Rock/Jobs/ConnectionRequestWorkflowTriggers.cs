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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;

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

    public class ConnectionRequestWorkflowTriggers : RockJob
    {
        private const string SOURCE_OF_CHANGE = "Connection Request Workflow Triggers";
        private HttpContext _httpContext = null;

        #region Keys

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

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
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

            var futureFollowupWorkflowResult = TriggerFutureFollowupWorkFlow( futureFollowupDateWorkflows );

            this.UpdateLastStatusMessage( $@"Future follow-up workflow triggered:<br>{futureFollowupWorkflowResult}" );
        }

        /// <summary>
        /// Trigger Future Follow-up Workflow
        /// </summary>
        /// <param name="futureFollowupDateWorkflows">The future follow-up date workflows.</param>
        /// <returns>System.String.</returns>
        private string TriggerFutureFollowupWorkFlow( List<ConnectionWorkflow> futureFollowupDateWorkflows )
        {
            try
            {
                this.UpdateLastStatusMessage( $"Processing future follow-up workflows." );

                int recordsUpdated = 0;
                int triggerWorkflow = 0;
                int recordsWithError = 0;

                var rockContext = new RockContext();
                DateTime midnightToday = RockDateTime.Today.AddDays( 1 );

                var connectionRequestService = new ConnectionRequestService( rockContext );
                var eligibleConnectionRequests = connectionRequestService
                            .Queryable()
                            .AsNoTracking()
                            .Where( cr => cr.ConnectionState == ConnectionState.FutureFollowUp &&
                                        cr.FollowupDate.HasValue &&
                                        cr.FollowupDate < midnightToday )
                            .Select( cr => cr.Id )
                            .ToList();

                // Fetch the FOLLOWUP_DATE_REACHED ConnectionActivityType
                int? followupDateReachedActivityId = null;
                if ( eligibleConnectionRequests.Any() )
                {
                    var guid = Rock.SystemGuid.ConnectionActivityType.FOLLOWUP_DATE_REACHED.AsGuid();
                    followupDateReachedActivityId = new ConnectionActivityTypeService( rockContext )
                        .Queryable()
                        .Where( t => t.Guid == guid )
                        .Select( t => t.Id )
                        .FirstOrDefault();
                }

                // For each eligible connection request, update the state to Active and trigger any applicable workflows.
                foreach ( var connectionRequestId in eligibleConnectionRequests )
                {
                    try
                    {
                        using ( var updateRockContext = new RockContext() )
                        {
                            updateRockContext.SourceOfChange = SOURCE_OF_CHANGE;

                            var updateConnectionRequest = new ConnectionRequestService( updateRockContext )
                                .Queryable()
                                .Include( cr => cr.ConnectionOpportunity )
                                .Include( cr => cr.PersonAlias )
                                .Include( cr => cr.PersonAlias.Person )
                                .FirstOrDefault( cr => cr.Id == connectionRequestId );

                            // Should not happen, unless someone just deleted it since the first query.
                            if ( updateConnectionRequest == null )
                            {
                                continue;
                            }

                            updateConnectionRequest.ConnectionState = ConnectionState.Active;

                            var connectionOpportunity = updateConnectionRequest.ConnectionOpportunity;
                            
                            // Log the activity for the 'follow-up date reached' and the record set back to active.
                            var connectionRequestActivity = new ConnectionRequestActivity
                            {
                                ConnectionRequestId = updateConnectionRequest.Id,
                                ConnectionOpportunityId = updateConnectionRequest.ConnectionOpportunityId,
                                ConnectionActivityTypeId = followupDateReachedActivityId.Value,
                                Note = "Connection State changed to 'Active'."
                            };

                            new ConnectionRequestActivityService( updateRockContext ).Add( connectionRequestActivity );
                            updateRockContext.SaveChanges();
                            recordsUpdated++;

                            // Lastly, launch the workflows that are applicable to this connection request.
                            var applicableWorkflows = futureFollowupDateWorkflows
                                                    .Where( w =>
                                                    ( w.ConnectionOpportunityId.HasValue && w.ConnectionOpportunityId.Value == connectionOpportunity.Id ) ||
                                                    ( w.ConnectionTypeId.HasValue && w.ConnectionTypeId.Value == connectionOpportunity.ConnectionTypeId ) );

                            foreach ( var connectionWorkflow in applicableWorkflows )
                            {
                                LaunchWorkflow( updateRockContext, updateConnectionRequest, connectionWorkflow, ConnectionWorkflowTriggerType.FutureFollowupDateReached.ConvertToString() );
                                triggerWorkflow++;
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        // Log exception and keep on trucking.
                        ExceptionLogService.LogException( new Exception( $"Exception occurred trying to trigger future follow-up workflow: {connectionRequestId}.", ex ), _httpContext );
                        recordsWithError++;
                    }
                }

                return SetJobResultSummaryForTriggerFutureFollowup( recordsUpdated, triggerWorkflow, recordsWithError );
            }
            catch ( Exception ex )
            {
                // Log exception and return the exception messages.
                ExceptionLogService.LogException( ex, _httpContext );

                return ex.Messages().AsDelimited( "; " );
            }
        }

        /// <summary>
        /// Builds a summary of the job results for the future follow-up workflow trigger section of this job.
        /// </summary>
        /// <param name="recordsUpdated"></param>
        /// <param name="workflowsTriggered"></param>
        /// <param name="recordsWithError"></param>
        /// <returns></returns>
        private string SetJobResultSummaryForTriggerFutureFollowup( int recordsUpdated, int workflowsTriggered, int recordsWithError )
        {
            StringBuilder jobSummaryBuilder = new StringBuilder();

            jobSummaryBuilder.AppendLine( $"<i class='fa fa-circle text-success'></i> {recordsUpdated:N0} connection {"request".PluralizeIf( recordsUpdated != 1 )} updated" );
            jobSummaryBuilder.AppendLine( $"<i class='fa fa-circle text-success'></i> {workflowsTriggered:N0} {"workflow".PluralizeIf( workflowsTriggered != 1 )} triggered" );

            if ( recordsWithError > 0 )
            {
                jobSummaryBuilder.AppendLine( $"<i class='fa fa-circle text-danger'></i> {recordsWithError:N0} {"record".PluralizeIf( recordsWithError != 1 )} logged an exception" );
            }

            return jobSummaryBuilder.ToString();
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
