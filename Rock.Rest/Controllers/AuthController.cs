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
using Rock.Rest.Jwt;
using Rock.Security;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Class AuthController.
    /// Implements the <see cref="System.Web.Http.ApiController" />
    /// </summary>
    [Rock.SystemGuid.RestControllerGuid( "713B9E66-E962-4637-B701-53372FB40DBF")]
    public class AuthController : ApiController 
    {
        /// <summary>
        /// Use this to Login a user and return an AuthCookie which can be used in subsequent REST calls
        /// </summary>
        /// <param name="loginParameters">The login parameters.</param>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [HttpPost]
        [System.Web.Http.Route( "api/Auth/Login" )]
        [Rock.SystemGuid.RestActionGuid( "6149C98B-134F-48EB-A92F-D37B9B08B322" )]
        public void Login( [FromBody] LoginParameters loginParameters )
        {
            string userName;
            if ( !IsLoginValid( loginParameters, out var errorMessage, out userName ) )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.Unauthorized, errorMessage );
                throw new HttpResponseException( errorResponse );
            }

            // TODO: Add 2FA support to API authentication.
            Rock.Security.Authorization.SetAuthCookie(
                userName,
                loginParameters.Persisted,
                isImpersonated: false,
                isTwoFactorAuthenticated: true );
        }

        /// <summary>
        /// Check if the login parameters are valid
        /// </summary>
        /// <param name="loginParameters">The parameters that describe the login request.</param>
        /// <param name="errorMessage">The error message if method returns <c>false</c>.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns><c>true</c> if the login request was valid; otherwise <c>false</c>.</returns>
        internal static bool IsLoginValid( LoginParameters loginParameters, out string errorMessage, out string userName )
        {
            userName = null;
            if ( loginParameters == null )
            {
                errorMessage = "Invalid Login Parameters";
                return false;
            }

            bool isAuthenticatedFromToken;
            UserLogin userLogin;

            using ( var rockContext = new RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );
                if ( loginParameters.Authorization.IsNotNullOrWhiteSpace() )
                {
                    userLogin = JwtHelper.GetUserLoginByJSONWebToken( rockContext, loginParameters.Authorization );
                    if ( userLogin == null )
                    {
                        errorMessage = "Invalid Token";
                        return false;
                    }

                    isAuthenticatedFromToken = true;
                }
                else if ( loginParameters.Username.IsNotNullOrWhiteSpace() )
                {
                    userLogin = userLoginService.GetByUserName( loginParameters.Username );
                    isAuthenticatedFromToken = false;
                }
                else
                {
                    errorMessage = "Invalid Login Parameters";
                    return false;
                }

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

            var component = AuthenticationContainer.GetComponent( userLogin.EntityType.Name );
            if ( component == null || !component.IsActive )
            {
                errorMessage = "Account type is inactive.";
                return false;
            }

            if ( component is Rock.Security.Authentication.PINAuthentication )
            {
                // Don't allow PIN authentications.
                errorMessage = "Account type is not supported.";
                return false;
            }

            bool isAuthenticated;
            if ( isAuthenticatedFromToken )
            {
                isAuthenticated = true;
            }
            else
            {
                isAuthenticated = component.Authenticate( userLogin, loginParameters.Password );
            }

            errorMessage = !isAuthenticated ? "Invalid user name or password." : null;
            userName = userLogin?.UserName;

            return isAuthenticated;
        }
    }
}