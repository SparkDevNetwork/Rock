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
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Contract;
using Rock.Data;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The immediate sync: what a save touched, read back after it commits and pushed to the chat
    /// platform at once, so a change made in Rock shows in chat without waiting for the next sync.
    /// </summary>
    internal static partial class ChatPlatformSyncHelper
    {
        #region Constants

        // A push reads a handful of keys, and the full sync repairs whatever one that gives up
        // leaves behind, so it is never worth the full sync's long wait. An estimate.
        private const int PushProjectionTimeoutSeconds = 10;

        #endregion Constants

        #region Types

        /// <summary>
        /// The keys one save touched, as the save hooks record them.
        /// </summary>
        internal sealed class ImmediateChanges
        {
            /// <summary>
            /// People whose own chat values may have changed.
            /// </summary>
            public HashSet<int> PersonIds { get; } = new HashSet<int>();

            /// <summary>
            /// Groups whose channel, or whose whole membership, may have changed. By guid, so a
            /// group that was deleted can still be named.
            /// </summary>
            public HashSet<Guid> GroupGuids { get; } = new HashSet<Guid>();

            /// <summary>
            /// Single memberships that may have changed, as the group's guid and the person's id.
            /// </summary>
            public HashSet<(Guid GroupGuid, int PersonId)> MemberKeys { get; } = new HashSet<(Guid GroupGuid, int PersonId)>();
        }

        /// <summary>
        /// The rows a push states and the moment it states them as of.
        /// </summary>
        internal sealed class PushBody
        {
            /// <summary>
            /// The database's own time, taken before any row was read, in UTC.
            /// </summary>
            public DateTime ReadAtUtc { get; set; }

            /// <summary>
            /// The four sections and the keys that no longer exist, as the push sends them.
            /// </summary>
            public JObject Body { get; set; }

            /// <summary>
            /// How many rows the sections carry.
            /// </summary>
            public int RowCount { get; set; }
        }

        #endregion Types

        #region Methods

        /// <summary>
        /// Reads back the committed rows a save touched through the one projection.
        /// </summary>
        /// <param name="rockContext">A context the save did not use.</param>
        /// <param name="configuration">The church's chat settings.</param>
        /// <param name="changes">What the save touched.</param>
        /// <returns>The push body.</returns>
        /// <remarks>
        /// The procedure writes the channel mark for the groups the save touched before it reads
        /// anything, and the mark must commit even when the push fails, so no transaction may span
        /// this call.
        /// </remarks>
        internal static PushBody ProjectChanges( RockContext rockContext, ChatPlatformConfiguration configuration, ImmediateChanges changes )
        {
            if ( rockContext == null )
            {
                throw new ArgumentNullException( nameof( rockContext ) );
            }

            if ( configuration == null )
            {
                throw new ArgumentNullException( nameof( configuration ) );
            }

            if ( changes == null )
            {
                throw new ArgumentNullException( nameof( changes ) );
            }

            var parameters = ProjectionParameters( configuration );

            // All three are sent even when empty, because any one of them set is what makes the
            // call scoped, and an unscoped call reads the whole church.
            parameters["@ScopePersonIdsJson"] = new JArray( changes.PersonIds ).ToString( Formatting.None );
            parameters["@ScopeGroupGuidsJson"] = new JArray( changes.GroupGuids.Select( g => g.ToString() ) ).ToString( Formatting.None );
            parameters["@ScopeMemberKeysJson"] = new JArray( changes.MemberKeys.Select( k => new JArray( k.GroupGuid.ToString(), k.PersonId ) ) ).ToString( Formatting.None );

            // The context owns the connection: close it only if it was opened here, never dispose it.
            var connection = rockContext.Database.Connection;
            var wasClosed = connection.State != ConnectionState.Open;

            try
            {
                if ( wasClosed )
                {
                    connection.Open();
                }

                using ( var command = CreateProjectionCommand( connection, parameters, PushProjectionTimeoutSeconds ) )
                using ( var reader = command.ExecuteReader() )
                {
                    return ReadPushBody( reader );
                }
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
        /// Builds the push body from a scoped call's result sets: the moment, the four sections
        /// written by the same rules as the full sync's, and the keys named absent.
        /// </summary>
        /// <param name="reader">The reader, on the procedure's first result set.</param>
        /// <returns>The push body.</returns>
        private static PushBody ReadPushBody( DbDataReader reader )
        {
            if ( !reader.Read() )
            {
                throw new InvalidOperationException( "the moment this projection describes could not be taken" );
            }

            var readAtUtc = DateTime.SpecifyKind( reader.GetDateTime( 0 ), DateTimeKind.Utc );
            var rowCounts = new Dictionary<string, int>();
            JObject body;

            // Written as tokens rather than text, through the one row writer, so a push row can
            // never differ from the same row in a restatement.
            using ( var writer = new JTokenWriter() )
            {
                WriteSections( reader, JObject.Parse( ChatWireContract.Json ), writer, RockDateTime.OrgTimeZoneInfo, rowCounts );
                body = ( JObject ) writer.Token;
            }

            // The configured badges left out, which a scoped call leaves empty, since a push never
            // carries badges.
            if ( !reader.NextResult() )
            {
                throw new InvalidOperationException( "the projection returned no result set for the badges it left out" );
            }

            using ( var writer = new JTokenWriter() )
            {
                writer.WriteStartObject();
                WriteAbsentKeys( reader, writer, "channels" );
                WriteAbsentKeys( reader, writer, "members" );
                writer.WriteEndObject();

                body["absent"] = writer.Token;
            }

            return new PushBody
            {
                ReadAtUtc = readAtUtc,
                Body = body,
                RowCount = rowCounts.Values.Sum()
            };
        }

        /// <summary>
        /// Writes one of the scoped call's absent key sets: a single key per row as a value, and a
        /// key of several parts as a positional array.
        /// </summary>
        /// <param name="reader">The reader, on the result set before this one.</param>
        /// <param name="writer">Where the keys are written.</param>
        /// <param name="kind">The name the push gives the set.</param>
        private static void WriteAbsentKeys( DbDataReader reader, JsonWriter writer, string kind )
        {
            // Missing only when the call was not scoped, and then nothing it returned may be read as
            // a push: an empty set would say that nothing left chat.
            if ( !reader.NextResult() )
            {
                throw new InvalidOperationException( string.Format( "the projection returned no absent {0}, so it did not read a scope", kind ) );
            }

            var section = "absent " + kind;

            writer.WritePropertyName( kind );
            writer.WriteStartArray();

            while ( reader.Read() )
            {
                if ( reader.FieldCount == 1 )
                {
                    WriteValue( writer, reader.GetValue( 0 ), section );
                    continue;
                }

                writer.WriteStartArray();

                for ( var i = 0; i < reader.FieldCount; i++ )
                {
                    WriteValue( writer, reader.GetValue( i ), section );
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }

        #endregion Methods
    }
}
