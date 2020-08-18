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

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// AchievementTypes REST API
    /// </summary>
    public partial class AchievementTypesController
    {
        /// <summary>
        /// Gets the progress for the person.
        /// </summary>
        /// <param name="personId">The person identifier. The current person is used if this is omitted.</param>
        /// <param name="includeOnlyEligible">Include only progress statements for achievement types that have no unmet prerequisites</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpGet]
        [RockObsolete( "1.12" )]
        [Obsolete( "Use api/AchievementTypes/Progress instead" )]
        [System.Web.Http.Route( "api/StreakTypeAchievementTypes/Progress" )]
        public virtual List<ProgressStatement> GetProgressForPerson( [FromUri] int personId = default, [FromUri] bool includeOnlyEligible = default )
        {
            var rockContext = Service.Context as RockContext;
            var personAliasId = default( int );

            // If not specified, use the current person id
            if ( personId != default )
            {
                var personAliasService = new PersonAliasService( rockContext );
                personAliasId = personAliasService.Queryable().AsNoTracking().FirstOrDefault( pa => pa.PersonId == personId )?.Id ?? default;

                if ( personAliasId == default )
                {
                    var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, $"The personAliasId for the person with id {personId} did not resolve" );
                    throw new HttpResponseException( errorResponse );
                }
            }

            var personAliasEntityTypeId = EntityTypeCache.Get<PersonAlias>().Id;
            return GetProgressForAchiever( personAliasEntityTypeId, personAliasId, includeOnlyEligible );
        }

        /// <summary>
        /// Gets the progress for the achiever.
        /// </summary>
        /// <param name="achieverEntityTypeId">The achiever entity type identifier.</param>
        /// <param name="achieverEntityId">The achiever identifier. The current person is used if this is omitted.</param>
        /// <param name="includeOnlyEligible">Include only progress statements for achievement types that have no unmet prerequisites</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException">
        /// </exception>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/AchievementTypes/Progress" )]
        public virtual List<ProgressStatement> GetProgressForAchiever( [FromUri]int achieverEntityTypeId, [FromUri]int achieverEntityId = default, [FromUri]bool includeOnlyEligible = default )
        {
            var rockContext = Service.Context as RockContext;
            var isPerson = achieverEntityTypeId == EntityTypeCache.Get<Person>().Id;
            var isPersonAlias = achieverEntityTypeId == EntityTypeCache.Get<PersonAlias>().Id;

            // If not specified, use the current person
            if ( achieverEntityId == default && isPerson )
            {
                achieverEntityId = GetPerson( rockContext )?.Id ?? default;

                if ( achieverEntityId == default )
                {
                    var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "The person Id for the current user did not resolve" );
                    throw new HttpResponseException( errorResponse );
                }
            }

            if ( achieverEntityId == default && isPersonAlias )
            {
                achieverEntityId = GetPersonAliasId( rockContext ) ?? default;

                if ( achieverEntityId == default )
                {
                    var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "The person alias Id for the current user did not resolve" );
                    throw new HttpResponseException( errorResponse );
                }
            }

            if ( achieverEntityId == default )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "The achiever entity id could not be resolved" );
                throw new HttpResponseException( errorResponse );
            }

            var achievementTypeService = Service as AchievementTypeService;
            var progressStatements = achievementTypeService.GetProgressStatements( achieverEntityTypeId, achieverEntityId );

            if ( includeOnlyEligible )
            {
                progressStatements = progressStatements.Where( ps => !ps.UnmetPrerequisites.Any() ).ToList();
            }

            return progressStatements;
        }

        /// <summary>
        /// Gets the badge data for the achiever.
        /// </summary>
        /// <param name="achievementTypeId">The achievement type identifier.</param>
        /// <param name="achieverEntityId">The achiever identifier. The current person is used if this is omitted.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException">
        /// </exception>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/AchievementTypes/{achievementTypeId}/BadgeData" )]
        public virtual BadgeData GetBadgeData( int achievementTypeId, [FromUri] int? achieverEntityId = null )
        {
            var rockContext = Service.Context as RockContext;
            var achievementType = AchievementTypeCache.Get( achievementTypeId );

            if ( achievementType == null )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.NotFound, "The achievement type did not resolve" );
                throw new HttpResponseException( errorResponse );
            }

            var isPerson = achievementType.AchieverEntityTypeId == EntityTypeCache.Get<Person>().Id;
            var isPersonAlias = achievementType.AchieverEntityTypeId == EntityTypeCache.Get<PersonAlias>().Id;

            // If not specified, use the current person
            if ( !achieverEntityId.HasValue && isPerson )
            {
                achieverEntityId = GetPerson( rockContext )?.Id;

                if ( !achieverEntityId.HasValue )
                {
                    var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "The person Id for the current user did not resolve" );
                    throw new HttpResponseException( errorResponse );
                }
            }

            if ( !achieverEntityId.HasValue && isPersonAlias )
            {
                achieverEntityId = GetPersonAliasId( rockContext );

                if ( !achieverEntityId.HasValue )
                {
                    var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "The person alias Id for the current user did not resolve" );
                    throw new HttpResponseException( errorResponse );
                }
            }

            if ( !achieverEntityId.HasValue )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "The achiever entity id could not be resolved" );
                throw new HttpResponseException( errorResponse );
            }

            var achievementTypeService = Service as AchievementTypeService;
            var markup = achievementTypeService.GetBadgeMarkup( achievementType, achieverEntityId.Value );

            return new BadgeData
            {
                AchievementTypeName = achievementType.Name,
                BadgeMarkup = markup
            };
        }

        /// <summary>
        /// <see cref="GetBadgeData(int, int?)"/> Response
        /// </summary>
        public class BadgeData
        {
            /// <summary>
            /// Gets or sets the name of the achievement type.
            /// </summary>
            /// <value>
            /// The name of the achievement type.
            /// </value>
            public string AchievementTypeName { get; set; }

            /// <summary>
            /// Gets or sets the badge markup.
            /// </summary>
            /// <value>
            /// The badge markup.
            /// </value>
            public string BadgeMarkup { get; set; }
        }
    }
}