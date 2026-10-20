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
    }
}
