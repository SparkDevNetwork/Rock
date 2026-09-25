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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The groups Rock ships to run chat are never chat channels themselves.
    /// </summary>
    /// <remarks>
    /// Chat People, the ban list and the chat administrators sit in group types an administrator
    /// can turn chat on for. A group's own chat setting is one edit away, and once a group is
    /// marked it stays a channel for good, so the ban list would become a room of banned people.
    /// </remarks>
    [TestClass]
    public class ProjectionSystemGroupTests : DatabaseTestsBase
    {
        [TestMethod]
        public void TheBanList_WithChatOnForItsGroupType_IsNotMarkedOrProjected()
        {
            AssertNeverAChannel( Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST.AsGuid() );
        }

        [TestMethod]
        public void TheAdministratorsGroup_WithChatOnForItsGroupType_IsNotMarkedOrProjected()
        {
            AssertNeverAChannel( Rock.SystemGuid.Group.GROUP_CHAT_ADMINISTRATORS.AsGuid() );
        }

        [TestMethod]
        public void ChatPeople_WithChatOnForItsGroupType_IsNotMarkedOrProjected()
        {
            AssertNeverAChannel( Rock.SystemGuid.Group.GROUP_CHAT_PEOPLE.AsGuid() );
        }

        [TestMethod]
        public void TheBanList_MarkedByAnEarlierRun_IsNotProjected()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var banListGuid = Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST.AsGuid();
                fixture.MarkChannelDirectly( banListGuid );

                Assert.IsNull( fixture.Project().Row( "channels", "channel_id", banListGuid ),
                    "a marked ban list was sent to the chat platform as a channel" );
            }
        }

        #region Support

        private static void AssertNeverAChannel( Guid systemGroupGuid )
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var groupTypeId = fixture.EnableChatForGroupTypeOf( systemGroupGuid );

                // Another group of the same type still qualifies, so the rule is not off for the type.
                var otherGuid = fixture.AddChannel( groupTypeId, "Same type as a system group" );

                var payload = fixture.Project();

                Assert.IsNull( fixture.ChannelMark( systemGroupGuid ), "a system chat group was marked as a channel" );
                Assert.IsNull( payload.Row( "channels", "channel_id", systemGroupGuid ), "a system chat group was sent to the chat platform as a channel" );
                Assert.IsNotNull( payload.Row( "channels", "channel_id", otherGuid ), "an ordinary group of the same type stopped being a channel" );
            }
        }

        #endregion Support
    }
}
