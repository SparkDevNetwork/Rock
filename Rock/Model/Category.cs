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
using Rock.Security;

namespace Rock.Model
{
    /// <summary>
    /// Represents a category or group of entity objects in RockChMS. A category can be used to group entity instances of <see cref="Rock.Model.EntityType">EntityTypes</see>. 
    /// For an EntityType to be categorizable the EntityType will need to implement the <see cref="Rock.Data.ICategorized"/> interface.
    /// </summary>
    [Table( "Category" )]
    [DataContract]
    public partial class Category : Model<Category>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this Category is part of the RockChMS core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the Category is part of the RockChMS core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the CategoryId of the parent Category. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the CategoryId of the parent Category.
        /// </value>
        [DataMember]
        public int? ParentCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that this Category belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that this Category belongs to.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier column that contains the value (see <see cref="EntityTypeQualifierValue"/>) that is used to narrow the scope of the Category.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the Qualifier Column/Property that contains the <see cref="EntityTypeQualiferValue"/> that is used to 
        /// narrow the scope of the Category.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value that is used to narrow the scope of the Category to a subset or specific instance of an EntityType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the value that is used to narrow the scope of the Category.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Category
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Name of the Category.
        /// </value>
        [MaxLength(100)]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the BinaryFileId of the <see cref="Rock.Model.BinaryFile"/> that is being used as the small icon. This property is only used for file based icons.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the BinaryFileId of the <see cref="Rock.Model.BinaryFile"/> that is being used as the small icon. This value will be
        /// null if a CSS based 
        /// </value>
        [DataMember]
        public int? IconSmallFileId { get; set; }

        /// <summary>
        /// Gets or sets the BinaryFileId of the <see cref="Rock.Model.BinaryFile"/> that is being used as the large icon. This property is only used for file based icons.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the BinaryFileId of the <see cref="Rock.Model.BinaryFile"/> that is being used as the large icon. This
        /// property will be null if a CSS based icon is being used 
        /// </value>
        [DataMember]
        public int? IconLargeFileId { get; set; }

        /// <summary>
        /// Gets or sets the name of the icon CSS class. This property is only used for CSS based icons.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the icon CSS class. This property will be null if a file based icon is being used.
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
        public virtual Category ParentCategory { get; set; }

        /// <summary>
        /// Gets or sets a collection of Categories that are children of the current Category.
        /// </summary>
        /// <value>
        /// The Categories that are children of the current Category.
        /// </value>
        [DataMember]
        public virtual ICollection<Category> ChildCategories { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType" /> that can use this Category.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> that can use this Category.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.BinaryFile"/> that is being used as the small icon file.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.BinaryFile"/> that is being used as the small icon file. If a CSS based icon is being used, this value will be null.
        /// </value>
        [DataMember]
        public virtual BinaryFile IconSmallFile { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.BinaryFile"/> that is being used as the large icon file.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.BinaryFile" /> that is being used as the large icon file. If a CSS based icon is being used, this value will be used.
        /// </value>
        [DataMember]
        public virtual BinaryFile IconLargeFile { get; set; }

        /// <summary>
        /// Gets the parent authority where security authorizations are being inherited from.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override ISecured ParentAuthority
        {
            get
            {
                if ( ParentCategory != null )
                {
                    return ParentCategory;
                }

                return base.ParentAuthority;
            }
        }

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
