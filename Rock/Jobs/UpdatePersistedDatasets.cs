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

using Rock.Constants;
using Rock.Data;
using Rock.Logging;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// This job will update the persisted data in any Persisted Datasets that need to be refreshed.
    /// </summary>
    [DisplayName( "Update Persisted Datasets" )]
    [Description( "This job will update the persisted data in any Persisted Datasets that need to be refreshed." )]

    [RockLoggingCategory]
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
            var warnings = new List<string>();
            int updatedDatasetCount = 0;

            var currentDateTime = RockDateTime.Now;

            /*
                 7/23/2025 - NA

                 Avoid loading all PersistedDatasets into memory, as some organizations store ResultData
                 exceeding 100 MB. Instead, we first query only for datasets that are active, not expired,
                 and have either a refresh interval or a schedule.  Full dataset objects are loaded later
                 only if they require an update.

                 Reason: Prevent excessive memory usage by deferring loading of large dataset objects.
            */
            var persistedDatasetQuery = new PersistedDatasetService( new RockContext() )
                .Queryable()
                .AsNoTracking()
                .Where(
                    a => a.IsActive &&
                    ( a.ExpireDateTime == null || a.ExpireDateTime > currentDateTime ) &&
                    (
                        // Either the refresh interval is valid and elapsed
                        ( a.RefreshIntervalMinutes.HasValue &&
                            ( a.LastRefreshDateTime == null ||
                            DbFunctions.AddMinutes( a.LastRefreshDateTime.Value, a.RefreshIntervalMinutes.Value ) < currentDateTime ) )
                        ||
                        // Or it has a schedule
                        a.PersistedScheduleId != null
                    )
                )
                .Select( a => new
                {
                    a.Name, // useful when debugging
                    a.Id,
                    a.LastRefreshDateTime,
                    a.RefreshIntervalMinutes,
                    a.PersistedScheduleId,
                } )
                .ToList();

            // Now, we can check the schedule (for those that are Schedule based)...
            var datasetsToBeUpdated = persistedDatasetQuery
                .Where( dataset =>
                {
                    // Apply schedule-based logic only if the dataset is associated with a schedule
                    if ( dataset.PersistedScheduleId.HasValue )
                    {
                        var schedule = new ScheduleService( new RockContext() ).Get( dataset.PersistedScheduleId.Value );
                        var beginDateTime = dataset.LastRefreshDateTime ?? schedule.GetFirstStartDateTime();
                        if ( !beginDateTime.HasValue )
                        {
                            return false;
                        }

                        var nextStartDateTimes = schedule.GetScheduledStartTimes( beginDateTime.Value, currentDateTime );
                        return nextStartDateTimes.Any() && nextStartDateTimes.First() <= currentDateTime;
                    }

                    // If not associated with a schedule, it must have a refresh interval that has elapsed
                    return true;
                } )
                .ToList();

            foreach ( var untrackedPersistedDataset in datasetsToBeUpdated )
            {
                using ( var rockContext = new RockContext() )
                {
                    // Get the full persisted dataset object to update
                    var persistedDatasetService = new PersistedDatasetService( rockContext );
                    var persistedDatasetToUpdate = persistedDatasetService.Get( untrackedPersistedDataset.Id );

                    // Would only happen if it was JUST deleted in another thread.
                    if ( persistedDatasetToUpdate == null )
                    {
                        continue;
                    }

                    var name = persistedDatasetToUpdate.Name;
                    this.UpdateLastStatusMessage( FormatStatusMessage( "Updating...", name, "info" ) );
                    try
                    {
                        var updateResult = persistedDatasetToUpdate.UpdateResultData();
                        if ( updateResult.IsSuccess )
                        {
                            this.UpdateLastStatusMessage( FormatStatusMessage( "Update", name, "success" ) );
                            updatedDatasetCount++;
                            rockContext.SaveChanges();
                        }
                        else
                        {
                            warnings.Add( $"{name} ({persistedDatasetToUpdate.Id})" );
                            this.UpdateLastStatusMessage( FormatStatusMessage( "Warning", name, "warning" ) );
                        }
                    }
                    catch ( Exception ex )
                    {
                        var warningMessage = $"Ran the job with Warnings: {name} was run but could not update due to the following error: {ex.Message}";
                        warnings.Add( warningMessage );
                        ExceptionLogService.LogException( ex, null );
                        this.UpdateLastStatusMessage( FormatStatusMessage( "Warning", name, "warning" ) );
                    }
                }
            }

            var resultMessage = new StringBuilder();
            resultMessage.AppendLine( $"<i class='ti ti-circle text-success'></i> {updatedDatasetCount} Updated" );

            // If there are warnings, concatenate them into the final result.
            if ( warnings.Any() )
            {
                resultMessage.AppendLine( $"<i class='ti ti-circle text-warning'></i> {warnings.Count} Warning{( warnings.Count == 1 ? "" : "s" )}. Run manually or see exception log for details." );
                resultMessage.AppendLine(string.Join( "<br>", warnings ));
            }
            this.Result = resultMessage.ToString();
        }

        private string FormatStatusMessage( string action, string datasetName, string statusType )
        {
            return $"<i class='ti ti-circle text-{statusType}'></i> {action}: {datasetName}";
        }
    }
}
