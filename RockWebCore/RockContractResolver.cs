using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Rock.Data;

namespace RockWebCore
{
    /// <summary>
    /// Custom Newtonsoft ContractResolver that handles any differences in the
    /// Rock v2 API from what would otherwise be done in the default resolver.
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Serialization.DefaultContractResolver" />
    public class RockContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
    {
        /// <inheritdoc/>
        protected override List<MemberInfo> GetSerializableMembers( Type objectType )
        {
            var members = base.GetSerializableMembers( objectType );

            if ( typeof( IEntity ).IsAssignableFrom( objectType ) )
            {
                // Exclude any navigation properties or collection type properties
                // as they are no longer supported in the v2 API.
                var excludedProperties = members.Where( m => m is PropertyInfo )
                    .Cast<PropertyInfo>()
                    .Where( p => typeof( IEntity ).IsAssignableFrom( p.PropertyType ) || ( p.PropertyType != typeof( string ) && typeof( IEnumerable ).IsAssignableFrom( p.PropertyType ) ) )
                    .ToList();

                foreach ( var property in excludedProperties )
                {
                    members.Remove( property );
                }
            }

            return members;
        }
    }
}
