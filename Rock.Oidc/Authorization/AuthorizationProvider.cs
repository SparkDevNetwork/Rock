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
using System.Security.Claims;
using System.Threading.Tasks;

using AspNet.Security.OpenIdConnect.Primitives;

using Microsoft.Owin.Security;

using Owin.Security.OpenIdConnect.Extensions;
using Owin.Security.OpenIdConnect.Server;

using Rock.Data;
using Rock.Enums.Security;
using Rock.Model;
using Rock.Security;

namespace Rock.Oidc.Authorization
{
    /// <summary>
    /// Authorization Provider
    /// </summary>
    /// <seealso cref="OpenIdConnectServerProvider" />
    public class AuthorizationProvider : OpenIdConnectServerProvider
    {
        /// <summary>
        /// Represents an event called for each validated token request
        /// to allow the user code to decide how the request should be handled.
        /// </summary>
        /// <param name="context">The context instance associated with this event.</param>
        public override Task HandleTokenRequest( HandleTokenRequestContext context )
        {
            if ( context.Request.IsAuthorizationCodeGrantType() || context.Request.IsRefreshTokenGrantType() )
            {
                // Let the Microsoft OIDC server implementation handle these requests automatically.
                //  1. If this is an `authorization_code` request, this means the individual has already authenticated
                //     using another Rock login approach, and the server can now decided if it's safe to issue an access
                //     token.
                //  2. If this is a `refresh_token` request, this also means the individual has already authenticated
                //     using another Rock login approach, but the previously-issued access token has expired. They are
                //     now attempting to exchange a previously-issued refresh token for a new access token; it's up to
                //     the server to decided if it's safe to issue a new access token.
                return Task.CompletedTask;
            }
            else if ( context.Request.IsPasswordGrantType() )
            {
                UserLogin user = null;
                ClaimsIdentity identity = null;
                IEnumerable<string> allowedClientScopes = null;
                var loginValid = false;

                // Do all the data access here so we can dispose of the rock context asap.
                using ( var rockContext = new RockContext() )
                {
                    var userLoginService = new UserLoginService( rockContext );
                    user = userLoginService.GetByUserName( context.Request.Username );

                    // Populate the entity type for use later.
                    _ = user.EntityType;

                    allowedClientScopes = RockIdentityHelper.NarrowRequestedScopesToApprovedScopes( rockContext, context.Request.ClientId, context.Request.GetScopes() ).ToList();
                    var allowedClientClaims = RockIdentityHelper.GetAllowedClientClaims( rockContext, context.Request.ClientId, allowedClientScopes );
                    identity = RockIdentityHelper.GetRockClaimsIdentity( user, allowedClientClaims, context.Request.ClientId );

                    var component = AuthenticationContainer.GetComponent( user.EntityType.Name );
                    if ( component != null && component.IsActive && !component.RequiresRemoteAuthentication )
                    {
                        loginValid = component.AuthenticateAndTrack( user, context.Request.Password );
                        rockContext.SaveChanges();
                    }
                }

                if ( identity == null || allowedClientScopes == null )
                {
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidClient,
                        description: "Invalid client configuration." );
                    return Task.CompletedTask;
                }

                if ( user == null || !loginValid )
                {
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidGrant,
                        description: "Invalid credentials." );
                    return Task.CompletedTask;
                }

                // Ensure the user is allowed to sign in.
                if ( !user.IsConfirmed.HasValue || !user.IsConfirmed.Value || ( user.IsPasswordChangeRequired != null && user.IsPasswordChangeRequired.Value ) )
                {
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidGrant,
                        description: "The specified user is not allowed to sign in." );
                    return Task.CompletedTask;
                }

                // Create a new authentication ticket holding the user identity.
                var ticket = new AuthenticationTicket( identity, new AuthenticationProperties() );

                // Set the list of scopes granted to the client application.
                ticket.SetScopes( allowedClientScopes );

                // Set the resource servers the access token should be issued for.
                ticket.SetResources( "resource_server" );
                context.Validate( ticket );
            }
            else if ( context.Request.IsClientCredentialsGrantType() )
            {
                // We don't need to validate the client id here because it was already validated in the ValidateTokenRequest method.
                var identity = new ClaimsIdentity( OpenIdConnectServerDefaults.AuthenticationType );

                identity.AddClaim( OpenIdConnectConstants.Claims.Subject, context.Request.ClientId, OpenIdConnectConstants.Destinations.AccessToken );

                // Create a new authentication ticket holding the user identity.
                var ticket = new AuthenticationTicket( identity, new AuthenticationProperties() );

                context.Validate( ticket );
            }
            else
            {
                // Reject unsupported grant types.
                context.Reject(
                    error: OpenIdConnectConstants.Errors.UnsupportedGrantType,
                    description: $"The '{context.Request.GrantType}' grant type is not supported by this authorization server." );
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Represents an event called for each request to the authorization endpoint
        /// to determine if the request is valid and should continue.
        /// </summary>
        /// <param name="context">The context instance associated with this event.</param>
        public override async Task ValidateAuthorizationRequest( ValidateAuthorizationRequestContext context )
        {
            // Note: the OpenID Connect server middleware supports the authorization code, implicit and hybrid flows
            // but this authorization provider only accepts response_type=code authorization/authentication requests.
            // You may consider relaxing it to support the implicit or hybrid flows. In this case, consider adding
            // checks rejecting implicit/hybrid authorization requests when the client is a confidential application.
            if ( !context.Request.IsAuthorizationCodeFlow() )
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.UnsupportedResponseType,
                    description: "Only the authorization code flow is supported by this authorization server." );

                return;
            }

            // Note: to support custom response modes, the OpenID Connect server middleware doesn't
            // reject unknown modes before the ApplyAuthorizationResponse event is invoked.
            // To ensure invalid modes are rejected early enough, a check is made here.
            if ( !context.Request.ResponseMode.IsNullOrWhiteSpace() && !context.Request.IsFormPostResponseMode() &&
                !context.Request.IsFragmentResponseMode() && !context.Request.IsQueryResponseMode() )
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidRequest,
                    description: "The specified 'response_mode' is unsupported." );

                return;
            }

            // Retrieve the application details corresponding to the requested client_id.
            var rockContext = new RockContext();
            var authClientService = new AuthClientService( rockContext );
            var authClient = await authClientService.GetByClientIdAsync( context.ClientId );

            if ( authClient == null )
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidClient,
                    description: "The specified client identifier is invalid." );

                return;
            }

            if ( !context.RedirectUri.IsNullOrWhiteSpace() &&
                !string.Equals( context.RedirectUri, authClient.RedirectUri, StringComparison.OrdinalIgnoreCase ) )
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidRequest,
                    description: "The specified 'redirect_uri' is invalid." );

                return;
            }

            context.Validate( authClient.RedirectUri );
        }

        /// <summary>
        /// Represents an event called for each request to the token endpoint
        /// to determine if the request is valid and should continue.
        /// </summary>
        /// <param name="context">The context instance associated with this event.</param>
        public override async Task ValidateTokenRequest( ValidateTokenRequestContext context )
        {
            // Note: the OpenID Connect server middleware supports authorization code, refresh token, client credentials
            // and resource owner password credentials grant types but this authorization provider uses a safer policy
            // rejecting the last two ones. You may consider relaxing it to support the ROPC or client credentials grant types.
            if ( !context.Request.IsAuthorizationCodeGrantType() && !context.Request.IsRefreshTokenGrantType() && !context.Request.IsTokenRequest() )
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.UnsupportedGrantType,
                    description: "Only authorization code and refresh token grant types are accepted by this authorization server." );

                return;
            }

            // Note: client authentication is not mandatory for non-confidential client applications like mobile apps
            // (except when using the client credentials grant type) but this authorization server uses a safer policy
            // that makes client authentication mandatory and returns an error if client_id or client_secret is missing.
            // You may consider relaxing it to support the resource owner password credentials grant type
            // with JavaScript or desktop applications, where client credentials cannot be safely stored.
            // In this case, call context.Skip() to inform the server middleware the client is not trusted.
            if ( context.ClientId.IsNullOrWhiteSpace() || context.ClientSecret.IsNullOrWhiteSpace() )
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidRequest,
                    description: "The mandatory 'client_id'/'client_secret' parameters are missing." );

                return;
            }

            // Retrieve the application details corresponding to the requested client_id.
            var rockContext = new RockContext();
            var authClientService = new AuthClientService( rockContext );
            var authClient = await authClientService.GetByClientIdAndSecretAsync( context.ClientId, context.ClientSecret );

            if ( authClient == null )
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidClient,
                    description: "The specified client credentials are invalid." );

                return;
            }

            context.Validate();
        }

        /// <summary>
        /// Represents an event called for each request to the logout endpoint
        /// to determine if the request is valid and should continue.
        /// </summary>
        /// <param name="context">The context instance associated with this event.</param>
        public override async Task ValidateLogoutRequest( ValidateLogoutRequestContext context )
        {
            // When provided, post_logout_redirect_uri must exactly
            // match the address registered by the client application.
            if ( !context.PostLogoutRedirectUri.IsNullOrWhiteSpace() )
            {
                var rockContext = new RockContext();
                var authClientService = new AuthClientService( rockContext );
                var authClient = await authClientService.GetByPostLogoutRedirectUrlAsync( context.PostLogoutRedirectUri );

                if ( authClient == null )
                {
                    context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidRequest,
                    description: "The specified 'post_logout_redirect_uri' is invalid." );

                    return;
                }
            }

            context.Validate();
        }

        /// <summary>
        /// Represents an event called for each validated userinfo request
        /// to allow the user code to decide how the request should be handled.
        /// </summary>
        /// <param name="context">The context instance associated with this event.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that can be used to monitor the asynchronous operation.</returns>
        public override Task HandleUserinfoRequest( HandleUserinfoRequestContext context )
        {
            var result = base.HandleUserinfoRequest( context );
            var clientId = context.Ticket?.Identity?.GetClaim( "client_id" );
            var userName = context.Ticket?.Identity?.GetClaim( "username" );
            if ( clientId.IsNullOrWhiteSpace() || userName.IsNullOrWhiteSpace() )
            {
                return result;
            }

            // Populate requested/allowed claims
            // See https://github.com/aspnet-contrib/AspNet.Security.OpenIdConnect.Server/issues/543
            using ( var rockContext = new RockContext() )
            {
                var user = new UserLoginService( rockContext ).GetByUserName( userName );
                if ( user == null )
                {
                    return result;
                }

                var requestedScopes = context.Ticket?.GetScopes();
                var clientAllowedScopes = RockIdentityHelper.NarrowRequestedScopesToApprovedScopes( rockContext, clientId, requestedScopes );
                var clientAllowedClaims = RockIdentityHelper.GetAllowedClientClaims( rockContext, clientId, clientAllowedScopes );
                var claimsIdentity = RockIdentityHelper.GetRockClaimsIdentity( user, clientAllowedClaims, clientId );

                foreach ( var claim in claimsIdentity?.Claims )
                {
                    context.Claims.Add( claim.Type, claim.Value );
                }
            }

            return result;
        }

        /// <summary>
        /// Represents an event called for each validated configuration request
        /// to allow the user code to decide how the request should be handled.
        /// </summary>
        /// <param name="context">The context instance associated with this event.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that can be used to monitor the asynchronous operation.</returns>
        public override Task HandleConfigurationRequest( HandleConfigurationRequestContext context )
        {
            var result = base.HandleConfigurationRequest( context );
            using ( var rockContext = new RockContext() )
            {
                var activeScopes = RockIdentityHelper.GetActiveAuthScopes( rockContext );
                context.Scopes.UnionWith( activeScopes );

                var activeClaims = RockIdentityHelper.GetActiveAuthClaims( rockContext, activeScopes );
                context.Claims.UnionWith( activeClaims );
            }

            return result;
        }

        #region Save Outcomes to HistoryLogin

        /// <summary>
        /// Represents an event called before the authorization response is returned to the caller.
        /// </summary>
        /// <param name="context">The context instance associated with this event.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that can be used to monitor the asynchronous operation.</returns>
        public override Task ApplyAuthorizationResponse( ApplyAuthorizationResponseContext context )
        {
            // Only log failures here; successes will be logged elsewhere.
            var isFailure = context?.Response?.Error.IsNotNullOrWhiteSpace() == true;
            if ( isFailure )
            {
                new HistoryLogin
                {
                    UserName = context.Ticket?.Identity?.GetClaim( OpenIdConnectConstants.Claims.Username ),
                    AuthClientClientId = context.Request?.ClientId,
                    WasLoginSuccessful = false,
                    LoginFailureReason = GetLoginFailureReason( context.Response.Error ),
                    LoginFailureMessage = context.Response.ErrorDescription
                }.SaveAfterDelay();
            }

            return base.ApplyAuthorizationResponse( context );
        }

        /// <summary>
        /// Represents an event called before the token response is returned to the caller.
        /// </summary>
        /// <param name="context">The context instance associated with this event.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that can be used to monitor the asynchronous operation.</returns>
        public override Task ApplyTokenResponse( ApplyTokenResponseContext context )
        {
            if ( context != null )
            {
                // Log all outcomes. It's unlikely that a failure would occur at this stage, but it is possible.
                new HistoryLogin
                {
                    UserName = context.Ticket?.Identity?.GetClaim( OpenIdConnectConstants.Claims.Username ),
                    AuthClientClientId = context.Request?.ClientId,
                    WasLoginSuccessful = context.Ticket != null && context.Response?.Error.IsNullOrWhiteSpace() == true,
                    LoginFailureReason = GetLoginFailureReason( context.Response?.Error ),
                    LoginFailureMessage = context.Response?.ErrorDescription
                }.SaveAfterDelay();
            }

            return base.ApplyTokenResponse( context );
        }

        /// <summary>
        /// Gets the <see cref="LoginFailureReason"/> for the provided error.
        /// </summary>
        /// <param name="error">The error for which to get the failure reason.</param>
        /// <returns>The failure reason.</returns>
        private LoginFailureReason? GetLoginFailureReason( string error )
        {
            if ( error.IsNullOrWhiteSpace() )
            {
                return null;
            }

            switch ( error )
            {
                case OpenIdConnectConstants.Errors.InvalidClient:
                    return LoginFailureReason.InvalidOidcClientId;
                default:
                    return LoginFailureReason.Other;
            }
        }

        #endregion Save Outcomes to HistoryLogin
    }
}
