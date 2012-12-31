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
    /// Category POCO Entity.
    /// </summary>
    [Table( "Category" )]
    [DataContract( IsReference = true )]
    public partial class Category : Entity<Category>
    {

        #region Entity Properties

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
        /// Gets or sets the parent category id.
        /// </summary>
        /// <value>
        /// The parent category id.
        /// </value>
        [DataMember]
        public int? ParentCategoryId { get; set; }

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
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [MaxLength(100)]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the small icon.
        /// </summary>
        /// <value>
        /// The small icon.
        /// </value>
        [DataMember]
        public int? IconSmallFileId { get; set; }

        /// <summary>
        /// Gets or sets the large icon.
        /// </summary>
        /// <value>
        /// The large icon.
        /// </value>
        [DataMember]
        public int? IconLargeFileId { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        public string IconCssClass { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the parent category.
        /// </summary>
        /// <value>
        /// The parent category
        /// </value>
        [DataMember]
        public virtual Category ParentCategory { get; set; }

        /// <summary>
        /// Gets or sets the child categories.
        /// </summary>
        /// <value>
        /// The child categories.
        /// </value>
        [DataMember]
        public virtual ICollection<Category> ChildCategories { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the small icon.
        /// </summary>
        /// <value>
        /// The small icon.
        /// </value>
        [DataMember]
        public virtual BinaryFile IconSmallFile { get; set; }

        /// <summary>
        /// Gets or sets the large icon.
        /// </summary>
        /// <value>
        /// The large icon.
        /// </value>
        [DataMember]
        public virtual BinaryFile IconLargeFile { get; set; }

        #endregion

        #region Public Methods

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

        #endregion

    }

    #region Entity Configuration

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
            this.HasOptional( p => p.IconSmallFile ).WithMany().HasForeignKey( p => p.IconSmallFileId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.IconLargeFile ).WithMany().HasForeignKey( p => p.IconLargeFileId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
