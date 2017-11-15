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
using System.Web.Http.Cors;

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
        
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route( "api/Web/Login" )]
        public HttpResponseMessage Login( [FromBody]LoginData loginData )
        {
            LoginResponse loginResponse = LoginResponse.Invalid;

            if( AuthenticateRequest( ) )
            {   
                loginResponse = WebUtil.Login( loginData );
            }

            // build and return the response
            StringContent restContent = new StringContent( loginResponse.ToString( ), Encoding.UTF8, "text/plain");

            HttpResponseMessage response = new HttpResponseMessage( ) { StatusCode = HttpStatusCode.OK, Content = restContent };
            return response;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route( "api/Web/SendConfirmAccountEmail" )]
        public HttpResponseMessage SendConfirmation( string confirmAccountUrl, string confirmAccountEmailTemplateGuid, string appUrl, string themeUrl, string username )
        {
            if ( AuthenticateRequest( ) )
            {
                RockContext rockContext = new RockContext( );
                var userLoginService = new UserLoginService(rockContext);

                var userLogin = userLoginService.GetByUserName( username );
                if ( userLogin != null )
                {
                    WebUtil.SendConfirmAccountEmail( userLogin, confirmAccountUrl, confirmAccountEmailTemplateGuid, appUrl, themeUrl );
                }
            }

            // build and return the response
            HttpResponseMessage response = new HttpResponseMessage( ) { StatusCode = HttpStatusCode.OK };
            return response;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route( "api/Web/CheckDuplicates" )]
        public HttpResponseMessage CheckDuplicates( string lastName, string email )
        {
            // this will test to see if the given lastname and email are already associated with one or more people,
            // and return them if they are.
            List<DuplicatePersonInfo> duplicateList = new List<DuplicatePersonInfo>( );

            if ( AuthenticateRequest( ) )
            {
                duplicateList = WebUtil.GetDuplicates( lastName, email );
            }

            // return a list of duplicates, which will be empty if there weren't any
            StringContent restContent = new StringContent( JsonConvert.SerializeObject( duplicateList ), Encoding.UTF8, "application/json" );

            HttpResponseMessage response = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = restContent };
            return response;
        }
        
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route( "api/Web/CreatePersonWithLogin" )]
        public HttpResponseMessage CreatePersonWithLogin( [FromBody]PersonWithLoginModel personWithLoginModel )
        {
            // creates a new person and user login FOR that person.
            bool success = false;

            if ( AuthenticateRequest( ) )
            {
                // create the new person
                success = WebUtil.CreatePersonWithLogin( personWithLoginModel );
            }
            
            // return OK, and whether we created their request or not
            StringContent restContent = new StringContent( success.ToString( ), Encoding.UTF8, "text/plain" );
            
            HttpResponseMessage response = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = restContent };
            return response;
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route( "api/Web/CreateLogin" )]
        public HttpResponseMessage CreateLogin( [FromBody]CreateLoginModel createLoginModel )
        {
            // IF there is no existing login for the given person, an unconfirmed account will be created.
            // If the person already has accounts, we simply send a "forgot password" style email to the email of that person.
            StringContent restContent = null;
            CreateLoginModel.Response loginResponse = CreateLoginModel.Response.Failed;

            if ( AuthenticateRequest( ) )
            {
                loginResponse = WebUtil.CreateLogin( createLoginModel );
            }
            
            // return OK, and whether we created their request or not
            restContent = new StringContent( loginResponse.ToString( ), Encoding.UTF8, "text/plain" );
            
            HttpResponseMessage response = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = restContent };

            return response;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route( "api/Web/SendForgotPasswordEmail" )]
        public HttpResponseMessage SendForgotPasswordEmail( string confirmAccountUrl, string forgotPasswordEmailTemplateGuid, string appUrl, string themeUrl, string personEmail )
        {
            // this will send a password reset email IF valid accounts are found tied to the email provided.
            if( AuthenticateRequest( ) )
            {
                WebUtil.SendForgotPasswordEmail( personEmail, confirmAccountUrl, forgotPasswordEmailTemplateGuid, appUrl, themeUrl );
            }

            HttpResponseMessage response = new HttpResponseMessage( ) { StatusCode = HttpStatusCode.OK };
            return response;
        }
    }
}
