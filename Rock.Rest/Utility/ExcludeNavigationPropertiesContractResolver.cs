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
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Rest.Utility
{
    /// <summary>
    /// Custom contract resolver that removes any navigation properties from
    /// the result when serializing.
    /// </summary>
    internal class ExcludeNavigationPropertiesContractResolver : DefaultContractResolver
    {
        /// <inheritdoc/>
        protected override IList<JsonProperty> CreateProperties( Type type, MemberSerialization memberSerialization )
        {
            var props = base.CreateProperties( type, memberSerialization );

            props = props
                .Where( p => !IsNavigationPropertyType( p.PropertyType ) )
                .ToList();

            return props;
        }

        /// <summary>
        /// Determines if the type is a navigation property. This is determined
        /// by it either being of type IEntity, IEntityCache, an ICollection&lt;&gt;
        /// of one of those types, or the special properties used by IHasAttributes.
        /// </summary>
        /// <param name="type">The property type.</param>
        /// <returns><c>true</c> if the property type is a navigation property; otherwise <c>false</c>.</returns>
        internal static bool IsNavigationPropertyType( Type type )
        {
            if ( typeof( IEntity ).IsAssignableFrom( type ) || typeof( IEntityCache ).IsAssignableFrom( type ) )
            {
                return true;
            }

            if ( !type.IsGenericType )
            {
                return false;
            }

            if ( type.GetGenericTypeDefinition() == typeof( ICollection<> ) )
            {
                var genericArgs = type.GetGenericArguments();

                return typeof( IEntity ).IsAssignableFrom( genericArgs[0] )
                    || typeof( IEntityCache ).IsAssignableFrom( genericArgs[0] );
            }

            // Special check for IHasAttributes properties.
            if ( type == typeof( Dictionary<string, AttributeCache> ) || type == typeof( Dictionary<string, AttributeValueCache> ) )
            {
                return true;
            }

            return false;
        }
    }
}
