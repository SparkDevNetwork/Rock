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

using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class PersonalLinkSection
    {
        /// <summary>
        /// Return <c>true</c> if the user is authorized for <paramref name="action"/>.
        /// In the case of non-shared link section, security it limited to the person who owns that section.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns><c>true</c> if the specified action is authorized; otherwise, <c>false</c>.</returns>
        public override bool IsAuthorized( string action, Person person )
        {
            // if it is non-shared personal link, than only the person that owns the link is authorized for that link. Everybody else has NO access (including admins).
            if ( !this.IsShared && this.PersonAlias != null )
            {
                return this.PersonAlias.PersonId == person.Id;
            }

            return base.IsAuthorized( action, person );
        }

        #region ICacheable

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Data.DbContext dbContext )
        {
            if ( entityState == EntityState.Deleted )
            {
                // If the link section was deleted, the "ModifiedDateTime" of link orders need to be updated.
                // Otherwise, we won't be able to detect that the links have changed due to deleting a record.
                new PersonalLinkSectionOrderService( dbContext as RockContext ).UpdateLinkOrdersModifiedDateTime( PreSaveChangesPersonAliasId );
            }

            if ( PreSaveChangesIsShared || this.IsShared )
            {
                // If this is a shared section, update the SharedPersonalLinkSectionCache
                SharedPersonalLinkSectionCache.UpdateCachedEntity( this.Id, entityState );
                SharedPersonalLinkSectionCache.FlushLastModifiedDateTime();
            }

            // Since this change probably impacts the current person's links, flush the current person's link's ModifiedDateTime 
            PersonalLinkService.PersonalLinksHelper.FlushPersonalLinksSessionDataLastModifiedDateTime();
        }

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns>IEntityCache.</returns>
        public IEntityCache GetCacheObject()
        {
            return SharedPersonalLinkSectionCache.Get( this.Id );
        }

        #endregion ICacheable
    }
}
