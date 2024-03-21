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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
    [DataContract]
    public class SiteCache : ModelCache<SiteCache, Site>
    {
        #region Static Fields

        private static ConcurrentDictionary<string, int?> _siteDomains = new ConcurrentDictionary<string, int?>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the configured theme. The <see cref="Theme"/> property will check to see if current request url/cookie has specified a different them and if so return that one
        /// </summary>
        /// <value>
        /// The configured theme.
        /// </value>
        [DataMember]
        public string ConfiguredTheme { get; private set; }

        /// <summary>
        /// Gets or sets the *active* theme for the page. NOTE: This might be different than the <see cref="ConfiguredTheme"/>.
        /// NOTE: The 'Theme' property will check to see if current request url/cookie has specified a different them and if so return that one.
        /// </summary>
        /// <value>
        /// The theme.
        /// </value>
        public string Theme
        {
            get
            {
                var httpContext = HttpContext.Current;
                var request = httpContext?.Request;

                if ( request == null )
                {
                    return ConfiguredTheme;
                }

                // FileManager, RockFileBrowser, RockImageBrowser and RockMergeField
                // controls need to use paths from the main internal site, but themed
                // as other sites (potentially). To avoid updating the site theme cookie
                // in these cases we use a different query string parameter.
                var editorTheme = request["EditorTheme"];
                if (
                    editorTheme.IsNotNullOrWhiteSpace() &&
                    System.IO.Directory.Exists( httpContext.Server.MapPath( "~/Themes/" + editorTheme ) ) )
                {
                    return editorTheme;
                }

                var cookieName = $"Site:{Id}:theme";
                var cookie = request.Cookies[cookieName];

                var theme = request["theme"];
                if ( theme != null )
                {
                    if ( theme.Trim() != string.Empty )
                    {
                        // Don't allow switching to an invalid theme
                        if ( System.IO.Directory.Exists( httpContext.Server.MapPath( "~/Themes/" + theme ) ) )
                        {
                            // Add the cookie with the updated theme string to the response.
                            Rock.Web.UI.RockPage.AddOrUpdateCookie( new HttpCookie( cookieName, theme ) );
                            return theme;
                        }
                    }
                    else
                    {
                        // if a blank theme was specified, remove any cookie (use default)
                        if ( cookie != null )
                        {
                            // The request cookie can include the SameSite property, which defaults to None and we can change.
                            // So create a new cookie to save to the response.
                            cookie.Expires = RockDateTime.Now.AddDays( -10 );
                            cookie.Value = null;
                            Rock.Web.UI.RockPage.AddOrUpdateCookie( cookie );
                            return ConfiguredTheme;
                        }
                    }
                }

                if ( cookie == null )
                {
                    return ConfiguredTheme;
                }

                // Don't allow switching to an invalid theme
                if ( System.IO.Directory.Exists( httpContext.Server.MapPath( "~/Themes/" + cookie.Value ) ) )
                {
                    return cookie.Value;
                }

                // Delete the invalid cookie
                cookie.Expires = RockDateTime.Now.AddDays( -10 );
                cookie.Value = null;
                Rock.Web.UI.RockPage.AddOrUpdateCookie( cookie );

                return ConfiguredTheme;
            }

            private set
            {
                ConfiguredTheme = value;
            }
        }

        /// <summary>
        /// Gets or sets the default page id.
        /// </summary>
        /// <value>
        /// The default page id.
        /// </value>
        [DataMember]
        public int? DefaultPageId { get; private set; }

        /// <summary>
        /// Gets or sets the default page route unique identifier.
        /// </summary>
        /// <value>
        /// The default page route unique identifier.
        /// </value>
        [DataMember]
        public int? DefaultPageRouteId { get; private set; }

        /// <summary>
        /// Gets the default page reference.
        /// </summary>
        /// <value>
        /// The default page reference.
        /// </value>
        public PageReference DefaultPageReference => new PageReference( DefaultPageId ?? 0, DefaultPageRouteId ?? 0 );


        /// <summary>
        /// Gets or sets the change password page identifier.
        /// </summary>
        /// <value>
        /// The change password page identifier.
        /// </value>
        [DataMember]
        public int? ChangePasswordPageId { get; private set; }

        /// <summary>
        /// Gets or sets the change password page route identifier.
        /// </summary>
        /// <value>
        /// The change password page route identifier.
        /// </value>
        [DataMember]
        public int? ChangePasswordPageRouteId { get; private set; }

        /// <summary>
        /// Gets the change password page reference.
        /// </summary>
        /// <value>
        /// The change password page reference.
        /// </value>
        public PageReference ChangePasswordPageReference => new PageReference( ChangePasswordPageId ?? 0, ChangePasswordPageRouteId ?? 0 );

        /// <summary>
        /// Gets or sets the 404 page id.
        /// </summary>
        /// <value>
        /// The 404 page id.
        /// </value>
        [DataMember]
        public int? PageNotFoundPageId { get; private set; }

        /// <summary>
        /// Gets or sets the 404 page route unique identifier.
        /// </summary>
        /// <value>
        /// The 404 page route unique identifier.
        /// </value>
        [DataMember]
        public int? PageNotFoundPageRouteId { get; private set; }

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
        [DataMember]
        public int? LoginPageId { get; private set; }

        /// <summary>
        /// Gets or sets the login page route id.
        /// </summary>
        /// <value>
        /// The login page route id.
        /// </value>
        [DataMember]
        public int? LoginPageRouteId { get; private set; }

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
        [DataMember]
        public int? CommunicationPageId { get; private set; }

        /// <summary>
        /// Gets or sets the communication page route identifier.
        /// </summary>
        /// <value>
        /// The communication page route identifier.
        /// </value>
        [DataMember]
        public int? CommunicationPageRouteId { get; private set; }

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
        [DataMember]
        public int? RegistrationPageId { get; private set; }

        /// <summary>
        /// Gets or sets the registration page route id.
        /// </summary>
        /// <value>
        /// The registration page route id.
        /// </value>
        [DataMember]
        public int? RegistrationPageRouteId { get; private set; }

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
        [DataMember]
        public string ErrorPage { get; private set; }

        /// <summary>
        /// Gets or sets the google analytics code.
        /// </summary>
        /// <value>
        /// The google analytics code.
        /// </value>
        [DataMember]
        public string GoogleAnalyticsCode { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable mobile redirect].
        /// </summary>
        /// <value>
        /// <c>true</c> if [enable mobile redirect]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableMobileRedirect { get; private set; }

        /// <summary>
        /// Gets or sets the mobile page identifier.
        /// </summary>
        /// <value>
        /// The mobile page identifier.
        /// </value>
        [DataMember]
        public int? MobilePageId { get; private set; }

        /// <summary>
        /// Gets or sets the external URL.
        /// </summary>
        /// <value>
        /// The external URL.
        /// </value>
        [DataMember]
        public string ExternalUrl { get; private set; }

        /// <summary>
        /// Gets or sets the allowed frame domains.
        /// </summary>
        /// <value>
        /// The allowed frame domains.
        /// </value>
        [DataMember]
        public string AllowedFrameDomains { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [redirect tablets].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [redirect tablets]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool RedirectTablets { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether page views will be stored in the Interaction tables
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable page views]; otherwise, <c>false</c>.
        /// </value>
		[DataMember]
        public bool EnablePageViews { get; private set; }

        /// <summary>
        /// Gets or sets the content of the page header.
        /// </summary>
        /// <value>
        /// The content of the page header.
        /// </value>
        [DataMember]
        public string PageHeaderContent { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow indexing].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow indexing]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowIndexing { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is index enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is index enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsIndexEnabled { get; private set; }

        /// <summary>
        /// Gets or sets the index starting location.
        /// </summary>
        /// <value>
        /// The index starting location.
        /// </value>
        [DataMember]
        public string IndexStartingLocation { get; private set; }

        /// <summary>
        /// Gets the default page.
        /// </summary>
        public PageCache DefaultPage
        {
            get
            {
                if ( DefaultPageId.HasValue && DefaultPageId.Value != 0 )
                {
                    return PageCache.Get( DefaultPageId.Value );
                }

                return null;
            }
        }

        /// <inheritdoc cref="Site.RequiresEncryption" />
        [DataMember]
        public bool RequiresEncryption { get; private set; }

        /// <inheritdoc cref="Site.EnabledForShortening" />
        [DataMember]
        public bool EnabledForShortening { get; private set; } = true;

        /// <inheritdoc cref="Site.FavIconBinaryFileId" />
        [DataMember]
        public int? FavIconBinaryFileId { get; private set; }

        /// <inheritdoc cref="Site.SiteLogoBinaryFileId" />
        [DataMember]
        public int? SiteLogoBinaryFileId { get; private set; }

        /// <inheritdoc cref="Site.DefaultDomainUri" />
        [DataMember]
        public Uri DefaultDomainUri { get; private set; }

        /// <inheritdoc cref="Site.IsActive" />
        [DataMember]
        public bool IsActive { get; private set; }

        /// <inheritdoc cref="Site.AdditionalSettings" />
        [DataMember]
        public string AdditionalSettings { get; private set; }

        /// <inheritdoc cref="Site.SiteType" />
        [DataMember]
        public SiteType SiteType { get; private set; }

        /// <inheritdoc cref="Site.ConfigurationMobilePhoneBinaryFileId" />
        [DataMember]
        public int? ConfigurationMobilePhoneBinaryFileId { get; private set; }

        /// <inheritdoc cref="Site.ConfigurationMobileTabletBinaryFileId" />
        [DataMember]
        public int? ConfigurationMobileTabletBinaryFileId { get; private set; }

        /// <inheritdoc cref="Site.ThumbnailBinaryFileId" />
        [DataMember]
        public int? ThumbnailBinaryFileId { get; private set; }

        /// <inheritdoc cref="Site.LatestVersionDateTime" />
        public DateTime? LatestVersionDateTime { get; private set; }

        /// <inheritdoc cref="Site.EnableExclusiveRoutes" />
        [DataMember]
        public bool EnableExclusiveRoutes { get; private set; }

        /// <inheritdoc cref="Site.EnablePageViewGeoTracking" />
        [DataMember]
        public bool EnablePageViewGeoTracking { get; private set; }

        /// <inheritdoc cref="Site.DisablePredictableIds" />
        [DataMember]
        public bool DisablePredictableIds { get; private set; }

        /// <inheritdoc cref="Site.EnableVisitorTracking" />
        [DataMember]
        public bool EnableVisitorTracking { get; private set; }

        /// <inheritdoc cref="Site.EnablePersonalization" />
        [DataMember]
        public bool EnablePersonalization { get; private set; }

        /// <inheritdoc cref="Site.ConfigurationMobilePhoneFileUrl" />
        [DataMember]
        public string ConfigurationMobilePhoneFileUrl { get; private set; }

        /// <inheritdoc cref="Site.ConfigurationTabletFileUrl" />
        [DataMember]
        public string ConfigurationMobileTabletFileUrl { get; private set; }

        /// <inheritdoc cref="Site.ThumbnailFileUrl" />
        [DataMember]
        public string ThumbnailFileUrl { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var site = entity as Site;
            if ( site == null )
            {
                return;
            }

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
            SiteLogoBinaryFileId = site.SiteLogoBinaryFileId;
            IsActive = site.IsActive;
            DefaultDomainUri = site.DefaultDomainUri;
            SiteType = site.SiteType;
            AdditionalSettings = site.AdditionalSettings;
            ConfigurationMobilePhoneBinaryFileId = site.ConfigurationMobilePhoneBinaryFileId;
            ConfigurationMobileTabletBinaryFileId = site.ConfigurationMobileTabletBinaryFileId;
            ConfigurationMobilePhoneFileUrl = site.ConfigurationMobilePhoneFileUrl;
            ConfigurationMobileTabletFileUrl = site.ConfigurationTabletFileUrl;
            ThumbnailBinaryFileId = site.ThumbnailBinaryFileId;
            ThumbnailFileUrl = site.ThumbnailFileUrl;
            LatestVersionDateTime = site.LatestVersionDateTime;
            EnableExclusiveRoutes = site.EnableExclusiveRoutes;
            EnablePageViewGeoTracking = site.EnablePageViewGeoTracking;
            DisablePredictableIds = site.DisablePredictableIds;
            EnableVisitorTracking = site.EnableVisitorTracking;
            EnablePersonalization = site.EnablePersonalization;

            foreach ( var domain in site.SiteDomains.Select( d => d.Domain ).ToList() )
            {
                _siteDomains.AddOrUpdate( domain, site.Id, ( k, v ) => site.Id );
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
            return Name;
        }

        /// <summary>
        /// Redirects to default page.
        /// </summary>
        public void RedirectToDefaultPage()
        {
            var context = HttpContext.Current;
            context.Response.Redirect( "/", false );
            context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Gets the login URL with return URL.
        /// </summary>
        /// <returns></returns>
        public string GetLoginUrlWithReturnUrl()
        {
            if ( LoginPageId is null )
            {
                return string.Empty;
            }

            var context = HttpContext.Current;
            var pageReference = LoginPageReference;
            var parms = new Dictionary<string, string>();

            // if there is a rckipid token, we don't want to include it when they go to login page since they are going there to log in as a real user
            // this also prevents an issue where they would log in as a real user, but then get logged in with the token instead after they are redirected
            var returnUrl = context.Request.QueryString["returnUrl"] ??
                context.Server.UrlEncode( PersonToken.RemoveRockMagicToken( context.Request.RawUrl ) );

            parms.Add( "returnurl", returnUrl );
            pageReference.Parameters = parms;
            var url = pageReference.BuildUrl();
            return url;
        }

        /// <summary>
        /// Redirects to login page.
        /// </summary>
        public void RedirectToLoginPage( bool includeReturnUrl )
        {
            var context = HttpContext.Current;

            if ( includeReturnUrl )
            {
                var url = GetLoginUrlWithReturnUrl();
                context.Response.Redirect( url, false );
            }
            else
            {
                var pageReference = LoginPageReference;
                context.Response.Redirect( pageReference.BuildUrl(), false );
            }

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
        /// Flushes this instance.
        /// </summary>
        [Obsolete( "This will not work with a distributed cache system such as Redis. Flush the Site from the cache instead.", true )]
        [RockObsolete( "1.10" )]
        public static void RemoveSiteDomains()
        {
            _siteDomains = new ConcurrentDictionary<string, int?>();
        }

        /// <summary>
        /// Returns site based on domain
        /// </summary>
        /// <param name="host">The host.</param>
        /// <returns></returns>
        public static SiteCache GetSiteByDomain( string host )
        {
            int? siteId;
            if ( _siteDomains.TryGetValue( host, out siteId ) )
            {
                return siteId.HasValue ? Get( siteId.Value ) : null;
            }

            using ( var rockContext = new RockContext() )
            {
                var siteDomainService = new SiteDomainService( rockContext );
                var siteDomain = siteDomainService.GetByDomain( host ) ?? siteDomainService.GetByDomainContained( host );

                if ( siteDomain != null )
                {
                    _siteDomains.AddOrUpdate( host, siteDomain.SiteId, ( k, v ) => siteDomain.SiteId );
                    return Get( siteDomain.SiteId );
                }
                else
                {
                    _siteDomains.AddOrUpdate( host, ( int? ) null, ( k, v ) => ( int? ) null );
                }
            }

            return null;
        }

        /// <summary>
        /// Returns an iterator over all the active sites in the site cache.
        /// The iterator may be further acted upon and collected to any collections as required by the caller.
        /// </summary>
        public static IEnumerable<SiteCache> GetAllActiveSites()
        {
            return All()
                .Where( site => site.IsActive );
        }

        #endregion
    }
}