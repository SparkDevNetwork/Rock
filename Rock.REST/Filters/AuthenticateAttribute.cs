using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.ServiceModel.Channels;
using System.Security.Principal;

namespace Rock.Rest.Filters
{
    public class AuthenticateAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization( HttpActionContext actionContext )
        {
            IPrincipal principal = actionContext.ControllerContext.Request.GetUserPrincipal();
            if ( principal != null )
            {
                actionContext.ControllerContext.Request.
            }

            base.OnAuthorization( actionContext );
        }
    }
}