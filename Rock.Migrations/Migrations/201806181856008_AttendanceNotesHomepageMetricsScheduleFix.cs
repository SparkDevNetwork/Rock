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
    public partial class AttendanceNotesHomepageMetricsScheduleFix : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.AttendanceOccurrence", "Notes", c => c.String());
            AddColumn("dbo.AttendanceOccurrence", "AnonymousAttendanceCount", c => c.Int());

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

            Sql( @"
INSERT INTO [dbo].[Schedule]
    ([Name]
    ,[Description]
    ,[iCalendarContent]
    ,[CheckInStartOffsetMinutes]
    ,[CheckInEndOffsetMinutes]
    ,[EffectiveStartDate]
    ,[EffectiveEndDate]
    ,[CategoryId]
    ,[Guid]
    ,[ForeignKey]
    ,[WeeklyDayOfWeek]
    ,[WeeklyTimeOfDay]
    ,[ForeignGuid]
    ,[ForeignId]
    ,[IsActive])
VALUES
('' ,NULL ,'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180507T010001
DTSTAMP:20180509T164128Z
DTSTART:20180507T010000
RRULE:FREQ=DAILY
SEQUENCE:0
UID:34d27dff-e110-47b0-9147-bebd312aae67
END:VEVENT
END:VCALENDAR
' ,NULL ,NULL ,'2018-05-07' ,'2018-05-07' ,( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '5A794741-5444-43F0-90D7-48E47276D426' ) , '9CDB85EC-CF0D-4AC2-81C0-266A7DBAFA06', NULL ,NULL ,NULL ,NULL ,NULL ,1 )" );

            Sql( @"
INSERT INTO [dbo].[Schedule]
    ([Name]
    ,[Description]
    ,[iCalendarContent]
    ,[CheckInStartOffsetMinutes]
    ,[CheckInEndOffsetMinutes]
    ,[EffectiveStartDate]
    ,[EffectiveEndDate]
    ,[CategoryId]
    ,[Guid]
    ,[ForeignKey]
    ,[WeeklyDayOfWeek]
    ,[WeeklyTimeOfDay]
    ,[ForeignGuid]
    ,[ForeignId]
    ,[IsActive])
VALUES
('' ,NULL ,'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180508T010001
DTSTAMP:20180509T164716Z
DTSTART:20180508T010000
RRULE:FREQ=DAILY
SEQUENCE:0
UID:72042074-e62b-4403-a999-2853d80aabba
END:VEVENT
END:VCALENDAR
' ,NULL ,NULL ,'2018-05-08' ,'2018-05-08' ,( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '5A794741-5444-43F0-90D7-48E47276D426' ) , '717D75F1-644F-45A4-B25E-64652A270AD9', NULL ,NULL ,NULL ,NULL ,NULL ,1 )
" );

            Sql( @"
INSERT INTO [dbo].[Schedule]
    ([Name]
    ,[Description]
    ,[iCalendarContent]
    ,[CheckInStartOffsetMinutes]
    ,[CheckInEndOffsetMinutes]
    ,[EffectiveStartDate]
    ,[EffectiveEndDate]
    ,[CategoryId]
    ,[Guid]
    ,[ForeignKey]
    ,[WeeklyDayOfWeek]
    ,[WeeklyTimeOfDay]
    ,[ForeignGuid]
    ,[ForeignId]
    ,[IsActive])
VALUES
('' ,NULL ,'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180508T010001
DTSTAMP:20180509T165611Z
DTSTART:20180508T010000
RRULE:FREQ=DAILY
SEQUENCE:0
UID:649969c3-118c-4021-a4b4-c47212d4f357
END:VEVENT
END:VCALENDAR
' ,NULL ,NULL ,'2018-05-08' ,'2018-05-08' ,( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '5A794741-5444-43F0-90D7-48E47276D426' ) , 'C376AF3C-5356-43BE-ACAC-3846D490CF08', NULL ,NULL ,NULL ,NULL ,NULL ,1 )
" );

            Sql( @"
UPDATE [dbo].[Metric]
SET
[ScheduleId] = ( SELECT TOP 1 [Id] FROM [dbo].[Schedule] WHERE [Guid] = '9CDB85EC-CF0D-4AC2-81C0-266A7DBAFA06' )
WHERE [Guid] = 'ecb1b552-9a3d-46fc-952b-d57dbc4a329d'" );

            Sql( @"
UPDATE [dbo].[Metric]
SET
[ScheduleId] = ( SELECT TOP 1 [Id] FROM [dbo].[Schedule] WHERE [Guid] = '717D75F1-644F-45A4-B25E-64652A270AD9' )
WHERE [Guid] = '491061b7-1834-44da-8ea1-bb73b2d52ad3'" );

            Sql( @"
UPDATE [dbo].[Metric]
SET
[ScheduleId] = ( SELECT TOP 1 [Id] FROM [dbo].[Schedule] WHERE [Guid] = 'C376AF3C-5356-43BE-ACAC-3846D490CF08' )
WHERE [Guid] = 'f0a24208-f8ac-4e04-8309-1a276885f6a6'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.AttendanceOccurrence", "AnonymousAttendanceCount");
            DropColumn("dbo.AttendanceOccurrence", "Notes");
        }
    }
}
