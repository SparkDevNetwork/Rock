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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Contract;
using Rock.Communication.Chat.Platform.Sync;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Tests the queries that read a church's chat rows out of Rock.
    /// </summary>
    /// <remarks>
    /// These read the query text rather than run it. What a query returns needs a database and is
    /// covered by the integration suite; what is asserted here is the two properties that are
    /// decided by where a line of SQL sits rather than by what it returns, and that a database
    /// cannot show: that the rule deciding what a chat channel is lives in exactly one query, and
    /// that each section returns its columns in the order the wire contract lists them.
    /// </remarks>
    [TestClass]
    public class ChatSyncProjectionTests
    {
        #region Methods

        /// <summary>
        /// The four payload sections, as the shipped contract names them.
        /// </summary>
        /// <returns>The section names.</returns>
        private static string[] Sections()
        {
            return JObject.Parse( ChatWireContract.Json )["payload"]["sections"]
                .Select( s => s.Value<string>() )
                .ToArray();
        }

        #endregion

        #region One definition of a chat channel

        /// <summary>
        /// The half of the rule that says a group is a chat channel right now is written once and
        /// used twice: the staging query adds the groups that ever were, and the stamp marks the
        /// ones that are. The two would drift silently, because a group missing its stamp is still
        /// projected by the live half of the staging predicate and nothing looks wrong until chat is
        /// turned off and the conversation is not archived.
        /// </summary>
        /// <remarks>
        /// Written out here rather than extracted from one of the files, so that changing either
        /// file turns this red and changing both means saying so in a third place.
        /// </remarks>
        [TestMethod]
        public void TheStampAndTheStagingQueryAgreeOnWhatQualifiesRightNow()
        {
            const string liveQualification =
                "[GT].[IsChatAllowed] = 1 AND COALESCE( [G].[IsChatEnabledOverride], [GT].[IsChatEnabledForAllGroups] ) = 1";

            StringAssert.Contains( Flatten( ChatSyncProjection.GetStagingSql() ), liveQualification,
                "the staging query no longer reads the rule this stamp is written against" );
            StringAssert.Contains( Flatten( ChatSyncProjection.GetStampSql() ), liveQualification,
                "the stamp marks a different set of groups than the projection reads" );
        }

        /// <summary>
        /// The stamp writes the marker rather than reading it: a stamp filtered by the marker
        /// being absent is right, and one that qualified on the marker being present would only
        /// ever re-stamp groups that already had one.
        /// </summary>
        [TestMethod]
        public void TheStampWritesTheMarkerAndOnlyWhereThereIsNotOneAlready()
        {
            var sql = Flatten( ChatSyncProjection.GetStampSql() );

            StringAssert.Contains( sql, "UPDATE" );
            StringAssert.Contains( sql, "[ChatChannelFirstEnabledDateTime] IS NULL" );
        }

        /// <summary>
        /// Collapses whitespace so the two files may be laid out as their own readability wants.
        /// </summary>
        private static string Flatten( string sql )
        {
            return System.Text.RegularExpressions.Regex.Replace( sql, @"\s+", " " );
        }

        /// <summary>
        /// Every query the projection needs is present and not empty.
        /// </summary>
        [TestMethod]
        public void Projection_ShipsAQueryForStagingAndForEverySection()
        {
            Assert.IsFalse( string.IsNullOrWhiteSpace( ChatSyncProjection.GetStagingSql() ), "the staging query is missing, so nothing stages the sets the sections read" );

            foreach ( var section in Sections() )
            {
                Assert.IsFalse( string.IsNullOrWhiteSpace( ChatSyncProjection.GetSectionSql( section ) ), string.Format( "the {0} section has no query", section ) );
            }
        }

        /// <summary>
        /// The rule that decides what a chat channel is belongs to the staging query alone.
        /// </summary>
        /// <remarks>
        /// Asserted as the absence of the columns it is built from rather than as the presence of a
        /// marker, because a marker can be copied along with the predicate it labels and the columns
        /// cannot: a second copy of the rule has to name them.
        /// </remarks>
        [TestMethod]
        public void SectionQueries_NameNoColumnThatDecidesWhetherAGroupIsAChatChannel()
        {
            foreach ( var section in Sections() )
            {
                var sql = ChatSyncProjection.GetSectionSql( section );

                foreach ( var column in ChatSyncProjection.QualificationColumns )
                {
                    Assert.IsFalse(
                        sql.IndexOf( column, StringComparison.OrdinalIgnoreCase ) >= 0,
                        string.Format( "the {0} section names {1}, so it decides for itself what a chat channel is and can disagree with the staging query", section, column ) );
                }
            }
        }

        /// <summary>
        /// The staging query does hold that rule, so the test above is not passing because the rule
        /// has gone missing altogether.
        /// </summary>
        [TestMethod]
        public void StagingQuery_HoldsTheRuleThatDecidesWhetherAGroupIsAChatChannel()
        {
            var sql = ChatSyncProjection.GetStagingSql();

            foreach ( var column in ChatSyncProjection.QualificationColumns )
            {
                Assert.IsTrue(
                    sql.IndexOf( column, StringComparison.OrdinalIgnoreCase ) >= 0,
                    string.Format( "the staging query does not name {0}, so the rule it is part of is not there", column ) );
            }
        }

        /// <summary>
        /// The sections read the staged sets rather than the tables the staging query read.
        /// </summary>
        /// <remarks>
        /// This is what makes a membership's channel and person certain to be in the same payload.
        /// A section that went back to the group membership table for its own set would be reading
        /// it seconds later than the staging query did, and a person who joined and left in between
        /// would arrive as a membership with no alias, which the far side refuses as a whole
        /// submission rather than as one row.
        /// </remarks>
        [TestMethod]
        public void MembershipSection_ReadsTheStagedSetRatherThanTheMembershipTable()
        {
            var sql = ChatSyncProjection.GetSectionSql( "members" );

            Assert.IsTrue( sql.IndexOf( "#MemberRows", StringComparison.OrdinalIgnoreCase ) >= 0, "the membership section does not read the staged set" );
            Assert.IsFalse(
                sql.IndexOf( "[GroupMember]", StringComparison.OrdinalIgnoreCase ) >= 0,
                "the membership section reads the group membership table directly, so its set is taken at a different moment than the channels it names" );
        }

        #endregion

        #region Column order

        /// <summary>
        /// Each section returns its columns in the order the wire contract lists them.
        /// </summary>
        /// <remarks>
        /// Rows travel as positional arrays, so this order is the wire. Two columns of the same type
        /// swapped here shift every value one place, a width check cannot see it, and the values
        /// land in the wrong columns on the far side while every guard passes.
        /// </remarks>
        [TestMethod]
        public void SectionQueries_ReturnTheirColumnsInTheOrderTheContractLists()
        {
            var contract = JObject.Parse( ChatWireContract.Json );
            var sections = Sections();

            for ( var i = 0; i < sections.Length; i++ )
            {
                var expected = contract["tables"][i]["columns"]
                    .Select( c => c.Value<string>() )
                    .SelectMany( DerivedFrom )
                    .Distinct()
                    .ToArray();

                var actual = ChatSyncProjection.GetSectionColumns( sections[i] ).ToArray();

                CollectionAssert.AreEqual(
                    expected,
                    actual,
                    string.Format( "the {0} section does not return the columns of {1} in the contract's order", sections[i], contract["tables"][i]["name"].Value<string>() ) );
            }
        }

        /// <summary>
        /// What a query has to return in order for a wire column to be built from it.
        /// </summary>
        /// <param name="wireColumn">The column the wire carries.</param>
        /// <returns>The column the query returns in its place.</returns>
        /// <remarks>
        /// Exactly one wire column is not read straight out of Rock. The wire carries a background
        /// and a foreground colour for a badge and Rock holds a single highlight, so the query
        /// returns the highlight and the pair is worked out from it before it is sent, which is what
        /// keeps that decision in one place rather than in every client that renders a badge.
        ///
        /// It is written here as a mapping rather than as an exemption so that a second derivation
        /// added later has to be declared, instead of the order check quietly having a hole in it.
        /// </remarks>
        private static string[] DerivedFrom( string wireColumn )
        {
            if ( wireColumn == "bg_color" || wireColumn == "fg_color" )
            {
                return new[] { "highlight_color" };
            }

            return new[] { wireColumn };
        }

        /// <summary>
        /// The badge section is the one that does not return a wire column directly.
        /// </summary>
        /// <remarks>
        /// The wire carries a background and a foreground colour and Rock holds one highlight, so
        /// the query returns the highlight and the pair is derived from it before it is sent. The
        /// order test above therefore has to be reading the section's own aliases rather than
        /// assuming they match the wire, and this is what says so out loud.
        /// </remarks>
        [TestMethod]
        public void BadgeSection_ReturnsTheHighlightColourRatherThanThePairTheWireCarries()
        {
            var sql = ChatSyncProjection.GetSectionSql( "badges" );

            Assert.IsTrue( sql.IndexOf( "highlight_color", StringComparison.OrdinalIgnoreCase ) >= 0, "the badge section does not return the highlight colour the pair is derived from" );
        }

        #endregion

        #region Unknown sections

        /// <summary>
        /// A section the contract does not name has no query, and asking for one says so.
        /// </summary>
        [TestMethod]
        public void UnknownSection_HasNoQuery()
        {
            var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                () => ChatSyncProjection.GetSectionSql( "sermons" ),
                "a section nothing ships a query for returned one anyway" );

            StringAssert.Contains( thrown.Message, "sermons", "the failure does not name the section that was asked for" );
        }

        #endregion
    }
}
