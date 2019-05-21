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
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using Rock.Data;

namespace Rock
{
    /// <summary>
    /// Handy string extensions that require Rock references or references to NuGet packages
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Attempts to convert a "MM/dd", "M/dd", "M/d" or "MM/d" string to a datetime, with the year as the current year. Returns null if unsuccessful.
        /// </summary>
        /// <param name="monthDayString">The month day string.</param>
        /// <returns></returns>
        public static DateTime? MonthDayStringAsDateTime( this string monthDayString )
        {
            if ( !string.IsNullOrEmpty( monthDayString ) )
            {
                if ( monthDayString.Length <= 5 )
                {
                    if ( monthDayString.Contains( '/' ) )
                    {
                        DateTime value;
                        var monthDayYearString = $"{monthDayString}/{RockDateTime.Today.Year}";
                        if ( DateTime.TryParseExact(
                                monthDayYearString,
                                new[] { "MM/dd/yyyy", "M/dd/yyyy", "M/d/yyyy", "MM/d/yyyy" },
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.AllowWhiteSpaces,
                                out value ) )
                        {
                            return value;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Attempts to convert string to DateTime.  Returns null if unsuccessful.
        /// NOTE: If this is a '#[#]/#[#]' string it will be interpreted as a "MM/dd", "M/dd", "M/d" or "MM/d" string and will resolve to a datetime with the year as the current year.
        /// However, in those cases, it would be better to use <seealso cref="MonthDayStringAsDateTime(string)"/>
        /// Non-ASCI and control characters are stripped from the string to prevent invisible control characters from causing a null to return.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough]
        public static DateTime? AsDateTime( this string str )
        {
            if ( str == null )
            {
                return null;
            }

            // Edge likes to put in 8206 when doing a toLocaleString(), which makes this method return null.
            // This will correct the error and any other caused by non-ASCI & control characters.
            str = new string( str.Where( c => c > 31 && c < 127 ).ToArray() );

            DateTime value;
            DateTime? valueFromMMDD = str.MonthDayStringAsDateTime();

            // first check if this is a "MM/dd", "M/dd", "M/d" or "MM/d" string ( We want Rock to treat "MM/dd", "M/dd", "M/d" or "MM/d" strings consistently regardless of culture )
            if ( valueFromMMDD.HasValue )
            {
                return valueFromMMDD;
            }
            else if ( DateTime.TryParse( str, out value ) )
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Determines whether [is not null or whitespace].
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>
        ///   <c>true</c> if [is not null or whitespace] [the specified string]; otherwise, <c>false</c>.
        /// </returns>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use IsNotNullOrWhiteSpace instead. Fixes non-standard casing.", false )]
        public static bool IsNotNullOrWhitespace( this string str )
        {
            return !string.IsNullOrWhiteSpace( str );
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
    }
}
