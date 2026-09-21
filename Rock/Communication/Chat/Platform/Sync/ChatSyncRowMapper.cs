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
    internal sealed class ChatSyncRowMapper
    {
        #region Fields

        private readonly JObject _contract;

        private readonly TimeZoneInfo _organizationTimeZone;

        #endregion

        #region Constructors

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

        private static object Normalize( object value )
        {
            return value == DBNull.Value ? null : value;
        }

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

        private static double RelativeLuminance( int red, int green, int blue )
        {
            return ( 0.2126 * Straighten( red ) ) + ( 0.7152 * Straighten( green ) ) + ( 0.0722 * Straighten( blue ) );
        }

        private static double Straighten( int channel )
        {
            var value = channel / 255.0;

            return value <= 0.03928 ? value / 12.92 : Math.Pow( ( value + 0.055 ) / 1.055, 2.4 );
        }

        #endregion
    }
}
