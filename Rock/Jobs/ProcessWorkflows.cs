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
using System.Linq;
using System.Text;

using Quartz;

using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{

    /// <summary>
    /// Job to process the persisted active workflows
    /// </summary>
    [DisallowConcurrentExecution]
    public class ProcessWorkflows : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public ProcessWorkflows()
        {
        }

        /// <summary>
        /// Called by the <see cref="IScheduler" /> when a <see cref="ITrigger" />
        /// fires that is associated with the <see cref="IJob" />.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <remarks>
        /// The implementation may wish to set a  result object on the
        /// JobExecutionContext before this method exits.  The result itself
        /// is meaningless to Quartz, but may be informative to
        /// <see cref="IJobListener" />s or
        /// <see cref="ITriggerListener" />s that are watching the job's
        /// execution.
        /// </remarks>
        public virtual void Execute( IJobExecutionContext context )
        {
            int workflowsProcessed = 0;
            int workflowErrors = 0;
            int workflowExceptions = 0;
            var processingErrors = new List<string>();
            var exceptionMsgs = new List<string>();

            foreach ( var workflowId in new WorkflowService( new RockContext() )
                .GetActive()
                .Select( w => w.Id )
                .ToList() )
            {
                try
                {
                    // create a new rockContext and service for every workflow to prevent a build-up of Context.ChangeTracker.Entries()
                    var rockContext = new RockContext();
                    var workflowService = new WorkflowService( rockContext );
                    var workflow = workflowService.Queryable().FirstOrDefault( a => a.Id == workflowId );
                    if ( workflow != null )
                    {
                        var workflowType = workflow.WorkflowTypeCache;
                        if ( workflowType != null )
                        {
                            try
                            {
                                if ( !workflow.LastProcessedDateTime.HasValue ||
                                    RockDateTime.Now.Subtract( workflow.LastProcessedDateTime.Value ).TotalSeconds >= ( workflowType.ProcessingIntervalSeconds ?? 0 ) )
                                {
                                    var errorMessages = new List<string>();

                                    var processed = workflowService.Process( workflow, out errorMessages );
                                    if ( processed )
                                    {
                                        workflowsProcessed++;
                                    }
                                    else
                                    {
                                        workflowErrors++;
                                        processingErrors.Add( string.Format( "{0} [{1}] - {2} [{3}]: {4}", workflowType.Name, workflowType.Id, workflow.Name, workflow.Id, errorMessages.AsDelimited( ", " ) ) );
                                    }
                                }
                            }
                            catch ( Exception ex )
                            {
                                string workflowDetails = string.Format( "{0} [{1}] - {2} [{3}]", workflowType.Name, workflowType.Id, workflow.Name, workflow.Id );
                                exceptionMsgs.Add( workflowDetails + ": " + ex.Message );
                                throw new Exception( "Exception occurred processing workflow: " + workflowDetails, ex );
                            }
                        }
                    }
                }

                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex, null );
                    workflowExceptions++;
                }
            }

            var resultMsg = new StringBuilder();
            resultMsg.AppendFormat( "{0} workflows processed", workflowsProcessed );
            if ( workflowErrors > 0 )
            {
                resultMsg.AppendFormat( ", {0} workflows reported an error", workflowErrors );
            }
            if ( workflowExceptions > 0 )
            {
                resultMsg.AppendFormat( ", {0} workflows caused an exception", workflowExceptions );
            }
            if ( processingErrors.Any() )
            {
                resultMsg.Append( Environment.NewLine + processingErrors.AsDelimited( Environment.NewLine ) );
            }

            if ( exceptionMsgs.Any() )
            {
                throw new Exception( "One or more exceptions occurred processing workflows..." + Environment.NewLine + exceptionMsgs.AsDelimited( Environment.NewLine ) );
            }

            context.Result = resultMsg.ToString();
        }
    }
}