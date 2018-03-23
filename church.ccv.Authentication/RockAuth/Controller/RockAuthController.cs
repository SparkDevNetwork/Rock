
using church.ccv.Authentication.RockAuth.Model;
using Newtonsoft.Json;
using Rock.Data;
using Rock.Model;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace church.ccv.Authentication.RockAuth
{
    public class RockAuthController : Rock.Rest.ApiControllerBase
    {
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route( "api/RockAuth/Login" )]
        public HttpResponseMessage Login( [FromBody]LoginData loginData )
        {
            LoginResponse loginResponse = Util.Login( loginData );
            
            // build and return the response
            StringContent restContent = new StringContent( loginResponse.ToString( ), Encoding.UTF8, "text/plain");
            HttpResponseMessage response = new HttpResponseMessage( ) { StatusCode = HttpStatusCode.OK, Content = restContent };
            return response;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route( "api/RockAuth/Logout" )]
        public HttpResponseMessage Logout( int? currPageId )
        {
            bool shouldRedirect = Util.Logout( Request.Headers.Referrer, currPageId );

            // build and return the response
            StringContent restContent = new StringContent( shouldRedirect.ToString( ), Encoding.UTF8, "text/plain" );
            HttpResponseMessage response = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = restContent };
            return response;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route( "api/RockAuth/SendConfirmAccountEmail" )]
        public HttpResponseMessage SendConfirmation( string confirmAccountUrl, string confirmAccountEmailTemplateGuid, string appUrl, string themeUrl, string username )
        {
            RockContext rockContext = new RockContext( );
            var userLoginService = new UserLoginService(rockContext);

            var userLogin = userLoginService.GetByUserName( username );
            if ( userLogin != null )
            {
                Util.SendConfirmAccountEmail( userLogin, confirmAccountUrl, confirmAccountEmailTemplateGuid, appUrl, themeUrl );
            }

            // build and return the response
            HttpResponseMessage response = new HttpResponseMessage( ) { StatusCode = HttpStatusCode.OK };
            return response;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route( "api/RockAuth/CheckDuplicates" )]
        public HttpResponseMessage CheckDuplicates( string lastName, string email )
        {
            // this will test to see if the given lastname and email are already associated with one or more people,
            // and return them if they are.
            List<DuplicatePersonInfo> duplicateList = Util.GetDuplicates( lastName, email );
            
            // return a list of duplicates, which will be empty if there weren't any
            StringContent restContent = new StringContent( JsonConvert.SerializeObject( duplicateList ), Encoding.UTF8, "application/json" );
            HttpResponseMessage response = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = restContent };
            return response;
        }
        
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route( "api/RockAuth/CreatePersonWithLogin" )]
        public HttpResponseMessage CreatePersonWithLogin( [FromBody]PersonWithLoginModel personWithLoginModel )
        {
            // creates a new person and user login FOR that person.
            bool success = Util.CreatePersonWithLogin( personWithLoginModel );

            // return OK, and whether we created their request or not
            StringContent restContent = new StringContent( success.ToString( ), Encoding.UTF8, "text/plain" );
            HttpResponseMessage response = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = restContent };
            return response;
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route( "api/RockAuth/CreateLogin" )]
        public HttpResponseMessage CreateLogin( [FromBody]CreateLoginModel createLoginModel )
        {
            // IF there is no existing login for the given person, an unconfirmed account will be created.
            // If the person already has accounts, we simply send a "forgot password" style email to the email of that person.
            StringContent restContent = null;
            CreateLoginModel.Response loginResponse = Util.CreateLogin( createLoginModel );
                        
            // return OK, and whether we created their request or not
            restContent = new StringContent( loginResponse.ToString( ), Encoding.UTF8, "text/plain" );
            HttpResponseMessage response = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = restContent };
            return response;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route( "api/RockAuth/SendForgotPasswordEmail" )]
        public HttpResponseMessage SendForgotPasswordEmail( string confirmAccountUrl, string forgotPasswordEmailTemplateGuid, string appUrl, string themeUrl, string personEmail )
        {
            // this will send a password reset email IF valid accounts are found tied to the email provided.
            Util.SendForgotPasswordEmail( personEmail, confirmAccountUrl, forgotPasswordEmailTemplateGuid, appUrl, themeUrl );

            HttpResponseMessage response = new HttpResponseMessage( ) { StatusCode = HttpStatusCode.OK };
            return response;
        }
    }
}
