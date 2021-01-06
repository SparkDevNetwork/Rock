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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;

namespace Rock.Tasks
{
    /// <summary>
    /// Runs a job now (rather than on a regular job schedule).
    /// </summary>
    public sealed class ProcessRunJobNow : BusStartedTask<ProcessRunJobNow.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( message.JobId );

                if ( job == null )
                {
                    return;
                }

                try
                {
                    // create a scheduler specific for the job
                    var scheduleConfig = new NameValueCollection();
                    var runNowSchedulerName = ( "RunNow:" + job.Guid.ToString( "N" ) ).Truncate( 40 );
                    scheduleConfig.Add( StdSchedulerFactory.PropertySchedulerInstanceName, runNowSchedulerName );
                    var schedulerFactory = new StdSchedulerFactory( scheduleConfig );
                    var sched = new StdSchedulerFactory( scheduleConfig ).GetScheduler();

                    if ( sched.IsStarted )
                    {
                        // the job is currently running as a RunNow job
                        return;
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
                                    j.JobDetail.Description == message.JobId.ToString() &&
                                    j.JobDetail.ConcurrentExectionDisallowed )
                                .Any();

                            if ( isAlreadyRunning )
                            {
                                // A job with that Id is already running and ConcurrentExectionDisallowed is true
                                System.Diagnostics.Debug.WriteLine( RockDateTime.Now.ToString() + $" Scheduler '{scheduler.SchedulerName}' is already executing job Id '{message.JobId}' (name: {job.Name})" );
                                return;
                            }
                        }
                    }
                    catch
                    {
                        // Was blank in the RunJobNowTransaction (intentional?)
                    }

                    // create the quartz job and trigger
                    var jobDetail = jobService.BuildQuartzJob( job );

                    if ( message.JobDataMap != null )
                    {
                        // Force the <string, string> dictionary so that within Jobs, it is always okay to use
                        // JobDataMap.GetString(). This mimics Rock attributes always being stored as strings.
                        // If we allow non-strings, like integers, then JobDataMap.GetString() throws an exception.
                        jobDetail.JobDataMap.PutAll( message.JobDataMap.ToDictionary( kvp => kvp.Key, kvp => ( object ) kvp.Value ) );
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

                    // Wait 10secs to give job chance to start
                    Task.Delay( 10 * 1000 ).Wait();

                    // stop the scheduler when done with job
                    sched.Shutdown( true );
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
                }
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the job identifier.
            /// </summary>
            /// <value>
            /// The job identifier.
            /// </value>
            public int JobId { get; set; }

            /// <summary>
            /// Gets or sets the job data map.
            /// </summary>
            /// <value>
            /// The job data map.
            /// </value>
            public Dictionary<string, string> JobDataMap { get; set; }
        }
    }
}