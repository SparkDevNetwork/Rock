using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a step workflow in Rock.
    /// </summary>
    [RockDomain( "Steps" )]
    [Table( "StepWorkflow" )]
    [DataContract]
    public partial class StepWorkflow : Model<StepWorkflow>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="StepWorkflowTrigger"/> by which this Workflow was triggered. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int StepWorkflowTriggerId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Workflow"/> that was triggered. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Step"/> that triggered the workflow. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int StepId { get; set; }

        #endregion Entity Properties

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the Step Workflow Trigger.
        /// </summary>
        [DataMember]
        public virtual StepWorkflowTrigger StepWorkflowTrigger { get; set; }

        /// <summary>
        /// Gets or sets the Step.
        /// </summary>
        [DataMember]
        public virtual Step Step { get; set; }

        /// <summary>
        /// Gets or sets the Workflow.
        /// </summary>
        [DataMember]
        public virtual Workflow Workflow { get; set; }

        #endregion Virtual Properties

        #region Entity Configuration

        /// <summary>
        /// Step Workflow Configuration class.
        /// </summary>
        public partial class StepWorkflowConfiguration : EntityTypeConfiguration<StepWorkflow>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StepWorkflowConfiguration"/> class.
            /// </summary>
            public StepWorkflowConfiguration()
            {
                HasRequired( sw => sw.StepWorkflowTrigger ).WithMany( swt => swt.StepWorkflows ).HasForeignKey( sw => sw.StepWorkflowTriggerId ).WillCascadeOnDelete( true );
                HasRequired( sw => sw.Workflow ).WithMany().HasForeignKey( sw => sw.WorkflowId ).WillCascadeOnDelete( true );
                HasRequired( sw => sw.Step ).WithMany( s => s.StepWorkflows ).HasForeignKey( sw => sw.StepId ).WillCascadeOnDelete( true );
            }
        }

        #endregion
    }
}
