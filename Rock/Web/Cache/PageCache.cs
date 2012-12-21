//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Caching;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;

using Rock.Model;
using Rock.Security;
using Rock.Web.UI;

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
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parent page id.
        /// </summary>
        /// <value>
        /// The parent page id.
        /// </value>
        public int? ParentPageId { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the site id.
        /// </summary>
        /// <value>
        /// The site id.
        /// </value>
        public int? SiteId { get; set; }

        /// <summary>
        /// Gets or sets the layout.
        /// </summary>
        /// <value>
        /// The layout.
        /// </value>
        public string Layout { get; set; }

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
        /// Gets or sets the display in nav when.
        /// </summary>
        /// <value>
        /// The display in nav when.
        /// </value>
        public DisplayInNavWhen DisplayInNavWhen { get; set; }

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
        /// Gets or sets the route id.
        /// </summary>
        /// <value>
        /// The route id.
        /// </value>
        public int RouteId 
        {
            get
            {
                return _routeId;
            }
            set
            {
                _routeId = value;
            }
        }
        private int _routeId = -1;

        /// <summary>
        /// Gets a <see cref="Rock.Web.UI.PageReference"/> for the current page
        /// </summary>
        public Rock.Web.UI.PageReference PageReference 
        {
            get
            {
                return new Rock.Web.UI.PageReference( Id, RouteId );
            }
        }

        /// <summary>
        /// Gets the URL to the current page using the page/{id} route.
        /// </summary>
        public string Url
        {
            get
            {
                return Rock.Web.UI.RockPage.BuildUrl( new Rock.Web.UI.PageReference( Id, -1 ), null, null );
            }
        }

        /// <summary>
        /// Gets or sets the layout path for the page
        /// </summary>
        /// <value>
        /// The layout path.
        /// </value>
        public string LayoutPath { get; set; }

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
        public SiteCache Site
        {
            get
            {
                if ( SiteId != null && SiteId.Value != 0 )
                {
                    return SiteCache.Read( SiteId.Value );
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets a List of child <see cref="PageCache"/> objects.
        /// </summary>
        public List<PageCache> Pages
        {
            get
            {
                List<PageCache> pages = new List<PageCache>();

                if ( pageIds != null )
                {
                    foreach ( int id in pageIds )
                    {
                        pages.Add( PageCache.Read( id ) );
                    }
                }
                else
                {
                    pageIds = new List<int>();

                    PageService pageService = new PageService();
                    foreach ( Page page in pageService.GetByParentPageId( this.Id ) )
                    {
                        page.LoadAttributes();
                        pageIds.Add( page.Id );
                        pages.Add( PageCache.Read( page ) );
                    }
                }

                return pages;
            }
        }
        private List<int> pageIds = null;

        /// <summary>
        /// Gets a List of all the <see cref="BlockCache"/> objects configured for the page and the page's layout.
        /// </summary>
        public List<BlockCache> Blocks
        {
            get
            {
                List<BlockCache> blocks = new List<BlockCache>();

                if ( blockIds != null )
                {
                    foreach ( int id in blockIds )
                    {
                        BlockCache block = BlockCache.Read( id );
                        if ( block != null )
                        {
                            blocks.Add( block );
                        }
                    }
                }
                else
                {
                    blockIds = new List<int>();

                    // Load Layout Blocks
                    BlockService blockService = new BlockService();
                    foreach ( Block block in blockService.GetByLayout( this.Layout ) )
                    {
                        blockIds.Add( block.Id );
                        block.LoadAttributes();
                        blocks.Add( BlockCache.Read( block ) );
                    }

                    // Load Page Blocks
                    foreach ( Block block in blockService.GetByPageId( this.Id ) )
                    {
                        blockIds.Add( block.Id );
                        block.LoadAttributes();
                        blocks.Add( BlockCache.Read( block ) );
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
        /// Gets a dictionary of the current context items (models).
        /// </summary>
        internal Dictionary<string, Rock.Data.KeyEntity> Context
        {
            get { return _context; }
            set { _context = value; }
        }
        private Dictionary<string, Data.KeyEntity> _context;

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
                    return this.Site;
                }
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
                this.Name = page.Name;
                this.ParentPageId = page.ParentPageId;
                this.Title = page.Title;
                this.IsSystem = page.IsSystem;
                this.SiteId = page.SiteId;
                this.Layout = page.Layout;
                this.RequiresEncryption = page.RequiresEncryption;
                this.EnableViewState = page.EnableViewState;
                this.MenuDisplayDescription = page.MenuDisplayDescription;
                this.MenuDisplayIcon = page.MenuDisplayIcon;
                this.MenuDisplayChildPages = page.MenuDisplayChildPages;
                this.DisplayInNavWhen = page.DisplayInNavWhen;
                this.Order = page.Order;
                this.OutputCacheDuration = page.OutputCacheDuration;
                this.Description = page.Description;
                this.IconFileId = page.IconFileId;
                this.IncludeAdminFooter = page.IncludeAdminFooter;

                this.PageContexts = new Dictionary<string, string>();
                if ( page.PageContexts != null )
                {
                    foreach ( var pageContext in page.PageContexts )
                    {
                        this.PageContexts.Add( pageContext.Entity, pageContext.IdParameter );
                    }
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
                    return this.IsAuthorized( "View", person );
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets the current context object for a given entity type.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public Rock.Data.IEntity GetCurrentContext( string entity )
        {
            if ( this.Context.ContainsKey( entity ) )
            {
                var keyModel = this.Context[entity];

                if ( keyModel.Entity == null )
                {
                    Type serviceType = typeof( Rock.Data.Service<> );
                    Type[] modelType = { Type.GetType( entity ) };
                    Type service = serviceType.MakeGenericType( modelType );
                    var serviceInstance = Activator.CreateInstance( service );

                    if ( string.IsNullOrWhiteSpace( keyModel.Key ) )
                    {
                        MethodInfo getMethod = service.GetMethod( "Get", new Type[] { typeof(int) } );
                        keyModel.Entity = getMethod.Invoke( serviceInstance, new object[] { keyModel.Id } ) as Rock.Data.IEntity;
                    }
                    else
                    {
                        MethodInfo getMethod = service.GetMethod( "GetByPublicKey" );
                        keyModel.Entity = getMethod.Invoke( serviceInstance, new object[] { keyModel.Key } ) as Rock.Data.IEntity;
                    }

                    if ( keyModel.Entity is Rock.Attribute.IHasAttributes )
                    {
                        Rock.Attribute.Helper.LoadAttributes( keyModel.Entity as Rock.Attribute.IHasAttributes );
                    }
                }

                return keyModel.Entity;
            }

            return null;
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
        /// Fires the block content updated event.
        /// </summary>
        public void BlockContentUpdated(object sender)
        {
            if ( OnBlockContentUpdated != null )
            {
                OnBlockContentUpdated( sender, new EventArgs() );
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #region SharedItemCaching

        /// <summary>
        /// Used to save an item to the current HTTPRequests items collection.  This is useful if multiple blocks
        /// on the same page will need access to the same object.  The first block can read the object and save
        /// it using this method for the other blocks to reference
        /// </summary>
        /// <param name="key"></param>
        /// <param name="item"></param>
        public void SaveSharedItem( string key, object item )
        {
            string itemKey = string.Format("{0}:Item:{1}", PageCache.CacheKey( Id ), key);
            
            System.Collections.IDictionary items = HttpContext.Current.Items;
            if ( items.Contains( itemKey ) )
                items[itemKey] = item;
            else
                items.Add( itemKey, item );
        }

        /// <summary>
        /// Retrieves an item from the current HTTPRequest items collection.  This is useful to retrieve an object
        /// that was saved by a previous block on the same page.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetSharedItem( string key )
        {
            string itemKey = string.Format( "{0}:Item:{1}", PageCache.CacheKey( Id ), key );

            System.Collections.IDictionary items = HttpContext.Current.Items;
            if ( items.Contains( itemKey ) )
                return items[itemKey];

            return null;
        }

        #endregion

        #region HtmlLinks

        /// <summary>
        /// Adds a new CSS link that will be added to the page header prior to the page being rendered
        /// </summary>
        /// <param name="page">Current System.Web.UI.Page</param>
        /// <param name="href">Path to css file.  Should be relative to layout template.  Will be resolved at runtime</param>
        public void AddCSSLink( System.Web.UI.Page page, string href )
        {
            RockPage.AddCSSLink( page, href );
        }

        /// <summary>
        /// Adds a new CSS link that will be added to the page header prior to the page being rendered
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="href">The href.</param>
        /// <param name="mediaType">MediaType to use in the css link.</param>
        public void AddCSSLink( System.Web.UI.Page page, string href, string mediaType )
        {
            RockPage.AddCSSLink( page, href, mediaType );
        }

        /// <summary>
        /// Adds a meta tag to the page header priore to the page being rendered
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="htmlMeta">The HTML meta tag.</param>
        public void AddMetaTag( System.Web.UI.Page page, HtmlMeta htmlMeta )
        {
            RockPage.AddMetaTag( page, htmlMeta );
        }

        /// <summary>
        /// Adds a new Html link that will be added to the page header prior to the page being rendered
        /// </summary>
        public void AddHtmlLink( System.Web.UI.Page page, HtmlLink htmlLink )
        {
            RockPage.AddHtmlLink( page, htmlLink );
        }

        /// <summary>
        /// Adds a new script tag to the page header prior to the page being rendered
        /// </summary>
        /// <param name="page">Current System.Web.UI.Page</param>
        /// <param name="path">Path to script file.  Should be relative to layout template.  Will be resolved at runtime</param>
        public void AddScriptLink( System.Web.UI.Page page, string path )
        {
            RockPage.AddScriptLink( page, path );
        }

        /// <summary>
        /// Builds a URL from a page and parameters with support for routes
        /// </summary>
        /// <param name="pageId">Page to link to</param>
        /// <param name="parms">Dictionary of parameters</param>
        public string BuildUrl( int pageId, Dictionary<string, string> parms )
        {
            return RockPage.BuildUrl( new Rock.Web.UI.PageReference( pageId, -1 ), parms, null );
        }

        /// <summary>
        /// Builds a URL from a page and parameters with support for routes
        /// </summary>
        /// <param name="pageId">Page to link to</param>
        /// <param name="parms">Dictionary of parameters</param>
        /// <param name="queryString">Querystring to include paramters from</param>
        public string BuildUrl( int pageId, Dictionary<string, string> parms, System.Collections.Specialized.NameValueCollection queryString )
        {
            return RockPage.BuildUrl( new Rock.Web.UI.PageReference( pageId, -1 ), parms, queryString );
        }

        /// <summary>
        /// Builds a URL from a page and parameters with support for routes
        /// </summary>
        /// <param name="pageRef">PageReference to use for the link</param>
        /// <param name="parms">Dictionary of parameters</param>
        public string BuildUrl( Rock.Web.UI.PageReference pageRef, Dictionary<string, string> parms )
        {
            return RockPage.BuildUrl( pageRef, parms, null );
        }

        /// <summary>
        /// Builds a URL from a page and parameters with support for routes
        /// </summary>
        /// <param name="pageRef">PageReference to use for the link</param>
        /// <param name="parms">Dictionary of parameters</param>
        /// <param name="queryString">Querystring to include paramters from</param>
        public string BuildUrl( Rock.Web.UI.PageReference pageRef, Dictionary<string, string> parms, System.Collections.Specialized.NameValueCollection queryString )
        {
            return RockPage.BuildUrl( pageRef, parms, queryString );
        }

        /// <summary>
        /// Builds the URL for detail page.
        /// </summary>
        /// <param name="pageId">The page id.</param>
        /// <param name="queryKey">The query key.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public string BuildUrlForDetailPage( int pageId, string queryKey, int id )
        {
            return RockPage.BuildUrlForDetailPage( pageId, queryKey, id );
        }

        #endregion

        #region Menu XML Methods

        /// <summary>
        /// Returns XML for a page menu.  XML will be 1 level deep
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public XDocument MenuXml( Person person )
        {
            return MenuXml( 1, person );
        }

        /// <summary>
        /// Returns XML for a page menu.
        /// </summary>
        /// <param name="levelsDeep">The page levels deep.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public XDocument MenuXml( int levelsDeep, Person person )
        {
            XElement menuElement = MenuXmlElement( levelsDeep, person );
            return new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), menuElement );
        }

        private XElement MenuXmlElement( int levelsDeep, Person person )
        {
            if ( levelsDeep >= 0 && this.DisplayInNav( person ) )
            {
                string iconUrl = string.Empty;
                if ( this.IconFileId.HasValue )
                {
                    iconUrl = string.Format( "{0}/image.ashx?{1}",
                        HttpContext.Current.Request.ApplicationPath,
                        this.IconFileId.Value );
                }

                XElement pageElement = new XElement( "page",
                    new XAttribute( "id", this.Id ),
                    new XAttribute( "title", this.Title ?? this.Name ),
                    new XAttribute( "url", this.Url ),
                    new XAttribute( "display-description", this.MenuDisplayDescription.ToString().ToLower() ),
                    new XAttribute( "display-icon", this.MenuDisplayIcon.ToString().ToLower() ),
                    new XAttribute( "display-child-pages", this.MenuDisplayChildPages.ToString().ToLower() ),
                    new XElement( "description", this.Description ?? "" ),
                    new XElement( "icon-url", iconUrl ) );

                XElement childPagesElement = new XElement( "pages" );

                pageElement.Add( childPagesElement );

                if ( levelsDeep > 0 && this.MenuDisplayChildPages )
                    foreach ( PageCache page in Pages )
                    {
                        XElement childPageElement = page.MenuXmlElement( levelsDeep - 1, person );
                        if ( childPageElement != null )
                            childPagesElement.Add( childPageElement );
                    }

                return pageElement;
            }
            else
                return null;
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
        /// <param name="id"></param>
        /// <returns></returns>
        public static PageCache Read( int id )
        {
            string cacheKey = PageCache.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            PageCache page = cache[cacheKey] as PageCache;

            if ( page != null )
            {
                return page;
            }
            else
            {
                var pageService = new PageService();
                var pageModel = pageService.Get( id );
                if ( pageModel != null )
                {
                    pageModel.LoadAttributes();
                    page = new PageCache( pageModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( cacheKey, page, cachePolicy );
                    cache.Set( page.Guid.ToString(), page.Id, cachePolicy );
                    
                    return page;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static PageCache Read( Guid guid )
        {
            ObjectCache cache = MemoryCache.Default;
            object cacheObj = cache[guid.ToString()];

            if ( cacheObj != null )
            {
                return Read( (int)cacheObj );
            }
            else
            {
                var pageService = new PageService();
                var pageModel = pageService.Get( guid );
                if ( pageModel != null )
                {
                    pageModel.LoadAttributes();
                    var page = new PageCache( pageModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( PageCache.CacheKey( page.Id ), page, cachePolicy );
                    cache.Set( page.Guid.ToString(), page.Id, cachePolicy );

                    return page;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Adds Page model to cache, and returns cached object
        /// </summary>
        /// <param name="pageModel"></param>
        /// <returns></returns>
        public static PageCache Read( Page pageModel )
        {
            string cacheKey = PageCache.CacheKey( pageModel.Id );

            ObjectCache cache = MemoryCache.Default;
            PageCache page = cache[cacheKey] as PageCache;

            if ( page != null )
            {
                return page;
            }
            else
            {
                page = new PageCache( pageModel );

                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, page, cachePolicy );
                cache.Set( page.Guid.ToString(), page.Id, cachePolicy );

                return page;
            }
        }

        /// <summary>
        /// Removes page from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( PageCache.CacheKey( id ) );
        }

        /// <summary>
        /// Flushes all the pages that use a specific layout.
        /// </summary>
        public static void FlushLayout( string layout )
        {
            ObjectCache cache = MemoryCache.Default;
            foreach ( var item in cache )
                if ( item.Key.StartsWith( "Rock:Page:" ) )
                {
                    PageCache page = cache[item.Key] as PageCache;
                    if ( page != null && page.Layout == layout )
                        cache.Remove( item.Key );
                }
        }

        /// <summary>
        /// Flushes the block instances for all the pages that use a specific layout.
        /// </summary>
        public static void FlushLayoutBlocks( string layout )
        {
            ObjectCache cache = MemoryCache.Default;
            foreach ( var item in cache )
                if ( item.Key.StartsWith( "Rock:Page:" ) )
                {
                    PageCache page = cache[item.Key] as PageCache;
                    if ( page != null && page.Layout == layout )
                        page.FlushBlocks();
                }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Occurs when a block on the page updates content.
        /// </summary>
        public event EventHandler OnBlockContentUpdated;

        #endregion
    }
}