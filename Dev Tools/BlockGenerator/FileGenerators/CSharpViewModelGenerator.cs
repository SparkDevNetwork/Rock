using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BlockGenerator.Utility;

namespace BlockGenerator.FileGenerators
{
    public class CSharpViewModelGenerator : Generator
    {
        public string GenerateOptionsBag( string bagName, string bagNamespace )
        {
            var sb = new StringBuilder();
            sb.AppendLine( $"    public class {bagName}" );
            sb.AppendLine( "    {" );
            sb.AppendLine( "    }" );

            return GenerateCSharpFile( new string[0], bagNamespace, sb.ToString(), false );
        }

        public string GenerateEntityBag( string entityName, string bagNamespace, List<EntityProperty> properties )
        {
            var usings = new List<string>
            {
                "Rock.ViewModels.NonEntities"
            };

            var sb = new StringBuilder();
            sb.AppendLine( $"    public class {entityName}Bag : ViewModelBase" );
            sb.AppendLine( "    {" );

            var sortedProperties = properties.OrderBy( p => p.Name ).ToList();

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

        private static PropertyDeclaration GetCSharpPropertyTypeDeclaration( Type type )
        {
            var entityType = typeof( Rock.Data.IEntity );

            if ( type.IsGenericType && type.GenericTypeArguments.Length == 1 )
            {
                var genericArg = type.GenericTypeArguments[0];
                var collectionType = typeof( ICollection<> ).MakeGenericType( genericArg );

                if ( collectionType.IsAssignableFrom( type ) )
                {
                    return new PropertyDeclaration( $"List<ListItemBag>", new[] { "System.Collections.Generic", "Rock.ViewModels.Utility" } );
                }
            }

            if ( entityType.IsAssignableFrom( type ) )
            {
                return new PropertyDeclaration( $"ListItemBag", new[] { "Rock.ViewModels.Utility" } );
            }

            return type.GetCSharpPropertyDeclaration();
        }
    }
}
