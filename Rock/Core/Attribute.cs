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
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Core
{
    /// <summary>
    /// Attribute POCO Entity.
    /// </summary>
    [Table( "coreAttribute" )]
    public partial class Attribute : Model<Attribute>, IAuditable, IOrdered
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
		[Required]
		[DataMember]
		public int FieldTypeId { get; set; }
		
		/// <summary>
		/// Gets or sets the Entity.
		/// </summary>
		/// <value>
		/// Entity.
		/// </value>
		[MaxLength( 50 )]
		[DataMember]
		public string Entity { get; set; }
		
		/// <summary>
		/// Gets or sets the Entity Qualifier Column.
		/// </summary>
		/// <value>
		/// Entity Qualifier Column.
		/// </value>
		[MaxLength( 50 )]
		[DataMember]
		public string EntityQualifierColumn { get; set; }
		
		/// <summary>
		/// Gets or sets the Entity Qualifier Value.
		/// </summary>
		/// <value>
		/// Entity Qualifier Value.
		/// </value>
		[MaxLength( 200 )]
		[DataMember]
		public string EntityQualifierValue { get; set; }
		
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
		/// Gets or sets the Category.
		/// </summary>
		/// <value>
		/// Category.
		/// </value>
		[MaxLength( 100 )]
		[DataMember]
		public string Category { get; set; }
		
		/// <summary>
		/// Gets or sets the Description.
		/// </summary>
		/// <value>
		/// Description.
		/// </value>
		[DataMember]
		public string Description { get; set; }
		
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
		/// Gets or sets the Grid Column.
		/// </summary>
		/// <value>
		/// Grid Column.
		/// </value>
		[Required]
		[DataMember]
		public bool IsGridColumn { get; set; }
		
		/// <summary>
		/// Gets or sets the Default Value.
		/// </summary>
		/// <value>
		/// Default Value.
		/// </value>
        [DataMember]
		public string DefaultValue { get; set; }
		
		/// <summary>
		/// Gets or sets the Multi Value.
		/// </summary>
		/// <value>
		/// Multi Value.
		/// </value>
		[Required]
		[DataMember]
		public bool IsMultiValue { get; set; }
		
		/// <summary>
		/// Gets or sets the Required.
		/// </summary>
		/// <value>
		/// Required.
		/// </value>
		[Required]
		[DataMember]
		public bool IsRequired { get; set; }
		
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
		public override string AuthEntity { get { return "Core.Attribute"; } }
        
		/// <summary>
        /// Gets or sets the Attribute Qualifiers.
        /// </summary>
        /// <value>
        /// Collection of Attribute Qualifiers.
        /// </value>
		public virtual ICollection<AttributeQualifier> AttributeQualifiers { get; set; }
        
		/// <summary>
        /// Gets or sets the Attribute Values.
        /// </summary>
        /// <value>
        /// Collection of Attribute Values.
        /// </value>
		public virtual ICollection<AttributeValue> AttributeValues { get; set; }
        
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
        /// A <see cref="Crm.Person"/> object.
        /// </value>
		public virtual Crm.Person CreatedByPerson { get; set; }
        
		/// <summary>
        /// Gets or sets the Modified By Person.
        /// </summary>
        /// <value>
        /// A <see cref="Crm.Person"/> object.
        /// </value>
		public virtual Crm.Person ModifiedByPerson { get; set; }

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        public override Security.ISecured ParentAuthority
        {
            get { return new Security.GenericEntity( "Global" ); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Attribute"/> class.
        /// </summary>
        public Attribute()
        {
            AttributeQualifiers = new System.Collections.ObjectModel.Collection<AttributeQualifier>();
        }
    }

    /// <summary>
    /// Attribute Configuration class.
    /// </summary>
    public partial class AttributeConfiguration : EntityTypeConfiguration<Attribute>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeConfiguration"/> class.
        /// </summary>
        public AttributeConfiguration()
        {
			this.HasRequired( p => p.FieldType ).WithMany( p => p.Attributes ).HasForeignKey( p => p.FieldTypeId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }
}
