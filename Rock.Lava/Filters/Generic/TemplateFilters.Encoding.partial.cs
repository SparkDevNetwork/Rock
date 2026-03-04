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

namespace Rock.Lava.Filters
{
    public static partial class TemplateFilters
    {
        /// <summary>
        /// Converts a string into a Base64 encoding.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Obsolete( "Use ToBase64 instead." )]
        [RockObsolete( "19.0" )]
        public static string Base64( object input )
        {
            return ToBase64( input );
        }

        /// <summary>
        /// Converts a string or byte array into a Base64 encoded string.
        /// </summary>
        /// <param name="input">The string or byte array to be converted.</param>
        /// <example><![CDATA[
        /// {{ 'hello' | ToBase64 }}
        /// ]]></example>
        /// <returns>a Base64 encoded string</returns>
        public static string ToBase64( object input )
        {
            // If already byte array
            if ( input is byte[] byteArray )
            {
                return Convert.ToBase64String( byteArray );
            }
            else if ( input is ICollection<byte> byteCollection )
            {
                return Convert.ToBase64String( byteCollection.ToArray() );
            }
            // Enumerable handling (covers List<object>, etc.)
            else if ( input is System.Collections.IEnumerable enumerable && !( input is string ) )
            {
                var objects = enumerable.Cast<object>().ToList();

                // If it's INTs in the byte range, treat as raw bytes.
                if ( objects.All( o => o is int ) )
                {
                    var ints = objects.Cast<int>().ToArray();

                    // Validate byte range to avoid silent truncation.
                    if ( ints.Any( i => i < byte.MinValue || i > byte.MaxValue ) )
                    {
                        // Fall back to text if these aren't byte-like values.
                        return Convert.ToBase64String( System.Text.Encoding.UTF8.GetBytes( input.ToStringSafe() ) );
                    }

                    var bytes = ints.Select( i => ( byte ) i ).ToArray();
                    return Convert.ToBase64String( bytes );
                }

                // If it's bytes boxed as objects (less common but possible)
                if ( objects.All( o => o is byte ) )
                {
                    return Convert.ToBase64String( objects.Cast<byte>().ToArray() );
                }

                // Otherwise, treat as text.
                return Convert.ToBase64String( System.Text.Encoding.UTF8.GetBytes( input.ToStringSafe() ) );
            }

            return Convert.ToBase64String( System.Text.Encoding.UTF8.GetBytes( input.ToStringSafe() ) );
        }

        /// <summary>
        /// Convert the given string or byte array into a base64 string.
        /// </summary>
        /// <param name="input">The string or byte array to be converted.</param>
        /// <param name="asString">If true then the returned data is cast to a string.</param>
        /// <example><![CDATA[
        /// {{ 'aGVsbG8=' | FromBase64:true }}
        /// {{ 'aGVsbG8=' | FromBase64 | ComputeHash }}
        /// ]]></example>
        public static object FromBase64( object input, object asString = null )
        {
            var data = Convert.FromBase64String( input.ToString() );

            if ( asString != null && asString.ToString().AsBoolean() )
            {
                return System.Text.Encoding.UTF8.GetString( data );
            }

            return data;
        }

        /// <summary>
        /// Converts a string into an MD5 hash.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Md5( object input )
        {
            return input.ToStringSafe().Md5Hash();
        }

        /// <summary>
        /// Converts a string into a SHA-1 hash using a hash message authentication code (HMAC).
        /// </summary>
        /// <param name="input"></param>
        /// <param name="arguments"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string HmacSha1( object input, string key )
        {
            return input.ToStringSafe().HmacSha1Hash( key );
        }

        /// <summary>
        /// Converts a string into a SHA-256 hash using a hash message authentication code (HMAC).
        /// </summary>
        /// <param name="input"></param>
        /// <param name="arguments"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string HmacSha256( object input, string key )
        {
            return input.ToStringSafe().HmacSha256Hash( key );
        }

        /// <summary>
        /// Converts a string into a SHA-1 hash.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="arguments"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string Sha1( object input )
        {
            return input.ToStringSafe().Sha1Hash();
        }

        /// <summary>
        /// Converts a string into a SHA-256 hash.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="arguments"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string Sha256( object input )
        {
            return input.ToStringSafe().Sha256Hash();
        }
    }
}
