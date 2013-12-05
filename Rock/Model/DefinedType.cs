//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// A DefinedType is a dictionary of consistent values for a particular thing in RockChMS. The individual items are refereed to as <see cref="Rock.Model.DefinedValue">DefinedValues</see> in RockChMS.  
    /// Several classic examples of DefinedTypes can be Shirt Sizes, a Country List, etc. Defined Values can be categorized, ordered and can be furthered specified by a <see cref="Rock.Model.FieldType"/>
    /// </summary>
    /// <remarks>
    /// Note: in some systems these are referred to as lookup values. The benefit of storing these values centrally is that it prevents us having to maintain <see cref="Rock.Model.EntityType">EntityTypes</see>
    /// for each defined value/lookup that you want to create.  In the case of attributes these can be created as the need arises without having to change the core base or add a plug-in just to 
    /// provide additional lookup data.
    /// </remarks>
    [Table( "DefinedType" )]
    [DataContract]
    public partial class DefinedType : Model<DefinedType>, IOrdered
    {
        /// <summary>
        /// Gets or sets a flag indicating if this DefinedType is part of the RockChMS core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this DefinedType is part of the RockChMS core system/framework; otherwise this value is <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the FieldTypeId of the <see cref="Rock.Model.FieldType"/> that is used to set/select, and at times display the <see cref="Rock.Model.DefinedValue">DefinedValues</see> that are associated with
        /// this DefinedType. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the FieldTypeId of the <see cref="Rock.Model.FieldType"/> that is used for DefinedType.
        /// </value>
        [DataMember]
        public int? FieldTypeId { get; set; }
        
        /// <summary>
        /// Gets or sets the display order of this DefinedType.  The lower the number the higher the display priority.  This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that represents the display order of this DefinedType.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the category that this DefinedType belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the name of the Category.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Category { get; set; }
        
        /// <summary>
        /// Gets or sets the Name of the DefinedType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the DefinedType.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets a user defined description of the DefinedType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the DefinedType.
        /// </value>
        [DataMember]
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.DefinedValue">DefinedValues</see> that belong to this DefinedType.
        /// </summary>
        /// <value>
        /// A collection of the <see cref="Rock.Model.DefinedValue">DefinedValues</see> that belong to this DefinedType.
        /// </value>
        [DataMember]
        public virtual ICollection<DefinedValue> DefinedValues { get; set; }
        
        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FieldType"/> that is used to set/select, and at times display the <see cref="Rock.Model.DefinedValue">DefinedValues</see> that are associated with
        /// this DefinedType. 
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FieldType"/>.
        /// </value>
        [DataMember]
        public virtual FieldType FieldType { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this DefinedType.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this DefinedType.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
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
        }
    }
}
