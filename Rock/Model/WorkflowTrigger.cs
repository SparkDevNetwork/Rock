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
    /// 
    /// </summary>
    [Table( "WorkflowTrigger" )]
    [DataContract]
    public partial class WorkflowTrigger : Entity<WorkflowTrigger>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Entity Type Id.
        /// </summary>
        /// <value>
        /// Entity Type Id.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Entity Qualifier Column.
        /// </summary>
        /// <value>
        /// Entity Qualifier Column.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the Entity Qualifier Value.
        /// </summary>
        /// <value>
        /// Entity Qualifier Value.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the workflow type id.
        /// </summary>
        /// <value>
        /// The workflow type id.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int WorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity change.
        /// </summary>
        /// <value>
        /// The type of the entity change.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public WorkflowTriggerType WorkflowTriggerType { get; set; }

        /// <summary>
        /// Gets or sets the name of the workflow.
        /// </summary>
        /// <value>
        /// The name of the workflow.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string WorkflowName { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual Model.EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the type of the workflow.
        /// </summary>
        /// <value>
        /// The type of the workflow.
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
