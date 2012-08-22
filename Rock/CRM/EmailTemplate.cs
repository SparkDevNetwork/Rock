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

namespace Rock.CRM
{
    /// <summary>
    /// Email Template POCO Entity.
    /// </summary>
    [Table( "crmEmailTemplate" )]
    public partial class EmailTemplate : ModelWithAttributes<EmailTemplate>, IAuditable
    {
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
		/// Gets or sets the Person Id.
		/// </summary>
		/// <value>
		/// Person Id.
		/// </value>
		[DataMember]
		public int? PersonId { get; set; }
		
		/// <summary>
		/// Gets or sets the Category.
		/// </summary>
		/// <value>
		/// Category.
		/// </value>
		[MaxLength( 100 )]
		[DataMember]
		public string Category { get; set; }
		
		/// <summary>
		/// Gets or sets the Title.
		/// </summary>
		/// <value>
		/// Title.
		/// </value>
		[Required]
		[MaxLength( 100 )]
		[DataMember]
		public string Title { get; set; }
		
		/// <summary>
		/// Gets or sets the From.
		/// </summary>
		/// <value>
		/// From.
		/// </value>
		[MaxLength( 200 )]
		[DataMember]
		public string From { get; set; }
		
		/// <summary>
		/// Gets or sets the To.
		/// </summary>
		/// <value>
		/// To.
		/// </value>
		[DataMember]
		public string To { get; set; }
		
		/// <summary>
		/// Gets or sets the Cc.
		/// </summary>
		/// <value>
		/// Cc.
		/// </value>
		[DataMember]
		public string Cc { get; set; }
		
		/// <summary>
		/// Gets or sets the Bcc.
		/// </summary>
		/// <value>
		/// Bcc.
		/// </value>
		[DataMember]
		public string Bcc { get; set; }
		
		/// <summary>
		/// Gets or sets the Subject.
		/// </summary>
		/// <value>
		/// Subject.
		/// </value>
		[Required]
		[MaxLength( 200 )]
		[DataMember]
		public string Subject { get; set; }
		
		/// <summary>
		/// Gets or sets the Body.
		/// </summary>
		/// <value>
		/// Body.
		/// </value>
		[Required]
		[DataMember]
		public string Body { get; set; }
		
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
		public override string AuthEntity { get { return "CRM.EmailTemplate"; } }
        
		/// <summary>
        /// Gets or sets the Created By Person.
        /// </summary>
        /// <value>
        /// A <see cref="Person"/> object.
        /// </value>
		public virtual Person CreatedByPerson { get; set; }
        
		/// <summary>
        /// Gets or sets the Person.
        /// </summary>
        /// <value>
        /// A <see cref="Person"/> object.
        /// </value>
		public virtual Person Person { get; set; }
        
		/// <summary>
        /// Gets or sets the Modified By Person.
        /// </summary>
        /// <value>
        /// A <see cref="Person"/> object.
        /// </value>
		public virtual Person ModifiedByPerson { get; set; }

    }
    
    /// <summary>
    /// Email Template Configuration class.
    /// </summary>
    public partial class EmailTemplateConfiguration : EntityTypeConfiguration<EmailTemplate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailTemplateConfiguration"/> class.
        /// </summary>
        public EmailTemplateConfiguration()
        {
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.Person ).WithMany( p => p.EmailTemplates ).HasForeignKey( p => p.PersonId ).WillCascadeOnDelete(true);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class EmailTemplateDTO : DTO<EmailTemplate>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Person Id.
        /// </summary>
        /// <value>
        /// Person Id.
        /// </value>
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the Category.
        /// </summary>
        /// <value>
        /// Category.
        /// </value>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the Title.
        /// </summary>
        /// <value>
        /// Title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the From.
        /// </summary>
        /// <value>
        /// From.
        /// </value>
        public string From { get; set; }

        /// <summary>
        /// Gets or sets the To.
        /// </summary>
        /// <value>
        /// To.
        /// </value>
        public string To { get; set; }

        /// <summary>
        /// Gets or sets the Cc.
        /// </summary>
        /// <value>
        /// Cc.
        /// </value>
        public string Cc { get; set; }

        /// <summary>
        /// Gets or sets the Bcc.
        /// </summary>
        /// <value>
        /// Bcc.
        /// </value>
        public string Bcc { get; set; }

        /// <summary>
        /// Gets or sets the Subject.
        /// </summary>
        /// <value>
        /// Subject.
        /// </value>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the Body.
        /// </summary>
        /// <value>
        /// Body.
        /// </value>
        public string Body { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public EmailTemplateDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public EmailTemplateDTO( EmailTemplate emailTemplate )
        {
            CopyFromModel( emailTemplate );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="emailTemplate"></param>
        public override void CopyFromModel( EmailTemplate emailTemplate )
        {
            this.Id = emailTemplate.Id;
            this.Guid = emailTemplate.Guid;
            this.IsSystem = emailTemplate.IsSystem;
            this.PersonId = emailTemplate.PersonId;
            this.Category = emailTemplate.Category;
            this.Title = emailTemplate.Title;
            this.From = emailTemplate.From;
            this.To = emailTemplate.To;
            this.Cc = emailTemplate.Cc;
            this.Bcc = emailTemplate.Bcc;
            this.Subject = emailTemplate.Subject;
            this.Body = emailTemplate.Body;
            this.CreatedDateTime = emailTemplate.CreatedDateTime;
            this.ModifiedDateTime = emailTemplate.ModifiedDateTime;
            this.CreatedByPersonId = emailTemplate.CreatedByPersonId;
            this.ModifiedByPersonId = emailTemplate.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="emailTemplate"></param>
        public override void CopyToModel( EmailTemplate emailTemplate )
        {
            emailTemplate.Id = this.Id;
            emailTemplate.Guid = this.Guid;
            emailTemplate.IsSystem = this.IsSystem;
            emailTemplate.PersonId = this.PersonId;
            emailTemplate.Category = this.Category;
            emailTemplate.Title = this.Title;
            emailTemplate.From = this.From;
            emailTemplate.To = this.To;
            emailTemplate.Cc = this.Cc;
            emailTemplate.Bcc = this.Bcc;
            emailTemplate.Subject = this.Subject;
            emailTemplate.Body = this.Body;
            emailTemplate.CreatedDateTime = this.CreatedDateTime;
            emailTemplate.ModifiedDateTime = this.ModifiedDateTime;
            emailTemplate.CreatedByPersonId = this.CreatedByPersonId;
            emailTemplate.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }
}
