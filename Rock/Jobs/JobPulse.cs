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
using System.ComponentModel;
using System.Linq;

using Quartz;
using Quartz.Impl.Matchers;

using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// </summary>
    [DisplayName( "Job Pulse" )]
    [Description( "System job that allows Rock to monitor the jobs engine." )]


    [BooleanField(
        "Save JobPulse DateTime to Global Attributes",
        Description = "If checked, a Global Attribute called 'JobPulse' will be updated to indicate the most recent time that the JobPulse job ran. This can usually be left disabled unless a 3rd party plugin needs it.",
        Key = AttributeKey.SetJobPulseDateTimeGlobalAttribute,
        DefaultBooleanValue = false,
        Order = 0 )]

    [DisallowConcurrentExecution]
    public class JobPulse : IJob
    {

        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string SetJobPulseDateTimeGlobalAttribute = "SetJobPulseDateTimeGlobalAttribute";
        }

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public JobPulse()
        {
        }

        /// <summary> 
        /// System job that allows Rock to monitor the jobs engine
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            context.GetJobId();

            var setJobPulseDateTimeGlobalAttribute = dataMap.GetString( AttributeKey.SetJobPulseDateTimeGlobalAttribute ).AsBoolean();

            if ( setJobPulseDateTimeGlobalAttribute )
            {
                var globalAttributesCache = GlobalAttributesCache.Get();

                // Update a JobPulse global attribute value so that 3rd Party plugins could query this value in case they need to know
                globalAttributesCache.SetValue( "JobPulse", RockDateTime.Now.ToString(), true );
            }

            UpdateScheduledJobs( context );
        }

        /// <summary>
        /// Updates the scheduled jobs.
        /// </summary>
        /// <param name="context">The context.</param>
        private void UpdateScheduledJobs( IJobExecutionContext context )
        {
            var scheduler = context.Scheduler;
            int jobsDeleted = 0;
            int jobsScheduleUpdated = 0;

            var rockContext = new Rock.Data.RockContext();
            ServiceJobService jobService = new ServiceJobService( rockContext );
            List<ServiceJob> activeJobList = jobService.GetActiveJobs().ToList();
            List<Quartz.JobKey> scheduledQuartzJobs = scheduler.GetJobKeys( GroupMatcher<JobKey>.GroupStartsWith( string.Empty ) ).ToList();

            // delete any jobs that are no longer exist (are are not set to active) in the database
            var quartsJobsToDelete = scheduledQuartzJobs.Where( a => !activeJobList.Any( j => j.Guid.Equals( a.Name.AsGuid() ) ) );
            foreach ( JobKey jobKey in quartsJobsToDelete )
            {
                scheduler.DeleteJob( jobKey );
                jobsDeleted++;
            }

            // add any jobs that are not yet scheduled
            var newActiveJobs = activeJobList.Where( a => !scheduledQuartzJobs.Any( q => q.Name.AsGuid().Equals( a.Guid ) ) );
            foreach ( Rock.Model.ServiceJob job in newActiveJobs )
            {
                const string errorSchedulingStatus = "Error scheduling Job";
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
                        scheduler.ScheduleJob( jobDetail, jobTrigger );
                        jobsScheduleUpdated++;
                    }

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
                    rockContext.SaveChanges();
                }
            }

            // reload the jobs in case any where added/removed
            scheduledQuartzJobs = scheduler.GetJobKeys( GroupMatcher<JobKey>.GroupStartsWith( string.Empty ) ).ToList();

            // update schedule if the schedule has changed
            foreach ( var jobKey in scheduledQuartzJobs )
            {
                var jobCronTrigger = scheduler.GetTriggersOfJob( jobKey ).OfType<ICronTrigger>().FirstOrDefault();
                var activeJob = activeJobList.FirstOrDefault( a => a.Guid.Equals( jobKey.Name.AsGuid() ) );
                if ( jobCronTrigger == null || activeJob == null )
                {
                    continue;
                }

                bool rescheduleJob = false;

                // fix up the schedule if it has changed
                if ( activeJob.CronExpression != jobCronTrigger.CronExpressionString )
                {
                    rescheduleJob = true;
                }
                else
                {
                    // update the job detail if it has changed
                    IJobDetail scheduledJobDetail = scheduler.GetJobDetail( jobKey );
                    var activeJobType = activeJob.GetCompiledType();

                    if ( scheduledJobDetail != null && activeJobType != null )
                    {
                        if ( scheduledJobDetail.JobType != activeJobType )
                        {
                            rescheduleJob = true;
                        }
                    }
                }

                if ( rescheduleJob )
                {
                    const string errorReschedulingStatus = "Error re-scheduling Job";
                    try
                    {
                        IJobDetail jobDetail = jobService.BuildQuartzJob( activeJob );
                        ITrigger newJobTrigger = jobService.BuildQuartzTrigger( activeJob );
                        bool deletedSuccessfully = scheduler.DeleteJob( jobKey );
                        scheduler.ScheduleJob( jobDetail, newJobTrigger );
                        jobsScheduleUpdated++;

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

            context.Result = string.Empty;

            if ( jobsDeleted > 0 )
            {
                context.Result += string.Format( "Deleted {0} job schedule(s)", jobsDeleted );
            }

            if ( jobsScheduleUpdated > 0 )
            {
                context.Result += ( string.IsNullOrEmpty( context.Result as string ) ? "" : " and " ) + string.Format( "Updated {0} schedule(s)", jobsScheduleUpdated );
            }
        }
    }
}