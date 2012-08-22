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
    [Table( "coreEntityChange" )]
    public partial class EntityChange : ModelWithAttributes<EntityChange>
    {
		/// <summary>
		/// Gets or sets the Change Set.
		/// </summary>
		/// <value>
		/// Change Set.
		/// </value>
		[Required]
		[DataMember]
		public Guid ChangeSet { get; set; }
		
		/// <summary>
		/// Gets or sets the Change Type.
		/// </summary>
		/// <value>
		/// Change Type.
		/// </value>
		[Required]
		[MaxLength( 10 )]
		[DataMember]
		public string ChangeType { get; set; }
		
		/// <summary>
		/// Gets or sets the Entity Type.
		/// </summary>
		/// <value>
		/// Entity Type.
		/// </value>
		[Required]
		[MaxLength( 100 )]
		[DataMember]
		public string EntityType { get; set; }
		
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
		/// Gets or sets the Property.
		/// </summary>
		/// <value>
		/// Property.
		/// </value>
		[Required]
		[MaxLength( 100 )]
		[DataMember]
		public string Property { get; set; }
		
		/// <summary>
		/// Gets or sets the Original Value.
		/// </summary>
		/// <value>
		/// Original Value.
		/// </value>
		[DataMember]
		public string OriginalValue { get; set; }
		
		/// <summary>
		/// Gets or sets the Current Value.
		/// </summary>
		/// <value>
		/// Current Value.
		/// </value>
		[DataMember]
		public string CurrentValue { get; set; }
		
		/// <summary>
		/// Gets or sets the Created Date Time.
		/// </summary>
		/// <value>
		/// Created Date Time.
		/// </value>
		[DataMember]
		public DateTime? CreatedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Created By Person Id.
		/// </summary>
		/// <value>
		/// Created By Person Id.
		/// </value>
		[DataMember]
		public int? CreatedByPersonId { get; set; }
		
        /// <summary>
        /// Gets the auth entity.
        /// </summary>
		[NotMapped]
		public override string AuthEntity { get { return "Core.EntityChange"; } }
        
		/// <summary>
        /// Gets or sets the Created By Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person CreatedByPerson { get; set; }

    }
    
    /// <summary>
    /// Entity Change Configuration class.
    /// </summary>
    public partial class EntityChangeConfiguration : EntityTypeConfiguration<EntityChange>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityChangeConfiguration"/> class.
        /// </summary>
        public EntityChangeConfiguration()
        {
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class EntityChangeDTO : DTO<EntityChange>
    {
        /// <summary>
        /// Gets or sets the Change Set.
        /// </summary>
        /// <value>
        /// Change Set.
        /// </value>
        public Guid ChangeSet { get; set; }

        /// <summary>
        /// Gets or sets the Change Type.
        /// </summary>
        /// <value>
        /// Change Type.
        /// </value>
        public string ChangeType { get; set; }

        /// <summary>
        /// Gets or sets the Entity Type.
        /// </summary>
        /// <value>
        /// Entity Type.
        /// </value>
        public string EntityType { get; set; }

        /// <summary>
        /// Gets or sets the Entity Id.
        /// </summary>
        /// <value>
        /// Entity Id.
        /// </value>
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the Property.
        /// </summary>
        /// <value>
        /// Property.
        /// </value>
        public string Property { get; set; }

        /// <summary>
        /// Gets or sets the Original Value.
        /// </summary>
        /// <value>
        /// Original Value.
        /// </value>
        public string OriginalValue { get; set; }

        /// <summary>
        /// Gets or sets the Current Value.
        /// </summary>
        /// <value>
        /// Current Value.
        /// </value>
        public string CurrentValue { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public EntityChangeDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public EntityChangeDTO( EntityChange entityChange )
        {
            CopyFromModel( entityChange );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="entityChange"></param>
        public override void CopyFromModel( EntityChange entityChange )
        {
            this.Id = entityChange.Id;
            this.Guid = entityChange.Guid;
            this.ChangeSet = entityChange.ChangeSet;
            this.ChangeType = entityChange.ChangeType;
            this.EntityType = entityChange.EntityType;
            this.EntityId = entityChange.EntityId;
            this.Property = entityChange.Property;
            this.OriginalValue = entityChange.OriginalValue;
            this.CurrentValue = entityChange.CurrentValue;
            this.CreatedDateTime = entityChange.CreatedDateTime;
            this.CreatedByPersonId = entityChange.CreatedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="entityChange"></param>
        public override void CopyToModel( EntityChange entityChange )
        {
            entityChange.Id = this.Id;
            entityChange.Guid = this.Guid;
            entityChange.ChangeSet = this.ChangeSet;
            entityChange.ChangeType = this.ChangeType;
            entityChange.EntityType = this.EntityType;
            entityChange.EntityId = this.EntityId;
            entityChange.Property = this.Property;
            entityChange.OriginalValue = this.OriginalValue;
            entityChange.CurrentValue = this.CurrentValue;
            entityChange.CreatedDateTime = this.CreatedDateTime;
            entityChange.CreatedByPersonId = this.CreatedByPersonId;
        }
    }
}
