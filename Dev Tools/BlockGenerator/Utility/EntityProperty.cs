using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using BlockGenerator.Lava;

namespace BlockGenerator.Utility
{
    public class EntityProperty : LavaDynamic
    {
        private static readonly Type[] _assignmentTypes = new[]
        {
            typeof( bool ),
            typeof( bool? ),
            typeof( int ),
            typeof( int? ),
            typeof( string ),
            typeof( Guid ),
            typeof( Guid? ),
            typeof( DateTime ),
            typeof( DateTime? )
        };

        public PropertyInfo PropertyInfo { get; }

        public Type PropertyType => PropertyInfo.PropertyType;

        public string Name => PropertyInfo.Name;

        public string ConvertToBagCode => GetConvertToBagCode( false );

        public string ConvertFromBagCode => GetConvertFromBagCode( false );

        public EntityProperty( PropertyInfo propertyInfo )
        {
            PropertyInfo = propertyInfo;
        }

        public string GetConvertToBagCode( bool throwOnError = true )
        {
            if ( _assignmentTypes.Contains( PropertyType ) )
            {
                return Name;
            }

            var entityType = typeof( Rock.Data.IEntity );

            if ( entityType.IsAssignableFrom( PropertyType ) )
            {
                return $"{Name}.ToListItemBag()";
            }

            if ( PropertyType.IsGenericType && PropertyType.GenericTypeArguments.Length == 1 )
            {
                var genericArg = PropertyType.GenericTypeArguments[0];
                var collectionType = typeof( ICollection<> ).MakeGenericType( genericArg );

                if ( collectionType.IsAssignableFrom( PropertyType ) )
                {
                    return $"{Name}.ToListItemBagList()";
                }
            }

            return throwOnError
                ? throw new Exception( $"Unknown property type '{PropertyType.GetFriendlyName()}' for conversion to bag." )
                : $"/* TODO: Unknown property type '{PropertyType.GetFriendlyName()}' for conversion to bag. */";
        }

        public string GetConvertFromBagCode( bool throwOnError = true )
        {
            if ( _assignmentTypes.Contains( PropertyType ) )
            {
                return Name;
            }

            var entityType = typeof( Rock.Data.IEntity );

            if ( entityType.IsAssignableFrom( PropertyType ) )
            {
                return $"{Name}.GetEntityId<{PropertyType.GetFriendlyName()}>( rockContext )";
            }

            return throwOnError
                ? throw new Exception( $"Unknown property type '{PropertyType.GetFriendlyName()}' for conversion to bag." )
                : $"/* TODO: Unknown property type '{PropertyType.GetFriendlyName()}' for conversion to bag. */";
        }

        public static bool IsSupportedPropertyType( Type type )
        {
            var entityType = typeof( Rock.Data.IEntity );

            if ( type.IsGenericType && type.GenericTypeArguments.Length == 1 )
            {
                var genericArg = type.GenericTypeArguments[0];
                var collectionType = typeof( ICollection<> ).MakeGenericType( genericArg );

                if ( collectionType.IsAssignableFrom( type ) )
                {
                    return true;
                }
            }

            if ( entityType.IsAssignableFrom( type ) )
            {
                return true;
            }
            else if ( type == typeof( bool ) || type == typeof( bool? ) )
            {
                return true;
            }
            else if ( type == typeof( int ) || type == typeof( int? ) )
            {
                return true;
            }
            else if ( type == typeof( Guid ) || type == typeof( Guid? ) )
            {
                return true;
            }
            else if ( type == typeof( string ) )
            {
                return true;
            }
            else if ( type == typeof( DateTime ) || type == typeof( DateTime? ) )
            {
                return true;
            }

            return false;
        }
    }
}
