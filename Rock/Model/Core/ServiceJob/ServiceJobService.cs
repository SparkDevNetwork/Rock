﻿// <copyright>
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
using System.Web.Compilation;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Rock.Data;
using Rock.Jobs;

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
        /// Schedules the Job to run immediately using the Quartz Scheduler
        /// and waits for the job to finish.
        /// Returns <c>false</c> with an <c>out</c> <paramref name="errorMessage"/> if the job is already running as a RunNow job or if an exception occurs.
        /// NOTE: This will take at least 10 seconds to ensure the Quartz scheduler successfully started the job, plus any additional time that might
        /// still be needed to complete the job.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public bool RunNow( ServiceJob job, out string errorMessage )
        {
            // use a new RockContext instead of using this.Context so we can SaveChanges without affecting other RockContext's with pending changes.
            var rockContext = new RockContext();
            errorMessage = string.Empty;
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
                    errorMessage = "Job already running as a RunNow job";
                    return false;
                }

                // Check if another scheduler is running this job
                try
                {
                    var otherSchedulers = new StdSchedulerFactory()
                        .AllSchedulers
                        .Where( s => s.SchedulerName != runNowSchedulerName );

                    foreach ( var scheduler in otherSchedulers )
                    {
                        var isAlreadyRunning = scheduler.GetCurrentlyExecutingJobs()
                            .Where( j =>
                                j.JobDetail.Description == jobId.ToString() &&
                                j.JobDetail.ConcurrentExectionDisallowed )
                            .Any();

                        if ( isAlreadyRunning )
                        {
                            // A job with that Id is already running and ConcurrentExectionDisallowed is true
                            errorMessage = $" Scheduler '{scheduler.SchedulerName}' is already executing job Id '{jobId}' (name: {job.Name})";
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

                return true;
            }
            catch ( Exception ex )
            {
                // create a friendly error message
                ExceptionLogService.LogException( ex, null );
                errorMessage = string.Format( "Error doing a 'Run Now' on job: {0}. \n\n{2}", job.Name, job.Assembly, ex.Message );
                job.LastStatusMessage = errorMessage;
                job.LastStatus = "Error Loading Job";
                rockContext.SaveChanges();

                var jobHistoryService = new ServiceJobHistoryService( rockContext );
                var jobHistory = new ServiceJobHistory
                {
                    ServiceJobId = job.Id,
                    StartDateTime = RockDateTime.Now,
                    StopDateTime = RockDateTime.Now,
                    Status = job.LastStatus,
                    StatusMessage = job.LastStatusMessage
                };

                jobHistoryService.Add( jobHistory );
                rockContext.SaveChanges();

                return false;
            }

        }

        /// <summary>
        /// Builds a Quartz Job for a specified <see cref="Rock.Model.ServiceJob">Job</see>
        /// </summary>
        /// <param name="job">The <see cref="Rock.Model.ServiceJob"/> to create a Quarts Job for.</param>
        /// <returns>A object that implements the <see cref="Quartz.IJobDetail"/> interface</returns>
        public IJobDetail BuildQuartzJob( ServiceJob job )
        {
            // build the type object, will depend if the class is in an assembly or the App_Code folder
            Type type = null;

            if ( string.IsNullOrWhiteSpace( job.Assembly ) )
            {
                // first, if no assembly is known, look in all the dlls for it
                type = Rock.Reflection.FindType( typeof( Quartz.IJob ), job.Class );

                if ( type == null )
                {
                    // if it can't be found in dlls, look in App_Code using BuildManager
                    type = BuildManager.GetType( job.Class, false );
                }
            }
            else
            {
                // if an assembly is specified, load the type from that
                string thetype = string.Format( "{0}, {1}", job.Class, job.Assembly );
                type = Type.GetType( thetype );
            }

            int? jobEntityTypeId = Rock.Web.Cache.EntityTypeCache.Get( "Rock.Model.ServiceJob" ).Id;
            Rock.Attribute.Helper.UpdateAttributes( type, jobEntityTypeId, "Class", type.FullName );

            // load up job attributes (parameters) 
            job.LoadAttributes();

            JobDataMap map = new JobDataMap();

            foreach ( var attrib in job.AttributeValues )
            {
                map.Add( attrib.Key, attrib.Value.Value );
            }

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
            // create quartz trigger
            ITrigger trigger = ( ICronTrigger ) TriggerBuilder.Create()
                .WithIdentity( job.Guid.ToString(), job.Name )
                .WithCronSchedule( job.CronExpression, x =>
                {
                    x.InTimeZone( RockDateTime.OrgTimeZoneInfo );
                    x.WithMisfireHandlingInstructionDoNothing();
                } )
                .StartNow()
                .Build();

            return trigger;
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
    }
}
