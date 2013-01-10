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

namespace Rock.Rest.Filters
{
    public class ValidateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting( HttpActionContext actionContext )
        {
            if ( !actionContext.ModelState.IsValid )
                actionContext.Response = actionContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, actionContext.ModelState );
        }
    }
}