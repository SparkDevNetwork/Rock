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
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Doors;

namespace Rock.Tests.Communication.Chat.Platform.Doors
{
    /// <summary>
    /// Who may remove a channel's leader, and which role the person named to lead next is given.
    /// </summary>
    /// <remarks>
    /// Only a Chat Administrator removes a leader of a shared channel, and removing the last one
    /// names who leads next in the same step, so a channel is never left with nobody leading it.
    /// The person named takes the role the removed leader held when that is one of the type's
    /// leader roles, so a church's own role name carries over; otherwise the type's first leader
    /// role.
    /// </remarks>
    [TestClass]
    public class ChatDoorLeaderRuleTests
    {
        #region Who may remove a leader

        [TestMethod]
        public void RemovingSomeoneWhoDoesNotLeadAsksNothingMore()
        {
            Assert.IsNull( ChatDoorHelper.RefuseLeaderRemoval( targetLeads: false, callerIsChatAdministrator: false, isLastLeader: false, hasReplacement: false ) );
        }

        [TestMethod]
        public void OnlyAChatAdministratorRemovesALeader()
        {
            var refusal = ChatDoorHelper.RefuseLeaderRemoval( targetLeads: true, callerIsChatAdministrator: false, isLastLeader: false, hasReplacement: false );

            Assert.IsNotNull( refusal, "a manager who is not a Chat Administrator is refused" );
            Assert.AreEqual( "door.not_allowed", refusal.Code );
            Assert.AreEqual( "Only Chat Administrators can remove a leader.", refusal.Message );
        }

        [TestMethod]
        public void AChatAdministratorRemovesALeaderWhenOthersStillLead()
        {
            Assert.IsNull( ChatDoorHelper.RefuseLeaderRemoval( targetLeads: true, callerIsChatAdministrator: true, isLastLeader: false, hasReplacement: false ) );
        }

        [TestMethod]
        public void RemovingTheLastLeaderNeedsSomeoneToLeadNext()
        {
            var refusal = ChatDoorHelper.RefuseLeaderRemoval( targetLeads: true, callerIsChatAdministrator: true, isLastLeader: true, hasReplacement: false );

            Assert.IsNotNull( refusal, "the last leader is not removed without a replacement" );
            Assert.AreEqual( "door.bad_request", refusal.Code );
            Assert.IsFalse( string.IsNullOrWhiteSpace( refusal.Message ), "the refusal carries a sentence" );
        }

        [TestMethod]
        public void RemovingTheLastLeaderWithSomeoneToLeadNextIsAllowed()
        {
            Assert.IsNull( ChatDoorHelper.RefuseLeaderRemoval( targetLeads: true, callerIsChatAdministrator: true, isLastLeader: true, hasReplacement: true ) );
        }

        [TestMethod]
        public void AReplacementDoesNotLetSomeoneWhoIsNotAChatAdministratorRemoveALeader()
        {
            Assert.AreEqual( "door.not_allowed", ChatDoorHelper.RefuseLeaderRemoval( targetLeads: true, callerIsChatAdministrator: false, isLastLeader: true, hasReplacement: true )?.Code );
        }

        #endregion Who may remove a leader

        #region Who counts as the last leader

        // Ada (1) is removed; Bo (2), Cy (3) and Di (4) lead too, as their standing allows.
        private static readonly int[] Leaders = { 1, 2, 3, 4 };

        [TestMethod]
        public void AnotherLeaderInTheRoomAndListedMeansNoneIsAsked()
        {
            Assert.IsFalse( ChatDoorHelper.IsLastLeader( 1, Leaders, live: new HashSet<int> { 1, 2 }, unlisted: new HashSet<int>() ) );
        }

        [TestMethod]
        public void SomeoneWhoDoesNotLeadIsNeverTheLastLeader()
        {
            Assert.IsFalse( ChatDoorHelper.IsLastLeader( 9, Leaders, live: new HashSet<int> { 9 }, unlisted: new HashSet<int>() ) );
        }

        [TestMethod]
        public void TheOnlyLeaderIsTheLast()
        {
            Assert.IsTrue( ChatDoorHelper.IsLastLeader( 1, new[] { 1 }, live: new HashSet<int> { 1 }, unlisted: new HashSet<int>() ) );
        }

        [TestMethod]
        public void ALeaderChatDoesNotListOrWhoIsNotInTheRoomDoesNotCount()
        {
            // Bo is on the Chat Ban List or inactive, Cy is not in the room on the platform (banned
            // there or left out of chat), and Di is both: none of them leads anyone in chat.
            Assert.IsTrue( ChatDoorHelper.IsLastLeader( 1, Leaders, live: new HashSet<int> { 1, 2, 4 }, unlisted: new HashSet<int> { 2, 4 } ) );
        }

        [TestMethod]
        public void TheScreenIsToldTheOneLeaderChatStillShowsInTheRoom()
        {
            // Of the four, only Ada (1) is in the room and listed, so removing her needs a successor.
            Assert.AreEqual( 1, ChatDoorHelper.LastLeaderOf( Leaders, live: new HashSet<int> { 1, 2, 4 }, unlisted: new HashSet<int> { 2, 4 } ) );
        }

        [TestMethod]
        public void TheScreenIsToldNoLastLeaderWhereTwoStillLead()
        {
            Assert.IsNull( ChatDoorHelper.LastLeaderOf( Leaders, live: new HashSet<int> { 1, 2 }, unlisted: new HashSet<int>() ) );
        }

        [TestMethod]
        public void TheScreenIsToldNoLastLeaderWhereNobodyChatShowsLeads()
        {
            // A leader row of someone not in the room is nobody the remove door could take out.
            Assert.IsNull( ChatDoorHelper.LastLeaderOf( Leaders, live: new HashSet<int> { 2, 9 }, unlisted: new HashSet<int> { 2 } ) );
            Assert.IsNull( ChatDoorHelper.LastLeaderOf( new int[0], live: new HashSet<int> { 9 }, unlisted: new HashSet<int>() ) );
        }

        #endregion Who counts as the last leader

        #region The role the next leader takes

        private static readonly (int Id, bool IsLeader, int Order)[] Roles =
        {
            ( 1, false, 0 ),
            ( 2, true, 2 ),
            ( 3, true, 1 ),
            ( 4, false, 3 )
        };

        [TestMethod]
        public void TheNextLeaderTakesTheRemovedLeadersOwnRole()
        {
            Assert.AreEqual( 2, ChatDoorHelper.PickLeaderRoleId( 2, Roles ) );
        }

        [TestMethod]
        public void AHeldRoleThatDoesNotLeadGivesTheTypesFirstLeaderRoleByOrder()
        {
            Assert.AreEqual( 3, ChatDoorHelper.PickLeaderRoleId( 4, Roles ) );
        }

        [TestMethod]
        public void ARoleOfAnotherTypeGivesTheTypesFirstLeaderRoleByOrder()
        {
            Assert.AreEqual( 3, ChatDoorHelper.PickLeaderRoleId( 99, Roles ) );
            Assert.AreEqual( 3, ChatDoorHelper.PickLeaderRoleId( null, Roles ) );
        }

        [TestMethod]
        public void ATypeWithNoLeaderRoleGivesNone()
        {
            Assert.IsNull( ChatDoorHelper.PickLeaderRoleId( 1, new[] { ( 1, false, 0 ), ( 4, false, 3 ) } ) );
        }

        #endregion The role the next leader takes

        #region Who a removal asks the projection about

        // Caller 1 removes 2, naming 3 to lead next; 10 to 59 lead the channel too.
        private static readonly List<int> ManyOtherLeaders = Enumerable.Range( 10, 50 ).ToList();

        [TestMethod]
        public void RemovingSomeoneWhoDoesNotLeadAsksOnlyAboutTheCallerTheirTargetAndTheNextLeader()
        {
            var leaders = new ChatDoorHelper.ChannelLeaders { TargetRoleId = null, OtherLeaderIds = ManyOtherLeaders };

            CollectionAssert.AreEquivalent( new[] { 1, 2, 3 }, ChatDoorHelper.PeopleToReadForRemoval( 1, 2, 3, leaders ),
                "a removal that leaves the channel led as it was read the room's leaders through the projection" );
            CollectionAssert.AreEquivalent( new[] { 1, 2 }, ChatDoorHelper.PeopleToReadForRemoval( 1, 2, null, leaders ) );
        }

        [TestMethod]
        public void RemovingALeaderAsksAboutAtMostTheBoundOfOtherLeaders()
        {
            var leaders = new ChatDoorHelper.ChannelLeaders { TargetRoleId = 7, OtherLeaderIds = ManyOtherLeaders };

            var people = ChatDoorHelper.PeopleToReadForRemoval( 1, 2, 3, leaders );

            CollectionAssert.IsSubsetOf( new[] { 1, 2, 3 }, people );
            Assert.AreEqual( 3 + ChatDoorHelper.MaxOtherLeadersConfirmed, people.Count,
                "a channel with many leaders sent every one of them through the projection" );
            CollectionAssert.IsSubsetOf( people.Except( new[] { 1, 2, 3 } ).ToList(), ManyOtherLeaders );
        }

        [TestMethod]
        public void SomeoneAskedAboutTwiceIsAskedAboutOnce()
        {
            var leaders = new ChatDoorHelper.ChannelLeaders { TargetRoleId = 7, OtherLeaderIds = new List<int> { 1, 3, 4 } };

            CollectionAssert.AreEquivalent( new[] { 1, 2, 3, 4 }, ChatDoorHelper.PeopleToReadForRemoval( 1, 2, 3, leaders ) );
        }

        #endregion Who a removal asks the projection about
    }
}
