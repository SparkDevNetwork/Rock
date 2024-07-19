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
using System.Diagnostics;
using System.Linq;
using System.Text;

using Microsoft.Extensions.Logging;

using Rock.Attribute;
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.SystemKey;

namespace Rock.Jobs
{
    /// <summary>
    /// This job will update Interaction counts and Durations for InteractionSession records.
    /// </summary>
    [DisplayName( "Populate Interaction Session Data" )]
    [Description( "This job will update Interaction counts and Durations for InteractionSession records." )]

    [IntegerField(
        "Command Timeout",
        AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of Interactions, this could take several hours or more.",
        IsRequired = false,
        DefaultIntegerValue = AttributeDefaultValue.CommandTimeout )]

    [RockLoggingCategory]
    public class PopulateInteractionSessionData : RockJob
    {
        #region Keys

        /// <summary>
        /// Keys to use for the attributes
        /// </summary>
        public static class AttributeKey
        {
            /// <summary>
            /// IP Address GeoCoding Component
            /// </summary>
            [RockObsolete( "1.17" )]
            [Obsolete]
            public const string IPAddressGeoCodingComponent = "IPAddressGeoCodingComponent";

            /// <summary>
            /// Lookback Maximum in Days
            /// </summary>
            [RockObsolete( "1.17" )]
            [Obsolete]
            public const string LookbackMaximumInDays = "LookbackMaximumInDays";

            /// <summary>
            /// How Many Records
            /// </summary>
            [RockObsolete( "1.17" )]
            [Obsolete]
            public const string MaxRecordsToProcessPerRun = "HowManyRecords";

            /// <summary>
            /// Command Timeout
            /// </summary>
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// Default Values for Attributes
        /// </summary>
        private static class AttributeDefaultValue
        {
            public const int CommandTimeout = 3600; // one hour in seconds
        }

        #endregion Keys

        #region Fields

        private List<string> _errors;

        private List<Exception> _exceptions;

        private int? _commandTimeout = null;

        #endregion

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public PopulateInteractionSessionData()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            // Get the configured timeout, or default to 20 minutes if it is blank
            _commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;

            var settings = Rock.Web.SystemSettings
                .GetValue( SystemSetting.POPULATE_INTERACTION_SESSION_DATA_JOB_SETTINGS )
                .FromJsonOrNull<PopulateInteractionSessionDataJobSettings>() ?? new PopulateInteractionSessionDataJobSettings();

            var jobResult = Execute( settings );

            this.Result = jobResult.GetResultSummaryHtml();

            // Log caught exceptions
            if ( jobResult.Exception != null )
            {
                throw jobResult.Exception;
            }
        }

        internal RockJobResult Execute( PopulateInteractionSessionDataJobSettings settings )
        {
            _errors = new List<string>();
            _exceptions = new List<Exception>();

            var jobResult = new RockJobResult();

            // Update Interaction Counts and Durations for Session
            var result = ProcessInteractionCountAndDuration( settings );
            if ( result.IsNotNullOrWhiteSpace() )
            {
                jobResult.OutputMessages.Add( result );
            }

            // Print error messages
            foreach ( var error in _errors )
            {
                jobResult.OutputMessages.Add( $"<i class='fa fa-circle text-danger'></i> {error}" );
            }

            // Log caught exceptions
            if ( _exceptions.Any() )
            {
                var exceptionList = new AggregateException( "One or more exceptions occurred in Process Interaction Session Data.", _exceptions );
                jobResult.Exception = new RockJobWarningException( "Process Interaction Session Data completed with warnings", exceptionList );
            }

            return jobResult;
        }

        /// <summary>
        /// <para>Processes interaction sessions that are yet to have their interaction count and duration properties updated.</para>
        /// <para> If the job was run within the last 24 hours then all interaction sessions for interactions that happened within the last 24 hours are processed,</para>
        /// <para>if not then sessions that happened an hour before the last time the job was run are processed.</para>
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>System.String.</returns>
        private string ProcessInteractionCountAndDuration( PopulateInteractionSessionDataJobSettings settings )
        {
            // This portion of the job looks for interaction sessions that need to have their interaction count and
            // duration properties updated. This de-normalization occurs to increase performance of the analytics.
            // We'll be looking for sessions that have not been processed yet OR have had interactions written
            // since their last update.

            var stopwatch = Stopwatch.StartNew();

            var batchSize = 500;
            var totalRecordsProcessed = 0;
            var interactionCalculationDateTime = RockDateTime.Now;
            var oneDayAgo = RockDateTime.Now.AddDays( -1 );

            // We'll limit our process to sessions that started in the last 24 hours, or ones that haven't been calculated.
            var startDate = oneDayAgo;

            var lastSuccessfulJobRunDateTime = settings.LastSuccessfulJobRunDateTime;
            DateTime? cutoffStartDateTime;
            if ( lastSuccessfulJobRunDateTime.HasValue )
            {
                if ( lastSuccessfulJobRunDateTime.Value > oneDayAgo )
                {
                    // Have the cutoffStartDateTime be at least 1 day ago
                    cutoffStartDateTime = oneDayAgo;
                }
                else
                {
                    // If it has been more than a day since the job ran, have cutoffStartDateTime be an hour before that
                    cutoffStartDateTime = lastSuccessfulJobRunDateTime.Value.AddHours( -1 );
                }
            }
            else
            {
                cutoffStartDateTime = null;
            }

            while ( true )
            {
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.CommandTimeout = _commandTimeout;

                    var interactionSessions = GetInteractionSessionsForActivityUpdate( rockContext, startDate, cutoffStartDateTime, batchSize );

                    this.UpdateLastStatusMessage( $"Processing Interaction Count And Session Duration : {batchSize} sessions are being processed currently. Total {totalRecordsProcessed} Interaction Session{( totalRecordsProcessed < 2 ? "" : "s" )} are processed till now." );

                    if ( interactionSessions.Any() )
                    {
                        var minSessionId = interactionSessions.Min( s => s.Id );
                        var maxSessionId = interactionSessions.Max( s => s.Id );

                        LogDebugInfo( "Update Interaction Metadata", $"BatchSize={interactionSessions.Count}, SessionId={minSessionId} --> {maxSessionId}." );

                        foreach ( var interactionSession in interactionSessions )
                        {
                            // If a Guid.Empty value exists in the database, subsequent attempts to update the record
                            // will cause a validation error. In this case, flag the invalid record so the source can be tracked and corrected.
                            if ( interactionSession.Guid == Guid.Empty )
                            {
                                _errors.Add( $"Interaction Session with invalid Guid ignored. [Id={interactionSession.Id}]" );
                                continue;
                            }

                            // Special Query to only get what we need to know.
                            // This could cause a lot of database calls, but it is consistently just a few milliseconds.
                            // This seems to increase overall performance vs Eager loading all the interactions of the session
                            var interactionStats = new InteractionSessionService( rockContext ).Queryable().Where( a => a.Id == interactionSession.Id ).Select( s => new
                            {
                                Count = s.Interactions.Count(),
                                MaxDateTime = s.Interactions.Max( i => ( DateTime? ) i.InteractionDateTime ),
                                MinDateTime = s.Interactions.Min( i => ( DateTime? ) i.InteractionDateTime ),
                                InteractionChannelId = s.Interactions.FirstOrDefault().InteractionComponent.InteractionChannelId
                            } ).FirstOrDefault();

                            interactionSession.InteractionCount = interactionStats?.Count ?? 0;
                            interactionSession.InteractionChannelId = interactionStats?.InteractionChannelId;

                            // Calculate the session duration depending on the number of interactions. Note that we won't know the
                            // duration of time spend on the last page so we'll assume 60 seconds as the average amount of time
                            // spent on a page is 52 seconds https://www.klipfolio.com/metrics/marketing/average-time-on-page
                            switch ( interactionSession.InteractionCount )
                            {
                                case int x when x > 1:
                                    {
                                        // When there is 2 or more interactions calculate the time between the interaction dates
                                        interactionSession.DurationSeconds = ( int ) ( ( interactionStats.MaxDateTime - interactionStats.MinDateTime )?.TotalSeconds ?? 0 ) + 60;
                                        break;
                                    }
                                case 1:
                                    {
                                        // Only one page view so we'll assume 60 seconds
                                        interactionSession.DurationSeconds = 60;
                                        break;
                                    }
                                default:
                                    {
                                        // Not sure how a session was created without an interaction but we give that a zero
                                        interactionSession.DurationSeconds = 0;
                                        break;
                                    }
                            }

                            interactionSession.DurationLastCalculatedDateTime = interactionCalculationDateTime;

                            totalRecordsProcessed += 1;
                        }

                        rockContext.SaveChanges( disablePrePostProcessing: true );
                    }

                    // Stop looping if we're out of sessions to process
                    if ( interactionSessions.Count() < batchSize )
                    {
                        break;
                    }
                }
            }

            settings.LastSuccessfulJobRunDateTime = interactionCalculationDateTime;
            settings.LastNullDurationLastCalculatedDateTimeUpdateDateTime = interactionCalculationDateTime;

            stopwatch.Stop();
            LogDebugInfo( "Process Interaction Count And Duration", $"Completed in {stopwatch.Elapsed.TotalSeconds:0.00}s." );

            Rock.Web.SystemSettings.SetValue( SystemSetting.POPULATE_INTERACTION_SESSION_DATA_JOB_SETTINGS, settings.ToJson() );

            return $"<i class='fa fa-circle text-success'></i> Updated Interaction Count And Session Duration for {totalRecordsProcessed} {"interaction session".PluralizeIf( totalRecordsProcessed != 1 )} in {Math.Round( stopwatch.Elapsed.TotalSeconds, 2 )} secs.";
        }

        /// <summary>
        /// Get recent sessions that have had new interactions since it was last processed and sessions that have not been processed yet.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="cutoffStartDateTime">The cutoff start date time.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <returns></returns>
        private List<InteractionSession> GetInteractionSessionsForActivityUpdate( RockContext rockContext, DateTime startDate, DateTime? cutoffStartDateTime, int batchSize )
        {
            // Get recent sessions that have had new interactions since it was last processed
            // and then also look for sessions that have not been processed yet (on first run this could be a lot)

            /* 2022-08-30 MDP

              If the Job has successfully run before, only look for sessions that have had interactions since the job successfully ran (or at least one day ago if it was run more recently).
              This will help avoid an expensive full table scan on the InteractionSession table and/or Interactions Table.

            */

            var outOfDateInteractionSessionIdsQuery = new InteractionService( rockContext ).Queryable()
                .Where( a => a.InteractionSessionId != null
                    && a.InteractionSession.DurationLastCalculatedDateTime.HasValue
                    && a.InteractionDateTime > startDate
                    && a.InteractionDateTime > a.InteractionSession.DurationLastCalculatedDateTime ).Select( a => a.InteractionSessionId.Value ).Distinct();

            List<InteractionSession> interactionSessionsWithOutOfDate;

            /* 2022-08-31 MP

             Add an OPTION (RECOMPILE) to these queries to help keep consistent performance. We
             are thinking that since the Interaction Table is heavily modified, this gives
             SQL Server a chance to make sure the query plan is still optimal. This
             seems to fix situations where the query would sometimes take several minutes, instead of
             just few seconds or less.

             */
            using ( new QueryHintScope( rockContext, QueryHintType.RECOMPILE ) )
            {
                interactionSessionsWithOutOfDate = new InteractionSessionService( rockContext )
                    .Queryable()
                    .Where( s => outOfDateInteractionSessionIdsQuery.Contains( s.Id ) )
                    .Take( batchSize ).ToList();
            }

            var remainingBatchSize = batchSize - interactionSessionsWithOutOfDate.Count();

            IQueryable<InteractionSession> interactionSessionsWithNullDurationLastCalculatedDateTimeQuery;

            if ( cutoffStartDateTime.HasValue )
            {
                var recentInteractionSessionIdsQuery = new InteractionService( rockContext ).Queryable().Where( a => a.CreatedDateTime > cutoffStartDateTime && a.InteractionSessionId.HasValue ).Select( a => a.InteractionSessionId.Value ).Distinct();
                interactionSessionsWithNullDurationLastCalculatedDateTimeQuery = new InteractionSessionService( rockContext )
                    .Queryable().Where( s => s.DurationLastCalculatedDateTime == null && recentInteractionSessionIdsQuery.Contains( s.Id ) );
            }
            else
            {
                interactionSessionsWithNullDurationLastCalculatedDateTimeQuery = new InteractionSessionService( rockContext )
                    .Queryable().Where( s => s.DurationLastCalculatedDateTime == null && s.Interactions.Any() );
            }

            List<InteractionSession> interactionSessionsWithNullDurationLastCalculatedDateTime;

            using ( new QueryHintScope( rockContext, QueryHintType.RECOMPILE ) )
            {
                interactionSessionsWithNullDurationLastCalculatedDateTime = interactionSessionsWithNullDurationLastCalculatedDateTimeQuery
                .OrderByDescending( a => a.Id )
                .Take( remainingBatchSize ).ToList();
            }

            var interactionSessions = interactionSessionsWithNullDurationLastCalculatedDateTime.Union( interactionSessionsWithOutOfDate ).ToList();

            return interactionSessions;
        }

        private void LogDebugInfo( string taskName, string message )
        {
            Logger.LogDebug( $"({taskName}): {message}" );
        }

        internal class PopulateInteractionSessionDataJobSettings
        {
            public DateTime? LastSuccessfulJobRunDateTime { get; set; }
            public DateTime? LastNullDurationLastCalculatedDateTimeUpdateDateTime { get; set; }
        }

        /// <summary>
        /// The result data from a Rock Job.
        /// </summary>
        internal class RockJobResult
        {
            public List<string> OutputMessages { get; set; } = new List<string>();

            public string GetResultSummaryHtml()
            {
                var results = new StringBuilder();

                foreach ( var message in OutputMessages )
                {
                    if ( message.IsNotNullOrWhiteSpace() )
                    {
                        results.AppendLine( message );
                    }
                }

                return results.ToString();
            }

            /// <summary>
            /// Gets or sets the amount of time taken
            /// </summary>
            /// <value>
            /// The time.
            /// </value>
            public TimeSpan ExecutionTime { get; set; }

            public bool HasException => Exception != null;

            public Exception Exception { get; set; }
        }
    }
}