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

using Rock.Model;
using Rock.Data;
using System.Collections.Generic;
using System.Data;
using System;
using System.Diagnostics;

namespace Rock.PersonProfile.Badge
{
    // TODO: Update to return actual data

    /// <summary>
    /// FamilyAttendance Badge
    /// </summary>
    [Description( "Family Attendance Badge" )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Family Attendance" )]
    public class FamilyAttendance : BadgeComponent
    {
        //private int _chartHeight = 25;
        private int _minBarHeight = 2;
        
        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override System.Collections.Generic.Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = base.AttributeValueDefaults;
                defaults["Order"] = "7";
                return defaults;
            }
        }

        public override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            writer.Write("<div class='badge badge-attendance' data-original-title='Family attendance for the last 24 months. Each bar is a month.'>");

            writer.Write("</div>");

            writer.Write(@"
                <script>
                    $( document ).ready(function() {
                        
                        var monthNames = [ 'January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December' ];
                        
                        
                        $.ajax({
                                type: 'GET',
                                url: Rock.settings.get('baseUrl') + 'api/PersonBadges/FamilyAttendance/2',
                                statusCode: {
                                    200: function (data, status, xhr) {
                                            var chartHtml = '<ul class=\'badge-attendance-chart list-unstyled\'>';
                                            $.each(data, function() {
                                                var barHeight = (this.AttendanceCount / this.SundaysInMonth) * 100;
                                                if (barHeight < 2) {
                                                    barHeight = 2;
                                                }
                                
                                                chartHtml += '<li title=\'' + monthNames[this.Month -1] + ' ' + this.Year +'\'><span style=\'height: ' + barHeight + '%\'></span></li>';                
                                            });
                                            chartHtml += '</ul>';
                                            
                                            $('.badge-attendance').html(chartHtml);

                                        }
                                },
                        });
                    });
                </script>
                
            ");

        }


    }
}
