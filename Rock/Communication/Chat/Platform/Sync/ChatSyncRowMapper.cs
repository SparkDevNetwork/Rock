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

using Newtonsoft.Json.Linq;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Turns a row as Rock returns it into a row as the wire carries it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Most columns cross unchanged. Three do not, and each of them is a place where sending the
    /// value as Rock holds it would be accepted by the far side and be wrong.
    /// </para>
    /// <para>
    /// The badge keys come back joined into one string, because a query cannot return a list in a
    /// single column, and the column they land in holds a list. A string arriving there is read as
    /// no badges at all, on a submission that is otherwise accepted, with nothing reporting it.
    /// </para>
    /// <para>
    /// The ban expiry comes back in the organisation's own time zone, as Rock stores every time.
    /// The far side reads a time with no zone as UTC, so sending it unchanged makes it wrong by
    /// this church's offset, and for a church behind UTC that lifts the ban early.
    /// </para>
    /// <para>
    /// The badge colours are a pair on the wire and one value in Rock. Deciding which foreground
    /// reads against which background is done once here rather than in each client, so the same
    /// badge does not come out differently on the web and on a phone.
    /// </para>
    /// <para>
    /// Nothing here is addressed by position. The values are matched by the name the query gave
    /// them and emitted in the order the contract lists, so neither this file nor the queries carry
    /// a column index that the other one has to agree with.
    /// </para>
    /// </remarks>
    internal sealed class ChatSyncRowMapper
    {
        #region Fields

        /// <summary>
        /// The parsed wire contract, which decides the order values are emitted in.
        /// </summary>
        private readonly JObject _contract;

        /// <summary>
        /// The zone Rock's stored times are in.
        /// </summary>
        private readonly TimeZoneInfo _organizationTimeZone;

        #endregion

        #region Constructors

        /// <summary>
        /// Maps rows for one church.
        /// </summary>
        /// <param name="contract">The parsed wire contract.</param>
        /// <param name="organizationTimeZone">The zone Rock's stored times are in.</param>
        public ChatSyncRowMapper( JObject contract, TimeZoneInfo organizationTimeZone )
        {
            if ( contract == null )
            {
                throw new ArgumentNullException( "contract" );
            }

            if ( organizationTimeZone == null )
            {
                throw new ArgumentNullException( "organizationTimeZone" );
            }

            _contract = contract;
            _organizationTimeZone = organizationTimeZone;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Maps one row of a section.
        /// </summary>
        /// <param name="section">The payload section, as the contract names it.</param>
        /// <param name="queryColumns">The names the query gave its columns, in the order it returned them.</param>
        /// <param name="rawValues">The values the query returned, in the same order.</param>
        /// <returns>The values the wire carries, in the order the contract lists them.</returns>
        public IList<object> Map( string section, IList<string> queryColumns, IList<object> rawValues )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Splits the joined badge keys into the list the wire carries.
        /// </summary>
        /// <param name="joined">The keys as the query returned them.</param>
        /// <returns>The keys.</returns>
        public static IList<Guid> ReadBadgeKeys( object joined )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Works out the colour pair a badge is drawn with.
        /// </summary>
        /// <param name="highlightColor">The colour the church configured, in whatever form.</param>
        /// <returns>The background and the foreground, both null when the colour cannot be read.</returns>
        public static Tuple<string, string> ReadBadgeColors( object highlightColor )
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
