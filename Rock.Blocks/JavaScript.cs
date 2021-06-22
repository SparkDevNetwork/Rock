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

namespace Rock.Blocks
{
    public static class JavaScript
    {
        /// <summary>
        /// Converts to javascript.
        /// </summary>
        /// <param name="value">The value.</param>
        public static string ToJavaScriptObject( object value )
        {
            if ( value == null )
            {
                return "null";
            }

            return value.ToJson();
        }

        /// <summary>
        /// Converts to javascript.
        /// </summary>
        /// <param name="value">The value.</param>
        public static string ToJavaScript( string value )
        {
            if ( value == null )
            {
                return "null";
            }

            return value.ToJson();
        }

        /// <summary>
        /// Converts to javascript.
        /// </summary>
        /// <param name="value">The value.</param>
        public static string ToJavaScript( DateTime? value )
        {
            if ( !value.HasValue )
            {
                return "null";
            }

            return ToJavaScript( value.ToISO8601DateString() );
        }

        /// <summary>
        /// Converts to javascript.
        /// </summary>
        /// <param name="value">The value.</param>
        public static string ToJavaScript( int? value )
        {
            if ( !value.HasValue )
            {
                return "null";
            }

            return value.ToString();
        }

        /// <summary>
        /// Converts to javascript.
        /// </summary>
        /// <param name="value">The values.</param>
        public static string ToJavaScript( List<int> values )
        {
            if ( values == null )
            {
                return "null";
            }

            return values.ToJson();
        }

        /// <summary>
        /// Converts to javascript.
        /// </summary>
        /// <param name="value">The values.</param>
        public static string ToJavaScript( List<string> values )
        {
            if ( values == null )
            {
                return "null";
            }

            return values.ToJson();
        }
    }
}
