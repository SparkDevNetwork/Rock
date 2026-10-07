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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Sync;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The immediate sync against a real chat platform: a save in Rock, the push after its commit,
    /// and the row on the platform, with the time between the two.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Every other immediate sync test stops at a stand-in for the platform, which can agree
    ///         with Rock about a request the real platform refuses: a header in the wrong form, a
    ///         token it cannot verify, a body it will not parse. This one makes the real call. Each
    ///         test provisions its own church on the platform with a key made on the spot, so it
    ///         never depends on what another run left behind, and it reads the rows back through the
    ///         platform's data API under the service role.
    ///     </para>
    ///     <para>
    ///         It runs only when a chat platform is named in the environment, as
    ///         <see cref="LocalChatPlatform"/> describes. The timings go to the test output.
    ///     </para>
    /// </remarks>
    [TestClass]
    [TestCategory( "ChatPlatformEndToEnd" )]
    public class ImmediateSyncEndToEndTests : DatabaseTestsBase
    {
        #region Constants

        private const int TimedSaves = 20;

        #endregion Constants

        #region Tests

        [TestMethod]
        public void AMemberSavedInsideARequestReachesThePlatformAtItsReadTimeAndItsDeletionStampsItAbsent()
        {
            var platform = LocalChatPlatform.FromEnvironment();

            using ( var fixture = new ChatSyncProjectionFixture() )
            using ( var recorder = new PushRecorder() )
            using ( ChatPlatformSyncHelper.OverrideImmediateSync( recorder ) )
            {
                var configuration = platform.ProvisionChurch();
                fixture.StoreConfiguration( configuration );

                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "End to end channel" );
                var personId = fixture.AddPerson( "EndToEnd" );
                var alias = fixture.PrimaryAliasGuid( personId );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    AddMember( rockContext, channel, personId );
                    rockContext.SaveChanges();
                }

                var row = platform.WaitForMember( configuration.TenantId.Value, channel, alias, r => r != null );
                Assert.IsNotNull( row, "the membership saved in Rock never reached the platform" );
                Assert.IsTrue( row["absent_since"].Type == JTokenType.Null, "a membership just added is present" );

                var readAt = recorder.ReadTimes.LastOrDefault();
                Assert.IsNotNull( readAt, "the push carried no read time" );
                Assert.AreEqual( ParseInstant( readAt ), ParseInstant( ( string ) row["synced_at"] ),
                    "the platform stamps the row with the moment Rock read it, never its own clock" );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    var service = new GroupMemberService( rockContext );
                    service.DeleteRange( service.Queryable().Where( m => m.Group.Guid == channel && m.PersonId == personId ).ToList() );
                    rockContext.SaveChanges();
                }

                var removed = platform.WaitForMember( configuration.TenantId.Value, channel, alias, r => r != null && r["absent_since"].Type != JTokenType.Null );
                Assert.IsNotNull( removed, "the membership deleted in Rock was never stamped absent on the platform" );
            }
        }

        [TestMethod]
        public void AMemberWhoseRoleMayMentionEveryoneAndPostAnnouncementsCarriesBothOnThePlatform()
        {
            var platform = LocalChatPlatform.FromEnvironment();

            using ( var fixture = new ChatSyncProjectionFixture() )
            using ( var recorder = new PushRecorder() )
            using ( ChatPlatformSyncHelper.OverrideImmediateSync( recorder ) )
            {
                var configuration = platform.ProvisionChurch();
                fixture.StoreConfiguration( configuration );

                var roleId = fixture.AddRole( fixture.SharedGroupTypeId, "Announcer", false );
                fixture.SetRoleCapabilitiesDirectly( roleId, true, true );

                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Capable channel" );
                var personId = fixture.AddPerson( "Capable" );
                var alias = fixture.PrimaryAliasGuid( personId );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    AddMember( rockContext, channel, personId, roleId );
                    rockContext.SaveChanges();
                }

                var row = platform.WaitForMember( configuration.TenantId.Value, channel, alias, r => r != null );
                Assert.IsNotNull( row, "the membership saved in Rock never reached the platform" );
                Assert.IsTrue( ( bool ) row["can_mention_all"], "the platform did not store that the member's role may mention everyone" );
                Assert.IsTrue( ( bool ) row["can_post_announcements"], "the platform did not store that the member's role may post announcements" );
            }
        }

        [TestMethod]
        public void ABanListAddSetsTheGloballyBannedFlagOnThePlatform()
        {
            var platform = LocalChatPlatform.FromEnvironment();

            using ( var fixture = new ChatSyncProjectionFixture() )
            using ( var recorder = new PushRecorder() )
            using ( ChatPlatformSyncHelper.OverrideImmediateSync( recorder ) )
            {
                var configuration = platform.ProvisionChurch();
                fixture.StoreConfiguration( configuration );

                var personId = fixture.AddPerson( "Banned" );
                var alias = fixture.PrimaryAliasGuid( personId );

                // Outside a request, as a workflow the job engine runs would add it.
                using ( var rockContext = new RockContext() )
                {
                    AddMember( rockContext, Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST.AsGuid(), personId );
                    rockContext.SaveChanges();
                }

                var row = platform.WaitForAlias( configuration.TenantId.Value, alias, r => r != null && ( bool? ) r["is_globally_banned"] == true );
                Assert.IsNotNull( row, "the ban made in Rock never reached the platform" );
            }
        }

        [TestMethod]
        public void APersonsOwnOpenDmSettingSavedThroughTheDoorReachesThePlatform()
        {
            var platform = LocalChatPlatform.FromEnvironment();

            using ( var fixture = new ChatSyncProjectionFixture() )
            using ( var recorder = new PushRecorder() )
            using ( ChatPlatformSyncHelper.OverrideImmediateSync( recorder ) )
            {
                var configuration = platform.ProvisionChurch();
                fixture.StoreConfiguration( configuration );

                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Settings channel" );
                var personId = fixture.AddPerson( "Settings" );
                var alias = fixture.PrimaryAliasGuid( personId );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    AddMember( rockContext, channel, personId );
                    rockContext.SaveChanges();
                }

                Assert.IsNotNull( platform.WaitForAlias( configuration.TenantId.Value, alias, r => r != null ),
                    "the person never reached the platform" );

                Rock.ViewModels.Blocks.Communication.Chat.ChatShell.ChatPersonSettingResultBag result;
                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    var person = new PersonService( rockContext ).Get( personId );
                    var context = Rock.Communication.Chat.Platform.Session.ChatSessionHelper.BuildSessionContext( person, configuration, rockContext );

                    result = Rock.Communication.Chat.Platform.Doors.ChatDoorHelper
                        .SavePersonSettingAsync( person, "open_dm", true, context, rockContext ).GetAwaiter().GetResult();
                }

                Assert.AreEqual( "ok", result.Code, result.Message );

                var row = platform.WaitForAlias( configuration.TenantId.Value, alias, r => r != null && ( bool? ) r["is_open_dm_allowed"] == true );
                Assert.IsNotNull( row, "the setting saved through the door never reached the platform" );

                // How long the door holds a person while it waits for the platform: twenty more
                // saves, alternating, each timed from the call to its answer.
                var elapsed = new List<double>();
                var pending = 0;
                for ( var i = 0; i < 20; i++ )
                {
                    using ( ChatSyncProjectionFixture.InsideRequest() )
                    using ( var rockContext = new RockContext() )
                    {
                        var person = new PersonService( rockContext ).Get( personId );
                        var context = Rock.Communication.Chat.Platform.Session.ChatSessionHelper.BuildSessionContext( person, configuration, rockContext );
                        var stopwatch = Stopwatch.StartNew();

                        var answer = Rock.Communication.Chat.Platform.Doors.ChatDoorHelper
                            .SavePersonSettingAsync( person, "open_dm", i % 2 == 0 ? false : true, context, rockContext ).GetAwaiter().GetResult();

                        elapsed.Add( stopwatch.Elapsed.TotalMilliseconds );
                        pending += answer.IsPending ? 1 : 0;
                        Assert.AreEqual( "ok", answer.Code, answer.Message );
                    }
                }

                elapsed.Sort();

                TestContext.WriteLine( string.Join( Environment.NewLine,
                    $"door saves timed: {elapsed.Count}, from the call to its answer, the awaited push included; pending: {pending}",
                    $"p50 ms: {Percentile( elapsed, 0.50 ):F1}",
                    $"p95 ms: {Percentile( elapsed, 0.95 ):F1}",
                    $"max ms: {elapsed.Last():F1}" ) );
            }
        }

        [TestMethod]
        public void TwentySavesAreTimedFromTheSaveToTheRowOnThePlatform()
        {
            var platform = LocalChatPlatform.FromEnvironment();

            using ( var fixture = new ChatSyncProjectionFixture() )
            using ( var recorder = new PushRecorder() )
            using ( ChatPlatformSyncHelper.OverrideImmediateSync( recorder ) )
            {
                var configuration = platform.ProvisionChurch();
                fixture.StoreConfiguration( configuration );

                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Timed channel" );
                var elapsed = new List<double>();

                // One more than is timed: the first push in a process signs in, which later ones do not.
                for ( var i = 0; i <= TimedSaves; i++ )
                {
                    var personId = fixture.AddPerson( "Timed" + i );
                    var alias = fixture.PrimaryAliasGuid( personId );
                    Stopwatch stopwatch;

                    using ( ChatSyncProjectionFixture.InsideRequest() )
                    using ( var rockContext = new RockContext() )
                    {
                        AddMember( rockContext, channel, personId );
                        rockContext.SaveChanges();
                        stopwatch = Stopwatch.StartNew();
                    }

                    var row = platform.WaitForMember( configuration.TenantId.Value, channel, alias, r => r != null );
                    stopwatch.Stop();

                    Assert.IsNotNull( row, $"save {i} never reached the platform" );

                    if ( i > 0 )
                    {
                        elapsed.Add( stopwatch.Elapsed.TotalMilliseconds );
                    }
                }

                elapsed.Sort();

                TestContext.WriteLine( string.Join( Environment.NewLine,
                    $"saves timed: {elapsed.Count}, from SaveChanges returning to the row readable on the platform, polled every {LocalChatPlatform.PollInterval.TotalMilliseconds:F0} ms",
                    "elapsed ms: " + string.Join( ", ", elapsed.Select( e => e.ToString( "F1", CultureInfo.InvariantCulture ) ) ),
                    $"p50 ms: {Percentile( elapsed, 0.50 ):F1}",
                    $"p95 ms: {Percentile( elapsed, 0.95 ):F1}",
                    $"max ms: {elapsed.Last():F1}" ) );
            }
        }

        #endregion Tests

        #region Support

        private static void AddMember( RockContext rockContext, Guid groupGuid, int personId )
        {
            AddMember( rockContext, groupGuid, personId, null );
        }

        /// <summary>
        /// Adds a person to a group in a role, or in the group type's default role when none is given.
        /// </summary>
        private static void AddMember( RockContext rockContext, Guid groupGuid, int personId, int? roleId )
        {
            var group = new GroupService( rockContext ).Queryable( "GroupType" ).Single( g => g.Guid == groupGuid );

            new GroupMemberService( rockContext ).Add( new GroupMember
            {
                Guid = Guid.NewGuid(),
                GroupId = group.Id,
                GroupTypeId = group.GroupTypeId,
                PersonId = personId,
                GroupRoleId = roleId ?? group.GroupType.DefaultGroupRoleId.Value,
                GroupMemberStatus = GroupMemberStatus.Active
            } );
        }

        private static DateTimeOffset ParseInstant( string value )
        {
            return DateTimeOffset.Parse( value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind );
        }

        /// <summary>
        /// The nearest-rank percentile of values already sorted.
        /// </summary>
        private static double Percentile( IList<double> sorted, double fraction )
        {
            var rank = ( int ) Math.Ceiling( fraction * sorted.Count );

            return sorted[Math.Max( 0, rank - 1 )];
        }

        /// <summary>
        /// Sends every request on to the real platform, and keeps the read time of each push.
        /// </summary>
        private sealed class PushRecorder : DelegatingHandler
        {
            private readonly object _sync = new object();

            private readonly List<string> _readTimes = new List<string>();

            public PushRecorder()
                : base( new HttpClientHandler() )
            {
            }

            public IList<string> ReadTimes
            {
                get
                {
                    lock ( _sync )
                    {
                        return _readTimes.ToList();
                    }
                }
            }

            protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
            {
                if ( request.RequestUri.AbsolutePath == "/rest/v1/rpc/sync_push" && request.Headers.TryGetValues( "x-sync-read-at", out var values ) )
                {
                    lock ( _sync )
                    {
                        _readTimes.Add( values.First() );
                    }
                }

                return base.SendAsync( request, cancellationToken );
            }
        }

        #endregion Support
    }
}
