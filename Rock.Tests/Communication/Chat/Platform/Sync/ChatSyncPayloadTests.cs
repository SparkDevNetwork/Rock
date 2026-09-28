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
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Contract;
using Rock.Communication.Chat.Platform.Sync;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Tests the body of a submission, as the job writes it from the projection's result sets.
    /// </summary>
    /// <remarks>
    /// Two of the four tables are the same width, so a body that is an array of four arrays has two
    /// sections that can be transposed without any width check seeing it. That is why the body is
    /// an object keyed by section name, and why the keys are read from the contract here rather
    /// than written into the assembly.
    /// </remarks>
    [TestClass]
    public class ChatSyncPayloadTests
    {
        #region Shape

        /// <summary>
        /// The body is keyed by the section names the contract carries, not by names written here.
        /// </summary>
        [TestMethod]
        public void Body_IsKeyedByTheContractsSectionNames()
        {
            var contract = ChatSyncTestBody.ShippedContract();
            var renamed = new[] { "aliases_renamed", "channels_renamed", "members_renamed", "badges_renamed" };
            contract["payload"]["sections"] = new JArray( renamed );

            var body = JObject.Parse( ChatSyncTestBody.Write( contract, ChatSyncTestBody.ShippedResultSets() ) );

            CollectionAssert.AreEquivalent(
                renamed,
                body.Properties().Select( p => p.Name ).ToArray(),
                "the body keys did not follow the contract, so they are written into this assembly instead of read from it" );
        }

        /// <summary>
        /// A section holds an array of rows, and each row is itself an array rather than an object.
        /// </summary>
        [TestMethod]
        public void Rows_AreWrittenAsPositionalArrays()
        {
            var resultSets = ChatSyncTestBody.ShippedResultSets();

            foreach ( var table in resultSets )
            {
                ChatSyncTestBody.AddRow( table, null );
            }

            var body = JObject.Parse( ChatSyncTestBody.Write( ChatSyncTestBody.ShippedContract(), resultSets ) );

            foreach ( var section in ChatSyncTestBody.ShippedSections )
            {
                Assert.AreEqual( JTokenType.Array, body[section].Type, string.Format( "the {0} section is not an array of rows", section ) );
                Assert.AreEqual( JTokenType.Array, body[section][0].Type, string.Format( "a {0} row is not a positional array", section ) );
            }
        }

        /// <summary>
        /// The section list and the table list line up by position, which is how a section's
        /// columns are known at all.
        /// </summary>
        [TestMethod]
        public void Sections_AndTables_LineUpByPosition()
        {
            var contract = ChatSyncTestBody.ShippedContract();

            var sections = contract["payload"]["sections"].Select( s => s.Value<string>() ).ToList();
            var tables = contract["tables"].Select( t => t["name"].Value<string>() ).ToList();

            Assert.AreEqual( tables.Count, sections.Count, "the contract names a different number of payload sections than tables, so no section can be matched to its columns" );

            var resultSets = ChatSyncTestBody.ShippedResultSets();

            foreach ( var table in resultSets )
            {
                ChatSyncTestBody.AddRow( table, null );
            }

            var body = JObject.Parse( ChatSyncTestBody.Write( contract, resultSets ) );

            for ( var i = 0; i < sections.Count; i++ )
            {
                var expected = contract["tables"][i]["columns"].Count();

                Assert.AreEqual( expected, body[sections[i]][0].Count(), string.Format( "the {0} section was not written at the width of {1}", sections[i], tables[i] ) );
            }
        }

        /// <summary>
        /// A contract whose section and table lists differ in length stops the submission, because
        /// no section could then be matched to its columns.
        /// </summary>
        [TestMethod]
        public void Contract_WithMoreSectionsThanTables_StopsTheSubmissionHere()
        {
            var contract = ChatSyncTestBody.ShippedContract();
            ( ( JArray ) contract["tables"] ).RemoveAt( 3 );

            Assert.ThrowsExactly<InvalidOperationException>(
                () => ChatSyncTestBody.Write( contract, ChatSyncTestBody.ShippedResultSets() ),
                "a section with no table in the contract was written, so its columns could only have been guessed" );
        }

        /// <summary>
        /// A section the projection returned no result set for stops the submission, because a
        /// section left out is not an empty church, it is a projection that did not run.
        /// </summary>
        [TestMethod]
        public void Body_WithAResultSetMissing_StopsTheSubmissionHere()
        {
            var shortOfOne = ChatSyncTestBody.ShippedResultSets().Take( 3 ).ToArray();

            var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                () => ChatSyncTestBody.Write( ChatSyncTestBody.ShippedContract(), shortOfOne ),
                "a body missing its last section was written, so a projection that never returned it would be applied as a church with no badges" );

            StringAssert.Contains( thrown.Message, "badges", "the failure does not name the section with no result set" );
        }

        /// <summary>
        /// Every value lands under the wire column the contract lists at its position, whatever
        /// order the result set returns its columns in.
        /// </summary>
        /// <remarks>
        /// Rows travel as positional arrays, so the order on the wire is the one thing both sides
        /// have to agree about. The result set's columns are reversed here, so a writer that
        /// followed the reader's order rather than the contract's would put every value in the
        /// wrong place.
        /// </remarks>
        [TestMethod]
        public void Values_AreWrittenInTheContractsColumnOrder_NotTheResultSets()
        {
            var contract = ChatSyncTestBody.ShippedContract();
            var resultSets = ChatSyncTestBody.ShippedSections
                .Select( s => ChatSyncTestBody.SectionTable( s, ChatSyncSqlText.SectionColumns( s ).Reverse().ToList() ) )
                .ToArray();

            foreach ( var table in resultSets )
            {
                // Every column carries its own name, except the three that are converted on the way.
                var values = table.Columns.Cast<DataColumn>()
                    .Where( c => c.ColumnName != "badge_keys" && c.ColumnName != "ban_expires_at" && c.ColumnName != "highlight_color" )
                    .ToDictionary( c => c.ColumnName, c => ( object ) c.ColumnName );

                ChatSyncTestBody.AddRow( table, values );
            }

            var body = JObject.Parse( ChatSyncTestBody.Write( contract, resultSets ) );
            var sections = contract["payload"]["sections"].Select( s => s.Value<string>() ).ToList();

            for ( var i = 0; i < sections.Count; i++ )
            {
                var wireColumns = contract["tables"][i]["columns"].Select( c => c.Value<string>() ).ToList();
                var row = ( JArray ) body[sections[i]][0];

                for ( var j = 0; j < wireColumns.Count; j++ )
                {
                    if ( row[j].Type != JTokenType.String )
                    {
                        continue;
                    }

                    Assert.AreEqual( wireColumns[j], row[j].Value<string>(), string.Format( "the {0} row carries another column's value at position {1}", sections[i], j ) );
                }
            }
        }

        #endregion

        #region Counts

        /// <summary>
        /// The row counts are a tally of what was written, which is the only form of the number
        /// that the guard it feeds can act on.
        /// </summary>
        /// <remarks>
        /// Counted with a second query it disagrees with the body whenever Rock changes between the
        /// two statements. Taken as the length of a finished collection it agrees by construction
        /// and can never fire. Tallied as rows stream out it agrees with what Rock built and
        /// disagrees with what arrives if the body is cut short on the way, which is the failure
        /// the header exists to catch.
        /// </remarks>
        [TestMethod]
        public void RowCounts_AreATallyOfTheRowsActuallyWritten()
        {
            var rowsBySection = new Dictionary<string, int>
            {
                { "aliases", 7 },
                { "channels", 3 },
                { "members", 11 },
                { "badges", 2 }
            };

            var resultSets = ChatSyncTestBody.ShippedResultSets();

            foreach ( var table in resultSets )
            {
                for ( var i = 0; i < rowsBySection[table.TableName]; i++ )
                {
                    ChatSyncTestBody.AddRow( table, null );
                }
            }

            var counts = new Dictionary<string, int>();
            var body = JObject.Parse( ChatSyncTestBody.Write( ChatSyncTestBody.ShippedContract(), resultSets, counts ) );

            foreach ( var section in rowsBySection.Keys )
            {
                Assert.AreEqual( rowsBySection[section], counts[section], string.Format( "the tally for {0} is not the number of rows written", section ) );
                Assert.AreEqual( rowsBySection[section], body[section].Count(), string.Format( "the body for {0} does not hold the number of rows written", section ) );
            }
        }

        /// <summary>
        /// A row the writer failed to put down is not counted.
        /// </summary>
        /// <remarks>
        /// This is what separates a tally of rows written from a tally of rows asked for. On every
        /// complete body the two agree, so the cell above cannot tell them apart; they disagree
        /// only when a write fails part way, which is the one moment the count has to be honest
        /// about. A count that ran ahead of the writer would agree with a body cut short.
        /// </remarks>
        [TestMethod]
        public void RowCounts_DoNotCountARowTheWriterFailedToWrite()
        {
            var resultSets = ChatSyncTestBody.ShippedResultSets();
            var aliases = resultSets[0];

            for ( var i = 0; i < 3; i++ )
            {
                ChatSyncTestBody.AddRow( aliases, null );
            }

            // The fourth row carries a badge key that cannot be read, so it fails as it is written.
            ChatSyncTestBody.AddRow( aliases, new Dictionary<string, object> { { "badge_keys", "not-an-identifier" } } );

            var counts = new Dictionary<string, int>();

            Assert.ThrowsExactly<InvalidOperationException>(
                () => ChatSyncTestBody.Write( ChatSyncTestBody.ShippedContract(), resultSets, counts ),
                "the fourth row did not fail, so this cell separates nothing" );

            Assert.AreEqual( 3, counts["aliases"], "the tally counted a row that was never written, so a body cut short would agree with its own count" );
        }

        #endregion

        #region Converted columns

        /// <summary>
        /// The badge keys reach the alias row as an array of identifiers.
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

        /// <summary>
        /// The converted ban expiry is what reaches the member row.
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
        public void BanExpiry_WhenThereIsNone_IsNullOnTheMemberRow()
        {
            var value = WrittenValue( "members", "ban_expires_at", null );

            Assert.AreEqual( JTokenType.Null, value.Type, "a ban that does not expire was given an expiry on the wire" );
        }

        /// <summary>
        /// Both halves of the colour pair reach the badge row.
        /// </summary>
        [TestMethod]
        public void BadgeColours_ReachTheBadgeRowAsTwoColumns()
        {
            var values = new Dictionary<string, object> { { "highlight_color", "#1B4D3E" } };

            Assert.AreEqual( "#1b4d3e", WrittenValue( "badges", "bg_color", values ).Value<string>(), "the background did not reach the badge row" );
            Assert.AreEqual( "#ffffff", WrittenValue( "badges", "fg_color", values ).Value<string>(), "the foreground did not reach the badge row" );
        }

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

        #region Missing columns

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
        /// Emitting null for the missing column would keep the width right, so nothing downstream
        /// could tell that a column had silently become empty for every row.
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

    /// <summary>
    /// Builds what the projection procedure returns, as a reader, and writes a body from it.
    /// </summary>
    internal static class ChatSyncTestBody
    {
        /// <summary>
        /// The shipped contract's sections, in its order.
        /// </summary>
        public static readonly string[] ShippedSections = { "aliases", "channels", "members", "badges" };

        /// <summary>
        /// The contract as it ships, parsed fresh so a test that edits it cannot reach another.
        /// </summary>
        /// <returns>The parsed contract.</returns>
        public static JObject ShippedContract()
        {
            return JObject.Parse( ChatWireContract.Json );
        }

        /// <summary>
        /// A zone that is not UTC and does not observe daylight saving, so the arithmetic in these
        /// tests is the same on every day of the year.
        /// </summary>
        /// <returns>The zone.</returns>
        public static TimeZoneInfo FixedOffsetZone()
        {
            return TimeZoneInfo.FindSystemTimeZoneById( "US Mountain Standard Time" );
        }

        /// <summary>
        /// One empty result set per section, with the columns the shipped procedure returns for it.
        /// </summary>
        /// <returns>The result sets, in the contract's section order.</returns>
        public static DataTable[] ShippedResultSets()
        {
            return ShippedSections.Select( s => SectionTable( s, ChatSyncSqlText.SectionColumns( s ) ) ).ToArray();
        }

        /// <summary>
        /// An empty result set for a section with the named columns, in the order given.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="columns">The column names.</param>
        /// <returns>The result set.</returns>
        public static DataTable SectionTable( string section, IEnumerable<string> columns )
        {
            var table = new DataTable( section );

            foreach ( var column in columns )
            {
                table.Columns.Add( column, typeof( object ) );
            }

            return table;
        }

        /// <summary>
        /// Adds a row, with the named values and null in every other column.
        /// </summary>
        /// <param name="table">The result set.</param>
        /// <param name="values">Values by column name, or null for none.</param>
        public static void AddRow( DataTable table, IDictionary<string, object> values )
        {
            var row = table.NewRow();

            foreach ( DataColumn column in table.Columns )
            {
                object value;
                row[column] = values != null && values.TryGetValue( column.ColumnName, out value ) && value != null ? value : DBNull.Value;
            }

            table.Rows.Add( row );
        }

        /// <summary>
        /// Writes a body from the result sets, read the way the job reads the procedure: after the
        /// first result set, which carries the moment and the identity marks.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <param name="sections">The section result sets.</param>
        /// <param name="rowCounts">Receives the tally, or null.</param>
        /// <returns>The body as text.</returns>
        public static string Write( JObject contract, DataTable[] sections, IDictionary<string, int> rowCounts = null )
        {
            var marks = new DataTable( "marks" );
            marks.Columns.Add( "read_at", typeof( DateTime ) );
            marks.Rows.Add( DateTime.UtcNow );

            var text = new StringWriter();

            using ( var reader = new DataTableReader( new[] { marks }.Concat( sections ).ToArray() ) )
            using ( var json = new JsonTextWriter( text ) )
            {
                ChatPlatformSyncHelper.WriteSections( reader, contract, json, FixedOffsetZone(), rowCounts ?? new Dictionary<string, int>() );
            }

            return text.ToString();
        }
    }
}
