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
using System.Diagnostics;
using System.Linq;

using Microsoft.Extensions.Logging;

using Quartz;

using Rock.Bus.Locking;
using Rock.Communication;
using Rock.Data;
using Rock.Lava;
using Rock.Logging;
using Rock.Model;
using Rock.Observability;

namespace Rock.Jobs
{
    /// <summary>
    /// Summary description for JobListener
    /// </summary>
    [RockLoggingCategory]
    public class RockJobListener : IJobListener
    {
        /// <summary>
        /// The logger for this instance.
        /// </summary>
        private ILogger _logger;

        /// <summary>
        /// The execution context key that carries the Id of the ServiceJobHistory record created when the job started.
        /// </summary>
        private const string ServiceJobHistoryIdKey = "Rock.ServiceJobHistoryId";

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
        /// Gets the logger for this instance.
        /// </summary>
        /// <value>The logger for this instance.</value>
        protected ILogger Logger
        {
            get
            {
                if ( _logger == null )
                {
                    _logger = RockLogger.LoggerFactory.CreateLogger( GetType().FullName );
                }

                return _logger;
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

            Logger.LogDebug(
                "Job ID: {jobId} (App PID: {processId}-{domainId}), Job Key: {jobKey}, Job is about to be executed.",
                jobId,
                Rock.WebFarm.RockWebFarm.ProcessId,
                AppDomain.CurrentDomain.Id,
                context.JobDetail?.Key
            );

            // load job
            var rockContext = new RockContext();
            var jobService = new ServiceJobService( rockContext );
            var job = jobService.Get( jobId );

            if ( job != null && job.Guid != Rock.SystemGuid.ServiceJob.JOB_PULSE.AsGuid() )
            {
                // An exception escaping this listener would prevent Quartz from moving the job's trigger out of its
                // Blocked state, permanently stopping the job until Rock restarts. A bookkeeping failure is logged
                // instead so the job still runs.
                try
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
                    var jobHistory = jobHistoryService.AddStartedServiceJobHistory( job, now );

                    rockContext.SaveChanges();

                    // Carry the history record's Id to JobWasExecuted so the same record can be completed by primary key.
                    context.Put( ServiceJobHistoryIdKey, jobHistory.Id );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( new Exception( $"Unable to record the started status for the '{job.Name}' job (ID: {job.Id}).", ex ), null );
                }
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
        public virtual void JobExecutionVetoed( IJobExecutionContext context )
        {
            var jobId = context.JobDetail?.Description.AsIntegerOrNull();
            var jobKey = context.JobDetail?.Key;

            Logger.LogDebug(
                "Job ID: {jobId} (App PID: {processId}-{domainId}), Job Key: {jobKey}, Job was vetoed.",
                jobId,
                Rock.WebFarm.RockWebFarm.ProcessId,
                AppDomain.CurrentDomain.Id,
                jobKey
            );

            // Defensive: when the veto came from our own distributed-lock
            // check, RockTriggerListener does not stash a handle on the
            // context. But if a future code path ever stashes a handle and
            // then vetoes anyway, this call prevents the leak.
            ReleaseDistributedLock( context );
        }

        /// <summary>
        /// Called by the <see cref="IScheduler" /> after a <see cref="IJobDetail" />
        /// has been executed, and before the associated <see cref="Quartz.Spi.IOperableTrigger" />'s
        /// <see cref="Quartz.Spi.IOperableTrigger.Triggered" /> method has been called.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="jobException">The job exception.</param>
        /// <returns>Task.</returns>
        public virtual void JobWasExecuted( IJobExecutionContext context, JobExecutionException jobException )
        {
            // get job id
#pragma warning disable CS0612 // Type or member is obsolete
            int jobId = context.GetJobId();
#pragma warning restore CS0612 // Type or member is obsolete

            var rockJobInstance = context.JobInstance as RockJob;
            var jobKey = context.JobDetail?.Key;

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
                Logger.LogDebug(
                    "Job ID: {jobId} (App PID: {processId}-{domainId}), Job Key: {jobKey}, Job was not found.",
                    jobId,
                    Rock.WebFarm.RockWebFarm.ProcessId,
                    AppDomain.CurrentDomain.Id,
                    jobKey
                );
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

                Logger.LogDebug(
                    "Job ID: {jobId} (App PID: {processId}-{domainId}), Job Key: {jobKey}, Job was executed.",
                    jobId,
                    Rock.WebFarm.RockWebFarm.ProcessId,
                    AppDomain.CurrentDomain.Id,
                    jobKey
                );
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

                Logger.LogDebug(
                    exceptionToLog,
                    "Job ID: {jobId} (App PID: {processId}-{domainId}), Job Key: {jobKey}, Job was executed with an exception.",
                    jobId,
                    Rock.WebFarm.RockWebFarm.ProcessId,
                    AppDomain.CurrentDomain.Id,
                    jobKey
                );
            }

            // An exception escaping this listener would prevent Quartz from moving the job's trigger out of its
            // Blocked state, permanently stopping the job until Rock restarts. Each bookkeeping step below is
            // guarded independently so a failure is logged instead of thrown and does not skip the remaining steps.
            try
            {
                rockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new Exception( $"Unable to save the last run details for the '{job.Name}' job (ID: {job.Id}).", ex ), null );
            }

            // Add job history
            try
            {
                // A separate context is used so a failure saving the last run details above cannot poison the
                // history write.
                using ( var historyRockContext = new RockContext() )
                {
                    var serviceJobHistoryService = new ServiceJobHistoryService( historyRockContext );
                    var jobHistory = GetStartedServiceJobHistory( context, serviceJobHistoryService, job );

                    if ( jobHistory != null )
                    {
                        serviceJobHistoryService.CompleteServiceJobHistory( jobHistory, job );
                    }
                    else
                    {
                        // Fall back to finding (or creating) the history record by timestamp matching.
                        serviceJobHistoryService.AddCompletedServiceJobHistory( job );
                    }

                    historyRockContext.SaveChanges();
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new Exception( $"Unable to add the job history record for the '{job.Name}' job (ID: {job.Id}).", ex ), null );
            }

            // send notification
            if ( sendMessage )
            {
                try
                {
                    SendNotificationMessage( jobException, job );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( new Exception( $"Unable to send the notification message for the '{job.Name}' job (ID: {job.Id}).", ex ), null );
                }
            }

            // Release the distributed lock after all bookkeeping is done.
            // Holding through bookkeeping ensures only the lock winner
            // updates ServiceJob run-history columns; another node cannot
            // fire this job and race the same columns until we release.
            ReleaseDistributedLock( context );
        }

        /// <summary>
        /// Retrieves the distributed lock handle stashed on
        /// <paramref name="context"/> by
        /// <see cref="RockTriggerListener.VetoJobExecution"/> and disposes it,
        /// releasing the lock in SQL Server. Safe to call when no handle was
        /// stashed (returns without side effect). Never throws; any error is
        /// logged and swallowed so a Dispose failure cannot bubble up and
        /// leave Quartz in a bad state.
        /// </summary>
        private void ReleaseDistributedLock( IJobExecutionContext context )
        {
            var handle = context.Get( RockTriggerListener.DistributedLockHandleKey ) as ILockHandle;

            if ( handle == null )
            {
                return;
            }

            try
            {
                handle.Dispose();
            }
            catch ( Exception ex )
            {
                Logger.LogWarning( ex, "Failed to release distributed lock for Job ID {jobId}.", context.JobDetail?.Description.AsIntegerOrNull() );
            }
            finally
            {
                // Prevent double-release if this method is invoked twice for
                // the same context (defensive; JobWasExecuted is the intended
                // caller and only runs once per fire).
                context.Put( RockTriggerListener.DistributedLockHandleKey, null );
            }
        }

        /// <summary>
        /// Gets the ServiceJobHistory record created when this execution started, using the Id carried in the
        /// execution context. Returns null if the Id is missing or the record cannot be found, in which case the
        /// caller should fall back to timestamp matching.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="serviceJobHistoryService">The job history service.</param>
        /// <param name="job">The job.</param>
        /// <returns>The started job history record, or null if it could not be found.</returns>
        private ServiceJobHistory GetStartedServiceJobHistory( IJobExecutionContext context, ServiceJobHistoryService serviceJobHistoryService, ServiceJob job )
        {
            var jobHistoryId = context.Get( ServiceJobHistoryIdKey ) as int?;

            if ( !jobHistoryId.HasValue )
            {
                // The pulse job never creates a started history record, so a missing Id is expected for it.
                if ( job.Guid != Rock.SystemGuid.ServiceJob.JOB_PULSE.AsGuid() )
                {
                    Logger.LogWarning( "Job ID: {jobId}, no ServiceJobHistory Id was found in the execution context. Falling back to timestamp matching.", job.Id );
                }

                return null;
            }

            var jobHistory = serviceJobHistoryService.Get( jobHistoryId.Value );

            if ( jobHistory == null )
            {
                Logger.LogWarning( "Job ID: {jobId}, ServiceJobHistory Id {jobHistoryId} was not found. Waiting briefly and retrying once.", job.Id, jobHistoryId.Value );
                System.Threading.Thread.Sleep( 250 );
                jobHistory = serviceJobHistoryService.Get( jobHistoryId.Value );

                if ( jobHistory == null )
                {
                    Logger.LogWarning( "Job ID: {jobId}, ServiceJobHistory Id {jobHistoryId} was still not found after retrying. Falling back to timestamp matching.", job.Id, jobHistoryId.Value );
                }
            }

            return jobHistory;
        }

        private static void SendNotificationMessage( JobExecutionException jobException, ServiceJob job )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null, new Lava.CommonMergeFieldsOptions() );
            mergeFields.Add( "Job", job );
            try
            {
                if ( jobException != null )
                {
                    mergeFields.Add( "Exception", LavaDataObject.FromAnonymousObject( jobException ) );
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
