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
using System.Reflection;
using Fluid;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// An implementation of a MemberAccessStrategy for the Fluid framework.
    /// The MemberAccessStrategy determines the way in which property values are retrieved from specific types of objects supported by Lava:
    /// anonymous types, types that implement ILavaDataDictionary or ILavaDataDictionarySource, and types that are decorated with the LavaTypeAttribute.
    /// </summary>
    internal class LavaObjectMemberAccessStrategy : MemberAccessStrategy
    {
        private Dictionary<Type, Dictionary<string, IMemberAccessor>> _map;
        private readonly MemberAccessStrategy _parent;

        private DynamicMemberAccessor _dynamicMemberAccessor = new DynamicMemberAccessor();
        private LavaDataSourceMemberAccessor _lavaDataSourceMemberAccessor = new LavaDataSourceMemberAccessor();

        public LavaObjectMemberAccessStrategy()
        {
            _map = new Dictionary<Type, Dictionary<string, IMemberAccessor>>();
        }

        public LavaObjectMemberAccessStrategy( MemberAccessStrategy parent ) : this()
        {
            _parent = parent;
            MemberNameStrategy = _parent.MemberNameStrategy;
        }

        /// <summary>
        /// A flag indicating if an exception should be thrown when a member access is invalid.
        /// If set to false, an invalid access returns an empty value.
        /// </summary>
        public bool ThrowOnInvalidMemberAccess { get; set; } = true;

        public override IMemberAccessor GetAccessor( Type type, string name )
        {
            IMemberAccessor accessor = null;

            /*
             * To access the members of an anonymous type, we need to use reflection.
             * The entire MemberAccessStrategy must be reimplemented rather than simply registering a new accessor via the Register() method,
             * because the type being accessed is not known in advance.
             * This check for an anonymous type is fairly naive, but it works correctly for the .Net and Mono frameworks.
             * Refer https://stackoverflow.com/questions/2483023/how-to-test-if-a-type-is-anonymous.
             */
            if ( type.Name.Contains( "AnonymousType" ) || type.Name.Contains( "AnonType" ) )
            {
                return _dynamicMemberAccessor;
            }

            // Check for ILavaDataDictionary implementation.
            if ( typeof( ILavaDataDictionarySource ).IsAssignableFrom( type )
                 || typeof( ILavaDataDictionary ).IsAssignableFrom( type ) )
            {
                return _lavaDataSourceMemberAccessor;
            }

            var isMapped = _map.ContainsKey( type );

            if ( !isMapped )
            {
                // Check for LavaTypeAttribute and if it exists, register a new member accessor for the decorated type.
                // Subsequent requests will use the registered map.
                var attr = (LavaTypeAttribute)type.GetCustomAttributes( typeof( LavaTypeAttribute ), false ).FirstOrDefault();

                if ( attr != null )
                {
                    RegisterLavaTypeProperties( type, attr );
                }
            }

            // Check for a specific property map for any Type in the inheritance chain.
            var mapType = type;

            while ( mapType != typeof( object ) )
            {
                // Look for specific property map
                if ( _map.TryGetValue( mapType, out var typeMap ) )
                {
                    if ( typeMap.TryGetValue( name, out accessor ) || typeMap.TryGetValue( "*", out accessor ) )
                    {
                        return accessor;
                    }
                }

                accessor = accessor ?? _parent?.GetAccessor( mapType, name );

                if ( accessor != null )
                {
                    return accessor;
                }

                mapType = mapType.GetTypeInfo().BaseType;
            }

            return null;
        }

        private void RegisterLavaTypeProperties( Type type, LavaTypeAttribute attr )
        {
            List<PropertyInfo> includedProperties;

            // Get the list of included properties, then remove the ignored properties.
            if ( attr.AllowedMembers == null || !attr.AllowedMembers.Any() )
            {
                // No included properties have been specified, so assume all are included.
                includedProperties = type.GetProperties().ToList();
            }
            else
            {
                includedProperties = type.GetProperties().Where( x => attr.AllowedMembers.Contains( x.Name, StringComparer.OrdinalIgnoreCase ) ).ToList();
            }

            var ignoredProperties = type.GetProperties().Where( x => x.GetCustomAttributes( typeof( LavaHiddenAttribute ), false ).Any() ).ToList();

            foreach ( var includedProperty in includedProperties )
            {
                if ( ignoredProperties.Contains( includedProperty ) )
                {
                    continue;
                }

                var newAccessor = new LavaTypeMemberAccessor( includedProperty );

                Register( type, new List<KeyValuePair<string, IMemberAccessor>> { new KeyValuePair<string, IMemberAccessor>( includedProperty.Name, newAccessor ) } );
            }
        }

        public override void Register( Type type, IEnumerable<KeyValuePair<string, IMemberAccessor>> accessors )
        {
            if ( !_map.TryGetValue( type, out var typeMap ) )
            {
                typeMap = new Dictionary<string, IMemberAccessor>( IgnoreCasing
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal );

                _map[type] = typeMap;
            }

            foreach ( var kvp in accessors )
            {
                typeMap[kvp.Key] = kvp.Value;
            }
        }
    }

    #region Support classes

    /// <summary>
    /// A Fluid Engine Member Accessor that can retrieve the value of a LavaDataSource.
    /// </summary>
    public class LavaDataSourceMemberAccessor : IMemberAccessor
    {
        public object Get( object obj, string name, TemplateContext ctx )
        {
            ILavaDataDictionary lavaObject;

            if ( obj is Rock.Lava.ILavaDataDictionarySource lavaSource )
            {
                lavaObject = lavaSource.GetLavaDataDictionary();
            }
            else
            {
                lavaObject = (ILavaDataDictionary)obj;
            }

            var value = lavaObject.GetValue( name );

            return value;
        }
    }

    /// <summary>
    /// A Fluid Engine Member Accessor that can retrieve the value of a member of an anonymously-typed object.
    /// </summary>
    public class DynamicMemberAccessor : IMemberAccessor
    {
        public object Get( object obj, string name, TemplateContext ctx )
        {
            return GetPropertyPathValue( obj, name );
        }

        private object GetPropertyPathValue( object obj, string propertyPath )
        {
            if ( string.IsNullOrWhiteSpace( propertyPath ) )
            {
                return obj;
            }

            var value = obj.GetPropertyValue( propertyPath );

            return value;
        }
    }

    /// <summary>
    /// A Fluid Engine Member Accessor that reads a specific property value of a class decorated with the LavaType attribute.
    /// </summary>
    public class LavaTypeMemberAccessor : IMemberAccessor
    {
        private PropertyInfo _info;

        public LavaTypeMemberAccessor( PropertyInfo info )
        {
            _info = info;
        }

        public object Get( object obj, string name, TemplateContext ctx )
        {
            var value = _info.GetValue( obj );

            return value;
        }
    }

    #endregion
}
