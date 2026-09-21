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
using System.Globalization;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Contract;
using Rock.Communication.Chat.Platform.Sync;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Tests the metadata a submission carries beside its body.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The two dictionary headers are the reason this file exists. Their key sets are checked
    /// exactly by the platform and are the one part of a submission that cannot be derived from the
    /// rest of the contract, so a builder with those strings typed into it compiles, passes any
    /// test that reads its own output back, and is refused on every cycle for a reason no test in
    /// this repository can see. The cells below hand the builder a contract whose key sets differ
    /// from the shipped one and require the output to differ with it, which is the only shape of
    /// assertion that can tell the two builders apart.
    /// </para>
    /// <para>
    /// Key order is deliberately not asserted anywhere here. Both sides of the platform comparison
    /// sort, so order cannot break a submission, and pinning it in a test would invent a constraint
    /// the wire does not have.
    /// </para>
    /// </remarks>
    [TestClass]
    public class ChatSyncHeaderBuilderTests
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
        /// Finds one submit header entry by name.
        /// </summary>
        /// <param name="contract">The parsed contract.</param>
        /// <param name="name">The header name.</param>
        /// <returns>The entry.</returns>
        private static JObject Header( JObject contract, string name )
        {
            return contract["submit_headers"]
                .Children<JObject>()
                .First( h => h["name"].Value<string>() == name );
        }

        /// <summary>
        /// A count for every section the contract names, each one different so a header that
        /// pairs a key with the wrong count is visible.
        /// </summary>
        /// <param name="sections">The section names.</param>
        /// <returns>The counts.</returns>
        private static IDictionary<string, int> CountsFor( IEnumerable<string> sections )
        {
            var counts = new Dictionary<string, int>();
            var next = 1;

            foreach ( var section in sections )
            {
                counts[section] = next * 11;
                next++;
            }

            return counts;
        }

        #endregion

        #region Row counts

        /// <summary>
        /// The row-count keys are read out of the contract, not written into this assembly.
        /// </summary>
        /// <remarks>
        /// Both the section list and the header's key set are renamed together, because the
        /// platform builds them from one constant and a builder is entitled to expect them to
        /// agree. A builder holding the four strings emits the shipped names and fails here.
        /// </remarks>
        [TestMethod]
        public void RowCounts_KeysFollowTheContract_RatherThanStringsTypedInThisAssembly()
        {
            var contract = ShippedContract();
            var renamed = new[] { "aliases_renamed", "channels_renamed", "members_renamed", "badges_renamed" };

            contract["payload"]["sections"] = new JArray( renamed );
            Header( contract, "x-sync-counts" )["keys"] = new JArray( renamed );

            var builder = new ChatSyncHeaderBuilder( contract );
            var header = JObject.Parse( builder.BuildRowCounts( CountsFor( renamed ) ) );

            CollectionAssert.AreEquivalent(
                renamed,
                header.Properties().Select( p => p.Name ).ToArray(),
                "the row-count keys did not follow the contract, so they are written into this assembly instead of read from it" );

            Assert.AreEqual( 11, header["aliases_renamed"].Value<int>(), "the first section's count did not land under the first section's key" );
            Assert.AreEqual( 44, header["badges_renamed"].Value<int>(), "the last section's count did not land under the last section's key" );
        }

        /// <summary>
        /// Against the contract as it ships, the keys are the payload's section names and are
        /// deliberately not the four table names.
        /// </summary>
        [TestMethod]
        public void RowCounts_AgainstTheShippedContract_AreTheSectionNamesAndNotTheTableNames()
        {
            var contract = ShippedContract();
            var builder = new ChatSyncHeaderBuilder( contract );
            var sections = builder.GetPayloadSections();

            var header = JObject.Parse( builder.BuildRowCounts( CountsFor( sections ) ) );
            var keys = header.Properties().Select( p => p.Name ).ToArray();

            CollectionAssert.AreEquivalent( new[] { "aliases", "channels", "members", "badges" }, keys, "the shipped key set is not the four payload sections" );

            var tableNames = contract["tables"].Select( t => t["name"].Value<string>() ).ToArray();

            CollectionAssert.AreNotEquivalent( tableNames, keys, "the row-count keys are the table names, which the platform refuses on every cycle" );
        }

        /// <summary>
        /// A contract whose section list and count keys disagree is a defect in the artifact, and
        /// the submission stops here rather than going out built from a guess about which of the
        /// two was meant.
        /// </summary>
        [TestMethod]
        public void RowCounts_WhenTheContractSectionsAndKeysDisagree_TheSubmissionStopsHere()
        {
            var contract = ShippedContract();
            Header( contract, "x-sync-counts" )["keys"] = new JArray( "aliases", "channels", "members", "unexpected" );

            var builder = new ChatSyncHeaderBuilder( contract );

            var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                () => builder.BuildRowCounts( CountsFor( new[] { "aliases", "channels", "members", "badges" } ) ),
                "a contract whose count keys and payload sections disagree was accepted, so one of the two was silently preferred" );

            StringAssert.Contains( thrown.Message, "unexpected", "the failure does not name the key that disagrees, so a church support call starts from nothing" );
        }

        /// <summary>
        /// A section the caller did not count is a projection that did not run, and it stops here
        /// rather than going out as a zero the platform cannot tell from an empty church.
        /// </summary>
        [TestMethod]
        public void RowCounts_WhenASectionHasNoCount_TheSubmissionStopsHere()
        {
            var builder = new ChatSyncHeaderBuilder( ShippedContract() );
            var partial = CountsFor( new[] { "aliases", "channels", "members" } );

            var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                () => builder.BuildRowCounts( partial ),
                "a section with no count was accepted, so a projection that never ran ships as an empty one" );

            StringAssert.Contains( thrown.Message, "badges", "the failure does not name the section that was not counted" );
        }

        #endregion

        #region Identity marks

        /// <summary>
        /// The mark keys are read out of the contract too, and a key this assembly cannot supply a
        /// value for stops the submission.
        /// </summary>
        /// <remarks>
        /// The correspondence from a key to a table in Rock has to live in this assembly, because
        /// the contract does not name Rock tables. What the contract decides is which keys must be
        /// present, so a contract that renames or adds one breaks the build here instead of being
        /// refused later under a code nobody can trace back to this file.
        /// </remarks>
        [TestMethod]
        public void IdentityMarks_WhenTheContractNamesAKeyThisAssemblyCannotSupply_TheSubmissionStopsHere()
        {
            var contract = ShippedContract();
            Header( contract, "x-sync-marks" )["keys"] = new JArray( "person", "person_alias", "group", "group_member", "person_search_key" );

            var builder = new ChatSyncHeaderBuilder( contract );

            var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                () => builder.BuildIdentityMarks( new ChatSyncIdentityMarks() ),
                "a mark key with no value behind it was accepted, so the header goes out short and is refused at the platform" );

            StringAssert.Contains( thrown.Message, "person_search_key", "the failure does not name the key that has no value behind it" );
        }

        /// <summary>
        /// A key this assembly holds that the contract does not list is the same defect from the
        /// other side, and it stops here as well.
        /// </summary>
        [TestMethod]
        public void IdentityMarks_WhenTheContractDropsAKeyThisAssemblyHolds_TheSubmissionStopsHere()
        {
            var contract = ShippedContract();
            Header( contract, "x-sync-marks" )["keys"] = new JArray( "person", "person_alias", "group" );

            var builder = new ChatSyncHeaderBuilder( contract );

            var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                () => builder.BuildIdentityMarks( new ChatSyncIdentityMarks() ),
                "a contract missing a mark key was accepted, so this assembly quietly stopped sending one" );

            StringAssert.Contains( thrown.Message, "group_member", "the failure does not name the key the contract no longer lists" );
        }

        /// <summary>
        /// Against the contract as it ships, each value lands under its own key.
        /// </summary>
        [TestMethod]
        public void IdentityMarks_AgainstTheShippedContract_CarryEachValueUnderItsOwnKey()
        {
            var builder = new ChatSyncHeaderBuilder( ShippedContract() );

            var marks = new ChatSyncIdentityMarks
            {
                Person = 196186,
                PersonAlias = 3028,
                Group = 40391,
                GroupMember = 1300457
            };

            var header = JObject.Parse( builder.BuildIdentityMarks( marks ) );

            CollectionAssert.AreEquivalent(
                new[] { "person", "person_alias", "group", "group_member" },
                header.Properties().Select( p => p.Name ).ToArray(),
                "the shipped mark key set is not the four the platform compares" );

            Assert.AreEqual( 196186L, header["person"].Value<long>(), "the person mark did not land under the person key" );
            Assert.AreEqual( 3028L, header["person_alias"].Value<long>(), "the person alias mark did not land under its own key" );
            Assert.AreEqual( 40391L, header["group"].Value<long>(), "the group mark did not land under its own key" );
            Assert.AreEqual( 1300457L, header["group_member"].Value<long>(), "the group member mark did not land under its own key" );
        }

        #endregion

        #region Read time

        /// <summary>
        /// The read time is truncated to microseconds and carries an explicit offset.
        /// </summary>
        /// <remarks>
        /// SQL Server keeps a hundred nanoseconds and the platform keeps a microsecond, and the
        /// guard the value feeds fails closed on a tie, so a value rounded up rather than truncated
        /// would let a stale write win a comparison built to refuse it. The offset is explicit
        /// because a value without one is read in the receiving session's zone.
        /// </remarks>
        [TestMethod]
        public void ReadTime_IsTruncatedTowardsThePastAndCarriesAnExplicitOffset()
        {
            // a hundred nanoseconds past a whole microsecond, which is the remainder the platform
            // cannot hold and the one that must not round up
            var readAt = new DateTime( 2026, 9, 21, 16, 45, 12, DateTimeKind.Utc ).AddTicks( 1234567 );

            var formatted = ChatSyncHeaderBuilder.FormatReadTime( readAt );

            StringAssert.EndsWith( formatted, "Z", "the read time carries no offset, so the platform reads it in its own zone" );
            Assert.AreEqual( "2026-09-21T16:45:12.123456Z", formatted, "the read time was not truncated towards the past at microsecond resolution" );

            var parsed = DateTime.Parse( formatted, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal );

            Assert.IsTrue( parsed <= readAt, "the formatted read time is later than the time it was taken, so a stale write could win the row clock comparison" );
        }

        /// <summary>
        /// A read time that is not UTC is a mistake this cannot silently absorb.
        /// </summary>
        [TestMethod]
        public void ReadTime_WhenItIsNotUtc_TheSubmissionStopsHere()
        {
            var local = new DateTime( 2026, 9, 21, 16, 45, 12, DateTimeKind.Local );

            Assert.ThrowsExactly<ArgumentException>(
                () => ChatSyncHeaderBuilder.FormatReadTime( local ),
                "a read time that is not UTC was accepted, so the whole payload would be judged by a clock shifted by the organisation's offset" );
        }

        #endregion
    }
}
