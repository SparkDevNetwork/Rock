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
using System.Diagnostics;
using System.Linq;
using System.Text;

using Rock.Attribute;
using Rock.Data;
using Rock.Logging;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to makes sure that persisted data views are updated based on their schedule interval.
    /// </summary>
    [DisplayName( "Update Persisted DataViews" )]
    [Description( "Job to makes sure that persisted data views are updated based on their schedule interval." )]

    #region Job Attributes

    [IntegerField(
        "SQL Command Timeout",
        Key = AttributeKey.SqlCommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. Leave blank to use the default for this job (300 seconds). ",
        IsRequired = false,
        DefaultIntegerValue = 5 * 60,
        Category = "General",
        Order = 1 )]

    #endregion

    public class UpdatePersistedDataviews : RockJob
    {
        #region Keys

        /// <summary>
        /// Keys to use for job Attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string SqlCommandTimeout = "SqlCommandTimeout";
        }

        #endregion

        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public UpdatePersistedDataviews()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            int sqlCommandTimeout = GetAttributeValue( AttributeKey.SqlCommandTimeout ).AsIntegerOrNull() ?? 300;
            JobSummary jobSummary = new JobSummary();

            int updatedDataViewCount = 0;
            var errors = new List<string>();

            using ( var rockContextList = new RockContext() )
            {
                var currentDateTime = RockDateTime.Now;
                var dataViewService = new DataViewService( rockContextList );

                // Get a list of all the data views with schedule interval minutes that need to be refreshed.
                var expiredPersistedDataViewIds = dataViewService.Queryable()
                    .Where( a => a.PersistedScheduleIntervalMinutes.HasValue )
                        .Where( a =>
                            ( a.PersistedLastRefreshDateTime == null )
                            || ( System.Data.Entity.SqlServer.SqlFunctions.DateAdd( "mi", a.PersistedScheduleIntervalMinutes.Value, a.PersistedLastRefreshDateTime.Value ) < currentDateTime ) )
                        .Select( a => a.Id )
                        .ToList();

                /*
                 * Get the data views with persisted schedules that might need to be refreshed (we'll refine this list next).
                 * Go ahead and materialize this list so we can perform some in-memory date/time comparisons.
                 */
                var persistedScheduleDataViews = dataViewService.Queryable()
                    .Include( a => a.PersistedSchedule )
                    .Where( a => a.PersistedScheduleId.HasValue
                        && (
                            a.PersistedLastRefreshDateTime == null
                            || a.PersistedLastRefreshDateTime.Value < currentDateTime
                    ) )
                    .ToList();

                // Perform a more thorough check of data views with persisted schedules to rule out those that don't need refreshed.
                foreach ( var dataView in persistedScheduleDataViews )
                {
                    var beginDateTime = dataView.PersistedLastRefreshDateTime ?? dataView.PersistedSchedule.GetFirstStartDateTime();
                    if ( !beginDateTime.HasValue )
                    {
                        // We don't know when this schedule was supposed to start, so ignore it (should never happen).
                        continue;
                    }

                    var nextStartDateTimes = dataView.PersistedSchedule.GetScheduledStartTimes( beginDateTime.Value, currentDateTime );
                    if ( !nextStartDateTimes.Any() )
                    {
                        // This Data View does not need to be refreshed.
                        continue;
                    }

                    if ( nextStartDateTimes.First() <= currentDateTime )
                    {
                        expiredPersistedDataViewIds.Add( dataView.Id );
                    }
                }

                foreach ( var dataViewId in expiredPersistedDataViewIds )
                {
                    using ( var persistContext = new RockContext() )
                    {
                        var startDateTime = RockDateTime.Now;
                        var stopwatch = Stopwatch.StartNew();
                        var errorOccurred = false;

                        var dataView = new DataViewService( persistContext ).Get( dataViewId );
                        var name = dataView.Name;
                        try
                        {
                            this.UpdateLastStatusMessage( $"Updating '{dataView.Name}'" );
                            dataView.PersistResult( sqlCommandTimeout );

                            persistContext.SaveChanges();

                            updatedDataViewCount++;

                            stopwatch.Stop();
                        }
                        catch ( Exception ex )
                        {
                            stopwatch.Stop();
                            errorOccurred = true;

                            // Capture and log the exception to the Rock Log because we're not going to mark this job as failed
                            // unless all the data views fail.
                            errors.Add( $"Data View: '{name}'" );
                            RockLogger.Log.Warning( RockLogDomains.Jobs, ex, $"{this.ServiceJobName}: An error occurred while trying to update persisted data view '{name}' so it was skipped." );
                            continue;
                        }
                        finally
                        {
                            Log(
                                RockLogLevel.Info,
                                "Data View ID: {dataViewId}, Data View Name: {dataViewName}, Error Occurred: {errorOccurred}",
                                startDateTime,
                                stopwatch.ElapsedMilliseconds,
                                dataViewId,
                                name,
                                errorOccurred );
                        }
                    }
                }
            }

            // Format the result message
            jobSummary.Successes.Add( $"Updated {updatedDataViewCount} data {"view".PluralizeIf( updatedDataViewCount != 1 )}" );
            this.Result = jobSummary.ToString();

            if ( errors.Any() )
            {
                var errorsSummary = new List<string>();
                errorsSummary.Add( $"Skipped {errors.Count} data {"view".PluralizeIf( errors.Count != 1 )} due to encountered errors:" );
                errorsSummary.AddRange( errors.Take( 10 ).ToList() );
                errorsSummary.Add( "" );
                errorsSummary.Add( "Enable 'Warning' logging level for 'Jobs' domain in Rock Logs and re-run this job to get a full list of issues." );

                // If all the data views had issues, add to the errors list of the job summary, otherwise add to the warnings.
                if ( updatedDataViewCount == 0 )
                {
                    jobSummary.Errors.AddRange( errorsSummary );
                }
                else
                {
                    jobSummary.Warnings.AddRange( errorsSummary );
                }

                this.Result = jobSummary.ToString();

                // If there were no successful data view updates we will throw a standard exception.
                // Otherwise, if there were some successes and some skipped data views,
                // we will throw a Rock Job warning exception to list the data views that gave warnings.
                if ( updatedDataViewCount == 0 )
                {
                    throw new Exception( "'Update Persisted Data Views' job could not complete.", new Exception( jobSummary.ToString() ) );
                }
                else
                {
                    throw new RockJobWarningException( "'Update Persisted Data Views' job completed with warnings.", new Exception( jobSummary.ToString() ) );
                }
            }
        }
    }
}