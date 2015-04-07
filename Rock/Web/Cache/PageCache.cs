// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Runtime.Caching;
using System.Web;
using System.Xml.Linq;

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
    public class PageCache : CachedModel<Page>
    {
        #region Constructors

        private PageCache()
        {
        }

        private PageCache( Page page )
        {
            CopyFromModel( page );
        }

        #endregion

        #region Properties

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
                    return PageCache.Read( ParentPageId.Value );
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="Site"/> object for the page.
        /// </summary>
        public LayoutCache Layout
        {
            get
            {
                return LayoutCache.Read( LayoutId );
            }
        }

        /// <summary>
        /// Gets a List of child <see cref="PageCache" /> objects.
        /// </summary>
        /// <returns></returns>
        public List<PageCache> GetPages( RockContext rockContext )
        {
            List<PageCache> pages = new List<PageCache>();

            if ( pageIds != null )
            {
                foreach ( int id in pageIds.ToList() )
                {
                    pages.Add( PageCache.Read( id, rockContext ) );
                }
            }
            else
            {
                pageIds = new List<int>();

                PageService pageService = new PageService( rockContext );
                foreach ( Page page in pageService.GetByParentPageId( this.Id, "PageRoutes,PageContexts" ) )
                {
                    page.LoadAttributes( rockContext );
                    pageIds.Add( page.Id );
                    pages.Add( PageCache.Read( page ) );
                }
            }

            return pages;
        }
        private List<int> pageIds = null;

        /// <summary>
        /// Gets a List of all the <see cref="BlockCache"/> objects configured for the page and the page's layout.
        /// </summary>
        public List<BlockCache> Blocks
        {
            get
            {
                if ( blockIds == null )
                {
                    blockIds = new List<int>();

                    using ( var rockContext = new RockContext() )
                    {
                        BlockService blockService = new BlockService( rockContext );

                        // Load Layout Blocks
                        blockService
                            .GetByLayout( this.LayoutId )
                            .Select( b => b.Id )
                            .ToList()
                            .ForEach( b => blockIds.Add( b ) );

                        // Load Page Blocks
                        blockService
                            .GetByPage( this.Id )
                            .Select( b => b.Id )
                            .ToList()
                            .ForEach( b => blockIds.Add( b ) );
                    }
                }

                var blocks = new List<BlockCache>();
                foreach ( int id in blockIds )
                {
                    var block = BlockCache.Read( id );
                    if ( block != null )
                    {
                        blocks.Add( block );
                    }
                }
                return blocks;
            }
        }
        private List<int> blockIds = null;

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
            public int Id;

            /// <summary>
            /// The GUID
            /// </summary>
            public Guid Guid;

            /// <summary>
            /// The route
            /// </summary>
            public string Route;
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
                if ( this.ParentPage != null )
                {
                    return this.ParentPage;
                }
                else
                {
                    return this.Layout.Site;
                }
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
                    bcName = string.Format( "<i class='{0}'></i> ", IconCssClass );
                }
                if ( BreadCrumbDisplayName )
                {
                    bcName += PageTitle;
                }

                return bcName;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is Page )
            {
                var page = (Page)model;
                this.InternalName = page.InternalName;
                this.PageTitle = page.PageTitle;
                this.BrowserTitle = page.BrowserTitle;
                this.ParentPageId = page.ParentPageId;
                this.LayoutId = page.LayoutId;
                this.IsSystem = page.IsSystem;
                this.RequiresEncryption = page.RequiresEncryption;
                this.EnableViewState = page.EnableViewState;
                this.PageDisplayTitle = page.PageDisplayTitle;
                this.PageDisplayBreadCrumb = page.PageDisplayBreadCrumb;
                this.PageDisplayIcon = page.PageDisplayIcon;
                this.PageDisplayDescription = page.PageDisplayDescription;
                this.DisplayInNavWhen = page.DisplayInNavWhen;
                this.MenuDisplayDescription = page.MenuDisplayDescription;
                this.MenuDisplayIcon = page.MenuDisplayIcon;
                this.MenuDisplayChildPages = page.MenuDisplayChildPages;
                this.BreadCrumbDisplayName = page.BreadCrumbDisplayName;
                this.BreadCrumbDisplayIcon = page.BreadCrumbDisplayIcon;
                this.IconCssClass = page.IconCssClass;
                this.Order = page.Order;
                this.OutputCacheDuration = page.OutputCacheDuration;
                this.Description = page.Description;
                this.KeyWords = page.KeyWords;
                this.HeaderContent = page.HeaderContent;
                this.IncludeAdminFooter = page.IncludeAdminFooter;

                this.PageContexts = new Dictionary<string, string>();
                if ( page.PageContexts != null )
                {
                    page.PageContexts.ToList().ForEach( c => this.PageContexts.Add( c.Entity, c.IdParameter ) );
                }

                this.PageRoutes = new List<PageRouteInfo>();
                if ( page.PageRoutes != null )
                {
                    page.PageRoutes.ToList().ForEach( r => this.PageRoutes.Add( new PageRouteInfo { Id = r.Id, Guid = r.Guid, Route = r.Route } ) );
                }

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
            switch ( this.DisplayInNavWhen )
            {
                case Model.DisplayInNavWhen.Always:
                    return true;
                case Model.DisplayInNavWhen.WhenAllowed:
                    return this.IsAuthorized( Authorization.VIEW, person );
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

            if ( ParentPage != null )
            {
                ParentPage.GetPageHierarchy().ForEach( p => pages.Add( p ) );
            }

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
            return this.InternalName;
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
        public XDocument MenuXml( int levelsDeep, Person person, RockContext rockContext,  PageCache currentPage = null, Dictionary<string, string> parameters = null, NameValueCollection queryString = null )
        {
            XElement menuElement = MenuXmlElement( levelsDeep, person, rockContext, currentPage, parameters, queryString );
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
            if ( levelsDeep >= 0 && this.DisplayInNav( person ) )
            {
                string iconUrl = string.Empty;
                if ( this.IconFileId.HasValue )
                {
                    iconUrl = string.Format( "{0}/GetImage.ashx?{1}",
                        HttpContext.Current.Request.ApplicationPath,
                        this.IconFileId.Value );
                }

                bool isCurrentPage = currentPage != null && currentPage.Id == this.Id;

                XElement pageElement = new XElement( "page",
                    new XAttribute( "id", this.Id ),
                    new XAttribute( "title", string.IsNullOrWhiteSpace( this.PageTitle ) ? this.InternalName : this.PageTitle ),
                    new XAttribute( "current", isCurrentPage.ToString() ),
                    new XAttribute( "url", new PageReference( this.Id, 0, parameters, queryString ).BuildUrl() ),
                    new XAttribute( "display-description", this.MenuDisplayDescription.ToString().ToLower() ),
                    new XAttribute( "display-icon", this.MenuDisplayIcon.ToString().ToLower() ),
                    new XAttribute( "display-child-pages", this.MenuDisplayChildPages.ToString().ToLower() ),
                    new XAttribute( "icon-css-class", this.IconCssClass ?? string.Empty ),
                    new XElement( "description", this.Description ?? "" ),
                    new XElement( "icon-url", iconUrl ) );

                XElement childPagesElement = new XElement( "pages" );

                pageElement.Add( childPagesElement );

                if ( levelsDeep > 0 && this.MenuDisplayChildPages )
                    foreach ( PageCache page in GetPages(rockContext) )
                    {
                        if ( page != null )
                        {
                            XElement childPageElement = page.MenuXmlElement( levelsDeep - 1, person, rockContext, currentPage, parameters, queryString );
                            if ( childPageElement != null )
                                childPagesElement.Add( childPageElement );
                        }
                    }

                return pageElement;
            }
            else
                return null;
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
            if ( levelsDeep >= 0 )
            {
                string iconUrl = string.Empty;
                if ( this.IconFileId.HasValue )
                {
                    iconUrl = string.Format( "{0}/GetImage.ashx?{1}",
                        HttpContext.Current.Request.ApplicationPath,
                        this.IconFileId.Value );
                }

                bool isCurrentPage = false;
                bool isParentOfCurrent = false;
                if ( currentPageHeirarchy != null && currentPageHeirarchy.Any() )
                {
                    isCurrentPage = currentPageHeirarchy.First() == this.Id;
                    isParentOfCurrent = currentPageHeirarchy.Skip( 1 ).Any( p => p == this.Id );
                }

                var properties = new Dictionary<string, object>();
                properties.Add( "Id", this.Id );
                properties.Add( "Title", string.IsNullOrWhiteSpace( this.PageTitle ) ? this.InternalName : this.PageTitle );
                properties.Add( "Current", isCurrentPage.ToString().ToLower() );
                properties.Add( "IsParentOfCurrent", isParentOfCurrent.ToString().ToLower() );
                properties.Add( "Url", new PageReference( this.Id, 0, parameters, queryString ).BuildUrl() );
                properties.Add( "DisplayDescription", this.MenuDisplayDescription.ToString().ToLower() );
                properties.Add( "DisplayIcon", this.MenuDisplayIcon.ToString().ToLower() );
                properties.Add( "DisplayChildPages", this.MenuDisplayChildPages.ToString().ToLower() );
                properties.Add( "IconCssClass", this.IconCssClass ?? string.Empty );
                properties.Add( "Description", this.Description ?? "" );
                properties.Add( "IconUrl", iconUrl );

                if ( levelsDeep > 0 && this.MenuDisplayChildPages )
                {
                    var childPages = new List<Dictionary<string, object>>();

                    foreach ( PageCache page in GetPages(rockContext) )
                    {
                        if ( page != null && page.DisplayInNav( person ) )
                        {
                            var childPageElement = page.GetMenuProperties( levelsDeep - 1, person, rockContext, currentPageHeirarchy, parameters, queryString );
                            if ( childPageElement != null )
                            {
                                childPages.Add( childPageElement );
                            }
                        }
                    }

                    if ( childPages.Any() )
                    {
                        properties.Add( "Pages", childPages );
                    }
                }

                return properties;
            }
            else
            {
                return null;
            }
        }

        #endregion


        #endregion

        #region Private Methods

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
            return string.Format( "~/Themes/{0}/Layouts/{1}.aspx", theme, layout );
        }

        /// <summary>
        /// Gets the cache key for the selected page id.
        /// </summary>
        /// <param name="pageId">The page id.</param>
        /// <returns></returns>
        public static string CacheKey( int pageId )
        {
            return string.Format( "Rock:Page:{0}", pageId );
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
            string cacheKey = PageCache.CacheKey( id );

            ObjectCache cache = RockMemoryCache.Default;
            PageCache page = cache[cacheKey] as PageCache;

            if ( page == null )
            {
                if ( rockContext != null )
                {
                    page = LoadById( id, rockContext );
                }
                else
                {
                    using ( var myRockContext = new RockContext() )
                    {
                        page = LoadById( id, myRockContext );
                    }
                }

                if ( page != null )
                {
                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( cacheKey, page, cachePolicy );
                    cache.Set( page.Guid.ToString(), page.Id, cachePolicy );
                }
            }

            return page;
        }

        private static PageCache LoadById( int id, RockContext rockContext = null )
        {
            var pageService = new PageService( rockContext );
            var pageModel = pageService.Queryable( "PageContexts,PageRoutes" ).FirstOrDefault( a => a.Id == id );
            if ( pageModel != null )
            {
                pageModel.LoadAttributes( rockContext );
                return new PageCache( pageModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static PageCache Read( Guid guid, RockContext rockContext = null )
        {
            ObjectCache cache = RockMemoryCache.Default;
            object cacheObj = cache[guid.ToString()];

            PageCache page = null;
            if ( cacheObj != null )
            {
                page = Read( (int)cacheObj, rockContext );
            }

            if ( page == null )
            {
                if ( rockContext != null )
                {
                    page = LoadByGuid( guid, rockContext );
                }
                else
                {
                    using ( var myRockContext = new RockContext() )
                    {
                        page = LoadByGuid( guid, myRockContext );
                    }
                }

                if ( page != null )
                {
                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( PageCache.CacheKey( page.Id ), page, cachePolicy );
                    cache.Set( page.Guid.ToString(), page.Id, cachePolicy );
                }
            }

            return page;
        }

        private static PageCache LoadByGuid( Guid guid, RockContext rockContext )
        {
            var pageService = new PageService( rockContext );
            var pageModel = pageService.Get( guid );
            if ( pageModel != null )
            {
                pageModel.LoadAttributes( rockContext );
                return new PageCache( pageModel );
            }

            return null;
        }

        /// <summary>
        /// Adds Page model to cache, and returns cached object
        /// </summary>
        /// <param name="pageModel"></param>
        /// <returns></returns>
        public static PageCache Read( Page pageModel )
        {
            string cacheKey = PageCache.CacheKey( pageModel.Id );
            ObjectCache cache = RockMemoryCache.Default;
            PageCache page = cache[cacheKey] as PageCache;

            if ( page != null )
            {
                page.CopyFromModel( pageModel );
            }
            else
            {
                page = new PageCache( pageModel );
                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, page, cachePolicy );
                cache.Set( page.Guid.ToString(), page.Id, cachePolicy );
            }

            return page;
        }

        /// <summary>
        /// Removes page from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = RockMemoryCache.Default;
            cache.Remove( PageCache.CacheKey( id ) );
        }

        /// <summary>
        /// Flushes all the pages that use a specific layout.
        /// </summary>
        public static void FlushLayout( int layoutId )
        {
            ObjectCache cache = RockMemoryCache.Default;
            foreach ( var item in cache )
                if ( item.Key.StartsWith( "Rock:Page:" ) )
                {
                    PageCache page = cache[item.Key] as PageCache;
                    if ( page != null && page.LayoutId == layoutId )
                        cache.Remove( item.Key );
                }
        }

        /// <summary>
        /// Flushes the block instances for all the pages that use a specific layout.
        /// </summary>
        public static void FlushLayoutBlocks( int layoutId )
        {
            ObjectCache cache = RockMemoryCache.Default;
            foreach ( var item in cache )
                if ( item.Key.StartsWith( "Rock:Page:" ) )
                {
                    PageCache page = cache[item.Key] as PageCache;
                    if ( page != null && page.LayoutId == layoutId )
                        page.FlushBlocks();
                }
        }

        #endregion

    }
}