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
    /// Site Domain POCO Entity.
    /// </summary>
    [Table( "cmsSiteDomain" )]
    public partial class SiteDomain : ModelWithAttributes<SiteDomain>, IAuditable
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
		/// Gets or sets the Site Id.
		/// </summary>
		/// <value>
		/// Site Id.
		/// </value>
		[Required]
		[DataMember]
		public int SiteId { get; set; }
		
		/// <summary>
		/// Gets or sets the Domain.
		/// </summary>
		/// <value>
		/// Domain.
		/// </value>
		[Required]
		[MaxLength( 200 )]
		[DataMember]
		public string Domain { get; set; }
		
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
		public override string AuthEntity { get { return "CMS.SiteDomain"; } }
        
		/// <summary>
        /// Gets or sets the Site.
        /// </summary>
        /// <value>
        /// A <see cref="Site"/> object.
        /// </value>
		public virtual Site Site { get; set; }
        
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
    /// Site Domain Configuration class.
    /// </summary>
    public partial class SiteDomainConfiguration : EntityTypeConfiguration<SiteDomain>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteDomainConfiguration"/> class.
        /// </summary>
        public SiteDomainConfiguration()
        {
			this.HasRequired( p => p.Site ).WithMany( p => p.SiteDomains ).HasForeignKey( p => p.SiteId ).WillCascadeOnDelete(true);
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class SiteDomainDTO : DTO<SiteDomain>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Site Id.
        /// </summary>
        /// <value>
        /// Site Id.
        /// </value>
        public int SiteId { get; set; }

        /// <summary>
        /// Gets or sets the Domain.
        /// </summary>
        /// <value>
        /// Domain.
        /// </value>
        public string Domain { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public SiteDomainDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public SiteDomainDTO( SiteDomain siteDomain )
        {
            CopyFromModel( siteDomain );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="siteDomain"></param>
        public override void CopyFromModel( SiteDomain siteDomain )
        {
            this.Id = siteDomain.Id;
            this.Guid = siteDomain.Guid;
            this.IsSystem = siteDomain.IsSystem;
            this.SiteId = siteDomain.SiteId;
            this.Domain = siteDomain.Domain;
            this.CreatedDateTime = siteDomain.CreatedDateTime;
            this.ModifiedDateTime = siteDomain.ModifiedDateTime;
            this.CreatedByPersonId = siteDomain.CreatedByPersonId;
            this.ModifiedByPersonId = siteDomain.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="siteDomain"></param>
        public override void CopyToModel( SiteDomain siteDomain )
        {
            siteDomain.Id = this.Id;
            siteDomain.Guid = this.Guid;
            siteDomain.IsSystem = this.IsSystem;
            siteDomain.SiteId = this.SiteId;
            siteDomain.Domain = this.Domain;
            siteDomain.CreatedDateTime = this.CreatedDateTime;
            siteDomain.ModifiedDateTime = this.ModifiedDateTime;
            siteDomain.CreatedByPersonId = this.CreatedByPersonId;
            siteDomain.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }
}
