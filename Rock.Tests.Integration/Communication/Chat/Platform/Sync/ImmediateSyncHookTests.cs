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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Contract;
using Rock.Communication.Chat.Platform.Sync;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Web.Cache;
using Rock.Workflow.Action;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The immediate sync as the save hooks drive it: what one save in Rock sends to the chat
    /// platform after it commits, against a stand-in for the platform.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A person added to or removed from a chat group anywhere in Rock should see it in chat
    ///         at once, not at the next full sync. So every save that touches a chat person, channel
    ///         or membership pushes what it touched, read back through the one projection after the
    ///         commit and sent in the background, never inside the save.
    ///     </para>
    ///     <para>
    ///         These tests hold the push to one per save, to nothing for a save that rolled back or
    ///         that no web request made, and to the Chat Ban List from every path, since a ban a
    ///         workflow makes is still a ban. They wait on the pushes a save began rather than on a
    ///         clock, through the override that stands in for the transport, so a push that never
    ///         began and one that has not finished yet cannot be confused.
    ///     </para>
    /// </remarks>
    [TestClass]
    public class ImmediateSyncHookTests : DatabaseTestsBase
    {
        #region Constants

        private const string ProjectUrl = "https://example.supabase.co";

        private const string PublishableKey = "sb_publishable_test";

        private const string ExchangedToken = "exchanged.platform.token";

        private const string ExchangePath = "/functions/v1/token-exchange";

        private const string PushPath = "/rest/v1/rpc/sync_push";

        // Far longer than any push against a stand-in takes, so a test that times out here has found
        // a push that never ended rather than a slow one.
        private static readonly TimeSpan PushWait = TimeSpan.FromSeconds( 30 );

        // The form the platform accepts, and the one the full sync already sends: UTC, microseconds.
        private static readonly Regex ReadTimeForm = new Regex( @"^\d{4}-\d\d-\d\dT\d\d:\d\d:\d\d\.\d{6}Z$" );

        #endregion Constants

        #region One push per save

        [TestMethod]
        public void OneSaveOfThreeMembersAndAChatPersonsNickNameMakesExactlyOnePush()
        {
            using ( var scene = new Scene() )
            {
                var fixture = scene.Fixture;
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "One push channel" );
                var otherChannel = fixture.AddChannel( fixture.SharedGroupTypeId, "Where the chat person already is" );
                var memberIds = new[] { fixture.AddPerson( "Ada" ), fixture.AddPerson( "Bo" ), fixture.AddPerson( "Cy" ) };
                var chatPersonId = fixture.AddPerson( "Dee" );
                fixture.AddMember( otherChannel, chatPersonId );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    AddMembers( rockContext, channel, memberIds );
                    new PersonService( rockContext ).Get( chatPersonId ).NickName = "Deedee";
                    rockContext.SaveChanges();
                }

                var pushes = scene.WaitForPushes();

                Assert.AreEqual( 1, pushes.Count, "one save is one push, however many rows it touched" );

                var push = pushes[0];
                AssertIsAPush( push );

                var memberAliases = memberIds.Select( fixture.PrimaryAliasGuid ).ToList();
                var chatPersonAlias = fixture.PrimaryAliasGuid( chatPersonId );

                CollectionAssert.AreEquivalent(
                    memberAliases.Select( a => channel + " " + a ).ToList(),
                    Rows( push.Json, "members" ).Select( r => ( Guid ) r[0] + " " + ( Guid ) r[1] ).ToList(),
                    "the three memberships the save added" );
                CollectionAssert.AreEquivalent( new[] { channel }, Keys( push.Json, "channels" ),
                    "with their channel, so the platform can take them into a mirror that has never seen it" );
                CollectionAssert.AreEquivalent( memberAliases.Concat( new[] { chatPersonAlias } ).ToList(), Keys( push.Json, "aliases" ),
                    "and the four alias rows: the three members and the chat person whose name changed" );

                var chatPersonRow = Row( push.Json, "aliases", chatPersonAlias );
                Assert.AreEqual( "Deedee", ( string ) chatPersonRow[ColumnIndex( "aliases", "nick_name" )],
                    "the alias row carries the name as the save committed it" );
                Assert.AreEqual( 0, Rows( push.Json, "badges" ).Count, "a push never carries badges" );
                Assert.AreEqual( 0, Absent( push.Json, "channels" ).Count );
                Assert.AreEqual( 0, Absent( push.Json, "members" ).Count );
            }
        }

        [TestMethod]
        public void ASecondSaveOnTheSameContextMakesASecondPushUnderTheSameToken()
        {
            using ( var scene = new Scene() )
            {
                var fixture = scene.Fixture;
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Two saves channel" );
                var adaId = fixture.AddPerson( "Ada" );
                var boId = fixture.AddPerson( "Bo" );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    AddMembers( rockContext, channel, adaId );
                    rockContext.SaveChanges();

                    AddMembers( rockContext, channel, boId );
                    rockContext.SaveChanges();
                }

                var pushes = scene.WaitForPushes();

                Assert.AreEqual( 2, pushes.Count, "each save that commits is its own push" );
                pushes.ForEach( AssertIsAPush );

                var pushedAliases = pushes.Select( p => Rows( p.Json, "members" ).Select( r => ( Guid ) r[1] ).Single() ).ToList();
                CollectionAssert.AreEquivalent( new[] { fixture.PrimaryAliasGuid( adaId ), fixture.PrimaryAliasGuid( boId ) }, pushedAliases,
                    "and each carries only what its own save touched" );
                Assert.AreEqual( 1, scene.Platform.Exchanges,
                    "the platform token is kept for the process, so the second push does not sign in again" );
            }
        }

        [TestMethod]
        public void ASaveThatRollsBackPushesNothing()
        {
            using ( var scene = new Scene() )
            {
                var fixture = scene.Fixture;
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Rolled back channel" );
                var adaId = fixture.AddPerson( "Ada" );
                var boId = fixture.AddPerson( "Bo" );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    rockContext.WrapTransactionIf( () =>
                    {
                        AddMembers( rockContext, channel, adaId );
                        rockContext.SaveChanges();

                        return false;
                    } );
                }

                Assert.AreEqual( 0, scene.WaitForPushes().Count, "a save that rolled back changed nothing the platform could be told" );

                // The same kind of save committed, so the silence above is the rollback's and not a
                // push that could never have been made.
                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    AddMembers( rockContext, channel, boId );
                    rockContext.SaveChanges();
                }

                var pushes = scene.WaitForPushes();

                Assert.AreEqual( 1, pushes.Count, "the committed save pushes" );
                CollectionAssert.AreEquivalent( new[] { fixture.PrimaryAliasGuid( boId ) },
                    Rows( pushes[0].Json, "members" ).Select( r => ( Guid ) r[1] ).ToList(),
                    "and carries only its own membership" );
            }
        }

        #endregion One push per save

        #region Where a save came from

        [TestMethod]
        public void ASaveOutsideARequestPushesNothingButABanListAddStillPushes()
        {
            using ( var scene = new Scene() )
            {
                var fixture = scene.Fixture;
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Job channel" );
                var adaId = fixture.AddPerson( "Ada" );
                var boId = fixture.AddPerson( "Bo" );

                // Saved as a job, a bus consumer or a migration saves: with no request around it.
                using ( var rockContext = new RockContext() )
                {
                    AddMembers( rockContext, channel, adaId );
                    rockContext.SaveChanges();
                }

                Assert.AreEqual( 0, scene.WaitForPushes().Count,
                    "a save no request made is left to the full sync, so a bulk job cannot flood the platform" );

                using ( var rockContext = new RockContext() )
                {
                    AddMembers( rockContext, Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST.AsGuid(), boId );
                    rockContext.SaveChanges();
                }

                var pushes = scene.WaitForPushes();

                Assert.AreEqual( 1, pushes.Count, "a ban is pushed from every path, a job's included" );
                AssertIsAPush( pushes[0] );

                var boAlias = Row( pushes[0].Json, "aliases", fixture.PrimaryAliasGuid( boId ) );
                Assert.IsNotNull( boAlias, "the banned person's alias row is what carries the ban" );
                Assert.AreEqual( true, ( bool ) boAlias[ColumnIndex( "aliases", "is_globally_banned" )] );
                Assert.AreEqual( 0, Rows( pushes[0].Json, "channels" ).Count, "the ban list is never a channel" );
                Assert.AreEqual( 0, Rows( pushes[0].Json, "members" ).Count );
            }
        }

        #endregion Where a save came from

        #region Absence

        [TestMethod]
        public void RemovingAMemberByDeleteOrByStatusPushesEachPairAbsent()
        {
            using ( var scene = new Scene() )
            {
                var fixture = scene.Fixture;
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Removal channel" );
                var adaId = fixture.AddPerson( "Ada" );
                var boId = fixture.AddPerson( "Bo" );
                fixture.AddMember( channel, adaId );
                fixture.AddMember( channel, boId );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    var service = new GroupMemberService( rockContext );
                    var members = service.Queryable().Where( m => m.Group.Guid == channel ).ToList();

                    service.DeleteRange( members.Where( m => m.PersonId == adaId ).ToList() );
                    members.Single( m => m.PersonId == boId ).GroupMemberStatus = GroupMemberStatus.Inactive;

                    rockContext.SaveChanges();
                }

                var pushes = scene.WaitForPushes();

                Assert.AreEqual( 1, pushes.Count );
                AssertIsAPush( pushes[0] );
                Assert.AreEqual( 0, Rows( pushes[0].Json, "members" ).Count, "neither membership is live" );
                CollectionAssert.AreEquivalent(
                    new[] { channel + " " + fixture.PrimaryAliasGuid( adaId ), channel + " " + fixture.PrimaryAliasGuid( boId ) },
                    Absent( pushes[0].Json, "members" ).Select( k => ( Guid ) k[0] + " " + ( Guid ) k[1] ).ToList(),
                    "a deleted membership and an inactive one are both named absent, since the platform stamps only what a push names" );
            }
        }

        [TestMethod]
        public void ArchivingAChannelPushesItsFormerMembersAbsentAndKeepsItsRow()
        {
            using ( var scene = new Scene() )
            {
                var fixture = scene.Fixture;
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Archived channel" );
                var adaId = fixture.AddPerson( "Ada" );
                var boId = fixture.AddPerson( "Bo" );
                fixture.AddMember( channel, adaId );
                fixture.AddMember( channel, boId );
                fixture.MarkChannelDirectly( channel );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    new GroupService( rockContext ).Queryable().Single( g => g.Guid == channel ).IsArchived = true;
                    rockContext.SaveChanges();
                }

                var pushes = scene.WaitForPushes();

                Assert.AreEqual( 1, pushes.Count );
                AssertIsAPush( pushes[0] );

                // An archived channel goes quiet rather than away, so its conversation survives an
                // unarchive; the platform refuses a key both carried and absent.
                CollectionAssert.AreEquivalent( new[] { channel }, Keys( pushes[0].Json, "channels" ),
                    "an archived channel keeps its row" );
                Assert.AreEqual( 0, Absent( pushes[0].Json, "channels" ).Count );
                CollectionAssert.AreEquivalent(
                    new[] { fixture.PrimaryAliasGuid( adaId ), fixture.PrimaryAliasGuid( boId ) },
                    Absent( pushes[0].Json, "members" ).Where( k => ( Guid ) k[0] == channel ).Select( k => ( Guid ) k[1] ).ToList(),
                    "and every one of its members is named absent" );
            }
        }

        [TestMethod]
        public void RenamingAChatGroupPushesItsChannelRowAndNoMemberships()
        {
            using ( var scene = new Scene() )
            {
                var fixture = scene.Fixture;
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Before the rename" );
                fixture.AddMember( channel, fixture.AddPerson( "Ada" ) );
                fixture.AddMember( channel, fixture.AddPerson( "Bo" ) );
                fixture.MarkChannelDirectly( channel );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    new GroupService( rockContext ).Queryable().Single( g => g.Guid == channel ).Name = "After the rename";
                    rockContext.SaveChanges();
                }

                var pushes = scene.WaitForPushes();

                Assert.AreEqual( 1, pushes.Count );
                AssertIsAPush( pushes[0] );

                // A group of thousands would otherwise push every member on a rename, past the
                // push's row ceiling, and wait for the full sync to show the new name.
                CollectionAssert.AreEquivalent( new[] { channel }, Keys( pushes[0].Json, "channels" ), "the renamed channel's row" );
                Assert.AreEqual( "After the rename", ( string ) Row( pushes[0].Json, "channels", channel )[ColumnIndex( "channels", "name" )] );
                Assert.AreEqual( 0, Rows( pushes[0].Json, "members" ).Count, "a rename cannot change who is in the channel, so no membership is read" );
                Assert.AreEqual( 0, Rows( pushes[0].Json, "aliases" ).Count, "and no member's alias with it" );
                Assert.AreEqual( 0, Absent( pushes[0].Json, "members" ).Count, "and no member is named absent" );
                Assert.AreEqual( 0, Absent( pushes[0].Json, "channels" ).Count );
            }
        }

        [TestMethod]
        public void TurningChatOnForAGroupPushesItsMembers()
        {
            using ( var scene = new Scene() )
            {
                var fixture = scene.Fixture;
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Chat turned on", group => group.IsChatEnabledOverride = false );
                var adaId = fixture.AddPerson( "Ada" );
                var boId = fixture.AddPerson( "Bo" );
                fixture.AddMember( channel, adaId );
                fixture.AddMember( channel, boId );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    new GroupService( rockContext ).Queryable().Single( g => g.Guid == channel ).IsChatEnabledOverride = true;
                    rockContext.SaveChanges();
                }

                var pushes = scene.WaitForPushes();

                Assert.AreEqual( 1, pushes.Count );
                AssertIsAPush( pushes[0] );
                CollectionAssert.AreEquivalent( new[] { channel }, Keys( pushes[0].Json, "channels" ), "the group is a channel now" );
                CollectionAssert.AreEquivalent(
                    new[] { fixture.PrimaryAliasGuid( adaId ), fixture.PrimaryAliasGuid( boId ) },
                    Rows( pushes[0].Json, "members" ).Where( r => ( Guid ) r[0] == channel ).Select( r => ( Guid ) r[1] ).ToList(),
                    "and every one of its members is in it, which no membership save told the platform" );
            }
        }

        [TestMethod]
        public void DeletingAChannelPushesItUnderAbsentChannels()
        {
            using ( var scene = new Scene() )
            {
                var fixture = scene.Fixture;
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Deleted channel" );
                fixture.MarkChannelDirectly( channel );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    var service = new GroupService( rockContext );
                    Assert.IsTrue( service.Delete( service.Queryable().Single( g => g.Guid == channel ) ), "the fixture's group can be deleted" );
                    rockContext.SaveChanges();
                }

                var pushes = scene.WaitForPushes();

                Assert.AreEqual( 1, pushes.Count );
                AssertIsAPush( pushes[0] );
                Assert.AreEqual( 0, Rows( pushes[0].Json, "channels" ).Count );
                CollectionAssert.AreEquivalent( new[] { channel }, Absent( pushes[0].Json, "channels" ).Select( k => ( Guid ) k ).ToList(),
                    "a deleted group is named absent by the guid its save recorded while it still had one" );
            }
        }

        #endregion Absence

        #region Awaited

        [TestMethod]
        public void AnAwaitedPushThePlatformDoesNotAnswerIsPendingWithinTheBudget()
        {
            using ( var scene = new Scene() )
            {
                scene.Platform.PushDelay = TimeSpan.FromSeconds( 5 );

                var (outcome, elapsed) = SaveAndFlush( scene );

                Assert.AreEqual( ChatPlatformSyncHelper.PushOutcome.Pending, outcome,
                    "a platform that has not answered is not an applied change" );
                Assert.IsTrue( elapsed < ChatPlatformSyncHelper.AwaitedPushBudget + TimeSpan.FromMilliseconds( 200 ),
                    $"a person waiting on a chat action is answered inside the budget, not when the platform gets round to it; took {elapsed.TotalMilliseconds:F0} ms" );
            }
        }

        [TestMethod]
        public void AnAwaitedPushThePlatformRefusesIsPendingAtOnce()
        {
            using ( var scene = new Scene() )
            {
                scene.Platform.PushStatus = HttpStatusCode.InternalServerError;

                var (outcome, elapsed) = SaveAndFlush( scene );

                Assert.AreEqual( ChatPlatformSyncHelper.PushOutcome.Pending, outcome, "a refused push leaves the change to the full sync" );
                Assert.IsTrue( elapsed < TimeSpan.FromSeconds( 1 ),
                    $"a refusal is known the moment it arrives, so nothing waits out the budget; took {elapsed.TotalMilliseconds:F0} ms" );
                Assert.AreEqual( 1, scene.Platform.Pushes.Count, "and it is not sent again: the full sync repairs it" );
            }
        }

        [TestMethod]
        public void AnAwaitedPushThePlatformTakesIsApplied()
        {
            using ( var scene = new Scene() )
            {
                var (outcome, _) = SaveAndFlush( scene );

                Assert.AreEqual( ChatPlatformSyncHelper.PushOutcome.Applied, outcome );
                Assert.AreEqual( 1, scene.Platform.Pushes.Count, "the awaited push is the save's own push, not a second one" );
                AssertIsAPush( scene.Platform.Pushes[0] );
            }
        }

        #endregion Awaited

        #region Ceiling

        [TestMethod]
        public void APushPastTheRowCeilingSendsNothingAndWarnsOnce()
        {
            using ( var scene = new Scene() )
            {
                var fixture = scene.Fixture;
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Bulk channel" );
                var templateId = fixture.AddPerson( "Template" );
                fixture.AddMember( channel, templateId );

                // Each member is a membership row and an alias row, and the channel is one more, so
                // this many members is one row past the ceiling. Written past the hooks, as a bulk
                // import would, so only the group's own save below reaches the immediate sync.
                var members = ChatPlatformSyncHelper.PushRowCeiling / 2;
                fixture.SeedMembersDirectly( channel, templateId, members - 1 );

                var scope = new ChatPlatformSyncHelper.ImmediateChanges();
                scope.GroupGuids.Add( channel );
                Assert.AreEqual( ChatPlatformSyncHelper.PushRowCeiling + 1, fixture.ProjectChanges( scope ).RowCount,
                    "the group's save projects one row more than the ceiling" );

                // A save that turns chat on for the group, which could change who is in it, so the
                // push reads the whole membership; a rename alone reads only the channel row.
                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    new GroupService( rockContext ).Queryable().Single( g => g.Guid == channel ).IsChatEnabledOverride = true;
                    rockContext.SaveChanges();
                }

                Assert.AreEqual( 0, scene.WaitForPushes().Count, "a push that large is left to the full sync" );
                Assert.AreEqual( 1, scene.Override.Warnings.Count,
                    "and says so once, since a push past the ceiling means a bulk write reached the hooks" );
            }
        }

        #endregion Ceiling

        #region Paths past the hooks

        [TestMethod]
        public void APersonMergeThroughTheBlocksPathPushesTheMergedAwayAliasesUnderTheSurvivor()
        {
            using ( var scene = new Scene() )
            {
                var fixture = scene.Fixture;
                var survivorChannel = fixture.AddChannel( fixture.SharedGroupTypeId, "Survivor channel" );
                var loserChannel = fixture.AddChannel( fixture.SharedGroupTypeId, "Merged away channel" );
                var survivorId = fixture.AddPerson( "Survivor" );
                var loserId = fixture.AddPerson( "Merged" );
                fixture.AddMember( survivorChannel, survivorId );
                fixture.AddMember( loserChannel, loserId );

                var survivorAlias = fixture.PrimaryAliasGuid( survivorId );
                var loserAlias = fixture.PrimaryAliasGuid( loserId );

                // The statement the Person Merge block runs inside its transaction, and the one call
                // it makes after, because the procedure moves aliases and memberships where no save
                // hook sees them.
                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.Database.ExecuteSqlCommand( "EXEC dbo.spCrm_PersonMerge @p0, @p1", loserId, survivorId );
                        ChatPlatformSyncHelper.RecordPersonChange( rockContext, survivorId );
                    } );
                }

                var pushes = scene.WaitForPushes();

                Assert.AreEqual( 1, pushes.Count, "the merge pushes once, after it commits" );
                AssertIsAPush( pushes[0] );

                var mergedAway = Row( pushes[0].Json, "aliases", loserAlias );
                Assert.IsNotNull( mergedAway, "the merged-away alias is pushed, so a client holding it resolves to the survivor" );
                Assert.AreEqual( survivorAlias, ( Guid ) mergedAway[ColumnIndex( "aliases", "primary_person_alias_guid" )],
                    "and it now names the survivor as its person" );
                Assert.IsTrue(
                    Rows( pushes[0].Json, "members" ).Any( r => ( Guid ) r[0] == loserChannel && ( Guid ) r[1] == survivorAlias ),
                    "the membership the merge moved to the survivor is pushed under the survivor's alias" );
            }
        }

        [TestMethod]
        public void TheAddPersonToGroupWorkflowActionOnTheBanListPushesTheBanFromOutsideARequest()
        {
            using ( var scene = new Scene() )
            {
                var fixture = scene.Fixture;
                var personId = fixture.AddPerson( "Workflow" );
                var banListGuid = Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST.AsGuid();

                // Run as the job engine runs a workflow: with no request around it.
                RunAddPersonToGroup( fixture, banListGuid, personId );

                using ( var rockContext = new RockContext() )
                {
                    Assert.IsTrue(
                        new GroupMemberService( rockContext ).Queryable().Any( m => m.Group.Guid == banListGuid && m.PersonId == personId ),
                        "the workflow put the person on the ban list" );
                }

                var pushes = scene.WaitForPushes();

                Assert.AreEqual( 1, pushes.Count, "a ban a workflow makes is pushed like any other" );
                AssertIsAPush( pushes[0] );

                var alias = Row( pushes[0].Json, "aliases", fixture.PrimaryAliasGuid( personId ) );
                Assert.IsNotNull( alias );
                Assert.AreEqual( true, ( bool ) alias[ColumnIndex( "aliases", "is_globally_banned" )] );
            }
        }

        #endregion Paths past the hooks

        #region Support

        /// <summary>
        /// Adds a new member to a chat group inside a request and waits on that save's push.
        /// </summary>
        private static (ChatPlatformSyncHelper.PushOutcome Outcome, TimeSpan Elapsed) SaveAndFlush( Scene scene )
        {
            var fixture = scene.Fixture;
            var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Awaited channel" );
            var personId = fixture.AddPerson( "Awaited" );

            using ( ChatSyncProjectionFixture.InsideRequest() )
            using ( var rockContext = new RockContext() )
            {
                AddMembers( rockContext, channel, personId );
                rockContext.SaveChanges();

                var stopwatch = Stopwatch.StartNew();
                var outcome = ChatPlatformSyncHelper.FlushAsync( rockContext ).GetAwaiter().GetResult();
                stopwatch.Stop();

                return (outcome, stopwatch.Elapsed);
            }
        }

        /// <summary>
        /// Adds people to a group in the caller's context, unsaved, with the group type's default role.
        /// </summary>
        private static void AddMembers( RockContext rockContext, Guid groupGuid, params int[] personIds )
        {
            var group = new GroupService( rockContext ).Queryable( "GroupType" ).Single( g => g.Guid == groupGuid );
            var service = new GroupMemberService( rockContext );

            foreach ( var personId in personIds )
            {
                service.Add( new GroupMember
                {
                    Guid = Guid.NewGuid(),
                    GroupId = group.Id,
                    GroupTypeId = group.GroupTypeId,
                    PersonId = personId,
                    GroupRoleId = group.GroupType.DefaultGroupRoleId.Value,
                    GroupMemberStatus = GroupMemberStatus.Active
                } );
            }
        }

        /// <summary>
        /// Runs a workflow of one Group Member Add action through the workflow engine, as a
        /// workflow the job engine starts would run, and takes the workflow type away after.
        /// </summary>
        private static void RunAddPersonToGroup( ChatSyncProjectionFixture fixture, Guid groupGuid, int personId )
        {
            int workflowTypeId;
            Guid workflowTypeGuid;

            using ( var rockContext = new RockContext() )
            {
                var workflowType = new WorkflowType
                {
                    Guid = Guid.NewGuid(),
                    Name = "Chat ban list add " + Guid.NewGuid().ToString( "N" ).Substring( 0, 8 ),
                    IsActive = true,
                    IsPersisted = false,
                    WorkTerm = "Ban",
                    CategoryId = CategoryCache.GetId( Rock.SystemGuid.Category.WORKFLOW_TYPE_SAMPLES.AsGuid() ),
                    ForeignKey = fixture.ForeignKey
                };

                new WorkflowTypeService( rockContext ).Add( workflowType );
                rockContext.SaveChanges();

                workflowTypeId = workflowType.Id;
                workflowTypeGuid = workflowType.Guid;

                var personAttribute = new Rock.Model.Attribute
                {
                    Guid = Guid.NewGuid(),
                    FieldTypeId = FieldTypeCache.GetId( Rock.SystemGuid.FieldType.PERSON.AsGuid() ).Value,
                    EntityTypeId = EntityTypeCache.GetId<Rock.Model.Workflow>(),
                    EntityTypeQualifierColumn = "WorkflowTypeId",
                    EntityTypeQualifierValue = workflowType.Id.ToString(),
                    Key = "Person",
                    Name = "Person",
                    ForeignKey = fixture.ForeignKey
                };

                new AttributeService( rockContext ).Add( personAttribute );

                var activityType = new WorkflowActivityType { Name = "Start", IsActive = true, IsActivatedWithWorkflow = true };
                workflowType.ActivityTypes.Add( activityType );

                var actionType = new WorkflowActionType
                {
                    Name = "Add to the ban list",
                    EntityTypeId = EntityTypeCache.GetId<AddPersonToGroup>().Value,
                    IsActionCompletedOnSuccess = true,
                    IsActivityCompletedOnSuccess = true
                };
                activityType.ActionTypes.Add( actionType );

                rockContext.SaveChanges();

                // The action's own settings, registered the way Rock registers every action's.
                Rock.Attribute.Helper.UpdateAttributes(
                    typeof( AddPersonToGroup ),
                    EntityTypeCache.GetId<WorkflowActionType>(),
                    "EntityTypeId",
                    EntityTypeCache.GetId<AddPersonToGroup>().ToString(),
                    rockContext );

                var group = new GroupService( rockContext ).Queryable( "GroupType" ).Single( g => g.Guid == groupGuid );

                actionType.LoadAttributes( rockContext );
                actionType.SetAttributeValue( "Person", personAttribute.Guid.ToString() );
                actionType.SetAttributeValue( "GroupAndRole", group.GroupType.Guid + "|" + group.Guid );
                actionType.SetAttributeValue( "GroupMemberStatus", ( ( int ) GroupMemberStatus.Active ).ToString() );
                actionType.SetAttributeValue( "UpdateExisting", "False" );
                actionType.SaveAttributeValues( rockContext );
            }

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    var workflow = Rock.Model.Workflow.Activate( WorkflowTypeCache.Get( workflowTypeGuid ), "Ban", rockContext );
                    workflow.SetAttributeValue( "Person", fixture.PrimaryAliasGuid( personId ).ToString() );

                    var isProcessed = new WorkflowService( rockContext ).Process( workflow, out var messages );
                    Assert.IsTrue( isProcessed, "the workflow ran: " + string.Join( "; ", messages ) );
                }
            }
            finally
            {
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.ExecuteSqlCommand(
                        "DELETE FROM [AttributeValue] WHERE [EntityId] IN ( SELECT [WAT].[Id] FROM [WorkflowActionType] AS [WAT] "
                        + "INNER JOIN [WorkflowActivityType] AS [WACT] ON [WACT].[Id] = [WAT].[ActivityTypeId] WHERE [WACT].[WorkflowTypeId] = @p0 ) "
                        + "AND [AttributeId] IN ( SELECT [Id] FROM [Attribute] WHERE [EntityTypeId] = @p1 );"
                        + "DELETE FROM [Attribute] WHERE [ForeignKey] = @p2;"
                        + "DELETE FROM [WorkflowType] WHERE [Id] = @p0;",
                        workflowTypeId,
                        EntityTypeCache.GetId<WorkflowActionType>(),
                        fixture.ForeignKey );
                }
            }
        }

        /// <summary>
        /// Everything a push request must carry to be taken: the method, the address, a JSON body,
        /// the read time and contract headers, and the token the exchange granted, never the
        /// church's own.
        /// </summary>
        private static void AssertIsAPush( RecordedRequest push )
        {
            Assert.AreEqual( HttpMethod.Post, push.Method );
            Assert.AreEqual( PushPath, push.Path );
            Assert.AreEqual( "application/json", push.ContentType, "a push is JSON, so the platform parses it on the way in" );
            Assert.IsNotNull( push.ReadAt, "a push carries the moment its rows were read at" );
            Assert.IsTrue( ReadTimeForm.IsMatch( push.ReadAt ), $"the read time is UTC to the microsecond: {push.ReadAt}" );
            Assert.AreEqual( ChatWireContract.ComputedHash, push.Contract, "a push names the contract its rows were written by" );
            Assert.AreEqual( ExchangedToken, push.Bearer, "a push carries the token the exchange granted, never the church's own" );
            Assert.AreEqual( PublishableKey, push.ApiKey );
            Assert.IsNotNull( push.Json, "a push body is a JSON object" );
            CollectionAssert.AreEquivalent( new[] { "aliases", "channels", "members", "badges", "absent" },
                push.Json.Properties().Select( p => p.Name ).ToList(), "the four sections and the keys no longer in chat" );
        }

        private static List<JArray> Rows( JObject body, string section )
        {
            return ( ( JArray ) body[section] ).Cast<JArray>().ToList();
        }

        /// <summary>
        /// The first column of every row of a section, which is its key.
        /// </summary>
        private static List<Guid> Keys( JObject body, string section )
        {
            return Rows( body, section ).Select( r => ( Guid ) r[0] ).ToList();
        }

        private static JArray Row( JObject body, string section, Guid key )
        {
            return Rows( body, section ).FirstOrDefault( r => ( Guid ) r[0] == key );
        }

        private static List<JToken> Absent( JObject body, string kind )
        {
            return ( ( JArray ) body["absent"][kind] ).ToList();
        }

        /// <summary>
        /// Where a section's column sits in its rows, by the contract's own column lists.
        /// </summary>
        private static int ColumnIndex( string section, string column )
        {
            var contract = JObject.Parse( ChatWireContract.Json );
            var position = contract["payload"]["sections"].Select( v => ( string ) v ).ToList().IndexOf( section );

            return contract["tables"][position]["columns"].Select( c => ( string ) c ).ToList().IndexOf( column );
        }

        private static ChatPlatformConfiguration Configuration()
        {
            const string kid = "kid-immediate-test";

            return new ChatPlatformConfiguration
            {
                TenantId = Guid.NewGuid(),
                ProjectUrl = ProjectUrl,
                PublishableKey = PublishableKey,
                Kid = kid,
                PrivateKey = ChatSyncProjectionFixture.CreateSigningKey( kid ).PrivateJwk,
                AreChatProfilesVisible = true,
                IsOpenDirectMessagingAllowed = true,
                ChatBadgeDataViewGuids = new List<Guid>()
            };
        }

        /// <summary>
        /// A church with chat set up, a stand-in platform, and the immediate sync sending to it.
        /// </summary>
        private sealed class Scene : IDisposable
        {
            public Scene()
            {
                Fixture = new ChatSyncProjectionFixture();
                Fixture.StoreConfiguration( Configuration() );
                Platform = new PlatformStandIn();
                Override = ChatPlatformSyncHelper.OverrideImmediateSync( Platform );
            }

            public ChatSyncProjectionFixture Fixture { get; }

            public PlatformStandIn Platform { get; }

            public ChatPlatformSyncHelper.ImmediateSyncOverride Override { get; }

            /// <summary>
            /// Waits for every push begun so far and returns every push the platform has received.
            /// </summary>
            public List<RecordedRequest> WaitForPushes()
            {
                Assert.IsTrue( Override.WaitForPushes( PushWait ), "a push was still running when its wait ran out" );

                return Platform.Pushes.ToList();
            }

            public void Dispose()
            {
                // A push still in flight would land in the next test's stand-in.
                Override.WaitForPushes( PushWait );
                Override.Dispose();
                Platform.Dispose();
                Fixture.Dispose();
            }
        }

        /// <summary>
        /// Answers the exchange and the push the way the platform would, and records every request.
        /// </summary>
        private sealed class PlatformStandIn : HttpMessageHandler
        {
            private readonly object _sync = new object();

            private readonly List<RecordedRequest> _requests = new List<RecordedRequest>();

            /// <summary>
            /// How the push is answered.
            /// </summary>
            public HttpStatusCode PushStatus { get; set; } = HttpStatusCode.OK;

            /// <summary>
            /// How long the push takes to be answered.
            /// </summary>
            public TimeSpan PushDelay { get; set; } = TimeSpan.Zero;

            public List<RecordedRequest> Pushes
            {
                get
                {
                    lock ( _sync )
                    {
                        return _requests.Where( r => r.Path == PushPath ).ToList();
                    }
                }
            }

            public int Exchanges
            {
                get
                {
                    lock ( _sync )
                    {
                        return _requests.Count( r => r.Path == ExchangePath );
                    }
                }
            }

            protected override async Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
            {
                var body = request.Content == null ? null : await request.Content.ReadAsStringAsync();
                var recorded = RecordedRequest.From( request, body );

                lock ( _sync )
                {
                    _requests.Add( recorded );
                }

                if ( recorded.Path == ExchangePath )
                {
                    return Answer( HttpStatusCode.OK, "{\"access_token\":\"" + ExchangedToken + "\",\"token_type\":\"bearer\",\"expires_in\":300}" );
                }

                if ( recorded.Path != PushPath )
                {
                    return new HttpResponseMessage( HttpStatusCode.NotFound );
                }

                if ( PushDelay > TimeSpan.Zero )
                {
                    await Task.Delay( PushDelay, cancellationToken );
                }

                if ( PushStatus != HttpStatusCode.OK )
                {
                    return Answer( PushStatus, "{\"code\":\"XX000\",\"message\":\"the stand-in refused\",\"details\":null,\"hint\":null}" );
                }

                var counters = "{\"inserted\":0,\"updated\":0,\"suppressed\":0,\"skipped_stale\":0,\"absent\":0}";

                return Answer( HttpStatusCode.OK, "{\"aliases\":" + counters + ",\"channels\":" + counters + ",\"members\":" + counters + "}" );
            }

            private static HttpResponseMessage Answer( HttpStatusCode status, string json )
            {
                return new HttpResponseMessage( status )
                {
                    Content = new StringContent( json, Encoding.UTF8, "application/json" )
                };
            }
        }

        /// <summary>
        /// One request as the stand-in saw it.
        /// </summary>
        private sealed class RecordedRequest
        {
            public HttpMethod Method { get; private set; }

            public string Path { get; private set; }

            public string ContentType { get; private set; }

            public string Bearer { get; private set; }

            public string ApiKey { get; private set; }

            public string ReadAt { get; private set; }

            public string Contract { get; private set; }

            /// <summary>
            /// The body as JSON with its dates left as written, or null where it is not JSON.
            /// </summary>
            public JObject Json { get; private set; }

            public static RecordedRequest From( HttpRequestMessage request, string body )
            {
                var authorization = request.Headers.Authorization;
                JObject json = null;

                if ( body.IsNotNullOrWhiteSpace() )
                {
                    try
                    {
                        using ( var reader = new JsonTextReader( new StringReader( body ) ) { DateParseHandling = DateParseHandling.None } )
                        {
                            json = JObject.Load( reader );
                        }
                    }
                    catch ( JsonException )
                    {
                        json = null;
                    }
                }

                return new RecordedRequest
                {
                    Method = request.Method,
                    Path = request.RequestUri.AbsolutePath,
                    ContentType = request.Content?.Headers.ContentType?.MediaType,
                    Bearer = authorization != null && authorization.Scheme == "Bearer" ? authorization.Parameter : null,
                    ApiKey = Header( request, "apikey" ),
                    ReadAt = Header( request, "x-sync-read-at" ),
                    Contract = Header( request, "x-sync-contract" ),
                    Json = json
                };
            }

            private static string Header( HttpRequestMessage request, string name )
            {
                return request.Headers.TryGetValues( name, out var values ) ? values.FirstOrDefault() : null;
            }
        }

        #endregion Support
    }
}
