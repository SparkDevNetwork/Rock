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
        [RockObsolete( "15.0" )]
        [Obsolete]
        public static void UpdateLastStatusMessage( this Quartz.IJobExecutionContext context, string message )
        {
            // save the message to this.Result so that RockJobListener will set the save the same message when the Job completes
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
        [RockObsolete( "15.0" )]
        [Obsolete]
        public static int GetJobId( this Quartz.IJobExecutionContext context )
        {
            return context.JobDetail.Description.AsInteger();
        }

        /// <summary>
        /// Gets the job identifier from quartz.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>System.Int32.</returns>
        internal static int GetJobIdFromQuartz( this Quartz.IJobExecutionContext context )
        {
            return context.JobDetail.Description.AsInteger();
        }

        /// <summary>
        /// Gets the Job of the Rock Job associated with the IJobExecutionContext.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>ServiceJob.</returns>
        [RockObsolete( "15.0" )]
        [Obsolete]
        public static ServiceJob GetJob( this Quartz.IJobExecutionContext context, RockContext rockContext )
        {
            var jobId = context.GetJobId();
            return new ServiceJobService( rockContext ).Get( jobId );
        }

        /// <summary>
        /// Loads from job attribute values.
        /// </summary>
        /// <param name="jobDataMap">The job data map.</param>
        /// <param name="job">The job.</param>
        [RockObsolete( "15.0" )]
        [Obsolete]
        internal static void LoadFromJobAttributeValues( this Quartz.JobDataMap jobDataMap, ServiceJob job )
        {
            if ( job.Attributes == null )
            {
                job.LoadAttributes();
            }

            jobDataMap.Clear();

            foreach ( var attrib in job.AttributeValues )
            {
                jobDataMap.Add( attrib.Key, attrib.Value.Value );
            }
        }

        #endregion

    }
}

