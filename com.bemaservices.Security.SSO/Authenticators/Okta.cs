using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using RestSharp;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace com.bemaservices.Security.SSO.Authenticators
{
    /// <summary>
    /// Authenticates a user using Okta
    /// </summary>
    [Description( "Okta Authentication Provider" )]
    [Export( typeof( AuthenticationComponent ) )]
    [ExportMetadata( "ComponentName", "Okta" )]

    [UrlLinkField( "Issuer URI", "The Issuer URI you obtain from Okta > API > Authorization Servers", true, "", "", 1)]
    [TextField( "Client Id", "This is the Client Id you will obtain from your Azure App Registration on the Overview page.", true, "", "", 3 )]
    [TextField( "Client Secret", "This is the Client Secret you will obtain from your Azure App Registration on the Certificates & Secrets page.", true, "", "", 4 )]
    [BooleanField("Enable Debug Mode", "Enabling this will generate exceptions at each point of the authentication process. This is very useful for troubleshooting.", false, "", 5)]
    public class Okta : AuthenticationComponent
    {
        private const string EXCEPTION_DEBUG_TEXT = "Okta Debug";

        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        /// <value>
        /// The type of the service.
        /// </value>
        public override AuthenticationServiceType ServiceType
        {
            get { return AuthenticationServiceType.External; }
        }

        /// <summary>
        /// Determines if user is directed to another site (i.e. Facebook, Gmail, Twitter, etc) to confirm approval of using
        /// that site's credentials for authentication.
        /// </summary>
        /// <value>
        /// The requires remote authentication.
        /// </value>
        public override bool RequiresRemoteAuthentication
        {
            get { return true; }
        }

        /// <summary>
        /// Tests the Http Request to determine if authentication should be tested by this
        /// authentication provider.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public override Boolean IsReturningFromAuthentication( HttpRequest request )
        {
            return ( !String.IsNullOrWhiteSpace( request.QueryString["code"] ) );
        }

        /// <summary>
        /// Gets the login button text.
        /// </summary>
        /// <value>
        /// The login button text.
        /// </value>
        public override string LoginButtonText => "<i class='fas fa-sign-in-alt'></i> Okta";

        /// <summary>
        /// Generates the login URL.
        /// </summary>
        /// <param name="request">Forming the URL to obtain user consent</param>
        /// <returns></returns>
        public override Uri GenerateLoginUrl( HttpRequest request )
        {
            string issuerURI = GetAttributeValue( "IssuerURI" );
            string clientId = GetAttributeValue( "ClientId" );
            string returnUrl = request.QueryString["returnurl"];
            string redirectUri = GetRedirectUrl( request );

            // Validating Authorization server URL format
            if ( issuerURI.Substring( issuerURI.Length - 1, 1 ) == "/" )
            {
                issuerURI = issuerURI.Left( issuerURI.Length - 1 );
            }

            string newUrl = string.Format( "{0}/oauth2/v1/authorize?client_id={1}&redirect_uri={2}&response_type=code&scope=openid profile email&state=authorize",
                issuerURI,
                clientId,
                HttpUtility.UrlEncode( redirectUri )
            );

            return new Uri( newUrl );
        }

        /// <summary>
        /// Authenticates the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="username">The username.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        public override Boolean Authenticate( HttpRequest request, out string username, out string returnUrl )
        {
            username = string.Empty;
            returnUrl = request.QueryString["redirect_uri"];
            string redirectUri = GetRedirectUrl( request );
            string issuerURI = GetAttributeValue( "IssuerURI" );
            bool debugModeEnabled = GetAttributeValue( "EnableDebugMode" ).AsBoolean();

            // Validating Authorization server URL format
            if ( issuerURI.Substring( issuerURI.Length - 1, 1 ) == "/" )
            {
                issuerURI = issuerURI.Left( issuerURI.Length - 1 );
            }
                
            try
            {
                // Get a new OAuth Access Token for the 'code' that was returned from the Okta user consent redirect
                var restClient = new RestClient( issuerURI + "/oauth2/v1/token" );
                var restRequest = new RestRequest( Method.POST );
                restRequest.AddParameter( "code", request.QueryString["code"] );
                restRequest.AddParameter( "client_id", GetAttributeValue( "ClientId" ) );
                restRequest.AddParameter( "client_secret", GetAttributeValue( "ClientSecret" ) );
                restRequest.AddParameter( "redirect_uri", redirectUri );
                restRequest.AddParameter( "grant_type", "authorization_code" );
                var restResponse = restClient.Execute( restRequest );

                if( debugModeEnabled )
                {
                    var exceptionText = string.Format( "Access Token: {0}", restResponse.Content );
                    ExceptionLogService.LogException( new Exception( exceptionText, new Exception( EXCEPTION_DEBUG_TEXT ) ) );
                }

                if ( restResponse.StatusCode == HttpStatusCode.OK )
                {
                    var accesstokenresponse = JsonConvert.DeserializeObject<Okta_AccessTokenResponse>( restResponse.Content );
                    string accessToken = accesstokenresponse.access_token;

                    // Get information about the person who logged in
                    restClient = new RestClient( issuerURI + "/oauth2/v1/userinfo" );
                    restRequest = new RestRequest( Method.POST );
                    restRequest.AddHeader( "Authorization", string.Format( "Bearer {0}", accessToken ) );
                    
                    restResponse = restClient.Execute( restRequest );

                    if ( debugModeEnabled )
                    {
                        var exceptionText = string.Format( "User: {0}", restResponse.Content );
                        ExceptionLogService.LogException( new Exception( exceptionText, new Exception( EXCEPTION_DEBUG_TEXT ) ) );
                    }

                    if ( restResponse.StatusCode == HttpStatusCode.OK )
                    {
                        Okta_User oktaUser = JsonConvert.DeserializeObject<Okta_User>( restResponse.Content );
                        username = GetOktaUser( oktaUser, accessToken );

                        if ( debugModeEnabled )
                        {
                            var exceptionText = string.Format( "UserName: {0}", username );
                            ExceptionLogService.LogException( new Exception( exceptionText, new Exception( EXCEPTION_DEBUG_TEXT ) ) );
                        }
                    }
                }
            }

            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, HttpContext.Current );
            }

            return !string.IsNullOrWhiteSpace( username );

        }

        /// <summary>
        /// Gets the URL of an image that should be displayed.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override String ImageUrl()
        {
            return string.Empty;
        }

        private string GetRedirectUrl( HttpRequest request )
        {
            Uri uri = new Uri( request.Url.ToString() );
            return uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + uri.LocalPath;
        }

        /// <summary>
        /// Authenticates the specified user name and password
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public override bool Authenticate( UserLogin user, string password )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Encodes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password"></param>
        /// <returns></returns>
        public override string EncodePassword( UserLogin user, string password )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a value indicating whether [supports change password].
        /// </summary>
        /// <value>
        /// <c>true</c> if [supports change password]; otherwise, <c>false</c>.
        /// </value>
        public override bool SupportsChangePassword
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <param name="warningMessage">The warning message.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool ChangePassword( UserLogin user, string oldPassword, string newPassword, out string warningMessage )
        {
            warningMessage = "not supported";
            return false;
        }

        /// <summary>
        /// Sets the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void SetPassword( UserLogin user, string password )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the name of the Okta user.
        /// </summary>
        /// <param name="oktaUser">The Okta user.</param>
        /// <param name="accessToken">The access token.</param>
        /// <returns></returns>
        public static string GetOktaUser( Okta_User oktaUser, string accessToken = "" )
        {
            // accessToken is required
            if ( accessToken.IsNullOrWhiteSpace() )
            {
                return null;
            }

            string username = string.Empty;
            string email = oktaUser.email;

            string userName = "Okta_" + email;
            UserLogin user = null;

            using ( var rockContext = new RockContext() )
            {

                // Query for an existing user 
                var userLoginService = new UserLoginService( rockContext );
                user = userLoginService.GetByUserName( userName );

                // If no user was found, see if we can find a match in the person table
                if ( user == null )
                {
                    // Get name/email from Okta
                    string lastName = oktaUser.family_name.ToString();
                    string firstName = oktaUser.given_name.ToString();

                    Person person = null;

                    // If person had an email, get the first person with the same name and email address.
                    if ( email.IsNotNullOrWhiteSpace() )
                    {
                        var personService = new PersonService( rockContext );
                        person = personService.FindPerson( firstName, lastName, email, true );
                    }

                    var personRecordTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                    var personStatusPending = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() ).Id;

                    rockContext.WrapTransaction( () =>
                    {
                        if ( person == null )
                        {
                            person = new Person();
                            person.IsSystem = false;
                            person.RecordTypeValueId = personRecordTypeId;
                            person.RecordStatusValueId = personStatusPending;
                            person.FirstName = firstName;
                            person.LastName = lastName;
                            person.Email = email;
                            person.IsEmailActive = true;
                            person.EmailPreference = EmailPreference.EmailAllowed;
                            person.Gender = Gender.Unknown;


                            if ( person != null )
                            {
                                PersonService.SaveNewPerson( person, rockContext, null, false );
                            }
                        }

                        if ( person != null )
                        {
                            int typeId = EntityTypeCache.Get( typeof( Okta ) ).Id;
                            user = UserLoginService.Create( rockContext, person, AuthenticationServiceType.External, typeId, userName, "okta", true );
                        }

                    } );
                }

                if ( user != null )
                {
                    return user.UserName;
                }

                return username;
            }
        }

        #region Models

        ///<summary>
        /// JSON Class for Access Token Response
        ///</summary>
        public class Okta_AccessTokenResponse
        {
            /// <summary>
            /// Gets or sets the access_token.
            /// </summary>
            /// <value>
            /// The access_token.
            /// </value>
            public string access_token { get; set; }

            /// <summary>
            /// Gets or sets the expires_in.
            /// </summary>
            /// <value>
            /// The expires_in.
            /// </value>
            public int expires_in { get; set; }

            /// <summary>
            /// Gets or sets the token_type.
            /// </summary>
            /// <value>
            /// The token_type.
            /// </value>
            public string token_type { get; set; }

            /// <summary>
            /// Gets or sets the scope.
            /// </summary>
            /// <value>
            /// The scope.
            /// </value>
            public string scope { get; set; }
        }

        /// <summary>
        /// Okta User Object
        /// </summary>
        public class Okta_User
        {
            /// <summary>
            /// Gets or sets the sub.
            /// </summary>
            /// <value>
            /// The sub.
            /// </value>
            public string sub { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string name { get; set; }

            /// <summary>
            /// Gets or sets the locale.
            /// </summary>
            /// <value>
            /// The locale.
            /// </value>
            public string locale { get; set; }

            /// <summary>
            /// Gets or sets the email.
            /// </summary>
            /// <value>
            /// The email.
            /// </value>
            public string email { get; set; }

            /// <summary>
            /// Gets or sets the preferred username.
            /// </summary>
            /// <value>
            /// The preferred username.
            /// </value>
            public string preferred_username { get; set; }

            /// <summary>
            /// Gets or sets the name of the given.
            /// </summary>
            /// <value>
            /// The name of the given.
            /// </value>
            public string given_name { get; set; }

            /// <summary>
            /// Gets or sets the name of the family.
            /// </summary>
            /// <value>
            /// The name of the family.
            /// </value>
            public string family_name { get; set; }

            /// <summary>
            /// Gets or sets the zoneinfo.
            /// </summary>
            /// <value>
            /// The zoneinfo.
            /// </value>
            public string zoneinfo { get; set; }

            /// <summary>
            /// Gets or sets the updated at.
            /// </summary>
            /// <value>
            /// The updated at.
            /// </value>
            public int updated_at { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [email verified].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [email verified]; otherwise, <c>false</c>.
            /// </value>
            public bool email_verified { get; set; }
        }
        #endregion
    }
}