﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

namespace Rock.Rest
{
    public class EnableCorsFromOriginAttribute : System.Attribute, ICorsPolicyProvider
    {
        public async Task<CorsPolicy> GetCorsPolicyAsync( HttpRequestMessage request, CancellationToken cancellationToken )
        {
            var requestInfo = request.GetCorsRequestContext();
            var origin = requestInfo.Origin;

            // Check if request is from an authorized origin
            if ( await IsOriginValid(origin))
            {
                // Valid request
                var policy = new CorsPolicy { AllowAnyHeader = true, AllowAnyMethod = true };
                policy.Origins.Add( origin );
                return policy;
            }

            // Invalid Request
            return null;
        }

        private async Task<bool> IsOriginValid( string origin )
        {
            bool result = false;

            var definedType = Rock.Web.Cache.DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.REST_API_ALLOWED_DOMAINS.AsGuid() );
            if (definedType != null)
            {
                result = definedType.DefinedValues.Select( v => v.Value ).Contains( origin, StringComparer.OrdinalIgnoreCase );
            }

            return await Task.FromResult<bool>( result );
        }
    }
}
