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

namespace Rock.Lava.Filters
{
    public static partial class TemplateFilters
    {
        /// <summary>
        /// Converts a string into a Base64 encoding.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Base64( object input )
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes( input.ToStringSafe() );

            var result = System.Convert.ToBase64String( bytes );

            return result;
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
