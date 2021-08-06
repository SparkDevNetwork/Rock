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
using System.Web.Http;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controls
{
    /// <summary>
    /// Common Entity Controller.
    /// This controller is NOT SECURED by default. DO NOT send sensitive data in responses.
    /// Common Entities are those that are frequently used on pages and should be shared among blocks.
    /// </summary>
    public class CommonEntitiesController : ControlsControllerBase
    {
        /// <summary>
        /// Gets the list of campuses.
        /// </summary>
        /// <returns></returns>
        [Authenticate]
        [HttpGet]
        [Route( "api/v2/CommonEntities/Campuses" )]
        public IEnumerable<object> GetCampuses()
        {
            var cacheItems = CampusCache.All();
            var viewModels = cacheItems.Select( c => new
            {
                Id = c.Id,
                Guid = c.Guid,
                Name = c.Name,
                IsActive = c.IsActive
            } );

            return viewModels;
        }

        /// <summary>
        /// Gets the list of defined types.
        /// </summary>
        /// <returns></returns>
        [Authenticate]
        [HttpGet]
        [Route( "api/v2/CommonEntities/DefinedTypes" )]
        public IEnumerable<object> GetDefinedTypes()
        {
            var cacheItems = DefinedTypeCache.All();
            var viewModels = cacheItems.Select( dt => new
            {
                Id = dt.Id,
                Guid = dt.Guid,
                Name = dt.Name,
                IsActive = dt.IsActive
            } );

            return viewModels;
        }
    }
}
