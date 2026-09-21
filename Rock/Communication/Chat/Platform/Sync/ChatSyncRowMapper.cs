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
            if ( queryColumns == null )
            {
                throw new ArgumentNullException( "queryColumns" );
            }

            if ( rawValues == null )
            {
                throw new ArgumentNullException( "rawValues" );
            }

            if ( queryColumns.Count != rawValues.Count )
            {
                throw new InvalidOperationException( string.Format(
                    "the {0} query returned {1} values for {2} columns",
                    section,
                    rawValues.Count,
                    queryColumns.Count ) );
            }

            var byName = new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase );

            for ( var i = 0; i < queryColumns.Count; i++ )
            {
                byName[queryColumns[i]] = Normalize( rawValues[i] );
            }

            return GetWireColumns( section ).Select( c => ReadWireColumn( section, c, byName ) ).ToList();
        }

        /// <summary>
        /// The one value a wire column carries.
        /// </summary>
        /// <param name="section">The payload section, for the failure message.</param>
        /// <param name="wireColumn">The wire column.</param>
        /// <param name="byName">What the query returned, keyed by the name it gave each column.</param>
        /// <returns>The value.</returns>
        private object ReadWireColumn( string section, string wireColumn, IDictionary<string, object> byName )
        {
            if ( wireColumn == "badge_keys" )
            {
                return ReadBadgeKeys( Require( section, wireColumn, "badge_keys", byName ) );
            }

            if ( wireColumn == "ban_expires_at" )
            {
                return ReadTime( Require( section, wireColumn, "ban_expires_at", byName ) );
            }

            if ( wireColumn == "bg_color" )
            {
                return ReadBadgeColors( Require( section, wireColumn, "highlight_color", byName ) ).Item1;
            }

            if ( wireColumn == "fg_color" )
            {
                return ReadBadgeColors( Require( section, wireColumn, "highlight_color", byName ) ).Item2;
            }

            return Require( section, wireColumn, wireColumn, byName );
        }

        /// <summary>
        /// Reads the query column a wire column is built from, refusing to invent one.
        /// </summary>
        /// <param name="section">The payload section.</param>
        /// <param name="wireColumn">The wire column being built.</param>
        /// <param name="queryColumn">The query column it is built from.</param>
        /// <param name="byName">What the query returned.</param>
        /// <returns>The value.</returns>
        /// <remarks>
        /// Filling a missing column with null would keep the row the right width and leave every
        /// other value in its correct place, so the payload would be accepted and that one column
        /// would be empty for every row of every church, with nothing anywhere reporting it.
        /// </remarks>
        private static object Require( string section, string wireColumn, string queryColumn, IDictionary<string, object> byName )
        {
            object value;

            if ( !byName.TryGetValue( queryColumn, out value ) )
            {
                throw new InvalidOperationException( string.Format(
                    "the {0} query returns no {1}, which the {2} column on the wire is built from",
                    section,
                    queryColumn,
                    wireColumn ) );
            }

            return value;
        }

        /// <summary>
        /// The wire columns of a section, in the order the contract lists them.
        /// </summary>
        /// <param name="section">The payload section.</param>
        /// <returns>The column names.</returns>
        private IList<string> GetWireColumns( string section )
        {
            var sections = _contract["payload"]["sections"].Select( s => s.Value<string>() ).ToList();
            var position = sections.IndexOf( section );

            if ( position < 0 )
            {
                throw new InvalidOperationException( string.Format( "the chat wire contract names no payload section called {0}", section ) );
            }

            return _contract["tables"][position]["columns"].Select( c => c.Value<string>() ).ToList();
        }

        /// <summary>
        /// Turns the absence a data reader reports into the absence the rest of this understands.
        /// </summary>
        /// <param name="value">The value as it was read.</param>
        /// <returns>The value, or null.</returns>
        private static object Normalize( object value )
        {
            return value == DBNull.Value ? null : value;
        }

        /// <summary>
        /// Moves a stored time onto the clock the far side reads it with.
        /// </summary>
        /// <param name="value">The time as Rock stores it.</param>
        /// <returns>The same instant, in UTC.</returns>
        private object ReadTime( object value )
        {
            if ( value == null )
            {
                return null;
            }

            var stored = (DateTime) value;

            if ( stored.Kind == DateTimeKind.Utc )
            {
                return stored;
            }

            // A time out of the database carries no zone, and it is in the organisation's, because
            // that is the only clock Rock writes by. Treating it as already UTC would make it wrong
            // by this church's offset, and for a church behind UTC a ban would lift early.
            var unspecified = DateTime.SpecifyKind( stored, DateTimeKind.Unspecified );

            return TimeZoneInfo.ConvertTimeToUtc( unspecified, _organizationTimeZone );
        }

        /// <summary>
        /// Splits the joined badge keys into the list the wire carries.
        /// </summary>
        /// <param name="joined">The keys as the query returned them.</param>
        /// <returns>The keys.</returns>
        public static IList<Guid> ReadBadgeKeys( object joined )
        {
            var text = Normalize( joined ) as string;

            if ( string.IsNullOrWhiteSpace( text ) )
            {
                // Empty rather than absent: the column on the far side cannot hold nothing, and a
                // person holding no badge is not the same as a row that did not say.
                return new List<Guid>();
            }

            var keys = new List<Guid>();

            foreach ( var part in text.Split( ',' ) )
            {
                var trimmed = part.Trim();

                if ( trimmed.Length == 0 )
                {
                    continue;
                }

                Guid key;

                // Dropped rather than refused, this would hand the church a badge that quietly
                // stops appearing on a submission the far side accepts, with nothing to look at.
                if ( !Guid.TryParse( trimmed, out key ) )
                {
                    throw new InvalidOperationException( string.Format( "the badge key {0} is not an identifier", trimmed ) );
                }

                keys.Add( key );
            }

            return keys;
        }

        /// <summary>
        /// Works out the colour pair a badge is drawn with.
        /// </summary>
        /// <param name="highlightColor">The colour the church configured, in whatever form.</param>
        /// <returns>The background and the foreground, both null when the colour cannot be read.</returns>
        public static Tuple<string, string> ReadBadgeColors( object highlightColor )
        {
            var text = ( Normalize( highlightColor ) as string ?? string.Empty ).Trim();

            // The field is free text in Rock, so a church can put a colour name, a function or
            // anything else in it. A badge with no colour still renders; a submission refused over
            // one badge takes that church down for the whole cycle.
            if ( text.Length == 0 || text[0] != '#' )
            {
                return Tuple.Create( (string) null, (string) null );
            }

            var digits = text.Substring( 1 );

            if ( digits.Length == 3 )
            {
                // The short form is not accepted on the far side, and doubling each digit is what
                // it means everywhere it is written.
                digits = new string( new[] { digits[0], digits[0], digits[1], digits[1], digits[2], digits[2] } );
            }

            if ( digits.Length != 6 || !digits.All( Uri.IsHexDigit ) )
            {
                return Tuple.Create( (string) null, (string) null );
            }

            var red = int.Parse( digits.Substring( 0, 2 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture );
            var green = int.Parse( digits.Substring( 2, 2 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture );
            var blue = int.Parse( digits.Substring( 4, 2 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture );

            var luminance = RelativeLuminance( red, green, blue );

            // Whichever of black and white the eye separates further from this background, by the
            // accessibility contrast ratio rather than by a brightness rule of thumb, so a colour
            // near the boundary gets the answer a checker would give.
            var contrastWithWhite = 1.05 / ( luminance + 0.05 );
            var contrastWithBlack = ( luminance + 0.05 ) / 0.05;

            var foreground = contrastWithWhite >= contrastWithBlack ? "#ffffff" : "#000000";

            return Tuple.Create( "#" + digits.ToLowerInvariant(), foreground );
        }

        /// <summary>
        /// How bright a colour is to the eye, on the scale the accessibility contrast ratio uses.
        /// </summary>
        /// <param name="red">The red channel, 0 to 255.</param>
        /// <param name="green">The green channel, 0 to 255.</param>
        /// <param name="blue">The blue channel, 0 to 255.</param>
        /// <returns>The relative luminance, 0 for black and 1 for white.</returns>
        /// <remarks>
        /// The channels are straightened out of the curve a display applies before they are weighed,
        /// and green counts for far more than blue, which is why a saturated blue reads as dark and
        /// a saturated yellow reads as light even though both are equally far from grey.
        /// </remarks>
        private static double RelativeLuminance( int red, int green, int blue )
        {
            return ( 0.2126 * Straighten( red ) ) + ( 0.7152 * Straighten( green ) ) + ( 0.0722 * Straighten( blue ) );
        }

        /// <summary>
        /// Takes one channel out of the curve a display applies to it.
        /// </summary>
        /// <param name="channel">The channel, 0 to 255.</param>
        /// <returns>The straightened value, 0 to 1.</returns>
        private static double Straighten( int channel )
        {
            var value = channel / 255.0;

            return value <= 0.03928 ? value / 12.92 : Math.Pow( ( value + 0.055 ) / 1.055, 2.4 );
        }

        #endregion
    }
}
