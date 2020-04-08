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
using Rock.Data;
using Rock.Model;

namespace Rock
{
    /// <summary>
    /// Quartz Extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region IJobExecutionContext extensions

        /// <summary>
        /// Updates the LastStatusMessage of the Rock Job associated with the IJobExecutionContext
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="message">The message.</param>
        public static void UpdateLastStatusMessage( this Quartz.IJobExecutionContext context, string message )
        {
            // save the message to context.Result so that RockJobListener will set the save the same message when the Job completes
            context.Result = message;

            int jobId = context.GetJobId();
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( jobId );
                if ( job != null )
                {
                    job.LastStatusMessage = message;
                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Gets the Job ID of the Rock Job associated with the IJobExecutionContext.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static int GetJobId( this Quartz.IJobExecutionContext context)
        {
            return context.JobDetail.Description.AsInteger();
        }

        #endregion

    }
}

