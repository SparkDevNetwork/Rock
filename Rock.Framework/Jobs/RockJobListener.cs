using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Rock.Services.Util;
using Rock.Models.Util;

using Quartz;

/// <summary>
/// Summary description for JobListener
/// </summary>
namespace Rock.Jobs
{
    public class RockJobListener : IJobListener
    {
        public string Name
        {
            get
            {
                return "RockJobListener";
            }
        }

        public RockJobListener()
        {
        }



        public void JobToBeExecuted( IJobExecutionContext context )
        {
        }

        public void JobExecutionVetoed( IJobExecutionContext context )
        {
        }

        public void JobWasExecuted( IJobExecutionContext context, JobExecutionException jobException )
        {
            // get job type id
            int jobId = Convert.ToInt16(context.JobDetail.Description);

            // load job
            JobService jobService = new JobService();
            Job job = jobService.GetJob(jobId);

            // set last run date
            job.LastRunDate = context.FireTimeUtc.Value.DateTime.ToLocalTime();

            // set run time
            job.LastRunDuration = Convert.ToInt32(context.JobRunTime.TotalSeconds);

            // determine if an error occured
            if ( jobException == null )
            {
                job.LastSuccessfulRun = job.LastRunDate;
                job.LastStatus = "Success";
                job.LastStatusMessage = "";
            }
            else
            {
                // put the exception into the status
                job.LastStatus = "Exception";
                job.LastStatusMessage = jobException.Message;

                if ( jobException.InnerException != null )
                    job.LastStatusMessage += " Inner Exception: " + jobException.InnerException.Message;
            }

            jobService.Save( job, 1 );
        }
    }
}