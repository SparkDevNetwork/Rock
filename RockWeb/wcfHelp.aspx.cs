using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Routing;

public partial class wcfHelp : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        List<RouteUrl> routes = new List<RouteUrl>();

        foreach(object route in RouteTable.Routes)
            if ( route is ServiceRoute )
            {
                ServiceRoute serviceRoute = route as ServiceRoute;
                routes.Add( new RouteUrl( serviceRoute.Url.Replace( "/{*pathInfo}", string.Empty ) ) );
            }

        gvRoutes.DataSource = routes;
        gvRoutes.DataBind();
    }

    protected class RouteUrl
    {
        public string Url { get; set; }
        public RouteUrl( string url )
        {
            Url = url;
        }
    }
}

