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
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Rock
{
    /// <summary>
    /// Extension methods related to <see cref="NameValueCollection"/>.
    /// </summary>
    internal static class NameValueCollectionExtensions
    {
        /// <summary>
        /// Converts a <see cref="NameValueCollection"/> into a query string by
        /// URL encoding the keys and values.
        /// </summary>
        /// <param name="nvc">The <see cref="NameValueCollection"/> that contains the key-value pairs.</param>
        /// <returns>A string that represents the <see cref="NameValueCollection"/> formatted as a query string, does not include the leading ? character.</returns>
        public static string ToQueryString( this NameValueCollection nvc )
        {
            return nvc.ToQueryString( true );
        }

        /// <summary>
        /// Converts a <see cref="NameValueCollection"/> into a query string.
        /// </summary>
        /// <param name="nvc">The <see cref="NameValueCollection"/> that contains the key-value pairs.</param>
        /// <param name="urlEncoded">If <c>true</c> then the keys and values will be URL encoded.</param>
        /// <returns>A string that represents the <see cref="NameValueCollection"/> formatted as a query string, does not include the leading ? character.</returns>
        /// <remarks>
        /// Note that this method uses HttpUtility.UrlEncode() to perform the encoding, which has some potential inconsistencies:
        /// it ignores some reserved characters such as "!()*", and encodes spaces as "+" rather than "%20".
        /// Refer to <see cref="ToQueryStringEscaped(NameValueCollection)"></see> for a stricter implementation of this method.
        /// </remarks>
        public static string ToQueryString( this NameValueCollection nvc, bool urlEncoded )
        {
            var s = new StringBuilder();

            // Loop over each key value pair and append them to the query string.
            for ( int i = 0; i < nvc.Count; i++ )
            {
                // Get the key for the current pair.
                var key = urlEncoded ? HttpUtility.UrlEncode( nvc.GetKey( i ) ) : nvc.GetKey( i );

                // If the key is valid we need to use a=b syntax, otherwise
                // we will just append the value later.
                var keyPrefix = key != null ? ( key + "=" ) : string.Empty;

                if ( s.Length > 0 )
                {
                    s.Append( '&' );
                }

                // Get all the values related to this key.
                var values = nvc.GetValues( i );

                if ( values == null || values.Length == 0 )
                {
                    s.Append( keyPrefix );
                }
                else
                {
                    // Append each value with the key prefix.
                    for ( int v = 0; v < values.Length; v++ )
                    {
                        if ( v > 0 )
                        {
                            s.Append( '&' );
                        }

                        s.Append( keyPrefix );
                        s.Append( urlEncoded ? HttpUtility.UrlEncode( values[v] ) : values[v] );
                    }
                }
            }

            return s.ToString();
        }

        /// <summary>
        /// Converts a <see cref="NameValueCollection"/> into a query string.
        /// </summary>
        /// <param name="nvc">The <see cref="NameValueCollection"/> that contains the key-value pairs.</param>
        /// <returns>A string that represents the <see cref="NameValueCollection"/> formatted as a query string, does not include the leading ? character.</returns>
        /// <remarks>
        /// This is a re-implementation of <see cref="ToQueryString(NameValueCollection, bool)"/> using the Uri.EscapeDataString() method to encode the data.
        /// This method is considered safer because it correctly encodes all illegal and reserved characters.
        /// See https://stackoverflow.com/questions/602642/server-urlencode-vs-httputility-urlencode/47877559#47877559 for more information.
        /// </remarks>
        public static string ToQueryStringEscaped( this NameValueCollection nvc )
        {
            var s = new StringBuilder();

            // Loop over each key value pair and append them to the query string.
            for ( int i = 0; i < nvc.Count; i++ )
            {
                // Get the key for the current pair.
                var key = Uri.EscapeDataString( nvc.GetKey( i ) );

                // If the key is valid we need to use a=b syntax, otherwise
                // we will just append the value later.
                var keyPrefix = key != null ? ( key + "=" ) : string.Empty;

                if ( s.Length > 0 )
                {
                    s.Append( '&' );
                }

                // Get all the values related to this key.
                var values = nvc.GetValues( i );

                if ( values == null || values.Length == 0 )
                {
                    s.Append( keyPrefix );
                }
                else
                {
                    // Append each value with the key prefix.
                    for ( int v = 0; v < values.Length; v++ )
                    {
                        if ( v > 0 )
                        {
                            s.Append( '&' );
                        }

                        s.Append( keyPrefix );
                        s.Append( Uri.EscapeDataString( values[v] ) );
                    }
                }
            }

            return s.ToString();
        }
    }
}
