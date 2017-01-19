using System;
using Rock.Data;
using Rock.Model;

namespace Rock
{
    /// <summary>
    /// Quartz Extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region IJobExecutionContext extensions

        /// <summary>
        /// Updates the LastStatusMessage of the Rock Job associated with the IJobExecutionContext
        /// </summary>
        /// <param name="entity">The entity.</param>
        public static void UpdateLastStatusMessage( this Quartz.IJobExecutionContext context, string message )
        {
            // save the message to context.Result so that RockJobListener will set the save the same message when the Job completes
            context.Result = message;

            int jobId = context.GetJobId();
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( jobId );
                if ( job != null )
                {
                    job.LastStatusMessage = message;
                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Gets the Job ID of the Rock Job associated with the IJobExecutionContext.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static int GetJobId( this Quartz.IJobExecutionContext context)
        {
            return context.JobDetail.Description.AsInteger();
        }

        #endregion

    }
}

