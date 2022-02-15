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
using System.Data.Entity;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class PageContext
    {
        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return null;
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            // PageCache has PageContexts that could get stale if PageContext is modified
            PageCache.UpdateCachedEntity( this.PageId, EntityState.Detached );
        }

        #endregion ICacheable

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the Entity (type name) and IdParamenter that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" />  containing the Entity (type name) and IdParamenter that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0}:{1}", this.Entity, this.IdParameter );
        }

        #endregion Methods
    }
}
