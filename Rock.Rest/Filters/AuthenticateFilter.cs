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
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

using Microsoft.Extensions.DependencyInjection;

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Rest.Jwt;

namespace Rock.Rest.Filters;

/// <summary>
/// Performs authentication for API requests. This attempts to authenticate the user using the following methods (in order):
/// <list type="number">
/// <item>Check if the user is already logged in.</item>
/// <item>Check if ASOS authentication occurred.</item>
/// <item>Check for a valid Rock APIKey token in the header or query string.</item>
/// <item>Check for a valid JSON Web Token in the header.</item>
/// </list>
/// </summary>
internal class AuthenticateFilter : IAuthorizationFilter, IFilter
{
    /// <inheritdoc/>
    public bool AllowMultiple => false;


    /// <inheritdoc/>
    Task<HttpResponseMessage> IAuthorizationFilter.ExecuteAuthorizationFilterAsync( HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation )
    {
        if ( actionContext == null )
        {
            throw new ArgumentNullException( nameof( actionContext ) );
        }

        if ( continuation == null )
        {
            throw new ArgumentNullException( nameof( continuation ) );
        }

        // Only trigger authentication if the action method has the
        // [Authenticate] attribute.
        if ( actionContext.ActionDescriptor is ReflectedHttpActionDescriptor reflectedActionDescriptor )
        {
            var methodInfo = reflectedActionDescriptor.MethodInfo;

            if ( methodInfo.GetCustomAttribute<Rest.AuthenticateAttribute>() != null )
            {
                AuthenticateRequest( actionContext );

                if ( actionContext.Response != null )
                {
                    return Task.FromResult( actionContext.Response );
                }
            }
        }

        return continuation();
    }

    /// <summary>
    /// Calls when a process requests authorization.
    /// </summary>
    /// <param name="actionContext">The action context, which encapsulates information for using <see cref="T:System.Web.Http.Filters.AuthorizationFilterAttribute" />.</param>
    internal static void AuthenticateRequest( HttpActionContext actionContext )
    {
        // 1. See if user is logged in
        var principal = System.Threading.Thread.CurrentPrincipal;
        if ( principal != null && principal.Identity != null && !string.IsNullOrWhiteSpace( principal.Identity.Name ) )
        {
            // Don't call SetCurrentPerson here because it is already been
            // set when the request first started.
            actionContext.Request.SetUserPrincipal( principal );
            return;
        }

        // 2. Check if ASOS authentication occurred.
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

        // 3. If not, see if there's a valid Rock APIKey token
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
                SetRequestContextUser( actionContext, userLogin );
                return;
            }
        }

        // 4. If still not successful, check for a JSON Web Token
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
    private static bool TryRetrieveHeader( HttpActionContext actionContext, string key, out string value )
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
    private static void SetRequestContextUser( HttpActionContext actionContext, UserLogin user )
    {
        if ( actionContext.Request.Properties.TryGetValue( "RockServiceProvider", out var objectProvider ) && objectProvider is IServiceProvider serviceProvider )
        {
            var accessor = serviceProvider.GetService<IRockRequestContextAccessor>();

            if ( accessor?.RockRequestContext != null )
            {
                accessor.RockRequestContext.CurrentUser = user;
            }
        }
    }

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
}
