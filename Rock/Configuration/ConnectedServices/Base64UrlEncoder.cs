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

using System;

namespace Rock.Configuration.ConnectedServices
{
    /// <summary>
    /// Provides methods for encoding and decoding base64url strings, as
    /// defined in RFC 4648 §5.
    /// </summary>
    internal static class Base64UrlEncoder
    {
        /// <summary>
        /// Encodes a byte array to a base64url string (RFC 4648 §5):
        /// '+' -> '-', '/' -> '_', and padding '=' stripped.
        /// </summary>
        /// <param name="input">The bytes to encode.</param>
        /// <returns>The base64url-encoded representation of <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> is <c>null</c>.</exception>
        public static string Encode( byte[] input )
        {
            if ( input == null ) throw new ArgumentNullException( nameof( input ) );

            string base64 = Convert.ToBase64String( input );

            // Convert to URL-safe alphabet and strip padding.
            return base64
                .Replace( '+', '-' )
                .Replace( '/', '_' )
                .TrimEnd( '=' );
        }

        /// <summary>
        /// Encodes a UTF-8 string to a base64url string.
        /// </summary>
        /// <param name="input">The string to encode; interpreted as UTF-8.</param>
        /// <returns>The base64url-encoded representation of <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> is <c>null</c>.</exception>
        public static string Encode( string input )
        {
            if ( input == null ) throw new ArgumentNullException( nameof( input ) );

            return Encode( System.Text.Encoding.UTF8.GetBytes( input ) );
        }

        /// <summary>
        /// Decodes a base64url string back to its original byte array.
        /// </summary>
        /// <param name="input">The base64url-encoded string to decode.</param>
        /// <returns>The decoded bytes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException"><paramref name="input"/> is not a valid base64url string.</exception>
        public static byte[] Decode( string input )
        {
            if ( input == null ) throw new ArgumentNullException( nameof( input ) );

            string base64 = input
                .Replace( '-', '+' )
                .Replace( '_', '/' );

            // Restore padding, since Convert.FromBase64String requires it.
            switch ( base64.Length % 4 )
            {
                case 0:
                    break; // no padding needed
                case 2:
                    base64 += "==";
                    break;
                case 3:
                    base64 += "=";
                    break;
                default:
                    throw new FormatException( "Invalid base64url string length." );
            }

            return Convert.FromBase64String( base64 );
        }

        /// <summary>
        /// Decodes a base64url string back to a UTF-8 string.
        /// </summary>
        /// <param name="input">The base64url-encoded string to decode.</param>
        /// <returns>The decoded value interpreted as a UTF-8 string.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException"><paramref name="input"/> is not a valid base64url string.</exception>
        public static string DecodeToString( string input )
        {
            return System.Text.Encoding.UTF8.GetString( Decode( input ) );
        }
    }
}
