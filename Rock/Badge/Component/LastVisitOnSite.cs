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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Text.Encodings.Web;
using Rock.Attribute;
#if REVIEW_NET5_0_OR_GREATER
using Rock.Configuration;
#endif
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Badge.Component
{
    /// <summary>
    /// Last Visit on Site Badge
    /// </summary>
    [Description( "Badge showing the number of days since the person last visited a specified site." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Last Visit on Site" )]

    [SiteField( "Site", "Site to filter for.", true, "3", "", 1 )]
    [LinkedPage( "Page View Details", "Page to show the details of the page views. If blank no link is created.", false, "", "", 2 )]
    [Rock.SystemGuid.EntityTypeGuid( "A8619A37-5DB6-4CD1-AC5A-B2FD9AC80F67")]
    public class LastVisitOnSite : BadgeComponent
    {
        /// <summary>
        /// Determines of this badge component applies to the given type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public override bool DoesApplyToEntityType( string type )
        {
            return type.IsNullOrWhiteSpace() || typeof( Person ).FullName == type;
        }

        /// <inheritdoc/>
        public override void Render( BadgeCache badge, IEntity entity, TextWriter writer )
        {
            if ( !( entity is Person ) )
            {
                return;
            }

            writer.Write( $"<div class='rockbadge rockbadge-overlay rockbadge-lastvisitonsite rockbadge-id-{badge.Id}' data-toggle='tooltip' data-original-title=''>" );
            writer.Write( "</div>" );
        }

        /// <inheritdoc/>
        protected override string GetJavaScript( BadgeCache badge, IEntity entity )
        {
            if ( !( entity is Person person ) )
            {
                return null;
            }

            var siteId = GetAttributeValue( badge, "Site" ).AsIntegerOrNull();

            if ( !siteId.HasValue )
            {
                return null;
            }

            var site = SiteCache.Get( siteId.Value );

            if ( site == null )
            {
                return null;
            }

            var siteName = site.Name;

            if ( string.IsNullOrEmpty( GetAttributeValue( badge, "PageViewDetails" ) ) )
            {
                return null;
            }

            var pageId = PageCache.Get( Guid.Parse( GetAttributeValue( badge, "PageViewDetails" ) ) ).Id;

            // NOTE: Since this block shows a history of sites a person visited in Rock, use Person.Guid instead of Person.Id to reduce the risk of somebody manually editing the URL to see somebody else pageview history
#if REVIEW_WEBFORMS
            var detailPageUrl = System.Web.VirtualPathUtility.ToAbsolute( $"~/page/{pageId}?PersonGuid={person.Guid}&SiteId={siteId}" );
#else
            var detailPageUrl = RockApp.Current.ResolveRockUrl( $"~/page/{pageId}?PersonGuid={person.Guid}&SiteId={siteId}" );
#endif

            return $@"
                $.ajax({{
                    type: 'GET',
                    url: Rock.settings.get('baseUrl') + 'api/Badges/LastVisitOnSite/{person.Id}/{siteId}' ,
                    statusCode: {{
                        200: function (data, status, xhr) {{
                            var badgeHtml = '';
                            var daysSinceVisit = data;
                            var cssClass = '';
                            var linkUrl = '{JavaScriptEncoder.Default.Encode( detailPageUrl )}';
                            var badgeContent = '';
                            var labelContent = '';
                            var siteName = '{JavaScriptEncoder.Default.Encode( siteName )}';

                            if (daysSinceVisit >= 0 && daysSinceVisit < 1000) {{

                                labelContent = 'It has been ' + daysSinceVisit + ' day(s) since the last visit to the ' + siteName + ' site.';

                                if (daysSinceVisit == 0) {{
                                    daysSinceVisit = 'Today';
                                    cssClass = 'today';
                                    labelContent = 'Visited the ' + siteName + ' site today.';
                                }} else if (daysSinceVisit < 7) {{
                                    cssClass = 'very-recent';
                                }} else if (daysSinceVisit < 21 ) {{
                                    cssClass = 'recent';
                                }} else if (daysSinceVisit < 90 ) {{
                                    cssClass = 'moderate';
                                }} else if (daysSinceVisit < 365 ) {{
                                    cssClass = 'not-recent';
                                }} else {{
                                    cssClass = 'old';
                                }}

                                if (linkUrl != '') {{
                                    badgeContent = '<a href=\'' + linkUrl + '\' class=\'badge-content ' + cssClass + '\'><i class=\'fa fa-desktop badge-icon\'></i><span class=\'metric-value\'>' + daysSinceVisit + '</span></a>';
                                }} else {{
                                    badgeContent = '<div class=\'badge-content ' + cssClass + '\'><i class=\'fa fa-desktop badge-icon\'></i><span class=\'metric-value\'>' + daysSinceVisit + '</span></div>';
                                }}

                                $('.rockbadge-lastvisitonsite.rockbadge-id-{badge.Id}').html(badgeContent).attr('data-original-title', labelContent);
                            }}
                        }}
                    }},
                }});";
        }
    }
}
