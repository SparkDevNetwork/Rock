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
using System.Linq;
using System.IO;
using System.Web.Compilation;
using System.Web.Routing;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Transactions;
using Rock.Data;

namespace Rock.Web
{
    /// <summary>
    /// Rock custom route handler
    /// </summary>
    public sealed class RockRouteHandler : IRouteHandler
    {

        private class PageCacheAndRouteId
        {
            public PageCache Page { get; set; }
            public int RouteId { get; set; }
        }

        // Page Functions

        private PageCache GetPageForDefaultRoute( RequestContext requestContext )
        {
            int? routeData_PageId = requestContext.RouteData.Values["PageId"]?.ToString()?.AsIntegerOrNull();
            if ( routeData_PageId.HasValue )
            {
                return PageCache.Get( routeData_PageId.Value );
            }
            else
            {
                return null;
            }
        }

        private IEnumerable<PageCacheAndRouteId> GetPagesForRoute( RequestContext requestContext )
        {
            List<PageAndRouteId> matchedRoutes = requestContext.RouteData.DataTokens["PageRoutes"] as List<PageAndRouteId>;

            if ( matchedRoutes != null )
            {
                return matchedRoutes.Select( r => new PageCacheAndRouteId { Page = PageCache.Get( r.PageId ), RouteId = r.RouteId } ).Where( p => p.Page != null );
            }
            else
            {
                return new List<PageCacheAndRouteId>();
            }
        }

        private IEnumerable<PageShortLink> GetShortLinksForRoute( RequestContext requestContext, RockContext rockContext = null )
        {
            if ( rockContext == null ) rockContext = new RockContext();

            // Get the shortlink
            string shortlink = (string)requestContext.RouteData.Values["shortlink"];

            // The shortlink might have gotten matched as if it were a route, so test the route name too
            if ( string.IsNullOrWhiteSpace( shortlink ) ) shortlink = (string)requestContext.RouteData.DataTokens["RouteName"];

            if ( !string.IsNullOrWhiteSpace( shortlink ) )
            {

                return new PageShortLinkService( rockContext ).Queryable().Where( l => l.Token == shortlink ).ToList();

            }
            else
            {
                return new List<PageShortLink>();
            }
        }

        // Site Functions

        private SiteCache GetSiteByQueryString( RequestContext requestContext )
        {
            int? query_SiteId = requestContext.HttpContext.Request.QueryString["SiteId"].AsIntegerOrNull();
            if ( query_SiteId.HasValue )
            {
                return SiteCache.Get( query_SiteId.Value );
            }
            return null;
        }

        private SiteCache GetSiteByDomainName( RequestContext requestContext )
        {
            return SiteCache.GetSiteByDomain( requestContext.HttpContext.Request.Url.Host );
        }

        private SiteCache GetSiteFromLastSite( RequestContext requestContext )
        {
            var siteCookie = requestContext.HttpContext.Request.Cookies["last_site"];
            if ( siteCookie != null && siteCookie.Value != null )
            {
                return SiteCache.Get( siteCookie.Value.AsInteger() );
            }
            return null;
        }

        private SiteCache GetDefaultSite()
        {
            return SiteCache.Get( SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid() );
        }

        // Handlers

        private System.Web.IHttpHandler GetHandlerForPage( RequestContext requestContext, PageCache page, int routeId = 0, bool checkMobile = true )
        {
            var site = page.Layout.Site;

            // Check for a mobile redirect on the site
            if ( checkMobile && site.EnableMobileRedirect )
            {
                var clientType = InteractionDeviceType.GetClientType( requestContext.HttpContext.Request.UserAgent );
                
                if ( clientType == "Mobile" || ( site.RedirectTablets && clientType == "Tablet" ) )
                {
                    if ( site.MobilePageId.HasValue )
                    {
                        var mobilePage = PageCache.Get( site.MobilePageId.Value );
                        if(mobilePage != null)
                        {
                            return GetHandlerForPage( requestContext, mobilePage, routeId, false );
                        }

                    }
                    else if ( !string.IsNullOrWhiteSpace( site.ExternalUrl ) )
                    {
                        requestContext.HttpContext.Response.Redirect( site.ExternalUrl );
                        return null;
                    }
                }
            }

            // Set the last site cookie
            var siteCookie = requestContext.HttpContext.Request.Cookies["last_site"];
            if ( siteCookie == null )
            {
                siteCookie = new System.Web.HttpCookie( "last_site", page.Layout.SiteId.ToString() );
            }
            else
            {
                siteCookie.Value = page.Layout.SiteId.ToString();
            }
            requestContext.HttpContext.Response.SetCookie( siteCookie );

            // Get the Layout & Theme Details
            string theme = page.Layout.Site.Theme;
            string layout = page.Layout.FileName;
            string layoutPath = PageCache.FormatPath( theme, layout );

            // Get any route parameters
            var parms = new Dictionary<string, string>();
            foreach ( var routeParm in requestContext.RouteData.Values )
            {
                parms.Add( routeParm.Key, (string)routeParm.Value );
            }

            try
            {
                // Return the page for the selected theme and layout
                Rock.Web.UI.RockPage cmsPage = (Rock.Web.UI.RockPage)BuildManager.CreateInstanceFromVirtualPath( layoutPath, typeof( Rock.Web.UI.RockPage ) );
                cmsPage.SetPage( page );
                cmsPage.PageReference = new PageReference( page.Id, routeId, parms, requestContext.HttpContext.Request.QueryString );
                return cmsPage;
            }
            catch ( System.Web.HttpException )
            {
                // The Selected theme and/or layout didn't exist, attempt first to use the layout in the default theme.
                theme = "Rock";

                // If not using the default layout, verify that Layout exists in the default theme directory
                if ( layout != "FullWidth" &&
                    !File.Exists( requestContext.HttpContext.Server.MapPath( string.Format( "~/Themes/Rock/Layouts/{0}.aspx", layout ) ) ) )
                {
                    // If selected layout doesn't exist in the default theme, switch to the Default layout
                    layout = "FullWidth";
                }

                // Build the path to the aspx file to
                layoutPath = PageCache.FormatPath( theme, layout );

                // Return the default layout and/or theme
                Rock.Web.UI.RockPage cmsPage = (Rock.Web.UI.RockPage)BuildManager.CreateInstanceFromVirtualPath( layoutPath, typeof( Rock.Web.UI.RockPage ) );
                cmsPage.SetPage( page );
                cmsPage.PageReference = new PageReference( page.Id, routeId, parms, requestContext.HttpContext.Request.QueryString );
                return cmsPage;
            }
        }

        private System.Web.IHttpHandler GetHandlerFor404( RequestContext requestContext, SiteCache site )
        {
            // If we couldn't match a route because it's the root, use the home page
            if ( site != null && requestContext.HttpContext.Request.Path == "/" ) return GetHandlerForPage( requestContext, site.DefaultPage, site.DefaultPageRouteId ?? 0 );

            if ( site != null && site.PageNotFoundPageId.HasValue )
            {
                if ( Convert.ToBoolean( GlobalAttributesCache.Get().GetValue( "Log404AsException" ) ) )
                {
                    Rock.Model.ExceptionLogService.LogException(
                        new Exception( string.Format( "404 Error: {0}", requestContext.HttpContext.Request.Url.AbsoluteUri ) ),
                        requestContext.HttpContext.ApplicationInstance.Context );
                }

                var page = PageCache.Get( site.PageNotFoundPageId.Value );
                requestContext.HttpContext.Response.StatusCode = 404;
                requestContext.HttpContext.Response.TrySkipIisCustomErrors = true;

                return GetHandlerForPage( requestContext, page );
            }
            else
            {
                // no 404 page found for the site, return the default 404 error page
                return (System.Web.UI.Page)BuildManager.CreateInstanceFromVirtualPath( "~/Http404Error.aspx", typeof( System.Web.UI.Page ) );
            }
        }

        private System.Web.IHttpHandler GetHandlerForShortLink( RequestContext requestContext, PageShortLink pageShortLink )
        {
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                string trimmedUrl = pageShortLink.Url.RemoveCrLf().Trim();
                
                RockQueue.TransactionQueue.Enqueue( new ShortLinkTransaction
                {
                    PageShortLinkId = pageShortLink.Id,
                    Token = pageShortLink.Token,
                    Url = trimmedUrl,
                    UserName = requestContext.HttpContext.User?.Identity?.Name,
                    DateViewed = RockDateTime.Now,
                    IPAddress = UI.RockPage.GetClientIpAddress( requestContext.HttpContext.Request ),
                    UserAgent = requestContext.HttpContext.Request.UserAgent ?? ""
                } );

                requestContext.HttpContext.Response.Redirect( trimmedUrl );

                return null;
            }
        }


        /// <summary>
        /// Determine the logical page being requested by evaluating the routedata, or querystring and
        /// then loading the appropriate layout (ASPX) page
        /// </summary>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        System.Web.IHttpHandler IRouteHandler.GetHttpHandler( RequestContext requestContext )
        {
            if ( requestContext == null )
            {
                throw new ArgumentNullException( "requestContext" );
            }

            try
            {



                /*
                 *
                 *
                 * Need to determine:
                 * 1. The Site
                 * 2. The Page OR Shortlink
                 *
                 * URL Formats:
                 * 1. domain/page/{PageId}
                 * 2. domain/route
                 * 3. domain/shortlink
                 *
                 * We can get the Site from:
                 * 1. The "SiteId" query string
                 * 2. The domain
                 *
                 * We can infer the Site from:
                 * 1. The "last_site" cookie
                 * 2. The default site
                 *
                 * We can get the Page from:
                 * 1. The Page Id
                 * 
                 * We can infer the Page from:
                 * 1. The first matching site + route
                 * 2. The first matching site + shortlink
                 * 3. The Site's default page
                 * 4. The first matching route
                 * 5. The first matching shortlink
                 *
                 * What are the possibilities?
                 * 
                 * - site id + page id
                 * - site id + route
                 * - site id + shortlink
                 * - domain + page id
                 * - domain + route
                 * - domain + shortlink
                 * - last site + page id
                 * - last site + route
                 * - last site + shortlink
                 * - default site + page id
                 * - default site + route
                 * - default site + shortlink
                 * - page id
                 * - route
                 * - shortlink
                 *
                 *
                 * How are we prioritizing them?
                 * - If site id or domain (strict matching mode)
                 *   - domain + page id
                 *   - domain + route
                 *   - domain + shortlink
                 *   - site id + page id
                 *   - site id + route
                 *   - site id + shortlink
                 *   - any site + page id + dialog layout
                 *   - any site + route + dialog layout
                 * - Else (loose matching mode)
                 *   - any site + page id
                 *   - last site + route
                 *   - last site + shortlink
                 *   - default site + shortlink
                 *   - default site + route
                 *   - any site + route
                 *   - any site + shortlink
                 *
                 * Note: page id can't be restricted by matched site because that breaks Block Properties dialogs
                 * Note 2: restricting routes breaks the Child Pages dialog, grrr
                 * Let's try prioritizing layout type == Dialog
                 *
                 *
                 */

                // Get page id
                var defaultRoutePage = GetPageForDefaultRoute( requestContext );


                // Get possible routes
                var routePages = GetPagesForRoute( requestContext );

                // Get possible shortlinks
                var routeShortLinks = GetShortLinksForRoute( requestContext );


                // domain
                var domainSite = GetSiteByDomainName( requestContext );
                if ( domainSite != null )
                {

                    // domain + page id
                    if ( defaultRoutePage != null && domainSite.Id == defaultRoutePage.SiteId) return GetHandlerForPage( requestContext, defaultRoutePage );

                    // domain + route
                    var domainSiteRoutePage = routePages.Where( p => domainSite.Id == p.Page.SiteId ).FirstOrDefault();
                    if ( domainSiteRoutePage != null ) return GetHandlerForPage( requestContext, domainSiteRoutePage.Page, domainSiteRoutePage.RouteId );

                    // domain + shortlink
                    var domainSiteRouteShortLink = routeShortLinks.Where( l => domainSite.Id == l.SiteId ).FirstOrDefault();
                    if ( domainSiteRouteShortLink != null ) return GetHandlerForShortLink( requestContext, domainSiteRouteShortLink );

                }


                // site id
                var querySite = GetSiteByQueryString( requestContext );
                if ( querySite != null )
                {

                    // site id + page id
                    if ( defaultRoutePage != null && querySite.Id == defaultRoutePage.SiteId ) return GetHandlerForPage( requestContext, defaultRoutePage );

                    // site id + route
                    var querySiteRoutePage = routePages.Where( p => querySite.Id == p.Page.SiteId ).FirstOrDefault();
                    if ( querySiteRoutePage != null ) return GetHandlerForPage( requestContext, querySiteRoutePage.Page, querySiteRoutePage.RouteId );

                    // site id + shortlink
                    var querySiteRouteShortLink = routeShortLinks.Where( l => querySite.Id == l.SiteId ).FirstOrDefault();
                    if ( querySiteRouteShortLink != null ) return GetHandlerForShortLink( requestContext, querySiteRouteShortLink );

                }


                // Match pages and routes with dialog layouts so dialogs still work properly

                // any site + page id + dialog layout
                if ( defaultRoutePage != null && defaultRoutePage.Layout.Name == "Dialog" ) return GetHandlerForPage( requestContext, defaultRoutePage );

                // any site + route + dialog layout
                var anySiteRouteDialogPage = routePages.Where( p => p.Page.Layout.Name == "Dialog" ).FirstOrDefault();
                if ( anySiteRouteDialogPage != null ) return GetHandlerForPage( requestContext, anySiteRouteDialogPage.Page, anySiteRouteDialogPage.RouteId );


                // Strict matching for site id or domain matches (We don't want routes from one site to be accessible from all others - It makes a complete mess of SEO)
                if ( domainSite != null || querySite != null ) return GetHandlerFor404( requestContext, domainSite ?? querySite );


                // any site + page id
                if ( defaultRoutePage != null ) return GetHandlerForPage( requestContext, defaultRoutePage );

                // last site
                var lastSite = GetSiteFromLastSite( requestContext );
                if ( lastSite != null )
                {

                    // last site + route
                    var lastSiteRoutePage = routePages.Where( p => lastSite.Id == p.Page.SiteId ).FirstOrDefault();
                    if ( lastSiteRoutePage != null ) return GetHandlerForPage( requestContext, lastSiteRoutePage.Page, lastSiteRoutePage.RouteId );

                    // last site + shortlink
                    var lastSiteRouteShortLink = routeShortLinks.Where( l => lastSite.Id == l.SiteId ).FirstOrDefault();
                    if ( lastSiteRouteShortLink != null ) return GetHandlerForShortLink( requestContext, lastSiteRouteShortLink );

                }


                // default site
                var defaultSite = GetDefaultSite();
                if ( defaultSite != null )
                {

                    // default site + route
                    var defaultSiteRoutePage = routePages.Where( p => defaultSite.Id == p.Page.SiteId ).FirstOrDefault();
                    if ( defaultSiteRoutePage != null ) return GetHandlerForPage( requestContext, defaultSiteRoutePage.Page, defaultSiteRoutePage.RouteId );

                    // default site + shortlink
                    var defaultSiteRouteShortLink = routeShortLinks.Where( l => defaultSite.Id == l.SiteId ).FirstOrDefault();
                    if ( defaultSiteRouteShortLink != null ) return GetHandlerForShortLink( requestContext, defaultSiteRouteShortLink );

                }

                // any site + route
                var firstRoutePage = routePages.FirstOrDefault();
                if ( firstRoutePage != null ) return GetHandlerForPage( requestContext, firstRoutePage.Page, firstRoutePage.RouteId );

                // any site + shortlink
                var firstRouteShortLink = routeShortLinks.FirstOrDefault();
                if ( firstRouteShortLink != null ) return GetHandlerForShortLink( requestContext, firstRouteShortLink );

                // If we got this far without any matches, do a 404
                return GetHandlerFor404( requestContext, lastSite ?? defaultSite );
                
            }
            catch ( Exception ex )
            {
                if ( requestContext.HttpContext != null )
                {
                    requestContext.HttpContext.Cache["RockExceptionOrder"] = "66";
                    requestContext.HttpContext.Cache["RockLastException"] = ex;
                }

                System.Web.UI.Page errorPage = (System.Web.UI.Page)BuildManager.CreateInstanceFromVirtualPath( "~/Error.aspx", typeof( System.Web.UI.Page ) );
                return errorPage;
            }
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
