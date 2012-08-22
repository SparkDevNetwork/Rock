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
    /// Defined Value POCO Entity.
    /// </summary>
    [Table( "coreDefinedValue" )]
    public partial class DefinedValue : ModelWithAttributes<DefinedValue>, IAuditable, IOrdered
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
		/// Gets or sets the Defined Type Id.
		/// </summary>
		/// <value>
		/// Defined Type Id.
		/// </value>
		[Required]
		[DataMember]
		public int DefinedTypeId { get; set; }
		
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
		public override string AuthEntity { get { return "Core.DefinedValue"; } }
        
		/// <summary>
        /// Gets or sets the Defined Type.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedType"/> object.
        /// </value>
		public virtual DefinedType DefinedType { get; set; }
        
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
    /// Defined Value Configuration class.
    /// </summary>
    public partial class DefinedValueConfiguration : EntityTypeConfiguration<DefinedValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValueConfiguration"/> class.
        /// </summary>
        public DefinedValueConfiguration()
        {
			this.HasRequired( p => p.DefinedType ).WithMany( p => p.DefinedValues ).HasForeignKey( p => p.DefinedTypeId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class DefinedValueDTO : DTO<DefinedValue>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Defined Type Id.
        /// </summary>
        /// <value>
        /// Defined Type Id.
        /// </value>
        public int DefinedTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        public int Order { get; set; }

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
        public DefinedValueDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public DefinedValueDTO( DefinedValue definedValue )
        {
            CopyFromModel( definedValue );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="definedValue"></param>
        public override void CopyFromModel( DefinedValue definedValue )
        {
            this.Id = definedValue.Id;
            this.Guid = definedValue.Guid;
            this.IsSystem = definedValue.IsSystem;
            this.DefinedTypeId = definedValue.DefinedTypeId;
            this.Order = definedValue.Order;
            this.Name = definedValue.Name;
            this.Description = definedValue.Description;
            this.CreatedDateTime = definedValue.CreatedDateTime;
            this.ModifiedDateTime = definedValue.ModifiedDateTime;
            this.CreatedByPersonId = definedValue.CreatedByPersonId;
            this.ModifiedByPersonId = definedValue.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="definedValue"></param>
        public override void CopyToModel( DefinedValue definedValue )
        {
            definedValue.Id = this.Id;
            definedValue.Guid = this.Guid;
            definedValue.IsSystem = this.IsSystem;
            definedValue.DefinedTypeId = this.DefinedTypeId;
            definedValue.Order = this.Order;
            definedValue.Name = this.Name;
            definedValue.Description = this.Description;
            definedValue.CreatedDateTime = this.CreatedDateTime;
            definedValue.ModifiedDateTime = this.ModifiedDateTime;
            definedValue.CreatedByPersonId = this.CreatedByPersonId;
            definedValue.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }
}
