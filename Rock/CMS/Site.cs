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
    /// Site POCO Entity.
    /// </summary>
    [Table( "cmsSite" )]
    public partial class Site : ModelWithAttributes<Site>, IAuditable
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
		/// Gets or sets the Name.
		/// </summary>
		/// <value>
		/// Name.
		/// </value>
		[Required]
		[MaxLength( 100 )]
		[DataMember]
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
		/// Gets or sets the Theme.
		/// </summary>
		/// <value>
		/// Theme.
		/// </value>
		[MaxLength( 100 )]
		[DataMember]
		public string Theme { get; set; }
		
		/// <summary>
		/// Gets or sets the Default Page Id.
		/// </summary>
		/// <value>
		/// Default Page Id.
		/// </value>
		[DataMember]
		public int? DefaultPageId { get; set; }
		
		/// <summary>
		/// Gets or sets the Favicon Url.
		/// </summary>
		/// <value>
		/// Favicon Url.
		/// </value>
		[MaxLength( 150 )]
		[DataMember]
		public string FaviconUrl { get; set; }
		
		/// <summary>
		/// Gets or sets the Apple Touch Icon Url.
		/// </summary>
		/// <value>
		/// Apple Touch Icon Url.
		/// </value>
		[MaxLength( 150 )]
		[DataMember]
		public string AppleTouchIconUrl { get; set; }
		
		/// <summary>
		/// Gets or sets the Facebook App Id.
		/// </summary>
		/// <value>
		/// Facebook App Id.
		/// </value>
		[MaxLength( 25 )]
		[DataMember]
		public string FacebookAppId { get; set; }
		
		/// <summary>
		/// Gets or sets the Facebook App Secret.
		/// </summary>
		/// <value>
		/// Facebook App Secret.
		/// </value>
		[MaxLength( 50 )]
		[DataMember]
		public string FacebookAppSecret { get; set; }
		
		/// <summary>
		/// Gets or sets the Login Page Reference.
		/// </summary>
		/// <value>
		/// Login Page Reference.
		/// </value>
		[MaxLength( 10 )]
		[DataMember]
		public string LoginPageReference { get; set; }
		
		/// <summary>
		/// Gets or sets the Registration Page Reference.
		/// </summary>
		/// <value>
		/// Registration Page Reference.
		/// </value>
		[MaxLength( 10 )]
		[DataMember]
		public string RegistrationPageReference { get; set; }
		
		/// <summary>
		/// Gets or sets the Error Page.
		/// </summary>
		/// <value>
		/// Path to the error page for this site..
		/// </value>
		[MaxLength( 200 )]
		[DataMember]
		public string ErrorPage { get; set; }
		
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
		public override string AuthEntity { get { return "CMS.Site"; } }
        
		/// <summary>
        /// Gets or sets the Pages.
        /// </summary>
        /// <value>
        /// Collection of Pages.
        /// </value>
		public virtual ICollection<Page> Pages { get; set; }
        
		/// <summary>
        /// Gets or sets the Site Domains.
        /// </summary>
        /// <value>
        /// Collection of Site Domains.
        /// </value>
		public virtual ICollection<SiteDomain> SiteDomains { get; set; }
        
		/// <summary>
        /// Gets or sets the Default Page.
        /// </summary>
        /// <value>
        /// A <see cref="Page"/> object.
        /// </value>
		public virtual Page DefaultPage { get; set; }
        
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

        public Site()
        {
            SiteDomains = new System.Collections.ObjectModel.Collection<SiteDomain>();
        }

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static Site Read( int id )
        {
            return Read<Site>( id );
        }
    }

    /// <summary>
    /// Site Configuration class.
    /// </summary>
    public partial class SiteConfiguration : EntityTypeConfiguration<Site>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteConfiguration"/> class.
        /// </summary>
        public SiteConfiguration()
        {
			this.HasOptional( p => p.DefaultPage ).WithMany( p => p.Sites ).HasForeignKey( p => p.DefaultPageId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class SiteDTO : DTO<Site>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Theme.
        /// </summary>
        /// <value>
        /// Theme.
        /// </value>
        public string Theme { get; set; }

        /// <summary>
        /// Gets or sets the Default Page Id.
        /// </summary>
        /// <value>
        /// Default Page Id.
        /// </value>
        public int? DefaultPageId { get; set; }

        /// <summary>
        /// Gets or sets the Favicon Url.
        /// </summary>
        /// <value>
        /// Favicon Url.
        /// </value>
        public string FaviconUrl { get; set; }

        /// <summary>
        /// Gets or sets the Apple Touch Icon Url.
        /// </summary>
        /// <value>
        /// Apple Touch Icon Url.
        /// </value>
        public string AppleTouchIconUrl { get; set; }

        /// <summary>
        /// Gets or sets the Facebook App Id.
        /// </summary>
        /// <value>
        /// Facebook App Id.
        /// </value>
        public string FacebookAppId { get; set; }

        /// <summary>
        /// Gets or sets the Facebook App Secret.
        /// </summary>
        /// <value>
        /// Facebook App Secret.
        /// </value>
        public string FacebookAppSecret { get; set; }

        /// <summary>
        /// Gets or sets the Login Page Reference.
        /// </summary>
        /// <value>
        /// Login Page Reference.
        /// </value>
        public string LoginPageReference { get; set; }

        /// <summary>
        /// Gets or sets the Registration Page Reference.
        /// </summary>
        /// <value>
        /// Registration Page Reference.
        /// </value>
        public string RegistrationPageReference { get; set; }

        /// <summary>
        /// Gets or sets the Error Page.
        /// </summary>
        /// <value>
        /// Error Page.
        /// </value>
        public string ErrorPage { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public SiteDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public SiteDTO( Site site )
        {
            CopyFromModel( site );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="site"></param>
        public override void CopyFromModel( Site site )
        {
            this.Id = site.Id;
            this.Guid = site.Guid;
            this.IsSystem = site.IsSystem;
            this.Name = site.Name;
            this.Description = site.Description;
            this.Theme = site.Theme;
            this.DefaultPageId = site.DefaultPageId;
            this.FaviconUrl = site.FaviconUrl;
            this.AppleTouchIconUrl = site.AppleTouchIconUrl;
            this.FacebookAppId = site.FacebookAppId;
            this.FacebookAppSecret = site.FacebookAppSecret;
            this.LoginPageReference = site.LoginPageReference;
            this.RegistrationPageReference = site.RegistrationPageReference;
            this.ErrorPage = site.ErrorPage;
            this.CreatedDateTime = site.CreatedDateTime;
            this.ModifiedDateTime = site.ModifiedDateTime;
            this.CreatedByPersonId = site.CreatedByPersonId;
            this.ModifiedByPersonId = site.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="site"></param>
        public override void CopyToModel( Site site )
        {
            site.Id = this.Id;
            site.Guid = this.Guid;
            site.IsSystem = this.IsSystem;
            site.Name = this.Name;
            site.Description = this.Description;
            site.Theme = this.Theme;
            site.DefaultPageId = this.DefaultPageId;
            site.FaviconUrl = this.FaviconUrl;
            site.AppleTouchIconUrl = this.AppleTouchIconUrl;
            site.FacebookAppId = this.FacebookAppId;
            site.FacebookAppSecret = this.FacebookAppSecret;
            site.LoginPageReference = this.LoginPageReference;
            site.RegistrationPageReference = this.RegistrationPageReference;
            site.ErrorPage = this.ErrorPage;
            site.CreatedDateTime = this.CreatedDateTime;
            site.ModifiedDateTime = this.ModifiedDateTime;
            site.CreatedByPersonId = this.CreatedByPersonId;
            site.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }
}
