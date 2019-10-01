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

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to launch a workflow
    /// </summary>
    [WorkflowTypeField( "Workflow", "The workflow this job should activate." )]
    [DisallowConcurrentExecution]
    public class LaunchWorkflow : RockBlock, IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public LaunchWorkflow()
        {
        }

        /// <summary>
        /// Job that will launch a workflow.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            string workflowName = dataMap.GetString( "Workflow" );
            LaunchTheWorkflow( workflowName, context );
        }

        /// <summary>
        /// Launch the workflow
        /// </summary>
        protected void LaunchTheWorkflow( string workflowName, IJobExecutionContext context )
        {
            Guid workflowTypeGuid = Guid.NewGuid();
            if ( Guid.TryParse( workflowName, out workflowTypeGuid ) )
            {
                var workflowType = WorkflowTypeCache.Get( workflowTypeGuid );
                if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                {
                    var workflow = Rock.Model.Workflow.Activate( workflowType, workflowName );

                    List<string> workflowErrors;
                    var processed = new Rock.Model.WorkflowService( new RockContext() ).Process( workflow, out workflowErrors );
                    context.Result = ( processed ? "Processed " : "Did not process " ) + workflow.ToString();
                }
            }
        }
    }
}
