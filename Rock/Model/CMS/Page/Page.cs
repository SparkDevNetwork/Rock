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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Rock.Data;
using Rock.Enums.Cms;
using Rock.Lava;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents an individual webpage in Rock. A page is a container on a <see cref="Rock.Model.Site"/> that has a <see cref="Rock.Model.Layout"/> which
    /// consists of one or more content area zones. Each content area zone on the page can contain zero or more <see cref="Rock.Model.Block">Blocks.</see>.
    /// 
    /// Pages are hierarchical, and are used to create the structure of the site.  Each page can have one parent Page and zero or more children pages, and the 
    /// page hierarchy is used to create the SiteMap.
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "Page" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.PAGE )]
    public partial class Page : Model<Page>, IOrdered, ICacheable, IHasAdditionalSettings
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the internal name to use when administering this page
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the internal name of the Page.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string InternalName { get; set; }

        /// <summary>
        /// Gets or sets the title of the of the Page to use as the page caption, in menu's, breadcrumb display etc.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the page title of the Page.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string PageTitle { get; set; }


        /// <summary>
        /// Gets or sets the Bot Guardian Level for the Page.
        /// </summary>
        [DataMember]
        public BotGuardianLevel BotGuardianLevel { get; set; } = BotGuardianLevel.Inherit;

        /// <summary>
        /// Gets or sets the browser title to use for the page.
        /// </summary>
        /// <value>
        /// The browser title.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string BrowserTitle { get; set; }

        /// <summary>
        /// Gets or sets the Id of the parent Page.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> that represents the Id of the parent Page.
        /// </value>
        [DataMember]
        public int? ParentPageId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Page is part of the Rock core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this Page is part of the Rock core system/framework, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Layout"/> that this Page uses.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> that represents the Id of the <see cref="Rock.Model.Layout"/> that this Page uses.
        /// </value>
        [DataMember]
        public int LayoutId { get; set; }

        /// <summary>
        /// Gets or sets a flag that indicates if the Page requires SSL encryption.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this Page requires encryption, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool RequiresEncryption { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if view state should be enabled on the page. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if view state should be enabled on the page, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableViewState
        {
            get { return _enableViewState; }
            set { _enableViewState = value; }
        }

        private bool _enableViewState = true;

        /// <summary>
        /// Gets or sets a value indicating whether the Page Title should be displayed on the page (if the <see cref="Rock.Model.Layout"/> supports it).
        /// </summary>
        /// <value>
        ///   A <see cref="System.Boolean"/> that is <c>true</c> if the title should be displayed on the Page, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool PageDisplayTitle
        {
            get { return _pageDisplayTitle; }
            set { _pageDisplayTitle = value; }
        }

        private bool _pageDisplayTitle = true;

        /// <summary>
        /// Gets or sets a flag indicating whether breadcrumbs are displayed on Page
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if breadcrumbs should be displayed on the page, otherwise, <c>false</c>
        /// </value>
        [DataMember]
        public bool PageDisplayBreadCrumb
        {
            get { return _pageDisplayBreadCrumb; }
            set { _pageDisplayBreadCrumb = value; }
        }

        private bool _pageDisplayBreadCrumb = true;

        /// <summary>
        /// Gets or sets a value indicating whether the Page icon should be displayed on the Page.
        /// </summary>
        /// <value>
        ///   A <see cref="System.Boolean"/> that is <c>true</c> if the icon should be displayed otherwise <c>false</c>
        /// </value>
        [DataMember]
        public bool PageDisplayIcon
        {
            get { return _pageDisplayIcon; }
            set { _pageDisplayIcon = value; }
        }

        private bool _pageDisplayIcon = true;

        /// <summary>
        /// Gets or sets a value indicating whether the Page description should be displayed on the page.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/>value that is <c>true</c> if the description should be displayed; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PageDisplayDescription
        {
            get { return _pageDisplayDescription; }
            set { _pageDisplayDescription = value; }
        }

        private bool _pageDisplayDescription = true;

        /// <summary>
        /// Gets or sets a value indicating when the Page should be displayed in the navigation.
        /// </summary>
        /// <value>
        /// An <see cref="DisplayInNavWhen"/> enum value that determines when to display in a navigation.
        /// 0 = When Security Allows
        /// 1 = Always
        /// 2 = Never   
        /// 
        /// Enum[DisplayInNavWhen].
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public DisplayInNavWhen DisplayInNavWhen { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Page description should be displayed in the menu.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the description should be displayed, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool MenuDisplayDescription { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Page icon should be displayed in the menu.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the Page description should be displayed, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool MenuDisplayIcon { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Page's children Pages should be displayed in the menu.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean" /> value that is <c>true</c> if the Page's child pages should be displayed, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool MenuDisplayChildPages { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the Page Name is displayed in the breadcrumb.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the Page's name should be displayed in breadcrumb, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool BreadCrumbDisplayName
        {
            get { return _breadCrumbDisplayName; }
            set { _breadCrumbDisplayName = value; }
        }

        private bool _breadCrumbDisplayName = true;

        /// <summary>
        /// Gets or sets a value indicating whether icon is displayed in breadcrumb.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if the icon is displayed in the breadcrumb, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool BreadCrumbDisplayIcon { get; set; }

        /// <summary>
        /// Gets or sets a number indicating the order of the page in the menu and in the site map.
        /// This will also affect the page order in the menu. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> indicating the order of the page in the page hierarchy and sitemap.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the length of time (in seconds) in that rendered output is cached. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> represents the length of time (in seconds) that output is cached. 0 = no caching.
        /// </value>
        [Obsolete( "You should use the new cache control header property." )]
        [RockObsolete( "1.12" )]
        [DataMember]
        public int OutputCacheDuration
        {
            get
            {
                if ( CacheControlHeader == null || CacheControlHeader.MaxAge == null )
                {
                    return 0;
                }

                return this.CacheControlHeader.MaxAge.ToSeconds();
            }

            private set
            {
            }
        }

        /// <summary>
        /// Gets or sets a user defined description of the page.  This will be added as a meta tag for the page 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Page description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the key words.
        /// </summary>
        /// <value>
        /// The key words.
        /// </value>
        [DataMember]
        public string KeyWords { get; set; }

        /// <summary>
        /// Gets or sets HTML content to add to the page header area of the page when rendered.
        /// </summary>
        /// <value>
        /// The content of the header.
        /// </value>
        [DataMember]
        public string HeaderContent { get; set; }

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
        /// Gets or sets the icon CSS class name for a font vector based icon.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the CSS Class Name of the icon being used. This value will be empty/null if a file based icon is being used.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the admin footer should be displayed when a Site Administrator is logged in.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if admin footer is displayed; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IncludeAdminFooter
        {
            get { return _includeAdminFooter; }
            set { _includeAdminFooter = value; }
        }

        private bool _includeAdminFooter = true;

        /// <summary>
        /// Gets or sets the body CSS class.
        /// </summary>
        /// <value>
        /// The body CSS class.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string BodyCssClass { get; set; }

        /// <summary>
        /// Gets or sets the icon binary file identifier.
        /// </summary>
        /// <value>
        /// The icon binary file identifier.
        /// </value>
        [DataMember]
        public int? IconBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the additional settings.
        /// </summary>
        /// <value>
        /// The additional settings.
        /// </value>
        [Obsolete( "Use AdditionalSettingsJson instead." )]
        [RockObsolete( "1.16" )]
        [DataMember]
        public string AdditionalSettings { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        /// <summary>
        /// Gets or sets the median page load time in seconds. Typically calculated from a set of
        /// <see cref="Interaction.InteractionTimeToServe"/> values.
        /// </summary>
        /// <value>
        /// The median page load time in seconds.
        /// </value>
        [DataMember]
        public double? MedianPageLoadTimeDurationSeconds { get; set; }

        private string _cacheControlHeaderSettings;

        /// <summary>
        /// Gets or sets the cache control header settings.
        /// </summary>
        /// <value>
        /// The cache control header settings.
        /// </value>
        [MaxLength( 500 )]
        [DataMember]
        public string CacheControlHeaderSettings
        {
            get => _cacheControlHeaderSettings;
            set
            {
                if ( _cacheControlHeaderSettings != value )
                {
                    _cacheControlHeader = null;
                }

                _cacheControlHeaderSettings = value;
            }
        }

        private RockCacheability _cacheControlHeader;

        private int? _originalParentPageId = null;

        /// <summary>
        /// Gets or sets the rate limit request per period.
        /// </summary>
        /// <value>
        /// The rate limit request per period.
        /// </value>
        [DataMember]
        public int? RateLimitRequestPerPeriod { get; set; }

        /// <summary>
        /// Gets or sets the rate limit period.
        /// </summary>
        /// <value>
        /// The rate limit period.
        /// </value>
        [DataMember]
        public int? RateLimitPeriod { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is rate limited.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is rate limited; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        [NotMapped]
        public bool IsRateLimited
        {
            get
            {
                return RateLimitPeriod != null && RateLimitRequestPerPeriod != null;
            }
        }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the Page entity for the parent page.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Page"/> entity for the parent Page
        /// </value>
        [LavaVisible]
        public virtual Page ParentPage { get; set; }

        /// <summary>
        /// Gets or sets the icon <see cref="Rock.Model.BinaryFile"/>.
        /// </summary>
        /// <value>
        /// The icon binary file.
        /// </value>
        [LavaVisible]
        public virtual BinaryFile IconBinaryFile { get; set; }

        /// <summary>
        /// Provides a <see cref="Dictionary{TKey, TValue}"/> of actions that this model supports, and the description of each.
        /// </summary>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var actions = new Dictionary<string, string>();
                actions.Add( Authorization.VIEW, "The roles and/or users that have access to view the page." );
                actions.Add( Authorization.EDIT, "The roles and/or users that have access to edit blocks on this page or any child page, when those block or pages don't specifically define security for the current user (i.e. when this page is used as a 'parent authority')." );
                actions.Add( Authorization.ADMINISTRATE, "The roles and/or users that have access to administrate the page.  This includes setting properties of the page, setting security for the page, managing the zones and blocks on the page, and editing the child pages." );
                return actions;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Layout"/> that the pages uses.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Layout"/> entity that the Page is using
        /// </value>
        [LavaVisible]
        public virtual Layout Layout { get; set; }

        /// <summary>
        /// Gets the site identifier of the Page's Layout.
        /// NOTE: This is needed so that Page Attributes qualified by SiteId work.
        /// </summary>
        /// <value>
        /// The site identifier.
        /// </value>
        public virtual int SiteId
        {
            get
            {
                var layout = LayoutCache.Get( this.LayoutId );
                return layout != null ? layout.SiteId : 0;
            }
        }

        /// <summary>
        /// Gets or sets the collection of <see cref="Rock.Model.Block">Blocks</see> that are used on the page.
        /// </summary>
        /// <value>
        /// Collection of <see cref="Rock.Model.Block"/> entities that are used on the Page.
        /// </value>
        [DataMember]
        public virtual ICollection<Block> Blocks
        {
            get { return _blocks ?? ( _blocks = new Collection<Block>() ); }
            set { _blocks = value; }
        }

        private ICollection<Block> _blocks;

        /// <summary>
        /// Gets or sets the collection of the current page's child pages.
        /// </summary>
        /// <value>
        /// Collection of child pages
        /// </value>
        [DataMember]
        public virtual ICollection<Page> Pages
        {
            get { return _pages ?? ( _pages = new Collection<Page>() ); }
            set { _pages = value; }
        }

        private ICollection<Page> _pages;

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PageRoute">PageRoutes</see> that reference this page.
        /// </summary>
        /// <value>
        /// Collection of <see cref="Rock.Model.PageRoute"/> entities that reference this page.
        /// </value>
        [DataMember]
        public virtual ICollection<PageRoute> PageRoutes
        {
            get { return _pageRoutes ?? ( _pageRoutes = new Collection<PageRoute>() ); }
            set { _pageRoutes = value; }
        }

        private ICollection<PageRoute> _pageRoutes;

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.PageContext" /> entities that are used on this page.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.PageContext"/> entities that are used on the page. 
        /// </value>
        [DataMember]
        public virtual ICollection<PageContext> PageContexts
        {
            get { return _pageContexts ?? ( _pageContexts = new Collection<PageContext>() ); }
            set { _pageContexts = value; }
        }

        private ICollection<PageContext> _pageContexts;

        #endregion Navigation Properties

        #region Methods

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return PageTitle;
        }

        #endregion Methods
    }

    #region Entity Configuration

    /// <summary>
    /// Page Configuration class.
    /// </summary>
    public partial class PageConfiguration : EntityTypeConfiguration<Page>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageConfiguration"/> class.
        /// </summary>
        public PageConfiguration()
        {
            this.HasOptional( p => p.ParentPage ).WithMany( p => p.Pages ).HasForeignKey( p => p.ParentPageId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.Layout ).WithMany( p => p.Pages ).HasForeignKey( p => p.LayoutId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.IconBinaryFile ).WithMany().HasForeignKey( p => p.IconBinaryFileId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
