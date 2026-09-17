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

using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Contract;
using Rock.Communication.Chat.Platform.Sync;
using Rock.Enums.Communication.Chat;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The restatement body and headers are built from the vendored contract,
    /// not from strings typed beside it.
    /// </summary>
    [TestClass]
    public class ChatSyncPayloadWriterTests
    {
        [TestMethod]
        public void BuildCounts_WalksTheArtifactKeys_NotTheTableNames()
        {
            var lengths = new Dictionary<string, int>
            {
                { "aliases", 3 },
                { "channels", 1 },
                { "members", 2 },
                { "badges", 0 },
                { "chat_aliases", 99 }
            };

            var counts = ChatSyncPayloadWriter.BuildCounts( lengths );

            CollectionAssert.AreEqual( ChatWireContract.CountKeys.ToArray(), counts.Keys.ToArray() );
            Assert.AreEqual( 3, counts["aliases"] );
            Assert.AreEqual( 0, counts["badges"] );
            Assert.IsFalse( counts.ContainsKey( "chat_aliases" ) );
        }

        [TestMethod]
        public void BuildMarks_WalksTheArtifactKeys()
        {
            var identity = new Dictionary<string, long>
            {
                { "person", 10 },
                { "person_alias", 20 },
                { "group", 30 },
                { "group_member", 40 }
            };

            var marks = ChatSyncPayloadWriter.BuildMarks( identity );

            CollectionAssert.AreEqual( ChatWireContract.MarksKeys.ToArray(), marks.Keys.ToArray() );
            Assert.AreEqual( 10L, marks["person"] );
            Assert.AreEqual( 40L, marks["group_member"] );
        }

        [TestMethod]
        public void WriteBody_IsAnObjectKeyedBySectionNames_WithPositionalRows()
        {
            var guid = Guid.Parse( "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa" );
            var sections = new Dictionary<string, IList<IDictionary<string, object>>>
            {
                {
                    "aliases",
                    new List<IDictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            { "person_alias_guid", guid },
                            { "primary_person_alias_guid", guid },
                            { "nick_name", "Ada" },
                            { "last_name", "Lovelace" },
                            { "avatar_url", null },
                            { "campus_id", null },
                            { "badge_keys", new[] { Guid.Parse( "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb" ) } },
                            { "show_profile_details", true },
                            { "is_open_dm_allowed", false },
                            { "is_globally_banned", false },
                            { "is_inactive", false }
                        }
                    }
                }
            };

            var json = JObject.Parse( ChatSyncPayloadWriter.WriteBody( sections ) );

            CollectionAssert.AreEqual( ChatWireContract.SectionKeys.ToArray(), json.Properties().Select( p => p.Name ).ToArray() );
            Assert.AreEqual( JTokenType.Array, json["aliases"][0].Type );
            Assert.AreEqual( ChatWireContract.ColumnsOf( "chat_aliases" ).Count, ( ( JArray ) json["aliases"][0] ).Count );
            Assert.AreEqual( "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa", json["aliases"][0][0].Value<string>() );
            Assert.AreEqual( "Ada", json["aliases"][0][2].Value<string>() );
            Assert.AreEqual( true, json["aliases"][0][7].Value<bool>() );
            Assert.AreEqual( 0, ( ( JArray ) json["channels"] ).Count );
        }

        [TestMethod]
        public void TruncateToMicroseconds_DropsTheSubMicrosecondTicks()
        {
            var input = new DateTime( 2026, 9, 17, 12, 0, 0, DateTimeKind.Utc ).AddTicks( 7 );
            var truncated = ChatSyncPayloadWriter.TruncateToMicroseconds( input );

            Assert.AreEqual( 0, truncated.Ticks % 10 );
            Assert.AreEqual( input.Ticks - 7, truncated.Ticks );
            StringAssert.EndsWith( ChatSyncPayloadWriter.FormatReadAt( input ), "Z" );
        }

        [TestMethod]
        public void HeaderHash_IsTheVendoredComputedHash()
        {
            Assert.AreEqual( ChatWireContract.PublishedHash, ChatWireContract.ComputedHash );
            Assert.AreEqual( 64, ChatWireContract.ComputedHash.Length );
        }

        [TestMethod]
        public void NotifyMode_EveryRockValue_IsInTheVendoredList()
        {
            var accepted = ChatWireContract.EnumValues( "chat_notify_mode" );

            foreach ( ChatNotificationMode mode in Enum.GetValues( typeof( ChatNotificationMode ) ) )
            {
                var wire = ChatSyncPayloadWriter.NotifyMode( mode );
                Assert.IsTrue( accepted.Contains( wire ), string.Format( "{0} maps to {1}, which the contract does not list", mode, wire ) );
            }

            Assert.AreEqual( "all", ChatSyncPayloadWriter.NotifyMode( ChatNotificationMode.AllMessages ) );
            Assert.AreEqual( "mentions", ChatSyncPayloadWriter.NotifyMode( ChatNotificationMode.Mentions ) );
            Assert.AreEqual( "silent", ChatSyncPayloadWriter.NotifyMode( ChatNotificationMode.Silent ) );
        }
    }
}
