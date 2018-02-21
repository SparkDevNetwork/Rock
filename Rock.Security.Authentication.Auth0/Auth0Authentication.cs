using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Web;
using RestSharp;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

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
            return !string.IsNullOrWhiteSpace( request.QueryString["code"] ) && !string.IsNullOrWhiteSpace( request.QueryString["state"] );
        }

        /// <summary>
        /// Generates the login URL.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public override Uri GenerateLoginUrl( System.Web.HttpRequest request )
        {
            // see: https://auth0.com/docs/api/authentication#support
            string returnUrl = request.QueryString["returnurl"] ?? System.Web.Security.FormsAuthentication.DefaultUrl;
            string redirectUri = GetRedirectUrl( request );
            string authDomain = this.GetAttributeValue( "ClientDomain" );
            string scope = HttpUtility.UrlEncode( "openid profile email phone_number birthdate" );
            string audience = $"https://{authDomain}/userinfo";
            string clientId = this.GetAttributeValue( "ClientID" );

            string authorizeUrl = $"https://{authDomain}/authorize?response_type=code&scope={scope}&audience={audience}&client_id={clientId}&redirect_uri={redirectUri}&state={returnUrl}";

            return new Uri( authorizeUrl );
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
            returnUrl = request.QueryString["state"];
            string code = request.QueryString["code"];
            string redirectUri = GetRedirectUrl( request );

            // see: https://auth0.com/docs/api/authentication#support
            var restClient = new RestClient( $"https://{authDomain}" );
            var tokenRequest = new RestRequest( "oauth/token", Method.POST );
            var tokenRequestBody = new
            {
                grant_type = "authorization_code",
                client_id = clientId,
                client_secret = clientSecret,
                code,
                redirect_uri = redirectUri
            };

            tokenRequest.AddJsonBody( tokenRequestBody );

            var tokenRestResponse = restClient.Execute<Auth0TokenResponse>( tokenRequest );

            if ( tokenRestResponse.StatusCode == HttpStatusCode.OK )
            {
                Auth0TokenResponse tokenResponse = tokenRestResponse.Data;
                if ( tokenResponse != null )
                {
                    var userInfoRequest = new RestRequest( $"userinfo?access_token={tokenResponse.access_token}", Method.GET );
                    var userInfoRestResponse = restClient.Execute<Auth0UserInfo>( userInfoRequest );
                    if ( userInfoRestResponse.StatusCode == HttpStatusCode.OK )
                    {
                        Auth0UserInfo auth0UserInfo = userInfoRestResponse.Data;
                        userName = GetAuth0UserName( auth0UserInfo );

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// see https://auth0.com/docs/user-profile/normalized/oidc 
        /// </summary>
        public class Auth0UserInfo
        {
            /// <summary>
            /// unique identifier for the user
            /// </summary>
            /// <value>
            /// The sub.
            /// </value>
            public string sub { get; set; }

            /// <summary>
            /// name of the user
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string name { get; set; }

            /// <summary>
            /// the first/given name of the user
            /// </summary>
            /// <value>
            /// The name of the given.
            /// </value>
            public string given_name { get; set; }

            /// <summary>
            /// the surname/last name of the use
            /// </summary>
            /// <value>
            /// The name of the family.
            /// </value>
            public string family_name { get; set; }

            /// <summary>
            /// Gets or sets the email.
            /// </summary>
            /// <value>
            /// The email.
            /// </value>
            public string email { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [email verified].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [email verified]; otherwise, <c>false</c>.
            /// </value>
            public bool email_verified { get; set; }

            /// <summary>
            /// casual name of the user that may/may not be the same as the given_name
            /// </summary>
            /// <value>
            /// The nickname.
            /// </value>
            public string nickname { get; set; }

            /// <summary>
            /// URL of the user's profile picture
            /// </summary>
            /// <value>
            /// The picture.
            /// </value>
            public string picture { get; set; }

            /// <summary>
            /// gender of the user
            /// </summary>
            /// <value>
            /// The gender.
            /// </value>
            public string gender { get; set; }

            /// <summary>
            /// location where the user is located
            /// </summary>
            /// <value>
            /// The locale.
            /// </value>
            public string locale { get; set; }

            /// <summary>
            /// time when the user's profile was last updated
            /// </summary>
            /// <value>
            /// The updated at.
            /// </value>
            public DateTime updated_at { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        private class Auth0TokenResponse
        {
            /// <summary>
            /// Gets or sets the access token.
            /// </summary>
            /// <value>
            /// The access token.
            /// </value>
            public string access_token { get; set; }

            /// <summary>
            /// Gets or sets the refresh token.
            /// </summary>
            /// <value>
            /// The refresh token.
            /// </value>
            public string refresh_token { get; set; }

            /// <summary>
            /// Gets or sets the identifier token.
            /// </summary>
            /// <value>
            /// The identifier token.
            /// </value>
            public string id_token { get; set; }

            /// <summary>
            /// Gets or sets the expires in.
            /// </summary>
            /// <value>
            /// The expires in.
            /// </value>
            public int expires_in { get; set; }

            /// <summary>
            /// Gets or sets the type of the token.
            /// </summary>
            /// <value>
            /// The type of the token.
            /// </value>
            public string token_type { get; set; }
        }

        /// <summary>
        /// Creates or Finds a Person and UserLogin record, and returns the userLogin.UserName
        /// </summary>
        /// <param name="auth0UserInfo">The auth0 user information.</param>
        /// <returns></returns>
        public static string GetAuth0UserName( Auth0UserInfo auth0UserInfo )
        {
            string username = string.Empty;

            string userName = "AUTH0_" + auth0UserInfo.sub;
            UserLogin user = null;

            using ( var rockContext = new RockContext() )
            {
                // Query for an existing user 
                var userLoginService = new UserLoginService( rockContext );
                user = userLoginService.GetByUserName( userName );

                // If no user was found, see if we can find a match in the person table
                if ( user == null )
                {
                    // Get name/email from auth0 userinfo
                    string lastName = auth0UserInfo.family_name;
                    string firstName = auth0UserInfo.given_name;
                    string nickName = auth0UserInfo.nickname;
                    string email = auth0UserInfo.email;

                    Person person = null;

                    // If person had an email, get the first person with the same name and email address.
                    if ( !string.IsNullOrWhiteSpace( email ) )
                    {
                        var personService = new PersonService( rockContext );
                        var people = personService.GetByMatch( firstName, lastName, email );
                        if ( people.Count() == 1 )
                        {
                            person = people.First();
                        }
                    }

                    var personRecordTypeId = DefinedValueCache.Read( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                    var personStatusPending = DefinedValueCache.Read( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() ).Id;

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

                            if ( auth0UserInfo.gender == "male" )
                            {
                                person.Gender = Gender.Male;
                            }
                            else if ( auth0UserInfo.gender == "female" )
                            {
                                person.Gender = Gender.Female;
                            }
                            else
                            {
                                person.Gender = Gender.Unknown;
                            }

                            if ( person != null )
                            {
                                PersonService.SaveNewPerson( person, rockContext, null, false );
                            }
                        }

                        if ( person != null )
                        {
                            int typeId = EntityTypeCache.Read( typeof( Auth0Authentication ) ).Id;
                            user = UserLoginService.Create( rockContext, person, AuthenticationServiceType.External, typeId, userName, "auth0", true );
                        }
                    } );
                }

                if ( user != null )
                {
                    username = user.UserName;

                    if ( user.PersonId.HasValue )
                    {
                        var personService = new PersonService( rockContext );
                        var person = personService.Get( user.PersonId.Value );
                        if ( person != null )
                        {
                            // If person does not have a photo, try to get the photo return with auth0
                            if ( !person.PhotoId.HasValue && !string.IsNullOrWhiteSpace( auth0UserInfo.picture ) )
                            {
                                // Download the photo from the url provided
                                var restClient = new RestClient( auth0UserInfo.picture );
                                var restRequest = new RestRequest( Method.GET );
                                var restResponse = restClient.Execute( restRequest );
                                if ( restResponse.StatusCode == HttpStatusCode.OK )
                                {
                                    var bytes = restResponse.RawBytes;

                                    // Create and save the image
                                    BinaryFileType fileType = new BinaryFileTypeService( rockContext ).Get( Rock.SystemGuid.BinaryFiletype.PERSON_IMAGE.AsGuid() );
                                    if ( fileType != null )
                                    {
                                        var binaryFileService = new BinaryFileService( rockContext );
                                        var binaryFile = new BinaryFile();
                                        binaryFileService.Add( binaryFile );
                                        binaryFile.IsTemporary = false;
                                        binaryFile.BinaryFileType = fileType;
                                        binaryFile.MimeType = restResponse.ContentType;
                                        binaryFile.FileName = user.Person.NickName + user.Person.LastName + ".jpg";
                                        binaryFile.FileSize = bytes.Length;
                                        binaryFile.ContentStream = new System.IO.MemoryStream( bytes );

                                        rockContext.SaveChanges();

                                        person.PhotoId = binaryFile.Id;
                                        rockContext.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }

                return username;
            }
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
