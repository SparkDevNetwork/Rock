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
    /// Field Type POCO Entity.
    /// </summary>
    [Table( "FieldType" )]
    [DataContract]
    public partial class FieldType : Model<FieldType>
    {
        /// <summary>
        /// Gets or sets a flag indicating if this FieldType is part of of the RockChMS core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this FieldType is part of the RockChMS core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the Name of the FieldType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Name of the FieldType.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets a user defined description of the FieldType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the FieldType.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Assembly name of the .dll file that contains the FieldType class. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that contains the Assembly name of the .dll file that contains the FieldType class.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Assembly { get; set; }
        
        /// <summary>
        /// Gets or sets the fully qualified name, with Namespace, of the FieldType class. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that contains the fully qualified name of the FieldType class.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Class { get; set; }
        
        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.DefinedType">DefinedTypes</see> that use this FieldType.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.DefinedType">DefinedTypes</see> that use this FieldType.
        /// </value>
        [DataMember]
        public virtual ICollection<DefinedType> DefinedTypes { get; set; }

        // <summary>
        // Gets or sets the Metrics.
        // </summary>
        // <value>
        // Collection of Metrics.
        // </value>
        //public virtual ICollection<Metric> Metrics { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this FieldType.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this FieldType.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

    }

    /// <summary>
    /// Field Type Configuration class.
    /// </summary>
    public partial class FieldTypeConfiguration : EntityTypeConfiguration<FieldType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldTypeConfiguration"/> class.
        /// </summary>
        public FieldTypeConfiguration()
        {
        }
    }
}
