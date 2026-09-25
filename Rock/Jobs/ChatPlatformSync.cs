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

            // Only the key is checked here, without minting: a key nothing can sign with would
            // otherwise cost a whole projection on the church's server before it was found. The
            // token itself is minted once, after the read.
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

            // Built after the payload, because the counts have to be the rows actually written: a
            // count of what was expected would agree with a truncated payload.
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
                        Message = Describe( submissionId, rowCounts, "nothing was submitted, because " + failure )
                    };
                }

                var acknowledgement = client.Submit( submissionId, projection.Payload, headers );

                if ( acknowledgement.CarriesBackoffAdvice )
                {
                    // Written for a refusal as for an acceptance: advice about the platform's load
                    // is no less true because this submission was turned away. Not written for a
                    // run that never got the platform's own answer, whose silence would otherwise
                    // clear advice the platform had given.
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
                    Message = Describe( submissionId, rowCounts, resolved.Message )
                };
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
        /// is compared against the platform's own clock. The marks are identity seeds, not maximum ids,
        /// because a maximum drops when the newest rows are deleted and would read as a restored
        /// database. The procedure names each by the contract's own mark key, so they are kept by name.
        /// </remarks>
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
            /// The identity seeds of the tables it read, by the contract's mark key.
            /// </summary>
            public IDictionary<string, long> Marks { get; set; }
        }

        #endregion What the run reports
    }
}
