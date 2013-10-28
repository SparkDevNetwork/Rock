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
    /// Site Model Entity. A Site in Rock is a collection of <see cref="Page">pages</see> and usually 
    /// associated with one or more <see cref="SiteDomain">SiteDomains </see>.
    /// </summary>
    [Table( "Site" )]
    [DataContract]
    public partial class Site : Model<Site>
    {
        /// <summary>
        /// Gets or sets a flag indicating if this Site was created by and is part of the RockChMS core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this Site is part of the RockChMS core system/framework, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the Site. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the name of the Site.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets a user defined description/summary  of the Site.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that contains the description of the Site. If no description is provided
        ///  this value will be an null string.
        /// </value>
        [DataMember]
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the Theme that is used on the Site.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that contains the name of the theme that is being used on the Site.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Theme { get; set; }
        
        /// <summary>
        /// Gets or sets the Id of the Site's default <see cref="Rock.Model.Page"/>. 
        /// </summary>
        /// <remarks>
        /// This is the <see cref="Rock.Model.Page"/> that is loaded when the user browses to the root of one of the Site's <see cref="Rock.Model.SiteDomain"/>
        /// (i.e. http://www.mychurchdomain.org/) without providing a page or route. 
        /// </remarks>
        /// <value>
        /// An <see cref="System.Int32"/> containing the Id of the Site's default <see cref="Rock.Model.Page"/>. If the site doesn't have a default <see cref="Rock.Model.Page"/>
        /// this value will be null.
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
        /// Gets or sets the Id of the Site's login <see cref="Rock.Model.Page"/>
        /// </summary>
        /// <remarks>
        /// This is the <see cref="Rock.Model.Page"/> that is loaded when the user is not logged in and they browse to a page or block that requires authentication.
        /// </remarks>
        /// <value>
        /// An <see cref="System.Int32"/> containing the Id of the Site's login <see cref="Rock.Model.Page"/>. If the site doesn't have a login <see cref="Rock.Model.Page"/>
        /// this value will be null.
        /// </value>
        [DataMember]
        public int? LoginPageId { get; set; }

        /// <summary>
        /// Gets or sets the login page route unique identifier.
        /// If this has a value (and the PageRoute can be found) use this instead of the LoginPageId
        /// </summary>
        /// <value>
        /// The login page route unique identifier.
        /// </value>
        [DataMember]
        public int? LoginPageRouteId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Site's registration <see cref="Rock.Model.Page"/>
        /// </summary>
        /// <remarks>
        /// This is the <see cref="Rock.Model.Page"/> that is loaded when the user requests to register for a <see cref="Rock.Model.Group"/>.
        /// </remarks>
        /// <value>
        /// An <see cref="System.Int32"/> containing the Id of the Site's registration <see cref="Rock.Model.Page"/>. If the site doesn't have a registration <see cref="Rock.Model.Page"/>
        /// this value will be null.
        /// </value>
        [DataMember]
        public int? RegistrationPageId { get; set; }

        /// <summary>
        /// Gets or sets the registration page route unique identifier.
        /// If this has a value (and the PageRoute can be found) use this instead of the RegistrationPageId
        /// </summary>
        /// <value>
        /// The registration page route unique identifier.
        /// </value>
        [DataMember]
        public int? RegistrationPageRouteId { get; set; }

        /// <summary>
        /// Gets or sets the path to the Site's Favicon.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the Url to the Site's Favicon. This property will be null if the site does not use a Favicon.
        /// </value>
        [MaxLength( 150 )]
        [DataMember]
        public string FaviconUrl { get; set; }
        
        /// <summary>
        /// Gets or sets the path to the site's AppleTouch icon. 
        /// </summary>
        /// <remarks>
        /// This icon is a .png file used on iOS devices that is shown when a web application saved to the home screen.  For complete details on 
        /// how to use these, please visit. https://developer.apple.com/library/ios/documentation/AppleApplications/Reference/SafariWebContent/ConfiguringWebApplications/ConfiguringWebApplications.html
        /// </remarks>
        /// <value>
        /// A <see cref="System.String"/> containing the path to the site's AppleTouch icon.
        /// </value>
        [MaxLength( 150 )]
        [DataMember]
        public string AppleTouchIconUrl { get; set; }
        
        /// <summary>
        /// Gets or sets the Site's Facebook AppId for utilizing the Facebook SDK.
        /// </summary>
        /// <remarks>
        /// Each site that utilizes the Facebook SDK requires a different AppId. More info is available at http://developer.facebook.com
        /// </remarks>
        /// <value>
        /// Facebook App Id.
        /// </value>
        [MaxLength( 25 )]
        [DataMember]
        public string FacebookAppId { get; set; }

        /// <summary>
        /// Gets or sets the site's App Secret for the Facebook API
        /// </summary>
        /// <value>
        /// Facebook App Secret.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string FacebookAppSecret { get; set; }
        
        /// <summary>
        /// Gets or sets the path to the error page.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the path to the error page.
        /// </value>
        [MaxLength( 260 )]
        [DataMember]
        public string ErrorPage { get; set; }
        
        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.Layout"/> entities that are a part of the Site.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.Layout"/> entities that are a part of the site.
        /// </value>
        [DataMember]
        public virtual ICollection<Layout> Layouts { get; set; }
        
        /// <summary>
        /// Gets or sets the collection of <see cref="Rock.Model.SiteDomain"/> entities that reference the Site.
        /// </summary>
        /// <value>
        /// Collection of <see cref="Rock.Model.SiteDomain"/> entities that reference the Site.
        /// </value>
        [DataMember]
        public virtual ICollection<SiteDomain> SiteDomains { get; set; }
        
        /// <summary>
        /// Gets or sets the default <see cref="Rock.Model.Page"/> page for the site.
        /// </summary>
        /// <value>
        /// The default <see cref="Rock.Model.Page"/> for the site. 
        /// </value>
        public virtual Page DefaultPage { get; set; }

        /// <summary>
        /// Gets or sets the default <see cref="Rock.Model.PageRoute"/> page route for this site. If this value is null, the DefaultPage will be used
        /// </summary>
        /// <value>
        /// The default page route.
        /// </value>
        public virtual PageRoute DefaultPageRoute { get; set; }

        /// <summary>
        /// Gets or sets the login <see cref="Rock.Model.Page"/> page for the site.
        /// </summary>
        /// <value>
        /// The login <see cref="Rock.Model.Page"/> for the site. 
        /// </value>
        public virtual Page LoginPage { get; set; }

        /// <summary>
        /// Gets or sets the login <see cref="Rock.Model.PageRoute"/> page route for this site. If this value is null, the LoginPage will be used
        /// </summary>
        /// <value>
        /// The login page route.
        /// </value>
        public virtual PageRoute LoginPageRoute { get; set; }

        /// <summary>
        /// Gets or sets the registration <see cref="Rock.Model.Page"/> page for the site.
        /// </summary>
        /// <value>
        /// The registration <see cref="Rock.Model.Page"/> for the site. 
        /// </value>
        public virtual Page RegistrationPage { get; set; }

        /// <summary>
        /// Gets or sets the registration <see cref="Rock.Model.PageRoute"/> page route for this site. If this value is null, the RegistrationPage will be used
        /// </summary>
        /// <value>
        /// The registration page route.
        /// </value>
        public virtual PageRoute RegistrationPageRoute { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the Name of the site that that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the Name of the site that represents this instance.
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
            this.HasOptional( p => p.LoginPage ).WithMany().HasForeignKey( p => p.LoginPageId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.LoginPageRoute ).WithMany().HasForeignKey( p => p.LoginPageRouteId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.RegistrationPage ).WithMany().HasForeignKey( p => p.RegistrationPageId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.RegistrationPageRoute ).WithMany().HasForeignKey( p => p.RegistrationPageRouteId ).WillCascadeOnDelete( false );
        }
    }
}
