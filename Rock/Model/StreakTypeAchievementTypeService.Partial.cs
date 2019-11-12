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
    /// Service/Data access class for <see cref="StreakTypeAchievementType"/> entity objects.
    /// </summary>
    public partial class StreakTypeAchievementTypeService
    {
        /// <summary>
        /// Processes attempts for the specified streak type achievement type identifier. This adds new attempts and updates existing attempts.
        /// </summary>
        /// <param name="streakTypeAchievementTypeId">The streak type achievement type identifier.</param>
        public static void Process( int streakTypeAchievementTypeId )
        {
            var achievementTypeCache = StreakTypeAchievementTypeCache.Get( streakTypeAchievementTypeId );

            if (achievementTypeCache == null)
            {
                throw new ArgumentException( $"The StreakTypeAchievementTypeCache did not resolve for record id {streakTypeAchievementTypeId}" );
            }

            var achievementComponent = achievementTypeCache.AchievementComponent;

            if ( achievementComponent == null )
            {
                throw new ArgumentException( $"The AchievementComponent did not resolve for record id {streakTypeAchievementTypeId}" );
            }

            var streakTypeId = achievementTypeCache.StreakTypeId;
            var streakService = new StreakService( new RockContext() );
            var streaks = streakService.Queryable().AsNoTracking()
                .Where( s => s.StreakTypeId == streakTypeId );

            foreach ( var streak in streaks )
            {
                // Process each streak in it's own data context to avoid the data context changes getting too big and slow
                var rockContext = new RockContext();
                achievementComponent.Process( rockContext, achievementTypeCache, streak );
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the progress statement for the person for this achievement.
        /// </summary>
        /// <param name="streakTypeAchievementTypeCache">The streak type achievement type cache.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public ProgressStatement GetProgressStatement( StreakTypeAchievementTypeCache streakTypeAchievementTypeCache, int personId )
        {
            var rockContext = Context as RockContext;
            var attemptService = new StreakAchievementAttemptService( rockContext );

            var attempts = attemptService.Queryable()
                .AsNoTracking()
                .Where( saa =>
                    saa.StreakTypeAchievementTypeId == streakTypeAchievementTypeCache.Id &&
                    saa.Streak.PersonAlias.PersonId == personId )
                .OrderByDescending( saa => saa.AchievementAttemptStartDateTime )
                .ToList();

            var progressStatement = new ProgressStatement( streakTypeAchievementTypeCache );

            if (!attempts.Any())
            {
                return progressStatement;
            }

            var mostRecentAttempt = attempts.First();
            var bestAttempt = attempts.OrderByDescending( saa => saa.Progress ).First();

            progressStatement.SuccessCount = attempts.Count( saa => saa.IsSuccessful );
            progressStatement.AttemptCount = attempts.Count();
            progressStatement.BestAttempt = bestAttempt;
            progressStatement.MostRecentAttempt = mostRecentAttempt;

            return progressStatement;
        }
    }

    /// <summary>
    /// Statement of Progress for an Achievement Type
    /// </summary>
    public class ProgressStatement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressStatement" /> class.
        /// </summary>
        /// <param name="streakTypeAchievementTypeCache">The streak type achievement type cache.</param>
        public ProgressStatement( StreakTypeAchievementTypeCache streakTypeAchievementTypeCache )
        {
            StreakTypeAchievementTypeId = streakTypeAchievementTypeCache.Id;
            StreakTypeAchievementTypeName = streakTypeAchievementTypeCache.Name;
            StreakTypeAchievementTypeDescription = streakTypeAchievementTypeCache.Description;
            Attributes = streakTypeAchievementTypeCache.AttributeValues?
                .Where( kvp => kvp.Key != "Active" && kvp.Key != "Order" )
                .ToDictionary( kvp => kvp.Key, kvp => kvp.Value.Value );
        }

        /// <summary>
        /// Gets or sets the streak type achievement type identifier.
        /// </summary>
        public int StreakTypeAchievementTypeId { get; }

        /// <summary>
        /// Gets or sets the name of the streak type achievement type.
        /// </summary>
        public string StreakTypeAchievementTypeName { get; }

        /// <summary>
        /// Gets or sets the streak type achievement type description.
        /// </summary>
        public string StreakTypeAchievementTypeDescription { get; }

        /// <summary>
        /// Gets or sets the success count.
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Gets or sets the attempt count.
        /// </summary>
        public int AttemptCount { get; set; }

        /// <summary>
        /// Gets or sets the best attempt.
        /// </summary>
        public StreakAchievementAttempt BestAttempt { get; set; }

        /// <summary>
        /// Gets or sets the most recent attempt.
        /// </summary>
        public StreakAchievementAttempt MostRecentAttempt { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public Dictionary<string, string> Attributes { get; set; }
    }
}