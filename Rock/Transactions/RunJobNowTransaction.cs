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
using Quartz.Impl;
using Quartz.Impl.Matchers;

using Rock.Data;
using Rock.Jobs;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Runs a job now
    /// </summary>
    public class RunJobNowTransaction : ITransaction
    {

        /// <summary>
        /// Gets or sets the job identifier.
        /// </summary>
        /// <value>
        /// The job identifier.
        /// </value>
        public int JobId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunJobNowTransaction"/> class.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        public RunJobNowTransaction(int jobId)
        {
            JobId = jobId;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );

                ServiceJob job = jobService.Get( JobId );
                if ( job != null )
                {
                    try
                    {
                        // create a scheduler specific for the job
                        var scheduleConfig = new System.Collections.Specialized.NameValueCollection();
                        var runNowSchedulerName = ( "RunNow:" + job.Guid.ToString( "N" ) ).Truncate( 40 );
                        scheduleConfig.Add( StdSchedulerFactory.PropertySchedulerInstanceName, runNowSchedulerName );
                        var schedulerFactory = new StdSchedulerFactory( scheduleConfig );
                        var sched = new StdSchedulerFactory( scheduleConfig ).GetScheduler();
                        if (sched.IsStarted)
                        {
                            // the job is currently running as a RunNow job
                            return;
                        }

                        // Check if another scheduler is running this job
                        try
                        {
                            var otherSchedulers = new Quartz.Impl.StdSchedulerFactory().AllSchedulers
                                .Where( s => s.SchedulerName != runNowSchedulerName );

                            foreach ( var scheduler in otherSchedulers )
                            {
                                if ( scheduler.GetCurrentlyExecutingJobs().Where( j => j.JobDetail.Description == JobId.ToString() &&
                                    j.JobDetail.ConcurrentExectionDisallowed ).Any() )
                                {
                                    // A job with that Id is already running and ConcurrentExectionDisallowed is true 
                                    return;
                                }
                            }
                        }
                        catch { }

                        // create the quartz job and trigger
                        IJobDetail jobDetail = jobService.BuildQuartzJob( job );
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
                        System.Threading.Tasks.Task.Delay( new TimeSpan( 0, 0, 10 ) ).Wait();

                        // stop the scheduler when done with job
                        sched.Shutdown( true );
                    }

                    catch ( Exception ex )
                    {
                        // create a friendly error message
                        ExceptionLogService.LogException( ex, null );
                        string message = string.Format( "Error doing a 'Run Now' on job: {0}. \n\n{2}", job.Name, job.Assembly, ex.Message );
                        job.LastStatusMessage = message;
                        job.LastStatus = "Error Loading Job";
                        rockContext.SaveChanges();

                        var jobHistoryService = new ServiceJobHistoryService( rockContext );
                        var jobHistory = new ServiceJobHistory()
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
        }
    }
}