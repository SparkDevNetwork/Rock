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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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

        // No save a person makes through Rock's screens comes near this, so a push past it is a bulk
        // write that reached the hooks, and the full sync carries it instead. An estimate.
        internal const int PushRowCeiling = 5000;

        // How long a push nobody waits on may take before it is abandoned. An estimate.
        internal static readonly TimeSpan BackgroundPushTimeout = TimeSpan.FromSeconds( 10 );

        // How long a block action waits on its own save's push before it answers without it. An
        // estimate, within what a person waiting on a chat action is given.
        internal static readonly TimeSpan AwaitedPushBudget = TimeSpan.FromSeconds( 2 );

        // The one transport override a test may hold at a time, or null in production.
        private static ImmediateSyncOverride _override;

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

        /// <summary>
        /// What a block action that waited on its save's push can tell the person.
        /// </summary>
        internal enum PushOutcome
        {
            /// <summary>
            /// The chat platform took the push.
            /// </summary>
            Applied,

            /// <summary>
            /// The push failed or did not answer in time. It may still land, and the full sync
            /// carries the change if it does not.
            /// </summary>
            Pending
        }

        /// <summary>
        /// Stands in for the immediate sync's transport while a test holds it, and records what
        /// the immediate sync began and warned about in that time.
        /// </summary>
        /// <remarks>
        /// Process wide, as the transport it replaces is, so a test waits on the pushes it caused
        /// rather than guessing how long a background push takes. Only one may be held at a time,
        /// because two tests sharing one transport could not tell whose push is whose.
        /// </remarks>
        internal sealed class ImmediateSyncOverride : IDisposable
        {
            private readonly object _sync = new object();

            private readonly List<Task> _pushes = new List<Task>();

            private readonly List<string> _warnings = new List<string>();

            /// <summary>
            /// Creates the override.
            /// </summary>
            /// <param name="handler">The transport every push is sent through while this is held.</param>
            internal ImmediateSyncOverride( HttpMessageHandler handler )
            {
                Handler = handler ?? throw new ArgumentNullException( nameof( handler ) );
            }

            /// <summary>
            /// The transport every push is sent through while this is held.
            /// </summary>
            public HttpMessageHandler Handler { get; }

            /// <summary>
            /// Every warning the immediate sync logged while this was held.
            /// </summary>
            public IList<string> Warnings
            {
                get
                {
                    lock ( _sync )
                    {
                        return _warnings.ToList();
                    }
                }
            }

            /// <summary>
            /// Waits for every push begun while this was held to finish, however it finishes.
            /// </summary>
            /// <param name="timeout">How long to wait.</param>
            /// <returns>True where every one finished in time.</returns>
            public bool WaitForPushes( TimeSpan timeout )
            {
                Task[] pushes;

                lock ( _sync )
                {
                    // A faulted push is still a finished one, so each is waited on through a
                    // continuation that cannot fault.
                    pushes = _pushes.Select( p => p.ContinueWith( _ => { }, TaskScheduler.Default ) ).ToArray();
                }

                return Task.WaitAll( pushes, timeout );
            }

            /// <summary>
            /// Records a push the immediate sync began.
            /// </summary>
            /// <param name="push">The push's work.</param>
            internal void Began( Task push )
            {
                lock ( _sync )
                {
                    _pushes.Add( push );
                }
            }

            /// <summary>
            /// Records a warning the immediate sync logged.
            /// </summary>
            /// <param name="message">The warning.</param>
            internal void Warned( string message )
            {
                lock ( _sync )
                {
                    _warnings.Add( message );
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {
                Interlocked.CompareExchange( ref _override, null, this );
            }
        }

        #endregion Types

        #region Methods

        /// <summary>
        /// Records a person's save for the immediate sync when it changed something chat shows.
        /// Called at the end of the person save hook's pre-save.
        /// </summary>
        /// <param name="entry">The save entry.</param>
        /// <remarks>
        /// Runs on every person save in Rock, so a save that changes nothing chat shows returns
        /// having read nothing and allocated nothing.
        /// </remarks>
        internal static void RecordPersonSave( IEntitySaveEntry entry )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Records a group's save for the immediate sync when the group can be a chat channel.
        /// Called at the end of the group save hook's pre-save.
        /// </summary>
        /// <param name="entry">The save entry.</param>
        /// <remarks>
        /// Runs on every group save in Rock, families included, so a group chat cannot reach returns
        /// having read nothing and allocated nothing.
        /// </remarks>
        internal static void RecordGroupSave( IEntitySaveEntry entry )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Records a membership's save for the immediate sync when its group can be a chat channel
        /// or is one of the groups that run chat. Called at the end of the group member save hook's
        /// pre-save, after the hook has set the member's group type.
        /// </summary>
        /// <param name="entry">The save entry.</param>
        /// <remarks>
        /// Runs on every membership save in Rock, so one chat cannot reach returns having read
        /// nothing and allocated nothing.
        /// </remarks>
        internal static void RecordGroupMemberSave( IEntitySaveEntry entry )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Whether a person's save changed a value chat shows, read from the entry alone.
        /// </summary>
        /// <param name="entry">The save entry.</param>
        /// <returns>True where the save is one the immediate sync pushes.</returns>
        internal static bool IsPersonChangeInScope( IEntitySaveEntry entry )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Whether a group, or a membership of it, is one the immediate sync pushes: its type
        /// allows chat, or it is one of the groups that run chat. Read from cached values alone.
        /// </summary>
        /// <param name="groupId">The group.</param>
        /// <param name="groupTypeId">The group's type.</param>
        /// <returns>True where the save is one the immediate sync pushes.</returns>
        internal static bool IsGroupInScope( int groupId, int groupTypeId )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Records that a person's aliases and memberships changed where no save hook sees it, so
        /// they are pushed after the context's transaction commits.
        /// </summary>
        /// <param name="rockContext">The context whose commit the push follows.</param>
        /// <param name="personId">The person.</param>
        /// <remarks>
        /// The person merge runs a procedure that moves aliases and memberships past Entity
        /// Framework, so it records the surviving person here, inside its transaction.
        /// </remarks>
        internal static void RecordPersonChange( RockContext rockContext, int personId )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Waits, within the awaited budget, for the push the context's last save began.
        /// </summary>
        /// <param name="rockContext">The context the block action saved with.</param>
        /// <returns>Applied where the platform took the push or there was nothing to push, and Pending otherwise. Never throws.</returns>
        /// <remarks>
        /// For a block action that tells a person their change is live. A push that outlasts the
        /// budget is abandoned by the caller, not cancelled: it may still land, and the full sync
        /// carries the change if it does not.
        /// </remarks>
        internal static Task<PushOutcome> FlushAsync( RockContext rockContext )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sends every immediate push through another transport until the returned override is
        /// disposed. For tests.
        /// </summary>
        /// <param name="handler">The transport.</param>
        /// <returns>The override, which records the pushes begun and the warnings logged while it is held.</returns>
        internal static ImmediateSyncOverride OverrideImmediateSync( HttpMessageHandler handler )
        {
            var replacement = new ImmediateSyncOverride( handler );

            if ( Interlocked.CompareExchange( ref _override, replacement, null ) != null )
            {
                throw new InvalidOperationException( "the immediate sync's transport is already overridden, and two overrides could not tell whose push is whose" );
            }

            return replacement;
        }

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
