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
    /// Tag POCO Entity.
    /// </summary>
    [Table( "Tag" )]
    [DataContract( IsReference = true )]
    public partial class Tag : Model<Tag>, IOrdered
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Entity Type Id.
        /// </summary>
        /// <value>
        /// Entity Type Id.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Entity Qualifier Column.
        /// </summary>
        /// <value>
        /// Entity Qualifier Column.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string EntityTypeQualifierColumn { get; set; }
        
        /// <summary>
        /// Gets or sets the Entity Qualifier Value.
        /// </summary>
        /// <value>
        /// Entity Qualifier Value.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string EntityTypeQualifierValue { get; set; }
        
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the Owner Person Id.
        /// </summary>
        /// <value>
        /// Owner Id.
        /// </value>
        [DataMember]
        public int? OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        /// <value>
        /// The owner.
        /// </value>
        [DataMember]
        public virtual Model.Person Owner { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual Model.EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the tagged items.
        /// </summary>
        /// <value>
        /// The tagged items.
        /// </value>
        [DataMember]
        public virtual ICollection<TaggedItem> TaggedItems { get; set; }
        
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }
    }

    /// <summary>
    /// Tag Configuration class.
    /// </summary>
    public partial class TagConfiguration : EntityTypeConfiguration<Tag>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TagConfiguration" /> class.
        /// </summary>
        public TagConfiguration()
        {
            this.HasOptional( p => p.Owner ).WithMany().HasForeignKey( p => p.OwnerId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.EntityType ).WithMany().HasForeignKey( p => p.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }
}
