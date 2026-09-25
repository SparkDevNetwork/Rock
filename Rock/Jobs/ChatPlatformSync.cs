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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Contract;
using Rock.Communication.Chat.Platform.Session;
using Rock.Communication.Chat.Platform.Sync;
using Rock.Data;
using Rock.Web.Cache;

using ChatSyncAcknowledgement = Rock.Communication.Chat.Platform.Sync.ChatSyncSubmitClient.ChatSyncAcknowledgement;
using ChatSyncOutcome = Rock.Communication.Chat.Platform.Sync.ChatSyncSubmitClient.ChatSyncOutcome;
using ChatSyncPollBudget = Rock.Communication.Chat.Platform.Sync.ChatSyncSubmitClient.ChatSyncPollBudget;
using ChatSyncSubmissionStatus = Rock.Communication.Chat.Platform.Sync.ChatSyncSubmitClient.ChatSyncSubmissionStatus;

namespace Rock.Jobs
{
    [DisplayName( "Chat Platform Sync" )]
    [Description( "Sends this church's people, channels, memberships and badges to the chat platform, as a whole picture each time." )]
    /// <summary>
    /// Sends this church's people, channels, memberships and badges to the chat platform.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Every run sends the whole picture rather than what changed, so a run that does not
    ///         happen costs nothing the next one cannot put right, and a run that happens twice
    ///         writes the same thing twice. That is what lets this job give up early, skip itself
    ///         when the platform asks for quiet, and be pressed by hand as often as anyone likes.
    ///     </para>
    ///     <para>
    ///         Named apart from the Chat Sync job, which belongs to the other chat provider. The two
    ///         appear side by side on the Jobs Administration page and do entirely different things,
    ///         so they must not be read as one job under two names.
    ///     </para>
    /// </remarks>
    public class ChatPlatformSync : RockJob
    {
        #region Constants

        // A run started by hand gets its own scheduler, named this way by the page that starts it.
        // It is the only thing here that tells a person's run from the schedule's, and the
        // difference matters twice: a person is waiting, so the platform's backoff does not hold
        // them up, and their submission is marked urgent so it is drained ahead of the queue.
        private const string ManualRunSchedulerPrefix = "RunNow:";

        // The longest quiet stretch a schedule may leave before it is worth saying so. A design
        // bound on how stale the platform's picture of a church may get, not a measurement, and
        // provisional until the platform is measured at full scale.
        private static readonly TimeSpan MaximumScheduleGap = TimeSpan.FromHours( 24 );

        // How far ahead a schedule is read. A fixed handful of fires is not enough: an hourly
        // weekday schedule spends its first fourteen on the hour and the overnight, and the
        // weekend, which is the gap over a day, sits further along. Eight days covers a weekly
        // pattern whichever day the reading starts on.
        private static readonly TimeSpan ScheduleSampleWindow = TimeSpan.FromDays( 8 );

        // Where the walk stops if it never reaches that window. Eight days of a fire every second
        // is 691200 steps, and the cap sits above that so a schedule that dense is still judged
        // across the whole window rather than cut off inside it.
        private const int ScheduleSampleCap = 700000;

        // How long the projection may take. Generous, because it reads the whole of a large
        // church's group membership and runs on that church's own server, and because the cost of
        // being wrong here is a cycle lost rather than a cycle wrong. An estimate.
        private const int ProjectionTimeoutSeconds = 300;

        #endregion Constants

        #region Execute

        /// <inheritdoc />
        public override void Execute()
        {
            var configuration = ChatPlatformConfigurationService.Read();

            if ( !configuration.IsConfigured )
            {
                Result = configuration.HasBeenEnabled
                    ? "Nothing was sent. Chat is set up for this church, but this installation cannot read the signing key it was given."
                    : "Nothing was sent. Chat is not set up for this church.";
                return;
            }

            var isManualRun = IsManualRun();

            // The instant, not the organisation's wall clock. The backoff below is an instant the
            // platform named with its offset, and a wall-clock reading would be stamped with this
            // server's offset, which on a hosted server is not the organisation's, and be wrong by
            // the difference.
            var now = DateTimeOffset.UtcNow;

            // Carried whether or not the run goes ahead. A church whose schedule is too slow and
            // whose platform is asking for quiet has two things wrong and should be told both.
            var scheduleWarning = ScheduleWarning( ServiceJob?.CronExpression, now );

            var skipReason = SkipReason( isManualRun, configuration.SyncBackoffUntil, now );
            if ( skipReason != null )
            {
                Result = Join( skipReason, scheduleWarning );
                return;
            }

            RunResult outcome;
            using ( var rockContext = new RockContext() )
            {
                outcome = Run( rockContext, configuration, isManualRun );
            }

            Result = Join( outcome.Message, scheduleWarning );

            if ( outcome.IsFailure )
            {
                // Thrown rather than returned, because the scheduler is what records a run as
                // failed and it only learns that from an exception. The message is already on the
                // result, so this carries no detail the church has not been shown.
                throw new RockJobWarningException( Result );
            }
        }

        /// <summary>
        /// Whether a person started this run.
        /// </summary>
        private bool IsManualRun()
        {
            var schedulerName = Scheduler?.SchedulerName;

            return schedulerName != null
                && schedulerName.StartsWith( ManualRunSchedulerPrefix, StringComparison.OrdinalIgnoreCase );
        }

        #endregion Execute

        #region Whether the run happens

        /// <summary>
        /// Why this run did nothing, or null where it goes ahead.
        /// </summary>
        /// <param name="isManualRun">Whether a person started this run rather than the schedule.</param>
        /// <param name="backoffUntil">The time the chat platform last asked not to be called before.</param>
        /// <param name="now">The current instant.</param>
        /// <returns>The reason, or null where the run may go ahead.</returns>
        /// <remarks>
        /// The backoff is the platform's advice about its own load, and it binds the schedule but not a
        /// person: someone who pressed Sync Now is at a screen waiting for an answer, and the cost of
        /// letting them through is one submission the platform would rather have had later.
        /// </remarks>
        internal static string SkipReason( bool isManualRun, DateTimeOffset? backoffUntil, DateTimeOffset now )
        {
            if ( isManualRun || !backoffUntil.HasValue || backoffUntil.Value <= now )
            {
                return null;
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "Nothing was submitted. The chat platform asked for a backoff until {0}, and this run was started by the schedule rather than by a person. Sync Now ignores the backoff.",
                backoffUntil.Value.ToUniversalTime().ToString( "yyyy-MM-dd HH:mm:ss'Z'", CultureInfo.InvariantCulture ) );
        }

        /// <summary>
        /// Refused at compile time.
        /// </summary>
        /// <param name="isManualRun">Whether a person started this run rather than the schedule.</param>
        /// <param name="backoffUntil">The time the chat platform last asked not to be called before.</param>
        /// <param name="now">A wall-clock reading, which is the mistake this exists to stop.</param>
        /// <returns>Nothing; calling it does not compile.</returns>
        /// <remarks>
        /// A <see cref="DateTime"/> handed to the overload above would be converted to an instant with
        /// this server's offset. Rock's own clock returns the organisation's wall clock, which on a
        /// hosted server is not in this server's zone, so the backoff would be compared against a moment
        /// wrong by the difference: honoured hours past its expiry, or released hours early.
        /// </remarks>
        [Obsolete( "Pass an instant, such as DateTimeOffset.UtcNow. A DateTime is converted with this server's offset, which is not the organisation's, and the backoff is then compared against the wrong moment.", true )]
        internal static string SkipReason( bool isManualRun, DateTimeOffset? backoffUntil, DateTime now )
        {
            throw new NotSupportedException( "a backoff cannot be judged against a wall-clock reading" );
        }

        /// <summary>
        /// What is worth saying about this schedule, or null where there is nothing.
        /// </summary>
        /// <param name="cronExpression">The schedule.</param>
        /// <param name="after">The moment to look forward from.</param>
        /// <returns>The warning, or null.</returns>
        /// <remarks>
        /// The schedule is the church's own setting and is remarked on rather than corrected: nothing
        /// here writes to the job.
        /// </remarks>
        internal static string ScheduleWarning( string cronExpression, DateTimeOffset after )
        {
            var longest = LongestGap( cronExpression, after );
            if ( !longest.HasValue || longest.Value <= MaximumScheduleGap )
            {
                return null;
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "This schedule leaves up to {0} between runs. Chat is meant to be restated at least every 24 hours, "
                    + "and a longer gap leaves the chat platform holding a stale picture of this church and reading it as offline. "
                    + "The schedule has not been changed.",
                DescribeGap( longest.Value ) );
        }

        /// <summary>
        /// The longest gap between consecutive runs of this schedule, looking forward from a moment.
        /// </summary>
        /// <param name="cronExpression">The schedule.</param>
        /// <param name="after">The moment to look forward from.</param>
        /// <returns>The longest gap, or null where the schedule cannot be read or has no future runs.</returns>
        /// <remarks>
        /// The longest gap and not the next one, because the schedules that go wrong quietly are the ones
        /// that look frequent. A weekday morning schedule fires five times a week and leaves seventy two
        /// hours over every weekend, and the gap after any given Monday run is a reassuring twenty four.
        /// The same is true of a schedule that fires every hour through a weekday: the first handful of
        /// gaps are an hour or the overnight, and the weekend is only visible once the walk has covered
        /// a week.
        /// </remarks>
        private static TimeSpan? LongestGap( string cronExpression, DateTimeOffset after )
        {
            if ( cronExpression.IsNullOrWhiteSpace() )
            {
                return null;
            }

            Quartz.CronExpression expression;
            try
            {
                expression = new Quartz.CronExpression( cronExpression );
            }
            catch ( Exception )
            {
                // A schedule this job cannot read is the scheduler's to complain about. It has
                // already refused to run on it, or it is running on something this does not
                // understand; either way a second opinion from here would only be noise.
                return null;
            }

            // Measured in elapsed time rather than wall clock. A daily schedule genuinely spans
            // twenty five hours on the morning the clocks go back, and reporting a church's
            // schedule as too slow once a year for that reason would be wrong every time.
            expression.TimeZone = TimeZoneInfo.Utc;

            var previous = after;
            var windowEnd = after + ScheduleSampleWindow;
            TimeSpan? longest = null;

            for ( var step = 0; step < ScheduleSampleCap && previous < windowEnd; step++ )
            {
                var next = expression.GetNextValidTimeAfter( previous );
                if ( !next.HasValue )
                {
                    break;
                }

                var gap = next.Value - previous;
                if ( !longest.HasValue || gap > longest.Value )
                {
                    longest = gap;
                }

                previous = next.Value;
            }

            return longest;
        }

        /// <summary>
        /// A gap in the roundest words it fits, because a church reads this on a job page.
        /// </summary>
        /// <param name="gap">The gap.</param>
        /// <returns>The words.</returns>
        private static string DescribeGap( TimeSpan gap )
        {
            if ( gap.TotalDays >= 2 )
            {
                return string.Format( CultureInfo.InvariantCulture, "{0:0.#} days", gap.TotalDays );
            }

            return string.Format( CultureInfo.InvariantCulture, "{0:0.#} hours", gap.TotalHours );
        }

        #endregion Whether the run happens

        #region The run

        /// <summary>
        /// One whole restatement: mark the channels, read the church, send it, and find out what happened
        /// to it.
        /// </summary>
        /// <param name="rockContext">The context the projection reads through.</param>
        /// <param name="configuration">The church's chat settings.</param>
        /// <param name="isManualRun">Whether a person started this run rather than the schedule.</param>
        /// <returns>What the run has to say for itself.</returns>
        internal static RunResult Run( RockContext rockContext, ChatPlatformConfiguration configuration, bool isManualRun )
        {
            if ( rockContext == null )
            {
                throw new ArgumentNullException( nameof( rockContext ) );
            }

            if ( configuration == null )
            {
                throw new ArgumentNullException( nameof( configuration ) );
            }

            var token = ChatSessionHelper.TryMintSyncToken( new ChatSessionContext { Configuration = configuration } );
            if ( !token.Success )
            {
                return new RunResult
                {
                    IsFailure = true,
                    Message = "Nothing was submitted. This church could not sign a request to the chat platform: " + token.Gate + "."
                };
            }

            var submissionId = Guid.NewGuid();
            var projection = Project( rockContext, configuration );
            var rowCounts = projection.RowCounts;

            // Built after the payload rather than before it, because the counts have to be the rows
            // that were actually written. Counts taken from what the projection was expected to
            // return would agree with a truncated payload and the platform's own check would pass
            // over it.
            var headers = new ChatSyncHeaderBuilder().BuildSubmissionHeaders(
                projection.ReadAtUtc,
                projection.Marks,
                rowCounts,
                Rock.VersionInfo.VersionInfo.GetRockSemanticVersionNumber(),
                isManualRun );

            string failure;
            // Minted fresh here rather than reused from the check at the top: reading a large church
            // can take long enough that the first token would be refused as too old to exchange.
            var platformToken = ChatSyncCredential.Exchange( configuration, MintToken( configuration ), out failure );
            if ( platformToken == null )
            {
                return new RunResult
                {
                    IsFailure = true,
                    Message = Describe( submissionId, rowCounts, "nothing was submitted, because " + failure )
                };
            }

            using ( var client = new ChatSyncSubmitClient( configuration, () => platformToken ) )
            {
                var acknowledgement = client.Submit( submissionId, projection.Payload, headers );

                if ( acknowledgement.CarriesBackoffAdvice )
                {
                    // Written for a refusal as for an acceptance: advice about the platform's load
                    // is no less true because this submission was turned away. Not written for a
                    // run that never got the platform's own answer, whose silence would otherwise
                    // clear advice the platform had given.
                    ChatPlatformConfigurationService.SaveSyncBackoff( acknowledgement.SyncBackoffUntil );
                }

                ChatSyncOutcome polled = null;
                if ( acknowledgement.Status == ChatSyncSubmissionStatus.Accepted )
                {
                    polled = client.Poll( submissionId,
                        isManualRun ? ChatSyncPollBudget.Manual : ChatSyncPollBudget.Scheduled );
                }

                var result = Resolve( acknowledgement, polled );

                result.Message = Describe( submissionId, rowCounts, result.Message );

                return result;
            }
        }

        /// <summary>
        /// Marks the groups that are chat channels right now, then reads the church once, without
        /// sending anything.
        /// </summary>
        /// <param name="rockContext">The context the projection reads through.</param>
        /// <param name="configuration">The church's chat settings.</param>
        /// <returns>The reading.</returns>
        /// <remarks>
        /// One call to the projection procedure, read as it streams. Its first statement is the
        /// marking, which commits on its own because no transaction spans the call: a mark rolled back
        /// with a failed submission would leave the next run treating a group as though it had never
        /// been a channel, and that is the one thing the mark exists to prevent. Then come the clock,
        /// the identity seeds and every section, as five result sets. The sections read sets the
        /// procedure stages in temporary tables that live until it returns, which is what stops a
        /// membership arriving in the same payload as neither the channel nor the person it names.
        /// </remarks>
        internal static ProjectionResult Project( RockContext rockContext, ChatPlatformConfiguration configuration )
        {
            if ( rockContext == null )
            {
                throw new ArgumentNullException( nameof( rockContext ) );
            }

            if ( configuration == null )
            {
                throw new ArgumentNullException( nameof( configuration ) );
            }

            // The context owns this connection, so it is closed here only if it was opened here.
            // Disposing it would leave the caller holding a context that cannot read anything.
            var connection = rockContext.Database.Connection;
            var wasClosed = connection.State != ConnectionState.Open;

            try
            {
                if ( wasClosed )
                {
                    connection.Open();
                }

                var result = new ProjectionResult();

                // Streamed rather than filled into tables, because a filled result holds every row of
                // the largest church in memory before the body is even written.
                using ( var command = CreateCommand( connection, configuration ) )
                using ( var reader = command.ExecuteReader() )
                {
                    ReadMarks( reader, result );

                    IDictionary<string, int> rowCounts;
                    result.Payload = BuildPayload( reader, out rowCounts );
                    result.RowCounts = rowCounts;
                }

                return result;
            }
            finally
            {
                if ( wasClosed && connection.State == ConnectionState.Open )
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// The moment this restatement describes and the identity seed of each table the projection
        /// reads, from the result set the reader is currently on.
        /// </summary>
        /// <param name="reader">The reader, on the procedure's first result set.</param>
        /// <param name="result">The reading to record them on.</param>
        /// <remarks>
        /// The moment is taken from the database rather than from this process, and in UTC because it
        /// is compared against the platform's own clock.
        /// </remarks>
        private static void ReadMarks( DbDataReader reader, ProjectionResult result )
        {
            if ( !reader.Read() )
            {
                throw new InvalidOperationException( "the identity marks of the tables this projection reads could not be taken" );
            }

            result.ReadAtUtc = DateTime.SpecifyKind( reader.GetDateTime( 0 ), DateTimeKind.Utc );
            result.Marks = new ChatSyncIdentityMarks
            {
                Person = reader.GetInt64( 1 ),
                PersonAlias = reader.GetInt64( 2 ),
                Group = reader.GetInt64( 3 ),
                GroupMember = reader.GetInt64( 4 )
            };
        }

        /// <summary>
        /// Writes every section from the result sets after the one the reader is on into one buffer.
        /// </summary>
        /// <param name="reader">The reader, on the result set before the first section's.</param>
        /// <param name="rowCounts">The rows actually written, by section.</param>
        /// <returns>The body, as the one buffer it was written into.</returns>
        private static ArraySegment<byte> BuildPayload( DbDataReader reader, out IDictionary<string, int> rowCounts )
        {
            var contract = JObject.Parse( ChatWireContract.Json );
            rowCounts = new Dictionary<string, int>();

            // The body is encoded as it is written and handed on as the one buffer it was written
            // into. Held as text and then encoded for the transport it would be two copies of the
            // same bytes, and at the largest church measured that is tens of megabytes on the large
            // object heap for nothing.
            var body = new MemoryStream();

            // No byte order mark: the platform reads this body as UTF-8 text, and those three bytes
            // would be the first thing its parser saw. The buffer size is here only because this is
            // the overload that leaves the stream open, which the buffer handed to the transport
            // below depends on; 64 KB rather than the 1 KB default is a choice, not a measurement.
            using ( var text = new StreamWriter( body, new UTF8Encoding( false ), 64 * 1024, true ) )
            using ( var jsonWriter = new JsonTextWriter( text ) { CloseOutput = false } )
            {
                WriteSections( reader, contract, jsonWriter, RockDateTime.OrgTimeZoneInfo, rowCounts );
            }

            ArraySegment<byte> buffer;

            if ( !body.TryGetBuffer( out buffer ) )
            {
                throw new InvalidOperationException( "the submission body was written into a buffer that cannot be handed on to the transport" );
            }

            return buffer;
        }

        /// <summary>
        /// The one call to the projection procedure.
        /// </summary>
        /// <param name="connection">The open connection.</param>
        /// <param name="configuration">The church's chat settings.</param>
        /// <returns>The command.</returns>
        private static DbCommand CreateCommand( DbConnection connection, ChatPlatformConfiguration configuration )
        {
            var command = connection.CreateCommand();
            command.CommandText = "[dbo].[spChat_SyncProjection]";
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = ProjectionTimeoutSeconds;

            foreach ( var parameter in ProjectionParameters( configuration ) )
            {
                var bound = command.CreateParameter();
                bound.ParameterName = parameter.Key;
                bound.Value = parameter.Value ?? DBNull.Value;
                command.Parameters.Add( bound );
            }

            return command;
        }

        /// <summary>
        /// Everything the projection procedure asks to be told rather than look up for itself.
        /// </summary>
        /// <param name="configuration">The church's chat settings.</param>
        /// <returns>The parameters, by name.</returns>
        private static IDictionary<string, object> ProjectionParameters( ChatPlatformConfiguration configuration )
        {
            var badgeGuids = configuration.ChatBadgeDataViewGuids ?? new List<Guid>();
            var activeStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );

            return new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase )
            {
                // The channel mark is in the organization's time, as every date Rock stores is.
                { "@StampedAt", RockDateTime.Now },
                { "@ChatPeopleGroupGuid", Rock.SystemGuid.Group.GROUP_CHAT_PEOPLE.AsGuid() },
                { "@ChatBanListGroupGuid", Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST.AsGuid() },
                { "@ChatAdministratorsGroupGuid", Rock.SystemGuid.Group.GROUP_CHAT_ADMINISTRATORS.AsGuid() },
                { "@ChatSystemAuthorGuid", Rock.SystemGuid.Person.CHAT_SYSTEM_AUTHOR.AsGuid() },
                { "@DirectMessageGroupTypeGuid", Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE.AsGuid() },
                { "@BadgeDataViewGuidsJson", new JArray( badgeGuids.Select( g => g.ToString() ) ).ToString( Formatting.None ) },
                { "@PersonEntityTypeId", EntityTypeCache.GetId<Rock.Model.Person>() },
                { "@ActiveRecordStatusValueId", activeStatus == null ? ( object ) DBNull.Value : activeStatus.Id },
                { "@ProfilesVisibleByDefault", configuration.AreChatProfilesVisible },
                { "@OpenDirectMessagesByDefault", configuration.IsOpenDirectMessagingAllowed },
                // The icon address is built from this, so the slash between root and path is
                // supplied here rather than trusted to however the administrator typed the root.
                { "@PublicApplicationRoot", ( GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) ?? string.Empty ).EnsureTrailingForwardslash() }
            };
        }

        /// <summary>
        /// A fresh church token, for the exchange that yields the run's platform token.
        /// </summary>
        /// <param name="configuration">The church's chat settings.</param>
        /// <returns>The token, or null where one could not be minted.</returns>
        /// <remarks>
        /// A church token lasts minutes, and the platform will not exchange one with less than two
        /// left, so a token minted before a large church was read could be too old by the time it
        /// is exchanged, on exactly the churches this matters most for.
        /// </remarks>
        private static string MintToken( ChatPlatformConfiguration configuration )
        {
            var minted = ChatSessionHelper.TryMintSyncToken( new ChatSessionContext { Configuration = configuration } );

            return minted.Success ? minted.ChurchToken : null;
        }

        /// <summary>
        /// How many rows a section carried, or zero where the section is not in the tally at all.
        /// </summary>
        /// <param name="rowCounts">The rows written, by section.</param>
        /// <param name="section">The section name.</param>
        /// <returns>The count.</returns>
        private static int Count( IDictionary<string, int> rowCounts, string section )
        {
            int count;
            return rowCounts != null && rowCounts.TryGetValue( section, out count ) ? count : 0;
        }

        #endregion The run

        #region Writing the body

        /// <summary>
        /// Writes the body: one object keyed by the contract's section names, each holding that
        /// section's rows as positional arrays in the contract's column order.
        /// </summary>
        /// <param name="reader">The reader, on the result set before the first section's.</param>
        /// <param name="contract">The parsed wire contract.</param>
        /// <param name="writer">Where the body is written.</param>
        /// <param name="organizationTimeZone">The zone Rock's stored times are in.</param>
        /// <param name="rowCounts">Receives the rows of each section, counted as each one is written.</param>
        internal static void WriteSections( DbDataReader reader, JObject contract, JsonWriter writer, TimeZoneInfo organizationTimeZone, IDictionary<string, int> rowCounts )
        {
            var sections = new ChatSyncHeaderBuilder( contract ).GetPayloadSections();
            var tables = contract["tables"] as JArray;

            // A section holds the rows of the table in the same position, which is the only thing
            // that ties a section to its columns.
            if ( tables == null || tables.Count != sections.Count )
            {
                throw new InvalidOperationException( "the chat wire contract names a different number of payload sections than tables, so no section can be matched to its columns" );
            }

            writer.WriteStartObject();

            for ( var i = 0; i < sections.Count; i++ )
            {
                var section = sections[i];

                // A section with no result set is a projection that did not run, not a church with none
                // of that row, and the platform applies a restatement as truth.
                if ( !reader.NextResult() )
                {
                    throw new InvalidOperationException( string.Format( "the projection returned no result set for the {0} section", section ) );
                }

                var columns = ResolveColumns( reader, section, tables[i]["columns"].Select( c => c.Value<string>() ).ToList() );
                var written = 0;
                rowCounts[section] = written;

                writer.WritePropertyName( section );
                writer.WriteStartArray();

                while ( reader.Read() )
                {
                    WriteRow( reader, columns, writer, organizationTimeZone, section );
                    rowCounts[section] = ++written;
                }

                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }

        /// <summary>
        /// Finds, once per section, the result set column each wire column is read from.
        /// </summary>
        /// <param name="reader">The reader, on the section's result set.</param>
        /// <param name="section">The payload section.</param>
        /// <param name="wireColumns">The section's wire columns, in the contract's order.</param>
        /// <returns>Where each wire column's value is read from, in the contract's order.</returns>
        private static WireColumn[] ResolveColumns( DbDataReader reader, string section, IList<string> wireColumns )
        {
            var ordinals = new Dictionary<string, int>( StringComparer.OrdinalIgnoreCase );

            for ( var i = 0; i < reader.FieldCount; i++ )
            {
                ordinals[reader.GetName( i )] = i;
            }

            var resolved = new WireColumn[wireColumns.Count];

            for ( var i = 0; i < wireColumns.Count; i++ )
            {
                var conversion = ConversionFor( wireColumns[i] );
                var source = conversion == WireConversion.Background || conversion == WireConversion.Foreground
                    ? "highlight_color"
                    : wireColumns[i];

                int ordinal;

                // Filling a missing column with null would keep every row the right width, and that
                // column would be empty for every church with nothing reporting it.
                if ( !ordinals.TryGetValue( source, out ordinal ) )
                {
                    throw new InvalidOperationException( string.Format(
                        "the {0} result set returns no {1}, which the {2} column on the wire is built from",
                        section,
                        source,
                        wireColumns[i] ) );
                }

                resolved[i] = new WireColumn( ordinal, conversion );
            }

            return resolved;
        }

        /// <summary>
        /// How a wire column's value is made from what Rock stores.
        /// </summary>
        /// <param name="wireColumn">The wire column.</param>
        /// <returns>The conversion.</returns>
        private static WireConversion ConversionFor( string wireColumn )
        {
            switch ( wireColumn )
            {
                case "badge_keys":
                    return WireConversion.BadgeKeys;
                case "ban_expires_at":
                    return WireConversion.Utc;
                case "bg_color":
                    return WireConversion.Background;
                case "fg_color":
                    return WireConversion.Foreground;
                default:
                    return WireConversion.None;
            }
        }

        /// <summary>
        /// Writes the row the reader is on.
        /// </summary>
        /// <param name="reader">The reader, on a row.</param>
        /// <param name="columns">Where each wire column is read from, in the contract's order.</param>
        /// <param name="writer">Where the row is written.</param>
        /// <param name="organizationTimeZone">The zone Rock's stored times are in.</param>
        /// <param name="section">The payload section, for a failure message.</param>
        private static void WriteRow( DbDataReader reader, WireColumn[] columns, JsonWriter writer, TimeZoneInfo organizationTimeZone, string section )
        {
            Tuple<string, string> colors = null;

            writer.WriteStartArray();

            foreach ( var column in columns )
            {
                var stored = reader.GetValue( column.Ordinal );

                switch ( column.Conversion )
                {
                    case WireConversion.BadgeKeys:
                        WriteValue( writer, ReadBadgeKeys( stored ), section );
                        break;
                    case WireConversion.Utc:
                        WriteValue( writer, ToUtc( stored, organizationTimeZone ), section );
                        break;
                    case WireConversion.Background:
                        colors = colors ?? ReadBadgeColors( stored );
                        WriteValue( writer, colors.Item1, section );
                        break;
                    case WireConversion.Foreground:
                        colors = colors ?? ReadBadgeColors( stored );
                        WriteValue( writer, colors.Item2, section );
                        break;
                    default:
                        WriteValue( writer, stored, section );
                        break;
                }
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// Writes one value in the form the platform parses it from.
        /// </summary>
        /// <param name="writer">Where the value is written.</param>
        /// <param name="value">The value.</param>
        /// <param name="section">The payload section, for a failure message.</param>
        internal static void WriteValue( JsonWriter writer, object value, string section )
        {
            if ( value == null || value == DBNull.Value )
            {
                writer.WriteNull();
                return;
            }

            if ( value is Guid )
            {
                // Lowercase and hyphenated is the one form that parses as a uuid on the far side and
                // compares equal to the same value already stored there. "D" is already lowercase.
                writer.WriteValue( ( ( Guid ) value ).ToString( "D" ) );
                return;
            }

            if ( value is DateTime )
            {
                var time = ( DateTime ) value;

                // The platform reads a time with no offset as UTC, so a time in any other zone would be
                // wrong by this church's offset. The caller converts; this refuses what it cannot vouch for.
                if ( time.Kind != DateTimeKind.Utc )
                {
                    throw new InvalidOperationException( string.Format(
                        "a {0} row carries a time that is not UTC, so the platform would read it in its own zone and the value would be wrong by this church's offset",
                        section ) );
                }

                writer.WriteValue( time.ToString( "yyyy-MM-ddTHH:mm:ss.ffffff'Z'", CultureInfo.InvariantCulture ) );
                return;
            }

            var guids = value as IEnumerable<Guid>;

            if ( guids != null )
            {
                // The column behind this is a uuid array, and the drain reads anything that is not a
                // JSON array as an empty one.
                writer.WriteStartArray();

                foreach ( var guid in guids )
                {
                    writer.WriteValue( guid.ToString( "D" ) );
                }

                writer.WriteEndArray();
                return;
            }

            if ( value is string || value is bool || value is int || value is long || value is short || value is byte || value is decimal || value is double )
            {
                writer.WriteValue( value );
                return;
            }

            // Anything else would be serialized however Json.NET decides, in a shape nobody chose.
            throw new InvalidOperationException( string.Format(
                "a {0} row carries a {1}, which has no agreed form on the wire",
                section,
                value.GetType().Name ) );
        }

        /// <summary>
        /// Moves a stored time onto the clock the far side reads it with.
        /// </summary>
        /// <param name="stored">The time as Rock stores it, or null.</param>
        /// <param name="organizationTimeZone">The zone Rock's stored times are in.</param>
        /// <returns>The same instant in UTC, or null.</returns>
        internal static DateTime? ToUtc( object stored, TimeZoneInfo organizationTimeZone )
        {
            if ( stored == null || stored == DBNull.Value )
            {
                return null;
            }

            var time = ( DateTime ) stored;

            if ( time.Kind == DateTimeKind.Utc )
            {
                return time;
            }

            // A time out of the database carries no zone, and it is in the organization's. Treated as
            // UTC it would be wrong by this church's offset, and for a church behind UTC a ban would
            // lift early.
            return TimeZoneInfo.ConvertTimeToUtc( DateTime.SpecifyKind( time, DateTimeKind.Unspecified ), organizationTimeZone );
        }

        /// <summary>
        /// Splits the joined badge keys into the list the wire carries.
        /// </summary>
        /// <param name="joined">The keys as the query returned them.</param>
        /// <returns>The keys.</returns>
        internal static IList<Guid> ReadBadgeKeys( object joined )
        {
            var text = joined as string;

            if ( string.IsNullOrWhiteSpace( text ) )
            {
                // Empty rather than absent: the column on the far side cannot hold nothing, and a
                // person holding no badge is not the same as a row that did not say.
                return new List<Guid>();
            }

            var keys = new List<Guid>();

            foreach ( var part in text.Split( ',' ) )
            {
                var trimmed = part.Trim();

                if ( trimmed.Length == 0 )
                {
                    continue;
                }

                Guid key;

                // Dropped rather than refused, this would hand the church a badge that quietly
                // stops appearing on a submission the far side accepts, with nothing to look at.
                if ( !Guid.TryParse( trimmed, out key ) )
                {
                    throw new InvalidOperationException( string.Format( "the badge key {0} is not an identifier", trimmed ) );
                }

                keys.Add( key );
            }

            return keys;
        }

        /// <summary>
        /// Works out the colour pair a badge is drawn with.
        /// </summary>
        /// <param name="highlightColor">The colour the church configured, in whatever form.</param>
        /// <returns>The background and the foreground, both null when the colour cannot be read.</returns>
        internal static Tuple<string, string> ReadBadgeColors( object highlightColor )
        {
            var text = ( highlightColor as string ?? string.Empty ).Trim();

            // The field is free text in Rock, so a church can put a colour name, a function or
            // anything else in it. A badge with no colour still renders; a submission refused over
            // one badge takes that church down for the whole cycle.
            if ( text.Length == 0 || text[0] != '#' )
            {
                return Tuple.Create( ( string ) null, ( string ) null );
            }

            var digits = text.Substring( 1 );

            if ( digits.Length == 3 )
            {
                // The short form is not accepted on the far side, and doubling each digit is what
                // it means everywhere it is written.
                digits = new string( new[] { digits[0], digits[0], digits[1], digits[1], digits[2], digits[2] } );
            }

            if ( digits.Length != 6 || !digits.All( Uri.IsHexDigit ) )
            {
                return Tuple.Create( ( string ) null, ( string ) null );
            }

            var red = int.Parse( digits.Substring( 0, 2 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture );
            var green = int.Parse( digits.Substring( 2, 2 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture );
            var blue = int.Parse( digits.Substring( 4, 2 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture );

            var luminance = RelativeLuminance( red, green, blue );

            // Whichever of black and white the eye separates further from this background, by the
            // accessibility contrast ratio rather than by a brightness rule of thumb, so a colour
            // near the boundary gets the answer a checker would give.
            var contrastWithWhite = 1.05 / ( luminance + 0.05 );
            var contrastWithBlack = ( luminance + 0.05 ) / 0.05;

            var foreground = contrastWithWhite >= contrastWithBlack ? "#ffffff" : "#000000";

            return Tuple.Create( "#" + digits.ToLowerInvariant(), foreground );
        }

        /// <summary>
        /// How bright a colour is to the eye, on the scale the accessibility contrast ratio uses.
        /// </summary>
        /// <param name="red">The red channel, 0 to 255.</param>
        /// <param name="green">The green channel, 0 to 255.</param>
        /// <param name="blue">The blue channel, 0 to 255.</param>
        /// <returns>The relative luminance, 0 for black and 1 for white.</returns>
        private static double RelativeLuminance( int red, int green, int blue )
        {
            return ( 0.2126 * Straighten( red ) ) + ( 0.7152 * Straighten( green ) ) + ( 0.0722 * Straighten( blue ) );
        }

        /// <summary>
        /// Takes one channel out of the curve a display applies to it.
        /// </summary>
        /// <param name="channel">The channel, 0 to 255.</param>
        /// <returns>The straightened value, 0 to 1.</returns>
        private static double Straighten( int channel )
        {
            var value = channel / 255.0;

            return value <= 0.03928 ? value / 12.92 : Math.Pow( ( value + 0.055 ) / 1.055, 2.4 );
        }

        /// <summary>
        /// How a wire column's value is made from what Rock stores.
        /// </summary>
        private enum WireConversion
        {
            /// <summary>Sent as stored.</summary>
            None,

            /// <summary>The joined keys, split into a list.</summary>
            BadgeKeys,

            /// <summary>A time in the organization's zone, moved to UTC.</summary>
            Utc,

            /// <summary>The background of the pair made from the highlight colour.</summary>
            Background,

            /// <summary>The foreground of the pair made from the highlight colour.</summary>
            Foreground
        }

        /// <summary>
        /// Where one wire column's value is read from and how it is converted.
        /// </summary>
        private struct WireColumn
        {
            public readonly int Ordinal;

            public readonly WireConversion Conversion;

            public WireColumn( int ordinal, WireConversion conversion )
            {
                Ordinal = ordinal;
                Conversion = conversion;
            }
        }

        #endregion Writing the body

        #region What the run reports

        /// <summary>
        /// Works out what a sync run has to say for itself.
        /// </summary>
        /// <param name="acknowledgement">What came back from the submission.</param>
        /// <param name="polled">What a status read found, or null where none could be made.</param>
        /// <returns>The result. Never null, and always carrying a sentence.</returns>
        /// <remarks>
        ///     <para>
        ///         Three answers, in order of how much they are worth. What became of this submission,
        ///         where the queue reached it while the run was still waiting. What became of the
        ///         previous one, where it did not, which the acknowledgement carries for exactly this
        ///         reason. And, where there is neither, a plain statement that the restatement is stored
        ///         and queued.
        ///     </para>
        ///     <para>
        ///         That last answer is not a failure. The queue runs on its own schedule and very often
        ///         has not reached a submission by the time the run that made it finishes, so a run that
        ///         failed over it would be red on most cycles at a healthy church and would teach its
        ///         administrator to stop reading the job.
        ///     </para>
        ///     <para>
        ///         The fallback names the submission it is talking about. Reporting the previous cycle's
        ///         result as though it were this one's would read as a success on the run after a
        ///         failure, and as a failure on the run after a fix.
        ///     </para>
        /// </remarks>
        internal static RunResult Resolve( ChatSyncAcknowledgement acknowledgement, ChatSyncOutcome polled )
        {
            if ( acknowledgement == null )
            {
                return new RunResult
                {
                    IsFailure = true,
                    Message = "the submission was never made"
                };
            }

            if ( acknowledgement.IsTransportFailure )
            {
                return new RunResult
                {
                    IsFailure = true,
                    Message = "the chat platform could not be reached: "
                        + ( acknowledgement.TransportDetail.IsNullOrWhiteSpace()
                            ? "no reason was given"
                            : acknowledgement.TransportDetail )
                };
            }

            // The gateway answers a token it cannot verify in its own shape, with no status, and
            // that says something about this church's credential, not about this version of Rock.
            if ( !acknowledgement.Status.HasValue && ( acknowledgement.HttpStatusCode == 401 || acknowledgement.HttpStatusCode == 403 ) )
            {
                return new RunResult
                {
                    IsFailure = true,
                    Message = "the chat platform refused this church's credential" + Reason( acknowledgement.ErrorCode )
                };
            }

            if ( !acknowledgement.Status.HasValue )
            {
                return new RunResult
                {
                    IsFailure = true,
                    Message = "the chat platform answered with nothing this version of Rock can read"
                        + Reason( acknowledgement.ErrorCode )
                };
            }

            // A submission the platform turned away never reaches the queue, so there is nothing to
            // poll for and nothing a previous cycle could say that would matter more.
            if ( acknowledgement.Status.Value == ChatSyncSubmissionStatus.Refused )
            {
                return new RunResult
                {
                    IsFailure = true,
                    Message = "the chat platform refused this restatement" + Reason( acknowledgement.ErrorCode )
                };
            }

            if ( polled != null && polled.Status.HasValue && polled.Status.Value != ChatSyncSubmissionStatus.Accepted )
            {
                return new RunResult
                {
                    IsFailure = !ChatSyncSubmitClient.IsJobSuccess( polled.Status.Value ),
                    Message = "this restatement was " + ChatSyncSubmitClient.WireValueFor( polled.Status.Value )
                        + Reason( polled.ErrorCode )
                };
            }

            var previous = acknowledgement.PreviousOutcome;
            if ( previous != null && previous.Status.HasValue )
            {
                return new RunResult
                {
                    IsFailure = !ChatSyncSubmitClient.IsJobSuccess( previous.Status.Value ),
                    Message = "this restatement was submitted, not yet applied. The previous submission, "
                        + previous.SubmissionId + ", was "
                        + ChatSyncSubmitClient.WireValueFor( previous.Status.Value )
                        + Reason( previous.ErrorCode )
                };
            }

            return new RunResult
            {
                IsFailure = false,
                Message = "this restatement was submitted, not yet applied"
            };
        }

        /// <summary>
        /// The named reason, where there is one, as a clause rather than a bare code.
        /// </summary>
        /// <param name="errorCode">The code, or null.</param>
        /// <returns>The clause, or an empty string.</returns>
        private static string Reason( string errorCode )
        {
            return errorCode.IsNullOrWhiteSpace() ? string.Empty : ": " + errorCode;
        }

        /// <summary>
        /// The whole sentence a run that reached the platform leaves on the job: which submission it was,
        /// what was sent, then what became of it.
        /// </summary>
        /// <param name="submissionId">The id the restatement was submitted under.</param>
        /// <param name="rowCounts">The rows written, by section.</param>
        /// <param name="outcome">What became of the submission.</param>
        /// <returns>The result line.</returns>
        /// <remarks>
        /// The id leads because it is the one thing that finds the platform's own record of this run, for
        /// whoever reads the job page, for Sync Now on a chat block, which learns nothing about the run
        /// but what the job leaves here, and for a support request.
        /// </remarks>
        internal static string Describe( Guid submissionId, IDictionary<string, int> rowCounts, string outcome )
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "Submission {0}: {1} channels, {2} people, {3} memberships and {4} badges were sent. {5}",
                submissionId,
                Count( rowCounts, "channels" ),
                Count( rowCounts, "aliases" ),
                Count( rowCounts, "members" ),
                Count( rowCounts, "badges" ),
                outcome );
        }

        /// <summary>
        /// The run's own sentence and the remark about its schedule, in that order, skipping whichever is
        /// absent.
        /// </summary>
        /// <param name="outcome">What the run has to say for itself.</param>
        /// <param name="scheduleWarning">What is worth saying about the schedule, or null.</param>
        /// <returns>The result line.</returns>
        internal static string Join( string outcome, string scheduleWarning )
        {
            if ( scheduleWarning.IsNullOrWhiteSpace() )
            {
                return outcome;
            }

            return outcome.IsNullOrWhiteSpace() ? scheduleWarning : outcome + " " + scheduleWarning;
        }

        /// <summary>
        /// What one sync run has to say for itself.
        /// </summary>
        internal sealed class RunResult
        {
            /// <summary>
            /// Whether the run should be recorded as a failure.
            /// </summary>
            public bool IsFailure { get; set; }

            /// <summary>
            /// What the run puts on its own result line, in words an administrator can act on.
            /// </summary>
            public string Message { get; set; }
        }

        /// <summary>
        /// One reading of the church: the bytes, what was counted into them, the moment they describe and
        /// the identity seeds taken at that moment.
        /// </summary>
        internal sealed class ProjectionResult
        {
            /// <summary>
            /// The whole restatement, as the wire carries it: UTF-8 text in the one buffer it was
            /// written into. It is handed to the transport as it is rather than decoded and encoded
            /// again, because a second copy of a large church's body is tens of megabytes for nothing.
            /// </summary>
            public ArraySegment<byte> Payload { get; set; }

            /// <summary>
            /// How many rows each section actually carries, counted as they were written.
            /// </summary>
            public IDictionary<string, int> RowCounts { get; set; }

            /// <summary>
            /// The moment this reading describes, taken before it began.
            /// </summary>
            public DateTime ReadAtUtc { get; set; }

            /// <summary>
            /// The identity seeds of the tables it read.
            /// </summary>
            public ChatSyncIdentityMarks Marks { get; set; }
        }

        #endregion What the run reports

        #region Building the submission

        /// <summary>
        /// Builds the metadata a submission carries beside its body.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Two of these values are dictionaries whose key sets the platform checks exactly, and those
        /// key sets are read out of the wire contract rather than written here. They are the one part
        /// of a submission that cannot be derived from the rest of the contract, and a copy of them
        /// typed into this file would compile, pass every test that only reads it back, and be refused
        /// at the platform on every cycle with nothing in this repository saying why.
        /// </para>
        /// <para>
        /// The row-count keys are the short names of the payload's own sections, deliberately not the
        /// four table names, which is why they are taken from the contract's header entry and checked
        /// against the contract's section list rather than assumed to be either.
        /// </para>
        /// <para>
        /// The mark keys name tables in Rock, and that correspondence has to live somewhere in this
        /// assembly because the contract does not name Rock tables. What the contract decides is which
        /// keys must be present: a key it lists and this builder cannot supply, or a key this builder
        /// holds and the contract does not list, stops the submission here rather than being refused
        /// later for a reason nobody can see. Key order is not part of either set and is not relied on.
        /// </para>
        /// </remarks>
        internal sealed class ChatSyncHeaderBuilder
        {
            #region Fields

            /// <summary>
            /// The contract entry naming the expected row count of each payload section.
            /// </summary>
            private const string RowCountsHeader = "x-sync-counts";

            /// <summary>
            /// The contract entry naming the identity high-water value of each table read.
            /// </summary>
            private const string IdentityMarksHeader = "x-sync-marks";

            /// <summary>
            /// The contract entry naming the moment every row of the payload is judged by.
            /// </summary>
            private const string ReadTimeHeader = "x-sync-read-at";

            /// <summary>
            /// The contract entry naming the version of Rock that produced the payload.
            /// </summary>
            private const string RockVersionHeader = "x-sync-rock-version";

            /// <summary>
            /// The contract entry carrying the hash of the column order the payload is in.
            /// </summary>
            private const string ContractHeader = "x-sync-contract";

            /// <summary>
            /// The one optional entry, set for a run a person started and is waiting on.
            /// </summary>
            private const string UrgentHeader = "x-sync-urgent";

            /// <summary>
            /// The idempotency key header. Set by this client rather than taken from a caller's
            /// dictionary, so a retry cannot quietly carry a different one.
            /// </summary>
            private const string SubmissionIdHeader = "x-sync-submission-id";

            /// <summary>
            /// A hundred nanoseconds is the smallest interval a tick counts and a microsecond is the
            /// smallest the platform stores, so this is what has to be removed from a read time.
            /// </summary>
            private const long TicksPerMicrosecond = 10L;

            /// <summary>
            /// The parsed wire contract, which decides the order values are emitted in.
            /// </summary>
            private readonly JObject _contract;

            #endregion

            #region Constructors

            /// <summary>
            /// Builds headers from the contract that ships in this assembly.
            /// </summary>
            public ChatSyncHeaderBuilder()
                : this( JObject.Parse( ChatWireContract.Json ) )
            {
            }

            /// <summary>
            /// Builds headers from a supplied contract.
            /// </summary>
            /// <param name="contract">The parsed wire contract.</param>
            /// <remarks>
            /// Taking the contract rather than always reading the embedded one is what lets a test hand
            /// this a contract whose key sets differ and require the headers to differ with them, which
            /// is the only way to tell a builder that reads the contract from one that agrees with
            /// itself.
            /// </remarks>
            public ChatSyncHeaderBuilder( JObject contract )
            {
                if ( contract == null )
                {
                    throw new ArgumentNullException( "contract" );
                }

                _contract = contract;
            }

            #endregion

            #region Methods

            /// <summary>
            /// The payload's section names, in the order the contract lists them.
            /// </summary>
            /// <returns>The section names.</returns>
            public IList<string> GetPayloadSections()
            {
                var sections = _contract["payload"] == null ? null : _contract["payload"]["sections"];

                if ( sections == null )
                {
                    throw new InvalidOperationException( "the chat wire contract does not name the payload sections, so nothing here can key a submission" );
                }

                return sections.Select( s => s.Value<string>() ).ToList();
            }

            /// <summary>
            /// Reads one header entry's key set out of the contract.
            /// </summary>
            /// <param name="headerName">The header the contract lists.</param>
            /// <returns>The keys, in the order the contract happens to list them.</returns>
            /// <remarks>
            /// The order is incidental. Both sides of the platform's comparison sort, so it cannot
            /// break a submission, and nothing here should come to depend on it.
            /// </remarks>
            private IList<string> GetHeaderKeys( string headerName )
            {
                var headers = _contract["submit_headers"];

                if ( headers == null )
                {
                    throw new InvalidOperationException( "the chat wire contract describes no submit headers" );
                }

                var header = headers.Children<JObject>().FirstOrDefault( h => h["name"] != null && h["name"].Value<string>() == headerName );

                if ( header == null )
                {
                    throw new InvalidOperationException( string.Format( "the chat wire contract describes no {0} header", headerName ) );
                }

                if ( header["keys"] == null )
                {
                    throw new InvalidOperationException( string.Format( "the chat wire contract does not carry the key set of {0} as data, so this header could only be built from prose about it", headerName ) );
                }

                return header["keys"].Select( k => k.Value<string>() ).ToList();
            }

            /// <summary>
            /// Builds the expected row count header.
            /// </summary>
            /// <param name="rowCountsBySection">How many rows each payload section carries.</param>
            /// <returns>The header value.</returns>
            public string BuildRowCounts( IDictionary<string, int> rowCountsBySection )
            {
                if ( rowCountsBySection == null )
                {
                    throw new ArgumentNullException( "rowCountsBySection" );
                }

                var sections = GetPayloadSections();
                var keys = GetHeaderKeys( RowCountsHeader );

                // The platform builds these two lists from one constant, so a copy of the contract
                // where they differ is a defect in the copy. Preferring either one would send a header
                // built from a guess and leave the disagreement to be found as a refusal.
                var disagreements = keys.Except( sections ).Concat( sections.Except( keys ) ).ToList();

                if ( disagreements.Any() )
                {
                    throw new InvalidOperationException( string.Format(
                        "the chat wire contract's row-count keys and payload sections disagree about {0}",
                        string.Join( ", ", disagreements ) ) );
                }

                var header = new JObject();

                foreach ( var key in keys )
                {
                    int rowCount;

                    // A section with no count is a projection that did not run. Sending it as zero
                    // would be indistinguishable from a church that genuinely has none of that row,
                    // and the platform would apply the emptiness as truth.
                    if ( !rowCountsBySection.TryGetValue( key, out rowCount ) )
                    {
                        throw new InvalidOperationException( string.Format( "no row count was taken for the {0} section", key ) );
                    }

                    header[key] = rowCount;
                }

                return header.ToString( Formatting.None );
            }

            /// <summary>
            /// Builds the identity high-water header.
            /// </summary>
            /// <param name="marks">The values read from Rock.</param>
            /// <returns>The header value.</returns>
            public string BuildIdentityMarks( ChatSyncIdentityMarks marks )
            {
                if ( marks == null )
                {
                    throw new ArgumentNullException( "marks" );
                }

                var keys = GetHeaderKeys( IdentityMarksHeader );

                // Which table each key names is the one part of this that has to live here, because
                // the contract describes a wire and never names a table in Rock. What the contract
                // decides is which keys have to be present, and the two checks below are what turn a
                // contract this assembly has fallen behind into a build failure rather than a refusal
                // at the platform on every cycle under a code that points at no file.
                var valuesByKey = new Dictionary<string, long>
                {
                    { "person", marks.Person },
                    { "person_alias", marks.PersonAlias },
                    { "group", marks.Group },
                    { "group_member", marks.GroupMember }
                };

                var unsupplied = keys.Except( valuesByKey.Keys ).ToList();

                if ( unsupplied.Any() )
                {
                    throw new InvalidOperationException( string.Format(
                        "the chat wire contract asks for the identity mark {0}, which nothing here reads",
                        string.Join( ", ", unsupplied ) ) );
                }

                var unlisted = valuesByKey.Keys.Except( keys ).ToList();

                if ( unlisted.Any() )
                {
                    throw new InvalidOperationException( string.Format(
                        "the chat wire contract no longer lists the identity mark {0}, which this assembly still reads",
                        string.Join( ", ", unlisted ) ) );
                }

                var header = new JObject();

                foreach ( var key in keys )
                {
                    header[key] = valuesByKey[key];
                }

                return header.ToString( Formatting.None );
            }

            /// <summary>
            /// Builds every header one submission carries beside its body, save the submission id.
            /// </summary>
            /// <param name="readAtUtc">The UTC time taken before the projection read.</param>
            /// <param name="marks">The identity seeds read from Rock.</param>
            /// <param name="rowCountsBySection">How many rows each payload section carries, tallied as they were written.</param>
            /// <param name="rockVersion">The version of Rock producing the payload.</param>
            /// <param name="isUrgent">Whether a person started this run and is waiting on it.</param>
            /// <returns>The headers, keyed by name.</returns>
            /// <remarks>
            /// <para>
            /// The contract hash goes out as the hash of the column lists this contract actually
            /// carries, and the submission stops here when that differs from the hash the contract
            /// publishes for itself. An artifact edited after it was generated is one whose column
            /// order nobody agreed to, and the platform's compare would refuse it on every cycle under
            /// a code that points at no file.
            /// </para>
            /// <para>
            /// The set is then held against the contract's own header list: every header the contract
            /// requires is present, other than the submission id which the transport sets, and nothing
            /// is sent that the contract does not list. The urgent header is absent rather than false
            /// on a scheduled run, because its absence is what ordinary priority looks like.
            /// </para>
            /// </remarks>
            public IDictionary<string, string> BuildSubmissionHeaders( DateTime readAtUtc, ChatSyncIdentityMarks marks, IDictionary<string, int> rowCountsBySection, string rockVersion, bool isUrgent )
            {
                if ( rockVersion.IsNullOrWhiteSpace() )
                {
                    throw new ArgumentException( "the version of Rock producing a payload is recorded with the submission and cannot be blank", "rockVersion" );
                }

                var publishedHash = _contract["wire_hash"] == null ? null : _contract["wire_hash"].Value<string>();
                var computedHash = ChatWireContract.HashColumnLists( _contract );

                if ( publishedHash != computedHash )
                {
                    throw new InvalidOperationException(
                        "the wire contract this submission would be built from does not hash to the value written inside it, so nothing here can say what column order the payload is in" );
                }

                var headers = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase )
                {
                    { ReadTimeHeader, FormatReadTime( readAtUtc ) },
                    { RowCountsHeader, BuildRowCounts( rowCountsBySection ) },
                    { IdentityMarksHeader, BuildIdentityMarks( marks ) },
                    { RockVersionHeader, rockVersion },
                    { ContractHeader, computedHash }
                };

                if ( isUrgent )
                {
                    headers.Add( UrgentHeader, "1" );
                }

                RequireTheContractsHeaderSet( headers );

                return headers;
            }

            /// <summary>
            /// Holds a built header set against the contract's own list of submit headers.
            /// </summary>
            /// <param name="headers">The headers as built.</param>
            private void RequireTheContractsHeaderSet( IDictionary<string, string> headers )
            {
                var listed = _contract["submit_headers"];

                if ( listed == null )
                {
                    throw new InvalidOperationException( "the chat wire contract describes no submit headers" );
                }

                var entries = listed.Children<JObject>().ToList();

                var required = entries
                    .Where( h => h["required"] != null && h["required"].Value<bool>() )
                    .Select( h => h["name"].Value<string>() )
                    .Where( name => !string.Equals( name, SubmissionIdHeader, StringComparison.OrdinalIgnoreCase ) )
                    .ToList();

                var missing = required.Where( name => !headers.ContainsKey( name ) ).ToList();

                if ( missing.Any() )
                {
                    throw new InvalidOperationException( string.Format(
                        "the chat wire contract requires the {0} header, which nothing here builds",
                        string.Join( ", ", missing ) ) );
                }

                var names = entries.Select( h => h["name"].Value<string>() ).ToList();
                var unlisted = headers.Keys.Where( name => !names.Contains( name, StringComparer.OrdinalIgnoreCase ) ).ToList();

                if ( unlisted.Any() )
                {
                    throw new InvalidOperationException( string.Format(
                        "the {0} header is not one the chat wire contract lists",
                        string.Join( ", ", unlisted ) ) );
                }
            }

            /// <summary>
            /// Formats the read time the whole payload is judged by.
            /// </summary>
            /// <param name="readAtUtc">The UTC time taken before the projection read.</param>
            /// <returns>The header value.</returns>
            public static string FormatReadTime( DateTime readAtUtc )
            {
                if ( readAtUtc.Kind != DateTimeKind.Utc )
                {
                    throw new ArgumentException( "the read time a payload is judged by has to be taken in UTC, because it is compared against times the platform holds in UTC", "readAtUtc" );
                }

                // Truncated rather than rounded, and towards the past. The guard this value feeds
                // refuses a row whose stored time is not strictly older, so a value rounded up by the
                // fraction of a microsecond the platform cannot hold would let a stale write win a
                // comparison built to fail closed.
                var truncated = new DateTime( readAtUtc.Ticks - ( readAtUtc.Ticks % TicksPerMicrosecond ), DateTimeKind.Utc );

                // The offset is explicit because a time without one is read in the receiving session's
                // own zone rather than in the zone it was taken in.
                return truncated.ToString( "yyyy-MM-ddTHH:mm:ss.ffffff'Z'", CultureInfo.InvariantCulture );
            }

            #endregion
        }

        /// <summary>
        /// The identity high-water value of each table the projection reads, sent with every
        /// submission so the platform can tell a restored database from a live one.
        /// </summary>
        /// <remarks>
        /// <para>
        /// These are identity seeds, not maximum ids. The maximum drops when the newest rows are
        /// deleted and the seed does not, so a church that deletes the person it just created would
        /// otherwise look like a database restored from an older backup and be refused on every cycle
        /// until someone intervened. A restore does carry the seed backwards with the table metadata,
        /// which is the case this exists to catch.
        /// </para>
        /// <para>
        /// It does not catch a live clone. A staging copy starts with values equal to production's, and
        /// equal is not backwards; worse, staging advances its own as people test on it, so production
        /// is the one that ends up refused. Staging installations are kept off chat or pointed at a
        /// staging project instead.
        /// </para>
        /// </remarks>
        internal sealed class ChatSyncIdentityMarks
        {
            #region Properties

            /// <summary>
            /// The identity high-water value of the person table.
            /// </summary>
            public long Person { get; set; }

            /// <summary>
            /// The identity high-water value of the person alias table.
            /// </summary>
            public long PersonAlias { get; set; }

            /// <summary>
            /// The identity high-water value of the group table.
            /// </summary>
            public long Group { get; set; }

            /// <summary>
            /// The identity high-water value of the group member table.
            /// </summary>
            public long GroupMember { get; set; }

            #endregion
        }

        #endregion Building the submission
    }
}
