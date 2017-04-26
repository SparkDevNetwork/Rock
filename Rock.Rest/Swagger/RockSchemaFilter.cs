using System;
using System.Web;
using Swashbuckle.Swagger;

namespace Rock.Rest.Swagger
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Swashbuckle.Swagger.ISchemaFilter" />
    public class RockSchemaFilter : ISchemaFilter
    {
        /// <summary>
        /// Applies the specified schema.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="schemaRegistry">The schema registry.</param>
        /// <param name="type">The type.</param>
        public void Apply( Schema schema, SchemaRegistry schemaRegistry, Type type )
        {
            var requestParams = HttpContext.Current?.Request?.Params;
            if ( requestParams != null )
            {
                var controllerName = requestParams["controllerName"];
            }
        }
    }
}