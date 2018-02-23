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
using System.Net;
using System.Web.Http;

namespace Rock.Security.Authentication.Auth0.Rest.Controllers
{
    /// NOTE: WebApi doesn't support Controllers with the Same Name, even if they have different namespaces, so can't call this AuthController
    public class Auth0Controller : ApiController
    {
        /// <summary>
        /// Use this to Login a user and return an AuthCookie which can be used in subsequent REST calls
        /// </summary>
        /// <param name="auth0UserInfo">The auth0 user information.</param>
        /// <exception cref="HttpResponseException"></exception>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [HttpPost]
        [System.Web.Http.Route( "api/Auth/Auth0" )]
        public void Auth0Login( [FromBody]Rock.Security.Authentication.Auth0.Auth0UserInfo auth0UserInfo )
        {
            string userName = Rock.Security.Authentication.Auth0.Auth0Authentication.GetAuth0UserName( auth0UserInfo );
            if ( !string.IsNullOrWhiteSpace( userName ) )
            {
                Rock.Security.Authorization.SetAuthCookie( userName, false, false );
            }
            else
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }
        }
    }
}
