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

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Page
    {
        /// <summary>
        /// Gets the parent authority for the page. Page security is automatically inherited from the parent page, unless 
        /// explicitly overridden.  If there is no parent page, it is inherited from the site (through the layout)
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                if ( this.ParentPage != null )
                {
                    return this.ParentPage;
                }
                else if ( this.Layout != null && this.Layout.Site != null )
                {
                    return this.Layout.Site;
                }
                else
                {
                    return base.ParentAuthority;
                }
            }
        }

        /// <summary>
        /// Gets the cache control header. This shouldn't be used to set the properties directly but the json version should be used to set the CacheControlHeaderSettings property.
        /// </summary>
        /// <value>
        /// The cache control header.
        /// </value>
        [NotMapped]
        public RockCacheability CacheControlHeader
        {
            get
            {
                if ( _cacheControlHeader == null && CacheControlHeaderSettings.IsNotNullOrWhiteSpace() )
                {
                    _cacheControlHeader = Newtonsoft.Json.JsonConvert.DeserializeObject<RockCacheability>( CacheControlHeaderSettings );
                }

                return _cacheControlHeader;
            }
        }

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return PageCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            PageCache.UpdateCachedEntity( this.Id, entityState );

            if ( this.ParentPageId.HasValue )
            {
                PageCache.UpdateCachedEntity( this.ParentPageId.Value, EntityState.Detached );
            }

            if ( _originalParentPageId.HasValue && _originalParentPageId != this.ParentPageId )
            {
                PageCache.UpdateCachedEntity( _originalParentPageId.Value, EntityState.Detached );
            }
        }

        #endregion ICacheable
    }
}
