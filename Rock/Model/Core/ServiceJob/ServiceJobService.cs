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
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

using Rock.Data;
using Rock.Jobs;
using CronExpressionDescriptor;
using Rock.Attribute;
using System.Collections.Generic;
using Rock.ViewModels.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.ServiceJob"/> entity objects.
    /// </summary>
    public partial class ServiceJobService
    {
        /// <summary>
        /// Returns a queryable collection of active <see cref="Rock.Model.ServiceJob">Jobs</see>
        /// </summary>
        /// <returns>A queryable collection that contains all active <see cref="Rock.Model.ServiceJob">Jobs</see></returns>
        public IQueryable<ServiceJob> GetActiveJobs()
        {
            return Queryable().Where( t => t.IsActive == true );
        }

        /// <summary>
        /// Returns a queryable collection of all <see cref="Rock.Model.ServiceJob">Jobs</see>
        /// </summary>
        /// <returns>A queryable collection of all <see cref="Rock.Model.ServiceJob"/>Jobs</returns>
        public IQueryable<ServiceJob> GetAllJobs()
        {
            return Queryable();
        }

        /// <summary>
        /// Schedules the Job to run immediately and waits for the job to finish.
        /// Returns <c>false</c> with an <c>out</c> if the job is already running as a RunNow job or if an exception occurs.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        [Obsolete]
        [RockObsolete( "1.15" )]
        public bool RunNow( ServiceJob job, out string errorMessage )
        {
            if ( RunNow( job ) )
            {
                errorMessage = string.Empty;
                return true;
            }
            else
            {
                errorMessage = "Unable to run job.";
                return false;
            }
        }

        /// <summary>
        /// Runs the now.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool RunNow( ServiceJob job )
        {
            var task = Task.Run( async () => await RunNowAsync( job ) );
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// Schedules the Job to run immediately using the Quartz Scheduler
        /// and waits for the job to finish.
        /// Returns <c>false</c> with an <c>out</c> if the job is already running as a RunNow job or if an exception occurs.
        /// NOTE: This will take at least 10 seconds to ensure the Quartz scheduler successfully started the job, plus any additional time that might
        /// still be needed to complete the job.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <returns></returns>
        internal static async Task<bool> RunNowAsync( ServiceJob job )
        {
            // use a new RockContext instead of using this.Context so we can SaveChanges without affecting other RockContext's with pending changes.
            var rockContext = new RockContext();

            try
            {
                // create a scheduler specific for the job
                var scheduleConfig = new NameValueCollection();

                var jobId = job.Id;

                var runNowSchedulerName = ( "RunNow:" + job.Guid.ToString( "N" ) ).Truncate( 40 );
                scheduleConfig.Add( StdSchedulerFactory.PropertySchedulerInstanceName, runNowSchedulerName );
                var schedulerFactory = new StdSchedulerFactory( scheduleConfig );
                var sched = new StdSchedulerFactory( scheduleConfig ).GetScheduler();

                if ( sched.IsStarted )
                {
                    // the job is currently running as a RunNow job
                    return false;
                }

                // Check if another scheduler is running this job
                try
                {
                    var allSchedulers = new StdSchedulerFactory().AllSchedulers;
                    var otherSchedulers = allSchedulers
                        .Where( s => s.SchedulerName != runNowSchedulerName );

                    foreach ( var scheduler in otherSchedulers )
                    {
                        var currentlyExecutingJobs = scheduler.GetCurrentlyExecutingJobs();
                        var isAlreadyRunning = currentlyExecutingJobs
                            .Where( j =>
                                j.JobDetail.Description == jobId.ToString() &&
                                j.JobDetail.ConcurrentExectionDisallowed )
                            .Any();

                        if ( isAlreadyRunning )
                        {
                            // A job with that Id is already running and ConcurrentExectionDisallowed is true
                            var errorMessage = $" Scheduler '{scheduler.SchedulerName}' is already executing job Id '{jobId}' (name: {job.Name})";
                            System.Diagnostics.Debug.WriteLine( $"{RockDateTime.Now.ToString()} {errorMessage}" );
                            return false;
                        }
                    }
                }
                catch
                {
                    // Was blank in the RunJobNowTransaction (intentional?)
                }

                // create the quartz job and trigger
                var jobDetail = new ServiceJobService( rockContext ).BuildQuartzJob( job );
                var jobDataMap = jobDetail.JobDataMap;

                if ( jobDataMap != null )
                {
                    // Force the <string, string> dictionary so that within Jobs, it is always okay to use
                    // JobDataMap.GetString(). This mimics Rock attributes always being stored as strings.
                    // If we allow non-strings, like integers, then JobDataMap.GetString() throws an exception.
                    jobDetail.JobDataMap.PutAll( jobDataMap.ToDictionary( kvp => kvp.Key, kvp => ( object ) kvp.Value ) );
                }

                var jobTrigger = TriggerBuilder.Create()
                    .WithIdentity( job.Guid.ToString(), job.Name )
                    .StartNow()
                    .Build();

                // schedule the job
                sched.ScheduleJob( jobDetail, jobTrigger );

                // set up the listener to report back from the job when it completes
                sched.ListenerManager.AddJobListener( new RockJobListener(), EverythingMatcher<JobKey>.AllJobs() );

                // start the scheduler
                sched.Start();

                // Wait 10secs to give scheduler to start the job.
                // If we don't do this, the scheduler might Shutdown thinking there are no running jobs
                Task.Delay( 10 * 1000 ).Wait();

                // stop the scheduler when done with job
                sched.Shutdown( true );

                return await Task.FromResult( true );
            }
            catch ( Exception ex )
            {
                // create a friendly error message
                ExceptionLogService.LogException( ex, null );
                var errorMessage = string.Format( "Error doing a 'Run Now' on job: {0}. \n\n{2}", job.Name, job.Assembly, ex.Message );
                job.LastStatusMessage = errorMessage;
                job.LastStatus = "Error Loading Job";
                rockContext.SaveChanges();

                var jobHistoryService = new ServiceJobHistoryService( rockContext );
                jobHistoryService.AddErrorServiceJobHistory( job );
                rockContext.SaveChanges();

                return await Task.FromResult( false );
            }

        }

        private static ConcurrentDictionary<string, bool> _verifiedJobTypeAttributes = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// Updates the attributes on the Job's Job Type if they haven't been verified yet
        /// </summary>
        /// <param name="jobCompiledType">Type of the job compiled.</param>
        public static void UpdateAttributesIfNeeded( Type jobCompiledType )
        {
            if ( !_verifiedJobTypeAttributes.ContainsKey( jobCompiledType.FullName ) )
            {
                int? jobEntityTypeId = Rock.Web.Cache.EntityTypeCache.Get( "Rock.Model.ServiceJob" ).Id;
                Rock.Attribute.Helper.UpdateAttributes( jobCompiledType, jobEntityTypeId, "Class", jobCompiledType.FullName );
                _verifiedJobTypeAttributes.TryAdd( jobCompiledType.FullName, true );
            }
        }

        /// <summary>
        /// Builds a Quartz Job for a specified <see cref="Rock.Model.ServiceJob">Job</see>
        /// </summary>
        /// <param name="job">The <see cref="Rock.Model.ServiceJob"/> to create a Quarts Job for.</param>
        /// <returns>A object that implements the <see cref="Quartz.IJobDetail"/> interface</returns>
        public IJobDetail BuildQuartzJob( ServiceJob job )
        {
            var type = job.GetCompiledType();
            if ( type == null )
            {
                return null;
            }

            UpdateAttributesIfNeeded( type );

#pragma warning disable CS0618 // Type or member is obsolete
            JobDataMap map = new JobDataMap();
#pragma warning restore CS0618 // Type or member is obsolete

            // create the quartz job object
            IJobDetail jobDetail = JobBuilder.Create( type )
            .WithDescription( job.Id.ToString() )
            .WithIdentity( job.Guid.ToString(), job.Name )
            .UsingJobData( map )
            .Build();

            return jobDetail;
        }

        /// <summary>
        /// Builds a Quartz schedule trigger
        /// </summary>
        /// <param name="job">The <see cref="Rock.Model.ServiceJob">Job</see> to create a <see cref="Quartz.ITrigger"/> compatible Trigger.</param>
        /// <returns>A Quartz trigger that implements <see cref="Quartz.ITrigger"/> for the specified job.</returns>
        public ITrigger BuildQuartzTrigger( ServiceJob job )
        {

            string cronExpression;
            if ( IsValidCronDescription( job.CronExpression ) )
            {
                cronExpression = job.CronExpression;
            }
            else
            {
                // Invalid cron expression, so specify to never run.
                // If they view the job in ScheduledJobDetail they'll see that it isn't a valid expression.
                cronExpression = ServiceJob.NeverScheduledCronExpression;
            }

            // create quartz trigger
            ITrigger trigger = ( ICronTrigger ) TriggerBuilder.Create()
                .WithIdentity( job.Guid.ToString(), job.Name )
                .WithCronSchedule( cronExpression, x =>
                {
                    x.InTimeZone( RockDateTime.OrgTimeZoneInfo );
                    x.WithMisfireHandlingInstructionDoNothing();
                } )
                .StartNow()
                .Build();

            return trigger;
        }

        /// <summary>
        /// Determines whether the Cron Expression is valid for Quartz
        /// </summary>
        /// <param name="cronExpression">The cron expression.</param>
        /// <returns>bool.</returns>
        public static bool IsValidCronDescription( string cronExpression )
        {
            return Quartz.CronExpression.IsValidExpression( cronExpression );
        }

        /// <summary>
        /// Gets a friendly cron description, or 'Invalid Cron Expression' if it isn't valid.
        /// </summary>
        /// <param name="cronExpression">The cron expression.</param>
        /// <returns>string.</returns>
        public static string GetCronDescription( string cronExpression )
        {
            if ( Quartz.CronExpression.IsValidExpression( cronExpression ) )
            {
                try
                {
                    return ExpressionDescriptor.GetDescription( cronExpression, new Options { ThrowExceptionOnParseError = true } );
                }
                catch
                {
                    return "Invalid Cron Expression";
                }
            }
            else
            {
                return "Invalid Cron Expression";
            }
        }

        /// <summary>
        /// Get the job types
        /// </summary>
        internal static List<ListItemBag> GetJobTypes()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var jobs = Rock.Reflection.FindTypes( typeof( Quartz.IJob ) ).Values
                .Union( Rock.Reflection.FindTypes( typeof( RockJob ) ).Values )
                .Distinct();
#pragma warning restore CS0618 // Type or member is obsolete
            var jobsList = jobs.ToList();
            foreach ( var job in jobs )
            {
                try
                {
                    ServiceJobService.UpdateAttributesIfNeeded( job );
                }
                catch
                {
                    jobsList.Remove( job );
                }
            }

            var jobTypes = new List<ListItemBag>();
            foreach ( var job in jobsList.OrderBy( a => a.FullName ) )
            {
                jobTypes.Add( new ListItemBag { Text = GetJobTypeFriendlyName( job.FullName ), Value = job.FullName } );
            }

            return jobTypes;
        }

        /// <summary>
        /// Get Job Type Friendly Name
        /// </summary>
        private static string GetJobTypeFriendlyName( string jobType )
        {
            string friendlyName;
            if ( jobType.Contains( "Rock.Jobs." ) )
            {
                friendlyName = jobType.Replace( "Rock.Jobs.", string.Empty ).SplitCase();
            }
            else
            {
                friendlyName = string.Format( "{0} (Plugin)", jobType.Split( '.' ).Last().SplitCase() );
            }
            return friendlyName;
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        public static void DeleteJob( int jobId )
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( jobId );

                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                    return;
                }
            }
        }

        /// <summary>
        /// Starts the quartz scheduler.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.15" )]
        public static void StartQuartzScheduler()
        {
            QuartzScheduler?.Start();
        }

        /// <summary>
        /// Shutdowns the quartz scheduler.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.15" )]
        public static void ShutdownQuartzScheduler()
        {
            QuartzScheduler?.Shutdown();
        }

        /// <summary>
        /// Gets the quartz scheduler.
        /// </summary>
        /// <value>The quartz scheduler.</value>
        internal static IScheduler QuartzScheduler { get; private set; } = null;

        internal static void InitializeJobScheduler()
        {
            using ( var rockContext = new RockContext() )
            {
                // create scheduler
                ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
                QuartzScheduler = schedulerFactory.GetScheduler();

                // get list of active jobs
                ServiceJobService jobService = new ServiceJobService( rockContext );
                var activeJobList = jobService.GetActiveJobs().OrderBy( a => a.Name ).ToList();
                foreach ( ServiceJob job in activeJobList )
                {
                    const string ErrorLoadingStatus = "Error Loading Job";

                    try
                    {
                        IJobDetail jobDetail = jobService.BuildQuartzJob( job );
                        if ( jobDetail == null )
                        {
                            continue;
                        }

                        ITrigger jobTrigger = jobService.BuildQuartzTrigger( job );

                        // Schedule the job (unless the cron expression is set to never run for an on-demand job like rebuild streaks)
                        if ( job.CronExpression != ServiceJob.NeverScheduledCronExpression )
                        {
                            QuartzScheduler.ScheduleJob( jobDetail, jobTrigger );
                        }

                        //// if the last status was an error, but we now loaded successful, clear the error
                        // also, if the last status was 'Running', clear that status because it would have stopped if the app restarted
                        if ( job.LastStatus == ErrorLoadingStatus || job.LastStatus == "Running" )
                        {
                            job.LastStatusMessage = string.Empty;
                            job.LastStatus = string.Empty;
                            rockContext.SaveChanges();
                        }
                    }
                    catch ( Exception exception )
                    {
                        // create a friendly error message
                        string message = $"Error loading the job: {job.Name}.\n\n{exception.Message}";

                        // log the error
                        var startupException = new Exception( message, exception );

                        ExceptionLogService.LogException( startupException, null );

                        job.LastStatusMessage = message;
                        job.LastStatus = ErrorLoadingStatus;
                        job.LastStatus = ErrorLoadingStatus;
                        rockContext.SaveChanges();

                        var jobHistoryService = new ServiceJobHistoryService( rockContext );
                        jobHistoryService.AddErrorServiceJobHistory( job );
                        rockContext.SaveChanges();
                    }
                }

                // set up the listener to report back from jobs as they complete
                QuartzScheduler.ListenerManager.AddJobListener( new RockJobListener(), EverythingMatcher<JobKey>.AllJobs() );

                // set up a trigger listener that can prevent a job from running if another scheduler is
                // already running it (i.e., someone running it manually).
                QuartzScheduler.ListenerManager.AddTriggerListener( new RockTriggerListener(), EverythingMatcher<JobKey>.AllTriggers() );

                // start the scheduler
                // Note, wait to start until Rock is fully started.
            }
        }
    }
}
