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
    /// Authenticates a user using Office 365
    /// </summary>
    [Description( "Office 365 Authentication Provider (Version 2.0)" )]
    [Export( typeof( AuthenticationComponent ) )]
    [ExportMetadata( "ComponentName", "Office 365" )]

    [UrlLinkField( "Authorization URI", "URI for authorization requests. You will get this information from your Azure App Registration under Endpoints.", true, "", "", 0 )]
    [UrlLinkField( "Token URI", "URI for token requests. You will get this information from your Azure App Registration under Endpoints.", true, "", "", 1 )]
    [TextField( "Client Id", "This is the Client Id you will obtain from your Azure App Registration on the Overview page.", true, "", "", 3 )]
    [TextField( "Client Secret", "This is the Client Secret you will obtain from your Azure App Registration on the Certificates & Secrets page.", true, "", "", 4 )]
    [BooleanField("Enable Debug Mode", "Enabling this will generate exceptions at each point of the authentication process. This is very useful for troubleshooting.", false, "", 5)]
    public class Office365 : AuthenticationComponent
    {
        private const string EXCEPTION_DEBUG_TEXT = "Office 365 Debug";

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
        public override string LoginButtonText => "<i class='fa fa-microsoft'></i> Office 365";

        /// <summary>
        /// Generates the login URL.
        /// </summary>
        /// <param name="request">Forming the URL to obtain user consent</param>
        /// <returns></returns>
        public override Uri GenerateLoginUrl( HttpRequest request )
        {
            string authorizationURI = GetAttributeValue( "AuthorizationURI" );
            string clientId = GetAttributeValue( "ClientId" );
            string returnUrl = request.QueryString["returnurl"];
            string redirectUri = GetRedirectUrl( request );
            string newUrl = string.Format( "{0}?client_id={1}&redirect_uri={2}&response_type=code&scope=openid https://graph.microsoft.com/User.Read",
                authorizationURI,
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
            string tokenURI = GetAttributeValue( "TokenURI" );
            bool debugModeEnabled = GetAttributeValue( "EnableDebugMode" ).AsBoolean();


            try
            {
                // Get a new OAuth Access Token for the 'code' that was returned from the Office 365 user consent redirect
                var restClient = new RestClient( tokenURI );
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
                    var accesstokenresponse = JsonConvert.DeserializeObject<Office365_AccessTokenResponse>( restResponse.Content );
                    string accessToken = accesstokenresponse.access_token;

                    // Get information about the person who logged in using Office 365
                    restClient = new RestClient( "https://graph.microsoft.com" );
                    restRequest = new RestRequest( "v1.0/me", Method.GET );
                    restRequest.AddHeader( "Authorization", string.Format( "Bearer {0}", accessToken ) );
                    
                    restResponse = restClient.Execute( restRequest );

                    if ( debugModeEnabled )
                    {
                        var exceptionText = string.Format( "User: {0}", restResponse.Content );
                        ExceptionLogService.LogException( new Exception( exceptionText, new Exception( EXCEPTION_DEBUG_TEXT ) ) );
                    }

                    if ( restResponse.StatusCode == HttpStatusCode.OK )
                    {
                        Office365_User office365User = JsonConvert.DeserializeObject<Office365_User>( restResponse.Content );
                        username = GetOffice365User( office365User, accessToken );

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
        /// Gets the name of the Office 365 user.
        /// </summary>
        /// <param name="office365User">The Office 365 user.</param>
        /// <param name="accessToken">The access token.</param>
        /// <returns></returns>
        public static string GetOffice365User( Office365_User office365User, string accessToken = "" )
        {
            // accessToken is required
            if ( accessToken.IsNullOrWhiteSpace() )
            {
                return null;
            }

            string username = string.Empty;
            string email = office365User.userPrincipalName;
            string office365Id = office365User.id;

            string userName = "Office365_" + email;
            UserLogin user = null;

            using ( var rockContext = new RockContext() )
            {

                // Query for an existing user 
                var userLoginService = new UserLoginService( rockContext );
                user = userLoginService.GetByUserName( userName );

                // If no user was found, see if we can find a match in the person table
                if ( user == null )
                {
                    // Get name/email from Office 365 login
                    string lastName = office365User.surName.ToString();
                    string firstName = office365User.givenName.ToString();

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
                            int typeId = EntityTypeCache.Get( typeof( Office365 ) ).Id;
                            user = UserLoginService.Create( rockContext, person, AuthenticationServiceType.External, typeId, userName, "office365", true );
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
        public class Office365_AccessTokenResponse
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

            /// <summary>
            /// Gets or sets the ext_expires_int.
            /// </summary>
            /// <value>
            /// The ext_expires_int.
            /// </value>
            public int ext_expires_int { get; set; }
        }

        /// <summary>
        /// Office 365 User Object
        /// </summary>
        public class Office365_User
        {
            /// <summary>
            /// Gets or sets the id.
            /// </summary>
            /// <value>
            /// The id.
            /// </value>
            public string id { get; set; }

            /// <summary>
            /// Gets or sets the displayName.
            /// </summary>
            /// <value>
            /// The displayName.
            /// </value>
            public string displayName { get; set; }

            /// <summary>
            /// Gets or sets the givenName.
            /// </summary>
            /// <value>
            /// The givenName.
            /// </value>
            public string givenName { get; set; }

            /// <summary>
            /// Gets or sets the jobTitle.
            /// </summary>
            /// <value>
            /// The jobTitle.
            /// </value>
            public string jobTitle { get; set; }

            /// <summary>
            /// Gets or sets the mail.
            /// </summary>
            /// <value>
            /// The mail.
            /// </value>
            public string mail { get; set; }

            /// <summary>
            /// Gets or sets the mobilePhone.
            /// </summary>
            /// <value>
            /// The mobilePhone.
            /// </value>
            public string mobilePhone { get; set; }

            /// <summary>
            /// Gets or sets the officeLocation.
            /// </summary>
            /// <value>
            /// The officeLocation.
            /// </value>
            public string officeLocation { get; set; }

            /// <summary>
            /// Gets or sets the preferredLanugage.
            /// </summary>
            /// <value>
            /// The preferredLanugage.
            /// </value>
            public string preferredLanguage { get; set; }

            /// <summary>
            /// Gets or sets the surName.
            /// </summary>
            /// <value>
            /// The surName.
            /// </value>
            public string surName { get; set; }

            /// <summary>
            /// Gets or sets the userPrincipalName.
            /// </summary>
            /// <value>
            /// The userPrincipalName.
            /// </value>
            public string userPrincipalName { get; set; }
        }
        #endregion
    }
}