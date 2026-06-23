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
using Rock.Net;
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

            /*
                6/23/26 - DSH

                This endpoint mints a full interactive PersonSession from
                credentials and returns its auth cookie for use in subsequent
                REST calls. It stamps MFA recency (mfaRecency) on purpose, so the
                resulting session reports MultiFactor strength WITHOUT verifying
                an actual second factor. Any caller with a valid
                username/password lands a session that bypasses MFA-gated pages.

                This is a deliberate rule-break. It is NOT secure and it is NOT a
                pattern to follow. It exists only because existing v1 REST API
                consumers depend on this endpoint granting two-factor-authenticated
                status. That was the pre-PersonSession behavior (the legacy auth
                ticket carried isTwoFactorAuthenticated: true), and dropping the
                MFA stamp here would silently break those consumers, so we
                preserve it through the migration.

                Do NOT replicate this anywhere. New code must never stamp MFA
                recency without a verified second factor. This compatibility shim
                stays until the v1 REST API is deprecated; the v2 REST conversion
                will require a genuine second factor before granting any elevated
                or MFA strength, at which point this behavior is removed.

                Reason: Preserve v1 REST MFA-granting behavior that existing API
                consumers depend on; remove when the v1 API is deprecated.
            */
            using ( var rockContext = new RockContext() )
            {
                var userLogin = new UserLoginService( rockContext ).GetByUserName( userName );
                var personAliasId = userLogin?.Person?.PrimaryAliasId;

                if ( userLogin == null || !personAliasId.HasValue || !userLogin.EntityTypeId.HasValue )
                {
                    var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.Unauthorized, "Unable to establish a session for this account." );
                    throw new HttpResponseException( errorResponse );
                }

                var requestContext = RockRequestContextAccessor.Current;
                var personSessionService = new PersonSessionService( rockContext );

                // Stamps MFA recency intentionally to preserve the v1 endpoint's
                // two-factor-authenticated behavior. See the engineering note
                // above: this is a documented rule-break, not a pattern to follow.
                var session = personSessionService.StartComponentSession(
                    requestContext,
                    personAliasId.Value,
                    userLogin.Id,
                    userLogin.EntityTypeId.Value,
                    loginParameters.Persisted,
                    mfaRecency: Rock.RockDateTime.Now );

                personSessionService.Add( session );
                rockContext.SaveChanges();

                personSessionService.SetAuthCookie( session, requestContext );
            }
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
                    isAuthenticated = component.AuthenticateAndTrack( userLogin, loginParameters.Password );

                    rockContext.SaveChanges();
                }

                errorMessage = !isAuthenticated ? "Invalid user name or password." : null;
                userName = userLogin?.UserName;

                return isAuthenticated;
            }
        }
    }
}