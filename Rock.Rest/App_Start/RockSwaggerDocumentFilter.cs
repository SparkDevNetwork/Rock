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
using Swashbuckle.Swagger;

namespace Rock.Rest
{
    public class RockSwaggerDocumentFilter : IDocumentFilter
    {
        // The default doc is huge and very slow, so this attempts to narrow down what is included
        public void Apply( SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer )
        {
            // to help make the doc smaller, remove paths for the following actions
            string[] actionsToHide = new string[]
            {
                "/DataView/",
                "/LaunchWorkflow/",
                "/DeleteAttributeValue/",
                "/AttributeValue/",
                "/SetAttributeValue/",
                "/SetContext/",
            };

            Dictionary<string, PathItem> pathsToKeep = new Dictionary<string, PathItem>();

            foreach ( var path in swaggerDoc.paths )
            {
                if ( !actionsToHide.Any( a => path.Key.Contains( a ) ) )
                {
                    pathsToKeep.Add( path.Key, path.Value );
                }
            }

            string[] namespacesToHide = new string[]
            {
                "System.Web.Http.OData",
                "Microsoft.Data.Edm",
                "Microsoft.Data.OData"
            };

            swaggerDoc.paths = pathsToKeep.OrderBy( a => a.Key ).ToDictionary( x => x.Key, x => x.Value );

            Dictionary<string, Schema> definitionsToKeep = new Dictionary<string, Schema>();
            foreach ( var definition in schemaRegistry.Definitions )
            {
                if ( !namespacesToHide.Any( a => definition.Key.Contains( a ) ) )
                {
                    definitionsToKeep.Add( definition.Key, definition.Value );
                }

                // to help make the doc smaller, don't include references to other models in the properties of an item
                definition.Value.properties = definition.Value.properties.Where( a => string.IsNullOrEmpty( a.Value.@ref ) ).ToDictionary( k => k.Key, v => v.Value );
            }

            swaggerDoc.definitions = definitionsToKeep.OrderBy( a => a.Key ).ToDictionary( x => x.Key, x => x.Value );
        }
    }
}
