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
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a step type in Rock.
    /// </summary>
    [RockDomain( "Steps" )]
    [Table( "StepType" )]
    [DataContract]
    public partial class StepType : Model<StepType>, IOrdered, IHasActiveFlag
    {
        #region Constants

        private const string _defaultCardLavaTemplate =
@"<div class=""card-top"">
    <h3 class=""step-name"">{{ StepType.Name }}</h3>
</div>
<div class=""card-middle"">
    {% if StepType.HighlightColor == '' or IsComplete == false %}
        <i class=""{{ StepType.IconCssClass }} fa-4x""></i>
    {% else %}
        <i class=""{{ StepType.IconCssClass }} fa-4x"" style=""color: {{ StepType.HighlightColor }};""></i>
    {% endif %}
</div>
<div class=""card-bottom"">
    <p class=""step-status"">
        {% if LatestStepStatus %}
            <span class=""label"" style=""background-color: {{ LatestStepStatus.StatusColor }};"">{{ LatestStepStatus.Name }}</span>
        {% endif %}
        {% if ShowCampus and LatestStep and LatestStep.Campus != '' %}
            <span class=""label label-campus"">{{ LatestStep.Campus.Name }}</span>
        {% endif %}
        {% if LatestStep and LatestStep.CompletedDateTime != '' %}
            <br />
            <small>{{ LatestStep.CompletedDateTime | Date:'M/d/yyyy' }}</small>
        {% endif %}
    </p>
    {% if StepCount > 1 %}
        <span class=""badge"">{{ StepCount }}</span>
    {% endif %}
</div>
";

        #endregion Constants

        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="StepProgram"/> to which this step type belongs. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int StepProgramId { get; set; }

        /// <summary>
        /// Gets or sets the name of the step type. This property is required.
        /// </summary>
        [MaxLength( 250 )]
        [DataMember( IsRequired = true )]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of the step type.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        [MaxLength( 100 )]
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this step type allows multiple step records per person.
        /// </summary>
        [DataMember]
        public bool AllowMultiple { get; set; } = true;

        /// <summary>
        /// Gets or sets a flag indicating if this step type happens over time (like being in a group) or is it achievement based (like attended a class).
        /// </summary>
        [DataMember]
        public bool HasEndDate { get; set; } = false;

        /// <summary>
        /// Gets or sets the Id of the <see cref="DataView"/> associated with this step type. The data view reveals the people that are allowed to be
        /// considered for this step type.
        /// </summary>
        [DataMember]
        public int? AudienceDataViewId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the number of occurences should be shown on the badge.
        /// </summary>
        [DataMember]
        public bool ShowCountOnBadge { get; set; } = true;

        /// <summary>
        /// Gets or sets the Id of the <see cref="DataView"/> associated with this step type. The data view reveals the people that should be considered
        /// as having completed this step.
        /// </summary>
        [DataMember]
        public int? AutoCompleteDataViewId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this item can be edited by a person.
        /// </summary>
        [DataMember]
        public bool AllowManualEditing { get; set; } = true;

        /// <summary>
        /// Gets or sets the highlight color for badges and cards.
        /// </summary>
        [MaxLength( 100 )]
        [DataMember]
        public string HighlightColor { get; set; }

        /// <summary>
        /// Gets or sets the lava template used to render custom card details.
        /// </summary>
        [DataMember]
        public string CardLavaTemplate
        {
            get
            {
                return _cardLavaTemplate.IsNullOrWhiteSpace() ? _defaultCardLavaTemplate : _cardLavaTemplate;
            }
            set
            {
                _cardLavaTemplate = value;
            }
        }
        private string _cardLavaTemplate;

        /// <summary>
        /// Gets or sets the Id of the <see cref="MergeTemplate"/> associated with this step type. This template can represent things like
        /// certificates or letters.
        /// </summary>
        [DataMember]
        public int? MergeTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the name used to describe the merge template (e.g. Certificate).
        /// </summary>
        [MaxLength( 50 )]
        [DataMember]
        public string MergeTemplateDescriptor { get; set; }

        #endregion Entity Properties

        #region IHasActiveFlag

        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        [DataMember]
        public bool IsActive { get; set; } = true;

        #endregion IHasActiveFlag

        #region IOrdered

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        [DataMember]
        public int Order { get; set; }

        #endregion IOrdered

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the Step Program.
        /// </summary>
        [DataMember]
        public virtual StepProgram StepProgram { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Step">Steps</see> that are of this step type.
        /// </summary>
        [DataMember]
        public virtual ICollection<Step> Steps
        {
            get => _steps ?? ( _steps = new Collection<Step>() );
            set => _steps = value;
        }
        private ICollection<Step> _steps;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="StepTypePrerequisite">Prerequisites</see> for this step type. These are StepTypes
        /// that must be completed prior to this step type.
        /// </summary>
        [DataMember]
        public virtual ICollection<StepTypePrerequisite> StepTypePrerequisites
        {
            get => _stepTypePrerequisites ?? ( _stepTypePrerequisites = new Collection<StepTypePrerequisite>() );
            set => _stepTypePrerequisites = value;
        }
        private ICollection<StepTypePrerequisite> _stepTypePrerequisites;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="StepTypePrerequisite">Dependencies</see> for this step type. These are StepTypes
        /// where this StepType is a prerequisite. These are step types that require this step type to be completed before that step type can
        /// be completed.
        /// </summary>
        [DataMember]
        public virtual ICollection<StepTypePrerequisite> StepTypeDependencies
        {
            get => _stepTypeDependencies ?? ( _stepTypeDependencies = new Collection<StepTypePrerequisite>() );
            set => _stepTypeDependencies = value;
        }
        private ICollection<StepTypePrerequisite> _stepTypeDependencies;

        /// <summary>
        /// Gets or sets the Data View.  The data view reveals the people that are allowed to be
        /// considered for this step type.
        /// </summary>
        [DataMember]
        public virtual DataView AudienceDataView { get; set; }

        /// <summary>
        /// Gets or sets the Data View.  The data view reveals the people that should be considered
        /// as having completed this step.
        /// </summary>
        [DataMember]
        public virtual DataView AutoCompleteDataView { get; set; }

        /// <summary>
        /// Gets or sets the Merge Template.  This template can represent things like
        /// certificates or letters.
        /// </summary>
        [DataMember]
        public virtual MergeTemplate MergeTemplate { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="StepWorkflowTrigger">StepWorkflowTriggers</see> that are of this step type.
        /// </summary>
        [DataMember]
        public virtual ICollection<StepWorkflowTrigger> StepWorkflowTriggers
        {
            get => _stepWorkflowTriggers ?? ( _stepWorkflowTriggers = new Collection<StepWorkflowTrigger>() );
            set => _stepWorkflowTriggers = value;
        }
        private ICollection<StepWorkflowTrigger> _stepWorkflowTriggers;

        /// <summary>
        /// Gets or sets the achievement types.
        /// </summary>
        /// <value>
        /// The streak type achievement types.
        /// </value>
        [DataMember]
        public virtual ICollection<AchievementType> AchievementTypes
        {
            get => _achievementTypes ?? ( _achievementTypes = new Collection<AchievementType>() );
            set => _achievementTypes = value;
        }
        private ICollection<AchievementType> _achievementTypes;

        #endregion Virtual Properties

        #region Entity Configuration

        /// <summary>
        /// Step Type Configuration class.
        /// </summary>
        public partial class StepTypeConfiguration : EntityTypeConfiguration<StepType>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StepTypeConfiguration"/> class.
            /// </summary>
            public StepTypeConfiguration()
            {
                HasRequired( st => st.StepProgram ).WithMany( sp => sp.StepTypes ).HasForeignKey( st => st.StepProgramId ).WillCascadeOnDelete( true );

                HasOptional( st => st.AudienceDataView ).WithMany().HasForeignKey( st => st.AudienceDataViewId ).WillCascadeOnDelete( false );
                HasOptional( st => st.AutoCompleteDataView ).WithMany().HasForeignKey( st => st.AutoCompleteDataViewId ).WillCascadeOnDelete( false );
                HasOptional( st => st.MergeTemplate ).WithMany().HasForeignKey( st => st.MergeTemplateId ).WillCascadeOnDelete( false );
            }
        }

        #endregion
    }
}
