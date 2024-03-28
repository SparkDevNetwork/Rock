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
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;
using System.Web.Security.AntiXss;

using Rock.Blocks;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Web
{
    /// <summary>
    /// Helper class to work with the PageReference field type
    /// </summary>
    public class PageReference
    {
        #region Properties

        /// <summary>
        /// Gets or sets the page id.
        /// </summary>
        public int PageId { get; set; }

        /// <summary>
        /// Gets the route id.
        /// </summary>
        public int RouteId { get; set; }

        /// <summary>
        /// Gets the route parameters.
        /// </summary>
        /// <value>
        /// The route parameters as a case-insensitive dictionary of key/value pairs.
        /// </value>
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

        /// <summary>
        /// Gets the query string.
        /// </summary>
        /// <value>
        /// The query string.
        /// </value>
        public NameValueCollection QueryString { get; set; } = new NameValueCollection();

        /// <summary>
        /// Gets or sets the bread crumbs.
        /// </summary>
        /// <value>
        /// The bread crumbs.
        /// </value>
        public List<BreadCrumb> BreadCrumbs { get; set; } = new List<BreadCrumb>();

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get
            {
                if ( PageId != 0 )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// If this is reference to a PageRoute, this will return the Route, otherwise it will return the normal URL of the page
        /// </summary>
        /// <value>
        /// The route.
        /// </value>
        public string Route
        {
            get
            {
                if ( PageId <= 0 )
                {
                    return null;
                }

                var pageCache = PageCache.Get( PageId );
                if ( pageCache == null )
                {
                    return null;
                }

                var pageRoute = pageCache.PageRoutes.FirstOrDefault( a => a.Id == RouteId );
                return pageRoute != null ? pageRoute.Route : BuildUrl();
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// This is a cached lookup of all entity parameter identifier names
        /// and then a set of alternate names. For example, "PersonId" is a
        /// known identifier name with an alternate of "PersonGuid".
        /// </summary>
        private static readonly Lazy<Dictionary<string, (int EntityTypeId, List<string> Names)>> _allEntityParameterNames = new Lazy<Dictionary<string, (int EntityTypeId, List<string> Names)>>( BuildEntityParameterNames );

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference"/> class.
        /// </summary>
        public PageReference()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference" /> class.
        /// </summary>
        /// <param name="linkedPageValue">The linked page value.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="queryString">The query string.</param>
        public PageReference( string linkedPageValue, Dictionary<string, string> parameters = null, NameValueCollection queryString = null )
        {
            if ( !string.IsNullOrWhiteSpace( linkedPageValue ) )
            {
                string[] items = linkedPageValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

                //// linkedPageValue is in format "Page.Guid,PageRoute.Guid"
                //// If only the Page.Guid is specified this is just a reference to a page without a special route
                //// In case the PageRoute record can't be found from PageRoute.Guid (maybe the pageroute was deleted), fall back to the Page without a PageRoute

                Guid pageGuid = Guid.Empty;
                if ( items.Length > 0 )
                {
                    if ( Guid.TryParse( items[0], out pageGuid ) )
                    {
                        var pageCache = PageCache.Get( pageGuid );
                        if ( pageCache != null )
                        {
                            // Set the page
                            PageId = pageCache.Id;

                            Guid pageRouteGuid = Guid.Empty;
                            if ( items.Length == 2 )
                            {
                                if ( Guid.TryParse( items[1], out pageRouteGuid ) )
                                {
                                    var pageRouteInfo = pageCache.PageRoutes.FirstOrDefault( a => a.Guid == pageRouteGuid );
                                    if ( pageRouteInfo != null )
                                    {
                                        // Set the route
                                        RouteId = pageRouteInfo.Id;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if ( parameters != null )
            {
                Parameters = new Dictionary<string, string>( parameters );
            }

            if ( queryString != null )
            {
                QueryString = new NameValueCollection( queryString );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference" /> class.
        /// </summary>
        /// <param name="pageId">The page id.</param>
        public PageReference( int pageId )
        {
            Parameters = new Dictionary<string, string>();
            PageId = pageId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference"/> class.
        /// </summary>
        /// <param name="pageId">The page id.</param>
        /// <param name="routeId">The route id.</param>
        public PageReference( int pageId, int routeId )
            : this( pageId )
        {
            RouteId = routeId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference" /> class.
        /// </summary>
        /// <param name="pageId">The page id.</param>
        /// <param name="routeId">The route id.</param>
        /// <param name="parameters">The route parameters.</param>
        public PageReference( int pageId, int routeId, Dictionary<string, string> parameters )
            : this( pageId, routeId )
        {
            Parameters = parameters != null ? new Dictionary<string, string>( parameters ) : new Dictionary<string, string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference" /> class.
        /// </summary>
        /// <param name="pageId">The page id.</param>
        /// <param name="routeId">The route id.</param>
        /// <param name="parameters">The route parameters.</param>
        /// <param name="queryString">The query string.</param>
        public PageReference( int pageId, int routeId, Dictionary<string, string> parameters, NameValueCollection queryString )
            : this( pageId, routeId, parameters )
        {
            QueryString = queryString != null ? new NameValueCollection( queryString ) : new NameValueCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference" /> class.
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        public PageReference( PageReference pageReference )
            : this( pageReference.PageId, pageReference.RouteId, pageReference.Parameters, pageReference.QueryString )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference"/> class from a url.
        /// </summary>
        /// <param name="uri">The URI e.g.: new Uri( ResolveRockUrlIncludeRoot("~/Person/5")</param>
        /// <param name="applicationPath">The application path e.g.: HttpContext.Current.Request.ApplicationPath</param>
        public PageReference( Uri uri, string applicationPath )
        {
            Parameters = new Dictionary<string, string>();

            var routeInfo = new Rock.Web.RouteInfo( uri, applicationPath );
            if ( routeInfo != null )
            {
                if ( routeInfo.RouteData.Values["PageId"] != null )
                {
                    PageId = routeInfo.RouteData.Values["PageId"].ToString().AsInteger();
                }
                else if ( routeInfo.RouteData.DataTokens["PageRoutes"] != null )
                {
                    var pages = routeInfo.RouteData.DataTokens["PageRoutes"] as List<PageAndRouteId>;
                    if ( pages != null && pages.Count > 0 )
                    {
                        if ( pages.Count == 1 )
                        {
                            var pageAndRouteId = pages.First();
                            PageId = pageAndRouteId.PageId;
                            RouteId = pageAndRouteId.RouteId;
                        }
                        else
                        {
                            SiteCache site = SiteCache.GetSiteByDomain( uri.Host );
                            if ( site != null )
                            {
                                foreach ( var pageAndRouteId in pages )
                                {
                                    var pageCache = PageCache.Get( pageAndRouteId.PageId );
                                    if ( pageCache != null && pageCache.Layout != null && pageCache.Layout.SiteId == site.Id )
                                    {
                                        PageId = pageAndRouteId.PageId;
                                        RouteId = pageAndRouteId.RouteId;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    // Add route parameters.
                    foreach ( var routeParm in routeInfo.RouteData.Values )
                    {
                        Parameters.Add( routeParm.Key, ( string ) routeParm.Value );
                    }
                }
            }

            // Add query parameters.
            QueryString = HttpUtility.ParseQueryString( uri.Query );
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the value that matches the given page parameter name. This will
        /// first try to search the route parameters and then try the query
        /// parameters.
        /// </summary>
        /// <param name="name">The name of the parameter to be retrieved.</param>
        /// <returns>The value of the parameter or <c>null</c> if it was found.</returns>
        public string GetPageParameter( string name )
        {
            if ( Parameters.TryGetValue( name, out var value ) )
            {
                return value;
            }

            if ( QueryString.AllKeys.Contains( name ) )
            {
                return QueryString[name];
            }

            return null;
        }

        /// <summary>
        /// Gets all the page parameters related to this page reference. This
        /// includes both route parameters and query string parameters.
        /// </summary>
        /// <returns>A dictionary of parameter data whose case are case-insensitive.</returns>
        public IDictionary<string, string> GetPageParameters()
        {
            var parameters = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

            foreach ( var p in Parameters )
            {
                parameters.AddOrIgnore( p.Key, p.Value );
            }

            foreach ( var k in QueryString.AllKeys )
            {
                parameters.AddOrIgnore( k, QueryString[k] );
            }

            return parameters;
        }

        /// <summary>
        /// Builds the URL.
        /// </summary>
        /// <returns></returns>
        public string BuildUrl()
        {
            return BuildUrl( false );
        }

        /// <summary>
        /// Builds the URL.
        /// </summary>
        /// <param name="removeMagicToken">if set to <c>true</c> [remove magic token].</param>
        /// <returns></returns>
        public string BuildUrl( bool removeMagicToken )
        {
            string url = string.Empty;

            var parms = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

            // Add any route parameters
            if ( Parameters != null )
            {
                foreach ( var route in Parameters )
                {
                    if ( ( !removeMagicToken || route.Key.ToLower() != "rckipid" ) && !parms.ContainsKey( route.Key ) )
                    {
                        parms.Add( route.Key, route.Value );
                    }
                }
            }

            // merge parms from query string to the parms dictionary to get a single list of parms
            // skipping those parms that are already in the dictionary
            if ( QueryString != null )
            {
                foreach ( string key in QueryString.AllKeys.Where( a => a.IsNotNullOrWhiteSpace() ) )
                {
                    if ( !removeMagicToken || key.ToLower() != "rckipid" )
                    {
                        // check that the dictionary doesn't already have this key
                        if ( key != null && !parms.ContainsKey( key ) && QueryString[key] != null )
                        {
                            parms.Add( key, QueryString[key].ToString() );
                        }
                    }
                }
            }

            // See if there's a route that matches all parms
            if ( RouteId == 0 )
            {
                RouteId = GetRouteIdFromPageAndParms() ?? 0;
            }

            // load route URL
            if ( RouteId != 0 )
            {
                url = BuildRouteURL( parms );
            }

            // build normal url if route url didn't process
            if ( url == string.Empty )
            {
                url = "page/" + PageId;

                // add parms to the url
                if ( parms != null )
                {
                    string delimitor = "?";
                    foreach ( KeyValuePair<string, string> parm in parms )
                    {
                        url += delimitor + HttpUtility.UrlEncode( parm.Key ) + "=" + HttpUtility.UrlEncode( parm.Value );
                        delimitor = "&";
                    }
                }
            }

            // add base path to url -- Fixed bug #84
            url = ( HttpContext.Current.Request.ApplicationPath == "/" ) ? "/" + url : HttpContext.Current.Request.ApplicationPath + "/" + url;

            return url;
        }

        /// <summary>
        /// Builds the URL for use by mobile applications.
        /// </summary>
        /// <returns>A string that represents the mobile formatted link.</returns>
        internal string BuildMobileUrl()
        {
            return BuildMobileUrl( false );
        }

        /// <summary>
        /// Builds the URL for use by mobile applications.
        /// </summary>
        /// <param name="removeMagicToken">if set to <c>true</c> then the <c>rckipid</c> token is removed if found.</param>
        /// <returns>A string that represents the mobile formatted link.</returns>
        internal string BuildMobileUrl( bool removeMagicToken )
        {
            var parms = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

            // Add any route parameters
            if ( Parameters != null )
            {
                foreach ( var route in Parameters )
                {
                    if ( ( !removeMagicToken || route.Key.ToLower() != "rckipid" ) && !parms.ContainsKey( route.Key ) )
                    {
                        parms.Add( route.Key, route.Value );
                    }
                }
            }

            // merge parms from query string to the parms dictionary to get a single list of parms
            // skipping those parms that are already in the dictionary
            if ( QueryString != null )
            {
                foreach ( string key in QueryString.AllKeys.Where( a => a.IsNotNullOrWhiteSpace() ) )
                {
                    if ( !removeMagicToken || key.ToLower() != "rckipid" )
                    {
                        // check that the dictionary doesn't already have this key
                        if ( key != null && !parms.ContainsKey( key ) && QueryString[key] != null )
                        {
                            parms.Add( key, QueryString[key].ToString() );
                        }
                    }
                }
            }

            var url = PageCache.Get( PageId )?.Guid.ToString() ?? string.Empty;
            var delimitor = "?";

            // add parms to the url
            foreach ( KeyValuePair<string, string> parm in parms )
            {
                url += $"{delimitor}{parm.Key.UrlEncode()}={parm.Value.UrlEncode()}";
                delimitor = "&";
            }

            return url;
        }

        /// <summary>
        /// Builds and HTML encodes the URL.
        /// </summary>
        /// <returns></returns>
        public string BuildUrlEncoded()
        {
            return BuildUrlEncoded( false );
        }

        /// <summary>
        /// Builds and HTML encodes the URL.
        /// </summary>
        /// <param name="removeMagicToken">if set to <c>true</c> [remove magic token].</param>
        /// <returns></returns>
        public string BuildUrlEncoded( bool removeMagicToken )
        {
            return AntiXssEncoder.HtmlEncode( BuildUrl( removeMagicToken ), false );
        }

        /// <summary>
        /// Builds the route URL.
        /// </summary>
        /// <param name="parms">The parms.</param>
        /// <returns></returns>
        public string BuildRouteURL( Dictionary<string, string> parms )
        {
            string routeUrl = string.Empty;

            foreach ( var route in RouteTable.Routes.OfType<Route>() )
            {
                if ( route != null && route.DataTokens != null && route.DataTokens.ContainsKey( "PageRoutes" ) )
                {
                    var pageAndRouteIds = route.DataTokens["PageRoutes"] as List<PageAndRouteId>;
                    if ( pageAndRouteIds != null && pageAndRouteIds.Any( r => r.RouteId == RouteId ) )
                    {
                        routeUrl = route.Url;
                        break;
                    }
                }
            }

            // get dictionary of parms in the route
            Dictionary<string, string> routeParms = new Dictionary<string, string>();
            bool allRouteParmsProvided = true;

            var regEx = new Regex( @"{([A-Za-z0-9\-]+)}" );
            foreach ( Match match in regEx.Matches( routeUrl ) )
            {
                // add parm to dictionary
                routeParms.Add( match.Groups[1].Value, match.Value );

                // check that a value for that parm is available
                if ( parms == null || !parms.ContainsKey( match.Groups[1].Value ) )
                {
                    allRouteParmsProvided = false;
                }
            }

            // if we have a value for all route parms build route url
            if ( allRouteParmsProvided )
            {
                // merge route parm values
                foreach ( KeyValuePair<string, string> parm in routeParms )
                {
                    // merge field. This has to be UrlPathEncode to ensure a space is replaced with a
                    // %20 instead of + since this is to the left of the query string delimeter.
                    routeUrl = routeUrl.Replace( parm.Value, HttpUtility.UrlPathEncode( parms[parm.Key] ) );

                    // remove parm from dictionary
                    parms.Remove( parm.Key );
                }

                // add remaining parms to the query string
                if ( parms != null )
                {
                    string delimitor = "?";
                    foreach ( KeyValuePair<string, string> parm in parms )
                    {
                        routeUrl += delimitor + HttpUtility.UrlEncode( parm.Key ) + "=" + HttpUtility.UrlEncode( parm.Value );
                        delimitor = "&";
                    }
                }

                return routeUrl;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( PageId <= 0 )
            {
                return base.ToString();
            }

            var pageCache = PageCache.Get( this.PageId );
            if ( pageCache == null )
            {
                return base.ToString();
            }

            var pageRoute = pageCache.PageRoutes.FirstOrDefault( a => a.Id == this.RouteId );
            return pageRoute != null ? pageRoute.Route : pageCache.InternalName;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the route id from page and parms.
        /// </summary>
        /// <returns></returns>
        private int? GetRouteIdFromPageAndParms()
        {
            var pageCache = PageCache.Get( PageId );
            if ( pageCache != null && pageCache.PageRoutes.Any() )
            {
                var r = new Regex( @"(?<={)[A-Za-z0-9\-]+(?=})" );

                foreach ( var item in pageCache.PageRoutes )
                {
                    // If route contains no parameters, and no parameters were provided, return this route
                    var matches = r.Matches( item.Route );
                    if ( matches.Count == 0 && ( Parameters == null || Parameters.Count == 0 ) )
                    {
                        return item.Id;
                    }

                    // If route contains the same number of parameters as provided, check to see if they all match names
                    if ( matches.Count > 0 && Parameters != null && Parameters.Count == matches.Count )
                    {
                        bool matchesAllParms = true;

                        foreach ( Match match in matches )
                        {
                            if ( !Parameters.ContainsKey( match.Value ) )
                            {
                                matchesAllParms = false;
                                break;
                            }
                        }

                        if ( matchesAllParms )
                        {
                            return item.Id;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Builds the storage key.
        /// </summary>
        /// <param name="suffix">The suffix - if any - that should be added to the base key.</param>
        /// <returns>The base key + any suffix that should be added.</returns>
        private static string BuildStorageKey( string suffix )
        {
            return $"RockPageReferenceHistory{( string.IsNullOrWhiteSpace( suffix ) ? string.Empty : suffix )}";
        }

        /// <summary>
        /// Translates a route parameter value representing an entity identifier
        /// into one that can be used by the targer parameter name.
        /// </summary>
        /// <param name="value">The value of the existing route parameter.</param>
        /// <param name="parameterName">The name of the destination route parameter.</param>
        /// <param name="entityTypeId">The identifier of the entity type that <paramref name="value"/> represents.</param>
        /// <param name="disablePredictableIdentifiers"><c>true</c> if the destination site does not allow predictable identifiers and "Id" values should be hashed.</param>
        /// <returns>The expected value for <paramref name="parameterName"/> or the original <paramref name="value"/>.</returns>
        private static string GetRouteAlternateEntityValue( string value, string parameterName, int entityTypeId, bool disablePredictableIdentifiers )
        {
            if ( parameterName.EndsWith( "Guid", StringComparison.OrdinalIgnoreCase ) )
            {
                var guid = Reflection.GetEntityGuidForEntityType( entityTypeId, value );

                return guid.HasValue ? guid.Value.ToString() : value;
            }
            else if ( parameterName.EndsWith( "Id", StringComparison.OrdinalIgnoreCase ) )
            {
                var id = Reflection.GetEntityIdForEntityType( entityTypeId, value );

                if ( !id.HasValue )
                {
                    return value;
                }

                return disablePredictableIdentifiers ? IdHasher.Instance.GetHash( id.Value ) : id.Value.ToString();
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Builds the lookup table for entity parameter names so that we
        /// know which route parameters can be converted between Id and Guid.
        /// </summary>
        /// <returns>A dictionary with all the known entity parameter names.</returns>
        private static Dictionary<string, (int EntityTypeId, List<string> Names)> BuildEntityParameterNames()
        {
            var entityParameterNames = new Dictionary<string, (int EntityTypeId, List<string> Names)>( StringComparer.OrdinalIgnoreCase );
            var entityNames = EntityTypeCache.All()
                .Where( e => e.IsEntity )
                .Select( e => new
                {
                    e.Id,
                    e.GetEntityType()?.Name
                } )
                .Where( t => t.Name != null );

            foreach ( var entityType in entityNames )
            {
                entityParameterNames.Add( $"{entityType.Name}Id", (entityType.Id, new List<string> { $"{entityType.Name}Guid" }) );
                entityParameterNames.Add( $"{entityType.Name}Guid", (entityType.Id, new List<string> { $"{entityType.Name}Id" }) );
            }

            return entityParameterNames;
        }

        /// <summary>
        /// <para>
        /// Checks if the parameters for the two routes match. This is not an
        /// exact match. Meaning, if <paramref name="sourceRoute"/> has three parameters
        /// and <paramref name="destinationRoute"/> only has two but those two
        /// parameters can be found in <paramref name="sourceRoute"/> then it is
        /// considered a satisfying matching.
        /// </para>
        /// <para>
        /// A parameter will be considered satisfied if it is either a
        /// case-insensitive exact match or if it is a compatible known entity
        /// identifier. Meaning, <c>PersonId</c> will not satisfy <c>GroupGuid</c>
        /// because they are not compatible - even though they are known entity
        /// identifiers. However, <c>PersonId</c> will satisfy <c>PersonGuid</c>.
        /// </para>
        /// </summary>
        /// <param name="sourceRoute">The source route that is providing the parameters.</param>
        /// <param name="destinationRoute">The destination route that requires the parameters.</param>
        /// <returns><c>true</c> if the route parameters in <paramref name="sourceRoute"/> will satisfy the parameters in <paramref name="destinationRoute"/>; otherwise <c>false</c>.</returns>
        private static bool DoRouteParametersSatisfy( PageCache.PageRouteInfo sourceRoute, PageCache.PageRouteInfo destinationRoute )
        {
            foreach ( var parameter in destinationRoute.Parameters )
            {
                // Check if this is an exact name match.
                if ( sourceRoute.Parameters.Contains( parameter, StringComparer.OrdinalIgnoreCase ) )
                {
                    continue;
                }

                // Check if this parameter might have altername names.
                if ( _allEntityParameterNames.Value.TryGetValue( parameter, out var alternate ) )
                {
                    // See if any of the source route parameters match the alternate names.
                    if ( alternate.Names.Any( n => sourceRoute.Parameters.Contains( n, StringComparer.OrdinalIgnoreCase ) ) )
                    {
                        continue;
                    }
                }

                // Parameter name mismatch, so it isn't a match.
                return false;
            }

            return true;
        }

        /// <summary>
        /// This looks for any destination routes that match any of the source
        /// routes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A route matches if the route text (minus parameter names) is equal
        /// and the parameter names match. Parameter names match if they either
        /// are an exact (case-insensitive) match or if they match via an alternate
        /// entity identifier. Meaning, the destination "PersonId" would match
        /// the source parameter "PersonGuid" because it's a known entity identifier.
        /// </para>
        /// <para>
        /// The route <c>person/{PersonId}</c> will be reduced to <c>person/{}</c>
        /// for comparison purposes. The route <c>person/{PersonGuid}</c> will be
        /// reduced to the same so they will be considered matching. Then the
        /// parameter names will be compared. <c>PersonId</c> does not equal
        /// <c>PersonGuid</c>, but both are known to be compatible entity identifier
        /// parameter names so they are considered a match.
        /// </para>
        /// </remarks>
        /// <param name="sourceRoutes">The collection of source routes.</param>
        /// <param name="destinationRoutes">The potential destination routes that must match one of the <paramref name="sourceRoutes"/>.</param>
        /// <returns>An enumeration of matching destination routes.</returns>
        private static IEnumerable<PageCache.PageRouteInfo> GetCrossMatchingRoutes( List<PageCache.PageRouteInfo> sourceRoutes, List<PageCache.PageRouteInfo> destinationRoutes )
        {
            // Loop through all destination routes looking for any that match.
            foreach ( var destinationRoute in destinationRoutes )
            {
                // Loop through all source routes looking for any with a route
                // that matches the possible destination route.
                foreach ( var sourceRoute in sourceRoutes )
                {
                    // Basic routes without parameters must match.
                    if ( !sourceRoute.RouteWithEmptyParameters.Equals( destinationRoute.RouteWithEmptyParameters, StringComparison.OrdinalIgnoreCase ) )
                    {
                        continue;
                    }

                    // Parameter names must match or be one of the auto-translatable names.
                    if ( !DoRouteParametersSatisfy( sourceRoute, destinationRoute ) )
                    {
                        continue;
                    }

                    yield return destinationRoute;

                    // Don't need to keep checking additional source routes.
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the page references and their breadcrumbs that make up the
        /// current page and it's tree of parent pages.
        /// </summary>
        /// <param name="rockPage">The page handling this request.</param>
        /// <param name="initialPage">The initial page to start building the references from.</param>
        /// <param name="initialPageReference">The page reference that contains the parameter data for <paramref name="initialPage"/>.</param>
        /// <param name="keySuffix">The cache key suffix when accessing cache.</param>
        /// <returns>An array of page references with the current page being the last item in the list.</returns>
        internal static List<PageReference> GetBreadCrumbPageReferences( RockPage rockPage, PageCache initialPage, PageReference initialPageReference, string keySuffix )
        {
            if ( initialPage == null )
            {
                return new List<PageReference>();
            }

            // Get previous page references in nav history
            var key = BuildStorageKey( keySuffix );
            var pageReferenceHistory = ( Dictionary<int, List<BreadCrumb>> ) HttpContext.Current.Session[key];
            var newPageReferenceHistory = new Dictionary<int, List<BreadCrumb>>();

            // Current page hierarchy references
            var pageReferences = new List<PageReference>();

            // Initial starting parameters.
            var trackedPageParameters = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

            if ( initialPageReference != null )
            {
                foreach ( var p in initialPageReference.Parameters )
                {
                    trackedPageParameters.AddOrIgnore( p.Key, p.Value );
                }

                // As Querystring is a NameValueCollection, it may contain entries with key as null.
                // However, adding null as a key to a Dictionary throws an exception and so we would like to filter those entries out of Querystring.
                foreach ( var qs in initialPageReference.QueryString.AllKeys.Where( k => k != null ) )
                {
                    trackedPageParameters.AddOrIgnore( qs, initialPageReference.QueryString[qs] );
                }
            }

            var currentParentPages = initialPage.GetPageHierarchy();

            foreach ( PageCache page in currentParentPages )
            {
                var pageBlocks = page.Blocks.Where( b => b.BlockLocation == BlockLocation.Page );
                var pageBreadCrumbs = new List<BreadCrumb>();

                // Check the blocks that support the new breadcrumb behavior.
                foreach ( var block in pageBlocks )
                {
                    var compiledType = block.BlockType.GetCompiledType();

                    if ( compiledType == null || !typeof( IBreadCrumbBlock ).IsAssignableFrom( compiledType ) )
                    {
                        continue;
                    }

                    var instance = ( IBreadCrumbBlock ) Activator.CreateInstance( compiledType );
                    var instancePageReference = new PageReference( page.Id, 0, trackedPageParameters );
                    var crumbResult = instance.GetBreadCrumbs( instancePageReference );

                    if ( crumbResult?.BreadCrumbs != null && crumbResult.BreadCrumbs.Count > 0 )
                    {
                        foreach ( var crumb in crumbResult.BreadCrumbs )
                        {
                            // In the future, when we can change PageReference.BreadCrumbs
                            // to be a list of IBreadCrumb then this can be simplified.
                            pageBreadCrumbs.Add( new BreadCrumb( crumb.Name, crumb.Url, crumb.Active ) );
                        }
                    }

                    if ( crumbResult?.AdditionalParameters != null && crumbResult.AdditionalParameters.Count > 0 )
                    {
                        foreach ( var ap in crumbResult.AdditionalParameters )
                        {
                            trackedPageParameters.AddOrReplace( ap.Key, ap.Value );
                        }
                    }
                }

                if ( initialPage.Id != page.Id && pageReferenceHistory != null && pageReferenceHistory.TryGetValue( page.Id, out var cachedBreadCrumbs ) )
                {
                    pageBreadCrumbs.AddRange( cachedBreadCrumbs );
                    newPageReferenceHistory.Add( page.Id, cachedBreadCrumbs );
                }
                else
                {
                    var blockPageReference = page.Id == initialPage.Id ? new PageReference( initialPageReference ) : new PageReference( page.Id );

                    foreach ( var block in pageBlocks.Where( b => b.BlockType.Path.IsNotNullOrWhiteSpace() ) )
                    {
                        try
                        {
                            System.Web.UI.Control control = rockPage.TemplateControl.LoadControl( block.BlockType.Path );
                            if ( control is RockBlock rockBlock )
                            {
                                rockBlock.SetBlock( page, block );
                                rockBlock.GetBreadCrumbs( blockPageReference ).ForEach( c => blockPageReference.BreadCrumbs.Add( c ) );
                            }

                            control = null;
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( ex, HttpContext.Current, initialPage.Id, initialPage.Layout.SiteId );
                        }
                    }

                    pageBreadCrumbs.AddRange( blockPageReference.BreadCrumbs );
                    newPageReferenceHistory.Add( page.Id, blockPageReference.BreadCrumbs );
                }

                var parentPageReference = new PageReference( page.Id );

                var bcName = page.BreadCrumbText;
                if ( bcName != string.Empty )
                {
                    parentPageReference.BreadCrumbs.Add( new BreadCrumb( bcName, parentPageReference.BuildUrl() ) );
                }

                parentPageReference.BreadCrumbs.AddRange( pageBreadCrumbs );
                parentPageReference.BreadCrumbs.ForEach( c => c.Active = false );
                pageReferences.Add( parentPageReference );
            }

            HttpContext.Current.Session[key] = newPageReferenceHistory;

            pageReferences.Reverse();

            return pageReferences;
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Gets the parent page references.
        /// </summary>
        /// <param name="rockPage">The rock page.</param>
        /// <param name="currentPage">The current page.</param>
        /// <param name="currentPageReference">The current page reference.</param>
        /// <returns></returns>
        [RockObsolete( "1.16.1" )]
        [Obsolete( "Parent page references is handled internally." )]
        public static List<PageReference> GetParentPageReferences( RockPage rockPage, PageCache currentPage, PageReference currentPageReference )
        {
            return new List<PageReference>();
        }

        /// <summary>
        /// Gets the parent page references.
        /// </summary>
        /// <param name="rockPage">The rock page.</param>
        /// <param name="currentPage">The current page.</param>
        /// <param name="currentPageReference">The current page reference.</param>
        /// <param name="keySuffix">The suffix - if any - that should be added to the base key that will be used to get the parent page references.</param>
        /// <returns></returns>
        [RockObsolete( "1.16.1" )]
        [Obsolete( "Parent page references is handled internally." )]
        public static List<PageReference> GetParentPageReferences( RockPage rockPage, PageCache currentPage, PageReference currentPageReference, string keySuffix )
        {
            return new List<PageReference>();
        }

        /// <summary>
        /// Saves the history.
        /// </summary>
        /// <param name="pageReferences">The page references.</param>
        [RockObsolete( "1.16.1" )]
        [Obsolete( "Caching of page references for use as breadcrumbs is handled internally." )]
        public static void SavePageReferences( List<PageReference> pageReferences )
        {
        }

        /// <summary>
        /// Saves the history.
        /// </summary>
        /// <param name="pageReferences">The page references.</param>
        /// <param name="keySuffix">The suffix - if any - that should be added to the base key that will be used to save the parent page references.</param>
        [RockObsolete( "1.16.1" )]
        [Obsolete( "Caching of page references for use as breadcrumbs is handled internally." )]
        public static void SavePageReferences( List<PageReference> pageReferences, string keySuffix )
        {
        }

        /// <summary>
        /// Gets a reference for the specified page including the route that matches the greatest number of supplied parameters.
        /// Parameters that do not match the route are included as query parameters.
        /// </summary>
        /// <param name="pageId">The target page.</param>
        /// <param name="parameters">The set of parameters that are candidates for matching to the routes associated with the target page.</param>
        /// <returns></returns>
        public static PageReference GetBestMatchForParameters( int pageId, Dictionary<string, string> parameters )
        {
            // Find a route associated with the page that contains the
            // maximum number of available parameters.
            var route = PageCache.Get( pageId )?.GetBestMatchingRoute( parameters );

            // Separate the query and route parameters.
            var queryValues = new NameValueCollection();
            var routeValues = new Dictionary<string, string>();
            foreach ( var p in parameters )
            {
                if ( route?.Parameters.Contains( p.Key ) == true )
                {
                    routeValues.Add( p.Key, p.Value );
                }
                else
                {
                    queryValues.Add( p.Key, p.Value );
                }
            }

            // Create and return a new page reference.
            return new PageReference( pageId, route?.Id ?? 0, routeValues, queryValues );
        }

        /// <summary>
        /// <para>
        /// Determines the best alternate page for the given parameters. An
        /// alternate page is one that has the same route but exists on a different
        /// site. For example, a mobile application might have a person profile
        /// page route with the same route name as the internal staff site person
        /// profile page route. This makes those two pages alternates of each other.
        /// </para>
        /// <para>
        /// If is possible a route to the original page will be returned depending
        /// on the type of site.
        /// </para>
        /// </summary>
        /// <param name="originalPageId">The original page to use when finding alternate page routes.</param>
        /// <param name="destinationSiteId">The destination site the alternate page is being requested for. This is used for filtering and prioritizing the results.</param>
        /// <param name="parameters">The available parameters to use when finding the available and best alternate routes.</param>
        /// <returns>A <see cref="PageReference"/> instance representing the best route or <c>null</c> if one could not be determined.</returns>
        internal static PageReference GetBestAlternatePageRouteForParameters( int originalPageId, int destinationSiteId, Dictionary<string, string> parameters )
        {
            var sourcePage = PageCache.Get( originalPageId );
            var destinationSite = SiteCache.Get( destinationSiteId );

            if ( sourcePage == null || destinationSite == null )
            {
                return null;
            }

            // Step 1: Get all routes from the original page that match the parameters.
            var sourcePageRoutes = sourcePage.GetAllMatchingRoutes( parameters );

            // Step 2: Find all destination pages that match the routes.
            var pageCacheQry = PageCache.All().AsQueryable();

            if ( destinationSite.SiteType == SiteType.Web )
            {
                // Web sites can only link to websites, not apps.
                pageCacheQry = pageCacheQry.Where( p => p.Layout.Site.SiteType == SiteType.Web );
            }
            else if ( destinationSite.SiteType == SiteType.Mobile )
            {
                // Mobile apps can only link within the one app or out to web.
                pageCacheQry = pageCacheQry.Where( p => p.Layout.SiteId == destinationSiteId || p.Layout.Site.SiteType == SiteType.Web );
            }
            else
            {
                // Anything else must be an exact site match.
                pageCacheQry = pageCacheQry.Where( p => p.Layout.SiteId == destinationSiteId );
            }

            // Get all the matching pages and any routes that page has that matches
            // one of the source routes. Then filter out any results with no
            // matching routes.
            var destinationPagesQry = pageCacheQry
                .Select( p => new
                {
                    Page = p,
                    Routes = GetCrossMatchingRoutes( sourcePageRoutes, p.PageRoutes ).ToList()
                } )
                .Where( p => p.Routes.Any() );

            // Step 3: Take the best matching page and route. This looks for a
            // page on the same site first. Then orders by the number of route
            // parameters to find the most specific route.
            var destinationPageRoute = destinationPagesQry
                .OrderByDescending( p => p.Page.Layout.SiteId == destinationSiteId )
                .SelectMany( p => p.Routes.Select( r => new
                {
                    p.Page,
                    Route = r
                } ) )
                .OrderByDescending( p => p.Page.Layout.SiteId == destinationSiteId )
                .ThenByDescending( p => p.Route.Parameters.Count )
                .FirstOrDefault();

            if ( destinationPageRoute == null )
            {
                return null;
            }

            var routeParameters = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );
            var queryStringCollection = new NameValueCollection( StringComparer.OrdinalIgnoreCase );

            // Convert the parameters to either route parameters or query parameters.
            foreach ( var parameter in parameters )
            {
                if ( destinationPageRoute.Route.Parameters.Contains( parameter.Key ) == true )
                {
                    // Exact name match on a route parameter.
                    routeParameters.Add( parameter.Key, parameter.Value );
                }
                else
                {
                    // No exact match, check if we can convert it.
                    if ( _allEntityParameterNames.Value.TryGetValue( parameter.Key, out var alternate ) )
                    {
                        var alternateKey = alternate.Names.FirstOrDefault( a => destinationPageRoute.Route.Parameters.Contains( a ) );

                        if ( alternateKey != null )
                        {
                            // We found an alternate, get the real value.
                            routeParameters.Add( alternateKey, GetRouteAlternateEntityValue( parameter.Value, alternateKey, alternate.EntityTypeId, destinationPageRoute.Page.Layout.Site.DisablePredictableIds ) );
                            continue;
                        }
                    }

                    // If we didn't find a match
                    queryStringCollection.Add( parameter.Key, parameter.Value );
                }
            }

            return new PageReference( destinationPageRoute.Page.Id, destinationPageRoute.Route.Id, routeParameters, queryStringCollection );
        }

        #endregion
    }
}