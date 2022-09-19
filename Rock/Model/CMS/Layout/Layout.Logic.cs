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
    public partial class Layout
    {
        #region Navigation Properties
        /// <summary>
        /// Gets the parent authority for the layout. Layout security is automatically inherited from the site.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.Site != null ? this.Site : base.ParentAuthority;
            }
        }

        #endregion Navigation Properties

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// 
        /// 
        public IEntityCache GetCacheObject()
        {
            return LayoutCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            LayoutCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion ICacheable
    }
}
