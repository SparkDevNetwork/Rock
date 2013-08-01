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
    /// Binary File Type POCO Entity.
    /// </summary>
    [Table( "BinaryFileType" )]
    [DataContract]
    public partial class BinaryFileType : Model<BinaryFileType>
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
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Given Name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [AlternateKey]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

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

        /// <summary>
        /// Gets or sets the storage mode entity type id.
        /// </summary>
        /// <value>
        /// The storage mode entity type id.
        /// </value>
        [DataMember]
        public int? StorageEntityTypeId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the small icon.
        /// </summary>
        /// <value>
        /// The small icon.
        /// </value>
        public virtual BinaryFile IconSmallFile { get; set; }

        /// <summary>
        /// Gets or sets the large icon.
        /// </summary>
        /// <value>
        /// The large icon.
        /// </value>
        public virtual BinaryFile IconLargeFile { get; set; }

        /// <summary>
        /// Gets the file count.
        /// </summary>
        /// <value>
        /// The file count.
        /// </value>
        public virtual int FileCount
        {
            get
            {
                return FileQuery.Count();
            }
        }

        /// <summary>
        /// Gets the file query.
        /// </summary>
        /// <value>
        /// The file query.
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
        /// Gets or sets the type of the storage mode entity.
        /// </summary>
        /// <value>
        /// The type of the storage mode entity.
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
