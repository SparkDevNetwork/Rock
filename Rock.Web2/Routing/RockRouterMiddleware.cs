using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Web.Cache;

using Rock.Web2.UI;

namespace Rock.Web2.Routing
{
    /// <summary>
    /// Determine the logical page being requested by evaluating all the request parameters.
    /// 
    /// 1. If there is no requested path, use default page
    /// 2. PageId
    /// 3. Route match and Site match
    /// 4. ShortLink match and Site match
    /// 5. Route match and no site match
    /// 6. ShortLink match and no site match
    /// </summary>
    public class RockRouterMiddleware
    {
        public const string SITE_COOKIE_NAME = "last_site";

        #region Private Fields

        private readonly RequestDelegate _next;
        private readonly IApplicationBuilder _applicationBuilder;
        private IRouter _staticRouter;
        private IRouter _dynamicRouter;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RockRouterMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next.</param>
        /// <param name="applicationBuilder">The application builder.</param>
        public RockRouterMiddleware( RequestDelegate next, IApplicationBuilder applicationBuilder )
        {
            _next = next;
            _applicationBuilder = applicationBuilder;

            BuildStaticRoutes();
        }

        #endregion

        #region Middleware Methods

        /// <summary>
        /// Invokes the asynchronous route handler.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task InvokeAsync( HttpContext context )
        {
            var routeContext = new RouteContext( context );

            //
            // Check our static routes, such as /page/{PageId}, first.
            //
            await _staticRouter.RouteAsync( routeContext );

            //
            // If no match was found, then check the dynamic routes, shortlinks, etc.
            //
            if ( routeContext.Handler == null )
            {
                if ( _dynamicRouter == null )
                {
                    BuildDynamicRoutes();
                }

                await _dynamicRouter.RouteAsync( routeContext );
            }

            //
            // If we found a matching route, then execute it.
            //
            if ( routeContext.Handler != null )
            {
                context.Features.Set<IRoutingFeature>( new RoutingFeature { RouteData = routeContext.RouteData } );

                await routeContext.Handler( context );

                return;
            }

            await _next( context );
        }

        #endregion

        #region Route Building Methods

        /// <summary>
        /// Builds the static routes.
        /// </summary>
        private void BuildStaticRoutes()
        {
            var constraintResolver = _applicationBuilder.ApplicationServices.GetRequiredService<IInlineConstraintResolver>();
            var builder = new RouteBuilder( _applicationBuilder );

            //
            // 1. Add the standard / route (default home page ).
            //
            builder.Routes.Add( new Route(
                new RouteHandler( DefaultPageAsync ),
                null,
                "",
                new RouteValueDictionary(),
                new RouteValueDictionary(),
                new RouteValueDictionary(),
                constraintResolver ) );

            //
            // 2. Add the standard /page/{Id} route.
            //
            builder.Routes.Add( new Route(
                new RouteHandler( PageRouteAsync ),
                null,
                "page/{PageId:int}",
                new RouteValueDictionary(),
                new RouteValueDictionary(),
                new RouteValueDictionary(),
                constraintResolver ) );

            _staticRouter = builder.Build();
        }

        /// <summary>
        /// Builds the dynamic routes.
        /// </summary>
        public void BuildDynamicRoutes()
        {
            var constraintResolver = _applicationBuilder.ApplicationServices.GetRequiredService<IInlineConstraintResolver>();
            var builder = new RouteBuilder( _applicationBuilder );

            using ( var rockContext = new RockContext() )
            {
                var routes = new PageRouteService( rockContext ).Queryable().ToList();
                var shortlinks = new PageShortLinkService( rockContext ).Queryable().ToList();

                //
                // 3. Add Routes with a site match.
                //
                foreach ( var route in routes )
                {
                    var pageCache = PageCache.Get( route.PageId );

                    builder.Routes.Add( new Route(
                        new RouteHandler( PageRouteAsync ),
                        null,
                        route.Route,
                        new RouteValueDictionary( new
                        {
                            route.PageId,
                            RouteId = route.Id
                        } ),
                        new RouteValueDictionary( new
                        {
                            Site = new SiteMatchConstraint( pageCache.Layout.SiteId )
                        } ),
                        new RouteValueDictionary(),
                        constraintResolver ) );
                }

                //
                // 4. Add Shortlink match with a site match.
                //
                //foreach ( var shortlink in shortlinks )
                //{
                //    builder.Routes.Add( new Route(
                //        new RouteHandler( ShortLinkAsync ),
                //        null,
                //        shortlink.Token,
                //        new RouteValueDictionary( new
                //        {
                //            ShortLinkId = shortlink.Id,
                //            shortlink.Url,
                //            shortlink.Token
                //        } ),
                //        new RouteValueDictionary( new { Site = new SiteMatchConstraint( shortlink.SiteId ) } ),
                //        new RouteValueDictionary(),
                //        constraintResolver ) );
                //}

                //
                // 5. Add Routes with no site match.
                //
                foreach ( var route in routes )
                {
                    var pageCache = PageCache.Get( route.PageId );

                    builder.Routes.Add( new Route(
                        new RouteHandler( PageRouteAsync ),
                        null,
                        route.Route,
                        new RouteValueDictionary( new
                        {
                            route.PageId,
                            RouteId = route.Id
                        } ),
                        new RouteValueDictionary(),
                        new RouteValueDictionary(),
                        constraintResolver ) );
                }

                //
                // 6. Add Shortlink match with no site match.
                //
                //foreach ( var shortlink in shortlinks )
                //{
                //    builder.Routes.Add( new Route(
                //        new RouteHandler( ShortLinkAsync ),
                //        null,
                //        shortlink.Token,
                //        new RouteValueDictionary( new
                //        {
                //            ShortLinkId = shortlink.Id,
                //            shortlink.Url,
                //            shortlink.Token
                //        } ),
                //        new RouteValueDictionary(),
                //        new RouteValueDictionary(),
                //        constraintResolver ) );
                //}
            }

            _dynamicRouter = builder.Build();
        }

        #endregion

        #region Route Handlers

        /// <summary>
        /// Processes a request for a specific Rock page.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private async Task PageRouteAsync( HttpContext context )
        {
            var routeData = context.GetRouteData();
            int pageId = routeData.Values["PageId"].ToString().AsInteger();
            var pageCache = PageCache.Get( pageId );

            var requestMessage = new HttpRequestWrapper( context.Request );

            var rockRequestContext = new RockRequestContext( requestMessage, new NullRockResponseContext() ); //context.RequestServices.GetRequiredService<RockRequestContext>();

            var currentPage = new RockPage( pageCache, rockRequestContext );

            CreateSiteCookie( context, pageCache );

            var actionContext = new ActionContext( context, routeData, new ActionDescriptor() );
            var response = await currentPage.RenderAsync();

            await response.ExecuteResultAsync( actionContext );
        }

        /// <summary>
        /// Returns the default page for the site.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private async Task DefaultPageAsync( HttpContext context )
        {
            var site = GetSite( context );

            if ( site == null )
            {
                site = SiteCache.Get( Rock.SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid() );
            }

            if ( !site.DefaultPageId.HasValue )
            {
                throw new SystemException( "Invalid Site Configuration" );
            }

            context.GetRouteData().Values["PageId"] = site.DefaultPageId.Value;

            await PageRouteAsync( context );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Gets the site from the site cache in the following order:
        /// 1. check the query string and try to get the site
        /// 2. Get the site using the domain of the current request
        /// 3. Get the last site from the site cookie
        /// </summary>
        /// <returns></returns>
        private SiteCache GetSite( HttpContext context )
        {
            SiteCache site = null;
            string siteCookie = context.Request.Cookies[SITE_COOKIE_NAME];
            string host = context.Request.Host.Host;

            // First check to see if site was specified in querystring
            int? siteId = context.Request.Query["SiteId"].ToString().AsIntegerOrNull();
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
                if ( siteCookie != null )
                {
                    site = SiteCache.Get( siteCookie.AsInteger() );
                }
            }

            return site;
        }

        /// <summary>
        /// Creates the site cookie with the site ID of the page
        /// </summary>
        /// <param name="context">The context of this request.</param>
        /// <param name="page">The page.</param>
        private void CreateSiteCookie( HttpContext context, PageCache page )
        {
            context.Response.Cookies.Append( SITE_COOKIE_NAME, page.Layout.SiteId.ToString() );
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Ensures that the route matches a specific site.
        /// </summary>
        private class SiteMatchConstraint : IRouteConstraint
        {
            readonly int _siteId;

            public SiteMatchConstraint( int siteId )
            {
                _siteId = siteId;
            }

            public bool Match( HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection )
            {
                var site = SiteCache.GetSiteByDomain( httpContext.Request.Host.Host );

                if ( site != null )
                {
                    return site.Id == _siteId;
                }

                return false;
            }
        }

        private class HttpRequestWrapper : IRequest
        {
            public IPAddress RemoteAddress => IPAddress.Parse( "127.0.0.1" );

            public Uri RequestUri { get; }

            public NameValueCollection QueryString { get; } = new NameValueCollection( StringComparer.OrdinalIgnoreCase );

            public NameValueCollection Headers { get; } = new NameValueCollection( StringComparer.OrdinalIgnoreCase );

            public IDictionary<string, string> Cookies { get; } = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

            public HttpRequestWrapper( HttpRequest request )
            {
                foreach ( var qs in request.Query )
                {
                    foreach ( var v in qs.Value )
                    {
                        QueryString.Add( qs.Key, v );
                    }
                }

                foreach ( var h in request.Headers )
                {
                    foreach ( var v in h.Value )
                    {
                        Headers.Add( h.Key, v );
                    }
                }

                foreach ( var c in request.Cookies )
                {
                    Cookies.AddOrIgnore( c.Key, c.Value );
                }

                RequestUri = new Uri( request.GetDisplayUrl() );
            }
        }

        #endregion
    }
}