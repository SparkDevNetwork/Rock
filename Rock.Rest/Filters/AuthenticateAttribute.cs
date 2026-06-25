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
            // See if user is logged in
            var principal = System.Threading.Thread.CurrentPrincipal;
            if ( principal != null && principal.Identity != null && !string.IsNullOrWhiteSpace( principal.Identity.Name ) )
            {
                // Don't call SetCurrentPerson here because it is already been
                // set when the request first started.
                actionContext.Request.SetUserPrincipal( principal );
                return;
            }

            // Check if ASOS (OpenID Connect) authentication occurred.
            //
            // ASOS bearer tokens intentionally do NOT participate in
            // PersonSession activity tracking. The OIDC client and its
            // token claims own the credential lifecycle; layering a
            // PersonSession on top would not change any platform decision.
            // See PersonSession spec "API key requests" subsection.
            principal = actionContext.RequestContext.Principal;
            if ( principal != null && principal.Identity != null )
            {
                var claimIdentity = principal.Identity as ClaimsIdentity;
                if ( claimIdentity != null )
                {
                    var clientId = claimIdentity.Claims.FirstOrDefault( c => c.Type == Claims.ClientId )?.Value;

                    if ( clientId.IsNotNullOrWhiteSpace() )
                    {
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

                            if ( authClient.AllowUserApiAccess || isScopeApproved )
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
                                        SetRequestContextUser( actionContext, userLogin );
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
                if ( userLogin != null )
                {
                    var identity = new GenericIdentity( userLogin.UserName );
                    principal = new GenericPrincipal( identity, null );
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
                    // those branches below.
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
                    return;
                }
            }

            // If still not successful, check for a JSON Web Token.
            //
            // JWT requests intentionally do NOT participate in PersonSession
            // activity tracking. JWT is a stateless bearer credential whose
            // lifecycle is owned by the token claims, not by Rock's session
            // table. See PersonSession spec "API key requests" subsection.
            if ( TryRetrieveHeader( actionContext, HeaderTokens.JWT, out var jwtString ) )
            {
                // If the JSON Web Token is in the header, we can determine the User from that
                var userLogin = JwtHelper.GetUserLoginByJSONWebToken( new RockContext(), jwtString );
                if ( userLogin != null )
                {
                    var identity = new GenericIdentity( userLogin.UserName );
                    principal = new GenericPrincipal( identity, null );
                    actionContext.Request.SetUserPrincipal( principal );
                    SetRequestContextUser( actionContext, userLogin );
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