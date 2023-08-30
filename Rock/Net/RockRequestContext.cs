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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Net
{
    /// <summary>
    /// Provides an abstraction from user-code and the incoming request. The user code (such as
    /// a block, page or API callback) does not need to interact directly with any low-level
    /// request objects. This allows for easier testing as well as adding new request types.
    /// </summary>
    public class RockRequestContext
    {
        #region Fields

        /// <summary>
        /// The cache object for the site this request is related to.
        /// </summary>
        private SiteCache _siteCache;

        /// <summary>
        /// The cache object for the page this request is related to.
        /// </summary>
        private PageCache _pageCache;

        /// <summary>
        /// The person preference collections. This is used as a cache so that
        /// we return the same instance for the entire request.
        /// </summary>
        private ConcurrentDictionary<string, PersonPreferenceCollection> _personPreferenceCollections = new ConcurrentDictionary<string, PersonPreferenceCollection>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the response object associated with this request.
        /// </summary>
        /// <value>
        /// The response object associated with this request.
        /// </value>
        public virtual IRockResponseContext Response { get; private set; }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <value>
        /// The current user.
        /// </value>
        public virtual UserLogin CurrentUser { get; protected set; }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <value>
        /// The current person.
        /// </value>
        public virtual Person CurrentPerson => CurrentUser?.Person;

        /// <summary>
        /// Gets the current visitor <see cref="PersonAlias"/> identifier. If
        /// a person is logged in then this value will be <c>null</c>.
        /// </summary>
        /// <value>The current visitor <see cref="PersonAlias"/> identifier.</value>
        internal virtual int? CurrentVisitorId { get; private set; }

        /// <summary>
        /// Gets or sets the root URL path of this request, e.g. <c>https://www.rocksolidchurchdemo.com</c>.
        /// </summary>
        /// <remarks>
        /// May be empty if the request came from a non-web source.
        /// </remarks>
        /// <value>
        /// The root URL path.
        /// </value>
        public virtual string RootUrlPath { get; protected set; }

        /// <summary>
        /// Gets or sets the request URI that initiated this request.
        /// </summary>
        /// <remarks>
        /// May be null if the request came from a non-web source.
        /// </remarks>
        /// <value>The request URI that initiated this request.</value>
        public virtual Uri RequestUri { get; protected set; }

        /// <summary>
        /// Gets the client information related to the client sending the request.
        /// </summary>
        /// <value>
        /// The client information related to the client sending the request.
        /// </value>
        public virtual ClientInformation ClientInformation { get; protected set; }

        /// <summary>
        /// Gets or sets the page parameters.
        /// </summary>
        /// <value>
        /// The page parameters.
        /// </value>
        internal protected virtual IDictionary<string, string> PageParameters { get; private set; }

        /// <summary>
        /// Gets or sets the context entities.
        /// </summary>
        /// <value>
        /// The context entities.
        /// </value>
        internal protected IDictionary<Type, Lazy<IEntity>> ContextEntities { get; set; }

        /// <summary>
        /// Gets the personalization segment identifiers. Will be empty if this
        /// request is not associated with a site or the site does not have
        /// personalization enabled.
        /// </summary>
        /// <value>The personalization segment identifiers.</value>
        internal IEnumerable<int> PersonalizationSegmentIds { get; private set; }

        /// <summary>
        /// Gets the personalization request filter identifiers. Will be empty if this
        /// request is not associated with a site or the site does not have
        /// personalization enabled.
        /// </summary>
        /// <value>The personalization request filter identifiers.</value>
        internal IEnumerable<int> PersonalizationRequestFilterIds { get; private set; }

        /// <summary>
        /// Gets the query string from the request.
        /// </summary>
        /// <value>The query string from the request.</value>
        internal NameValueCollection QueryString { get; private set; }

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        /// <value>
        /// The headers.
        /// </value>
        private IDictionary<string, IEnumerable<string>> Headers { get; set; }

        /// <summary>
        /// Gets or sets the cookies that were found in the request.
        /// </summary>
        /// <value>The cookies that were found in the request.</value>
        private IDictionary<string, string> Cookies { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an empty instance of the <see cref="RockRequestContext"/> class.
        /// </summary>
        internal RockRequestContext()
        {
            PageParameters = new Dictionary<string, string>( StringComparer.InvariantCultureIgnoreCase );
            ContextEntities = new Dictionary<Type, Lazy<IEntity>>();
            Headers = new Dictionary<string, IEnumerable<string>>( StringComparer.InvariantCultureIgnoreCase );
            Cookies = new Dictionary<string, string>();
            QueryString = new NameValueCollection( StringComparer.OrdinalIgnoreCase );
            RootUrlPath = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockRequestContext" /> class.
        /// </summary>
        /// <param name="request">The request from an HttpContext load that we will initialize from.</param>
        /// <param name="response">The object that handles response updates.</param>
        internal RockRequestContext( HttpRequest request, IRockResponseContext response )
        {
            Response = response;

            CurrentUser = UserLoginService.GetCurrentUser( true );

            RequestUri = request.UrlProxySafe();
            RootUrlPath = GetRootUrlPath( RequestUri );

            ClientInformation = new ClientInformation( request );

            // Setup the page parameters.
            QueryString = new NameValueCollection( request.QueryString );
            PageParameters = new Dictionary<string, string>( StringComparer.InvariantCultureIgnoreCase );
            foreach ( var key in request.QueryString.AllKeys.Where( k => !k.IsNullOrWhiteSpace() ) )
            {
                PageParameters.AddOrReplace( key, request.QueryString[key] );
            }
            foreach ( var kvp in request.RequestContext.RouteData.Values )
            {
                PageParameters.AddOrReplace( kvp.Key, kvp.Value.ToStringSafe() );
            }

            // Setup the headers.
            Headers = request.Headers.AllKeys
                .Select( k => new KeyValuePair<string, IEnumerable<string>>( k, request.Headers.GetValues( k ) ) )
                .ToDictionary( kvp => kvp.Key, kvp => kvp.Value, StringComparer.InvariantCultureIgnoreCase );

            // Setup the cookies.
            Cookies = new Dictionary<string, string>();
            foreach ( var cookieName in request.Cookies.AllKeys )
            {
                var cookie = request.Cookies[cookieName];

                Cookies.AddOrReplace( cookie.Name, cookie.Value );
            }

            // Initialize any context entities found.
            ContextEntities = new Dictionary<Type, Lazy<IEntity>>();
            AddContextEntitiesFromHeaders();

            CurrentVisitorId = LoadCurrentVisitorId();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockRequestContext" /> class.
        /// </summary>
        /// <param name="request">The request that we will initialize from.</param>
        /// <param name="response">The object that handles response updates.</param>
        internal RockRequestContext( IRequest request, IRockResponseContext response )
        {
            Response = response;

            CurrentUser = UserLoginService.GetCurrentUser( true );

            RequestUri = request.RequestUri != null ? request.UrlProxySafe() : null;
            RootUrlPath = GetRootUrlPath( RequestUri );

            ClientInformation = new ClientInformation( request );

            // Setup the page parameters.
            QueryString = new NameValueCollection( request.QueryString );
            PageParameters = new Dictionary<string, string>( StringComparer.InvariantCultureIgnoreCase );
            foreach ( var key in request.QueryString.AllKeys.Where( k => !k.IsNullOrWhiteSpace() ) )
            {
                PageParameters.AddOrReplace( key, request.QueryString[key] );
            }

            // Setup the headers.
            Headers = request.Headers.AllKeys
                .Select( k => new KeyValuePair<string, IEnumerable<string>>( k, request.Headers.GetValues( k ) ) )
                .ToDictionary( kvp => kvp.Key, kvp => kvp.Value, StringComparer.InvariantCultureIgnoreCase );

            // Setup the cookies.
            Cookies = new Dictionary<string, string>();
            foreach ( var cookieName in request.Cookies.Keys )
            {
                Cookies.AddOrReplace( cookieName, request.Cookies[cookieName] );
            }

            // Initialize any context entities found.
            ContextEntities = new Dictionary<Type, Lazy<IEntity>>();
            AddContextEntitiesFromHeaders();

            CurrentVisitorId = LoadCurrentVisitorId();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepares the request for use with the specified page. This should
        /// be called for requests that are related to a specific page, for
        /// example block actions or loading of the main page HTML.
        /// </summary>
        /// <param name="page">The page being loaded.</param>
        internal void PrepareRequestForPage( PageCache page )
        {
            _pageCache = page ?? throw new ArgumentNullException( nameof( page ) );
            _siteCache = SiteCache.Get( page.SiteId );

            if ( _siteCache?.EnablePersonalization == true )
            {
                PersonalizationSegmentIds = LoadPersonalizationSegments();
                PersonalizationRequestFilterIds = LoadPersonalizationRequestFilters();
            }

            if ( _siteCache?.EnableVisitorTracking == false )
            {
                CurrentVisitorId = null;
            }

            AddContextEntitiesForPage( _pageCache );
        }

        /// <summary>
        /// Gets the root URL path from the given URI. This is effectively just
        /// the scheme and hostname without any path or query string.
        /// </summary>
        /// <param name="uri">The URL to extract the root from.</param>
        /// <returns>A string that represents the root url path, such as <c>https://rock.rocksolidchurch.com</c>.</returns>
        private static string GetRootUrlPath( Uri uri )
        {
            if ( uri == null )
            {
                return string.Empty;
            }

            var url = $"{uri.Scheme}://{uri.Host}";

            if ( !uri.IsDefaultPort )
            {
                url += $":{uri.Port}";
            }

            return url;
        }

        /// <summary>
        /// Adds the context entities from headers.
        /// </summary>
        protected virtual void AddContextEntitiesFromHeaders()
        {
            foreach ( var kvp in Headers )
            {
                //
                // Skip any header that isn't an entity context header.
                //
                if ( !kvp.Key.StartsWith( "X-ENTITYCONTEXT-", StringComparison.InvariantCultureIgnoreCase ) )
                {
                    continue;
                }

                //
                // Determine the entity type in question.
                //
                var entityName = kvp.Key.Substring( 16 );
                var type = EntityTypeCache.All()
                    .Where( a => a.IsEntity )
                    .FirstOrDefault( a => a.FriendlyName.Equals( entityName, StringComparison.InvariantCultureIgnoreCase ) )
                    ?.GetEntityType();
                string entityKey = kvp.Value.First();

                //
                // If we got an unknown type or no Id/Guid then skip.
                //
                if ( type == null || entityKey.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                //
                // Lazy load the entity so we don't actually load if it is never
                // accessed.
                //
                ContextEntities.AddOrReplace( type, new Lazy<IEntity>( () =>
                {
                    IEntity entity = null;

                    if ( _siteCache?.DisablePredictableIds != true && int.TryParse( entityKey, out int entityId ) )
                    {
                        entity = Reflection.GetIEntityForEntityType( type, entityId );
                    }
                    else if ( Guid.TryParse( entityKey, out Guid entityGuid ) )
                    {
                        entity = Reflection.GetIEntityForEntityType( type, entityGuid );
                    }

                    if ( entity != null && entity is IHasAttributes attributedEntity )
                    {
                        Helper.LoadAttributes( attributedEntity );
                    }

                    return entity;
                } ) );
            }
        }

        /// <summary>
        /// Adds the context entities for page.
        /// </summary>
        /// <param name="pageCache">The page cache.</param>
        private void AddContextEntitiesForPage( PageCache pageCache )
        {
            foreach ( var pageContext in pageCache.PageContexts )
            {
                var entityType = EntityTypeCache.Get( pageContext.Key )?.GetEntityType();
                if ( entityType == null )
                {
                    continue;
                }

                var contextId = GetPageParameter( pageContext.Value );

                if ( contextId.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                if ( int.TryParse( contextId, out var id ) )
                {
                    // Load from plain integer Id, but only if not disabled by site.
                    ContextEntities.AddOrReplace( entityType, new Lazy<IEntity>( () =>
                    {
                        if ( _siteCache?.DisablePredictableIds == true )
                        {
                            return null;
                        }

                        var entity = Reflection.GetIEntityForEntityType( entityType, id );

                        if ( entity != null && entity is IHasAttributes attributedEntity )
                        {
                            Helper.LoadAttributes( attributedEntity );
                        }

                        return entity;
                    } ) );
                }
                else if ( Guid.TryParse( contextId, out var guid ) )
                {
                    // Load from Guid.
                    ContextEntities.AddOrReplace( entityType, new Lazy<IEntity>( () =>
                    {
                        var entity = Reflection.GetIEntityForEntityType( entityType, guid );

                        if ( entity != null && entity is IHasAttributes attributedEntity )
                        {
                            Helper.LoadAttributes( attributedEntity );
                        }

                        return entity;
                    } ) );
                }
                else
                {
                    // Load from IdKey.
                    ContextEntities.AddOrReplace( entityType, new Lazy<IEntity>( () =>
                    {
                        var entity = Reflection.GetIEntityForEntityType( entityType, contextId );

                        if ( entity != null && entity is IHasAttributes attributedEntity )
                        {
                            Helper.LoadAttributes( attributedEntity );
                        }

                        return entity;
                    } ) );
                }
            }
        }

        /// <summary>
        /// Sets the page parameters. This is used by things like block actions
        /// so they can update the request with the original page parameters
        /// rather than what is currently on the query string.
        /// </summary>
        /// <param name="parameters">The parameters to use for the page.</param>
        internal virtual void SetPageParameters( IDictionary<string, string> parameters )
        {
            PageParameters = new Dictionary<string, string>( parameters, StringComparer.InvariantCultureIgnoreCase );
        }

        /// <summary>
        /// Gets the page parameter value given it's name.
        /// </summary>
        /// <param name="name">The name of the page parameter to retrieve.</param>
        /// <returns>The text string representation of the page parameter or an empty string if no matching parameter was found.</returns>
        public virtual string GetPageParameter( string name )
        {
            if ( PageParameters.ContainsKey( name ) )
            {
                return PageParameters[name];
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the page parameters.
        /// </summary>
        /// <returns></returns>
        public virtual IDictionary<string, string> GetPageParameters()
        {
            return new Dictionary<string, string>( PageParameters );
        }

        /// <summary>
        /// Gets the entity object given it's type.
        /// </summary>
        /// <typeparam name="T">The IEntity type to retrieve.</typeparam>
        /// <returns>A reference to the IEntity object or null if none was found.</returns>
        public virtual T GetContextEntity<T>() where T : IEntity
        {
            return ( T ) GetContextEntity( typeof( T ) );
        }

        /// <summary>
        /// Gets the entity object given it's type.
        /// </summary>
        /// <returns>A reference to the IEntity object or null if none was found.</returns>
        public virtual IEntity GetContextEntity( Type entityType )
        {
            if ( ContextEntities.ContainsKey( entityType ) )
            {
                return ContextEntities[entityType].Value;
            }

            return default;
        }

        /// <summary>
        /// Gets the common merge fields to be used with a Lava merge process.
        /// </summary>
        /// <param name="currentPersonOverride">The current person override.</param>
        /// <param name="options">The options to use when initializing the merge fields.</param>
        /// <returns>A new dictionary of merge fields.</returns>
        public virtual Dictionary<string, object> GetCommonMergeFields( Person currentPersonOverride = null, CommonMergeFieldsOptions options = null )
        {
            var mergeFields = new Dictionary<string, object>();

            options = options ?? new CommonMergeFieldsOptions();

            if ( options.GetPageContext )
            {
                var contextObjects = new LazyDictionary<string, object>();

                foreach ( var ctx in ContextEntities )
                {
                    contextObjects.Add( ctx.Key.Name, () => ctx.Value.Value );
                }

                if ( contextObjects.Any() )
                {
                    mergeFields.Add( "Context", contextObjects );
                }
            }

            if ( options.GetPageParameters )
            {
                mergeFields.Add( "PageParameter", PageParameters );
            }

            if ( options.GetOSFamily )
            {
                mergeFields.Add( "OSFamily", ClientInformation.Browser.OS.Family.ToLower() );
            }

            if ( options.GetDeviceFamily )
            {
                mergeFields.Add( "DeviceFamily", ClientInformation.Browser.Device.Family );
            }

            var person = currentPersonOverride ?? CurrentPerson;
            if ( options.GetCurrentPerson && person != null )
            {
                mergeFields.Add( "CurrentPerson", person );
            }

            if ( options.GetCampuses )
            {
                mergeFields.Add( "Campuses", CampusCache.All() );
            }

            if ( Headers.ContainsKey( "X-Rock-DeviceData" ) )
            {
                mergeFields.Add( "Device", Headers["X-Rock-DeviceData"].FirstOrDefault().FromJsonOrNull<Common.Mobile.DeviceData>() );
            }

            return mergeFields;
        }

        /// <summary>
        /// Gets the values associated with the specified header.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of all string values associated with the requested header.</returns>
        public virtual IEnumerable<string> GetHeader( string header )
        {
            if ( !Headers.ContainsKey( header ) )
            {
                return new string[0];
            }

            return Headers[header];
        }

        /// <summary>
        /// Gets the value fot the specified cookie name.
        /// </summary>
        /// <param name="cookieName">Name of the cookie.</param>
        /// <returns>A <see cref="string"/> that represents the cookie value or <c>null</c> if cookie was not found.</returns>
        internal virtual string GetCookieValue( string cookieName )
        {
            if ( Cookies.TryGetValue( cookieName, out var value ) )
            {
                return value;
            }

            return null;
        }

        /// <summary>
        /// Loads the matching personalization segment ids for
        /// the <see cref="CurrentPerson"/> or <see cref="CurrentVisitorId"/>.
        /// </summary>
        private IEnumerable<int> LoadPersonalizationSegments()
        {
            var cookieValueJson = GetCookieValue( Rock.Personalization.RequestCookieKey.ROCK_SEGMENT_FILTERS );
            var personalizationPersonAliasId = CurrentVisitorId ?? CurrentPerson?.PrimaryAliasId;

            if ( !personalizationPersonAliasId.HasValue )
            {
                // no visitor or person logged in
                return Array.Empty<int>();
            }

            Personalization.SegmentFilterCookieData segmentFilterCookieData = null;
            if ( cookieValueJson != null )
            {
                segmentFilterCookieData = cookieValueJson.FromJsonOrNull<Personalization.SegmentFilterCookieData>();
                bool isCookieDataValid = false;
                if ( segmentFilterCookieData != null )
                {
                    if ( segmentFilterCookieData.IsSamePersonAlias( personalizationPersonAliasId.Value ) && segmentFilterCookieData.SegmentIdKeys != null )
                    {
                        isCookieDataValid = true;
                    }

                    if ( segmentFilterCookieData.IsStale( RockDateTime.Now ) )
                    {
                        isCookieDataValid = false;
                    }
                }

                if ( !isCookieDataValid )
                {
                    segmentFilterCookieData = null;
                }
            }

            if ( segmentFilterCookieData == null )
            {
                segmentFilterCookieData = new Personalization.SegmentFilterCookieData();
                segmentFilterCookieData.PersonAliasIdKey = IdHasher.Instance.GetHash( personalizationPersonAliasId.Value );
                segmentFilterCookieData.LastUpdateDateTime = RockDateTime.Now;
                var segmentIdKeys = new PersonalizationSegmentService( new RockContext() ).GetPersonalizationSegmentIdKeysForPersonAliasId( personalizationPersonAliasId.Value );
                segmentFilterCookieData.SegmentIdKeys = segmentIdKeys;
            }

            //AddOrUpdateCookie( new HttpCookie( Rock.Personalization.RequestCookieKey.ROCK_SEGMENT_FILTERS, segmentFilterCookieData.ToJson() ) );

            return segmentFilterCookieData.GetSegmentIds();
        }

        /// <summary>
        /// Loads the matching personalization request filter ids for
        /// the current request.
        /// </summary>
        private IEnumerable<int> LoadPersonalizationRequestFilters()
        {
            var requestFilters = RequestFilterCache.All().Where( a => a.IsActive );
            var requestFilterIds = new List<int>();

            foreach ( var requestFilter in requestFilters )
            {
                if ( requestFilter.RequestMeetsCriteria( this, _siteCache ) )
                {
                    requestFilterIds.Add( requestFilter.Id );
                }
            }

            return requestFilterIds;
        }

        /// <summary>
        /// Loads the current visitor <see cref="PersonAlias"/> identifier.
        /// </summary>
        /// <remarks>
        /// This method does not do all the same logic that happens in RockPage
        /// for updating cookies, merging person records, etc. That needs to be
        /// handled some other way. Right now it is handled by RockPage slightly
        /// after this method is called.
        /// </remarks>
        private int? LoadCurrentVisitorId()
        {
            if ( CurrentPerson != null )
            {
                return null;
            }

            var visitorKeyCookie = GetCookieValue( Rock.Personalization.RequestCookieKey.ROCK_VISITOR_KEY );

            // If we have a visitor key, try to get the person alias Id from the IdKey.
            if ( visitorKeyCookie.IsNotNullOrWhiteSpace() )
            {
                return IdHasher.Instance.GetId( visitorKeyCookie );
            }

            return null;
        }

        /// <summary>
        /// Resolves the rock URL.
        /// </summary>
        /// <remarks>
        ///     <para>An input starting with "~~/" will return a theme URL like, "/Themes/{CurrentSiteTheme}/{input}".</para>
        ///     <para>An input starting with "~/" will return the input without the leading "~".</para>
        ///     <para>The input will be returned as supplied for all other cases.</para>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        /// <param name="input">The input with prefix <c>"~~/"</c> or <c>"~/"</c>.</param>
        /// <returns>The resolved URL.</returns>
        [RockInternal( "1.15" )]
        public string ResolveRockUrl( string input )
        {
            if ( input.IsNullOrWhiteSpace() )
            {
                return input;
            }

            if ( input.StartsWith( "~~/" ) )
            {
                var themeRoot = $"/Themes/{_pageCache.SiteTheme}/";
                return themeRoot + ( input.Length > 3 ? input.Substring( 3 ) : string.Empty );
            }

            if ( input.StartsWith( "~" ) && input.Length > 1 )
            {
                return input.Substring( 1 );
            }

            // The input format is unrecognized so return it.
            return input;
        }

        #endregion

        #region Person Preferences

        /// <summary>
        /// Gets the global person preferences. These are unique to the person
        /// but global across the entire system. Global preferences should be
        /// used with extreme caution and care.
        /// </summary>
        /// <returns>An instance of <see cref="PersonPreferenceCollection"/> that provides access to the preferences. This will never return <c>null</c>.</returns>
        public PersonPreferenceCollection GetGlobalPersonPreferences()
        {
            return _personPreferenceCollections.GetOrAdd( PersonPreferenceService.GetGlobalPreferencePrefix(), k =>
            {
                if ( CurrentVisitorId.HasValue )
                {
                    return PersonPreferenceCache.GetVisitorPreferenceCollection( CurrentVisitorId.Value );
                }
                else if ( CurrentPerson != null )
                {
                    return PersonPreferenceCache.GetPersonPreferenceCollection( CurrentPerson );
                }
                else
                {
                    return new PersonPreferenceCollection();
                }
            } );
        }

        /// <summary>
        /// Gets the person preferences scoped to the specified entity.
        /// </summary>
        /// <param name="scopedEntity">The entity to use when scoping the preferences for a particular use.</param>
        /// <returns>An instance of <see cref="PersonPreferenceCollection"/> that provides access to the preferences. This will never return <c>null</c>.</returns>
        public PersonPreferenceCollection GetScopedPersonPreferences( IEntity scopedEntity )
        {
            return _personPreferenceCollections.GetOrAdd( PersonPreferenceService.GetPreferencePrefix( scopedEntity ), k =>
            {
                if ( CurrentVisitorId.HasValue )
                {
                    return PersonPreferenceCache.GetVisitorPreferenceCollection( CurrentVisitorId.Value, scopedEntity );
                }
                else if ( CurrentPerson != null )
                {
                    return PersonPreferenceCache.GetPersonPreferenceCollection( CurrentPerson, scopedEntity );
                }
                else
                {
                    return new PersonPreferenceCollection();
                }
            } );
        }

        /// <summary>
        /// Gets the person preferences scoped to the specified entity.
        /// </summary>
        /// <param name="scopedEntity">The entity to use when scoping the preferences for a particular use.</param>
        /// <returns>An instance of <see cref="PersonPreferenceCollection"/> that provides access to the preferences. This will never return <c>null</c>.</returns>
        public PersonPreferenceCollection GetScopedPersonPreferences( IEntityCache scopedEntity )
        {
            return _personPreferenceCollections.GetOrAdd( PersonPreferenceService.GetPreferencePrefix( scopedEntity ), k =>
            {
                if ( CurrentVisitorId.HasValue )
                {
                    return PersonPreferenceCache.GetVisitorPreferenceCollection( CurrentVisitorId.Value, scopedEntity );
                }
                else if ( CurrentPerson != null )
                {
                    return PersonPreferenceCache.GetPersonPreferenceCollection( CurrentPerson, scopedEntity );
                }
                else
                {
                    return new PersonPreferenceCollection();
                }
            } );
        }

        #endregion
    }
}
