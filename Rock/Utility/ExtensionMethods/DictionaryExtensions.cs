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
using System.Collections.Generic;

namespace Rock
{
    /// <summary>
    /// Dictionary Extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Gets the value associated with the key or the default of the value's type (typically null)
        /// </summary>
        /// <typeparam name="TK">The key type</typeparam>
        /// <typeparam name="TV">The value type</typeparam>
        /// <param name="dictionary">The dictionary in use</param>
        /// <param name="key">The key to use to retrieve a value from the dictionary</param>
        /// <param name="defaultValue">The value to return if the key does not exist</param>
        /// <returns></returns>
        public static TV GetValueOrDefault<TK, TV>( this IDictionary<TK, TV> dictionary, TK key, TV defaultValue = default( TV ) )
        {
            return dictionary.TryGetValue( key, out TV value ) ? value : defaultValue;
        }
    }
}
