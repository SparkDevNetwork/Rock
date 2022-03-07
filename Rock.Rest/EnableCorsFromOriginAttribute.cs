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
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http.Cors;

#if REVIEW_NET5_0_OR_GREATER
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
#endif

using Rock.Web.Cache;

namespace Rock.Rest
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Attribute" />
    /// <seealso cref="System.Web.Http.Cors.ICorsPolicyProvider" />
    public class EnableCorsFromOriginAttribute : System.Attribute, ICorsPolicyProvider
    {
        /// <summary>
        /// Gets the <see cref="T:System.Web.Cors.CorsPolicy" />.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The <see cref="T:System.Web.Cors.CorsPolicy" />.
        /// </returns>
#if REVIEW_NET5_0_OR_GREATER
        public async Task<CorsPolicy> GetPolicyAsync( HttpContext context, string policyName )
#else
        public async Task<CorsPolicy> GetCorsPolicyAsync( HttpRequestMessage request, CancellationToken cancellationToken )
#endif
        {
#if REVIEW_NET5_0_OR_GREATER
            var origin = context.Request.Headers[CorsConstants.Origin].ToString();
#else
            var requestInfo = request.GetCorsRequestContext();
            var origin = requestInfo.Origin;
#endif

            // Check if request is from an authorized origin
            if ( await IsOriginValid(origin))
            {
                // Valid request
#if REVIEW_NET5_0_OR_GREATER
                var policy = new CorsPolicyBuilder()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .Build();
#else
                var policy = new CorsPolicy { AllowAnyHeader = true, AllowAnyMethod = true, SupportsCredentials = true };
#endif

                /*
                  9/27/2018 - JME 
                  If the SupportsCredentials property is true, the HTTP response will include an
                  Access-Control-Allow-Credentials header. This header tells the browser that the
                  server allows credentials for a cross-origin request.

                  If the browser sends credentials, but the response does not include
                  a valid Access-Control-Allow-Credentials header, the browser will not expose the
                  response to the application, and the AJAX request fails.

                  In the future we could move this to be a configuration on the CORS defined value
                  to enable/disable the header for each domain, but at this time it doesn't see
                  to be justified.

                  Reason: Rocket Chat Integration
                */

                policy.Origins.Add( origin );
                return policy;
            }

            // Invalid Request
            return null;
        }

        private async Task<bool> IsOriginValid( string origin )
        {
            bool result = false;

            var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.REST_API_ALLOWED_DOMAINS.AsGuid() );
            if (definedType != null)
            {
                result = definedType.DefinedValues.Select( v => v.Value ).Contains( origin, StringComparer.OrdinalIgnoreCase );
            }

            return await Task.FromResult<bool>( result );
        }
    }
}
