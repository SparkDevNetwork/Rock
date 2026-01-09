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

using Rock.Configuration;
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
    }
}
