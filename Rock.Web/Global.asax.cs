using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Linq;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

using Rock.Cms;
using Rock.Cms.Security;
using Rock.Models.Cms;
using Rock.Services.Cms;
using Rock.Web.WebServices;

namespace Rock.Web
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start( object sender, EventArgs e )
        {
            RegisterRoutes( RouteTable.Routes );
            Authorization.Load();
        }

        protected void Session_Start( object sender, EventArgs e )
        {

        }

        protected void Application_BeginRequest( object sender, EventArgs e )
        {
            Context.Items.Add( "Request_Start_Time", DateTime.Now );
        }

        protected void Application_AuthenticateRequest( object sender, EventArgs e )
        {

        }

        protected void Application_Error( object sender, EventArgs e )
        {

        }

        protected void Session_End( object sender, EventArgs e )
        {

        }

        protected void Application_End( object sender, EventArgs e )
        {

        }

        private void RegisterRoutes( RouteCollection routes )
        {
            PageRouteService pageRouteService = new PageRouteService();

            // find each page that has defined a custom routes.
            foreach ( PageRoute pageRoute in pageRouteService.Queryable())
            {
                // Create the custom route and save the page id in the DataTokens collection
                Route route = new Route( pageRoute.Route, new RockRouteHandler() );
                route.DataTokens = new RouteValueDictionary();
                route.DataTokens.Add( "PageId", pageRoute.PageId.ToString() );
                routes.Add( route );
            }

            var factory = new DataServiceHostFactory();
            routes.Add(new ServiceRoute("api", factory, typeof (RockDataService)));

            // Add a default page route
            routes.Add( new Route( "page/{PageId}", new RockRouteHandler() ) );

            // Add a default route for when no parameters are passed
            routes.Add( new Route( "", new RockRouteHandler() ) );
        }

    }
}