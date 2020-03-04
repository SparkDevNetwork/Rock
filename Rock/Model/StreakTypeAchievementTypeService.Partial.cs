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
        #region Overrides

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public override bool Delete( StreakTypeAchievementType item )
        {
            // Since Entity Framework cannot cascade delete dependent prerequisites because of a possible circular reference,
            // we need to delete them here
            var prerequisiteService = new StreakTypeAchievementTypePrerequisiteService( Context as RockContext );
            var dependencyQuery = prerequisiteService.Queryable().Where( statp => statp.PrerequisiteStreakTypeAchievementTypeId == item.Id );
            prerequisiteService.DeleteRange( dependencyQuery );

            // Now we can delete the item as normal
            return base.Delete( item );
        }

        #endregion Overrides

        /// <summary>
        /// Processes attempts for the specified streak type achievement type identifier. This adds new attempts and updates existing attempts.
        /// </summary>
        /// <param name="streakTypeAchievementTypeId">The streak type achievement type identifier.</param>
        public static void Process( int streakTypeAchievementTypeId )
        {
            var achievementTypeCache = StreakTypeAchievementTypeCache.Get( streakTypeAchievementTypeId );

            if ( achievementTypeCache == null )
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
        /// Sorts for processing. This considers prerequisites so that prerequisites are processed first.
        /// This is a slightly modified depth first search based on https://stackoverflow.com/a/11027096
        /// </summary>
        /// <param name="achievementTypesToSort">The achievement types to sort.</param>
        /// <returns></returns>
        public static List<StreakTypeAchievementTypeCache> SortAccordingToPrerequisites( List<StreakTypeAchievementTypeCache> achievementTypesToSort )
        {
            var sorted = new List<StreakTypeAchievementTypeCache>();
            var visitedIds = new HashSet<int>();

            foreach ( var achievementType in achievementTypesToSort )
            {
                VisitForSort( achievementType, visitedIds, sorted );
            }

            // It is possible that prerequisites of the achievement types in the input list were not actually in the input list. Therefore
            // we want to return the intersection of the sorted list and the input list, but preserve order.
            return sorted.Intersect( achievementTypesToSort ).ToList();
        }

        /// <summary>
        /// Visit for sort. Part of the <see cref="SortAccordingToPrerequisites(List{StreakTypeAchievementTypeCache})" /> algorithm
        /// </summary>
        /// <param name="currentAchievementType">The current achievement type being visited.</param>
        /// <param name="visitedIds">The IDs of achievement types that have been visited already.</param>
        /// <param name="sorted">The sorted achievement types thus far.</param>
        /// <exception cref="System.Exception">Attempting to sort achievement types according to prerequisites and encountered a cyclic dependency on id: {item.Id}</exception>
        private static void VisitForSort( StreakTypeAchievementTypeCache currentAchievementType, HashSet<int> visitedIds, List<StreakTypeAchievementTypeCache> sorted )
        {
            if ( !visitedIds.Contains( currentAchievementType.Id ) )
            {
                visitedIds.Add( currentAchievementType.Id );

                foreach ( var prerequisite in currentAchievementType.Prerequisites )
                {
                    VisitForSort( prerequisite.PrerequisiteStreakTypeAchievementType, visitedIds, sorted );
                }

                sorted.Add( currentAchievementType );
            }
            else if ( !sorted.Contains( currentAchievementType ) )
            {
                throw new Exception( $"Attempting to sort achievement types according to prerequisites and encountered a cyclic dependency on id: {currentAchievementType.Id}" );
            }
        }

        /// <summary>
        /// Gets the unmet prerequisites.
        /// </summary>
        /// <param name="achievementTypeId">The achievement type identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public List<ProgressStatement> GetUnmetPrerequisites( int achievementTypeId, int personId )
        {
            var achievementType = StreakTypeAchievementTypeCache.Get( achievementTypeId );

            if ( achievementType == null || !achievementType.Prerequisites.Any() )
            {
                return new List<ProgressStatement>();
            }

            return achievementType.Prerequisites
                .Select( stat => GetFlatProgressStatement( stat.PrerequisiteStreakTypeAchievementType, personId ) )
                .Where( ps => ps.SuccessCount == 0 )
                .ToList();
        }

        /// <summary>
        /// Gets the progress statements for the person for all active achievements.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public List<ProgressStatement> GetProgressStatements( int personId )
        {
            var achievementTypes = StreakTypeAchievementTypeCache.All().Where( stat => stat.IsActive ).ToList();
            var orderedAchievementTypes = SortAccordingToPrerequisites( achievementTypes );
            var progressStatementsDictionary = new Dictionary<int, ProgressStatement>();
            var progressStatements = new List<ProgressStatement>();

            foreach ( var achievementType in orderedAchievementTypes )
            {
                var progressStatement = GetFlatProgressStatement( achievementType, personId );
                progressStatementsDictionary[achievementType.Id] = progressStatement;
                progressStatements.Add( progressStatement );

                foreach ( var prerequisite in achievementType.PrerequisiteAchievementTypes )
                {
                    var prerequisiteProgressStatement = progressStatementsDictionary[prerequisite.Id];

                    if ( prerequisiteProgressStatement.SuccessCount == 0 )
                    {
                        progressStatement.UnmetPrerequisites.Add( prerequisiteProgressStatement );
                    }
                }
            }

            return progressStatements;
        }

        /// <summary>
        /// Gets the progress statement for the person for this achievement.
        /// </summary>
        /// <param name="streakTypeAchievementTypeCache">The streak type achievement type cache.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public ProgressStatement GetProgressStatement( StreakTypeAchievementTypeCache streakTypeAchievementTypeCache, int personId )
        {
            var progressStatement = GetFlatProgressStatement( streakTypeAchievementTypeCache, personId );

            foreach ( var prerequisite in streakTypeAchievementTypeCache.PrerequisiteAchievementTypes )
            {
                var prerequisiteProgressStatement = GetFlatProgressStatement( prerequisite, personId );
                progressStatement.UnmetPrerequisites.Add( prerequisiteProgressStatement );
            }

            return progressStatement;
        }

        /// <summary>
        /// Gets the progress statement for the person for this achievement. Flat means that the unmet prerequisites are not computed.
        /// </summary>
        /// <param name="streakTypeAchievementTypeCache">The streak type achievement type cache.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        private ProgressStatement GetFlatProgressStatement( StreakTypeAchievementTypeCache streakTypeAchievementTypeCache, int personId )
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

            // If there are no attempts, no other information can be derived
            if ( !attempts.Any() )
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

        /// <summary>
        /// Returns a collection of Achievement Types that can be selected as prerequisites of the specified Achievement Type.
        /// An Achievement Type cannot be a prerequisite of itself, or of any Achievement Type that has it as a prerequisite.
        /// </summary>
        /// <param name="streakTypeAchievementTypeCache">The Achievement Type for which prerequisites are required.</param>
        /// <returns></returns>
        public static List<StreakTypeAchievementTypeCache> GetEligiblePrerequisiteAchievementTypeCaches( StreakTypeAchievementTypeCache streakTypeAchievementTypeCache )
        {
            // Get achievement types of which the specified achievement type is not already a prerequisite.
            return StreakTypeAchievementTypeCache.All()
                .Where( stat =>
                    stat.IsActive &&
                    stat.Id != streakTypeAchievementTypeCache.Id &&
                    stat.StreakTypeId == streakTypeAchievementTypeCache.StreakTypeId &&
                    !stat.Prerequisites.Any( p => p.PrerequisiteStreakTypeAchievementTypeId == streakTypeAchievementTypeCache.Id ) )
                .ToList();
        }

        /// <summary>
        /// Returns a collection of Achievement Types that can be selected as prerequisites for a new Achievement Type.
        /// </summary>
        /// <param name="streakTypeCache">The streak type cache.</param>
        /// <returns></returns>
        public static List<StreakTypeAchievementTypeCache> GetEligiblePrerequisiteAchievementTypeCaches( StreakTypeCache streakTypeCache )
        {
            return StreakTypeAchievementTypeCache.All()
                .Where( stat =>
                    stat.IsActive &&
                    stat.StreakTypeId == streakTypeCache.Id )
                .ToList();
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
            UnmetPrerequisites = new List<ProgressStatement>();
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

        /// <summary>
        /// Gets or sets the unmet prerequisites.
        /// </summary>
        /// <value>
        /// The unmet prerequisites.
        /// </value>
        public List<ProgressStatement> UnmetPrerequisites { get; }
    }
}