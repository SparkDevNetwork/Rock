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
using System.Runtime.Serialization;
using Rock.Achievement;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cache object for <see cref="StreakTypeAchievementType" />
    /// </summary>
    [Serializable]
    [DataContract]
    public class StreakTypeAchievementTypeCache : ModelCache<StreakTypeAchievementTypeCache, StreakTypeAchievementType>
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
        /// Gets or sets the Id of the <see cref="Model.StreakType"/> to which this StreakTypeAchievementType belongs. This property is required.
        /// </summary>
        [DataMember]
        public int StreakTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the Id of the achievement component <see cref="EntityType"/>
        /// </summary>
        [DataMember]
        public int AchievementEntityTypeId { get; private set; }

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

        #region Related Cache Objects

        /// <summary>
        /// Gets the Achievement Component Entity Type Cache.
        /// </summary>
        public EntityTypeCache AchievementEntityType
        {
            get => EntityTypeCache.Get( AchievementEntityTypeId );
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
        /// Gets the Streak Type Cache.
        /// </summary>
        public StreakTypeCache StreakTypeCache
        {
            get => StreakTypeCache.Get( StreakTypeId );
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
        public List<StreakTypeAchievementTypePrerequisiteCache> Prerequisites
            => StreakTypeAchievementTypePrerequisiteCache.All().Where( statp => statp.StreakTypeAchievementTypeId == Id ).ToList();

        /// <summary>
        /// Gets the prerequisite achievement types.
        /// </summary>
        /// <value>
        /// The prerequisite achievement types.
        /// </value>
        public List<StreakTypeAchievementTypeCache> PrerequisiteAchievementTypes
            => Prerequisites.Select( statp => statp.PrerequisiteStreakTypeAchievementType ).ToList();

        #endregion Related Cache Objects

        #region Public Methods

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity"></param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );
            var streakTypeAchievementType = entity as StreakTypeAchievementType;

            if ( streakTypeAchievementType == null )
            {
                return;
            }

            Name = streakTypeAchievementType.Name;
            Description = streakTypeAchievementType.Description;
            IsActive = streakTypeAchievementType.IsActive;
            StreakTypeId = streakTypeAchievementType.StreakTypeId;
            AchievementEntityTypeId = streakTypeAchievementType.AchievementEntityTypeId;
            AchievementStartWorkflowTypeId = streakTypeAchievementType.AchievementStartWorkflowTypeId;
            AchievementFailureWorkflowTypeId = streakTypeAchievementType.AchievementFailureWorkflowTypeId;
            AchievementSuccessWorkflowTypeId = streakTypeAchievementType.AchievementSuccessWorkflowTypeId;
            AchievementStepTypeId = streakTypeAchievementType.AchievementStepTypeId;
            AchievementStepStatusId = streakTypeAchievementType.AchievementStepStatusId;
            BadgeLavaTemplate = streakTypeAchievementType.BadgeLavaTemplate;
            ResultsLavaTemplate = streakTypeAchievementType.ResultsLavaTemplate;
            AchievementIconCssClass = streakTypeAchievementType.AchievementIconCssClass;
            MaxAccomplishmentsAllowed = streakTypeAchievementType.MaxAccomplishmentsAllowed;
            AllowOverAchievement = streakTypeAchievementType.AllowOverAchievement;
            CategoryId = streakTypeAchievementType.CategoryId;
        }

        #endregion Public Methods
    }
}
