// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Routing;

namespace Rock
{
    /// <summary>
    /// Rock.Model.PageRoute extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region Route Extensions

        /// <summary>
        /// Returns a list of page Ids that match the route.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns></returns>
        public static List<int> PageIds( this Route route )
        {
            if ( route.DataTokens != null && route.DataTokens["PageRoutes"] != null )
            {
                var pages = route.DataTokens["PageRoutes"] as List<Rock.Web.PageAndRouteId>;
                if ( pages != null )
                {
                    return pages.Select( p => p.PageId ).ToList();
                }
            }

            return new List<int>();
        }

        /// <summary>
        /// Returns a list of route Ids that match the route.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns></returns>
        public static List<int> RouteIds( this Route route )
        {
            if ( route.DataTokens != null && route.DataTokens["PageRoutes"] != null )
            {
                var pages = route.DataTokens["PageRoutes"] as List<Rock.Web.PageAndRouteId>;
                if ( pages != null )
                {
                    return pages.Select( p => p.RouteId ).ToList();
                }
            }

            return new List<int>();
        }

        /// <summary>
        /// Adds the page route.
        /// </summary>
        /// <param name="routes">The routes.</param>
        /// <param name="routeName">Name of the route.</param>
        /// <param name="pageAndRouteIds">The page and route ids.</param>
        [RockObsolete( "1.9" )]
        [Obsolete( "Use the override without the Generic list instead." )]
        public static void AddPageRoute( this Collection<RouteBase> routes, string routeName, List<Rock.Web.PageAndRouteId> pageAndRouteIds)
        {
            Route route = new Route( routeName, new Rock.Web.RockRouteHandler() );
            route.DataTokens = new RouteValueDictionary();
            route.DataTokens.Add( "RouteName", routeName );
            route.DataTokens.Add( "PageRoutes", pageAndRouteIds );
            routes.Add( route );
        }

        /// <summary>
        /// Adds the page route. If the route name already exists then the PageAndRouteId obj will be added to the DataTokens "PageRoutes" List.
        /// </summary>
        /// <param name="routes">The routes.</param>
        /// <param name="routeName">Name of the route.</param>
        /// <param name="pageAndRouteId">The page and route identifier.</param>
        public static void AddPageRoute( this Collection<RouteBase> routes, string routeName, Rock.Web.PageAndRouteId pageAndRouteId)
        {
            Route route;
            List<Route> filteredRoutes = new List<Route>();

            // The list of Route Names being used is case sensitive but IIS's usage of them is not. This can cause problems when the same
            // route name is used on different Rock sites. In order for the correct route to be selected they must be group together.
            // So we need to check if an existing route has been created first and then add to the data tokens if it has.
            foreach( var rb in routes )
            {
                // Make sure this is a route
                if ( rb.GetType() == typeof( Route ) )
                {
                    Route r = rb as Route;
                    filteredRoutes.Add( r );
                }
            }

            if ( filteredRoutes.Where( r => string.Compare( r.Url, routeName, true ) == 0 ).Any() )
            {
                route = filteredRoutes.Where( r => string.Compare( r.Url, routeName, true ) == 0 ).First();

                var pageRoutes = ( List<Rock.Web.PageAndRouteId> ) route.DataTokens["PageRoutes"];
                if ( pageRoutes == null )
                {
                    route.DataTokens.Add( "PageRoutes", pageAndRouteId );
                }
                else
                {
                    pageRoutes.Add( pageAndRouteId );
                }
            }
            else
            {
                var pageRoutes = new List<Rock.Web.PageAndRouteId>();
                pageRoutes.Add( pageAndRouteId );

                route = new Route( routeName, new Rock.Web.RockRouteHandler() );
                route.DataTokens = new RouteValueDictionary();
                route.DataTokens.Add( "RouteName", routeName );
                route.DataTokens.Add( "PageRoutes", pageRoutes );
                routes.Add( route );
            }
        }
        #endregion Route Extensions
    }
}
