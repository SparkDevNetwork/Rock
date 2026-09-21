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
    /// <summary>
    /// Writes the body of a submission: one object keyed by the payload's section names, each
    /// holding that section's rows as positional arrays.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Rows go over the wire as arrays of values rather than as named fields, which halves the
    /// bytes and makes the column order something both sides have to agree about. This writer does
    /// not choose the order: the projection selects its columns in the order the contract lists
    /// them and this writes them out in the order it is handed them. What it does enforce is the
    /// width, because a row one value short shifts every later value one place and the only other
    /// thing that could notice is a type mismatch that may never happen.
    /// </para>
    /// <para>
    /// It writes as it goes rather than building a document and serializing it at the end. The
    /// largest church measured restates in about ten megabytes and the platform's bound is
    /// thirty-two, so holding the whole body as objects and then again as text is tens of megabytes
    /// of large-object heap for nothing. Streaming is also what makes the row counts honest: they
    /// are a tally of what was actually written, so a read that stopped early is short in both the
    /// body and the count, and a body truncated after this point disagrees with a count that was
    /// already taken.
    /// </para>
    /// </remarks>
    internal sealed class ChatSyncPayloadWriter : IDisposable
    {
        #region Fields

        /// <summary>
        /// The parsed wire contract, which decides the section names and each section's width.
        /// </summary>
        private readonly JObject _contract;

        /// <summary>
        /// The writer the body is streamed to.
        /// </summary>
        private readonly JsonWriter _writer;

        /// <summary>
        /// How many rows have been written to each section.
        /// </summary>
        private readonly Dictionary<string, int> _rowCounts = new Dictionary<string, int>();

        /// <summary>
        /// The section currently open, or null between sections.
        /// </summary>
        private string _openSection;

        #endregion

        #region Constructors

        /// <summary>
        /// Writes a body to the supplied writer.
        /// </summary>
        /// <param name="contract">The parsed wire contract.</param>
        /// <param name="writer">Where the body is written.</param>
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

        /// <summary>
        /// How many rows were written to each section, which is what the row-count header carries.
        /// </summary>
        public IDictionary<string, int> RowCounts
        {
            get { return _rowCounts; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// How many values a row of a section carries.
        /// </summary>
        /// <param name="section">The payload section.</param>
        /// <returns>The column count.</returns>
        public int GetRowWidth( string section )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Opens a section and begins its row array.
        /// </summary>
        /// <param name="section">The payload section.</param>
        public void BeginSection( string section )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes one row of the open section.
        /// </summary>
        /// <param name="values">The row's values, in the contract's column order.</param>
        public void WriteRow( IList<object> values )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Closes the open section.
        /// </summary>
        public void EndSection()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Closes the body, which is only valid once every section the contract names has been
        /// written.
        /// </summary>
        public void Complete()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Releases the writer.
        /// </summary>
        public void Dispose()
        {
            _writer.Close();
        }

        #endregion
    }
}
