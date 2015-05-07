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
    /// Authenticates a user using Google
    /// </summary>
    [Description("Google Authentication Provider")]
    [Export(typeof(AuthenticationComponent))]
    [ExportMetadata("ComponentName", "Google")]

    [TextField("Client ID", "The Google Client ID")]
    [TextField("Client Secret", "The Google Client Secret")]
    
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
            return ( !String.IsNullOrWhiteSpace(request.QueryString["code"]) &&
                !String.IsNullOrWhiteSpace(request.QueryString["state"]) );
        }

        /// <summary>
        /// Generates the login URL.
        /// </summary>
        /// <param name="request">Forming the URL to obtain user consent</param>
        /// <returns></returns>
        public override Uri GenerateLoginUrl( HttpRequest request )
        {
            string returnUrl = request.QueryString["returnurl"];
            string redirectUri = GetRedirectUrl(request);

            return new Uri(string.Format("https://accounts.google.com/o/oauth2/auth?response_type=code&client_id={0}&redirect_uri={1}&state={2}&scope=email profile",
                GetAttributeValue("ClientID"),
                HttpUtility.UrlEncode(redirectUri),
                HttpUtility.UrlEncode(returnUrl ?? FormsAuthentication.DefaultUrl)));
        }

        ///<summary>
        ///JSON Class for Access Token Response
        ///</summary>
        public class accesstokenresponse
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
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
            string redirectUri = GetRedirectUrl(request);

            try
            {
                // Get a new OAuth Access Token for the 'code' that was returned from the Google user consent redirect
                var restClient = new RestClient(
                    string.Format("https://www.googleapis.com/oauth2/v3/token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}&grant_type=authorization_code",
                        GetAttributeValue("ClientID"),
                        HttpUtility.UrlEncode(redirectUri),
                        GetAttributeValue("ClientSecret"),
                        request.QueryString["code"]));
                var restRequest = new RestRequest(Method.POST);
                var restResponse = restClient.Execute(restRequest);

                if ( restResponse.StatusCode == HttpStatusCode.OK )
                {
                    var accesstokenresponse = JsonConvert.DeserializeObject<accesstokenresponse>(restResponse.Content);
                    string accessToken = accesstokenresponse.access_token;

                    // Get information about the person who logged in using Google
                    restRequest = new RestRequest(Method.GET);
                    restRequest.AddParameter("access_token", accessToken);
                    restRequest.AddParameter("fields", "id,hd,email,family_name,gender,given_name");
                    restRequest.AddParameter("key", GetAttributeValue("ClientID"));
                    restRequest.RequestFormat = DataFormat.Json;
                    restRequest.AddHeader("Accept", "application/json");
                    restClient = new RestClient("https://www.googleapis.com/oauth2/v2/userinfo");
                    restResponse = restClient.Execute(restRequest);

                    if ( restResponse.StatusCode == HttpStatusCode.OK )
                    {
                        GoogleUser googleUser = JsonConvert.DeserializeObject<GoogleUser>(restResponse.Content);
                        username = GetGoogleUser(googleUser, accessToken);
                    }
                }
            }

            catch ( Exception ex )
            {
                ExceptionLogService.LogException(ex, HttpContext.Current);
            }

            return !string.IsNullOrWhiteSpace(username);
        }

        /// <summary>
        /// Gets the URL of an image that should be displayed.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override String ImageUrl( )
        {
            return ""; /*~/Assets/Images/facebook-login.png*/
        }

        private string GetRedirectUrl( HttpRequest request )
        {
            Uri uri = new Uri(request.Url.ToString());
            return uri.Scheme + "://" + uri.GetComponents(UriComponents.HostAndPort, UriFormat.UriEscaped) + uri.LocalPath;
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
        /// Google User Object
        /// </summary>
        public class GoogleUser
        {
            public string family_name { get; set; }
            public string name { get; set; }
            public string picture { get; set; }
            public string locale { get; set; }
            public string gender { get; set; }
            public string email { get; set; }
            public string link { get; set; }
            public string given_name { get; set; }
            public string id { get; set; }
            public string hd { get; set; }
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
            string username = string.Empty;
            string googleId = googleUser.id;
            string googleLink = googleUser.link;

            string userName = "Google_" + googleId;
            UserLogin user = null;

            using (var rockContext = new RockContext() )
            {

                // Query for an existing user 
                var userLoginService = new UserLoginService(rockContext);
                user = userLoginService.GetByUserName(userName);

                // If no user was found, see if we can find a match in the person table
                if ( user == null )
                    {
                    // Get name/email from Google login
                    string lastName = googleUser.family_name.ToString();
                    string firstName = googleUser.given_name.ToString();
                    string email = string.Empty;
                    try { email = googleUser.email.ToString(); }
                    catch { }

                    Person person = null;

                    // If person had an email, get the first person with the same name and email address.
                    if ( !string.IsNullOrWhiteSpace(email) )
                    {
                        var personService = new PersonService(rockContext);
                        var people = personService.GetByMatch(firstName, lastName, email);
                        if ( people.Count() == 1 )
                        {
                            person = people.First();
                        }
                    }

                    var personRecordTypeId = DefinedValueCache.Read(SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid()).Id;
                    var personStatusPending = DefinedValueCache.Read(SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid()).Id;

                    rockContext.WrapTransaction(( ) =>
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
                                if ( googleUser.gender.ToString() == "male" )
                                {
                                    person.Gender = Gender.Male;
                                }
                                else if ( googleUser.gender.ToString() == "female" )
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
                                PersonService.SaveNewPerson(person, rockContext, null, false);
                            }
                        }

                        if ( person != null )
                        {
                            int typeId = EntityTypeCache.Read(typeof(Google)).Id;
                            user = UserLoginService.Create(rockContext, person, AuthenticationServiceType.External, typeId, userName, "goog", true);
                        }

                    });
                }
                if ( user != null )
                {
                    username = user.UserName;

                    if ( user.PersonId.HasValue )
                    {
                        var converter = new ExpandoObjectConverter();

                        var personService = new PersonService(rockContext);
                        var person = personService.Get(user.PersonId.Value);
                    }
                }

                return username;
            }
        }
    }
}