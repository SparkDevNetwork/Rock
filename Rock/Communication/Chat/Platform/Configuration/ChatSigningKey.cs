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

using Microsoft.IdentityModel.Tokens;

namespace Rock.Communication.Chat.Platform.Configuration
{
    /// <summary>
    /// What makes a church's signing key usable. Asked in two places, and so written
    /// once: when a key arrives and is about to be stored, and when one is read back
    /// to sign with. A key the first accepted and the second refuses is a church that
    /// reads as set up and cannot chat, with nothing on either screen to say why.
    /// </summary>
    internal static class ChatSigningKey
    {
        /// <summary>
        /// Whether this JSON is a key chat can sign with: EC on P-256, with a key id
        /// and a private part. Anything that does not parse is not.
        /// </summary>
        /// <param name="privateKeyJwk">The key as JWK JSON.</param>
        /// <returns><c>true</c> when the key can be signed with.</returns>
        public static bool IsUsable( string privateKeyJwk )
        {
            if ( privateKeyJwk.IsNullOrWhiteSpace() )
            {
                return false;
            }

            try
            {
                return IsUsable( new JsonWebKey( privateKeyJwk ) );
            }
            catch ( Exception )
            {
                // Anything the key library will not read is a key nothing can sign with.
                // The text is deliberately not kept: it can quote the key material.
                return false;
            }
        }

        /// <summary>
        /// Whether this key is EC P-256 with a key id and a private part.
        /// </summary>
        /// <param name="jwk">The parsed key.</param>
        /// <returns><c>true</c> when the key can be signed with.</returns>
        public static bool IsUsable( JsonWebKey jwk )
        {
            if ( jwk == null )
            {
                return false;
            }

            if ( jwk.Kid.IsNullOrWhiteSpace() || jwk.D.IsNullOrWhiteSpace() )
            {
                return false;
            }

            if ( !string.Equals( jwk.Kty, "EC", StringComparison.Ordinal ) )
            {
                return false;
            }

            return string.Equals( jwk.Crv, "P-256", StringComparison.Ordinal );
        }
    }
}
