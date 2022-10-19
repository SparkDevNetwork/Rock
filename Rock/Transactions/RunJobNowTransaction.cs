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
using System.Collections.Generic;

using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <inheritdoc cref="Rock.Tasks.ProcessRunJobNow"/>
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
        public RunJobNowTransaction( int jobId )
        {
            JobId = jobId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunJobNowTransaction"/> class.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="jobDataMapDictionary">Data for the job.</param>
        [System.Obsolete( "Use the RunJobNowTransaction( int jobId ) constructor instead. The jobDataMapDictionary parameter isn't needed since the Job already knows all that." )]
        [RockObsolete( "1.13" )]
        public RunJobNowTransaction( int jobId, Dictionary<string, string> jobDataMapDictionary ) : this( jobId )
        {
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
                    jobService.RunNow( job );
                }
            }
        }
    }
}