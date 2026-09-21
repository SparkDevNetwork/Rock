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
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Contract;

namespace Rock.Tests.Communication.Chat.Platform
{
    /// <summary>
    /// Tests the wire contract that ships with Rock as an embedded resource.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The chat platform receives each synced row as a positional JSON array rather than as named
    /// fields, so the column order in this file is a wire that both sides have to agree about. Two
    /// columns of the same width swapped on one side shift every value one place and no row-width
    /// check can see it. The contract exists to make that disagreement loud, and the hash over its
    /// column lists is what the submission carries so the platform can refuse a payload built from
    /// a different one.
    /// </para>
    /// <para>
    /// Every hash in this file is recomputed here from the recipe the artifact itself states, never
    /// by calling the code under test to check itself. That is deliberate: the recipe is prose in a
    /// generated file and this is the only place a second implementation of it is written down, so
    /// a loader that reads the recipe wrongly has to disagree with something rather than agree with
    /// itself.
    /// </para>
    /// </remarks>
    [TestClass]
    public class ChatWireContractTests
    {
        #region Fields

        /// <summary>
        /// The four synced tables, in the order the platform applies them: an alias row has to
        /// exist before a membership can reference it. The order is part of the hash, so it is part
        /// of the wire.
        /// </summary>
        private static readonly string[] _expectedTables = new[]
        {
            "chat_aliases",
            "chat_channels",
            "chat_channel_members",
            "chat_badges"
        };

        #endregion

        #region Methods

        /// <summary>
        /// Reads the contract from the assembly's manifest rather than from a file beside the
        /// tests, which is what proves the artifact is actually packaged into Rock.
        /// </summary>
        /// <returns>The parsed contract.</returns>
        private static JObject ReadEmbeddedContract()
        {
            var assembly = typeof( ChatWireContract ).Assembly;
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault( n => n.EndsWith( "chat-wire-contract.json", StringComparison.Ordinal ) );

            Assert.IsNotNull( resourceName, "the wire contract is not embedded in the assembly that carries the chat platform code" );

            using ( var stream = assembly.GetManifestResourceStream( resourceName ) )
            {
                Assert.IsNotNull( stream, "the wire contract resource exists in the manifest but could not be opened" );

                using ( var reader = new StreamReader( stream, Encoding.UTF8 ) )
                {
                    return JObject.Parse( reader.ReadToEnd() );
                }
            }
        }

        /// <summary>
        /// Builds the hash the way the artifact says it is built: one line per table in the order
        /// the tables are listed, the table name, a colon, the wire column names joined by commas,
        /// each line closed by a line feed, hashed as UTF-8 and rendered lowercase hex.
        /// </summary>
        /// <param name="tables">The table name and its ordered column names, in order.</param>
        /// <returns>The hash, lowercase hex.</returns>
        private static string HashFromRecipe( IEnumerable<KeyValuePair<string, IEnumerable<string>>> tables )
        {
            var builder = new StringBuilder();

            foreach ( var table in tables )
            {
                builder.Append( table.Key );
                builder.Append( ':' );
                builder.Append( string.Join( ",", table.Value ) );
                builder.Append( '\n' );
            }

            using ( var sha = SHA256.Create() )
            {
                var digest = sha.ComputeHash( Encoding.UTF8.GetBytes( builder.ToString() ) );

                return BitConverter.ToString( digest ).Replace( "-", string.Empty ).ToLowerInvariant();
            }
        }

        /// <summary>
        /// Reads the table name and ordered column names out of the parsed contract.
        /// </summary>
        /// <param name="contract">The parsed contract.</param>
        /// <returns>One entry per table, in the order the contract lists them.</returns>
        private static List<KeyValuePair<string, IEnumerable<string>>> TablesOf( JObject contract )
        {
            return contract["tables"]
                .Select( t => new KeyValuePair<string, IEnumerable<string>>(
                    t["name"].Value<string>(),
                    t["columns"].Select( c => c.Value<string>() ).ToList() ) )
                .ToList();
        }

        [TestMethod]
        public void EmbeddedContract_OnTheRockAssembly_NamesTheFourWireTablesInOrder()
        {
            var contract = ReadEmbeddedContract();
            var tables = TablesOf( contract );

            CollectionAssert.AreEqual( _expectedTables, tables.Select( t => t.Key ).ToArray(), "the wire tables are not the four the platform applies, or not in the order it applies them" );

            foreach ( var table in tables )
            {
                Assert.IsTrue( table.Value.Any(), string.Format( "{0} carries no wire columns", table.Key ) );
            }
        }

        [TestMethod]
        public void WireHash_RecomputedFromTheEmbeddedLists_EqualsThePublishedHash()
        {
            var contract = ReadEmbeddedContract();

            var recomputed = HashFromRecipe( TablesOf( contract ) );

            Assert.AreEqual( contract["wire_hash"].Value<string>(), recomputed, "the published hash is not the hash of the column lists it is published beside" );
        }

        [TestMethod]
        public void WireHash_ComputedByTheContractType_EqualsTheIndependentRecomputation()
        {
            var contract = ReadEmbeddedContract();

            var recomputed = HashFromRecipe( TablesOf( contract ) );

            Assert.AreEqual( recomputed, ChatWireContract.ComputedHash, "the hash this assembly computes is not the hash the artifact's own recipe produces" );
            Assert.AreEqual( contract["wire_hash"].Value<string>(), ChatWireContract.PublishedHash, "the published hash this assembly reports is not the one in the artifact" );
        }

        [TestMethod]
        public void WireHash_WithTwoColumnsSwapped_DiffersFromThePublishedHash()
        {
            var contract = ReadEmbeddedContract();
            var tables = TablesOf( contract );

            var first = tables[0];
            var columns = first.Value.ToList();

            Assert.IsGreaterThanOrEqualTo( 2, columns.Count, "the first table needs two columns for this mutation to mean anything" );

            var swapped = columns.ToList();
            swapped[0] = columns[1];
            swapped[1] = columns[0];

            tables[0] = new KeyValuePair<string, IEnumerable<string>>( first.Key, swapped );

            Assert.AreNotEqual( contract["wire_hash"].Value<string>(), HashFromRecipe( tables ), "swapping two columns of the same table leaves the hash alone, so the hash cannot see a reordered wire" );
        }

        [TestMethod]
        public void WireHash_WithTwoTablesSwapped_DiffersFromThePublishedHash()
        {
            var contract = ReadEmbeddedContract();
            var tables = TablesOf( contract );

            var reordered = tables.ToList();
            reordered[0] = tables[1];
            reordered[1] = tables[0];

            Assert.AreNotEqual( contract["wire_hash"].Value<string>(), HashFromRecipe( reordered ), "swapping two tables leaves the hash alone, so the order the drain applies them in is not covered" );
        }

        [TestMethod]
        public void ErrorCodes_EveryPublishedCode_MatchesThePublishedPattern()
        {
            var contract = ReadEmbeddedContract();
            var pattern = new Regex( contract["error_codes"]["pattern"].Value<string>() );
            var codes = contract["error_codes"]["codes"].Select( c => c.Value<string>() ).ToList();

            Assert.IsTrue( codes.Any(), "the contract carries no error codes, so nothing on this side can branch on one" );

            foreach ( var code in codes )
            {
                Assert.IsTrue( pattern.IsMatch( code ), string.Format( "the published code {0} does not match the pattern published beside it", code ) );
            }

            Assert.IsFalse( pattern.IsMatch( "Rpc.UnknownErrorCode" ), "the pattern accepts an uppercase code, so it constrains nothing" );
            Assert.IsFalse( pattern.IsMatch( "nope.unknown_error_code" ), "the pattern accepts a family that is not one of the published families" );
        }


        /// <summary>
        /// The body is one object keyed by four section names, and the contract has to say so as
        /// data rather than in prose.
        /// </summary>
        /// <remarks>
        /// The rows of two of these tables are the same width, so two sections transposed inside an
        /// array pass a row-width check and are caught only if their declared counts happen to
        /// differ. Naming the sections is what makes the shape unambiguous, and naming them here is
        /// what lets a producer be built from the artifact instead of from a sentence about it.
        /// </remarks>
        [TestMethod]
        public void EmbeddedContract_NamesTheFourPayloadSections()
        {
            var contract = ReadEmbeddedContract();

            Assert.IsNotNull( contract["payload"], "the contract does not describe the payload, so nothing here can be built from it" );

            var sections = contract["payload"]["sections"];

            Assert.IsNotNull( sections, "the contract does not name the payload sections as data" );

            CollectionAssert.AreEquivalent(
                new[] { "aliases", "channels", "members", "badges" },
                sections.Select( s => s.Value<string>() ).ToArray(),
                "the payload sections the contract names are not the four the platform reads" );
        }

        /// <summary>
        /// Every header the platform requires is described, including the identity marks, which are
        /// how a restored database is told apart from a live one.
        /// </summary>
        [TestMethod]
        public void EmbeddedContract_DescribesEveryRequiredSubmitHeader()
        {
            var contract = ReadEmbeddedContract();
            var names = contract["submit_headers"].Select( h => h["name"].Value<string>() ).ToList();

            foreach ( var required in new[]
            {
                "x-sync-submission-id",
                "x-sync-read-at",
                "x-sync-counts",
                "x-sync-marks",
                "x-sync-rock-version",
                "x-sync-contract"
            } )
            {
                Assert.IsTrue( names.Contains( required ), string.Format( "the contract does not describe the {0} header, which every submission has to carry", required ) );
            }
        }

        /// <summary>
        /// The two dictionary headers carry their key sets as data, because a key set is the one
        /// part of a submission a producer cannot derive from the rest of the contract.
        /// </summary>
        /// <remarks>
        /// The counts keys are the short payload section names and deliberately not the four table
        /// names. A producer built from the table list is refused at ingest on every cycle, with
        /// nothing in this repository saying why, so the assertion below is written as the
        /// difference rather than as a list: it fails against an artifact that says table names,
        /// which is what the copy that shipped here before said.
        /// </remarks>
        [TestMethod]
        public void EmbeddedContract_CarriesTheKeySetsOfTheTwoDictionaryHeadersAsData()
        {
            var contract = ReadEmbeddedContract();

            var counts = contract["submit_headers"].FirstOrDefault( h => h["name"].Value<string>() == "x-sync-counts" );
            var marks = contract["submit_headers"].FirstOrDefault( h => h["name"].Value<string>() == "x-sync-marks" );

            Assert.IsNotNull( counts, "the contract describes no x-sync-counts header at all" );
            Assert.IsNotNull( marks, "the contract describes no x-sync-marks header at all" );

            Assert.IsNotNull( counts["keys"], "the counts header does not carry its key set as data, so a producer can only be built from prose about it" );
            Assert.IsNotNull( marks["keys"], "the marks header does not carry its key set as data" );

            var countsKeys = counts["keys"].Select( k => k.Value<string>() ).ToArray();
            var marksKeys = marks["keys"].Select( k => k.Value<string>() ).ToArray();

            CollectionAssert.AreEquivalent(
                contract["payload"]["sections"].Select( s => s.Value<string>() ).ToArray(),
                countsKeys,
                "the counts keys are not the payload section names" );

            CollectionAssert.AreEquivalent(
                new[] { "person", "person_alias", "group", "group_member" },
                marksKeys,
                "the marks keys are not the four Rock tables the platform compares" );

            CollectionAssert.AreNotEquivalent(
                _expectedTables,
                countsKeys,
                "the counts keys are the four table names, which is the header the platform refuses on every cycle" );
        }

        /// <summary>
        /// The copy shipping here is the copy the platform generated, whole. The hash covers the
        /// column lists alone by design, so everything else in the file can drift without moving
        /// it, and this is what notices.
        /// </summary>
        /// <remarks>
        /// Compared as a set rather than as a count, because a count agrees by accident as soon as
        /// one code is added on one side and one removed on the other.
        /// </remarks>
        [TestMethod]
        public void EmbeddedContract_CarriesEverySyncErrorCodeTheIngestFunctionCanReturn()
        {
            var contract = ReadEmbeddedContract();
            var codes = contract["error_codes"]["codes"].Select( c => c.Value<string>() ).ToList();

            foreach ( var code in new[]
            {
                "sync.bad_counts",
                "sync.bad_header",
                "sync.bad_marks",
                "sync.bad_submission_id",
                "sync.contract_mismatch",
                "sync.duplicate_submission",
                "sync.future_read",
                "sync.kill_switch",
                "sync.marks_regressed",
                "sync.payload_too_large",
                "sync.stale_read",
                "sync.unknown_submission"
            } )
            {
                Assert.IsTrue( codes.Contains( code ), string.Format( "the contract does not carry {0}, so nothing here can branch on a refusal that names it", code ) );
            }
        }
        #endregion
    }
}
