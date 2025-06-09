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
using System.Linq;

using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class LavaApplication
    {

        #region Security
        /// <summary>
        /// Gets the parent authority where security authorizations are being inherited from.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override ISecured ParentAuthority
        {
            get
            {
                // We intentionally want to break security inheritance to force the application to
                // explicitly declare cases where everyone should have access. Otherwise, they would
                // inherit the global default of "Can View".
                return null;
            }
        }

        /// <summary>
        /// Provides custom security authorization logic. 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="person"></param>
        /// <returns></returns>
        public override bool IsAuthorized( string action, Person person )
        {
            // If the person is a Rock Admin always allow access to View/Edit/Administrate (but not Execute)
            var isInOverrideRole = RoleCache.Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() ).IsPersonInRole( person.Guid )
                || RoleCache.Get( SystemGuid.Group.GROUP_LAVA_APPLICATION_DEVELOPERS.AsGuid() ).IsPersonInRole( person.Guid );

            if ( isInOverrideRole )
            {
                return true;
            }

            // Check to see if user is authorized using normal authorization rules
            return base.IsAuthorized( action, person );
        }
        #endregion

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return LavaApplicationCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            var cachedLavaEndpoints = LavaApplicationCache.Get( this.Id, ( RockContext ) dbContext )?.LavaEndpoints;
            if ( cachedLavaEndpoints?.Any() == true )
            {
                foreach ( var cachedDefinedValue in cachedLavaEndpoints )
                {
                    LavaEndpointCache.UpdateCachedEntity( cachedDefinedValue.Id, EntityState.Detached );
                }
            }

            LavaApplicationCache.UpdateCachedEntity( this.Id, entityState );
            LavaApplicationCache.FlushSlugCache();
        }

        #endregion
    }
}
