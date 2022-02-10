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
using System.Linq;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Site
    {
        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return SiteCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            SiteCache.UpdateCachedEntity( this.Id, entityState );

            foreach ( int pageId in new PageService( ( RockContext ) dbContext ).GetBySiteId( this.Id )
                    .Select( p => p.Id )
                    .ToList() )
            {
                PageCache.UpdateCachedEntity( pageId, EntityState.Detached );
            }
        }

        #endregion ICacheable

        /// <summary>
        /// Gets or sets the configuration mobile file path.
        /// </summary>
        /// <value>
        /// The configuration mobile file path.
        /// </value>
        [NotMapped]
        public string ConfigurationMobilePhoneFileUrl
        {
            get
            {
                return Site.GetFileUrl( this.ConfigurationMobilePhoneBinaryFileId );
            }

            private set
            {
            }
        }

        /// <summary>
        /// Gets or sets the configuration tablet file path.
        /// </summary>
        /// <value>
        /// The configuration tablet file path.
        /// </value>
        [NotMapped]
        public string ConfigurationTabletFileUrl
        {
            get
            {
                return Site.GetFileUrl( this.ConfigurationMobileTabletBinaryFileId );
            }

            private set
            {
            }
        }

        /// <summary>
        /// Gets the thumbnail file URL.
        /// </summary>
        /// <value>
        /// The thumbnail file URL.
        /// </value>
        [NotMapped]
        public string ThumbnailFileUrl
        {
            get
            {
                return Site.GetFileUrl( this.ThumbnailBinaryFileId );
            }

            private set
            {
            }
        }
    }
}
