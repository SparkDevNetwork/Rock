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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Newtonsoft.Json.Linq;

namespace Rock.Communication.Chat.Platform.Contract
{
    /// <summary>
    /// The chat wire contract that ships inside this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The chat platform generates this artifact from the catalog of the database its migrations
    /// produce, and Rock carries a copy of the same bytes. It describes the wire between the two:
    /// the ordered column list for each synced table, every enum type's values, the submit headers,
    /// the JWT claim names, and the error codes both sides branch on.
    /// </para>
    /// <para>
    /// Rows are submitted as positional JSON arrays rather than as named fields, so the column
    /// order is something the two sides cannot be allowed to disagree about: two columns of the
    /// same width swapped on one side shift every value one place, and a row-width check cannot
    /// see it. <see cref="ComputedHash"/> is what a submission carries so the platform can refuse a
    /// payload built from a different contract before any of it is applied.
    /// </para>
    /// <para>
    /// The hash is computed here from the artifact's own column lists rather than read from its
    /// <c>wire_hash</c> field, so a file edited by hand disagrees with itself instead of quietly
    /// announcing a wire it does not describe. The recipe is carried in the artifact and is
    /// deliberately over a plain-text serialization rather than over the JSON, so no key order,
    /// whitespace or escaping rule has to be agreed between the language that writes the file and
    /// the language that reads it. The hash covers the column lists and nothing else: it is
    /// compared when a submission arrives, so anything it covered would refuse every submission
    /// from every church still on the previous Rock build the moment it changed, and an added enum
    /// value is meant to reach the platform first and leave those churches working.
    /// </para>
    /// </remarks>
    internal static class ChatWireContract
    {
        #region Fields

        /// <summary>
        /// The manifest name of the embedded artifact. Must stay in sync with the EmbeddedResource
        /// entry in Rock.csproj.
        /// </summary>
        private const string ResourceName = "Rock.Communication.Chat.Platform.Contract.chat-wire-contract.json";

        /// <summary>
        /// The artifact as it ships, read once for the lifetime of the process.
        /// </summary>
        private static readonly string _json;

        /// <summary>
        /// The hash the artifact publishes for itself.
        /// </summary>
        private static readonly string _publishedHash;

        /// <summary>
        /// The hash of the column lists the artifact actually carries.
        /// </summary>
        private static readonly string _computedHash;

        #endregion

        #region Constructors

        /// <summary>
        /// Reads the embedded artifact and hashes its column lists once.
        /// </summary>
        static ChatWireContract()
        {
            _json = ReadEmbeddedJson();

            var contract = JObject.Parse( _json );

            _publishedHash = contract["wire_hash"].Value<string>();
            _computedHash = HashColumnLists( contract );
        }

        #endregion

        #region Properties

        /// <summary>
        /// The artifact as it ships, unparsed.
        /// </summary>
        internal static string Json
        {
            get { return _json; }
        }

        /// <summary>
        /// The hash the artifact publishes for itself. It equals <see cref="ComputedHash"/> for an
        /// artifact that has not been edited since it was generated.
        /// </summary>
        internal static string PublishedHash
        {
            get { return _publishedHash; }
        }

        /// <summary>
        /// The hash of the column lists this assembly will actually build payloads from. This is
        /// the value a submission carries.
        /// </summary>
        internal static string ComputedHash
        {
            get { return _computedHash; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reads the artifact out of this assembly's manifest.
        /// </summary>
        /// <returns>The artifact text.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the artifact is not packaged into the assembly, which is a build failure
        /// rather than a runtime condition: without it nothing here can shape a payload, so it
        /// fails at once rather than shipping rows in an order nobody has agreed to.
        /// </exception>
        private static string ReadEmbeddedJson()
        {
            var assembly = typeof( ChatWireContract ).Assembly;

            using ( var stream = assembly.GetManifestResourceStream( ResourceName ) )
            {
                if ( stream == null )
                {
                    throw new InvalidOperationException( string.Format( "The chat wire contract is not embedded in this assembly as {0}.", ResourceName ) );
                }

                using ( var reader = new StreamReader( stream, Encoding.UTF8 ) )
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Hashes the contract's column lists by the recipe the contract states: one line per table
        /// in the order the tables are listed, the table name, a colon, the wire column names joined
        /// by commas, each line closed by a line feed, hashed as UTF-8 and rendered lowercase hex.
        /// </summary>
        /// <param name="contract">The parsed contract.</param>
        /// <returns>The hash, lowercase hex.</returns>
        private static string HashColumnLists( JObject contract )
        {
            var builder = new StringBuilder();

            foreach ( var table in contract["tables"] )
            {
                builder.Append( table["name"].Value<string>() );
                builder.Append( ':' );
                builder.Append( string.Join( ",", table["columns"].Select( c => c.Value<string>() ) ) );
                builder.Append( '\n' );
            }

            using ( var sha = SHA256.Create() )
            {
                var digest = sha.ComputeHash( Encoding.UTF8.GetBytes( builder.ToString() ) );

                return BitConverter.ToString( digest ).Replace( "-", string.Empty ).ToLowerInvariant();
            }
        }

        #endregion
    }
}
