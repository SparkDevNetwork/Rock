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
            // Only handle grant_type=password requests and let ASOS
            // process grant_type=refresh_token requests automatically.
            if ( context.Request.IsPasswordGrantType() )
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
                    return Task.FromResult( 0 );
                }

                if ( user == null || !loginValid )
                {
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidGrant,
                        description: "Invalid credentials." );
                    return Task.FromResult( 0 );
                }

                // Ensure the user is allowed to sign in.
                if ( !user.IsConfirmed.HasValue || !user.IsConfirmed.Value || ( user.IsPasswordChangeRequired != null && user.IsPasswordChangeRequired.Value ) )
                {
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidGrant,
                        description: "The specified user is not allowed to sign in." );
                    return Task.FromResult( 0 );
                }

                // Create a new authentication ticket holding the user identity.
                var ticket = new AuthenticationTicket( identity, new AuthenticationProperties() );

                // Set the list of scopes granted to the client application.
                ticket.SetScopes( allowedClientScopes );

                // Set the resource servers the access token should be issued for.
                ticket.SetResources( "resource_server" );
                context.Validate( ticket );
            }

            if ( context.Request.IsClientCredentialsGrantType() )
            {
                // We don't need to validate the client id here because it was already validated in the ValidateTokenRequest method.
                var identity = new ClaimsIdentity( OpenIdConnectServerDefaults.AuthenticationType );

                identity.AddClaim( OpenIdConnectConstants.Claims.Subject, context.Request.ClientId,
                    OpenIdConnectConstants.Destinations.AccessToken );

                // Create a new authentication ticket holding the user identity.
                var ticket = new AuthenticationTicket(
                    identity,
                    new AuthenticationProperties() );

                context.Validate( ticket );
            }
            return Task.FromResult( 0 );
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
                    error: OpenIdConnectConstants.Errors.InvalidClient,
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
                    description: "Only authorization code and refresh token grant types " +
                                 "are accepted by this authorization server." );

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
    }
}
