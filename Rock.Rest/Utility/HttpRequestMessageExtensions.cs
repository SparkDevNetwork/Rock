// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace System.Web.Http
{

    /// <summary>
    /// Extends the HttpRequestMessage collection
    /// </summary>
    public static class HttpRequestMessageExtensions
    {

        /// <summary>
        /// Returns a dictionary of QueryStrings that's easier to work with 
        /// than GetQueryNameValuePairs KevValuePairs collection.
        /// 
        /// If you need to pull a few single values use GetQueryString instead.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetQueryStrings( this HttpRequestMessage request )
        {
            return request.GetQueryNameValuePairs()
                          .ToDictionary( kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Returns an individual querystring value
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetQueryString( this HttpRequestMessage request, string key )
        {
            // IEnumerable<KeyValuePair<string,string>> - right!
            var queryStrings = request.GetQueryNameValuePairs();
            if ( queryStrings == null )
                return null;

            var match = queryStrings.FirstOrDefault( kv => string.Compare( kv.Key, key, true ) == 0 );
            if ( string.IsNullOrEmpty( match.Value ) )
                return null;

            return match.Value;
        }

        /// <summary>
        /// Returns an individual HTTP Header value
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetHeader( this HttpRequestMessage request, string key )
        {
            IEnumerable<string> keys = null;
            if ( !request.Headers.TryGetValues( key, out keys ) )
                return null;

            return keys.First();
        }

        /// <summary>
        /// Retrieves an individual cookie from the cookies collection
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cookieName"></param>
        /// <returns></returns>
        public static string GetCookie( this HttpRequestMessage request, string cookieName )
        {
            CookieHeaderValue cookie = request.Headers.GetCookies( cookieName ).FirstOrDefault();
            if ( cookie != null )
                return cookie[cookieName].Value;

            return null;
        }

    }
}