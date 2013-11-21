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
    /// Represents a collection or group of entity objects that share one or more common characteristics . A tag can either be private (owned by an individual <see cref="Rock.Model.Person"/>)
    /// or public.
    /// </summary>
    [Table( "Tag" )]
    [DataContract]
    public partial class Tag : Model<Tag>, IOrdered
    {
        /// <summary>
        /// Gets or sets a flag indicating if this Tag is part of the RockChMS core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this Tag is part of the RockChMS core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> containing the entities that can use this Tag. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that contains the entities that can use this Tag.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the column/property that contains the value that can narrow the scope of entities that can receive this Tag. Entities where this 
        /// column contains the <see cref="EntityTypeQualifierValue"/> will be eligible to have this Tag. This property must be used in conjunction with the <see cref="EntityTypeQualifierValue"/>
        /// property. If all entities of the the specified <see cref="Rock.Model.EntityType"/> are eligible to use this Tag, this property will be null.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the EntityTypeQualifierColumn.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string EntityTypeQualifierColumn { get; set; }
        
        /// <summary>
        /// Gets or sets the value in the <see cref="EntityTypeQualifierColumn"/> that narrows the scope of entities that can receive this Tag. Entities that contain this value 
        /// in the <see cref="EntityTypeQualifierColumn"/> are eligible to use this Tag. This property must be used in conjunction with the <see cref="EntityTypeQualifierColumn"/> property.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the EntityTypeQualiferValue that limits which entities of the specified EntityType that can use this Tag.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string EntityTypeQualifierValue { get; set; }
        
        /// <summary>
        /// Gets or sets the Name of the Tag. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Name of the Tag
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the display order of the tag. the lower the number, the higher display priority that the Tag has.  For example the Tags with the lower Order could be displayed higher on the Tag list.
        /// This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that represents the display Order of the Tag.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person"/> who is the Owner this Tag. If this value is Null, the Tag will be considered an Public/all user tag.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that represents the PersonId of the <see cref="Rock.Model.Person"/> who is the Owner of this Tag.  If this is a Public tag, this value will be null.
        /// </value>
        [DataMember]
        public int? OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> who is the Owner of this Tag.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Person"/> who is the owner of the tag.
        /// </value>
        public virtual Model.Person Owner { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> of the Entities that this Tag can be applied to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> of Entities that this Tag can be applied to.
        /// </value>
        public virtual Model.EntityType EntityType { get; set; }


        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.TaggedItem">TaggedItems</see> representing the entities that are tagged with this Tag.
        /// </summary>
        /// <value>
        /// A collection containing of <see cref="Rock.Model.TaggedItem">TaggedItems</see> representing the entities that use this tag.
        /// </value>
        public virtual ICollection<TaggedItem> TaggedItems { get; set; }
        
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this Tag.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this Tag.
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
