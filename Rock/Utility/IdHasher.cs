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

using System.Configuration;

using HashidsNet;

namespace Rock.Utility
{
    /// <summary>
    /// Provides id hasing functionality to Rock. This allows for the conversion
    /// of integer id numbers into a hashed text key. These keys are non-sequential
    /// and cannot be guessed. This is not a cryptographic one-way hash.
    /// </summary>
    public class IdHasher
    {
        /// <summary>
        /// Gets the default instance.
        /// </summary>
        /// <value>The default instance.</value>
        public static IdHasher Instance { get; } = new IdHasher();

        /// <summary>
        /// The underlying hashing provider used to encode and decode hashes.
        /// </summary>
        private readonly IHashids _hasher;

        /// <summary>
        /// Prevents a default instance of the <see cref="IdHasher"/> class from being created.
        /// </summary>
        private IdHasher()
        {
            // Initialize the hasher with the encryption key trimmed down to an
            // acceptable length. The salt must be shorter than the length of
            // the available characters.
            var salt = ConfigurationManager.AppSettings["DataEncryptionKey"].Left( 40 );

            _hasher = new Hashids( salt, 10 );
        }

        /// <summary>
        /// Gets the identifier from the hashed key. The hashed key must contain
        /// one and only one identifier.
        /// </summary>
        /// <param name="hashedKey">The hashed key.</param>
        /// <returns>The integer identifier contained in the hash, or <c>null</c> if not valid.</returns>
        public int? GetId( string hashedKey )
        {
            var ids = _hasher.Decode( hashedKey );

            return ids.Length == 1 ? ( int? ) ids[0] : null;
        }

        /// <summary>
        /// Attempts to get the identifier from the hashed key. The hashed key
        /// must contain one and only one identifier.
        /// </summary>
        /// <param name="hashedKey">The hashed key.</param>
        /// <param name="id">On return contains the integer identifier found in the hash, or <c>0</c> if not valid.</param>
        /// <returns><c>true</c> if an integer identifer was found, <c>false</c> otherwise.</returns>
        public bool TryGetId( string hashedKey, out int id )
        {
            var unhashedId = GetId( hashedKey );

            if ( unhashedId.HasValue )
            {
                id = unhashedId.Value;
                return true;
            }
            else
            {
                id = 0;
                return false;
            }
        }

        /// <summary>
        /// Gets the hash key for the single identifier value.
        /// </summary>
        /// <param name="id">The identifier to be encoded as a hashed key.</param>
        /// <returns>A string containing the hashed key that represents the identifier.</returns>
        public string GetHash( int id )
        {
            return _hasher.Encode( id );
        }
    }
}
