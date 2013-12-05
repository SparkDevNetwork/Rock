//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents an Audit Log entry that is created when an add/update/delete is performed against an <see cref="Rock.Data.IEntity"/> of an
    /// auditable <see cref="Rock.Model.EntityType"/>.
    /// </summary>
    [NotAudited]
    [Table( "Audit" )]
    [DataContract]
    public partial class Audit : Entity<Audit>
    {
        /// <summary>
        /// Gets or sets the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of entity that was modified. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of the entity that was modified.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityTypeId { get; set; }
        
        /// <summary>
        /// Gets or sets the Id of the specific entity that was modified. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the entity that was modified.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the Name/Title of the specific entity that was updated. This is usually the value that is return when the entity's ToString() function is called. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Entity's Name.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        [DataMember( IsRequired = true )]
        public string Title { get; set; }
        
        ///// <summary>
        ///// Type of change: 0:Add, 1:Modify, 2:Delete
        ///// Gets or sets the
        ///// </summary>
        ///// <value>
        ///// Original Value.
        ///// </value>


        /// <summary>
        /// Gets or sets the type of change that was made to the entity. This property is required.
        /// </summary>
        /// <value>
        /// The type of the audit.
        /// A <see cref="Rock.Model.AuditType"/> enumeration that indicates the type of change that was made. 
        ///     <c>AuditType.Add</c> (0) indicates that a new entity was added; 
        ///     <c>AuditType.Modify</c> (1) indicates that the entity was modified; 
        ///     <c>AuditType.Delete</c> (2) indicates that the entity was deleted.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public AuditType AuditType { get; set; }
        
        /// <summary>
        /// Gets or sets a comma delimited list of the properties that were modified. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the properties that were modified.
        /// </value>
        [DataMember]
        public string Properties { get; set; }


        /// <summary>
        /// Gets or sets the date and time that the entity was modified and the audit entry was created.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> that represents the date and time that the entity was modified.
        /// </value>
        [DataMember]
        public DateTime? DateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person"/> who modified the entity and created the audit entry.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> who modified the entity and crated the audit entity.
        /// </value>
        [DataMember]
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> of the entity that was modified.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> of the entity that was modified.
        /// </value>
        [DataMember]
        public virtual Model.EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> who modified the entity and created the audit entity.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Person"/> who modified the entity and created the audit entry.
        /// </value>
        [DataMember]
        public virtual Rock.Model.Person Person { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Title;
        }

    }
    
    /// <summary>
    /// Entity Change Configuration class.
    /// </summary>
    public partial class AuditConfiguration : EntityTypeConfiguration<Audit>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTypeConfiguration" /> class.
        /// </summary>
        public AuditConfiguration()
        {
            this.HasOptional( p => p.Person ).WithMany().HasForeignKey( p => p.PersonId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.EntityType ).WithMany().HasForeignKey( p => p.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #region Enumerations

    /// <summary>
    /// Type of audit done to an entity
    /// </summary>
    public enum AuditType
    {
        /// <summary>
        /// Add
        /// </summary>
        Add = 0,

        /// <summary>
        /// Modify
        /// </summary>
        Modify = 1,

        /// <summary>
        /// Delete
        /// </summary>
        Delete = 2
    }

    #endregion

}
