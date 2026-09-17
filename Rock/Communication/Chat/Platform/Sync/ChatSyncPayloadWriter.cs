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

using Rock.Communication.Chat.Platform.Contract;
using Rock.Enums.Communication.Chat;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Shapes a restatement body and its headers from named rows and the vendored
    /// contract. Column order, section keys, counts keys and marks keys all come
    /// from the artifact, so a same-width reorder or a typed-in key set is a
    /// disagreement with that file rather than a silent shift.
    /// </summary>
    internal static class ChatSyncPayloadWriter
    {
        #region Constants

        /// <summary>
        /// Synced table that holds alias rows. The payload section is a short
        /// name from the artifact; this is the table the drain applies.
        /// </summary>
        internal const string AliasesTable = "chat_aliases";

        /// <summary>
        /// Synced table that holds channel rows.
        /// </summary>
        internal const string ChannelsTable = "chat_channels";

        /// <summary>
        /// Synced table that holds membership rows.
        /// </summary>
        internal const string MembersTable = "chat_channel_members";

        /// <summary>
        /// Synced table that holds badge definitions.
        /// </summary>
        internal const string BadgesTable = "chat_badges";

        #endregion

        #region Methods

        /// <summary>
        /// Truncates a UTC instant to microseconds. SQL Server datetime2 is 100 ns
        /// and the platform clock is 1 us; a round-up would let a stale write win
        /// a comparison built to fail closed.
        /// </summary>
        internal static DateTime TruncateToMicroseconds( DateTime utc )
        {
            var ticks = utc.ToUniversalTime().Ticks;
            return new DateTime( ticks - ( ticks % 10 ), DateTimeKind.Utc );
        }

        /// <summary>
        /// Formats the truncated read time for the submit header.
        /// </summary>
        internal static string FormatReadAt( DateTime utc )
        {
            return TruncateToMicroseconds( utc ).ToString( "yyyy-MM-dd'T'HH:mm:ss.ffffff'Z'", CultureInfo.InvariantCulture );
        }

        /// <summary>
        /// Builds the counts header by walking the artifact's counts keys, so a
        /// producer that typed the four table names is a missing key rather than
        /// a coincidental match.
        /// </summary>
        /// <summary>
        /// Row counts keyed by the artifact's section names.
        /// </summary>
        internal static IDictionary<string, int> SectionLengths( IDictionary<string, IList<IDictionary<string, object>>> sections )
        {
            var lengths = new Dictionary<string, int>();
            foreach ( var key in ChatWireContract.SectionKeys )
            {
                IList<IDictionary<string, object>> rows;
                lengths[key] = sections != null && sections.TryGetValue( key, out rows ) && rows != null
                    ? rows.Count
                    : 0;
            }

            return lengths;
        }

        internal static IDictionary<string, int> BuildCounts( IDictionary<string, int> sectionLengths )
        {
            var counts = new Dictionary<string, int>();
            foreach ( var key in ChatWireContract.CountKeys )
            {
                int length;
                counts[key] = sectionLengths != null && sectionLengths.TryGetValue( key, out length )
                    ? length
                    : 0;
            }

            return counts;
        }

        /// <summary>
        /// Builds the marks header by walking the artifact's marks keys.
        /// </summary>
        internal static IDictionary<string, long> BuildMarks( IDictionary<string, long> identityByKey )
        {
            var marks = new Dictionary<string, long>();
            foreach ( var key in ChatWireContract.MarksKeys )
            {
                long value;
                marks[key] = identityByKey != null && identityByKey.TryGetValue( key, out value )
                    ? value
                    : 0;
            }

            return marks;
        }

        /// <summary>
        /// The wire value for a Rock notification mode, refused if it is not in
        /// the vendored list.
        /// </summary>
        internal static string NotifyMode( ChatNotificationMode mode )
        {
            var wire = ChatWireContract.NotifyModeWireValue( mode );
            if ( !ChatWireContract.EnumValues( "chat_notify_mode" ).Contains( wire ) )
            {
                throw new InvalidOperationException( string.Format( "Chat notification mode {0} is not a value the chat platform accepts.", mode ) );
            }

            return wire;
        }

        /// <summary>
        /// Serialises one object with the four section keys, each an array of
        /// positional rows in contract column order.
        /// </summary>
        internal static string WriteBody( IDictionary<string, IList<IDictionary<string, object>>> sections )
        {
            var body = new JObject();
            var tableBySection = TableBySection();

            foreach ( var section in ChatWireContract.SectionKeys )
            {
                IList<IDictionary<string, object>> rows;
                if ( sections == null || !sections.TryGetValue( section, out rows ) || rows == null )
                {
                    rows = new List<IDictionary<string, object>>();
                }

                var columns = ChatWireContract.ColumnsOf( tableBySection[section] );
                var array = new JArray();
                foreach ( var row in rows )
                {
                    var values = new JArray();
                    foreach ( var column in columns )
                    {
                        object raw;
                        row.TryGetValue( column, out raw );
                        values.Add( ToToken( raw ) );
                    }

                    array.Add( values );
                }

                body[section] = array;
            }

            return body.ToString( Formatting.None );
        }

        /// <summary>
        /// JSON for a counts or marks header object. Key order follows the
        /// artifact array, which ingest does not depend on.
        /// </summary>
        internal static string WriteHeaderObject<T>( IDictionary<string, T> values )
        {
            var obj = new JObject();
            foreach ( var pair in values )
            {
                obj[pair.Key] = JToken.FromObject( pair.Value );
            }

            return obj.ToString( Formatting.None );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Maps each payload section to the synced table of the same position
        /// in the artifact. The section names and the table names are different
        /// lists; this is the join between them.
        /// </summary>
        private static IReadOnlyDictionary<string, string> TableBySection()
        {
            var tables = new[] { AliasesTable, ChannelsTable, MembersTable, BadgesTable };
            var map = new Dictionary<string, string>();
            var sections = ChatWireContract.SectionKeys;
            for ( var i = 0; i < sections.Count; i++ )
            {
                map[sections[i]] = tables[i];
            }

            return map;
        }

        private static JToken ToToken( object value )
        {
            if ( value == null || value is DBNull )
            {
                return JValue.CreateNull();
            }

            if ( value is Guid guid )
            {
                return guid.ToString( "D" ).ToLowerInvariant();
            }

            if ( value is Guid? )
            {
                var nullable = ( Guid? ) value;
                return nullable.HasValue
                    ? nullable.Value.ToString( "D" ).ToLowerInvariant()
                    : JValue.CreateNull();
            }

            if ( value is DateTime dateTime )
            {
                return FormatReadAt( dateTime );
            }

            if ( value is DateTime? )
            {
                var nullable = ( DateTime? ) value;
                return nullable.HasValue ? ( JToken ) FormatReadAt( nullable.Value ) : JValue.CreateNull();
            }

            if ( value is IEnumerable<string> strings && !( value is string ) )
            {
                return new JArray( strings.Select( s => ( JToken ) s.ToLowerInvariant() ) );
            }

            if ( value is IEnumerable<Guid> guids )
            {
                return new JArray( guids.Select( g => ( JToken ) g.ToString( "D" ).ToLowerInvariant() ) );
            }

            if ( value is bool || value is int || value is long || value is decimal || value is double || value is float )
            {
                return JToken.FromObject( value );
            }

            var text = Convert.ToString( value, CultureInfo.InvariantCulture );
            return text == null ? JValue.CreateNull() : ( JToken ) text;
        }

        #endregion
    }
}
