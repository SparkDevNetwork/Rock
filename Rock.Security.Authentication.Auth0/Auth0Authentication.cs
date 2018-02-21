using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Rock.Attribute;
using Rock.Model;

namespace Rock.Security.Authentication.Auth0
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Security.AuthenticationComponent" />
    [Description( "Auth0 Authentication Provider" )]
    [Export( typeof( AuthenticationComponent ) )]
    [ExportMetadata( "ComponentName", "Auth0" )]

    [TextField( "Client ID", "The Auth0 Client ID" )]
    [TextField( "Client Secret", "The Auth0 Client Secret" )]
    [TextField( "Client Domain", "The Auth0 Domain" )]
    public class Auth0Authentication : AuthenticationComponent
    {
        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        /// <value>
        /// The type of the service.
        /// </value>
        public override AuthenticationServiceType ServiceType => AuthenticationServiceType.External;

        /// <summary>
        /// Determines if user is directed to another site (i.e. Facebook, Gmail, Twitter, etc) to confirm approval of using
        /// that site's credentials for authentication.
        /// </summary>
        /// <value>
        /// The requires remote authentication.
        /// </value>
        public override bool RequiresRemoteAuthentication => true;

        /// <summary>
        /// Tests the Http Request to determine if authentication should be tested by this
        /// authentication provider.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public override bool IsReturningFromAuthentication( System.Web.HttpRequest request )
        {
            return ( !string.IsNullOrWhiteSpace( request.QueryString["code"] ) &&
                !string.IsNullOrWhiteSpace( request.QueryString["state"] ) );
        }

        /// <summary>
        /// Generates the login URL.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public override Uri GenerateLoginUrl( System.Web.HttpRequest request )
        {

            /*
            string returnUrl = request.QueryString["returnurl"];
            string redirectUri = GetRedirectUrl( request );
            string authDomain = this.GetAttributeValue( "ClientDomain" );
            string clientId = this.GetAttributeValue( "ClientID" );
            var client = new AuthenticationApiClient( new Uri( string.Format( "https://{0}", authDomain ) ) );

            var authorizeUrlBuilder = client.BuildAuthorizationUrl()
                .WithClient( clientId )
                .WithRedirectUrl( redirectUri )
                .WithResponseType( AuthorizationResponseType.Code )
                .WithScope( "openid profile" )
                // adding this audience will cause Auth0 to use the OIDC-Conformant pipeline
                // you don't need it if your client is flagged as OIDC-Conformant (Advance Settings | OAuth)
                .WithAudience( "https://" + authDomain + "/userinfo" );

            if ( !string.IsNullOrEmpty( returnUrl ) )
            {
                var state = "ru=" + HttpUtility.UrlEncode( returnUrl );
                authorizeUrlBuilder.WithState( state );
            }

            var result = authorizeUrlBuilder.Build();

            return result;
            */

            // TODO: https://auth0.com/docs/api/authentication#support
            return null;
        }

        /// <summary>
        /// Gets the redirect URL.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        private string GetRedirectUrl( HttpRequest request )
        {
            Uri uri = new Uri( request.Url.ToString() );
            return uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + uri.LocalPath;
        }

        /// <summary>
        /// Authenticates the user based on a request from a third-party provider.  Will set the username and returnUrl values.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        public override bool Authenticate( System.Web.HttpRequest request, out string userName, out string returnUrl )
        {
            string authDomain = this.GetAttributeValue( "ClientDomain" );
            string clientId = this.GetAttributeValue( "ClientID" );
            string clientSecret = this.GetAttributeValue( "ClientSecret" );

            userName = string.Empty;
            returnUrl = request.QueryString["State"];
            string redirectUri = GetRedirectUrl( request );


            /*
            AuthenticationApiClient client = new AuthenticationApiClient(new Uri( string.Format( "https://{0}", authDomain ) ) );

            var tokenTask = client.GetTokenAsync( new AuthorizationCodeTokenRequest
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                Code = request.QueryString["code"],
                RedirectUri = redirectUri
            } );

            tokenTask.Wait();

            var token = tokenTask.Result;
            var profileTask = client.GetUserInfoAsync( token.AccessToken );
            profileTask.Wait();

            var profile = profileTask.Result;
            */





            // TODO: https://auth0.com/docs/api/authentication#support
            return true;

        }

        /// <summary>
        /// Gets a value indicating whether [supports change password].
        /// </summary>
        /// <value>
        /// <c>true</c> if [supports change password]; otherwise, <c>false</c>.
        /// </value>
        public override bool SupportsChangePassword => false;

        /// <summary>
        /// Authenticates the user based on user name and password
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override bool Authenticate( UserLogin user, string password )
        {
            // don't implement
            throw new NotImplementedException();
        }

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <param name="warningMessage">The warning message.</param>
        /// <returns></returns>
        public override bool ChangePassword( UserLogin user, string oldPassword, string newPassword, out string warningMessage )
        {
            warningMessage = "not supported";
            return false;
        }

        /// <summary>
        /// Encodes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override string EncodePassword( UserLogin user, string password )
        {
            // don't implement
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the URL of an image that should be displayed.
        /// </summary>
        /// <returns></returns>
        public override string ImageUrl()
        {
            // no image
            return string.Empty;
        }

        /// <summary>
        /// Sets the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <exception cref="NotImplementedException"></exception>
        public override void SetPassword( UserLogin user, string password )
        {
            // don't implement
            throw new NotImplementedException();
        }
    }
}
