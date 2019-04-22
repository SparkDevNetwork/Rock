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
using System.Linq;
using System.Text;

using DotLiquid;

using Quartz;

using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Summary description for JobListener
    /// </summary>
    public class RockJobListener : IJobListener
    {
        /// <summary>
        /// Get the name of the <see cref="IJobListener"/>.
        /// </summary>
        public string Name
        {
            get
            {
                return "RockJobListener";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockJobListener"/> class.
        /// </summary>
        public RockJobListener()
        {
        }

        /// <summary>
        /// Called by the <see cref="IScheduler"/> when a <see cref="IJobDetail"/>
        /// is about to be executed (an associated <see cref="ITrigger"/>
        /// has occurred).
        /// <para>
        /// This method will not be invoked if the execution of the Job was vetoed
        /// by a <see cref="ITriggerListener"/>.
        /// </para>
        /// </summary>
        /// <param name="context"></param>
        /// <seealso cref="JobExecutionVetoed(IJobExecutionContext)"/>
        public void JobToBeExecuted( IJobExecutionContext context )
        {
            StringBuilder message = new StringBuilder();

            // get job type id
            int jobId = context.JobDetail.Description.AsInteger();

            // load job
            var rockContext = new RockContext();
            var jobService = new ServiceJobService( rockContext );
            var job = jobService.Get( jobId );

            if ( job != null && job.Guid != Rock.SystemGuid.ServiceJob.JOB_PULSE.AsGuid() )
            {
                job.LastStatus = "Running";
                job.LastStatusMessage = "Started at " + RockDateTime.Now.ToString();
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Called by the <see cref="IScheduler"/> when a <see cref="IJobDetail"/>
        /// was about to be executed (an associated <see cref="ITrigger"/>
        /// has occurred), but a <see cref="ITriggerListener"/> vetoed its
        /// execution.
        /// </summary>
        /// <param name="context"></param>
        /// <seealso cref="JobToBeExecuted(IJobExecutionContext)"/>
        public void JobExecutionVetoed( IJobExecutionContext context )
        {
        }

        /// <summary>
        /// Adds the service job history.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <param name="rockContext">The rock context.</param>
        private void AddServiceJobHistory( ServiceJob job, RockContext rockContext )
        {
            var jobHistoryService = new ServiceJobHistoryService( rockContext );
            var jobHistory = new ServiceJobHistory()
            {
                ServiceJobId = job.Id,
                StartDateTime = job.LastRunDateTime?.AddSeconds( 0.0d - ( double ) job.LastRunDurationSeconds ),
                StopDateTime = job.LastRunDateTime,
                Status = job.LastStatus,
                StatusMessage = job.LastStatusMessage,
                ServiceWorker = Environment.MachineName.ToLower()
            };
            jobHistoryService.Add( jobHistory );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Called by the <see cref="IScheduler"/> after a <see cref="IJobDetail"/>
        /// has been executed, and before the associated <see cref="Quartz.Spi.IOperableTrigger"/>'s
        /// <see cref="Quartz.Spi.IOperableTrigger.Triggered"/> method has been called.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="jobException"></param>
        public void JobWasExecuted( IJobExecutionContext context, JobExecutionException jobException )
        {
            bool sendMessage = false;

            // get job id
            int jobId = context.GetJobId();

            // load job
            var rockContext = new RockContext();
            var jobService = new ServiceJobService( rockContext );
            var job = jobService.Get( jobId );

            if ( job == null )
            {
                // if job was deleted or wasn't found, just exit
                return;
            }

            // if notification status is all set flag to send message
            if ( job.NotificationStatus == JobNotificationStatus.All )
            {
                sendMessage = true;
            }

            // set last run date
            job.LastRunDateTime = RockDateTime.Now; // context.FireTimeUtc.Value.DateTime.ToLocalTime();

            // set run time
            job.LastRunDurationSeconds = Convert.ToInt32( context.JobRunTime.TotalSeconds );

            // set the scheduler name
            job.LastRunSchedulerName = context.Scheduler.SchedulerName;

            // determine if an error occurred
            if ( jobException == null )
            {
                job.LastSuccessfulRunDateTime = job.LastRunDateTime;
                job.LastStatus = "Success";
                if ( context.Result is string )
                {
                    job.LastStatusMessage = context.Result as string;
                }
                else
                {
                    job.LastStatusMessage = string.Empty;
                }

                // determine if message should be sent
                if ( job.NotificationStatus == JobNotificationStatus.Success )
                {
                    sendMessage = true;
                }
            }
            else
            {
                Exception exceptionToLog = jobException;

                // drill down to the interesting exception
                while ( exceptionToLog is Quartz.SchedulerException && exceptionToLog.InnerException != null )
                {
                    exceptionToLog = exceptionToLog.InnerException;
                }

                // log the exception to the database
                ExceptionLogService.LogException( exceptionToLog, null );

                var summaryException = exceptionToLog;

                // put the exception into the status
                job.LastStatus = "Exception";

                AggregateException aggregateException = summaryException as AggregateException;
                if ( aggregateException != null && aggregateException.InnerExceptions != null && aggregateException.InnerExceptions.Count == 1 )
                {
                    // if it's an aggregate, but there is only one, convert it to a single exception
                    summaryException = aggregateException.InnerExceptions[0];
                    aggregateException = null;
                }

                if ( aggregateException != null )
                {
                    var firstException = aggregateException.InnerExceptions.First();
                    job.LastStatusMessage = "One or more exceptions occurred. First Exception: " + firstException.Message;
                    summaryException = firstException;
                }
                else
                {
                    job.LastStatusMessage = summaryException.Message;
                }

                if ( job.NotificationStatus == JobNotificationStatus.Error )
                {
                    sendMessage = true;
                }
            }

            rockContext.SaveChanges();

            // Add job history
            AddServiceJobHistory( job, rockContext );

            // send notification
            if ( sendMessage )
            {
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null, new Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
                mergeFields.Add( "Job", job );
                try
                {
                    if ( jobException != null )
                    {
                        mergeFields.Add( "Exception", Hash.FromAnonymousObject( jobException ) );
                    }

                }
                catch
                {
                    // ignore
                }

                var notificationEmailAddresses = job.NotificationEmails.ResolveMergeFields( mergeFields ).SplitDelimitedValues().ToList();
                var emailMessage = new RockEmailMessage( Rock.SystemGuid.SystemEmail.CONFIG_JOB_NOTIFICATION.AsGuid() );
                emailMessage.AdditionalMergeFields = mergeFields;

                // NOTE: the EmailTemplate may also have TO: defined, so even if there are no notificationEmailAddress defined for this specific job, we still should send the mail
                foreach ( var notificationEmailAddress in notificationEmailAddresses )
                {
                    emailMessage.AddRecipient( new RecipientData( notificationEmailAddress ) );
                }

                emailMessage.Send();
            }
        }
    }
}
