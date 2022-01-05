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
using System.Net.Http;
using System.Web.Http;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Class AuthController.
    /// Implements the <see cref="System.Web.Http.ApiController" />
    /// </summary>
    [RockGuid( "713b9e66-e962-4637-b701-53372fb40dbf" )]
    public class AuthController : ApiController
    {
        /// <summary>
        /// Use this to Login a user and return an AuthCookie which can be used in subsequent REST calls
        /// </summary>
        /// <param name="loginParameters">The login parameters.</param>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [HttpPost]
        [System.Web.Http.Route( "api/Auth/Login" )]
        [RockGuid( "6149c98b-134f-48eb-a92f-d37b9b08b322" )]
        public void Login( [FromBody] LoginParameters loginParameters )
        {
            if ( !IsLoginValid( loginParameters, out var errorMessage ) )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.Unauthorized, errorMessage );
                throw new HttpResponseException( errorResponse );
            }

            Rock.Security.Authorization.SetAuthCookie( loginParameters.Username, loginParameters.Persisted, false );
        }

        /// <summary>
        /// Check if the login parameters are valid
        /// </summary>
        /// <param name="loginParameters">The parameters that describe the login request.</param>
        /// <param name="errorMessage">The error message if method returns <c>false</c>.</param>
        /// <returns><c>true</c> if the login request was valid; otherwise <c>false</c>.</returns>
        private bool IsLoginValid( LoginParameters loginParameters, out string errorMessage )
        {
            if ( loginParameters == null || loginParameters.Username.IsNullOrWhiteSpace() )
            {
                errorMessage = "Invalid user name.";
                return false;
            }

            UserLogin userLogin;
            using ( var rockContext = new RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );
                userLogin = userLoginService.GetByUserName( loginParameters.Username );

                if ( userLogin == null || userLogin.EntityType == null )
                {
                    errorMessage = "Invalid login type.";
                    return false;
                }

                // Do not allow login if account is locked out.
                if ( userLogin.IsLockedOut.HasValue && userLogin.IsLockedOut.Value )
                {
                    errorMessage = "Account is locked out.";
                    return false;
                }

                // Do not allow login if account is not confirmed.
                if ( !userLogin.IsConfirmed.HasValue || userLogin.IsConfirmed.Value == false )
                {
                    errorMessage = "Account is not confirmed.";
                    return false;
                }
            }

            var pinAuthentication = AuthenticationContainer.GetComponent( typeof( Rock.Security.Authentication.PINAuthentication ).FullName );

            // Don't allow PIN authentications.
            var userLoginEntityType = EntityTypeCache.Get( userLogin.EntityTypeId.Value );
            if ( userLoginEntityType != null && userLoginEntityType.Id == pinAuthentication.EntityType.Id )
            {
                errorMessage = "Account type is not supported.";
                return false;
            }

            var component = AuthenticationContainer.GetComponent( userLogin.EntityType.Name );

            if ( component == null || !component.IsActive )
            {
                errorMessage = "Account type is inactive.";
                return false;
            }

            var result = component.Authenticate( userLogin, loginParameters.Password );

            errorMessage = !result ? "Invalid user name or password." : null;

            return result;
        }
    }
}
