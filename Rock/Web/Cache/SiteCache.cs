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
using System.Web;

using Rock.Cache;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a site that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.CacheSite instead" )]
    public class SiteCache : CachedModel<Site>
    {
        #region Constructors

        private SiteCache()
        {
        }

        private SiteCache( CacheSite cacheSite )
        {
            CopyFromNewCache( cacheSite );
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
        public string Theme { get; set; }

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
        public PageReference DefaultPageReference => new PageReference( DefaultPageId ?? 0, DefaultPageRouteId ?? 0 );

        /// <summary>
        /// Gets or sets the 404 page id.
        /// </summary>
        /// <value>
        /// The 404 page id.
        /// </value>
        public int? PageNotFoundPageId { get; set; }

        /// <summary>
        /// Gets or sets the change password page identifier.
        /// </summary>
        /// <value>
        /// The change password page identifier.
        /// </value>
        public int? ChangePasswordPageId { get; set; }

        /// <summary>
        /// Gets or sets the change password page route identifier.
        /// </summary>
        /// <value>
        /// The change password page route identifier.
        /// </value>
        public int? ChangePasswordPageRouteId { get; set; }

        /// <summary>
        /// Gets the change password page reference.
        /// </summary>
        /// <value>
        /// The change password page reference.
        /// </value>
        public PageReference ChangePasswordPageReference => new PageReference( ChangePasswordPageId ?? 0, ChangePasswordPageRouteId ?? 0 );

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
        public PageReference PageNotFoundPageReference => new PageReference( PageNotFoundPageId ?? 0, PageNotFoundPageRouteId ?? 0 );

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
        public PageReference LoginPageReference => new PageReference( LoginPageId ?? 0, LoginPageRouteId ?? 0 );

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
        public PageReference CommunicationPageReference => new PageReference( CommunicationPageId ?? 0, CommunicationPageRouteId ?? 0 );

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
        public PageReference RegistrationPageReference => new PageReference( RegistrationPageId ?? 0, RegistrationPageRouteId ?? 0 );

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
        /// Gets or sets a value indicating whether [enable mobile redirect].
        /// </summary>
        /// <value>
        /// <c>true</c> if [enable mobile redirect]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableMobileRedirect { get; set; }

        /// <summary>
        /// Gets or sets the mobile page identifier.
        /// </summary>
        /// <value>
        /// The mobile page identifier.
        /// </value>
        public int? MobilePageId { get; set; }

        /// <summary>
        /// Gets or sets the external URL.
        /// </summary>
        /// <value>
        /// The external URL.
        /// </value>
        public string ExternalUrl { get; set; }

        /// <summary>
        /// Gets or sets the allowed frame domains.
        /// </summary>
        /// <value>
        /// The allowed frame domains.
        /// </value>
        public string AllowedFrameDomains { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [redirect tablets].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [redirect tablets]; otherwise, <c>false</c>.
        /// </value>
        public bool RedirectTablets { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether page views will be stored in the Interaction tables
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable page views]; otherwise, <c>false</c>.
        /// </value>
        public bool EnablePageViews { get; set; }

        /// <summary>
        /// Gets or sets the content of the page header.
        /// </summary>
        /// <value>
        /// The content of the page header.
        /// </value>
        public string PageHeaderContent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow indexing].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow indexing]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowIndexing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is index enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is index enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsIndexEnabled { get; set; }

        /// <summary>
        /// Gets or sets the index starting location.
        /// </summary>
        /// <value>
        /// The index starting location.
        /// </value>
        public string IndexStartingLocation { get; set; }

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

        /// <summary>
        /// Gets or sets a value indicating whether [requires encryption].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [requires encryption]; otherwise, <c>false</c>.
        /// </value>
        public bool RequiresEncryption { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this site should be available to be used for shortlinks (the shortlink can still reference url of other sites).
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enabled for shortening]; otherwise, <c>false</c>.
        /// </value>
        public bool EnabledForShortening { get; set; } = true;

        /// <summary>
        /// Gets or sets the favicon binary file identifier.
        /// </summary>
        /// <value>
        /// The favicon binary file identifier.
        /// </value>
        public int? FavIconBinaryFileId { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is Site ) ) return;

            var site = (Site)model;
            IsSystem = site.IsSystem;
            Name = site.Name;
            Description = site.Description;
            Theme = site.Theme;
            DefaultPageId = site.DefaultPageId;
            DefaultPageRouteId = site.DefaultPageRouteId;
            LoginPageId = site.LoginPageId;
            LoginPageRouteId = site.LoginPageRouteId;
            ChangePasswordPageId = site.ChangePasswordPageId;
            ChangePasswordPageRouteId = site.ChangePasswordPageRouteId;
            RegistrationPageId = site.RegistrationPageId;
            RegistrationPageRouteId = site.RegistrationPageRouteId;
            PageNotFoundPageId = site.PageNotFoundPageId;
            PageNotFoundPageRouteId = site.PageNotFoundPageRouteId;
            CommunicationPageId = site.CommunicationPageId;
            CommunicationPageRouteId = site.CommunicationPageRouteId;
            ErrorPage = site.ErrorPage;
            GoogleAnalyticsCode = site.GoogleAnalyticsCode;
            EnableMobileRedirect = site.EnableMobileRedirect;
            MobilePageId = site.MobilePageId;
            ExternalUrl = site.ExternalUrl;
            AllowedFrameDomains = site.AllowedFrameDomains;
            RedirectTablets = site.RedirectTablets;
            EnablePageViews = site.EnablePageViews;
            PageHeaderContent = site.PageHeaderContent;
            AllowIndexing = site.AllowIndexing;
            IsIndexEnabled = site.IsIndexEnabled;
            IndexStartingLocation = site.IndexStartingLocation;
            RequiresEncryption = site.RequiresEncryption;
            EnabledForShortening = site.EnabledForShortening;
            FavIconBinaryFileId = site.FavIconBinaryFileId;
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CacheSite ) ) return;

            var site = (CacheSite)cacheEntity;
            IsSystem = site.IsSystem;
            Name = site.Name;
            Description = site.Description;
            Theme = site.Theme;
            DefaultPageId = site.DefaultPageId;
            DefaultPageRouteId = site.DefaultPageRouteId;
            LoginPageId = site.LoginPageId;
            LoginPageRouteId = site.LoginPageRouteId;
            ChangePasswordPageId = site.ChangePasswordPageId;
            ChangePasswordPageRouteId = site.ChangePasswordPageRouteId;
            RegistrationPageId = site.RegistrationPageId;
            RegistrationPageRouteId = site.RegistrationPageRouteId;
            PageNotFoundPageId = site.PageNotFoundPageId;
            PageNotFoundPageRouteId = site.PageNotFoundPageRouteId;
            CommunicationPageId = site.CommunicationPageId;
            CommunicationPageRouteId = site.CommunicationPageRouteId;
            ErrorPage = site.ErrorPage;
            GoogleAnalyticsCode = site.GoogleAnalyticsCode;
            EnableMobileRedirect = site.EnableMobileRedirect;
            MobilePageId = site.MobilePageId;
            ExternalUrl = site.ExternalUrl;
            AllowedFrameDomains = site.AllowedFrameDomains;
            RedirectTablets = site.RedirectTablets;
            EnablePageViews = site.EnablePageViews;
            PageHeaderContent = site.PageHeaderContent;
            AllowIndexing = site.AllowIndexing;
            IsIndexEnabled = site.IsIndexEnabled;
            IndexStartingLocation = site.IndexStartingLocation;
            RequiresEncryption = site.RequiresEncryption;
            EnabledForShortening = site.EnabledForShortening;
            FavIconBinaryFileId = site.FavIconBinaryFileId;
            Theme = site.Theme;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
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

                // if there is a rckipid token, we don't want to include it when they go to login page since they are going there to login as a real user
                // this also prevents an issue where they would log in as a real user, but then get logged in with the token instead after they are redirected
                var returnUrl = context.Request.QueryString["returnUrl"] ??
                    context.Server.UrlEncode( PersonToken.RemoveRockMagicToken( context.Request.RawUrl ) );

                parms.Add( "returnurl", returnUrl );
                pageReference.Parameters = parms;
            }

            context.Response.Redirect( pageReference.BuildUrl(), false );
            context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Redirects to change password page.
        /// </summary>
        /// <param name="isChangePasswordRequired">if set to <c>true</c> [is change password required].</param>
        /// <param name="includeReturnUrl">if set to <c>true</c> [include return URL].</param>
        public void RedirectToChangePasswordPage( bool isChangePasswordRequired, bool includeReturnUrl )
        {
            var context = HttpContext.Current;

            var pageReference = ChangePasswordPageReference;

            var parms = new Dictionary<string, string>();

            if ( isChangePasswordRequired )
            {
                parms.Add( "ChangeRequired", "True" );
            }

            if ( includeReturnUrl )
            {
                parms.Add( "ReturnUrl", context.Request.QueryString["returnUrl"] ?? context.Server.UrlEncode( context.Request.RawUrl ) );
            }

            pageReference.Parameters = parms;

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

        /// <summary>
        /// Returns Site object from cache.  If site does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static SiteCache Read( int id, RockContext rockContext = null )
        {
            return new SiteCache( CacheSite.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static SiteCache Read( Guid guid, RockContext rockContext = null )
        {
            return new SiteCache( CacheSite.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Adds Site model to cache, and returns cached object
        /// </summary>
        /// <param name="siteModel"></param>
        /// <returns></returns>
        public static SiteCache Read( Site siteModel )
        {
            return new SiteCache( CacheSite.Get( siteModel ) );
        }

        /// <summary>
        /// Removes site from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CacheSite.Remove( id );
            CacheSite.RemoveSiteDomains();
        }

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        public static void Flush()
        {
            CacheSite.RemoveSiteDomains();
        }

        /// <summary>
        /// Returns site based on domain
        /// </summary>
        /// <param name="host">The host.</param>
        /// <returns></returns>
        public static SiteCache GetSiteByDomain( string host )
        {
            return new SiteCache( CacheSite.GetSiteByDomain( host ) );
        }

        #endregion
    }
}