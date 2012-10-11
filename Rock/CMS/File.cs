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
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Cms
{
    /// <summary>
    /// File POCO Entity.
    /// </summary>
    [Table( "cmsFile" )]
    public partial class File : Model<File>
    {
        /// <summary>
        /// Gets or sets the Temporary.
        /// </summary>
        /// <value>
        /// Temporary.
        /// </value>
        [Required]
        [DataMember]
        public bool IsTemporary { get; set; }
        
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        [DataMember]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the Data.
        /// </summary>
        /// <value>
        /// Data.
        /// </value>
        [DataMember]
        public byte[] Data { get; set; }
        
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
        [DataMember]
        public string FileName { get; set; }
        
        /// <summary>
        /// Gets or sets the Mime Type.
        /// </summary>
        /// <value>
        /// Mime Type.
        /// </value>
        [Required]
        [MaxLength( 255 )]
        [DataMember]
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the time that file was last modified.
        /// </summary>
        /// <value>
        /// The last modified time.
        /// </value>
        [DataMember]
        public DateTimeOffset? LastModifiedTime { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        [DataMember]
        public string Description { get; set; }
        
        /// <summary>
        /// Gets the auth entity.
        /// </summary>
        [NotMapped]
        public override string EntityTypeName { get { return "Cms.File"; } }
        
        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static File Read( int id )
        {
            return Read<File>( id );
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
    public partial class FileConfiguration : EntityTypeConfiguration<File>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileConfiguration"/> class.
        /// </summary>
        public FileConfiguration()
        {
        }
    }
}
