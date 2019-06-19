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
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;

using Rock.Attribute;

using Swashbuckle.Swagger;

namespace Rock.Rest.Swagger
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Swashbuckle.Swagger.IOperationFilter" />
    public class RockOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Applies the specified operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="schemaRegistry">The schema registry.</param>
        /// <param name="apiDescription">The API description.</param>
        public void Apply( Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription )
        {
            // limit swagger docs to only json to help speed up the Swagger UI (supporting XML is slower since the Swagger UI javascript has to create an example in XML format for all the models)
            operation.consumes = operation.consumes?.Where( a => a == "application/json" ).ToList();
            operation.produces = operation.produces?.Where( a => a == "application/json" ).ToList();
            operation.operationId = apiDescription.ID.Replace( '/', '_' ).RemoveSpecialCharacters();

            if ( apiDescription.HttpMethod.Method == "GET" )
            {
                operation.parameters = operation.parameters ?? new List<Parameter>();
                var responseType = apiDescription.ResponseDescription?.DeclaredType;

                // Add OData helpers if this is an IQueryable GET
                if ( responseType != null )
                {
                    bool supportsLoadAttributes = false;
                    if ( responseType.GetInterfaces().Contains( typeof( IQueryable ) ) )
                    {
                        // add the odata parameters
                        operation.parameters = operation.parameters
                            .Parameter( "$expand", "query", "Expands related entities inline.", "string", false )
                            .Parameter( "$filter", "query", "Filters the results, based on a Boolean condition.", "string", false )
                            .Parameter( "$select", "query", "Selects which properties to include in the response.", "string", false )
                            .Parameter( "$orderby", "query", "Sorts the results.", "string", false )
                            .Parameter( "$top", "query", "Returns only the first n results.", "integer", false, "int32" )
                            .Parameter( "$skip", "query", "Skips the first n results.", "integer", false, "int32" );

                        var queryableArgumentType = responseType.GenericTypeArguments.FirstOrDefault();
                        if ( queryableArgumentType != null && queryableArgumentType.GetInterfaces().Contains( typeof( IHasAttributes ) ) )
                        {
                            supportsLoadAttributes = true;
                        }
                    }

                    if ( responseType.GetInterfaces().Contains(typeof( IHasAttributes ) ) )
                    {
                        supportsLoadAttributes = true;
                    }

                    if ( supportsLoadAttributes )
                    {
                        // Add loadAttributes helper
                        operation.parameters.Parameter( "loadAttributes", "query", "Specify 'simple' or 'expanded' to load attributes", "string", false, null, new string[] { "simple", "expanded" } );
                    }
                }
            }

            if ( string.IsNullOrEmpty( operation.summary ) )
            {
                // Manually set the documentation summary if swashbuckle couldn't figure it out. Swashbuckle 5 isn't able to figure these out since they have Generic parameters
                if ( apiDescription.HttpMethod.Method == "POST" )
                {
                    operation.summary = "POST endpoint. Use this to add a record";
                }
                else if ( apiDescription.HttpMethod.Method == "PUT" )
                {
                    operation.summary = "PUT endpoint. Use this to update a record";
                }
            }
        }
    }
}
