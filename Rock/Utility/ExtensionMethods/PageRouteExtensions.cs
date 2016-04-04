﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.ObjectModel;
using System.Web.Routing;
using Rock.Model;

namespace Rock
{
    /// <summary>
    /// Rock.Model.PageRoute extensions
    /// </summary>
    public static partial class ExtensionMethods
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
