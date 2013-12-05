//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a attribute qualifier that limits or qualifies the values that can be accepted as <see cref="Rock.Model.AttributeValue">AttributeValues</see>.
    /// </summary>
    /// <remarks>
    /// Examples this can be a <see cref="Rock.Model.DefinedValue"/>, SQL query, or a list of options.
    /// </remarks>
    [Table( "AttributeQualifier" )]
    [DataContract]
    public partial class AttributeQualifier : Entity<AttributeQualifier>
    {
        /// <summary>
        /// Gets or sets a flag indicating if the AttributeQualifer is part of the RockChMS core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the AttributeQualifer is part of the RockChMS core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the AttributeId of the <see cref="Rock.Model.Attribute"/> that this AttributeQualifier limits the values of.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that represents the AttributeId of the <see cref="Rock.Model.Attribute"/>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int AttributeId { get; set; }
        
        /// <summary>
        /// Gets or sets the Key value that represents the type of qualifier that is being used.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the type of qualifier that is being used.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Key { get; set; }
        
        /// <summary>
        /// Gets or sets the value of the AttributeQualifier
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the value of the AttributeQualifier.
        /// </value>
        [DataMember]
        public string Value { get; set; }
        
        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Attribute"/> that uses this AttributeQualifier.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Attribute"/> that uses this AttributeQualifier.
        /// </value>
        [DataMember]
        public virtual Attribute Attribute { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Key;
        }
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
        }
    }
}
