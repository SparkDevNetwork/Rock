using System;

using Quartz;

using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Class RockJobContext.
    /// </summary>
    [Obsolete("Maybe we don't need this")]
    public class RockJobContext
    {
        /// <summary>
        /// The service job
        /// </summary>
        public readonly Rock.Model.ServiceJob ServiceJob;

        /// <summary>
        /// The scheduler
        /// </summary>
        public readonly Quartz.IScheduler Scheduler;

        /// <summary>
        /// The job execution context
        /// </summary>
        private readonly IJobExecutionContext JobExecutionContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="RockJobContext"/> class.
        /// </summary>
        /// <param name="serviceJob">The service job.</param>
        /// <param name="jobExecutionContext">The job execution context.</param>
        public RockJobContext( ServiceJob serviceJob, IJobExecutionContext jobExecutionContext )
        {
            JobExecutionContext = jobExecutionContext;
            Scheduler = jobExecutionContext.Scheduler;
            ServiceJob = serviceJob;
        }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>The result.</value>
        [Obsolete( "Use RockJob.LastStatusMessage instead" )]
        public string Result
        {
            get => ServiceJob.LastStatusMessage;
            set => ServiceJob.LastStatusMessage = value;
        }

        /// <summary>
        /// Updates the last status message.
        /// </summary>
        /// <param name="statusMessage">The status message.</param>
        [Obsolete( "Use RockJob.UpdateLastStatusMessage instead" )]
        public void UpdateLastStatusMessage( string statusMessage )
        {
            ServiceJob.LastStatusMessage = statusMessage;
        }

        /// <summary>
        /// Gets the job detail.
        /// </summary>
        /// <value>The job detail.</value>
        public IJobDetail JobDetail => JobExecutionContext.JobDetail;

        /// <summary>
        /// Gets the job identifier.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int GetJobId()
        {
            return ServiceJob.Id;
        }
    }
}