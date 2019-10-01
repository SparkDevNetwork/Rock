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
    /// 
    /// </summary>
    /// <seealso cref="Swashbuckle.Swagger.IDocumentFilter" />
    public class RockDocumentFilter : IDocumentFilter
    {
        /// <summary>
        /// Applies the specified swagger document.
        /// </summary>
        /// <param name="swaggerDoc">The swagger document.</param>
        /// <param name="schemaRegistry">The schema registry.</param>
        /// <param name="apiExplorer">The API explorer.</param>
        public void Apply( SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer )
        {
            // sort the definitions and paths
            swaggerDoc.definitions = swaggerDoc.definitions.OrderBy( a => a.Key ).ToDictionary( x => x.Key, x => x.Value );
            swaggerDoc.paths = swaggerDoc.paths.OrderBy( a => a.Key ).ToDictionary( x => x.Key, x => x.Value );
        }
    }
}
