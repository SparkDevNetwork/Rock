using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Web.Compilation;
using System.Web.Routing;
using System.Web.UI;
using System.IO;

namespace Rock.Cms
{
    /// <summary>
    /// Rock custom route handler
    /// </summary>
    public class RockRouteHandler : IRouteHandler
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

                Rock.Cms.Cached.Site site;
                if ( sites.ContainsKey( host ) )
                    site = Rock.Cms.Cached.Site.Read( sites[host] );
                else
                {
                    Rock.Services.Cms.SiteDomainService siteDomainService = new Rock.Services.Cms.SiteDomainService();
                    Rock.Models.Cms.SiteDomain siteDomain = siteDomainService.GetByDomain( requestContext.HttpContext.Request.Url.Host );
                    if ( siteDomain != null )
                        sites.Add( host, siteDomain.SiteId );
                    site = Rock.Cms.Cached.Site.Read( siteDomain.SiteId );
                }

                if ( site != null && site.DefaultPageId.HasValue )
                    pageId = site.DefaultPageId.Value.ToString();

                cache[cacheKey] = sites;
			}

            Rock.Cms.Cached.Page page = null;
			
			if ( ! string.IsNullOrEmpty( pageId ) )
			{
				page = Rock.Cms.Cached.Page.Read( Convert.ToInt32( pageId ) );
			}

            if ( page != null && !String.IsNullOrEmpty( page.LayoutPath ) )
            {
                // load the route id
                page.RouteId = routeId;

                // Return the page using the cached route
                CmsPage cmsPage = ( CmsPage )BuildManager.CreateInstanceFromVirtualPath( page.LayoutPath, typeof( CmsPage ) );
                cmsPage.PageInstance = page;
                return cmsPage;
            }
            else
            {
                string theme = "Default";
                string layout = "Default";
                string layoutPath = Rock.Cms.Cached.Page.FormatPath( theme, layout );

                if ( page != null )
                {
                    // load the route id
                    page.RouteId = routeId;

                    theme = page.Site.Theme;
                    layout = page.Layout;
                    layoutPath = Rock.Cms.Cached.Page.FormatPath( theme, layout );

                    page.LayoutPath = layoutPath;
                }

                try
                {
                    // Return the page for the selected theme and layout
                    CmsPage cmsPage = ( CmsPage )BuildManager.CreateInstanceFromVirtualPath( layoutPath, typeof( CmsPage ) );
                    cmsPage.PageInstance = page;
                    return cmsPage;
                }
                catch ( System.Web.HttpException )
                {
                    // The Selected theme and/or layout didn't exist, attempt first to use the default layout in the selected theme
                    layout = "Default";

                    // If not using the default theme, verify that default Layout exists in the selected theme directory
                    if ( theme != "Default" &&
                        !File.Exists( requestContext.HttpContext.Server.MapPath( string.Format( "~/Themes/{0}/Layouts/Default.aspx", theme ) ) ) )
                    {
                        // If default layout doesn't exist in the selected theme, switch to the default theme and us it's default layout
                        theme = "Default";
                    }

                    // Build the path to the aspx file to
                    layoutPath = Rock.Cms.Cached.Page.FormatPath( theme, layout );

                    if ( page != null )
                        page.LayoutPath = layoutPath;

                    // Return the default layout and/or theme
                    CmsPage cmsPage = ( CmsPage )BuildManager.CreateInstanceFromVirtualPath( layoutPath, typeof( CmsPage ) );
                    cmsPage.PageInstance = page;
                    return cmsPage;
                }
            }
        }
    }
}
