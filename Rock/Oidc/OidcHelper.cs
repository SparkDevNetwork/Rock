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
using System.Linq;

namespace Rock.Oidc
{
    /// <summary>
    /// A helper class to assist with common OIDC tasks.
    /// </summary>
    internal static class OidcHelper
    {
        /// <summary>
        /// OIDC query parameter keys.
        /// </summary>
        private static class QueryParameterKey
        {
            public const string ClientId = "client_id";

            public const string ResponseType = "response_type";

            public const string Scope = "scope";
        }

        /// <summary>
        /// Given the provided return URL, returns the inner query parameters (those that are contained within the
        /// return URL itself).
        /// </summary>
        /// <param name="returnUrl">The return URL from which to get the inner query parameters.</param>
        /// <returns>A dictionary containing each inner query parameter key value combination.</returns>
        public static Dictionary<string, string> GetInnerQueryParams( string returnUrl )
        {
            if ( returnUrl.IsNullOrWhiteSpace() )
            {
                return new Dictionary<string, string>();
            }

            // The return URL parameters will have effectively been double-decoded.
            var decodedReturnUrl = returnUrl.GetFullyUrlDecodedValue();

            var returnUrlParts = decodedReturnUrl.Split( new[] { '?' }, StringSplitOptions.RemoveEmptyEntries );
            if ( returnUrlParts.Length < 2 || returnUrlParts[1].IsNullOrWhiteSpace() )
            {
                return new Dictionary<string, string>();
            }

            return returnUrlParts[1]
                .Split( new[] { '&' }, StringSplitOptions.RemoveEmptyEntries )
                .Select( p => p.Split( new[] { '=' }, 2 ) )
                .ToDictionary(
                    kvp => Uri.UnescapeDataString( kvp[0] ),
                    kvp => kvp.Length > 1 ? Uri.UnescapeDataString( kvp[1] ) : string.Empty
                );
        }

        /// <summary>
        /// Given the provided query parameters, returns the OIDC client_id value.
        /// </summary>
        /// <param name="queryParams">The query parameters from which to get the OIDC client_id value.</param>
        /// <returns>The OIDC client_id value or null.</returns>
        public static string GetClientId( Dictionary<string, string> queryParams )
        {
            if ( queryParams?.TryGetValue( QueryParameterKey.ClientId, out var clientId ) == true )
            {
                return clientId;
            }

            return null;
        }

        /// <summary>
        /// Given the provided query parameters, returns whether an OIDC login attempt is represented within.
        /// </summary>
        /// <param name="queryParams">The query parameters to inspect for evidence of an OIDC login attempt.</param>
        /// <returns>Whether an OIDC login attempt is represented within the provided query parameters.</returns>
        public static bool IsOidcLogin( Dictionary<string, string> queryParams )
        {
            var clientId = GetClientId( queryParams );
            if ( clientId.IsNullOrWhiteSpace() )
            {
                return false;
            }

            if ( !queryParams.TryGetValue( QueryParameterKey.ResponseType, out var responseType ) || responseType.IsNullOrWhiteSpace() )
            {
                return false;
            }

            if ( !queryParams.TryGetValue( QueryParameterKey.Scope, out var scope ) || scope.IsNullOrWhiteSpace() )
            {
                return false;
            }

            return true;
        }
    }
}
