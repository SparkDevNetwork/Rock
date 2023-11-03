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
using System.Linq;
using Rock.Attribute;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.ServiceJobHistory"/> entity objects.
    /// </summary>
    public partial class ServiceJobHistoryService
    {
        /// <summary>
        /// Returns a queryable collection of all <see cref="Rock.Model.ServiceJobHistory">jobs history</see>
        /// </summary>
        /// <param name="serviceJobId">The service job identifier.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="stopDateTime">The stop date time.</param>
        /// <returns>A queryable collection of all <see cref="Rock.Model.ServiceJobHistory"/>jobs history</returns>
        public IQueryable<ServiceJobHistory> GetServiceJobHistory( int? serviceJobId, DateTime? startDateTime = null, DateTime? stopDateTime = null )
        {
            var ServiceJobHistoryQuery = this.AsNoFilter();

            if ( serviceJobId.HasValue )
            {
                ServiceJobHistoryQuery = ServiceJobHistoryQuery.Where( a => a.ServiceJobId == serviceJobId );
            }

            if ( startDateTime.HasValue )
            {
                ServiceJobHistoryQuery = ServiceJobHistoryQuery.Where( a => a.StartDateTime >= startDateTime.Value );
            }

            if ( stopDateTime.HasValue )
            {
                ServiceJobHistoryQuery = ServiceJobHistoryQuery.Where( a => a.StopDateTime < stopDateTime.Value );
            }

            return ServiceJobHistoryQuery.OrderBy( a => a.ServiceJobId ).ThenByDescending( a => a.StartDateTime );
        }

        /// <summary>
        /// Deletes job history items more than maximum.
        /// </summary>
        public void DeleteMoreThanMax()
        {
            ServiceJobService serviceJobService = new ServiceJobService( (RockContext)this.Context );
            var serviceJobIds = serviceJobService.AsNoFilter().Select( sj => sj.Id ).ToArray();
            foreach ( var serviceJobId in serviceJobIds)
            {
                DeleteMoreThanMax( serviceJobId );
            }
        }

        /// <summary>
        /// Deletes job history items more than maximum.
        /// </summary>
        /// <param name="serviceJobId">The service job identifier.</param>
        public void DeleteMoreThanMax( int serviceJobId )
        {
            var rockContext = this.Context as RockContext;
            var historyCount = new ServiceJobService( rockContext ).GetSelect( serviceJobId, s => s.HistoryCount );
            historyCount = historyCount <= 0 ? historyCount = 500 : historyCount;

            ServiceJobHistoryService serviceJobHistoryService = new ServiceJobHistoryService( rockContext );
            var serviceJobHistoryToDeleteQuery = serviceJobHistoryService.Queryable().Where( a => a.ServiceJobId == serviceJobId ).OrderByDescending( a => a.StartDateTime ).Skip( historyCount );
            rockContext.BulkDelete( serviceJobHistoryToDeleteQuery );
        }

        /// <summary>
        /// Adds a job history record for a job that has just started (does not save the context).
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        /// <param name="job">The job.</param>
        /// <param name="startDateTime">The time the job started (defaults to RockDateTime.Now).</param>
        /// <returns>The created job history.</returns>
        [RockInternal( "1.16" )]
        internal ServiceJobHistory AddStartedServiceJobHistory( ServiceJob job, DateTime startDateTime )
        {
            // The history record is being written before job execution (with a null stop date) so it is visible sooner.
            // When the job completes, the history record will be updated.
            var jobHistory = new ServiceJobHistory()
            {
                ServiceJobId = job.Id,
                StartDateTime = startDateTime,
                StopDateTime = null,
                Status = job.LastStatus,
                StatusMessage = job.LastStatusMessage,
                ServiceWorker = Environment.MachineName.ToLower()
            };

            this.Add( jobHistory );

            return jobHistory;
        }

        /// <summary>
        /// Adds a job history record for a job that has just completed (does not save the context).
        /// <para>This should only be called after the job's last run details are updated.</para>
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        /// <param name="job">The job.</param>
        [RockInternal( "1.16" )]
        internal void AddCompletedServiceJobHistory( ServiceJob job )
        {
            // If there is an incomplete job history record for this job's last run then update it; otherwise, create a new job history record.
            var jobHistory = GetIncompleteServiceJobHistoryForLastRun( job );

            if ( jobHistory == null )
            {
                // This is a new job history record so calculate the job's start date time given its last run date time and run duration.
                jobHistory = new ServiceJobHistory()
                {
                    ServiceJobId = job.Id,
                    // We're rewriting the ServiceJobHistory's start date so that its start and end dates match the ServiceJob's last run duration and last run date.
                    StartDateTime = GetStartedDateTimeForLastRun( job ),
                };

                this.Add( jobHistory );
            }

            jobHistory.StopDateTime = job.LastRunDateTime;
            jobHistory.Status = job.LastStatus;
            jobHistory.StatusMessage = job.LastStatusMessage;
            jobHistory.ServiceWorker = Environment.MachineName.ToLower();
        }

        /// <summary>
        /// Adds a job history record for a job that has just stopped due to an error (does not save the context).
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        /// <param name="job">The job.</param>
        [RockInternal( "1.16" )]
        internal void AddErrorServiceJobHistory( ServiceJob job )
        {
            var now = RockDateTime.Now;

            // If there is an incomplete job history record for this job's last run then update it; otherwise, create a new job history record.
            var jobHistory = GetIncompleteServiceJobHistoryForLastRun( job );

            if ( jobHistory == null )
            {
                // This is a new job history record so set the start date time to now as the error just occurred.
                jobHistory = new ServiceJobHistory()
                {
                    ServiceJobId = job.Id,
                    StartDateTime = now
                };

                this.Add( jobHistory );
            }

            // Set the stop date time to now since the job just stopped due to an error .
            jobHistory.StopDateTime = now;

            // Save the last job status and message in history.
            jobHistory.Status = job.LastStatus;
            jobHistory.StatusMessage = job.LastStatusMessage;
        }

        /// <summary>
        /// Gets the started date time of a job's last run.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        /// <param name="job">The job.</param>
        /// <returns>The started date time of a job's last run.</returns>
        private DateTime? GetStartedDateTimeForLastRun( ServiceJob job )
        {
            return job.LastRunDateTime?.AddSeconds( -( job.LastRunDurationSeconds ?? 0 ) );
        }

        /// <summary>
        /// Gets the incomplete service job history for a job's last run.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <returns>The incomplete service job history for a job's last run.</returns>
        private ServiceJobHistory GetIncompleteServiceJobHistoryForLastRun( ServiceJob job )
        {
            // Since the times for ServiceJob, ServiceJobHistory, and the internal Quartz execution are distinct,
            // we will try our best to find the existing ServiceJobHistory using facts we know about the timing.
            // Fact #1: ServiceJobHistory.ServiceJobId == ServiceJob.Id 
            // Fact #2: ServiceJobHistory.StopDateTime == null (not completed yet)
            // Fact #3: ServiceJobHistory.StartDateTime < ServiceJob.LastRunDateTime (if ServiceJob.LastRunDurationSeconds > 0)
            // Fact #4: ServiceJobHistory.StartDateTime <= ServiceJob.LastRunDateTime (if ServiceJob.LastRunDurationSeconds == 0) 
            var startedDateTimeForLastRun = GetStartedDateTimeForLastRun( job );

            // Check if there is an incomplete job history record for this job.
            var jobHistoryQuery = this.AsNoFilter()
                .Where( h => h.ServiceJobId == job.Id )
                .Where( h => !h.StopDateTime.HasValue )
                .Where( h => h.StartDateTime.HasValue );

            if ( job.LastRunDurationSeconds.HasValue && job.LastRunDurationSeconds > 0 )
            {
                jobHistoryQuery = jobHistoryQuery.Where( h => h.StartDateTime < job.LastRunDateTime );
            }
            else
            {
                jobHistoryQuery = jobHistoryQuery.Where( h => h.StartDateTime <= job.LastRunDateTime );
            }

            // Get the most recent job history record that matches the query.
            return jobHistoryQuery
                .OrderByDescending( h => h.StartDateTime.Value )
                .FirstOrDefault();
        }
    }
}
