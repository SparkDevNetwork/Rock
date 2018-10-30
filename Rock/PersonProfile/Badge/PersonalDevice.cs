﻿// <copyright>
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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Model;
using Rock.Web;

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
            if ( Person != null )
            {
                //  create url for link to details
                string detailPageUrl = new PageReference( GetAttributeValue( badge, "PersonalDevicesDetail" ), new Dictionary<string, string> { { "PersonGuid", Person.Guid.ToString() } } ).BuildUrl();

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
        
                                            if ( devicesNumber > 1 ) {{
                                                labelContent = 'There are ' + devicesNumber + ' devices linked to this individual.';                                 
                                            }} else {{
                                                labelContent = 'There is 1 device linked to this individual.';
                                            }}
        
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
}

