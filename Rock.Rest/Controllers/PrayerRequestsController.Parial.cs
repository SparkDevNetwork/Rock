// <copyright>
// Copyright 2013 by the Spark Development Network
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
using System.Linq;
using System.Web.Http;
using Rock.Data;
using Rock.Model;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PrayerRequestsController : IHasCustomRoutes
    {
        /// <summary>
        /// Adds the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public void AddRoutes( System.Web.Routing.RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "GetByCategory",
                routeTemplate: "api/PrayerRequests/GetByCategory/{categoryId}",
                defaults: new
                {
                    controller = "PrayerRequests",
                    action = "GetByCategory"
                } );
        }

        /// <summary>
        /// Gets Prayer Requests for the specified top-level category
        /// Prayer Requests that are in categories that are decendents of the specified category will also be included
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Queryable]
        public IQueryable<PrayerRequest> GetByCategory( int categoryId )
        {
            var rockContext = ( this.Service.Context as RockContext ) ?? new RockContext();
            var decendentsCategoriesQry = new CategoryService( rockContext ).GetAllDescendents( categoryId ).Select( a => a.Id );
            return this.Get().Where( a => a.CategoryId.HasValue ).Where( a => decendentsCategoriesQry.Contains( a.CategoryId.Value ) || ( a.CategoryId.Value == categoryId ) );
        }
    }
}
