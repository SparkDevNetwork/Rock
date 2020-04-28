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
using System.Linq;
using System.Text;

using Quartz;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// This job will update the persisted data in any Persisted Datasets that need to be refreshed.
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisplayName( "Update Persisted Datasets" )]
    [Description( "This job will update the persisted data in any Persisted Datasets that need to be refreshed." )]

    [DisallowConcurrentExecution]
    public class UpdatePersistedDatasets : IJob
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

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            StringBuilder results = new StringBuilder();
            int updatedDatasetCount = 0;
            int updatedDatasetTotalCount;
            var errors = new List<string>();
            List<Exception> exceptions = new List<Exception>();

            using ( var rockContext = new RockContext() )
            {
                var currentDateTime = RockDateTime.Now;

                var persistedDatasetQuery = new PersistedDatasetService( rockContext ).Queryable();
                updatedDatasetTotalCount = persistedDatasetQuery.Count();

                // exclude datasets that are no longer active
                persistedDatasetQuery = persistedDatasetQuery.Where( a => a.IsActive && ( a.ExpireDateTime == null || a.ExpireDateTime > currentDateTime ) );

                // exclude datasets that are already up-to-date based on the Refresh Interval and LastRefreshTime
                persistedDatasetQuery = persistedDatasetQuery
                    .Where( a =>
                        a.LastRefreshDateTime == null
                        || ( System.Data.Entity.SqlServer.SqlFunctions.DateAdd( "mi", a.RefreshIntervalMinutes.Value, a.LastRefreshDateTime.Value ) < currentDateTime ) );

                var expiredPersistedDatasetsList = persistedDatasetQuery.ToList();
                foreach ( var persistedDataset in expiredPersistedDatasetsList )
                {
                    var name = persistedDataset.Name;
                    try
                    {
                        context.UpdateLastStatusMessage( $"Updating {persistedDataset.Name}" );
                        persistedDataset.UpdateResultData();
                        rockContext.SaveChanges();
                        updatedDatasetCount++;
                    }
                    catch ( Exception ex )
                    {
                        // Capture and log the exception because we're not going to fail this job
                        // unless all the data views fail.
                        var errorMessage = $"An error occurred while trying to update persisted dataset '{name}' so it was skipped. Error: {ex.Message}";
                        errors.Add( errorMessage );
                        var ex2 = new Exception( errorMessage, ex );
                        exceptions.Add( ex2 );
                        ExceptionLogService.LogException( ex2, null );
                        continue;
                    }
                }
            }

            int notUpdatedCount = updatedDatasetTotalCount - updatedDatasetCount;

            // Format the result message
            results.AppendLine( $"Updated {updatedDatasetCount} {"persisted dataset".PluralizeIf( updatedDatasetCount != 1 )}." );
            if (notUpdatedCount > 0)
            {
                results.AppendLine( $"Skipped {notUpdatedCount} {"persisted dataset".PluralizeIf( updatedDatasetCount != 1 )} that are already up-to-date or inactive." );
            }
            context.Result = results.ToString();

            if ( errors.Any() )
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.Append( "Errors: " );
                errors.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                string errorMessage = sb.ToString();
                context.Result += errorMessage;
                // We're not going to throw an aggregate exception unless there were no successes.
                // Otherwise the status message does not show any of the success messages in
                // the last status message.
                if ( updatedDatasetCount == 0 )
                {
                    throw new AggregateException( exceptions.ToArray() );
                }
            }
        }
    }
}
