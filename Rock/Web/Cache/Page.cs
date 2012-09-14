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

using Rock.Cms;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
	/// Information about a page that is required by the rendering engine.
	/// This information will be cached by the engine
	/// </summary>
    [Serializable]
    public class Page : PageDto, Security.ISecured, Rock.Attribute.IHasAttributes
    {
		#region Constructors

		private Page() : base() { }
		private Page( Rock.Cms.Page page ) : base( page ) { }

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
                return Rock.Web.UI.Page.BuildUrl( new Rock.Web.UI.PageReference( Id, -1 ), null, null );
            }
        }

        /// <summary>
        /// Dictionary of all attributes and their value.
        /// </summary>
        public Dictionary<string, KeyValuePair<string, List<Rock.Core.AttributeValueDto>>> AttributeValues { get; set; }

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

                    Rock.Cms.PageService pageService = new Rock.Cms.PageService();
                    foreach ( Rock.Cms.Page page in pageService.GetByParentPageId( this.Id ) )
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
                    Rock.Cms.BlockInstanceService blockInstanceService = new Rock.Cms.BlockInstanceService();
                    foreach ( Rock.Cms.BlockInstance blockInstance in blockInstanceService.GetByLayout( this.Layout ) )
                    {
                        blockInstanceIds.Add( blockInstance.Id );
                        Rock.Attribute.Helper.LoadAttributes( blockInstance );
                        blockInstances.Add( BlockInstance.Read( blockInstance ) );
                    }

                    // Load Page Blocks
                    foreach ( Rock.Cms.BlockInstance blockInstance in blockInstanceService.GetByPageId( this.Id ) )
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
        internal Dictionary<string, Rock.Data.KeyModel> Context
        {
            get { return _context; }
            set { _context = value; }
        }
        private Dictionary<string, Data.KeyModel> _context;

        #endregion

		#region Public Methods

		/// <summary>
        /// Saves the attribute values for the page
        /// </summary>
        /// <param name="personId">The person id.</param>
        public void SaveAttributeValues(int? personId)
        {
            Rock.Cms.PageService pageService = new Cms.PageService();
            Rock.Cms.Page pageModel = pageService.Get( this.Id );
            if ( pageModel != null )
            {
                Rock.Attribute.Helper.LoadAttributes( pageModel );
                foreach ( var category in pageModel.Attributes )
                    foreach ( var attribute in category.Value )
                        Rock.Attribute.Helper.SaveAttributeValues( pageModel, attribute, this.AttributeValues[attribute.Key].Value, personId );
            }
        }

        /// <summary>
        /// <c>true</c> or <c>false</c> value of whether the page can be displayed in a navigation menu 
        /// based on the <see cref="DisplayInNavWhen"/> property value and the security of the currently logged in user
        /// </summary>
        /// <param name="user">The current user.</param>
        /// <returns></returns>
        public bool DisplayInNav( Rock.Crm.Person person )
        {
            switch ( this.DisplayInNavWhen )
            {
                case Cms.DisplayInNavWhen.Always:
                    return true;
                case Cms.DisplayInNavWhen.WhenAllowed:
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
        public Rock.Data.IModel GetCurrentContext( string entity )
        {
            if ( this.Context.ContainsKey( entity ) )
            {
                var keyModel = this.Context[entity];

                if ( keyModel.Model == null )
                {
                    Type serviceType = typeof( Rock.Data.Service<> );
                    Type[] modelType = { Type.GetType( entity ) };
                    Type service = serviceType.MakeGenericType( modelType );
                    var serviceInstance = Activator.CreateInstance( service );

                    if ( string.IsNullOrWhiteSpace( keyModel.Key ) )
                    {
                        MethodInfo getMethod = service.GetMethod( "Get" );
                        keyModel.Model = getMethod.Invoke( serviceInstance, new object[] { keyModel.Id } ) as Rock.Data.IModel;
                    }
                    else
                    {
                        MethodInfo getMethod = service.GetMethod( "GetByPublicKey" );
                        keyModel.Model = getMethod.Invoke( serviceInstance, new object[] { keyModel.Key } ) as Rock.Data.IModel;
                    }

                    if ( keyModel.Model is Rock.Attribute.IHasAttributes )
                        Rock.Attribute.Helper.LoadAttributes( keyModel.Model as Rock.Attribute.IHasAttributes );
                }

                return keyModel.Model;
            }

            return null;
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
        public static Page Read( Rock.Cms.Page pageModel )
        {
            string cacheKey = Page.CacheKey( pageModel.Id );

            ObjectCache cache = MemoryCache.Default;
            Page page = cache[cacheKey] as Page;

			if ( page != null )
				return page;
			else
			{
				page = Page.CopyModel( pageModel );
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
        public static Page Read( int id )
        {
            string cacheKey = Page.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            Page page = cache[cacheKey] as Page;

            if ( page != null )
                return page;
            else
            {
                Rock.Cms.PageService pageService = new Cms.PageService();
                Rock.Cms.Page pageModel = pageService.Get( id );
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
        private static Page CopyModel( Rock.Cms.Page pageModel )
        {
			// Creates new object by copying properties of model
            var page = new Rock.Web.Cache.Page(pageModel);

            if (pageModel.Attributes != null)
                foreach ( var category in pageModel.Attributes )
                    foreach ( var attribute in category.Value )
                        page.AttributeIds.Add( attribute.Id );

            page.PageContexts = new Dictionary<string,string>();
            if ( pageModel.PageContexts != null )
                foreach ( var pageContext in pageModel.PageContexts )
                    page.PageContexts.Add( pageContext.Entity, pageContext.IdParameter );

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
		/// Flushes the block instances for all the pages that use a specific layout.
		/// </summary>
		public static void FlushLayoutBlockInstances( string layout )
		{
			ObjectCache cache = MemoryCache.Default;
			foreach ( var item in cache )
				if ( item.Key.StartsWith( "Rock:Page:" ) )
				{
					Page page = cache[item.Key] as Page;
					if ( page != null && page.Layout == layout )
						page.FlushBlockInstances();
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
        public virtual bool IsAuthorized( string action, Rock.Crm.Person person )
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
        public XDocument MenuXml( Rock.Crm.Person person )
        {
            return MenuXml( 1, person );
        }

        /// <summary>
        /// Returns XML for a page menu.
        /// </summary>
        /// <param name="levelsDeep">The page levels deep.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public XDocument MenuXml( int levelsDeep, Rock.Crm.Person person )
        {
            XElement menuElement = MenuXmlElement( levelsDeep, person );
            return new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), menuElement );
        }

        private XElement MenuXmlElement( int levelsDeep,  Rock.Crm.Person person )
        {
            if ( levelsDeep >= 0 && this.DisplayInNav( person ) )
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

    }
}