using System;
using System.Linq;
using System.Reflection;

using Rock.CodeGeneration.Utility;

namespace Rock.CodeGeneration
{
    /// <summary>
    /// Extensions to <see cref="Type"/>.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets a friendly name for the type. This turns generic type names into
        /// something closer to C# syntax.
        /// </summary>
        /// <param name="type">The type whose name should be fetched.</param>
        /// <returns>A string that represents the friendly name.</returns>
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

        /// <summary>
        /// Gets the C# property declaration for the given type.
        /// </summary>
        /// <param name="type">The type that needs to be declared.</param>
        /// <returns>A <see cref="PropertyDeclaration"/> instance that represents the declaration.</returns>
        /// <exception cref="System.Exception">Unable to convert {type.GetFriendlyName()} to CSharp declaration.</exception>
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
            else if ( type == typeof( long ) )
            {
                return new PropertyDeclaration( "long" );
            }
            else if ( type == typeof( long? ) )
            {
                return new PropertyDeclaration( "long?" );
            }
            else if ( type == typeof( decimal ) )
            {
                return new PropertyDeclaration( "decimal" );
            }
            else if ( type == typeof( decimal? ) )
            {
                return new PropertyDeclaration( "decimal?" );
            }
            else if ( type == typeof( double ) )
            {
                return new PropertyDeclaration( "double" );
            }
            else if ( type == typeof( double? ) )
            {
                return new PropertyDeclaration( "double?" );
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
            else if ( type == typeof( DateTimeOffset ) )
            {
                return new PropertyDeclaration( "DateTimeOffset", new[] { "System" } );
            }
            else if ( type == typeof( DateTimeOffset ) )
            {
                return new PropertyDeclaration( "DateTimeOffset?", new[] { "System" } );
            }
            else if ( type.IsEnum && type.Namespace.StartsWith( "Rock.Enums" ) )
            {
                return new PropertyDeclaration( type.Name, new[] { type.Namespace } );
            }
            else if ( type.IsEnum && type.Namespace.StartsWith( "Rock.Model") && type.GetCustomAttributes().FirstOrDefault( a => a.GetType().FullName == "Rock.Enums.EnumDomainAttribute" ) != null )
            {
                return new PropertyDeclaration( type.Name, new[] { type.Namespace } );
            }
            else
            {
                throw new Exception( $"Unable to convert {type.GetFriendlyName()} to CSharp declaration." );
            }
        }
    }
}
