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

namespace Rock.Obsidian.UI
{
    /// <summary>
    /// The entity keys found in the grid rows a person was shown, grouped by
    /// the entity type each key identifies.
    /// </summary>
    internal sealed class GridEntityKeys
    {
        // IdKeys are case sensitive, so keys must match exactly.
        private readonly Dictionary<int, HashSet<string>> _keysByEntityTypeId = new Dictionary<int, HashSet<string>>();

        private readonly HashSet<string> _untypedKeys = new HashSet<string>( StringComparer.Ordinal );

        /// <summary>
        /// Adds keys that identify records of the entity type.
        /// </summary>
        /// <param name="entityTypeId">The identifier of the entity type the keys identify.</param>
        /// <param name="keys">The keys to add.</param>
        internal void AddKeys( int entityTypeId, IEnumerable<string> keys )
        {
            if ( !_keysByEntityTypeId.TryGetValue( entityTypeId, out var typedKeys ) )
            {
                typedKeys = new HashSet<string>( StringComparer.Ordinal );
                _keysByEntityTypeId.Add( entityTypeId, typedKeys );
            }

            typedKeys.UnionWith( keys );
        }

        /// <summary>
        /// Adds keys that are not tied to an entity type.
        /// </summary>
        /// <param name="keys">The keys to add, which are accepted for every entity type.</param>
        internal void AddUntypedKeys( IEnumerable<string> keys )
        {
            _untypedKeys.UnionWith( keys );
        }

        /// <summary>
        /// Gets the keys that may be used for records of the entity type.
        /// </summary>
        /// <param name="entityTypeId">The identifier of the entity type being requested.</param>
        /// <returns>The keys of that entity type plus every untyped key, compared with exact case.</returns>
        internal ICollection<string> GetKeys( int entityTypeId )
        {
            var keys = new HashSet<string>( _untypedKeys, StringComparer.Ordinal );

            if ( _keysByEntityTypeId.TryGetValue( entityTypeId, out var typedKeys ) )
            {
                keys.UnionWith( typedKeys );
            }

            return keys;
        }
    }
}
