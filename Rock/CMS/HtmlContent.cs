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
    /// Html Content POCO Entity.
    /// </summary>
    [Table( "cmsHtmlContent" )]
    public partial class HtmlContent : ModelWithAttributes<HtmlContent>, IAuditable
    {
		/// <summary>
		/// Gets or sets the Block Id.
		/// </summary>
		/// <value>
		/// Block Id.
		/// </value>
		[Required]
		[DataMember]
		public int BlockId { get; set; }
		
		/// <summary>
		/// Gets or sets the Entity Value.
		/// </summary>
		/// <value>
		/// Entity Value.
		/// </value>
		[MaxLength( 200 )]
		[DataMember]
		public string EntityValue { get; set; }
		
		/// <summary>
		/// Gets or sets the Version.
		/// </summary>
		/// <value>
		/// Version.
		/// </value>
		[Required]
		[DataMember]
		public int Version { get; set; }
		
		/// <summary>
		/// Gets or sets the Content.
		/// </summary>
		/// <value>
		/// Content.
		/// </value>
		[Required]
		[DataMember]
		public string Content { get; set; }
		
		/// <summary>
		/// Gets or sets the Approved.
		/// </summary>
		/// <value>
		/// Approved.
		/// </value>
		[Required]
		[DataMember]
		public bool IsApproved { get; set; }
		
		/// <summary>
		/// Gets or sets the Approved By Person Id.
		/// </summary>
		/// <value>
		/// Approved By Person Id.
		/// </value>
		[DataMember]
		public int? ApprovedByPersonId { get; set; }
		
		/// <summary>
		/// Gets or sets the Approved Date Time.
		/// </summary>
		/// <value>
		/// Approved Date Time.
		/// </value>
		[DataMember]
		public DateTime? ApprovedDateTime { get; set; }
		
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
		/// Gets or sets the Start Date Time.
		/// </summary>
		/// <value>
		/// Start Date Time.
		/// </value>
		[DataMember]
		public DateTime? StartDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Expire Date Time.
		/// </summary>
		/// <value>
		/// Expire Date Time.
		/// </value>
		[DataMember]
		public DateTime? ExpireDateTime { get; set; }
		
        /// <summary>
        /// Gets the auth entity.
        /// </summary>
		[NotMapped]
		public override string AuthEntity { get { return "CMS.HtmlContent"; } }
        
		/// <summary>
        /// Gets or sets the Block.
        /// </summary>
        /// <value>
        /// A <see cref="BlockInstance"/> object.
        /// </value>
		public virtual BlockInstance Block { get; set; }
        
		/// <summary>
        /// Gets or sets the Approved By Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person ApprovedByPerson { get; set; }
        
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

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Content;
        }
    }

    /// <summary>
    /// Html Content Configuration class.
    /// </summary>
    public partial class HtmlContentConfiguration : EntityTypeConfiguration<HtmlContent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlContentConfiguration"/> class.
        /// </summary>
        public HtmlContentConfiguration()
        {
			this.HasRequired( p => p.Block ).WithMany( p => p.HtmlContents ).HasForeignKey( p => p.BlockId ).WillCascadeOnDelete(true);
			this.HasOptional( p => p.ApprovedByPerson ).WithMany().HasForeignKey( p => p.ApprovedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class HtmlContentDTO : DTO<HtmlContent>
    {
        /// <summary>
        /// Gets or sets the Block Id.
        /// </summary>
        /// <value>
        /// Block Id.
        /// </value>
        public int BlockId { get; set; }

        /// <summary>
        /// Gets or sets the Entity Value.
        /// </summary>
        /// <value>
        /// Entity Value.
        /// </value>
        public string EntityValue { get; set; }

        /// <summary>
        /// Gets or sets the Version.
        /// </summary>
        /// <value>
        /// Version.
        /// </value>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the Content.
        /// </summary>
        /// <value>
        /// Content.
        /// </value>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the Approved.
        /// </summary>
        /// <value>
        /// Approved.
        /// </value>
        public bool IsApproved { get; set; }

        /// <summary>
        /// Gets or sets the Approved By Person Id.
        /// </summary>
        /// <value>
        /// Approved By Person Id.
        /// </value>
        public int? ApprovedByPersonId { get; set; }

        /// <summary>
        /// Gets or sets the Approved Date Time.
        /// </summary>
        /// <value>
        /// Approved Date Time.
        /// </value>
        public DateTime? ApprovedDateTime { get; set; }
        /// <summary>
        /// Gets or sets the Start Date Time.
        /// </summary>
        /// <value>
        /// Start Date Time.
        /// </value>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Expire Date Time.
        /// </summary>
        /// <value>
        /// Expire Date Time.
        /// </value>
        public DateTime? ExpireDateTime { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public HtmlContentDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public HtmlContentDTO( HtmlContent htmlContent )
        {
            CopyFromModel( htmlContent );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="htmlContent"></param>
        public override void CopyFromModel( HtmlContent htmlContent )
        {
            this.Id = htmlContent.Id;
            this.Guid = htmlContent.Guid;
            this.BlockId = htmlContent.BlockId;
            this.EntityValue = htmlContent.EntityValue;
            this.Version = htmlContent.Version;
            this.Content = htmlContent.Content;
            this.IsApproved = htmlContent.IsApproved;
            this.ApprovedByPersonId = htmlContent.ApprovedByPersonId;
            this.ApprovedDateTime = htmlContent.ApprovedDateTime;
            this.CreatedDateTime = htmlContent.CreatedDateTime;
            this.ModifiedDateTime = htmlContent.ModifiedDateTime;
            this.CreatedByPersonId = htmlContent.CreatedByPersonId;
            this.ModifiedByPersonId = htmlContent.ModifiedByPersonId;
            this.StartDateTime = htmlContent.StartDateTime;
            this.ExpireDateTime = htmlContent.ExpireDateTime;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="htmlContent"></param>
        public override void CopyToModel( HtmlContent htmlContent )
        {
            htmlContent.Id = this.Id;
            htmlContent.Guid = this.Guid;
            htmlContent.BlockId = this.BlockId;
            htmlContent.EntityValue = this.EntityValue;
            htmlContent.Version = this.Version;
            htmlContent.Content = this.Content;
            htmlContent.IsApproved = this.IsApproved;
            htmlContent.ApprovedByPersonId = this.ApprovedByPersonId;
            htmlContent.ApprovedDateTime = this.ApprovedDateTime;
            htmlContent.CreatedDateTime = this.CreatedDateTime;
            htmlContent.ModifiedDateTime = this.ModifiedDateTime;
            htmlContent.CreatedByPersonId = this.CreatedByPersonId;
            htmlContent.ModifiedByPersonId = this.ModifiedByPersonId;
            htmlContent.StartDateTime = this.StartDateTime;
            htmlContent.ExpireDateTime = this.ExpireDateTime;
        }
    }
}
