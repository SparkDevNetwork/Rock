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
using System.Collections.Generic;
using Rock.Field;

namespace Rock
{
    /// <summary>
    /// Dictionary Extensions that are specific to Rock.Field
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Gets ConfigurationValue's Value for the specified key, or null if the dictionary doesn't contain the key or the ConfigurationValue is null
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetValueOrNull<TKey>( this IDictionary<TKey, ConfigurationValue> dictionary, TKey key )
        {
            if ( dictionary.ContainsKey( key ) && dictionary[key] != null )
            {
                return dictionary[key].Value;
            }
            else
            {
                return null;
            }
        }
    }
}
