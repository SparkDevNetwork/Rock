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

using Microsoft.Extensions.DependencyInjection;

using Rock.Configuration;
using Rock.Net;
using Rock.Observability;
using Rock.ViewModels.Crm;

namespace Rock.Web
{
    /// <summary>
    /// Contains common logic and helper methods for rendering Rock Pages. This
    /// is meant to be used by both WebForms and next-gen implementations.
    /// </summary>
    internal static class RockPageHelper
    {
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
    }
}
