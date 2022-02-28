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
using System.Linq;
using System.Web;

/*
 * 2020-11-16 ETD
 * IMPORTANT!
 * This class is used by the CheckScanner which does not have the Rock dll. This file cannot contain any dependencies on that assembly or NuGet packages.
 */

namespace Rock
{
    /// <summary>
    /// Handy string extensions that don't require any NuGet packages or Rock references
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region String Extensions

        /// <summary>
        /// Attempts to convert string to an dictionary using the |/comma and ^ delimiter Key/Value syntax.  Returns an empty dictionary if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static System.Collections.Generic.Dictionary<string, string> AsDictionary( this string str )
        {
            var dictionary = new System.Collections.Generic.Dictionary<string, string>();
            string[] nameValues = str.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );

            // url decode array items just in case they were UrlEncoded (See KeyValueListFieldType and the KeyValueList controls)
            nameValues = nameValues.Select( s => HttpUtility.UrlDecode( s ) ).ToArray();

            // If we haven't found any pipes, check for commas
            if ( nameValues.Count() == 1 && nameValues[0] == str )
            {
                nameValues = str.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            }

            foreach ( string nameValue in nameValues )
            {
                string[] nameAndValue = nameValue.Split( new char[] { '^' }, 2 );
                if ( nameAndValue.Count() == 2 )
                {
                    dictionary[nameAndValue[0]] = nameAndValue[1];
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Attempts to convert string to an dictionary using the |/comma and ^ delimiter Key/Value syntax.  Returns null if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough]
        public static System.Collections.Generic.Dictionary<string, string> AsDictionaryOrNull( this string str )
        {
            var dictionary = AsDictionary( str );
            if ( dictionary.Count() > 0 )
            {
                return dictionary;
            }

            return null;
        }

        #endregion String Extensions
    }
}
