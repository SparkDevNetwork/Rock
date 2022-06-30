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

using Rock.Personalization;
using Rock.Web.Cache;

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace Rock.Model
{
    public partial class RequestFilter
    {
        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return RequestFilterCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Data.DbContext dbContext )
        {
            RequestFilterCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion ICacheable

        #region PersonalizationRequestFilterConfiguration

        /// <summary>
        /// Gets or sets the configuration of the filters
        /// </summary>
        /// <value>The configuration.</value>
        [NotMapped]
        public virtual PersonalizationRequestFilterConfiguration FilterConfiguration
        {
            get
            {
                if ( FilterJson.IsNullOrWhiteSpace() )
                {
                    return new PersonalizationRequestFilterConfiguration();
                }

                return FilterJson.FromJsonOrNull<PersonalizationRequestFilterConfiguration>() ?? new PersonalizationRequestFilterConfiguration();
            }

            set
            {
                FilterJson = value?.ToJson();
            }
        }

        #endregion PersonalizationRequestFilterConfiguration
    }
}
