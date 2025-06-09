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

using Rock.Enums.Cms;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class LavaEndpoint
    {
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
                // Send the endpoint's parent application
                if ( this.LavaApplication != null)
                {
                    return this.LavaApplication;
                }

                // If we have the application id then send back a skeleton Lava application (only it's Id is needed to authorize it)
                return new LavaApplication { Id = this.LavaApplicationId };
            }
        }

        /// <inheritdoc/>
        public override bool IsAuthorized( string action, Person person )
        {
            // We have custom logic if we're trying to determine execute permission and we're configured
            // to read it off of the application.

            // Process the execute logic
            if ( action == Authorization.EXECUTE )
            {
                // If endpoint is handling it's own security use that.
                if ( this.SecurityMode == LavaEndpointSecurityMode.EndpointExecute )
                {
                    return base.IsAuthorized( action, person );
                }

                // If the endpoint is deferring to the application use the application logic.
                // If it's not default look to the application for direction
                if ( this.SecurityMode == LavaEndpointSecurityMode.ApplicationView )
                {
                    return this.LavaApplication.IsAuthorized( LavaApplication.EXECUTE_VIEW, person );
                }

                if ( this.SecurityMode == LavaEndpointSecurityMode.ApplicationEdit )
                {
                    return this.LavaApplication.IsAuthorized( LavaApplication.EXECUTE_EDIT, person );
                }

                if ( this.SecurityMode == LavaEndpointSecurityMode.ApplicationAdministrate )
                {
                    return this.LavaApplication.IsAuthorized( LavaApplication.EXECUTE_ADMINISTRATE, person );
                }
            }

            // If it's not EXECUTE then allow if they are in the override roles.
            var isInOverrideRole = RoleCache.Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() ).IsPersonInRole( person.Guid )
                                || RoleCache.Get( SystemGuid.Group.GROUP_LAVA_APPLICATION_DEVELOPERS.AsGuid() ).IsPersonInRole( person.Guid );

            if ( isInOverrideRole )
            {
                return true;
            }

            // Otherwise use default security.
            return base.IsAuthorized( action, person );
        }

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return LavaEndpointCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            LavaEndpointCache.UpdateCachedEntity( this.Id, entityState );

            // Flush the endpoints on the connected Lava Application cache.
            LavaApplicationCache.Get( this.LavaApplicationId )?.FlushEndpoints();
        }

        #endregion
    }
}
