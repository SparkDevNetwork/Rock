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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;

namespace Rock.Badge.Component
{
    /// <summary>
    /// FamilyAttendance Badge
    /// </summary>
    [Description( "Shows a chart of the attendance history with each bar representing one month." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Family Attendance" )]

    [IntegerField( "Months To Display",
        Description = "The number of months to show on the chart (default 24.)",
        IsRequired = false,
        DefaultIntegerValue = 24,
        Key = AttributeKey.MonthsToDisplay,
        Order = 0)]

    [IntegerField( "Minimum Bar Height",
        Description = "The minimum height of a bar (in pixels). Useful for showing hint of bar when attendance was 0. (default 2.)",
        IsRequired = false,
        DefaultIntegerValue = 2,
        Key = AttributeKey.MinimumBarHeight,
        Order = 1)]

    [BooleanField( "Animate Bars",
        Description = "Determine whether bars should animate when displayed.",
        DefaultBooleanValue = true,
        Key = AttributeKey.AnimateBars,
        Order = 2)]

    [BooleanField(
        "Adult Attendance Type",
        Description = "When viewing an adult profile with this badge, should the graph reflect the weekend attendance of anyone in the family, or just their own? Most churches don't have weekend attendance records for adults, in which case you should leave this set to \"Family\". Child profiles will always display only individual attendance in the graph.",
        TrueText = "Individual",
        FalseText = "Family",
        IsRequired = false,
        Key = AttributeKey.AdultAttendanceType,
        Order = 3 )]

    [Rock.SystemGuid.EntityTypeGuid( "78F5527E-0E90-4AC9-AAAB-F8F2F061BDFB" )]
    public class FamilyAttendance : BadgeComponent
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
        /// The block setting attribute keys for the <see cref="FamilyAttendance"/> block.
        /// </summary>
        private static class AttributeKey
        {
            public const string MonthsToDisplay = "Months To Display";
            public const string MinimumBarHeight = "Minimum Bar Height";
            public const string AnimateBars = "Animate Bars";
            public const string AdultAttendanceType = "AdultAttendanceType";
        }

        /// <inheritdoc/>
        public override void Render( BadgeCache badge, IEntity entity, TextWriter writer )
        {
            if ( !( entity is Person person ) )
            {
                return;
            }

            string animateClass = string.Empty;

            if ( GetAttributeValue( badge, AttributeKey.AnimateBars ) == null || GetAttributeValue( badge, "AnimateBars" ).AsBoolean() )
            {
                animateClass = " animate";
            }

            string tooltip;
            var monthsToDisplay = GetAttributeValue( badge, AttributeKey.MonthsToDisplay ).AsIntegerOrNull() ?? 24;
            var showAsIndividual = GetAttributeValue( badge, AttributeKey.AdultAttendanceType ).AsBoolean();

            if ( showAsIndividual || person.AgeClassification == AgeClassification.Child )
            {
                tooltip = $"{person.NickName.ToPossessive().EncodeHtml()} attendance for the last {monthsToDisplay} months. Each bar is a month.";
            }
            else
            {
                tooltip = $"Family attendance for the last {monthsToDisplay} months. Each bar is a month.";
            }

            writer.Write( string.Format( "<div class='rockbadge rockbadge-attendance{0} rockbadge-id-{1}' data-toggle='tooltip' data-original-title='{2}'>", animateClass, badge.Id, tooltip ) );

            writer.Write( "</div>" );
        }

        /// <inheritdoc/>
        protected override string GetJavaScript( BadgeCache badge, IEntity entity )
        {
            var minBarHeight = GetAttributeValue( badge, AttributeKey.MinimumBarHeight ).AsIntegerOrNull() ?? 2;
            var monthsToDisplay = GetAttributeValue( badge, AttributeKey.MonthsToDisplay ).AsIntegerOrNull() ?? 24;
            var showAsIndividual = GetAttributeValue( badge, AttributeKey.AdultAttendanceType ).AsBoolean();
            var personId = ( entity as Person )?.Id ?? 0;

            return
$@"var monthNames = [ 'January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December' ];

$.ajax({{
    type: 'GET',
    url: Rock.settings.get('baseUrl') + 'api/Badges/FamilyAttendance/{personId}/{monthsToDisplay}/{showAsIndividual}' ,
    statusCode: {{
        200: function (data, status, xhr) {{
            var chartHtml = '<ul class=\'attendance-chart trend-chart list-unstyled\'>';
            $.each(data, function() {{
                var barHeight = (this.AttendanceCount / this.SundaysInMonth) * 100;
                if (barHeight < {minBarHeight}) {{
                    barHeight = {minBarHeight};
                }}

                chartHtml += '<li title=\'' + monthNames[this.Month -1] + ' ' + this.Year +'\'><span style=\'height: ' + barHeight + '%\'></span></li>';
            }});
            chartHtml += '</ul>';

            $('.rockbadge-attendance.rockbadge-id-{badge.Id}').html(chartHtml);
        }}
    }},
}});";
        }
    }
}
