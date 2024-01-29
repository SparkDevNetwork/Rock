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

using Rock.Data;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;
using System.Data.Entity;
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// AdaptiveMessageAdaptation Logic
    /// </summary>
    public partial class AdaptiveMessageAdaptation
    {
        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return AdaptiveMessageAdaptationCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            AdaptiveMessageAdaptationCache.UpdateCachedEntity( this.Id, entityState );
            AdaptiveMessageCache.FlushItem( this.AdaptiveMessageId );
        }

        #endregion
    }
}
