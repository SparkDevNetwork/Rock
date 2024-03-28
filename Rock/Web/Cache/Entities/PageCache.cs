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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a page that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class PageCache : ModelCache<PageCache, Page>
    {
        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets the internal name to use when administering this page
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the internal name of the Page.
        /// </value>
        [DataMember]
        public string InternalName { get; private set; }

        /// <summary>
        /// Gets or sets the title of the of the Page to use as the page caption, in menu's, breadcrumb display etc.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the page title of the Page.
        /// </value>
        [DataMember]
        public string PageTitle { get; private set; }

        /// <summary>
        /// Gets or sets the browser title to use for the page
        /// </summary>
        /// <value>
        /// The browser title.
        /// </value>
        [DataMember]
        public string BrowserTitle { get; private set; }

        /// <summary>
        /// Gets or sets the parent page id.
        /// </summary>
        /// <value>
        /// The parent page id.
        /// </value>
        [DataMember]
        public int? ParentPageId { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <summary>
        /// Gets or sets the layout id.
        /// </summary>
        /// <value>
        /// The layout id.
        /// </value>
        [DataMember]
        public int LayoutId { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [requires encryption].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [requires encryption]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool RequiresEncryption { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable view state].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable view state]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableViewState { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [page display title].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [page display title]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PageDisplayTitle { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [page display breadcrumb].
        /// </summary>
        /// <value>
        /// <c>true</c> if [page display breadcrumb]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PageDisplayBreadCrumb { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [page display icon].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [page display icon]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PageDisplayIcon { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [page display description].
        /// </summary>
        /// <value>
        /// <c>true</c> if [page display description]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PageDisplayDescription { get; private set; }

        /// <summary>
        /// Gets or sets the display in nav when.
        /// </summary>
        /// <value>
        /// The display in nav when.
        /// </value>
        [DataMember]
        public DisplayInNavWhen DisplayInNavWhen { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [menu display description].
        /// </summary>
        /// <value>
        /// <c>true</c> if [menu display description]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool MenuDisplayDescription { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [menu display icon].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [menu display icon]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool MenuDisplayIcon { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [menu display child pages].
        /// </summary>
        /// <value>
        /// <c>true</c> if [menu display child pages]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool MenuDisplayChildPages { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [breadcrumb display name].
        /// </summary>
        /// <value>
        /// <c>true</c> if [breadcrumb display name]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool BreadCrumbDisplayName { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [breadcrumb display icon].
        /// </summary>
        /// <value>
        /// <c>true</c> if [breadcrumb display icon]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool BreadCrumbDisplayIcon { get; private set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        public string IconCssClass { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; private set; }

        /// <summary>
        /// Gets or sets the duration (in seconds) of the output cache.
        /// </summary>
        /// <value>
        /// The duration (in seconds) of the output cache.
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
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the key words.
        /// </summary>
        /// <value>
        /// The key words.
        /// </value>
        [DataMember]
        public string KeyWords { get; private set; }

        /// <summary>
        /// Gets or sets html content to add to the page header area of the page when rendered.
        /// </summary>
        /// <value>
        /// The content of the header.
        /// </value>
        [DataMember]
        public string HeaderContent { get; private set; }

        /// <summary>
        /// Gets or sets the icon file id.
        /// </summary>
        /// <value>
        /// The icon file id.
        /// </value>
        [DataMember]
        public int? IconFileId { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include admin footer].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include admin footer]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IncludeAdminFooter { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow indexing].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow indexing]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowIndexing { get; private set; }

        /// <summary>
        /// Gets or sets the body CSS class.
        /// </summary>
        /// <value>
        /// The body CSS class.
        /// </value>
        [DataMember]
        public string BodyCssClass { get; private set; }

        /// <summary>
        /// Gets or sets the icon binary file identifier.
        /// </summary>
        /// <value>
        /// The icon binary file identifier.
        /// </value>
        [DataMember]
        public int? IconBinaryFileId { get; private set; }

        /// <summary>
        /// Gets the additional settings.
        /// </summary>
        /// <value>
        /// The additional settings.
        /// </value>
        [DataMember]
        public string AdditionalSettings { get; private set; }

        /// <summary>
        /// Gets or sets the median page load time in seconds. Typically calculated from a set of
        /// <see cref="Interaction.InteractionTimeToServe"/> values.
        /// </summary>
        /// <value>
        /// The median page load time in seconds.
        /// </value>
        [DataMember]
        public double? MedianPageLoadTimeDurationSeconds { get; private set; }

        private string _cacheControlHeaderSettings;

        /// <summary>
        /// Gets or sets the cache control header settings.
        /// </summary>
        /// <value>
        /// The cache control header settings.
        /// </value>
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

        /// <summary>
        /// Gets the cache control header. This shouldn't be used to set the properties directly but the json version should be used to set the CacheControlHeaderSettings property.
        /// </summary>
        /// <value>
        /// The cache control header.
        /// </value>
        [NotMapped]
        public RockCacheability CacheControlHeader
        {
            get
            {
                if ( _cacheControlHeader == null && CacheControlHeaderSettings.IsNotNullOrWhiteSpace() )
                {
                    _cacheControlHeader = Newtonsoft.Json.JsonConvert.DeserializeObject<RockCacheability>( CacheControlHeaderSettings );
                }

                return _cacheControlHeader;
            }
        }

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
        public bool IsRateLimited
        {
            get
            {
                return RateLimitPeriod != null && RateLimitRequestPerPeriod != null;
            }
        }

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
                    return Get( ParentPageId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the <see cref="Site"/> object for the page.
        /// </summary>
        public LayoutCache Layout => LayoutCache.Get( LayoutId );

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

            if ( _pageIds == null )
            {
                lock ( _obj )
                {
                    if ( _pageIds == null )
                    {
                        _pageIds = new PageService( rockContext )
                            .GetByParentPageId( Id )
                            .Select( p => p.Id )
                            .ToList();
                    }
                }
            }

            if ( _pageIds == null )
            {
                return pages;
            }

            foreach ( var id in _pageIds )
            {
                var page = Get( id, rockContext );
                if ( page != null )
                {
                    pages.Add( page );
                }
            }

            return pages;
        }

        private List<int> _pageIds;

        /// <summary>
        /// Gets a List of all the <see cref="BlockCache"/> objects configured for the page and the page's layout.
        /// </summary>
        public List<BlockCache> Blocks
        {
            get
            {
                var blocks = new List<BlockCache>();

                if ( _blockIds == null )
                {
                    lock ( _obj )
                    {
                        if ( _blockIds == null )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                BlockService blockService = new BlockService( rockContext );

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
                                _blockIds = siteBlockIds.Union( layoutBlockIds ).Union( pageBlockIds ).ToList();
                            }
                        }
                    }
                }

                if ( _blockIds == null )
                {
                    return blocks;
                }

                blocks.AddRange(
                    _blockIds.Distinct()
                        .Select( BlockCache.Get )
                        .Where( block => block != null ) );

                return blocks;
            }
        }

        private List<int> _blockIds;

        /// <summary>
        /// Gets or sets the page contexts that have been defined for the page
        /// </summary>
        /// <value>
        /// The page contexts.
        /// </value>
        [DataMember]
        public Dictionary<string, string> PageContexts { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// Helper class for PageRoute information
        /// </summary>
        [Serializable]
        [DataContract]
        public class PageRouteInfo
        {
            private string _route;

            /// <summary>
            /// The id
            /// </summary>
            [DataMember]
            public int Id { get; internal set; }

            /// <summary>
            /// The GUID
            /// </summary>
            [DataMember]
            public Guid Guid { get; internal set; }

            /// <summary>
            /// Gets the page identifier.
            /// </summary>
            /// <value>The page identifier.</value>
            [DataMember]
            public int PageId { get; internal set; }

            /// <summary>
            /// The route
            /// </summary>
            [DataMember]
            public string Route
            {
                get => _route;
                internal set
                {
                    _route = value;

                    UpdateRouteParameters();
                }
            }

            /// <summary>
            /// If true then the route should work on all sites regardless of the site exclusive setting.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is global; otherwise, <c>false</c>.
            /// </value>
            [DataMember]
            public bool IsGlobal { get; internal set; }

            /// <summary>
            /// Gets a copy of <see cref="Route"/> with parameters replaced with
            /// empty <c>{}</c> instead of the full parameter name. This can be
            /// used for comparison purposes.
            /// </summary>
            /// <value>The route with empty parameter placeholders.</value>
            internal string RouteWithEmptyParameters { get; private set; }

            /// <summary>
            /// Gets the route parameter names. The set is configured to be
            /// case-insensitive.
            /// </summary>
            /// <value>The route parameter names.</value>
            internal HashSet<string> Parameters { get; private set; } = new HashSet<string>();

            /// <summary>
            /// Updates parameters related to the route. This should be called
            /// whenever the <see cref="Route"/> property changes.
            /// </summary>
            private void UpdateRouteParameters()
            {
                var parameterRegex = new Regex( @"(?<={)[A-Za-z0-9\-]+(?=})" );
                var matches = parameterRegex.Matches( Route ?? string.Empty ).OfType<Match>().ToList();

                Parameters = new HashSet<string>( matches.Select( m => m.Value ), StringComparer.OrdinalIgnoreCase );
                RouteWithEmptyParameters = parameterRegex.Replace( Route ?? string.Empty, m => string.Empty );
            }
        }

        /// <summary>
        /// Gets or sets the page routes.
        /// </summary>
        /// <value>
        /// The page routes.
        /// </value>
        [DataMember]
        public List<PageRouteInfo> PageRoutes { get; private set; } = new List<PageRouteInfo>();

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
                var bcName = string.Empty;

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
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var page = entity as Page;
            if ( page == null )
            {
                return;
            }

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
            CacheControlHeaderSettings = page.CacheControlHeaderSettings;
            Description = page.Description;
            KeyWords = page.KeyWords;
            HeaderContent = page.HeaderContent;
            IncludeAdminFooter = page.IncludeAdminFooter;
            AllowIndexing = page.AllowIndexing;
            BodyCssClass = page.BodyCssClass;
            IconBinaryFileId = page.IconBinaryFileId;
#pragma warning disable CS0618
            AdditionalSettings = page.AdditionalSettings;
#pragma warning restore CS0618
            MedianPageLoadTimeDurationSeconds = page.MedianPageLoadTimeDurationSeconds;
            RateLimitPeriod = page.RateLimitPeriod;
            RateLimitRequestPerPeriod = page.RateLimitRequestPerPeriod;

            PageContexts = new Dictionary<string, string>();
            page.PageContexts?.ToList().ForEach( c => PageContexts.Add( c.Entity, c.IdParameter ) );

            PageRoutes = new List<PageRouteInfo>();
            page.PageRoutes?.ToList().ForEach( r => PageRoutes.Add( new PageRouteInfo { Id = r.Id, Guid = r.Guid, PageId = page.Id, Route = r.Route, IsGlobal = r.IsGlobal } ) );
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
        [Obsolete( "This will not work with a distributed cache system such as Redis. Remove the page from the cache so it can safely reload all its properties on Get().", true )]
        [RockObsolete( "1.10" )]
        public void RemoveBlocks()
        {
            _blockIds = null;
        }

        /// <summary>
        /// Flushes the cached child pages.
        /// </summary>
        [Obsolete( "This will not work with a distributed cache system such as Redis. Remove the page from the cache so it can safely reload all its properties on Get().", true )]
        [RockObsolete( "1.10" )]
        public void RemoveChildPages()
        {
            _pageIds = null;
        }

        /// <summary>
        /// Gets all routes that match the given parameters. A route is considered
        /// matching if the parameters contains all the parameter names of the route.
        /// If there are extra parameters the route is still considered a match.
        /// </summary>
        /// <param name="parameters">The parameters that contain the route and query string data.</param>
        /// <returns>A collection of matching page routes.</returns>
        internal List<PageRouteInfo> GetAllMatchingRoutes( Dictionary<string, string> parameters )
        {
            var matchingRoutes = new List<PageRouteInfo>();

            foreach ( var route in PageRoutes )
            {
                // If the route has no parameters, we take it.
                if ( route.Parameters.Count == 0 )
                {
                    matchingRoutes.Add( route );
                    continue;
                }

                // See how many parameters we have that match route parameter names.
                var foundParameterCount = route.Parameters
                    .Where( p => parameters.ContainsKey( p ) )
                    .Count();

                // If we have values for all the route parameters, take it.
                if ( route.Parameters.Count == foundParameterCount )
                {
                    matchingRoutes.Add( route );
                    continue;
                }
            }

            return matchingRoutes;
        }

        /// <summary>
        /// Gets the best matching route to the given set of parameters. If no
        /// matching route is found then <c>null</c> is returned.
        /// </summary>
        /// <param name="parameters">The parameters that contain the route and query string data.</param>
        /// <returns>A single matching route.</returns>
        internal PageRouteInfo GetBestMatchingRoute( Dictionary<string, string> parameters )
        {
            // Takes all the matching routes and orders them in descending order
            // by parameter count. This is our best guess as the best match.
            return GetAllMatchingRoutes( parameters )
                .OrderByDescending( r => r.Parameters.Count )
                .FirstOrDefault();
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
            if ( levelsDeep < 0 || !DisplayInNav( person ) )
            {
                return null;
            }

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

            if ( levelsDeep <= 0 || !MenuDisplayChildPages )
            {
                return pageElement;
            }

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
        /// <param name="currentPageHeirarchy">The current page hierarchy.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="queryString">The query string.</param>
        /// <returns></returns>
        public Dictionary<string, object> GetMenuProperties( int levelsDeep, Person person, RockContext rockContext, List<int> currentPageHeirarchy = null, Dictionary<string, string> parameters = null, NameValueCollection queryString = null )
        {
            if ( levelsDeep < 0 )
            {
                return null;
            }

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
                { "Id", Id },
                { "Title", string.IsNullOrWhiteSpace(PageTitle) ? InternalName : PageTitle },
                { "Current", isCurrentPage },
                { "IsParentOfCurrent", isParentOfCurrent },
                { "Url", new PageReference(Id, 0, parameters, queryString).BuildUrl() },
                { "DisplayDescription", MenuDisplayDescription },
                { "DisplayIcon", MenuDisplayIcon.ToString().ToLower() },
                { "DisplayChildPages", MenuDisplayChildPages },
                { "IconCssClass", IconCssClass ?? string.Empty },
                { "Description", Description ?? string.Empty },
                { "IconUrl", iconUrl }
            };

            if ( levelsDeep <= 0 || !MenuDisplayChildPages )
            {
                return properties;
            }

            var childPages = new List<Dictionary<string, object>>();

            foreach ( var page in GetPages( rockContext ) )
            {
                if ( page == null || !page.DisplayInNav( person ) )
                {
                    continue;
                }

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

        /// <summary>
        /// Gets the URL of the Page along with the BreadCrumbs
        /// This method does not honor the page routes.
        /// </summary>
        /// <returns></returns>
        [RockInternal( "1.16.3" )]
        public string GetHyperLinkedPageBreadCrumbs()
        {
            var pageHyperLinkFormat = "<a href='{0}'>{1}</a>";
            var result = new StringBuilder( string.Format( pageHyperLinkFormat, $"/page/{this.Id}", HttpUtility.HtmlEncode( this.InternalName ) ) );

            var parentPageId = this.ParentPageId;
            while ( parentPageId.HasValue )
            {
                var parentPage = Get( parentPageId.Value );
                result.Insert( 0, " / " );
                result.Insert( 0, string.Format( pageHyperLinkFormat, $"/page/{parentPage.Id}", HttpUtility.HtmlEncode( parentPage.InternalName ) ) );
                parentPageId = parentPage.ParentPageId;
            }

            return result.ToString();
        }

        /// <summary>
        /// Gets the name of the fully qualified page.
        /// </summary>
        /// <returns>A string which would have the bread crumbs of the page with the routes hyper-linked</returns>
        [RockInternal( "1.16.3" )]
        public string GetFullyQualifiedPageName()
        {
            var pageHyperLinkFormat = "<a href='{0}'>{1}</a>";
            var result = new StringBuilder( string.Format( pageHyperLinkFormat, new PageReference( this.Id ).BuildUrl(), HttpUtility.HtmlEncode( this.InternalName ) ) );

            var parentPageId = this.ParentPageId;
            while ( parentPageId.HasValue )
            {
                var parentPage = Get( parentPageId.Value );
                result.Insert( 0, " / " );
                result.Insert( 0, string.Format( pageHyperLinkFormat, new PageReference( parentPage.Id ).BuildUrl(), HttpUtility.HtmlEncode( parentPage.InternalName ) ) );
                parentPageId = parentPage.ParentPageId;
            }

            return string.Format( "<li>{0}</li>", result );
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
            return $"~/Themes/{theme}/Layouts/{layout}.aspx";
        }

        /// <summary>
        /// Flushes all the pages that use a specific layout.
        /// </summary>
        public static void RemoveLayout( int layoutId )
        {
            foreach ( var page in All() )
            {
                if ( page != null && page.LayoutId == layoutId )
                {
                    Remove( page.Id );
                }
            }
        }

        /// <summary>
        /// Flushes the block instances for all the pages that use a specific layout.
        /// </summary>
        [Obsolete( "This will not work with a distributed cache system such as Redis. In order to refresh the list of blocks in the PageCache obj we need to flush the page. Use FlushPagesForLayout( int ) instead.", true )]
        [RockObsolete( "1.10" )]
        public static void RemoveLayoutBlocks( int layoutId )
        {
            foreach ( var page in All() )
            {
                if ( page != null && page.LayoutId == layoutId )
                {
                    page.RemoveBlocks();
                }
            }
        }

        /// <summary>
        /// Flushes the block instances for all the pages that use a specific site.
        /// </summary>
        [Obsolete( "This will not work with a distributed cache system such as Redis. In order to refresh the list of blocks in the PageCache obj we need to flush the page. Use FlushPagesForSite( int ) instead.", true )]
        [RockObsolete( "1.10" )]
        public static void RemoveSiteBlocks( int siteId )
        {
            foreach ( var page in All() )
            {
                if ( page != null && page.SiteId == siteId )
                {
                    page.RemoveBlocks();
                }
            }
        }

        /// <summary>
        /// Flushes the pages for a layout.
        /// </summary>
        /// <param name="layoutId">The layout identifier.</param>
        public static void FlushPagesForLayout( int layoutId )
        {
            foreach ( var page in All() )
            {
                if ( page != null && page.LayoutId == layoutId )
                {
                    PageCache.FlushItem( page.Id );
                }
            }
        }

        /// <summary>
        /// Flushes the pages for a site.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        public static void FlushPagesForSite( int siteId )
        {
            foreach ( var page in All() )
            {
                if ( page != null && page.SiteId == siteId )
                {
                    PageCache.FlushItem( page.Id );
                }
            }
        }

        /// <summary>
        /// Flushes the page from the cache without removing it from AllIds.
        /// Call this to force the cache to reload the page from the database the next time it is requested.
        /// This is needed when a change is made to a PageRoute (which is not an ICacheable)
        /// </summary>
        /// <param name="pageId">The page identifier.</param>
        public static void FlushPage( int pageId )
        {
            PageCache.FlushItem( pageId );
        }

        #endregion
    }
}