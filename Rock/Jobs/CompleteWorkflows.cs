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
using System.Web;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to close workflows
    /// </summary>
    [WorkflowTypeField("Workflow Types", "The type of workflows to close.", true, true, order: 0 )]
    [TextField("Close Status", "The status to set the workflow to when closed.", true, "Completed", order: 1)]
    [IntegerField("Expiration Age", "The age in minutes that a workflow needs to be in order to close them.", false, order: 2)]
    [DisallowConcurrentExecution]
    public class CompleteWorkflows : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public CompleteWorkflows()
        {
        }

        /// <summary>
        /// Job that will close workflows.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // run a SQL query to do something
            var workflowTypeGuids = dataMap.GetString( "WorkflowTypes" ).Split(',').Select(Guid.Parse).ToList();
            int? expirationAge = dataMap.GetString( "ExpirationAge" ).AsIntegerOrNull();
            string closeStatus = dataMap.GetString( "CloseStatus" );

            RockContext rockContext = new RockContext();
            var workflowService = new WorkflowService( rockContext );

            var qry = workflowService.Queryable()
                        .Where( w => workflowTypeGuids.Contains( w.WorkflowType.Guid ) );

            if ( expirationAge.HasValue )
            {
                var expirationDate = RockDateTime.Now.AddMinutes( 0 - expirationAge.Value );
                qry = qry.Where(w => w.CreatedDateTime <= expirationDate );
            }

            var workflows = qry.ToList();

            foreach(var workflow in workflows )
            {
                workflow.MarkComplete();
                workflow.Status = closeStatus;

                rockContext.SaveChanges();
            }
        }

    }
}
