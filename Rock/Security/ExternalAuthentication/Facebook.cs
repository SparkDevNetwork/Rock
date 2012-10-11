//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;

using Facebook;

using Rock.Core;
using Rock.Cms;
using Rock.Crm;

namespace Rock.Security.ExternalAuthentication
{
    /// <summary>
    /// Authenticates a user using Facebook
    /// </summary>
    [Description( "Facebook Authentication Provider" )]
    [Export( typeof( ExternalAuthenticationComponent ) )]
    [ExportMetadata( "ComponentName", "Facebook" )]
    [Rock.Attribute.Property( 1, "App ID", "Facebook", "The Facebook App ID", true, "" )]
    [Rock.Attribute.Property( 2, "App Secret", "Faceboook", "The Facebook App Secret", true, "" )]
    public class Facebook : ExternalAuthenticationComponent
    {
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
            parameters.client_id = AttributeValue( "AppID" );
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
                    parameters.client_id = AttributeValue( "AppID" );
                    parameters.client_secret = AttributeValue( "AppSecret" );
                    parameters.redirect_uri = redirectUri.AbsoluteUri; 
                    parameters.code = oAuthResult.Code;

                    dynamic result = fbClient.Post( "oauth/access_token", parameters );

                    string accessToken = result.access_token;

                    fbClient = new FacebookClient( accessToken );
                    dynamic me = fbClient.Get( "me" );
                    string facebookId = "FACEBOOK_" + me.id.ToString();

                    // query for matching id in the user table 
                    UserService userService = new UserService();
                    var user = userService.GetByUserName( facebookId );

                    // if not user was found see if we can find a match in the person table
                    if ( user == null )
                    {
                        try
                        {
                            // determine if we can find a match and if so add an user login record

                            // get properties from Facebook dynamic object
                            string lastName = me.last_name.ToString();
                            string firstName = me.first_name.ToString();
                            string email = me.email.ToString();

                            var personService = new PersonService();
                            var person = personService.Queryable().FirstOrDefault( u => u.LastName == lastName && ( u.GivenName == firstName || u.NickName == firstName ) && u.Email == email );

                            if ( person != null )
                            {
                                // since we have the data enter the birthday from Facebook to the db if we don't have it yet
                                DateTime birthdate = Convert.ToDateTime( me.birthday.ToString() );

                                if ( person.BirthDay == null )
                                {
                                    person.BirthDate = birthdate;
                                    personService.Save( person, person.Id );
                                }

                            }
                            else
                            {

                                var dvService = new DefinedValueService();

                                person = new Person();
                                person.IsSystem = false;
                                person.RecordTypeId = dvService.GetIdByGuid( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON );
                                person.RecordStatusId = dvService.GetIdByGuid( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE );

                                person.GivenName = me.first_name.ToString();
                                person.LastName = me.last_name.ToString();
                                person.Email = me.email.ToString();

                                if ( me.gender.ToString() == "male" )
                                    person.Gender = Gender.Male;
                                else if ( me.gender.ToString() == "female" )
                                    person.Gender = Gender.Female;
                                else
                                    person.Gender = Gender.Unknown;

                                person.BirthDate = Convert.ToDateTime( me.birthday.ToString() );
                                person.DoNotEmail = false;

                                personService.Add( person, null );
                                personService.Save( person, null );
                            }

                            user = userService.Create( person, AuthenticationServiceType.External, this.GetType().FullName, facebookId, "fb", true, person.Id );
                        }
                        catch ( Exception ex )
                        {
                            string msg = ex.Message;
                            // TODO: probably should report something...
                        }

                        // TODO: Show label indicating inability to find user corresponding to facebook id
                    }

                    username = user.UserName;
                    returnUrl = oAuthResult.State;
                    return true;

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
            return "~/Assets/Images/facebook-login.png";
        }

        private string GetRedirectUrl( HttpRequest request )
        {
            Uri uri = new Uri( request.Url.ToString() );
            return uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + uri.LocalPath;
        }
    }
}