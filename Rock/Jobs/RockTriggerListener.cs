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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Quartz;

using Rock.Logging;

namespace Rock.Jobs
{
    /// <summary>
    /// Implementation of the ITriggerListener so Rock can be informed when a ITrigger fires
    /// -- and namely make a decision about whether or not to veto a job that's about to run.
    /// This is primarily needed because running a job manually will create a new scheduler
    /// to do it.  This class's VetoJobExecution is the traffic cop that will query all
    /// schedulers to decide if a job should be vetoed from running.
    /// </summary>
    public class RockTriggerListener : ITriggerListener
    {
        /// <summary>
        /// The logger for this instance.
        /// </summary>
        private ILogger _logger;

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
        /// Get the name of the <see cref="ITriggerListener"/>.
        /// </summary>
        public string Name => "RockTriggerListener";

        /// <summary>
        /// Called by the <see cref="IScheduler" /> when a  <see cref="ITrigger" /> has fired, and it's
        /// associated <see cref="IJobDetail" /> is about to be executed. It is called after the
        /// TriggerFired(ITrigger, IJobExecutionContext) method of this interface.
        /// If the execution is vetoed (via returning true), the job's execute method will not be called.
        /// </summary>
        /// <param name="trigger">The trigger.</param>
        /// <param name="context">The context.</param>
        /// <returns>Returns true if job execution should be vetoed, false otherwise.</returns>
        public bool VetoJobExecution( ITrigger trigger, IJobExecutionContext context )
        {
            // get job type id
            int jobId = context.JobDetail.Description.AsInteger();

            var allSchedulers = new Quartz.Impl.StdSchedulerFactory().AllSchedulers;

            // Check if other schedulers are running this job...
            // TODO NOTE: Someday we would want to see if this also can work across
            // multiple 'hosts' in a Rock cluster else we should handle this explicitly.
            var otherSchedulers = allSchedulers
                .Where( s => s.SchedulerName != context.Scheduler.SchedulerName );

            foreach ( var scheduler in otherSchedulers )
            {
                var currentlyExecutingJobs = scheduler.GetCurrentlyExecutingJobs();
                if ( currentlyExecutingJobs.Where( j => j.JobDetail.Description == context.JobDetail.Description &&
                    j.JobDetail.ConcurrentExectionDisallowed ).Any() )
                {
                    System.Diagnostics.Debug.WriteLine( RockDateTime.Now.ToString() + $" VETOED! Scheduler '{scheduler.SchedulerName}' is already executing job Id '{context.JobDetail.Description}' (key: {context.JobDetail.Key})" );

                    Logger.LogDebug( $"Job ID: {{jobId}}, Job Key: {{jobKey}}, Job trigger was vetoed because scheduler '{scheduler.SchedulerName}' is already executing job.", jobId, context.JobDetail?.Key );

                    return true;
                }
            }

            Logger.LogDebug( "Job ID: {jobId}, Job Key: {jobKey}, Job trigger was not vetoed.", jobId, context.JobDetail?.Key );

            return false;
        }

        /// <summary>
        /// Called by the <see cref="IScheduler"/> when a <see cref="ITrigger"/> has fired, it's associated <see cref="IJobDetail"/>
        /// has been executed, and it's Triggered(<see cref="ICalendar"/>) method has been called.
        /// </summary>
        /// <param name="trigger">The trigger.</param>
        /// <param name="context">The context.</param>
        /// <param name="triggerInstructionCode">The trigger instruction code.</param>
        public void TriggerComplete( ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode )
        {
            // Do nothing but log a message.
            Logger.LogDebug( "Job ID: {jobId}, Job Key: {jobKey}, Job trigger completed.", context.JobDetail?.Description.AsIntegerOrNull(), context.JobDetail?.Key );
        }

        /// <summary>
        /// Called by the <see cref="IScheduler" /> when a <see cref="ITrigger" /> has fired, and it's
        /// associated <see cref="IJobDetail" /> is about to be executed.
        /// </summary>
        /// <param name="trigger">The trigger.</param>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        public void TriggerFired( ITrigger trigger, IJobExecutionContext context )
        {
            // Do nothing but log a message.
            Logger.LogDebug( "Job ID: {jobId}, Job Key: {jobKey}, Job trigger fired.", context.JobDetail?.Description.AsIntegerOrNull(), context.JobDetail?.Key );
        }

        /// <summary>
        /// Called by the <see cref="IScheduler" /> when a <see cref="ITrigger" />
        /// has misfired.
        /// </summary>
        /// <param name="trigger">The trigger.</param>
        /// <returns>Task.</returns>
        public void TriggerMisfired( ITrigger trigger )
        {
            // Do nothing but log a message.
            Logger.LogDebug( "Job Key: {jobKey}, Job trigger misfired.", trigger.Key );
            return;
        }

        /// <summary>
        /// Called by the <see cref="T:Quartz.IScheduler" /> when a <see cref="T:Quartz.ITrigger" />
        /// has fired, it's associated <see cref="T:Quartz.IJobDetail" />
        /// has been executed, and it's <see cref="M:Quartz.Spi.IOperableTrigger.Triggered(Quartz.ICalendar)" /> method has been
        /// called.
        /// </summary>
        /// <param name="trigger">The <see cref="T:Quartz.ITrigger" /> that was fired.</param>
        /// <param name="context">The <see cref="T:Quartz.IJobExecutionContext" /> that was passed to the
        /// <see cref="T:Quartz.IJob" />'s<see cref="M:Quartz.IJob.Execute(Quartz.IJobExecutionContext)" /> method.</param>
        /// <param name="triggerInstructionCode">The result of the call on the <see cref="T:Quartz.ITrigger" />'s<see cref="M:Quartz.Spi.IOperableTrigger.Triggered(Quartz.ICalendar)" />  method.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>Task.</returns>
        public Task TriggerComplete( ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode, CancellationToken cancellationToken = default )
        {
            Logger.LogDebug( "Job ID: {jobId}, Job Key: {jobKey}, Job trigger completed.", context.JobDetail?.Description.AsIntegerOrNull(), context.JobDetail?.Key );
            return Task.CompletedTask;
        }
    }
}
