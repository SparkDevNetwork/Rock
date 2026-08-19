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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Quartz;

using Rock.Bus.Locking;
using Rock.Configuration;
using Rock.Logging;

namespace Rock.Jobs
{
    /// <summary>
    /// Implementation of <see cref="ITriggerListener"/> that gates every Rock
    /// job fire behind a distributed lock. Exactly one node (or one scheduler
    /// on one node) wins the lock and runs the job; every other fire vetos
    /// silently. This is what prevents duplicate execution during IIS app
    /// pool overlaps, across a Rock farm, and between the main scheduler and
    /// a Run Now-created scheduler on the same node. Every Rock job inherits
    /// <c>[DisallowConcurrentExecution]</c> from <see cref="RockJob"/>, so
    /// Quartz already prevents same-scheduler double-fires; this listener
    /// covers the cases Quartz cannot. See
    /// <c>specs/completed/core/*distributed-locking*</c> for the design.
    /// </summary>
    [RockLoggingCategory]
    public class RockTriggerListener : ITriggerListener
    {
        /// <summary>
        /// The key used to stash the acquired <see cref="ILockHandle"/> on the
        /// <see cref="IJobExecutionContext"/> for the duration of a job's
        /// execution. <see cref="RockJobListener.JobWasExecuted"/> retrieves
        /// and disposes the handle to release the distributed lock.
        /// </summary>
        internal const string DistributedLockHandleKey = "Rock.DistributedLockHandle";
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
            // ServiceJob.Id is stashed on JobDetail.Description at
            // BuildQuartzJob time, so this pulls the Rock domain identity we
            // key the distributed lock on.
            int jobId = context.JobDetail.Description.AsInteger();

            // Every fire races for the same lock keyed by ServiceJob.Id.
            // The legacy "check every local scheduler for a concurrent run"
            // enumeration used to live here but was fully redundant with the
            // distributed lock: every Rock job inherits
            // [DisallowConcurrentExecution], so Quartz already prevents
            // same-scheduler double-fires, and the distributed lock covers
            // the remaining cases (Run Now scheduler vs main scheduler on
            // the same node, and cross-node fires across the farm).
            if ( !TryAcquireDistributedLock( context, jobId, out var lockAcquired ) )
            {
                // Provider threw. Vetoed to avoid uncoordinated execution.
                return true;
            }

            if ( !lockAcquired )
            {
                Logger.LogDebug(
                    "Job ID: {jobId} (App PID: {processId}-{domainId}), Job Key: {jobKey}, Job trigger was vetoed because another node holds the distributed lock.",
                    jobId,
                    Rock.WebFarm.RockWebFarm.ProcessId,
                    AppDomain.CurrentDomain.Id,
                    context.JobDetail?.Key
                );

                return true;
            }

            Logger.LogDebug(
                "Job ID: {jobId} (App PID: {processId}-{domainId}), Job Key: {jobKey}, Job trigger was not vetoed.",
                jobId,
                Rock.WebFarm.RockWebFarm.ProcessId,
                AppDomain.CurrentDomain.Id,
                context.JobDetail?.Key
            );

            return false;
        }

        /// <summary>
        /// Attempts to acquire the distributed lock for
        /// <paramref name="jobId"/>. On success the handle is stashed on
        /// <paramref name="context"/> so
        /// <see cref="RockJobListener.JobWasExecuted"/> can dispose it.
        /// </summary>
        /// <param name="context">The Quartz execution context.</param>
        /// <param name="jobId">The <c>ServiceJob.Id</c> being fired.</param>
        /// <param name="lockAcquired">
        /// Set to <c>true</c> when the lock was obtained and the job may
        /// proceed. Set to <c>false</c> when another node holds the lock or
        /// an infrastructure error occurred (the caller should veto in that
        /// case).
        /// </param>
        /// <returns>
        /// <c>false</c> when the provider threw an unexpected exception (an
        /// argument-validation failure or a programmer error). The caller
        /// MUST veto in this case rather than run the job without
        /// coordination. <c>true</c> in every other case; inspect
        /// <paramref name="lockAcquired"/> to distinguish "acquired" from
        /// "not acquired."
        /// </returns>
        private bool TryAcquireDistributedLock( IJobExecutionContext context, int jobId, out bool lockAcquired )
        {
            lockAcquired = false;

            // GetRequiredService is deliberate: if the provider is not
            // registered, that means Rock's startup ordering is broken (Quartz
            // is running before DI is complete) or an operator has deregistered
            // the primitive. Both are catastrophic developer bugs, not a
            // recoverable condition. The InvalidOperationException from
            // GetRequiredService (or an NRE if RockApp.Current is null)
            // propagates out of VetoJobExecution and wedges the trigger — the
            // loud, correct consequence for a systemic misconfiguration.
            // Silently running jobs without cross-node coordination is worse
            // than not running them at all; the operator needs to see the
            // failure and fix it.
            var lockProvider = RockApp.Current.GetRequiredService<IDistributedLockProvider>();

            ILockHandle handle;

            try
            {
                handle = lockProvider.TryAcquire( typeof( RockTriggerListener ), jobId.ToString(), TimeSpan.Zero );
            }
            catch ( Exception ex )
            {
                // Argument validation failure or provider bug. The primitive
                // has already logged at Error. From the scheduler's
                // perspective the safest response is to veto: running the
                // job without coordination is worse than not running it
                // this cycle (it will fire again on the next scheduled tick).
                Logger.LogError( ex, "Distributed lock provider threw for Job ID {jobId}. Vetoing this fire.", jobId );
                return false;
            }

            if ( !handle.IsAcquired )
            {
                // Contention loss or infrastructure failure. The provider
                // has already logged infrastructure failures at Warning;
                // contention loss is deliberately silent. Dispose the handle
                // (safe on an unacquired one) and let the caller veto.
                handle.Dispose();
                return true;
            }

            // Stash the handle on the context so it survives to the
            // JobWasExecuted callback where it can be released. Using the
            // context is safe because Rock runs Quartz with RAMJobStore, so
            // the context is in-process and never serialized.
            context.Put( DistributedLockHandleKey, handle );
            lockAcquired = true;
            return true;
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
            Logger.LogDebug(
                "Job ID: {jobId} (App PID: {processId}-{domainId}), Job Key: {jobKey}, Job trigger completed.",
                context.JobDetail?.Description.AsIntegerOrNull(),
                Rock.WebFarm.RockWebFarm.ProcessId,
                AppDomain.CurrentDomain.Id,
                context.JobDetail?.Key
            );
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
            Logger.LogDebug(
                "Job ID: {jobId} (App PID: {processId}-{domainId}), Job Key: {jobKey}, Job trigger fired.",
                context.JobDetail?.Description.AsIntegerOrNull(),
                Rock.WebFarm.RockWebFarm.ProcessId,
                AppDomain.CurrentDomain.Id,
                context.JobDetail?.Key
            );
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
            Logger.LogDebug(
                "(App PID: {processId}-{domainId}), Job Key: {jobKey}, Job trigger misfired.",
                Rock.WebFarm.RockWebFarm.ProcessId,
                AppDomain.CurrentDomain.Id,
                trigger.Key
            );
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
            Logger.LogDebug(
                "Job ID: {jobId} (App PID: {processId}-{domainId}), Job Key: {jobKey}, Job trigger completed.",
                context.JobDetail?.Description.AsIntegerOrNull(),
                Rock.WebFarm.RockWebFarm.ProcessId,
                AppDomain.CurrentDomain.Id,
                context.JobDetail?.Key
            );
            return Task.CompletedTask;
        }
    }
}
