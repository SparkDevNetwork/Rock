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

        // How many fire times to look at. Enough to walk a weekly pattern round to its own repeat,
        // which is the longest shape that hides its gap; anything slower than weekly shows its gap
        // on the first step.
        private const int ScheduleSampleSize = 14;

        // How long the projection may take. Generous, because it reads the whole of a large
        // church's group membership and runs on that church's own server, and because the cost of
        // being wrong here is a cycle lost rather than a cycle wrong. An estimate.
        private const int ProjectionTimeoutSeconds = 300;

        #endregion Constants

        #region Execute

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

        private bool IsManualRun()
        {
            var schedulerName = Scheduler?.SchedulerName;

            return schedulerName != null
                && schedulerName.StartsWith( ManualRunSchedulerPrefix, StringComparison.OrdinalIgnoreCase );
        }

        #endregion Execute

        #region Whether the run happens

        // Why this run did nothing, or null where it goes ahead. The backoff is the platform's
        // advice about its own load, and it binds the schedule but not a person: someone who
        // pressed Sync Now is at a screen waiting for an answer, and the cost of letting them
        // through is one submission the platform would rather have had later.
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

        // Refused at compile time. A DateTime handed to the overload above would be converted to an
        // instant with this server's offset. Rock's own clock returns the organisation's wall
        // clock, which on a hosted server is not in this server's zone, so the backoff would be
        // compared against a moment wrong by the difference: honoured hours past its expiry, or
        // released hours early.
        [Obsolete( "Pass an instant, such as DateTimeOffset.UtcNow. A DateTime is converted with this server's offset, which is not the organisation's, and the backoff is then compared against the wrong moment.", true )]
        internal static string SkipReason( bool isManualRun, DateTimeOffset? backoffUntil, DateTime now )
        {
            throw new NotSupportedException( "a backoff cannot be judged against a wall-clock reading" );
        }

        // What is worth saying about this schedule, or null where there is nothing. The schedule is
        // the church's own setting and is remarked on rather than corrected: nothing here writes to
        // the job.
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

        // The longest gap between consecutive runs, not the next one, because the schedules that go
        // wrong quietly are the ones that look frequent. A weekday morning schedule fires five
        // times a week and leaves seventy two hours over every weekend, and the gap after any given
        // Monday run is a reassuring twenty four hours.
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
            TimeSpan? longest = null;

            for ( var step = 0; step < ScheduleSampleSize; step++ )
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

        // A gap in the roundest words it fits, because a church reads this on a job page.
        private static string DescribeGap( TimeSpan gap )
        {
            if ( gap.TotalDays >= 2 )
            {
                return string.Format( CultureInfo.InvariantCulture, "{0:0.#} days", gap.TotalDays );
            }

            return string.Format( CultureInfo.InvariantCulture, "{0:0.#} hours", gap.TotalHours );
        }

        #endregion Whether the run happens

        #region The projection queries

        internal static string GetStagingSql()
        {
            return ReadSql( "ChatSyncStage.sql" );
        }

        internal static string GetStampSql()
        {
            return ReadSql( "ChatSyncStampChannels.sql" );
        }

        internal static string GetSectionSql( string section )
        {
            switch ( section )
            {
                case "aliases":
                    return ReadSql( "ChatSyncAliases.sql" );
                case "channels":
                    return ReadSql( "ChatSyncChannels.sql" );
                case "members":
                    return ReadSql( "ChatSyncMembers.sql" );
                case "badges":
                    return ReadSql( "ChatSyncBadges.sql" );
                default:
                    throw new InvalidOperationException( string.Format( "no chat projection query ships for the {0} section", section ?? "(none)" ) );
            }
        }

        private static string ReadSql( string fileName )
        {
            var assembly = typeof( ChatPlatformSync ).Assembly;
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault( n => n.EndsWith( "." + fileName, StringComparison.OrdinalIgnoreCase ) );

            if ( resourceName == null )
            {
                throw new InvalidOperationException( string.Format( "the chat projection query {0} is not embedded in this assembly", fileName ) );
            }

            using ( var stream = assembly.GetManifestResourceStream( resourceName ) )
            using ( var reader = new StreamReader( stream, Encoding.UTF8 ) )
            {
                return reader.ReadToEnd();
            }
        }

        #endregion The projection queries

        #region The run

        // One whole restatement: mark the channels, read the church, send it, and find out what
        // happened to it.
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

            // The marking runs first and on its own. A mark rolled back alongside a failed
            // submission would leave the next run treating a group as though it had never been a
            // channel, and that is the one thing the mark exists to prevent.
            StampChannels( rockContext );

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

            using ( var client = new ChatSyncSubmitClient( configuration, () => MintToken( configuration ) ) )
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

                result.Message = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} channels, {1} people, {2} memberships and {3} badges were sent. {4}",
                    Count( rowCounts, "channels" ),
                    Count( rowCounts, "aliases" ),
                    Count( rowCounts, "members" ),
                    Count( rowCounts, "badges" ),
                    result.Message );

                return result;
            }
        }

        // Reads the church once, without sending anything. The clock, the identity seeds and every
        // section are taken on one open connection, and the staging query and the sections go as one
        // batch. The staging query leaves its sets in temporary tables that live as long as that
        // batch, which is what stops a membership arriving in the same payload as neither the
        // channel nor the person it names.
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

                var result = new ProjectionResult
                {
                    ReadAtUtc = ReadClock( connection, configuration ),
                    Marks = ReadIdentityMarks( connection, configuration )
                };

                IDictionary<string, int> rowCounts;
                result.Payload = BuildPayload( connection, configuration, out rowCounts );
                result.RowCounts = rowCounts;

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

        // Marks the groups that are chat channels right now, in its own transaction and before the
        // clock is read, so the projection sees one settled set of marks.
        internal static void StampChannels( RockContext rockContext )
        {
            rockContext.Database.CommandTimeout = ProjectionTimeoutSeconds;
            rockContext.Database.ExecuteSqlCommand(
                GetStampSql(),
                new System.Data.SqlClient.SqlParameter( "@StampedAt", RockDateTime.Now ) );
        }

        // The moment this restatement describes, taken from the database rather than from this
        // process, and in UTC because it is compared against the platform's own clock.
        private static DateTime ReadClock( DbConnection connection, ChatPlatformConfiguration configuration )
        {
            using ( var command = CreateCommand( connection, "SELECT SYSUTCDATETIME();", configuration ) )
            {
                return DateTime.SpecifyKind( ( DateTime ) command.ExecuteScalar(), DateTimeKind.Utc );
            }
        }

        // The identity seed of each table the projection reads, and not the largest id in the
        // table. Deleting the newest rows lowers the largest id and leaves the seed where it was,
        // and a database restored from a backup is the case these exist to catch: its seeds go
        // backwards and the platform refuses the submission rather than quietly writing a church's
        // older picture over its newer one.
        private static ChatSyncIdentityMarks ReadIdentityMarks( DbConnection connection, ChatPlatformConfiguration configuration )
        {
            const string sql =
                "SELECT CAST( IDENT_CURRENT( 'Person' ) AS BIGINT ), "
                + "CAST( IDENT_CURRENT( 'PersonAlias' ) AS BIGINT ), "
                + "CAST( IDENT_CURRENT( '[Group]' ) AS BIGINT ), "
                + "CAST( IDENT_CURRENT( 'GroupMember' ) AS BIGINT );";

            using ( var command = CreateCommand( connection, sql, configuration ) )
            using ( var reader = command.ExecuteReader() )
            {
                if ( !reader.Read() )
                {
                    throw new InvalidOperationException( "the identity marks of the tables this projection reads could not be taken" );
                }

                return new ChatSyncIdentityMarks
                {
                    Person = reader.GetInt64( 0 ),
                    PersonAlias = reader.GetInt64( 1 ),
                    Group = reader.GetInt64( 2 ),
                    GroupMember = reader.GetInt64( 3 )
                };
            }
        }

        // The staging and the four section queries go as one command, and the sections come back as
        // its four result sets. That is not a round trip saved: a command carrying parameters is
        // sent as a nested batch, and a temporary table made inside one of those is dropped the
        // moment it ends. Split across commands, every section would ask for sets that no longer
        // existed.
        private static ArraySegment<byte> BuildPayload( DbConnection connection, ChatPlatformConfiguration configuration, out IDictionary<string, int> rowCounts )
        {
            var contract = JObject.Parse( ChatWireContract.Json );
            var sections = new ChatSyncHeaderBuilder( contract ).GetPayloadSections();
            var mapper = new ChatSyncRowMapper( contract, RockDateTime.OrgTimeZoneInfo );

            var sql = new StringBuilder();
            sql.AppendLine( GetStagingSql() );

            foreach ( var section in sections )
            {
                sql.AppendLine( GetSectionSql( section ) );
            }

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
            using ( var payloadWriter = new ChatSyncPayloadWriter( contract, jsonWriter ) )
            {
                using ( var command = CreateCommand( connection, sql.ToString(), configuration ) )
                using ( var reader = command.ExecuteReader() )
                {
                    foreach ( var section in sections )
                    {
                        WriteSection( reader, payloadWriter, mapper, section );

                        reader.NextResult();
                    }
                }

                payloadWriter.Complete();
                rowCounts = payloadWriter.RowCounts;
            }

            ArraySegment<byte> buffer;

            if ( !body.TryGetBuffer( out buffer ) )
            {
                throw new InvalidOperationException( "the submission body was written into a buffer that cannot be handed on to the transport" );
            }

            return buffer;
        }

        private static void WriteSection( DbDataReader reader, ChatSyncPayloadWriter payloadWriter, ChatSyncRowMapper mapper, string section )
        {
            payloadWriter.BeginSection( section );

            var columns = Enumerable.Range( 0, reader.FieldCount ).Select( reader.GetName ).ToList();

            while ( reader.Read() )
            {
                var values = new object[reader.FieldCount];
                reader.GetValues( values );

                payloadWriter.WriteRow( mapper.Map( section, columns, values ) );
            }

            payloadWriter.EndSection();
        }

        // One command, with only the parameters the text it runs actually names.
        private static DbCommand CreateCommand( DbConnection connection, string sql, ChatPlatformConfiguration configuration )
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = ProjectionTimeoutSeconds;

            foreach ( var parameter in ProjectionParameters( configuration ) )
            {
                if ( sql.IndexOf( parameter.Key, StringComparison.OrdinalIgnoreCase ) < 0 )
                {
                    continue;
                }

                var bound = command.CreateParameter();
                bound.ParameterName = parameter.Key;
                bound.Value = parameter.Value ?? DBNull.Value;
                command.Parameters.Add( bound );
            }

            return command;
        }

        // Everything the projection texts ask to be told rather than look up for themselves.
        private static IDictionary<string, object> ProjectionParameters( ChatPlatformConfiguration configuration )
        {
            var badgeGuids = configuration.ChatBadgeDataViewGuids ?? new List<Guid>();
            var activeStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );

            return new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase )
            {
                { "@ChatPeopleGroupGuid", Rock.SystemGuid.Group.GROUP_CHAT_PEOPLE.AsGuid() },
                { "@ChatBanListGroupGuid", Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST.AsGuid() },
                { "@ChatAdministratorsGroupGuid", Rock.SystemGuid.Group.GROUP_CHAT_ADMINISTRATORS.AsGuid() },
                { "@ChatSystemAuthorGuid", Rock.SystemGuid.Person.CHAT_SYSTEM_AUTHOR.AsGuid() },
                { "@DirectMessageGroupTypeGuid", Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE.AsGuid() },
                { "@BadgeDataViewGuidsJson", new JArray( badgeGuids.Select( g => g.ToString() ) ).ToString( Formatting.None ) },
                { "@ActiveRecordStatusValueId", activeStatus == null ? ( object ) DBNull.Value : activeStatus.Id },
                { "@ProfilesVisibleByDefault", configuration.AreChatProfilesVisible },
                { "@OpenDirectMessagesByDefault", configuration.IsOpenDirectMessagingAllowed },
                // The icon address is built from this, so the slash between root and path is
                // supplied here rather than trusted to however the administrator typed the root.
                { "@PublicApplicationRoot", ( GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) ?? string.Empty ).EnsureTrailingForwardslash() }
            };
        }

        // A fresh church token per request. A church token lasts minutes and a run that read a
        // large church and then waited out its poll budget can outlast one, so minting it once at
        // the top would expire mid-run on exactly the churches this matters most for.
        private static string MintToken( ChatPlatformConfiguration configuration )
        {
            var minted = ChatSessionHelper.TryMintSyncToken( new ChatSessionContext { Configuration = configuration } );

            return minted.Success ? minted.ChurchToken : null;
        }

        private static int Count( IDictionary<string, int> rowCounts, string section )
        {
            int count;
            return rowCounts != null && rowCounts.TryGetValue( section, out count ) ? count : 0;
        }

        #endregion The run

        #region What the run reports

        // Three answers, in order of how much they are worth. What became of this submission, where
        // the queue reached it while the run was still waiting. What became of the previous one,
        // where it did not, which the acknowledgement carries for exactly this reason. And, where
        // there is neither, a plain statement that the restatement is stored and queued.
        //
        // That last answer is not a failure. The queue runs on its own schedule and very often has
        // not reached a submission by the time the run that made it finishes, so a run that failed
        // over it would be red on most cycles at a healthy church and would teach its administrator
        // to stop reading the job. The fallback names the submission it is talking about, because
        // reporting the previous cycle's result as though it were this one's would read as a
        // success on the run after a failure, and as a failure on the run after a fix.
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

        // The named reason, where there is one, as a clause rather than a bare code.
        private static string Reason( string errorCode )
        {
            return errorCode.IsNullOrWhiteSpace() ? string.Empty : ": " + errorCode;
        }

        // The run's own sentence and the remark about its schedule, in that order, skipping
        // whichever is absent.
        internal static string Join( string outcome, string scheduleWarning )
        {
            if ( scheduleWarning.IsNullOrWhiteSpace() )
            {
                return outcome;
            }

            return outcome.IsNullOrWhiteSpace() ? scheduleWarning : outcome + " " + scheduleWarning;
        }

        internal sealed class RunResult
        {
            public bool IsFailure { get; set; }

            public string Message { get; set; }
        }

        // One reading of the church: the bytes, what was counted into them, the moment they
        // describe and the identity seeds taken at that moment.
        internal sealed class ProjectionResult
        {
            public ArraySegment<byte> Payload { get; set; }

            public IDictionary<string, int> RowCounts { get; set; }

            public DateTime ReadAtUtc { get; set; }

            public ChatSyncIdentityMarks Marks { get; set; }
        }

        #endregion What the run reports

        #region Building the submission

        internal sealed class ChatSyncHeaderBuilder
        {
            #region Fields

            private const string RowCountsHeader = "x-sync-counts";

            private const string IdentityMarksHeader = "x-sync-marks";

            private const string ReadTimeHeader = "x-sync-read-at";

            private const string RockVersionHeader = "x-sync-rock-version";

            private const string ContractHeader = "x-sync-contract";

            private const string UrgentHeader = "x-sync-urgent";

            private const string SubmissionIdHeader = "x-sync-submission-id";

            private const long TicksPerMicrosecond = 10L;

            private readonly JObject _contract;

            #endregion

            #region Constructors

            public ChatSyncHeaderBuilder()
                : this( JObject.Parse( ChatWireContract.Json ) )
            {
            }

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

            public IList<string> GetPayloadSections()
            {
                var sections = _contract["payload"] == null ? null : _contract["payload"]["sections"];

                if ( sections == null )
                {
                    throw new InvalidOperationException( "the chat wire contract does not name the payload sections, so nothing here can key a submission" );
                }

                return sections.Select( s => s.Value<string>() ).ToList();
            }

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

        internal sealed class ChatSyncRowMapper
        {
            #region Fields

            private readonly JObject _contract;

            private readonly TimeZoneInfo _organizationTimeZone;

            #endregion

            #region Constructors

            public ChatSyncRowMapper( JObject contract, TimeZoneInfo organizationTimeZone )
            {
                if ( contract == null )
                {
                    throw new ArgumentNullException( "contract" );
                }

                if ( organizationTimeZone == null )
                {
                    throw new ArgumentNullException( "organizationTimeZone" );
                }

                _contract = contract;
                _organizationTimeZone = organizationTimeZone;
            }

            #endregion

            #region Methods

            public IList<object> Map( string section, IList<string> queryColumns, IList<object> rawValues )
            {
                if ( queryColumns == null )
                {
                    throw new ArgumentNullException( "queryColumns" );
                }

                if ( rawValues == null )
                {
                    throw new ArgumentNullException( "rawValues" );
                }

                if ( queryColumns.Count != rawValues.Count )
                {
                    throw new InvalidOperationException( string.Format(
                        "the {0} query returned {1} values for {2} columns",
                        section,
                        rawValues.Count,
                        queryColumns.Count ) );
                }

                var byName = new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase );

                for ( var i = 0; i < queryColumns.Count; i++ )
                {
                    byName[queryColumns[i]] = Normalize( rawValues[i] );
                }

                return GetWireColumns( section ).Select( c => ReadWireColumn( section, c, byName ) ).ToList();
            }

            private object ReadWireColumn( string section, string wireColumn, IDictionary<string, object> byName )
            {
                if ( wireColumn == "badge_keys" )
                {
                    return ReadBadgeKeys( Require( section, wireColumn, "badge_keys", byName ) );
                }

                if ( wireColumn == "ban_expires_at" )
                {
                    return ReadTime( Require( section, wireColumn, "ban_expires_at", byName ) );
                }

                if ( wireColumn == "bg_color" )
                {
                    return ReadBadgeColors( Require( section, wireColumn, "highlight_color", byName ) ).Item1;
                }

                if ( wireColumn == "fg_color" )
                {
                    return ReadBadgeColors( Require( section, wireColumn, "highlight_color", byName ) ).Item2;
                }

                return Require( section, wireColumn, wireColumn, byName );
            }

            private static object Require( string section, string wireColumn, string queryColumn, IDictionary<string, object> byName )
            {
                object value;

                if ( !byName.TryGetValue( queryColumn, out value ) )
                {
                    throw new InvalidOperationException( string.Format(
                        "the {0} query returns no {1}, which the {2} column on the wire is built from",
                        section,
                        queryColumn,
                        wireColumn ) );
                }

                return value;
            }

            private IList<string> GetWireColumns( string section )
            {
                var sections = _contract["payload"]["sections"].Select( s => s.Value<string>() ).ToList();
                var position = sections.IndexOf( section );

                if ( position < 0 )
                {
                    throw new InvalidOperationException( string.Format( "the chat wire contract names no payload section called {0}", section ) );
                }

                return _contract["tables"][position]["columns"].Select( c => c.Value<string>() ).ToList();
            }

            private static object Normalize( object value )
            {
                return value == DBNull.Value ? null : value;
            }

            private object ReadTime( object value )
            {
                if ( value == null )
                {
                    return null;
                }

                var stored = (DateTime) value;

                if ( stored.Kind == DateTimeKind.Utc )
                {
                    return stored;
                }

                // A time out of the database carries no zone, and it is in the organisation's, because
                // that is the only clock Rock writes by. Treating it as already UTC would make it wrong
                // by this church's offset, and for a church behind UTC a ban would lift early.
                var unspecified = DateTime.SpecifyKind( stored, DateTimeKind.Unspecified );

                return TimeZoneInfo.ConvertTimeToUtc( unspecified, _organizationTimeZone );
            }

            public static IList<Guid> ReadBadgeKeys( object joined )
            {
                var text = Normalize( joined ) as string;

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

            public static Tuple<string, string> ReadBadgeColors( object highlightColor )
            {
                var text = ( Normalize( highlightColor ) as string ?? string.Empty ).Trim();

                // The field is free text in Rock, so a church can put a colour name, a function or
                // anything else in it. A badge with no colour still renders; a submission refused over
                // one badge takes that church down for the whole cycle.
                if ( text.Length == 0 || text[0] != '#' )
                {
                    return Tuple.Create( (string) null, (string) null );
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
                    return Tuple.Create( (string) null, (string) null );
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

            private static double RelativeLuminance( int red, int green, int blue )
            {
                return ( 0.2126 * Straighten( red ) ) + ( 0.7152 * Straighten( green ) ) + ( 0.0722 * Straighten( blue ) );
            }

            private static double Straighten( int channel )
            {
                var value = channel / 255.0;

                return value <= 0.03928 ? value / 12.92 : Math.Pow( ( value + 0.055 ) / 1.055, 2.4 );
            }

            #endregion
        }

        internal sealed class ChatSyncPayloadWriter : IDisposable
        {
            #region Fields

            private readonly JObject _contract;

            private readonly JsonWriter _writer;

            private readonly Dictionary<string, int> _rowCounts = new Dictionary<string, int>();

            private string _openSection;

            #endregion

            #region Constructors

            public ChatSyncPayloadWriter( JObject contract, JsonWriter writer )
            {
                if ( contract == null )
                {
                    throw new ArgumentNullException( "contract" );
                }

                if ( writer == null )
                {
                    throw new ArgumentNullException( "writer" );
                }

                _contract = contract;
                _writer = writer;

                _writer.WriteStartObject();
            }

            #endregion

            #region Properties

            public IDictionary<string, int> RowCounts
            {
                get { return _rowCounts; }
            }

            #endregion

            #region Methods

            public int GetRowWidth( string section )
            {
                var sections = GetSections();
                var position = sections.IndexOf( section );

                if ( position < 0 )
                {
                    throw new InvalidOperationException( string.Format( "the chat wire contract names no payload section called {0}", section ) );
                }

                var tables = _contract["tables"];

                // The contract states that a section holds the rows of the table in the same position
                // in its table list, which is the only thing that ties a section to a width.
                if ( tables == null || tables.Count() != sections.Count )
                {
                    throw new InvalidOperationException( "the chat wire contract names a different number of payload sections than tables, so no section can be matched to a width" );
                }

                return tables[position]["columns"].Count();
            }

            private IList<string> GetSections()
            {
                var sections = _contract["payload"] == null ? null : _contract["payload"]["sections"];

                if ( sections == null )
                {
                    throw new InvalidOperationException( "the chat wire contract does not name the payload sections, so nothing here can key a body" );
                }

                return sections.Select( s => s.Value<string>() ).ToList();
            }

            public void BeginSection( string section )
            {
                if ( _openSection != null )
                {
                    throw new InvalidOperationException( string.Format( "the {0} section is still open", _openSection ) );
                }

                if ( _rowCounts.ContainsKey( section ) )
                {
                    throw new InvalidOperationException( string.Format( "the {0} section has already been written", section ) );
                }

                // Asks the contract for the width now rather than at the first row, so a section name
                // the contract does not know fails where it was named.
                GetRowWidth( section );

                _openSection = section;
                _rowCounts[section] = 0;

                _writer.WritePropertyName( section );
                _writer.WriteStartArray();
            }

            public void WriteRow( IList<object> values )
            {
                if ( _openSection == null )
                {
                    throw new InvalidOperationException( "no payload section is open" );
                }

                if ( values == null )
                {
                    throw new ArgumentNullException( "values" );
                }

                var width = GetRowWidth( _openSection );

                // A row of the wrong width shifts every value after the gap one place. Nothing further
                // down can see that once the types on either side of the gap happen to agree, so it is
                // refused here rather than sent.
                if ( values.Count != width )
                {
                    throw new InvalidOperationException( string.Format(
                        "a {0} row carries {1} values where the contract gives that table {2} columns",
                        _openSection,
                        values.Count,
                        width ) );
                }

                _writer.WriteStartArray();

                foreach ( var value in values )
                {
                    WriteValue( value );
                }

                _writer.WriteEndArray();

                _rowCounts[_openSection] = _rowCounts[_openSection] + 1;
            }

            private void WriteValue( object value )
            {
                if ( value == null )
                {
                    _writer.WriteNull();
                    return;
                }

                if ( value is Guid )
                {
                    // Lowercase and hyphenated is the one form that parses as a uuid on the far side
                    // and compares equal to the same value already stored there. SQL Server renders
                    // them uppercase by default and orders their bytes differently again.
                    _writer.WriteValue( ( (Guid)value ).ToString( "D" ).ToLowerInvariant() );
                    return;
                }

                if ( value is DateTime )
                {
                    WriteTime( (DateTime)value );
                    return;
                }

                var guids = value as IEnumerable<Guid>;

                if ( guids != null )
                {
                    // The column behind this is a uuid array, and the drain reads anything that is not
                    // a JSON array as an empty one, so a joined string would give a person no badges
                    // on a submission the platform accepts with nothing reported anywhere.
                    _writer.WriteStartArray();

                    foreach ( var guid in guids )
                    {
                        _writer.WriteValue( guid.ToString( "D" ).ToLowerInvariant() );
                    }

                    _writer.WriteEndArray();
                    return;
                }

                if ( value is string || value is bool || value is int || value is long || value is short || value is byte || value is decimal || value is double )
                {
                    _writer.WriteValue( value );
                    return;
                }

                // Anything else would be serialized by whatever Json.NET decides, which is how a value
                // reaches the wire in a shape nobody chose.
                throw new InvalidOperationException( string.Format(
                    "a {0} row carries a {1}, which has no agreed form on the wire",
                    _openSection,
                    value.GetType().Name ) );
            }

            private void WriteTime( DateTime value )
            {
                if ( value.Kind != DateTimeKind.Utc )
                {
                    throw new InvalidOperationException( string.Format(
                        "a {0} row carries a time that is not UTC, so the platform would read it in its own zone and the value would be wrong by this church's offset",
                        _openSection ) );
                }

                _writer.WriteValue( value.ToString( "yyyy-MM-ddTHH:mm:ss.ffffff'Z'", CultureInfo.InvariantCulture ) );
            }

            public void EndSection()
            {
                if ( _openSection == null )
                {
                    throw new InvalidOperationException( "no payload section is open" );
                }

                _writer.WriteEndArray();
                _openSection = null;
            }

            public void Complete()
            {
                if ( _openSection != null )
                {
                    throw new InvalidOperationException( string.Format( "the {0} section is still open", _openSection ) );
                }

                // A section left out is not a church with none of that row, it is a projection that did
                // not run, and the platform applies a restatement as truth.
                var missing = GetSections().Except( _rowCounts.Keys ).ToList();

                if ( missing.Any() )
                {
                    throw new InvalidOperationException( string.Format(
                        "the body was completed without the {0} section, which the platform would apply as an empty church",
                        string.Join( ", ", missing ) ) );
                }

                _writer.WriteEndObject();
            }

            public void Dispose()
            {
                _writer.Close();
            }

            #endregion
        }

        internal sealed class ChatSyncIdentityMarks
        {
            #region Properties

            public long Person { get; set; }

            public long PersonAlias { get; set; }

            public long Group { get; set; }

            public long GroupMember { get; set; }

            #endregion
        }

        #endregion Building the submission
    }
}
