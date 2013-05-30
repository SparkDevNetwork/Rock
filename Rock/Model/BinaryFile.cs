//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// File POCO Entity.
    /// </summary>
    [Table( "BinaryFile" )]
    [DataContract]
    public partial class BinaryFile : Model<BinaryFile>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the Temporary.
        /// </summary>
        /// <value>
        /// Temporary.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsTemporary { get; set; }
        
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
        /// Gets or sets the binary file type id.
        /// </summary>
        /// <value>
        /// The binary file type id.
        /// </value>
        [DataMember]
        public int? BinaryFileTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Url.
        /// </summary>
        /// <value>
        /// Url.
        /// </value>
        [MaxLength( 255 )]
        [DataMember]
        public string Url { get; set; }
        
        /// <summary>
        /// Gets or sets the File Name.
        /// </summary>
        /// <value>
        /// File Name.
        /// </value>
        [Required] 
        [MaxLength( 255 )]
        [DataMember( IsRequired = true )]
        public string FileName { get; set; }
        
        /// <summary>
        /// Gets or sets the Mime Type.
        /// </summary>
        /// <value>
        /// Mime Type.
        /// </value>
        [Required]
        [MaxLength( 255 )]
        [DataMember( IsRequired = true )]
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the last modified date time.
        /// </summary>
        /// <value>
        /// The last modified date time.
        /// </value>
        [DataMember]
        public DateTime? LastModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the binary file type.
        /// </summary>
        /// <value>
        /// The binary file type.
        /// </value>
        public virtual BinaryFileType BinaryFileType { get; set; }

        /// <summary>
        /// Gets or sets the binary file data.
        /// </summary>
        /// <value>
        /// The binary file data.
        /// </value>
        public virtual BinaryFileData Data { get; set; }

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
            return this.FileName;
        }

        #endregion

        #region StaticMethods

        /// <summary>
        /// Makes a comma delimited list of the permanent.
        /// </summary>
        /// <param name="commaDelimitedIds">The comma delimited ids.</param>
        public static void MakePermanent( string commaDelimitedIds )
        {
            string query = string.Format( "UPDATE BinaryFile SET IsTemporary = 0 WHERE Id IN ({0})", commaDelimitedIds );
            var service = new Service();
            service.ExecuteCommand( query );
        }

        public static void MakePermanent( int id)
        {
            string query = string.Format("UPDATE BinaryFile SET IsTemporary = 0 WHERE Id = {0}", id);
            var service = new Service();
            service.ExecuteCommand( query );
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class BinaryFileConfiguration : EntityTypeConfiguration<BinaryFile>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryFileConfiguration"/> class.
        /// </summary>
        public BinaryFileConfiguration()
        {
            this.HasOptional( f => f.BinaryFileType ).WithMany().HasForeignKey( f => f.BinaryFileTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( f => f.Data ).WithRequired().WillCascadeOnDelete();
        }
    }

    #endregion

}
