using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Web.Compilation;
using System.Web.Routing;
using System.Web.UI;
using System.IO;

namespace Rock.Web
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

                Rock.Web.Cache.Site site = null;
                if ( sites.ContainsKey( host ) )
                    site = Rock.Web.Cache.Site.Read( sites[host] );
                else
                {
                    Rock.CMS.SiteDomainService siteDomainService = new Rock.CMS.SiteDomainService();
                    Rock.CMS.SiteDomain siteDomain = siteDomainService.GetByDomain( requestContext.HttpContext.Request.Url.Host );
                    if ( siteDomain != null )
                    {
                        sites.Add( host, siteDomain.SiteId );
                        site = Rock.Web.Cache.Site.Read( siteDomain.SiteId );
                    }
                }

                if ( site != null && site.DefaultPageId.HasValue )
                    pageId = site.DefaultPageId.Value.ToString();

                cache[cacheKey] = sites;
			}

            Rock.Web.Cache.Page page = null;
			
			if ( ! string.IsNullOrEmpty( pageId ) )
				page = Rock.Web.Cache.Page.Read( Convert.ToInt32( pageId ) );

            if ( page == null )
                throw new SystemException( "Invalid Site Configuration" );

            if ( page != null && !String.IsNullOrEmpty( page.LayoutPath ) )
            {
                // load the route id
                page.RouteId = routeId;

                // Return the page using the cached route
                Rock.Web.UI.Page cmsPage = ( Rock.Web.UI.Page )BuildManager.CreateInstanceFromVirtualPath( page.LayoutPath, typeof( Rock.Web.UI.Page ) );
                cmsPage.PageInstance = page;
                return cmsPage;
            }
            else
            {
                string theme = "RockCMS";
                string layout = "Default";
                string layoutPath = Rock.Web.Cache.Page.FormatPath( theme, layout );

                if ( page != null )
                {
                    // load the route id
                    page.RouteId = routeId;

                    theme = page.Site.Theme;
                    layout = page.Layout;
                    layoutPath = Rock.Web.Cache.Page.FormatPath( theme, layout );

                    page.LayoutPath = layoutPath;
                }
                else
                    page = Cache.Page.Read( new CMS.Page() );

                try
                {
                    // Return the page for the selected theme and layout
                    Rock.Web.UI.Page cmsPage = ( Rock.Web.UI.Page )BuildManager.CreateInstanceFromVirtualPath( layoutPath, typeof( Rock.Web.UI.Page ) );
                    cmsPage.PageInstance = page;
                    return cmsPage;
                }
                catch ( System.Web.HttpException )
                {
                    // The Selected theme and/or layout didn't exist, attempt first to use the default layout in the selected theme
                    layout = "Default";

                    // If not using the Rock theme, verify that default Layout exists in the selected theme directory
                    if ( theme != "RockCMS" &&
                        !File.Exists( requestContext.HttpContext.Server.MapPath( string.Format( "~/Themes/{0}/Layouts/Default.aspx", theme ) ) ) )
                    {
                        // If default layout doesn't exist in the selected theme, switch to the Default layout
                        theme = "RockCMS";
                        layout = "Default";
                    }

                    // Build the path to the aspx file to
                    layoutPath = Rock.Web.Cache.Page.FormatPath( theme, layout );

                    if ( page != null )
                        page.LayoutPath = layoutPath;

                    // Return the default layout and/or theme
                    Rock.Web.UI.Page cmsPage = ( Rock.Web.UI.Page )BuildManager.CreateInstanceFromVirtualPath( layoutPath, typeof( Rock.Web.UI.Page ) );
                    cmsPage.PageInstance = page;
                    return cmsPage;
                }
            }
        }
    }
}
