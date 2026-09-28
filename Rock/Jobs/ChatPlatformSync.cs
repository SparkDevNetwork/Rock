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
using System.Collections.Concurrent;
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
using Rock.Communication.Chat.Platform.Sync;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    [DisplayName( "Chat Platform Sync" )]
    [Description( "Sends this church's people, channels, memberships and badges to the chat platform, as a whole picture each time." )]
    /// <summary>
    /// Sends this church's people, channels, memberships and badges to the chat platform.
    /// </summary>
    /// <remarks>
    /// Every run restates the whole picture, so a skipped or repeated run is harmless. Not to be
    /// confused with the Chat Sync job, which belongs to the other chat provider.
    /// </remarks>
    public class ChatPlatformSync : RockJob
    {
        #region Constants

        // The page that starts a run by hand names its scheduler this way. A manual run ignores the
        // platform's backoff and is marked urgent, because a person is waiting on it.
        private const string ManualRunSchedulerPrefix = "RunNow:";

        // The longest gap between runs worth warning about. A design bound, not a measurement.
        private static readonly TimeSpan MaximumScheduleGap = TimeSpan.FromHours( 24 );

        // Eight days, so a weekly pattern is seen whichever day the reading starts on.
        private static readonly TimeSpan ScheduleSampleWindow = TimeSpan.FromDays( 8 );

        // Above eight days of a fire every second (691200), so even that schedule is judged whole.
        private const int ScheduleSampleCap = 700000;

        // Sync Now reads a run whose result begins this way as the schedule's, which sent nothing.
        internal const string BackoffSkipPrefix = "Nothing was submitted. The chat platform asked for a backoff until ";

        // Generous, because a timeout costs no more than one cycle. An estimate.
        internal const int ProjectionTimeoutSeconds = 300;

        // The longest gap each schedule leaves, by cron expression.
        private static readonly ConcurrentDictionary<string, TimeSpan?> _longestGaps = new ConcurrentDictionary<string, TimeSpan?>( StringComparer.Ordinal );

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

            // The instant, not the wall clock: the backoff carries the platform's offset, and this
            // server's offset may not be the organization's.
            var now = DateTimeOffset.UtcNow;

            // Taken whether or not the run goes ahead, so a skipped run still reports it.
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
                // Thrown because the scheduler records a failure only from an exception.
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
        internal static string SkipReason( bool isManualRun, DateTimeOffset? backoffUntil, DateTimeOffset now )
        {
            if ( isManualRun || !backoffUntil.HasValue || backoffUntil.Value <= now )
            {
                return null;
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                BackoffSkipPrefix + "{0}, and this run was started by the schedule rather than by a person. Sync Now ignores the backoff.",
                backoffUntil.Value.ToUniversalTime().ToString( "yyyy-MM-dd HH:mm:ss'Z'", CultureInfo.InvariantCulture ) );
        }

        /// <summary>
        /// What is worth saying about this schedule, or null where there is nothing.
        /// </summary>
        /// <param name="cronExpression">The schedule.</param>
        /// <param name="after">The moment to look forward from.</param>
        /// <returns>The warning, or null.</returns>
        internal static string ScheduleWarning( string cronExpression, DateTimeOffset after )
        {
            // Walked once per expression, since a fire every second is hundreds of thousands of steps.
            var longest = cronExpression.IsNullOrWhiteSpace()
                ? null
                : _longestGaps.GetOrAdd( cronExpression, expression => LongestGap( expression, after ) );
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
        private static TimeSpan? LongestGap( string cronExpression, DateTimeOffset after )
        {
            Quartz.CronExpression expression;
            try
            {
                expression = new Quartz.CronExpression( cronExpression );
            }
            catch ( Exception )
            {
                // A schedule this cannot read is the scheduler's to report.
                return null;
            }

            // Elapsed time, not wall clock, so the morning the clocks go back is not a 25 hour gap.
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
        /// A gap in days or hours, for the job page.
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
        /// One whole restatement over a given transport, so a test can stand in for the platform
        /// and see every call the run makes.
        /// </summary>
        /// <param name="rockContext">The context the projection reads through.</param>
        /// <param name="configuration">The church's chat settings.</param>
        /// <param name="isManualRun">Whether a person started this run rather than the schedule.</param>
        /// <param name="handler">The transport to send through, or null for the network.</param>
        /// <returns>What the run has to say for itself.</returns>
        internal static RunResult Run( RockContext rockContext, ChatPlatformConfiguration configuration, bool isManualRun, System.Net.Http.HttpMessageHandler handler )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// One whole restatement: read the church, send it, and learn what became of it.
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

            // Checked without minting, so an unusable key fails before the projection runs.
            if ( !configuration.IsConfigured || !ChatSigningKey.IsUsable( configuration.PrivateKey ) )
            {
                return new RunResult
                {
                    IsFailure = true,
                    Message = "Nothing was submitted. This church could not sign a request to the chat platform."
                };
            }

            var submissionId = Guid.NewGuid();
            var projection = Project( rockContext, configuration );
            var rowCounts = projection.RowCounts;

            // Built after the payload, so the counts are the rows actually written.
            var headers = ChatPlatformSyncHelper.BuildSubmissionHeaders(
                JObject.Parse( ChatWireContract.Json ),
                projection.ReadAtUtc,
                projection.Marks,
                rowCounts,
                Rock.VersionInfo.VersionInfo.GetRockSemanticVersionNumber(),
                isManualRun );

            using ( var client = new ChatPlatformSyncHelper.PlatformClient( configuration ) )
            {
                string failure;
                if ( !client.SignIn( out failure ) )
                {
                    return new RunResult
                    {
                        IsFailure = true,
                        Message = Join( Describe( submissionId, rowCounts, "nothing was submitted, because " + failure ), projection.BadgeWarning )
                    };
                }

                var acknowledgement = client.Submit( submissionId, projection.Payload, headers );

                if ( acknowledgement.CarriesBackoffAdvice )
                {
                    // Saved for a refusal as for an acceptance.
                    ChatPlatformConfigurationService.SaveSyncBackoff( acknowledgement.SyncBackoffUntil );
                }

                ChatPlatformSyncHelper.Outcome polled = null;
                if ( acknowledgement.Status == ChatPlatformSyncHelper.SubmissionStatus.Accepted )
                {
                    polled = client.Poll( submissionId,
                        isManualRun ? ChatPlatformSyncHelper.PollBudget.Manual : ChatPlatformSyncHelper.PollBudget.Scheduled );
                }

                var resolved = ChatPlatformSyncHelper.Resolve( acknowledgement, polled );

                return new RunResult
                {
                    IsFailure = resolved.IsFailure,
                    Message = Join( Describe( submissionId, rowCounts, resolved.Message ), projection.BadgeWarning )
                };
            }
        }

        /// <summary>
        /// Marks the chat channels and reads the church once, without sending anything.
        /// </summary>
        /// <param name="rockContext">The context the projection reads through.</param>
        /// <param name="configuration">The church's chat settings.</param>
        /// <returns>The reading.</returns>
        /// <remarks>
        /// The procedure's first statement writes the channel mark, which must commit even when the
        /// submission fails, so no transaction may span this call.
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

            // The context owns the connection: close it only if it was opened here, never dispose it.
            var connection = rockContext.Database.Connection;
            var wasClosed = connection.State != ConnectionState.Open;

            try
            {
                if ( wasClosed )
                {
                    connection.Open();
                }

                var result = new ProjectionResult();

                // Streamed, so the largest church is never held in memory as filled tables.
                using ( var command = CreateCommand( connection, configuration ) )
                using ( var reader = command.ExecuteReader() )
                {
                    ReadMarks( reader, result );

                    IDictionary<string, int> rowCounts;
                    result.Payload = BuildPayload( reader, out rowCounts );
                    result.RowCounts = rowCounts;
                    result.BadgeWarning = ReadBadgeWarning( reader );
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
        /// Reads the moment and the identity seeds from the procedure's first result set.
        /// </summary>
        /// <param name="reader">The reader, on the procedure's first result set.</param>
        /// <param name="result">The reading to record them on.</param>
        private static void ReadMarks( DbDataReader reader, ProjectionResult result )
        {
            if ( !reader.Read() )
            {
                throw new InvalidOperationException( "the identity marks of the tables this projection reads could not be taken" );
            }

            result.ReadAtUtc = DateTime.SpecifyKind( reader.GetDateTime( 0 ), DateTimeKind.Utc );

            var marks = new Dictionary<string, long>( StringComparer.Ordinal );

            for ( var i = 1; i < reader.FieldCount; i++ )
            {
                marks[reader.GetName( i )] = reader.GetInt64( i );
            }

            result.Marks = marks;
        }

        /// <summary>
        /// Names each configured badge the procedure left out, from its last result set.
        /// </summary>
        /// <param name="reader">The reader, on the badges section's result set.</param>
        /// <returns>The sentences, or null where no badge was left out.</returns>
        private static string ReadBadgeWarning( DbDataReader reader )
        {
            var sentences = new List<string>();

            if ( reader.NextResult() )
            {
                while ( reader.Read() )
                {
                    var reason = reader.GetString( 2 );
                    var badge = reason == "missing" ? reader.GetGuid( 0 ).ToString() : reader.GetString( 1 );
                    var why = reason == "missing" ? "no longer exists" : reason == "not_people" ? "does not list people" : "is not persisted";

                    sentences.Add( string.Format( "Badge '{0}' was skipped: its Data View {1}.", badge, why ) );
                }
            }

            return sentences.Count == 0 ? null : string.Join( " ", sentences );
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

            // One buffer, encoded as written: a second copy of a large body is tens of megabytes.
            var body = new MemoryStream();

            // No byte order mark: the platform parses the body as UTF-8 text. This overload is used
            // because it leaves the stream open for the buffer below; 64 KB is a choice.
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
                // The slash between root and path is ensured here, whatever the administrator typed.
                { "@PublicApplicationRoot", ( GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) ?? string.Empty ).EnsureTrailingForwardslash() }
            };
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
            var sections = ChatPlatformSyncHelper.GetPayloadSections( contract );
            var tables = contract["tables"] as JArray;

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
                        ChatPlatformSyncHelper.WriteValue( writer, ChatPlatformSyncHelper.ReadBadgeKeys( stored ), section );
                        break;
                    case WireConversion.Utc:
                        ChatPlatformSyncHelper.WriteValue( writer, ChatPlatformSyncHelper.ToUtc( stored, organizationTimeZone ), section );
                        break;
                    case WireConversion.Background:
                        colors = colors ?? ChatPlatformSyncHelper.ReadBadgeColors( stored );
                        ChatPlatformSyncHelper.WriteValue( writer, colors.Item1, section );
                        break;
                    case WireConversion.Foreground:
                        colors = colors ?? ChatPlatformSyncHelper.ReadBadgeColors( stored );
                        ChatPlatformSyncHelper.WriteValue( writer, colors.Item2, section );
                        break;
                    default:
                        ChatPlatformSyncHelper.WriteValue( writer, stored, section );
                        break;
                }
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// How a wire column's value is made from what Rock stores.
        /// </summary>
        private enum WireConversion
        {
            /// <summary>
            /// Sent as stored.
            /// </summary>
            None,

            /// <summary>
            /// The joined keys, split into a list.
            /// </summary>
            BadgeKeys,

            /// <summary>
            /// A time in the organization's zone, moved to UTC.
            /// </summary>
            Utc,

            /// <summary>
            /// The background of the pair made from the highlight colour.
            /// </summary>
            Background,

            /// <summary>
            /// The foreground of the pair made from the highlight colour.
            /// </summary>
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
        /// The result line of a run that reached the platform. The submission id leads, because it
        /// finds the platform's record of the run, for a reader of the job page and for support.
        /// </summary>
        /// <param name="submissionId">The id the restatement was submitted under.</param>
        /// <param name="rowCounts">The rows written, by section.</param>
        /// <param name="outcome">What became of the submission.</param>
        /// <returns>The result line.</returns>
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
        /// Joins the run's sentence and a remark, such as the schedule's, skipping whichever is absent.
        /// </summary>
        /// <param name="outcome">What the run has to say for itself.</param>
        /// <param name="remark">The remark, or null.</param>
        /// <returns>The result line.</returns>
        internal static string Join( string outcome, string remark )
        {
            if ( remark.IsNullOrWhiteSpace() )
            {
                return outcome;
            }

            return outcome.IsNullOrWhiteSpace() ? remark : outcome + " " + remark;
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
        /// One reading of the church and what its headers are built from.
        /// </summary>
        internal sealed class ProjectionResult
        {
            /// <summary>
            /// The whole restatement as UTF-8, in the one buffer it was written into.
            /// </summary>
            public ArraySegment<byte> Payload { get; set; }

            /// <summary>
            /// How many rows each section actually carries, counted as they were written.
            /// </summary>
            public IDictionary<string, int> RowCounts { get; set; }

            /// <summary>
            /// The moment this reading describes, taken by the database.
            /// </summary>
            public DateTime ReadAtUtc { get; set; }

            /// <summary>
            /// The identity seeds of the tables it read, by the contract's mark key.
            /// </summary>
            public IDictionary<string, long> Marks { get; set; }

            /// <summary>
            /// A sentence naming each configured badge the reading left out and why, or null where
            /// it left none out.
            /// </summary>
            public string BadgeWarning { get; set; }
        }

        #endregion What the run reports
    }
}
