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
using Quartz;

using Rock.Model;

using System.Threading;
using System.Threading.Tasks;

namespace Rock.Jobs
{
    /// <summary>
    /// Special job listener for RunNow jobs. This is used by
    /// <see cref="ServiceJobService.RunNowAsync(ServiceJob)"/> to know when
    /// the job has completed so it can shut down the scheduler.
    /// </summary>
    internal class RunNowRockJobListener : RockJobListener
    {
        /// <summary>
        /// The identifier of the job being run.
        /// </summary>
        private readonly int _jobId;

        /// <summary>
        /// Used to signal the <see cref="JobCompletedTask"/> that the job has
        /// finished executing.
        /// </summary>
        private readonly TaskCompletionSource<object> _taskCompletionSource;

        /// <summary>
        /// Signals that the job has finished executing.
        /// </summary>
        public Task JobCompletedTask => _taskCompletionSource.Task;

        /// <summary>
        /// Creates a new instance of <see cref="RunNowRockJobListener"/>. The
        /// listener will monitor for when a job matching <paramref name="jobId"/>
        /// has finished (or when the start was vetoed).
        /// </summary>
        /// <param name="jobId">The identifier of the job to be run.</param>
        internal RunNowRockJobListener( int jobId )
        {
            _jobId = jobId;

            // This RunContinuationsAsynchronously resolves issues where the
            // task that is awaiting the TCS will be run synchronously. The
            // problem is we might be on the same thread and thus have a deadlock.
            _taskCompletionSource = new TaskCompletionSource<object>( TaskCreationOptions.RunContinuationsAsynchronously );
        }

        /// <inheritdoc/>
#if REVIEW_NET5_0_OR_GREATER
        public override Task JobExecutionVetoed( IJobExecutionContext context, CancellationToken cancellationToken )
#else
        public override void JobExecutionVetoed( IJobExecutionContext context )
#endif
        {
#pragma warning disable CS0612 // Type or member is obsolete
            var jobId = context.GetJobId();
#pragma warning restore CS0612 // Type or member is obsolete

            if ( jobId == _jobId )
            {
                // Run in a task since the awaiter might be on the same thread
                // as us which would cause a deadlock.
                _taskCompletionSource.TrySetResult( null );
            }

#if REVIEW_NET5_0_OR_GREATER
            return base.JobExecutionVetoed( context, cancellationToken );
#else
            base.JobExecutionVetoed( context );
#endif
        }

        /// <inheritdoc/>
#if REVIEW_NET5_0_OR_GREATER
        public override Task JobWasExecuted( IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken )
#else
        public override void JobWasExecuted( IJobExecutionContext context, JobExecutionException jobException )
#endif
        {
#pragma warning disable CS0612 // Type or member is obsolete
            var jobId = context.GetJobId();
#pragma warning restore CS0612 // Type or member is obsolete

            if ( jobId == _jobId )
            {
                // Run in a task since the awaiter might be on the same thread
                // as us which would cause a deadlock.
                _taskCompletionSource.TrySetResult( null );
            }

#if REVIEW_NET5_0_OR_GREATER
            return base.JobWasExecuted( context, jobException, cancellationToken );
#else
            base.JobWasExecuted( context, jobException );
#endif
        }
    }
}
