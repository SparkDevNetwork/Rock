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
            using ( var scope = _serviceProvider.CreateScope() )
            {
                var accessor = scope.ServiceProvider.GetRequiredService<IRockRequestContextAccessor>();
                var rockContext = scope.ServiceProvider.GetRequiredService<RockContext>();

                try
                {
                    request.Properties["RockServiceProvider"] = scope.ServiceProvider;

                    var responseContext = new RockMessageResponseContext();
                    var wrapper = new HttpRequestMessageWrapper( request );
                    var user = UserLoginService.GetCurrentUser( false, rockContext );
                    var rockRequestContext = new RockRequestContext( wrapper, responseContext, user );

                    if ( accessor is RockRequestContextAccessor internalAccessor )
                    {
                        internalAccessor.RockRequestContext = rockRequestContext;
                    }

                    var responseMessage = await base.SendAsync( request, cancellationToken );

                    responseContext.Update( responseMessage );

                    return responseMessage;
                }
                finally
                {
                    request.Properties.Remove( "RockServiceProvider" );
                }
            }
        }
    }
}
