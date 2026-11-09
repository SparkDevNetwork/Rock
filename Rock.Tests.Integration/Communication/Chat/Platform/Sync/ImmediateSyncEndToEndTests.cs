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
        public void AChannelDescriptionEditedInsideARequestReachesThePlatformWithItsGroupTypesName()
        {
            var platform = LocalChatPlatform.FromEnvironment();

            using ( var fixture = new ChatSyncProjectionFixture() )
            using ( var recorder = new PushRecorder() )
            using ( ChatPlatformSyncHelper.OverrideImmediateSync( recorder ) )
            {
                var configuration = platform.ProvisionChurch();
                fixture.StoreConfiguration( configuration );

                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Described channel" );

                // A description edit changes nothing about who is in the room, so the save pushes
                // the channel row alone.
                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    var group = new GroupService( rockContext ).Queryable().First( g => g.Guid == channel );
                    group.Description = "Where the youth team plans each week.";
                    rockContext.SaveChanges();
                }

                var row = platform.WaitForChannel( configuration.TenantId.Value, channel,
                    r => r != null && ( string ) r["description"] == "Where the youth team plans each week." );
                Assert.IsNotNull( row, "the description edited in Rock never reached the platform" );
                Assert.AreEqual( ProjectionChannelTests.GroupTypeName( fixture.SharedGroupTypeId ), ( string ) row["group_type_name"],
                    "the platform did not store the name of the channel's group type" );
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
        public void ABanListRemovalClearsTheGloballyBannedFlagOnThePlatform()
        {
            var platform = LocalChatPlatform.FromEnvironment();

            using ( var fixture = new ChatSyncProjectionFixture() )
            using ( var recorder = new PushRecorder() )
            using ( ChatPlatformSyncHelper.OverrideImmediateSync( recorder ) )
            {
                var configuration = platform.ProvisionChurch();
                fixture.StoreConfiguration( configuration );

                var personId = fixture.AddPerson( "Unbanned" );
                var alias = fixture.PrimaryAliasGuid( personId );
                var banListGuid = Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST.AsGuid();

                // Enrolled in chat, as every person who has used it is: someone the Ban List alone
                // brought into chat leaves it with the ban, and no row is pushed for them.
                fixture.EnsureChatPeopleGroup();
                fixture.AddMember( Rock.SystemGuid.Group.GROUP_CHAT_PEOPLE.AsGuid(), personId );

                using ( var rockContext = new RockContext() )
                {
                    AddMember( rockContext, banListGuid, personId );
                    rockContext.SaveChanges();
                }

                Assert.IsNotNull( platform.WaitForAlias( configuration.TenantId.Value, alias, r => r != null && ( bool? ) r["is_globally_banned"] == true ),
                    "the ban made in Rock never reached the platform, so its removal cannot be told apart from it" );

                // Outside a request too, as a workflow the job engine runs would take it away.
                using ( var rockContext = new RockContext() )
                {
                    var service = new GroupMemberService( rockContext );
                    service.DeleteRange( service.Queryable().Where( m => m.Group.Guid == banListGuid && m.PersonId == personId ).ToList() );
                    rockContext.SaveChanges();
                }

                var row = platform.WaitForAlias( configuration.TenantId.Value, alias, r => r != null && ( bool? ) r["is_globally_banned"] == false );
                Assert.IsNotNull( row, "the person taken off the ban list in Rock is still banned on the platform" );
            }
        }

        [TestMethod]
        public void AChatAdministratorsAddAndRemovalReachThePlatformFromOutsideARequest()
        {
            var platform = LocalChatPlatform.FromEnvironment();

            using ( var fixture = new ChatSyncProjectionFixture() )
            using ( var recorder = new PushRecorder() )
            using ( ChatPlatformSyncHelper.OverrideImmediateSync( recorder ) )
            {
                var configuration = platform.ProvisionChurch();
                fixture.StoreConfiguration( configuration );

                var personId = fixture.AddPerson( "Administrator" );
                var alias = fixture.PrimaryAliasGuid( personId );
                var administratorsGuid = Rock.SystemGuid.Group.GROUP_CHAT_ADMINISTRATORS.AsGuid();

                // Enrolled in chat, so the removal still pushes a row for the person.
                fixture.EnsureChatPeopleGroup();
                fixture.AddMember( Rock.SystemGuid.Group.GROUP_CHAT_PEOPLE.AsGuid(), personId );

                // Outside a request, as a workflow the job engine runs would add it.
                using ( var rockContext = new RockContext() )
                {
                    AddMember( rockContext, administratorsGuid, personId );
                    rockContext.SaveChanges();
                }

                Assert.IsNotNull( platform.WaitForAlias( configuration.TenantId.Value, alias, r => r != null && ( bool? ) r["is_chat_administrator"] == true ),
                    "the Chat Administrator made in Rock never reached the platform" );

                using ( var rockContext = new RockContext() )
                {
                    var service = new GroupMemberService( rockContext );
                    service.DeleteRange( service.Queryable().Where( m => m.Group.Guid == administratorsGuid && m.PersonId == personId ).ToList() );
                    rockContext.SaveChanges();
                }

                Assert.IsNotNull( platform.WaitForAlias( configuration.TenantId.Value, alias, r => r != null && ( bool? ) r["is_chat_administrator"] == false ),
                    "the person taken out of Chat Administrators in Rock still manages every channel on the platform" );
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

        [TestMethod]
        public void ABanUntilATimeSavedThroughTheMemberScreenHidesAPublicRoomUntilThatTimePasses()
        {
            var platform = LocalChatPlatform.FromEnvironment();

            using ( var fixture = new ChatSyncProjectionFixture() )
            using ( var recorder = new PushRecorder() )
            using ( var sync = ChatPlatformSyncHelper.OverrideImmediateSync( recorder ) )
            {
                var configuration = platform.ProvisionChurch();
                fixture.StoreConfiguration( configuration );

                // The fixture's type makes every channel public.
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Timed ban channel" );
                var personId = fixture.AddPerson( "TimedBan" );
                var alias = fixture.PrimaryAliasGuid( personId );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    AddMember( rockContext, channel, personId );
                    rockContext.SaveChanges();
                }

                Assert.IsNotNull( platform.WaitForMember( configuration.TenantId.Value, channel, alias, r => r != null ), "the membership never reached the platform" );

                var token = PersonToken( personId, configuration );
                Assert.IsTrue( CallAsPerson( configuration, token, "chat_get_history", channel ).IsSuccess, "a member reads the room before the ban" );

                // Whole seconds, as the screen's picker sets them.
                var until = RockDateTime.Now.AddDays( 1 );
                until = until.AddTicks( -( until.Ticks % TimeSpan.TicksPerSecond ) );
                var memberId = MemberId( channel, personId );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                {
                    var save = Blocks.GroupMemberDetailChatBanTests.SaveThroughBlock( memberId, bag =>
                    {
                        bag.IsChatBanned = true;
                        bag.ChatBannedUntil = until.ToRockDateTimeOffset();
                    } );
                    Assert.AreEqual( System.Net.HttpStatusCode.OK, save.StatusCode, save.Error );
                }

                var banned = platform.WaitForMember( configuration.TenantId.Value, channel, alias, r => r != null && ( bool ) r["is_banned"] );
                Assert.IsNotNull( banned, "the ban saved on the member screen never reached the platform" );
                Assert.AreEqual( TimeZoneInfo.ConvertTimeToUtc( until, RockDateTime.OrgTimeZoneInfo ), ParseInstant( ( string ) banned["ban_expires_at"] ).UtcDateTime,
                    "the platform holds the ban's end as the same instant Rock stored, in UTC" );

                var refused = CallAsPerson( configuration, token, "chat_get_history", channel );
                Assert.IsFalse( refused.IsSuccess, "a banned member reads a public room's history" );
                StringAssert.Contains( refused.Body, "authz.not_readable" );

                // The end moved behind now by a workflow, which a ban's edit pushes from any path.
                using ( var rockContext = new RockContext() )
                {
                    var member = new GroupMemberService( rockContext ).Get( memberId );
                    member.ChatBannedUntil = RockDateTime.Now.AddMinutes( -1 );
                    rockContext.SaveChanges();
                }

                Assert.IsNotNull( platform.WaitForMember( configuration.TenantId.Value, channel, alias,
                    r => r != null && r["ban_expires_at"].Type != JTokenType.Null && ParseInstant( ( string ) r["ban_expires_at"] ) < DateTimeOffset.UtcNow ),
                    "the ended ban never reached the platform" );
                Assert.IsTrue( sync.WaitForPushes( TimeSpan.FromSeconds( 10 ) ) );

                var read = CallAsPerson( configuration, token, "chat_get_history", channel );
                Assert.IsTrue( read.IsSuccess, "once its time has passed the room reads again: " + read.Body );
            }
        }

        [TestMethod]
        public void ABannedMemberStaffRemoveStaysOutOfThePublicRoomAfterTryingToJoinAgain()
        {
            var platform = LocalChatPlatform.FromEnvironment();

            using ( var fixture = new ChatSyncProjectionFixture() )
            using ( var recorder = new PushRecorder() )
            using ( var sync = ChatPlatformSyncHelper.OverrideImmediateSync( recorder ) )
            {
                var configuration = platform.ProvisionChurch();
                fixture.StoreConfiguration( configuration );

                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Removed ban channel" );
                var personId = fixture.AddPerson( "RemovedBan" );
                var alias = fixture.PrimaryAliasGuid( personId );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    AddMember( rockContext, channel, personId );
                    rockContext.SaveChanges();
                    var member = new GroupMemberService( rockContext ).Queryable().Single( m => m.Group.Guid == channel && m.PersonId == personId );
                    member.IsChatBanned = true;
                    rockContext.SaveChanges();
                }

                Assert.IsNotNull( platform.WaitForMember( configuration.TenantId.Value, channel, alias, r => r != null && ( bool ) r["is_banned"] ), "the ban never reached the platform" );

                // Staff remove her the way Rock's screens do, in a group that keeps no history.
                using ( ChatSyncProjectionFixture.InsideRequest() )
                {
                    ChatBanRemovalTests.DeleteThroughRock( channel, personId );
                }

                Assert.IsTrue( sync.WaitForPushes( TimeSpan.FromSeconds( 10 ) ), "the removal's push did not finish" );
                var kept = platform.WaitForMember( configuration.TenantId.Value, channel, alias, r => r != null );
                Assert.IsTrue( kept["absent_since"].Type == JTokenType.Null && ( bool ) kept["is_banned"], "the platform still holds her ban after the removal" );

                Rock.ViewModels.Blocks.Communication.Chat.ChatShell.ChatMembershipResultBag joined;
                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    var person = new PersonService( rockContext ).Get( personId );
                    var context = Rock.Communication.Chat.Platform.Session.ChatSessionHelper.BuildSessionContext( person, configuration, rockContext );
                    joined = Rock.Communication.Chat.Platform.Doors.ChatDoorHelper.JoinChannelAsync( person, channel, context, rockContext ).GetAwaiter().GetResult();
                }

                Assert.AreEqual( "door.not_allowed", joined.Code, "joining again does not step around the ban" );

                var token = PersonToken( personId, configuration );
                var refused = CallAsPerson( configuration, token, "chat_get_history", channel );
                Assert.IsFalse( refused.IsSuccess, "the removed banned member reads the public room" );
                StringAssert.Contains( refused.Body, "authz.not_readable" );

                // Staff lift the ban on the archived row: she is no member, and the public room reads.
                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    var member = new GroupMemberService( rockContext ).AsNoFilter().Single( m => m.Group.Guid == channel && m.PersonId == personId );
                    member.IsChatBanned = false;
                    rockContext.SaveChanges();
                }

                Assert.IsNotNull( platform.WaitForMember( configuration.TenantId.Value, channel, alias, r => r != null && r["absent_since"].Type != JTokenType.Null ),
                    "the lifted ban never reached the platform" );

                var read = CallAsPerson( configuration, token, "chat_get_history", channel );
                Assert.IsTrue( read.IsSuccess, "with the ban lifted the public room reads: " + read.Body );
            }
        }

        [TestMethod]
        public void APersonStaffAddBackBesideAnArchivedBanStaysOutOfThePublicRoomUntilThatBanEnds()
        {
            var platform = LocalChatPlatform.FromEnvironment();

            using ( var fixture = new ChatSyncProjectionFixture() )
            using ( var recorder = new PushRecorder() )
            using ( var sync = ChatPlatformSyncHelper.OverrideImmediateSync( recorder ) )
            {
                var configuration = platform.ProvisionChurch();
                fixture.StoreConfiguration( configuration );

                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Added back channel" );
                var personId = fixture.AddPerson( "AddedBack" );
                var alias = fixture.PrimaryAliasGuid( personId );

                // Whole seconds, as the screen's picker sets them.
                var until = RockDateTime.Now.AddDays( 1 );
                until = until.AddTicks( -( until.Ticks % TimeSpan.TicksPerSecond ) );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    AddMember( rockContext, channel, personId );
                    rockContext.SaveChanges();
                    var member = new GroupMemberService( rockContext ).Queryable().Single( m => m.Group.Guid == channel && m.PersonId == personId );
                    member.IsChatBanned = true;
                    member.ChatBannedUntil = until;
                    rockContext.SaveChanges();
                }

                Assert.IsNotNull( platform.WaitForMember( configuration.TenantId.Value, channel, alias, r => r != null && ( bool ) r["is_banned"] ), "the ban never reached the platform" );

                // Staff remove her, which archives the banned row, then add her back with a new row
                // rather than restoring the old one.
                using ( ChatSyncProjectionFixture.InsideRequest() )
                {
                    ChatBanRemovalTests.DeleteThroughRock( channel, personId );
                }

                Assert.IsTrue( sync.WaitForPushes( TimeSpan.FromSeconds( 10 ) ), "the removal's push did not finish" );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    AddMember( rockContext, channel, personId );
                    rockContext.SaveChanges();
                }

                Assert.IsTrue( sync.WaitForPushes( TimeSpan.FromSeconds( 10 ) ), "the new row's push did not finish" );

                var row = platform.WaitForMember( configuration.TenantId.Value, channel, alias, r => r != null && r["ban_expires_at"].Type != JTokenType.Null );
                Assert.IsNotNull( row, "the platform never heard the ban's end once a live row stood behind it" );
                Assert.IsTrue( row["absent_since"].Type == JTokenType.Null && ( bool ) row["is_banned"], "the platform holds her as a banned member, as the doors do" );
                Assert.AreEqual( TimeZoneInfo.ConvertTimeToUtc( until, RockDateTime.OrgTimeZoneInfo ), ParseInstant( ( string ) row["ban_expires_at"] ).UtcDateTime,
                    "until the archived row's ban ends" );

                var token = PersonToken( personId, configuration );
                var refused = CallAsPerson( configuration, token, "chat_get_history", channel );
                Assert.IsFalse( refused.IsSuccess, "the person added back reads the public room past the ban" );
                StringAssert.Contains( refused.Body, "authz.not_readable" );
            }
        }

        [TestMethod]
        public void APrivateRoomThatHidesItsRosterGivesAPlainMemberNoReachToItsPeopleAndItsLeaderReach()
        {
            var platform = LocalChatPlatform.FromEnvironment();

            using ( var fixture = new ChatSyncProjectionFixture() )
            using ( var recorder = new PushRecorder() )
            using ( var sync = ChatPlatformSyncHelper.OverrideImmediateSync( recorder ) )
            {
                var configuration = platform.ProvisionChurch();
                fixture.StoreConfiguration( configuration );

                var leaderRoleId = fixture.AddRole( fixture.SharedGroupTypeId, "Leader", true );
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Hidden roster room", g =>
                {
                    g.IsChatChannelPublicOverride = false;
                    g.CanViewMembersOverride = false;
                } );
                var annId = fixture.AddPerson( "Annreach" );
                var bobId = fixture.AddPerson( "Bobreach" );
                var leeId = fixture.AddPerson( "Leereach" );
                var ann = fixture.PrimaryAliasGuid( annId );

                // Ann has Open DM off, so only a room she shares can give anyone reach to her.
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.ExecuteSqlCommand( "UPDATE [Person] SET [IsChatOpenDirectMessageAllowed] = 0 WHERE [Id] = @p0", annId );
                }

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    AddMember( rockContext, channel, annId );
                    AddMember( rockContext, channel, bobId );
                    AddMember( rockContext, channel, leeId, leaderRoleId );
                    rockContext.SaveChanges();
                }

                Assert.IsTrue( sync.WaitForPushes( TimeSpan.FromSeconds( 10 ) ), "the memberships' push did not finish" );
                Assert.IsNotNull( platform.WaitForChannel( configuration.TenantId.Value, channel, r => r != null && !( bool ) r["can_view_members"] ),
                    "the platform never heard the room hides its roster" );
                Assert.IsNotNull( platform.WaitForMember( configuration.TenantId.Value, channel, fixture.PrimaryAliasGuid( leeId ), r => r != null && ( bool ) r["is_leader"] ),
                    "the platform never heard who leads the room" );
                Assert.IsNotNull( platform.WaitForAlias( configuration.TenantId.Value, ann, r => r != null && ( bool? ) r["is_open_dm_allowed"] == false ),
                    "the platform never heard Ann has Open DM off" );

                // The same three questions for each caller, through the real token exchange: the
                // people search, the card's Message, and Rock's own Start door.
                (bool Found, bool CanMessage, string Start) Ask( int callerId )
                {
                    var token = PersonToken( callerId, configuration );

                    var search = Post( configuration, "/rest/v1/rpc/chat_search_people", token, new JObject { ["p_query"] = "annreach" } );
                    Assert.IsTrue( search.IsSuccess, "the people search refused: " + search.Body );
                    var found = JObject.Parse( search.Body )["people"].Any( p => ( Guid ) p["person_alias_guid"] == ann );

                    var card = Post( configuration, "/rest/v1/rpc/chat_get_person", token,
                        new JObject { ["p_person_alias_guid"] = ann.ToString(), ["p_channel_id"] = channel.ToString() } );
                    Assert.IsTrue( card.IsSuccess, "the card refused: " + card.Body );
                    var canMessage = ( bool ) JObject.Parse( card.Body )["can_message"];

                    using ( ChatSyncProjectionFixture.InsideRequest() )
                    using ( var rockContext = new RockContext() )
                    {
                        var caller = new PersonService( rockContext ).Get( callerId );
                        var context = Rock.Communication.Chat.Platform.Session.ChatSessionHelper.BuildSessionContext( caller, configuration, rockContext );
                        var start = Rock.Communication.Chat.Platform.Doors.ChatDoorHelper
                            .StartDirectMessageAsync( caller, new[] { ann }, context, rockContext ).GetAwaiter().GetResult();

                        return (found, canMessage, start.Code);
                    }
                }

                // The plain member first, since the leader's start makes a conversation the two
                // would then carry on whatever the room says.
                var bob = Ask( bobId );
                Assert.IsFalse( bob.Found, "the search lists Ann to a plain member of a room that hides who is in it" );
                Assert.IsFalse( bob.CanMessage, "the card offers a plain member Message to Ann" );
                Assert.AreEqual( "door.target_not_eligible", bob.Start, "Rock's door lets a plain member start a conversation with Ann" );

                var lee = Ask( leeId );
                Assert.IsTrue( lee.Found, "the search hides Ann from the room's leader, who may see its roster" );
                Assert.IsTrue( lee.CanMessage, "the card refuses the leader Message to Ann" );
                Assert.AreEqual( "ok", lee.Start, "Rock's door refuses the leader a conversation with Ann" );
            }
        }

        #endregion Tests

        #region Support

        private static readonly HttpClient _http = new HttpClient();

        /// <summary>
        /// The membership's id, archived or not.
        /// </summary>
        private static int MemberId( Guid groupGuid, int personId )
        {
            using ( var rockContext = new RockContext() )
            {
                return new GroupMemberService( rockContext ).AsNoFilter()
                    .Where( m => m.Group.Guid == groupGuid && m.PersonId == personId )
                    .Select( m => m.Id )
                    .Single();
            }
        }

        /// <summary>
        /// Signs a person token with Rock's own code and exchanges it for the person's platform
        /// token, as the Chat block does.
        /// </summary>
        private static string PersonToken( int personId, Rock.Communication.Chat.Platform.Configuration.ChatPlatformConfiguration configuration )
        {
            using ( var rockContext = new RockContext() )
            {
                var person = new PersonService( rockContext ).Get( personId );
                var context = Rock.Communication.Chat.Platform.Session.ChatSessionHelper.BuildSessionContext( person, configuration, rockContext );
                var minted = Rock.Communication.Chat.Platform.Session.ChatSessionHelper.TryMintChurchToken( person, context, rockContext );
                Assert.IsNotNull( minted.ChurchToken, "the person passes the session gates: " + minted.Gate );

                var exchanged = Post( configuration, "/functions/v1/token-exchange", minted.ChurchToken, null );
                Assert.IsTrue( exchanged.IsSuccess, "the token exchange refused: " + exchanged.Body );

                return ( string ) JObject.Parse( exchanged.Body )["access_token"];
            }
        }

        /// <summary>
        /// Calls a platform function as the person, naming one room, as the browser does.
        /// </summary>
        private static (bool IsSuccess, string Body) CallAsPerson( Rock.Communication.Chat.Platform.Configuration.ChatPlatformConfiguration configuration, string token, string name, Guid channelGuid )
        {
            return Post( configuration, "/rest/v1/rpc/" + name, token, new JObject { ["p_channel_id"] = channelGuid.ToString() } );
        }

        private static (bool IsSuccess, string Body) Post( Rock.Communication.Chat.Platform.Configuration.ChatPlatformConfiguration configuration, string path, string bearer, JObject body )
        {
            using ( var request = new HttpRequestMessage( HttpMethod.Post, configuration.ProjectUrl.TrimEnd( '/' ) + path ) )
            {
                request.Headers.TryAddWithoutValidation( "apikey", configuration.PublishableKey );
                request.Headers.TryAddWithoutValidation( "Authorization", "Bearer " + bearer );
                request.Content = new StringContent( body?.ToString() ?? "{}", System.Text.Encoding.UTF8, "application/json" );

                using ( var response = _http.SendAsync( request ).GetAwaiter().GetResult() )
                {
                    return (response.IsSuccessStatusCode, response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                }
            }
        }

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
