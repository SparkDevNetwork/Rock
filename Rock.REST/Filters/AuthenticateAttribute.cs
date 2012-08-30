using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.ServiceModel.Channels;
using System.Security.Principal;

using Rock.Cms;

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
				var userService = new UserService();
				var user = userService.GetByUserName(principal.Identity.Name);
				if ( user != null )
					return;
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
                var userService = new UserService();
                var user = userService.Queryable().Where( u => u.ApiKey == authToken ).FirstOrDefault();
				if ( user != null )
				{
					var identity = new GenericIdentity( user.UserName );
					principal = new GenericPrincipal(identity, null);
					actionContext.Request.SetUserPrincipal( principal );
					return;
				}
            }
            actionContext.Response = actionContext.Request.CreateErrorResponse( HttpStatusCode.Unauthorized, "The Rock API requires that requests include either an Authorization-Token, and ApiKey querystring parameter, or are made by a logged-in user" ); 
        }
    }
}