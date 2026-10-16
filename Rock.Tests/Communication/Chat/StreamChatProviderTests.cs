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

using Newtonsoft.Json.Linq;

using Rock.Communication.Chat;
using Rock.Communication.Chat.Exceptions;
using Rock.Tests.Shared;

namespace Rock.Tests.Communication.Chat
{
    [TestCategory( TestFeatures.Communications )]
    [TestClass]
    public class StreamChatProviderTests
    {
        #region TryGetChatGroupIdentifiers

        [TestMethod]
        public void TryGetChatGroupIdentifiers_WithChannelType_ReturnsChannelType()
        {
            var json = JObject.Parse( @"{ ""channel_id"": ""rock-group-5"", ""channel_type"": ""rock-grouptype-25"", ""cid"": ""rock-grouptype-25:rock-group-5"" }" );

            var identifiers = StreamChatProvider.TryGetChatGroupIdentifiers( json );

            Assert.AreEqual( "rock-group-5", identifiers.ChannelKey );
            Assert.AreEqual( "rock-grouptype-25", identifiers.ChannelTypeKey );
            Assert.AreEqual( 5, identifiers.GroupId );
        }

        [TestMethod]
        public void TryGetChatGroupIdentifiers_WithChannelTypeAndOtherCid_ReturnsChannelType()
        {
            var json = JObject.Parse( @"{ ""channel_id"": ""rock-group-5"", ""channel_type"": ""rock-grouptype-25"", ""cid"": ""messaging:rock-group-5"" }" );

            var identifiers = StreamChatProvider.TryGetChatGroupIdentifiers( json );

            Assert.AreEqual( "rock-grouptype-25", identifiers.ChannelTypeKey );
        }

        [TestMethod]
        public void TryGetChatGroupIdentifiers_WithoutChannelTypeButMatchingCid_ReturnsCidType()
        {
            var json = JObject.Parse( @"{ ""channel_id"": ""rock-group-5"", ""cid"": ""rock-grouptype-25:rock-group-5"" }" );

            var identifiers = StreamChatProvider.TryGetChatGroupIdentifiers( json );

            Assert.AreEqual( "rock-group-5", identifiers.ChannelKey );
            Assert.AreEqual( "rock-grouptype-25", identifiers.ChannelTypeKey );
        }

        [TestMethod]
        public void TryGetChatGroupIdentifiers_WithBlankChannelTypeButMatchingCid_ReturnsCidType()
        {
            var json = JObject.Parse( @"{ ""channel_id"": ""!members-abc123"", ""channel_type"": "" "", ""cid"": ""rock-grouptype-30:!members-abc123"" }" );

            var identifiers = StreamChatProvider.TryGetChatGroupIdentifiers( json );

            Assert.AreEqual( "rock-grouptype-30", identifiers.ChannelTypeKey );
        }

        [TestMethod]
        public void TryGetChatGroupIdentifiers_WithoutChannelTypeOrCid_Throws()
        {
            var json = JObject.Parse( @"{ ""channel_id"": ""rock-group-5"" }" );

            Assert.ThrowsExactly<ChatWebhookParseException>( () => StreamChatProvider.TryGetChatGroupIdentifiers( json ) );
        }

        [TestMethod]
        [DataRow( "rock-grouptype-25:rock-group-6" )]
        [DataRow( "rock-grouptype-25:ROCK-GROUP-5" )]
        [DataRow( "rock-grouptype-25:rock-group-5:x" )]
        [DataRow( "rock-grouptype-25:" )]
        [DataRow( ":rock-group-5" )]
        [DataRow( " :rock-group-5" )]
        [DataRow( "rock-group-5" )]
        [DataRow( "rock-grouptype-25" )]
        [DataRow( "" )]
        public void TryGetChatGroupIdentifiers_WithoutChannelTypeAndMismatchedCid_Throws( string cid )
        {
            var json = new JObject
            {
                ["channel_id"] = "rock-group-5",
                ["cid"] = cid
            };

            Assert.ThrowsExactly<ChatWebhookParseException>( () => StreamChatProvider.TryGetChatGroupIdentifiers( json ) );
        }

        [TestMethod]
        public void TryGetChatGroupIdentifiers_WithoutChannelTypeAndMismatchedCidWhenNotRequired_ReturnsNoType()
        {
            var json = JObject.Parse( @"{ ""channel_id"": ""rock-group-5"", ""cid"": ""rock-grouptype-25:rock-group-6"" }" );
            var config = new StreamChatProvider.TryGetChatGroupIdentifiersConfig { ShouldThrowIfMissing = false };

            var identifiers = StreamChatProvider.TryGetChatGroupIdentifiers( json, config );

            Assert.AreEqual( "rock-group-5", identifiers.ChannelKey );
            Assert.IsNull( identifiers.ChannelTypeKey );
        }

        [TestMethod]
        public void TryGetChatGroupIdentifiers_WithMuteChannelWithoutTypeButMatchingCid_ReturnsCidType()
        {
            var json = JObject.Parse( @"{ ""id"": ""rock-group-5"", ""cid"": ""rock-grouptype-25:rock-group-5"" }" );
            var config = new StreamChatProvider.TryGetChatGroupIdentifiersConfig
            {
                ChannelIdProperty = "id",
                ChannelIdPath = "mute.channel.id",
                ChannelTypeProperty = "type",
                ChannelTypePath = "mute.channel.type"
            };

            var identifiers = StreamChatProvider.TryGetChatGroupIdentifiers( json, config );

            Assert.AreEqual( "rock-grouptype-25", identifiers.ChannelTypeKey );
        }

        [TestMethod]
        public void TryGetChatGroupIdentifiers_WithMuteChannelWithoutTypeOrMatchingCid_Throws()
        {
            var json = JObject.Parse( @"{ ""id"": ""rock-group-5"", ""cid"": ""rock-grouptype-25:rock-group-6"" }" );
            var config = new StreamChatProvider.TryGetChatGroupIdentifiersConfig
            {
                ChannelIdProperty = "id",
                ChannelIdPath = "mute.channel.id",
                ChannelTypeProperty = "type",
                ChannelTypePath = "mute.channel.type"
            };

            Assert.ThrowsExactly<ChatWebhookParseException>( () => StreamChatProvider.TryGetChatGroupIdentifiers( json, config ) );
        }

        #endregion
    }
}
