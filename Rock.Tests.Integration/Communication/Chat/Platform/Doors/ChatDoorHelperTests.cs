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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Doors;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Doors
{
    /// <summary>
    /// The door that starts a direct message: who may be put in one, which conversation an existing
    /// set of people reopens, and the one group Rock makes for a new one.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The browser names the people and nothing else. Everything the door decides it reads
    ///         from Rock: the caller's right to start a conversation, each person's Open DM setting,
    ///         the private rooms they share, and any conversation those exact people already have.
    ///         Picking people creates nothing; this door is called by the first message sent.
    ///     </para>
    ///     <para>
    ///         A new group's Guid is worked out from the church and its people, so two people
    ///         starting the same conversation at once are refused a second group by Rock's unique
    ///         index on the Guid rather than by a lock. These tests run against a stand-in for the
    ///         chat platform, which answers every push at once unless a test slows it.
    ///     </para>
    /// </remarks>
    [TestClass]
    public class ChatDoorHelperTests : DatabaseTestsBase
    {
        #region Only people cross the door

        [TestMethod]
        public void TheBlockActionTakesOnlyPeople()
        {
            var action = typeof( Rock.Blocks.Communication.Chat.ChatShell ).GetMethod( "StartDirectMessage" );

            Assert.IsNotNull( action, "the Chat block has a StartDirectMessage action" );

            var parameters = action.GetParameters();
            Assert.AreEqual( 1, parameters.Length, "the action takes one argument" );
            Assert.AreEqual( "personAliasGuids", parameters[0].Name );
            Assert.IsTrue( typeof( IEnumerable<Guid> ).IsAssignableFrom( parameters[0].ParameterType ),
                "a list of the people chosen, and no channel or group of any kind" );
        }

        [TestMethod]
        public void AGroupGuidPassedAsAPersonFindsNobodyAndAddsNobodyToThatGroup()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var caller = scene.AddChatPerson( "Caller" );
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Somebody else's room", g => g.IsChatChannelPublicOverride = false );
                var stranger = scene.AddChatPerson( "Stranger" );
                scene.Fixture.AddMember( room, stranger );

                var result = scene.Start( caller, room );

                Assert.AreEqual( "door.not_found", result.Code );
                Assert.IsNull( result.ChannelGuid );
                Assert.AreEqual( 1, scene.ActiveMemberCount( room ), "the room the Guid named gained nobody" );
            }
        }

        #endregion Only people cross the door

        #region Create

        [TestMethod]
        public void ANewOneToOneIsOneGroupWithTheDerivedGuidAndBothPeople()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var caller = scene.AddChatPerson( "Caller" );
                var other = scene.AddChatPerson( "Other", isOpenDmAllowed: true );

                var result = scene.Start( caller, scene.Alias( other ) );

                Assert.AreEqual( "ok", result.Code, result.Message );
                Assert.IsFalse( result.IsPending, "the stand-in answered the push at once" );
                Assert.AreEqual( ChatDoorHelper.DirectMessageGuid( scene.TenantId, new[] { caller, other } ), result.ChannelGuid );

                var group = scene.Group( result.ChannelGuid.Value );
                Assert.AreEqual( Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE.AsGuid(), GroupTypeCache.Get( group.GroupTypeId ).Guid );
                CollectionAssert.AreEquivalent( new[] { caller, other }, scene.ActiveMembers( result.ChannelGuid.Value ) );

                var pushed = scene.WaitForPushes().SelectMany( p => p.ChannelGuids ).ToList();
                CollectionAssert.Contains( pushed, result.ChannelGuid.Value, "the new conversation was pushed before the door answered" );
            }
        }

        [TestMethod]
        public void ANewGroupConversationHoldsEveryoneChosen()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var caller = scene.AddChatPerson( "Caller" );
                var people = Enumerable.Range( 1, 8 ).Select( i => scene.AddChatPerson( "Other" + i, isOpenDmAllowed: true ) ).ToList();

                var result = scene.Start( caller, people.Select( scene.Alias ).ToArray() );

                Assert.AreEqual( "ok", result.Code, result.Message );
                CollectionAssert.AreEquivalent( people.Concat( new[] { caller } ).ToList(), scene.ActiveMembers( result.ChannelGuid.Value ),
                    "nine people, the most a conversation holds" );
            }
        }

        [TestMethod]
        public void TwoStartingTheSameConversationAtOnceMakeOneGroup()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada", isOpenDmAllowed: true );
                var bo = scene.AddChatPerson( "Bo", isOpenDmAllowed: true );

                // Each side three times, all released together, each in its own request and context.
                var gate = new ManualResetEventSlim( false );
                var starts = Enumerable.Range( 0, 6 )
                    .Select( i => Task.Run( () =>
                    {
                        gate.Wait();
                        return i % 2 == 0 ? scene.Start( ada, scene.Alias( bo ) ) : scene.Start( bo, scene.Alias( ada ) );
                    } ) )
                    .ToList();
                gate.Set();
                Task.WaitAll( starts.ToArray() );

                var answers = starts.Select( t => t.Result ).ToList();
                Assert.IsTrue( answers.All( a => a.Code == "ok" ), string.Join( "; ", answers.Select( a => a.Code + " " + a.Message ) ) );
                Assert.AreEqual( 1, answers.Select( a => a.ChannelGuid ).Distinct().Count(), "every caller was given the same conversation" );
                Assert.AreEqual( 1, scene.DirectMessagesWithExactly( ada, bo ).Count, "and Rock holds one group for it" );
            }
        }

        [TestMethod]
        public void AGroupHoldingTheDerivedGuidWhosePeopleChangedLeavesItAloneAndMakesANewOne()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var bo = scene.AddChatPerson( "Bo", isOpenDmAllowed: true );
                var cy = scene.AddChatPerson( "Cy", isOpenDmAllowed: true );

                // Ada and Bo's conversation, which Cy has since been added to.
                var derived = ChatDoorHelper.DirectMessageGuid( scene.TenantId, new[] { ada, bo } );
                scene.Fixture.AddChannel( scene.Fixture.DirectMessageGroupTypeId, "Chat Direct Message", g => g.Guid = derived );
                scene.Fixture.AddMember( derived, ada );
                scene.Fixture.AddMember( derived, bo );
                scene.Fixture.AddMember( derived, cy );

                var result = scene.Start( ada, scene.Alias( bo ) );

                Assert.AreEqual( "ok", result.Code, result.Message );
                Assert.AreNotEqual( derived, result.ChannelGuid, "a new conversation, under a Guid of its own" );
                CollectionAssert.AreEquivalent( new[] { ada, bo }, scene.ActiveMembers( result.ChannelGuid.Value ) );
                CollectionAssert.AreEquivalent( new[] { ada, bo, cy }, scene.ActiveMembers( derived ), "the group of three is untouched" );
            }
        }

        [TestMethod]
        public void AnInactiveGroupHoldingTheDerivedGuidWithTheSamePeopleLeavesItAloneAndMakesANewOne()
        {
            AssertADeadHolderIsLeftAlone( "[IsActive] = 0" );
        }

        [TestMethod]
        public void AnArchivedGroupHoldingTheDerivedGuidWithTheSamePeopleLeavesItAloneAndMakesANewOne()
        {
            AssertADeadHolderIsLeftAlone( "[IsArchived] = 1" );
        }

        /// <summary>
        /// Starts Ada and Bo's conversation while the group holding its derived Guid, with exactly
        /// them as active members, is one the platform does not hold as live.
        /// </summary>
        /// <param name="ending">The column assignment that ends the group.</param>
        private static void AssertADeadHolderIsLeftAlone( string ending )
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var bo = scene.AddChatPerson( "Bo", isOpenDmAllowed: true );

                var derived = ChatDoorHelper.DirectMessageGuid( scene.TenantId, new[] { ada, bo } );
                scene.Fixture.AddChannel( scene.Fixture.DirectMessageGroupTypeId, "Chat Direct Message", g => g.Guid = derived );
                scene.Fixture.AddMember( derived, ada );
                scene.Fixture.AddMember( derived, bo );

                // Ended in SQL, past the save hook that would end its members too: the group's own
                // state is what the platform reads, whatever its members still say.
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.ExecuteSqlCommand( $"UPDATE [Group] SET {ending} WHERE [Guid] = @p0", derived );
                }

                var before = scene.AnyGroup( derived );

                var result = scene.Start( ada, scene.Alias( bo ) );

                Assert.AreEqual( "ok", result.Code, result.Message );
                Assert.AreNotEqual( derived, result.ChannelGuid, "a new conversation, since the platform holds the old one as gone" );
                Assert.IsTrue( scene.Group( result.ChannelGuid.Value ).IsActive, "the new conversation is live" );
                CollectionAssert.AreEquivalent( new[] { ada, bo }, scene.ActiveMembers( result.ChannelGuid.Value ) );

                var after = scene.AnyGroup( derived );
                Assert.AreEqual( before.IsActive, after.IsActive, "the old group is untouched" );
                Assert.AreEqual( before.IsArchived, after.IsArchived, "the old group is untouched" );
                Assert.AreEqual( before.ModifiedDateTime, after.ModifiedDateTime, "the old group is untouched" );
                CollectionAssert.AreEquivalent( new[] { ada, bo }, scene.ActiveMembers( derived ), "its members are untouched" );
            }
        }

        #endregion Create

        #region Reuse

        [TestMethod]
        public void AnExistingConversationIsReopenedWhateverItsGuid()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var bo = scene.AddChatPerson( "Bo", isOpenDmAllowed: true );

                // As the earlier chat made them: a random Guid and Rock's placeholder name.
                var existing = scene.Fixture.AddChannel( scene.Fixture.DirectMessageGroupTypeId, "Chat Direct Message" );
                scene.Fixture.AddMember( existing, ada );
                scene.Fixture.AddMember( existing, bo );

                var result = scene.Start( ada, scene.Alias( bo ) );

                Assert.AreEqual( "ok", result.Code, result.Message );
                Assert.AreEqual( existing, result.ChannelGuid );
                Assert.AreEqual( 1, scene.DirectMessagesWithExactly( ada, bo ).Count, "no second group" );
            }
        }

        [TestMethod]
        public void AGroupConversationIsReopenedByItsExactPeopleInAnyOrder()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var bo = scene.AddChatPerson( "Bo", isOpenDmAllowed: true );
                var cy = scene.AddChatPerson( "Cy", isOpenDmAllowed: true );
                var dee = scene.AddChatPerson( "Dee", isOpenDmAllowed: true );

                var existing = scene.Fixture.AddChannel( scene.Fixture.DirectMessageGroupTypeId, "Book Club" );
                scene.Fixture.AddMember( existing, ada );
                scene.Fixture.AddMember( existing, bo );
                scene.Fixture.AddMember( existing, cy );

                Assert.AreEqual( existing, scene.Start( ada, scene.Alias( cy ), scene.Alias( bo ) ).ChannelGuid );
                Assert.AreEqual( existing, scene.Start( bo, scene.Alias( ada ), scene.Alias( cy ) ).ChannelGuid,
                    "whichever of them starts it" );

                var fewer = scene.Start( ada, scene.Alias( bo ) );
                var more = scene.Start( ada, scene.Alias( bo ), scene.Alias( cy ), scene.Alias( dee ) );
                Assert.AreNotEqual( existing, fewer.ChannelGuid, "fewer people is another conversation" );
                Assert.AreNotEqual( existing, more.ChannelGuid, "and so is more" );
            }
        }

        [TestMethod]
        public void APersonChosenByAnotherOfTheirAliasesIsTheSamePerson()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var bo = scene.AddChatPerson( "Bo", isOpenDmAllowed: true );
                var boEarlierAlias = scene.Fixture.AddExtraAlias( bo );

                var existing = scene.Fixture.AddChannel( scene.Fixture.DirectMessageGroupTypeId, "Chat Direct Message" );
                scene.Fixture.AddMember( existing, ada );
                scene.Fixture.AddMember( existing, bo );

                var result = scene.Start( ada, boEarlierAlias );

                Assert.AreEqual( existing, result.ChannelGuid, "a client holding an alias from before a merge still reaches the person" );
            }
        }

        [TestMethod]
        public void AConversationCarriesOnAfterTheOtherPersonTurnsOpenDmOff()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var bo = scene.AddChatPerson( "Bo", isOpenDmAllowed: false );

                var existing = scene.Fixture.AddChannel( scene.Fixture.DirectMessageGroupTypeId, "Chat Direct Message" );
                scene.Fixture.AddMember( existing, ada );
                scene.Fixture.AddMember( existing, bo );

                var result = scene.Start( ada, scene.Alias( bo ) );

                Assert.AreEqual( "ok", result.Code, result.Message );
                Assert.AreEqual( existing, result.ChannelGuid );
            }
        }

        #endregion Reuse

        #region Who may be put in a conversation

        [TestMethod]
        public void OpenDmOffAndAPrivateRoomSharedIsAllowed()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var bo = scene.AddChatPerson( "Bo", isOpenDmAllowed: false );
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Shared private room", g => g.IsChatChannelPublicOverride = false );
                scene.Fixture.AddMember( room, ada );
                scene.Fixture.AddMember( room, bo );

                Assert.AreEqual( "ok", scene.Start( ada, scene.Alias( bo ) ).Code );
            }
        }

        [TestMethod]
        public void OpenDmOffWithNothingPrivateSharedIsRefusedNamingThatPerson()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var bo = scene.AddChatPerson( "Bo", isOpenDmAllowed: true );
                var cy = scene.AddChatPerson( "Cy", isOpenDmAllowed: false );

                // The lobby every chat person here is in is public, which shares nothing.
                var result = scene.Start( ada, scene.Alias( bo ), scene.Alias( cy ) );

                Assert.AreEqual( "door.target_not_eligible", result.Code );
                Assert.AreEqual( scene.Alias( cy ), result.PersonAliasGuid, "the answer names who could not be added" );
                Assert.IsNull( result.ChannelGuid );
                Assert.AreEqual( 0, scene.DirectMessagesWithExactly( ada, bo, cy ).Count, "and nothing was made" );
            }
        }

        [TestMethod]
        public void NobodyOnTheBanListInactiveOrOutsideChatCanBePutInAConversation()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var banned = scene.AddChatPerson( "Banned", isOpenDmAllowed: true );
                scene.AddToBanList( banned );
                var inactive = scene.AddChatPerson( "Inactive", isOpenDmAllowed: true );
                scene.MakeInactive( inactive );
                var outsider = scene.Fixture.AddPerson( "Outsider" );
                scene.SetOpenDm( outsider, true );

                foreach ( var person in new[] { banned, inactive, outsider } )
                {
                    var result = scene.Start( ada, scene.Alias( person ) );

                    Assert.AreEqual( "door.target_not_eligible", result.Code, "person " + person );
                    Assert.AreEqual( scene.Alias( person ), result.PersonAliasGuid );
                }
            }
        }

        [TestMethod]
        public void AConversationWhereSomeoneIsBannedIsNotReopenedOrStartedAgain()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var bo = scene.AddChatPerson( "Bo", isOpenDmAllowed: true );

                var existing = scene.Fixture.AddChannel( scene.Fixture.DirectMessageGroupTypeId, "Chat Direct Message" );
                scene.Fixture.AddMember( existing, ada );
                scene.Fixture.AddMember( existing, bo, m => m.IsChatBanned = true );

                var result = scene.Start( ada, scene.Alias( bo ) );

                Assert.AreEqual( "door.target_not_eligible", result.Code, "a ban is not stepped around by starting again" );
                Assert.AreEqual( 1, scene.DirectMessagesWithExactly( ada, bo ).Count );
            }
        }

        [TestMethod]
        public void ACallerWithoutTheRightToStartConversationsIsRefused()
        {
            // A Direct Message Access data view that no longer exists admits nobody.
            using ( var scene = new ChatDoorScene( directMessageAccess: Guid.NewGuid() ) )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var bo = scene.AddChatPerson( "Bo", isOpenDmAllowed: true );

                Assert.AreEqual( "door.not_allowed", scene.Start( ada, scene.Alias( bo ) ).Code );
            }
        }

        [TestMethod]
        public void TheShapeOfTheRequestIsChecked()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var nine = Enumerable.Range( 1, 9 ).Select( i => scene.Alias( scene.AddChatPerson( "Other" + i, isOpenDmAllowed: true ) ) ).ToArray();

                Assert.AreEqual( "door.bad_request", scene.Start( ada ).Code, "nobody chosen" );
                Assert.AreEqual( "door.bad_request", scene.Start( ada, nine ).Code, "nine others, one more than a conversation holds" );
                Assert.AreEqual( "door.self", scene.Start( ada, scene.Alias( ada ) ).Code, "the caller among the people" );
                Assert.AreEqual( "door.not_found", scene.Start( ada, Guid.NewGuid() ).Code, "an alias Rock does not have" );
            }
        }

        #endregion Who may be put in a conversation

        #region The push

        [TestMethod]
        public void APushThatMissesItsBudgetAnswersPendingAndTheGroupStands()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                var bo = scene.AddChatPerson( "Bo", isOpenDmAllowed: true );
                scene.Platform.PushDelay = TimeSpan.FromSeconds( 4 );

                var result = scene.Start( ada, scene.Alias( bo ) );

                Assert.AreEqual( "ok", result.Code, "Rock committed the conversation, so the door succeeded" );
                Assert.IsTrue( result.IsPending, "but the platform had not taken it yet" );
                Assert.AreEqual( 1, scene.DirectMessagesWithExactly( ada, bo ).Count );
            }
        }

        #endregion The push

        #region A person's own settings

        [TestMethod]
        public void TheSettingsActionTakesOneSettingAndOneValue()
        {
            var action = typeof( Rock.Blocks.Communication.Chat.ChatShell ).GetMethod( "SavePersonSetting" );

            Assert.IsNotNull( action, "the Chat block has a SavePersonSetting action" );

            var parameters = action.GetParameters();
            Assert.AreEqual( 2, parameters.Length, "the action takes two arguments" );
            Assert.AreEqual( "setting", parameters[0].Name );
            Assert.AreEqual( typeof( string ), parameters[0].ParameterType );
            Assert.AreEqual( "value", parameters[1].Name );
            Assert.AreEqual( typeof( bool ), parameters[1].ParameterType, "on or off, never a person or a default" );
        }

        [TestMethod]
        public void SavingOneSettingWritesItAndLeavesTheOtherAtTheChurchDefault()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );

                Assert.AreEqual( "ok", scene.SaveSetting( ada, "open_dm", true ).Code );
                Assert.AreEqual( true, scene.Person( ada ).IsChatOpenDirectMessageAllowed );
                Assert.IsNull( scene.Person( ada ).IsChatProfilePublic, "the setting not named still follows the church" );

                Assert.AreEqual( "ok", scene.SaveSetting( ada, "profile_details", false ).Code );
                Assert.AreEqual( false, scene.Person( ada ).IsChatProfilePublic );
                Assert.AreEqual( true, scene.Person( ada ).IsChatOpenDirectMessageAllowed, "and the first is kept" );
            }
        }

        [TestMethod]
        public void ASavedSettingIsPushedBeforeTheDoorAnswers()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                scene.WaitForPushes();

                var result = scene.SaveSetting( ada, "open_dm", true );

                Assert.AreEqual( "ok", result.Code, result.Message );
                Assert.IsFalse( result.IsPending, "the stand-in answered the push at once" );

                var row = scene.WaitForPushes().SelectMany( p => p.Aliases ).LastOrDefault( r => ( Guid ) r[0] == scene.Alias( ada ) );
                Assert.IsNotNull( row, "the person was pushed" );
                Assert.AreEqual( true, ( bool ) row[8], "carrying Open DM on" );
            }
        }

        [TestMethod]
        public void ASettingWhosePushMissesItsBudgetAnswersPendingAndStaysSaved()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );
                scene.Platform.PushDelay = TimeSpan.FromSeconds( 4 );

                var result = scene.SaveSetting( ada, "profile_details", false );

                Assert.AreEqual( "ok", result.Code, "Rock saved the setting, so the door succeeded" );
                Assert.IsTrue( result.IsPending, "but the platform had not taken it yet" );
                Assert.AreEqual( false, scene.Person( ada ).IsChatProfilePublic );
            }
        }

        [TestMethod]
        public void AnUnknownSettingIsRefusedAndNothingIsWritten()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var ada = scene.AddChatPerson( "Ada" );

                Assert.AreEqual( "door.bad_request", scene.SaveSetting( ada, "is_favorite", true ).Code );
                Assert.AreEqual( "door.bad_request", scene.SaveSetting( ada, null, true ).Code );
                Assert.IsNull( scene.Person( ada ).IsChatProfilePublic );
                Assert.IsNull( scene.Person( ada ).IsChatOpenDirectMessageAllowed );
            }
        }

        [TestMethod]
        public void ACallerTheSessionGatesRefuseChangesNothing()
        {
            using ( var scene = new ChatDoorScene( minimumAge: 13 ) )
            {
                var banned = scene.AddChatPerson( "Banned" );
                scene.SetAge( banned, 40 );
                scene.AddToBanList( banned );
                var child = scene.AddChatPerson( "Child" );
                scene.SetAge( child, 12 );

                Assert.AreEqual( "banned", scene.SaveSetting( banned, "open_dm", true ).Code );
                Assert.AreEqual( "age_restricted", scene.SaveSetting( child, "open_dm", true ).Code );
                Assert.IsNull( scene.Person( banned ).IsChatOpenDirectMessageAllowed );
                Assert.IsNull( scene.Person( child ).IsChatOpenDirectMessageAllowed );
            }
        }

        #endregion A person's own settings

        #region A workflow's direct message

        [TestMethod]
        public void AWorkflowMessagesSomeoneWithOpenDmOffBecauseTheAdminIsTheOneActing()
        {
            using ( var scene = new ChatDoorScene( directMessageAccess: Guid.NewGuid() ) )
            {
                // Neither has opened chat, the sender may not start conversations, and the
                // recipient has Open DM off with nothing shared.
                var sender = scene.Fixture.AddPerson( "Sender" );
                var recipient = scene.Fixture.AddPerson( "Recipient" );
                scene.SetOpenDm( recipient, false );

                var outcome = scene.SendWorkflowDirectMessage( sender, recipient, "Welcome" );

                Assert.AreEqual( "ok", outcome.Code, outcome.Message );
                Assert.AreEqual( ChatDoorHelper.DirectMessageGuid( scene.TenantId, new[] { sender, recipient } ), outcome.ChannelGuid );
                CollectionAssert.AreEquivalent( new[] { sender, recipient }, scene.ActiveMembers( outcome.ChannelGuid.Value ) );
                Assert.AreEqual( 1, scene.Platform.Posts, "and the message was posted" );
            }
        }

        [TestMethod]
        public void AWorkflowCannotMessageSomeoneOnTheBanListOrInactive()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var sender = scene.AddChatPerson( "Sender" );
                var banned = scene.AddChatPerson( "Banned", isOpenDmAllowed: true );
                scene.AddToBanList( banned );
                var inactive = scene.AddChatPerson( "Inactive", isOpenDmAllowed: true );
                scene.MakeInactive( inactive );

                foreach ( var person in new[] { banned, inactive } )
                {
                    var outcome = scene.SendWorkflowDirectMessage( sender, person, "Hello" );

                    Assert.AreEqual( "door.target_not_eligible", outcome.Code, "person " + person );
                    Assert.AreEqual( 0, scene.DirectMessagesWithExactly( sender, person ).Count );
                }

                var asBanned = scene.SendWorkflowDirectMessage( banned, sender, "Hello" );
                Assert.AreEqual( "door.target_not_eligible", asBanned.Code, "nor send as someone on the Ban List" );
                Assert.AreEqual( 0, scene.Platform.Posts );
            }
        }

        [TestMethod]
        public void AWorkflowCannotMessageSomeoneUnderTheMinimumAge()
        {
            using ( var scene = new ChatDoorScene( minimumAge: 18 ) )
            {
                var sender = scene.AddChatPerson( "Sender" );
                scene.SetAge( sender, 40 );
                var child = scene.AddChatPerson( "Child", isOpenDmAllowed: true );
                scene.SetAge( child, 12 );

                var outcome = scene.SendWorkflowDirectMessage( sender, child, "Hello" );

                Assert.AreEqual( "door.target_not_eligible", outcome.Code, "chat itself would refuse this person a session, so a workflow cannot reach them" );
                Assert.AreEqual( 0, scene.DirectMessagesWithExactly( sender, child ).Count, "and no conversation was made" );
                Assert.AreEqual( 0, scene.Platform.Posts, "and nothing was posted" );
            }
        }

        [TestMethod]
        public void AWorkflowCannotMessageSomeoneWithNoBirthdateWhenThereIsAMinimumAge()
        {
            using ( var scene = new ChatDoorScene( minimumAge: 18 ) )
            {
                var sender = scene.AddChatPerson( "Sender" );
                scene.SetAge( sender, 40 );
                var unknown = scene.AddChatPerson( "Unknown", isOpenDmAllowed: true );

                var outcome = scene.SendWorkflowDirectMessage( sender, unknown, "Hello" );

                Assert.AreEqual( "door.target_not_eligible", outcome.Code, "an unknown age is refused, as chat refuses it" );
                Assert.AreEqual( 0, scene.DirectMessagesWithExactly( sender, unknown ).Count );
                Assert.AreEqual( 0, scene.Platform.Posts );
            }
        }

        [TestMethod]
        public void AWorkflowCannotSendAsSomeoneUnderTheMinimumAge()
        {
            using ( var scene = new ChatDoorScene( minimumAge: 18 ) )
            {
                var child = scene.AddChatPerson( "Child" );
                scene.SetAge( child, 12 );
                var recipient = scene.AddChatPerson( "Recipient", isOpenDmAllowed: true );
                scene.SetAge( recipient, 40 );

                var outcome = scene.SendWorkflowDirectMessage( child, recipient, "Hello" );

                Assert.AreEqual( "door.target_not_eligible", outcome.Code, "nor put a message under the name of someone chat keeps out" );
                Assert.AreEqual( 0, scene.DirectMessagesWithExactly( child, recipient ).Count );
                Assert.AreEqual( 0, scene.Platform.Posts );
            }
        }

        [TestMethod]
        public void AWorkflowStillMessagesSomeoneAtOrOverTheMinimumAge()
        {
            using ( var scene = new ChatDoorScene( minimumAge: 18 ) )
            {
                // Open DM off on both, so only the age gates stand between them and the message.
                var sender = scene.AddChatPerson( "Sender", isOpenDmAllowed: false );
                scene.SetAge( sender, 40 );
                var recipient = scene.AddChatPerson( "Recipient", isOpenDmAllowed: false );
                scene.SetAge( recipient, 18 );

                var outcome = scene.SendWorkflowDirectMessage( sender, recipient, "Welcome" );

                Assert.AreEqual( "ok", outcome.Code, outcome.Message );
                Assert.AreEqual( 1, scene.DirectMessagesWithExactly( sender, recipient ).Count );
                Assert.AreEqual( 1, scene.Platform.Posts, "a person exactly the minimum age may be messaged" );
            }
        }

        #endregion A workflow's direct message

        #region A workflow's channel post

        [TestMethod]
        public void AWorkflowCannotPostInAChannelAsSomeoneUnderTheMinimumAge()
        {
            using ( var scene = new ChatDoorScene( minimumAge: 18 ) )
            {
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Announcements" );
                var child = scene.AddChatPerson( "Child" );
                scene.SetAge( child, 12 );

                var outcome = scene.SendWorkflowChannelMessage( room, child, "Hello" );

                Assert.AreEqual( "door.target_not_eligible", outcome.Code, "a named sender chat keeps out cannot post through a workflow" );
                Assert.AreEqual( 0, scene.Platform.Posts, "and nothing was posted" );
            }
        }

        #endregion A workflow's channel post

    }
}
