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
using System.Data;
using System.Linq;
using System.Web.Http;
using Rock.Data;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Obsidian.Controllers.CommonEntities
{
    /// <summary>
    /// Abstract Cached Model Controller.
    /// This controller is NOT SECURED by default. DO NOT send sensitive data in responses.
    /// </summary>
    public abstract class CachedModelController<TCache, TModel> : ObsidianController
        where TCache : ModelCache<TCache, TModel>, new()
        where TModel : Model<TModel>, new()
    {
        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <returns></returns>
        protected virtual object GetViewModel( TCache cache ) {
            return cache;
        }

        /// <summary>
        /// Gets the list of cached entities.
        /// </summary>
        /// <param name="IsActive">if set to <c>true</c> [is active].</param>
        /// <returns></returns>
        [Authenticate]
        [HttpGet]
        public IEnumerable<object> GetList()
        {
            var cacheItems = ModelCache<TCache, TModel>.All();
            var viewModels = cacheItems.Select( GetViewModel );
            return viewModels;
        }
    }
}
