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

namespace Rock
{
    /// <summary>
    /// Extension methods for <see cref="string"/> that relate to pluralization.
    /// </summary>
    public static class StringPluralizationExtensions
    {
        /// <summary>
        /// Pluralizes the specified string.
        /// </summary>
        /// <param name="str">The string to pluralize.</param>
        /// <returns></returns>
        public static string Pluralize( this string str )
        {
            // Humanizer handles most words, but there are some exceptions (i.e. CAMPUS)
            switch ( str )
            {
                case "CAMPUS":
                    return str + "ES";

                default:
                    return Humanizer.InflectorExtensions.Pluralize( str, false );
            }
        }

        /// <summary>
        /// Pluralizes if the condition is true
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="condition">if set to <c>true</c> [condition].</param>
        /// <returns></returns>
        public static string PluralizeIf( this string str, bool condition )
        {
            if ( condition )
            {
                return str.Pluralize();
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// Singularizes the specified string.
        /// </summary>
        /// <param name="str">The string to singularize.</param>
        /// <returns></returns>
        public static string Singularize( this string str )
        {
            return Humanizer.InflectorExtensions.Singularize( str, false );
        }

        /// <summary>
        /// Convert string to possessive (’s)
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string ToPossessive( this string str )
        {
            if ( str == null )
            {
                return str;
            }

            if ( str.ToLower().EndsWith( "s" ) )
            {
                return $"{str}’";
            }

            if ( str.Length > 0 )
            {
                string poss = char.IsUpper( str[str.Length - 1] ) ? "S" : "s";
                return $"{str}’{poss}";
            }

            return str;
        }

    }
}
