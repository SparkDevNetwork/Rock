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
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// One whole restatement: mark the channels, read the church, send it, and find out what
    /// happened to it.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Every section of the payload is read on one open connection, because the staging
    ///         query leaves its sets in temporary tables and those belong to the session that made
    ///         them. That is not an implementation convenience: it is the whole of what stops a
    ///         membership arriving in the same payload as neither the channel nor the person it
    ///         names.
    ///     </para>
    ///     <para>
    ///         The marking runs first and on its own. A mark rolled back alongside a failed
    ///         submission would leave the next run treating a group as though it had never been a
    ///         channel, and that is the one thing the mark exists to prevent.
    ///     </para>
    /// </remarks>
    internal sealed class ChatSyncRunner
    {
        #region Constants

        /// <summary>
        /// How long the projection may take. Generous, because it reads the whole of a large
        /// church's group membership and runs on that church's own server, and because the cost of
        /// being wrong here is a cycle lost rather than a cycle wrong.
        /// </summary>
        private const int ProjectionTimeoutSeconds = 300;

        #endregion Constants

        #region Fields

        private readonly ChatPlatformConfiguration _configuration;

        private readonly bool _isManualRun;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Prepares a run for one church.
        /// </summary>
        /// <param name="configuration">The church's chat settings.</param>
        /// <param name="isManualRun">Whether a person started this run rather than the schedule.</param>
        public ChatSyncRunner( ChatPlatformConfiguration configuration, bool isManualRun )
        {
            if ( configuration == null )
            {
                throw new ArgumentNullException( nameof( configuration ) );
            }

            _configuration = configuration;
            _isManualRun = isManualRun;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Builds the transport. Replaced where a caller does not want a real one.
        /// </summary>
        public Func<ChatSyncSubmitClient> ClientFactory { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Runs one cycle.
        /// </summary>
        /// <param name="rockContext">The context the projection reads through.</param>
        /// <returns>What the run has to say for itself.</returns>
        public ChatSyncRunResult Run( RockContext rockContext )
        {
            if ( rockContext == null )
            {
                throw new ArgumentNullException( nameof( rockContext ) );
            }

            var token = ChatSessionHelper.TryMintSyncToken( new ChatSessionContext { Configuration = _configuration } );
            if ( !token.Success )
            {
                return new ChatSyncRunResult
                {
                    IsFailure = true,
                    Message = "Nothing was submitted. This church could not sign a request to the chat platform: " + token.Gate + "."
                };
            }

            StampChannels( rockContext );

            var submissionId = Guid.NewGuid();
            DateTime readAtUtc;
            ChatSyncIdentityMarks marks;
            string payload;
            IDictionary<string, int> rowCounts;

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

                readAtUtc = ReadClock( connection );
                marks = ReadIdentityMarks( connection );
                payload = BuildPayload( connection, out rowCounts );
            }
            finally
            {
                if ( wasClosed && connection.State == ConnectionState.Open )
                {
                    connection.Close();
                }
            }

            var headers = BuildHeaders( readAtUtc, marks, rowCounts );

            using ( var client = BuildClient() )
            {
                var acknowledgement = client.Submit( submissionId, payload, headers );

                // Written whatever the outcome was: advice about the platform's load is no less
                // true because this submission was turned away.
                ChatPlatformConfigurationService.SaveSyncBackoff( acknowledgement.SyncBackoffUntil );

                ChatSyncOutcome polled = null;
                if ( acknowledgement.Status == ChatSyncSubmissionStatus.Accepted )
                {
                    polled = client.Poll( submissionId,
                        _isManualRun ? ChatSyncPollBudget.Manual : ChatSyncPollBudget.Scheduled );
                }

                var result = ChatSyncStatusPath.Resolve( acknowledgement, polled );

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

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// Marks the groups that are chat channels right now, in its own transaction and before the
        /// clock below is read, so the projection sees one settled set of marks.
        /// </summary>
        private void StampChannels( RockContext rockContext )
        {
            rockContext.Database.CommandTimeout = ProjectionTimeoutSeconds;
            rockContext.Database.ExecuteSqlCommand(
                ChatSyncProjection.GetStampSql(),
                new System.Data.SqlClient.SqlParameter( "@StampedAt", RockDateTime.Now ) );
        }

        /// <summary>
        /// The moment this restatement describes, taken from the database rather than from this
        /// process, and in UTC because it is compared against the platform's own clock.
        /// </summary>
        private DateTime ReadClock( DbConnection connection )
        {
            using ( var command = CreateCommand( connection, "SELECT SYSUTCDATETIME();" ) )
            {
                return DateTime.SpecifyKind( ( DateTime ) command.ExecuteScalar(), DateTimeKind.Utc );
            }
        }

        /// <summary>
        /// The identity seed of each table the projection reads.
        /// </summary>
        /// <remarks>
        /// The seed and not the largest id in the table. Deleting the newest rows lowers the
        /// largest id and leaves the seed where it was, and a database restored from a backup is
        /// the case these exist to catch: its seeds go backwards and the platform refuses the
        /// submission rather than quietly writing a church's older picture over its newer one.
        /// </remarks>
        private ChatSyncIdentityMarks ReadIdentityMarks( DbConnection connection )
        {
            const string sql =
                "SELECT CAST( IDENT_CURRENT( 'Person' ) AS BIGINT ), "
                + "CAST( IDENT_CURRENT( 'PersonAlias' ) AS BIGINT ), "
                + "CAST( IDENT_CURRENT( '[Group]' ) AS BIGINT ), "
                + "CAST( IDENT_CURRENT( 'GroupMember' ) AS BIGINT );";

            using ( var command = CreateCommand( connection, sql ) )
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

        /// <summary>
        /// Stages the sets once and reads every section from them.
        /// </summary>
        private string BuildPayload( DbConnection connection, out IDictionary<string, int> rowCounts )
        {
            using ( var command = CreateCommand( connection, ChatSyncProjection.GetStagingSql() ) )
            {
                command.ExecuteNonQuery();
            }

            var contract = JObject.Parse( ChatWireContract.Json );
            var mapper = new ChatSyncRowMapper( contract, RockDateTime.OrgTimeZoneInfo );
            var text = new StringBuilder();

            using ( var jsonWriter = new JsonTextWriter( new StringWriter( text, CultureInfo.InvariantCulture ) ) )
            using ( var payloadWriter = new ChatSyncPayloadWriter( contract, jsonWriter ) )
            {
                foreach ( var section in new ChatSyncHeaderBuilder( contract ).GetPayloadSections() )
                {
                    WriteSection( connection, payloadWriter, mapper, section );
                }

                payloadWriter.Complete();
                rowCounts = payloadWriter.RowCounts;
            }

            return text.ToString();
        }

        private void WriteSection( DbConnection connection, ChatSyncPayloadWriter payloadWriter, ChatSyncRowMapper mapper, string section )
        {
            payloadWriter.BeginSection( section );

            using ( var command = CreateCommand( connection, ChatSyncProjection.GetSectionSql( section ) ) )
            using ( var reader = command.ExecuteReader() )
            {
                var columns = Enumerable.Range( 0, reader.FieldCount ).Select( reader.GetName ).ToList();

                while ( reader.Read() )
                {
                    var values = new object[reader.FieldCount];
                    reader.GetValues( values );

                    payloadWriter.WriteRow( mapper.Map( section, columns, values ) );
                }
            }

            payloadWriter.EndSection();
        }

        /// <summary>
        /// One command, with only the parameters the text it runs actually names.
        /// </summary>
        private DbCommand CreateCommand( DbConnection connection, string sql )
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = ProjectionTimeoutSeconds;

            foreach ( var parameter in ProjectionParameters() )
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

        /// <summary>
        /// Everything the projection texts ask to be told rather than look up for themselves.
        /// </summary>
        private IDictionary<string, object> ProjectionParameters()
        {
            var badgeGuids = _configuration.ChatBadgeDataViewGuids ?? new List<Guid>();
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
                { "@ProfilesVisibleByDefault", _configuration.AreChatProfilesVisible },
                { "@OpenDirectMessagesByDefault", _configuration.IsOpenDirectMessagingAllowed },
                { "@PublicApplicationRoot", GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) ?? string.Empty }
            };
        }

        /// <summary>
        /// The metadata this submission carries beside its body.
        /// </summary>
        /// <remarks>
        /// Built after the payload rather than before it, because the counts have to be the rows
        /// that were actually written. Counts taken from what the projection was expected to return
        /// would agree with a truncated payload and the platform's own check would pass over it.
        /// </remarks>
        private IDictionary<string, string> BuildHeaders( DateTime readAtUtc, ChatSyncIdentityMarks marks, IDictionary<string, int> rowCounts )
        {
            if ( ChatWireContract.ComputedHash != ChatWireContract.PublishedHash )
            {
                throw new InvalidOperationException(
                    "the wire contract this build of Rock ships does not hash to the value written inside it, so nothing here can say what column order the payload is in" );
            }

            var builder = new ChatSyncHeaderBuilder();

            var headers = new Dictionary<string, string>
            {
                { "x-sync-read-at", ChatSyncHeaderBuilder.FormatReadTime( readAtUtc ) },
                { "x-sync-counts", builder.BuildRowCounts( rowCounts ) },
                { "x-sync-marks", builder.BuildIdentityMarks( marks ) },
                { "x-sync-rock-version", Rock.VersionInfo.VersionInfo.GetRockSemanticVersionNumber() },
                { "x-sync-contract", ChatWireContract.PublishedHash }
            };

            if ( _isManualRun )
            {
                // The one optional header. Its absence is ordinary priority, so a scheduled run
                // sets nothing rather than setting it to a false-looking value.
                headers.Add( "x-sync-urgent", "1" );
            }

            return headers;
        }

        private ChatSyncSubmitClient BuildClient()
        {
            if ( ClientFactory != null )
            {
                return ClientFactory();
            }

            return new ChatSyncSubmitClient( _configuration, MintToken );
        }

        /// <summary>
        /// A fresh church token per request. A church token lasts minutes and a run that read a
        /// large church and then waited out its poll budget can outlast one, so minting it once at
        /// the top would expire mid-run on exactly the churches this matters most for.
        /// </summary>
        private string MintToken()
        {
            var minted = ChatSessionHelper.TryMintSyncToken( new ChatSessionContext { Configuration = _configuration } );

            return minted.Success ? minted.ChurchToken : null;
        }

        private static int Count( IDictionary<string, int> rowCounts, string section )
        {
            int count;
            return rowCounts != null && rowCounts.TryGetValue( section, out count ) ? count : 0;
        }

        #endregion Private Methods
    }
}
