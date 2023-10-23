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
using System.Data;
using System.IO;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Badge.Component
{
    /// <summary>
    /// Attended Group Of Type Badge
    /// </summary>
    [Description( "This badge looks at the person's attendance of a provided group type and highlight if they have attended within the provided duration." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Attended Group Of Type" )]

    [GroupTypeField( "Group Type", "The type of group to use.", true, order: 1 )]
    [SlidingDateRangeField( "Date Range", "The date range in which the person attended.", required: false, order: 2 )]
    [CodeEditorField( "Lava Template", "The lava template to use for the badge display", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, order: 3, defaultValue:
        @"
{% if GroupType.IconCssClass and GroupType.IconCssClass != '' %}
  {% assign groupIcon = GroupType.IconCssClass %}
{% else %}
  {% assign groupIcon = 'fa fa-users' %}
{% endif %}

{% if DateRange and DateRange.Summary != '' %}
  {% capture dateRangeText %} in the {{ DateRange.Summary | Downcase }}{% endcapture %}
{% else %}
  {% assign dateRangeText = '' %}
{% endif %}

{% if Attendance and Attendance.Count > 0 %}
  {% assign iconColor = '#0ab4dd' %}
  {% capture tooltipText %}{{ Person.NickName }} has attended {{ Attendance.Count }} times{{ dateRangeText }}. The most recent of which was {{ Attendance.LastDateTime | Date:'MM/d/yyyy' }}.{% endcapture %}
{% else %}
  {% assign iconColor = '#c4c4c4' %}
  {% capture tooltipText %}{{ Person.NickName }} has not attended a group of type {{ GroupType.Name }}{{ dateRangeText }}.{% endcapture %}
{% endif %}

<div class='rockbadge rockbadge-grouptypeattendance rockbadge-id-{{Badge.Id}}' data-toggle='tooltip' data-original-title='{{ tooltipText }}'>
  <i class='badge-icon {{ groupIcon }}' style='color: {{ iconColor }}'></i>
</div>
" )]
    [Rock.SystemGuid.EntityTypeGuid( "2A6DB456-8D8F-4D82-BFE2-F4545204BD90")]
    public class GroupTypeAttendance : BadgeComponent
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

        /// <inheritdoc/>
        public override void Render( BadgeCache badge, IEntity entity, TextWriter writer )
        {
            if ( !( entity is Person person ) )
            {
                return;
            }

            Guid? groupTypeGuid = GetAttributeValue( badge, "GroupType" ).AsGuidOrNull();
            if ( groupTypeGuid.HasValue )
            {
                var lavaTemplate = this.GetAttributeValue( badge, "LavaTemplate" );
                var slidingDateRangeDelimitedValues = this.GetAttributeValue( badge, "DateRange" );
                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( slidingDateRangeDelimitedValues );
                var dateRangeSummary = SlidingDateRangePicker.FormatDelimitedValues( slidingDateRangeDelimitedValues );

                var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, null, new Lava.CommonMergeFieldsOptions() );
                mergeFields.Add( "Person", person );
                using ( var rockContext = new RockContext() )
                {
                    var groupType = GroupTypeCache.Get( groupTypeGuid.Value );
                    int groupTypeId = groupType?.Id ?? 0;
                    mergeFields.Add( "GroupType", groupType );
                    mergeFields.Add( "Badge", badge );
                    mergeFields.Add( "DateRange", new { Dates = dateRange, Summary = dateRangeSummary } );

                    var personAliasIds = person.Aliases.Select( a => a.Id ).ToList();

                    var attendanceQuery = new AttendanceService( rockContext )
                        .Queryable()
                        .Where( a =>
                            a.Occurrence.Group != null &&
                            a.Occurrence.Group.GroupTypeId == groupTypeId &&
                            a.DidAttend == true &&
                            personAliasIds.Contains( a.PersonAliasId.Value ) );

                    if ( dateRange.Start.HasValue )
                    {
                        attendanceQuery = attendanceQuery.Where( a => a.StartDateTime >= dateRange.Start.Value );
                    }

                    if ( dateRange.End.HasValue )
                    {
                        attendanceQuery = attendanceQuery.Where( a => a.StartDateTime < dateRange.End.Value );
                    }

                    var attendanceDateTimes = attendanceQuery.Select( a => a.StartDateTime ).ToList();

                    if ( attendanceDateTimes.Any() )
                    {
                        var attendanceResult = new
                        {
                            Count = attendanceDateTimes.Count(),
                            LastDateTime = attendanceDateTimes.Max()
                        };

                        mergeFields.Add( "Attendance", attendanceResult );
                    }

                    string output = lavaTemplate.ResolveMergeFields( mergeFields );

                    writer.Write( output );
                }
            }
        }
    }
}
