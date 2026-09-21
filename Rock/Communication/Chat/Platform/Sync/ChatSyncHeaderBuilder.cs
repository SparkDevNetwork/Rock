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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds the expected row count header.
        /// </summary>
        /// <param name="rowCountsBySection">How many rows each payload section carries.</param>
        /// <returns>The header value.</returns>
        public string BuildRowCounts( IDictionary<string, int> rowCountsBySection )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds the identity high-water header.
        /// </summary>
        /// <param name="marks">The values read from Rock.</param>
        /// <returns>The header value.</returns>
        public string BuildIdentityMarks( ChatSyncIdentityMarks marks )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Formats the read time the whole payload is judged by.
        /// </summary>
        /// <param name="readAtUtc">The UTC time taken before the projection read.</param>
        /// <returns>The header value.</returns>
        public static string FormatReadTime( DateTime readAtUtc )
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
