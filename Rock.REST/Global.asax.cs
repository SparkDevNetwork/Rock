using System;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;

namespace Rock.REST
{
    public class Global : HttpApplication
    {
        void Application_Start( object sender, EventArgs e )
        {
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            // Add API Service routes
            RouteTable.Routes.MapPageRoute( "", "help", "~/RESTHelp.aspx" );
            new Rock.REST.ServiceHelper( this.Server.MapPath("~/Custom") ).AddRoutes( RouteTable.Routes );
        }
    }
}
