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
using System.Data.Entity;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for the <see cref="Rock.Model.PageRoute"/> class.
    /// </summary>
    public partial class PageRouteService
    {
        /// <summary>
        /// Gets an enumerable list of <see cref="Rock.Model.PageRoute"/> entities that are linked to a <see cref="Rock.Model.Page"/> by the 
        /// by the <see cref="Rock.Model.Page">Page's</see> Id.
        /// </summary>
        /// <param name="pageId">An <see cref="System.Int32"/> value containing the Id of the <see cref="Rock.Model.Page"/> .</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.PageRoute"/> entities that reference the supplied PageId.</returns>
        public IQueryable<PageRoute> GetByPageId( int pageId )
        {
            return Queryable().Where( t => t.PageId == pageId );
        }

        /// <summary>
        /// Registers the routes in Model.PageRoute to IIS's routing table.
        /// This is called at application start and can be run again if the routes
        /// somehow get out of sync.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public static void RegisterRoutes()
        {
            var routes = System.Web.Routing.RouteTable.Routes;
            using ( routes.GetWriteLock() )
            {
                routes.Clear();

                PageRouteService pageRouteService = new PageRouteService( new RockContext() );

                // Add ingore rule for asp.net ScriptManager files. 
                routes.Ignore( "{resource}.axd/{*pathInfo}" );

                var pageRoutes = pageRouteService
                    .Queryable()
                    .AsNoTracking()
                    .GroupBy( r => r.Route )
                    .Select( s => new
                    {
                        Name = s.Key,
                        Pages = s.Select( pr => new Rock.Web.PageAndRouteId { PageId = pr.PageId, RouteId = pr.Id } ).ToList()
                    } )
                    .ToList();

                // Add page routes
                foreach ( var route in pageRoutes )
                {
                    routes.AddPageRoute( route.Name, route.Pages );
                }

                // Add a default page route
                routes.Add( new System.Web.Routing.Route( "page/{PageId}", new Rock.Web.RockRouteHandler() ) );

                // Add a default route for when no parameters are passed
                routes.Add( new System.Web.Routing.Route( "", new Rock.Web.RockRouteHandler() ) );

                // Add a default route for shortlinks
                routes.Add( new System.Web.Routing.Route( "{shortlink}", new Rock.Web.RockRouteHandler() ) );
            }
        }
    }
}
