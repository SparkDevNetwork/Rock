using System.Collections.ObjectModel;
using System.Web.Routing;
using Rock.Model;

namespace Rock
{
    /// <summary>
    /// Rock.Model.PageRoute extensions
    /// </summary>
    public static class PageRouteExtensions
    {
        #region Route Extensions

        /// <summary>
        /// Returns the page Id of the route or -1 if not found.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns></returns>
        public static int PageId( this Route route )
        {
            if ( route.DataTokens != null && route.DataTokens["PageId"] != null )
            {
                return ( route.DataTokens["PageId"] as string ).AsIntegerOrNull() ?? -1;
            }

            return -1;
        }

        /// <summary>
        /// Returns the route Id of the route or -1 if not found.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns></returns>
        public static int RouteId( this Route route )
        {
            if ( route.DataTokens != null && route.DataTokens["RouteId"] != null )
            {
                return ( route.DataTokens["RouteId"] as string ).AsIntegerOrNull() ?? -1;
            }

            return -1;
        }

        /// <summary>
        /// Adds the page route.
        /// </summary>
        /// <param name="routes">The routes.</param>
        /// <param name="pageRoute">The page route.</param>
        public static void AddPageRoute( this Collection<RouteBase> routes, PageRoute pageRoute )
        {
            Route route = new Route( pageRoute.Route, new Rock.Web.RockRouteHandler() );
            route.DataTokens = new RouteValueDictionary();
            route.DataTokens.Add( "PageId", pageRoute.PageId.ToString() );
            route.DataTokens.Add( "RouteId", pageRoute.Id.ToString() );
            routes.Add( route );
        }

        #endregion Route Extensions
    }
}
