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
    public partial class UpdateGroupScheduleSystemEmails : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateSchedulingResponseEmailTemplateUp();
            UpdateSchedulingConfirmationSystemEmailUp();
            UpdateScehdulingReminderEmailUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            ResetSchedulingResponseEmailTemplateDown();
            ResetSchedulingConfirmationSystemEmailDown();
            UpdateScehdulingReminderEmailDown();
        }

        /// <summary>
        /// Updates the scheduling response email template.
        /// </summary>
        private void UpdateSchedulingResponseEmailTemplateUp()
        {
            RockMigrationHelper.UpdateSystemEmail( "Groups", "Scheduling Response Email", "", "", "", "", "", "{%- assign rsvp = ScheduledItem.RSVP | AsBoolean -%} {% if rsvp %}Accepted{% else %}Declined{% endif %}",
            @"{{ 'Global' | Attribute:'EmailHeader' }}
<h1>Scheduling Response</h1>
<p>Hi {{ Scheduler.NickName }}!</p>

<p>{{ Person.FullName }}{%- assign rsvp = ScheduledItem.RSVP | AsBoolean -%} {% if rsvp %} has confirmed and will be at the:{% else %} is unable to attend the: {% endif %}</p>

<h2>{{OccurrenceDate | Date:'dddd, MMMM d, yyyy'}}</h2>
{{ Group.Name }}
{{ ScheduledItem.Location.Name }} - {{ ScheduledItem.Schedule.Name }} ({{ ScheduledStartTime }})

<br/>
<br/>

{{ 'Global' | Attribute:'OrganizationName' }}  

{{ 'Global' | Attribute:'EmailFooter' }}
", "D095F78D-A5CF-4EF6-A038-C7B07E250611" );
        }

        /// <summary>
        /// Resets the scheduling response email template.
        /// </summary>
        private void ResetSchedulingResponseEmailTemplateDown()
        {
            RockMigrationHelper.UpdateSystemEmail( "Groups", "Scheduling Response Email", "", "", "", "", "", "{%- assign rsvp = ScheduledItem.RSVP | AsBoolean -%} {% if rsvp %}Accepted{% else %}Declined{% endif %}",
            @"{{ ""Global"" | Attribute:""EmailHeader"" }}
<h1>Scheduling Response</h1>
<p>Hi {{ Scheduler.NickName }}!</p>
<br/>
<p>{{ Person.FullName }}{%- assign rsvp = ScheduledItem.RSVP | AsBoolean -%} {% if rsvp %} has confirmed and will be at the:{%else %} is unable to attend the: {% endif %}</p>
<br/>
{{ Group.Name }}
{{ ScheduledItem.Location.Name }} {{ScheduledItem.Schedule.Name}}
<br/>
{{ ""Global"" | Attribute:""OrganizationName"" }}<br/>
<h2>{{ScheduledItem.Occurence.OccurenceDate | Date: ""dddd, MMMM, d, yyyy""}}</h2>
<p>&nbsp;</p>
{{ ""Global"" | Attribute:""EmailFooter"" }}", "D095F78D-A5CF-4EF6-A038-C7B07E250611" );
        }


        /// <summary>
        /// Updates the scheduling confirmation system email up.
        /// </summary>
        private void UpdateSchedulingConfirmationSystemEmailUp()
        {
            RockMigrationHelper.UpdateSystemEmail( "Groups", "Scheduling Confirmation Email", "", "", "", "", "", "Scheduling Confirmation", @"{{ 'Global' | Attribute:'EmailHeader' }}
<h1>Scheduling Confirmation</h1>
<p>Hi {{  Attendance.PersonAlias.Person.NickName  }}!</p>

<p>You have been added to the schedule for the following dates and times. Please let us know if you'll be attending as soon as possible.</p>

<p>Thanks!</p>
{{ Attendance.ScheduledByPersonAlias.Person.FullName  }}
<br/>
{{ 'Global' | Attribute:'OrganizationName' }}

<table>
{% for attendance in Attendances %}
    <tr><td>&nbsp;</td></tr>
    <tr><td><h5>{{attendance.Occurrence.OccurrenceDate | Date:'dddd, MMMM d, yyyy'}}</h5></td></tr>
    <tr><td>{{ attendance.Occurrence.Group.Name }}</td></tr>
    <tr><td>{{ attendance.Location.Name }}&nbsp;{{ attendance.Schedule.Name }}</td></tr>
    {% if forloop.first  %}
    <tr><td>  
        <!--[if mso]><v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}ScheduleConfirmation?attendanceId={{attendance.Id}}&Person={{Attendance.PersonAlias.Person | PersonActionIdentifier:'ScheduleConfirm'}}&isConfirmed=true"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""5%"" strokecolor=""#339933"" fillcolor=""#669966"">
    			<w:anchorlock/>
    			<center style=""color:#ffffff;font-family:sans-serif;font-size:18px;font-weight:normal;"">Accept</center>
    		  </v:roundrect>
    		<![endif]--><a style=""mso-hide:all; background-color:#669966;border:1px solid #339933;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:18px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"" href=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}ScheduleConfirmation?attendanceId={{attendance.Id}}&Person={{Attendance.PersonAlias.Person | PersonActionIdentifier:'ScheduleConfirm'}}&isConfirmed=true"">Accept</a>&nbsp;
    		
    	<!--[if mso]><v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}ScheduleConfirmation?attendanceId={{attendance.Id}}&Person={{Attendance.PersonAlias.Person | PersonActionIdentifier:'ScheduleConfirm'}}&isConfirmed=false"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""5%"" strokecolor=""#cc0000"" fillcolor=""#cc3333"">
    			<w:anchorlock/>
    			<center style=""color:#ffffff;font-family:sans-serif;font-size:18px;font-weight:normal;"">Decline</center>
    		  </v:roundrect>
    		<![endif]--><a style=""background-color:#cc3333;border:1px solid #cc0000;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:18px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"" href=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}ScheduleConfirmation?attendanceId={{attendance.Id}}&Person={{Attendance.PersonAlias.Person | PersonActionIdentifier:'ScheduleConfirm'}}&isConfirmed=false"">Decline</a>
    </td>
    </tr>
    <tr><td>&nbsp;</td></tr>
{% endif %}
{% endfor %}
</table>

<br/>

{{ 'Global' | Attribute:'EmailFooter' }}", "F8E4CE07-68F5-4169-A865-ECE915CF421C" );
        }

        /// <summary>
        /// Resets the scheduling response email template up.
        /// </summary>
        private void ResetSchedulingConfirmationSystemEmailDown()
        {
            RockMigrationHelper.UpdateSystemEmail( "Groups", "Scheduling Confirmation Email", "", "", "", "", "", "Scheduling Confirmation", @"{{ 'Global' | Attribute:'EmailHeader' }}
<h1>Scheduling Confirmation</h1>
<p>Hi {{  Attendance.PersonAlias.Person.NickName  }}!</p>

<p>You have been added to the schedule for the following dates and times. Please let us know if you'll be attending as soon as possible.</p>

<p>Thanks!</p>
{{ Attendance.ScheduledByPersonAlias.Person.FullName  }}
<br/>
{{ 'Global' | Attribute:'OrganizationName' }}

<table>
{% for attendance in Attendances %}
    <tr><td>&nbsp;</td></tr>
    <tr><td><h5>{{attendance.Occurrence.OccurrenceDate | Date:'dddd, MMMM, d, yyyy'}}</h5></td></tr>
    <tr><td>{{ attendance.Occurrence.Group.Name }}</td></tr>
    <tr><td>{{ attendance.Location.Name }}&nbsp;{{ attendance.Schedule.Name }}</td></tr>
    {% if forloop.first  %}
    <tr><td><a href=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}ScheduleConfirmation?attendanceId={{attendance.Id}}&isConfirmed=true"">Accept</a>&nbsp;<a href=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}ScheduleConfirmation?attendanceId={{attendance.Id}}&isConfirmed=false"">Decline</a></td>
    </tr>
    <tr><td>&nbsp;</td></tr>
{% endif %}
{% endfor %}
</table>

<br/>

{{ 'Global' | Attribute:'EmailFooter' }}", "F8E4CE07-68F5-4169-A865-ECE915CF421C" );
        }

        /// <summary>
        /// Updates the Scheduling Reminder Email's date formatting.
        /// </summary>
        private void UpdateScehdulingReminderEmailUp()
        {
            // Scheduling Reminder Email
            RockMigrationHelper.UpdateSystemEmail( "Groups", "Scheduling Reminder Email", "", "", "", "", "", "Scheduling Reminder for {{ Attendance.Occurrence.OccurrenceDate | Date:'dddd, MMMM d, yyyy' }}", @"{{ 'Global' | Attribute:'EmailHeader' }}
<h1>Scheduling Reminder</h1>
<p>Hi {{  Attendance.PersonAlias.Person.NickName  }}!</p>

<p>This is just a reminder that you are scheduled for the following on {{ Attendance.Occurrence.OccurrenceDate | Date:'dddd, MMMM d, yyyy' }} </p>

<p>Thanks!</p>
{{ Attendance.ScheduledByPersonAlias.Person.FullName  }}
<br/>
{{ 'Global' | Attribute:'OrganizationName' }}

<table>
{% for attendance in Attendances %}
    <tr><td>&nbsp;</td></tr>
    <tr><td><strong>{{ attendance.Occurrence.OccurrenceDate | Date:'dddd, MMMM d, yyyy' }}</strong></td></tr>
    <tr><td><strong>{{ attendance.Occurrence.Group.Name }}</strong></td></tr>
    <tr><td>{{ attendance.Location.Name }}&nbsp;{{ attendance.Schedule.Name }}</td></tr>
{% endfor %}
</table>

<br/>

{{ 'Global' | Attribute:'EmailFooter' }}", "8A20FE79-B73C-447A-82B1-416F9B50C038" );
        }

        /// <summary>
        /// Updates the Scheduling Reminder Email back to the previous version.
        /// </summary>
        private void UpdateScehdulingReminderEmailDown()
        {
            // Scheduling Reminder Email
            RockMigrationHelper.UpdateSystemEmail( "Groups", "Scheduling Reminder Email", "", "", "", "", "", "Scheduling Reminder for {{Attendance.Occurrence.OccurrenceDate | Date:'dddd, MMMM, d, yyyy'}}", @"{{ 'Global' | Attribute:'EmailHeader' }}
<h1>Scheduling Reminder</h1>
<p>Hi {{  Attendance.PersonAlias.Person.NickName  }}!</p>

<p>This is just a reminder that you are scheduled for the following on {{Attendance.Occurrence.OccurrenceDate | Date:'dddd, MMMM, d, yyyy'}} </p>

<p>Thanks!</p>
{{ Attendance.ScheduledByPersonAlias.Person.FullName  }}
<br/>
{{ 'Global' | Attribute:'OrganizationName' }}

<table>
{% for attendance in Attendances %}
    <tr><td>&nbsp;</td></tr>
    <tr><td><strong>{{attendance.Occurrence.OccurrenceDate | Date:'dddd, MMMM, d, yyyy'}}</strong></td></tr>
    <tr><td><strong>{{ attendance.Occurrence.Group.Name }}</strong></td></tr>
    <tr><td>{{ attendance.Location.Name }}&nbsp;{{ attendance.Schedule.Name }}</td></tr>
{% endfor %}
</table>

<br/>

{{ 'Global' | Attribute:'EmailFooter' }}", "8A20FE79-B73C-447A-82B1-416F9B50C038" );
        }

    }
}
