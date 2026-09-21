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
    internal sealed class ChatSyncHeaderBuilder
    {
        #region Fields

        private const string RowCountsHeader = "x-sync-counts";

        private const string IdentityMarksHeader = "x-sync-marks";

        private const string ReadTimeHeader = "x-sync-read-at";

        private const string RockVersionHeader = "x-sync-rock-version";

        private const string ContractHeader = "x-sync-contract";

        private const string UrgentHeader = "x-sync-urgent";

        private const string SubmissionIdHeader = "x-sync-submission-id";

        private const long TicksPerMicrosecond = 10L;

        private readonly JObject _contract;

        #endregion

        #region Constructors

        public ChatSyncHeaderBuilder()
            : this( JObject.Parse( Contract.ChatWireContract.Json ) )
        {
        }

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

        public IList<string> GetPayloadSections()
        {
            var sections = _contract["payload"] == null ? null : _contract["payload"]["sections"];

            if ( sections == null )
            {
                throw new InvalidOperationException( "the chat wire contract does not name the payload sections, so nothing here can key a submission" );
            }

            return sections.Select( s => s.Value<string>() ).ToList();
        }

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

        public IDictionary<string, string> BuildSubmissionHeaders( DateTime readAtUtc, ChatSyncIdentityMarks marks, IDictionary<string, int> rowCountsBySection, string rockVersion, bool isUrgent )
        {
            if ( rockVersion.IsNullOrWhiteSpace() )
            {
                throw new ArgumentException( "the version of Rock producing a payload is recorded with the submission and cannot be blank", "rockVersion" );
            }

            var publishedHash = _contract["wire_hash"] == null ? null : _contract["wire_hash"].Value<string>();
            var computedHash = Contract.ChatWireContract.HashColumnLists( _contract );

            if ( publishedHash != computedHash )
            {
                throw new InvalidOperationException(
                    "the wire contract this submission would be built from does not hash to the value written inside it, so nothing here can say what column order the payload is in" );
            }

            var headers = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase )
            {
                { ReadTimeHeader, FormatReadTime( readAtUtc ) },
                { RowCountsHeader, BuildRowCounts( rowCountsBySection ) },
                { IdentityMarksHeader, BuildIdentityMarks( marks ) },
                { RockVersionHeader, rockVersion },
                { ContractHeader, computedHash }
            };

            if ( isUrgent )
            {
                headers.Add( UrgentHeader, "1" );
            }

            RequireTheContractsHeaderSet( headers );

            return headers;
        }

        private void RequireTheContractsHeaderSet( IDictionary<string, string> headers )
        {
            var listed = _contract["submit_headers"];

            if ( listed == null )
            {
                throw new InvalidOperationException( "the chat wire contract describes no submit headers" );
            }

            var entries = listed.Children<JObject>().ToList();

            var required = entries
                .Where( h => h["required"] != null && h["required"].Value<bool>() )
                .Select( h => h["name"].Value<string>() )
                .Where( name => !string.Equals( name, SubmissionIdHeader, StringComparison.OrdinalIgnoreCase ) )
                .ToList();

            var missing = required.Where( name => !headers.ContainsKey( name ) ).ToList();

            if ( missing.Any() )
            {
                throw new InvalidOperationException( string.Format(
                    "the chat wire contract requires the {0} header, which nothing here builds",
                    string.Join( ", ", missing ) ) );
            }

            var names = entries.Select( h => h["name"].Value<string>() ).ToList();
            var unlisted = headers.Keys.Where( name => !names.Contains( name, StringComparer.OrdinalIgnoreCase ) ).ToList();

            if ( unlisted.Any() )
            {
                throw new InvalidOperationException( string.Format(
                    "the {0} header is not one the chat wire contract lists",
                    string.Join( ", ", unlisted ) ) );
            }
        }

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
