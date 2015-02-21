// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Security.ExternalAuthentication
{
    /// <summary>
    /// Authenticates a user using Facebook
    /// </summary>
    [Description( "Facebook Authentication Provider" )]
    [Export( typeof( AuthenticationComponent ) )]
    [ExportMetadata( "ComponentName", "Facebook" )]

    [TextField( "App ID", "The Facebook App ID" )]
    [TextField( "App Secret", "The Facebook App Secret" )]
    [BooleanField( "Sync Friends", "Should the person's Facebook friends whe are also in Rock be added as a known relationship?", true )]
    public class Facebook : AuthenticationComponent
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
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public override Uri GenerateLoginUrl( HttpRequest request )
        {
            string returnUrl = request.QueryString["returnurl"];
            string redirectUri = GetRedirectUrl( request );

            return new Uri( string.Format( "https://www.facebook.com/dialog/oauth?client_id={0}&redirect_uri={1}&state={2}&scope=public_profile,email,user_friends",
                GetAttributeValue( "AppID" ),
                HttpUtility.UrlEncode( redirectUri ),
                HttpUtility.UrlEncode( returnUrl ?? FormsAuthentication.DefaultUrl ) ) );
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
                // Get a new Facebook Access Token for the 'code' that was returned from the Facebook login redirect
                var restClient = new RestClient(
                    string.Format( "https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}",
                        GetAttributeValue( "AppID" ),
                        HttpUtility.UrlEncode( redirectUri ),
                        GetAttributeValue( "AppSecret" ),
                        request.QueryString["code"] ) );
                var restRequest = new RestRequest( Method.GET );
                var restResponse = restClient.Execute( restRequest );

                if ( restResponse.StatusCode == HttpStatusCode.OK )
                {
                    string accessToken = HttpUtility.ParseQueryString( "?" + restResponse.Content )["access_token"];

                    // Get information about the person who logged in using Facebook
                    restRequest = new RestRequest( Method.GET );
                    restRequest.AddParameter( "access_token", accessToken );
                    restRequest.RequestFormat = DataFormat.Json;
                    restRequest.AddHeader( "Accept", "application/json" );
                    restClient = new RestClient( "https://graph.facebook.com/v2.2/me" );
                    restResponse = restClient.Execute( restRequest );

                    if ( restResponse.StatusCode == HttpStatusCode.OK )
                    {
                        FacebookUser facebookUser = JsonConvert.DeserializeObject<FacebookUser>( restResponse.Content );
                        username = GetFacebookUserName( facebookUser, GetAttributeValue( "SyncFriends" ).AsBoolean(), accessToken );
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
            return ""; /*~/Assets/Images/facebook-login.png*/
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
        /// Facebook User Object
        /// </summary>
        public class FacebookUser
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public string id { get; set; }

            /// <summary>
            /// Gets or sets the email.
            /// </summary>
            /// <value>
            /// The email.
            /// </value>
            public string email { get; set; }

            /// <summary>
            /// Gets or sets the first_name.
            /// </summary>
            /// <value>
            /// The first_name.
            /// </value>
            public string first_name { get; set; }

            /// <summary>
            /// Gets or sets the gender.
            /// </summary>
            /// <value>
            /// The gender.
            /// </value>
            public string gender { get; set; }

            /// <summary>
            /// Gets or sets the last_name.
            /// </summary>
            /// <value>
            /// The last_name.
            /// </value>
            public string last_name { get; set; }

            /// <summary>
            /// Gets or sets the link.
            /// </summary>
            /// <value>
            /// The link.
            /// </value>
            public string link { get; set; }

            /// <summary>
            /// Gets or sets the locale.
            /// </summary>
            /// <value>
            /// The locale.
            /// </value>
            public string locale { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string name { get; set; }

            /// <summary>
            /// Gets or sets the timezone.
            /// </summary>
            /// <value>
            /// The timezone.
            /// </value>
            public string timezone { get; set; }

            /// <summary>
            /// Gets or sets the updated_time.
            /// </summary>
            /// <value>
            /// The updated_time.
            /// </value>
            public string updated_time { get; set; }

            /// <summary>
            /// Gets or sets the verified.
            /// </summary>
            /// <value>
            /// The verified.
            /// </value>
            public string verified { get; set; }
        }

        /// <summary>
        /// Gets the name of the facebook user.
        /// </summary>
        /// <param name="facebookUser">The facebook user.</param>
        /// <param name="syncFriends">if set to <c>true</c> [synchronize friends].</param>
        /// <param name="accessToken">The access token.</param>
        /// <returns></returns>
        public static string GetFacebookUserName( FacebookUser facebookUser, bool syncFriends = false, string accessToken = "" )
        {
            string username = string.Empty;
            string facebookId = facebookUser.id;
            string facebookLink = facebookUser.link;

            string userName = "FACEBOOK_" + facebookId;
            UserLogin user = null;

            var rockContext = new RockContext();

            // Query for an existing user 
            var userLoginService = new UserLoginService( rockContext );
            user = userLoginService.GetByUserName( userName );

            // If no user was found, see if we can find a match in the person table
            if ( user == null )
            {
                // Get name/email from Facebook login
                string lastName = facebookUser.last_name.ToString();
                string firstName = facebookUser.first_name.ToString();
                string email = string.Empty;
                try { email = facebookUser.email.ToString(); }
                catch { }

                Person person = null;

                // If person had an email, get the first person with the same name and email address.
                if ( string.IsNullOrWhiteSpace( email ) )
                {
                    var personService = new PersonService( rockContext );
                    person = personService.Queryable( "Aliases" )
                        .FirstOrDefault( u =>
                            u.LastName == lastName &&
                            u.FirstName == firstName &&
                            u.Email == email );
                }

                rockContext.WrapTransaction( () =>
                {
                    if ( person == null )
                    {
                        person = new Person();
                        person.IsSystem = false;
                        person.RecordTypeValueId = DefinedValueCache.Read( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                        person.RecordStatusValueId = DefinedValueCache.Read( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() ).Id;
                        person.FirstName = firstName;
                        person.LastName = lastName;
                        person.Email = email;
                        person.IsEmailActive = true;
                        person.EmailPreference = EmailPreference.EmailAllowed;
                        try
                        {
                            if ( facebookUser.gender.ToString() == "male" )
                            {
                                person.Gender = Gender.Male;
                            }
                            else if ( facebookUser.gender.ToString() == "female" )
                            {
                                person.Gender = Gender.Female;
                            }
                            else
                            {
                                person.Gender = Gender.Unknown;
                            }
                        }
                        catch { }

                        if ( person != null )
                        {
                            PersonService.SaveNewPerson( person, rockContext, null, false );
                        }
                    }

                    if ( person != null )
                    {
                        int typeId = EntityTypeCache.Read( typeof( Facebook ) ).Id;
                        user = UserLoginService.Create( rockContext, person, AuthenticationServiceType.External, typeId, userName, "fb", true );
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
                        // If person does not have a photo, try to get their Facebook photo
                        if ( !person.PhotoId.HasValue )
                        {
                            var restClient = new RestClient( string.Format( "https://graph.facebook.com/v2.2/{0}/picture?redirect=false&type=square&height=400&width=400", facebookId ) );
                            var restRequest = new RestRequest( Method.GET );
                            restRequest.RequestFormat = DataFormat.Json;
                            restRequest.AddHeader( "Accept", "application/json" );
                            var restResponse = restClient.Execute( restRequest );
                            if ( restResponse.StatusCode == HttpStatusCode.OK )
                            {
                                dynamic picData = JsonConvert.DeserializeObject<ExpandoObject>( restResponse.Content, converter );
                                bool isSilhouette = picData.data.is_silhouette;
                                string url = picData.data.url;

                                // If Facebook returned a photo url
                                if ( !isSilhouette && !string.IsNullOrWhiteSpace( url ) )
                                {
                                    // Download the photo from the url provided
                                    restClient = new RestClient( url );
                                    restRequest = new RestRequest( Method.GET );
                                    restResponse = restClient.Execute( restRequest );
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
                                            binaryFile.ContentStream = new MemoryStream( bytes );

                                            rockContext.SaveChanges();

                                            person.PhotoId = binaryFile.Id;
                                            rockContext.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }

                        // Save the facebook social media link
                        var facebookAttribute = AttributeCache.Read( Rock.SystemGuid.Attribute.PERSON_FACEBOOK.AsGuid(), rockContext );
                        if ( facebookAttribute != null )
                        {
                            person.LoadAttributes( rockContext );
                            person.SetAttributeValue( facebookAttribute.Key, facebookLink );
                            person.SaveAttributeValues( rockContext );
                        }

                        if ( syncFriends && !string.IsNullOrWhiteSpace( accessToken ) )
                        {
                            // Get the friend list (only includes friends who have also authorized this app)
                            var restRequest = new RestRequest( Method.GET );
                            restRequest.AddParameter( "access_token", accessToken );
                            restRequest.RequestFormat = DataFormat.Json;
                            restRequest.AddHeader( "Accept", "application/json" );

                            var restClient = new RestClient( string.Format( "https://graph.facebook.com/v2.2/{0}/friends", facebookId ) );
                            var restResponse = restClient.Execute( restRequest );

                            if ( restResponse.StatusCode == HttpStatusCode.OK )
                            {
                                // Get a list of the facebook ids for each friend
                                dynamic friends = JsonConvert.DeserializeObject<ExpandoObject>( restResponse.Content, converter );
                                var facebookIds = new List<string>();
                                foreach ( var friend in friends.data )
                                {
                                    facebookIds.Add( friend.id );
                                }

                                // Queue a transaction to add/remove friend relationships in Rock
                                var transaction = new Rock.Transactions.UpdateFacebookFriends( person.Id, facebookIds );
                                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
                            }
                        }
                    }
                }
            }

            return username;
        }
    }
}