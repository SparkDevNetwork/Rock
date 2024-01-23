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
            var warnings = new List<string>();
            List<PersistedDataset> datasetsToBeUpdated;
            int updatedDatasetCount = 0;

            using ( var rockContext = new RockContext() )
            {
                var currentDateTime = RockDateTime.Now;

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
                    this.UpdateLastStatusMessage( FormatStatusMessage( "Updating", name, "success" ) );
                    try
                    {
                        persistedDataset.UpdateResultData();
                        this.UpdateLastStatusMessage( FormatStatusMessage( "Updating", name, "success" ) );
                        updatedDatasetCount++;
                    }
                    catch ( Exception ex )
                    {
                        var warningMessage = $"Ran the job with Warnings: {name} was run but could not update due to the following error: {ex.Message}";
                        warnings.Add( warningMessage );
                        ExceptionLogService.LogException( ex, null );
                        this.UpdateLastStatusMessage( FormatStatusMessage( "Warning", name, "warning" ) );
                    }
                    finally
                    {
                        rockContext.SaveChanges();
                    }
                }
            }

            var resultMessage = new StringBuilder();
            resultMessage.AppendLine( $"<i class='fa fa-circle text-success'></i> Updated {updatedDatasetCount} dataset{( updatedDatasetCount == 1 ? "" : "s" )}" );

            // If there are warnings, concatenate them into the final result.
            if ( warnings.Any() )
            {
                resultMessage.AppendLine(string.Join( "<br>", warnings ));
            }
            this.Result = resultMessage.ToString();
        }

        private string FormatStatusMessage( string action, string datasetName, string statusType )
        {
            string iconClass = statusType == "success" ? "fa-circle text-success" :
                               statusType == "warning" ? "fa-circle text-warning" : "";
            return $"<i class='fa {iconClass}'></i> {action}: {datasetName}";
        }
    }
}
