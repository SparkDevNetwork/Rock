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
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Tag POCO Entity.
    /// </summary>
    [Table( "Tag" )]
    public partial class Tag : Model<Tag>, IOrdered
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Entity Type Id.
        /// </summary>
        /// <value>
        /// Entity Type Id.
        /// </value>
        [Required]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Entity Qualifier Column.
        /// </summary>
        /// <value>
        /// Entity Qualifier Column.
        /// </value>
        [MaxLength( 50 )]
        public string EntityTypeQualifierColumn { get; set; }
        
        /// <summary>
        /// Gets or sets the Entity Qualifier Value.
        /// </summary>
        /// <value>
        /// Entity Qualifier Value.
        /// </value>
        [MaxLength( 200 )]
        public string EntityTypeQualifierValue { get; set; }
        
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        [Required]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the Owner Person Id.
        /// </summary>
        /// <value>
        /// Owner Id.
        /// </value>
        public int? OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        /// <value>
        /// The owner.
        /// </value>
        public virtual Model.Person Owner { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public virtual Model.EntityType EntityType { get; set; }

        /// <summary>
        /// Gets the dto.
        /// </summary>
        /// <returns></returns>
        public override IDto Dto
        {
            get { return this.ToDto(); }
        }

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static Tag Read( int id )
        {
            return Read<Tag>( id );
        }

        /// <summary>
        /// Gets or sets the tagged items.
        /// </summary>
        /// <value>
        /// The tagged items.
        /// </value>
        public virtual ICollection<TaggedItem> TaggedItems { get; set; }
        
        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        public override Security.ISecured ParentAuthority
        {
            get { return new Security.GenericEntity( "Global" ); }
        }

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
