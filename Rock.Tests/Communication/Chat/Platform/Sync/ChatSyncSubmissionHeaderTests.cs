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
    /// The two keyed headers are the reason this file exists. Their key sets are checked exactly by
    /// the platform and cannot be derived from the rest of the contract, so a builder with those
    /// strings typed into it compiles, passes any test that reads its own output back, and is
    /// refused on every cycle. The cells below hand the helper a contract whose key sets differ
    /// from the shipped one and require the output to differ with it.
    /// </para>
    /// <para>
    /// Key order is deliberately not asserted: both sides of the platform comparison sort.
    /// </para>
    /// </remarks>
    [TestClass]
    public class ChatSyncSubmissionHeaderTests
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
        /// A count for every section named, each one different so a header that pairs a key with
        /// the wrong count is visible.
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

        /// <summary>
        /// The counts in the form the keyed header takes.
        /// </summary>
        private static IDictionary<string, long> AsLong( IDictionary<string, int> counts )
        {
            return counts.ToDictionary( c => c.Key, c => ( long ) c.Value );
        }

        /// <summary>
        /// The four marks the shipped contract lists, each a different value.
        /// </summary>
        private static IDictionary<string, long> Marks( long person = 11, long personAlias = 22, long group = 33, long groupMember = 44 )
        {
            return new Dictionary<string, long>
            {
                { "person", person },
                { "person_alias", personAlias },
                { "group", group },
                { "group_member", groupMember }
            };
        }

        #endregion

        #region Row counts

        /// <summary>
        /// The row-count keys are read out of the contract, not written into this assembly.
        /// </summary>
        /// <remarks>
        /// Both the section list and the header's key set are renamed together, because the
        /// platform builds them from one constant. A helper holding the four strings emits the
        /// shipped names and fails here.
        /// </remarks>
        [TestMethod]
        public void RowCounts_KeysFollowTheContract_RatherThanStringsTypedInThisAssembly()
        {
            var contract = ShippedContract();
            var renamed = new[] { "aliases_renamed", "channels_renamed", "members_renamed", "badges_renamed" };

            contract["payload"]["sections"] = new JArray( renamed );
            Header( contract, "x-sync-counts" )["keys"] = new JArray( renamed );

            var header = JObject.Parse( ChatPlatformSyncHelper.BuildKeyedHeader( contract, "x-sync-counts", AsLong( CountsFor( renamed ) ) ) );

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
            var sections = ChatPlatformSyncHelper.GetPayloadSections( contract );

            var header = JObject.Parse( ChatPlatformSyncHelper.BuildKeyedHeader( contract, "x-sync-counts", AsLong( CountsFor( sections ) ) ) );
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
        /// <remarks>
        /// The counts supplied match the header's keys exactly, so only the check of those keys
        /// against the sections can refuse this.
        /// </remarks>
        [TestMethod]
        public void RowCounts_WhenTheContractSectionsAndKeysDisagree_TheSubmissionStopsHere()
        {
            var contract = ShippedContract();
            var keys = new[] { "aliases", "channels", "members", "unexpected" };
            Header( contract, "x-sync-counts" )["keys"] = new JArray( keys );

            var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                () => ChatPlatformSyncHelper.BuildSubmissionHeaders( contract, ReadAt, Marks(), CountsFor( keys ), "20.0.0", false ),
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
            var partial = CountsFor( new[] { "aliases", "channels", "members" } );

            var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                () => ChatPlatformSyncHelper.BuildKeyedHeader( ShippedContract(), "x-sync-counts", AsLong( partial ) ),
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
        /// The correspondence from a key to a table in Rock lives in the projection procedure's
        /// column names, because the contract does not name Rock tables. What the contract decides
        /// is which keys must be present.
        /// </remarks>
        [TestMethod]
        public void IdentityMarks_WhenTheContractNamesAKeyThisAssemblyCannotSupply_TheSubmissionStopsHere()
        {
            var contract = ShippedContract();
            Header( contract, "x-sync-marks" )["keys"] = new JArray( "person", "person_alias", "group", "group_member", "person_search_key" );

            var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                () => ChatPlatformSyncHelper.BuildKeyedHeader( contract, "x-sync-marks", Marks() ),
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

            var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                () => ChatPlatformSyncHelper.BuildKeyedHeader( contract, "x-sync-marks", Marks() ),
                "a contract missing a mark key was accepted, so this assembly quietly stopped sending one" );

            StringAssert.Contains( thrown.Message, "group_member", "the failure does not name the key the contract no longer lists" );
        }

        /// <summary>
        /// Against the contract as it ships, each value lands under its own key.
        /// </summary>
        [TestMethod]
        public void IdentityMarks_AgainstTheShippedContract_CarryEachValueUnderItsOwnKey()
        {
            var header = JObject.Parse( ChatPlatformSyncHelper.BuildKeyedHeader(
                ShippedContract(),
                "x-sync-marks",
                Marks( person: 196186, personAlias: 3028, group: 40391, groupMember: 1300457 ) ) );

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
        /// guard the value feeds fails closed on a tie, so a value rounded up would let a stale
        /// write win a comparison built to refuse it.
        /// </remarks>
        [TestMethod]
        public void ReadTime_IsTruncatedTowardsThePastAndCarriesAnExplicitOffset()
        {
            // a hundred nanoseconds past a whole microsecond, which is the remainder the platform
            // cannot hold and the one that must not round up
            var readAt = new DateTime( 2026, 9, 21, 16, 45, 12, DateTimeKind.Utc ).AddTicks( 1234567 );

            var formatted = ChatPlatformSyncHelper.FormatReadTime( readAt );

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
                () => ChatPlatformSyncHelper.FormatReadTime( local ),
                "a read time that is not UTC was accepted, so the whole payload would be judged by a clock shifted by the organisation's offset" );
        }

        #endregion

        #region The whole set

        /// <summary>
        /// A read time for the cells that build the whole set.
        /// </summary>
        private static readonly DateTime ReadAt = new DateTime( 2026, 9, 21, 18, 0, 0, DateTimeKind.Utc );

        /// <summary>
        /// Builds the whole header set from a contract.
        /// </summary>
        private static IDictionary<string, string> Build( JObject contract, bool isUrgent )
        {
            var sections = contract["payload"]["sections"].Select( s => s.Value<string>() );

            return ChatPlatformSyncHelper.BuildSubmissionHeaders( contract, ReadAt, Marks(), CountsFor( sections ), "20.0.0", isUrgent );
        }

        /// <summary>
        /// The contract header is the hash of the column lists this assembly actually builds
        /// payloads from, which for the shipped artifact is also the hash it publishes.
        /// </summary>
        [TestMethod]
        public void ContractHeader_IsTheHashOfTheColumnListsThisAssemblyBuildsFrom()
        {
            var headers = Build( ShippedContract(), false );

            Assert.AreEqual( ChatWireContract.ComputedHash, headers["x-sync-contract"], "the contract header is not the hash of the column lists the payload is built in, so the platform's compare cannot see a reorder here" );
            Assert.AreEqual( ChatWireContract.PublishedHash, headers["x-sync-contract"], "the shipped artifact publishes a hash other than the one its column lists give, so it has been edited since it was generated" );
        }

        /// <summary>
        /// An artifact whose published and computed hashes disagree has been edited since it was
        /// generated, and nothing can say what column order its payload would be in.
        /// </summary>
        [TestMethod]
        public void ContractWhosePublishedAndComputedHashesDisagree_StopsTheSubmissionHere()
        {
            var contract = ShippedContract();

            // Two columns of the first table swapped: the same width, a different wire, and the
            // published hash left as it was.
            var columns = contract["tables"][0]["columns"].Select( c => c.Value<string>() ).ToArray();
            var first = columns[0];
            columns[0] = columns[1];
            columns[1] = first;
            contract["tables"][0]["columns"] = new JArray( columns );

            var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                () => Build( contract, false ),
                "a contract edited since it was generated built headers anyway, so a payload in an order nobody agreed to would be sent under the agreed hash" );

            StringAssert.Contains( thrown.Message, "hash", "the failure does not say that the hashes disagree" );
        }

        /// <summary>
        /// The urgent header is set for a run a person started and absent, not false, for one the
        /// schedule started. The manual poll budget assumes urgent jumps the queue.
        /// </summary>
        [TestMethod]
        public void UrgentHeader_IsSetForAManualRunAndAbsentForAScheduledOne()
        {
            var manual = Build( ShippedContract(), true );
            var scheduled = Build( ShippedContract(), false );

            Assert.IsTrue( manual.ContainsKey( "x-sync-urgent" ), "a manual run did not mark itself urgent, so the person waiting on it waits behind the queue" );
            Assert.IsFalse( scheduled.ContainsKey( "x-sync-urgent" ), "a scheduled run marked itself urgent, so every scheduled restatement jumps the queue" );
        }

        /// <summary>
        /// The set is exactly what the contract lists: every required header but the submission id,
        /// which the transport sets, and nothing the contract does not name.
        /// </summary>
        [TestMethod]
        public void TheSet_IsExactlyWhatTheContractLists()
        {
            var contract = ShippedContract();
            var listed = contract["submit_headers"].Children<JObject>().ToList();

            var required = listed
                .Where( h => h["required"].Value<bool>() )
                .Select( h => h["name"].Value<string>() )
                .Where( name => name != "x-sync-submission-id" )
                .ToArray();
            var names = listed.Select( h => h["name"].Value<string>() ).ToArray();

            var headers = Build( contract, true );

            foreach ( var name in required )
            {
                Assert.IsTrue( headers.ContainsKey( name ), string.Format( "the contract requires {0} and the helper did not set it", name ) );
            }

            foreach ( var name in headers.Keys )
            {
                CollectionAssert.Contains( names, name, string.Format( "the helper set {0}, which the contract does not list", name ) );
            }
        }

        /// <summary>
        /// A contract that requires a header this assembly does not build stops the submission
        /// here, which is what makes the check above more than a list read back against itself.
        /// </summary>
        [TestMethod]
        public void ContractThatRequiresAHeaderNothingHereBuilds_StopsTheSubmissionHere()
        {
            var contract = ShippedContract();

            ( ( JArray ) contract["submit_headers"] ).Add( new JObject
            {
                ["name"] = "x-sync-weather",
                ["required"] = true,
                ["carries"] = "A header this assembly has never heard of."
            } );

            var thrown = Assert.ThrowsExactly<InvalidOperationException>(
                () => Build( contract, false ),
                "a contract requiring a header nothing here builds was accepted, so the platform would refuse every cycle for a reason nothing in this repository can see" );

            StringAssert.Contains( thrown.Message, "x-sync-weather", "the failure does not name the header the contract requires" );
        }

        #endregion
    }
}
