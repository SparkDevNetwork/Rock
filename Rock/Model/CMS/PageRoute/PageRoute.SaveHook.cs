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
using System.Linq;
#if !NET5_0_OR_GREATER
using System.Web.Routing;
#endif

using Rock.Data;

namespace Rock.Model
{
    public partial class PageRoute
    {
        /// <summary>
        /// Save hook implementation for <see cref="PageRoute"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<PageRoute>
        {
            /// <summary>
            /// Method that will be called on an entity immediately before the item is saved by context.
            /// </summary>
            protected override void PreSave()
            {
                if ( State == EntityContextState.Deleted )
                {
#if !NET5_0_OR_GREATER
                    var routes = RouteTable.Routes;
                    if ( routes != null )
                    {
                        var existingRoute = RouteTable.Routes.OfType<Route>().FirstOrDefault( a => a.RouteIds().Contains( Entity.Id ) );
                        if ( existingRoute != null )
                        {
                            var pageAndRouteIds = existingRoute.DataTokens["PageRoutes"] as List<Rock.Web.PageAndRouteId>;
                            pageAndRouteIds = pageAndRouteIds.Where( p => p.RouteId != Entity.Id ).ToList();
                            if ( pageAndRouteIds.Any() )
                            {
                                existingRoute.DataTokens["PageRoutes"] = pageAndRouteIds;
                            }
                            else
                            {
                                RouteTable.Routes.Remove( existingRoute );
                            }
                        }
                    }
#endif
                }

                base.PreSave();
            }
        }
    }
}
