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
    /// Base class for achievement components
    /// </summary>
    public abstract class AchievementComponent : Rock.Extension.Component
    {
        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get => new Dictionary<string, string>
            {
                { "Active", "True" },
                { "Order", "0" }
            };
        }

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public override bool IsActive
        {
            get => true;
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public override int Order
        {
            get => 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AchievementComponent" /> class.
        /// </summary>
        public AchievementComponent() : base( false )
        {
            // Override default constructor of Component that loads attributes (needs to be done by each instance)
        }

        /// <summary>
        /// Loads the attributes for the <see cref="StreakTypeAchievementType" />.
        /// </summary>
        /// <param name="streakTypeAchievementType"></param>
        public void LoadAttributes( StreakTypeAchievementType streakTypeAchievementType )
        {
            if ( streakTypeAchievementType is null )
            {
                throw new ArgumentNullException( nameof( streakTypeAchievementType ) );
            }

            streakTypeAchievementType.LoadAttributes();
        }

        /// <summary>
        /// Gets the value of an attribute key. Do not use this method. Use <see cref="GetAttributeValue(StreakTypeAchievementTypeCache, string)" />
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public override string GetAttributeValue( string key )
        {
            throw new Exception( "Use the GetAttributeValue( streakTypeAchievementTypeCache, key ) method instead." );
        }

        /// <summary>
        /// Gets the attribute value for the achievement
        /// </summary>
        /// <param name="streakTypeAchievementTypeCache"></param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected string GetAttributeValue( StreakTypeAchievementTypeCache streakTypeAchievementTypeCache, string key )
        {
            if ( streakTypeAchievementTypeCache is null )
            {
                throw new ArgumentNullException( nameof( streakTypeAchievementTypeCache ) );
            }

            return streakTypeAchievementTypeCache.GetAttributeValue( key );
        }

        /// <summary>
        /// Copies the source attempt properties to the target.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        protected void CopyAttempt( StreakAchievementAttempt source, StreakAchievementAttempt target )
        {
            target.Progress = source.Progress;
            target.IsClosed = source.IsClosed;
            target.IsSuccessful = source.IsSuccessful;
            target.AchievementAttemptStartDateTime = source.AchievementAttemptStartDateTime;
            target.AchievementAttemptEndDateTime = source.AchievementAttemptEndDateTime;
        }

        /// <summary>
        /// Calculates the minimum date for the next achievement attempt.
        /// </summary>
        /// <param name="enrollmentDate">The streak type start date.</param>
        /// <param name="mostRecentClosedAttempt">The most recent closed attempt.</param>
        /// <param name="achievementTypeStartDate">The achievement type start date.</param>
        /// <param name="targetCount">How many engagements are required to be successful</param>
        /// <returns></returns>
        protected DateTime CalculateMinDateForAchievementAttempt( DateTime enrollmentDate, StreakAchievementAttempt mostRecentClosedAttempt, DateTime? achievementTypeStartDate, int targetCount )
        {
            // Use the enrollment date as a starting point for the calculation
            var minDate = enrollmentDate;

            // If the achievement start date is later, then use that
            if ( achievementTypeStartDate.HasValue && achievementTypeStartDate.Value > minDate )
            {
                minDate = achievementTypeStartDate.Value;
            }

            if ( mostRecentClosedAttempt != null )
            {
                var deficiency = CalculateDeficiency( mostRecentClosedAttempt, targetCount );

                // If the most recent closed attempt has an end date, then the next attempt must be at least one day after
                if ( mostRecentClosedAttempt.AchievementAttemptEndDateTime.HasValue && deficiency == 0 )
                {
                    // We know the end date and it used all the bits, so just start after the end date.
                    minDate = mostRecentClosedAttempt.AchievementAttemptEndDateTime.Value.AddDays( 1 );
                }
                else if ( deficiency >= 1 )
                {
                    // Increment from the start date by the deficiency
                    minDate = mostRecentClosedAttempt.AchievementAttemptStartDateTime.AddDays( deficiency );
                }
                else
                {
                    // This shouldn't happen
                    minDate = mostRecentClosedAttempt.AchievementAttemptStartDateTime.AddDays( 1 );
                }
            }

            return minDate;
        }

        /// <summary>
        /// Calculates the maximum date for an achievement attempt to be completed.
        /// </summary>
        /// <param name="minDate">The minimum date.</param>
        /// <param name="achievementTypeEndDate">The achievement type end date.</param>
        /// <returns></returns>
        protected DateTime CalculateMaxDateForAchievementAttempt( DateTime minDate, DateTime? achievementTypeEndDate )
        {
            // Use today as a starting point for the end date
            var maxDate = RockDateTime.Today;

            // If the achievement type has an end date and it is before today, then the attempt cannot be beyond that
            if ( achievementTypeEndDate.HasValue && achievementTypeEndDate.Value < maxDate )
            {
                maxDate = achievementTypeEndDate.Value;
            }

            // If somehow the max became less than the min date, use the min date
            if ( maxDate < minDate )
            {
                maxDate = minDate;
            }

            return maxDate;
        }

        /// <summary>
        /// Calculates the progress.
        /// </summary>
        /// <param name="actualCount">The actual count.</param>
        /// <param name="targetCount">The target count.</param>
        /// <returns></returns>
        protected static decimal CalculateProgress( int actualCount, int targetCount )
        {
            return decimal.Divide( actualCount, targetCount );
        }

        /// <summary>
        /// Calculates the deficiency.
        /// </summary>
        /// <param name="attempt">The attempt.</param>
        /// <param name="targetCount">The target count.</param>
        /// <returns></returns>
        protected static int CalculateDeficiency( StreakAchievementAttempt attempt, int targetCount )
        {
            var progress = attempt?.Progress ?? 0m;

            if ( progress < 0m )
            {
                progress = 0m;
            }
            else if ( progress > 1m )
            {
                progress = 1m;
            }

            var attemptCount = ( int ) decimal.Round( progress * targetCount );
            return targetCount - attemptCount;
        }

        /// <summary>
        /// Processes the specified streak type achievement type cache.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="streakTypeAchievementTypeCache">The streak type achievement type cache.</param>
        /// <param name="streak">The streak.</param>
        public virtual void Process( RockContext rockContext, StreakTypeAchievementTypeCache streakTypeAchievementTypeCache, Streak streak )
        {
            // If the achievement type is not active (or null) then there is nothing to do
            if ( streakTypeAchievementTypeCache?.IsActive != true )
            {
                return;
            }

            // Determine the person id
            var personAliasService = new PersonAliasService( rockContext );
            var personId = personAliasService.GetPersonId( streak.PersonAliasId );

            if ( !personId.HasValue )
            {
                ExceptionLogService.LogException(
                    $"Could not derive personId from personAliasId {streak.PersonAliasId} used on streak {streak.Id} when processing achievement {streakTypeAchievementTypeCache.Name}" );
                return;
            }

            // If there are unmet prerequisites, then there is nothing to do
            var streakTypeAchievementTypeService = new StreakTypeAchievementTypeService( rockContext );
            var unmetPrerequisites = streakTypeAchievementTypeService.GetUnmetPrerequisites( streakTypeAchievementTypeCache.Id, personId.Value );

            if ( unmetPrerequisites.Any() )
            {
                return;
            }

            // Get all of the attempts for this streak and achievement combo, ordered by start date DESC so that
            // the most recent attempts can be found with FirstOrDefault
            var streakAchievementAttemptService = new StreakAchievementAttemptService( rockContext );
            var attempts = streakAchievementAttemptService.Queryable()
                .OrderByDescending( saa => saa.AchievementAttemptStartDateTime )
                .Where( saa =>
                    saa.StreakTypeAchievementTypeId == streakTypeAchievementTypeCache.Id &&
                    saa.StreakId == streak.Id )
                .ToList();

            var mostRecentSuccess = attempts.FirstOrDefault( saa => saa.AchievementAttemptEndDateTime.HasValue && saa.IsSuccessful );
            var overachievementPossible = streakTypeAchievementTypeCache.AllowOverAchievement && mostRecentSuccess != null && !mostRecentSuccess.IsClosed;
            var successfulAttemptCount = attempts.Count( saa => saa.IsSuccessful );
            var maxSuccessesAllowed = streakTypeAchievementTypeCache.MaxAccomplishmentsAllowed ?? int.MaxValue;

            // If the most recent success is still open and overachievement is allowed, then update it
            if ( overachievementPossible )
            {
                UpdateOpenAttempt( mostRecentSuccess, streakTypeAchievementTypeCache, streak );

                if ( !mostRecentSuccess.IsClosed )
                {
                    // New records can only be created once the open records are all closed
                    return;
                }
            }

            // If the success count limit has been reached, then no more processing should be done
            if ( successfulAttemptCount >= maxSuccessesAllowed )
            {
                return;
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

            var newAttempts = CreateNewAttempts( streakTypeAchievementTypeCache, streak, mostRecentSuccess );

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
                    }
                    else
                    {
                        newAttempt.StreakId = streak.Id;
                        newAttempt.StreakTypeAchievementTypeId = streakTypeAchievementTypeCache.Id;
                        streakAchievementAttemptService.Add( newAttempt );
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
                streakAchievementAttemptService.DeleteRange( attemptsToDelete );
            }
        }

        /// <summary>
        /// Update the open attempt record if there are changes. Be sure to close the attempt if it is no longer possible to make
        /// progress on this open attempt.
        /// </summary>
        /// <param name="openAttempt">The open attempt.</param>
        /// <param name="streakTypeAchievementTypeCache">The streak type achievement type cache.</param>
        /// <param name="streak">The streak.</param>
        protected abstract void UpdateOpenAttempt( StreakAchievementAttempt openAttempt, StreakTypeAchievementTypeCache streakTypeAchievementTypeCache, Streak streak );

        /// <summary>
        /// Create new attempt records and return them in a list. All new attempts should be after the most recent successful attempt.
        /// </summary>
        /// <param name="streakTypeAchievementTypeCache">The streak type achievement type cache.</param>
        /// <param name="streak">The streak.</param>
        /// <param name="mostRecentSuccess">The most recent successful attempt.</param>
        /// <returns></returns>
        protected abstract List<StreakAchievementAttempt> CreateNewAttempts( StreakTypeAchievementTypeCache streakTypeAchievementTypeCache, Streak streak, StreakAchievementAttempt mostRecentSuccess );
    }
}
