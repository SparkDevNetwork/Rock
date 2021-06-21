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
using System.Linq;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents a step workflow trigger in Rock.
    /// </summary>
    [RockDomain( "Engagement" )]
    [Table( "StepWorkflowTrigger" )]
    [DataContract]
    public partial class StepWorkflowTrigger : Model<StepWorkflowTrigger>, IHasActiveFlag
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.StepProgram"/> by which this Workflow is triggered.
        /// </summary>
        [DataMember]
        public int? StepProgramId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.StepType"/> by which this Workflow is triggered.
        /// </summary>
        [DataMember]
        public int? StepTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.WorkflowType"/> that is triggered. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int WorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the type of the trigger.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public WorkflowTriggerCondition TriggerType { get; set; }

        /// <summary>
        /// Gets or sets the type qualifier.
        /// </summary>
        [MaxLength( 200 )]
        [DataMember]
        public string TypeQualifier { get; set; }

        /// <summary>
        /// Gets or sets the name of the workflow trigger.
        /// </summary>
        [MaxLength( 100 )]
        [DataMember]
        public string WorkflowName { get; set; }

        #endregion Entity Properties

        #region IHasActiveFlag

        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        [DataMember]
        public bool IsActive { get; set; } = true;

        #endregion IHasActiveFlag

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.StepProgram"/>.
        /// </summary>
        [DataMember]
        public virtual StepProgram StepProgram { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.StepType"/>.
        /// </summary>
        [DataMember]
        public virtual StepType StepType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowType"/>.
        /// </summary>
        [DataMember]
        public virtual WorkflowType WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.StepWorkflow">StepWorkflows</see> that are of this trigger.
        /// </summary>
        [DataMember]
        public virtual ICollection<StepWorkflow> StepWorkflows
        {
            get => _stepWorkflows ?? ( _stepWorkflows = new Collection<StepWorkflow>() );
            set => _stepWorkflows = value;
        }
        private ICollection<StepWorkflow> _stepWorkflows;

        #endregion Virtual Properties

        #region Overrides

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        public override bool IsValid
        {
            get
            {
                var isValid = base.IsValid;

                if ( !StepProgramId.HasValue && !StepTypeId.HasValue )
                {
                    ValidationResults.Add( new ValidationResult( "The StepProgramId or the StepTypeId must be set" ) );
                    isValid = false;
                }

                return isValid;
            }
        }

        #endregion Overrides

        #region Entity Configuration

        /// <summary>
        /// Step Workflow Trigger Configuration class.
        /// </summary>
        public partial class StepWorkflowTriggerConfiguration : EntityTypeConfiguration<StepWorkflowTrigger>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StepWorkflowTriggerConfiguration"/> class.
            /// </summary>
            public StepWorkflowTriggerConfiguration()
            {
                // This has to be CascadeDelete false because otherwise SQL server doesn't like the possibility of dependency cycles
                HasRequired( swt => swt.WorkflowType ).WithMany().HasForeignKey( swt => swt.WorkflowTypeId ).WillCascadeOnDelete( false );

                // These both have to be CascadeDelete false because otherwise SQL server doesn't like the possibility of dependency cycles
                HasOptional( swt => swt.StepProgram ).WithMany( sp => sp.StepWorkflowTriggers ).HasForeignKey( swt => swt.StepProgramId ).WillCascadeOnDelete( false );
                HasOptional( swt => swt.StepType ).WithMany( st => st.StepWorkflowTriggers ).HasForeignKey( swt => swt.StepTypeId ).WillCascadeOnDelete( false );
            }
        }

        #endregion

        #region Enumerations

        /// <summary>
        /// Represents the type of trigger of a <see cref="StepWorkflowTrigger"/>.
        /// </summary>
        public enum WorkflowTriggerCondition
        {
            /// <summary>
            /// The <see cref="StepWorkflowTrigger"/> is triggered when the status of the step changes.
            /// </summary>
            StatusChanged = 0,

            /// <summary>
            /// The <see cref="StepWorkflowTrigger"/> is triggered manually.
            /// </summary>
            Manual = 1,

            /// <summary>
            /// The <see cref="StepWorkflowTrigger"/> is triggered when the step is completed.
            /// </summary>
            IsComplete = 2
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Represents the parameters for the Step Workflow Status Change Trigger.
        /// </summary>
        public class StatusChangeTriggerSettings : SettingsStringBase
        {
            /// <summary>
            /// Gets or sets the "from" status identifier.
            /// </summary>
            /// <value>
            /// From status identifier.
            /// </value>
            public int? FromStatusId { get; set; }

            /// <summary>
            /// Gets or sets the "to" status identifier.
            /// </summary>
            /// <value>
            /// To status identifier.
            /// </value>
            public int? ToStatusId { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="StatusChangeTriggerSettings"/> class.
            /// </summary>
            public StatusChangeTriggerSettings()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="StatusChangeTriggerSettings"/> class.
            /// </summary>
            /// <param name="settingsString">The settings string.</param>
            public StatusChangeTriggerSettings( string settingsString )
            {
                FromSelectionString( settingsString );
            }

            /// <summary>
            /// Gets an ordered set of property values that can be used to construct the
            /// settings string.
            /// </summary>
            /// <returns>
            /// An ordered collection of strings representing the parameter values.
            /// </returns>
            protected override IEnumerable<string> OnGetParameters()
            {
                var parameters = new List<string> { FromStatusId.ToStringSafe(), ToStatusId.ToStringSafe() };

                return parameters;
            }

            /// <summary>
            /// Set the property values parsed from a settings string.
            /// </summary>
            /// <param name="version">The version number of the parameter set.</param>
            /// <param name="parameters">An ordered collection of strings representing the parameter values.</param>
            protected override void OnSetParameters( int version, IReadOnlyList<string> parameters )
            {
                if ( parameters.Count > 0 )
                {
                    FromStatusId = parameters[0].AsIntegerOrNull();
                }

                if ( parameters.Count > 1 )
                {
                    ToStatusId = parameters[1].AsIntegerOrNull();
                }
            }
        }

        #endregion
    }
}
