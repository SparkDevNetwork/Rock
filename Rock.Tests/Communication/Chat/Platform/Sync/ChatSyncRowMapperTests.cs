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
        /// A zone that is not UTC and does not observe daylight saving, so the arithmetic in these
        /// tests is the same on every day of the year.
        /// </summary>
        /// <returns>The zone.</returns>
        private static TimeZoneInfo FixedOffsetZone()
        {
            return TimeZoneInfo.FindSystemTimeZoneById( "US Mountain Standard Time" );
        }

        /// <summary>
        /// A mapper for one church.
        /// </summary>
        /// <returns>The mapper.</returns>
        private static ChatSyncRowMapper Mapper()
        {
            return new ChatSyncRowMapper( JObject.Parse( ChatWireContract.Json ), FixedOffsetZone() );
        }

        /// <summary>
        /// The columns a section's query returns, and a value for each.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="overrides">Values to use for named columns; the rest are null.</param>
        /// <returns>The column names and the values.</returns>
        private static Tuple<IList<string>, IList<object>> Row( string section, IDictionary<string, object> overrides )
        {
            var columns = ChatSyncProjection.GetSectionColumns( section );
            var values = columns
                .Select( c => overrides != null && overrides.ContainsKey( c ) ? overrides[c] : null )
                .ToList();

            return Tuple.Create( columns, (IList<object>) values );
        }

        /// <summary>
        /// Maps one row and returns the value under a named wire column.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="wireColumn">The wire column to read back.</param>
        /// <param name="overrides">Values for named query columns.</param>
        /// <returns>The mapped value.</returns>
        private static object MappedValue( string section, string wireColumn, IDictionary<string, object> overrides )
        {
            var contract = JObject.Parse( ChatWireContract.Json );
            var sections = contract["payload"]["sections"].Select( s => s.Value<string>() ).ToList();
            var wireColumns = contract["tables"][sections.IndexOf( section )]["columns"].Select( c => c.Value<string>() ).ToList();

            var row = Row( section, overrides );
            var mapped = Mapper().Map( section, row.Item1, row.Item2 );

            return mapped[wireColumns.IndexOf( wireColumn )];
        }

        #endregion

        #region Badge keys

        /// <summary>
        /// The joined keys become a list, in the order the church configured them.
        /// </summary>
        [TestMethod]
        public void BadgeKeys_BecomeAListRatherThanStayingOneString()
        {
            var keys = ChatSyncRowMapper.ReadBadgeKeys( "c1000000-0000-4000-8000-000000000001,c1000000-0000-4000-8000-000000000002" );

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
                var keys = ChatSyncRowMapper.ReadBadgeKeys( nothing );

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
                () => ChatSyncRowMapper.ReadBadgeKeys( "c1000000-0000-4000-8000-000000000001,not-an-identifier" ),
                "a badge key that is not an identifier was dropped, so the badge disappears with the submission still accepted" );

            StringAssert.Contains( thrown.Message, "not-an-identifier", "the failure does not name the value that could not be read" );
        }

        /// <summary>
        /// The keys reach the alias row as a list.
        /// </summary>
        [TestMethod]
        public void BadgeKeys_ReachTheAliasRowAsAList()
        {
            var value = MappedValue( "aliases", "badge_keys", new Dictionary<string, object>
            {
                { "badge_keys", "c1000000-0000-4000-8000-000000000001" }
            } );

            Assert.IsInstanceOfType( value, typeof( IEnumerable<Guid> ), "the badge keys reached the row as something other than a list of identifiers" );
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
            var value = MappedValue( "members", "ban_expires_at", new Dictionary<string, object>
            {
                { "ban_expires_at", new DateTime( 2026, 9, 21, 18, 0, 0, DateTimeKind.Unspecified ) }
            } );

            Assert.IsInstanceOfType( value, typeof( DateTime ), "the ban expiry did not reach the row as a time" );

            var converted = (DateTime) value;

            Assert.AreEqual( DateTimeKind.Utc, converted.Kind, "the ban expiry is not marked as UTC, so the writer cannot vouch for it" );
            Assert.AreEqual( new DateTime( 2026, 9, 22, 1, 0, 0, DateTimeKind.Utc ), converted, "the ban expiry was not moved by the church's offset" );
        }

        /// <summary>
        /// A ban with no expiry has none on the wire either.
        /// </summary>
        [TestMethod]
        public void BanExpiry_WhenThereIsNone_StaysAbsent()
        {
            foreach ( var nothing in new object[] { null, DBNull.Value } )
            {
                var value = MappedValue( "members", "ban_expires_at", new Dictionary<string, object> { { "ban_expires_at", nothing } } );

                Assert.IsNull( value, "a ban that does not expire was given an expiry" );
            }
        }

        #endregion

        #region Badge colours

        /// <summary>
        /// A dark badge is written on in white and a light one in black.
        /// </summary>
        [TestMethod]
        public void BadgeColours_PickTheForegroundThatReadsAgainstTheBackground()
        {
            var dark = ChatSyncRowMapper.ReadBadgeColors( "#1B4D3E" );

            Assert.AreEqual( "#1b4d3e", dark.Item1, "the background is not the configured colour" );
            Assert.AreEqual( "#ffffff", dark.Item2, "a dark badge is not written on in white" );

            var light = ChatSyncRowMapper.ReadBadgeColors( "#FFE08A" );

            Assert.AreEqual( "#ffe08a", light.Item1, "the background is not the configured colour" );
            Assert.AreEqual( "#000000", light.Item2, "a light badge is not written on in black" );
        }

        /// <summary>
        /// The three digit form is expanded, because the far side accepts only the six digit one.
        /// </summary>
        [TestMethod]
        public void BadgeColours_InTheShortForm_AreExpanded()
        {
            var colors = ChatSyncRowMapper.ReadBadgeColors( "#ABC" );

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
                var colors = ChatSyncRowMapper.ReadBadgeColors( unreadable );

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
            var overrides = new Dictionary<string, object> { { "highlight_color", "#1B4D3E" } };

            Assert.AreEqual( "#1b4d3e", MappedValue( "badges", "bg_color", overrides ), "the background did not reach the badge row" );
            Assert.AreEqual( "#ffffff", MappedValue( "badges", "fg_color", overrides ), "the foreground did not reach the badge row" );
        }

        #endregion

        #region Shape

        /// <summary>
        /// A mapped row is as wide as the contract says and in its order.
        /// </summary>
        [TestMethod]
        public void MappedRows_AreAsWideAsTheContractAndInItsOrder()
        {
            var contract = JObject.Parse( ChatWireContract.Json );
            var sections = contract["payload"]["sections"].Select( s => s.Value<string>() ).ToList();

            for ( var i = 0; i < sections.Count; i++ )
            {
                var expectedWidth = contract["tables"][i]["columns"].Count();
                var row = Row( sections[i], null );
                var mapped = Mapper().Map( sections[i], row.Item1, row.Item2 );

                Assert.AreEqual( expectedWidth, mapped.Count, string.Format( "the {0} section maps to a row of the wrong width", sections[i] ) );
            }
        }

        /// <summary>
        /// A wire column with nothing behind it stops the submission.
        /// </summary>
        /// <remarks>
        /// It means the query and the contract have drifted apart. Emitting null for the missing
        /// column would keep the width right and put every later value in the correct place, so
        /// nothing downstream could tell that a column had silently become empty for every row.
        /// </remarks>
        [TestMethod]
        public void WireColumn_WithNothingBehindIt_StopsTheSubmissionHere()
        {
            var row = Row( "members", null );
            var shortened = row.Item1.Where( c => c != "is_leader" ).ToList();
            var values = row.Item2.Take( shortened.Count ).ToList();

            var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                () => Mapper().Map( "members", shortened, values ),
                "a wire column the query no longer returns was filled in silently" );

            StringAssert.Contains( thrown.Message, "is_leader", "the failure does not name the column that has nothing behind it" );
        }

        #endregion
    }
}
