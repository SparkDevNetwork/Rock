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
    public partial class FixGroupScheduleConfirmationSystemCommunication : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
UPDATE
	[SystemCommunication]
SET
	[Body] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<h1>Scheduling Confirmation</h1>
<p>Hi {{  Attendance.PersonAlias.Person.NickName  }}!</p>

<p>You have been added to the schedule for the following dates and times. Please let us know if you''ll attend as soon as possible.</p>

<p>Thanks!</p>
{{ Attendance.ScheduledByPersonAlias.Person.FullName  }}
<br/>
{{ ''Global'' | Attribute:''OrganizationName'' }}

<table>

{% assign acceptText = ''Accept'' %}
{% assign declineText = ''Decline'' %}
{% if Attendances.size > 1 %}
    {% assign acceptText = ''Accept All'' %}
    {% assign declineText = ''Decline All'' %}
{% endif %}
{% capture attendanceIdList %}{% for attendance in Attendances %}{{ attendance.Id }}{% unless forloop.last %},{% endunless %}{% endfor %}{% endcapture %}
{% assign lastDate = '''' %}

{% for attendance in Attendances %}

  {% assign currentDate = attendance.Occurrence.OccurrenceDate | Date:''dddd, MMMM d, yyyy'' %}
  {% if lastDate != currentDate %}
    {% if lastDate != '''' %}
    <tr><td><hr /></td></tr>
    {% endif %}
    <tr><td><h5>{{ currentDate }}</h5></td></tr>
    {% assign lastDate = currentDate %}
  {% else %}
    <tr><td>&nbsp;</td></tr>
  {% endif %}

    <tr><td>{{ attendance.Occurrence.Group.Name }}</td></tr>
    <tr><td>{{ attendance.Occurrence.Location.Name }}&nbsp;{{ attendance.Occurrence.Schedule.Name }}</td></tr>

  {% assign AttendancePerson = Attendance.PersonAlias.Person %}

{% endfor %}

    <tr><td><hr /></td></tr>
    <tr><td>
        <!--[if mso]><v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}ScheduleConfirmation?attendanceIds={{attendance.Id}}&Person={{AttendancePerson | PersonActionIdentifier:''ScheduleConfirm''}}&isConfirmed=true"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""5%"" strokecolor=""#339933"" fillcolor=""#669966"">
    			<w:anchorlock/>
    			<center style=""color:#ffffff;font-family:sans-serif;font-size:18px;font-weight:normal;"">{{ acceptText }}</center>
    		  </v:roundrect>
    		<![endif]--><a style=""mso-hide:all; background-color:#669966;border:1px solid #339933;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:18px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"" href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}ScheduleConfirmation?attendanceIds={{attendanceIdList | UrlEncode}}&Person={{AttendancePerson| PersonActionIdentifier:''ScheduleConfirm''}}&isConfirmed=true"">{{ acceptText }}</a>&nbsp;
    		
    	<!--[if mso]><v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}ScheduleConfirmation?attendanceId={{attendance.Id}}&Person={{Attendance.PersonAlias.Person | PersonActionIdentifier:''ScheduleConfirm''}}&isConfirmed=false"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""5%"" strokecolor=""#cc0000"" fillcolor=""#cc3333"">
    			<w:anchorlock/>
    			<center style=""color:#ffffff;font-family:sans-serif;font-size:18px;font-weight:normal;"">{{ declineText }}</center>
    		  </v:roundrect>
    		<![endif]--><a style=""background-color:#cc3333;border:1px solid #cc0000;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:18px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"" href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}ScheduleConfirmation?attendanceIds={{attendanceIdList | UrlEncode}}&Person={{Attendance.PersonAlias.Person | PersonActionIdentifier:''ScheduleConfirm''}}&isConfirmed=false"">{{ declineText }}</a>
    </td>
    </tr>
    <tr><td>&nbsp;</td></tr>
    <tr><td><a href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}ScheduleToolbox"">View Schedule Toolbox</a></td></tr>
    <tr><td>&nbsp;</td></tr>
</table>

<br/>

{{ ''Global'' | Attribute:''EmailFooter'' }}'
WHERE
	[Guid] = 'F8E4CE07-68F5-4169-A865-ECE915CF421C'
	AND	CHECKSUM([Body]) = 361247703;" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
