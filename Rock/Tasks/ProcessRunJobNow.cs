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
using Rock.Bus;
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
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( message.JobId );

                if ( job == null )
                {
                    return;
                }

                jobService.RunNow( job );
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