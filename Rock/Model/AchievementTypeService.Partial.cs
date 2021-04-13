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
using System.Reflection;
using Rock.Data;
using Rock.Lava;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="AchievementType"/> entity objects.
    /// </summary>
    public partial class AchievementTypeService
    {
        #region Overrides

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public override bool Delete( AchievementType item )
        {
            // Since Entity Framework cannot cascade delete dependent prerequisites because of a possible circular reference,
            // we need to delete them here
            var prerequisiteService = new AchievementTypePrerequisiteService( Context as RockContext );
            var dependencyQuery = prerequisiteService.Queryable().Where( statp => statp.PrerequisiteAchievementTypeId == item.Id );
            prerequisiteService.DeleteRange( dependencyQuery );

            // Now we can delete the item as normal
            return base.Delete( item );
        }

        #endregion Overrides

        #region Component Based Logic

        /// <summary>
        /// Processes attempts for the specified streak type achievement type identifier. This adds new attempts and updates existing attempts.
        /// </summary>
        /// <param name="achievementTypeId">The streak type achievement type identifier.</param>
        public static void Process( int achievementTypeId )
        {
            var achievementTypeCache = AchievementTypeCache.Get( achievementTypeId );

            if ( achievementTypeCache == null )
            {
                throw new ArgumentException( $"The AchievementTypeCache did not resolve for record id {achievementTypeId}" );
            }

            var achievementComponent = achievementTypeCache.AchievementComponent;

            if ( achievementComponent == null )
            {
                throw new ArgumentException( $"The AchievementComponent did not resolve for record id {achievementTypeId}" );
            }

            var sourceEntitiesQuery = achievementComponent.GetSourceEntitiesQuery( achievementTypeCache, new RockContext() )
                .AsNoTracking()
                .ToList();

            foreach ( var sourceEntity in sourceEntitiesQuery )
            {
                // Process each streak in it's own data context to avoid the data context changes getting too big and slow
                var rockContext = new RockContext();
                achievementComponent.Process( rockContext, achievementTypeCache, sourceEntity );
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the badge markup for this achiever for this achievement.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="achieverEntityId">The achiever entity identifier.</param>
        /// <returns></returns>
        public string GetBadgeMarkup( AchievementTypeCache achievementTypeCache, int achieverEntityId )
        {
            var achievementComponent = achievementTypeCache.AchievementComponent;

            if ( achievementComponent == null )
            {
                throw new ArgumentException( $"The AchievementComponent did not resolve for {achievementTypeCache}" );
            }

            return achievementComponent.GetBadgeMarkup( achievementTypeCache, achieverEntityId );
        }

        #endregion Component Based Logic

        #region Sort

        /// <summary>
        /// Sorts for processing. This considers prerequisites so that prerequisites are processed first.
        /// This is a slightly modified depth first search based on https://stackoverflow.com/a/11027096
        /// </summary>
        /// <param name="achievementTypesToSort">The achievement types to sort.</param>
        /// <returns></returns>
        public static List<AchievementTypeCache> SortAccordingToPrerequisites( List<AchievementTypeCache> achievementTypesToSort )
        {
            var sorted = new List<AchievementTypeCache>();
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
        /// Visit for sort. Part of the <see cref="SortAccordingToPrerequisites(List{AchievementTypeCache})" /> algorithm
        /// </summary>
        /// <param name="currentAchievementType">The current achievement type being visited.</param>
        /// <param name="visitedIds">The IDs of achievement types that have been visited already.</param>
        /// <param name="sorted">The sorted achievement types thus far.</param>
        /// <exception cref="System.Exception">Attempting to sort achievement types according to prerequisites and encountered a cyclic dependency on id: {item.Id}</exception>
        private static void VisitForSort( AchievementTypeCache currentAchievementType, HashSet<int> visitedIds, List<AchievementTypeCache> sorted )
        {
            if ( !visitedIds.Contains( currentAchievementType.Id ) )
            {
                visitedIds.Add( currentAchievementType.Id );

                foreach ( var prerequisite in currentAchievementType.Prerequisites )
                {
                    VisitForSort( prerequisite.PrerequisiteAchievementType, visitedIds, sorted );
                }

                sorted.Add( currentAchievementType );
            }
            else if ( !sorted.Contains( currentAchievementType ) )
            {
                throw new Exception( $"Attempting to sort achievement types according to prerequisites and encountered a cyclic dependency on id: {currentAchievementType.Id}" );
            }
        }

        #endregion Sort

        #region Progress Statements

        /// <summary>
        /// Gets the progress statements for the achiever for all active achievements.
        /// </summary>
        /// <param name="achieverEntityTypeId">The achiever entity type identifier.</param>
        /// <param name="achieverEntityId">The achiever identifier.</param>
        /// <returns></returns>
        public List<ProgressStatement> GetProgressStatements( int achieverEntityTypeId, int achieverEntityId )
        {
            var achievementTypes = AchievementTypeCache.All()
                .Where( at =>
                    at.IsActive &&
                    at.AchieverEntityTypeId == achieverEntityTypeId )
                .ToList();

            var orderedAchievementTypes = SortAccordingToPrerequisites( achievementTypes );
            var progressStatementsDictionary = new Dictionary<int, ProgressStatement>();
            var progressStatements = new List<ProgressStatement>();

            foreach ( var achievementType in orderedAchievementTypes )
            {
                var progressStatement = GetFlatProgressStatement( achievementType, achieverEntityId );
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
        /// Gets the progress statement for the achiever for this achievement.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="achieverEntityId">The achiever entity identifier.</param>
        /// <returns></returns>
        public ProgressStatement GetProgressStatement( AchievementTypeCache achievementTypeCache, int achieverEntityId )
        {
            var progressStatement = GetFlatProgressStatement( achievementTypeCache, achieverEntityId );

            foreach ( var prerequisite in achievementTypeCache.PrerequisiteAchievementTypes )
            {
                var prerequisiteProgressStatement = GetFlatProgressStatement( prerequisite, achieverEntityId );
                progressStatement.UnmetPrerequisites.Add( prerequisiteProgressStatement );
            }

            return progressStatement;
        }

        /// <summary>
        /// Gets the progress statement for the achiever for this achievement. Flat means that the unmet prerequisites are not computed.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="achieverEntityId">The achiever entity identifier.</param>
        /// <returns></returns>
        private ProgressStatement GetFlatProgressStatement( AchievementTypeCache achievementTypeCache, int achieverEntityId )
        {
            var rockContext = Context as RockContext;
            var attemptService = new AchievementAttemptService( rockContext );

            var attemptsQuery = attemptService.Queryable().AsNoTracking();
                
            var attempts = attemptService.GetOrderedAchieverAttempts( attemptsQuery, achievementTypeCache, achieverEntityId );
            
            var progressStatement = new ProgressStatement( achievementTypeCache );

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

        #endregion Progress Statements

        #region Prerequisites

        /// <summary>
        /// Gets the unmet prerequisites.
        /// </summary>
        /// <param name="achievementTypeId">The achievement type identifier.</param>
        /// <param name="achieverEntityId">The achiever entity identifier.</param>
        /// <returns></returns>
        public List<ProgressStatement> GetUnmetPrerequisites( int achievementTypeId, int achieverEntityId )
        {
            var achievementType = AchievementTypeCache.Get( achievementTypeId );

            if ( achievementType == null || !achievementType.Prerequisites.Any() )
            {
                return new List<ProgressStatement>();
            }

            return achievementType.Prerequisites
                .Select( stat => GetFlatProgressStatement( stat.PrerequisiteAchievementType, achieverEntityId ) )
                .Where( ps => ps.SuccessCount == 0 )
                .ToList();
        }

        /// <summary>
        /// Returns a collection of Achievement Types that can be selected as prerequisites of the specified Achievement Type.
        /// An Achievement Type cannot be a prerequisite of itself, or of any Achievement Type that has it as a prerequisite.
        /// </summary>
        /// <param name="achievementTypeCache">The Achievement Type for which prerequisites are required.</param>
        /// <returns></returns>
        public static List<AchievementTypeCache> GetEligiblePrerequisiteAchievementTypeCaches( AchievementTypeCache achievementTypeCache )
        {
            // Get achievement types of which the specified achievement type is not already a prerequisite.
            return AchievementTypeCache.All()
                .Where( at =>
                    at.IsActive &&
                    at.AchieverEntityTypeId == achievementTypeCache.AchieverEntityTypeId &&
                    at.Id != achievementTypeCache.Id &&
                    !at.Prerequisites.Any( p => p.PrerequisiteAchievementTypeId == achievementTypeCache.Id ) )
                .ToList();
        }

        /// <summary>
        /// Returns a collection of Achievement Types that can be selected as prerequisites of a new Achievement Type.
        /// An Achievement Type cannot be a prerequisite of itself, or of any Achievement Type that has it as a prerequisite.
        /// </summary>
        /// <returns></returns>
        public static List<AchievementTypeCache> GetEligiblePrerequisiteAchievementTypeCachesForNewAchievement( int achieverEntityTypeId )
        {
            // Get achievement types of which the specified achievement type is not already a prerequisite.
            return AchievementTypeCache.All()
                .Where( at =>
                    at.IsActive &&
                    at.AchieverEntityTypeId == achieverEntityTypeId )
                .ToList();
        }

        #endregion Prerequisites
    }

    #region Helper Classes

    /// <summary>
    /// Statement of Progress for an Achievement Type
    /// </summary>
    public class ProgressStatement: ILiquidizable, ILavaDataDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressStatement" /> class.
        /// </summary>
        /// <param name="achievementTypeCache">The streak type achievement type cache.</param>
        public ProgressStatement( AchievementTypeCache achievementTypeCache )
        {
            UnmetPrerequisites = new List<ProgressStatement>();
            AchievementTypeId = achievementTypeCache.Id;
            AchievementTypeName = achievementTypeCache.Name;
            AchievementTypeDescription = achievementTypeCache.Description;
            Attributes = achievementTypeCache.AttributeValues?
                .Where( kvp => kvp.Key != "Active" && kvp.Key != "Order" )
                .ToDictionary( kvp => kvp.Key, kvp => kvp.Value.Value );
        }

        /// <summary>
        /// Gets or sets the streak type achievement type identifier.
        /// </summary>
        [LavaVisible]
        public int AchievementTypeId { get; }

        /// <summary>
        /// Gets or sets the streak type achievement type identifier.
        /// </summary>
        [Obsolete( "Use AchievementTypeId instead." )]
        [RockObsolete( "1.12" )]
        public int StreakTypeAchievementTypeId => AchievementTypeId;

        /// <summary>
        /// Gets or sets the name of the streak type achievement type.
        /// </summary>
        [LavaVisible]
        public string AchievementTypeName { get; }

        /// <summary>
        /// Gets or sets the name of the streak type achievement type.
        /// </summary>
        [Obsolete( "Use AchievementTypeName instead." )]
        [RockObsolete( "1.12" )]
        public string StreakTypeAchievementTypeName => AchievementTypeName;

        /// <summary>
        /// Gets or sets the streak type achievement type description.
        /// </summary>
        [LavaVisible]
        public string AchievementTypeDescription { get; }

        /// <summary>
        /// Gets or sets the streak type achievement type description.
        /// </summary>
        [Obsolete( "Use AchievementTypeDescription instead." )]
        [RockObsolete( "1.12" )]
        public string StreakTypeAchievementTypeDescription => AchievementTypeDescription;

        /// <summary>
        /// Gets or sets the success count.
        /// </summary>
        [LavaVisible]
        public int SuccessCount { get; set; }

        /// <summary>
        /// Gets or sets the attempt count.
        /// </summary>
        [LavaVisible]
        public int AttemptCount { get; set; }

        /// <summary>
        /// Gets or sets the best attempt.
        /// </summary>
        [LavaVisible]
        public AchievementAttempt BestAttempt { get; set; }

        /// <summary>
        /// Gets or sets the most recent attempt.
        /// </summary>
        [LavaVisible]
        public AchievementAttempt MostRecentAttempt { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        [LavaVisible]
        public Dictionary<string, string> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the unmet prerequisites.
        /// </summary>
        /// <value>
        /// The unmet prerequisites.
        /// </value>
        [LavaVisible]
        public List<ProgressStatement> UnmetPrerequisites { get; }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object GetValue( string key )
        {
            return this[key];
        }

        #region ILiquidizable

        /// <summary>
        /// Creates a DotLiquid compatible dictionary that represents the current entity object. 
        /// </summary>
        /// <returns>DotLiquid compatible dictionary.</returns>
        public object ToLiquid()
        {
            return this;
        }

        /// <summary>
        /// Gets the available keys (for debugging info).
        /// </summary>
        /// <value>
        /// The available keys.
        /// </value>
        [LavaHidden]
        public virtual List<string> AvailableKeys
        {
            get
            {
                var availableKeys = new List<string>();

                foreach ( var propInfo in GetType().GetProperties() )
                {
                    if ( propInfo != null && LiquidizableProperty( propInfo ) )
                    {
                        availableKeys.Add( propInfo.Name );
                    }
                }

                return availableKeys;
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object GetValue( object key )
        {
            return this[key];
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public virtual bool ContainsKey( string key )
        {
            string propertyKey = key.ToStringSafe();
            var propInfo = GetType().GetProperty( propertyKey );
            if ( propInfo != null && LiquidizableProperty( propInfo ) )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [LavaHidden]
        public virtual object this[object key]
        {
            get
            {
                string propertyKey = key.ToStringSafe();
                var propInfo = GetType().GetProperty( propertyKey );

                try
                {
                    object propValue = null;
                    if ( propInfo != null && LiquidizableProperty( propInfo ) )
                    {
                        propValue = propInfo.GetValue( this, null );
                    }

                    if ( propValue is Guid )
                    {
                        return ( ( Guid ) propValue ).ToString();
                    }
                    else
                    {
                        return propValue;
                    }
                }
                catch
                {
                    // intentionally ignore
                }

                return null;
            }
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public virtual bool ContainsKey( object key )
        {
            string propertyKey = key.ToStringSafe();
            var propInfo = GetType().GetProperty( propertyKey );
            if ( propInfo != null && LiquidizableProperty( propInfo ) )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the property is available to Lava
        /// </summary>
        /// <param name="propInfo">The property information.</param>
        /// <returns></returns>
        private bool LiquidizableProperty( PropertyInfo propInfo )
        {
            return LavaHelper.IsLavaProperty( propInfo );
        }

        #endregion ILiquidizable
    }

    #endregion Helper Classes
}