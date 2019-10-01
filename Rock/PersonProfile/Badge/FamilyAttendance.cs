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

using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.PersonProfile.Badge
{

    /// <summary>
    /// FamilyAttendance Badge
    /// </summary>
    [Description( "Shows a chart of the attendance history with each bar representing one month." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Family Attendance" )]
    
    
    [IntegerField("Months To Display", "The number of months to show on the chart (default 24.)", false, 24)]
    [IntegerField("Minimum Bar Height", "The minimum height of a bar (in pixels). Useful for showing hint of bar when attendance was 0. (default 2.)", false, 2)]
    [BooleanField("Animate Bars", "Determine whether bars should animate when displayed.", true)]
    public class FamilyAttendance : BadgeComponent
    {
        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            int minBarHeight = GetAttributeValue(badge, "MinimumBarHeight").AsIntegerOrNull() ?? 2;
            int monthsToDisplay = GetAttributeValue(badge, "MonthsToDisplay").AsIntegerOrNull() ?? 24;
            
            string animateClass = string.Empty;

            if (GetAttributeValue(badge, "AnimateBars") == null || GetAttributeValue(badge, "AnimateBars").AsBoolean())
            {
                animateClass = " animate";
            }

            string tooltip = string.Empty;
            if ( Person.AgeClassification == AgeClassification.Child )
            {
                tooltip = $"{Person.NickName.ToPossessive().EncodeHtml()} attendance for the last 24 months. Each bar is a month.";
            }
            else
            {
                tooltip = "Family attendance for the last 24 months. Each bar is a month.";
            }

            writer.Write( String.Format( "<div class='badge badge-attendance{0} badge-id-{1}' data-toggle='tooltip' data-original-title='{2}'>", animateClass, badge.Id, tooltip ) );

            writer.Write("</div>");

            writer.Write(String.Format( @"
                <script>
                    Sys.Application.add_load(function () {{
                        
                        var monthNames = [ 'January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December' ];
                        
                        
                        $.ajax({{
                                type: 'GET',
                                url: Rock.settings.get('baseUrl') + 'api/PersonBadges/FamilyAttendance/{0}/{1}' ,
                                statusCode: {{
                                    200: function (data, status, xhr) {{
                                            var chartHtml = '<ul class=\'badge-attendance-chart list-unstyled\'>';
                                            $.each(data, function() {{
                                                var barHeight = (this.AttendanceCount / this.SundaysInMonth) * 100;
                                                if (barHeight < {2}) {{
                                                    barHeight = {2};
                                                }}
                                
                                                chartHtml += '<li title=\'' + monthNames[this.Month -1] + ' ' + this.Year +'\'><span style=\'height: ' + barHeight + '%\'></span></li>';                
                                            }});
                                            chartHtml += '</ul>';
                                            
                                            $('.badge-attendance.badge-id-{3}').html(chartHtml);

                                        }}
                                }},
                        }});
                    }});
                </script>
                
            ", Person.Id.ToString(), monthsToDisplay , minBarHeight, badge.Id ));

        }


    }
}
