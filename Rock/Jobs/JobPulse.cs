// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;
using Quartz;
using Quartz.Impl.Matchers;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to keep a heartbeat of the job process so we know when the jobs stop working
    /// </summary>
    [DisallowConcurrentExecution]
    public class JobPulse : IJob
    {
        /// <summary> 
        /// Empty constructor for job initilization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public JobPulse()
        {
        }

        /// <summary> 
        /// Job that updates the JobPulse setting with the current date/time.
        /// This will allow us to notify an admin if the jobs stop running.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            var globalAttributesCache = GlobalAttributesCache.Read();
            globalAttributesCache.SetValue( "JobPulse", RockDateTime.Now.ToString(), true );

            UpdateScheduledJobs( context );
        }

        /// <summary>
        /// Updates the scheduled jobs.
        /// </summary>
        /// <param name="context">The context.</param>
        private void UpdateScheduledJobs( IJobExecutionContext context )
        {
            var scheduler = context.Scheduler;

            var rockContext = new Rock.Data.RockContext();
            ServiceJobService jobService = new ServiceJobService( rockContext );
            List<ServiceJob> activeJobList = jobService.GetActiveJobs().ToList();
            List<Quartz.JobKey> scheduledQuartzJobs = scheduler.GetJobKeys( GroupMatcher<JobKey>.GroupStartsWith( string.Empty ) ).ToList();

            // delete any jobs that are no longer exist (are are not set to active) in the database
            var quartsJobsToDelete = scheduledQuartzJobs.Where( a => !activeJobList.Any( j => j.Guid.Equals( a.Name.AsGuid() ) ) );
            foreach ( JobKey jobKey in quartsJobsToDelete )
            {
                scheduler.DeleteJob( jobKey );
            }

            // add any jobs that are not yet scheduled
            var newActiveJobs = activeJobList.Where( a => !scheduledQuartzJobs.Any( q => q.Name.AsGuid().Equals( a.Guid ) ) );
            foreach ( Rock.Model.ServiceJob job in newActiveJobs )
            {
                const string errorSchedulingStatus = "Error scheduling Job";
                try
                {
                    IJobDetail jobDetail = jobService.BuildQuartzJob( job );
                    ITrigger jobTrigger = jobService.BuildQuartzTrigger( job );

                    scheduler.ScheduleJob( jobDetail, jobTrigger );

                    if ( job.LastStatus == errorSchedulingStatus )
                    {
                        job.LastStatusMessage = string.Empty;
                        job.LastStatus = string.Empty;
                        rockContext.SaveChanges();
                    }
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex, null );

                    // create a friendly error message
                    string message = string.Format( "Error scheduling the job: {0}.\n\n{2}", job.Name, job.Assembly, ex.Message );
                    job.LastStatusMessage = message;
                    job.LastStatus = errorSchedulingStatus;
                }
            }

            rockContext.SaveChanges();

            // reload the jobs in case any where added/removed
            scheduledQuartzJobs = scheduler.GetJobKeys( GroupMatcher<JobKey>.GroupStartsWith( string.Empty ) ).ToList();

            // update schedule if the schedule has changed
            foreach ( var jobKey in scheduledQuartzJobs )
            {
                var jobCronTrigger = scheduler.GetTriggersOfJob( jobKey ).OfType<ICronTrigger>().FirstOrDefault();
                if ( jobCronTrigger != null )
                {
                    var activeJob = activeJobList.FirstOrDefault( a => a.Guid.Equals( jobKey.Name.AsGuid() ) );
                    if ( activeJob != null )
                    {
                        bool rescheduleJob = false;

                        // fix up the schedule if it has changed
                        if ( activeJob.CronExpression != jobCronTrigger.CronExpressionString )
                        {
                            rescheduleJob = true;
                        }

                        // update the job detail if it has changed
                        var scheduledJobDetail = scheduler.GetJobDetail( jobKey );
                        var jobDetail = jobService.BuildQuartzJob( activeJob );

                        if ( scheduledJobDetail != null && jobDetail != null )
                        {
                            if ( scheduledJobDetail.JobType != jobDetail.JobType )
                            {
                                rescheduleJob = true;
                            }

                            if ( scheduledJobDetail.JobDataMap.ToJson() != jobDetail.JobDataMap.ToJson() )
                            {
                                rescheduleJob = true;
                            }
                        }

                        if ( rescheduleJob )
                        {
                            const string errorReschedulingStatus = "Error re-scheduling Job";
                            try
                            {
                                ITrigger newJobTrigger = jobService.BuildQuartzTrigger( activeJob );
                                bool deletedSuccessfully = scheduler.DeleteJob( jobKey );
                                scheduler.ScheduleJob( jobDetail, newJobTrigger );

                                if ( activeJob.LastStatus == errorReschedulingStatus )
                                {
                                    activeJob.LastStatusMessage = string.Empty;
                                    activeJob.LastStatus = string.Empty;
                                    rockContext.SaveChanges();
                                }
                            }
                            catch ( Exception ex )
                            {
                                ExceptionLogService.LogException( ex, null );

                                // create a friendly error message
                                string message = string.Format( "Error re-scheduling the job: {0}.\n\n{2}", activeJob.Name, activeJob.Assembly, ex.Message );
                                activeJob.LastStatusMessage = message;
                                activeJob.LastStatus = errorReschedulingStatus;
                            }
                        }
                    }
                }
            }
        }
    }
}