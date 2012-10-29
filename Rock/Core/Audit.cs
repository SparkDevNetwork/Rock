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
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Core
{
    /// <summary>
    /// Entity Change POCO Entity.
    /// </summary>
    [NotAudited]
    [Table( "coreAudit" )]
    public partial class Audit : Entity<Audit>
    {
        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Entity Id.
        /// </summary>
        /// <value>
        /// Entity Id.
        /// </value>
        [Required]
        [DataMember]
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the Entity Name.
        /// </summary>
        /// <value>
        /// Entity Name.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        [DataMember]
        public string EntityName { get; set; }
        
        /// <summary>
        /// Type of change: 0:Add, 1:Modify, 2:Delete
        /// </summary>
        /// <value>
        /// Original Value.
        /// </value>
        [Required]
        [DataMember]
        public AuditType AuditType { get; set; }
        
        /// <summary>
        /// Gets or sets the properties modified.
        /// </summary>
        /// <value>
        /// Properties.
        /// </value>
        [DataMember]
        public string Properties { get; set; }

        /// <summary>
        /// Gets or sets the Date Time.
        /// </summary>
        /// <value>
        /// Date Time.
        /// </value>
        [DataMember]
        public DateTime? DateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the Person Id.
        /// </summary>
        /// <value>
        /// Person Id.
        /// </value>
        [DataMember]
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public virtual Rock.Core.EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the Person.
        /// </summary>
        /// <value>
        /// A <see cref="Person"/> object.
        /// </value>
        public virtual Rock.Crm.Person Person { get; set; }

        /// <summary>
        /// Gets the auth entity.
        /// </summary>
        [NotMapped]
        public string DeprecatedEntityTypeName { get { return "Core.Audit"; } }
        
        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static Audit Read( int id )
        {
            return Read<Audit>( id );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.EntityName;
        }

    }
    
    /// <summary>
    /// Entity Change Configuration class.
    /// </summary>
    public partial class AuditConfiguration : EntityTypeConfiguration<Audit>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityChangeConfiguration"/> class.
        /// </summary>
        public AuditConfiguration()
        {
            this.HasOptional( p => p.Person ).WithMany().HasForeignKey( p => p.PersonId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.EntityType ).WithMany().HasForeignKey( p => p.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

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


}
