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
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;

using Rock.Model;
using Rock.Utility;

namespace Rock.Rest.Swagger
{
    /// <summary>
    /// Late startup process to begin building the v2 API documentation cache.
    /// </summary>
    internal class SwaggerV2CacheBuilder : IRockStartup
    {
        /// <inheritdoc/>
        public int StartupOrder => 1000;

        /// <inheritdoc/>
        public void OnStartup()
        {
            Task.Run( async () =>
            {
                // Execute a fake HTTP request to the API documentation handler.
                // We do it this way because trying to hit the server directly
                // via configured URL would only hit one server in a farm and we
                // need to initialize all of them. And trying to send a real
                // request to localhost may not work because of potential
                // firewall issues or even non-standard port configurations.
                try
                {
                    // Wait 1 minute for Rock to finish most of it's startup
                    // tasks like compiling blocks. We don't want to chew up
                    // CPU time on a low-core system that causes Rock to take
                    // longer to finish starting up.
                    await Task.Delay( 60_000 );

                    // Find the configured route, request handler and then
                    // the handler's SendAsync method.
                    var config = GlobalConfiguration.Configuration;
                    var route = config.Routes["swagger_docs" + "api/{apiVersion}/doc"];
                    var handler = route.Handler;
                    var method = handler.GetType().GetMethod( "SendAsync", BindingFlags.Instance | BindingFlags.NonPublic );

                    // Build the fake request to pass to the handler.
                    var fakeRequest = new HttpRequestMessage( HttpMethod.Get, "http://localhost/api/v2/doc" );
                    var fakeRouteData = new HttpRouteData( route );

                    fakeRouteData.Values.Add( "apiVersion", "v2" );
                    fakeRequest.SetConfiguration( config );
                    fakeRequest.SetRouteData( fakeRouteData );

                    // Send the request. We don't actually need to do anything
                    // with the response since it will now be cached.
                    var responseTask = ( Task<HttpResponseMessage> ) method.Invoke( handler, new object[] { fakeRequest, CancellationToken.None } );
                    await responseTask;
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( new Exception( "Failed to initialize the API documentation during startup.", ex ) );
                }
            } );
        }
    }
}
