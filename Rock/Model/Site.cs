//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Site POCO Entity.
    /// </summary>
    [Table( "Site" )]
    [DataContract]
    public partial class Site : Model<Site>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
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
        public int DefaultPageId { get; set; }

        /// <summary>
        /// Gets or sets the default page route unique identifier.
        /// If this has a value (and the PageRoute can be found) use this instead of the DefaultPageId
        /// </summary>
        /// <value>
        /// The default page route unique identifier.
        /// </value>
        [DataMember]
        public int? DefaultPageRouteId { get; set; }
        
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
        [MaxLength( 260 )]
        [DataMember]
        public string LoginPageReference { get; set; }
        
        /// <summary>
        /// Gets or sets the Registration Page Reference.
        /// </summary>
        /// <value>
        /// Registration Page Reference.
        /// </value>
        [MaxLength( 260 )]
        [DataMember]
        public string RegistrationPageReference { get; set; }
        
        /// <summary>
        /// Gets or sets the Error Page.
        /// </summary>
        /// <value>
        /// Path to the error page for this site..
        /// </value>
        [MaxLength( 260 )]
        [DataMember]
        public string ErrorPage { get; set; }
        
        /// <summary>
        /// Gets or sets the Pages.
        /// </summary>
        /// <value>
        /// Collection of Pages.
        /// </value>
        [DataMember]
        public virtual ICollection<Page> Pages { get; set; }
        
        /// <summary>
        /// Gets or sets the Site Domains.
        /// </summary>
        /// <value>
        /// Collection of Site Domains.
        /// </value>
        [DataMember]
        public virtual ICollection<SiteDomain> SiteDomains { get; set; }
        
        /// <summary>
        /// Gets or sets the Default Page.
        /// </summary>
        /// <value>
        /// A <see cref="Page"/> object.
        /// </value>
        public virtual Page DefaultPage { get; set; }

        /// <summary>
        /// Gets or sets the default page route.
        /// </summary>
        /// <value>
        /// The default page route.
        /// </value>
        public virtual PageRoute DefaultPageRoute { get; set; }

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
            this.HasRequired( p => p.DefaultPage ).WithMany().HasForeignKey( p => p.DefaultPageId ).WillCascadeOnDelete(false);
            this.HasOptional( p => p.DefaultPageRoute ).WithMany().HasForeignKey( p => p.DefaultPageRouteId ).WillCascadeOnDelete( false );
        }
    }
}
