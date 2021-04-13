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
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Achievement
{
    /// <summary>
    /// Streak Sourced Achievement Component
    /// </summary>
    /// <seealso cref="Rock.Achievement.AchievementComponent" />
    public abstract class StreakSourcedAchievementComponent : AchievementComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AchievementComponent" /> class.
        /// </summary>
        /// <param name="streakTypeAttributeKey">The streak type attribute key.</param>
        public StreakSourcedAchievementComponent( string streakTypeAttributeKey ) : base(
            new AchievementConfiguration( typeof( Streak ), typeof( PersonAlias ) ),
            new HashSet<string> { streakTypeAttributeKey } )
        {
            StreakTypeAttributeKey = streakTypeAttributeKey;
        }

        #region Helpers

        /// <summary>
        /// Gets the streak type unique identifier.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <returns></returns>
        protected Guid? GetStreakTypeGuid( AchievementTypeCache achievementTypeCache )
        {
            return GetAttributeValue( achievementTypeCache, StreakTypeAttributeKey ).AsGuidOrNull();
        }

        /// <summary>
        /// Gets the streak type cache.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <returns></returns>
        protected StreakTypeCache GetStreakTypeCache( AchievementTypeCache achievementTypeCache )
        {
            var streakTypeGuid = GetStreakTypeGuid( achievementTypeCache );
            return streakTypeGuid.HasValue ? StreakTypeCache.Get( streakTypeGuid.Value ) : null;
        }

        #endregion Helpers

        #region Base Component Overrides

        /// <summary>
        /// Processes the specified achievement type cache for the source entity.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="achievementTypeCache">The streak type achievement type cache.</param>
        /// <param name="sourceEntity">The source entity.</param>
        /// <returns>The set of attempts that were created or updated</returns>
        public override HashSet<AchievementAttempt> Process( RockContext rockContext, AchievementTypeCache achievementTypeCache, IEntity sourceEntity )
        {
            var streak = sourceEntity as Streak;
            var updatedAttempts = new HashSet<AchievementAttempt>();

            if ( streak == null )
            {
                return updatedAttempts;
            }

            // If the achievement type is not active (or null) then there is nothing to do
            if ( achievementTypeCache?.IsActive != true )
            {
                return updatedAttempts;
            }

            // If there are unmet prerequisites, then there is nothing to do
            var achievementTypeService = new AchievementTypeService( rockContext );
            var unmetPrerequisites = achievementTypeService.GetUnmetPrerequisites( achievementTypeCache.Id, streak.PersonAliasId );

            if ( unmetPrerequisites.Any() )
            {
                return updatedAttempts;
            }

            // Get all of the attempts for this streak and achievement combo, ordered by start date DESC so that
            // the most recent attempts can be found with FirstOrDefault
            var achievementAttemptService = new AchievementAttemptService( rockContext );
            var attempts = achievementAttemptService.GetOrderedAchieverAttempts( achievementAttemptService.Queryable(), achievementTypeCache, streak.PersonAliasId );
                
            var mostRecentSuccess = attempts.FirstOrDefault( saa => saa.AchievementAttemptEndDateTime.HasValue && saa.IsSuccessful );
            var overachievementPossible = achievementTypeCache.AllowOverAchievement && mostRecentSuccess != null && !mostRecentSuccess.IsClosed;
            var successfulAttemptCount = attempts.Count( saa => saa.IsSuccessful );
            var maxSuccessesAllowed = achievementTypeCache.MaxAccomplishmentsAllowed ?? int.MaxValue;

            // If the most recent success is still open and overachievement is allowed, then update it
            if ( overachievementPossible )
            {
                UpdateOpenAttempt( mostRecentSuccess, achievementTypeCache, streak );
                updatedAttempts.Add( mostRecentSuccess );

                if ( !mostRecentSuccess.IsClosed )
                {
                    // New records can only be created once the open records are all closed
                    return updatedAttempts;
                }
            }

            // If the success count limit has been reached, then no more processing should be done
            if ( successfulAttemptCount >= maxSuccessesAllowed )
            {
                return updatedAttempts;
            }

            // Everything after the most recent success is on the table for deletion. Successes should not be
            // deleted. Everything after a success might be recalculated because of streak map data changes.
            // Try to reuse these attempts if they match for continuity, but if the start date is changed, they
            // get deleted.
            var attemptsToDelete = attempts;

            if ( mostRecentSuccess != null )
            {
                attemptsToDelete = attemptsToDelete
                    .Where( saa => saa.AchievementAttemptStartDateTime > mostRecentSuccess.AchievementAttemptStartDateTime )
                    .ToList();
            }

            var newAttempts = CreateNewAttempts( achievementTypeCache, streak, mostRecentSuccess );

            if ( newAttempts != null && newAttempts.Any() )
            {
                newAttempts = newAttempts.OrderBy( saa => saa.AchievementAttemptStartDateTime ).ToList();

                foreach ( var newAttempt in newAttempts )
                {
                    // Keep the old attempt if possible, otherwise add a new one
                    var existingAttempt = attemptsToDelete.FirstOrDefault( saa => saa.AchievementAttemptStartDateTime == newAttempt.AchievementAttemptStartDateTime );

                    if ( existingAttempt != null )
                    {
                        attemptsToDelete.Remove( existingAttempt );
                        CopyAttempt( newAttempt, existingAttempt );
                        updatedAttempts.Add( existingAttempt );
                    }
                    else
                    {
                        newAttempt.AchieverEntityId = streak.PersonAliasId;
                        newAttempt.AchievementTypeId = achievementTypeCache.Id;
                        achievementAttemptService.Add( newAttempt );
                        updatedAttempts.Add( newAttempt );
                    }

                    // If this attempt was successful then make re-check the max success limit
                    if ( newAttempt.IsSuccessful )
                    {
                        successfulAttemptCount++;

                        if ( successfulAttemptCount >= maxSuccessesAllowed )
                        {
                            break;
                        }
                    }
                }
            }

            if ( attemptsToDelete.Any() )
            {
                updatedAttempts.RemoveAll( attemptsToDelete );
                achievementAttemptService.DeleteRange( attemptsToDelete );
            }

            return updatedAttempts;
        }

        /// <summary>
        /// Should the achievement type process attempts if the given source entity has been modified in some way.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="sourceEntity">The source entity.</param>
        /// <returns></returns>
        public override bool ShouldProcess( AchievementTypeCache achievementTypeCache, IEntity sourceEntity )
        {
            var streak = sourceEntity as Streak;

            if ( streak == null )
            {
                return false;
            }

            return streak.StreakTypeId == GetStreakTypeCache( achievementTypeCache )?.Id;
        }

        /// <summary>
        /// Gets the source entities query. This is the set of source entities that should be passed to the process method
        /// when processing this achievement type.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override IQueryable<IEntity> GetSourceEntitiesQuery( AchievementTypeCache achievementTypeCache, RockContext rockContext )
        {
            var streakTypeCache = GetStreakTypeCache( achievementTypeCache );

            if ( streakTypeCache == null )
            {
                return Enumerable.Empty<Streak>().AsQueryable();
            }

            var service = new StreakService( rockContext );
            return service.Queryable().Where( s => s.StreakTypeId == streakTypeCache.Id );
        }

        /// <summary>
        /// Gets the achiever attempt query. This is the query (not enumerated) that joins attempts of this achievement type with the
        /// achiever entities, as well as the name (<see cref="AchieverAttemptItem.AchieverName"/> that could represent the achiever
        /// in a grid or other such display.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public override IQueryable<AchieverAttemptItem> GetAchieverAttemptQuery( AchievementTypeCache achievementTypeCache, RockContext rockContext )
        {
            var attemptService = new AchievementAttemptService( rockContext );
            var personAliasService = new PersonAliasService( rockContext );

            var attemptQuery = attemptService.Queryable().Where( aa => aa.AchievementTypeId == achievementTypeCache.Id );
            var personAliasQuery = personAliasService.Queryable();

            return attemptQuery.Join(
                    personAliasQuery,
                    aa => aa.AchieverEntityId,
                    pa => pa.Id,
                    ( aa, pa ) => new AchieverAttemptItem
                    {
                        AchievementAttempt = aa,
                        Achiever = pa,
                        AchieverName = pa.Person.NickName + " " + pa.Person.LastName
                    } );
        }

        /// <summary>
        /// Gets the name of the source that these achievements are measured from.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <returns></returns>
        public override string GetSourceName( AchievementTypeCache achievementTypeCache )
        {
            var streakTypeCache = GetStreakTypeCache( achievementTypeCache );

            if ( streakTypeCache != null )
            {
                return streakTypeCache.Name;
            }

            return string.Empty;
        }

        /// <summary>
        /// Determines whether this achievement type applies given the set of filters. The filters could be the query string
        /// of a web request.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="filters">The filters.</param>
        /// <returns>
        ///   <c>true</c> if [is relevant to all filters] [the specified filters]; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsRelevantToAllFilters( AchievementTypeCache achievementTypeCache, List<KeyValuePair<string, string>> filters )
        {
            if ( filters.Count == 0 )
            {
                return true;
            }

            if ( filters.Count > 1 )
            {
                return false;
            }

            var filter = filters.First();

            if ( filter.Key.Equals( "StreakTypeId", StringComparison.OrdinalIgnoreCase ) )
            {
                return filter.Value.AsInteger() == GetStreakTypeCache( achievementTypeCache )?.Id;
            }

            return false;
        }

        #endregion Base Component Overrides

        #region Abstract Members

        /// <summary>
        /// Gets the streak type attribute key.
        /// This is done so that the attribute remains on the derived class as it originally was. Moving the attribute
        /// was considered risky because the attribute could be created in a migration.
        /// </summary>
        protected readonly string StreakTypeAttributeKey;

        /// <summary>
        /// Update the open attempt record if there are changes. Be sure to close the attempt if it is no longer possible to make
        /// progress on this open attempt.
        /// </summary>
        /// <param name="openAttempt">The open attempt.</param>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="streak">The streak.</param>
        protected abstract void UpdateOpenAttempt( AchievementAttempt openAttempt, AchievementTypeCache achievementTypeCache, Streak streak );

        /// <summary>
        /// Create new attempt records and return them in a list. All new attempts should be after the most recent successful attempt.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="streak">The streak.</param>
        /// <param name="mostRecentSuccess">The most recent successful attempt.</param>
        /// <returns></returns>
        protected abstract List<AchievementAttempt> CreateNewAttempts( AchievementTypeCache achievementTypeCache, Streak streak, AchievementAttempt mostRecentSuccess );

        #endregion Abstract Members
    }
}
