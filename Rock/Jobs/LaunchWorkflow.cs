//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Web;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to launch a workflow
    /// </summary>
    /// <author>Rich Dubay</author>
    /// <author>Spark Development Network</author>
    [WorkflowTypeField( "Workflow", "The workflow this job should activate." )]
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
            LaunchTheWorkflow( workflowName );
        }

        /// <summary>
        /// Launch the workflow
        /// </summary>
        protected void LaunchTheWorkflow(string workflowName)
        {
            Guid workflowTypeGuid = Guid.NewGuid();
            if ( Guid.TryParse( workflowName, out workflowTypeGuid ) )
            {
                var workflowTypeService = new WorkflowTypeService();
                var workflowType = workflowTypeService.Get( workflowTypeGuid );
                if ( workflowType != null )
                {
                    var workflow = Rock.Model.Workflow.Activate( workflowType, workflowName );

                    List<string> workflowErrors;
                    if ( workflow.Process( out workflowErrors ) )
                    {
                        if ( workflowType.IsPersisted )
                        {
                            var workflowService = new Rock.Model.WorkflowService();
                            workflowService.Add( workflow, CurrentPersonId );
                            workflowService.Save( workflow, CurrentPersonId );
                        }
                    }
                }
            }     
        }
    }
}
