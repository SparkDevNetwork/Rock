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
using System.Linq;
using System.Web;
using System.Web.Security;

using Facebook;

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
            var returnUrl = request.QueryString["returnurl"];
            var redirectUri = new Uri( GetRedirectUrl( request ));

            dynamic parameters = new ExpandoObject();
            parameters.client_id = GetAttributeValue( "AppID" );
            parameters.display = "popup";
            parameters.redirect_uri = redirectUri.AbsoluteUri;
            parameters.scope = "user_birthday,email,read_stream,read_friendlists";
            parameters.state = returnUrl ?? FormsAuthentication.DefaultUrl;


            var fbClient = new FacebookClient();
            return fbClient.GetLoginUrl(parameters);
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
            var fbClient = new FacebookClient();
            FacebookOAuthResult oAuthResult;

            if ( fbClient.TryParseOAuthCallbackUrl( request.Url, out oAuthResult ) && oAuthResult.IsSuccess )
            {
                try
                {
                    var redirectUri = new Uri( GetRedirectUrl( request ) );

                    dynamic parameters = new ExpandoObject();
                    parameters.client_id = GetAttributeValue( "AppID" );
                    parameters.client_secret = GetAttributeValue( "AppSecret" );
                    parameters.redirect_uri = redirectUri.AbsoluteUri; 
                    parameters.code = oAuthResult.Code;

                    dynamic result = fbClient.Post( "oauth/access_token", parameters );

                    string accessToken = result.access_token;

                    fbClient = new FacebookClient( accessToken );
                    dynamic me = fbClient.Get( "me" );
                    string facebookId = "FACEBOOK_" + me.id.ToString();

                    UserLogin user = null;

                    var rockContext = new RockContext();
                    rockContext.WrapTransaction( () =>
                    {
                        // query for matching id in the user table 
                        var userLoginService = new UserLoginService( rockContext );
                        user = userLoginService.GetByUserName( facebookId );

                        // if no user was found see if we can find a match in the person table
                        if ( user == null )
                        {
                            try
                            {

                                var familyChanges = new List<string>();
                                var familyMemberChanges = new List<string>();
                                var PersonChanges = new List<string>();

                                // determine if we can find a match and if so add an user login record

                                // get properties from Facebook dynamic object
                                string lastName = me.last_name.ToString();
                                string firstName = me.first_name.ToString();
                                string email = me.email.ToString();

                                var personService = new PersonService( rockContext );
                                var person = personService.Queryable( "Aliases" ).FirstOrDefault( u => u.LastName == lastName && u.FirstName == firstName && u.Email == email );

                                if ( person != null )
                                {
                                    // since we have the data enter the birthday from Facebook to the db if we don't have it yet
                                    DateTime birthdate = Convert.ToDateTime( me.birthday.ToString() );

                                    if ( person.BirthDay == null )
                                    {
                                        History.EvaluateChange( PersonChanges, "Birth Date", person.BirthDate, person.BirthDate );
                                        person.BirthDate = birthdate;

                                        rockContext.SaveChanges();
                                        HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                                            person.Id, PersonChanges );
                                    }
                                }
                                else
                                {
                                    person = new Person();
                                    person.IsSystem = false;
                                    person.RecordTypeValueId = DefinedValueCache.Read( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                                    person.RecordStatusValueId = DefinedValueCache.Read( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() ).Id;
                                    person.FirstName = me.first_name.ToString();
                                    person.LastName = me.last_name.ToString();
                                    person.Email = me.email.ToString();
                                    if ( me.gender.ToString() == "male" )
                                        person.Gender = Gender.Male;
                                    else if ( me.gender.ToString() == "female" )
                                        person.Gender = Gender.Female;
                                    else
                                        person.Gender = Gender.Unknown;
                                    person.BirthDate = Convert.ToDateTime( me.birthday.ToString() );
                                    person.EmailPreference = EmailPreference.EmailAllowed;

                                    GroupService.SaveNewFamily( rockContext, person, null, false );
                                }

                                user = UserLoginService.Create( rockContext, person, AuthenticationServiceType.External, this.TypeId, facebookId, "fb", true );

                            }
                            catch ( Exception ex )
                            {
                                string msg = ex.Message;
                                // TODO: probably should report something...
                            }
                        }
                        else
                        {
                            // TODO: Show label indicating inability to find user corresponding to facebook id
                        }
                    } );

                    if ( user != null )
                    {
                        username = user.UserName;
                        returnUrl = oAuthResult.State;
                        return true;
                    }
                    else
                    {
                        username = string.Empty;
                        returnUrl = string.Empty;
                        return false;
                    }

                }
                catch ( FacebookOAuthException oae )
                {
                    string msg = oae.Message;
                    // TODO: Add error handeling
                    // Error validating verification code. (usually from wrong return url very picky with formatting)
                    // Error validating client secret.
                    // Error validating application.
                }
            }

            username = null;
            returnUrl = null;
            return false;
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
    }
}