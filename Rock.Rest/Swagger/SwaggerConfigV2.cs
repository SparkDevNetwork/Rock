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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;

using Swashbuckle.Application;

namespace Rock.Rest.Swagger
{
    /// <summary>
    /// Configure the auto-documentation system for the v2 API.
    /// </summary>
    internal class SwaggerConfigV2
    {
        /// <summary>
        /// Registers the specified configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public static void Register( HttpConfiguration config )
        {
            var thisAssembly = typeof( SwaggerConfig ).Assembly;

            var swaggerRoute = "api/{apiVersion}/doc";
            var swaggerUiRoute = "api/v2/docs/{*assetPath}";

            config
                .EnableSwagger( swaggerRoute, c =>
                {
                    // Resolve the root URL in a way that is safe with proxies.
                    c.RootUrl( req => DefaultRootUrlResolver( req ) );

                    // Configure our API version information.
                    c.MultipleApiVersions(
                        ( apiDesc, targetApiVersion ) => apiDesc.RelativePath.StartsWith( $"api/v2/" ),
                        ( vc ) =>
                        {
                            vc.Version( "v2", "Rock Rest API v2" );
                        } );

                    // Ignore obsolete items.
                    c.IgnoreObsoleteActions();
                    c.IgnoreObsoleteProperties();

                    // Include all the XML documentation for the API endpoints.
                    foreach ( var xmlDocPath in GetXmlDocsFiles() )
                    {
                        try
                        {
                            // load into an xmlPathDoc to ensure it is a valid xml doc
                            var xmlPathDoc = new System.Xml.XPath.XPathDocument( xmlDocPath );
                            c.IncludeXmlComments( xmlDocPath );
                        }
                        catch
                        {
                            // ignore bad xml doc
                        }
                    }

                    // Use the full type name (including namespace) since we have
                    // types with the same name in different namespaces.
                    c.UseFullTypeNameInSchemaIds();

                    // Apply custom filter rules to how operations and
                    // documents are generated.
                    c.OperationFilter<RockV2OperationFilter>();
                    c.DocumentFilter<RockV2DocumentFilter>();
                    c.SchemaFilter<RockV2SchemaFilter>();

                    // In contrast to WebApi, Swagger 2.0 does not include the query string component when mapping a URL
                    // to an action. As a result, Swashbuckle will raise an exception if it encounters multiple actions
                    // with the same path (sans query string) and HTTP method. You can workaround this by providing a
                    // custom strategy to pick a winner or merge the descriptions for the purposes of the Swagger docs 
                    //
                    c.ResolveConflictingActions( apiDescriptions =>
                    {
                        // If there are multiple, show the one the shortest "RelativePath", which would hopefully be the most common one they would need
                        return apiDescriptions.OrderBy( a => a.RelativePath.Length ).First();
                    } );
                } )
                .EnableSwaggerUi( swaggerUiRoute, c =>
                {
                    // Disable validation against the remote swagger server.
                    c.DisableValidator();
                } );

            // Since we are using a custom route for the swagger ui, manually set up the shortcut
            config.Routes.MapHttpRoute(
                name: "swagger_ui_v2_shortcut",
                routeTemplate: "api/v2/docs",
                defaults: null,
                constraints: new { uriResolution = new HttpRouteDirectionConstraint( HttpRouteDirection.UriResolution ) },
                handler: new RedirectHandler( SwaggerDocsConfig.DefaultRootUrlResolver, "api/v2/docs/index" ) );
        }

        /// <summary>
        /// Gets the paths and filenames of the XML documentation files related
        /// to any API controllers.
        /// </summary>
        /// <returns>A list of paths as strings.</returns>
        private static List<string> GetXmlDocsFiles()
        {
            var apiControllerTypes = Rock.Reflection.FindTypes( typeof( ApiController ) );
            var rockWebBinPath = Path.Combine( System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, "bin" );
            var restAssemblyNames = apiControllerTypes
                .Select( a =>
                {
                    if ( a.Value.Assembly?.ManifestModule?.Name == "<Unknown>" )
                    {
                        return a.Value.Assembly?.ManifestModule?.ScopeName;
                    }
                    else
                    {
                        return a.Value.Assembly?.ManifestModule?.Name;
                    }

                } )
                .Distinct()
                .Where( a => a != null )
                .ToList();

            try
            {
                return restAssemblyNames
                    .Select( a => Path.Combine( rockWebBinPath, Path.ChangeExtension( a, "xml" ) ) )
                    .Where( a => File.Exists( a ) )
                    .ToList();
            }
            catch ( Exception ex )
            {
                Rock.Model.ExceptionLogService.LogException( new Exception( "Error loading XML Docs for REST Api plugins", ex ) );
                return new List<string>();
            }
        }

        /// <summary>
        /// Gets the root URL of the request message.
        /// </summary>
        /// <param name="request">The request message.</param>
        /// <returns>The root URL.</returns>
        private static string DefaultRootUrlResolver( HttpRequestMessage request )
        {
            var uri = new HttpRequestMessageWrapper( request ).UrlProxySafe();

            var url = $"{uri.Scheme}://{uri.Host}";

            if ( !uri.IsDefaultPort )
            {
                url += $":{uri.Port}";
            }

            return url;
        }
    }
}
