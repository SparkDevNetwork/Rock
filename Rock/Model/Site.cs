// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Rock.Data;
using Rock.UniversalSearch;
using Rock.UniversalSearch.Crawler;
using Rock.UniversalSearch.IndexModels;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Site Model Entity. A Site in Rock is a collection of <see cref="Page">pages</see> and usually 
    /// associated with one or more <see cref="SiteDomain">SiteDomains </see>.
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "Site" )]
    [DataContract]
    public partial class Site : Model<Site>, IRockIndexable, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this Site was created by and is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this Site is part of the Rock core system/framework, otherwise <c>false</c>.
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
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
        private bool _isActive = true;

        /// <summary>
        /// Gets or sets the configuration mobile phone file identifier.
        /// </summary>
        /// <value>
        /// The configuration mobile phone file identifier.
        /// </value>
        [DataMember]
        public int? ConfigurationMobilePhoneFileId { get; set; }

        /// <summary>
        /// Gets or sets the configuration tablet file identifier.
        /// </summary>
        /// <value>
        /// The configuration tablet file identifier.
        /// </value>
        [DataMember]
        public int? ConfigurationMobileTabletFileId { get; set; }

        /// <summary>
        /// Gets or sets the additional settings.
        /// </summary>
        /// <value>
        /// The additional settings.
        /// </value>
        [DataMember]
        public string AdditionalSettings { get; set; }

        /// <summary>
        /// Gets or sets the type of the site.
        /// </summary>
        /// <value>
        /// The type of the site.
        /// </value>
        [DataMember]
        public SiteType SiteType { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail file identifier.
        /// </summary>
        /// <value>
        /// The thumbnail file identifier.
        /// </value>
        [DataMember]
        public int? ThumbnailFileId { get; set; }

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
        public int? DefaultPageId { get; set; }

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
        /// Gets or sets the change password page identifier.
        /// </summary>
        /// <value>
        /// The change password page identifier.
        /// </value>
        [DataMember]
        public int? ChangePasswordPageId { get; set; }

        /// <summary>
        /// Gets or sets the change password page route identifier.
        /// </summary>
        /// <value>
        /// The change password page route identifier.
        /// </value>
        [DataMember]
        public int? ChangePasswordPageRouteId { get; set; }

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
        /// Gets or sets the Id of the 404 <see cref="Rock.Model.Page"/>
        /// </summary>
        /// <remarks>
        /// This is the <see cref="Rock.Model.Page"/> that is loaded when a page is not found.
        /// </remarks>
        /// <value>
        /// An <see cref="System.Int32"/> containing the Id of the Site's 404 <see cref="Rock.Model.Page"/>. 
        /// </value>
        [DataMember]
        public int? PageNotFoundPageId { get; set; }

        /// <summary>
        /// Gets or sets the 404 page route unique identifier.
        /// </summary>
        /// <value>
        /// The 404 page route unique identifier.
        /// </value>
        [DataMember]
        public int? PageNotFoundPageRouteId { get; set; }

        /// <summary>
        /// Gets or sets the communication page identifier.
        /// </summary>
        /// <value>
        /// The communication page identifier.
        /// </value>
        [DataMember]
        public int? CommunicationPageId { get; set; }

        /// <summary>
        /// Gets or sets the communication page route identifier.
        /// </summary>
        /// <value>
        /// The communication page route identifier.
        /// </value>
        [DataMember]
        public int? CommunicationPageRouteId { get; set; }

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
        /// Gets or sets the google analytics code.
        /// </summary>
        /// <value>
        /// The google analytics code.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string GoogleAnalyticsCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable mobile redirect].
        /// </summary>
        /// <value>
        /// <c>true</c> if [enable mobile redirect]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableMobileRedirect { get; set; }

        /// <summary>
        /// Gets or sets the mobile page identifier.
        /// </summary>
        /// <value>
        /// The mobile page identifier.
        /// </value>
        [DataMember]
        public int? MobilePageId { get; set; }

        /// <summary>
        /// Gets or sets the external URL.
        /// </summary>
        /// <value>
        /// The external URL.
        /// </value>
        [MaxLength( 260 )]
        [DataMember]
        public string ExternalUrl { get; set; }

        /// <summary>
        /// The Allowed Frame Domains designates which external domains/sites are allowed to embed iframes of this site.
        /// It controls what is put into the Content-Security-Policy HTTP response header.
        /// This is in accordance with the Content Security Policy described here http://w3c.github.io/webappsec-csp/#csp-header
        /// and here https://www.owasp.org/index.php/Content_Security_Policy_Cheat_Sheet
        /// </summary>
        /// <value>
        /// A space delimited list of domains.
        /// </value>
        [DataMember]
        public string AllowedFrameDomains { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [redirect tablets].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [redirect tablets]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool RedirectTablets { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to log Page Views into the Interaction tables for pages in this site
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable page views]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        [DefaultValue( true )]
        public bool EnablePageViews
        {
            get { return _enablePageViews; }
            set { _enablePageViews = value; }
        }
        private bool _enablePageViews = true;

        /// <summary>
        /// Gets or sets the content of the page header.
        /// </summary>
        /// <value>
        /// The content of the page header.
        /// </value>
        [DataMember]
        public string PageHeaderContent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow indexing].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow indexing]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowIndexing
        {
            get { return _allowIndexing; }
            set { _allowIndexing = value; }
        }
        private bool _allowIndexing = true;


        /// <summary>
        /// Gets or sets a value indicating whether this instance is index enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is index enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsIndexEnabled { get; set; }

        /// <summary>
        /// Gets or sets the index starting location.
        /// </summary>
        /// <value>
        /// The index starting location.
        /// </value>
        [DataMember]
        public string IndexStartingLocation { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether [requires encryption].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [requires encryption]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool RequiresEncryption { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this site should be available to be used for shortlinks (the shortlink can still reference url of other sites).
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enabled for shortening]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnabledForShortening { get; set; } = true;

        /// <summary>
        /// Gets or sets the favicon binary file identifier.
        /// </summary>
        /// <value>
        /// The favicon binary file identifier.
        /// </value>
        [DataMember]
        public int? FavIconBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the site logo binary file identifier.
        /// </summary>
        /// <value>
        /// The site logo binary file identifier.
        /// </value>
        [DataMember]
        public int? SiteLogoBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the latest version date time.
        /// </summary>
        /// <value>
        /// The latest version date time.
        /// </value>
        [DataMember]
        public DateTime? LatestVersionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the configuration mobile file path.
        /// </summary>
        /// <value>
        /// The configuration mobile file path.
        /// </value>
        [NotMapped]
        public string ConfigurationMobilePhoneFileUrl
        {
            get
            {
                return Site.GetFileUrl( this.ConfigurationMobilePhoneFileId );
            }
            private set { }
        }

        /// <summary>
        /// Gets or sets the configuration tablet file path.
        /// </summary>
        /// <value>
        /// The configuration tablet file path.
        /// </value>
        [NotMapped]
        public string ConfigurationTabletFileUrl
        {
            get
            {
                return Site.GetFileUrl( this.ConfigurationMobileTabletFileId );
            }
            private set { }
        }

        /// <summary>
        /// Gets the thumbnail file URL.
        /// </summary>
        /// <value>
        /// The thumbnail file URL.
        /// </value>
        [NotMapped]
        public string ThumbnailFileUrl
        {
            get
            {
                return Site.GetFileUrl( this.ThumbnailFileId );
            }
            private set { }
        }


        /// <summary>
        /// Gets or sets the FontAwesome icon CSS weight that will be used for the Site
        /// </summary>
        /// <value>
        /// The icon CSS weight.
        /// </value>
        [DataMember]
        [RockObsolete( "1.8" )]
        [Obsolete( "Moved to Theme" )]
        public IconCssWeight IconCssWeight { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.Layout"/> entities that are a part of the Site.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.Layout"/> entities that are a part of the site.
        /// </value>
        [DataMember]
        public virtual ICollection<Layout> Layouts { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="Rock.Model.Block">Blocks</see> that are used on the site.
        /// </summary>
        /// <value>
        /// Collection of <see cref="Rock.Model.Block"/> entities that are used on the site.
        /// </value>
        [DataMember]
        public virtual ICollection<Block> Blocks { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="Rock.Model.SiteDomain"/> entities that reference the Site.
        /// </summary>
        /// <value>
        /// Collection of <see cref="Rock.Model.SiteDomain"/> entities that reference the Site.
        /// </value>
        [DataMember]
        public virtual ICollection<SiteDomain> SiteDomains { get; set; }

        /// <summary>
        /// Gets or sets the icon extensions.
        /// </summary>
        /// <value>
        /// The icon extensions.
        /// </value>
        [RockObsolete( "1.8" )]
        [Obsolete( "Moved to Theme" )]
        public virtual ICollection<DefinedValue> IconExtensions { get; set; } = new Collection<DefinedValue>();

        /// <summary>
        /// Gets or sets the default <see cref="Rock.Model.Page"/> page for the site.
        /// </summary>
        /// <value>
        /// The default <see cref="Rock.Model.Page"/> for the site. 
        /// </value>
        [DataMember]
        public virtual Page DefaultPage { get; set; }

        /// <summary>
        /// Gets or sets the default <see cref="Rock.Model.PageRoute"/> page route for this site. If this value is null, the DefaultPage will be used
        /// </summary>
        /// <value>
        /// The default page route.
        /// </value>
        [DataMember]
        public virtual PageRoute DefaultPageRoute { get; set; }

        /// <summary>
        /// Gets or sets the login <see cref="Rock.Model.Page"/> page for the site.
        /// </summary>
        /// <value>
        /// The login <see cref="Rock.Model.Page"/> for the site. 
        /// </value>
        [DataMember]
        public virtual Page LoginPage { get; set; }

        /// <summary>
        /// Gets or sets the login <see cref="Rock.Model.PageRoute"/> page route for this site. If this value is null, the LoginPage will be used
        /// </summary>
        /// <value>
        /// The login page route.
        /// </value>
        [DataMember]
        public virtual PageRoute LoginPageRoute { get; set; }

        /// <summary>
        /// Gets or sets the change password page.
        /// </summary>
        /// <value>
        /// The change password page.
        /// </value>
        [LavaInclude]
        public virtual Page ChangePasswordPage { get; set; }

        /// <summary>
        /// Gets or sets the change password page route.
        /// </summary>
        /// <value>
        /// The change password page route.
        /// </value>
        [DataMember]
        public virtual PageRoute ChangePasswordPageRoute { get; set; }

        /// <summary>
        /// Gets or sets the registration <see cref="Rock.Model.Page"/> page for the site.
        /// </summary>
        /// <value>
        /// The registration <see cref="Rock.Model.Page"/> for the site. 
        /// </value>
        [DataMember]
        public virtual Page RegistrationPage { get; set; }

        /// <summary>
        /// Gets or sets the registration <see cref="Rock.Model.PageRoute"/> page route for this site. If this value is null, the RegistrationPage will be used
        /// </summary>
        /// <value>
        /// The registration page route.
        /// </value>
        [DataMember]
        public virtual PageRoute RegistrationPageRoute { get; set; }

        /// <summary>
        /// Gets or sets the 404 <see cref="Rock.Model.Page"/> page for the site.
        /// </summary>
        /// <value>
        /// The 404 <see cref="Rock.Model.Page"/> for the site. 
        /// </value>
        [DataMember]
        public virtual Page PageNotFoundPage { get; set; }

        /// <summary>
        /// Gets or sets the 404 <see cref="Rock.Model.PageRoute"/> page route for this site. 
        /// </summary>
        /// <value>
        /// The registration page route.
        /// </value>
        [DataMember]
        public virtual PageRoute PageNotFoundPageRoute { get; set; }

        /// <summary>
        /// Gets or sets the communication page.
        /// </summary>
        /// <value>
        /// The communication page.
        /// </value>
        [DataMember]
        public virtual Page CommunicationPage { get; set; }

        /// <summary>
        /// Gets or sets the communication page route.
        /// </summary>
        /// <value>
        /// The communication page route.
        /// </value>
        [DataMember]
        public virtual PageRoute CommunicationPageRoute { get; set; }

        /// <summary>
        /// Gets or sets the mobile page.
        /// </summary>
        /// <value>
        /// The mobile page.
        /// </value>
        [DataMember]
        public virtual Page MobilePage { get; set; }

        /// <summary>
        /// Gets or sets the favicon binary file.
        /// </summary>
        /// <value>
        /// The fav icon binary file.
        /// </value>
        [LavaInclude]
        public virtual BinaryFile FavIconBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the site logo binary file.
        /// </summary>
        /// <value>
        /// The site logo binary file.
        /// </value>
        [LavaInclude]
        public virtual BinaryFile SiteLogoBinaryFile { get; set; }

        /// <summary>
        /// Gets the default domain URI.
        /// </summary>
        /// <value>
        /// The default domain URI.
        /// </value>
        [LavaInclude]
        public virtual Uri DefaultDomainUri
        {
            get
            {
                try
                {
                    string protocol = this.RequiresEncryption ? "https://" : "http://";
                    string host = this.SiteDomains.OrderBy( d => d.Order ).Select( d => d.Domain ).FirstOrDefault();
                    if ( host != null )
                    {
                        host = host.ToLower().StartsWith( "http://" ) ? host.Substring( 7 ) : host;
                        host = host.ToLower().StartsWith( "https://" ) ? host.Substring( 8 ) : host;

                        return new Uri( protocol + host );
                    }
                }
                catch { }

                return new Uri( GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) );
            }
        }

        #endregion

        #region Methods

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

        /// <summary>
        /// Bulks the index documents.
        /// </summary>
        public void BulkIndexDocuments()
        {
            // get list of sites that with indexing enabled
            var sites = new SiteService( new RockContext() ).Queryable().Where( s => s.IsIndexEnabled );

            foreach ( var site in sites )
            {
                // delete current items index
                IndexContainer.DeleteDocumentByProperty( typeof( SitePageIndex ), "SiteId", site.Id );

                // clear current documents out
                var pageCount = new Crawler().CrawlSite( site );
            }
        }

        /// <summary>
        /// Deletes the indexed documents.
        /// </summary>
        public void DeleteIndexedDocuments()
        {
            IndexContainer.DeleteDocumentsByType<SitePageIndex>();
        }

        /// <summary>
        /// Indexes the name of the model.
        /// </summary>
        /// <returns></returns>
        public Type IndexModelType()
        {
            return typeof( SitePageIndex );
        }

        /// <summary>
        /// Indexes the document.
        /// </summary>
        /// <param name="id"></param>
        public void IndexDocument( int id )
        {
            return;
        }

        /// <summary>
        /// Deletes the indexed document.
        /// </summary>
        /// <param name="id"></param>
        public void DeleteIndexedDocument( int id )
        {
            return;
        }

        /// <summary>
        /// Gets the index filter values.
        /// </summary>
        /// <returns></returns>
        public ModelFieldFilterConfig GetIndexFilterConfig()
        {
            ModelFieldFilterConfig filterConfig = new ModelFieldFilterConfig();
            filterConfig.FilterValues = new SiteService( new RockContext() ).Queryable().AsNoTracking().Where( s => s.IsIndexEnabled ).Select( s => s.Name ).ToList();
            filterConfig.FilterLabel = "Sites";
            filterConfig.FilterField = "siteName";

            return filterConfig;
        }

        /// <summary>
        /// Gets the index filter field.
        /// </summary>
        /// <returns></returns>
        public bool SupportsIndexFieldFiltering()
        {
            return true;
        }

        /// <summary>
        /// Gets the supported actions.
        /// </summary>
        /// <value>
        /// The supported actions.
        /// </value>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var result = base.SupportedActions;
                result.AddOrReplace( Rock.Security.Authorization.APPROVE, "The roles and/or users that have access to approve. Used as a base for blocks that use the approve action." );
                return result;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [allows interactive bulk indexing].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allows interactive bulk indexing]; otherwise, <c>false</c>.
        /// </value>
        [NotMapped]
        public bool AllowsInteractiveBulkIndexing
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the file URL.
        /// </summary>
        /// <param name="configurationMobilePhoneFileId">The configuration mobile phone file identifier.</param>
        /// <returns>full path of resource from Binary file path</returns>
        private static string GetFileUrl( int? configurationMobilePhoneFileId )
        {
            string virtualPath = string.Empty;
            if ( configurationMobilePhoneFileId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var binaryFile = new BinaryFileService( rockContext ).Get( ( int ) configurationMobilePhoneFileId );
                    if ( binaryFile != null )
                    {
                        if ( binaryFile.Path.Contains( "~" ) )
                        {
                            // Need to build out full path
                            virtualPath = VirtualPathUtility.ToAbsolute( binaryFile.Path );
                            var globalAttributes = GlobalAttributesCache.Get();
                            string publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" ).EnsureTrailingForwardslash();
                            virtualPath = $"{publicAppRoot}{virtualPath}";
                        }
                        else
                        {
                            virtualPath = binaryFile.Path;
                        }
                    }
                }
            }

            return virtualPath;
        }

        #endregion

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return SiteCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            SiteCache.UpdateCachedEntity( this.Id, entityState );

            using ( var rockContext = new RockContext() )
            {
                foreach ( int pageId in new PageService( rockContext ).GetBySiteId( this.Id )
                        .Select( p => p.Id )
                        .ToList() )
                {
                    PageCache.UpdateCachedEntity( pageId, EntityState.Detached );
                }
            }
        }

        #endregion
    }

    #region enums


    /// <summary>
    /// Types Web, Mobile
    /// </summary>
    public enum SiteType
    {
        /// <summary>
        /// </summary>
        Web,

        /// <summary>
        /// </summary>
        Mobile
    }

    /// <summary>
    /// Font Awesome Icon CSS Weight
    /// </summary>
    [RockObsolete( "1.8" )]
    [Obsolete( "Moved to Theme" )]
    public enum IconCssWeight
    {

        /// <summary>
        /// regular
        /// </summary>
        Regular,

        /// <summary>
        /// solid
        /// </summary>
        Solid,

        /// <summary>
        /// light
        /// </summary>
        Light,

        /// <summary>
        /// thin
        /// </summary>
        Thin
    }

    #endregion

    #region Entity Configuration

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
            this.HasOptional( p => p.DefaultPage ).WithMany().HasForeignKey( p => p.DefaultPageId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.DefaultPageRoute ).WithMany().HasForeignKey( p => p.DefaultPageRouteId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.LoginPage ).WithMany().HasForeignKey( p => p.LoginPageId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.LoginPageRoute ).WithMany().HasForeignKey( p => p.LoginPageRouteId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ChangePasswordPage ).WithMany().HasForeignKey( p => p.ChangePasswordPageId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ChangePasswordPageRoute ).WithMany().HasForeignKey( p => p.ChangePasswordPageRouteId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.RegistrationPage ).WithMany().HasForeignKey( p => p.RegistrationPageId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.RegistrationPageRoute ).WithMany().HasForeignKey( p => p.RegistrationPageRouteId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.PageNotFoundPage ).WithMany().HasForeignKey( p => p.PageNotFoundPageId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.PageNotFoundPageRoute ).WithMany().HasForeignKey( p => p.PageNotFoundPageRouteId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.CommunicationPage ).WithMany().HasForeignKey( p => p.CommunicationPageId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.CommunicationPageRoute ).WithMany().HasForeignKey( p => p.CommunicationPageRouteId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.MobilePage ).WithMany().HasForeignKey( p => p.MobilePageId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.FavIconBinaryFile ).WithMany().HasForeignKey( p => p.FavIconBinaryFileId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.SiteLogoBinaryFile ).WithMany().HasForeignKey( p => p.SiteLogoBinaryFileId ).WillCascadeOnDelete( false );

#pragma warning disable 0618
            // Need Associative table for IconExtensions (which are Defined Values)
            this.HasMany( p => p.IconExtensions ).WithMany().Map( p =>
            {
                p.MapLeftKey( "SiteId" );
                p.MapRightKey( "DefinedValueId" );
                p.ToTable( "SiteIconExtensions" );
            } );
#pragma warning restore 0618
        }
    }

    #endregion
}
