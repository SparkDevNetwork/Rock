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
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Xml.Linq;

using Rock.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a page that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.CachePage instead" )]
    public class PageCache : CachedModel<Page>
    {
        #region Constructors

        private PageCache( CachePage cachePage )
        {
            CopyFromNewCache( cachePage );
        }

        #endregion

        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets the internal name to use when administering this page
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the internal name of the Page.
        /// </value>
        public string InternalName { get; set; }

        /// <summary>
        /// Gets or sets the title of the of the Page to use as the page caption, in menu's, breadcrumb display etc.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the page title of the Page.
        /// </value>
        public string PageTitle { get; set; }

        /// <summary>
        /// Gets or sets the browser title to use for the page
        /// </summary>
        /// <value>
        /// The browser title.
        /// </value>
        public string BrowserTitle { get; set; }

        /// <summary>
        /// Gets or sets the parent page id.
        /// </summary>
        /// <value>
        /// The parent page id.
        /// </value>
        public int? ParentPageId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the layout id.
        /// </summary>
        /// <value>
        /// The layout id.
        /// </value>
        public int LayoutId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [requires encryption].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [requires encryption]; otherwise, <c>false</c>.
        /// </value>
        public bool RequiresEncryption { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable view state].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable view state]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableViewState { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [page display title].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [page display title]; otherwise, <c>false</c>.
        /// </value>
        public bool PageDisplayTitle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [page display breadcrumb].
        /// </summary>
        /// <value>
        /// <c>true</c> if [page display breadcrumb]; otherwise, <c>false</c>.
        /// </value>
        public bool PageDisplayBreadCrumb { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [page display icon].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [page display icon]; otherwise, <c>false</c>.
        /// </value>
        public bool PageDisplayIcon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [page display description].
        /// </summary>
        /// <value>
        /// <c>true</c> if [page display description]; otherwise, <c>false</c>.
        /// </value>
        public bool PageDisplayDescription { get; set; }

        /// <summary>
        /// Gets or sets the display in nav when.
        /// </summary>
        /// <value>
        /// The display in nav when.
        /// </value>
        public DisplayInNavWhen DisplayInNavWhen { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [menu display description].
        /// </summary>
        /// <value>
        /// <c>true</c> if [menu display description]; otherwise, <c>false</c>.
        /// </value>
        public bool MenuDisplayDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [menu display icon].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [menu display icon]; otherwise, <c>false</c>.
        /// </value>
        public bool MenuDisplayIcon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [menu display child pages].
        /// </summary>
        /// <value>
        /// <c>true</c> if [menu display child pages]; otherwise, <c>false</c>.
        /// </value>
        public bool MenuDisplayChildPages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [breadcrumb display name].
        /// </summary>
        /// <value>
        /// <c>true</c> if [breadcrumb display name]; otherwise, <c>false</c>.
        /// </value>
        public bool BreadCrumbDisplayName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [breadcrumb display icon].
        /// </summary>
        /// <value>
        /// <c>true</c> if [breadcrumb display icon]; otherwise, <c>false</c>.
        /// </value>
        public bool BreadCrumbDisplayIcon { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the duration of the output cache.
        /// </summary>
        /// <value>
        /// The duration of the output cache.
        /// </value>
        public int OutputCacheDuration { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the key words.
        /// </summary>
        /// <value>
        /// The key words.
        /// </value>
        public string KeyWords { get; set; }

        /// <summary>
        /// Gets or sets html content to add to the page header area of the page when rendered.
        /// </summary>
        /// <value>
        /// The content of the header.
        /// </value>
        public string HeaderContent { get; set; }

        /// <summary>
        /// Gets or sets the icon file id.
        /// </summary>
        /// <value>
        /// The icon file id.
        /// </value>
        public int? IconFileId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include admin footer].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include admin footer]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeAdminFooter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow indexing].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow indexing]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowIndexing { get; set; }

        /// <summary>
        /// Gets or sets the body CSS class.
        /// </summary>
        /// <value>
        /// The body CSS class.
        /// </value>
        public string BodyCssClass { get; set; }

        /// <summary>
        /// Gets the parent page.
        /// </summary>
        /// <value>
        /// The parent page.
        /// </value>
        public PageCache ParentPage
        {
            get
            {
                if ( ParentPageId != null && ParentPageId.Value != 0 )
                {
                    return Read( ParentPageId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the <see cref="Site"/> object for the page.
        /// </summary>
        public LayoutCache Layout => LayoutCache.Read( LayoutId );

        /// <summary>
        /// Gets the site identifier of the Page's Layout
        /// NOTE: This is needed so that Page Attributes qualified by SiteId work
        /// </summary>
        /// <value>
        /// The site identifier.
        /// </value>
        public virtual int SiteId => Layout?.SiteId ?? 0;

        /// <summary>
        /// Gets a List of child <see cref="PageCache" /> objects.
        /// </summary>
        /// <returns></returns>
        public List<PageCache> GetPages( RockContext rockContext )
        {
            var pages = new List<PageCache>();

            lock ( _obj )
            {
                if ( pageIds == null )
                {
                    pageIds = new PageService( rockContext )
                        .GetByParentPageId( Id )
                        .Select( p => p.Id )
                        .ToList();
                }
            }

            foreach ( var id in pageIds )
            {
                var page = Read( id, rockContext );
                if ( page != null )
                {
                    pages.Add( page );
                }
            }

            return pages;
        }
        private List<int> pageIds;

        /// <summary>
        /// Gets a List of all the <see cref="BlockCache"/> objects configured for the page and the page's layout.
        /// </summary>
        public List<BlockCache> Blocks
        {
            get
            {
                lock ( _obj )
                {
                    if ( blockIds != null )
                        return blockIds
                            .Distinct()
                            .Select( id => BlockCache.Read( id ) ).Where( block => block != null )
                            .ToList();

                    using ( var rockContext = new RockContext() )
                    {
                        var blockService = new BlockService( rockContext );

                        // Load Site Blocks (blocks that should be shown on all pages of a site)
                        var siteBlockIds = blockService
                            .GetBySite( SiteId )
                            .Select( b => b.Id )
                            .ToList();

                        // Load Layout Blocks
                        var layoutBlockIds = blockService
                            .GetByLayout( LayoutId )
                            .Select( b => b.Id )
                            .ToList();

                        // Load Page Blocks
                        var pageBlockIds = blockService
                            .GetByPage( Id )
                            .Select( b => b.Id )
                            .ToList();

                        // NOTE: starting from the top of zone, starts with all Site Blocks, then Layout Blocks, then any page specific blocks
                        blockIds = siteBlockIds.Union( layoutBlockIds ).Union( pageBlockIds ).ToList();
                    }
                }

                return blockIds.Distinct().Select( id => BlockCache.Read( id ) ).Where( block => block != null ).ToList();
            }
        }
        private List<int> blockIds;

        /// <summary>
        /// Gets or sets the page contexts that have been defined for the page
        /// </summary>
        /// <value>
        /// The page contexts.
        /// </value>
        public Dictionary<string, string> PageContexts { get; set; }

        /// <summary>
        /// Helper class for PageRoute information
        /// </summary>
        public class PageRouteInfo
        {
            /// <summary>
            /// The id
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The GUID
            /// </summary>
            public Guid Guid { get; set; }

            /// <summary>
            /// The route
            /// </summary>
            public string Route { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="PageRouteInfo"/> class.
            /// </summary>
            public PageRouteInfo()
            {

            }

            /// <summary>
            /// Initializes a new instance of the <see cref="PageRouteInfo"/> class.
            /// </summary>
            /// <param name="cachePageRouteInfo">The cache page route information.</param>
            public PageRouteInfo( CachePage.PageRouteInfo cachePageRouteInfo )
            {
                Id = cachePageRouteInfo.Id;
                Guid = cachePageRouteInfo.Guid;
                Route = cachePageRouteInfo.Route;
            }
        }

        /// <summary>
        /// Gets or sets the page routes.
        /// </summary>
        /// <value>
        /// The page routes.
        /// </value>
        public List<PageRouteInfo> PageRoutes { get; set; }

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override ISecured ParentAuthority
        {
            get
            {
                if ( ParentPage != null )
                {
                    return ParentPage;
                }

                return Layout.Site;
            }
        }

        /// <summary>
        /// Gets the bread crumb text.
        /// </summary>
        /// <value>
        /// The bread crumb text.
        /// </value>
        public string BreadCrumbText
        {
            get
            {
                string bcName = string.Empty;

                if ( BreadCrumbDisplayIcon && !string.IsNullOrWhiteSpace( IconCssClass ) )
                {
                    bcName = $"<i class='{IconCssClass}'></i> ";
                }

                if ( BreadCrumbDisplayName )
                {
                    bcName += PageTitle;
                }

                return bcName;
            }
        }

        #endregion

        #region Additional Properties 

        /// <summary>
        /// Gets the site name 
        /// NOTE: This is mainly for backwards compatibility for how HtmlContentDetail did Lava for CurrentPage
        /// </summary>
        /// <value>
        /// The site.
        /// </value>
        public string Site => Layout?.Site?.Name;

        /// <summary>
        /// Gets the site theme.
        /// NOTE: This is mainly for backwards compatibility for how HtmlContentDetail did Lava for CurrentPage
        /// </summary>
        /// <value>
        /// The site theme.
        /// </value>
        public string SiteTheme => Layout?.Site?.Theme;

        /// <summary>
        /// Gets the page icon.
        /// NOTE: This is mainly for backwards compatibility for how HtmlContentDetail did Lava for CurrentPage
        /// </summary>
        /// <value>
        /// The page icon.
        /// </value>
        public string PageIcon => IconCssClass;

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is Page ) ) return;

            var page = (Page)model;
            InternalName = page.InternalName;
            PageTitle = page.PageTitle;
            BrowserTitle = page.BrowserTitle;
            ParentPageId = page.ParentPageId;
            LayoutId = page.LayoutId;
            IsSystem = page.IsSystem;
            RequiresEncryption = page.RequiresEncryption;
            EnableViewState = page.EnableViewState;
            PageDisplayTitle = page.PageDisplayTitle;
            PageDisplayBreadCrumb = page.PageDisplayBreadCrumb;
            PageDisplayIcon = page.PageDisplayIcon;
            PageDisplayDescription = page.PageDisplayDescription;
            DisplayInNavWhen = page.DisplayInNavWhen;
            MenuDisplayDescription = page.MenuDisplayDescription;
            MenuDisplayIcon = page.MenuDisplayIcon;
            MenuDisplayChildPages = page.MenuDisplayChildPages;
            BreadCrumbDisplayName = page.BreadCrumbDisplayName;
            BreadCrumbDisplayIcon = page.BreadCrumbDisplayIcon;
            IconCssClass = page.IconCssClass;
            Order = page.Order;
            OutputCacheDuration = page.OutputCacheDuration;
            Description = page.Description;
            KeyWords = page.KeyWords;
            HeaderContent = page.HeaderContent;
            IncludeAdminFooter = page.IncludeAdminFooter;
            AllowIndexing = page.AllowIndexing;
            BodyCssClass = page.BodyCssClass;

            PageContexts = new Dictionary<string, string>();
            page.PageContexts?.ToList().ForEach( c => PageContexts.Add( c.Entity, c.IdParameter ) );

            PageRoutes = new List<PageRouteInfo>();
            page.PageRoutes?.ToList().ForEach( r => PageRoutes.Add( new PageRouteInfo { Id = r.Id, Guid = r.Guid, Route = r.Route } ) );
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CachePage ) ) return;

            var page = (CachePage)cacheEntity;
            InternalName = page.InternalName;
            PageTitle = page.PageTitle;
            BrowserTitle = page.BrowserTitle;
            ParentPageId = page.ParentPageId;
            LayoutId = page.LayoutId;
            IsSystem = page.IsSystem;
            RequiresEncryption = page.RequiresEncryption;
            EnableViewState = page.EnableViewState;
            PageDisplayTitle = page.PageDisplayTitle;
            PageDisplayBreadCrumb = page.PageDisplayBreadCrumb;
            PageDisplayIcon = page.PageDisplayIcon;
            PageDisplayDescription = page.PageDisplayDescription;
            DisplayInNavWhen = page.DisplayInNavWhen;
            MenuDisplayDescription = page.MenuDisplayDescription;
            MenuDisplayIcon = page.MenuDisplayIcon;
            MenuDisplayChildPages = page.MenuDisplayChildPages;
            BreadCrumbDisplayName = page.BreadCrumbDisplayName;
            BreadCrumbDisplayIcon = page.BreadCrumbDisplayIcon;
            IconCssClass = page.IconCssClass;
            Order = page.Order;
            OutputCacheDuration = page.OutputCacheDuration;
            Description = page.Description;
            KeyWords = page.KeyWords;
            HeaderContent = page.HeaderContent;
            IncludeAdminFooter = page.IncludeAdminFooter;
            AllowIndexing = page.AllowIndexing;
            BodyCssClass = page.BodyCssClass;
            PageContexts = new Dictionary<string, string>( page.PageContexts );

            PageRoutes = new List<PageRouteInfo>();
            foreach ( var pagePageRoute in page.PageRoutes )
            {
                PageRoutes.Add( new PageRouteInfo( pagePageRoute ) );
            }
        }

        /// <summary>
        ///   <c>true</c> or <c>false</c> value of whether the page can be displayed in a navigation menu
        /// based on the <see cref="DisplayInNavWhen" /> property value and the security of the currently logged in user
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public bool DisplayInNav( Person person )
        {
            switch ( DisplayInNavWhen )
            {
                case DisplayInNavWhen.Always:
                    return true;
                case DisplayInNavWhen.WhenAllowed:
                    return IsAuthorized( Authorization.VIEW, person );
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets all the pages in the current hierarchy
        /// </summary>
        /// <returns></returns>
        public List<PageCache> GetPageHierarchy()
        {
            var pages = new List<PageCache> { this };

            ParentPage?.GetPageHierarchy().ForEach( p => pages.Add( p ) );

            return pages;
        }

        /// <summary>
        /// Flushes the cached block instances.
        /// </summary>
        public void FlushBlocks()
        {
            blockIds = null;
        }

        /// <summary>
        /// Flushes the cached child pages.
        /// </summary>
        public void FlushChildPages()
        {
            pageIds = null;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return InternalName;
        }

        #region Menu XML Methods

        /// <summary>
        /// Returns XML for a page menu.  XML will be 1 level deep
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public XDocument MenuXml( Person person, RockContext rockContext )
        {
            return MenuXml( 1, person, rockContext );
        }

        /// <summary>
        /// Returns XML for a page menu.
        /// </summary>
        /// <param name="levelsDeep">The page levels deep.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="currentPage">The current page.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="queryString">The query string.</param>
        /// <returns></returns>
        public XDocument MenuXml( int levelsDeep, Person person, RockContext rockContext, PageCache currentPage = null, Dictionary<string, string> parameters = null, NameValueCollection queryString = null )
        {
            var menuElement = MenuXmlElement( levelsDeep, person, rockContext, currentPage, parameters, queryString );
            return new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), menuElement );
        }

        /// <summary>
        /// Menus the XML element.
        /// </summary>
        /// <param name="levelsDeep">The levels deep.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="currentPage">The current page.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="queryString">The query string.</param>
        /// <returns></returns>
        private XElement MenuXmlElement( int levelsDeep, Person person, RockContext rockContext, PageCache currentPage = null, Dictionary<string, string> parameters = null, NameValueCollection queryString = null )
        {
            if ( levelsDeep < 0 || !DisplayInNav( person ) ) return null;

            var iconUrl = string.Empty;
            if ( IconFileId.HasValue )
            {
                iconUrl = $"{HttpContext.Current.Request.ApplicationPath}/GetImage.ashx?{IconFileId.Value}";
            }

            var isCurrentPage = currentPage != null && currentPage.Id == Id;

            var pageElement = new XElement(
                "page",
                new XAttribute( "id", Id ),
                new XAttribute( "title", string.IsNullOrWhiteSpace( PageTitle ) ? InternalName : PageTitle ),
                new XAttribute( "current", isCurrentPage.ToString() ),
                new XAttribute( "url", new PageReference( Id, 0, parameters, queryString ).BuildUrl() ),
                new XAttribute( "display-description", MenuDisplayDescription.ToString().ToLower() ),
                new XAttribute( "display-icon", MenuDisplayIcon.ToString().ToLower() ),
                new XAttribute( "display-child-pages", MenuDisplayChildPages.ToString().ToLower() ),
                new XAttribute( "icon-css-class", IconCssClass ?? string.Empty ),
                new XElement( "description", Description ?? string.Empty ),
                new XElement( "icon-url", iconUrl ) );

            var childPagesElement = new XElement( "pages" );

            pageElement.Add( childPagesElement );

            if ( levelsDeep <= 0 || !MenuDisplayChildPages ) return pageElement;

            foreach ( var page in GetPages( rockContext ) )
            {
                var childPageElement = page?.MenuXmlElement( levelsDeep - 1, person, rockContext, currentPage, parameters, queryString );
                if ( childPageElement != null )
                {
                    childPagesElement.Add( childPageElement );
                }
            }

            return pageElement;

        }

        #endregion

        #region Menu Property Methods

        /// <summary>
        /// Gets the menu properties.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public Dictionary<string, object> GetMenuProperties( Person person, RockContext rockContext )
        {
            return GetMenuProperties( 1, person, rockContext );
        }

        /// <summary>
        /// Gets the menu properties.
        /// </summary>
        /// <param name="levelsDeep">The levels deep.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="currentPageHeirarchy">The current page heirarchy.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="queryString">The query string.</param>
        /// <returns></returns>
        public Dictionary<string, object> GetMenuProperties( int levelsDeep, Person person, RockContext rockContext, List<int> currentPageHeirarchy = null, Dictionary<string, string> parameters = null, NameValueCollection queryString = null )
        {
            if ( levelsDeep < 0 ) return null;

            var iconUrl = string.Empty;
            if ( IconFileId.HasValue )
            {
                iconUrl = $"{HttpContext.Current.Request.ApplicationPath}/GetImage.ashx?{IconFileId.Value}";
            }

            var isCurrentPage = false;
            var isParentOfCurrent = false;
            if ( currentPageHeirarchy != null && currentPageHeirarchy.Any() )
            {
                isCurrentPage = currentPageHeirarchy.First() == Id;
                isParentOfCurrent = currentPageHeirarchy.Skip( 1 ).Any( p => p == Id );
            }

            var properties = new Dictionary<string, object>
            {
                {"Id", Id},
                {"Title", string.IsNullOrWhiteSpace(PageTitle) ? InternalName : PageTitle},
                {"Current", isCurrentPage.ToString().ToLower()},
                {"IsParentOfCurrent", isParentOfCurrent.ToString().ToLower()},
                {"Url", new PageReference(Id, 0, parameters, queryString).BuildUrl()},
                {"DisplayDescription", MenuDisplayDescription.ToString().ToLower()},
                {"DisplayIcon", MenuDisplayIcon.ToString().ToLower()},
                {"DisplayChildPages", MenuDisplayChildPages.ToString().ToLower()},
                {"IconCssClass", IconCssClass ?? string.Empty},
                {"Description", Description ?? string.Empty},
                {"IconUrl", iconUrl}
            };

            if ( levelsDeep <= 0 || !MenuDisplayChildPages ) return properties;
            var childPages = new List<Dictionary<string, object>>();

            foreach ( var page in GetPages( rockContext ) )
            {
                if ( page == null || !page.DisplayInNav( person ) ) continue;
                var childPageElement = page.GetMenuProperties( levelsDeep - 1, person, rockContext, currentPageHeirarchy, parameters, queryString );
                if ( childPageElement != null )
                {
                    childPages.Add( childPageElement );
                }
            }

            // Add the Pages property so that Lava users can check it via the "empty" check or size.
            properties.Add( "Pages", childPages );

            return properties;

        }

        #endregion

        #endregion

        #region Static Methods

        /// <summary>
        /// Formats the page url based on the selected theme and layout
        /// </summary>
        /// <param name="theme"></param>
        /// <param name="layout"></param>
        /// <returns></returns>
        public static string FormatPath( string theme, string layout )
        {
            return CachePage.FormatPath( theme, layout );
        }

        /// <summary>
        /// Gets the cache key for the selected page id.
        /// </summary>
        /// <param name="pageId">The page id.</param>
        /// <returns></returns>
        public static string CacheKey( int pageId )
        {
            return $"Rock:Page:{pageId}";
        }

        /// <summary>
        /// Returns Page object from cache.  If page does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static PageCache Read( int id, RockContext rockContext = null )
        {
            return new PageCache( CachePage.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static PageCache Read( Guid guid, RockContext rockContext = null )
        {
            return new PageCache( CachePage.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Adds Page model to cache, and returns cached object
        /// </summary>
        /// <param name="pageModel"></param>
        /// <returns></returns>
        public static PageCache Read( Page pageModel )
        {
            return new PageCache( CachePage.Get( pageModel ) );
        }

        /// <summary>
        /// Removes page from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CachePage.Remove( id );
        }

        /// <summary>
        /// Flushes all the pages that use a specific layout.
        /// </summary>
        public static void FlushLayout( int layoutId )
        {
            CachePage.RemoveLayout( layoutId );
        }

        /// <summary>
        /// Flushes the block instances for all the pages that use a specific layout.
        /// </summary>
        public static void FlushLayoutBlocks( int layoutId )
        {
            CachePage.RemoveLayoutBlocks( layoutId );
        }

        /// <summary>
        /// Flushes the block instances for all the pages that use a specific site.
        /// </summary>
        public static void FlushSiteBlocks( int siteId )
        {
            CachePage.RemoveSiteBlocks( siteId );
        }

        #endregion
    }
}