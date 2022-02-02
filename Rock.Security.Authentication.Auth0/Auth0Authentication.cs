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
//
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

    [TextField( "Client Domain", "The Auth0 Domain", order: 0 )]
    [TextField( "Client ID", "The Auth0 Client ID", order: 1 )]
    [TextField( "Client Secret", "The Auth0 Client Secret", order: 2 )]

    [TextField( "Login Button Text", "The text shown on the log in button.", defaultValue: "Auth0 Login", required: false, order: 3 )]
    [TextField( "Login Button CSS Class", "The CSS class applied to the log in button.", required: false, order: 4 )]
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
            var authTokenRequest = new RestRequest( "oauth/token", Method.POST );
            var authTokenRequestBody = new
            {
                grant_type = "authorization_code",
                client_id = clientId,
                client_secret = clientSecret,
                code,
                redirect_uri = redirectUri
            };

            authTokenRequest.AddJsonBody( authTokenRequestBody );

            var authTokenRestResponse = restClient.Execute( authTokenRequest );

            if ( authTokenRestResponse.StatusCode == HttpStatusCode.OK )
            {
                Auth0TokenResponse authTokenResponse = authTokenRestResponse.Content.FromJsonOrNull<Auth0TokenResponse>();
                if ( authTokenResponse != null )
                {
                    var userInfoRequest = new RestRequest( $"userinfo?access_token={authTokenResponse.access_token}", Method.GET );
                    var userInfoRestResponse = restClient.Execute( userInfoRequest );
                    if ( userInfoRestResponse.StatusCode == HttpStatusCode.OK )
                    {
                        Auth0UserInfo auth0UserInfo = userInfoRestResponse.Content.FromJsonOrNull<Auth0UserInfo>();

                        var managementTokenRequest = new RestRequest( "oauth/token", Method.POST );
                        var managementTokenRequestBody = new
                        {
                            grant_type = "client_credentials",
                            client_id = clientId,
                            client_secret = clientSecret,
                            audience = $"https://{authDomain}/api/v2/"
                        };

                        managementTokenRequest.AddJsonBody( managementTokenRequestBody );

                        var managementTokenRestResponse = restClient.Execute( managementTokenRequest );
                        if ( managementTokenRestResponse.StatusCode == HttpStatusCode.OK )
                        {
                            Auth0TokenResponse managementTokenResponse = managementTokenRestResponse.Content.FromJsonOrNull<Auth0TokenResponse>();
                            var managementUserLookupRequest = new RestRequest( $"api/v2/users/{auth0UserInfo.sub}", Method.GET );
                            managementUserLookupRequest.AddHeader( "authorization", $"Bearer {managementTokenResponse?.access_token}" );
                            var managementUserLookupResponse = restClient.Execute( managementUserLookupRequest );

                            if ( managementUserLookupResponse.StatusCode == HttpStatusCode.OK )
                            {
                                // if we were able to look up the user using the management api, fill in null given_name from user_metadata or app_metadata;
                                var managementUserInfo = managementUserLookupResponse.Content.FromJsonOrNull<Auth0ManagementUserInfo>();

                                if ( managementUserInfo != null )
                                {
                                    if ( auth0UserInfo.given_name == null )
                                    {
                                        auth0UserInfo.given_name = managementUserInfo.user_metadata?.given_name ?? managementUserInfo.app_metadata?.given_name;

                                        // if we had to get given_name from user_metadata/app_metadata, then the nickname that we got back might not be correct, so just have it be given_name
                                        auth0UserInfo.nickname = auth0UserInfo.given_name;
                                    }

                                    if ( auth0UserInfo.family_name == null )
                                    {
                                        auth0UserInfo.family_name = managementUserInfo.user_metadata?.family_name ?? managementUserInfo.app_metadata?.family_name;
                                    }
                                }
                            }
                        }


                        userName = GetAuth0UserName( auth0UserInfo );

                        return true;
                    }
                }
            }

            return false;
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
                    string lastName = auth0UserInfo.family_name?.Trim()?.FixCase();
                    string firstName = auth0UserInfo.given_name?.Trim()?.FixCase();
                    string nickName = auth0UserInfo.nickname?.Trim()?.FixCase();
                    string email = auth0UserInfo.email;

                    Person person = null;

                    // If person had an email, get the first person with the same name and email address.
                    if ( !string.IsNullOrWhiteSpace( email ) )
                    {
                        var personService = new PersonService( rockContext );
                        person = personService.FindPerson( firstName, lastName, email, true );
                    }

                    var personRecordTypeId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                    var personStatusPending = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() ).Id;

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
                            int typeId = EntityTypeCache.Get( typeof( Auth0Authentication ) ).Id;
                            user = UserLoginService.Create( rockContext, person, AuthenticationServiceType.External, typeId, userName, "auth0", true );
                            user.ForeignKey = auth0UserInfo.sub;
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
        /// Gets the log in button CSS class.
        /// </summary>
        /// <value>
        /// The log in button CSS class.
        /// </value>
        public override string LoginButtonCssClass => this.GetAttributeValue( "LoginButtonCSSClass" );

        /// <summary>
        /// Gets the log in button text.
        /// </summary>
        /// <value>
        /// The log in button text.
        /// </value>
        public override string LoginButtonText => this.GetAttributeValue( "LoginButtonText" );

        /// <summary>
        /// Gets the redirect URL.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        private string GetRedirectUrl( HttpRequest request )
        {
            Uri uri = new Uri( request.UrlProxySafe().ToString() );
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
