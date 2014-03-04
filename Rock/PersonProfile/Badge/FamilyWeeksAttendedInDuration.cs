// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    // TODO: Update to return actual data

    /// <summary>
    /// FamilyAttendance Badge
    /// </summary>
    [Description( "Shows a the number of times a family attended in a duration of weeks." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Family Weeks Attended In Duration" )]
    
    
    [IntegerField("Duration", "The number of weeks to use for the duration (default 16.)", false, 16)]
    public class FamilyWeeksAttendedInDuration : BadgeComponent
    {        
        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            writer.Write(String.Format("<div class='badge badge-weeksattendanceduration' data-original-title='Family attendance for the last {0} weeks.'>", GetAttributeValue(badge, "Duration")));

            writer.Write("</div>");

            writer.Write(String.Format(@"
                <script>
                    $( document ).ready(function() {{
                                                
                        $.ajax({{
                                type: 'GET',
                                url: Rock.settings.get('baseUrl') + 'api/PersonBadges/WeeksAttendedInDuration/{1}/{0}' ,
                                statusCode: {{
                                    200: function (data, status, xhr) {{
                                            var badgeHtml = '<div class=\'weeks-metric\'>';
                                            
                                            badgeHtml += '<span class=\'weeks-attended\'>' + data + '</span><span class=\'week-duration\'>/{0}</span>';                
                                            badgeHtml += '</div>';
                                            
                                            $('.badge-weeksattendanceduration').html(badgeHtml);

                                        }}
                                }},
                        }});
                    }});
                </script>
                
            ", GetAttributeValue(badge, "Duration"), Person.Id.ToString() ));

        }


    }
}
