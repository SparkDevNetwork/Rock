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
using Quartz;

using DotLiquid;

using Rock.Communication;
using Rock.Data;
using Rock.Lava;
using Rock.Logging;
using Rock.Model;
using Rock.Bus;
using Rock.Observability;
using System.Diagnostics;

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
        /// Called by the <see cref="IScheduler" /> when a <see cref="IJobDetail" />
        /// is about to be executed (an associated <see cref="ITrigger" />
        /// has occurred).
        /// <para>
        /// This method will not be invoked if the execution of the Job was vetoed
        /// by a <see cref="ITriggerListener" />.
        /// </para>
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        /// <seealso cref="M:Quartz.IJobListener.JobExecutionVetoed(Quartz.IJobExecutionContext,System.Threading.CancellationToken)" />
        public void JobToBeExecuted( IJobExecutionContext context )
        {
            // get job type id
            int jobId = context.JobDetail.Description.AsInteger();

            RockLogger.Log.Debug( RockLogDomains.Jobs, "Job ID: {jobId}, Job Key: {jobKey}, Job is about to be executed.", jobId, context.JobDetail?.Key );

            // load job
            var rockContext = new RockContext();
            var jobService = new ServiceJobService( rockContext );
            var job = jobService.Get( jobId );

            if ( job != null && job.Guid != Rock.SystemGuid.ServiceJob.JOB_PULSE.AsGuid() )
            {
                var now = RockDateTime.Now;
                job.LastStatus = "Running";
                job.LastStatusMessage = "Started at " + now.ToString();

                /* 
                     5/25/2023 - JMH
                     
                     Before the job executes, a partial "started" ServiceJobHistory record is created.
                     After the job is executed, the ServiceJobHistory record's status, started,
                     and stopped date times will be updated to match the job's last run.
                     
                     The job scheduler does not expose the job execution's actual start or stop time,
                     but it does expose the execution's run duration (in seconds) once the job is executed
                     (available in the "JobWasExecuted" callback).
                     
                     In the "JobWasExecuted" callback, we update the ServiceJob.LastRunDurationSeconds value
                     to the actual run duration returned by the scheduler, and the ServiceJob.LastRunDateTime
                     to the current system time. The last run start time is not stored in the ServiceJob.
                     
                     Lastly, the ServiceJobHistory data will be updated to match the ServiceJob's last run data.
                     
                     Reason: Rock Jobs Scheduler                     
                 */
                var jobHistoryService = new ServiceJobHistoryService( rockContext );
                jobHistoryService.AddStartedServiceJobHistory( job, now );

                rockContext.SaveChanges();
            }

#pragma warning disable CS0612 // Type or member is obsolete
            context.JobDetail.JobDataMap.LoadFromJobAttributeValues( job );
#pragma warning restore CS0612 // Type or member is obsolete

            // Add job observability if this is a legacy job.
            if ( !( context.JobInstance is RockJob ) )
            {
                var activity = ObservabilityHelper.StartActivity( $"JOB: {job.Class.Replace( "Rock.Jobs.", "" )} - {job.Name}" );
                activity?.AddTag( "rock.otel_type", "rock-job" );
                activity?.AddTag( "rock.job.id", job.Id );
                activity?.AddTag( "rock.job.type", job.Class.Replace( "Rock.Jobs.", "" ) );
                activity?.AddTag( "rock.job.description", job.Description );
            }
        }

        /// <summary>
        /// Called by the <see cref="IScheduler" /> when a <see cref="IJobDetail" />
        /// was about to be executed (an associated <see cref="ITrigger" />
        /// has occurred), but a <see cref="ITriggerListener" /> vetoed its
        /// execution.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        /// <seealso cref="M:Quartz.IJobListener.JobToBeExecuted(Quartz.IJobExecutionContext,System.Threading.CancellationToken)" />
        public void JobExecutionVetoed( IJobExecutionContext context )
        {
            RockLogger.Log.Debug( RockLogDomains.Jobs, "Job ID: {jobId}, Job Key: {jobKey}, Job was vetoed.", context.JobDetail?.Description.AsIntegerOrNull(), context.JobDetail?.Key );
        }

        /// <summary>
        /// Called by the <see cref="IScheduler" /> after a <see cref="IJobDetail" />
        /// has been executed, and before the associated <see cref="Quartz.Spi.IOperableTrigger" />'s
        /// <see cref="Quartz.Spi.IOperableTrigger.Triggered" /> method has been called.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="jobException">The job exception.</param>
        /// <returns>Task.</returns>
        public void JobWasExecuted( IJobExecutionContext context, JobExecutionException jobException )
        {
            // get job id
#pragma warning disable CS0612 // Type or member is obsolete
            int jobId = context.GetJobId();
#pragma warning restore CS0612 // Type or member is obsolete

            var rockJobInstance = context.JobInstance as RockJob;

            // Complete the observability if this is a legacy job.
            if ( !( context.JobInstance is RockJob ) )
            {
                Activity.Current?.AddTag( "rock.job.duration", context.JobRunTime.TotalSeconds );
                Activity.Current?.AddTag( "rock.job.message", rockJobInstance?.Result ?? context.Result as string );
                Activity.Current?.AddTag( "rock.job.result", jobException == null ? "Success" : "Failed" );
                Activity.Current?.Dispose();
            }

            // load job
            var rockContext = new RockContext();
            var jobService = new ServiceJobService( rockContext );
            var job = jobService.Get( jobId );

            if ( job == null )
            {
                // if job was deleted or wasn't found, just exit
                RockLogger.Log.Debug( RockLogDomains.Jobs, "Job ID: {jobId}, Job Key: {jobKey}, Job was not found.", jobId, context.JobDetail?.Key );
                return;
            }

            // if notification status is all set flag to send message
            bool sendMessage = job.NotificationStatus == JobNotificationStatus.All;

            // set last run date
            job.LastRunDateTime = RockDateTime.Now;

            // set run time
            job.LastRunDurationSeconds = Convert.ToInt32( context.JobRunTime.TotalSeconds );

            // set the scheduler name
            job.LastRunSchedulerName = rockJobInstance?.Scheduler?.SchedulerName ?? context.Scheduler.SchedulerName;

            // determine if an error occurred
            if ( jobException == null )
            {
                job.LastSuccessfulRunDateTime = job.LastRunDateTime;
                job.LastStatus = "Success";

                var result = rockJobInstance?.Result ?? context.Result as string;
                job.LastStatusMessage = result ?? string.Empty;

                // determine if message should be sent
                if ( job.NotificationStatus == JobNotificationStatus.Success )
                {
                    sendMessage = true;
                }

                RockLogger.Log.Debug( RockLogDomains.Jobs, "Job ID: {jobId}, Job Key: {jobKey}, Job was executed.", jobId, context.JobDetail?.Key );
            }
            else
            {
                var exceptionToLog = GetExceptionToLog( jobException );

                var warningException = exceptionToLog as RockJobWarningException;

                // log the exception to the database (even if it is a RockJobWarningException)
                ExceptionLogService.LogException( exceptionToLog, null );

                if ( warningException == null )
                {
                    // put the exception into the status
                    job.LastStatus = "Exception";

                    AggregateException aggregateException = exceptionToLog as AggregateException;
                    if ( aggregateException != null && aggregateException.InnerExceptions != null && aggregateException.InnerExceptions.Count > 1 )
                    {
                        var firstException = aggregateException.InnerExceptions.First();
                        job.LastStatusMessage = "One or more exceptions occurred. First Exception: " + firstException.Message;
                    }
                    else
                    {
                        job.LastStatusMessage = exceptionToLog.Message;
                    }
                }
                else
                {
                    // if the this.Result hasn't been set, use the warningException.Message
                    job.LastStatus = "Warning";
                    job.LastStatusMessage = rockJobInstance?.Result ?? context.Result?.ToString() ?? warningException.Message;
                }

                if ( job.NotificationStatus == JobNotificationStatus.Error )
                {
                    sendMessage = true;
                }

                RockLogger.Log.Debug( RockLogDomains.Jobs, exceptionToLog, "Job ID: {jobId}, Job Key: {jobKey}, Job was executed with an exception.", jobId, context.JobDetail?.Key );
            }

            rockContext.SaveChanges();

            // Add job history
            var serviceJobHistoryService = new ServiceJobHistoryService( rockContext );
            serviceJobHistoryService.AddCompletedServiceJobHistory( job );
            rockContext.SaveChanges();

            // send notification
            if ( sendMessage )
            {
                SendNotificationMessage( jobException, job );
            }
        }

        private static void SendNotificationMessage( JobExecutionException jobException, ServiceJob job )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null, new Lava.CommonMergeFieldsOptions() );
            mergeFields.Add( "Job", job );
            try
            {
                if ( jobException != null )
                {
                    if ( LavaService.RockLiquidIsEnabled )
                    {
                        mergeFields.Add( "Exception", Hash.FromAnonymousObject( jobException ) );
                    }
                    else
                    {
                        mergeFields.Add( "Exception", LavaDataObject.FromAnonymousObject( jobException ) );
                    }
                }

            }
            catch
            {
                // ignore
            }

            var notificationEmailAddresses = job.NotificationEmails.ResolveMergeFields( mergeFields ).SplitDelimitedValues().ToList();
            var emailMessage = new RockEmailMessage( Rock.SystemGuid.SystemCommunication.CONFIG_JOB_NOTIFICATION.AsGuid() );
            emailMessage.AdditionalMergeFields = mergeFields;
            emailMessage.CreateCommunicationRecord = false;

            // NOTE: the EmailTemplate may also have TO: defined, so even if there are no notificationEmailAddress defined for this specific job, we still should send the mail
            foreach ( var notificationEmailAddress in notificationEmailAddresses )
            {
                emailMessage.AddRecipient( RockEmailMessageRecipient.CreateAnonymous( notificationEmailAddress, null ) );
            }

            emailMessage.Send();
        }

        private Exception GetExceptionToLog( JobExecutionException jobException )
        {
            Exception exceptionToLog = jobException;

            // drill down to the interesting exception
            while ( exceptionToLog is Quartz.SchedulerException && exceptionToLog.InnerException != null )
            {
                exceptionToLog = exceptionToLog.InnerException;
            }

            AggregateException aggregateException = exceptionToLog as AggregateException;
            if ( aggregateException != null && aggregateException.InnerExceptions != null && aggregateException.InnerExceptions.Count == 1 )
            {
                // if it's an aggregate, but there is only one, convert it to a single exception
                exceptionToLog = aggregateException.InnerExceptions[0];
                aggregateException = null;
            }

            return exceptionToLog;
        }


    }
}
