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
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using System.Web.Routing;

using Rock.Model;
using Rock.Transactions;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Web
{
    /// <summary>
    /// Rock custom route handler
    /// </summary>
    public sealed class RockRouteHandler : IRouteHandler
    {
        /// <summary>
        /// Determine the logical page being requested by evaluating the routedata, or querystring and
        /// then loading the appropriate layout (ASPX) page
        /// 
        /// Pick url on the following priority order:
        /// 1. PageId
        /// 2. Route match and site match
        /// 3. ShortLink match and site match
        /// 4. Route and no site match
        /// 5. ShortLink with no site match
        /// 6. If there is no routing info in the request then set to default page
        /// 7. 404 if route does not exist
        /// 
        /// </summary>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        System.Web.IHttpHandler IRouteHandler.GetHttpHandler( RequestContext requestContext )
        {
            string pageId = string.Empty;
            int routeId = 0;
            bool isSiteMatch = false;
            Dictionary<string, string> parms;
            string host;
            HttpRequestBase routeHttpRequest;
            HttpCookie siteCookie;

            // Context cannot be null
            if ( requestContext == null )
            {
                throw new ArgumentNullException( "requestContext" );
            }

            try
            {
                routeHttpRequest = requestContext.HttpContext.Request;
                siteCookie = routeHttpRequest.Cookies["last_site"];
                parms = new Dictionary<string, string>();
                host = WebRequestHelper.GetHostNameFromRequest( HttpContext.Current );

                if ( requestContext.RouteData.Values["PageId"] != null )
                {
                    // Pages using the default routing URL will have the page id in the RouteData.Values collection
                    pageId = ( string ) requestContext.RouteData.Values["PageId"];
                    isSiteMatch = true;
                }
                else if ( requestContext.RouteData.DataTokens["PageRoutes"] != null )
                {
                    SiteCache site = GetSite(routeHttpRequest, host, siteCookie );
                    // Pages that use a custom URL route will have the page id in the RouteData.DataTokens collection
                    GetPageIdFromDataTokens(requestContext, site, out pageId, out routeId, out isSiteMatch );

                    foreach ( var routeParm in requestContext.RouteData.Values )
                    {
                        parms.Add( routeParm.Key, ( string ) routeParm.Value );
                    }
                }
                else if ( ( ( System.Web.Routing.Route ) requestContext.RouteData.Route ).Url.IsNullOrWhiteSpace() )
                {
                    // if we don't have routing info then set the page ID to the default page for the site.

                    // Get the site, if not found use the default site
                    SiteCache site = GetSite( routeHttpRequest, host, siteCookie );
                    if ( site == null )
                    {
                        site = SiteCache.Get( SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid() );
                    }

                    if ( site.DefaultPageId.HasValue )
                    {
                        pageId = site.DefaultPageId.Value.ToString();
                        isSiteMatch = true;
                    }
                    else
                    {
                        throw new SystemException( "Invalid Site Configuration" );
                    }
                }

                // If the the page ID and site has not yet been matched
                if ( string.IsNullOrEmpty( pageId ) || !isSiteMatch )
                {
                    SiteCache site = GetSite( routeHttpRequest, host, siteCookie );

                    // if not found use the default site
                    if ( site == null )
                    {
                        site = SiteCache.Get( SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid() );
                    }

                    // Are shortlinks enabled for this site? If so, check for a matching shortlink route.
                    if ( site != null && site.EnabledForShortening )
                    {
                        // Check to see if this is a short link route
                        string shortlink = null;
                        if ( requestContext.RouteData.Values.ContainsKey( "shortlink" ) )
                        {
                            shortlink = requestContext.RouteData.Values["shortlink"].ToString();
                        }
                        else 
                        {
                            // Because we implemented shortlinks using a {shortlink} (catchall) route, it's
                            // possible the organization added a custom {catchall} route (at root level; no slashes)
                            // and it is overriding our shortlink route.  If they did, use it for a possible 'shortlink'
                            // route match.
                            if ( requestContext.RouteData.DataTokens["RouteName"] != null && requestContext.RouteData.DataTokens["RouteName"].ToStringSafe().StartsWith( "{" ) )
                            {
                                var routeName = requestContext.RouteData.DataTokens["RouteName"].ToStringSafe().Trim( new Char[] { '{', '}'} );
                                shortlink = requestContext.RouteData.Values[routeName].ToStringSafe();
                            }
                        }
                        
                        // If shortlink have the same name as route and route's site did not match, then check if shortlink site match.
                        if ( shortlink.IsNullOrWhiteSpace() && requestContext.RouteData.DataTokens["RouteName"] != null )
                        {
                            shortlink = requestContext.RouteData.DataTokens["RouteName"].ToString();
                        }

                        if ( shortlink.IsNotNullOrWhiteSpace() )
                        {
                            using ( var rockContext = new Rock.Data.RockContext() )
                            {
                                var pageShortLink = new PageShortLinkService( rockContext ).GetByToken( shortlink, site.Id );

                                if ( pageShortLink != null && ( pageShortLink.SiteId == site.Id || requestContext.RouteData.DataTokens["RouteName"] == null ) )
                                {
                                    pageId = string.Empty;
                                    routeId = 0;

                                    string trimmedUrl = pageShortLink.Url.RemoveCrLf().Trim();

                                    var transaction = new ShortLinkTransaction();
                                    transaction.PageShortLinkId = pageShortLink.Id;
                                    transaction.Token = pageShortLink.Token;
                                    transaction.Url = trimmedUrl;
                                    if ( requestContext.HttpContext.User != null )
                                    {
                                        transaction.UserName = requestContext.HttpContext.User.Identity.Name;
                                    }

                                    transaction.DateViewed = RockDateTime.Now;
                                    transaction.IPAddress = WebRequestHelper.GetClientIpAddress( routeHttpRequest );
                                    transaction.UserAgent = routeHttpRequest.UserAgent ?? string.Empty;
                                    RockQueue.TransactionQueue.Enqueue( transaction );

                                    requestContext.HttpContext.Response.Redirect( trimmedUrl );
                                    return null;
                                }
                            }
                        }

                        // If site has has been enabled for mobile redirect, then we'll need to check what type of device is being used
                        if ( site.EnableMobileRedirect )
                        {
                            // get the device type
                            string u = routeHttpRequest.UserAgent;

                            var clientType = InteractionDeviceType.GetClientType( u );

                            bool redirect = false;

                            // first check if device is a mobile device
                            if ( clientType == "Mobile" )
                            {
                                redirect = true;
                            }

                            // if not, mobile device and tables should be redirected also, check if device is a tablet
                            if ( !redirect && site.RedirectTablets && clientType == "Tablet")
                            {
                                redirect = true;
                            }

                            if ( redirect )
                            {
                                if ( site.MobilePageId.HasValue )
                                {
                                    pageId = site.MobilePageId.Value.ToString();
                                    routeId = 0;
                                }
                                else if ( !string.IsNullOrWhiteSpace( site.ExternalUrl ) )
                                {
                                    requestContext.HttpContext.Response.Redirect( site.ExternalUrl );
                                    return null;
                                }
                            }
                        }
                    }
                }

                PageCache page = null;
                if ( !string.IsNullOrEmpty( pageId ) )
                {
                    int pageIdNumber = 0;
                    if ( int.TryParse( pageId, out pageIdNumber ) )
                    {
                        page = PageCache.Get( pageIdNumber );
                    }
                }

                if ( page == null )
                {
                    // try to get site's 404 page
                    SiteCache site = GetSite( routeHttpRequest, host, siteCookie );
                    if ( site != null && site.PageNotFoundPageId != null )
                    {
                        if ( Convert.ToBoolean( GlobalAttributesCache.Get().GetValue( "Log404AsException" ) ) )
                        {
                            Rock.Model.ExceptionLogService.LogException(
                                new Exception( $"404 Error: {routeHttpRequest.Url.AbsoluteUri}" ),
                                requestContext.HttpContext.ApplicationInstance.Context );
                        }

                        page = PageCache.Get( site.PageNotFoundPageId ?? 0 );
                        requestContext.HttpContext.Response.StatusCode = 404;
                        requestContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                    }
                    else
                    {
                        // no 404 page found for the site, return the default 404 error page
                        return ( System.Web.UI.Page ) BuildManager.CreateInstanceFromVirtualPath( "~/Http404Error.aspx", typeof( System.Web.UI.Page ) );
                    }
                }

                CreateOrUpdateSiteCookie( siteCookie, requestContext, page );

                string theme = page.Layout.Site.Theme;
                string layout = page.Layout.FileName;
                string layoutPath = PageCache.FormatPath( theme, layout );
                
                try
                {
                    return CreateRockPage( page, layoutPath, routeId, parms, routeHttpRequest );
                }
                catch ( System.Web.HttpException )
                {
                    // The Selected theme and/or layout didn't exist so try to use the layout in the default theme.
                    theme = "Rock";

                    // Verify that Layout exists in the default theme directory and if not try use the default layout of the default theme
                    string layoutPagePath = string.Format( "~/Themes/Rock/Layouts/{0}.aspx", layout );
                    if ( !File.Exists( requestContext.HttpContext.Server.MapPath( layoutPagePath ) ) )
                    {
                        layout = "FullWidth";
                    }

                    layoutPath = PageCache.FormatPath( theme, layout );

                    return CreateRockPage( page, layoutPath, routeId, parms, routeHttpRequest );
                }
            }
            catch ( Exception ex )
            {
                if ( requestContext.HttpContext != null )
                {
                    requestContext.HttpContext.Cache["RockExceptionOrder"] = "66";
                    requestContext.HttpContext.Cache["RockLastException"] = ex;
                }

                return ( System.Web.UI.Page ) BuildManager.CreateInstanceFromVirtualPath( "~/Error.aspx", typeof( System.Web.UI.Page ) );
            }
        }

        /// <summary>
        /// Reregisters the routes from PageRoute and default routes. Does not affect ODataService routes. Call this method after saving changes to PageRoute entities.
        /// </summary>
        public static void ReregisterRoutes()
        {
            RemoveRockPageRoutes();
            RegisterRoutes();
        }

        /// <summary>
        /// Registers the routes from PageRoute and default routes.
        /// </summary>
        public static void RegisterRoutes()
        {
            RouteCollection routes = RouteTable.Routes;

            PageRouteService pageRouteService = new PageRouteService( new Rock.Data.RockContext() );

            var routesToInsert = new RouteCollection();

            // Add ignore rule for asp.net ScriptManager files. 
            routesToInsert.Ignore( "{resource}.axd/{*pathInfo}" );

            //Add page routes, order is very important here as IIS takes the first match
            IOrderedEnumerable<PageRoute> pageRoutes = pageRouteService.Queryable().AsNoTracking().ToList().OrderBy( r => r.Route, StringComparer.OrdinalIgnoreCase );

            foreach ( var pageRoute in pageRoutes )
            {
                routesToInsert.AddPageRoute( pageRoute.Route, new Rock.Web.PageAndRouteId { PageId = pageRoute.PageId, RouteId = pageRoute.Id } );
            }

            // Add a default page route
            routesToInsert.Add( new Route( "page/{PageId}", new Rock.Web.RockRouteHandler() ) );

            // Add a default route for when no parameters are passed
            routesToInsert.Add( new Route( "", new Rock.Web.RockRouteHandler() ) );

            // Add a default route for shortlinks
            routesToInsert.Add( new Route( "{shortlink}", new Rock.Web.RockRouteHandler() ) );

            // Insert the list of routes to the beginning of the Routes so that PageRoutes, etc are before OdataRoutes. Even when Re-Registering routes
            // Since we are inserting at 0, reverse the list to they end up in the original order
            foreach ( var pageRoute in routesToInsert.Reverse() )
            {
                routes.Insert( 0, pageRoute );
            }
        }

        /// <summary>
        /// Removes the rock page and default routes from RouteTable.Routes but leaves the ones created by ODataService.
        /// </summary>
        public static void RemoveRockPageRoutes()
        {
            RouteCollection routes = RouteTable.Routes;
            PageRouteService pageRouteService = new PageRouteService( new Rock.Data.RockContext() );
            var pageRoutes = pageRouteService.Queryable().ToList();

            // First we have to remove the routes stored in the DB without removing the ODataService routes because we can't reload them.
            // Routes that were removed from the DB have already been removed from the RouteTable in PreSaveChanges()
            foreach( var pageRoute in pageRoutes )
            {
                var route = routes.OfType<Route>().Where( a => a.Url == pageRoute.Route ).FirstOrDefault();

                if ( route != null )
                {
                    routes.Remove( route );
                }
            }

            // Remove the shortlink route
            var shortLinkRoute = routes.OfType<Route>().Where( r => r.Url == "{shortlink}" ).FirstOrDefault();
            if ( shortLinkRoute != null )
            {
                routes.Remove( shortLinkRoute );
            }

            // Remove the page route
            var pageIdRoute = routes.OfType<Route>().Where( r => r.Url == "page/{PageId}" ).FirstOrDefault();
            if ( pageIdRoute != null )
            {
                routes.Remove( pageIdRoute );
            }

            // Remove the default route for when no parameters are passed
            var defaultRoute = routes.OfType<Route>().Where( r => r.Url == "" ).FirstOrDefault();
            if( defaultRoute != null )
            {
                routes.Remove( pageIdRoute );
            }

            // Remove scriptmanager ignore route
            var scriptmanagerRoute = routes.OfType<Route>().Where( r => r.Url == "{resource}.axd/{*pathInfo}" ).FirstOrDefault();
            if ( scriptmanagerRoute != null )
            {
                routes.Remove( scriptmanagerRoute );
            }
        }

        /// <summary>
        /// Uses the DataTokens to contstruct a list of PageAndRouteIds
        /// If any exist then set page and route ID to the first one found.
        /// Then loop through the collection looking for a site match, if one is found then set the page and route ID to that
        /// and the IsSiteMatch property to true.
        /// </summary>
        private void GetPageIdFromDataTokens( RequestContext routeRequestContext, SiteCache site, out string pageId, out int routeId, out bool isSiteMatch )
        {
            pageId = string.Empty;
            routeId = 0;
            isSiteMatch = false;

            // Pages that use a custom URL route will have the page id in the RouteData.DataTokens collection
            List<PageAndRouteId> pageAndRouteIds = routeRequestContext.RouteData.DataTokens["PageRoutes"] as List<PageAndRouteId>;

            if ( pageAndRouteIds != null && pageAndRouteIds.Any() )
            {
                // Default to first site/page
                var pageAndRouteIdDefault = pageAndRouteIds.First();
                pageId = pageAndRouteIdDefault.PageId.ToJson();
                routeId = pageAndRouteIdDefault.RouteId;

                // Then check to see if any can be matched by site
                

                if ( site == null )
                {
                    return;
                }

                foreach ( var pageAndRouteId in pageAndRouteIds )
                {
                    var pageCache = PageCache.Get( pageAndRouteId.PageId );
                    if ( pageCache != null && pageCache.Layout != null && pageCache.Layout.SiteId == site.Id )
                    {
                        pageId = pageAndRouteId.PageId.ToJson();
                        routeId = pageAndRouteId.RouteId;
                        isSiteMatch = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the site from the site cache in the following order:
        /// 1. check the query string and try to get the site
        /// 2. Get the site using the domain of the current request
        /// 3. Get the last site from the site cookie
        /// </summary>
        /// <returns></returns>
        private SiteCache GetSite(HttpRequestBase routeHttpRequest, string host, HttpCookie siteCookie )
        {
            SiteCache site = null;

            // First check to see if site was specified in querystring
            int? siteId = routeHttpRequest.QueryString["SiteId"].AsIntegerOrNull();
            if ( siteId.HasValue )
            {
                site = SiteCache.Get( siteId.Value );
            }

            // Then check to see if site can be determined by domain
            if ( site == null )
            {
                site = SiteCache.GetSiteByDomain( host );
            }

            // Then check the last site
            if ( site == null )
            {
                if ( siteCookie != null && siteCookie.Value != null )
                {
                    site = SiteCache.Get( siteCookie.Value.AsInteger() );
                }
            }

            return site;
        }

        /// <summary>
        /// Creates the or updates site cookie with the last_site value using the site ID of the page
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="routeRequestContext">The routeRequestContext.</param>
        /// <param name="siteCookie">The siteCookie.</param>
        private void CreateOrUpdateSiteCookie( HttpCookie siteCookie, RequestContext routeRequestContext, PageCache page )
        {
            if ( siteCookie == null )
            {
                siteCookie = new System.Web.HttpCookie( "last_site", page.Layout.SiteId.ToString() );
            }
            else
            {
                siteCookie.Value = page.Layout.SiteId.ToString();
            }

            routeRequestContext.HttpContext.Response.SetCookie( siteCookie );
        }

        /// <summary>
        /// Creates the rock page for the selected theme and layout.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="layoutPath">The layout path.</param>
        /// <param name="routeHttpRequest">The routeHttpRequest.</param>
        /// <param name="parms">The parms.</param>
        /// /// <param name="routeId">The routeId.</param>
        /// <returns></returns>
        private Rock.Web.UI.RockPage CreateRockPage( PageCache page, string layoutPath, int routeId, Dictionary<string, string> parms, HttpRequestBase routeHttpRequest )
        {
            // Return the page for the selected theme and layout
            Rock.Web.UI.RockPage cmsPage = ( Rock.Web.UI.RockPage ) BuildManager.CreateInstanceFromVirtualPath( layoutPath, typeof( Rock.Web.UI.RockPage ) );
            cmsPage.SetPage( page );
            cmsPage.PageReference = new PageReference( page.Id, routeId, parms, routeHttpRequest.QueryString );
            return cmsPage;
        }
    }

    /// <summary>
    /// Helper for storing page an route ids in a System.Web.Routing.Route datatoken
    /// </summary>
    public class PageAndRouteId
    {
        /// <summary>
        /// Gets or sets the page identifier.
        /// </summary>
        /// <value>
        /// The page identifier.
        /// </value>
        public int PageId { get; set; }

        /// <summary>
        /// Gets or sets the route identifier.
        /// </summary>
        /// <value>
        /// The route identifier.
        /// </value>
        public int RouteId { get; set; }
    }

    /// <summary>
    /// Handler used when an error occurs
    /// </summary>
    public class HttpHandlerError : System.Web.IHttpHandler
    {
        /// <summary>
        /// Gets the status code.
        /// </summary>
        public int StatusCode { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHandlerError"/> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        public HttpHandlerError( int statusCode )
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.</returns>
        public bool IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest( System.Web.HttpContext context )
        {
            context.Response.StatusCode = StatusCode;
            context.Response.End();
            return;
        }
    }
}