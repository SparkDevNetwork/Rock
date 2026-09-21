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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rock.Communication.Chat.Platform.Sync
{
    internal sealed class ChatSyncPayloadWriter : IDisposable
    {
        #region Fields

        private readonly JObject _contract;

        private readonly JsonWriter _writer;

        private readonly Dictionary<string, int> _rowCounts = new Dictionary<string, int>();

        private string _openSection;

        #endregion

        #region Constructors

        public ChatSyncPayloadWriter( JObject contract, JsonWriter writer )
        {
            if ( contract == null )
            {
                throw new ArgumentNullException( "contract" );
            }

            if ( writer == null )
            {
                throw new ArgumentNullException( "writer" );
            }

            _contract = contract;
            _writer = writer;

            _writer.WriteStartObject();
        }

        #endregion

        #region Properties

        public IDictionary<string, int> RowCounts
        {
            get { return _rowCounts; }
        }

        #endregion

        #region Methods

        public int GetRowWidth( string section )
        {
            var sections = GetSections();
            var position = sections.IndexOf( section );

            if ( position < 0 )
            {
                throw new InvalidOperationException( string.Format( "the chat wire contract names no payload section called {0}", section ) );
            }

            var tables = _contract["tables"];

            // The contract states that a section holds the rows of the table in the same position
            // in its table list, which is the only thing that ties a section to a width.
            if ( tables == null || tables.Count() != sections.Count )
            {
                throw new InvalidOperationException( "the chat wire contract names a different number of payload sections than tables, so no section can be matched to a width" );
            }

            return tables[position]["columns"].Count();
        }

        private IList<string> GetSections()
        {
            var sections = _contract["payload"] == null ? null : _contract["payload"]["sections"];

            if ( sections == null )
            {
                throw new InvalidOperationException( "the chat wire contract does not name the payload sections, so nothing here can key a body" );
            }

            return sections.Select( s => s.Value<string>() ).ToList();
        }

        public void BeginSection( string section )
        {
            if ( _openSection != null )
            {
                throw new InvalidOperationException( string.Format( "the {0} section is still open", _openSection ) );
            }

            if ( _rowCounts.ContainsKey( section ) )
            {
                throw new InvalidOperationException( string.Format( "the {0} section has already been written", section ) );
            }

            // Asks the contract for the width now rather than at the first row, so a section name
            // the contract does not know fails where it was named.
            GetRowWidth( section );

            _openSection = section;
            _rowCounts[section] = 0;

            _writer.WritePropertyName( section );
            _writer.WriteStartArray();
        }

        public void WriteRow( IList<object> values )
        {
            if ( _openSection == null )
            {
                throw new InvalidOperationException( "no payload section is open" );
            }

            if ( values == null )
            {
                throw new ArgumentNullException( "values" );
            }

            var width = GetRowWidth( _openSection );

            // A row of the wrong width shifts every value after the gap one place. Nothing further
            // down can see that once the types on either side of the gap happen to agree, so it is
            // refused here rather than sent.
            if ( values.Count != width )
            {
                throw new InvalidOperationException( string.Format(
                    "a {0} row carries {1} values where the contract gives that table {2} columns",
                    _openSection,
                    values.Count,
                    width ) );
            }

            _writer.WriteStartArray();

            foreach ( var value in values )
            {
                WriteValue( value );
            }

            _writer.WriteEndArray();

            _rowCounts[_openSection] = _rowCounts[_openSection] + 1;
        }

        private void WriteValue( object value )
        {
            if ( value == null )
            {
                _writer.WriteNull();
                return;
            }

            if ( value is Guid )
            {
                // Lowercase and hyphenated is the one form that parses as a uuid on the far side
                // and compares equal to the same value already stored there. SQL Server renders
                // them uppercase by default and orders their bytes differently again.
                _writer.WriteValue( ( (Guid)value ).ToString( "D" ).ToLowerInvariant() );
                return;
            }

            if ( value is DateTime )
            {
                WriteTime( (DateTime)value );
                return;
            }

            var guids = value as IEnumerable<Guid>;

            if ( guids != null )
            {
                // The column behind this is a uuid array, and the drain reads anything that is not
                // a JSON array as an empty one, so a joined string would give a person no badges
                // on a submission the platform accepts with nothing reported anywhere.
                _writer.WriteStartArray();

                foreach ( var guid in guids )
                {
                    _writer.WriteValue( guid.ToString( "D" ).ToLowerInvariant() );
                }

                _writer.WriteEndArray();
                return;
            }

            if ( value is string || value is bool || value is int || value is long || value is short || value is byte || value is decimal || value is double )
            {
                _writer.WriteValue( value );
                return;
            }

            // Anything else would be serialized by whatever Json.NET decides, which is how a value
            // reaches the wire in a shape nobody chose.
            throw new InvalidOperationException( string.Format(
                "a {0} row carries a {1}, which has no agreed form on the wire",
                _openSection,
                value.GetType().Name ) );
        }

        private void WriteTime( DateTime value )
        {
            if ( value.Kind != DateTimeKind.Utc )
            {
                throw new InvalidOperationException( string.Format(
                    "a {0} row carries a time that is not UTC, so the platform would read it in its own zone and the value would be wrong by this church's offset",
                    _openSection ) );
            }

            _writer.WriteValue( value.ToString( "yyyy-MM-ddTHH:mm:ss.ffffff'Z'", CultureInfo.InvariantCulture ) );
        }

        public void EndSection()
        {
            if ( _openSection == null )
            {
                throw new InvalidOperationException( "no payload section is open" );
            }

            _writer.WriteEndArray();
            _openSection = null;
        }

        public void Complete()
        {
            if ( _openSection != null )
            {
                throw new InvalidOperationException( string.Format( "the {0} section is still open", _openSection ) );
            }

            // A section left out is not a church with none of that row, it is a projection that did
            // not run, and the platform applies a restatement as truth.
            var missing = GetSections().Except( _rowCounts.Keys ).ToList();

            if ( missing.Any() )
            {
                throw new InvalidOperationException( string.Format(
                    "the body was completed without the {0} section, which the platform would apply as an empty church",
                    string.Join( ", ", missing ) ) );
            }

            _writer.WriteEndObject();
        }

        public void Dispose()
        {
            _writer.Close();
        }

        #endregion
    }
}
