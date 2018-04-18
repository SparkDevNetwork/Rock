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
    /// Badge showing the number of devices configured for the person.
    /// </summary>
    [Description( "Badge showing the number of devices configured for the person." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Personal Device" )]

    [LinkedPage( "Personal Devices Detail", "Page to show the details of the personal devices added.", false, order: 1 )]
    public class PersonalDevice : BadgeComponent
    {
        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {

            //  create url for link to details
            string detailPageUrl = string.Empty;

            if ( !String.IsNullOrEmpty( GetAttributeValue( badge, "PersonalDevicesDetail" ) ) )
            {
                int pageId = Rock.Web.Cache.PageCache.Read( Guid.Parse( GetAttributeValue( badge, "PersonalDevicesDetail" ) ) ).Id;

                // NOTE: Since this block shows a history of sites a person visited in Rock, use Person.Guid instead of Person.Id to reduce the risk of somebody manually editing the URL to see somebody else pageview history
                detailPageUrl = System.Web.VirtualPathUtility.ToAbsolute( $"~/page/{pageId}?PersonGuid={Person.Guid}" );
            }

            writer.Write( $"<div class='badge badge-personaldevice badge-id-{badge.Id}' data-toggle='tooltip' data-original-title=''>" );

            writer.Write( "</div>" );

            writer.Write( $@"
                <script>
                    Sys.Application.add_load(function () {{
                                                
                        $.ajax({{
                                type: 'GET',
                                url: Rock.settings.get('baseUrl') + 'api/PersonBadges/PersonalDevicesNumber/{Person.Id}' ,
                                statusCode: {{
                                    200: function (data, status, xhr) {{
                                        var badgeHtml = '';
                                        var devicesNumber = data;
                                        var cssClass = '';
                                        var linkUrl = '{detailPageUrl}';
                                        var badgeContent = '';
                                        var labelContent = '';

                                        if (devicesNumber > 0) {{
        
                                            labelContent = devicesNumber + ' device found.';                                    
        
                                            if (linkUrl != '') {{
                                                badgeContent = '<a href=\'' + linkUrl + '\'><div class=\'badge-content \'><i class=\'fa fa-mobile badge-icon\'></i><span class=\'deviceCount\'>' + devicesNumber + '</span></div></a>';
                                            }} else {{
                                                badgeContent = '<div class=\'badge-content \'><i class=\'fa fa-mobile badge-icon\'></i><span class=\'deviceCount\'>' + devicesNumber + '</span></div>';
                                            }}

                                            $('.badge-personaldevice.badge-id-{badge.Id}').html(badgeContent);
                                            $('.badge-personaldevice.badge-id-{badge.Id}').attr('data-original-title', labelContent);
                                            
                                        }}
                                        else {{
                                            $('.badge-personaldevice.badge-id-{badge.Id}').css('display', 'none');
                                            $('.badge-personaldevice.badge-id-{badge.Id}').html('');
                                        }}
                                        
                                    }}
                                }},
                        }});
                    }});
                </script>
            " );
        }
    }
}

