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
using System.Data.Common;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Contract;
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Net;
using Rock.Web.Cache;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The immediate sync: what a save touched, read back after it commits and pushed to the chat
    /// platform at once, so a change made in Rock shows in chat without waiting for the next sync.
    /// </summary>
    public static partial class ChatPlatformSyncHelper
    {
        #region Constants

        // A push reads a handful of keys, and the full sync repairs one that gives up, so it is
        // never worth the full sync's long wait. Every figure here is an estimate. A background
        // push is two steps with a limit each, this read and then the request below, so the
        // longest one takes is the two added: 20 seconds.
        private const int PushProjectionTimeoutSeconds = 10;

        // No save a person makes through Rock's screens comes near this, so a push past it is a bulk
        // write that reached the hooks, and the full sync carries it instead.
        internal const int PushRowCeiling = 5000;

        // The request alone, sign-in included; the projection read before it has its own limit.
        internal static readonly TimeSpan BackgroundPushTimeout = TimeSpan.FromSeconds( 10 );

        // Within what a person waiting on a chat action is given.
        internal static readonly TimeSpan AwaitedPushBudget = TimeSpan.FromSeconds( 2 );

        // So a platform that is down for an hour leaves a handful of rows in the exception log
        // rather than one for every save in that hour. An estimate.
        private static readonly TimeSpan FailureLogInterval = TimeSpan.FromMinutes( 1 );

        // Chat People is added by a startup fix, so a save made before it ran must not hide it for
        // good. An estimate: often enough that the group is found soon after the fix, rarely enough
        // that a missing group costs a cache read a minute, not one a save.
        private const int MissingSystemGroupRetryMilliseconds = 60000;

        private static ImmediateSyncOverride _override;

        // Kept for the process, so its platform token is exchanged once every few minutes rather
        // than once a save.
        private static PlatformClient _transport;

        private static ChatSystemGroups _systemGroups;

        private static long _failureLoggedAtTicks;

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
            /// Groups whose whole membership may have changed, with their channel, by guid so a
            /// deleted group can still be named.
            /// </summary>
            public HashSet<Guid> GroupGuids { get; } = new HashSet<Guid>();

            /// <summary>
            /// Groups whose channel row alone may have changed: a save that left alone everything
            /// that decides who is in the channel, such as a rename.
            /// </summary>
            public HashSet<Guid> ChannelGuids { get; } = new HashSet<Guid>();

            /// <summary>
            /// Single memberships that may have changed, as the group's guid and the person's id.
            /// </summary>
            public HashSet<(Guid GroupGuid, int PersonId)> MemberKeys { get; } = new HashSet<(Guid GroupGuid, int PersonId)>();

            /// <summary>
            /// The church's chat settings, read for the first key a context records.
            /// </summary>
            internal ChatPlatformConfiguration Configuration { get; set; }

            /// <summary>
            /// Whether the context will push these keys after its next commit.
            /// </summary>
            internal bool IsFlushRegistered { get; set; }

            /// <summary>
            /// The address of the request that made the save, for the warning a push too large leaves.
            /// </summary>
            internal string Source { get; set; }

            /// <summary>
            /// The push the context's last committed save began, which a block action may wait on.
            /// </summary>
            internal Task<PushOutcome> LastPush { get; set; }

            /// <summary>
            /// Whether nothing is recorded.
            /// </summary>
            internal bool IsEmpty => PersonIds.Count == 0 && GroupGuids.Count == 0 && ChannelGuids.Count == 0 && MemberKeys.Count == 0;

            /// <summary>
            /// Moves the recorded keys into a new set, so the next save on the context records afresh.
            /// </summary>
            /// <returns>The keys recorded until now.</returns>
            internal ImmediateChanges TakeKeys()
            {
                var keys = new ImmediateChanges();
                keys.PersonIds.UnionWith( PersonIds );
                keys.GroupGuids.UnionWith( GroupGuids );
                keys.ChannelGuids.UnionWith( ChannelGuids );
                keys.MemberKeys.UnionWith( MemberKeys );

                PersonIds.Clear();
                GroupGuids.Clear();
                ChannelGuids.Clear();
                MemberKeys.Clear();

                return keys;
            }
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

            /// <summary>
            /// How many keys the push names absent.
            /// </summary>
            public int AbsentCount { get; set; }
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
        /// Stands in for the immediate sync's transport while a test holds it, and records the pushes
        /// begun and the warnings logged in that time.
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

                // The client was built over this override's transport, so the next push builds its own.
                Interlocked.Exchange( ref _transport, null );
            }
        }

        /// <summary>
        /// The ids of the groups that run chat, as they were when last looked up.
        /// </summary>
        private sealed class ChatSystemGroups
        {
            public int? ChatPeopleId { get; set; }

            public int? BanListId { get; set; }

            public int? AdministratorsId { get; set; }

            public int ReadAtTickCount { get; set; }

            public bool IsComplete => ChatPeopleId.HasValue && BanListId.HasValue && AdministratorsId.HasValue;
        }

        #endregion Types

        #region Methods

        /// <summary>
        /// Records a person's save when it changed something chat shows. Called at the end of the
        /// person save hook's pre-save, on every person save in Rock.
        /// </summary>
        /// <param name="entry">The save entry.</param>
        internal static void RecordPersonSave( IEntitySaveEntry entry )
        {
            var isRecorded = IsPersonChangeInScope( entry ) && RockRequestContextAccessor.Current != null;
            if ( !isRecorded )
            {
                return;
            }

            ChangesFor( entry.DataContext as RockContext )?.PersonIds.Add( ( ( Person ) entry.Entity ).Id );
        }

        /// <summary>
        /// Records a group's save when the group can be a chat channel. Called at the end of the
        /// group save hook's pre-save, on every group save in Rock, families included.
        /// </summary>
        /// <param name="entry">The save entry.</param>
        internal static void RecordGroupSave( IEntitySaveEntry entry )
        {
            var group = entry?.Entity as Group;

            // The groups that run chat are never channels, so their own saves have nothing to push.
            var isRecorded = group != null
                && !IsChatSystemGroup( group.Id )
                && IsGroupInScope( group.Id, group.GroupTypeId )
                && RockRequestContextAccessor.Current != null;

            if ( !isRecorded )
            {
                return;
            }

            var changes = ChangesFor( entry.DataContext as RockContext );
            if ( changes == null )
            {
                return;
            }

            // A rename of a group of thousands would otherwise push every member, past the row
            // ceiling, and leave the new name waiting for the full sync.
            if ( IsGroupMembershipChangeInScope( entry ) )
            {
                changes.GroupGuids.Add( group.Guid );
            }
            else
            {
                changes.ChannelGuids.Add( group.Guid );
            }
        }

        /// <summary>
        /// Records a membership's save when its group can be a chat channel or is one of the groups
        /// that run chat. Called at the end of the group member save hook's pre-save, after the hook
        /// has set the member's group type.
        /// </summary>
        /// <param name="entry">The save entry.</param>
        internal static void RecordGroupMemberSave( IEntitySaveEntry entry )
        {
            var member = entry?.Entity as GroupMember;
            if ( member == null || !IsGroupInScope( member.GroupId, member.GroupTypeId ) )
            {
                return;
            }

            // A ban is pushed from every path, because a workflow the job engine runs is still a
            // ban. Every other save is pushed only from a request, so a job or an import is left to
            // the full sync rather than sending a push for each row it writes.
            var isBanList = member.GroupId == SystemGroups().BanListId;
            if ( !isBanList && RockRequestContextAccessor.Current == null )
            {
                return;
            }

            var changes = ChangesFor( entry.DataContext as RockContext );
            if ( changes == null )
            {
                return;
            }

            // A membership of a group that runs chat changes the person's own alias row, such as
            // whether they are banned, rather than a membership row.
            if ( IsChatSystemGroup( member.GroupId ) )
            {
                changes.PersonIds.Add( member.PersonId );
                return;
            }

            AddMemberKey( changes, member.Group?.Guid, member.GroupId, member.PersonId );

            if ( entry.State != EntityContextState.Modified )
            {
                return;
            }

            // A membership moved to another group or person leaves its old pair behind, and only a
            // pair the push names is stamped absent.
            var originalGroupId = entry.OriginalValues.TryGetValue( nameof( GroupMember.GroupId ), out var groupId ) ? groupId as int? : null;
            var originalPersonId = entry.OriginalValues.TryGetValue( nameof( GroupMember.PersonId ), out var personId ) ? personId as int? : null;
            var isMoved = ( originalGroupId.HasValue && originalGroupId.Value != member.GroupId )
                || ( originalPersonId.HasValue && originalPersonId.Value != member.PersonId );

            if ( isMoved )
            {
                AddMemberKey( changes, null, originalGroupId ?? member.GroupId, originalPersonId ?? member.PersonId );
            }
        }

        /// <summary>
        /// Whether a person's save changed a value chat shows, read from the entry alone so an
        /// ordinary save reads and allocates nothing.
        /// </summary>
        /// <param name="entry">The save entry.</param>
        /// <returns>True where the save is one the immediate sync pushes.</returns>
        internal static bool IsPersonChangeInScope( IEntitySaveEntry entry )
        {
            var person = entry?.Entity as Person;

            // A new person is in no chat group yet, so the membership that brings them in pushes
            // their alias row; a deleted one has no alias left to project.
            if ( person == null || entry.State != EntityContextState.Modified )
            {
                return false;
            }

            var original = entry.OriginalValues;

            return !IsUnchanged( original, nameof( Person.NickName ), person.NickName )
                || !IsUnchanged( original, nameof( Person.LastName ), person.LastName )
                || !IsUnchanged( original, nameof( Person.PhotoId ), person.PhotoId )
                || !IsUnchanged( original, nameof( Person.PrimaryCampusId ), person.PrimaryCampusId )
                || !IsUnchanged( original, nameof( Person.RecordStatusValueId ), person.RecordStatusValueId )
                || !IsUnchanged( original, nameof( Person.IsDeceased ), person.IsDeceased )
                || !IsUnchanged( original, nameof( Person.IsChatProfilePublic ), person.IsChatProfilePublic )
                || !IsUnchanged( original, nameof( Person.IsChatOpenDirectMessageAllowed ), person.IsChatOpenDirectMessageAllowed );
        }

        /// <summary>
        /// Whether a group's save could have changed who is in its channel, read from the entry
        /// alone so it reads and allocates nothing.
        /// </summary>
        /// <param name="entry">The save entry.</param>
        /// <returns>True where the push must read the group's whole membership.</returns>
        internal static bool IsGroupMembershipChangeInScope( IEntitySaveEntry entry )
        {
            var group = entry?.Entity as Group;

            // A group added or deleted brings its members into chat or takes them out.
            if ( group == null || entry.State != EntityContextState.Modified )
            {
                return true;
            }

            var original = entry.OriginalValues;

            // What the projection reads to decide whether a group is a channel that takes members.
            return !IsUnchanged( original, nameof( Group.IsChatEnabledOverride ), group.IsChatEnabledOverride )
                || !IsUnchanged( original, nameof( Group.GroupTypeId ), group.GroupTypeId )
                || !IsUnchanged( original, nameof( Group.IsActive ), group.IsActive )
                || !IsUnchanged( original, nameof( Group.IsArchived ), group.IsArchived );
        }

        /// <summary>
        /// Whether a group, or a membership of it, is one the immediate sync pushes: its type allows
        /// chat, or it is one of the groups that run chat. Read from cached values alone, so an
        /// ordinary save never reaches the database here.
        /// </summary>
        /// <param name="groupId">The group.</param>
        /// <param name="groupTypeId">The group's type.</param>
        /// <returns>True where the save is one the immediate sync pushes.</returns>
        internal static bool IsGroupInScope( int groupId, int groupTypeId )
        {
            if ( IsChatSystemGroup( groupId ) )
            {
                return true;
            }

            var groupType = GroupTypeCache.Get( groupTypeId );

            return groupType != null && groupType.IsChatAllowed;
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
        [Rock.Attribute.RockInternal( "20.0", true )]
        public static void RecordPersonChange( RockContext rockContext, int personId )
        {
            var changes = ChangesFor( rockContext );
            if ( changes == null )
            {
                return;
            }

            changes.PersonIds.Add( personId );

            // A person's key carries their aliases alone, so each membership that can be a channel's
            // is named too, read inside the caller's transaction where the merge has already moved it.
            var memberships = new GroupMemberService( rockContext ).Queryable()
                .Where( m => m.PersonId == personId )
                .Select( m => new { m.GroupId, m.GroupTypeId, GroupGuid = m.Group.Guid } )
                .ToList();

            foreach ( var membership in memberships )
            {
                if ( !IsChatSystemGroup( membership.GroupId ) && IsGroupInScope( membership.GroupId, membership.GroupTypeId ) )
                {
                    changes.MemberKeys.Add( (membership.GroupGuid, personId) );
                }
            }
        }

        /// <summary>
        /// Waits, within the awaited budget, for the push the context's last save began.
        /// </summary>
        /// <param name="rockContext">The context the block action saved with.</param>
        /// <returns>Applied where the platform took the push or there was nothing to push, and Pending otherwise. Never throws.</returns>
        /// <remarks>
        /// A push that outlasts the budget is abandoned, not cancelled: it may still land, and the
        /// full sync carries the change if it does not.
        /// </remarks>
        internal static async Task<PushOutcome> FlushAsync( RockContext rockContext )
        {
            var push = rockContext?.GetOptions<ImmediateChanges>()?.LastPush;

            // Nothing began means nothing chat shows changed, so there is nothing pending.
            if ( push == null )
            {
                return PushOutcome.Applied;
            }

            try
            {
                var finished = await Task.WhenAny( push, Task.Delay( AwaitedPushBudget ) ).ConfigureAwait( false );

                return finished == push ? await push.ConfigureAwait( false ) : PushOutcome.Pending;
            }
            catch ( Exception exception )
            {
                // The push absorbs its own failures, so this only keeps the promise never to throw.
                LogPushFailure( exception );
                return PushOutcome.Pending;
            }
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

            // Dropped so the first push under the override signs in through it.
            Interlocked.Exchange( ref _transport, null );

            return replacement;
        }

        /// <summary>
        /// The keys the context has recorded, with the push after its next commit registered, or
        /// null where the church has not set chat up.
        /// </summary>
        /// <param name="rockContext">The context the save is made in.</param>
        /// <returns>The context's keys, or null.</returns>
        private static ImmediateChanges ChangesFor( RockContext rockContext )
        {
            if ( rockContext == null )
            {
                return null;
            }

            var changes = rockContext.GetOrCreateOptions<ImmediateChanges>();

            // Read once per context, because the read parses and decrypts a stored setting.
            if ( changes.Configuration == null )
            {
                changes.Configuration = ChatPlatformConfigurationService.Read();
            }

            if ( !changes.Configuration.IsConfigured )
            {
                return null;
            }

            // One registration per commit: the callback clears the flag. After a rollback the
            // registration and the keys ride along with the context's next commit, which is
            // harmless because a push reads committed truth.
            if ( !changes.IsFlushRegistered )
            {
                changes.IsFlushRegistered = true;
                changes.Source = RockRequestContextAccessor.Current?.RequestUri?.AbsolutePath;
                rockContext.ExecuteAfterCommit( () => Flush( changes ) );
            }

            return changes;
        }

        /// <summary>
        /// Begins the push of what a commit touched, in the background.
        /// </summary>
        /// <param name="changes">The context's keys.</param>
        /// <remarks>
        /// The read runs in the background too, so the save's thread returns at once, and a read
        /// made while the caller still holds a transaction waits for the commit on its own thread
        /// rather than blocking the thread that holds the locks. That wait assumes the database
        /// reads committed data under locks, as SQL Server does by default: with read committed
        /// snapshot on, the read does not wait, and sees the rows as they were before the caller's
        /// transaction, which the next full sync then corrects.
        /// </remarks>
        private static void Flush( ImmediateChanges changes )
        {
            changes.IsFlushRegistered = false;

            if ( changes.IsEmpty )
            {
                return;
            }

            var keys = changes.TakeKeys();
            var configuration = changes.Configuration;
            var source = changes.Source;

            var push = Task.Run( () => PushChangesAsync( configuration, keys, source ) );
            changes.LastPush = push;

            Volatile.Read( ref _override )?.Began( push );
        }

        /// <summary>
        /// Reads back what a commit touched and pushes it, absorbing every failure, since the full
        /// sync repairs a push that did not land.
        /// </summary>
        /// <param name="configuration">The church's chat settings.</param>
        /// <param name="keys">What the commit touched.</param>
        /// <param name="source">Where the save came from, for a warning.</param>
        /// <returns>Applied where the platform took the push or there was nothing to push.</returns>
        private static async Task<PushOutcome> PushChangesAsync( ChatPlatformConfiguration configuration, ImmediateChanges keys, string source )
        {
            try
            {
                PushBody push;

                // Disposed before any request is sent, so a slow platform never holds a connection.
                using ( var rockContext = new RockContext() )
                {
                    push = ProjectChanges( rockContext, configuration, keys );
                }

                if ( push.RowCount == 0 && push.AbsentCount == 0 )
                {
                    return PushOutcome.Applied;
                }

                if ( push.RowCount > PushRowCeiling )
                {
                    var warning = string.Format(
                        "A chat push of {0} rows from {1} was not sent, because no ordinary save touches that many; the next full chat sync carries it.",
                        push.RowCount,
                        source ?? "a save outside a web request" );

                    RockLogger.LoggerFactory.CreateLogger( typeof( ChatPlatformSyncHelper ).FullName ).LogWarning( warning );
                    Volatile.Read( ref _override )?.Warned( warning );

                    return PushOutcome.Pending;
                }

                using ( var timeout = new CancellationTokenSource( BackgroundPushTimeout ) )
                {
                    return await TransportFor( configuration ).PushAsync( push, timeout.Token ).ConfigureAwait( false );
                }
            }
            catch ( Exception exception )
            {
                LogPushFailure( exception );
                return PushOutcome.Pending;
            }
        }

        /// <summary>
        /// The process's client, built anew when the settings or the test transport changed.
        /// </summary>
        /// <param name="configuration">The church's chat settings.</param>
        /// <returns>The client.</returns>
        private static PlatformClient TransportFor( ChatPlatformConfiguration configuration )
        {
            var handler = Volatile.Read( ref _override )?.Handler;
            var current = Volatile.Read( ref _transport );

            if ( current != null && current.IsFor( configuration, handler ) )
            {
                return current;
            }

            // Two pushes racing to replace it each get a working client, and one of them is kept.
            // The client is replaced only when the church's connection settings change (tenant,
            // project address, publishable key, signing key), which happens once when chat is
            // enabled and otherwise essentially never, so a replaced client is left for the
            // runtime to collect rather than closed under a push that may still be using it.
            var created = new PlatformClient( configuration, handler );
            var previous = Interlocked.CompareExchange( ref _transport, created, current );

            return previous == current ? created : previous ?? created;
        }

        /// <summary>
        /// Whether a group is one of the groups that run chat.
        /// </summary>
        private static bool IsChatSystemGroup( int groupId )
        {
            var groups = SystemGroups();

            return groupId == groups.ChatPeopleId || groupId == groups.BanListId || groupId == groups.AdministratorsId;
        }

        /// <summary>
        /// The ids of the groups that run chat, looked up again only while one is missing, and then
        /// no more than once a minute.
        /// </summary>
        private static ChatSystemGroups SystemGroups()
        {
            var current = Volatile.Read( ref _systemGroups );
            var isTrusted = current != null
                && ( current.IsComplete || unchecked( Environment.TickCount - current.ReadAtTickCount ) < MissingSystemGroupRetryMilliseconds );

            if ( isTrusted )
            {
                return current;
            }

            var read = new ChatSystemGroups
            {
                ChatPeopleId = GroupCache.GetId( Rock.SystemGuid.Group.GROUP_CHAT_PEOPLE.AsGuid() ),
                BanListId = GroupCache.GetId( Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST.AsGuid() ),
                AdministratorsId = GroupCache.GetId( Rock.SystemGuid.Group.GROUP_CHAT_ADMINISTRATORS.AsGuid() ),
                ReadAtTickCount = Environment.TickCount
            };

            Volatile.Write( ref _systemGroups, read );

            return read;
        }

        /// <summary>
        /// Records a membership pair by its group's guid, which names it even once the group is gone.
        /// </summary>
        private static void AddMemberKey( ImmediateChanges changes, Guid? groupGuid, int groupId, int personId )
        {
            var guid = groupGuid ?? GroupCache.Get( groupId )?.Guid;

            if ( guid.HasValue )
            {
                changes.MemberKeys.Add( (guid.Value, personId) );
            }
        }

        /// <summary>
        /// Whether a text value is as it was before the save. A value the entry does not carry is
        /// read as unchanged.
        /// </summary>
        private static bool IsUnchanged( IReadOnlyDictionary<string, object> original, string name, string current )
        {
            return !original.TryGetValue( name, out var value ) || string.Equals( value as string, current, StringComparison.Ordinal );
        }

        /// <summary>
        /// Whether a number is as it was before the save. Unboxed in place, so it allocates nothing.
        /// </summary>
        private static bool IsUnchanged( IReadOnlyDictionary<string, object> original, string name, int? current )
        {
            return !original.TryGetValue( name, out var value ) || ( value as int? ) == current;
        }

        /// <summary>
        /// Whether a flag is as it was before the save. Unboxed in place, so it allocates nothing.
        /// </summary>
        private static bool IsUnchanged( IReadOnlyDictionary<string, object> original, string name, bool? current )
        {
            return !original.TryGetValue( name, out var value ) || ( value as bool? ) == current;
        }

        /// <summary>
        /// Logs why a push did not land, at most once a minute, since every save made while the
        /// platform is unreachable fails the same way.
        /// </summary>
        /// <param name="exception">What went wrong. It never carries a token or a key.</param>
        private static void LogPushFailure( Exception exception )
        {
            var now = DateTime.UtcNow.Ticks;
            var last = Interlocked.Read( ref _failureLoggedAtTicks );

            var isDue = now - last >= FailureLogInterval.Ticks
                && Interlocked.CompareExchange( ref _failureLoggedAtTicks, now, last ) == last;

            if ( isDue )
            {
                ExceptionLogService.LogException( exception );
            }
        }

        /// <summary>
        /// Reads back the committed rows a save touched through the one projection.
        /// </summary>
        /// <param name="rockContext">A context the save did not use.</param>
        /// <param name="configuration">The church's chat settings.</param>
        /// <param name="changes">What the save touched.</param>
        /// <returns>The push body.</returns>
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

            // All four are sent even when empty, because any one of them set is what makes the
            // call scoped, and an unscoped call reads the whole church.
            parameters["@ScopePersonIdsJson"] = new JArray( changes.PersonIds ).ToString( Formatting.None );
            parameters["@ScopeGroupGuidsJson"] = new JArray( changes.GroupGuids.Select( g => g.ToString() ) ).ToString( Formatting.None );
            parameters["@ScopeChannelGuidsJson"] = new JArray( changes.ChannelGuids.Select( g => g.ToString() ) ).ToString( Formatting.None );
            parameters["@ScopeMemberKeysJson"] = new JArray( changes.MemberKeys.Select( k => new JArray( k.GroupGuid.ToString(), k.PersonId ) ) ).ToString( Formatting.None );

            var contract = JObject.Parse( ChatWireContract.Json );

            return ReadProjection( rockContext, parameters, PushProjectionTimeoutSeconds, reader => ReadPushBody( reader, contract ) );
        }

        /// <summary>
        /// Builds the push body from a scoped call's result sets: the moment, the four sections
        /// written by the full sync's own row writer, and the keys named absent.
        /// </summary>
        /// <param name="reader">The reader, on the procedure's first result set.</param>
        /// <param name="contract">The parsed wire contract.</param>
        /// <returns>The push body.</returns>
        internal static PushBody ReadPushBody( DbDataReader reader, JObject contract )
        {
            if ( !reader.Read() )
            {
                throw new InvalidOperationException( "the moment this projection describes could not be taken" );
            }

            var readAtUtc = DateTime.SpecifyKind( reader.GetDateTime( 0 ), DateTimeKind.Utc );
            var rowCounts = new Dictionary<string, int>();
            // The platform refuses a push keyed any other way, so the names are read from the
            // contract rather than typed here.
            var push = contract["push"];
            JObject body;

            // The one row writer, so a push row can never differ from the same row in a restatement.
            using ( var writer = new JTokenWriter() )
            {
                WriteSections( reader, contract, writer, RockDateTime.OrgTimeZoneInfo, rowCounts );
                body = ( JObject ) writer.Token;
            }

            // The badges left out, which only the whole restatement reports.
            if ( !reader.NextResult() )
            {
                throw new InvalidOperationException( "the projection returned no result set for the badges it left out" );
            }

            var absentCount = 0;

            using ( var writer = new JTokenWriter() )
            {
                writer.WriteStartObject();

                foreach ( var set in push["absent"]["sets"] )
                {
                    absentCount += WriteAbsentKeys( reader, writer, set["name"].Value<string>(), set["key_columns"].Select( c => c.Value<string>() ).ToList() );
                }

                writer.WriteEndObject();

                body[push["absent"]["key"].Value<string>()] = writer.Token;
            }

            return new PushBody
            {
                ReadAtUtc = readAtUtc,
                Body = body,
                RowCount = rowCounts.Values.Sum(),
                AbsentCount = absentCount
            };
        }

        /// <summary>
        /// Writes one of the scoped call's absent key sets: a key of one column as that value, and a
        /// key of several as a positional array in the contract's order.
        /// </summary>
        /// <param name="reader">The reader, on the result set before this one.</param>
        /// <param name="writer">Where the keys are written.</param>
        /// <param name="name">The name the contract gives the set.</param>
        /// <param name="keyColumns">The wire columns the contract says the key is made of.</param>
        /// <returns>How many keys were written.</returns>
        private static int WriteAbsentKeys( DbDataReader reader, JsonWriter writer, string name, IList<string> keyColumns )
        {
            // Missing only when the call was not scoped, and then an empty set would say that
            // nothing left chat.
            if ( !reader.NextResult() )
            {
                throw new InvalidOperationException( string.Format( "the projection returned no absent {0}, so it did not read a scope", name ) );
            }

            var section = "absent " + name;

            // Resolved by name, as a section's columns are, so a result set in another order or a
            // contract naming another key fails here rather than sending a key nobody holds.
            var ordinals = keyColumns.Select( column =>
            {
                for ( var i = 0; i < reader.FieldCount; i++ )
                {
                    if ( string.Equals( reader.GetName( i ), column, StringComparison.Ordinal ) )
                    {
                        return i;
                    }
                }

                throw new InvalidOperationException( string.Format( "the projection's {0} result set has no {1} column", section, column ) );
            } ).ToList();

            var written = 0;

            writer.WritePropertyName( name );
            writer.WriteStartArray();

            while ( reader.Read() )
            {
                written++;

                if ( ordinals.Count == 1 )
                {
                    WriteValue( writer, reader.GetValue( ordinals[0] ), section );
                    continue;
                }

                writer.WriteStartArray();

                foreach ( var ordinal in ordinals )
                {
                    WriteValue( writer, reader.GetValue( ordinal ), section );
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray();

            return written;
        }

        #endregion Methods
    }
}
