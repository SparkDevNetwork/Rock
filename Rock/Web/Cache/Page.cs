//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using System.Runtime.Caching;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;

using Rock.CMS;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a page that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    public class Page : Security.ISecured, Rock.Attribute.IHasAttributes
    {
        /// <summary>
        /// Use Static Read() method to instantiate a new Page object
        /// </summary>
        private Page() { }

        #region Properties

        /// <summary>
        /// Gets the id.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the layout.
        /// </summary>
        public string Layout { get; private set; }

        /// <summary>
        /// Gets the order.
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// Gets the duration of the output cache.
        /// </summary>
        /// <value>
        /// The duration of the output cache.
        /// </value>
        public int OutputCacheDuration { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the title.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the icon URL.
        /// </summary>
		public string IconUrl { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the page administration footer should be displayed on the page
        /// </summary>
        /// <value>
        ///   <c>true</c> if the footer should be displayed; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeAdminFooter { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to display the page description in the page navigation menu.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if description should be displayed; otherwise, <c>false</c>.
        /// </value>
        public bool MenuDisplayDescription { get; private set; }

        /// <summary>
        /// Gets a value indicating whether page icon should be included in the page navigation menu.
        /// </summary>
        /// <value>
        ///   <c>true</c> if icon should be included; otherwise, <c>false</c>.
        /// </value>
        public bool MenuDisplayIcon { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the pages child pages should be displayed in the page navigation menu.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if child pages should be included; otherwise, <c>false</c>.
        /// </value>
        public bool MenuDisplayChildPages { get; private set; }

        /// <summary>
        /// Gets a <see cref="CMS.DisplayInNavWhen"/> value indicating when or if the page should be included in a page navigation menu
        /// </summary>
        public CMS.DisplayInNavWhen DisplayInNavWhen { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the page requires SSL encryption.
        /// </summary>
        /// <value>
        ///   <c>true</c> if requires encryption; otherwise, <c>false</c>.
        /// </value>
        public bool RequiresEncryption { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the page should use viewstate
        /// </summary>
        /// <value>
        ///   <c>true</c> if viewstate should be enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableViewstate { get; private set; }

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
                return Rock.Web.UI.Page.BuildUrl( new Rock.Web.UI.PageReference( this.Id, -1 ), null, null );
            }
        }

        /// <summary>
        /// Dictionary of all attributes and their value.
        /// </summary>
        public Dictionary<string, KeyValuePair<string, string>> AttributeValues { get; set; }

        /// <summary>
        /// List of attributes associated with the page.  This object will not include values.
        /// To get values associated with the current page instance, use the AttributeValues
        /// </summary>
        public SortedDictionary<string, List<Rock.Web.Cache.Attribute>> Attributes
        {
            get
            {
                SortedDictionary<string, List<Rock.Web.Cache.Attribute>> attributes = new SortedDictionary<string, List<Rock.Web.Cache.Attribute>>();

                foreach ( int id in AttributeIds )
                {
                    Rock.Web.Cache.Attribute attribute = Attribute.Read( id );
                    if ( !attributes.ContainsKey( attribute.Category ) )
                        attributes.Add( attribute.Category, new List<Attribute>() );

                    attributes[attribute.Category].Add( attribute );
                }

                return attributes;
            }

            set
            {
                this.AttributeIds = new List<int>();
                foreach ( var category in value )
                    foreach ( var attribute in category.Value )
                        this.AttributeIds.Add( attribute.Id );
            }
        }
        private List<int> AttributeIds = new List<int>();

        /// <summary>
        /// Gets or sets the layout path for the page
        /// </summary>
        /// <value>
        /// The layout path.
        /// </value>
        public string LayoutPath { get; set; }

        /// <summary>
        /// Gets the parent <see cref="Page"/> object.
        /// </summary>
        public Page ParentPage
        {
            get
            {
                if ( ParentPageId != null && ParentPageId.Value != 0 )
                    return Page.Read( ParentPageId.Value );
                else
                    return null;
            }
        }
        private int? ParentPageId;

        /// <summary>
        /// Gets the <see cref="Site"/> object for the page.
        /// </summary>
        public Site Site
        {
            get
            {
                if ( SiteId != null && SiteId.Value != 0 )
                    return Site.Read( SiteId.Value );
                else
                    return null;
            }
        }
        private int? SiteId;

        /// <summary>
        /// Gets a List of child <see cref="Page"/> objects.
        /// </summary>
        public List<Page> Pages
        {
            get
            {
                List<Page> pages = new List<Page>();

                if ( pageIds != null )
                {
                    foreach ( int id in pageIds )
                        pages.Add( Page.Read( id ) );
                }
                else
                {
                    pageIds = new List<int>();

                    Rock.CMS.PageService pageService = new Rock.CMS.PageService();
                    foreach ( Rock.CMS.Page page in pageService.GetByParentPageId( this.Id ) )
                    {
                        pageIds.Add( page.Id );
                        pages.Add( Page.Read( page ) );
                    }
                }

                return pages;
            }
        }
        private List<int> pageIds = null;

        /// <summary>
        /// Gets a List of all the <see cref="BlockInstance"/> objects configured for the page and the page's layout.
        /// </summary>
        public List<BlockInstance> BlockInstances
        {
            get
            {
                List<BlockInstance> blockInstances = new List<BlockInstance>();

                if ( blockInstanceIds != null )
                {
                    foreach ( int id in blockInstanceIds )
                    {
                        BlockInstance blockInstance = BlockInstance.Read( id );
                        if ( blockInstance != null )
                            blockInstances.Add( blockInstance );
                    }
                }
                else
                {
                    blockInstanceIds = new List<int>();

                    // Load Layout Blocks
                    Rock.CMS.BlockInstanceService blockInstanceService = new Rock.CMS.BlockInstanceService();
                    foreach ( Rock.CMS.BlockInstance blockInstance in blockInstanceService.GetByLayout( this.Layout ) )
                    {
                        blockInstanceIds.Add( blockInstance.Id );
                        Rock.Attribute.Helper.LoadAttributes( blockInstance );
                        blockInstances.Add( BlockInstance.Read( blockInstance ) );
                    }

                    // Load Page Blocks
                    foreach ( Rock.CMS.BlockInstance blockInstance in blockInstanceService.GetByPageId( this.Id ) )
                    {
                        blockInstanceIds.Add( blockInstance.Id );
                        Rock.Attribute.Helper.LoadAttributes( blockInstance );
                        blockInstances.Add( BlockInstance.Read( blockInstance ) );
                    }

                }
                return blockInstances;
            }
        }
        private List<int> blockInstanceIds = null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Saves the attribute values for the page
        /// </summary>
        /// <param name="personId">The person id.</param>
        public void SaveAttributeValues(int? personId)
        {
            Rock.CMS.PageService pageService = new CMS.PageService();
            Rock.CMS.Page pageModel = pageService.Get( this.Id );
            if ( pageModel != null )
            {
                Rock.Attribute.Helper.LoadAttributes( pageModel );
                foreach ( var category in pageModel.Attributes )
                    foreach ( var attribute in category.Value )
                        Rock.Attribute.Helper.SaveAttributeValue( pageModel, attribute, this.AttributeValues[attribute.Key].Value, personId );
            }
        }

        /// <summary>
        /// <c>true</c> or <c>false</c> value of whether the page can be displayed in a navigation menu 
        /// based on the <see cref="DisplayInNavWhen"/> property value and the security of the currently logged in user
        /// </summary>
        /// <param name="user">The current user.</param>
        /// <returns></returns>
        public bool DisplayInNav( User user )
        {
            switch ( this.DisplayInNavWhen )
            {
                case CMS.DisplayInNavWhen.Always:
                    return true;
                case CMS.DisplayInNavWhen.WhenAllowed:
                    return this.Authorized( "View", user );
                default:
                    return false;
            }
        }

        /// <summary>
        /// Flushes the cached block instances.
        /// </summary>
        public void FlushBlockInstances()
        {
            blockInstanceIds = null;
        }

        /// <summary>
        /// Flushes the cached child pages.
        /// </summary>
        public void FlushChildPages()
        {
            pageIds = null;
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
            string itemKey = string.Format("{0}:Item:{1}", Page.CacheKey( Id ), key);
            
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
            string itemKey = string.Format( "{0}:Item:{1}", Page.CacheKey( Id ), key );

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
            Rock.Web.UI.Page.AddCSSLink( page, href );
        }

        /// <summary>
        /// Adds a new CSS link that will be added to the page header prior to the page being rendered
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="href">The href.</param>
        /// <param name="mediaType">MediaType to use in the css link.</param>
        public void AddCSSLink( System.Web.UI.Page page, string href, string mediaType )
        {
            Rock.Web.UI.Page.AddCSSLink( page, href, mediaType );
        }

        /// <summary>
        /// Adds a meta tag to the page header priore to the page being rendered
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="htmlMeta">The HTML meta tag.</param>
        public void AddMetaTag( System.Web.UI.Page page, HtmlMeta htmlMeta )
        {
            Rock.Web.UI.Page.AddMetaTag( page, htmlMeta );
        }

        /// <summary>
        /// Adds a new Html link that will be added to the page header prior to the page being rendered
        /// </summary>
        public void AddHtmlLink( System.Web.UI.Page page, HtmlLink htmlLink )
        {
            Rock.Web.UI.Page.AddHtmlLink( page, htmlLink );
        }

        /// <summary>
        /// Adds a new script tag to the page header prior to the page being rendered
        /// </summary>
        /// <param name="page">Current System.Web.UI.Page</param>
        /// <param name="path">Path to script file.  Should be relative to layout template.  Will be resolved at runtime</param>
        public void AddScriptLink( System.Web.UI.Page page, string path )
        {
            Rock.Web.UI.Page.AddScriptLink( page, path );
        }

        /// <summary>
        /// Builds a URL from a page and parameters with support for routes
        /// </summary>
        /// <param name="pageId">Page to link to</param>
        /// <param name="parms">Dictionary of parameters</param>
        public string BuildUrl( int pageId, Dictionary<string, string> parms )
        {
            return Rock.Web.UI.Page.BuildUrl( new Rock.Web.UI.PageReference( pageId, -1 ), parms, null );
        }

        /// <summary>
        /// Builds a URL from a page and parameters with support for routes
        /// </summary>
        /// <param name="pageId">Page to link to</param>
        /// <param name="parms">Dictionary of parameters</param>
        /// <param name="queryString">Querystring to include paramters from</param>
        public string BuildUrl( int pageId, Dictionary<string, string> parms, System.Collections.Specialized.NameValueCollection queryString )
        {
            return Rock.Web.UI.Page.BuildUrl( new Rock.Web.UI.PageReference( pageId, -1 ), parms, queryString );
        }

        /// <summary>
        /// Builds a URL from a page and parameters with support for routes
        /// </summary>
        /// <param name="pageRef">PageReference to use for the link</param>
        /// <param name="parms">Dictionary of parameters</param>
        public string BuildUrl( Rock.Web.UI.PageReference pageRef, Dictionary<string, string> parms )
        {
            return Rock.Web.UI.Page.BuildUrl( pageRef, parms, null );
        }

        /// <summary>
        /// Builds a URL from a page and parameters with support for routes
        /// </summary>
        /// <param name="pageRef">PageReference to use for the link</param>
        /// <param name="parms">Dictionary of parameters</param>
        /// <param name="queryString">Querystring to include paramters from</param>
        public string BuildUrl( Rock.Web.UI.PageReference pageRef, Dictionary<string, string> parms, System.Collections.Specialized.NameValueCollection queryString )
        {
            return Rock.Web.UI.Page.BuildUrl( pageRef, parms, queryString );
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
        public static Page Read( Rock.CMS.Page pageModel )
        {
            Page page = Page.CopyModel( pageModel );

            string cacheKey = Page.CacheKey( pageModel.Id );
            ObjectCache cache = MemoryCache.Default;
            cache.Set( cacheKey, page, new CacheItemPolicy() );

            return page;
        }

        /// <summary>
        /// Returns Page object from cache.  If page does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Page Read( int id )
        {
            string cacheKey = Page.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            Page page = cache[cacheKey] as Page;

            if ( page != null )
                return page;
            else
            {
                Rock.CMS.PageService pageService = new CMS.PageService();
                Rock.CMS.Page pageModel = pageService.Get( id );
                if ( pageModel != null )
                {
                    Rock.Attribute.Helper.LoadAttributes( pageModel );

                    page = Page.CopyModel( pageModel );
 
                    cache.Set( cacheKey, page, new CacheItemPolicy() );

                    return page;
                }
                else
                    return null;

            }
        }

        // Copies the Model object to the Cached object
        private static Page CopyModel( Rock.CMS.Page pageModel )
        {
            Page page = new Page();
            page.Id = pageModel.Id;
            page.ParentPageId = pageModel.ParentPageId;
            page.SiteId = pageModel.SiteId;
            page.Layout = pageModel.Layout;
            page.Order = pageModel.Order;
            page.OutputCacheDuration = pageModel.OutputCacheDuration;
            page.Name = pageModel.Name;
            page.Description = pageModel.Description;
			page.IconUrl = pageModel.IconUrl;
            page.AttributeValues = pageModel.AttributeValues;
            page.IncludeAdminFooter = pageModel.IncludeAdminFooter;
            page.Title = pageModel.Title;
            page.DisplayInNavWhen = pageModel.DisplayInNavWhen;
            page.MenuDisplayChildPages = pageModel.MenuDisplayChildPages;
            page.MenuDisplayDescription = pageModel.MenuDisplayDescription;
            page.MenuDisplayIcon = pageModel.MenuDisplayIcon;
            page.EnableViewstate = pageModel.EnableViewState;
            page.RequiresEncryption = pageModel.RequiresEncryption;

            if (pageModel.Attributes != null)
                foreach ( var category in pageModel.Attributes )
                    foreach ( var attribute in category.Value )
                        page.AttributeIds.Add( attribute.Id );

            page.AuthEntity = pageModel.AuthEntity;
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
            cache.Remove( Page.CacheKey( id ) );
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
                    Page page = cache[item.Key] as Page;
                    if ( page != null && page.Layout == layout )
                        cache.Remove( item.Key );
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
        /// Gets or sets the auth entity.
        /// </summary>
        /// <value>
        /// The auth entity.
        /// </value>
        public string AuthEntity { get; set; }

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check access to the parent authority specified by this property.
        /// </summary>
        public Security.ISecured ParentAuthority
        {
            get { return this.ParentPage; }
        }

        /// <summary>
        /// A list of actions that this class supports.
        /// </summary>
        public List<string> SupportedActions { get; set; }

        /// <summary>
        /// Return <c>true</c> if the user is authorized to perform the selected action on this object.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public virtual bool Authorized( string action, User user )
        {
            return Security.Authorization.Authorized( this, action, user );
        }

        /// <summary>
        /// If a user or role is not specifically allowed or denied to perform the selected action,
        /// return <c>true</c> if they should be allowed anyway or <c>false</c> if not.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public bool DefaultAuthorization( string action )
        {
            return action == "View";
        }

        #endregion

        #region Menu XML Methods

        /// <summary>
        /// Returns XML for a page menu.  XML will be 1 level deep
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public XDocument MenuXml( User user )
        {
            return MenuXml( 1, user );
        }

        /// <summary>
        /// Returns XML for a page menu.
        /// </summary>
        /// <param name="levelsDeep">The page levels deep.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public XDocument MenuXml( int levelsDeep, User user )
        {
            XElement menuElement = MenuXmlElement( levelsDeep, user );
            return new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), menuElement );
        }

        private XElement MenuXmlElement( int levelsDeep,  User user )
        {
            if ( levelsDeep >= 0 && this.DisplayInNav( user ) )
            {
				XElement pageElement = new XElement( "page",
					new XAttribute( "id", this.Id ),
					new XAttribute( "title", this.Title ?? this.Name ),
                    new XAttribute( "url", this.Url),
                    new XAttribute( "display-description", this.MenuDisplayDescription.ToString().ToLower() ),
					new XAttribute( "display-icon", this.MenuDisplayIcon.ToString().ToLower() ),
					new XAttribute( "display-child-pages", this.MenuDisplayChildPages.ToString().ToLower() ),
					new XElement( "description", this.Description ?? "" ),
					new XElement( "icon-url", this.IconUrl ?? "" ) );

                XElement childPagesElement = new XElement( "pages" );

                pageElement.Add( childPagesElement );

                if ( levelsDeep > 0 && this.MenuDisplayChildPages)
                foreach ( Page page in Pages )
                {
                    XElement childPageElement = page.MenuXmlElement( levelsDeep - 1, user );
                    if ( childPageElement != null )
                        childPagesElement.Add( childPageElement );
                }

                return pageElement;
            }
            else
                return null;
        }

        #endregion

    }
}