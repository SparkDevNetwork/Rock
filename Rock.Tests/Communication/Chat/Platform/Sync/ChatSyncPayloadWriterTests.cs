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
    /// Tests the body of a submission.
    /// </summary>
    /// <remarks>
    /// Two of the four tables are the same width, so a body that is an array of four arrays has two
    /// sections that can be transposed without any width check seeing it. That is why the body is
    /// an object keyed by section name, and why the keys are read from the contract here rather
    /// than written into the assembly.
    /// </remarks>
    [TestClass]
    public class ChatSyncPayloadWriterTests
    {
        #region Methods

        /// <summary>
        /// The contract as it ships, parsed fresh so a test that edits it cannot reach another.
        /// </summary>
        /// <returns>The parsed contract.</returns>
        private static JObject ShippedContract()
        {
            return JObject.Parse( ChatWireContract.Json );
        }

        /// <summary>
        /// A row of the right width for a section, every value null.
        /// </summary>
        /// <param name="writer">The writer, which knows the width.</param>
        /// <param name="section">The section.</param>
        /// <returns>The row.</returns>
        private static IList<object> EmptyRow( ChatSyncPayloadWriter writer, string section )
        {
            return Enumerable.Repeat( (object)null, writer.GetRowWidth( section ) ).ToList();
        }

        /// <summary>
        /// Writes every section the contract names, calling back for the rows of each.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <param name="rowsForSection">Supplies the rows of a section.</param>
        /// <param name="rowCounts">Receives what the writer tallied.</param>
        /// <returns>The body as text.</returns>
        private static string WriteBody( JObject contract, Func<ChatSyncPayloadWriter, string, IEnumerable<IList<object>>> rowsForSection, out IDictionary<string, int> rowCounts )
        {
            var sections = contract["payload"]["sections"].Select( s => s.Value<string>() ).ToList();
            var text = new StringWriter();

            using ( var json = new JsonTextWriter( text ) )
            {
                var writer = new ChatSyncPayloadWriter( contract, json );

                foreach ( var section in sections )
                {
                    writer.BeginSection( section );

                    foreach ( var row in rowsForSection( writer, section ) )
                    {
                        writer.WriteRow( row );
                    }

                    writer.EndSection();
                }

                writer.Complete();
                rowCounts = writer.RowCounts;
            }

            return text.ToString();
        }

        #endregion

        #region Shape

        /// <summary>
        /// The body is keyed by the section names the contract carries, not by names written here.
        /// </summary>
        [TestMethod]
        public void Body_IsKeyedByTheContractsSectionNames()
        {
            var contract = ShippedContract();
            var renamed = new[] { "aliases_renamed", "channels_renamed", "members_renamed", "badges_renamed" };
            contract["payload"]["sections"] = new JArray( renamed );

            IDictionary<string, int> counts;
            var body = JObject.Parse( WriteBody( contract, ( w, s ) => Enumerable.Empty<IList<object>>(), out counts ) );

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
            var contract = ShippedContract();

            IDictionary<string, int> counts;
            var body = JObject.Parse( WriteBody( contract, ( w, s ) => new[] { EmptyRow( w, s ) }, out counts ) );

            foreach ( var section in new[] { "aliases", "channels", "members", "badges" } )
            {
                Assert.AreEqual( JTokenType.Array, body[section].Type, string.Format( "the {0} section is not an array of rows", section ) );
                Assert.AreEqual( JTokenType.Array, body[section][0].Type, string.Format( "a {0} row is not a positional array", section ) );
            }
        }

        /// <summary>
        /// The section list and the table list line up by position, which is how a section's width
        /// is known at all.
        /// </summary>
        [TestMethod]
        public void Sections_AndTables_LineUpByPosition()
        {
            var contract = ShippedContract();

            var sections = contract["payload"]["sections"].Select( s => s.Value<string>() ).ToList();
            var tables = contract["tables"].Select( t => t["name"].Value<string>() ).ToList();

            Assert.AreEqual( tables.Count, sections.Count, "the contract names a different number of payload sections than tables, so no section can be matched to a width" );

            var writer = new ChatSyncPayloadWriter( contract, new JsonTextWriter( new StringWriter() ) );

            for ( var i = 0; i < sections.Count; i++ )
            {
                var expected = contract["tables"][i]["columns"].Count();

                Assert.AreEqual( expected, writer.GetRowWidth( sections[i] ), string.Format( "the {0} section was not matched to the width of {1}", sections[i], tables[i] ) );
            }
        }

        /// <summary>
        /// A row of the wrong width shifts every later value one place, and no check on the far
        /// side can see it once the types happen to line up.
        /// </summary>
        [TestMethod]
        public void Rows_OfTheWrongWidth_StopTheSubmissionHere()
        {
            var contract = ShippedContract();
            var text = new StringWriter();

            using ( var json = new JsonTextWriter( text ) )
            {
                var writer = new ChatSyncPayloadWriter( contract, json );
                writer.BeginSection( "members" );

                var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                    () => writer.WriteRow( new List<object> { null, null, null, null } ),
                    "a row one value short was written, so every value after the gap lands in the wrong column" );

                StringAssert.Contains( thrown.Message, "members", "the failure does not name the section whose row was the wrong width" );
                StringAssert.Contains( thrown.Message, "5", "the failure does not name the width the contract expects" );
            }
        }

        /// <summary>
        /// Every section the contract names has to be written, because a section left out is not an
        /// empty church, it is a projection that did not run.
        /// </summary>
        [TestMethod]
        public void Body_WithASectionNeverWritten_StopsTheSubmissionHere()
        {
            var contract = ShippedContract();
            var text = new StringWriter();

            using ( var json = new JsonTextWriter( text ) )
            {
                var writer = new ChatSyncPayloadWriter( contract, json );

                writer.BeginSection( "aliases" );
                writer.EndSection();

                var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                    () => writer.Complete(),
                    "a body missing three of its four sections was completed, so a projection that never ran would be applied as an empty church" );

                StringAssert.Contains( thrown.Message, "channels", "the failure does not name a section that was never written" );
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
            var contract = ShippedContract();
            var rowsBySection = new Dictionary<string, int>
            {
                { "aliases", 7 },
                { "channels", 3 },
                { "members", 11 },
                { "badges", 2 }
            };

            IDictionary<string, int> counts;
            var body = JObject.Parse( WriteBody(
                contract,
                ( w, s ) => Enumerable.Range( 0, rowsBySection[s] ).Select( _ => EmptyRow( w, s ) ),
                out counts ) );

            foreach ( var section in rowsBySection.Keys )
            {
                Assert.AreEqual( rowsBySection[section], counts[section], string.Format( "the tally for {0} is not the number of rows written", section ) );
                Assert.AreEqual( rowsBySection[section], body[section].Count(), string.Format( "the body for {0} does not hold the number of rows written", section ) );
            }
        }

        #endregion

        #region Values

        /// <summary>
        /// Guids are written lowercase and hyphenated, which is the one form that parses as a
        /// Postgres uuid and compares equal to the same value stored there.
        /// </summary>
        [TestMethod]
        public void Guids_AreWrittenLowercaseAndHyphenated()
        {
            var contract = ShippedContract();
            var guid = new Guid( "DFDC14A3-D1DC-4342-A012-5CE9E8994B5E" );
            var text = new StringWriter();

            using ( var json = new JsonTextWriter( text ) )
            {
                var writer = new ChatSyncPayloadWriter( contract, json );
                writer.BeginSection( "members" );
                writer.WriteRow( new List<object> { guid, null, null, null, null } );
                writer.EndSection();
            }

            StringAssert.Contains( text.ToString(), "dfdc14a3-d1dc-4342-a012-5ce9e8994b5e", "the guid was not written lowercase and hyphenated" );
        }

        /// <summary>
        /// A collection of guids is written as a JSON array.
        /// </summary>
        /// <remarks>
        /// The badge column on the far side is a uuid array, and the drain reads anything that is
        /// not a JSON array as an empty one. A joined string would therefore give every person no
        /// badges, on a submission the platform accepts, with no error raised anywhere.
        /// </remarks>
        [TestMethod]
        public void CollectionsOfGuids_AreWrittenAsArraysRatherThanJoinedText()
        {
            var contract = ShippedContract();
            var badges = new List<Guid>
            {
                new Guid( "C1000000-0000-4000-8000-000000000001" ),
                new Guid( "C1000000-0000-4000-8000-000000000002" )
            };

            var text = new StringWriter();

            using ( var json = new JsonTextWriter( text ) )
            {
                var writer = new ChatSyncPayloadWriter( contract, json );
                writer.BeginSection( "aliases" );

                var row = EmptyRow( writer, "aliases" );
                row[6] = badges;
                writer.WriteRow( row );

                writer.EndSection();
            }

            // The writer closes the containers it opened when it is disposed, so the text is
            // already a whole object by the time it is read back here.
            var written = JObject.Parse( text.ToString() ).Value<JArray>( "aliases" );
            var element = written[0][6];

            Assert.AreEqual( JTokenType.Array, element.Type, "the badge keys were not written as an array, so every person arrives with none and nothing reports it" );
            CollectionAssert.AreEqual(
                new[] { "c1000000-0000-4000-8000-000000000001", "c1000000-0000-4000-8000-000000000002" },
                element.Select( e => e.Value<string>() ).ToArray(),
                "the badge keys are not lowercase hyphenated uuids" );
        }

        /// <summary>
        /// A time that is not UTC stops the submission rather than going out to be read in the
        /// platform's own zone.
        /// </summary>
        /// <remarks>
        /// Rock keeps times in the organisation's zone and the far side reads a value with no
        /// offset as UTC, so a ban expiry sent as stored is wrong by that church's offset and, for
        /// a church behind UTC, wrong in the direction that lifts the ban early. Converting here
        /// silently would hide which times were already right, so the caller converts and this
        /// refuses anything it cannot vouch for.
        /// </remarks>
        [TestMethod]
        public void TimesThatAreNotUtc_StopTheSubmissionHere()
        {
            var contract = ShippedContract();
            var text = new StringWriter();

            using ( var json = new JsonTextWriter( text ) )
            {
                var writer = new ChatSyncPayloadWriter( contract, json );
                writer.BeginSection( "members" );

                var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                    () => writer.WriteRow( new List<object> { null, null, null, null, new DateTime( 2026, 9, 21, 18, 0, 0, DateTimeKind.Unspecified ) } ),
                    "a time with no zone was written, so the platform reads it in its own zone and the value is wrong by the church's offset" );

                StringAssert.Contains( thrown.Message, "UTC", "the failure does not say what is wrong with the time" );
            }
        }

        /// <summary>
        /// A UTC time is written with an explicit offset.
        /// </summary>
        [TestMethod]
        public void UtcTimes_AreWrittenWithAnExplicitOffset()
        {
            var contract = ShippedContract();
            var text = new StringWriter();

            using ( var json = new JsonTextWriter( text ) )
            {
                var writer = new ChatSyncPayloadWriter( contract, json );
                writer.BeginSection( "members" );
                writer.WriteRow( new List<object> { null, null, null, null, new DateTime( 2026, 9, 21, 18, 0, 0, DateTimeKind.Utc ) } );
                writer.EndSection();
            }

            StringAssert.Contains( text.ToString(), "2026-09-21T18:00:00", "the time was not written in a form the platform parses" );
            StringAssert.Contains( text.ToString(), "Z", "the time carries no offset, so it is read in the receiving session's zone" );
        }

        #endregion
    }
}
