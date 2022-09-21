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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Interactive Experience Action.
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "InteractiveExperienceAction" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "8635E7E7-3576-47FF-92DE-30A69EB5D011" )]
    public partial class InteractiveExperienceAction : Model<InteractiveExperienceAction>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.InteractiveExperience"/> that this InteractiveExperienceAction is associated with. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.InteractiveExperience"/> that the InteractiveExperienceAction is associated with.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int InteractiveExperienceId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of action. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of action.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int ActionEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of the response visual.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of the response visual.
        /// </value>
        [DataMember]
        public int? ResponseVisualEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is moderation required.
        /// </summary>
        /// <value><c>true</c> if this instance is moderation required; otherwise, <c>false</c>.</value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsModerationRequired { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if multiple submission allowed.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if multiple submission allowed; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsMultipleSubmissionAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if response anonymous.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if response anonymous; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsResponseAnonymous { get; set; }

        /// <summary>
        /// Gets or sets the action settings json.
        /// </summary>
        /// <value>
        /// The action settings json.
        /// </value>
        [DataMember]
        public string ActionSettingsJson { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.InteractiveExperience"/> that the InteractiveExperienceAction belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.InteractiveExperience"/> representing the InteractiveExperience that the InteractiveExperienceSchedule is a part of.
        /// </value>
        [DataMember]
        public virtual InteractiveExperience InteractiveExperience { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> of the action.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> of the action.
        /// </value>
        [DataMember]
        public virtual EntityType ActionEntityType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> of the response visual.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> of the response visual.
        /// </value>
        [DataMember]
        public virtual EntityType ResponseVisualEntityType { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Interactive Experience Action Configuration class.
    /// </summary>
    public partial class InteractiveExperienceActionConfiguration : EntityTypeConfiguration<InteractiveExperienceAction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveExperienceActionConfiguration" /> class.
        /// </summary>
        public InteractiveExperienceActionConfiguration()
        {
            this.HasRequired( iea => iea.InteractiveExperience ).WithMany( ie => ie.InteractiveExperienceActions ).HasForeignKey( iea => iea.InteractiveExperienceId ).WillCascadeOnDelete( true );
            this.HasRequired( iea => iea.ActionEntityType ).WithMany().HasForeignKey( iea => iea.ActionEntityTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( iea => iea.ResponseVisualEntityType ).WithMany().HasForeignKey( iea => iea.ResponseVisualEntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}