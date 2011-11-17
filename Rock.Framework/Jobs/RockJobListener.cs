using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Text;
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
            StringBuilder message = new StringBuilder();
            bool sendMessage = false;
            
            // get job type id
            int jobId = Convert.ToInt16(context.JobDetail.Description);

            // load job
            JobService jobService = new JobService();
            Job job = jobService.Get(jobId);

            // format the message
            message.Append( String.Format( "The job {0} ran for {1} seconds on {2}.  Below is the results:<p>" , job.Name, context.JobRunTime.TotalSeconds, context.FireTimeUtc.Value.DateTime.ToLocalTime()) );

            // if noticiation staus is all set flag to send message
            if ( job.NotificationStatus == Job.JobNotificationStatus.All )
                sendMessage = true;

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

                message.Append( "Result: Success" );

                // determine if message should be sent
                if ( job.NotificationStatus == Job.JobNotificationStatus.Success )
                    sendMessage = true;
            }
            else
            {
                // put the exception into the status
                job.LastStatus = "Exception";
                job.LastStatusMessage = jobException.Message;

                message.Append( "Result: Exception<p>Message:<br>" + jobException.Message );

                if ( jobException.InnerException != null ) {
                    job.LastStatusMessage += " Inner Exception: " + jobException.InnerException.Message;
                    message.Append( "<p>Inner Exception:<br>" + jobException.InnerException.Message );
                }

                if ( job.NotificationStatus == Job.JobNotificationStatus.Error )
                    sendMessage = true;
            }

            jobService.Save( job, 1 );

            // send notification
            if ( sendMessage )
            {
                // TODO: implement email send once it's available 
            }


        }
    }
}