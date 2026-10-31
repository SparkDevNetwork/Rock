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
using System;
using System.Threading.Tasks;

using Rock.Bus;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;

namespace Rock.Tasks
{
    /// <summary>
    /// There are 3 ways to run a job, using the following methods.
    /// 1) To queue a <see cref="ServiceJob"/> to run almost immediately using the <see cref="RockMessageBus"/>, use <see cref="ProcessRunJobNow"/>. Note: This could cause jobs to run in parallel.
    /// 2) To queue the job with a lower priority using the Transaction Queue, use <see cref="Rock.Transactions.RunJobNowTransaction" />. This will run queued tasks in order, one at a time.
    /// 3) To run the job immediately (and wait for it to finish) use <see cref="Rock.Model.ServiceJobService.RunNow(ServiceJob)"/>
    /// </summary>
    public sealed class ProcessRunJobNow : BusStartedTask<ProcessRunJobNow.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            /*
                10/30/2026 - MSE

                This consumer is synchronous and RunNow waits for the job to complete, so running the job inline
                here holds a start task queue consumer for the whole job and stalls the other tasks waiting on
                that queue. Offload the work to the thread pool so this consumer returns right away. The offloaded
                work uses its own RockContext and reloads the job because the consumer's context must not be
                shared across threads. It awaits RunNowAsync directly instead of the blocking RunNow wrapper so
                it does not hold an extra thread pool thread for the whole job. All exceptions are caught and
                logged so the task can never fault unobserved.

                Reason: Prevent a long running Run Now job from blocking all other bus started tasks.
            */
            var jobId = message.JobId;

            Task.Run( async () => await RunJobAsync( jobId ) );
        }

        /// <summary>
        /// Loads the job using a new <see cref="RockContext"/> and runs it, completing when the job finishes.
        /// Any exception is logged so it is never left unobserved.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        private static async Task RunJobAsync( int jobId )
        {
            try
            {
                // The context stays alive until the job finishes so lazy loading works for the job entity.
                using ( var rockContext = RockApp.Current.CreateRockContext() )
                {
                    var jobService = new ServiceJobService( rockContext );
                    var job = jobService.Get( jobId );

                    if ( job == null )
                    {
                        return;
                    }

                    await ServiceJobService.RunNowAsync( job );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
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
        }
    }
}