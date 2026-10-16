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

using Rock.Communication.Chat;
using Rock.Tests.Shared;

namespace Rock.Tests.Communication.Chat
{
    [TestCategory( TestFeatures.Communications )]
    [TestClass]
    public class ChatHelperTests
    {
        #region GetGroupId

        [TestMethod]
        [DataRow( "rock-group-5", 5 )]
        [DataRow( "rock-group-123", 123 )]
        [DataRow( "rock-group-2147483647", 2147483647 )]
        public void GetGroupId_WithExactRockKey_ReturnsGroupId( string chatChannelKey, int expectedGroupId )
        {
            var groupId = ChatHelper.GetGroupId( chatChannelKey );

            Assert.AreEqual( expectedGroupId, groupId );
        }

        [TestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( " " )]
        [DataRow( "5" )]
        [DataRow( "rock-group-rock-group-5" )]
        [DataRow( "12rock-group-3" )]
        [DataRow( "x-rock-group-5" )]
        [DataRow( "rock-group-05" )]
        [DataRow( "rock-group-+5" )]
        [DataRow( "rock-group- 5" )]
        [DataRow( "rock-group-5 " )]
        [DataRow( "rock-group-0" )]
        [DataRow( "rock-group-" + "-5" )] // Negative group ID.
        [DataRow( "ROCK-GROUP-5" )]
        [DataRow( "rock-group-" )]
        [DataRow( "!members-abc123" )]
        public void GetGroupId_WithNonRockKey_ReturnsNull( string chatChannelKey )
        {
            var groupId = ChatHelper.GetGroupId( chatChannelKey );

            Assert.IsNull( groupId );
        }

        [TestMethod]
        public void DidChatChannelOriginateInRock_WithBareNumber_ReturnsFalse()
        {
            Assert.IsFalse( ChatHelper.DidChatChannelOriginateInRock( "5" ) );
            Assert.IsTrue( ChatHelper.DidChatChannelOriginateInRock( "rock-group-5" ) );
        }

        #endregion

        #region IsChatChannelForGroup

        [TestMethod]
        public void IsChatChannelForGroup_WithExactTypeAndKey_ReturnsTrue()
        {
            Assert.IsTrue( ChatHelper.IsChatChannelForGroup( 5, 25, null, "rock-grouptype-25", "rock-group-5" ) );
        }

        [TestMethod]
        public void IsChatChannelForGroup_WithStoredKey_ReturnsTrue()
        {
            Assert.IsTrue( ChatHelper.IsChatChannelForGroup( 5, 30, "!members-abc123", "rock-grouptype-30", "!members-abc123" ) );
        }

        [TestMethod]
        public void IsChatChannelForGroup_WithWrongType_ReturnsFalse()
        {
            Assert.IsFalse( ChatHelper.IsChatChannelForGroup( 5, 25, null, "rock-grouptype-30", "rock-group-5" ) );
        }

        [TestMethod]
        public void IsChatChannelForGroup_WithBareNumberAlias_ReturnsFalse()
        {
            Assert.IsFalse( ChatHelper.IsChatChannelForGroup( 5, 25, null, "rock-grouptype-25", "5" ) );
        }

        [TestMethod]
        public void IsChatChannelForGroup_WithRockKeyWhenGroupHasStoredKey_ReturnsFalse()
        {
            Assert.IsFalse( ChatHelper.IsChatChannelForGroup( 5, 30, "!members-abc123", "rock-grouptype-30", "rock-group-5" ) );
        }

        [TestMethod]
        public void IsChatChannelForGroup_WithDifferentCase_ReturnsFalse()
        {
            Assert.IsFalse( ChatHelper.IsChatChannelForGroup( 5, 30, "!members-abc123", "rock-grouptype-30", "!MEMBERS-ABC123" ) );
            Assert.IsFalse( ChatHelper.IsChatChannelForGroup( 5, 25, null, "ROCK-GROUPTYPE-25", "rock-group-5" ) );
        }

        [TestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( " " )]
        public void IsChatChannelForGroup_WithMissingType_ReturnsFalse( string chatChannelTypeKey )
        {
            Assert.IsFalse( ChatHelper.IsChatChannelForGroup( 5, 25, null, chatChannelTypeKey, "rock-group-5" ) );
        }

        [TestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        public void IsChatChannelForGroup_WithMissingKey_ReturnsFalse( string chatChannelKey )
        {
            Assert.IsFalse( ChatHelper.IsChatChannelForGroup( 5, 25, null, "rock-grouptype-25", chatChannelKey ) );
        }

        #endregion
    }
}
