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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;

using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Data;
using System.Collections.Generic;
using System.Data;
using System;
using System.Diagnostics;
using Rock.Web.Cache;

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// Last Visit on Site Badge
    /// </summary>
    [Description( "Badge showing the number of days since the person last visited a specified site." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Last Visit on Site" )]
    
    [SiteField("Site", "Site to filter for.", true, "3", "", 1)]
    [LinkedPage("Page View Details", "Page to show the details of the page views. If blank no link is created.", false, "", "", 2)]
    public class LastVisitOnSite : BadgeComponent
    {        
        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            int? siteId = GetAttributeValue( badge, "Site" ).AsIntegerOrNull();
            if ( siteId.HasValue )
            {
                var site = Rock.Web.Cache.SiteCache.Read( siteId.Value );
                if ( site != null )
                {
                    string siteName = site.Name;

                    //  create url for link to details
                    string detailPageUrl = string.Empty;

                    if ( !String.IsNullOrEmpty( GetAttributeValue( badge, "PageViewDetails" ) ) )
                    {
                        int pageId = Rock.Web.Cache.PageCache.Read( Guid.Parse( GetAttributeValue( badge, "PageViewDetails" ) ) ).Id;
                        detailPageUrl = System.Web.VirtualPathUtility.ToAbsolute( String.Format( "~/page/{0}?Person={1}&SiteId={2}", pageId, Person.UrlEncodedKey, siteId ) );
                    }

                    writer.Write( String.Format( "<div class='badge badge-lastvisitonsite badge-id-{0}' data-toggle='tooltip' data-original-title=''>", badge.Id ) );

                    writer.Write( "</div>" );

                    writer.Write( String.Format( @"
                <script>
                    Sys.Application.add_load(function () {{
                                                
                        $.ajax({{
                                type: 'GET',
                                url: Rock.settings.get('baseUrl') + 'api/PersonBadges/LastVisitOnSite/{0}/{1}' ,
                                statusCode: {{
                                    200: function (data, status, xhr) {{
                                        var badgeHtml = '';
                                        var daysSinceVisit = data;
                                        var cssClass = '';
                                        var linkUrl = '{3}';
                                        var badgeContent = '';
                                        var labelContent = '';
                                        var siteName = '{4}';

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
                                            
                                            

                                            $('.badge-lastvisitonsite.badge-id-{2}').html(badgeContent);
                                            $('.badge-lastvisitonsite.badge-id-{2}').attr('data-original-title', labelContent);
                                        }}
                                        
                                    }}
                                }},
                        }});
                    }});
                </script>
                
            ", Person.Id, siteId, badge.Id, detailPageUrl, siteName ) );
                }
            }
        }
    }

}
