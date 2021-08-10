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
using System.Text.Encodings.Web;
using Rock.Attribute;
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

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( BadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            if ( Person == null )
            {
                return;
            }

            writer.Write( $"<div class='badge badge-lastvisitonsite badge-id-{badge.Id}' data-toggle='tooltip' data-original-title=''>" );
            writer.Write( "</div>" );
        }

        /// <summary>
        /// Gets the java script.
        /// </summary>
        /// <param name="badge"></param>
        /// <returns></returns>
        protected override string GetJavaScript( BadgeCache badge )
        {
            if ( Person == null )
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
            var detailPageUrl = System.Web.VirtualPathUtility.ToAbsolute( $"~/page/{pageId}?PersonGuid={Person.Guid}&SiteId={siteId}" );

            return $@"
                $.ajax({{
                    type: 'GET',
                    url: Rock.settings.get('baseUrl') + 'api/Badges/LastVisitOnSite/{Person.Id}/{siteId}' ,
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
                                    badgeContent = '<a href=\'' + linkUrl + '\'><div class=\'badge-content ' + cssClass + '\'><i class=\'fa fa-desktop badge-icon\'></i><span class=\'duration\'>' + daysSinceVisit + '</span></div></a>';
                                }} else {{
                                    badgeContent = '<div class=\'badge-content ' + cssClass + '\'><i class=\'fa fa-desktop badge-icon\'></i><span class=\'duration\'>' + daysSinceVisit + '</span></div>';
                                }}

                                $('.badge-lastvisitonsite.badge-id-{badge.Id}').html(badgeContent);
                                $('.badge-lastvisitonsite.badge-id-{badge.Id}').attr('data-original-title', labelContent);
                            }}
                        }}
                    }},
                }});";
        }
    }
}
