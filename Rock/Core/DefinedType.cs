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
    /// Defined Type POCO Entity.
    /// </summary>
    [Table( "coreDefinedType" )]
    public partial class DefinedType : ModelWithAttributes<DefinedType>, IAuditable, IOrdered
    {
		/// <summary>
		/// Gets or sets the System.
		/// </summary>
		/// <value>
		/// System.
		/// </value>
		[Required]
		[DataMember]
		public bool IsSystem { get; set; }
		
		/// <summary>
		/// Gets or sets the Field Type Id.
		/// </summary>
		/// <value>
		/// Field Type Id.
		/// </value>
		[DataMember]
		public int? FieldTypeId { get; set; }
		
		/// <summary>
		/// Gets or sets the Order.
		/// </summary>
		/// <value>
		/// Order.
		/// </value>
		[Required]
		[DataMember]
		public int Order { get; set; }
		
		/// <summary>
		/// Gets or sets the Category.
		/// </summary>
		/// <value>
		/// Category.
		/// </value>
		[MaxLength( 100 )]
		[DataMember]
		public string Category { get; set; }
		
		/// <summary>
		/// Gets or sets the Name.
		/// </summary>
		/// <value>
		/// Name.
		/// </value>
		[Required]
		[MaxLength( 100 )]
		[DataMember]
		public string Name { get; set; }
		
		/// <summary>
		/// Gets or sets the Description.
		/// </summary>
		/// <value>
		/// Description.
		/// </value>
		[DataMember]
		public string Description { get; set; }
		
		/// <summary>
		/// Gets or sets the Created Date Time.
		/// </summary>
		/// <value>
		/// Created Date Time.
		/// </value>
		[DataMember]
		public DateTime? CreatedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Modified Date Time.
		/// </summary>
		/// <value>
		/// Modified Date Time.
		/// </value>
		[DataMember]
		public DateTime? ModifiedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Created By Person Id.
		/// </summary>
		/// <value>
		/// Created By Person Id.
		/// </value>
		[DataMember]
		public int? CreatedByPersonId { get; set; }
		
		/// <summary>
		/// Gets or sets the Modified By Person Id.
		/// </summary>
		/// <value>
		/// Modified By Person Id.
		/// </value>
		[DataMember]
		public int? ModifiedByPersonId { get; set; }

        /// <summary>
        /// Gets the auth entity.
        /// </summary>
		[NotMapped]
		public override string AuthEntity { get { return "Core.DefinedType"; } }
        
		/// <summary>
        /// Gets or sets the Defined Values.
        /// </summary>
        /// <value>
        /// Collection of Defined Values.
        /// </value>
		public virtual ICollection<DefinedValue> DefinedValues { get; set; }
        
		/// <summary>
        /// Gets or sets the Field Type.
        /// </summary>
        /// <value>
        /// A <see cref="FieldType"/> object.
        /// </value>
		public virtual FieldType FieldType { get; set; }
        
		/// <summary>
        /// Gets or sets the Created By Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person CreatedByPerson { get; set; }
        
		/// <summary>
        /// Gets or sets the Modified By Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person ModifiedByPerson { get; set; }

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        public override Security.ISecured ParentAuthority
        {
            get { return new Security.GenericEntity( "Global" ); }
        }

    }

    /// <summary>
    /// Defined Type Configuration class.
    /// </summary>
    public partial class DefinedTypeConfiguration : EntityTypeConfiguration<DefinedType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedTypeConfiguration"/> class.
        /// </summary>
        public DefinedTypeConfiguration()
        {
			this.HasOptional( p => p.FieldType ).WithMany( p => p.DefinedTypes ).HasForeignKey( p => p.FieldTypeId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class DefinedTypeDTO : DTO<DefinedType>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Field Type Id.
        /// </summary>
        /// <value>
        /// Field Type Id.
        /// </value>
        public int? FieldTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the Category.
        /// </summary>
        /// <value>
        /// Category.
        /// </value>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public DefinedTypeDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public DefinedTypeDTO( DefinedType definedType )
        {
            CopyFromModel( definedType );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="definedType"></param>
        public override void CopyFromModel( DefinedType definedType )
        {
            this.Id = definedType.Id;
            this.Guid = definedType.Guid;
            this.IsSystem = definedType.IsSystem;
            this.FieldTypeId = definedType.FieldTypeId;
            this.Order = definedType.Order;
            this.Category = definedType.Category;
            this.Name = definedType.Name;
            this.Description = definedType.Description;
            this.CreatedDateTime = definedType.CreatedDateTime;
            this.ModifiedDateTime = definedType.ModifiedDateTime;
            this.CreatedByPersonId = definedType.CreatedByPersonId;
            this.ModifiedByPersonId = definedType.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="definedType"></param>
        public override void CopyToModel( DefinedType definedType )
        {
            definedType.Id = this.Id;
            definedType.Guid = this.Guid;
            definedType.IsSystem = this.IsSystem;
            definedType.FieldTypeId = this.FieldTypeId;
            definedType.Order = this.Order;
            definedType.Category = this.Category;
            definedType.Name = this.Name;
            definedType.Description = this.Description;
            definedType.CreatedDateTime = this.CreatedDateTime;
            definedType.ModifiedDateTime = this.ModifiedDateTime;
            definedType.CreatedByPersonId = this.CreatedByPersonId;
            definedType.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }
}
