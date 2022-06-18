using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Quartz;

using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{

    /// <summary>
    /// Class RockJob.
    /// Implements the <see cref="IJob" />
    /// </summary>
    /// <seealso cref="IJob" />
    public abstract class RockJob : IJob
    {
        /// <summary>
        /// Gets the attribute values.
        /// </summary>
        /// <value>The attribute values.</value>
        public Dictionary<string, AttributeValueCache> AttributeValues { get; internal set; }

        /// <summary>
        /// Gets the service job.
        /// </summary>
        /// <value>The service job.</value>
        public Rock.Model.ServiceJob ServiceJob { get; private set; }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <inheritdoc cref="IJob.Execute(IJobExecutionContext)" />
        public abstract void Execute( RockJobContext context );

        /// <inheritdoc/>
        public Task Execute( IJobExecutionContext context )
        {
            var serviceJobId = context.GetJobId();
            var rockContext = new Rock.Data.RockContext();
            ServiceJob = new ServiceJobService( rockContext ).Get( serviceJobId );
            ServiceJob.LoadAttributes();

            Execute( new RockJobContext( ServiceJob, context ) );
            return Task.CompletedTask;
        }

        //public abstract void Execute( RockJobContext context );

        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>System.String.</returns>
        public string GetAttributeValue( string key )
        {
            return ServiceJob.GetAttributeValue( key );
        }
    }


    /// <summary>
    /// Class RockJobContext.
    /// </summary>
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
        public string Result
        {
            get => ServiceJob.LastStatusMessage;
            set => ServiceJob.LastStatusMessage = value;
        }

        /// <summary>
        /// Updates the last status message.
        /// </summary>
        /// <param name="statusMessage">The status message.</param>
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
