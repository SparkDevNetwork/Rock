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
using System.Linq;
using System.Web;
using System.IO;

using Quartz;

using Rock.Model;
using Rock.Data;

namespace Rock.Jobs
{

    /// <summary>
    /// Job to process the persisted active workflows
    /// </summary>
    [DisallowConcurrentExecution]
    public class ProcessWorkflows : IJob
    {
        /// <summary> 
        /// Empty constructor for job initilization
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
            foreach ( var workflowId in new WorkflowService( new RockContext() ).GetActive().Select(a => a.Id).ToList() )
            {
                // create a new rockContext and service for every workflow to prevent a build-up of Context.ChangeTracker.Entries()
                var rockContext = new RockContext();
                var workflowService = new WorkflowService( rockContext );
                var workflow = workflowService.Queryable( "WorkflowType" ).FirstOrDefault( a => a.Id == workflowId );
                if ( workflow != null )
                {
                    if ( !workflow.LastProcessedDateTime.HasValue ||
                        RockDateTime.Now.Subtract( workflow.LastProcessedDateTime.Value ).TotalSeconds >= ( workflow.WorkflowType.ProcessingIntervalSeconds ?? 0 ) )
                    {
                        var errorMessages = new List<string>();

                        workflowService.Process( workflow, out errorMessages );
                    }
                }
            }
        }
    }
}