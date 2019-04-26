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
using System.Net;
using System.Web;
using System.Web.Security;

using Newtonsoft.Json;

using RestSharp;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Security.ExternalAuthentication
{
    /// <summary>
    /// Authenticates a user using Google
    /// </summary>
    /// <seealso cref="Rock.Security.AuthenticationComponent" />
    [Description( "Google Authentication Provider" )]
    [Export( typeof( AuthenticationComponent ) )]
    [ExportMetadata( "ComponentName", "Google" )]

    [TextField( "Client ID", "The Google Client ID" )]
    [TextField( "Client Secret", "The Google Client Secret" )]

    public class Google : AuthenticationComponent
    {
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
            return ( !String.IsNullOrWhiteSpace( request.QueryString["code"] ) &&
                !String.IsNullOrWhiteSpace( request.QueryString["state"] ) );
        }

        /// <summary>
        /// Generates the login URL.
        /// </summary>
        /// <param name="request">Forming the URL to obtain user consent</param>
        /// <returns></returns>
        public override Uri GenerateLoginUrl( HttpRequest request )
        {
            string returnUrl = request.QueryString["returnurl"];
            string redirectUri = GetRedirectUrl( request );

            return new Uri( string.Format( "https://accounts.google.com/o/oauth2/auth?response_type=code&client_id={0}&redirect_uri={1}&state={2}&scope=https://www.googleapis.com/auth/userinfo.email+https://www.googleapis.com/auth/userinfo.profile",
                GetAttributeValue( "ClientID" ),
                HttpUtility.UrlEncode( redirectUri ),
                HttpUtility.UrlEncode( returnUrl ?? FormsAuthentication.DefaultUrl ) ) );
        }

        ///<summary>
        ///JSON Class for Access Token Response
        ///</summary>
        public class AccessTokenResponse
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
            returnUrl = request.QueryString["State"];
            string redirectUri = GetRedirectUrl( request );

            try
            {
                // Get a new OAuth Access Token for the 'code' that was returned from the Google user consent redirect
                var restClient = new RestClient( "https://www.googleapis.com/oauth2/v4/token" );
                var restRequest = new RestRequest( Method.POST );
                restRequest.AddParameter( "code", request.QueryString["code"] );
                restRequest.AddParameter( "client_id", GetAttributeValue( "ClientID" ) );
                restRequest.AddParameter( "client_secret", GetAttributeValue( "ClientSecret" ) );
                restRequest.AddParameter( "redirect_uri", redirectUri );
                restRequest.AddParameter( "grant_type", "authorization_code" );
                var restResponse = restClient.Execute( restRequest );

                if ( restResponse.StatusCode == HttpStatusCode.OK )
                {
                    var accesstokenresponse = JsonConvert.DeserializeObject<AccessTokenResponse>( restResponse.Content );
                    string accessToken = accesstokenresponse.access_token;

                    // Get information about the person who logged in using Google
                    restRequest = new RestRequest( Method.GET );
                    restRequest.AddParameter( "access_token", accessToken );
                    restRequest.AddParameter( "fields", "id,hd,email,family_name,gender,given_name" );
                    restClient = new RestClient( "https://www.googleapis.com/oauth2/v2/userinfo" );
                    restResponse = restClient.Execute( restRequest );

                    if ( restResponse.StatusCode == HttpStatusCode.OK )
                    {
                        GoogleUser googleUser = JsonConvert.DeserializeObject<GoogleUser>( restResponse.Content );
                        username = GetGoogleUser( googleUser, accessToken );
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
        /// Authenticates the user based on user name and password
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Authenticate( UserLogin user, string password )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Encodes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
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
        /// Google User Object
        /// </summary>
        public class GoogleUser
        {
            /// <summary>
            /// Gets or sets the family_name.
            /// </summary>
            /// <value>
            /// The family_name.
            /// </value>
            public string family_name { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string name { get; set; }

            /// <summary>
            /// Gets or sets the picture.
            /// </summary>
            /// <value>
            /// The picture.
            /// </value>
            public string picture { get; set; }

            /// <summary>
            /// Gets or sets the locale.
            /// </summary>
            /// <value>
            /// The locale.
            /// </value>
            public string locale { get; set; }

            /// <summary>
            /// Gets or sets the gender.
            /// </summary>
            /// <value>
            /// The gender.
            /// </value>
            public string gender { get; set; }

            /// <summary>
            /// Gets or sets the email.
            /// </summary>
            /// <value>
            /// The email.
            /// </value>
            public string email { get; set; }

            /// <summary>
            /// Gets or sets the link.
            /// </summary>
            /// <value>
            /// The link.
            /// </value>
            public string link { get; set; }

            /// <summary>
            /// Gets or sets the given_name.
            /// </summary>
            /// <value>
            /// The given_name.
            /// </value>
            public string given_name { get; set; }

            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public string id { get; set; }

            /// <summary>
            /// Gets or sets the hd.
            /// </summary>
            /// <value>
            /// The hd.
            /// </value>
            public string hd { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="GoogleUser"/> is verified_email.
            /// </summary>
            /// <value>
            ///   <c>true</c> if verified_email; otherwise, <c>false</c>.
            /// </value>
            public bool verified_email { get; set; }
        }

        /// <summary>
        /// Gets the name of the Google user.
        /// </summary>
        /// <param name="googleUser">The Google user.</param>
        /// <param name="accessToken">The access token.</param>
        /// <returns></returns>
        public static string GetGoogleUser( GoogleUser googleUser, string accessToken = "" )
        {
            // accessToken is required
            if ( accessToken.IsNullOrWhiteSpace() )
            {
                return null;
            }

            string username = string.Empty;
            string googleId = googleUser.id;
            string googleLink = googleUser.link;

            string userName = "Google_" + googleId;
            UserLogin user = null;

            using ( var rockContext = new RockContext() )
            {

                // Query for an existing user 
                var userLoginService = new UserLoginService( rockContext );
                user = userLoginService.GetByUserName( userName );

                // If no user was found, see if we can find a match in the person table
                if ( user == null )
                {
                    // Get name/email from Google login
                    string lastName = googleUser.family_name.ToString();
                    string firstName = googleUser.given_name.ToString();
                    string email = googleUser.email ?? string.Empty;

                    Person person = null;

                    // If person had an email, get the first person with the same name and email address.
                    if ( email.IsNotNullOrWhiteSpace() )
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
                             try
                             {
                                 if ( googleUser.gender.ToString().ToLower() == "male" )
                                 {
                                     person.Gender = Gender.Male;
                                 }
                                 else if ( googleUser.gender.ToString().ToLower() == "female" )
                                 {
                                     person.Gender = Gender.Female;
                                 }
                                 else
                                 {
                                     person.Gender = Gender.Unknown;
                                 }
                             }
                             catch
                             {
                                // Empty catch
                            }

                             if ( person != null )
                             {
                                 PersonService.SaveNewPerson( person, rockContext, null, false );
                             }
                         }

                         if ( person != null )
                         {
                             int typeId = EntityTypeCache.Get( typeof( Google ) ).Id;
                             user = UserLoginService.Create( rockContext, person, AuthenticationServiceType.External, typeId, userName, "goog", true );
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
    }
}