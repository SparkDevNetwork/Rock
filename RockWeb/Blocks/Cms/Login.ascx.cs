using System;
using System.Collections.Generic;
using System.Web.Security;

using Facebook;
using System.Web;
using Rock.Cms;
using Rock.Repository.Cms;
using Rock.Repository.Crm;

namespace RockWeb.Blocks.Cms
{
    public partial class Login : CmsBlock
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // determine if Facebook login enabled
            string facebookAppId = PageInstance.Site.FacebookAppId;
            string facebookAppSecret = PageInstance.Site.FacebookAppSecret;
            bool facebookEnabled = Convert.ToBoolean( AttributeValue( "FacebookEnabled" ) );

            // disable the facebook login button if it's not able to be used
            if ( !facebookEnabled ||  facebookAppId == "" || facebookAppSecret == "")
            {
                phFacebookLogin.Visible = false;
            }
            
            // Check for Facebook query string params. If exists, assume it's a redirect back from Facebook.
            if ( Request.QueryString["code"] != null )
            {
                ProcessOAuth( Request.QueryString["code"], Request.QueryString["state"] );
            }
        }

        /// <summary>
        /// Redirects to Facebook w/ necessary permissions required to gain user approval.
        /// </summary>
        /// <param name="sender">Trigger object of event</param>
        /// <param name="e">Arguments passed in</param>
        protected void ibFacebookLogin_Click( object sender, EventArgs e )
        {
            var returnUrl = Request.QueryString["returnurl"];
            var oAuthClient = new FacebookOAuthClient( FacebookApplication.Current ) { RedirectUri = new Uri( GetOAuthRedirectUrl() ) };
            oAuthClient.AppId = PageInstance.Site.FacebookAppId;
            oAuthClient.AppSecret = PageInstance.Site.FacebookAppSecret;

            // setup some facebook connection settings
            var settings = new Dictionary<string, object>
            {
                        { "display", "popup" },
                        { "scope", "user_birthday,email,read_stream,read_friendlists"},
                        { "state", returnUrl}
            };

            // Grab publically available information. No special permissions needed for authentication.
            var loginUri = oAuthClient.GetLoginUrl( settings );
            Response.Redirect( loginUri.AbsoluteUri );
        }

        /// <summary>
        /// Awaits permission of facebook user and will issue authenication cookie if successful.
        /// </summary>
        /// <param name="code">Facebook authorization code</param>
        /// <param name="state">Redirect url</param>
        private void ProcessOAuth( string code, string state )
        {
            FacebookOAuthResult oAuthResult;

            if ( FacebookOAuthResult.TryParse( Request.Url, out oAuthResult ) && oAuthResult.IsSuccess )
            {
                try
                {
                    var oAuthClient = new FacebookOAuthClient( FacebookApplication.Current ) { RedirectUri = new Uri( GetOAuthRedirectUrl() ) };
                    oAuthClient.AppId = PageInstance.Site.FacebookAppId;
                    oAuthClient.AppSecret = PageInstance.Site.FacebookAppSecret;
                    dynamic tokenResult = oAuthClient.ExchangeCodeForAccessToken( code );
                    string accessToken = tokenResult.access_token;

                    FacebookClient fbClient = new FacebookClient( accessToken );
                    dynamic me = fbClient.Get( "me" );
                    var userRepository = new EntityUserRepository();
                    string facebookId = me.id.ToString();
                    var user = userRepository.FirstOrDefault( u => u.Username == facebookId && u.AuthenticationType == 2 );

                    if ( user == null )
                    {
                        // determine if we can find a match and if so add an user login record
                        string lastName = me.last_name.ToString();
                        string firstName = me.first_name.ToString();
                        string email = me.email.ToString();

                        var personRepository = new EntityPersonRepository();
                        //var person = personRepository.FirstOrDefault( u => u.LastName == lastName && u.FirstName == firstName );
                        
                        
                        // TODO: Show label indicating inability to find user corresponding to facebook id
                        return;
                    }

                    FormsAuthentication.SetAuthCookie( user.Username, false );

                    if ( state != null )
                    {
                        Response.Redirect( state );
                    }
                }
                catch ( FacebookOAuthException oae )
                {
                    // TODO: Add error handeling
                    // Error validating verification code. (usually from wrong return url very picky with formatting)
                    // Error validating client secret.
                    // Error validating application.
                }
            }
        }

        private string GetOAuthRedirectUrl()
        {
            Uri uri = new Uri( HttpContext.Current.Request.Url.ToString() );
            return uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + uri.LocalPath;
        }
    }

    // helpful links
    //  http://blog.prabir.me/post/Facebook-CSharp-SDK-Writing-your-first-Facebook-Application.aspx
}