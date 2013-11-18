//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Web;
using Quartz;
using Rock.Attribute;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to launch a workflow
    /// </summary>
    /// <author>Rich Dubay</author>
    /// <author>Spark Development Network</author>
    [TextField( "Workflow Name", "The name of the workflow to launch", true, "", "General", 0, "WorkflowName" )]
    public class LaunchWorkflow : IJob
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
            string workflowName = dataMap.GetString( "WorkflowName" );
            LaunchTheWorkflow( workflowName );
            //if ( ProcessActivity( "Family Search", out errors ) )
        }

        /// <summary>
        /// Launch the workflow
        /// </summary>
        protected void LaunchTheWorkflow(string workflowName)
        {
            var errorMessages = new List<string>();
            var workflowTypeService = new WorkflowTypeService();
            var workflowTypeId = 0;
            foreach ( var wft in workflowTypeService.Queryable() )
            {
                if ( wft.Name == workflowName )
                {
                    workflowTypeId = wft.Id;
                }
            }
            var workflowType = workflowTypeService.Get( workflowTypeId );
            if ( workflowType != null )
            {
                var currentWorkflow = Rock.Model.Workflow.Activate( workflowType, workflowName );
                foreach ( var activityType in workflowType.ActivityTypes )
                {
                    WorkflowActivity.Activate( activityType, currentWorkflow );
                    currentWorkflow.Process( out errorMessages );
                }
            }
        }

        /// <summary>
        /// Activates and processes a workflow activity.  If the workflow has not yet been activated, it will
        /// also be activated
        /// </summary>
        /// <param name="activityName">Name of the activity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        //protected bool ProcessActivity( string activityName, out List<string> errorMessages )
        //{
        //    errorMessages = new List<string>();

        //    int workflowTypeId = 0;
        //    if ( Int32.TryParse( GetAttributeValue( "WorkflowTypeId" ), out workflowTypeId ) )
        //    {
        //        var workflowTypeService = new WorkflowTypeService();
        //        var workflowType = workflowTypeService.Get( workflowTypeId );
        //        if ( workflowType != null )
        //        {
        //            if ( CurrentWorkflow == null )
        //            {
        //                CurrentWorkflow = Rock.Model.Workflow.Activate( workflowType, CurrentCheckInState.Kiosk.Device.Name );
        //            }

        //            var activityType = workflowType.ActivityTypes.Where( a => a.Name == activityName ).FirstOrDefault();
        //            if ( activityType != null )
        //            {
        //                WorkflowActivity.Activate( activityType, CurrentWorkflow );
        //                if ( CurrentWorkflow.Process( CurrentCheckInState, out errorMessages ) )
        //                {
        //                    return true;
        //                }
        //            }
        //            else
        //            {
        //                errorMessages.Add( string.Format( "Workflow type does not have a '{0}' activity type", activityName ) );
        //            }
        //        }
        //        else
        //        {
        //            errorMessages.Add( string.Format( "Invalid Workflow type Id", activityName ) );
        //        }

        //    }

        //    return false;
        //}

    }
}
