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

using System.Collections.Generic;
using System.Linq;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Class RequestFilter.
    /// </summary>
    public partial class RequestFilterService
    {
        /// <summary>
        /// Gets a Queryable of <see cref="PersonalizedEntity"/> that have a <see cref="PersonAliasPersonalization.PersonalizationType"/>
        /// of <see cref="PersonalizationType.RequestFilter"/>
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public IQueryable<PersonalizedEntity> GetPersonalizedEntityRequestFilterQuery( int entityTypeId, int entityId )
        {
            return ( this.Context as RockContext ).PersonalizedEntities
                .Where( a => a.PersonalizationType == PersonalizationType.RequestFilter && a.EntityTypeId == entityTypeId && a.EntityId == entityId );
        }

        /// <summary>
        /// Updates the data in <see cref="Rock.Model.PersonalizedEntity"/> table based on the specified request filters.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="requestFilterIds">The request filter ids.</param>
        public void UpdatePersonalizedEntityForRequestFilters( int entityTypeId, int entityId, List<int> requestFilterIds )
        {
            var rockContext = this.Context as RockContext;
            var personalizedEntities = GetPersonalizedEntityRequestFilterQuery( entityTypeId, entityId );
            // Delete personalizedEntities that are no longer in the segment Ids provided.
            var personalizedEntitiesToDelete = personalizedEntities.Where( a => !requestFilterIds.Contains( a.PersonalizationEntityId ) );
            var countRemovedFromPersonalizedEntities = rockContext.BulkDelete( personalizedEntitiesToDelete );

            // Add personalizationEntityIds that are new.
            var personalizedEntityIdsToAdd = requestFilterIds
                .Where( requestFilterId => !personalizedEntities.Any( pe => pe.PersonalizationEntityId == requestFilterId ) )
                .ToList();
            var personalizedEntitiesToInsert = personalizedEntityIdsToAdd.Distinct().Select( personalizationEntityId => new PersonalizedEntity
            {
                EntityId = entityId,
                EntityTypeId = entityTypeId,
                PersonalizationType = PersonalizationType.RequestFilter,
                PersonalizationEntityId = personalizationEntityId
            } ).ToList();

            rockContext.PersonalizedEntities.AddRange( personalizedEntitiesToInsert );
            rockContext.SaveChanges();
        }
    }
}