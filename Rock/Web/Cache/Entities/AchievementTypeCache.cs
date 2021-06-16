﻿// <copyright>
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
using System.Reflection;
using System.Runtime.Serialization;
using Rock.Achievement;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cache object for <see cref="AchievementType" />
    /// </summary>
    [Serializable]
    [DataContract]
    public class AchievementTypeCache : ModelCache<AchievementTypeCache, AchievementType>
    {
        #region Entity Properties

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets the achiever entity type id.
        /// </summary>
        [DataMember]
        public int AchieverEntityTypeId { get; private set; }

        /// <summary>
        /// Gets the source entity type id.
        /// </summary>
        [DataMember]
        public int? SourceEntityTypeId { get; private set; }

        /// <summary>
        /// Gets the component config JSON.
        /// </summary>
        [DataMember]
        public string ComponentConfigJson { get; private set; }

        /// <summary>
        /// Gets or sets the Id of the component <see cref="EntityType"/>
        /// </summary>
        [DataMember]
        public int ComponentEntityTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="WorkflowType"/> to be triggered when an achievement is started
        /// </summary>
        [DataMember]
        public int? AchievementStartWorkflowTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="WorkflowType"/> to be triggered when an achievement is failed (closed and not successful)
        /// </summary>
        [DataMember]
        public int? AchievementFailureWorkflowTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="WorkflowType"/> to be triggered when an achievement is successful
        /// </summary>
        [DataMember]
        public int? AchievementSuccessWorkflowTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="StepType"/> of which a <see cref="Step"/> will be created when an achievement is completed
        /// </summary>
        [DataMember]
        public int? AchievementStepTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="StepStatus"/> of which a <see cref="Step"/> will be created when an achievement is completed
        /// </summary>
        [DataMember]
        public int? AchievementStepStatusId { get; private set; }

        /// <summary>
        /// Gets or sets the lava template used to render a badge.
        /// </summary>
        [DataMember]
        public string BadgeLavaTemplate { get; private set; }

        /// <summary>
        /// Gets or sets the lava template used to render results.
        /// </summary>
        [DataMember]
        public string ResultsLavaTemplate { get; private set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        [DataMember]
        public string AchievementIconCssClass { get; private set; }

        /// <summary>
        /// Gets or sets the maximum accomplishments allowed.
        /// </summary>
        /// <value>
        /// The maximum accomplishments allowed.
        /// </value>
        [DataMember]
        public int? MaxAccomplishmentsAllowed { get; private set; }

        /// <summary>
        /// Gets or sets whether over achievement is allowed. This cannot be true if <see cref="MaxAccomplishmentsAllowed"/> is greater than 1.
        /// </summary>
        /// <value>
        /// The allow over achievement.
        /// </value>
        [DataMember]
        public bool AllowOverAchievement { get; private set; }

        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        [DataMember]
        public int? CategoryId { get; private set; }

        #endregion Entity Properties

        #region IHasActiveFlag

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; private set; }

        #endregion IHasActiveFlag

        #region Related Entity Helpers

        /// <summary>
        /// Gets the source entity type cache.
        /// </summary>
        /// <value>
        /// The source entity type cache.
        /// </value>
        public EntityTypeCache SourceEntityTypeCache
        {
            get => SourceEntityTypeId.HasValue ?
                EntityTypeCache.Get( SourceEntityTypeId.Value ) :
                null;
        }

        /// <summary>
        /// Gets the achiever entity type cache.
        /// </summary>
        /// <value>
        /// The achiever entity type cache.
        /// </value>
        public EntityTypeCache AchieverEntityTypeCache
        {
            get => EntityTypeCache.Get( AchieverEntityTypeId );
        }

        #endregion Related Entity Helpers

        #region Related Cache Objects

        /// <summary>
        /// Gets the Achievement Component Entity Type Cache.
        /// </summary>
        public EntityTypeCache AchievementEntityType
        {
            get => EntityTypeCache.Get( ComponentEntityTypeId );
        }

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public CategoryCache Category
        {
            get => CategoryId.HasValue ? CategoryCache.Get( CategoryId.Value ) : null;
        }

        /// <summary>
        /// Gets the achievement component.
        /// </summary>
        /// <value>
        /// The badge component.
        /// </value>
        public virtual AchievementComponent AchievementComponent
        {
            get => AchievementEntityType != null ? AchievementContainer.GetComponent( AchievementEntityType.Name ) : null;
        }

        /// <summary>
        /// Gets the prerequisites.
        /// </summary>
        /// <value>
        /// The prerequisites.
        /// </value>
        public List<AchievementTypePrerequisiteCache> Prerequisites
            => AchievementTypePrerequisiteCache.All().Where( statp => statp.AchievementTypeId == Id ).ToList();

        /// <summary>
        /// Gets the prerequisite achievement types.
        /// </summary>
        /// <value>
        /// The prerequisite achievement types.
        /// </value>
        public List<AchievementTypeCache> PrerequisiteAchievementTypes
            => Prerequisites.Select( statp => statp.PrerequisiteAchievementType ).ToList();

        #endregion Related Cache Objects

        #region Public Methods

        /// <summary>
        /// Gets the component configuration.
        /// </summary>
        /// <value>
        /// The component configuration.
        /// </value>
        public Dictionary<string, string> ComponentConfig =>
            ComponentConfigJson.FromJsonOrNull<Dictionary<string, string>>() ??
                new Dictionary<string, string>();

        /// <summary>
        /// Gets the component configuration value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetComponentConfigValue(string key)
        {
            return ComponentConfig.GetValueOrNull( key );
        }

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity"></param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );
            var achievementType = entity as AchievementType;

            if ( achievementType == null )
            {
                return;
            }

            Name = achievementType.Name;
            Description = achievementType.Description;
            IsActive = achievementType.IsActive;
            AchieverEntityTypeId = achievementType.AchieverEntityTypeId;
            SourceEntityTypeId = achievementType.SourceEntityTypeId;
            ComponentConfigJson = achievementType.ComponentConfigJson;
            ComponentEntityTypeId = achievementType.ComponentEntityTypeId;
            AchievementStartWorkflowTypeId = achievementType.AchievementStartWorkflowTypeId;
            AchievementFailureWorkflowTypeId = achievementType.AchievementFailureWorkflowTypeId;
            AchievementSuccessWorkflowTypeId = achievementType.AchievementSuccessWorkflowTypeId;
            AchievementStepTypeId = achievementType.AchievementStepTypeId;
            AchievementStepStatusId = achievementType.AchievementStepStatusId;
            BadgeLavaTemplate = achievementType.BadgeLavaTemplate;
            ResultsLavaTemplate = achievementType.ResultsLavaTemplate;
            AchievementIconCssClass = achievementType.AchievementIconCssClass;
            MaxAccomplishmentsAllowed = achievementType.MaxAccomplishmentsAllowed;
            AllowOverAchievement = achievementType.AllowOverAchievement;
            CategoryId = achievementType.CategoryId;
        }

        /// <summary>
        /// Gets the by source entity.
        /// </summary>
        /// <param name="sourceEntity">The entity.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static List<AchievementTypeCache> GetBySourceEntity( IEntity sourceEntity )
        {
            if ( sourceEntity == null )
            {
                return new List<AchievementTypeCache>();
            }

            var sourceEntityTypeCache = EntityTypeCache.Get( sourceEntity.TypeId );

            if ( sourceEntityTypeCache?.IsAchievementsEnabled != true )
            {
                return new List<AchievementTypeCache>();
            }

            return All()
                .Where( at =>
                    at.SourceEntityTypeId == sourceEntityTypeCache.Id &&
                    at.IsActive )
                .Where( at => {
                    var component = at.AchievementComponent;
                    return component.ShouldProcess( at, sourceEntity );
                } )
                .ToList();
        }

        /// <summary>
        /// Processes achievements, returning any resulting changed or created <see cref="AchievementAttempt"/>s.
        /// </summary>
        /// <param name="sourceEntity">The updated entity.</param>
        public static List<AchievementAttempt> ProcessAchievements( IEntity sourceEntity )
        {
            if ( sourceEntity == null )
            {
                return new List<AchievementAttempt>();
            }

            var updatedAttempts = new Dictionary<int, AchievementAttempt>();

            /*
             * 2019-01-13 BJW
             *
             * Achievements need to be processed in order according to dependencies (prerequisites). Prerequisites should be processed first so that,
             * if the prerequisite becomes completed, the dependent achievement will be processed at this time as well. Furthermore, each achievement
             * needs to be processed and changes saved to the database so that subsequent achievements will see the changes (for example: now met
             * prerequisites).
             */
            var achievementTypes = GetBySourceEntity( sourceEntity );
            var sortedAchievementTypes = AchievementTypeService.SortAccordingToPrerequisites( achievementTypes );

            foreach ( var achievementTypeCache in sortedAchievementTypes )
            {
                var loopRockContext = new RockContext();
                var component = achievementTypeCache.AchievementComponent;
                var loopUpdatedAttempts = component.Process( loopRockContext, achievementTypeCache, sourceEntity );
                loopRockContext.SaveChanges();

                foreach ( var attempt in loopUpdatedAttempts )
                {
                    updatedAttempts[attempt.Id] = attempt;
                }
            }

            return updatedAttempts.Values.ToList();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance Title.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance Title.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion Public Methods
    }
}
