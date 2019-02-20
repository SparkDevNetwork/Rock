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
using System.Web;
using Rock.Utility;

namespace Rock.Web
{
    /// <summary>
    /// Rock custom route handler
    /// </summary>
    public sealed class RockRouteHandler : IRouteHandler
    {
        private RequestContext RouteRequestContext { get; set; }

        private string PageId { get; set; }

        private int RouteId { get; set; }

        private bool IsSiteMatch { get; set; }

        private Dictionary<string, string> Parms { get; set; }

        private string Host { get; set; }

        private HttpRequestBase RouteHttpRequest { get; set; }

        private HttpCookie SiteCookie { get; set; }

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
            // Context cannot be null
            if ( requestContext == null )
            {
                throw new ArgumentNullException( "requestContext" );
            }

            RouteRequestContext = requestContext;

            PageId = string.Empty;
            RouteId = 0;
            IsSiteMatch = false;

            try
            {
                RouteHttpRequest = RouteRequestContext.HttpContext.Request;
                SiteCookie = RouteHttpRequest.Cookies["last_site"];
                Parms = new Dictionary<string, string>();
                Host = WebRequestHelper.GetHostNameFromRequest( HttpContext.Current );

                if ( RouteRequestContext.RouteData.Values["PageId"] != null )
                {
                    // Pages using the default routing URL will have the page id in the RouteData.Values collection
                    PageId = ( string ) RouteRequestContext.RouteData.Values["PageId"];
                    IsSiteMatch = true;
                }
                else if ( RouteRequestContext.RouteData.DataTokens["PageRoutes"] != null )
                {
                    // Pages that use a custom URL route will have the page id in the RouteData.DataTokens collection
                    GetPageIdFromDataTokens();

                    foreach ( var routeParm in RouteRequestContext.RouteData.Values )
                    {
                        Parms.Add( routeParm.Key, ( string ) routeParm.Value );
                    }
                }
                else if ( ( ( System.Web.Routing.Route ) RouteRequestContext.RouteData.Route ).Url.IsNullOrWhiteSpace() )
                {
                    // if we don't have routing info then set the page ID to the default page for the site.

                    // Get the site, if not found use the default site
                    SiteCache site = GetSite();
                    if ( site == null )
                    {
                        site = SiteCache.Get( SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid() );
                    }

                    if ( site.DefaultPageId.HasValue )
                    {
                        PageId = site.DefaultPageId.Value.ToString();
                        IsSiteMatch = true;
                    }
                    else
                    {
                        throw new SystemException( "Invalid Site Configuration" );
                    }
                }

                // If the the page ID and site has not yet been matched
                if ( string.IsNullOrEmpty( PageId ) || !IsSiteMatch )
                {
                    SiteCache site = GetSite();

                    // if not found use the default site
                    if ( site == null )
                    {
                        site = SiteCache.Get( SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid() );
                    }

                    if ( site != null )
                    {
                        // Check to see if this is a short link route
                        string shortlink = null;
                        if ( RouteRequestContext.RouteData.Values.ContainsKey( "shortlink" ) )
                        {
                            shortlink = RouteRequestContext.RouteData.Values["shortlink"].ToString();
                        }

                        // If shortlink have the same name as route and route's site did not match, then check if shortlink site match.
                        if ( shortlink.IsNullOrWhiteSpace() && RouteRequestContext.RouteData.DataTokens["RouteName"] != null )
                        {
                            shortlink = RouteRequestContext.RouteData.DataTokens["RouteName"].ToString();
                        }

                        if ( shortlink.IsNotNullOrWhiteSpace() )
                        {
                            using ( var rockContext = new Rock.Data.RockContext() )
                            {
                                var pageShortLink = new PageShortLinkService( rockContext ).GetByToken( shortlink, site.Id );

                                if ( pageShortLink != null && ( pageShortLink.SiteId == site.Id || RouteRequestContext.RouteData.DataTokens["RouteName"] == null ) )
                                {
                                    PageId = string.Empty;
                                    RouteId = 0;

                                    string trimmedUrl = pageShortLink.Url.RemoveCrLf().Trim();

                                    var transaction = new ShortLinkTransaction();
                                    transaction.PageShortLinkId = pageShortLink.Id;
                                    transaction.Token = pageShortLink.Token;
                                    transaction.Url = trimmedUrl;
                                    if ( RouteRequestContext.HttpContext.User != null )
                                    {
                                        transaction.UserName = RouteRequestContext.HttpContext.User.Identity.Name;
                                    }

                                    transaction.DateViewed = RockDateTime.Now;
                                    transaction.IPAddress = WebRequestHelper.GetClientIpAddress( RouteHttpRequest );
                                    transaction.UserAgent = RouteHttpRequest.UserAgent ?? string.Empty;
                                    RockQueue.TransactionQueue.Enqueue( transaction );

                                    RouteRequestContext.HttpContext.Response.Redirect( trimmedUrl );
                                    return null;
                                }
                            }
                        }

                        // If site has has been enabled for mobile redirect, then we'll need to check what type of device is being used
                        if ( site.EnableMobileRedirect )
                        {
                            // get the device type
                            string u = RouteHttpRequest.UserAgent;

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
                                    PageId = site.MobilePageId.Value.ToString();
                                    RouteId = 0;
                                }
                                else if ( !string.IsNullOrWhiteSpace( site.ExternalUrl ) )
                                {
                                    RouteRequestContext.HttpContext.Response.Redirect( site.ExternalUrl );
                                    return null;
                                }
                            }
                        }
                    }
                }

                PageCache page = null;
                if ( !string.IsNullOrEmpty( PageId ) )
                {
                    int pageIdNumber = 0;
                    if ( int.TryParse( PageId, out pageIdNumber ) )
                    {
                        page = PageCache.Get( pageIdNumber );
                    }
                }

                if ( page == null )
                {
                    // try to get site's 404 page
                    SiteCache site = GetSite();
                    if ( site != null && site.PageNotFoundPageId != null )
                    {
                        if ( Convert.ToBoolean( GlobalAttributesCache.Get().GetValue( "Log404AsException" ) ) )
                        {
                            Rock.Model.ExceptionLogService.LogException(
                                new Exception( $"404 Error: {RouteHttpRequest.Url.AbsoluteUri}" ),
                                RouteRequestContext.HttpContext.ApplicationInstance.Context );
                        }

                        page = PageCache.Get( site.PageNotFoundPageId ?? 0 );
                        RouteRequestContext.HttpContext.Response.StatusCode = 404;
                        RouteRequestContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                    }
                    else
                    {
                        // no 404 page found for the site, return the default 404 error page
                        return ( System.Web.UI.Page ) BuildManager.CreateInstanceFromVirtualPath( "~/Http404Error.aspx", typeof( System.Web.UI.Page ) );
                    }
                }

                CreateOrUpdateSiteCookie( page );

                string theme = page.Layout.Site.Theme;
                string layout = page.Layout.FileName;
                string layoutPath = PageCache.FormatPath( theme, layout );
                
                try
                {
                    return CreateRockPage( page, layoutPath );
                }
                catch ( System.Web.HttpException )
                {
                    // The Selected theme and/or layout didn't exist so try to use the layout in the default theme.
                    theme = "Rock";

                    // Verify that Layout exists in the default theme directory and if not try use the default layout of the default theme
                    string layoutPagePath = string.Format( "~/Themes/Rock/Layouts/{0}.aspx", layout );
                    if ( !File.Exists( RouteRequestContext.HttpContext.Server.MapPath( layoutPagePath ) ) )
                    {
                        layout = "FullWidth";
                    }

                    layoutPath = PageCache.FormatPath( theme, layout );

                    return CreateRockPage( page, layoutPath );
                }
            }
            catch ( Exception ex )
            {
                if ( RouteRequestContext.HttpContext != null )
                {
                    RouteRequestContext.HttpContext.Cache["RockExceptionOrder"] = "66";
                    RouteRequestContext.HttpContext.Cache["RockLastException"] = ex;
                }

                return ( System.Web.UI.Page ) BuildManager.CreateInstanceFromVirtualPath( "~/Error.aspx", typeof( System.Web.UI.Page ) );
            }
        }

        /// <summary>
        /// Uses the DataTokens to contstruct a list of PageAndRouteIds
        /// If any exist then set page and route ID to the first one found.
        /// Then loop through the collection looking for a site match, if one is found then set the page and route ID to that
        /// and the IsSiteMatch property to true.
        /// </summary>
        private void GetPageIdFromDataTokens()
        {
            // Pages that use a custom URL route will have the page id in the RouteData.DataTokens collection
            List<PageAndRouteId> pageAndRouteIds = RouteRequestContext.RouteData.DataTokens["PageRoutes"] as List<PageAndRouteId>;

            if ( pageAndRouteIds != null && pageAndRouteIds.Any() )
            {
                // Default to first site/page
                var pageAndRouteIdDefault = pageAndRouteIds.First();
                PageId = pageAndRouteIdDefault.PageId.ToJson();
                RouteId = pageAndRouteIdDefault.RouteId;

                // Then check to see if any can be matched by site
                SiteCache site = GetSite();

                if ( site == null )
                {
                    return;
                }

                foreach ( var pageAndRouteId in pageAndRouteIds )
                {
                    var pageCache = PageCache.Get( pageAndRouteId.PageId );
                    if ( pageCache != null && pageCache.Layout != null && pageCache.Layout.SiteId == site.Id )
                    {
                        PageId = pageAndRouteId.PageId.ToJson();
                        RouteId = pageAndRouteId.RouteId;
                        IsSiteMatch = true;
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
        private SiteCache GetSite()
        {
            SiteCache site = null;

            // First check to see if site was specified in querystring
            int? siteId = RouteHttpRequest.QueryString["SiteId"].AsIntegerOrNull();
            if ( siteId.HasValue )
            {
                site = SiteCache.Get( siteId.Value );
            }

            // Then check to see if site can be determined by domain
            if ( site == null )
            {
                site = SiteCache.GetSiteByDomain( Host );
            }

            // Then check the last site
            if ( site == null )
            {
                if ( SiteCookie != null && SiteCookie.Value != null )
                {
                    site = SiteCache.Get( SiteCookie.Value.AsInteger() );
                }
            }

            return site;
        }

        /// <summary>
        /// Creates the or updates site cookie with the last_site value using the site ID of the page
        /// </summary>
        /// <param name="page">The page.</param>
        private void CreateOrUpdateSiteCookie( PageCache page )
        {
            if ( SiteCookie == null )
            {
                SiteCookie = new System.Web.HttpCookie( "last_site", page.Layout.SiteId.ToString() );
            }
            else
            {
                SiteCookie.Value = page.Layout.SiteId.ToString();
            }

            RouteRequestContext.HttpContext.Response.SetCookie( SiteCookie );
        }

        /// <summary>
        /// Creates the rock page for the selected theme and layout.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="layoutPath">The layout path.</param>
        /// <returns></returns>
        private Rock.Web.UI.RockPage CreateRockPage( PageCache page, string layoutPath )
        {
            // Return the page for the selected theme and layout
            Rock.Web.UI.RockPage cmsPage = ( Rock.Web.UI.RockPage ) BuildManager.CreateInstanceFromVirtualPath( layoutPath, typeof( Rock.Web.UI.RockPage ) );
            cmsPage.SetPage( page );
            cmsPage.PageReference = new PageReference( page.Id, RouteId, Parms, RouteHttpRequest.QueryString );
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
