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
using Rock.Web.Cache;

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
            return ( this.Context as RockContext ).Set<PersonalizedEntity>()
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

            rockContext.Set<PersonalizedEntity>().AddRange( personalizedEntitiesToInsert );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Requests the meets criteria.
        /// </summary>
        /// <param name="requestFilterId">The request filter identifier.</param>
        /// <param name="request">The request.</param>
        /// <param name="site">The site.</param>
        /// <returns>
        ///   <c>true</c> if the request matches the filter, <c>false</c> otherwise.
        /// </returns>
        internal static bool RequestMeetsCriteria( int requestFilterId, Net.RockRequestContext request, SiteCache site )
        {
            var requestFilter = RequestFilterCache.Get( requestFilterId );
            var requestFilterConfiguration = requestFilter?.FilterConfiguration;
            if ( requestFilter == null || requestFilterConfiguration == null )
            {
                // somehow null so return false
                return false;
            }

            if ( requestFilter.SiteId.HasValue && site != null )
            {
                if ( requestFilter.SiteId.Value != site.Id )
                {
                    // this request filter is limited to a site other than the one we are on.
                    return false;
                }
            }

            /* All of these are AND'd so, if any are false we can return false */

            // Check against Previous Activity
            var previousActivityFilter = requestFilterConfiguration.PreviousActivityRequestFilter;
            if ( !previousActivityFilter.IsMatch( request ) )
            {
                return false;
            }

            // Check against Device Type
            var deviceTypeFilter = requestFilterConfiguration.DeviceTypeRequestFilter;
            if ( !deviceTypeFilter.IsMatch( request ) )
            {
                return false;
            }

            // Check against Query String
            if ( requestFilterConfiguration.QueryStringRequestFilters.Any() )
            {
                bool queryStringRequestFiltersMatch;
                if ( requestFilterConfiguration.QueryStringRequestFilterExpressionType == FilterExpressionType.GroupAll )
                {
                    queryStringRequestFiltersMatch = requestFilterConfiguration.QueryStringRequestFilters.All( a => a.IsMatch( request ) );
                }
                else
                {
                    queryStringRequestFiltersMatch = requestFilterConfiguration.QueryStringRequestFilters.Any( a => a.IsMatch( request ) );
                }

                if ( !queryStringRequestFiltersMatch )
                {
                    return false;
                }
            }

            // Check against Cookie values
            if ( requestFilterConfiguration.CookieRequestFilters.Any() )
            {
                bool cookieFiltersMatch;
                if ( requestFilterConfiguration.CookieRequestFilterExpressionType == FilterExpressionType.GroupAll )
                {
                    cookieFiltersMatch = requestFilterConfiguration.CookieRequestFilters.All( a => a.IsMatch( request ) );
                }
                else
                {
                    cookieFiltersMatch = requestFilterConfiguration.CookieRequestFilters.Any( a => a.IsMatch( request ) );
                }

                if ( !cookieFiltersMatch )
                {
                    return false;
                }
            }

            // Check against Browser type and version
            bool browserFiltersMatch = true;
            foreach ( var browserRequestFilter in requestFilterConfiguration.BrowserRequestFilters )
            {
                var isMatch = browserRequestFilter.IsMatch( request );
                browserFiltersMatch = browserFiltersMatch || isMatch;
            }

            if ( !browserFiltersMatch )
            {
                return false;
            }

            // Check based on IPAddress Range
            bool ipAddressFiltersMatch = true;
            foreach ( var ipAddressRequestFilter in requestFilterConfiguration.IPAddressRequestFilters )
            {
                var isMatch = ipAddressRequestFilter.IsMatch( request );
                ipAddressFiltersMatch = ipAddressFiltersMatch || isMatch;
            }

            if ( !ipAddressFiltersMatch )
            {
                return false;
            }

            // Check against Environment
            var environmentRequestFilter = requestFilterConfiguration.EnvironmentRequestFilter;
            if ( !environmentRequestFilter.IsMatch( request ) )
            {
                return false;
            }

            // if none of the filters return false, then return true;
            return true;
        }
    }
}