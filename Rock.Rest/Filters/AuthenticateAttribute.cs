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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel.Channels;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

using Microsoft.Extensions.DependencyInjection;

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Rest.Jwt;
using Rock.Security;
using Rock.Web.Cache;

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

            /// <summary>
            /// The list of scopes, separated by spaces.
            /// </summary>
            public const string Scope = "scope";
        }

        /// <summary>
        /// Calls when a process requests authorization.
        /// </summary>
        /// <param name="actionContext">The action context, which encapsulates information for using <see cref="T:System.Web.Http.Filters.AuthorizationFilterAttribute" />.</param>
        public override void OnAuthorization( HttpActionContext actionContext )
        {
            // Each helper attempts one authentication state and returns true when
            // it has handled the request - either by authenticating the caller or
            // by rejecting it (e.g. a PIN login). A false result means "not my
            // case, try the next state." When none handle the request it proceeds
            // anonymously, which downstream authorization filters enforce.
            if ( TryAuthenticateFromCurrentPrincipal( actionContext ) )
            {
                return;
            }

            if ( TryAuthenticateFromOidcToken( actionContext ) )
            {
                return;
            }

            if ( TryAuthenticateFromApiKey( actionContext ) )
            {
                return;
            }

            if ( TryAuthenticateFromJwt( actionContext ) )
            {
                return;
            }
        }

        /// <summary>
        /// Attempts to authenticate the request from the principal already set on
        /// the current thread. This is the common case: the <c>.ROCK</c> cookie
        /// session was resolved earlier in the pipeline and established the
        /// current principal, so this state only validates and forwards it.
        /// </summary>
        /// <param name="actionContext">The context that describes the API action request.</param>
        /// <returns><c>true</c> when the request has been handled (authenticated or rejected) and no further authentication should be attempted; otherwise <c>false</c>.</returns>
        private bool TryAuthenticateFromCurrentPrincipal( HttpActionContext actionContext )
        {
            // See if user is logged in
            var principal = System.Threading.Thread.CurrentPrincipal;
            if ( principal == null || principal.Identity == null || string.IsNullOrWhiteSpace( principal.Identity.Name ) )
            {
                return false;
            }

            // PIN authentications are not permitted to access the REST API.
            // A .ROCK session can never be backed by a PIN login (the login
            // paths reject them), so this is defense-in-depth against the
            // current user having been established as a PIN login upstream.
            if ( IsPinAuthentication( TryGetRequestContext( actionContext )?.CurrentUser ) )
            {
                actionContext.Response = new HttpResponseMessage( HttpStatusCode.Unauthorized );
                return true;
            }

            // Don't call SetCurrentPerson here because it is already been
            // set when the request first started.
            actionContext.Request.SetUserPrincipal( principal );
            return true;
        }

        /// <summary>
        /// Attempts to authenticate the request from an ASOS (OpenID Connect)
        /// bearer token. The token's client must be approved for user API access,
        /// or the request's action must be covered by one of the token's scopes.
        /// </summary>
        /// <remarks>
        /// ASOS bearer tokens intentionally do NOT participate in PersonSession
        /// activity tracking. The OIDC client and its token claims own the
        /// credential lifecycle; layering a PersonSession on top would not change
        /// any platform decision. See PersonSession spec "API key requests"
        /// subsection.
        /// </remarks>
        /// <param name="actionContext">The context that describes the API action request.</param>
        /// <returns><c>true</c> when the request has been handled (authenticated or rejected) and no further authentication should be attempted; otherwise <c>false</c>.</returns>
        private bool TryAuthenticateFromOidcToken( HttpActionContext actionContext )
        {
            var principal = actionContext.RequestContext.Principal;
            if ( principal == null || principal.Identity == null )
            {
                return false;
            }

            var claimIdentity = principal.Identity as ClaimsIdentity;
            if ( claimIdentity == null )
            {
                return false;
            }

            var clientId = claimIdentity.Claims.FirstOrDefault( c => c.Type == Claims.ClientId )?.Value;
            if ( clientId.IsNullOrWhiteSpace() )
            {
                return false;
            }

            var scopes = claimIdentity.Claims.FirstOrDefault( c => c.Type == Claims.Scope )?.Value?.SplitDelimitedValues( " " ) ?? Array.Empty<string>();
            IReadOnlyList<string> requiredScopes = Array.Empty<string>();

            // Check for any scopes defined on the action method.
            // This is used to allow OAuth clients to be approved
            // for specific APIs instead of the entire API set.
            if ( actionContext.ActionDescriptor is ReflectedHttpActionDescriptor reflectedActionDescriptor )
            {
                var methodInfo = reflectedActionDescriptor.MethodInfo;

                if ( methodInfo.GetCustomAttribute<RequiredScopeAttribute>() is RequiredScopeAttribute requiredScopeAttribute )
                {
                    requiredScopes = requiredScopeAttribute.Scopes;
                }
            }

            using ( var rockContext = new RockContext() )
            {
                var authClientService = new AuthClientService( rockContext );
                var authClient = authClientService.GetByClientId( clientId );
                var isScopeApproved = false;

                // If we have any scopes defined on the action method
                // then check to see if any of them are included with
                // the token.
                if ( requiredScopes.Any() )
                {
                    isScopeApproved = requiredScopes.Any( rs => scopes.Contains( rs ) );
                }

                if ( !authClient.AllowUserApiAccess && !isScopeApproved )
                {
                    return false;
                }

                var userName = claimIdentity.Claims.FirstOrDefault( c => c.Type == Claims.Username )?.Value;
                if ( userName.IsNullOrWhiteSpace() || clientId.IsNullOrWhiteSpace() )
                {
                    return false;
                }

                var userLoginService = new UserLoginService( rockContext );
                var userLogin = userLoginService.GetByUserName( userName );
                if ( userLogin == null )
                {
                    return false;
                }

                // PIN authentications are not permitted to access the REST API.
                if ( IsPinAuthentication( userLogin ) )
                {
                    actionContext.Response = new HttpResponseMessage( HttpStatusCode.Unauthorized );
                    return true;
                }

                var identity = new GenericIdentity( userLogin.UserName );
                principal = new GenericPrincipal( identity, null );
                actionContext.Request.SetUserPrincipal( principal );
                SetRequestContextUser( actionContext, userLogin );
                return true;
            }
        }

        /// <summary>
        /// Attempts to authenticate the request from a Rock API key, supplied
        /// either in the <c>Authorization-Token</c> header or the <c>?apikey=</c>
        /// query string parameter. On success the caller participates in
        /// PersonSession via a long-lived ApiKey-source session.
        /// </summary>
        /// <param name="actionContext">The context that describes the API action request.</param>
        /// <returns><c>true</c> when the request has been handled (authenticated or rejected) and no further authentication should be attempted; otherwise <c>false</c>.</returns>
        private bool TryAuthenticateFromApiKey( HttpActionContext actionContext )
        {
            // If not, see if there's a valid Rock APIKey token
            TryRetrieveHeader( actionContext, HeaderTokens.AuthorizationToken, out var authToken );

            if ( string.IsNullOrWhiteSpace( authToken ) )
            {
                string queryString = actionContext.Request.RequestUri.Query;
                authToken = System.Web.HttpUtility.ParseQueryString( queryString ).Get( "apikey" );
            }

            if ( string.IsNullOrWhiteSpace( authToken ) )
            {
                return false;
            }

            // The RockContext below is intentionally NOT wrapped in a
            // using. The resolved UserLogin (and the PersonSession
            // attached to it on the apikey path) are handed off to the
            // RockRequestContext and live for the remainder of the
            // request. Disposing the context here would tear down its
            // ObjectStateManager and break lazy loading of UserLogin's
            // navigation properties for any downstream consumer.
            var rockContext = new Rock.Data.RockContext();
            var userLoginService = new UserLoginService( rockContext );
            var userLogin = userLoginService.Queryable( "Person" )
                .Where( u => u.ApiKey == authToken )
                .FirstOrDefault();
            if ( userLogin == null )
            {
                return false;
            }

            // PIN authentications are not permitted to access the REST API.
            if ( IsPinAuthentication( userLogin ) )
            {
                actionContext.Response = new HttpResponseMessage( HttpStatusCode.Unauthorized );
                return true;
            }

            var identity = new GenericIdentity( userLogin.UserName );
            var principal = new GenericPrincipal( identity, null );
            actionContext.Request.SetUserPrincipal( principal );
            SetRequestContextUser( actionContext, userLogin );

            // API-key requests
            // participate in PersonSession via a long-lived
            // ApiKey-source session. FindOrCreateApiKeySession
            // reuses an existing active session for this UserLogin
            // (the common case after the first request) or creates
            // one via the upsert-with-unique-key pattern (concurrent
            // first requests cannot race to create duplicates). The
            // resolved session is attached to the request so the
            // activity hook below stamps LastActivityDateTime
            // against the correct row. JWT and ASOS bearer paths
            // intentionally do NOT participate; see the comments on
            // those branches.
            //
            // If the apikey does not match any UserLogin (the
            // current branch is skipped), or that UserLogin was
            // previously deleted, any orphaned PersonSession had its
            // FK SET NULL by the cascade configuration and is
            // unreachable through FindOrCreateApiKeySession (which
            // keys off UserLoginId). Orphans cannot be resurrected.
            var requestContext = TryGetRequestContext( actionContext );
            if ( requestContext != null && userLogin.PersonId.HasValue )
            {
                var session = new PersonSessionService( rockContext )
                    .FindOrCreateApiKeySession( requestContext, userLogin );
                requestContext.SetPersonSession( session );
            }

            FireUpdatePersonSessionLastActivityIfPresent( actionContext );
            return true;
        }

        /// <summary>
        /// Attempts to authenticate the request from a JSON Web Token supplied in
        /// the request header. When the token resolves to a <see cref="UserLogin"/>
        /// that user is authenticated; otherwise, in the rare case the
        /// person-search-key feature is used, the token is resolved directly to a
        /// <see cref="Person"/>.
        /// </summary>
        /// <remarks>
        /// JWT requests intentionally do NOT participate in PersonSession activity
        /// tracking. JWT is a stateless bearer credential whose lifecycle is owned
        /// by the token claims, not by Rock's session table. See PersonSession spec
        /// "API key requests" subsection.
        /// </remarks>
        /// <param name="actionContext">The context that describes the API action request.</param>
        /// <returns><c>true</c> when the request has been handled (authenticated or rejected) and no further authentication should be attempted; otherwise <c>false</c>.</returns>
        private bool TryAuthenticateFromJwt( HttpActionContext actionContext )
        {
            // If still not successful, check for a JSON Web Token.
            if ( !TryRetrieveHeader( actionContext, HeaderTokens.JWT, out var jwtString ) )
            {
                return false;
            }

            UserLogin userLogin;
            try
            {
                userLogin = JwtHelper.GetUserLoginByJSONWebToken( new RockContext(), jwtString );
            }
            catch ( Microsoft.IdentityModel.Tokens.SecurityTokenMalformedException )
            {
                // Silently ignore this exception. It means the JWT was
                // malformed and we will just treat it as an anonymous request.
                userLogin = null;
            }

            // If the JSON Web Token is in the header, we can determine the User from that
            if ( userLogin != null )
            {
                // PIN authentications are not permitted to access the REST API.
                if ( IsPinAuthentication( userLogin ) )
                {
                    actionContext.Response = new HttpResponseMessage( HttpStatusCode.Unauthorized );
                    return true;
                }

                var identity = new GenericIdentity( userLogin.UserName );
                var principal = new GenericPrincipal( identity, null );
                actionContext.Request.SetUserPrincipal( principal );
                SetRequestContextUser( actionContext, userLogin );
                return true;
            }

            // Just in rare case the GetPersonFromJWTPersonSearchKey feature is being used, see if person can be determined this way
            var person = JwtHelper.GetPersonFromJWTPersonSearchKey( jwtString );

            if ( person != null )
            {
                actionContext.Request.Properties.Add( "Person", person );
                return true;
            }

            return false;
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

        /// <summary>
        /// Sets the UserLogin for the RockRequestContext so that it is available
        /// during the request. This is normally already set from the .ROCK cookie
        /// but if the request if using the ?apikey or a JWT then it needs to
        /// be set here.
        /// </summary>
        /// <param name="actionContext">The context that describes the API action request.</param>
        /// <param name="user">The <see cref="UserLogin"/> of the authorized individual.</param>
        private void SetRequestContextUser( HttpActionContext actionContext, UserLogin user )
        {
            var requestContext = TryGetRequestContext( actionContext );

            if ( requestContext != null )
            {
                // These authentication paths (apikey / JWT / OIDC) are backed
                // by a UserLogin, so the current person is that user's person.
                requestContext.SetCurrentIdentity( user?.Person, user );
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="UserLogin"/> authenticated
        /// via <see cref="Rock.Security.Authentication.PINAuthentication"/>. PIN
        /// logins identify a person for low-trust scenarios (e.g. check-in) and
        /// are intentionally not permitted to access the REST API.
        /// </summary>
        /// <param name="userLogin">The user login to check. May be <c>null</c>.</param>
        /// <returns><c>true</c> if the user login is a PIN authentication; otherwise <c>false</c>.</returns>
        private static bool IsPinAuthentication( UserLogin userLogin )
        {
            if ( userLogin?.EntityTypeId == null )
            {
                return false;
            }

            var pinAuthentication = AuthenticationContainer.GetComponent( typeof( Rock.Security.Authentication.PINAuthentication ).FullName );
            var userLoginEntityType = EntityTypeCache.Get( userLogin.EntityTypeId.Value );

            return userLoginEntityType != null && userLoginEntityType.Id == pinAuthentication?.EntityType?.Id;
        }

        /// <summary>
        /// Returns the <see cref="RockRequestContext"/> attached to the current
        /// request via the <see cref="IRockRequestContextAccessor"/>, or
        /// <c>null</c> when no service provider / accessor is available on the
        /// request (e.g., unusual hosting scenarios).
        /// </summary>
        /// <param name="actionContext">The context that describes the API action request.</param>
        /// <returns>The current <see cref="RockRequestContext"/>, or <c>null</c>.</returns>
        private static RockRequestContext TryGetRequestContext( HttpActionContext actionContext )
        {
            if ( !actionContext.Request.Properties.TryGetValue( "RockServiceProvider", out var objectProvider )
                || !( objectProvider is IServiceProvider serviceProvider ) )
            {
                return null;
            }

            return serviceProvider.GetService<IRockRequestContextAccessor>()?.RockRequestContext;
        }

        /// <summary>
        /// Fires the <see cref="Rock.Tasks.UpdatePersonSessionLastActivity"/>
        /// bus task against the <see cref="PersonSession"/> resolved on the
        /// current request, when one is present. No-op for requests
        /// authenticated by paths that intentionally do not produce a
        /// <see cref="PersonSession"/> (JWT, ASOS bearer, OIDC password
        /// grant); see the per-branch comments in
        /// <see cref="OnAuthorization(HttpActionContext)"/>.
        /// </summary>
        /// <param name="actionContext">The context that describes the API action request.</param>
        private void FireUpdatePersonSessionLastActivityIfPresent( HttpActionContext actionContext )
        {
            var personSession = TryGetRequestContext( actionContext )?.PersonSession;

            if ( personSession == null )
            {
                return;
            }

            new Rock.Tasks.UpdatePersonSessionLastActivity.Message
            {
                PersonSessionId = personSession.Id,
                LastActivityDateTime = RockDateTime.Now,
            }.SendIfNeeded();
        }
    }
}