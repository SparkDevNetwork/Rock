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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Jobs;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Tests the step between what Rock returns and what the wire carries.
    /// </summary>
    /// <remarks>
    /// Three columns change on the way across, and every one of them would be accepted by the far
    /// side if it were sent as Rock holds it, and be wrong. That is what makes this worth its own
    /// tests: none of these failures raises anything anywhere.
    /// </remarks>
    [TestClass]
    public class ChatSyncRowMapperTests
    {
        #region Methods

        /// <summary>
        /// Writes one row of a section, with the shipped procedure's columns, and returns the value
        /// under a named wire column.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="wireColumn">The wire column to read back.</param>
        /// <param name="values">Values for named result set columns; the rest are null.</param>
        /// <returns>The written value.</returns>
        private static JToken WrittenValue( string section, string wireColumn, IDictionary<string, object> values )
        {
            var contract = ChatSyncTestBody.ShippedContract();
            var sections = contract["payload"]["sections"].Select( s => s.Value<string>() ).ToList();
            var wireColumns = contract["tables"][sections.IndexOf( section )]["columns"].Select( c => c.Value<string>() ).ToList();

            var resultSets = ChatSyncTestBody.ShippedResultSets();
            ChatSyncTestBody.AddRow( resultSets[sections.IndexOf( section )], values );

            // Read back as written, so a time stays the text the platform receives.
            var body = JsonConvert.DeserializeObject<JObject>(
                ChatSyncTestBody.Write( contract, resultSets ),
                new JsonSerializerSettings { DateParseHandling = DateParseHandling.None } );

            return body[section][0][wireColumns.IndexOf( wireColumn )];
        }

        #endregion

        #region Badge keys

        /// <summary>
        /// The joined keys become a list, in the order the church configured them.
        /// </summary>
        [TestMethod]
        public void BadgeKeys_BecomeAListRatherThanStayingOneString()
        {
            var keys = ChatPlatformSync.ReadBadgeKeys( "c1000000-0000-4000-8000-000000000001,c1000000-0000-4000-8000-000000000002" );

            CollectionAssert.AreEqual(
                new[]
                {
                    new Guid( "c1000000-0000-4000-8000-000000000001" ),
                    new Guid( "c1000000-0000-4000-8000-000000000002" )
                },
                keys.ToArray(),
                "the badge keys did not come out as a list in the order they were given" );
        }

        /// <summary>
        /// A person with no badges has an empty list, never a missing one.
        /// </summary>
        /// <remarks>
        /// The column on the far side cannot hold null, and it is the difference between a person
        /// who holds no badge and a row that did not say.
        /// </remarks>
        [TestMethod]
        public void BadgeKeys_ForAPersonWithNone_AreAnEmptyListRatherThanNothing()
        {
            foreach ( var nothing in new object[] { null, "", "   ", DBNull.Value } )
            {
                var keys = ChatPlatformSync.ReadBadgeKeys( nothing );

                Assert.IsNotNull( keys, "a person with no badges produced no list at all" );
                Assert.AreEqual( 0, keys.Count, "a person with no badges produced a list with something in it" );
            }
        }

        /// <summary>
        /// A key that is not an identifier stops the submission rather than being dropped.
        /// </summary>
        /// <remarks>
        /// Dropping it would hand the church a badge that silently stops appearing, with the
        /// submission still accepted and nothing to look at.
        /// </remarks>
        [TestMethod]
        public void BadgeKeys_ThatAreNotIdentifiers_StopTheSubmissionHere()
        {
            var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                () => ChatPlatformSync.ReadBadgeKeys( "c1000000-0000-4000-8000-000000000001,not-an-identifier" ),
                "a badge key that is not an identifier was dropped, so the badge disappears with the submission still accepted" );

            StringAssert.Contains( thrown.Message, "not-an-identifier", "the failure does not name the value that could not be read" );
        }

        /// <summary>
        /// The keys reach the alias row as an array of identifiers.
        /// </summary>
        [TestMethod]
        public void BadgeKeys_ReachTheAliasRowAsAList()
        {
            var value = WrittenValue( "aliases", "badge_keys", new Dictionary<string, object>
            {
                { "badge_keys", "C1000000-0000-4000-8000-000000000001" }
            } );

            Assert.AreEqual( JTokenType.Array, value.Type, "the badge keys reached the row as something other than a list of identifiers" );
            Assert.AreEqual( "c1000000-0000-4000-8000-000000000001", value[0].Value<string>(), "the badge key in the row is not the one the result set carried" );
        }

        #endregion

        #region Ban expiry

        /// <summary>
        /// The ban expiry is converted out of the organisation's zone.
        /// </summary>
        /// <remarks>
        /// The zone here is seven hours behind UTC and does not observe daylight saving, so six in
        /// the evening on the church's clock is one in the morning the next day in UTC. Sent
        /// unchanged it would be read as six in the evening UTC, and the ban would lift seven hours
        /// early.
        /// </remarks>
        [TestMethod]
        public void BanExpiry_IsConvertedFromTheOrganisationsZoneToUtc()
        {
            var converted = ChatPlatformSync.ToUtc( new DateTime( 2026, 9, 21, 18, 0, 0, DateTimeKind.Unspecified ), ChatSyncTestBody.FixedOffsetZone() );

            Assert.IsTrue( converted.HasValue, "the ban expiry did not come out as a time" );
            Assert.AreEqual( DateTimeKind.Utc, converted.Value.Kind, "the ban expiry is not marked as UTC, so the writer cannot vouch for it" );
            Assert.AreEqual( new DateTime( 2026, 9, 22, 1, 0, 0, DateTimeKind.Utc ), converted.Value, "the ban expiry was not moved by the church's offset" );
        }

        /// <summary>
        /// The converted expiry is what reaches the member row.
        /// </summary>
        [TestMethod]
        public void BanExpiry_ReachesTheMemberRowInUtc()
        {
            var value = WrittenValue( "members", "ban_expires_at", new Dictionary<string, object>
            {
                { "ban_expires_at", new DateTime( 2026, 9, 21, 18, 0, 0, DateTimeKind.Unspecified ) }
            } );

            Assert.AreEqual( "2026-09-22T01:00:00.000000Z", value.Value<string>(), "the ban expiry on the wire was not moved by the church's offset" );
        }

        /// <summary>
        /// A ban with no expiry has none on the wire either.
        /// </summary>
        [TestMethod]
        public void BanExpiry_WhenThereIsNone_StaysAbsent()
        {
            foreach ( var nothing in new object[] { null, DBNull.Value } )
            {
                Assert.IsNull( ChatPlatformSync.ToUtc( nothing, ChatSyncTestBody.FixedOffsetZone() ), "a ban that does not expire was given an expiry" );
            }

            var value = WrittenValue( "members", "ban_expires_at", null );

            Assert.AreEqual( JTokenType.Null, value.Type, "a ban that does not expire was given an expiry on the wire" );
        }

        #endregion

        #region Badge colours

        /// <summary>
        /// A dark badge is written on in white and a light one in black.
        /// </summary>
        [TestMethod]
        public void BadgeColours_PickTheForegroundThatReadsAgainstTheBackground()
        {
            var dark = ChatPlatformSync.ReadBadgeColors( "#1B4D3E" );

            Assert.AreEqual( "#1b4d3e", dark.Item1, "the background is not the configured colour" );
            Assert.AreEqual( "#ffffff", dark.Item2, "a dark badge is not written on in white" );

            var light = ChatPlatformSync.ReadBadgeColors( "#FFE08A" );

            Assert.AreEqual( "#ffe08a", light.Item1, "the background is not the configured colour" );
            Assert.AreEqual( "#000000", light.Item2, "a light badge is not written on in black" );
        }

        /// <summary>
        /// The three digit form is expanded, because the far side accepts only the six digit one.
        /// </summary>
        [TestMethod]
        public void BadgeColours_InTheShortForm_AreExpanded()
        {
            var colors = ChatPlatformSync.ReadBadgeColors( "#ABC" );

            Assert.AreEqual( "#aabbcc", colors.Item1, "the three digit form was not expanded, so the far side refuses it" );
        }

        /// <summary>
        /// A colour this cannot read leaves the badge uncoloured rather than failing the church.
        /// </summary>
        /// <remarks>
        /// The field is free text in Rock, so a church can put a colour name or anything else in
        /// it. A badge with no colour still renders; a submission refused over one badge takes the
        /// whole church down for that cycle.
        /// </remarks>
        [TestMethod]
        public void BadgeColours_ThatCannotBeRead_LeaveTheBadgeUncoloured()
        {
            foreach ( var unreadable in new object[] { null, DBNull.Value, "", "cornflowerblue", "rgb(1,2,3)", "#12345", "#GGGGGG" } )
            {
                var colors = ChatPlatformSync.ReadBadgeColors( unreadable );

                Assert.IsNull( colors.Item1, string.Format( "a colour that cannot be read produced a background: {0}", unreadable ?? "(null)" ) );
                Assert.IsNull( colors.Item2, string.Format( "a colour that cannot be read produced a foreground: {0}", unreadable ?? "(null)" ) );
            }
        }

        /// <summary>
        /// Both halves of the pair reach the badge row.
        /// </summary>
        [TestMethod]
        public void BadgeColours_ReachTheBadgeRowAsTwoColumns()
        {
            var values = new Dictionary<string, object> { { "highlight_color", "#1B4D3E" } };

            Assert.AreEqual( "#1b4d3e", WrittenValue( "badges", "bg_color", values ).Value<string>(), "the background did not reach the badge row" );
            Assert.AreEqual( "#ffffff", WrittenValue( "badges", "fg_color", values ).Value<string>(), "the foreground did not reach the badge row" );
        }

        #endregion

        #region Shape

        /// <summary>
        /// A written row is as wide as the contract says.
        /// </summary>
        [TestMethod]
        public void WrittenRows_AreAsWideAsTheContract()
        {
            var contract = ChatSyncTestBody.ShippedContract();
            var sections = contract["payload"]["sections"].Select( s => s.Value<string>() ).ToList();
            var resultSets = ChatSyncTestBody.ShippedResultSets();

            foreach ( var table in resultSets )
            {
                ChatSyncTestBody.AddRow( table, null );
            }

            var body = JObject.Parse( ChatSyncTestBody.Write( contract, resultSets ) );

            for ( var i = 0; i < sections.Count; i++ )
            {
                Assert.AreEqual( contract["tables"][i]["columns"].Count(), body[sections[i]][0].Count(), string.Format( "the {0} section writes a row of the wrong width", sections[i] ) );
            }
        }

        /// <summary>
        /// A wire column the result set no longer returns stops the submission, naming the column.
        /// </summary>
        /// <remarks>
        /// It means the procedure and the contract have drifted apart. Emitting null for the
        /// missing column would keep the width right and put every later value in the correct
        /// place, so nothing downstream could tell that a column had silently become empty for
        /// every row.
        /// </remarks>
        [TestMethod]
        public void WireColumn_WithNothingBehindIt_StopsTheSubmissionHere()
        {
            AssertRefusedNaming( "is_leader", columns => columns.Where( c => c != "is_leader" ) );
        }

        /// <summary>
        /// A wire column whose source the procedure renamed stops the submission, naming the column
        /// it no longer finds.
        /// </summary>
        [TestMethod]
        public void WireColumn_WhoseSourceWasRenamed_StopsTheSubmissionHere()
        {
            AssertRefusedNaming( "is_leader", columns => columns.Select( c => c == "is_leader" ? "is_group_leader" : c ) );
        }

        /// <summary>
        /// A colour pair whose one source the procedure no longer returns stops the submission,
        /// naming that source.
        /// </summary>
        [TestMethod]
        public void ColourPair_WithNoHighlightColour_StopsTheSubmissionHere()
        {
            var resultSets = ChatSyncTestBody.ShippedResultSets();
            resultSets[3] = ChatSyncTestBody.SectionTable( "badges", ChatSyncSqlText.SectionColumns( "badges" ).Where( c => c != "highlight_color" ) );

            var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                () => ChatSyncTestBody.Write( ChatSyncTestBody.ShippedContract(), resultSets ),
                "a badge row with no colour behind it was written with its colours silently empty" );

            StringAssert.Contains( thrown.Message, "highlight_color", "the failure does not name the column that has nothing behind it" );
        }

        /// <summary>
        /// Writes a body whose members result set has had its columns changed, and asserts it is
        /// refused with the named column in the message.
        /// </summary>
        /// <param name="column">The column the failure must name.</param>
        /// <param name="change">How the members columns are changed.</param>
        private static void AssertRefusedNaming( string column, Func<IList<string>, IEnumerable<string>> change )
        {
            var resultSets = ChatSyncTestBody.ShippedResultSets();
            resultSets[2] = ChatSyncTestBody.SectionTable( "members", change( ChatSyncSqlText.SectionColumns( "members" ) ) );
            ChatSyncTestBody.AddRow( resultSets[2], null );

            var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                () => ChatSyncTestBody.Write( ChatSyncTestBody.ShippedContract(), resultSets ),
                "a wire column the result set no longer returns was filled in silently" );

            StringAssert.Contains( thrown.Message, column, "the failure does not name the column that has nothing behind it" );
            StringAssert.Contains( thrown.Message, "members", "the failure does not name the section" );
        }

        #endregion
    }
}
