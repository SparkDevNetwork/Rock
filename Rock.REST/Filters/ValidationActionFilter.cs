using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Rock.REST.Filters
{
    public class ValidationFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting( HttpActionContext actionContext )
        {
            if ( !actionContext.ModelState.IsValid )
                actionContext.Response = actionContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, actionContext.ModelState );
        }
    }
}