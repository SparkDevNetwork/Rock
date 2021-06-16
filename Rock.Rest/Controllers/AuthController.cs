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
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

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
        public void Login( [FromBody] LoginParameters loginParameters )
        {
            if ( !IsLoginValid( loginParameters ) )
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }

            Rock.Security.Authorization.SetAuthCookie( loginParameters.Username, loginParameters.Persisted, false );
        }

        /// <summary>
        /// Check if the login parameters are valid
        /// </summary>
        /// <param name="loginParameters"></param>
        /// <returns></returns>
        private bool IsLoginValid( LoginParameters loginParameters )
        {
            if ( loginParameters == null || loginParameters.Username.IsNullOrWhiteSpace() )
            {
                return false;
            }

            UserLogin userLogin;
            using ( var rockContext = new RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );
                userLogin = userLoginService.GetByUserName( loginParameters.Username );

                if ( userLogin == null || userLogin.EntityType == null )
                {
                    return false;
                }
            }

            var pinAuthentication = AuthenticationContainer.GetComponent( typeof( Rock.Security.Authentication.PINAuthentication ).FullName );

            // Don't allow PIN authentications.
            var userLoginEntityType = EntityTypeCache.Get( userLogin.EntityTypeId.Value );
            if ( userLoginEntityType != null && userLoginEntityType.Id == pinAuthentication.EntityType.Id )
            {
                return false;
            }

            var component = AuthenticationContainer.GetComponent( userLogin.EntityType.Name );

            if ( component == null || !component.IsActive )
            {
                return false;
            }

            return component.Authenticate( userLogin, loginParameters.Password );
        }
    }
}
