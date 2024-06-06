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

using Microsoft.Extensions.Logging;

using Rock.Attribute;
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Web.Cache;

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

    [RockLoggingCategory]
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
            int updatedDataViewCount = 0;
            var failedDataViews = new List<string>();
            var exceptions = new List<Exception>();

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
                        // This DataView does not need to be refreshed.
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
                            this.UpdateLastStatusMessage( $"Updating {dataView.Name}" );
                            dataView.PersistResult( sqlCommandTimeout );

                            /*
                                5/11/2023 - KA
                                We are calling the SaveChanges( true ) overload that disables pre/post processing hooks
                                because the PersistedLastRefreshDateTime and PersistedLastRunDurationMilliseconds properties
                                of the DataView is updated above. If we don't disable these hooks, the [ModifiedDateTime]
                                value will also be updated every time a DataView result is persisted, which is not what we want here.
                            */
                            persistContext.SaveChanges( true );

                            // Because we are disabling the pre-post logic (which
                            // includes cache flush), we need to manually flush
                            // the cached item.
                            DataViewCache.FlushItem( dataView.Id );

                            updatedDataViewCount++;

                            stopwatch.Stop();
                        }
                        catch ( Exception ex )
                        {
                            stopwatch.Stop();
                            errorOccurred = true;

                            // Capture and log the exception because we're not going to fail this job
                            // unless all the data views fail.
                            var errorMessage = $"An error occurred while trying to update persisted data view '{name}' so it was skipped. Error: {ex.Message}";
                            failedDataViews.Add( name );
                            var ex2 = new Exception( errorMessage, ex );
                            exceptions.Add( ex2 );
                            ExceptionLogService.LogException( ex2, null );
                            Logger.LogError( ex, errorMessage );
                            continue;
                        }
                        finally
                        {
                            Log(
                                LogLevel.Information,
                                $"DataView ID: {dataViewId}, DataView Name: {name}, Error Occurred: {errorOccurred}",
                                startDateTime,
                                stopwatch.ElapsedMilliseconds );
                        }
                    }
                }
            }

            StringBuilder results = new StringBuilder();
            // Format the result message
            results.AppendLine( $"<i class='fa fa-circle text-success'></i> Updated {updatedDataViewCount} {"dataview".PluralizeIf( updatedDataViewCount != 1 )}" );

            if ( failedDataViews.Any() )
            {
                var topTwenty = failedDataViews.Take( 20 ).ToList();
                results.Append( $"<i class='fa fa-circle text-warning'></i> Skipped: {failedDataViews.Count} dataviews due to encountered errors:" );
                results.Append( "<ul>" );
                topTwenty.ForEach( e => results.Append( $"<li>{e}</li>" ) );
                results.Append( "</ul>" );
                results.AppendLine( "Enable \"Warning\" and \"Jobs\" domain under Rock Logs to view details" );
            }

            this.Result = results.ToString();

            if ( exceptions.Any() )
            {
                var exceptionList = new AggregateException( "One or more exceptions occurred in RockCleanup.", exceptions );
                throw new RockJobWarningException( "Update Persisted Dataviews completed with warnings", exceptionList );
            }
        }
    }
}