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
        public static void AddPageRoute( this Collection<RouteBase> routes, string routeName, List<Rock.Web.PageAndRouteId> pageAndRouteIds)
        {
            Route route = new Route( routeName, new Rock.Web.RockRouteHandler() );
            route.DataTokens = new RouteValueDictionary();
            route.DataTokens.Add( "PageRoutes", pageAndRouteIds );
            routes.Add( route );
        }

        #endregion Route Extensions
    }
}
