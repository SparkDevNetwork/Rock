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
    /// Attribute Value POCO Entity.
    /// </summary>
    [Table( "coreAttributeValue" )]
    public partial class AttributeValue : Model<AttributeValue>, IAuditable
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
		/// Gets or sets the Attribute Id.
		/// </summary>
		/// <value>
		/// Attribute Id.
		/// </value>
		[Required]
		[DataMember]
		public int AttributeId { get; set; }
		
		/// <summary>
		/// Gets or sets the Entity Id.
		/// </summary>
		/// <value>
		/// Entity Id.
		/// </value>
		[DataMember]
		public int? EntityId { get; set; }
		
		/// <summary>
		/// Gets or sets the Order.
		/// </summary>
		/// <value>
		/// Order.
		/// </value>
		[DataMember]
		public int? Order { get; set; }
		
		/// <summary>
		/// Gets or sets the Value.
		/// </summary>
		/// <value>
		/// Value.
		/// </value>
		[DataMember]
		public string Value { get; set; }
		
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
		public override string AuthEntity { get { return "Core.AttributeValue"; } }
        
		/// <summary>
        /// Gets or sets the Attribute.
        /// </summary>
        /// <value>
        /// A <see cref="Attribute"/> object.
        /// </value>
		public virtual Attribute Attribute { get; set; }
        
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
    /// Attribute Value Configuration class.
    /// </summary>
    public partial class AttributeValueConfiguration : EntityTypeConfiguration<AttributeValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValueConfiguration"/> class.
        /// </summary>
        public AttributeValueConfiguration()
        {
			this.HasRequired( p => p.Attribute ).WithMany( p => p.AttributeValues ).HasForeignKey( p => p.AttributeId ).WillCascadeOnDelete(true);
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class AttributeValueDTO : DTO<AttributeValue>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Attribute Id.
        /// </summary>
        /// <value>
        /// Attribute Id.
        /// </value>
        public int AttributeId { get; set; }

        /// <summary>
        /// Gets or sets the Entity Id.
        /// </summary>
        /// <value>
        /// Entity Id.
        /// </value>
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        public int? Order { get; set; }

        /// <summary>
        /// Gets or sets the Value.
        /// </summary>
        /// <value>
        /// Value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public AttributeValueDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public AttributeValueDTO( AttributeValue attributeValue )
        {
            CopyFromModel( attributeValue );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="attributeValue"></param>
        public override void CopyFromModel( AttributeValue attributeValue )
        {
            this.Id = attributeValue.Id;
            this.Guid = attributeValue.Guid;
            this.IsSystem = attributeValue.IsSystem;
            this.AttributeId = attributeValue.AttributeId;
            this.EntityId = attributeValue.EntityId;
            this.Order = attributeValue.Order;
            this.Value = attributeValue.Value;
            this.CreatedDateTime = attributeValue.CreatedDateTime;
            this.ModifiedDateTime = attributeValue.ModifiedDateTime;
            this.CreatedByPersonId = attributeValue.CreatedByPersonId;
            this.ModifiedByPersonId = attributeValue.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="attributeValue"></param>
        public override void CopyToModel( AttributeValue attributeValue )
        {
            attributeValue.Id = this.Id;
            attributeValue.Guid = this.Guid;
            attributeValue.IsSystem = this.IsSystem;
            attributeValue.AttributeId = this.AttributeId;
            attributeValue.EntityId = this.EntityId;
            attributeValue.Order = this.Order;
            attributeValue.Value = this.Value;
            attributeValue.CreatedDateTime = this.CreatedDateTime;
            attributeValue.ModifiedDateTime = this.ModifiedDateTime;
            attributeValue.CreatedByPersonId = this.CreatedByPersonId;
            attributeValue.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }
}
