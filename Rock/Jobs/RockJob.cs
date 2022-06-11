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

    public abstract class RockJob : IJob
    {
        public Dictionary<string, AttributeValueCache> AttributeValues { get; internal set; }

        public Rock.Model.ServiceJob ServiceJob { get; private set; }

        /// <inheritdoc cref="IJob.Execute(IJobExecutionContext)"/>
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

        public string GetAttributeValue( string key )
        {
            return ServiceJob.GetAttributeValue(key);
        }
    }


    public class RockJobContext
    {
        public readonly Rock.Model.ServiceJob ServiceJob;
        public readonly Quartz.IScheduler Scheduler;
        private readonly IJobExecutionContext JobExecutionContext;

        public RockJobContext(ServiceJob serviceJob, IJobExecutionContext jobExecutionContext )
        {
            JobExecutionContext = jobExecutionContext;
            Scheduler = jobExecutionContext.Scheduler;
            ServiceJob = serviceJob;
        }

        public string Result
        {
            get => ServiceJob.LastStatusMessage;
            set => ServiceJob.LastStatusMessage = value;
        }

        public void UpdateLastStatusMessage(string statusMessage )
        {
            ServiceJob.LastStatusMessage = statusMessage;
        }

        public IJobDetail JobDetail => JobExecutionContext.JobDetail;

        public int GetJobId()
        {
            return ServiceJob.Id;
        }

        
    }

}
