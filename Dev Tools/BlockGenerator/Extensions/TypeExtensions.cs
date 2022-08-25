using System;
using System.Linq;

using BlockGenerator.Utility;

namespace BlockGenerator
{
    public static class TypeExtensions
    {
        public static string GetFriendlyName( this Type type )
        {
            if ( type.IsGenericType )
            {
                var namePrefix = type.Name.Split( new[] { '`' }, StringSplitOptions.RemoveEmptyEntries )[0];
                var genericParameters = type.GetGenericArguments().Select( GetFriendlyName );

                return $"{namePrefix}<{string.Join( ", ", genericParameters)}>";
            }

            return type.Name;
        }

        public static PropertyDeclaration GetCSharpPropertyDeclaration( this Type type )
        {
            if ( type == typeof( bool ) )
            {
                return new PropertyDeclaration( "bool" );
            }
            else if ( type == typeof( bool? ) )
            {
                return new PropertyDeclaration( "bool?" );
            }
            else if ( type == typeof( int ) )
            {
                return new PropertyDeclaration( "int" );
            }
            else if ( type == typeof( int? ) )
            {
                return new PropertyDeclaration( "int?" );
            }
            else if ( type == typeof( string ) )
            {
                return new PropertyDeclaration( "string" );
            }
            else if ( type == typeof( Guid ) )
            {
                return new PropertyDeclaration( "Guid", new[] { "System" } );
            }
            else if ( type == typeof( Guid? ) )
            {
                return new PropertyDeclaration( "Guid?", new[] { "System" } );
            }
            else if ( type == typeof( DateTime ) )
            {
                return new PropertyDeclaration( "DateTime", new[] { "System" } );
            }
            else if ( type == typeof( DateTime? ) )
            {
                return new PropertyDeclaration( "DateTime?", new[] { "System" } );
            }
            else
            {
                throw new Exception( $"Unable to convert {type.GetFriendlyName()} to CSharp declaration." );
            }
        }
    }
}
