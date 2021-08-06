using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using HotChocolate.Types.Descriptors;

using Rock.Data;
using Rock.Rest;

namespace RockWebCore
{
    /// <summary>
    /// Type inspector for Rock to ensure we are only making safe types
    /// available.
    /// </summary>
    /// <seealso cref="HotChocolate.Types.Descriptors.DefaultTypeInspector" />
    internal class RockTypeInspector : DefaultTypeInspector
    {
        /// <summary>
        /// Some models have data members with unsafe types, i.e. cache and
        /// other things that they probably shouldn't. This filters out any
        /// unknown types. Otherwise we end up generating an error about
        /// unable to create type of "" because it eventually hits some weird
        /// type that it can't deal with.
        /// </summary>
        /// <param name="type">The type to be checked.</param>
        /// <returns>
        ///   <c>true</c> if the type is safe; otherwise, <c>false</c>.
        /// </returns>
        private bool IsSafeType( Type type )
        {
            var nullableType = Nullable.GetUnderlyingType( type );

            if ( nullableType != null )
            {
                return IsSafeType( nullableType );
            }

            if ( type.IsGenericType )
            {
                var genericType = type.GetGenericTypeDefinition();
                var genericArgs = type.GetGenericArguments();

                if ( genericArgs.Length == 1 && genericType == typeof( ICollection<> ) )
                {
                    var collectionType = typeof( ICollection<> ).MakeGenericType( genericArgs[0] );

                    if ( collectionType.IsAssignableFrom( type ) )
                    {
                        return IsSafeType( genericArgs[0] );
                    }
                }
            }

            return type.IsPrimitive
                || type.IsEnum
                || type == typeof( string )
                || type == typeof( Guid )
                || type == typeof( DateTime )
                || typeof( IEntity ).IsAssignableFrom( type );
        }

        /// <summary>
        /// Gets the members that should be exposed on this type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="includeIgnored">if set to <c>true</c> include ignored.</param>
        /// <returns></returns>
        public override IEnumerable<MemberInfo> GetMembers( System.Type type, bool includeIgnored )
        {
            var members = base.GetMembers( type, includeIgnored );

            if ( type.GetCustomAttribute<GraphQueryRootAttribute>() != null )
            {
                return members;
            }

            if ( type.GetCustomAttribute<GraphQueryTypeAttribute>() != null )
            {
                return members.Where( a => a.GetCustomAttribute<GraphQueryExcludeAttribute>() == null );
            }

            var safeProperties = members.Where( a => a is PropertyInfo )
                .Cast<PropertyInfo>()
                .Where( p => p.GetCustomAttribute<DataMemberAttribute>() != null )
                .Where( p => IsSafeType( p.PropertyType ) );

            var includedMembers = members.Where( a => a.GetCustomAttribute<GraphQueryIncludeAttribute>() != null );

            return includedMembers.Union( safeProperties );
        }
    }
}
