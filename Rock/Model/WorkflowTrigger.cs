//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents an a WorkflowTrigger in RockChMS. A WorkflowTrigger can be setup on an EntityType or a subset of entities to start/trigger a workflow
    /// when a save or delete is performed on the entity. If the workflow does not complete successfully, the database action will not be performed.
    /// </summary>
    [Table( "WorkflowTrigger" )]
    [DataContract]
    public partial class WorkflowTrigger : Entity<WorkflowTrigger>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this WorkflowTrigger is part of RockChMS core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this WorkflowTrigger is part of the RockChMs core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType" /> of the entities that this trigger applies to
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that this trigger applies to.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the Entity Qualifier Column that contains the value that filters the scope of the WorkflowTrigger. This
        /// property must be used in conjunction with the <see cref="EntityTypeQualifierValue"/> property.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the column that contains the value that filters the scope of the WorkflowTrigger.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeQualifierValue in the <see cref="EntityTypeQualifierColumn"/> that is used to filter the scope of the WorkflowTrigger.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the EntityTypeQualifierValue that is used to filter the scope of the WorkflowTrigger.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the WorkflowTypeId of the <see cref="Rock.Model.WorkflowType"/> that is executed by this WorkflowTrigger. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing WorkflowTypeId of the <see cref="Rock.Model.WorkflowType"/> that is executed by the WorkflowTrigger.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int WorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the type of the workflow trigger. Indicates the type of change and  the timing the trigger.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.WorkflowTriggerType"/> enum value indicating the type of trigger.
        /// When <c>WorkflowTriggerType.PreSave</c> the workflow is triggered prior to a save action being executed.
        /// When <c>WorkflowTriggerType.PostSave</c> the workflow is triggered after a save action is executed.
        /// When <c>WorkflowTriggerType.PreDelete</c> the workflow is triggered prior to a delete action being executed.
        /// When <c>WorkflowTriggerType.PostDelete</c> the workflow is triggered after the delete action is executed.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public WorkflowTriggerType WorkflowTriggerType { get; set; }

        /// <summary>
        /// Gets or sets the name of the workflow trigger.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the workflow trigger.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string WorkflowName { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> that contains the entities that are affected by this WorkflowTrigger.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> that contains the entities that are affected by this WorkflowTrigger.
        /// </value>
        [DataMember]
        public virtual Model.EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowType"/> that is executed by this WorkflowTrigger.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowType"/> that is executed by this WorkflowTrigger.
        /// </value>
        [DataMember]
        public virtual WorkflowType WorkflowType { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.WorkflowName ?? EntityType.Name + " " + WorkflowTriggerType.ConvertToString();
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// EntityTypeWorkflowTrigger Configuration class.
    /// </summary>
    public partial class WorkflowTriggerConfiguration : EntityTypeConfiguration<WorkflowTrigger>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowTriggerConfiguration"/> class.
        /// </summary>
        public WorkflowTriggerConfiguration()
        {
            this.HasRequired( m => m.EntityType ).WithMany().HasForeignKey( m => m.EntityTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( m => m.WorkflowType ).WithMany().HasForeignKey( m => m.WorkflowTypeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Type of workflow trigger
    /// </summary>
    public enum WorkflowTriggerType
    {
        /// <summary>
        /// Pre Save
        /// </summary>
        PreSave = 0,

        /// <summary>
        /// Post Save
        /// </summary>
        PostSave = 1,

        /// <summary>
        /// Pre Delete
        /// </summary>
        PreDelete = 2,

        /// <summary>
        /// Post Delete
        /// </summary>
        PostDelete = 3
    }

    #endregion

}
