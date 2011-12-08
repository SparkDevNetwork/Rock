//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ServiceModel.Activation;
using System.Web.Routing;

public partial class RESTHelp : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        var routes = new List<RouteUrl>();

        foreach(object route in RouteTable.Routes)
            if ( route is ServiceRoute )
            {
                ServiceRoute serviceRoute = route as ServiceRoute;
                routes.Add( new RouteUrl( serviceRoute.Url.Replace( "/{*pathInfo}", string.Empty ) ) );
            }

        routes.Sort();
        gvRoutes.DataSource = routes;
        gvRoutes.DataBind();
    }

    protected class RouteUrl : IComparable
    {
        public string Url { get; set; }
        public RouteUrl( string url )
        {
            Url = url;
        }

        public int CompareTo( object obj )
        {
            return Url.CompareTo( ( ( RouteUrl )obj ).Url );
        }
    }
}

