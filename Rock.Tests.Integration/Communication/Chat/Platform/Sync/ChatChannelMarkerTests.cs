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
    /// The mark that says a group was a chat channel once.
    /// </summary>
    /// <remarks>
    /// It is what makes turning chat off on a group archive its conversation instead of losing it.
    /// A marked group keeps being sent whatever its own settings later say, so the platform keeps
    /// the channel and its history rather than reading the group as gone. It is written once and
    /// never moved, so a group that had chat, lost it and got it back keeps the date it first had
    /// it.
    /// </remarks>
    [TestClass]
    public class ChatChannelMarkerTests : DatabaseTestsBase
    {
        [TestMethod]
        public void AQualifyingGroupIsMarkedOnce_AndASecondRunDoesNotMoveIt()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Marker channel" );

                Assert.IsNull( fixture.ChannelMark( channelGuid ), "the group was marked before anything ran" );

                fixture.StampChannels();
                var first = fixture.ChannelMark( channelGuid );

                Assert.IsNotNull( first, "a group that is a chat channel was not marked" );

                fixture.StampChannels();

                Assert.AreEqual( first, fixture.ChannelMark( channelGuid ),
                    "the mark moved, so the record of when this group first had chat is whatever the last run wrote" );
            }
        }

        [TestMethod]
        public void AGroupWhoseChatIsTurnedOffAfterwards_IsStillProjected()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Retired marker channel" );

                fixture.StampChannels();
                fixture.EditChannel( channelGuid, group => group.IsChatEnabledOverride = false );

                var payload = fixture.Project();

                Assert.IsNotNull( payload.Row( "channels", "channel_id", channelGuid ),
                    "a group that was a chat channel stopped being sent, which loses its conversation on the far side instead of archiving it" );
            }
        }

        [TestMethod]
        public void AGroupThatWasNeverAChatChannel_IsNotMarkedAndIsNotProjected()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Not a channel",
                    group => group.IsChatEnabledOverride = false );

                fixture.StampChannels();

                Assert.IsNull( fixture.ChannelMark( channelGuid ), "a group with chat switched off was marked as a channel" );
                Assert.IsNull( fixture.Project().Row( "channels", "channel_id", channelGuid ),
                    "a group that was never a chat channel was sent to the chat platform" );
            }
        }
    }
}
