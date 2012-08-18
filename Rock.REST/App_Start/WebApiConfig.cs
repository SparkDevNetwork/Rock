using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Rock.REST
{
    public static class WebApiConfig
    {
        public static void Register( HttpConfiguration config )
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
