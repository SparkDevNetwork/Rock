// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Web;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a site that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class SiteCache : CachedModel<Site>
    {
        #region Constructors

        private SiteCache()
        {
        }

        private SiteCache( Site site )
        {
            CopyFromModel( site );
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the theme.
        /// </summary>
        /// <value>
        /// The theme.
        /// </value>
        public string Theme
        {
            get
            {
                var httpContext = HttpContext.Current;
                if ( httpContext != null )
                {
                    var request = httpContext.Request;
                    if ( request != null )
                    {
                        string cookieName = SiteCache.CacheKey( this.Id ) + ":theme";
                        HttpCookie cookie = request.Cookies[cookieName];

                        string theme = request["theme"];
                        if ( theme != null )
                        {
                            if ( theme.Trim() != string.Empty )
                            {
                                // Don't allow switching to an invalid theme
                                if ( System.IO.Directory.Exists( httpContext.Server.MapPath( "~/Themes/" + theme ) ) )
                                {
                                    if ( cookie == null )
                                    {
                                        cookie = new HttpCookie( cookieName, theme );
                                    }
                                    else
                                    {
                                        cookie.Value = theme;
                                    }
                                    httpContext.Response.SetCookie( cookie );

                                    return theme;
                                }
                            }
                            else
                            {
                                // if a blank theme was specified, remove any cookie (use default)
                                if ( cookie != null )
                                {
                                    cookie.Expires = RockDateTime.Now.AddDays( -10 );
                                    cookie.Value = null;
                                    httpContext.Response.SetCookie( cookie );
                                    return _theme;
                                }
                            }
                        }

                        if ( cookie != null )
                        {
                            theme = cookie.Value;

                            // Don't allow switching to an invalid theme
                            if ( System.IO.Directory.Exists( httpContext.Server.MapPath( "~/Themes/" + theme ) ) )
                            {
                                return cookie.Value;
                            }
                            else
                            {
                                // Delete the invalid cookie
                                cookie.Expires = RockDateTime.Now.AddDays( -10 );
                                cookie.Value = null;
                                httpContext.Response.SetCookie( cookie );
                            }

                        }
                    }
                }

                return _theme;
            }
            set
            {
                _theme = value;
            }
        }
        private string _theme = string.Empty;

        /// <summary>
        /// Gets or sets the default page id.
        /// </summary>
        /// <value>
        /// The default page id.
        /// </value>
        public int? DefaultPageId { get; set; }

        /// <summary>
        /// Gets or sets the default page route unique identifier.
        /// </summary>
        /// <value>
        /// The default page route unique identifier.
        /// </value>
        public int? DefaultPageRouteId { get; set; }

        /// <summary>
        /// Gets the default page reference.
        /// </summary>
        /// <value>
        /// The default page reference.
        /// </value>
        public PageReference DefaultPageReference
        {
            get
            {
                return new Rock.Web.PageReference( DefaultPageId ?? 0, DefaultPageRouteId ?? 0 );
            }
        }

        /// <summary>
        /// Gets or sets the 404 page id.
        /// </summary>
        /// <value>
        /// The 404 page id.
        /// </value>
        public int? PageNotFoundPageId { get; set; }

        /// <summary>
        /// Gets or sets the 404 page route unique identifier.
        /// </summary>
        /// <value>
        /// The 404 page route unique identifier.
        /// </value>
        public int? PageNotFoundPageRouteId { get; set; }

        /// <summary>
        /// Gets the page not found page reference.
        /// </summary>
        /// <value>
        /// The page not found page reference.
        /// </value>
        public PageReference PageNotFoundPageReference
        {
            get
            {
                return new Rock.Web.PageReference( PageNotFoundPageId ?? 0, PageNotFoundPageRouteId ?? 0 );
            }
        }

        /// <summary>
        /// Gets or sets the login page id.
        /// </summary>
        /// <value>
        /// The login page id.
        /// </value>
        public int? LoginPageId { get; set; }

        /// <summary>
        /// Gets or sets the login page route id.
        /// </summary>
        /// <value>
        /// The login page route id.
        /// </value>
        public int? LoginPageRouteId { get; set; }

        /// <summary>
        /// Gets the login page reference.
        /// </summary>
        /// <value>
        /// The login page reference.
        /// </value>
        public PageReference LoginPageReference
        {
            get
            {
                return new Rock.Web.PageReference( LoginPageId ?? 0, LoginPageRouteId ?? 0 );
            }
        }

        /// <summary>
        /// Gets or sets the communication page identifier.
        /// </summary>
        /// <value>
        /// The communication page identifier.
        /// </value>
        public int? CommunicationPageId { get; set; }

        /// <summary>
        /// Gets or sets the communication page route identifier.
        /// </summary>
        /// <value>
        /// The communication page route identifier.
        /// </value>
        public int? CommunicationPageRouteId { get; set; }

        /// <summary>
        /// Gets the communication page reference.
        /// </summary>
        /// <value>
        /// The communication page reference.
        /// </value>
        public PageReference CommunicationPageReference
        {
            get
            {
                return new Rock.Web.PageReference( CommunicationPageId ?? 0, CommunicationPageRouteId ?? 0 );
            }
        }

        /// <summary>
        /// Gets or sets the registration page id.
        /// </summary>
        /// <value>
        /// The registration page id.
        /// </value>
        public int? RegistrationPageId { get; set; }

        /// <summary>
        /// Gets or sets the registration page route id.
        /// </summary>
        /// <value>
        /// The registration page route id.
        /// </value>
        public int? RegistrationPageRouteId { get; set; }

        /// <summary>
        /// Gets the registration page reference.
        /// </summary>
        /// <value>
        /// The registration page reference.
        /// </value>
        public PageReference RegistrationPageReference
        {
            get
            {
                return new Rock.Web.PageReference( RegistrationPageId ?? 0, RegistrationPageRouteId ?? 0 );
            }
        }

        /// <summary>
        /// Gets or sets the error page.
        /// </summary>
        /// <value>
        /// The error page.
        /// </value>
        public string ErrorPage { get; set; }

        /// <summary>
        /// Gets or sets the google analytics code.
        /// </summary>
        /// <value>
        /// The google analytics code.
        /// </value>
        public string GoogleAnalyticsCode { get; set; }

        /// <summary>
        /// Gets or sets the facebook app id.
        /// </summary>
        /// <value>
        /// The facebook app id.
        /// </value>
        public string FacebookAppId { get; set; }

        /// <summary>
        /// Gets or sets the facebook app secret.
        /// </summary>
        /// <value>
        /// The facebook app secret.
        /// </value>
        public string FacebookAppSecret { get; set; }

        /// <summary>
        /// Gets the default page.
        /// </summary>
        public PageCache DefaultPage
        {
            get
            {
                if ( DefaultPageId.HasValue && DefaultPageId.Value != 0 )
                {
                    return PageCache.Read( DefaultPageId.Value );
                }
                
                return null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is Site )
            {
                var site = (Site)model;
                this.IsSystem = site.IsSystem;
                this.Name = site.Name;
                this.Description = site.Description;
                this.Theme = site.Theme;
                this.DefaultPageId = site.DefaultPageId;
                this.DefaultPageRouteId = site.DefaultPageRouteId;
                this.LoginPageId = site.LoginPageId;
                this.LoginPageRouteId = site.LoginPageRouteId;
                this.CommunicationPageId = site.CommunicationPageId;
                this.CommunicationPageRouteId = site.CommunicationPageRouteId;
                this.RegistrationPageId = site.RegistrationPageId;
                this.RegistrationPageRouteId = site.RegistrationPageRouteId;
                this.ErrorPage = site.ErrorPage;
                this.GoogleAnalyticsCode = site.GoogleAnalyticsCode;
                this.FacebookAppId = site.FacebookAppId;
                this.FacebookAppSecret = site.FacebookAppSecret;
                this.PageNotFoundPageId = site.PageNotFoundPageId;
                this.PageNotFoundPageRouteId = site.PageNotFoundPageRouteId;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Redirects to default page.
        /// </summary>
        public void RedirectToDefaultPage()
        {
            var context = HttpContext.Current;
            context.Response.Redirect( DefaultPageReference.BuildUrl(), false );
            context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Redirects to login page.
        /// </summary>
        public void RedirectToLoginPage( bool includeReturnUrl )
        {
            var context = HttpContext.Current;

            var pageReference = LoginPageReference;

            if ( includeReturnUrl )
            {
                var parms = new Dictionary<string, string>();
                parms.Add( "returnurl", context.Request.QueryString["returnUrl"] ?? context.Server.UrlEncode( context.Request.RawUrl ) );
                pageReference.Parameters = parms;
            }

            context.Response.Redirect( pageReference.BuildUrl(), false );
            context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Redirects to communication page.
        /// </summary>
        public void RedirectToCommunicationPage()
        {
            var context = HttpContext.Current;
            context.Response.Redirect( CommunicationPageReference.BuildUrl(), false );
            context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Redirects to registration page.
        /// </summary>
        public void RedirectToRegistrationPage()
        {
            var context = HttpContext.Current;
            context.Response.Redirect( RegistrationPageReference.BuildUrl(), false );
            context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Redirects to registration page.
        /// </summary>
        public void RedirectToPageNotFoundPage()
        {
            var context = HttpContext.Current;
            context.Response.Redirect( PageNotFoundPageReference.BuildUrl(), false );
            context.ApplicationInstance.CompleteRequest();
        }

        #endregion

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:Site:{0}", id );
        }

        /// <summary>
        /// Returns Site object from cache.  If site does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static SiteCache Read( int id, RockContext rockContext = null )
        {
            string cacheKey = SiteCache.CacheKey( id );
            ObjectCache cache = RockMemoryCache.Default;
            SiteCache site = cache[cacheKey] as SiteCache;

            if ( site == null )
            {
                if ( rockContext != null )
                {
                    site = LoadById( id, rockContext );
                }
                else
                {
                    using ( var myRockContext = new RockContext() )
                    {
                        site = LoadById( id, myRockContext );
                    }
                }

                if ( site != null )
                {
                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( cacheKey, site, cachePolicy );
                    cache.Set( site.Guid.ToString(), site.Id, cachePolicy );
                }
            }

            return site;
        }

        private static SiteCache LoadById( int id, RockContext rockContext )
        {
            var siteService = new SiteService( rockContext );
            var siteModel = siteService.Get( id );
            if ( siteModel != null )
            {
                siteModel.LoadAttributes( rockContext );
                return new SiteCache( siteModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static SiteCache Read( Guid guid, RockContext rockContext = null )
        {
            ObjectCache cache = RockMemoryCache.Default;
            object cacheObj = cache[guid.ToString()];

            SiteCache site = null;
            if ( cacheObj != null )
            {
                site = Read( (int)cacheObj, rockContext );
            }

            if ( site == null )
            {
                if ( rockContext != null )
                {
                    site = LoadByGuid( guid, rockContext );
                }
                else
                {
                    using ( var myRockContext = new RockContext() )
                    {
                        site = LoadByGuid( guid, myRockContext );
                    }
                }

                if ( site != null )
                {
                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( SiteCache.CacheKey( site.Id ), site, cachePolicy );
                    cache.Set( site.Guid.ToString(), site.Id, cachePolicy );
                }
            }

            return site;
        }

        private static SiteCache LoadByGuid( Guid guid, RockContext rockContext = null )
        {
            var siteService = new SiteService( rockContext );
            var siteModel = siteService.Get( guid );
            if ( siteModel != null )
            {
                siteModel.LoadAttributes( rockContext );
                return new SiteCache( siteModel );
            }

            return null;
        }

        /// <summary>
        /// Adds Site model to cache, and returns cached object
        /// </summary>
        /// <param name="siteModel"></param>
        /// <returns></returns>
        public static SiteCache Read( Site siteModel )
        {
            string cacheKey = SiteCache.CacheKey( siteModel.Id );
            ObjectCache cache = RockMemoryCache.Default;
            SiteCache site = cache[cacheKey] as SiteCache;

            if ( site != null )
            {
                site.CopyFromModel( siteModel );
            }
            else
            {
                site = new SiteCache( siteModel );
                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, site, cachePolicy );
                cache.Set( site.Guid.ToString(), site.Id, cachePolicy );
            }

            return site;
        }

        /// <summary>
        /// Removes site from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = RockMemoryCache.Default;
            cache.Remove( SiteCache.CacheKey( id ) );
        }

        /// <summary>
        /// Returns site based on domain
        /// </summary>
        /// <param name="host">The host.</param>
        /// <returns></returns>
        public static SiteCache GetSiteByDomain( string host )
        {
            SiteCache site = null;

            string cacheKey = "Rock:DomainSites";

            ObjectCache cache = RockMemoryCache.Default;
            var sites = cache[cacheKey] as ConcurrentDictionary<string, int>;
            if ( sites == null )
            {
                sites = new ConcurrentDictionary<string, int>();
                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, sites, cachePolicy );
            }

            // look in cache
            if ( sites.ContainsKey( host ) )
            {
                site = SiteCache.Read( sites[host] );
            }
            else
            {
                // get from database
                using ( var rockContext = new RockContext() )
                {
                    Rock.Model.SiteDomainService siteDomainService = new Rock.Model.SiteDomainService( rockContext );
                    Rock.Model.SiteDomain siteDomain = siteDomainService.GetByDomain( host );
                    if ( siteDomain == null )
                    {
                        siteDomain = siteDomainService.GetByDomainContained( host );
                    }

                    if ( siteDomain != null )
                    {
                        sites.AddOrUpdate( host, siteDomain.SiteId, ( k, v ) => siteDomain.SiteId );
                        site = SiteCache.Read( siteDomain.SiteId );
                    }
                }
            }

            return site;
        }

        #endregion

    }
}