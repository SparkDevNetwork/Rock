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
    /// Page Route POCO Entity.
    /// </summary>
    [Table( "cmsPageContext" )]
    public partial class PageContext : ModelWithAttributes<PageContext>, IAuditable
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
		/// Gets or sets the Page Id.
		/// </summary>
		/// <value>
		/// Page Id.
		/// </value>
		[Required]
		[DataMember]
		public int PageId { get; set; }
		
		/// <summary>
		/// Gets or sets the Entity.
		/// </summary>
		/// <value>
		/// Entity.
		/// </value>
		[Required]
		[MaxLength( 200 )]
		[DataMember]
		public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the page parameter that contains the entity's id.
        /// </summary>
        /// <value>
        /// Id Parameter.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember]
        public string IdParameter { get; set; }

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
		public override string AuthEntity { get { return "CMS.PageContext"; } }
        
		/// <summary>
        /// Gets or sets the Page.
        /// </summary>
        /// <value>
        /// A <see cref="Page"/> object.
        /// </value>
		public virtual Page Page { get; set; }
        
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
    /// Page Route Configuration class.
    /// </summary>
    public partial class PageContextConfiguration : EntityTypeConfiguration<PageContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageContextConfiguration"/> class.
        /// </summary>
        public PageContextConfiguration()
        {
			this.HasRequired( p => p.Page ).WithMany( p => p.PageContexts ).HasForeignKey( p => p.PageId ).WillCascadeOnDelete(true);
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class PageContextDTO : DTO<PageContext>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Page Id.
        /// </summary>
        /// <value>
        /// Page Id.
        /// </value>
        public int PageId { get; set; }

        /// <summary>
        /// Gets or sets the Entity.
        /// </summary>
        /// <value>
        /// Entity.
        /// </value>
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the page parameter that contains the entity's id.
        /// </summary>
        /// <value>
        /// Id Parameter.
        /// </value>
        public string IdParameter { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public PageContextDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public PageContextDTO( PageContext pageContext )
        {
            CopyFromModel( pageContext );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="pageContext"></param>
        public override void CopyFromModel( PageContext pageContext )
        {
            this.Id = pageContext.Id;
            this.Guid = pageContext.Guid;
            this.IsSystem = pageContext.IsSystem;
            this.PageId = pageContext.PageId;
            this.Entity = pageContext.Entity;
            this.IdParameter = pageContext.IdParameter;
            this.CreatedDateTime = pageContext.CreatedDateTime;
            this.ModifiedDateTime = pageContext.ModifiedDateTime;
            this.CreatedByPersonId = pageContext.CreatedByPersonId;
            this.ModifiedByPersonId = pageContext.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="pageContext"></param>
        public override void CopyToModel( PageContext pageContext )
        {
            pageContext.Id = this.Id;
            pageContext.Guid = this.Guid;
            pageContext.IsSystem = this.IsSystem;
            pageContext.PageId = this.PageId;
            pageContext.Entity = this.Entity;
            pageContext.IdParameter = this.IdParameter;
            pageContext.CreatedDateTime = this.CreatedDateTime;
            pageContext.ModifiedDateTime = this.ModifiedDateTime;
            pageContext.CreatedByPersonId = this.CreatedByPersonId;
            pageContext.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }
}
