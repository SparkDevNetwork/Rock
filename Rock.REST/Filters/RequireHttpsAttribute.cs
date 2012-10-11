using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Rock.Rest.Filters
    
    public class RequireHttpsAttribute : ActionFilterAttribute
        
        public override void OnActionExecuting( HttpActionContext actionContext )
            
            if (!String.Equals(actionContext.Request.RequestUri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
                actionContext.Response = actionContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "HTTPS Required");
        }
    }
}