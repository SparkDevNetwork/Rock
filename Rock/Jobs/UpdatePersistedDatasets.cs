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
using System.Data.Entity;
using System.Linq;
using System.Text;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// This job will update the persisted data in any Persisted Datasets that need to be refreshed.
    /// </summary>
    [DisplayName( "Update Persisted Datasets" )]
    [Description( "This job will update the persisted data in any Persisted Datasets that need to be refreshed." )]

    public class UpdatePersistedDatasets : RockJob
    {

        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public UpdatePersistedDatasets()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            StringBuilder results = new StringBuilder();
            int updatedDatasetCount = 0;
            var errors = new List<string>();
            List<Exception> exceptions = new List<Exception>();
            List<PersistedDataset> datasetsToBeUpdated;
            int totalDatasetCount = 0;

            using ( var rockContext = new RockContext() )
            {
                var currentDateTime = RockDateTime.Now;

                // Count of all persisted datasets in the system
                totalDatasetCount = new PersistedDatasetService( rockContext ).Queryable().Count();

                // Fetch datasets that are active, not expired, and include their associated schedules
                var persistedDatasetQuery = new PersistedDatasetService( rockContext )
                    .Queryable( "PersistedSchedule" )
                    .Where( a => a.IsActive &&
                          ( a.ExpireDateTime == null || a.ExpireDateTime > currentDateTime ) &&
                          ( !a.RefreshIntervalMinutes.HasValue || a.LastRefreshDateTime == null || DbFunctions.AddMinutes( a.LastRefreshDateTime.Value, a.RefreshIntervalMinutes.Value ) < currentDateTime ) );

                datasetsToBeUpdated = persistedDatasetQuery.ToList()
                    .Where( dataset =>
                    {
                        // Apply schedule-based logic only if the dataset is associated with a schedule
                        if ( dataset.PersistedScheduleId.HasValue )
                        {
                            var schedule = new ScheduleService( rockContext ).Get( dataset.PersistedScheduleId.Value );
                            var beginDateTime = dataset.LastRefreshDateTime ?? schedule.GetFirstStartDateTime();
                            if ( !beginDateTime.HasValue )
                            {
                                return false;
                            }

                            var nextStartDateTimes = schedule.GetScheduledStartTimes( beginDateTime.Value, currentDateTime );
                            return nextStartDateTimes.Any() && nextStartDateTimes.First() <= currentDateTime;
                        }

                        // If not associated with a schedule, it's already included in the initial query
                        return true;
                    } )
                    .ToList();

                foreach ( var persistedDataset in datasetsToBeUpdated )
                {
                    var name = persistedDataset.Name;
                    try
                    {
                        this.UpdateLastStatusMessage( $"Updating {name}" );
                        persistedDataset.UpdateResultData();
                        rockContext.SaveChanges();
                        updatedDatasetCount++;
                    }
                    catch ( Exception ex )
                    {
                        var errorMessage = $"An error occurred while trying to update persisted dataset '{name}' so it was skipped. Error: {ex.Message}";
                        errors.Add( errorMessage );
                        exceptions.Add( new Exception( errorMessage, ex ) );
                        ExceptionLogService.LogException( ex, null );
                    }
                }
            }

            // Format the result message
            FormatResultMessage( results, updatedDatasetCount, datasetsToBeUpdated.Count, totalDatasetCount, errors );

            if ( exceptions.Any() )
            {
                throw new RockJobWarningException( "UpdatePersistedDatasets completed with warnings", new AggregateException( exceptions ) );
            }
        }

        private void FormatResultMessage( StringBuilder results, int updatedCount, int eligibleToUpdateCount, int totalCount, List<string> errors )
        {
            results.AppendLine( $"<i class='fa fa-circle text-success'></i> Updated {updatedCount} {"persisted dataset".PluralizeIf( updatedCount != 1 )}" );
            int notUpdatedCount = eligibleToUpdateCount - updatedCount;
            if ( notUpdatedCount > 0 )
            {
                results.AppendLine( $"<i class='fa fa-circle text-warning'></i> Skipped {notUpdatedCount} {"up-to-date/inactive dataset".PluralizeIf( notUpdatedCount != 1 )}" );
            }

            results.AppendLine( $"<i class='fa fa-circle text-info'></i> Total datasets in system: {totalCount}" );

            foreach ( var error in errors )
            {
                results.AppendLine( $"<i class='fa fa-circle text-danger'></i> {error}" );
            }

            results.ToString();
        }
    }
}
