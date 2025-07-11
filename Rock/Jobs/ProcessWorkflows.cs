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

using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to process the persisted active workflows
    /// </summary>
    [DisplayName( "Process Workflows" )]
    [Description( "Runs continuously to process in workflows activities/actions in progress." )]

    public class ProcessWorkflows : RockJob
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

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            int workflowsProcessed = 0;
            int workflowErrors = 0;
            int workflowExceptions = 0;
            var processingErrors = new List<string>();
            var exceptionMsgs = new List<string>();

            // Get all the "active" workflows that are ready for processing based on their LastProcessedDateTime
            // and the Workflow Type's ProcessingIntervalSeconds.
            //
            // NOTE: Be sure to use RockDateTime.Now otherwise DateTime.Now will use SysDateTime()
            //       which would be the time on the SQL server!

            var workflowIdsToProcess = new WorkflowService( new RockContext() )
                .GetActive()
                .Where( wf => ( wf.WorkflowType.IsActive == true || !wf.WorkflowType.IsActive.HasValue ) )
                .Where( wf =>
                    !wf.IsProcessing // Don't attempt to process workflows that are already being processed, as this can cause workflow actions to execute twice.
                    && ( !wf.LastProcessedDateTime.HasValue
                        || ( DbFunctions.AddSeconds( wf.LastProcessedDateTime.Value, wf.WorkflowType.ProcessingIntervalSeconds ?? 0 ) <= RockDateTime.Now ) ) )
                .Select( w => w.Id )
                .ToList();

            foreach ( var workflowId in workflowIdsToProcess )
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

            this.Result = resultMsg.ToString();
        }
    }
}