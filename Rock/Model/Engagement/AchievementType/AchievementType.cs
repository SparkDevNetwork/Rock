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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents an Achievement Type in Rock.
    /// </summary>
    [RockDomain( "Engagement" )]
    [Table( "AchievementType" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.ACHIEVEMENT_TYPE )]
    public partial class AchievementType : Model<AchievementType>, IHasActiveFlag, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the achievement type. This property is required.
        /// </summary>
        [MaxLength( 250 )]
        [DataMember( IsRequired = true )]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of the achievement type.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the configuration from the <see cref="ComponentEntityTypeId"/>.
        /// </summary>
        [DataMember]
        public string ComponentConfigJson { get; set; }

        /// <summary>
        /// Gets or sets the source entity type. The source supplies the data framework from which achievements are computed.
        /// The original achievement sources were <see cref="Streak">Streaks</see>.
        /// </summary>
        [DataMember]
        public int? SourceEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the achiever entity type. The achiever is the object that earns the achievement.
        /// The original achiever was a <see cref="PersonAlias"/> via <see cref="Streak.PersonAliasId"/>.
        /// </summary>
        [DataMember( IsRequired = true )]
        [Required]
        public int AchieverEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the achievement component <see cref="EntityType"/>
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int ComponentEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="WorkflowType"/> to be triggered when an achievement is started
        /// </summary>
        [DataMember]
        public int? AchievementStartWorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="WorkflowType"/> to be triggered when an achievement is successful
        /// </summary>
        [DataMember]
        public int? AchievementSuccessWorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="WorkflowType"/> to be triggered when an achievement is failed (closed and not successful)
        /// </summary>
        [DataMember]
        public int? AchievementFailureWorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="StepType"/> of which a <see cref="Step"/> will be created when an achievement is completed
        /// </summary>
        [DataMember]
        public int? AchievementStepTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="StepStatus"/> of which a <see cref="Step"/> will be created when an achievement is completed
        /// </summary>
        [DataMember]
        public int? AchievementStepStatusId { get; set; }

        /// <summary>
        /// Gets or sets the lava template used to render a badge.
        /// </summary>
        [DataMember]
        public string BadgeLavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets the lava template used to render results.
        /// </summary>
        [DataMember]
        public string ResultsLavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        [MaxLength( 100 )]
        [DataMember]
        public string AchievementIconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the maximum accomplishments allowed.
        /// </summary>
        /// <value>
        /// The maximum accomplishments allowed.
        /// </value>
        [Range( 1, int.MaxValue )]
        [DataMember]
        public int? MaxAccomplishmentsAllowed { get; set; } = 1;

        /// <summary>
        /// Gets or sets whether over achievement is allowed. This cannot be true if <see cref="MaxAccomplishmentsAllowed"/> is greater than 1.
        /// </summary>
        /// <value>
        /// The allow over achievement.
        /// </value>
        [DataMember]
        public bool AllowOverAchievement { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Model.Category"/> identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is public.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is public; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets the image binary file identifier. This would be the image
        /// that would be shown in the achievement summary (for example, a trophy).
        /// </summary>
        /// <value>
        /// The image binary file identifier.
        /// </value>
        [DataMember]
        public int? ImageBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the lava template used to render the status summary of the achievement.
        /// </summary>
        /// <value>
        /// The custom summary lava template.
        /// </value>
        [DataMember]
        public string CustomSummaryLavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets the color of the highlight.
        /// </summary>
        /// <value>
        /// The color of the highlight.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string HighlightColor { get; set; }

        /// <summary>
        /// An alternate image that can be used for custom purposes.
        /// </summary>
        /// <value>
        /// The image binary file identifier.
        /// </value>
        [DataMember]
        public int? AlternateImageBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the target count of things that must be done for this
        /// achievement to be considered accomplished.
        /// </summary>
        /// <value>
        /// The number of things that must be accomplished to complete this achievement or <c>null</c> if not known.
        /// </value>
        public int? TargetCount { get; set; }

        #endregion Entity Properties

        #region IHasActiveFlag

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; }

        #endregion IHasActiveFlag

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="EntityType"/> of the achievement.
        /// </summary>
        [DataMember]
        public virtual EntityType AchievementEntityType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="WorkflowType"/> to be launched when the achievement starts.
        /// </summary>
        [DataMember]
        public virtual WorkflowType AchievementStartWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="WorkflowType"/> to be launched when the achievement is successful.
        /// </summary>
        [DataMember]
        public virtual WorkflowType AchievementSuccessWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="WorkflowType"/> to be launched when the achievement is failed (closed and not successful).
        /// </summary>
        [DataMember]
        public virtual WorkflowType AchievementFailureWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="StepType"/> to be created when the achievement is completed.
        /// </summary>
        [DataMember]
        public virtual StepType AchievementStepType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="StepStatus"/> to be used for the <see cref="StepType"/> created when the achievement is completed.
        /// </summary>
        [DataMember]
        public virtual StepStatus AchievementStepStatus { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Model.Category"/>.
        /// </summary>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the image binary file.
        /// </summary>
        /// <value>
        /// The image binary file.
        /// </value>
        [DataMember]
        public virtual BinaryFile ImageBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the attempts.
        /// </summary>
        /// <value>
        /// The attempts.
        /// </value>
        [DataMember]
        [JsonIgnore]
        public virtual ICollection<AchievementAttempt> Attempts
        {
            get => _attempts ?? ( _attempts = new Collection<AchievementAttempt>() );
            set => _attempts = value;
        }

        private ICollection<AchievementAttempt> _attempts;

        /// <summary>
        /// Gets or sets the prerequisites.
        /// </summary>
        /// <value>
        /// The prerequisites.
        /// </value>
        [DataMember]
        public virtual ICollection<AchievementTypePrerequisite> Prerequisites
        {
            get => _prerequisites ?? ( _prerequisites = new Collection<AchievementTypePrerequisite>() );
            set => _prerequisites = value;
        }

        private ICollection<AchievementTypePrerequisite> _prerequisites;

        /// <summary>
        /// Gets or sets the dependencies.
        /// </summary>
        /// <value>
        /// The dependencies.
        /// </value>
        [DataMember]
        public virtual ICollection<AchievementTypePrerequisite> Dependencies
        {
            get => _dependencies ?? ( _dependencies = new Collection<AchievementTypePrerequisite>() );
            set => _dependencies = value;
        }

        private ICollection<AchievementTypePrerequisite> _dependencies;

        /// <summary>
        /// Gets or sets the alternate image binary file.
        /// </summary>
        /// <value>
        /// The image binary file.
        /// </value>
        [DataMember]
        public virtual BinaryFile AlternateImageBinaryFile { get; set; }

        #endregion Navigation Properties

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the Person's FullName that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the Person's FullName that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region Entity Configuration

        /// <summary>
        /// Achievement Type Configuration class.
        /// </summary>
        public partial class AchievementTypeConfiguration : EntityTypeConfiguration<AchievementType>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AchievementTypeConfiguration"/> class.
            /// </summary>
            public AchievementTypeConfiguration()
            {
                HasRequired( stat => stat.AchievementEntityType ).WithMany().HasForeignKey( stat => stat.ComponentEntityTypeId ).WillCascadeOnDelete( true );

                HasOptional( stat => stat.AchievementStartWorkflowType ).WithMany().HasForeignKey( stat => stat.AchievementStartWorkflowTypeId ).WillCascadeOnDelete( false );
                HasOptional( stat => stat.AchievementSuccessWorkflowType ).WithMany().HasForeignKey( stat => stat.AchievementSuccessWorkflowTypeId ).WillCascadeOnDelete( false );
                HasOptional( stat => stat.AchievementFailureWorkflowType ).WithMany().HasForeignKey( stat => stat.AchievementFailureWorkflowTypeId ).WillCascadeOnDelete( false );
                HasOptional( stat => stat.AchievementStepType ).WithMany( st => st.AchievementTypes ).HasForeignKey( stat => stat.AchievementStepTypeId ).WillCascadeOnDelete( false );
                HasOptional( stat => stat.AchievementStepStatus ).WithMany().HasForeignKey( stat => stat.AchievementStepStatusId ).WillCascadeOnDelete( false );
                HasOptional( stat => stat.Category ).WithMany().HasForeignKey( stat => stat.CategoryId ).WillCascadeOnDelete( false );
                HasOptional( stat => stat.ImageBinaryFile ).WithMany().HasForeignKey( stat => stat.ImageBinaryFileId ).WillCascadeOnDelete( false );
                HasOptional( stat => stat.AlternateImageBinaryFile ).WithMany().HasForeignKey( stat => stat.AlternateImageBinaryFileId ).WillCascadeOnDelete( false );
            }
        }

        #endregion Entity Configuration
    }
}
