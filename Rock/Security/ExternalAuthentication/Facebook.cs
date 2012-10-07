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
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using Facebook;
using Rock.Cms;
using Rock.Crm;

namespace Rock.Security.Authentication
{
    /// <summary>
    /// Authenticates a user using Facebook
    /// </summary>
    [Description( "Facebook Authentication Provider" )]
	[Export( typeof( ExternalAuthenticationComponent ) )]
    [ExportMetadata("ComponentName", "Facebook")]
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
		/// Gets the external url to redirect user to
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns></returns>
		public override string ExternalUrl( HttpRequest request )
		{
            var returnUrl = request.QueryString["returnurl"];
            var oAuthClient = new FacebookOAuthClient( FacebookApplication.Current ) { RedirectUri = new Uri( GetOAuthRedirectUrl(request) ) };
			oAuthClient.AppId = AttributeValue("AppID");
			oAuthClient.AppSecret = AttributeValue("AppSecret");

            // setup some facebook connection settings
            var settings = new Dictionary<string, object>
            {
                { "display", "popup" },
                { "scope", "user_birthday,email,read_stream,read_friendlists"},
                { "state", returnUrl ?? FormsAuthentication.DefaultUrl}
            };

            // Grab publically available information. No special permissions needed for authentication.
            var loginUri = oAuthClient.GetLoginUrl( settings );
			return loginUri.AbsoluteUri;
		}

		/// <summary>
		/// Authenticates the specified request.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="username">The username.</param>
		/// <returns></returns>
		public override Boolean Authenticate( HttpRequest request, out string username, out string returnUrl )
		{
			string code = request.QueryString["code"];
			string state = request.QueryString["state"];

			FacebookOAuthResult oAuthResult;

			if ( FacebookOAuthResult.TryParse( request.Url, out oAuthResult ) && oAuthResult.IsSuccess )
			{
				try
				{
					// create client to read response
					var oAuthClient = new FacebookOAuthClient( FacebookApplication.Current ) { RedirectUri = new Uri( GetOAuthRedirectUrl(request) ) };
					oAuthClient.AppId = AttributeValue("AppId");
					oAuthClient.AppSecret = AttributeValue("AppSecret");
					dynamic tokenResult = oAuthClient.ExchangeCodeForAccessToken( code );
					string accessToken = tokenResult.access_token;

					FacebookClient fbClient = new FacebookClient( accessToken );
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
								person = new Person();
								person.GivenName = me.first_name.ToString();
								person.LastName = me.last_name.ToString();
								person.Email = me.email.ToString();

								if ( me.gender.ToString() == "male" )
									person.Gender = Gender.Male;
								if ( me.gender.ToString() == "female" )
									person.Gender = Gender.Female;

								person.BirthDate = Convert.ToDateTime( me.birthday.ToString() );

								personService.Add( person, null );
								personService.Save( person, null );
							}

							user = userService.Create( person, AuthenticationType.Facebook, facebookId, "fb", true, person.Id );
						}
						catch ( Exception ex )
						{
							string msg = ex.Message;
							// TODO: probably should report something...
						}

						// TODO: Show label indicating inability to find user corresponding to facebook id
					}

					username = user.UserName;
					returnUrl = state;                
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

		private string GetOAuthRedirectUrl(HttpRequest request)
		{
			Uri uri = new Uri( request.Url.ToString() );
			return uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + uri.LocalPath;
		}
	}
}