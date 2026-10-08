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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;
using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Doors
{
    /// <summary>
    /// The doors that change who is in a room after it exists: joining a public or pinned channel,
    /// leaving, a manager adding and removing people, and a group conversation's members adding,
    /// removing, leaving and renaming.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Each door is handed a channel by the browser, so each first decides from Rock that the
    ///         group is a chat channel of the kind the door is for, and that the caller may do this
    ///         to it, before anything is written. Then it changes Rock, waits for the platform to take
    ///         the change, and posts the line the room shows for it: a join and an add everywhere,
    ///         and in a group conversation a leave, a remove and a rename too.
    ///     </para>
    ///     <para>
    ///         These tests run against a stand-in for the chat platform, which records every push
    ///         and every line, and answers at once unless a test slows it or fails its posts.
    ///     </para>
    /// </remarks>
    [TestClass]
    public class ChatMembershipDoorTests : DatabaseTestsBase
    {
        #region What crosses the doors

        [TestMethod]
        public void EachMembershipActionTakesTheChannelAndOnlyWhatItNeeds()
        {
            var block = typeof( Rock.Blocks.Communication.Chat.ChatShell );
            var expected = new Dictionary<string, (string Name, Type Type)[]>
            {
                ["JoinChannel"] = new[] { ( "channelGuid", typeof( Guid ) ) },
                ["LeaveChannel"] = new[] { ( "channelGuid", typeof( Guid ) ) },
                ["AddMembers"] = new[] { ( "channelGuid", typeof( Guid ) ), ( "personAliasGuids", typeof( List<Guid> ) ) },
                ["RemoveMember"] = new[] { ( "channelGuid", typeof( Guid ) ), ( "personAliasGuid", typeof( Guid ) ) },
                ["RenameConversation"] = new[] { ( "channelGuid", typeof( Guid ) ), ( "name", typeof( string ) ) }
            };

            foreach ( var action in expected )
            {
                var method = block.GetMethod( action.Key );
                Assert.IsNotNull( method, $"the Chat block has a {action.Key} action" );

                var parameters = method.GetParameters().Select( p => (p.Name, p.ParameterType) ).ToArray();
                CollectionAssert.AreEqual( action.Value, parameters, $"{action.Key} takes the channel and only what it needs, never the caller" );
            }
        }

        [TestMethod]
        public void AGuidThatIsNotAChatChannelFindsNothingAndChangesNothing()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var notChat = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Not chat", g => g.IsChatEnabledOverride = false );

                Assert.AreEqual( "door.not_found", scene.Join( ada, notChat ).Code );
                Assert.AreEqual( "door.not_found", scene.Join( ada, Guid.NewGuid() ).Code );
                Assert.AreEqual( "door.not_found", scene.Join( ada, scene.Alias( ada ) ).Code, "a person's Guid names no channel" );
                Assert.AreEqual( 0, scene.ActiveMemberCount( notChat ) );
                Assert.AreEqual( 0, scene.Platform.Posts );
            }
        }

        #endregion What crosses the doors

        #region Join

        [TestMethod]
        public void JoiningAPublicChannelAddsTheCallerPushesThemAndPostsTheJoinLine()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Open room" );
                scene.WaitForPushes();

                var result = scene.Join( ada, room );

                Assert.AreEqual( "ok", result.Code, result.Message );
                Assert.IsFalse( result.IsPending, "the stand-in answered the push at once" );
                CollectionAssert.Contains( scene.ActiveMembers( room ), ada );
                Assert.IsTrue( scene.WaitForPushes().SelectMany( p => p.Members ).Any( r => ( Guid ) r[0] == room && ( Guid ) r[1] == scene.Alias( ada ) ),
                    "the membership was pushed before the door answered" );

                var line = scene.Platform.Lines.Single();
                Assert.AreEqual( room, line.ChannelGuid );
                Assert.AreEqual( scene.Name( ada ) + " joined the channel.", line.Body );
                Assert.IsNull( line.SenderAliasGuid, "a system line is under nobody" );
            }
        }

        [TestMethod]
        public void APinnedChannelThatIsNotPublicCanBeJoined()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Pinned room", g =>
                {
                    g.IsChatChannelPublicOverride = false;
                    g.IsChatChannelAlwaysShownOverride = true;
                } );

                Assert.AreEqual( "ok", scene.Join( ada, room ).Code );
                CollectionAssert.Contains( scene.ActiveMembers( room ), ada );
            }
        }

        [TestMethod]
        public void APrivateChannelOrAConversationCannotBeJoined()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var bo = scene.AddChatPerson( "Bo", isOpenDmAllowed: true );
                var cy = scene.AddChatPerson( "Cy", isOpenDmAllowed: true );
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Private room", g => g.IsChatChannelPublicOverride = false );
                var conversation = scene.Start( bo, scene.Alias( cy ) ).ChannelGuid.Value;

                Assert.AreEqual( "door.not_allowed", scene.Join( ada, room ).Code );
                Assert.AreEqual( "door.not_allowed", scene.Join( ada, conversation ).Code );
                Assert.AreEqual( 0, scene.ActiveMemberCount( room ) );
                Assert.AreEqual( 2, scene.ActiveMemberCount( conversation ) );
                Assert.AreEqual( 0, scene.Platform.Posts );
            }
        }

        [TestMethod]
        public void APersonBannedFromTheChannelCannotJoinItAgain()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Open room" );
                scene.Fixture.AddMember( room, ada, m => m.IsChatBanned = true );

                Assert.AreEqual( "door.not_allowed", scene.Join( ada, room ).Code );

                var rows = scene.Memberships( room, ada );
                Assert.AreEqual( 1, rows.Count, "no second row beside the banned one" );
                Assert.IsTrue( rows[0].IsChatBanned, "and the ban stands" );
                Assert.AreEqual( 0, scene.Platform.Posts );
            }
        }

        [TestMethod]
        public void AnArchivedOrInactiveMembershipIsRestoredRatherThanDuplicated()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var bo = scene.AddChatPerson( "Bo" );
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Open room" );
                scene.Fixture.AddMember( room, ada, m => m.IsArchived = true );
                scene.Fixture.AddMember( room, bo, m => m.GroupMemberStatus = GroupMemberStatus.Inactive );

                Assert.AreEqual( "ok", scene.Join( ada, room ).Code );
                Assert.AreEqual( "ok", scene.Join( bo, room ).Code );

                foreach ( var person in new[] { ada, bo } )
                {
                    var rows = scene.Memberships( room, person );
                    Assert.AreEqual( 1, rows.Count, "the old row is used again" );
                    Assert.IsFalse( rows[0].IsArchived );
                    Assert.AreEqual( GroupMemberStatus.Active, rows[0].GroupMemberStatus );
                }
            }
        }

        [TestMethod]
        public void JoiningAChannelThePersonIsAlreadyInChangesNothingAndPostsNoLine()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Open room" );
                scene.Fixture.AddMember( room, ada );

                Assert.AreEqual( "ok", scene.Join( ada, room ).Code, "a join sent twice is answered as done" );
                Assert.AreEqual( 1, scene.Memberships( room, ada ).Count );
                Assert.AreEqual( 0, scene.Platform.Posts );
            }
        }

        [TestMethod]
        public void ACallerTheSessionGatesRefuseJoinsNothing()
        {
            using ( var scene = new ChatDoorScene( minimumAge: 13 ) )
            {
                var banned = scene.AddChatPerson( "Banned" );
                scene.SetAge( banned, 40 );
                scene.AddToBanList( banned );
                var child = scene.AddChatPerson( "Child" );
                scene.SetAge( child, 12 );
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Open room" );

                Assert.AreEqual( "banned", scene.Join( banned, room ).Code );
                Assert.AreEqual( "age_restricted", scene.Join( child, room ).Code );
                Assert.AreEqual( 0, scene.ActiveMemberCount( room ) );
            }
        }

        [TestMethod]
        public void AJoinWhosePushMissesItsBudgetAnswersPendingAndTheMembershipStands()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Open room" );
                scene.Platform.PushDelay = TimeSpan.FromSeconds( 4 );

                var result = scene.Join( ada, room );

                Assert.AreEqual( "ok", result.Code, "Rock committed the membership, so the door succeeded" );
                Assert.IsTrue( result.IsPending, "but the platform had not taken it yet" );
                CollectionAssert.Contains( scene.ActiveMembers( room ), ada );
            }
        }

        [TestMethod]
        public void ALineThePlatformCannotTakeDoesNotUndoTheJoin()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Open room" );
                scene.Platform.IsPostFailing = true;

                Assert.AreEqual( "ok", scene.Join( ada, room ).Code, "the membership is what was asked for, and it was made" );
                CollectionAssert.Contains( scene.ActiveMembers( room ), ada );
            }
        }

        #endregion Join

        #region Leave

        [TestMethod]
        public void LeavingAChannelRemovesTheRockMembershipAndPushesItsAbsenceWithNoLine()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Open room" );
                scene.Fixture.AddMember( room, ada );
                scene.WaitForPushes();

                var result = scene.Leave( ada, room );

                Assert.AreEqual( "ok", result.Code, result.Message );
                CollectionAssert.DoesNotContain( scene.ActiveMembers( room ), ada, "leaving the chat leaves the Rock group" );
                Assert.IsTrue( scene.WaitForPushes().SelectMany( p => p.AbsentMembers ).Contains( (room, scene.Alias( ada )) ),
                    "the platform was told the membership is gone" );
                Assert.AreEqual( 0, scene.Platform.Posts, "a channel shows no line when someone leaves" );
            }
        }

        [TestMethod]
        public void AChannelThatDoesNotAllowLeavingCannotBeLeft()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Kept room", g => g.IsLeavingChatChannelAllowedOverride = false );
                scene.Fixture.AddMember( room, ada );

                Assert.AreEqual( "door.not_allowed", scene.Leave( ada, room ).Code );
                CollectionAssert.Contains( scene.ActiveMembers( room ), ada );
            }
        }

        [TestMethod]
        public void ABannedMemberCannotLeaveSinceThatWouldDeleteTheBan()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Open room" );
                scene.Fixture.AddMember( room, ada, m => m.IsChatBanned = true );

                Assert.AreEqual( "door.not_allowed", scene.Leave( ada, room ).Code );
                Assert.IsTrue( scene.Memberships( room, ada ).Single().IsChatBanned );
            }
        }

        [TestMethod]
        public void AChannelThePersonIsNotInHasNothingToLeave()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Open room" );

                Assert.AreEqual( "door.not_member", scene.Leave( ada, room ).Code );
            }
        }

        [TestMethod]
        public void AConversationOfTwoCannotBeLeftAndAGroupConversationCanWithALine()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada", isOpenDmAllowed: true );
                var bo = scene.AddChatPerson( "Bo", isOpenDmAllowed: true );
                var cy = scene.AddChatPerson( "Cy", isOpenDmAllowed: true );
                var pair = scene.Start( ada, scene.Alias( bo ) ).ChannelGuid.Value;
                var group = scene.Start( ada, scene.Alias( bo ), scene.Alias( cy ) ).ChannelGuid.Value;

                Assert.AreEqual( "door.not_allowed", scene.Leave( ada, pair ).Code, "a conversation of two is hidden, not left" );
                Assert.AreEqual( 2, scene.ActiveMemberCount( pair ) );

                Assert.AreEqual( "ok", scene.Leave( cy, group ).Code );
                CollectionAssert.AreEquivalent( new[] { ada, bo }, scene.ActiveMembers( group ) );
                Assert.AreEqual( scene.Name( cy ) + " left the conversation.", scene.Platform.Lines.Single( l => l.ChannelGuid == group ).Body );
            }
        }

        #endregion Leave

        #region Add

        [TestMethod]
        public void AManagerAddsPeopleToAChannelPushesThemAndPostsOneLine()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var lee = scene.AddChatPerson( "Lee" );
                var bo = scene.AddChatPerson( "Bo" );
                var cy = scene.AddChatPerson( "Cy" );
                var manager = scene.AddManagerRole();
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Team", g => g.IsChatChannelPublicOverride = false );
                scene.Fixture.AddMember( room, lee, m => m.GroupRoleId = manager );
                scene.WaitForPushes();

                var result = scene.Add( lee, room, scene.Alias( bo ), scene.Alias( cy ) );

                Assert.AreEqual( "ok", result.Code, result.Message );
                CollectionAssert.IsSubsetOf( new[] { bo, cy }, scene.ActiveMembers( room ) );
                var pushed = scene.WaitForPushes().SelectMany( p => p.Members ).Where( r => ( Guid ) r[0] == room ).Select( r => ( Guid ) r[1] ).ToList();
                CollectionAssert.IsSubsetOf( new[] { scene.Alias( bo ), scene.Alias( cy ) }, pushed );
                Assert.AreEqual( $"{scene.Name( lee )} added {scene.Name( bo )} and {scene.Name( cy )}.", scene.Platform.Lines.Single().Body );
            }
        }

        [TestMethod]
        public void SomeoneWithoutTheRightToManageMembersCannotAddToAChannel()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var bo = scene.AddChatPerson( "Bo" );
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Team", g => g.IsChatChannelPublicOverride = false );
                scene.Fixture.AddMember( room, ada );

                Assert.AreEqual( "door.not_allowed", scene.Add( ada, room, scene.Alias( bo ) ).Code );
                CollectionAssert.DoesNotContain( scene.ActiveMembers( room ), bo );
                Assert.AreEqual( 0, scene.Platform.Posts );
            }
        }

        [TestMethod]
        public void SomeoneChatKeepsOutIsNotAddedAndNobodyElseIsEither()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var lee = scene.AddChatPerson( "Lee" );
                var bo = scene.AddChatPerson( "Bo" );
                var gil = scene.AddChatPerson( "Gil" );
                scene.AddToBanList( gil );
                var manager = scene.AddManagerRole();
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Team", g => g.IsChatChannelPublicOverride = false );
                scene.Fixture.AddMember( room, lee, m => m.GroupRoleId = manager );

                var result = scene.Add( lee, room, scene.Alias( bo ), scene.Alias( gil ) );

                Assert.AreEqual( "door.target_not_eligible", result.Code );
                Assert.AreEqual( scene.Alias( gil ), result.PersonAliasGuid, "the refusal names who cannot be added" );
                Assert.AreEqual( 1, scene.ActiveMemberCount( room ), "a refusal adds nobody" );
            }
        }

        [TestMethod]
        public void TheShapeOfAnAddIsChecked()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var lee = scene.AddChatPerson( "Lee" );
                var manager = scene.AddManagerRole();
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Team" );
                scene.Fixture.AddMember( room, lee, m => m.GroupRoleId = manager );
                var nine = Enumerable.Range( 0, 9 ).Select( i => scene.Alias( scene.AddChatPerson( "Many" + i ) ) ).ToArray();

                Assert.AreEqual( "door.bad_request", scene.Add( lee, room ).Code, "nobody named" );
                Assert.AreEqual( "door.bad_request", scene.Add( lee, room, nine ).Code, "more than eight at once" );
                Assert.AreEqual( "door.not_found", scene.Add( lee, room, Guid.NewGuid() ).Code, "a Guid that is nobody" );
                Assert.AreEqual( 1, scene.ActiveMemberCount( room ) );
            }
        }

        [TestMethod]
        public void AGroupConversationsMemberAddsSomeoneTheyMayMessageWithALine()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada", isOpenDmAllowed: true );
                var bo = scene.AddChatPerson( "Bo", isOpenDmAllowed: true );
                var cy = scene.AddChatPerson( "Cy", isOpenDmAllowed: true );
                var dee = scene.AddChatPerson( "Dee", isOpenDmAllowed: true );
                var group = scene.Start( ada, scene.Alias( bo ), scene.Alias( cy ) ).ChannelGuid.Value;

                Assert.AreEqual( "ok", scene.Add( bo, group, scene.Alias( dee ) ).Code, "any member may add" );
                CollectionAssert.Contains( scene.ActiveMembers( group ), dee );
                Assert.AreEqual( $"{scene.Name( bo )} added {scene.Name( dee )}.", scene.Platform.Lines.Single( l => l.ChannelGuid == group ).Body );
            }
        }

        [TestMethod]
        public void AGroupConversationTakesNobodyTheAdderCouldNotMessageAndNeverMoreThanNine()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada", isOpenDmAllowed: true );
                var others = Enumerable.Range( 0, 7 ).Select( i => scene.AddChatPerson( "Other" + i, isOpenDmAllowed: true ) ).ToList();
                var closed = scene.AddChatPerson( "Closed", isOpenDmAllowed: false );
                var group = scene.Start( ada, others.Select( scene.Alias ).ToArray() ).ChannelGuid.Value;

                var refused = scene.Add( ada, group, scene.Alias( closed ) );
                Assert.AreEqual( "door.target_not_eligible", refused.Code, "Open DM off and nothing private shared" );
                Assert.AreEqual( scene.Alias( closed ), refused.PersonAliasGuid );

                var ninth = scene.AddChatPerson( "Ninth", isOpenDmAllowed: true );
                var tenth = scene.AddChatPerson( "Tenth", isOpenDmAllowed: true );
                Assert.AreEqual( "door.bad_request", scene.Add( ada, group, scene.Alias( ninth ), scene.Alias( tenth ) ).Code, "ten would be one too many" );
                Assert.AreEqual( 8, scene.ActiveMemberCount( group ) );

                Assert.AreEqual( "ok", scene.Add( ada, group, scene.Alias( ninth ) ).Code, "nine is the most" );
                Assert.AreEqual( 9, scene.ActiveMemberCount( group ) );
            }
        }

        [TestMethod]
        public void AddingToAConversationOfTwoIsRefusedSoItStaysPrivate()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada", isOpenDmAllowed: true );
                var bo = scene.AddChatPerson( "Bo", isOpenDmAllowed: true );
                var cy = scene.AddChatPerson( "Cy", isOpenDmAllowed: true );
                var pair = scene.Start( ada, scene.Alias( bo ) ).ChannelGuid.Value;

                Assert.AreEqual( "door.not_allowed", scene.Add( ada, pair, scene.Alias( cy ) ).Code, "a new group conversation is started instead" );
                Assert.AreEqual( 2, scene.ActiveMemberCount( pair ) );
            }
        }

        #endregion Add

        #region Remove

        [TestMethod]
        public void AManagerRemovesSomeoneFromAChannelWithNoLine()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var lee = scene.AddChatPerson( "Lee" );
                var bo = scene.AddChatPerson( "Bo" );
                var manager = scene.AddManagerRole();
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Team" );
                scene.Fixture.AddMember( room, lee, m => m.GroupRoleId = manager );
                scene.Fixture.AddMember( room, bo );
                scene.WaitForPushes();

                Assert.AreEqual( "ok", scene.Remove( lee, room, scene.Alias( bo ) ).Code );
                CollectionAssert.DoesNotContain( scene.ActiveMembers( room ), bo );
                Assert.IsTrue( scene.WaitForPushes().SelectMany( p => p.AbsentMembers ).Contains( (room, scene.Alias( bo )) ) );
                Assert.AreEqual( 0, scene.Platform.Posts );
            }
        }

        [TestMethod]
        public void ARemoveIsRefusedWithoutTheRightForABannedMemberAndForOneself()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var lee = scene.AddChatPerson( "Lee" );
                var ada = scene.AddChatPerson( "Ada" );
                var bea = scene.AddChatPerson( "Bea" );
                var manager = scene.AddManagerRole();
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Team" );
                scene.Fixture.AddMember( room, lee, m => m.GroupRoleId = manager );
                scene.Fixture.AddMember( room, ada );
                scene.Fixture.AddMember( room, bea, m => m.IsChatBanned = true );

                Assert.AreEqual( "door.not_allowed", scene.Remove( ada, room, scene.Alias( lee ) ).Code, "no right to manage members" );
                Assert.AreEqual( "door.not_allowed", scene.Remove( lee, room, scene.Alias( bea ) ).Code, "removing a banned member would lift the ban" );
                Assert.AreEqual( "door.bad_request", scene.Remove( lee, room, scene.Alias( lee ) ).Code, "leaving has its own door" );
                Assert.AreEqual( 3, scene.ActiveMemberCount( room ) );
            }
        }

        [TestMethod]
        public void AnyMemberRemovesSomeoneFromAGroupConversationWithALineButNotFromAConversationOfTwo()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada", isOpenDmAllowed: true );
                var bo = scene.AddChatPerson( "Bo", isOpenDmAllowed: true );
                var cy = scene.AddChatPerson( "Cy", isOpenDmAllowed: true );
                var pair = scene.Start( ada, scene.Alias( bo ) ).ChannelGuid.Value;
                var group = scene.Start( ada, scene.Alias( bo ), scene.Alias( cy ) ).ChannelGuid.Value;

                Assert.AreEqual( "door.not_allowed", scene.Remove( ada, pair, scene.Alias( bo ) ).Code );
                Assert.AreEqual( "ok", scene.Remove( bo, group, scene.Alias( cy ) ).Code );
                CollectionAssert.AreEquivalent( new[] { ada, bo }, scene.ActiveMembers( group ) );
                Assert.AreEqual( $"{scene.Name( bo )} removed {scene.Name( cy )}.", scene.Platform.Lines.Single( l => l.ChannelGuid == group ).Body );
            }
        }

        #endregion Remove

        #region Rename

        [TestMethod]
        public void AnyMemberRenamesAGroupConversationPushesItAndPostsALine()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada", isOpenDmAllowed: true );
                var bo = scene.AddChatPerson( "Bo", isOpenDmAllowed: true );
                var cy = scene.AddChatPerson( "Cy", isOpenDmAllowed: true );
                var group = scene.Start( ada, scene.Alias( bo ), scene.Alias( cy ) ).ChannelGuid.Value;
                scene.WaitForPushes();

                Assert.AreEqual( "ok", scene.Rename( cy, group, "  Trip planning  " ).Code );
                Assert.AreEqual( "Trip planning", scene.Group( group ).Name, "trimmed" );
                Assert.IsTrue( scene.WaitForPushes().SelectMany( p => p.Channels ).Any( r => ( Guid ) r[0] == group && ( string ) r[1] == "Trip planning" ) );
                Assert.AreEqual( scene.Name( cy ) + " named the conversation Trip planning.", scene.Platform.Lines.Single( l => l.ChannelGuid == group ).Body );
            }
        }

        [TestMethod]
        public void ABlankNameGivesTheConversationBackItsMembersNames()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada", isOpenDmAllowed: true );
                var bo = scene.AddChatPerson( "Bo", isOpenDmAllowed: true );
                var cy = scene.AddChatPerson( "Cy", isOpenDmAllowed: true );
                var group = scene.Start( ada, scene.Alias( bo ), scene.Alias( cy ) ).ChannelGuid.Value;
                scene.Rename( ada, group, "Trip planning" );

                Assert.AreEqual( "ok", scene.Rename( ada, group, "   " ).Code );
                Assert.AreEqual( "Chat Direct Message", scene.Group( group ).Name, "Rock's placeholder, which chat shows as the members' names" );
            }
        }

        [TestMethod]
        public void ARenameIsRefusedForATooLongNameAConversationOfTwoAChannelAndANonMember()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada", isOpenDmAllowed: true );
                var bo = scene.AddChatPerson( "Bo", isOpenDmAllowed: true );
                var cy = scene.AddChatPerson( "Cy", isOpenDmAllowed: true );
                var dee = scene.AddChatPerson( "Dee" );
                var pair = scene.Start( ada, scene.Alias( bo ) ).ChannelGuid.Value;
                var group = scene.Start( ada, scene.Alias( bo ), scene.Alias( cy ) ).ChannelGuid.Value;
                var lee = scene.AddChatPerson( "Lee" );
                var manager = scene.AddManagerRole();
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Team" );
                scene.Fixture.AddMember( room, lee, m => m.GroupRoleId = manager );

                Assert.AreEqual( "door.bad_request", scene.Rename( ada, group, new string( 'x', 101 ) ).Code );
                Assert.AreEqual( "door.not_allowed", scene.Rename( ada, pair, "Us" ).Code, "a conversation of two is titled by the other person" );
                Assert.AreEqual( "door.not_allowed", scene.Rename( lee, room, "Renamed" ).Code, "a channel's name is staff's, in Rock" );
                Assert.AreEqual( "door.not_member", scene.Rename( dee, group, "Mine" ).Code );
                Assert.AreEqual( "Team", scene.Group( room ).Name );
                Assert.AreEqual( 0, scene.Platform.Posts );
            }
        }

        #endregion Rename
    }
}
