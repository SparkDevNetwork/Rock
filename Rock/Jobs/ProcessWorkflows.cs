//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Web;
using System.IO;

using Quartz;

using Rock.Util;

namespace Rock.Jobs
{

    /// <summary>
    /// Job to process the persisted active workflows
    /// </summary>
    /// <author>David Turner</author>
    /// <author>Spark Development Network</author>
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
            var service = new WorkflowService();

            foreach ( var workflow in service.GetActive() )
            {
                if ( !workflow.LastProcessedDateTime.HasValue ||
                    DateTime.Now.Subtract( workflow.LastProcessedDateTime.Value ).TotalSeconds >= workflow.WorkflowType.ProcessingIntervalSeconds )
                {
                    service.Process( workflow, null );
                }
            }
        }
    }
}