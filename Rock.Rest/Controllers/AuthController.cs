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

using Rock.Model;
using Rock.Security;

namespace Rock.Rest.Controllers
{
    /// <summary>
    ///
    /// </summary>
    public class AuthController : ApiController
    {
        /// <summary>
        /// Use this to Login a user and return an AuthCookie which can be used in subsequent REST calls
        /// </summary>
        /// <param name="loginParameters">The login parameters.</param>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [HttpPost]
        [System.Web.Http.Route( "api/Auth/Login" )]
        public void Login( [FromBody]LoginParameters loginParameters )
        {
            bool valid = false;

            var userLoginService = new UserLoginService( new Rock.Data.RockContext() );
            var userLogin = userLoginService.GetByUserName( loginParameters.Username );
            if ( userLogin != null && userLogin.EntityType != null )
            {
                var component = AuthenticationContainer.GetComponent( userLogin.EntityType.Name );
                if ( component != null && component.IsActive )
                {
                    if ( component.Authenticate( userLogin, loginParameters.Password ) )
                    {
                        valid = true;
                        Rock.Security.Authorization.SetAuthCookie( loginParameters.Username, loginParameters.Persisted, false );
                    }
                }
            }

            if ( !valid )
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }
        }
    }
}
