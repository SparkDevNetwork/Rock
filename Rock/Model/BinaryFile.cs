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
    /// File POCO Entity.
    /// </summary>
    [Table( "BinaryFile" )]
    public partial class BinaryFile : Model<BinaryFile>
    {
        /// <summary>
        /// Gets or sets the Temporary.
        /// </summary>
        /// <value>
        /// Temporary.
        /// </value>
        [Required]
        public bool IsTemporary { get; set; }
        
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the Data.
        /// </summary>
        /// <value>
        /// Data.
        /// </value>
        public byte[] Data { get; set; }
        
        /// <summary>
        /// Gets or sets the Url.
        /// </summary>
        /// <value>
        /// Url.
        /// </value>
        [MaxLength( 255 )]
        public string Url { get; set; }
        
        /// <summary>
        /// Gets or sets the File Name.
        /// </summary>
        /// <value>
        /// File Name.
        /// </value>
        [Required]
        [MaxLength( 255 )]
        public string FileName { get; set; }
        
        /// <summary>
        /// Gets or sets the Mime Type.
        /// </summary>
        /// <value>
        /// Mime Type.
        /// </value>
        [Required]
        [MaxLength( 255 )]
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the time that file was last modified.
        /// </summary>
        /// <value>
        /// The last modified time.
        /// </value>
        public DateTime? LastModifiedTime { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        public string Description { get; set; }

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
        public static BinaryFile Read( int id )
        {
            return Read<BinaryFile>( id );
        }

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

    }

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
        }
    }
}
