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
using System.Web;

using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// Interactions in range Badge
    /// </summary>
    [Description( "Shows the number of interactions in a given date range." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Interaction In Range" )]

    [InteractionChannelField( "Interaction Channel", "The Interaction channel to use.", true, order: 0 )]
    [SlidingDateRangeField( "Date Range", "The date range in which the interactions were made.", required: false, order: 1 )]
    [LinkedPage( "Detail Page", "Select the page to navigate when the badge is clicked.", false, order: 2 )]
    [TextField( "Badge Icon CSS", "The CSS icon to use for the badge.", true, "fa-random", key: "BadgeIconCss", order:3 )]
    [TextField( "Badge Color", "The color of the badge (#ffffff).", true, "#0ab4dd", order: 4 )]
    public class InteractionsInRange : BadgeComponent
    {
        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            Guid? interactionChannelGuid = GetAttributeValue( badge, "InteractionChannel" ).AsGuid();
            string badgeColor = GetAttributeValue( badge, "BadgeColor" );
            
            if ( interactionChannelGuid.HasValue && !String.IsNullOrEmpty( badgeColor ) )
            {
                string dateRange = GetAttributeValue( badge, "DateRange" );
                string badgeIcon = GetAttributeValue( badge, "BadgeIconCss" );

                var interactionChannel = InteractionChannelCache.Get( interactionChannelGuid.Value );

                string detailPageUrl = string.Empty;

                if ( !String.IsNullOrEmpty( GetAttributeValue( badge, "DetailPage" ) ) )
                {
                    int pageId = PageCache.Get( Guid.Parse( GetAttributeValue( badge, "DetailPage" ) ) ).Id;

                    detailPageUrl = System.Web.VirtualPathUtility.ToAbsolute( $"~/page/{pageId}?ChannelId={interactionChannel.Id}" );
                }


                writer.Write( $"<div class='badge badge-interactioninrange badge-id-{badge.Id} fa-3x' data-toggle='tooltip' data-original-title=''>" );

                writer.Write( "</div>" );

                writer.Write($@"
                <script>
                    Sys.Application.add_load(function () {{
                                                
                        $.ajax({{
                                type: 'GET',
                                url: Rock.settings.get('baseUrl') + 'api/PersonBadges/InteractionsInRange/{Person.Id}/{interactionChannel.Id}/{HttpUtility.UrlEncode(dateRange)}' ,
                                statusCode: {{
                                    200: function (data, status, xhr) {{
                                    
                                var interactionCount = data;
                                var opacity = 0;
                                if(data===0){{
                                    opacity = 0.4;
                                }} else {{
                                    opacity = 1;    
                                }}
                                var linkUrl = '{detailPageUrl}';

                                 if (linkUrl != '') {{
                                                badgeContent = '<a href=\'' + linkUrl + '\'><span class=\'badge-content fa-layers fa-fw\' style=\'opacity:'+ opacity +'\'><i class=\'fas {badgeIcon} badge-icon\' style=\'color: {badgeColor}\'></i><span class=\'fa-layers-counter\'>'+ interactionCount +'</span></span></a>';
                                            }} else {{
                                                badgeContent = '<div class=\'badge-content fa-layers \' style=\'opacity:'+ opacity +'\'><i class=\'fas {badgeIcon} badge-icon\' style=\'color: {badgeColor}\'></i><span class=\'fa-layers-counter\'>'+ interactionCount +'</span></div>';
                                            }}

                                            $('.badge-interactioninrange.badge-id-{badge.Id}').html(badgeContent);
                                        }}
                                }},
                        }});
                    }});
                </script>
                
            " );

            }

        }
    }
}