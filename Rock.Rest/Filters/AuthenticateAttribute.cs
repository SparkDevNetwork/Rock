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

using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel.Channels;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Jwt;

namespace Rock.Rest.Filters
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Web.Http.Filters.AuthorizationFilterAttribute" />
    public class AuthenticateAttribute : AuthorizationFilterAttribute
    {
        /// <summary>
        /// Standard Claims needed for OIDC Authentication.
        /// </summary>
        private class Claims
        {
            /// <summary>
            /// The username
            /// </summary>
            public const string Username = "username";

            /// <summary>
            /// The client identifier
            /// </summary>
            public const string ClientId = "client_id";
        }

        /// <summary>
        /// Calls when a process requests authorization.
        /// </summary>
        /// <param name="actionContext">The action context, which encapsulates information for using <see cref="T:System.Web.Http.Filters.AuthorizationFilterAttribute" />.</param>
        public override void OnAuthorization( HttpActionContext actionContext )
        {
            // See if user is logged in
            var principal = System.Threading.Thread.CurrentPrincipal;
            if ( principal != null && principal.Identity != null && !string.IsNullOrWhiteSpace( principal.Identity.Name ) )
            {
                actionContext.Request.SetUserPrincipal( principal );
                return;
            }

            // If check if ASOS authentication occurred.
            principal = actionContext.RequestContext.Principal;
            if ( principal != null && principal.Identity != null )
            {
                var claimIdentity = principal.Identity as ClaimsIdentity;
                if ( claimIdentity != null )
                {
                    var clientId = claimIdentity.Claims.FirstOrDefault( c => c.Type == Claims.ClientId )?.Value;
                    if ( clientId.IsNotNullOrWhiteSpace() )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var authClientService = new AuthClientService( rockContext );
                            var authClient = authClientService.GetByClientId( clientId );
                            if ( authClient.AllowUserApiAccess )
                            {
                                var userName = claimIdentity.Claims.FirstOrDefault( c => c.Type == Claims.Username )?.Value;

                                if ( userName.IsNotNullOrWhiteSpace() && clientId.IsNotNullOrWhiteSpace() )
                                {
                                    UserLogin userLogin = null;

                                    var userLoginService = new UserLoginService( rockContext );
                                    userLogin = userLoginService.GetByUserName( userName );

                                    if ( userLogin != null )
                                    {
                                        var identity = new GenericIdentity( userLogin.UserName );
                                        principal = new GenericPrincipal( identity, null );
                                        actionContext.Request.SetUserPrincipal( principal );
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // If not, see if there's a valid Rock APIKey token
            TryRetrieveHeader( actionContext, HeaderTokens.AuthorizationToken, out var authToken );

            if ( string.IsNullOrWhiteSpace( authToken ) )
            {
                string queryString = actionContext.Request.RequestUri.Query;
                authToken = System.Web.HttpUtility.ParseQueryString( queryString ).Get( "apikey" );
            }

            if ( !string.IsNullOrWhiteSpace( authToken ) )
            {
                var userLoginService = new UserLoginService( new Rock.Data.RockContext() );
                var userLogin = userLoginService.Queryable().Where( u => u.ApiKey == authToken ).FirstOrDefault();
                if ( userLogin != null )
                {
                    var identity = new GenericIdentity( userLogin.UserName );
                    principal = new GenericPrincipal( identity, null );
                    actionContext.Request.SetUserPrincipal( principal );
                    return;
                }
            }

            // If still not successful, check for a JSON Web Token
            if ( TryRetrieveHeader( actionContext, HeaderTokens.JWT, out var jwtString ) )
            {
                // If the JSON Web Token is in the header, we can determine the User from that
                var userLogin = JwtHelper.GetUserLoginByJSONWebToken( new RockContext(), jwtString );
                if ( userLogin != null )
                {
                    var identity = new GenericIdentity( userLogin.UserName );
                    principal = new GenericPrincipal( identity, null );
                    actionContext.Request.SetUserPrincipal( principal );
                    return;
                }

                // Just in rare case the GetPersonFromJWTPersonSearchKey feature is being used, see if person can be determined this way 
                var person = JwtHelper.GetPersonFromJWTPersonSearchKey( jwtString );

                if ( person != null )
                {
                    actionContext.Request.Properties.Add( "Person", person );
                    return;
                }
            }
        }

        /// <summary>
        /// Get a header value from the request headers
        /// </summary>
        /// <param name="actionContext"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool TryRetrieveHeader( HttpActionContext actionContext, string key, out string value )
        {
            value = null;
            var hasValue = actionContext.Request.Headers.TryGetValues( key, out var values );
            hasValue = hasValue && values.Any();

            if ( hasValue )
            {
                value = values.First();
            }

            return hasValue;
        }
    }
}