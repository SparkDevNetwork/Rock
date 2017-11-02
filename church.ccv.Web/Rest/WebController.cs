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
        /// </summary>
        public enum LoginResponse
        {
            Invalid,
            LockedOut,
            Confirm,
            Success
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route( "api/Web/Login" )]
        public HttpResponseMessage Login( string username, string password, bool persist )
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;
            StringContent restContent = null;
         
            if( AuthenticateRequest( ) )
            {   
                LoginResponse loginResponse = LoginResponse.Invalid;

                RockContext rockContext = new RockContext( );
                var userLoginService = new UserLoginService(rockContext);

                var userLogin = userLoginService.GetByUserName( username );
                if ( userLogin != null && userLogin.EntityType != null)
                {
                    var component = AuthenticationContainer.GetComponent(userLogin.EntityType.Name);
                    if (component != null && component.IsActive && !component.RequiresRemoteAuthentication)
                    {
                        // see if the credentials are valid
                        if ( component.Authenticate( userLogin, password ) )
                        {
                            // if the account isn't locked or needing confirmation
                            if ( ( userLogin.IsConfirmed ?? true ) && !(userLogin.IsLockedOut ?? false ) )
                            {
                                // then proceed to the final step, validating them with PMG2's site
                                if ( TryPMG2Login( username, password ) )
                                {
                                    // generate their cookie
                                    UserLoginService.UpdateLastLogin( username );
                                    Rock.Security.Authorization.SetAuthCookie( username, persist, false );

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

                restContent = new StringContent( loginResponse.ToString( ), Encoding.UTF8, "text/plain");
            }

            // build and return the response
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
        public void SendConfirmation( string confirmationUrl, string confirmAccountTemplate, string appRoot, string themeRoot, string username )
        {
            if ( AuthenticateRequest( ) )
            {
                RockContext rockContext = new RockContext( );
                var userLoginService = new UserLoginService(rockContext);

                var userLogin = userLoginService.GetByUserName( username );
                if ( userLogin != null )
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null );
                    mergeFields.Add( "ConfirmAccountUrl", confirmationUrl );

                    var personDictionary = userLogin.Person.ToLiquid() as Dictionary<string, object>;
                    mergeFields.Add( "Person", personDictionary );
                    mergeFields.Add( "User", userLogin );

                    var recipients = new List<RecipientData>();
                    recipients.Add( new RecipientData( userLogin.Person.Email, mergeFields ) );

                    Email.Send( new Guid( confirmAccountTemplate ), recipients, appRoot, themeRoot, false );
                }
            }
        }
    }
}
