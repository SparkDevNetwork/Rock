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
using System.Collections;
using System.Collections.Generic;
using Rock.Common;

namespace Rock.Lava.Filters
{
    public static partial class TemplateFilters
    {
        /// <summary>
        /// Returns the value of a named property implemented by the input object.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="propertyKey">The property key.</param>
        /// <param name="qualifier">The qualifier.</param>
        /// <returns></returns>
        public static object Property( object input, string propertyKey )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            var dictionaryObject = input as IDictionary<string, object>;

            if ( dictionaryObject != null )
            {
                if ( dictionaryObject.ContainsKey( propertyKey ) )
                {
                    return dictionaryObject[propertyKey];
                }
            }

            return input.GetPropertyValue( propertyKey );
        }
    }
}
