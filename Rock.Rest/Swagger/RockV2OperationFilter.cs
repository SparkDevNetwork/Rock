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
using System.Linq;
using System.Web.Http.Description;

using Swashbuckle.Swagger;

namespace Rock.Rest.Swagger
{
    /// <summary>
    /// Updates operation data for the V2 endpoints.
    /// </summary>
    internal class RockV2OperationFilter : IOperationFilter
    {
        /// <summary>
        /// Applies the specified operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="schemaRegistry">The schema registry.</param>
        /// <param name="apiDescription">The API description.</param>
        public void Apply( Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription )
        {
            // Limit swagger docs to only json since we don't officially support
            // XML anyway.
            operation.consumes = operation.consumes?.Where( a => a == "application/json" ).ToList();
            operation.produces = operation.produces?.Where( a => a == "application/json" ).ToList();
            operation.operationId = apiDescription.ID.Replace( '/', '_' ).RemoveSpecialCharacters();

            // If the operation has both a 200-OK and another 2xx response code
            // and the 200-OK looks like the default empty response type, then
            // remove the 200-OK.
            if ( operation.responses.ContainsKey( "201" ) || operation.responses.ContainsKey( "204" ) )
            {
                if ( operation.responses.ContainsKey( "200" ) && operation.responses["200"].schema.type == "object" )
                {
                    operation.responses.Remove( "200" );
                }
            }
        }
    }
}
