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

namespace Rock.CMS
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
		public override string AuthEntity { get { return "CMS.File"; } }
        
		/// <summary>
        /// Gets or sets the Created By Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person CreatedByPerson { get; set; }
        
		/// <summary>
        /// Gets or sets the Modified By Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person ModifiedByPerson { get; set; }

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

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class FileDTO : DTO<File>
    {
        /// <summary>
        /// Gets or sets the Temporary.
        /// </summary>
        /// <value>
        /// Temporary.
        /// </value>
        public bool IsTemporary { get; set; }

        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
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
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the File Name.
        /// </summary>
        /// <value>
        /// File Name.
        /// </value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the Mime Type.
        /// </summary>
        /// <value>
        /// Mime Type.
        /// </value>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public FileDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public FileDTO( File file )
        {
            CopyFromModel( file );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="file"></param>
        public override void CopyFromModel( File file )
        {
            this.Id = file.Id;
            this.Guid = file.Guid;
            this.IsTemporary = file.IsTemporary;
            this.IsSystem = file.IsSystem;
            this.Data = file.Data;
            this.Url = file.Url;
            this.FileName = file.FileName;
            this.MimeType = file.MimeType;
            this.Description = file.Description;
            this.CreatedDateTime = file.CreatedDateTime;
            this.ModifiedDateTime = file.ModifiedDateTime;
            this.CreatedByPersonId = file.CreatedByPersonId;
            this.ModifiedByPersonId = file.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="file"></param>
        public override void CopyToModel( File file )
        {
            file.Id = this.Id;
            file.Guid = this.Guid;
            file.IsTemporary = this.IsTemporary;
            file.IsSystem = this.IsSystem;
            file.Data = this.Data;
            file.Url = this.Url;
            file.FileName = this.FileName;
            file.MimeType = this.MimeType;
            file.Description = this.Description;
            file.CreatedDateTime = this.CreatedDateTime;
            file.ModifiedDateTime = this.ModifiedDateTime;
            file.CreatedByPersonId = this.CreatedByPersonId;
            file.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }
}
