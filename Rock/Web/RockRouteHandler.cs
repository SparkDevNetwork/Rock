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
            {
                throw new ArgumentNullException( "requestContext" );
            }

            try
            {
                string pageId = "";
                int routeId = 0;

                var parms = new Dictionary<string, string>();

                // Pages using the default routing URL will have the page id in the RouteData.Values collection
                if ( requestContext.RouteData.Values["PageId"] != null )
                {
                    pageId = (string)requestContext.RouteData.Values["PageId"];
                }

                // Pages that use a custom URL route will have the page id in the RouteDate.DataTokens collection
                else if ( requestContext.RouteData.DataTokens["PageRoutes"] != null )
                {
                    var pageAndRouteIds = requestContext.RouteData.DataTokens["PageRoutes"] as List<PageAndRouteId>;
                    if ( pageAndRouteIds != null && pageAndRouteIds.Count > 0 )
                    {
                        if ( pageAndRouteIds.Count == 1 )
                        {
                            var pageAndRouteId = pageAndRouteIds.First();
                            pageId = pageAndRouteId.PageId.ToJson();
                            routeId = pageAndRouteId.RouteId;
                        }
                        else
                        {
                            SiteCache site = SiteCache.GetSiteByDomain( requestContext.HttpContext.Request.Url.Host );
                            if ( site != null )
                            {
                                foreach ( var pageAndRouteId in pageAndRouteIds )
                                {
                                    var pageCache = PageCache.Read( pageAndRouteId.PageId );
                                    if ( pageCache != null && pageCache.Layout != null && pageCache.Layout.SiteId == site.Id )
                                    {
                                        pageId = pageAndRouteId.PageId.ToJson();
                                        routeId = pageAndRouteId.RouteId;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    foreach ( var routeParm in requestContext.RouteData.Values )
                    {
                        parms.Add( routeParm.Key, (string)routeParm.Value );
                    }
                }

                // If page has not been specified get the site by the domain and use the site's default page
                if ( string.IsNullOrEmpty( pageId ) )
                {
                    SiteCache site = SiteCache.GetSiteByDomain( requestContext.HttpContext.Request.Url.Host );

                    // if not found use the default site
                    if ( site == null )
                    {
                        site = SiteCache.Read( SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid() );
                    }

                    if ( site != null )
                    {
                        // If site has has been enabled for mobile redirect, then we'll need to check what type of device is being used
                        if ( site.EnableMobileRedirect )
                        {
                            bool redirect = false;

                            // get the device type
                            string u = requestContext.HttpContext.Request.UserAgent;

                            var clientType = PageViewUserAgent.GetClientType( u );

                            // first check if device is a mobile device
                            if ( clientType == "Mobile" )
                            {
                                redirect = true;
                            }

                            // if not, mobile device and tables should be redirected also, check if device is a tablet
                            if ( !redirect && site.RedirectTablets )
                            {
                                if ( clientType == "Tablet" )
                                {
                                    redirect = true;
                                }
                            }

                            if ( redirect )
                            {
                                if ( site.MobilePageId.HasValue )
                                {
                                    pageId = site.MobilePageId.Value.ToString();
                                }
                                else if ( !string.IsNullOrWhiteSpace( site.ExternalUrl ) )
                                {
                                    requestContext.HttpContext.Response.Redirect( site.ExternalUrl );
                                    return null;
                                }
                            }
                        }

                        if ( string.IsNullOrWhiteSpace( pageId ) )
                        {
                            if ( site.DefaultPageId.HasValue )
                            {
                                pageId = site.DefaultPageId.Value.ToString();
                            }

                            if ( site.DefaultPageRouteId.HasValue )
                            {
                                routeId = site.DefaultPageRouteId.Value;
                            }
                        }
                    }

                    if ( string.IsNullOrEmpty( pageId ) )
                    {
                        throw new SystemException( "Invalid Site Configuration" );
                    }
                }

                PageCache page = null;

                if ( !string.IsNullOrEmpty( pageId ) )
                {
                    int pageIdNumber = 0;
                    if ( Int32.TryParse( pageId, out pageIdNumber ) )
                    {
                        page = PageCache.Read( pageIdNumber );
                    }
                }

                if ( page == null )
                {
                    // try to get site's 404 page
                    SiteCache site = SiteCache.GetSiteByDomain( requestContext.HttpContext.Request.Url.Host );
                    if ( site != null && site.PageNotFoundPageId != null )
                    {
                        if ( Convert.ToBoolean( GlobalAttributesCache.Read().GetValue( "Log404AsException" ) ) )
                        {
                            Rock.Model.ExceptionLogService.LogException(
                                new Exception( string.Format( "404 Error: {0}", requestContext.HttpContext.Request.Url.AbsoluteUri ) ),
                                requestContext.HttpContext.ApplicationInstance.Context );
                        }

                        page = PageCache.Read( site.PageNotFoundPageId ?? 0 );
                    }
                    else
                    {
                        // no 404 page found for the site, return the default 404 error page
                        return (System.Web.UI.Page)BuildManager.CreateInstanceFromVirtualPath( "~/Http404Error.aspx", typeof( System.Web.UI.Page ) );
                    }

                }

                string theme = page.Layout.Site.Theme;
                string layout = page.Layout.FileName;
                string layoutPath = PageCache.FormatPath( theme, layout );

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
            catch (Exception ex)
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
