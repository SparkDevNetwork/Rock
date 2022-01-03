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
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Text;

using Rock.Data;

namespace Rock
{
    /// <summary>
    /// Handy string extensions that require Rock references or references to NuGet packages
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Converts a string to a hash value using xxHash, a fast non-cryptographic hash algorithm.
        /// Refer https://www.xxhash.com.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string XxHash( this string str )
        {
            var hash = xxHashSharp.xxHash.CalculateHash( Encoding.UTF8.GetBytes( str ) );

            return hash.ToString();
        }

        /// <summary>
        /// Trims a string using an entities MaxLength attribute value
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static string TrimForMaxLength( this string str, IEntity entity, string propertyName )
        {
            if ( str.IsNotNullOrWhiteSpace() )
            {
                var maxLengthAttr = entity.GetAttributeFrom<MaxLengthAttribute>( propertyName );
                if ( maxLengthAttr != null )
                {
                    return str.Left( maxLengthAttr.Length );
                }
            }

            return str;
        }

        /// <summary>
        /// Parses the query string into a <see cref="NameValueCollection"/> that
        /// can be used to access all the name-value pairs.
        /// </summary>
        /// <param name="str">The string to be parsed.</param>
        /// <returns>A <see cref="NameValueCollection"/> that represents the query string.</returns>
        public static NameValueCollection ParseQueryString( this string str )
        {
            return System.Web.HttpUtility.ParseQueryString( str );
        }
    }
}
