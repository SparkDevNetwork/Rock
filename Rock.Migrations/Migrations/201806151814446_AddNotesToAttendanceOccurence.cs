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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class AddNotesToAttendanceOccurence : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.AttendanceOccurrence", "Notes", c => c.String() );
            AddColumn( "dbo.AttendanceOccurrence", "AnonymousAttendanceCount", c => c.Int() );

            // MP: Job Notification System Email
            RockMigrationHelper.UpdateSystemEmail( "System", "Attendance Notification", "", "", "", "", "", "{{ Group.Name }} Attendance Summary : {{ AttendanceOccurrence.OccurrenceDate | Date:'M/d/yyyy' }}", @"
{{ 'Global' | Attribute:'EmailHeader' }}
<p>Group Name: {{ Group.Name }}</p>
<p>Attendance Date: {{ AttendanceOccurrence.OccurrenceDate | Date:'M/d/yyyy' }}</p>
<p>Entered By: {{ CurrentPerson.FullName }}</p>
{% if AttendanceOccurrence.Notes %}
<p>{{ AttendanceNoteLabel }}: <br/> {{ AttendanceOccurrence.Notes }}</p>
{% endif %}
{% assign attendeesCount = AttendanceOccurrence.Attendees | Size %}
{% if attendeesCount > 0 %}
<table style=""border: 1px solid #c4c4c4; border-collapse:collapse; mso-table-lspace:0pt; mso-table-rspace:0pt; width: 100%; margin-bottom: 24px;"" cellspacing=""0"" cellpadding=""4"">
<td colspan=""2"" bgcolor=""#a6a5a5"" align=""left"" style=""color: #ffffff; padding: 4px 8px; border: 1px solid #c4c4c4;"">
<h4>Attendees:</h4>
</td>
{% endif %}
{% for attendee in AttendanceOccurrence.Attendees %}
    {% if attendee.DidAttend != null and attendee.DidAttend == true %}
        {% assign attended = 'X' %}
    {% else %}
        {% assign attended = ' ' %}
    {% endif %}
        <tr style=""border: 1px solid #c4c4c4;"">
            <td colspan=""2"" bgcolor=""#a6a5a5"" align=""left"" style=""color: #ffffff; padding: 4px 8px; border: 1px solid #c4c4c4;"">
                <p>
                   [{{ attended }}] {{ attendee.PersonAlias.Person.FullName }}
                </p>
            </td>
        </tr>
{% endfor %}
{% if attendeesCount > 0 %}
</table>
&nbsp;
{% endif %}

<p>&nbsp;</p>
{{ 'Global' | Attribute:'EmailFooter' }}
", Rock.SystemGuid.SystemEmail.ATTENDANCE_NOTIFICATION );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.AttendanceOccurrence", "AnonymousAttendanceCount" );
            DropColumn( "dbo.AttendanceOccurrence", "Notes" );
        }
    }
}
