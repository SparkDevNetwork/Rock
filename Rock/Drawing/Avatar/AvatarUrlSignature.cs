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
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;

using Microsoft.IdentityModel.Tokens;

using Rock.Attribute;
using Rock.Security;

namespace Rock.Drawing.Avatar
{
    /// <summary>
    /// Signs avatar URL parameters and validates the signature and expiration of avatar requests.
    /// </summary>
    [RockInternal( "20.1" )]
    public static class AvatarUrlSignature
    {
        #region Fields

        /// <summary>
        /// The query string key that holds the expiration date and time.
        /// </summary>
        private const string ExpiresKey = "e";

        /// <summary>
        /// The query string key that holds the signature token.
        /// </summary>
        private const string TokenKey = "t";

        /// <summary>
        /// The number of days a signed URL keeps showing the photo, counted from the start of the day it was created.
        /// </summary>
        private const int ExpirationDays = 7;

        /// <summary>
        /// The purpose that scopes the signing key to avatar URLs.
        /// </summary>
        private static readonly string[] _purposes = new[] { "Rock.Avatar.UrlSignature" };

        /*
            10/06/26 - JMH

            Only the parameters that decide whether a photo is shown are signed: the photo's
            fileIdKey here, with the expiration appended last. Display parameters (size, style,
            colors, radius, text, gender, age) stay unsigned because Rock blocks, shipped mobile
            templates, and custom Lava append them to PhotoUrl, and changing them can only restyle
            the same photo, never reveal a different one.

            Reason: Protect which photo is shown without breaking URLs that callers extend.
        */
        private static readonly string[] _signedKeys = new[]
        {
            "fileidkey"
        };

        #endregion Fields

        #region Methods

        /// <summary>
        /// Builds a URL-encoded avatar query string that includes an expiration and a signature.
        /// </summary>
        /// <param name="parameters">The avatar parameters in the order they should appear in the URL; parameters with an empty value are left out.</param>
        /// <returns>The query string without a leading question mark.</returns>
        internal static string GetSignedQueryString( IEnumerable<KeyValuePair<string, string>> parameters )
        {
            var includedParameters = parameters
                .Where( p => p.Value.IsNotNullOrWhiteSpace() )
                .ToList();

            var values = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

            foreach ( var parameter in includedParameters )
            {
                values[parameter.Key] = parameter.Value;
            }

            // Rounding to the start of the day keeps the URL identical all day so browser and server caches still hit.
            var expires = RockDateTime.Today.AddDays( ExpirationDays ).ToString( "s" );
            var token = ComputeToken( key => values.TryGetValue( key, out var value ) ? value : null, expires );

            var pairs = includedParameters
                .Select( p => $"{p.Key}={Uri.EscapeDataString( p.Value )}" )
                .ToList();

            pairs.Add( $"{ExpiresKey}={Uri.EscapeDataString( expires )}" );
            pairs.Add( $"{TokenKey}={token}" );

            return string.Join( "&", pairs );
        }

        /// <summary>
        /// Determines whether an avatar request carries a valid signature that has not expired.
        /// </summary>
        /// <param name="queryString">The request's query string, with case-insensitive keys and URL-decoded values.</param>
        /// <param name="expiresDateTime">When this method returns <c>true</c>, the date and time the URL stops showing the photo; otherwise <see cref="DateTime.MinValue"/>.</param>
        /// <returns><c>true</c> if the token matches the signed parameters and the URL has not expired; otherwise <c>false</c>.</returns>
        public static bool TryValidate( NameValueCollection queryString, out DateTime expiresDateTime )
        {
            expiresDateTime = DateTime.MinValue;

            var token = queryString?[TokenKey];
            var expires = queryString?[ExpiresKey];

            if ( token.IsNullOrWhiteSpace() || expires.IsNullOrWhiteSpace() )
            {
                return false;
            }

            // Parse strictly with the same "s" format we sign with; AsDateTime() is lenient and culture-sensitive.
            if ( !DateTime.TryParseExact( expires, "s", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedExpires ) )
            {
                return false;
            }

            if ( RockDateTime.Now >= parsedExpires )
            {
                return false;
            }

            var expectedToken = ComputeToken( key => queryString[key], expires );
            var isMatch = Encryption.ConstantTimeEquals( Encoding.ASCII.GetBytes( expectedToken ), Encoding.ASCII.GetBytes( token ) );

            if ( isMatch )
            {
                expiresDateTime = parsedExpires;
            }

            return isMatch;
        }

        /// <summary>
        /// Computes the signature token for the signed avatar parameters and expiration.
        /// </summary>
        /// <param name="getValue">Returns the value of a parameter by its lowercase key, or <c>null</c> when it is absent.</param>
        /// <param name="expires">The expiration value as it appears in the URL.</param>
        /// <returns>The base64url-encoded HMAC-SHA256 signature.</returns>
        private static string ComputeToken( Func<string, string> getValue, string expires )
        {
            // Escaping each value keeps a value that contains "&" or "=" from shifting into the next key.
            var canonicalPairs = _signedKeys
                .Select( key => $"{key}={Uri.EscapeDataString( getValue( key ) ?? string.Empty )}" )
                .Concat( new[] { $"{ExpiresKey}={Uri.EscapeDataString( expires )}" } );

            var canonical = string.Join( "&", canonicalPairs );
            var signature = Encryption.ComputePurposeHmacSha256( Encoding.UTF8.GetBytes( canonical ), _purposes );

            return Base64UrlEncoder.Encode( signature );
        }

        #endregion Methods
    }
}
