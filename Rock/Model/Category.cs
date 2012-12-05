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

using Rock.Model;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Category POCO Entity.
    /// </summary>
    [Table( "Category" )]
    public partial class Category : Entity<Category>
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
        /// Gets or sets the parent category id.
        /// </summary>
        /// <value>
        /// The parent category id.
        /// </value>
        public int? ParentCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the parent category.
        /// </summary>
        /// <value>
        /// The parent category
        /// </value>
        public virtual Category ParentCategory { get; set; }

        /// <summary>
        /// Gets or sets the child categories.
        /// </summary>
        /// <value>
        /// The child categories.
        /// </value>
        public virtual ICollection<Category> ChildCategories { get; set; }

        /// <summary>
        /// Gets or sets the Entity Type Id.
        /// </summary>
        /// <value>
        /// Entity Type Id.
        /// </value>
        [Required]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public virtual EntityType EntityType { get; set; }

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
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the file id.
        /// </summary>
        /// <value>
        /// The file id.
        /// </value>
        public int? FileId { get; set; }

        /// <summary>
        /// Gets or sets the file.
        /// </summary>
        /// <value>
        /// The file.
        /// </value>
        public virtual BinaryFile File { get; set; }

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
        public static Category Read( int id )
        {
            return Read<Category>( id );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static Category Read( Guid guid )
        {
            return Read<Category>( guid );
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
    /// Category Configuration class.
    /// </summary>
    public partial class CategoryConfiguration : EntityTypeConfiguration<Category>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryConfiguration" /> class.
        /// </summary>
        public CategoryConfiguration()
        {
            this.HasOptional( p => p.ParentCategory ).WithMany( p => p.ChildCategories).HasForeignKey( p => p.ParentCategoryId).WillCascadeOnDelete( false );
            this.HasRequired( p => p.EntityType ).WithMany().HasForeignKey( p => p.EntityTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.File ).WithMany().HasForeignKey( p => p.FileId ).WillCascadeOnDelete( false );
        }
    }
}
