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

namespace Rock.Badge.Component
{
    /// <summary>
    /// Family Weeks Attended In Duration Badge
    /// </summary>
    [Description( "Shows the number of times a family attended in a duration of weeks." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Family Weeks Attended In Duration" )]

    [IntegerField( "Duration", "The number of weeks to use for the duration (default 16.)", false, 16 )]
    public class FamilyWeeksAttendedInDuration : BadgeComponent
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

            int duration = GetAttributeValue( badge, "Duration" ).AsIntegerOrNull() ?? 16;

            writer.Write( string.Format( "<div class='badge badge-weeksattendanceduration badge-id-{0}' data-toggle='tooltip' data-original-title='Family attendance for the last {1} weeks.'>", badge.Id, duration ) );

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

            var duration = GetAttributeValue( badge, "Duration" ).AsIntegerOrNull() ?? 16;

            return string.Format( @"
                $.ajax({{
                    type: 'GET',
                    url: Rock.settings.get('baseUrl') + 'api/Badges/WeeksAttendedInDuration/{1}/{0}' ,
                    statusCode: {{
                        200: function (data, status, xhr) {{
                            var badgeHtml = '<div class=\'weeks-metric\'>';
                                            
                            badgeHtml += '<span class=\'weeks-attended\'>' + data + '</span><span class=\'week-duration\'>/{0}</span>';                
                            badgeHtml += '</div>';
                                            
                            $('.badge-weeksattendanceduration.badge-id-{2}').html(badgeHtml);
                        }}
                    }},
                }});", duration, Person.Id.ToString(), badge.Id );
        }
    }
}
