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
            List<PersistedDataset> datasetsToBeUpdated;

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
                    var updateResult = persistedDataset.UpdateResultData();

                    if ( updateResult.IsSuccess )
                    {
                        var successMessage = $"<i class='fa fa-circle text-success'></i> Success: {name} was run successfully";
                        results.AppendLine( successMessage );
                    }
                    else
                    {
                        var warningMessage = $"<i class='fa fa-circle text-warning'></i> Warning: {name} was run but encountered issues: {updateResult.WarningMessage}";
                        results.AppendLine( warningMessage );
                    }
                }

                if ( results.Length > 0 )
                {
                    this.Result = results.ToString();
                }
                rockContext.SaveChanges();
            }
        }
    }
}
