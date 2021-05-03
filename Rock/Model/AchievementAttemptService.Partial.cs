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
using System.Data.Entity;
using System.Linq;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="AchievementAttempt"/> entity objects.
    /// </summary>
    public partial class AchievementAttemptService
    {
        /// <summary>
        /// Queries attempts by person identifier.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<AchievementAttempt> QueryByPersonId( int personId )
        {
            var rockContext = Context as RockContext;
            var personAliasService = new PersonAliasService( rockContext );
            var personEntityTypeId = EntityTypeCache.Get<Person>().Id;
            var personAliasEntityTypeId = EntityTypeCache.Get<PersonAlias>().Id;

            var personAliasIdQuery = personAliasService.Queryable()
                .AsNoTracking()
                .Where( pa => pa.PersonId == personId )
                .Select( pa => pa.Id );

            return Queryable().Where( aa =>
                (
                    aa.AchievementType.AchieverEntityTypeId == personEntityTypeId &&
                    aa.AchieverEntityId == personId
                ) ||
                (
                    aa.AchievementType.AchieverEntityTypeId == personAliasEntityTypeId &&
                    personAliasIdQuery.Contains( aa.AchieverEntityId )
                )
            );
        }

        /// <summary>
        /// Gets the ordered person attempts.
        /// </summary>
        /// <param name="attemptsQuery">The attempts query.</param>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="achieverEntityId">The achiever entity identifier.</param>
        /// <returns></returns>
        public List<AchievementAttempt> GetOrderedAchieverAttempts( IQueryable<AchievementAttempt> attemptsQuery, AchievementTypeCache achievementTypeCache, int achieverEntityId )
        {
            attemptsQuery = attemptsQuery.Where( aa => aa.AchievementTypeId == achievementTypeCache.Id );

            // If the achiever type is person alias we need to add all achievements of this type for that person.
            if ( EntityTypeCache.Get<PersonAlias>().Id == achievementTypeCache.AchieverEntityTypeId )
            {
                var personAliasService = new PersonAliasService( ( RockContext ) Context );
                var personAliasQuery = personAliasService
                    .Queryable()
                    .AsNoTracking()
                    .Where( pa => pa.Id == achieverEntityId )
                    .SelectMany( pa => pa.Person.Aliases )
                    .Select( pa => pa.Id );

                attemptsQuery = attemptsQuery
                    .Where( aa => personAliasQuery.Contains( aa.AchieverEntityId ) );
            }
            else
            {
                attemptsQuery = attemptsQuery
                    .Where( aa => aa.AchieverEntityId == achieverEntityId );
            }

            return attemptsQuery
                .OrderByDescending( saa => saa.AchievementAttemptStartDateTime )
                .ToList();
        }
    }
}