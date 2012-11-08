//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Rock.Data;
using Rock.Util;

namespace Rock.Core
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "coreEntityTypeWorkflowTrigger" )]
    public partial class EntityTypeWorkflowTrigger : Entity<EntityTypeWorkflowTrigger>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Entity Type Id.
        /// </summary>
        /// <value>
        /// Entity Type Id.
        /// </value>
        [Required]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Entity Qualifier Column.
        /// </summary>
        /// <value>
        /// Entity Qualifier Column.
        /// </value>
        [MaxLength( 50 )]
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the Entity Qualifier Value.
        /// </summary>
        /// <value>
        /// Entity Qualifier Value.
        /// </value>
        [MaxLength( 200 )]
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the workflow type id.
        /// </summary>
        /// <value>
        /// The workflow type id.
        /// </value>
        [Required]
        public int WorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity change.
        /// </summary>
        /// <value>
        /// The type of the entity change.
        /// </value>
        [Required]
        public EntityTriggerType EntityTriggerType { get; set; }

        /// <summary>
        /// Gets or sets the name of the workflow.
        /// </summary>
        /// <value>
        /// The name of the workflow.
        /// </value>
        [MaxLength( 100 )]
        [Required]
        public string WorkflowName { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public virtual Core.EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the type of the workflow.
        /// </summary>
        /// <value>
        /// The type of the workflow.
        /// </value>
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
            return this.WorkflowName;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static EntityTypeWorkflowTrigger Read( int id )
        {
            return Read<EntityTypeWorkflowTrigger>( id );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static EntityTypeWorkflowTrigger Read( Guid guid )
        {
            return Read<EntityTypeWorkflowTrigger>( guid );
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// EntityTypeWorkflowTrigger Configuration class.
    /// </summary>
    public partial class EntityTypeWorkflowTriggerConfiguration : EntityTypeConfiguration<EntityTypeWorkflowTrigger>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTypeWorkflowTriggerConfiguration"/> class.
        /// </summary>
        public EntityTypeWorkflowTriggerConfiguration()
        {
            this.HasRequired( m => m.EntityType ).WithMany().HasForeignKey( m => m.EntityTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( m => m.WorkflowType ).WithMany().HasForeignKey( m => m.WorkflowTypeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Type of audit done to an entity
    /// </summary>
    public enum EntityTriggerType
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
