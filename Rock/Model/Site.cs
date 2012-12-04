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
    /// Site POCO Entity.
    /// </summary>
    [Table( "Site" )]
    public partial class Site : Model<Site>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
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
        [MaxLength( 100 )]
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
        [MaxLength( 150 )]
        public string FaviconUrl { get; set; }
        
        /// <summary>
        /// Gets or sets the Apple Touch Icon Url.
        /// </summary>
        /// <value>
        /// Apple Touch Icon Url.
        /// </value>
        [MaxLength( 150 )]
        public string AppleTouchIconUrl { get; set; }
        
        /// <summary>
        /// Gets or sets the Facebook App Id.
        /// </summary>
        /// <value>
        /// Facebook App Id.
        /// </value>
        [MaxLength( 25 )]
        public string FacebookAppId { get; set; }
        
        /// <summary>
        /// Gets or sets the Facebook App Secret.
        /// </summary>
        /// <value>
        /// Facebook App Secret.
        /// </value>
        [MaxLength( 50 )]
        public string FacebookAppSecret { get; set; }
        
        /// <summary>
        /// Gets or sets the Login Page Reference.
        /// </summary>
        /// <value>
        /// Login Page Reference.
        /// </value>
        [MaxLength( 10 )]
        public string LoginPageReference { get; set; }
        
        /// <summary>
        /// Gets or sets the Registration Page Reference.
        /// </summary>
        /// <value>
        /// Registration Page Reference.
        /// </value>
        [MaxLength( 10 )]
        public string RegistrationPageReference { get; set; }
        
        /// <summary>
        /// Gets or sets the Error Page.
        /// </summary>
        /// <value>
        /// Path to the error page for this site..
        /// </value>
        [MaxLength( 200 )]
        public string ErrorPage { get; set; }
        
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
        public static Site Read( int id )
        {
            return Read<Site>( id );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
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
        }
    }
}
