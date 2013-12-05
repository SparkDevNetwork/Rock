//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a type or category of binary files in RockChMS, and configures how binary files of this type are stored and accessed.
    /// </summary>
    [Table( "BinaryFileType" )]
    [DataContract]
    public partial class BinaryFileType : Model<BinaryFileType>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this BinaryFileType is part of the RockChMS core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this is part of the core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the given Name of the BinaryFileType. This value is an alternate key and is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the given Name of the BinaryFileType. 
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [AlternateKey]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of the BinaryFileType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the BinaryFileType.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.BinaryFile"/> that is used as the small icon representing this BinaryFileType.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.BinaryFile"/> that is used as the small icon. If a file based icon is not used, or a small icon isn't used this value will be null.
        /// </value>
        [DataMember]
        public int? IconSmallFileId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.BinaryFile"/> that is as used as the large icon representing this BinaryFileType.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.BinaryFile"/> that is used as the large icon. If a file based icon is not used, or a large icon is not used, this value will be null.
        /// </value>
        [DataMember]
        public int? IconLargeFileId { get; set; }

        /// <summary>
        /// Gets or sets the CSS class that is used for a vector/CSS icon.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the CSS class that is used for a vector/CSS based icon.
        /// </value>
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the Id of the storage service <see cref="Rock.Model.EntityType"/> that is used to store files of this type.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32" /> representing the Id of the storage service <see cref="Rock.Model.EntityType"/>.
        /// </value>
        [DataMember]
        public int? StorageEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether to allow caching on any <see cref="Rock.Model.BinaryFile"/> child entities.
        /// </summary>
        /// <value>
        ///  A <see cref="System.Boolean"/> value that is <c>true</c> if caching is allowed; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowCaching { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.BinaryFile"/> that represents the small icon.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.BinaryFile"/> that represents the small icon.
        /// </value>
        public virtual BinaryFile IconSmallFile { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.BinaryFile"/> that represents the large icon.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.BinaryFile"/> that represents the large icon
        /// </value>
        public virtual BinaryFile IconLargeFile { get; set; }

        /// <summary>
        /// Gets the count of <see cref="Rock.Model.BinaryFile" /> entities that are children of this <see cref="Rock.Model.BinaryFileType"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the count of <see cref="Rock.Model.BinaryFile"/> entities that are children of this <see cref="Rock.Model.BinaryFileType"/>.
        /// </value>
        public virtual int FileCount
        {
            get
            {
                return FileQuery.Count();
            }
        }

        /// <summary>
        /// Gets a queryable collection of <see cref="Rock.Model.BinaryFile" /> entities that are children of this <see cref="Rock.Model.BinaryFileType"/>.
        /// </summary>
        /// <value>
        /// A queryable collection of <see cref="Rock.Model.BinaryFile"/> entities that are children of this<see cref="Rock.Model.BinaryFileType"/>.
        /// </value>
        public virtual IQueryable<BinaryFile> FileQuery
        {
            get
            {
                var fileService = new BinaryFileService();
                var qry = fileService.Queryable()
                    .Where( f => f.BinaryFileTypeId.HasValue && f.BinaryFileTypeId == this.Id );
                return qry;
            }
        }

        /// <summary>
        /// Gets or sets the storage mode <see cref="Rock.Model.EntityType"/>.
        /// </summary>
        /// <value>
        /// The storage mode <see cref="Rock.Model.EntityType"/>.
        /// </value>
        [DataMember]
        public virtual EntityType StorageEntityType { get; set; }

        #endregion

        #region Methods

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
    /// File Configuration class.
    /// </summary>
    public partial class BinaryFileTypeConfiguration : EntityTypeConfiguration<BinaryFileType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryFileTypeConfiguration"/> class.
        /// </summary>
        public BinaryFileTypeConfiguration()
        {
            this.HasOptional( f => f.IconSmallFile ).WithMany().HasForeignKey( f => f.IconSmallFileId ).WillCascadeOnDelete( false );
            this.HasOptional( f => f.IconLargeFile ).WithMany().HasForeignKey( f => f.IconLargeFileId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
