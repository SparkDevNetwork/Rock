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
                ) );
        }

        /// <summary>
        /// Queries attempts by person alias ids
        /// </summary>
        /// <param name="personAliasIds">The person alias ids.</param>
        /// <returns></returns>
        public IQueryable<AchievementAttempt> QueryByPersonAliasIds( int[] personAliasIds )
        {
            var rockContext = Context as RockContext;
            var personAliasService = new PersonAliasService( rockContext );
            var personEntityTypeId = EntityTypeCache.Get<Person>().Id;
            var personAliasEntityTypeId = EntityTypeCache.Get<PersonAlias>().Id;

            var personIdQuery = personAliasService.Queryable()
                .AsNoTracking()
                .Where( pa => personAliasIds.Contains( pa.Id ) )
                .Select( pa => pa.PersonId );

            return Queryable().Where( aa =>
                (
                    aa.AchievementType.AchieverEntityTypeId == personEntityTypeId &&
                    personIdQuery.Contains( aa.AchieverEntityId )
                ) ||
                (
                    aa.AchievementType.AchieverEntityTypeId == personAliasEntityTypeId &&
                    personAliasIds.Contains( aa.AchieverEntityId )
                ) );
        }

        /// <summary>
        /// Returns a queryable of <see cref="AchievementAttemptWithPersonAlias"/> where <see cref="AchievementType.AchieverEntityTypeId"/> is a Person or PersonAlias EntityType
        /// </summary>
        /// <returns></returns>
        public IQueryable<AchievementAttemptWithPersonAlias> GetAchievementAttemptWithAchieverPersonAliasQuery()
        {
            int personAliasEntityTypeId = EntityTypeCache.Get<Rock.Model.PersonAlias>().Id;
            int personEntityTypeId = EntityTypeCache.Get<Rock.Model.Person>().Id;
            var personEntityQry = this.GetAchievementAttemptWithAchieverPersonAliasQuery( personEntityTypeId );
            var personAliasEntityQry = this.GetAchievementAttemptWithAchieverPersonAliasQuery( personAliasEntityTypeId );

            return personEntityQry.Union( personAliasEntityQry );
        }

        /// <summary>
        /// Includes the AchieverPersonAlias as Rock.Model.PersonAuse for AchievementType that are either a PersonAlias or Person entity type
        /// </summary>
        /// <param name="achieverEntityTypeId">The achiever entity type identifier.</param>
        /// <returns></returns>
        private IQueryable<AchievementAttemptWithPersonAlias> GetAchievementAttemptWithAchieverPersonAliasQuery( int achieverEntityTypeId )
        {
            var achievementAttemptQuery = Queryable().Where( a => a.AchievementType.AchieverEntityTypeId == achieverEntityTypeId );
            IQueryable<AchievementAttemptWithPersonAlias> achievementAttemptWithPersonQuery;

            int personAliasEntityTypeId = EntityTypeCache.Get<Rock.Model.PersonAlias>().Id;
            int personEntityTypeId = EntityTypeCache.Get<Rock.Model.Person>().Id;
            if ( achieverEntityTypeId == personAliasEntityTypeId )
            {
                var personAliasQry = new PersonAliasService( this.Context as RockContext ).Queryable();
                achievementAttemptWithPersonQuery = achievementAttemptQuery.Join(
                    personAliasQry,
                    a => a.AchieverEntityId,
                    pa => pa.Id,
                    ( a, pa ) => new AchievementAttemptWithPersonAlias
                    {
                        AchievementAttempt = a,
                        AchieverPersonAlias = pa
                    } );
            }
            else if ( achieverEntityTypeId == personEntityTypeId )
            {
                var personQry = new PersonService( this.Context as RockContext ).Queryable();
                achievementAttemptWithPersonQuery = achievementAttemptQuery.Join(
                    personQry,
                    a => a.AchieverEntityId,
                    p => p.Id,
                    ( a, p ) => new AchievementAttemptWithPersonAlias
                    {
                        AchievementAttempt = a,
                        AchieverPersonAlias = p.Aliases.Where( pa => pa.AliasPersonId == pa.PersonId ).FirstOrDefault()
                    } );
            }
            else
            {
                return null;
            }

            return achievementAttemptWithPersonQuery;
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

        /// <summary>
        /// 
        /// </summary>
        public class AchievementAttemptWithPersonAlias
        {
            /// <summary>
            /// Gets or sets the achievement attempt.
            /// </summary>
            /// <value>
            /// The achievement attempt.
            /// </value>
            public AchievementAttempt AchievementAttempt { get; set; }

            /// <summary>
            /// Gets or sets the achiever person.
            /// </summary>
            /// <value>
            /// The achiever person.
            /// </value>
            public PersonAlias AchieverPersonAlias { get; set; }
        }
    }
}