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
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;

namespace Rock
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Pluralizes the specified string.
        /// </summary>
        /// <param name="str">The string to pluralize.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static string Pluralize( string str )
        {
            // Pluralization services handles most words, but there are some exceptions (i.e. campus)
            switch ( str )
            {
                case "Campus":
                case "campus":
                    return str + "es";

                case "CAMPUS":
                    return str + "ES";

                default:
                    var pluralizationService = PluralizationService.CreateService( new CultureInfo( "en-US" ) );
                    return pluralizationService.Pluralize( str );
            }
        }

        /// <summary>
        /// Pluralizes if the condition is true
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="condition">if set to <c>true</c> [condition].</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static string PluralizeIf( string str, bool condition )
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
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static string Singularize( string str )
        {
            var pluralizationService = PluralizationService.CreateService( new CultureInfo( "en-US" ) );
            return pluralizationService.Singularize( str );
        }

        /// <summary>
        /// Convert string to possessive (’s)
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static string ToPossessive( string str )
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
