using System.Collections.Generic;
using Swashbuckle.Swagger;

namespace Rock.Rest.Swagger
{
    /// <summary>
    /// 
    /// </summary>
    public static class SwaggerExtensions
    {
        /// <summary>
        /// Adds a new parameter and returns a reference to parameter list
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="name">The name.</param>
        /// <param name="in">The in.</param>
        /// <param name="description">The description.</param>
        /// <param name="type">The type.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="format">The format.</param>
        /// <param name="enum">The enum.</param>
        /// <returns></returns>
        public static IList<Parameter> Parameter(this IList<Parameter> parameters, string name, string @in, string description, string @type, bool required, string format = null, object[] @enum = null )
        {
            parameters.Add( new Parameter
            {
                name = name,
                @type = @type,
                @in = @in,
                description = description,
                required = required,
                format = format,
                @enum = @enum
            } );

            return parameters;
        }
    }
}
