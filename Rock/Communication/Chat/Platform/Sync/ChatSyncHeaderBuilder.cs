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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Builds the metadata a submission carries beside its body.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Two of these values are dictionaries whose key sets the platform checks exactly, and those
    /// key sets are read out of the wire contract rather than written here. They are the one part
    /// of a submission that cannot be derived from the rest of the contract, and a copy of them
    /// typed into this file would compile, pass every test that only reads it back, and be refused
    /// at the platform on every cycle with nothing in this repository saying why.
    /// </para>
    /// <para>
    /// The row-count keys are the short names of the payload's own sections, deliberately not the
    /// four table names, which is why they are taken from the contract's header entry and checked
    /// against the contract's section list rather than assumed to be either.
    /// </para>
    /// <para>
    /// The mark keys name tables in Rock, and that correspondence has to live somewhere in this
    /// assembly because the contract does not name Rock tables. What the contract decides is which
    /// keys must be present: a key it lists and this builder cannot supply, or a key this builder
    /// holds and the contract does not list, stops the submission here rather than being refused
    /// later for a reason nobody can see. Key order is not part of either set and is not relied on.
    /// </para>
    /// </remarks>
    internal sealed class ChatSyncHeaderBuilder
    {
        #region Fields

        /// <summary>
        /// The contract entry naming the expected row count of each payload section.
        /// </summary>
        private const string RowCountsHeader = "x-sync-counts";

        /// <summary>
        /// The contract entry naming the identity high-water value of each table read.
        /// </summary>
        private const string IdentityMarksHeader = "x-sync-marks";

        /// <summary>
        /// A hundred nanoseconds is the smallest interval a tick counts and a microsecond is the
        /// smallest the platform stores, so this is what has to be removed from a read time.
        /// </summary>
        private const long TicksPerMicrosecond = 10L;

        /// <summary>
        /// The parsed wire contract this builder reads its key sets from.
        /// </summary>
        private readonly JObject _contract;

        #endregion

        #region Constructors

        /// <summary>
        /// Builds headers from the contract that ships in this assembly.
        /// </summary>
        public ChatSyncHeaderBuilder()
            : this( JObject.Parse( Contract.ChatWireContract.Json ) )
        {
        }

        /// <summary>
        /// Builds headers from a supplied contract.
        /// </summary>
        /// <param name="contract">The parsed wire contract.</param>
        /// <remarks>
        /// Taking the contract rather than always reading the embedded one is what lets a test hand
        /// this a contract whose key sets differ and require the headers to differ with them, which
        /// is the only way to tell a builder that reads the contract from one that agrees with
        /// itself.
        /// </remarks>
        public ChatSyncHeaderBuilder( JObject contract )
        {
            if ( contract == null )
            {
                throw new ArgumentNullException( "contract" );
            }

            _contract = contract;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The payload's section names, in the order the contract lists them.
        /// </summary>
        /// <returns>The section names.</returns>
        public IList<string> GetPayloadSections()
        {
            var sections = _contract["payload"] == null ? null : _contract["payload"]["sections"];

            if ( sections == null )
            {
                throw new InvalidOperationException( "the chat wire contract does not name the payload sections, so nothing here can key a submission" );
            }

            return sections.Select( s => s.Value<string>() ).ToList();
        }

        /// <summary>
        /// Reads one header entry's key set out of the contract.
        /// </summary>
        /// <param name="headerName">The header the contract lists.</param>
        /// <returns>The keys, in the order the contract happens to list them.</returns>
        /// <remarks>
        /// The order is incidental. Both sides of the platform's comparison sort, so it cannot
        /// break a submission, and nothing here should come to depend on it.
        /// </remarks>
        private IList<string> GetHeaderKeys( string headerName )
        {
            var headers = _contract["submit_headers"];

            if ( headers == null )
            {
                throw new InvalidOperationException( "the chat wire contract describes no submit headers" );
            }

            var header = headers.Children<JObject>().FirstOrDefault( h => h["name"] != null && h["name"].Value<string>() == headerName );

            if ( header == null )
            {
                throw new InvalidOperationException( string.Format( "the chat wire contract describes no {0} header", headerName ) );
            }

            if ( header["keys"] == null )
            {
                throw new InvalidOperationException( string.Format( "the chat wire contract does not carry the key set of {0} as data, so this header could only be built from prose about it", headerName ) );
            }

            return header["keys"].Select( k => k.Value<string>() ).ToList();
        }

        /// <summary>
        /// Builds the expected row count header.
        /// </summary>
        /// <param name="rowCountsBySection">How many rows each payload section carries.</param>
        /// <returns>The header value.</returns>
        public string BuildRowCounts( IDictionary<string, int> rowCountsBySection )
        {
            if ( rowCountsBySection == null )
            {
                throw new ArgumentNullException( "rowCountsBySection" );
            }

            var sections = GetPayloadSections();
            var keys = GetHeaderKeys( RowCountsHeader );

            // The platform builds these two lists from one constant, so a copy of the contract
            // where they differ is a defect in the copy. Preferring either one would send a header
            // built from a guess and leave the disagreement to be found as a refusal.
            var disagreements = keys.Except( sections ).Concat( sections.Except( keys ) ).ToList();

            if ( disagreements.Any() )
            {
                throw new InvalidOperationException( string.Format(
                    "the chat wire contract's row-count keys and payload sections disagree about {0}",
                    string.Join( ", ", disagreements ) ) );
            }

            var header = new JObject();

            foreach ( var key in keys )
            {
                int rowCount;

                // A section with no count is a projection that did not run. Sending it as zero
                // would be indistinguishable from a church that genuinely has none of that row,
                // and the platform would apply the emptiness as truth.
                if ( !rowCountsBySection.TryGetValue( key, out rowCount ) )
                {
                    throw new InvalidOperationException( string.Format( "no row count was taken for the {0} section", key ) );
                }

                header[key] = rowCount;
            }

            return header.ToString( Formatting.None );
        }

        /// <summary>
        /// Builds the identity high-water header.
        /// </summary>
        /// <param name="marks">The values read from Rock.</param>
        /// <returns>The header value.</returns>
        public string BuildIdentityMarks( ChatSyncIdentityMarks marks )
        {
            if ( marks == null )
            {
                throw new ArgumentNullException( "marks" );
            }

            var keys = GetHeaderKeys( IdentityMarksHeader );

            // Which table each key names is the one part of this that has to live here, because
            // the contract describes a wire and never names a table in Rock. What the contract
            // decides is which keys have to be present, and the two checks below are what turn a
            // contract this assembly has fallen behind into a build failure rather than a refusal
            // at the platform on every cycle under a code that points at no file.
            var valuesByKey = new Dictionary<string, long>
            {
                { "person", marks.Person },
                { "person_alias", marks.PersonAlias },
                { "group", marks.Group },
                { "group_member", marks.GroupMember }
            };

            var unsupplied = keys.Except( valuesByKey.Keys ).ToList();

            if ( unsupplied.Any() )
            {
                throw new InvalidOperationException( string.Format(
                    "the chat wire contract asks for the identity mark {0}, which nothing here reads",
                    string.Join( ", ", unsupplied ) ) );
            }

            var unlisted = valuesByKey.Keys.Except( keys ).ToList();

            if ( unlisted.Any() )
            {
                throw new InvalidOperationException( string.Format(
                    "the chat wire contract no longer lists the identity mark {0}, which this assembly still reads",
                    string.Join( ", ", unlisted ) ) );
            }

            var header = new JObject();

            foreach ( var key in keys )
            {
                header[key] = valuesByKey[key];
            }

            return header.ToString( Formatting.None );
        }

        /// <summary>
        /// Formats the read time the whole payload is judged by.
        /// </summary>
        /// <param name="readAtUtc">The UTC time taken before the projection read.</param>
        /// <returns>The header value.</returns>
        public static string FormatReadTime( DateTime readAtUtc )
        {
            if ( readAtUtc.Kind != DateTimeKind.Utc )
            {
                throw new ArgumentException( "the read time a payload is judged by has to be taken in UTC, because it is compared against times the platform holds in UTC", "readAtUtc" );
            }

            // Truncated rather than rounded, and towards the past. The guard this value feeds
            // refuses a row whose stored time is not strictly older, so a value rounded up by the
            // fraction of a microsecond the platform cannot hold would let a stale write win a
            // comparison built to fail closed.
            var truncated = new DateTime( readAtUtc.Ticks - ( readAtUtc.Ticks % TicksPerMicrosecond ), DateTimeKind.Utc );

            // The offset is explicit because a time without one is read in the receiving session's
            // own zone rather than in the zone it was taken in.
            return truncated.ToString( "yyyy-MM-ddTHH:mm:ss.ffffff'Z'", CultureInfo.InvariantCulture );
        }

        #endregion
    }
}
