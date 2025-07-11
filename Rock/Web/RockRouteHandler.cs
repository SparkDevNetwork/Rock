﻿// <copyright>
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

using Microsoft.Extensions.Logging;

using Rock.Bus.Message;
using Rock.Cms.Utm;
using Rock.Logging;
using Rock.Model;
using Rock.Tasks;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Web
{
    /// <summary>
    /// Rock custom route handler
    /// </summary>
    [RockLoggingCategory]
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
            SiteCache site;

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
                site = GetSite( host, siteCookie );

                if ( requestContext.RouteData.Values["PageId"] != null )
                {
                    // Pages using the default routing URL will have the page id in the RouteData.Values collection
                    pageId = ( string ) requestContext.RouteData.Values["PageId"];

                    // Does the page ID exist on the requesting site
                    isSiteMatch = IsSiteMatch( site, pageId.AsIntegerOrNull() );

                    if ( site != null && site.EnableExclusiveRoutes && !isSiteMatch )
                    {
                        // If the site has to match and does not then don't use the page ID. Set it to empty so the 404 can be returned.
                        pageId = string.Empty;
                    }
                    else if ( !isSiteMatch )
                    {
                        // This page belongs to another site, make sure it is allowed to be loaded.
                        if ( IsPageExclusiveFromRequestingSite( site, pageId.AsIntegerOrNull(), null ) )
                        {
                            // If the page has to match the site and does not then don't use the page ID. Set it to empty so the 404 can be returned.
                            pageId = string.Empty;
                        }
                    }
                }
                else if ( requestContext.RouteData.DataTokens["PageRoutes"] != null )
                {
                    // Pages that use a custom URL route will have the page id in the RouteData.DataTokens collection
                    GetPageIdFromDataTokens( requestContext, site, out pageId, out routeId, out isSiteMatch );

                    foreach ( var routeParm in requestContext.RouteData.Values )
                    {
                        parms.Add( routeParm.Key, ( string ) routeParm.Value );
                    }
                }
                else if ( ( ( System.Web.Routing.Route ) requestContext.RouteData.Route ).Url.IsNullOrWhiteSpace() )
                {
                    // if we don't have routing info then set the page ID to the default page for the site.

                    // Get the site, if not found use the default site
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

                // If the page ID and site has not yet been matched
                if ( string.IsNullOrEmpty( pageId ) || !isSiteMatch )
                {
                    // if not found use the default site
                    if ( site == null )
                    {
                        site = SiteCache.Get( SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid() );
                    }

                    // Are shortlinks enabled for this site? If so, check for a matching shortlink route.
                    if ( site != null )
                    {
                        if ( site.EnabledForShortening )
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
                                    var routeName = requestContext.RouteData.DataTokens["RouteName"].ToStringSafe().Trim( new Char[] { '{', '}' } );
                                    shortlink = requestContext.RouteData.Values[routeName].ToStringSafe();
                                }
                            }

                            if ( shortlink.IsNullOrWhiteSpace() && requestContext.RouteData.DataTokens["RouteName"] != null )
                            {
                                shortlink = requestContext.RouteData.DataTokens["RouteName"].ToString();
                            }

                            if ( shortlink.IsNotNullOrWhiteSpace() )
                            {
                                using ( var rockContext = new Rock.Data.RockContext() )
                                {
                                    var pageShortLink = new PageShortLinkService( rockContext ).GetByToken( shortlink, site.Id );
                                    var pageShortLinkCache = pageShortLink != null ? PageShortLinkCache.Get( pageShortLink.Id ) : null;

                                    // Use the short link if the site IDs match or the current site and shortlink site are not exclusive.
                                    // Note: this is only a restriction based on the site chosen as the owner of the shortlink, the actual URL can go anywhere.
                                    if ( pageShortLinkCache != null && ( pageShortLinkCache.SiteId == site.Id || ( !site.EnableExclusiveRoutes && !pageShortLinkCache.Site.EnableExclusiveRoutes ) ) )
                                    {
                                        if ( pageShortLinkCache.SiteId == site.Id || requestContext.RouteData.DataTokens["RouteName"] == null )
                                        {
                                            pageId = string.Empty;
                                            routeId = 0;

                                            string visitorPersonAliasIdKey = null;

                                            if ( site.EnableVisitorTracking )
                                            {
                                                if ( routeHttpRequest.Cookies.AllKeys.Contains( Rock.Personalization.RequestCookieKey.ROCK_VISITOR_KEY ) )
                                                {
                                                    visitorPersonAliasIdKey = routeHttpRequest.Cookies[Rock.Personalization.RequestCookieKey.ROCK_VISITOR_KEY]?.Value;
                                                }
                                            }

                                            var (_, urlWithUtm, purposeKey) = pageShortLinkCache.GetCurrentUrlData( rockContext );

                                            // Dummy interaction to get UTM source value from the Request/ShortLink url.
                                            var interactionUtm = new Interaction();

                                            // First, set the UTM field values associated with the shortlink;
                                            // then overwrite with any values that are specified in the original request.
                                            interactionUtm.SetUTMFieldsFromURL( urlWithUtm );

                                            var addShortLinkInteractionMsg = new AddShortLinkInteraction.Message
                                            {
                                                PageShortLinkId = pageShortLinkCache.Id,
                                                Token = pageShortLinkCache.Token,
                                                Url = urlWithUtm,
                                                DateViewed = RockDateTime.Now,
                                                IPAddress = WebRequestHelper.GetClientIpAddress( routeHttpRequest ),
                                                UserAgent = routeHttpRequest.UserAgent ?? string.Empty,
                                                UserName = requestContext.HttpContext.User?.Identity.Name,
                                                VisitorPersonAliasIdKey = visitorPersonAliasIdKey,
                                                UtmSource = UtmHelper.GetUtmSourceNameFromDefinedValueOrText( interactionUtm.SourceValueId, interactionUtm.Source ),
                                                UtmMedium = UtmHelper.GetUtmMediumNameFromDefinedValueOrText( interactionUtm.MediumValueId, interactionUtm.Medium ),
                                                UtmCampaign = UtmHelper.GetUtmCampaignNameFromDefinedValueOrText( interactionUtm.CampaignValueId, interactionUtm.Campaign ),
                                                PurposeKey = purposeKey
                                            };

                                            addShortLinkInteractionMsg.Send();

                                            // Set cache headers to prevent the CDNs from caching the temporary redirection to avoid redirection to stale urls.
                                            requestContext.HttpContext.Response.Cache.SetCacheability( System.Web.HttpCacheability.NoCache );
                                            requestContext.HttpContext.Response.Cache.SetNoStore();
                                            requestContext.HttpContext.Response.Redirect( urlWithUtm, false );
                                            requestContext.HttpContext.ApplicationInstance.CompleteRequest();

                                            // Global.asax.cs will throw and log an exception if null is returned, so just return a new page.
                                            return new System.Web.UI.Page();
                                        }
                                    }
                                }
                            }
                        }

                        // Store the current UTM values in a non-persistent session cookie.
                        var interaction = new Interaction();

                        interaction.SetUTMFieldsFromURL( requestContext.HttpContext?.Request?.Url?.OriginalString );
                        var utmCookieData = new UtmCookieData
                        {
                            Source = interaction.GetUtmSourceName(),
                            Medium = interaction.GetUtmMediumName(),
                            Campaign = interaction.GetUtmCampaignName(),
                            Content = interaction.Content,
                            Term = interaction.Term
                        };

                        UtmHelper.SetUtmCookieDataForRequest( requestContext.HttpContext, utmCookieData );

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
                                    requestContext.HttpContext.Response.Redirect( site.ExternalUrl, false );
                                    requestContext.HttpContext.ApplicationInstance.CompleteRequest();

                                    // Global.asax.cs will throw and log an exception if null is returned, so just return a new page.
                                    return new System.Web.UI.Page();
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
                    if ( site != null && site.PageNotFoundPageId != null )
                    {
                        if ( Convert.ToBoolean( GlobalAttributesCache.Get().GetValue( "Log404AsException" ) ) )
                        {
                            Rock.Model.ExceptionLogService.LogException(
                                new Exception( $"404 Error: {routeHttpRequest.UrlProxySafe().AbsoluteUri}" ),
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

                if ( page.IsRateLimited )
                {
                    
                    var canProcess = RateLimiterCache.CanProcessPage( page.Id, RockPage.GetClientIpAddress(), TimeSpan.FromSeconds( page.RateLimitPeriodDurationSeconds.Value ), page.RateLimitRequestPerPeriod.Value );
                    if ( !canProcess )
                    {
                        return ( System.Web.UI.Page ) BuildManager.CreateInstanceFromVirtualPath( "~/Http429Error.aspx", typeof( System.Web.UI.Page ) );
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

                    var defaultLayoutPath = PageCache.FormatPath( theme, layout );

                    RockLogger.LoggerFactory.CreateLogger<RockRouteHandler>()
                        .LogError( $"Page Layout \"{ layoutPath }\" is invalid. Reverting to default layout \"{ defaultLayoutPath }\"..." );

                    return CreateRockPage( page, defaultLayoutPath, routeId, parms, routeHttpRequest );
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
        /// Determines whether the given PageId or Route exists on the requesting site
        /// </summary>
        /// <param name="requestingSite">The requesting site.</param>
        /// <param name="pageId">The page identifier.</param>
        /// <returns>
        ///   <c>true</c> if [is site match] [the specified requesting site]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsSiteMatch( SiteCache requestingSite, int? pageId )
        {
            // No requesting site, no page, no match
            if ( requestingSite == null || pageId == null )
            {
                return false;
            }

            int? pageSiteId = PageCache.Get( pageId.Value )?.Layout.SiteId;
            if ( pageSiteId != null && pageSiteId == requestingSite.Id)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether a specified Page and Route should be considered exclusive from the requesting site.
        /// </summary>
        /// <param name="requestingSite">The requesting site.</param>
        /// <param name="pageId">The page identifier.</param>
        /// <param name="routeId">The route identifier. Provide this to check the route's IsGlobal property.</param>
        /// <returns>
        ///   <c>true</c> if [the specified Page and Route should be considered exclusive from the requesting site];
        ///   otherwise, <c>false</c>.
        /// </returns>
        private static bool IsPageExclusiveFromRequestingSite( SiteCache requestingSite, int? pageId, int? routeId )
        {
            /*
                4/7/2025 - SMC

                This method should return true when a page from another site is requested, and either the requesting
                site or the site the page belongs to has "Exclusive Routes" enabled, UNLESS it was accessed using a
                PageRoute with the "IsGlobal" flag set.

                Examples:

                    Request Site | Page Site | Route    |
                    Exclusive    | Exclusive | IsGlobal | Result
                    --------------------------------------------
                    False        | False     | False    | False (not exclusive)
                    True         | False     | False    | True (exclusive)
                    False        | True      | False    | True (exclusive)
                    True         | True      | False    | True (exclusive)
                    True         | True      | True     | False (not exclusive)

                Reason: To ensure route exclusivity rules are enforced, except when the route is globally shared.
            */


            if ( pageId == null || requestingSite == null )
            {
                // The default value is not to be exclusive
                return false;
            }

            var pageCache = PageCache.Get( pageId.Value );

            var pageSite = pageCache?.Layout.Site;
            if ( pageSite == null )
            {
                return false;
            }

            // If the page belongs to the requesting site, the page is usable by the site.
            if ( requestingSite.Id == pageSite.Id )
            {
                return false;
            }

            // If pageRoute.IsGlobal then return false, this page is usable by all sites.
            bool isGlobalRoute = routeId != null ? pageCache.PageRoutes.Where( r => r.Id == routeId ).Select( r => r.IsGlobal ).FirstOrDefault() : false;
            if ( isGlobalRoute )
            {
                return false;
            }

            // The page and requesting sites do not match, and the route is not flagged as global, so if either
            // the requesting site _OR_ the site the page belongs to has "exclusive routes" enabled, then return
            // true, indicating that the requesting site should NOT be permitted to load this page.
            return ( requestingSite.EnableExclusiveRoutes || pageSite.EnableExclusiveRoutes );
        }

        /// <summary>
        /// Reregisters the routes from PageRoute and default routes. Does not affect ODataService routes. Call this method after saving changes to PageRoute entities.
        /// </summary>
        public static void ReregisterRoutes()
        {
            RemoveRockPageRoutes();
            RegisterRoutes();
            PageRouteWasUpdatedMessage.Publish();
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
            IOrderedEnumerable<PageRoute> pageRoutes = pageRouteService.Queryable()
                .AsNoTracking()
                .Where( r => r.Page.Layout.Site.SiteType == SiteType.Web )
                .ToList()
                .OrderBy( r => r.Route, StringComparer.OrdinalIgnoreCase );

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

            if ( site == null )
            {
                return;
            }

            // Pages that use a custom URL route will have the page id in the RouteData.DataTokens collection, if there are not any then just return.
            var pageAndRouteIds = ( List<PageAndRouteId> ) routeRequestContext.RouteData.DataTokens["PageRoutes"];
            if ( pageAndRouteIds == null || !pageAndRouteIds.Any() )
            {
                return;
            }

            // First try to find a match for the site
            // See if this is a possible shortlink
            if ( routeRequestContext.RouteData.DataTokens["RouteName"] != null && routeRequestContext.RouteData.DataTokens["RouteName"].ToStringSafe().StartsWith( "{" ) )
            {
                // Get the route value
                string routeValue = routeRequestContext.RouteData.Values.Values.FirstOrDefault().ToStringSafe();

                // See if the route value string matches a shortlink for this site.
                var pageShortLink = new PageShortLinkService( new Rock.Data.RockContext() ).GetByToken( routeValue, site.Id );
                if ( pageShortLink != null && pageShortLink.SiteId == site.Id )
                {
                    // The route entered matches a shortlink for the site, so lets NOT set the page ID for a catch-all route and let the shortlink logic take over.
                    return;
                }
            }
                
            // Not a short link so cycle through the pages and routes to find a match for the site.
            foreach ( var pageAndRouteId in pageAndRouteIds )
            {
                var pageCache = PageCache.Get( pageAndRouteId.PageId );
                if ( pageCache != null && pageCache.Layout != null && pageCache.Layout.SiteId == site.Id )
                {
                    pageId = pageAndRouteId.PageId.ToStringSafe();
                    routeId = pageAndRouteId.RouteId;
                    isSiteMatch = true;
                    return;
                }
            }

            // Default to first site/page that is not Exclusive
            foreach ( var pageAndRouteId in pageAndRouteIds )
            {
                if ( !IsPageExclusiveFromRequestingSite( site, pageAndRouteId.PageId, pageAndRouteId.RouteId ) )
                {
                    // These are safe to assign as defaults
                    pageId = pageAndRouteId.PageId.ToStringSafe();
                    routeId = pageAndRouteId.RouteId;

                    return;
                }
            }
        }
       
        /// <summary>
        /// Gets the site from the site cache in the following order:
        /// 1. Get the site using the domain of the current request
        /// 2. Get the last site from the site cookie
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="siteCookie">The site cookie.</param>
        /// <returns></returns>
        private SiteCache GetSite( string host, HttpCookie siteCookie )
        {
            // Check to see if site can be determined by domain
            SiteCache site = SiteCache.GetSiteByDomain( host );

            // Then check the last site
            if ( site == null && siteCookie != null && siteCookie.Value != null)
            {
                site = SiteCache.Get( siteCookie.Value.AsInteger() );
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
            Rock.Web.UI.RockPage.AddOrUpdateCookie( "last_site", page.Layout.SiteId.ToString(), null );
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
