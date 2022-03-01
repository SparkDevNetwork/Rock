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
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a step program in Rock.
    /// </summary>
    [RockDomain( "Engagement" )]
    [Table( "StepProgram" )]
    [DataContract]
    public partial class StepProgram : Model<StepProgram>, IOrdered, IHasActiveFlag, ICacheable
    {
        #region Constants

        private const string DefaultStepTerm = "Step";

        #endregion Constants

        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the program. This property is required.
        /// </summary>
        [MaxLength( 250 )]
        [DataMember( IsRequired = true )]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of the program.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        private string _stepTerm;

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        [MaxLength( 100 )]
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Category"/>.
        /// </summary>
        /// [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the default view mode for the program. This value is required.
        /// </summary>
        [DataMember]
        public ViewMode DefaultListView { get; set; } = ViewMode.Cards;

        /// <summary>
        /// Gets or sets the term used for steps within this program. This property is required.
        /// </summary>
        [MaxLength( 100 )]
        [DataMember]
        public string StepTerm
        {
            get => _stepTerm.IsNullOrWhiteSpace() ? DefaultStepTerm : _stepTerm;
            set => _stepTerm = value;
        }

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

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Category"/>.
        /// </summary>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="StepStatus">StepStatuses</see> that this Program utilizes.
        /// </summary>
        [DataMember]
        public virtual ICollection<StepStatus> StepStatuses
        {
            get => _stepStatuses ?? ( _stepStatuses = new Collection<StepStatus>() );
            set => _stepStatuses = value;
        }

        private ICollection<StepStatus> _stepStatuses;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="StepType">Step Types</see> that are of this program.
        /// </summary>
        [DataMember]
        public virtual ICollection<StepType> StepTypes
        {
            get => _stepTypes ?? ( _stepTypes = new Collection<StepType>() );
            set => _stepTypes = value;
        }

        private ICollection<StepType> _stepTypes;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="StepWorkflowTrigger">StepWorkflowTriggers</see> that are of this step program.
        /// </summary>
        [DataMember]
        public virtual ICollection<StepWorkflowTrigger> StepWorkflowTriggers
        {
            get => _stepWorkflowTriggers ?? ( _stepWorkflowTriggers = new Collection<StepWorkflowTrigger>() );
            set => _stepWorkflowTriggers = value;
        }

        private ICollection<StepWorkflowTrigger> _stepWorkflowTriggers;

        #endregion Navigation Properties

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> containing the Location's Name that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> containing the Location's Name that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion Public Methods

        #region Entity Configuration

        /// <summary>
        /// Step Program Configuration class.
        /// </summary>
        public partial class StepProgramConfiguration : EntityTypeConfiguration<StepProgram>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StepProgramConfiguration"/> class.
            /// </summary>
            public StepProgramConfiguration()
            {
                HasOptional( sp => sp.Category ).WithMany().HasForeignKey( sp => sp.CategoryId ).WillCascadeOnDelete( false );
            }
        }

        #endregion Entity Configuration

        #region Enumerations

        /// <summary>
        /// Represents the view mode of a <see cref="StepProgram"/>.
        /// </summary>
        public enum ViewMode
        {
            /// <summary>
            /// The <see cref="StepProgram"/> is viewed as cards.
            /// </summary>
            Cards = 0,

            /// <summary>
            /// The <see cref="StepProgram"/> is viewed as a grid.
            /// </summary>
            Grid = 1
        }

        #endregion Enumerations
    }
}
