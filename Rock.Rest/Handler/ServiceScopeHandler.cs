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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Rest.Utility;

namespace Rock.Rest.Handler
{
    /// <summary>
    /// A handler for the ASP.Net pipeline that will put the entire API request
    /// in scoped section of the service provider.
    /// </summary>
    class ServiceScopeHandler : DelegatingHandler
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceScopeHandler"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider to use as the root for all requests.</param>
        public ServiceScopeHandler( IServiceProvider serviceProvider )
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
        {
            var scope = _serviceProvider.CreateScope();

            try
            {
                var accessor = scope.ServiceProvider.GetRequiredService<IRockRequestContextAccessor>();
                var rockContext = scope.ServiceProvider.GetRequiredService<RockContext>();

                request.Properties["RockServiceProvider"] = scope.ServiceProvider;

                // The RockRequestContext is created in Global.asax.cs
                // Application_BeginRequest and attached to the accessor
                // before any handler runs. Missing it here means the
                // BeginRequest hook did not run for this request, which
                // ASP.NET guarantees should not happen.
                var rockRequestContext = accessor.RockRequestContext
                    ?? throw new InvalidOperationException( "Request information was incomplete: no RockRequestContext was attached during Application_BeginRequest." );

                rockRequestContext.CurrentUser = UserLoginService.GetCurrentUser( false, rockContext );

                if ( rockRequestContext.IsClientForbidden() )
                {
                    return request.CreateResponse( HttpStatusCode.Forbidden );
                }

                var responseMessage = await base.SendAsync( request, cancellationToken );

                // If we are using a PushStreamContent, we need to wrap it so
                // we can dispose of the scope after the stream is finished.
                // This will mimic the ASP.Net Core behavior of disposing the
                // scope after the response is sent. Otherwise we get errors
                // because the DbContext is disposed and navigation properties
                // can fail to work.
                if ( responseMessage.Content is PushStreamContent pushStreamContent )
                {
                    var originalContent = responseMessage.Content;

                    responseMessage.Content = new ScopedPushStreamContent( scope, pushStreamContent );
                }
                else
                {
                    // No content? Dispose immediately.
                    scope.Dispose();
                }

                return responseMessage;
            }
            catch
            {
                scope.Dispose();
                throw;
            }
            finally
            {
                request.Properties.Remove( "RockServiceProvider" );
            }
        }
    }
}
