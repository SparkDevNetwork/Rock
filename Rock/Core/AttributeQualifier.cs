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
    /// Attribute Qualifier POCO Entity.
    /// </summary>
    [Table( "coreAttributeQualifier" )]
    public partial class AttributeQualifier : Model<AttributeQualifier>, IAuditable
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
		/// Gets or sets the Key.
		/// </summary>
		/// <value>
		/// Key.
		/// </value>
		[Required]
		[MaxLength( 50 )]
		[DataMember]
		public string Key { get; set; }
		
		/// <summary>
		/// Gets or sets the Value.
		/// </summary>
		/// <value>
		/// Value.
		/// </value>
		[Required]
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
		public override string AuthEntity { get { return "Core.AttributeQualifier"; } }
        
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

    }

    /// <summary>
    /// Attribute Qualifier Configuration class.
    /// </summary>
    public partial class AttributeQualifierConfiguration : EntityTypeConfiguration<AttributeQualifier>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeQualifierConfiguration"/> class.
        /// </summary>
        public AttributeQualifierConfiguration()
        {
			this.HasRequired( p => p.Attribute ).WithMany( p => p.AttributeQualifiers ).HasForeignKey( p => p.AttributeId ).WillCascadeOnDelete(true);
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class AttributeQualifierDTO : DTO<AttributeQualifier>
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
        /// Gets or sets the Key.
        /// </summary>
        /// <value>
        /// Key.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Name.
        /// </value>
        public string Name { get; set; }

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
        public AttributeQualifierDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public AttributeQualifierDTO( AttributeQualifier attributeQualifier )
        {
            CopyFromModel( attributeQualifier );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="attributeQualifier"></param>
        public override void CopyFromModel( AttributeQualifier attributeQualifier )
        {
            this.Id = attributeQualifier.Id;
            this.Guid = attributeQualifier.Guid;
            this.IsSystem = attributeQualifier.IsSystem;
            this.AttributeId = attributeQualifier.AttributeId;
            this.Key = attributeQualifier.Key;
            this.Value = attributeQualifier.Value;
            this.CreatedDateTime = attributeQualifier.CreatedDateTime;
            this.ModifiedDateTime = attributeQualifier.ModifiedDateTime;
            this.CreatedByPersonId = attributeQualifier.CreatedByPersonId;
            this.ModifiedByPersonId = attributeQualifier.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="attributeQualifier"></param>
        public override void CopyToModel( AttributeQualifier attributeQualifier )
        {
            attributeQualifier.Id = this.Id;
            attributeQualifier.Guid = this.Guid;
            attributeQualifier.IsSystem = this.IsSystem;
            attributeQualifier.AttributeId = this.AttributeId;
            attributeQualifier.Key = this.Key;
            attributeQualifier.Value = this.Value;
            attributeQualifier.CreatedDateTime = this.CreatedDateTime;
            attributeQualifier.ModifiedDateTime = this.ModifiedDateTime;
            attributeQualifier.CreatedByPersonId = this.CreatedByPersonId;
            attributeQualifier.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }
}
