using System;
using System.Xml.XPath;

using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace RockWebCore
{
    /// <summary>
    /// Amends the description of enums to include documentation on the
    /// individual values in the enumeration.
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter" />
    public class EnumTypesSchemaFilter : ISchemaFilter
    {
        private readonly XPathNavigator _xmlNavigator;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumTypesSchemaFilter"/> class.
        /// </summary>
        /// <param name="xmlDocument">The XML document.</param>
        public EnumTypesSchemaFilter( XPathDocument xmlDocument )
        {
            _xmlNavigator = xmlDocument.CreateNavigator();
        }

        /// <inheritdoc/>
        public void Apply( OpenApiSchema schema, SchemaFilterContext context )
        {
            if ( ( schema?.Enum?.Count ?? 0 ) == 0 || context.Type == null || !context.Type.IsEnum )
            {
                return;
            }

            var fullTypeName = context.Type.FullName;
            if ( _xmlNavigator.SelectSingleNode( $"/doc/members/member[@name='T:{fullTypeName}']" ) == null )
            {
                return;
            }

            schema.Description += "<p>Members:</p><ul>";

            var enumValues = Enum.GetValues( context.Type );

            foreach ( int enumValue in enumValues )
            {
                var enumName = Enum.GetName( context.Type, enumValue );
                var fullEnumMemberName = $"F:{fullTypeName}.{enumName}";

                var summary = _xmlNavigator.SelectSingleNode( $"/doc/members/member[@name='{fullEnumMemberName}']/summary" );

                if ( summary == null )
                {
                    schema.Description += $"<li><i>{enumValue}</i> - {enumName}</li>";
                }
                else
                {
                    schema.Description += $"<li><i>{enumValue}</i> - {enumName} - {summary.Value.Trim()}</li>";
                }
            }

            schema.Description += "</ul>";
        }
    }
}
