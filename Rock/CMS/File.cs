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
    public partial class File : ModelWithAttributes<File>, IAuditable
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
		/// Gets or sets the Description.
		/// </summary>
		/// <value>
		/// Description.
		/// </value>
		[DataMember]
		public string Description { get; set; }
		
		/// <summary>
		/// Gets or sets the Created Date Time.
		/// </summary>
		/// <value>
		/// Created Date Time.
		/// </value>
		[DataMember]
		public DateTime? CreatedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Modified Date Time.
		/// </summary>
		/// <value>
		/// Modified Date Time.
		/// </value>
		[DataMember]
		public DateTime? ModifiedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Created By Person Id.
		/// </summary>
		/// <value>
		/// Created By Person Id.
		/// </value>
		[DataMember]
		public int? CreatedByPersonId { get; set; }
		
		/// <summary>
		/// Gets or sets the Modified By Person Id.
		/// </summary>
		/// <value>
		/// Modified By Person Id.
		/// </value>
		[DataMember]
		public int? ModifiedByPersonId { get; set; }
		
        /// <summary>
        /// Gets the auth entity.
        /// </summary>
		[NotMapped]
		public override string AuthEntity { get { return "Cms.File"; } }
        
		/// <summary>
        /// Gets or sets the Created By Person.
        /// </summary>
        /// <value>
        /// A <see cref="Crm.Person"/> object.
        /// </value>
		public virtual Crm.Person CreatedByPerson { get; set; }
        
		/// <summary>
        /// Gets or sets the Modified By Person.
        /// </summary>
        /// <value>
        /// A <see cref="Crm.Person"/> object.
        /// </value>
		public virtual Crm.Person ModifiedByPerson { get; set; }

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
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }
}
