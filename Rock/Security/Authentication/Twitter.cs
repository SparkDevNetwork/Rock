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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Security;

using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

using RestSharp;
using RestSharp.Authenticators;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Security.ExternalAuthentication
{
    /// <summary>
    /// Authenticates a user using Twitter
    /// </summary>
    [Description( "Twitter Authentication Provider" )]
    [Export( typeof( AuthenticationComponent ) )]
    [ExportMetadata( "ComponentName", "Twitter" )]

    [TextField( "Consumer Key", "The Twitter Consumer Key" )]
    [TextField( "Consumer Secret", "The Twitter Consumer Secret" )]

    public class Twitter : AuthenticationComponent
    {
        /// <summary>
        /// The _oauth token
        /// </summary>
        private string _oauthToken = null;
        
        /// <summary>
        /// The _oauth token secret
        /// </summary>
        private string _oauthTokenSecret = null;
        
        /// <summary>
        /// The _return URL
        /// </summary>
        private string _returnUrl = null;

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
            return ( !String.IsNullOrWhiteSpace( request.QueryString["oauth_verifier"] ) &&
                !String.IsNullOrWhiteSpace( request.QueryString["oauth_token"] ) );
        }

        /// <summary>
        /// Generates the login URL.
        /// </summary>
        /// <param name="request">Forming the URL to obtain user consent</param>
        /// <returns></returns>
        public override Uri GenerateLoginUrl( HttpRequest request )
        {
            _returnUrl = FormsAuthentication.DefaultUrl;
            if ( !string.IsNullOrWhiteSpace( request.QueryString.ToString() ) )
            {
                _returnUrl = HttpUtility.UrlDecode( request.Url.GetLeftPart( UriPartial.Authority ) + request.QueryString["returnurl"] );
            }

            string redirectUri = GetRedirectUrl( request );

            RequestToken( redirectUri );

            // Redirect user to /authenticate
            return new Uri
                ( string.Format( "https://api.twitter.com/oauth/authenticate?oauth_token={0}",
                _oauthToken ) );

        }

        /// <summary>
        /// Gets the request token.
        /// </summary>
        /// <param name="redirectUri">The redirect URI.</param>
        public void RequestToken( string redirectUri )
        {
            // Obtain request token
            var restClient = new RestClient( "https://api.twitter.com" )
            {
                Authenticator = OAuth1Authenticator.ForRequestToken( GetAttributeValue( "ConsumerKey" ), GetAttributeValue( "ConsumerSecret" ), redirectUri )
            };
            var restRequest = new RestRequest( "oauth/request_token", Method.POST );
            var response = restClient.Execute( restRequest );
            NameValueCollection r = HttpUtility.ParseQueryString( response.Content );
            _oauthToken = r["oauth_token"];
            _oauthTokenSecret = r["oauth_token_secret"];
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
            returnUrl = _returnUrl;
            string oauthVerifier = request.QueryString["oauth_verifier"];
            string callbackOAuthToken = request.QueryString["oauth_token"];

            // Verify callback oauth token matches previous request token
            if ( callbackOAuthToken == _oauthToken )
            {
                try
                {
                    string consumerKey = GetAttributeValue( "ConsumerKey" );
                    string consumerSecret = GetAttributeValue( "ConsumerSecret" );
                    // Get an access token for the authenticated request token that was returned in the callback URL
                    var restClient = new RestClient( "https://api.twitter.com" )
                    {
                        Authenticator = OAuth1Authenticator.ForAccessToken( consumerKey, consumerSecret, _oauthToken, _oauthTokenSecret, oauthVerifier )
                    };
                    var restRequest = new RestRequest( "oauth/access_token", Method.POST ); ; ;
                    var restResponse = restClient.Execute( restRequest );
                    if ( restResponse.StatusCode == HttpStatusCode.OK )
                    {
                        NameValueCollection r = HttpUtility.ParseQueryString( restResponse.Content );
                        string accessToken = r["oauth_token"];
                        string accessTokenSecret = r["oauth_token_secret"];

                        // Get information about the person who logged in
                        restRequest = new RestRequest( "1.1/account/verify_credentials.json", Method.GET );
                        restClient.Authenticator = OAuth1Authenticator.ForProtectedResource( consumerKey, consumerSecret, accessToken, accessTokenSecret );
                        restRequest.AddParameter( "skip_status", true );
                        restRequest.AddParameter( "include_email", true );
                        restResponse = restClient.Execute( restRequest );

                        if ( restResponse.StatusCode == HttpStatusCode.OK )
                        {
                            dynamic twitterUser = JObject.Parse( restResponse.Content );
                            username = GetTwitterUser( twitterUser, accessToken );
                        }
                    }
                }

                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex, HttpContext.Current );
                }

            }


            return !string.IsNullOrWhiteSpace( username );
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
        /// Gets the URL of an image that should be displayed.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override String ImageUrl()
        {
            return "";
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
            warningMessage = "Not supported";
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
        /// Gets the name of the Twitter user.
        /// </summary>
        /// <param name="twitterUser">The Twitter user.</param>
        /// <param name="accessToken">The access token.</param>
        /// <returns></returns>
        public static string GetTwitterUser( dynamic twitterUser, string accessToken = "" )
        {
            // accessToken is required
            if ( accessToken.IsNullOrWhiteSpace() )
            {
                return null;
            }

            string username = string.Empty;
            string twitterId = twitterUser.id_str;
            string twitterLink = "https://twitter.com/" + twitterUser.screen_name;

            string userName = "Twitter_" + twitterId;
            UserLogin user = null;

            using ( var rockContext = new RockContext() )
            {

                // Query for an existing user 
                var userLoginService = new UserLoginService( rockContext );
                user = userLoginService.GetByUserName( userName );

                // If no user was found, see if we can find a match in the person table
                if ( user == null )
                {
                    // Get name and email from twitterUser object and then split the name
                    string fullName = twitterUser.name;
                    string firstName = null;
                    string lastName = null;
                    var personService = new PersonService( rockContext );
                    personService.SplitName( fullName, out firstName, out lastName );
                    string email = string.Empty;
                    try { email = twitterUser.email; }
                    catch { }

                    Person person = null;

                    // If person had an email, get the first person with the same name and email address.
                    if ( !string.IsNullOrWhiteSpace( email ) )
                    {
                        person = personService.FindPerson( firstName, lastName, email, true );
                    }

                    var personRecordTypeId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                    var personStatusPending = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() ).Id;

                    rockContext.WrapTransaction( () =>
                    {
                        // If not an existing person, create a new one
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
                            int typeId = EntityTypeCache.Get( typeof( Facebook ) ).Id;
                            user = UserLoginService.Create( rockContext, person, AuthenticationServiceType.External, typeId, userName, "Twitter", true );
                        }

                    } );
                }

                if ( user != null )
                {
                    username = user.UserName;

                    if ( user.PersonId.HasValue )
                    {
                        var converter = new ExpandoObjectConverter();

                        var personService = new PersonService( rockContext );
                        var person = personService.Get( user.PersonId.Value );
                        if ( person != null )
                        {
                            string twitterImageUrl = twitterUser.profile_image_url;
                            bool twitterImageDefault = twitterUser.default_profile_image;
                            twitterImageUrl = twitterImageUrl.Replace( "_normal", "" );
                            // If person does not have a photo, use their Twitter photo if it exists
                            if ( !person.PhotoId.HasValue && !twitterImageDefault && !string.IsNullOrWhiteSpace( twitterImageUrl ) )
                            {
                                // Download the photo from the url provided
                                var restClient = new RestClient( twitterImageUrl );
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
                                        binaryFile.MimeType = "image/jpeg";
                                        binaryFile.FileName = user.Person.NickName + user.Person.LastName + ".jpg";
                                        binaryFile.FileSize = bytes.Length;
                                        binaryFile.ContentStream = new MemoryStream( bytes );

                                        rockContext.SaveChanges();

                                        person.PhotoId = binaryFile.Id;
                                        rockContext.SaveChanges();
                                    }
                                }
                            }

                            // Save the Twitter social media link
                            var twitterAttribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_TWITTER.AsGuid() );
                            if ( twitterAttribute != null )
                            {
                                person.LoadAttributes( rockContext );
                                person.SetAttributeValue( twitterAttribute.Key, twitterLink );
                                person.SaveAttributeValues( rockContext );
                            }
                        }

                    }
                }

                return username;
            }
        }

        /// <summary>
        /// Twitter User Object
        /// </summary>
        public class TwitterUser
        {

            /// <summary>
            /// 
            /// </summary>
            public class Coordinates
            {
                /// <summary>
                /// Gets or sets the coordinates.
                /// </summary>
                /// <value>
                /// The coordinates.
                /// </value>
                public List<double> coordinates { get; set; }
                /// <summary>
                /// Gets or sets the type.
                /// </summary>
                /// <value>
                /// The type.
                /// </value>
                public string type { get; set; }
            }

            /// <summary>
            /// 
            /// </summary>
            public class Geo
            {
                /// <summary>
                /// Gets or sets the coordinates.
                /// </summary>
                /// <value>
                /// The coordinates.
                /// </value>
                public List<double> coordinates { get; set; }
                /// <summary>
                /// Gets or sets the type.
                /// </summary>
                /// <value>
                /// The type.
                /// </value>
                public string type { get; set; }
            }

            /// <summary>
            /// 
            /// </summary>
            public class Attributes
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public class BoundingBox
            {
                /// <summary>
                /// Gets or sets the coordinates.
                /// </summary>
                /// <value>
                /// The coordinates.
                /// </value>
                public List<List<List<double>>> coordinates { get; set; }
                /// <summary>
                /// Gets or sets the type.
                /// </summary>
                /// <value>
                /// The type.
                /// </value>
                public string type { get; set; }
            }

            /// <summary>
            /// 
            /// </summary>
            public class Place
            {
                /// <summary>
                /// Gets or sets the attributes.
                /// </summary>
                /// <value>
                /// The attributes.
                /// </value>
                public Attributes attributes { get; set; }
                /// <summary>
                /// Gets or sets the bounding_box.
                /// </summary>
                /// <value>
                /// The bounding_box.
                /// </value>
                public BoundingBox bounding_box { get; set; }
                /// <summary>
                /// Gets or sets the country.
                /// </summary>
                /// <value>
                /// The country.
                /// </value>
                public string country { get; set; }
                /// <summary>
                /// Gets or sets the country_code.
                /// </summary>
                /// <value>
                /// The country_code.
                /// </value>
                public string country_code { get; set; }
                /// <summary>
                /// Gets or sets the full_name.
                /// </summary>
                /// <value>
                /// The full_name.
                /// </value>
                public string full_name { get; set; }
                /// <summary>
                /// Gets or sets the identifier.
                /// </summary>
                /// <value>
                /// The identifier.
                /// </value>
                public string id { get; set; }
                /// <summary>
                /// Gets or sets the name.
                /// </summary>
                /// <value>
                /// The name.
                /// </value>
                public string name { get; set; }
                /// <summary>
                /// Gets or sets the place_type.
                /// </summary>
                /// <value>
                /// The place_type.
                /// </value>
                public string place_type { get; set; }
                /// <summary>
                /// Gets or sets the URL.
                /// </summary>
                /// <value>
                /// The URL.
                /// </value>
                public string url { get; set; }
            }

            /// <summary>
            /// 
            /// </summary>
            public class Status
            {
                /// <summary>
                /// Gets or sets the contributors.
                /// </summary>
                /// <value>
                /// The contributors.
                /// </value>
                public object contributors { get; set; }
                /// <summary>
                /// Gets or sets the coordinates.
                /// </summary>
                /// <value>
                /// The coordinates.
                /// </value>
                public Coordinates coordinates { get; set; }
                /// <summary>
                /// Gets or sets the created_at.
                /// </summary>
                /// <value>
                /// The created_at.
                /// </value>
                public string created_at { get; set; }
                /// <summary>
                /// Gets or sets a value indicating whether this <see cref="Status"/> is favorited.
                /// </summary>
                /// <value>
                ///   <c>true</c> if favorited; otherwise, <c>false</c>.
                /// </value>
                public bool favorited { get; set; }
                /// <summary>
                /// Gets or sets the geo.
                /// </summary>
                /// <value>
                /// The geo.
                /// </value>
                public Geo geo { get; set; }
                /// <summary>
                /// Gets or sets the identifier.
                /// </summary>
                /// <value>
                /// The identifier.
                /// </value>
                public long id { get; set; }
                /// <summary>
                /// Gets or sets the id_str.
                /// </summary>
                /// <value>
                /// The id_str.
                /// </value>
                public string id_str { get; set; }
                /// <summary>
                /// Gets or sets the in_reply_to_screen_name.
                /// </summary>
                /// <value>
                /// The in_reply_to_screen_name.
                /// </value>
                public string in_reply_to_screen_name { get; set; }
                /// <summary>
                /// Gets or sets the in_reply_to_status_id.
                /// </summary>
                /// <value>
                /// The in_reply_to_status_id.
                /// </value>
                public long in_reply_to_status_id { get; set; }
                /// <summary>
                /// Gets or sets the in_reply_to_status_id_str.
                /// </summary>
                /// <value>
                /// The in_reply_to_status_id_str.
                /// </value>
                public string in_reply_to_status_id_str { get; set; }
                /// <summary>
                /// Gets or sets the in_reply_to_user_id.
                /// </summary>
                /// <value>
                /// The in_reply_to_user_id.
                /// </value>
                public int in_reply_to_user_id { get; set; }
                /// <summary>
                /// Gets or sets the in_reply_to_user_id_str.
                /// </summary>
                /// <value>
                /// The in_reply_to_user_id_str.
                /// </value>
                public string in_reply_to_user_id_str { get; set; }
                /// <summary>
                /// Gets or sets the place.
                /// </summary>
                /// <value>
                /// The place.
                /// </value>
                public Place place { get; set; }
                /// <summary>
                /// Gets or sets the retweet_count.
                /// </summary>
                /// <value>
                /// The retweet_count.
                /// </value>
                public int retweet_count { get; set; }
                /// <summary>
                /// Gets or sets a value indicating whether this <see cref="Status"/> is retweeted.
                /// </summary>
                /// <value>
                ///   <c>true</c> if retweeted; otherwise, <c>false</c>.
                /// </value>
                public bool retweeted { get; set; }
                /// <summary>
                /// Gets or sets the source.
                /// </summary>
                /// <value>
                /// The source.
                /// </value>
                public string source { get; set; }
                /// <summary>
                /// Gets or sets the text.
                /// </summary>
                /// <value>
                /// The text.
                /// </value>
                public string text { get; set; }
                /// <summary>
                /// Gets or sets a value indicating whether this <see cref="Status"/> is truncated.
                /// </summary>
                /// <value>
                ///   <c>true</c> if truncated; otherwise, <c>false</c>.
                /// </value>
                public bool truncated { get; set; }
            }

            /// <summary>
            /// 
            /// </summary>
            public class RootObject
            {
                /// <summary>
                /// Gets or sets a value indicating whether this <see cref="RootObject"/> is contributors_enabled.
                /// </summary>
                /// <value>
                ///   <c>true</c> if contributors_enabled; otherwise, <c>false</c>.
                /// </value>
                public bool contributors_enabled { get; set; }
                /// <summary>
                /// Gets or sets the created_at.
                /// </summary>
                /// <value>
                /// The created_at.
                /// </value>
                public string created_at { get; set; }
                /// <summary>
                /// Gets or sets a value indicating whether this <see cref="RootObject"/> is default_profile.
                /// </summary>
                /// <value>
                ///   <c>true</c> if default_profile; otherwise, <c>false</c>.
                /// </value>
                public bool default_profile { get; set; }
                /// <summary>
                /// Gets or sets a value indicating whether this <see cref="RootObject"/> is default_profile_image.
                /// </summary>
                /// <value>
                ///   <c>true</c> if default_profile_image; otherwise, <c>false</c>.
                /// </value>
                public bool default_profile_image { get; set; }
                /// <summary>
                /// Gets or sets the description.
                /// </summary>
                /// <value>
                /// The description.
                /// </value>
                public string description { get; set; }
                /// <summary>
                /// Gets or sets the email.
                /// </summary>
                /// <value>
                /// The email.
                /// </value>
                public string email { get; set; }
                /// <summary>
                /// Gets or sets the favourites_count.
                /// </summary>
                /// <value>
                /// The favourites_count.
                /// </value>
                public int favourites_count { get; set; }
                /// <summary>
                /// Gets or sets the follow_request_sent.
                /// </summary>
                /// <value>
                /// The follow_request_sent.
                /// </value>
                public object follow_request_sent { get; set; }
                /// <summary>
                /// Gets or sets the followers_count.
                /// </summary>
                /// <value>
                /// The followers_count.
                /// </value>
                public int followers_count { get; set; }
                /// <summary>
                /// Gets or sets the following.
                /// </summary>
                /// <value>
                /// The following.
                /// </value>
                public object following { get; set; }
                /// <summary>
                /// Gets or sets the friends_count.
                /// </summary>
                /// <value>
                /// The friends_count.
                /// </value>
                public int friends_count { get; set; }
                /// <summary>
                /// Gets or sets a value indicating whether this <see cref="RootObject"/> is geo_enabled.
                /// </summary>
                /// <value>
                ///   <c>true</c> if geo_enabled; otherwise, <c>false</c>.
                /// </value>
                public bool geo_enabled { get; set; }
                /// <summary>
                /// Gets or sets the identifier.
                /// </summary>
                /// <value>
                /// The identifier.
                /// </value>
                public int id { get; set; }
                /// <summary>
                /// Gets or sets the id_str.
                /// </summary>
                /// <value>
                /// The id_str.
                /// </value>
                public string id_str { get; set; }
                /// <summary>
                /// Gets or sets a value indicating whether this <see cref="RootObject"/> is is_translator.
                /// </summary>
                /// <value>
                ///   <c>true</c> if is_translator; otherwise, <c>false</c>.
                /// </value>
                public bool is_translator { get; set; }
                /// <summary>
                /// Gets or sets the language.
                /// </summary>
                /// <value>
                /// The language.
                /// </value>
                public string lang { get; set; }
                /// <summary>
                /// Gets or sets the listed_count.
                /// </summary>
                /// <value>
                /// The listed_count.
                /// </value>
                public int listed_count { get; set; }
                /// <summary>
                /// Gets or sets the location.
                /// </summary>
                /// <value>
                /// The location.
                /// </value>
                public string location { get; set; }
                /// <summary>
                /// Gets or sets the name.
                /// </summary>
                /// <value>
                /// The name.
                /// </value>
                public string name { get; set; }
                /// <summary>
                /// Gets or sets the notifications.
                /// </summary>
                /// <value>
                /// The notifications.
                /// </value>
                public object notifications { get; set; }
                /// <summary>
                /// Gets or sets the profile_background_color.
                /// </summary>
                /// <value>
                /// The profile_background_color.
                /// </value>
                public string profile_background_color { get; set; }
                /// <summary>
                /// Gets or sets the profile_background_image_url.
                /// </summary>
                /// <value>
                /// The profile_background_image_url.
                /// </value>
                public string profile_background_image_url { get; set; }
                /// <summary>
                /// Gets or sets the profile_background_image_url_https.
                /// </summary>
                /// <value>
                /// The profile_background_image_url_https.
                /// </value>
                public string profile_background_image_url_https { get; set; }
                /// <summary>
                /// Gets or sets a value indicating whether this <see cref="RootObject"/> is profile_background_tile.
                /// </summary>
                /// <value>
                ///   <c>true</c> if profile_background_tile; otherwise, <c>false</c>.
                /// </value>
                public bool profile_background_tile { get; set; }
                /// <summary>
                /// Gets or sets the profile_image_url.
                /// </summary>
                /// <value>
                /// The profile_image_url.
                /// </value>
                public string profile_image_url { get; set; }
                /// <summary>
                /// Gets or sets the profile_image_url_https.
                /// </summary>
                /// <value>
                /// The profile_image_url_https.
                /// </value>
                public string profile_image_url_https { get; set; }
                /// <summary>
                /// Gets or sets the profile_link_color.
                /// </summary>
                /// <value>
                /// The profile_link_color.
                /// </value>
                public string profile_link_color { get; set; }
                /// <summary>
                /// Gets or sets the profile_sidebar_border_color.
                /// </summary>
                /// <value>
                /// The profile_sidebar_border_color.
                /// </value>
                public string profile_sidebar_border_color { get; set; }
                /// <summary>
                /// Gets or sets the profile_sidebar_fill_color.
                /// </summary>
                /// <value>
                /// The profile_sidebar_fill_color.
                /// </value>
                public string profile_sidebar_fill_color { get; set; }
                /// <summary>
                /// Gets or sets the profile_text_color.
                /// </summary>
                /// <value>
                /// The profile_text_color.
                /// </value>
                public string profile_text_color { get; set; }
                /// <summary>
                /// Gets or sets a value indicating whether this <see cref="RootObject"/> is profile_use_background_image.
                /// </summary>
                /// <value>
                /// <c>true</c> if profile_use_background_image; otherwise, <c>false</c>.
                /// </value>
                public bool profile_use_background_image { get; set; }
                /// <summary>
                /// Gets or sets a value indicating whether this <see cref="RootObject"/> is protected.
                /// </summary>
                /// <value>
                ///   <c>true</c> if protected; otherwise, <c>false</c>.
                /// </value>
                public bool @protected { get; set; }
                /// <summary>
                /// Gets or sets the screen_name.
                /// </summary>
                /// <value>
                /// The screen_name.
                /// </value>
                public string screen_name { get; set; }
                /// <summary>
                /// Gets or sets a value indicating whether this <see cref="RootObject"/> is show_all_inline_media.
                /// </summary>
                /// <value>
                ///   <c>true</c> if show_all_inline_media; otherwise, <c>false</c>.
                /// </value>
                public bool show_all_inline_media { get; set; }
                /// <summary>
                /// Gets or sets the status.
                /// </summary>
                /// <value>
                /// The status.
                /// </value>
                public Status status { get; set; }
                /// <summary>
                /// Gets or sets the statuses_count.
                /// </summary>
                /// <value>
                /// The statuses_count.
                /// </value>
                public int statuses_count { get; set; }
                /// <summary>
                /// Gets or sets the time_zone.
                /// </summary>
                /// <value>
                /// The time_zone.
                /// </value>
                public string time_zone { get; set; }
                /// <summary>
                /// Gets or sets the URL.
                /// </summary>
                /// <value>
                /// The URL.
                /// </value>
                public object url { get; set; }
                /// <summary>
                /// Gets or sets the utc_offset.
                /// </summary>
                /// <value>
                /// The utc_offset.
                /// </value>
                public int utc_offset { get; set; }
                /// <summary>
                /// Gets or sets a value indicating whether this <see cref="RootObject"/> is verified.
                /// </summary>
                /// <value>
                ///   <c>true</c> if verified; otherwise, <c>false</c>.
                /// </value>
                public bool verified { get; set; }
            }
        }
    }
}