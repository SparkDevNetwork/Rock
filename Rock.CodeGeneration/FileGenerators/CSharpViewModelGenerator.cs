using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rock.CodeGeneration.Utility;

namespace Rock.CodeGeneration.FileGenerators
{
    /// <summary>
    /// Provides methods for generating specific C# files.
    /// </summary>
    public class CSharpViewModelGenerator : Generator
    {
        #region Methods

        /// <summary>
        /// Generates an empty options bag.
        /// </summary>
        /// <param name="bagName">Name of the bag.</param>
        /// <param name="bagNamespace">The namespace the bag will be placed in.</param>
        /// <returns>A string that contains the contents of the file.</returns>
        public string GenerateOptionsBag( string bagName, string bagNamespace )
        {
            var sb = new StringBuilder();
            sb.AppendLine( $"    public class {bagName}" );
            sb.AppendLine( "    {" );
            sb.AppendLine( "    }" );

            return GenerateCSharpFile( new string[0], bagNamespace, sb.ToString(), false );
        }

        /// <summary>
        /// Generates the entity bag from a set of properties.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="bagNamespace">The namespace the bag will be placed in.</param>
        /// <param name="properties">The properties that will be contained in the bag.</param>
        /// <returns>A string that contains the contents of the file.</returns>
        public string GenerateEntityBag( string entityName, string bagNamespace, List<EntityProperty> properties )
        {
            var usings = new List<string>
            {
                "Rock.ViewModels.Utility"
            };

            var sb = new StringBuilder();
            sb.AppendLine( $"    public class {entityName}Bag : EntityBagBase" );
            sb.AppendLine( "    {" );

            var sortedProperties = properties.OrderBy( p => p.Name ).ToList();

            // Loop through the sorted list of properties and emit each one.
            for ( int i = 0; i < sortedProperties.Count; i++ )
            {
                var property = sortedProperties[i];

                if ( i > 0 )
                {
                    sb.AppendLine();
                }

                var declaration = GetCSharpPropertyTypeDeclaration( property.PropertyType );

                usings.AddRange( declaration.RequiredUsings );
                sb.AppendLine( $"        public {declaration.TypeName} {property.Name} {{ get; set; }}" );
            }

            sb.AppendLine( "    }" );

            return GenerateCSharpFile( usings, bagNamespace, sb.ToString(), false );
        }

        /// <summary>
        /// Gets the C# property type declaration.
        /// </summary>
        /// <param name="type">The type that will need to be declared.</param>
        /// <returns>a <see cref="PropertyDeclaration"/> that represents the property.</returns>
        private static PropertyDeclaration GetCSharpPropertyTypeDeclaration( Type type )
        {
            var entityType = typeof( Data.IEntity );

            // If the type is a collection of entities then use a collection
            // of ListItemBag objects.
            if ( type.IsGenericType && type.GenericTypeArguments.Length == 1 )
            {
                var genericArg = type.GenericTypeArguments[0];
                var collectionType = typeof( ICollection<> ).MakeGenericType( genericArg );

                if ( collectionType.IsAssignableFrom( type ) )
                {
                    return new PropertyDeclaration( $"List<ListItemBag>", new[] { "System.Collections.Generic", "Rock.ViewModels.Utility" } );
                }
            }

            // If the type is an entity then use a ListItemBag object.
            if ( entityType.IsAssignableFrom( type ) )
            {
                return new PropertyDeclaration( $"ListItemBag", new[] { "Rock.ViewModels.Utility" } );
            }

            // Try for a primitive property type.
            return type.GetCSharpPropertyDeclaration();
        }

        #endregion
    }
}
