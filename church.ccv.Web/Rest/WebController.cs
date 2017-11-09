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
using Rock;

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

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route( "api/Web/CheckDuplicates" )]
        public HttpResponseMessage CheckDuplicates( string lastName, string email )
        {
            // this will test to see if the given lastname and email are already associated with one or more people,
            // and return them if they are.
            StringContent restContent = null;
            List<DuplicatePersonInfo> duplicateList = new List<DuplicatePersonInfo>( );

            if ( AuthenticateRequest( ) )
            {
                // first, see if there's already a person with this matching last name and email
                PersonService personService = new PersonService( new RockContext() );
                var matches = personService.Queryable().Where( p =>
                        p.Email.ToLower() == email.ToLower() && p.LastName.ToLower() == lastName.ToLower() ).ToList();

                // add all duplicates to our list
                foreach ( Person match in matches )
                {
                    DuplicatePersonInfo duplicateInfo = new DuplicatePersonInfo( )
                    {
                        Id = match.Id,
                        FullName = match.FullName,
                        Gender = match.Gender.ToString( ),
                        Birthday = match.BirthDay + " " + match.BirthMonth.ToString( )
                    };

                    duplicateList.Add( duplicateInfo );
                }
            }

            // return a list of duplicates, which will be empty if there weren't any
            restContent = new StringContent( JsonConvert.SerializeObject( duplicateList ), Encoding.UTF8, "application/json" );

            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = restContent
            };
        }
        
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route( "api/Web/CreatePersonWithLogin" )]
        public HttpResponseMessage CreatePersonWithLogin( [FromBody]PersonWithLoginModel personWithLoginModel)
        {
            // creates a new person and user login FOR that person.
            StringContent restContent = null;
            bool success = false;

            if ( AuthenticateRequest( ) )
            {
                // create the new person
                Person newPerson;
                UserLogin newLogin;
                bool result = WebUtil.CreatePersonWithLogin( personWithLoginModel, out newPerson, out newLogin );

                if ( result )
                {
                    // email them confirmation
                    var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null );
                    mergeObjects.Add( "ConfirmAccountUrl", personWithLoginModel.ConfirmAccountUrl );
                    mergeObjects.Add( "Person", newPerson );
                    mergeObjects.Add( "User", newLogin );

                    var recipients = new List<RecipientData>();
                    recipients.Add( new RecipientData( newPerson.Email, mergeObjects ) );

                    Email.Send( new Guid( personWithLoginModel.AccountCreatedEmailTemplateGuid ), recipients, personWithLoginModel.AppUrl, personWithLoginModel.ThemeUrl, false );

                    success = true;
                }
            }
            
            // return OK, and whether we created their request or not
            restContent = new StringContent( success.ToString( ), Encoding.UTF8, "text/plain" );
            return new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = restContent };
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route( "api/Web/TryCreateLogin" )]
        public HttpResponseMessage TryCreateLogin( [FromBody]CreateLoginModel createLoginModel )
        {
            // IF there is no existing login for the given person, an unconfirmed account will be created.
            // If the person already has accounts, we simply send a "forgot password" style email to the email of that person.
            StringContent restContent = null;
            CreateLoginModel.Response response = CreateLoginModel.Response.Failed;

            if ( AuthenticateRequest( ) )
            {
                RockContext rockContext = new RockContext();

                // start by getting the person being worked on
                PersonService personService = new PersonService( rockContext );
                Person person = personService.Get( createLoginModel.PersonId );
                if ( person != null )
                {
                    // now, see if the person already has ANY logins attached
                    var userLoginService = new Rock.Model.UserLoginService( rockContext );
                    var userLogins = userLoginService.GetByPersonId( createLoginModel.PersonId ).ToList();
                    if ( userLogins.Count == 0 )
                    {
                        // and create a new, UNCONFIRMED login for them.
                        var newLogin = UserLoginService.Create(
                                        rockContext,
                                        person,
                                        Rock.Model.AuthenticationServiceType.Internal,
                                        EntityTypeCache.Read( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                                        createLoginModel.Username,
                                        createLoginModel.Password,
                                        false,
                                        false );

                        // send them an email asking them to confirm the account
                        var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null );
                        mergeObjects.Add( "ConfirmAccountUrl", createLoginModel.ConfirmAccountUrl );
                        mergeObjects.Add( "Person", person );
                        mergeObjects.Add( "User", newLogin );

                        var recipients = new List<RecipientData>();
                        recipients.Add( new RecipientData( person.Email, mergeObjects ) );

                        Email.Send( new Guid( createLoginModel.ConfirmAccountEmailTemplateGuid ), recipients, createLoginModel.AppUrlWithRoot, createLoginModel.ThemeUrlWithRoot, false );

                        response = CreateLoginModel.Response.Created;
                    }
                    else
                    {
                        // they DO have a login, so simply email the person being worked on a list of them.
                        response = CreateLoginModel.Response.Emailed;

                        WebUtil.SendForgotPasswordEmail( createLoginModel.ConfirmAccountUrl, createLoginModel.ForgotPasswordEmailTemplateGuid, createLoginModel.AppUrlWithRoot, createLoginModel.ThemeUrlWithRoot, person.Email );
                    }
                }
            }
            
            // return OK, and whether we created their request or not
            restContent = new StringContent( response.ToString( ), Encoding.UTF8, "text/plain" );
            return new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = restContent };
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route( "api/Web/SendForgotPasswordEmail" )]
        public HttpResponseMessage SendForgotPasswordEmail( string confirmAccountUrl, string forgotPasswordEmailTemplateGuid, string appUrlWithRoot, string themeUrlWithRoot, string personEmail )
        {
            // this will send a password reset email IF valid accounts are found tied to the email provided.
            if( AuthenticateRequest( ) )
            {
                WebUtil.SendForgotPasswordEmail( confirmAccountUrl, forgotPasswordEmailTemplateGuid, appUrlWithRoot, themeUrlWithRoot, personEmail );
            }

            HttpResponseMessage response = new HttpResponseMessage( ) { StatusCode = HttpStatusCode.OK };
            return response;
        }
    }
}
