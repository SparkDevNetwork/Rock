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
using System.Data;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Sync;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The immediate push's body, as Rock writes it from a scoped projection call.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The platform refuses a push whose keys are anything but the four sections and one key
    ///         naming the rows no longer in chat, so those names are wire. They come from the wire
    ///         contract, as the full sync's section names do, and never from strings typed here in
    ///         C#: a push built from typed names would pass every test on this side while the two
    ///         copies drifted apart. These tests hold the body to the shipped contract, and then
    ///         hand the writer a contract with every push name changed, which only a writer that
    ///         reads the names from the contract can follow.
    ///     </para>
    /// </remarks>
    [TestClass]
    public class ChatSyncPushBodyTests
    {
        private static readonly Guid AbsentChannel = new Guid( "5a9f3c1e-2b7d-4e60-9a8c-1d2e3f405a61" );

        private static readonly Guid AbsentAlias = new Guid( "6b0a4d2f-3c8e-4f71-8b9d-2e3f40516b72" );

        [TestMethod]
        public void APushBodyCarriesExactlyTheContractsPushKeys()
        {
            var contract = ChatSyncTestBody.ShippedContract();
            var push = contract["push"];
            Assert.IsNotNull( push, "the contract does not describe the push, so nothing here can be built from it" );

            var body = Read( contract ).Body;

            var expected = push["sections"].Select( s => ( string ) s ).Concat( new[] { ( string ) push["absent"]["key"] } ).ToList();
            CollectionAssert.AreEquivalent( expected, body.Properties().Select( p => p.Name ).ToList(),
                "the push carries the contract's sections and its absent key, and nothing else" );

            var absent = ( JObject ) body[( string ) push["absent"]["key"]];
            CollectionAssert.AreEquivalent( push["absent"]["sets"].Select( s => ( string ) s["name"] ).ToList(), absent.Properties().Select( p => p.Name ).ToList(),
                "the absent key holds exactly the contract's absent sets" );
        }

        [TestMethod]
        public void APushBodyWritesEachAbsentKeyByItsKeyColumns()
        {
            var contract = ChatSyncTestBody.ShippedContract();
            var push = contract["push"];
            var absent = Read( contract ).Body[( string ) push["absent"]["key"]];

            foreach ( var set in push["absent"]["sets"] )
            {
                var keyColumns = set["key_columns"].Select( c => ( string ) c ).ToList();
                var keys = ( JArray ) absent[( string ) set["name"]];

                Assert.AreEqual( 1, keys.Count, $"the one absent {set["name"]} key the projection named" );

                if ( keyColumns.Count == 1 )
                {
                    Assert.AreEqual( JTokenType.String, keys[0].Type, $"a key of one column travels as that value, in {set["name"]}" );
                    continue;
                }

                Assert.AreEqual( JTokenType.Array, keys[0].Type, $"a key of several columns travels as a positional array, in {set["name"]}" );
                Assert.AreEqual( keyColumns.Count, ( ( JArray ) keys[0] ).Count, $"with one value per key column, in {set["name"]}" );
            }
        }

        [TestMethod]
        public void APushBodyFollowsTheContractsNamesRatherThanNamesOfItsOwn()
        {
            var contract = ChatSyncTestBody.ShippedContract();
            var absent = contract["push"]["absent"];
            absent["key"] = "no_longer_in_chat";

            foreach ( var set in absent["sets"] )
            {
                set["name"] = ( string ) set["name"] + "_renamed";
            }

            var body = Read( contract ).Body;

            Assert.IsNull( body["absent"], "the writer named the absent key itself instead of reading it from the contract" );
            Assert.IsNotNull( body["no_longer_in_chat"], "the absent key the contract names is missing" );
            CollectionAssert.AreEquivalent( new[] { "channels_renamed", "members_renamed" },
                ( ( JObject ) body["no_longer_in_chat"] ).Properties().Select( p => p.Name ).ToList(),
                "the absent sets are named as the contract names them" );
        }

        #region Support

        /// <summary>
        /// Reads a push body from the result sets a scoped call returns: the moment, the four
        /// sections, the badges left out, and one absent key of each kind.
        /// </summary>
        private static ChatPlatformSyncHelper.PushBody Read( JObject contract )
        {
            var marks = new DataTable( "marks" );
            marks.Columns.Add( "read_at", typeof( DateTime ) );
            marks.Rows.Add( DateTime.UtcNow );

            var leftOut = ChatSyncTestBody.SectionTable( "badges left out", new[] { "badge_key", "name", "reason" } );

            var absentChannels = ChatSyncTestBody.SectionTable( "absent channels", new[] { "channel_id" } );
            ChatSyncTestBody.AddRow( absentChannels, new Dictionary<string, object> { { "channel_id", AbsentChannel } } );

            var absentMembers = ChatSyncTestBody.SectionTable( "absent members", new[] { "channel_id", "person_alias_guid" } );
            ChatSyncTestBody.AddRow( absentMembers, new Dictionary<string, object> { { "channel_id", AbsentChannel }, { "person_alias_guid", AbsentAlias } } );

            var resultSets = new[] { marks }
                .Concat( ChatSyncTestBody.ShippedResultSets() )
                .Concat( new[] { leftOut, absentChannels, absentMembers } )
                .ToArray();

            using ( var reader = new DataTableReader( resultSets ) )
            {
                return ChatPlatformSyncHelper.ReadPushBody( reader, contract );
            }
        }

        #endregion Support
    }
}
