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
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Contract;
using Rock.Communication.Chat.Platform.Sync;
using Rock.Jobs;
using Rock.Enums.Communication.Chat;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Tests the values the projection can put in the wire's two enumerated channel columns.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Rock numbers its notification modes and the wire names them, so the mapping between the two
    /// is the thing that can be wrong, and it is written in the channels query as a CASE rather than
    /// anywhere a compiler could see. A value the wire does not list fails the whole church at the
    /// far side, not the one row, because the restatement is applied in one transaction. These
    /// cells read the arms of that CASE out of the shipped query and hold them member by member
    /// against Rock's own enumeration and against the list the contract publishes.
    /// </para>
    /// <para>
    /// The channel type is asserted as a subset rather than as equality: Rock ships group types for
    /// a direct message and a shared channel and none for a livestream, so the third value the
    /// contract lists is one this projection never emits.
    /// </para>
    /// </remarks>
    [TestClass]
    public class ChatSyncEnumMappingTests
    {
        #region Methods

        /// <summary>
        /// The arms of the CASE expression the channels query returns under one wire column.
        /// </summary>
        /// <param name="wireColumn">The column, as the contract names it.</param>
        /// <returns>The text between CASE and END.</returns>
        private static string CaseFor( string wireColumn )
        {
            var sql = ChatPlatformSync.GetSectionSql( "channels" );

            var match = Regex.Match(
                sql,
                @"\bCASE\b(?<arms>(?:(?!\bCASE\b).)*?)\bEND\s+AS\s+\[" + Regex.Escape( wireColumn ) + @"\]",
                RegexOptions.Singleline | RegexOptions.IgnoreCase );

            Assert.IsTrue( match.Success, string.Format( "the channels query does not build {0} with a CASE this test can read", wireColumn ) );

            return match.Groups["arms"].Value;
        }

        /// <summary>
        /// The wire values the contract lists for one enumerated type.
        /// </summary>
        /// <param name="enumType">The type, as the contract names it.</param>
        /// <returns>The values.</returns>
        private static string[] ContractValues( string enumType )
        {
            var listed = JObject.Parse( ChatWireContract.Json )["enum_types"]
                .Children<JObject>()
                .FirstOrDefault( e => e["name"].Value<string>() == enumType );

            Assert.IsNotNull( listed, string.Format( "the contract lists no {0} type", enumType ) );

            return listed["values"].Select( v => v.Value<string>() ).ToArray();
        }

        /// <summary>
        /// The notify mode CASE, as what each numbered arm yields and what the ELSE yields.
        /// </summary>
        /// <param name="byNumber">The value each WHEN arm yields, by the number it tests.</param>
        /// <param name="otherwise">The value the ELSE arm yields, or null where there is none.</param>
        private static void ReadNotifyModeArms( out IDictionary<int, string> byNumber, out string otherwise )
        {
            var arms = CaseFor( "notify_mode_default" );

            byNumber = Regex.Matches( arms, @"\bWHEN\s+(?<number>\d+)\s+THEN\s+'(?<value>[a-z_]+)'", RegexOptions.IgnoreCase )
                .Cast<Match>()
                .ToDictionary( m => int.Parse( m.Groups["number"].Value ), m => m.Groups["value"].Value );

            var fallback = Regex.Match( arms, @"\bELSE\s+'(?<value>[a-z_]+)'", RegexOptions.IgnoreCase );
            otherwise = fallback.Success ? fallback.Groups["value"].Value : null;

            Assert.IsTrue( byNumber.Any() || otherwise != null, "the notify mode CASE has no arms this test can read" );
        }

        /// <summary>
        /// What the query emits for one member of Rock's enumeration.
        /// </summary>
        private static string WireValueFor( ChatNotificationMode mode, IDictionary<int, string> byNumber, string otherwise )
        {
            string value;
            return byNumber.TryGetValue( ( int ) mode, out value ) ? value : otherwise;
        }

        /// <summary>
        /// Every member of Rock's enumeration.
        /// </summary>
        private static IList<ChatNotificationMode> RockModes()
        {
            return Enum.GetValues( typeof( ChatNotificationMode ) ).Cast<ChatNotificationMode>().ToList();
        }

        #endregion

        #region Notify mode

        /// <summary>
        /// Each member of Rock's enumeration reaches the wire as the value that means the same
        /// thing. Written out member by member, so a renumbering on either side turns this red.
        /// </summary>
        [TestMethod]
        public void NotifyMode_MapsEachRockMemberToTheWireValueThatMeansTheSameThing()
        {
            IDictionary<int, string> byNumber;
            string otherwise;
            ReadNotifyModeArms( out byNumber, out otherwise );

            var expected = new Dictionary<ChatNotificationMode, string>
            {
                { ChatNotificationMode.AllMessages, "all" },
                { ChatNotificationMode.Mentions, "mentions" },
                { ChatNotificationMode.Silent, "silent" }
            };

            foreach ( var member in RockModes() )
            {
                Assert.IsTrue( expected.ContainsKey( member ), string.Format( "Rock gained the notification mode {0} and this test does not say what it means on the wire", member ) );
                Assert.AreEqual( expected[member], WireValueFor( member, byNumber, otherwise ), string.Format( "the channels query sends {0} as something other than {1}", member, expected[member] ) );
            }
        }

        /// <summary>
        /// Nothing the query can emit is outside the contract's list, because an unlisted value
        /// fails the whole church rather than one row.
        /// </summary>
        [TestMethod]
        public void NotifyMode_EveryValueTheQueryCanEmit_IsOneTheContractLists()
        {
            IDictionary<int, string> byNumber;
            string otherwise;
            ReadNotifyModeArms( out byNumber, out otherwise );

            var emitted = byNumber.Values
                .Concat( otherwise == null ? Enumerable.Empty<string>() : new[] { otherwise } )
                .Distinct()
                .ToArray();

            var listed = ContractValues( "chat_notify_mode" );

            foreach ( var value in emitted )
            {
                CollectionAssert.Contains( listed, value, string.Format( "the channels query can send {0}, which the contract does not list, so a channel set that way fails this church's whole submission", value ) );
            }
        }

        /// <summary>
        /// Rock's members reach the wire as distinct values, and together they are exactly the
        /// contract's list, so neither side names a mode the other cannot express.
        /// </summary>
        [TestMethod]
        public void NotifyMode_TheRockMembersCoverTheContractsListExactlyOnce()
        {
            IDictionary<int, string> byNumber;
            string otherwise;
            ReadNotifyModeArms( out byNumber, out otherwise );

            var members = RockModes();
            var values = members.Select( m => WireValueFor( m, byNumber, otherwise ) ).ToList();

            Assert.AreEqual( members.Count, values.Distinct().Count(), "two notification modes reach the wire as the same value, so they cannot be told apart on the far side" );
            CollectionAssert.AreEquivalent(
                ContractValues( "chat_notify_mode" ),
                values.Distinct().ToArray(),
                "the values Rock's modes reach the wire as are not the contract's list, so one side names a mode the other cannot express" );
        }

        /// <summary>
        /// Every number the CASE tests is a member Rock actually has, so an arm left behind by a
        /// renumbering cannot sit there matching nothing.
        /// </summary>
        [TestMethod]
        public void NotifyMode_TestsNoNumberThatIsNotARockMember()
        {
            IDictionary<int, string> byNumber;
            string otherwise;
            ReadNotifyModeArms( out byNumber, out otherwise );

            foreach ( var number in byNumber.Keys )
            {
                Assert.IsTrue( Enum.IsDefined( typeof( ChatNotificationMode ), number ), string.Format( "the channels query tests for notification mode {0}, which Rock does not have", number ) );
            }
        }

        #endregion

        #region Channel type

        /// <summary>
        /// Nothing the query can emit is outside the contract's list.
        /// </summary>
        [TestMethod]
        public void ChannelType_EveryValueTheQueryCanEmit_IsOneTheContractLists()
        {
            var arms = CaseFor( "channel_type" );

            var emitted = Regex.Matches( arms, @"'(?<value>[a-z_]+)'" )
                .Cast<Match>()
                .Select( m => m.Groups["value"].Value )
                .Distinct()
                .ToArray();

            Assert.IsTrue( emitted.Any(), "the channel type CASE yields no value this test can read" );

            var listed = ContractValues( "chat_channel_type" );

            foreach ( var value in emitted )
            {
                CollectionAssert.Contains( listed, value, string.Format( "the channels query can send the channel type {0}, which the contract does not list", value ) );
            }
        }

        /// <summary>
        /// The direct message group type is the one that is a direct message, and everything else
        /// that qualifies is a shared channel. Livestream is not emitted, because Rock ships no
        /// group type for it.
        /// </summary>
        [TestMethod]
        public void ChannelType_IsDmForTheDirectMessageGroupTypeAndSharedForEveryOther()
        {
            var arms = CaseFor( "channel_type" );

            StringAssert.Matches(
                arms,
                new Regex( @"\bWHEN\s+\[GT\]\.\[Guid\]\s*=\s*@DirectMessageGroupTypeGuid\s+THEN\s+'dm'", RegexOptions.IgnoreCase ),
                "the direct message group type does not project as dm" );
            StringAssert.Matches(
                arms,
                new Regex( @"\bELSE\s+'shared'", RegexOptions.IgnoreCase ),
                "a channel that is not a direct message does not project as shared" );
            Assert.IsFalse( arms.IndexOf( "'livestream'", StringComparison.OrdinalIgnoreCase ) >= 0, "the channels query emits livestream, which Rock has no group type for" );
        }

        #endregion
    }
}
