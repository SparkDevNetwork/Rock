// <copyright>
// Copyright by the Spark Development Network
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
using System.IO;
using System.Net.Http;
using System.Text;
using Rock.Model;
using Newtonsoft.Json;
using System.Net;
using System.Web.Http;
using Rock.Security;
using Rock.Data;
using System.Collections.Generic;
using System.Linq;
using Rock.Rest.Filters;
using RestSharp;
using Rock.Communication;
using System;
using Rock.Web.Cache;
using System.Web.Configuration;
using church.ccv.Web.Model;
using church.ccv.Web.Data;

namespace church.ccv.Web.Rest
{
    public class WebController : Rock.Rest.ApiControllerBase
    {
        public bool AuthenticateRequest( )
        {
            //CompilationSection compilationSection = (CompilationSection)System.Configuration.ConfigurationManager.GetSection(@"system.web/compilation");

            // ensure that the request comes only from known and trusted domains. (or anything if we're running in debug)
            //string internalAppUrl = GlobalAttributesCache.Value( "InternalApplicationRoot" );

            // convert this to a URI and compare our trusted host with the request's host
            //Uri applicationUri = new Uri( internalAppUrl );

            //if( System.Web.HttpContext.Current.Request.UserHostAddress == "::1" )
            //if ( Request.Headers.Host.StartsWith( applicationUri.Host, true, System.Globalization.CultureInfo.CurrentCulture ) || compilationSection.Debug == true )
            {
                return true;
            }

            //return false;
        }

        /// <summary>
        /// This should only be called by Rock Blocks.
        /// See "RockWeb\Plugins\church_ccv\Core\Login.ascx" for an example.
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route( "api/Web/Login" )]
        public HttpResponseMessage Login( [FromBody]LoginData loginData )
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;
            LoginResponse loginResponse = LoginResponse.Invalid;

            if( AuthenticateRequest( ) )
            {   
                RockContext rockContext = new RockContext( );
                var userLoginService = new UserLoginService(rockContext);

                var userLogin = userLoginService.GetByUserName( loginData.Username );
                if ( userLogin != null && userLogin.EntityType != null)
                {
                    var component = AuthenticationContainer.GetComponent(userLogin.EntityType.Name);
                    if (component != null && component.IsActive && !component.RequiresRemoteAuthentication)
                    {
                        // see if the credentials are valid
                        if ( component.Authenticate( userLogin, loginData.Password ) )
                        {
                            // if the account isn't locked or needing confirmation
                            if ( ( userLogin.IsConfirmed ?? true ) && !(userLogin.IsLockedOut ?? false ) )
                            {
                                // then proceed to the final step, validating them with PMG2's site
                                if ( TryPMG2Login( loginData.Username, loginData.Password) )
                                {
                                    // generate their cookie
                                    UserLoginService.UpdateLastLogin( loginData.Username );
                                    Rock.Security.Authorization.SetAuthCookie( loginData.Username, bool.Parse( loginData.Persist ), false );

                                    // no issues!
                                    loginResponse = LoginResponse.Success;
                                }
                            }
                            else
                            {
                                if ( userLogin.IsLockedOut ?? false )
                                {
                                    loginResponse = LoginResponse.LockedOut;
                                }
                                else
                                {
                                    loginResponse = LoginResponse.Confirm;
                                }
                            }
                        }
                    }
                }
            }

            // build and return the response
            StringContent restContent = new StringContent( loginResponse.ToString( ), Encoding.UTF8, "text/plain");
            HttpResponseMessage response = new HttpResponseMessage( ) { StatusCode = statusCode, Content = restContent };

            return response;
        }

        protected bool TryPMG2Login( string username, string password )
        {
            // contact PMG2's site and attempt to login with the same credentials
            string pmg2RootSite = GlobalAttributesCache.Value( "PMG2Server" );
            
            var restClient = new RestClient(
                string.Format( pmg2RootSite + "auth?user[username]={0}&user[password]={1}", username, password ) );

            var restRequest = new RestRequest( Method.POST );
            var restResponse = restClient.Execute( restRequest );

            if ( restResponse.StatusCode == HttpStatusCode.Created )
            {
                return true;
            }

            return false;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route( "api/Web/SendConfirmationEmail" )]
        public void SendConfirmation( string confirmAccountUrl, string confirmAccountEmailTemplateGuid, string appUrl, string themeUrl, string username )
        {
            if ( AuthenticateRequest( ) )
            {
                RockContext rockContext = new RockContext( );
                var userLoginService = new UserLoginService(rockContext);

                var userLogin = userLoginService.GetByUserName( username );
                if ( userLogin != null )
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null );
                    mergeFields.Add( "ConfirmAccountUrl", confirmAccountUrl );

                    var personDictionary = userLogin.Person.ToLiquid() as Dictionary<string, object>;
                    mergeFields.Add( "Person", personDictionary );
                    mergeFields.Add( "User", userLogin );

                    var recipients = new List<RecipientData>();
                    recipients.Add( new RecipientData( userLogin.Person.Email, mergeFields ) );

                    Email.Send( new Guid( confirmAccountEmailTemplateGuid ), recipients, appUrl, themeUrl, false );
                }
            }
        }
        
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route( "api/Web/RegisterNewUser" )]
        public HttpResponseMessage RegisterNewUser( [FromBody]RegAccountData regAccountData )
        {
            StringContent restContent = null;
            RegisterResponseData registrationResponse = new RegisterResponseData( );

            if ( AuthenticateRequest( ) )
            {
                // first, see if there's already a person with this matching last name and email
                PersonService personService = new PersonService( new RockContext() );
                var matches = personService.Queryable().Where( p =>
                        p.Email.ToLower() == regAccountData.Email.ToLower() && p.LastName.ToLower() == regAccountData.LastName.ToLower() ).ToList();

                // we have matches, so we need to have the user decide what to do
                if ( matches.Count > 0 )
                {
                    registrationResponse.RegisterStatus = RegisterResponseData.Status.Duplicates.ToString();

                    registrationResponse.Duplicates = new List<DuplicatePersonInfo>( );
                    foreach ( Person match in matches )
                    {
                        DuplicatePersonInfo duplicateInfo = new DuplicatePersonInfo( )
                        {
                            Id = match.Id,
                            FullName = match.FullName,
                            Gender = match.Gender.ToString( ),
                            Birthday = match.BirthDay + " " + match.BirthMonth.ToString( )
                        };

                        registrationResponse.Duplicates.Add( duplicateInfo );
                    }
                }
                else
                {
                    Person newPerson;
                    UserLogin newLogin;
                    bool result = WebUtil.RegisterNewPerson( regAccountData, out newPerson, out newLogin );

                    if ( result )
                    {
                        // email them confirmation
                        var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null );
                        mergeObjects.Add( "ConfirmAccountUrl", regAccountData.ConfirmAccountUrl );
                        mergeObjects.Add( "Person", newPerson );
                        mergeObjects.Add( "User", newLogin );

                        var recipients = new List<RecipientData>();
                        recipients.Add( new RecipientData( newPerson.Email, mergeObjects ) );

                        Email.Send( new Guid( regAccountData.AccountCreatedEmailTemplateGuid ), recipients, regAccountData.AppUrl, regAccountData.ThemeUrl, false );

                        registrationResponse.RegisterStatus = RegisterResponseData.Status.Created.ToString( );
                    }
                    else
                    {
                        // if there was an error for any reason, let the requester know they should contact CCV for help
                        registrationResponse.RegisterStatus = RegisterResponseData.Status.Help.ToString( );
                    }
                }
            }
            
            restContent = new StringContent( JsonConvert.SerializeObject( registrationResponse ), Encoding.UTF8, "application/json" );

            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = restContent
            };
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route( "api/Web/SendForgotPasswordEmail" )]
        public HttpResponseMessage SendForgotPasswordEmail( string confirmAccountUrl, string forgotPasswordEmailTemplateGuid, string appUrlWithRoot, string themeUrlWithRoot, string personEmail )
        {
            // this will send a password reset email IF valid accounts are found tied to the email provided.
            if( AuthenticateRequest( ) )
            {
                // setup merge fields
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null );
                mergeFields.Add( "ConfirmAccountUrl", confirmAccountUrl );
                var results = new List<IDictionary<string, object>>();

                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );
                var userLoginService = new UserLoginService( rockContext );

                // get all the accounts associated with the person(s) using this email address
                bool hasAccountWithPasswordResetAbility = false;
                List<string> accountTypes = new List<string>();
                
                foreach ( Person person in personService.GetByEmail( personEmail )
                    .Where( p => p.Users.Any()))
                {
                    var users = new List<UserLogin>();
                    foreach ( UserLogin user in userLoginService.GetByPersonId( person.Id ) )
                    {
                        if ( user.EntityType != null )
                        {
                            var component = AuthenticationContainer.GetComponent( user.EntityType.Name );
                            if ( !component.RequiresRemoteAuthentication )
                            {
                                users.Add( user );
                                hasAccountWithPasswordResetAbility = true;
                            }

                            accountTypes.Add( user.EntityType.FriendlyName );
                        }
                    }

                    var resultsDictionary = new Dictionary<string, object>();
                    resultsDictionary.Add( "Person", person);
                    resultsDictionary.Add( "Users", users );
                    results.Add( resultsDictionary );
                }

                // if we found user accounts that were valid, send the email
                if ( results.Count > 0 && hasAccountWithPasswordResetAbility )
                {
                    mergeFields.Add( "Results", results.ToArray() );
                    var recipients = new List<RecipientData>();
                    recipients.Add( new RecipientData( personEmail, mergeFields ) );

                    Email.Send( new Guid( forgotPasswordEmailTemplateGuid ), recipients, appUrlWithRoot, themeUrlWithRoot, false );
                }
            }

            HttpResponseMessage response = new HttpResponseMessage( ) { StatusCode = HttpStatusCode.OK };
            return response;
        }
    }
}
