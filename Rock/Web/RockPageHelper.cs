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
using System.Diagnostics;
using System.Linq;
using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Rock.Configuration;
using Rock.Logging;
using Rock.Net;
using Rock.Observability;
using Rock.ViewModels.Crm;
using Rock.Web.Cache;

namespace Rock.Web
{
    /// <summary>
    /// Contains common logic and helper methods for rendering Rock Pages. This
    /// is meant to be used by both WebForms and next-gen implementations.
    /// </summary>
    internal static class RockPageHelper
    {
        #region Fields

        /// <summary>
        /// The version of Rock to inject in startup scripts.
        /// </summary>
        private static readonly string _rockVersion = "Rock v" + typeof( RockPageHelper ).Assembly.GetName().Version.ToString();

        #endregion

        /// <summary>
        /// Adds configuration specific to the Rock Page to the observability
        /// activity.
        /// </summary>
        /// <param name="activity">The activity to be updated, this may be null.</param>
        /// <param name="requestContext">The context that contains information about the request.</param>
        /// <param name="isPostBack">If set to <c>true</c> then this is a legacy WebForms postback.</param>
        public static void ConfigureActivity( Activity activity, RockRequestContext requestContext, bool isPostBack = false )
        {
            if ( activity == null )
            {
                return;
            }

            // If the route has parameters show the route slug, otherwise use the request path
            if ( requestContext.PageReference.Parameters.Count > 0 )
            {
                activity.DisplayName = $"PAGE: {requestContext.HttpMethod} {requestContext.PageReference.Route}";
            }
            else
            {
                activity.DisplayName = $"PAGE: {requestContext.HttpMethod} {requestContext.RequestUri?.AbsolutePath}";
            }

            // Highlight postbacks
            if ( isPostBack )
            {
                activity.DisplayName += " [Postback]";
            }
            else
            {
                // Only add a metric if for non-postback requests
                var pageTags = RockMetricSource.CommonTags;
                pageTags.Add( "rock-page", requestContext.Page?.Id );
                pageTags.Add( "rock-site", requestContext.Page?.Layout.Site.Name );
                RockMetricSource.PageRequestCounter.Add( 1, pageTags );
            }

            // Add attributes
            activity.AddTag( "rock.otel_type", "rock-page" );
            activity.AddTag( "rock.current_user", requestContext.CurrentUser?.UserName );
            activity.AddTag( "rock.current_person", requestContext.CurrentPerson?.FullName );
            activity.AddTag( "rock.current_visitor", requestContext.CurrentVisitorId );
            activity.AddTag( "rock.site.id", requestContext.Page?.Layout.SiteId );
            activity.AddTag( "rock.page.id", requestContext.Page?.Id );
            activity.AddTag( "rock.page.ispostback", isPostBack );
            activity.AddTag( "rock.page.issystem", requestContext.Page?.IsSystem );
        }

        /// <summary>
        /// Gets the JavaScript block that is required to initialize the Obsidian
        /// environment. The Obsidian script bundles must also be imported manually.
        /// </summary>
        /// <param name="requestContext">The context that describes the current request.</param>
        /// <returns>A string of text that should be rendered inside a <c>&lt;script&gt;</c> block.</returns>
        public static string GetObsidianInitScript( RockRequestContext requestContext )
        {
            var currentPersonJson = "null";
            var isAnonymousVisitor = false;
            var currentPerson = requestContext.CurrentPerson;

            if ( currentPerson != null && currentPerson.Guid != new Guid( SystemGuid.Person.GIVER_ANONYMOUS ) )
            {
                currentPersonJson = new CurrentPersonBag
                {
                    IdKey = currentPerson.IdKey,
                    Guid = currentPerson.Guid,
                    PrimaryAliasIdKey = currentPerson.PrimaryAlias.IdKey,
                    PrimaryAliasGuid = currentPerson.PrimaryAlias.Guid,
                    FirstName = currentPerson.FirstName,
                    NickName = currentPerson.NickName,
                    LastName = currentPerson.LastName,
                    FullName = currentPerson.FullName,
                    Email = currentPerson.Email,
                }.ToCamelCaseJson( false, false );
            }
            else if ( currentPerson != null )
            {
                isAnonymousVisitor = true;
            }

            // Prevent XSS attacks in page parameters.
            var sanitizedPageParameters = new Dictionary<string, string>();
            foreach ( var pageParam in requestContext.PageParameters )
            {
                var sanitizedKey = pageParam.Key.Replace( "</", "<\\/" );
                var sanitizedValue = pageParam.Value.ToStringSafe().Replace( "</", "<\\/" );

                sanitizedPageParameters.AddOrReplace( sanitizedKey, sanitizedValue );
            }

            var trailblazerMode = SystemSettings.GetValue( SystemKey.SystemSetting.TRAILBLAZER_MODE ).AsBoolean();
            var fingerprint = RockApp.Current.GetRequiredService<ObsidianFingerprintManager>().GetFingerprint();

            return $@"
Obsidian.onReady(() => {{
    System.import('@Obsidian/Templates/rockPage.js').then(module => {{
        module.initializePage({{
            executionStartTime: new Date().getTime(),
            pageId: {requestContext.Page.Id},
            pageGuid: '{requestContext.Page.Guid}',
            pageParameters: {sanitizedPageParameters.ToJson()},
            sessionGuid: '{requestContext.SessionGuid}',
            interactionGuid: '{requestContext.RelatedInteractionGuid}',
            currentPerson: {currentPersonJson},
            isAnonymousVisitor: {( isAnonymousVisitor ? "true" : "false" )},
            loginUrlWithReturnUrl: '{requestContext.Page.Layout.Site.GetLoginUrlWithReturnUrl()}',
            trailblazerMode: {( trailblazerMode ? "true" : "false" )}
        }});
    }});
}});
Obsidian.init({{ debug: true, fingerprint: ""v={fingerprint}"" }});
";
        }

        /// <summary>
        /// Gets the JavaScript that enables shortcut key functionality for
        /// elements that have the <c>data-shortcut-key</c> attribute defined.
        /// </summary>
        /// <returns></returns>
        public static string GetShortcutKeyScript()
        {
            return @"
(function() {
    var lastDispatchTime = 0;
    var lastDispatchedElement = null;
    var debounceDelay = 500;

    document.addEventListener('keydown', function (event) {
        if (event.altKey) {
            var shortcutKey = event.key.toLowerCase();

            // Check if a shortcut key is registered for the pressed key
            var element = document.querySelector('[data-shortcut-key=""' + shortcutKey + '""]');

                    
            if (element) {
                var currentTime = performance.now();

                if (lastDispatchedElement === element && (currentTime - lastDispatchTime) < debounceDelay) {
                    return;
                }

                lastDispatchTime = currentTime;
                lastDispatchedElement = element;

                if (shortcutKey === 'arrowright' || shortcutKey === 'arrowleft') {
                    event.preventDefault();
                }

                event.preventDefault();
                element.click();
            }
        }
    });
})();
            ";

        }

        /// <summary>
        /// Gets the JavaScript content that should be added to each page when
        /// it is rendered.
        /// </summary>
        /// <returns>A string of text to be rendered inside a &lt;script&gt; tag.</returns>
        public static string GetJesusScript()
        {
            return $@"
console.info(
  '%cCrafting Code For Christ | Col. 3:23-24',
  'background: #ee7625; border-radius:0.5em; padding:0.2em 0.5em; color: white; font-weight: bold');
console.info('{_rockVersion}');
";
        }

        /// <summary>
        /// Gets the tags that make up the Google Analytics initialization
        /// script. This contains raw HTML script tags that should be added
        /// to the &lt;head&gt; section of the page.
        /// </summary>
        /// <param name="page">The page that is being rendered.</param>
        /// <returns>A string that contains the various script elements or null if Google Analytics is not configured for this site.</returns>
        public static string GetGoogleAnalyticsScriptTags( PageCache page )
        {
            var code = page.Layout.Site.GoogleAnalyticsCode ?? string.Empty;

            // Parse the list of codes, we want the "G-" codes to be first
            // because the first code is used as the default in the <script>
            // src property.
            var gtagCodes = code.Split( ',' )
                .Select( a => a.Trim() )
                .Where( a => a.StartsWith( "G-", StringComparison.OrdinalIgnoreCase ) )
                .ToList();

            // Add the measurement codes that start with 'UA' to the gtag script.
            // If there are multiple measurement IDs the first one is used as the
            // default.
            var uaCodes = code.Split( ',' )
                .Select( a => a.Trim() )
                .Where( a => a.StartsWith( "UA-", StringComparison.OrdinalIgnoreCase ) );

            gtagCodes.AddRange( uaCodes );

            if ( gtagCodes.Any() )
            {
                var sb = new StringBuilder();

                sb.Append( $@"
    <!-- BEGIN Global site tag (gtag.js) - Google Analytics -->
    <script async src=""https://www.googletagmanager.com/gtag/js?id={gtagCodes.First()}""></script>
    <script>
      window.dataLayer = window.dataLayer || [];
      function gtag(){{window.dataLayer.push(arguments);}}
      gtag('js', new Date());" );
                sb.AppendLine( "" );
                gtagCodes.ForEach( a => sb.AppendLine( $"      gtag('config', '{a}');" ) );
                sb.AppendLine( "    </script>" );
                sb.AppendLine( "    <!-- END Global site tag (gtag.js) - Google Analytics -->" );

                return sb.ToString();
            }

            return null;
        }

        /// <summary>
        /// Gets the HTML content that is used to render the Obsidian page
        /// timings visualizer.
        /// </summary>
        /// <returns>A string of HTML text or an empty string.</returns>
        public static string GetObsidianPageTimingsContent()
        {
            return Activity.Current != null
                ? $"<div id=\"lObsidianPageTimings\" data-trace-id=\"{Activity.Current.TraceId}\"></div>"
                : string.Empty;
        }

        /// <summary>
        /// Gets the JavaScript that initializes the page timings visualizer.
        /// </summary>
        /// <returns>A string of text that should be rendered inside a <c>&lt;script&gt;</c> block.</returns>
        public static string GetObsidianPageTimingsScript()
        {
            return @"
Obsidian.onReady(() => {
    System.import('@Obsidian/Templates/rockPage.js').then(module => {
        module.initializePageTimings({
            elementId: 'lObsidianPageTimings'
        });
    });
});";
        }

        /// <summary>
        /// Gets the script that initializes the Rock.settings object.
        /// </summary>
        /// <param name="requestContext">The context that describes the current request.</param>
        /// <returns>A string of text to be rendered inside a &lt;script&gt; tag.</returns>
        public static string GetRockSettingsInitializeScript( RockRequestContext requestContext )
        {
            var realTimeUrl = "/rock-rt";
            var realTimeHostname = SystemSettings.GetValue( SystemKey.SystemSetting.REALTIME_HOSTNAME );
            var pageCache = requestContext.Page;

            if ( realTimeHostname.IsNotNullOrWhiteSpace() )
            {
                try
                {
                    var requestUrl = System.Web.HttpContext.Current.Request.Url;
                    requestUrl = requestContext.RequestUri;

                    realTimeUrl = new UriBuilder
                    {
                        Scheme = requestUrl.Scheme,
                        Host = realTimeHostname,
                        Port = requestUrl.Port,
                        Path = "/rock-rt"
                    }.ToString();
                }
                catch ( Exception ex )
                {
                    RockLogger.LoggerFactory.CreateLogger( typeof( RockPageHelper ).FullName )
                        .LogError( ex, "Unable to create URL for real-time engine." );
                }
            }

            return $@"
Rock.settings.initialize({{
    siteId: {pageCache.Layout.SiteId},
    layoutId: {pageCache.LayoutId},
    pageId: {pageCache.Id},
    layout: '{pageCache.Layout.FileName}',
    baseUrl: '{requestContext.ResolveRockUrl( "~" )}',
    realTimeUrl: '{realTimeUrl}',
}});";
        }

        /// <summary>
        /// Gets the URL of the login page for the current site.
        /// </summary>
        /// <param name="requestContext">The context that describes the current request.</param>
        /// <returns>The URL of the login page.</returns>
        public static string GetLoginPageUrl( RockRequestContext requestContext )
        {
            var site = requestContext.Page.Layout.Site;

            if ( requestContext.CurrentPerson == null )
            {
                // If the user hasn't logged in yet, redirect to the login page.

                if ( site.LoginPageId.HasValue )
                {
                    return site.GetLoginUrlWithReturnUrl();
                }
                else
                {
#if NET472_OR_GREATER
                    return GetFormsLoginPage();
#endif
                }
            }

            // If the user has logged in, redirect to error page.
            // We can also get here if we couldn't determine a login page.
            if ( site.ErrorPage.IsNotNullOrWhiteSpace() )
            {
                return string.Format( "{0}?type=security", site.ErrorPage.TrimEnd( new char[] { '/' } ) );
            }
            else
            {
                return "~/Error.aspx?type=security";
            }
        }

#if NET472_OR_GREATER
        /// <summary>
        /// This is used on .NET Framework systems to get the Forms Authentication
        /// login page if it wasn't defined on the Site.
        /// </summary>
        /// <returns>The URL to use for the login page.</returns>
        internal static string GetFormsLoginPage()
        {
            var current = System.Web.HttpContext.Current;
            var loginComponents = System.Web.Security.FormsAuthentication.LoginUrl.Split( '?' );
            var loginUrl = loginComponents[0];

            var queryString = loginComponents.Length > 1
                ? loginComponents[1].ParseQueryString()
                : "".ParseQueryString();

            var originalUrl = System.Web.HttpUtility.UrlEncode( current.Request.RawUrl, current.Request.ContentEncoding );
            queryString["ReturnUrl"] = originalUrl;

            return $"{loginUrl}?{queryString.ToQueryString(true)}";
        }
#endif

        /// <summary>
        /// Gets the JavaScript that initializes the color mode (light, dark, system)
        /// for the site.
        /// </summary>
        /// <returns>A string of text that should be rendered inside a <c>&lt;script&gt;</c> block.</returns>
        public static string GetColorModeScript()
        {
            return @"
        (function () {
            var attr = 'theme';
            var states = ['light', 'dark', 'system'];
            var html = document.documentElement;

            // init state
            var saved = localStorage.getItem(attr);
            var currentIndex = Math.max(0, states.indexOf(saved));
            if ( saved == null ) {
                currentIndex = 2; // default to system
            }

            html.setAttribute( ""theme"", states[currentIndex] );
        })();
";
        }
    }
}
