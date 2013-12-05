//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.ServiceModel.Channels;
using System.Security.Principal;

using Rock.Model;

namespace Rock.Rest.Filters
{
    public class AuthenticateAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization( HttpActionContext actionContext )
        {
            // See if user is logged in
            var principal = System.Threading.Thread.CurrentPrincipal;
            if ( principal != null && principal.Identity != null && !String.IsNullOrWhiteSpace(principal.Identity.Name))
            {
                var userLoginService = new UserLoginService();
                var user = userLoginService.GetByUserName(principal.Identity.Name);
                if ( user != null )
                {
                    actionContext.Request.SetUserPrincipal( principal );
                    return;
                }
            }

            // If not, see if there's a valid token
            string authToken = null;
            if (actionContext.Request.Headers.Contains("Authorization-Token"))
                authToken = actionContext.Request.Headers.GetValues( "Authorization-Token" ).FirstOrDefault();
            if ( String.IsNullOrWhiteSpace( authToken ) )
            {
                string queryString = actionContext.Request.RequestUri.Query;
                authToken = System.Web.HttpUtility.ParseQueryString(queryString).Get("apikey");
            }

            if (! String.IsNullOrWhiteSpace( authToken ) )
            {
                var userLoginService = new UserLoginService();
                var userLogin = userLoginService.Queryable().Where( u => u.ApiKey == authToken ).FirstOrDefault();
                if ( userLogin != null )
                {
                    var identity = new GenericIdentity( userLogin.UserName );
                    principal = new GenericPrincipal(identity, null);
                    actionContext.Request.SetUserPrincipal( principal );
                    return;
                }
            }
            actionContext.Response = actionContext.Request.CreateErrorResponse( HttpStatusCode.Unauthorized, "The Rock API requires that requests include either an Authorization-Token, and ApiKey querystring parameter, or are made by a logged-in user" ); 
        }
    }
}