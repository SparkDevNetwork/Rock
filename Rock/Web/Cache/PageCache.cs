//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
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
    public class PageCache : PageDto, Security.ISecured, Rock.Attribute.IHasAttributes
    {
        #region Constructors

        private PageCache() : base() { }
        private PageCache( Rock.Model.Page page ) : base( page ) { }

        #endregion

        #region Properties

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
        /// Dictionary of categorized attributes.  Key is the category name, and Value is list of attributes in the category
        /// </summary>
        /// <value>
        /// The attribute categories.
        /// </value>
        public SortedDictionary<string, List<string>> AttributeCategories
        {
            get
            {
                var attributeCategories = new SortedDictionary<string, List<string>>();

                foreach ( int id in AttributeIds )
                {
                    var attribute = AttributeCache.Read( id );
                    if ( !attributeCategories.ContainsKey( attribute.Key ) )
                        attributeCategories.Add( attribute.Category, new List<string>() );
                    attributeCategories[attribute.Category].Add( attribute.Key );
                }

                return attributeCategories;
            }

            set { }
        }

        /// <summary>
        /// List of attributes associated with the page.  This object will not include values.
        /// To get values associated with the current page instance, use the AttributeValues
        /// </summary>
        public Dictionary<string, Rock.Web.Cache.AttributeCache> Attributes
        {
            get
            {
                var attributes = new Dictionary<string, Rock.Web.Cache.AttributeCache>();

                foreach ( int id in AttributeIds )
                {
                    Rock.Web.Cache.AttributeCache attribute = AttributeCache.Read( id );
                    attributes.Add( attribute.Key, attribute );
                }

                return attributes;
            }

            set
            {
                this.AttributeIds = new List<int>();
                foreach ( var attribute in value )
                    this.AttributeIds.Add( attribute.Value.Id );
            }
        }

        private List<int> AttributeIds = new List<int>();

        /// <summary>
        /// Dictionary of all attributes and their value.
        /// </summary>
        public Dictionary<string, List<Rock.Model.AttributeValueDto>> AttributeValues { get; set; }

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
                    return PageCache.Read( ParentPageId.Value );
                else
                    return null;
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
                    return SiteCache.Read( SiteId.Value );
                else
                    return null;
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
                        pages.Add( PageCache.Read( id ) );
                }
                else
                {
                    pageIds = new List<int>();

                    Rock.Model.PageService pageService = new Rock.Model.PageService();
                    foreach ( Rock.Model.Page page in pageService.GetByParentPageId( this.Id ) )
                    {
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
                            blocks.Add( block );
                    }
                }
                else
                {
                    blockIds = new List<int>();

                    // Load Layout Blocks
                    Rock.Model.BlockService blockService = new Rock.Model.BlockService();
                    foreach ( Rock.Model.Block block in blockService.GetByLayout( this.Layout ) )
                    {
                        blockIds.Add( block.Id );
                        block.LoadAttributes();
                        blocks.Add( BlockCache.Read( block ) );
                    }

                    // Load Page Blocks
                    foreach ( Rock.Model.Block block in blockService.GetByPageId( this.Id ) )
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

        #endregion

        #region Public Methods

        /// <summary>
        /// Saves the attribute values for the page
        /// </summary>
        /// <param name="personId">The person id.</param>
        public void SaveAttributeValues(int? personId)
        {
            Rock.Model.PageService pageService = new Model.PageService();
            Rock.Model.Page pageModel = pageService.Get( this.Id );
            if ( pageModel != null )
            {
                pageModel.LoadAttributes();
                foreach ( var attribute in pageModel.Attributes )
                    Rock.Attribute.Helper.SaveAttributeValues( pageModel, attribute.Value, this.AttributeValues[attribute.Key], personId );
            }
        }

        /// <summary>
        ///   <c>true</c> or <c>false</c> value of whether the page can be displayed in a navigation menu
        /// based on the <see cref="DisplayInNavWhen" /> property value and the security of the currently logged in user
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public bool DisplayInNav( Rock.Model.Person person )
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
                        Rock.Attribute.Helper.LoadAttributes( keyModel.Entity as Rock.Attribute.IHasAttributes );
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
                OnBlockContentUpdated( sender, new EventArgs() );
        }

        #endregion

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
        /// Adds Page model to cache, and returns cached object
        /// </summary>
        /// <param name="pageModel"></param>
        /// <returns></returns>
        public static PageCache Read( Rock.Model.Page pageModel )
        {
            string cacheKey = PageCache.CacheKey( pageModel.Id );

            ObjectCache cache = MemoryCache.Default;
            PageCache page = cache[cacheKey] as PageCache;

            if ( page != null )
                return page;
            else
            {
                page = PageCache.CopyModel( pageModel );
                cache.Set( cacheKey, page, new CacheItemPolicy() );

                return page;
            }
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
                return page;
            else
            {
                Rock.Model.PageService pageService = new Model.PageService();
                Rock.Model.Page pageModel = pageService.Get( id );
                if ( pageModel != null )
                {
                    pageModel.LoadAttributes();

                    page = PageCache.CopyModel( pageModel );
 
                    cache.Set( cacheKey, page, new CacheItemPolicy() );

                    return page;
                }
                else
                    return null;

            }
        }

        // Copies the Model object to the Cached object
        private static PageCache CopyModel( Rock.Model.Page pageModel )
        {
            // Creates new object by copying properties of model
            var page = new Rock.Web.Cache.PageCache(pageModel);

            if (pageModel.Attributes != null)
                foreach ( var attribute in pageModel.Attributes )
                    page.AttributeIds.Add( attribute.Value.Id );

            page.PageContexts = new Dictionary<string,string>();
            if ( pageModel.PageContexts != null )
                foreach ( var pageContext in pageModel.PageContexts )
                    page.PageContexts.Add( pageContext.Entity, pageContext.IdParameter );

            page.TypeId = pageModel.TypeId;
            page.TypeName = pageModel.TypeName;
            page.SupportedActions = pageModel.SupportedActions;

            return page;
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

        #endregion

        #region ISecure Implementation

        /// <summary>
        /// Gets the Entity Type ID for this entity.
        /// </summary>
        /// <value>
        /// The type id.
        /// </value>
        public int TypeId { get; set; }

        /// <summary>
        /// Gets or sets the auth entity.
        /// </summary>
        /// <value>
        /// The auth entity.
        /// </value>
        public string TypeName { get; set; }

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check access to the parent authority specified by this property.
        /// </summary>
        public Security.ISecured ParentAuthority
        {
            get
            {
                if ( this.ParentPage != null )
                    return this.ParentPage;
                else
                    return this.Site;
            }
        }

        /// <summary>
        /// A list of actions that this class supports.
        /// </summary>
        public List<string> SupportedActions { get; set; }

        /// <summary>
        /// Return <c>true</c> if the user is authorized to perform the selected action on this object.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsAuthorized( string action, Rock.Model.Person person )
        {
            return Security.Authorization.Authorized( this, action, person );
        }

        /// <summary>
        /// If a user or role is not specifically allowed or denied to perform the selected action,
        /// return <c>true</c> if they should be allowed anyway or <c>false</c> if not.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public bool IsAllowedByDefault( string action )
        {
            return action == "View";
        }

        /// <summary>
        /// Finds the AuthRule records associated with the current object.
        /// </summary>
        /// <returns></returns>
        public IQueryable<AuthRule> FindAuthRules()
        {
            return Authorization.FindAuthRules( this );
        }

        #endregion

        #region Menu XML Methods

        /// <summary>
        /// Returns XML for a page menu.  XML will be 1 level deep
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public XDocument MenuXml( Rock.Model.Person person )
        {
            return MenuXml( 1, person );
        }

        /// <summary>
        /// Returns XML for a page menu.
        /// </summary>
        /// <param name="levelsDeep">The page levels deep.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public XDocument MenuXml( int levelsDeep, Rock.Model.Person person )
        {
            XElement menuElement = MenuXmlElement( levelsDeep, person );
            return new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), menuElement );
        }

        private XElement MenuXmlElement( int levelsDeep,  Rock.Model.Person person )
        {
            if ( levelsDeep >= 0 && this.DisplayInNav( person ) )
            {
                string iconUrl = this.IconUrl ?? "";
                if ( iconUrl.StartsWith( @"~/" ) )
                {
                    iconUrl = HttpContext.Current.Request.ApplicationPath + iconUrl.Substring( 1 );
                }
                else if ( iconUrl.StartsWith( @"/" ) )
                {
                    iconUrl = HttpContext.Current.Request.ApplicationPath + iconUrl;
                }
                                
                XElement pageElement = new XElement( "page",
                    new XAttribute( "id", this.Id ),
                    new XAttribute( "title", this.Title ?? this.Name ),
                    new XAttribute( "url", this.Url),
                    new XAttribute( "display-description", this.MenuDisplayDescription.ToString().ToLower() ),
                    new XAttribute( "display-icon", this.MenuDisplayIcon.ToString().ToLower() ),
                    new XAttribute( "display-child-pages", this.MenuDisplayChildPages.ToString().ToLower() ),
                    new XElement( "description", this.Description ?? "" ),
                    new XElement( "icon-url", iconUrl ) );

                XElement childPagesElement = new XElement( "pages" );

                pageElement.Add( childPagesElement );

                if ( levelsDeep > 0 && this.MenuDisplayChildPages)
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

        #region Event Handlers

        /// <summary>
        /// Occurs when a block on the page updates content.
        /// </summary>
        public event EventHandler OnBlockContentUpdated;

        #endregion
    }
}