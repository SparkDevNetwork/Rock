//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using System.Web.Compilation;
using System.Web.Routing;

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
        /// </summary>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        System.Web.IHttpHandler IRouteHandler.GetHttpHandler( RequestContext requestContext )
        {
            if ( requestContext == null )
                throw new ArgumentNullException( "requestContext" );

            string pageId = "";
            int routeId = -1;

            // Pages using the default routing URL will have the page id in the RouteData.Values collection
            if ( requestContext.RouteData.Values["PageId"] != null )
            {
                pageId = (string)requestContext.RouteData.Values["PageId"];
            }
            // Pages that use a custom URL route will have the page id in the RouteDate.DataTokens collection
            else if ( requestContext.RouteData.DataTokens["PageId"] != null )
            {
                pageId = (string)requestContext.RouteData.DataTokens["PageId"];
                routeId = Int32.Parse( (string)requestContext.RouteData.DataTokens["RouteId"] );
            }
            // If page has not been specified get the site by the domain and use the site's default page
            else
            {
                string host = requestContext.HttpContext.Request.Url.Host;
                string cacheKey = "Rock:DomainSites";

                ObjectCache cache = MemoryCache.Default;
                Dictionary<string, int> sites = cache[cacheKey] as Dictionary<string, int>;
                if ( sites == null )
                    sites = new Dictionary<string, int>();

                Rock.Web.Cache.SiteCache site = null;
                if ( sites.ContainsKey( host ) )
                    site = Rock.Web.Cache.SiteCache.Read( sites[host] );
                else
                {
                    int siteId = 1;

                    Rock.Model.SiteDomainService siteDomainService = new Rock.Model.SiteDomainService();
                    Rock.Model.SiteDomain siteDomain = siteDomainService.GetByDomainContained( requestContext.HttpContext.Request.Url.Host );
                    if ( siteDomain != null )
                    {
                        siteId = siteDomain.SiteId;
                    }
                    else
                    {
                        var siteService = new Rock.Model.SiteService();
                        var rockSite = siteService.Get( new Guid( SystemGuid.Site.SITE_ROCK_CHMS ) );
                        if ( rockSite != null )
                        {
                            siteId = rockSite.Id;
                        }
                    }

                    sites.Add( host, siteId );
                    site = Rock.Web.Cache.SiteCache.Read( siteId );

                }

                cache[cacheKey] = sites;

                if ( site != null && site.DefaultPageId.HasValue )
                    pageId = site.DefaultPageId.Value.ToString();

                if ( string.IsNullOrEmpty( pageId ) )
                    throw new SystemException( "Invalid Site Configuration" );
            }

            Rock.Web.Cache.PageCache page = null;

            if ( !string.IsNullOrEmpty( pageId ) )
            {
                int pageIdNumber = 0;
                if ( Int32.TryParse( pageId, out pageIdNumber ) )
                {
                    page = Rock.Web.Cache.PageCache.Read( pageIdNumber );
                }

                if ( page == null )
                {
                    return new HttpHandlerError( 404 );
                }
            }

            string theme = "RockChMS";
            string layout = "Default";
            string layoutPath = Rock.Web.Cache.PageCache.FormatPath( theme, layout );

            if ( page != null )
            {
                // load the route id
                page.RouteId = routeId;

                theme = page.Site.Theme;
                layout = page.Layout;
                layoutPath = Rock.Web.Cache.PageCache.FormatPath( theme, layout );
            }
            else
            {
                page = Cache.PageCache.Read( new Model.Page() );
            }

            try
            {
                // Return the page for the selected theme and layout
                Rock.Web.UI.RockPage cmsPage = (Rock.Web.UI.RockPage)BuildManager.CreateInstanceFromVirtualPath( layoutPath, typeof( Rock.Web.UI.RockPage ) );
                cmsPage.CurrentPage = page;
                return cmsPage;
            }
            catch ( System.Web.HttpException )
            {
                // The Selected theme and/or layout didn't exist, attempt first to use the layout in the default theme.
                theme = "RockChMS";
                
                // If not using the default layout, verify that Layout exists in the default theme directory
                if ( layout != "Default" &&
                    !File.Exists( requestContext.HttpContext.Server.MapPath( string.Format( "~/Themes/RockChMS/Layouts/{0}.aspx", layout ) ) ) )
                {
                    // If selected layout doesn't exist in the default theme, switch to the Default layout
                    layout = "Default";
                }

                // Build the path to the aspx file to
                layoutPath = Rock.Web.Cache.PageCache.FormatPath( theme, layout );

                // Return the default layout and/or theme
                Rock.Web.UI.RockPage cmsPage = (Rock.Web.UI.RockPage)BuildManager.CreateInstanceFromVirtualPath( layoutPath, typeof( Rock.Web.UI.RockPage ) );
                cmsPage.CurrentPage = page;
                return cmsPage;
            }
        }
    }

    /// <summary>
    /// Handler used when an error occurrs
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
