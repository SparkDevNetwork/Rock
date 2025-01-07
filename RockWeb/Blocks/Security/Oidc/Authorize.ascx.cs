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
using System.ComponentModel;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;

using AspNet.Security.OpenIdConnect.Primitives;

using Microsoft.Owin.Security;

using Owin;
using Owin.Security.OpenIdConnect.Extensions;

using Rock;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Oidc.Authorization;
using Rock.Web.UI;

namespace RockWeb.Blocks.Security.Oidc
{
    /// <summary>
    /// Choose to authorize the auth client to access the user's data.
    /// </summary>
    [DisplayName( "Authorize" )]
    [Category( "Security > OIDC" )]
    [Description( "Choose to authorize the auth client to access the user's data." )]

    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.OIDC_AUTHORIZE )]
    public partial class Authorize : RockBlock
    {
        #region Keys

        /// <summary>
        /// Page Param Keys
        /// </summary>
        private static class PageParamKey
        {
            /// <summary>
            /// The client identifier
            /// </summary>
            public const string ClientId = "client_id";

            /// <summary>
            /// The scope
            /// </summary>
            public const string Scope = "scope";

            /// <summary>
            /// The accept
            /// </summary>
            public const string Action = "action";
        }

        #endregion Keys

        private const string AntiXsrfTokenKey = "__AntiXsrfToken";
        protected string _antiXsrfTokenValue;
        private const string ScopeCookiePrefix = ".ROCK-OidcScopeApproval-";

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        private string GetAntiForgeryToken()
        {
            //First, check for the existence of the Anti-XSS cookie
            var requestCookie = Request.Cookies[AntiXsrfTokenKey];
            Guid requestCookieGuidValue;

            //If the CSRF cookie is found, parse the token from the cookie.
            //Then, set the global page variable and view state user
            //key. The global variable will be used to validate that it matches in the view state form field in the Page.PreLoad
            //method.
            if ( requestCookie != null
            && Guid.TryParse( requestCookie.Value, out requestCookieGuidValue ) )
            {
                //Set the global token variable so the cookie value can be
                //validated against the value in the view state form field in
                //the Page.PreLoad method.
                _antiXsrfTokenValue = requestCookie.Value;

                //Set the view state user key, which will be validated by the
                //framework during each request
                //Page.ViewStateUserKey = _antiXsrfTokenValue;
            }
            return _antiXsrfTokenValue;
        }

        private string CreateAntiForgeryToken()
        {

            //Generate a new Anti-XSRF token
            _antiXsrfTokenValue = Guid.NewGuid().ToString( "N" );

            //Set the view state user key, which will be validated by the
            //framework during each request
            //Page.ViewStateUserKey = _antiXsrfTokenValue;

            //Create the non-persistent CSRF cookie
            var responseCookie = new HttpCookie( AntiXsrfTokenKey )
            {
                //Set the HttpOnly property to prevent the cookie from
                //being accessed by client side script
                HttpOnly = true,

                //Add the Anti-XSRF token to the cookie value
                Value = _antiXsrfTokenValue
            };

            //If we are using SSL, the cookie should be set to secure to
            //prevent it from being sent over HTTP connections
            // TODO: if ssl secur cookie
            //if ( FormsAuthentication.RequireSSL &&
            //Request.IsSecureConnection )
            //    responseCookie.Secure = true;

            //Add the CSRF cookie to the response
            RockPage.AddOrUpdateCookie( responseCookie );

            return _antiXsrfTokenValue;
        }

        private bool ValidateAntiForgeryToken( string antiXsrfTokenKey )
        {
            //During the initial page load, add the Anti-XSRF token and user
            GetAntiForgeryToken();

            //Validate the Anti-XSRF token
            if ( string.IsNullOrWhiteSpace( antiXsrfTokenKey ) || string.IsNullOrWhiteSpace( _antiXsrfTokenValue ) || antiXsrfTokenKey != _antiXsrfTokenValue )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            // Get the auth client for the request
            var authClient = GetAuthClient();
            if ( authClient == null )
            {
                DenyAuthorization( "Invalid+client" );
                base.OnLoad( e );
                return;
            }

            // Check if this client has already approved the scopes. We'll look for the cookie and check that the scopes have not changed.
            var scopesApprovalCookieValue = RockPage.GetCookie( $"{ScopeCookiePrefix}{authClient.Guid}" )?.Value;
            var scopesPreviouslyApproved = Rock.Security.Encryption.DecryptString( scopesApprovalCookieValue ) == authClient.AllowedScopes.ToString();

            // We have to use querystring, because something in the .net postback chain writes to the Response object which breaks the auth call.
            var action = PageParameter( PageParamKey.Action );
            var token = PageParameter( "token" );

            if ( (action.IsNotNullOrWhiteSpace() && ValidateAntiForgeryToken( token ) ) || scopesPreviouslyApproved )
            {
                if (action == "deny" )
                {
                    DenyAuthorization( "The+user+declined+claim+permissions" );
                    base.OnLoad( e );
                    return;
                }

                AcceptAuthorization();
                base.OnLoad( e );
                return;
            }

            CreateAntiForgeryToken();

            BindClientName();
            BindScopes();

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Methods

        /// <summary>
        /// Denies the authorization.
        /// </summary>
        private void DenyAuthorization( string errorDescription )
        {
            // Notify the client that the authorization grant has been denied by the resource owner.
            var owinContext = Context.GetOwinContext();
            var redirectUri = owinContext.Request.Query["redirect_uri"];
            Response.Redirect( redirectUri + $"?error=access_denied&error_description={errorDescription.Replace( ' ', '+' )}", true );
            ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="message">The message.</param>
        private void ShowError( string message )
        {
            nbNotificationBox.Text = message;
            nbNotificationBox.Visible = true;
        }

        #endregion Methods

        #region UI Bindings

        /// <summary>
        /// Binds the name of the client.
        /// </summary>
        private void BindClientName()
        {
            var authClient = GetAuthClient();

            if ( authClient != null )
            {
                lClientName.Text = authClient.Name;
            }
        }

        /// <summary>
        /// Binds the scopes.
        /// </summary>
        private void BindScopes()
        {
            var scopes = GetRequestedScopes();
            var scopeViewModels = scopes.Select( s => new ScopeViewModel
            {
                Name = s
            } );

            rScopes.DataSource = scopeViewModels;
            rScopes.DataBind();
        }

        #endregion UI Bindings

        #region Data Access

        /// <summary>
        /// Gets the requested scopes.
        /// </summary>
        /// <returns></returns>
        private List<string> GetRequestedScopes()
        {
            var scopes = new List<string> { "Authorization" };
            var owinContext = Context.GetOwinContext();
            var request = owinContext.GetOpenIdConnectRequest();
            var requestedScopes = request.GetScopes();
            var rockContext = new RockContext();
            var authClientService = new AuthClientService( rockContext );

            var clientAllowedClaims = authClientService
                .Queryable()
                .Where( ac => ac.ClientId == request.ClientId )
                .Select( ac => ac.AllowedClaims ).FirstOrDefault();

            var parsedAllowedClientClaims = clientAllowedClaims.FromJsonOrNull<List<string>>();
            if ( parsedAllowedClientClaims == null )
            {
                return new List<string>();
            }
            var authClaimService = new AuthClaimService( rockContext );
            var activeAllowedClientClaims = authClaimService
                .Queryable()
                .Where( ac => parsedAllowedClientClaims.Contains( ac.Name ) )
                .Where( ac => ac.IsActive )
                .Where( ac => requestedScopes.Contains( ac.Scope.Name ) )
                .Select( ac => new { Scope = ac.Scope.PublicName, Claim = ac.PublicName } )
                .GroupBy( ac => ac.Scope, ac => ac.Claim )
                .ToList()
                .Select( ac => new { Scope = ac.Key, Claims = string.Join( ", ", ac.ToArray() ) } );

            scopes.AddRange( activeAllowedClientClaims.Select( ac => ac.Scope == ac.Claims ? ac.Scope : ac.Scope + " (" + ac.Claims + ")" ) );
            return scopes;
            //return scopeString.SplitDelimitedValues().ToList();
        }

        /// <summary>
        /// Gets the authentication client.
        /// </summary>
        /// <returns></returns>
        private AuthClient GetAuthClient()
        {
            if ( _authClient == null )
            {
                var rockContext = new RockContext();
                var authClientService = new AuthClientService( rockContext );
                var authClientId = PageParameter( PageParamKey.ClientId );
                _authClient = authClientService.GetByClientId( authClientId );
            }

            return _authClient;
        }
        private AuthClient _authClient = null;

        #endregion Data Access

        #region Private Methods

        private void AcceptAuthorization()
        {
            var owinContext = Context.GetOwinContext();
            var request = owinContext.GetOpenIdConnectRequest();

            // TODO: only allow valid scopes.
            var requestedScopes = request.GetScopes();
            var authClientId = PageParameter( PageParamKey.ClientId );
            IDictionary<string, string> clientAllowedClaims = null;

            AuthClient authClient = null;
            IEnumerable<string> clientAllowedScopes = null;
            using ( var rockContext = new RockContext() )
            {
                var authClientService = new AuthClientService( rockContext );
                authClient = authClientService.GetByClientId( authClientId );
                clientAllowedScopes = RockIdentityHelper.NarrowRequestedScopesToApprovedScopes( rockContext, authClientId, requestedScopes );
                clientAllowedClaims = RockIdentityHelper.GetAllowedClientClaims( rockContext, authClientId, clientAllowedScopes );
            }

            if ( authClient == null || clientAllowedScopes == null || clientAllowedClaims == null )
            {
                // TODO: Error
                return;
            }

            // Create a new ClaimsIdentity containing the claims that
            // will be used to create an id_token, a token or a code.
            var identity = RockIdentityHelper.GetRockClaimsIdentity( CurrentUser, clientAllowedClaims, authClientId );

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket( identity, new AuthenticationProperties() );
            
            // We should set the scopes to the requested valid scopes.
            ticket.SetScopes( requestedScopes );

            // Set the resource servers the access token should be issued for.
            ticket.SetResources( "resource_server" );

            // Set cookie to remember the fact that this individual as approved the scopes.
            var cookieValue = $"{Rock.Security.Encryption.EncryptString( authClient.AllowedScopes.ToString() )}";
            RockPage.AddOrUpdateCookie( $"{ScopeCookiePrefix}{authClient.Guid}", cookieValue, RockDateTime.Now.AddDays( authClient.ScopeApprovalExpiration ) );

            // Returning a SignInResult will ask ASOS to serialize the specified identity
            // to build appropriate tokens. You should always make sure the identities
            // you return contain the OpenIdConnectConstants.Claims.Subject claim. In this sample,
            // the identity always contains the name identifier returned by the external provider.

            owinContext.Authentication.SignIn( ticket.Properties, identity );
        }

        #endregion Events

        #region View Models

        /// <summary>
        /// Scope View Model
        /// </summary>
        private class ScopeViewModel
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }
        }

        #endregion View Models
    }
}