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
    public partial class UpdateAttendanceSummary : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attendance Summary Notification
            RockMigrationHelper.UpdateSystemEmail("Groups", "Attendance Summary Notification", "", "", "", "", "", "{{ Group.Name }} Attendance Summary : {{ AttendanceOccurrence.OccurrenceDate | Date:'M/d/yyyy' }}", @"{{ 'Global' | Attribute:'EmailHeader' }}
<h1 style=""color:#484848;line-height:1.3;word-break:normal;hyphens:none;-moz-hyphens:none;-webkit-hyphens:none;font-size:32px;-ms-hyphens:none;padding-bottom:8px;margin-bottom:0 !important;"">{{ Group.Name }} Attendance Summary</h1>
<p style=""padding:0;margin:0;line-height:1.4;font-weight:300;color:#484848;font-size:18px;hyphens:none;-ms-hyphens:none;-webkit-hyphens:none;-moz-hyphens:none;text-align:left;margin-bottom:0px !important;"">Attendance for {{ AttendanceOccurrence.OccurrenceDate | Date: 'MMMM d, yyyy a\t h:mm tt' | Remove:':00' | Replace:'AM','am' | Replace:'PM','pm' }}, entered by <a style=""color:#484848;"" href=""{{ 'Global' | Attribute:'InternalApplicationRoot' }}/person/{{ CurrentPerson.Id }}"">{{ CurrentPerson.FullName }}</a></p>
{%- if AttendanceOccurrence.DidNotOccur == true -%}
The {{ Group.Name }} group did not meet.
{%- else -%}
{%- if AttendanceOccurrence.Notes -%}
<p>{{ AttendanceNoteLabel }}: <br/> {{ AttendanceOccurrence.Notes }}</p>
{%- endif -%}
{%- assign attendeesCount = 0 -%}
{%- assign absenteeCount = 0 -%}
{%- assign attendeeRows = '' -%}
{%- assign absenteeRows = '' -%}


{%- for attendee in AttendanceOccurrence.Attendees -%}
    {%- if attendee.DidAttend == true -%}
        {%- assign attendeesCount = attendeesCount | Plus:1 -%}
        {%- capture attender -%}
        <tr style=""border: 1px solid #c4c4c4;"">
            <td bgcolor=""#ffffff"" align=""left"" style=""color:#000000; padding:4px 8px; border:1px solid #c4c4c4;"">
                <a style=""color:#000000;text-decoration:none"" href=""{{ 'Global' | Attribute:'InternalApplicationRoot' }}/person/{{ attendee.PersonAlias.Person.Id }}"">{{ attendee.PersonAlias.Person.FullName }}</a>
            </td>
        </tr>
        {%- endcapture -%}
        {%- assign attendeeRows = attendeeRows | Append:attender -%}
    {%- else -%}
        {%- assign absenteeCount = absenteeCount | Plus:1 -%}
        {%- capture absentee -%}
        <tr style=""border: 1px solid #c4c4c4;"">
            <td bgcolor=""#ffffff"" align=""left"" style=""color:#000000; padding:4px 8px; border:1px solid #c4c4c4;"">
                <a style=""color:#000000;text-decoration:none"" href=""{{ 'Global' | Attribute:'InternalApplicationRoot' }}/person/{{ attendee.PersonAlias.Person.Id }}"">{{ attendee.PersonAlias.Person.FullName }}</a>
            </td>
        </tr>
        {%- endcapture -%}
        {%- assign absenteeRows = absenteeRows | Append:absentee -%}
    {%- endif -%}
{%- endfor- %}

<table style=""border: 1px solid #c4c4c4; border-collapse:collapse; mso-table-lspace:0pt; mso-table-rspace:0pt; width: 100%; margin-bottom: 24px;"" cellspacing=""0"" cellpadding=""4"">
{%- if attendeeRows != '' -%}
    <tr>
        <td bgcolor=""#a6a5a5"" align=""left"" style=""color: #ffffff; padding: 4px 8px; border: 1px solid #c4c4c4;"">
            <h4 style=""color: #ffffff;"">Attended</h4>
        </td>
    </tr>
    {{ attendeeRows }}
{%- endif -%}
{%- if absenteeRows != '' -%}
    <tr>
        <td bgcolor=""#a6a5a5"" align=""left"" style=""color: #ffffff; padding: 4px 8px; border: 1px solid #c4c4c4;"">
            <h4 style=""color: #ffffff;"">Absent</h4>
        </td>
    </tr>
    {{ absenteeRows }}
{%- endif -%}
</table>
{%- endif -%}

<p>&nbsp;</p>
{{ 'Global' | Attribute:'EmailFooter' }}
", "CA794BD8-25C5-46D9-B7C2-AD8190AC27E6");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
