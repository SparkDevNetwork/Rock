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
    public partial class RestAction
    {
        #region Properties

        private RockCacheability _cacheControlHeader;
        /// <summary>
        /// Gets the cache control header.
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

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.Controller != null ? this.Controller : base.ParentAuthority;
            }
        }

        #endregion
        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return RestActionCache.Get( this.ApiId );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            RestActionCache.UpdateCachedEntity( this.ApiId, entityState );
        }

        #endregion
    }
}
