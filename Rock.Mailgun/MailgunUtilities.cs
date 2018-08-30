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
using System.Security.Cryptography;
using System.Text;

namespace Rock.Mailgun
{
    /// <summary>
    /// Static utilities needed to make the mailgun experience more pleasant
    /// </summary>
    public static class MailgunUtilities
    {
        /// <summary>
        /// Authenticates the mailgun request.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="token">The token.</param>
        /// <param name="signature">The signature.</param>
        /// <param name="apiKey">The Mailgun API key.</param>
        /// <returns></returns>
        public static bool AuthenticateMailgunRequest( int timestamp, string token, string signature, string apiKey )
        {
            var key = timestamp.ToString() + token;
            var result = GenerateNewHMAC( apiKey, key );

            return result.Equals( signature, StringComparison.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Generates a signature for hash-based message authentication
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="challenge">The challenge.</param>
        /// <returns></returns>
        private static string GenerateNewHMAC( string password, string challenge )
        {
            // https://github.com/adimoraret/MailgunWebhooks
            var hmac = new HMACSHA256( Encoding.ASCII.GetBytes( password ) );
            var signature = hmac.ComputeHash( Encoding.ASCII.GetBytes( challenge ) );
            return BitConverter.ToString( signature ).Replace( "-", "" );
        }
    }
}
