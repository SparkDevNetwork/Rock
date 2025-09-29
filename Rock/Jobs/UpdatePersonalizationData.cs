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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Rock.Attribute;
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Observability;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Update Personalization Data" )]
    [Description( "Job that updates Personalization Data." )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeoutSeconds,
        Description = "Maximum amount of time (in seconds) to wait for the sql operations to complete. Leave blank to use the default for this job (180).",
        IsRequired = false,
        DefaultIntegerValue = 60 * 3,
        Order = 1 )]

    [RockLoggingCategory]
    public class UpdatePersonalizationData : RockJob
    {
        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string CommandTimeoutSeconds = "CommandTimeoutSeconds";
        }

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public UpdatePersonalizationData()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            int commandTimeoutSeconds = this.GetAttributeValue( AttributeKey.CommandTimeoutSeconds ).AsIntegerOrNull() ?? 180;
            int updatedPersonalizationSegmentCount = 0;
            var failedPersonalizationSegments = new List<string>();
            var exceptions = new List<Exception>();
            var resultsBuilder = new StringBuilder();

            using ( var rockContext = new RockContext() )
            {
                var currentDateTime = RockDateTime.Now;
                var personalizationSegmentService = new PersonalizationSegmentService( rockContext );

                // Get all interval-based personalization segments due for refresh
                var pSegmentsWithIntervalForRefresh = personalizationSegmentService.Queryable()
                    .Where( s => s.IsActive && s.PersistedScheduleIntervalMinutes.HasValue )
                    .Where( s => !s.PersistedLastRefreshDateTime.HasValue ||
                        System.Data.Entity.SqlServer.SqlFunctions.DateAdd(
                            "mi",
                            s.PersistedScheduleIntervalMinutes.Value,
                            s.PersistedLastRefreshDateTime.Value
                        ) < currentDateTime )
                    .Select( s => s.Id )
                    .ToList();

                // Get all schedule-based personalization segments
                var pSegmentsWithScheduleSoft = personalizationSegmentService.Queryable()
                    .Where( s => s.IsActive && s.PersistedScheduleId.HasValue
                        && ( !s.PersistedLastRefreshDateTime.HasValue || s.PersistedLastRefreshDateTime.Value < currentDateTime ) )
                    .Include( s => s.PersistedSchedule )
                    .ToList();

                // Perform a more thorough check of personalization segments with persisted schedules to rule out those that don't need refreshed.
                var pSegmentsWithScheduleForRefresh = new HashSet<int>();

                foreach ( var pSegment in pSegmentsWithScheduleSoft )
                {
                    var schedule = pSegment.PersistedSchedule;
                    if ( schedule == null )
                    {
                        resultsBuilder.AppendLine( $"<i class='fa fa-circle text-warning'></i> {pSegment.Name} (ID: {pSegment.Id}) - Missing schedule (ID: {pSegment.PersistedScheduleId.Value})" );
                        continue;
                    }

                    var beginDateTime = pSegment.PersistedLastRefreshDateTime ?? schedule.GetFirstStartDateTime();
                    if ( !beginDateTime.HasValue )
                    {
                        // We don't know when this schedule was supposed to start, so ignore it (should never happen).
                        resultsBuilder.AppendLine( $"<i class='fa fa-circle text-warning'></i> {pSegment.Name} (ID: {pSegment.Id}) - Skipped: Unable to determine start time." );
                        continue;
                    }

                    var nextStartDateTimes = schedule.GetScheduledStartTimes( beginDateTime.Value, currentDateTime );
                    if ( nextStartDateTimes.Any() )
                    {
                        pSegmentsWithScheduleForRefresh.Add( pSegment.Id );
                    }
                }

                // Combine and deduplicate IDs
                var personalizationSegmentIdsToPersist = pSegmentsWithIntervalForRefresh
                    .Concat( pSegmentsWithScheduleForRefresh )
                    .Distinct()
                    .ToList();

                if ( !personalizationSegmentIdsToPersist.Any() )
                {
                    resultsBuilder.AppendLine( "No Personalization Segments are due for persistence." );
                }

                // Process each personalization segment by ID
                foreach ( var pSegmentId in personalizationSegmentIdsToPersist )
                {
                    this.UpdateLastStatusMessage( $"Personalization Segment ID {pSegmentId}: Persisting..." );
                    using ( var activity = ObservabilityHelper.StartActivity( $"Personalization Segment: {pSegmentId}" ) )
                    {
                        using ( var persistContext = new RockContext() )
                        {
                            persistContext.Database.SetCommandTimeout( commandTimeoutSeconds );
                            var stopwatch = Stopwatch.StartNew();
                            var startDateTime = RockDateTime.Now;
                            var errorOccurred = false;
                            var pSegment = new PersonalizationSegmentService( persistContext ).Get( pSegmentId );
                            string name = pSegment?.Name ?? $"ID {pSegmentId}";
                            try
                            {
                                var pSegmentCache = PersonalizationSegmentCache.Get( pSegmentId );
                                var pSegmentUpdateResults = new PersonalizationSegmentService( persistContext ).UpdatePersonAliasPersonalizationDataForSegment( pSegmentCache );

                                // Update persistence info
                                pSegment.PersistedLastRefreshDateTime = RockDateTime.Now;
                                pSegment.PersistedLastRunDurationMilliseconds = ( int ) stopwatch.Elapsed.TotalMilliseconds;
                                // Use SaveChanges(true) to disable pre/post processing hooks so that only persistence metadata is updated.
                                // This prevents ModifiedDateTime and other audit fields from being updated when only persisting results.
                                persistContext.SaveChanges( true );

                                // Because we are disabling the pre-post logic (includes cache flush), we need to manually flush the cached item.
                                PersonalizationSegmentCache.FlushItem( pSegmentId );

                                updatedPersonalizationSegmentCount++;

                                stopwatch.Stop();

                                if ( pSegmentUpdateResults.CountAddedSegment == 0 && pSegmentUpdateResults.CountRemovedFromSegment == 0 )
                                {
                                    resultsBuilder.AppendLine( $"<i class='fa fa-circle text-muted'></i> {name} - No changes." );
                                }
                                else
                                {
                                    resultsBuilder.AppendLine( $"<i class='fa fa-circle text-success'></i> {name} - {pSegmentUpdateResults.CountAddedSegment} added and {pSegmentUpdateResults.CountRemovedFromSegment} removed." );
                                }
                            }
                            catch ( Exception ex )
                            {
                                stopwatch.Stop();
                                errorOccurred = true;
                                failedPersonalizationSegments.Add( $"{name} (ID: {pSegmentId})" );
                                var errorMessage = $"An error occurred while updating personalization segment '{name}' (ID: {pSegmentId}). Error: {ex.Message}";
                                var ex2 = new Exception( errorMessage, ex );
                                exceptions.Add( ex2 );
                                ExceptionLogService.LogException( ex2, null );
                                Logger.LogError( ex, errorMessage );
                                resultsBuilder.AppendLine( $"<i class='fa fa-circle text-warning'></i> {name} (ID: {pSegmentId}) - Error: {ex.Message}" );
                                continue;
                            }
                            finally
                            {
                                Logger.LogInformation(
                                    $"PersonalizationSegment ID: {pSegmentId}, Name: {name}, Error Occurred: {errorOccurred}, Start: {startDateTime}, Duration: {stopwatch.ElapsedMilliseconds}ms"
                                );
                            }
                        }
                    }
                }
            }

            try
            {
                using ( var cleanupRockContext = new RockContext() )
                {
                    cleanupRockContext.Database.SetCommandTimeout( commandTimeoutSeconds );
                    var cleanedUpCount = new PersonalizationSegmentService( cleanupRockContext ).CleanupPersonAliasPersonalizationDataForSegmentsThatDontExist();
                    if ( cleanedUpCount > 0 )
                    {
                        resultsBuilder.AppendLine( $"<i class='fa fa-circle text-info'></i> Cleaned up {cleanedUpCount}" );
                    }
                }
            }
            catch ( Exception ex )
            {
                var errorMessage = $"An error occurred during cleanup: {ex.Message}";
                resultsBuilder.AppendLine( $"<i class='fa fa-circle text-warning'></i> Cleanup Error: {ex.Message}" );
                exceptions.Add( new Exception( errorMessage, ex ) );
            }

            if ( updatedPersonalizationSegmentCount > 0 || failedPersonalizationSegments.Any() )
            {
                resultsBuilder.Insert( 0, $"<i class='fa fa-circle text-success'></i> Updated {updatedPersonalizationSegmentCount} {( updatedPersonalizationSegmentCount == 1 ? "personalization segment" : "personalization segments" )}<br/>" );
            }

            if ( failedPersonalizationSegments.Any() )
            {
                var topTwenty = failedPersonalizationSegments.Take( 20 ).ToList();
                resultsBuilder.Append( $"<i class='fa fa-circle text-warning'></i> Skipped: {failedPersonalizationSegments.Count} personalization segments due to errors:<ul>" );
                topTwenty.ForEach( e => resultsBuilder.Append( $"<li>{e}</li>" ) );
                resultsBuilder.Append( "</ul>Enable 'Warning' and 'Jobs' domain under Rock Logs to view details<br/>" );
            }

            this.Result = resultsBuilder.ToString();
            this.UpdateLastStatusMessage( resultsBuilder.ToString() );

            if ( exceptions.Any() )
            {
                var exceptionList = new AggregateException( "One or more exceptions occurred in Update Personalization Data.", exceptions );
                throw new RockJobWarningException( "Update Personalization Data completed with warnings", exceptionList );
            }
        }
    }
}
