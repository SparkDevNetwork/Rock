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
    public partial class AddGroupAttendanceDigestSystemCommunication : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateSystemCommunication( "Groups",
                "Group Attendance Digest",
                "", // from
                "", // fromName
                "", // to
                "", // cc
                "", // bcc
                "Group Attendance Digest", // subject
                // body
                @"{{ 'Global' | Attribute:'EmailHeader' }}

<p>
    Below is the attendance summary for groups that meet from {{ StartDate | Date:'M/d/yyyy' }} - {{ EndDate | DateAdd:-1 | Date:'M/d/yyyy' }}.
</p>

<p>
    Please review each note and reply to the group leader if needed.
</p>

<table style=""border: 1px solid #c4c4c4; border-collapse: collapse; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; margin-bottom: 24px;"" cellspacing=""0"" cellpadding=""4"">
    <thead>
        <tr style=""border: 1px solid #c4c4c4;"">
            <th style=""background-color: #a6a5a5; color: #ffffff; padding: 4px 8px; border: 1px solid #c4c4c4; text-align: left;"">
                Small Group Name
            </th>
            <th style=""background-color: #a6a5a5; color: #ffffff; padding: 4px 8px; border: 1px solid #c4c4c4; text-align: left;"">
                Meeting Date
            </th>
            <th style=""background-color: #a6a5a5; color: #ffffff; padding: 4px 8px; border: 1px solid #c4c4c4; text-align: left;"">
                Attendance
            </th>
            <th style=""background-color: #a6a5a5; color: #ffffff; padding: 4px 8px; border: 1px solid #c4c4c4; text-align: left;"">
                Notes
            </th>
            <th style=""background-color: #a6a5a5; color: #ffffff; padding: 4px 8px; border: 1px solid #c4c4c4; text-align: left;"">
                &nbsp;
            </th>
        </tr>
    </thead>
    <tbody>
        {% for GroupAttendance in GroupAttendances %}
            <tr style=""border: 1px solid #c4c4c4;"">
                <td style=""border: 1px solid #c4c4c4; padding: 6px;"">
                    {{ GroupAttendance.GroupName }}
                </td>
                <td style=""border: 1px solid #c4c4c4; padding: 6px;"">
                    {{ GroupAttendance.MeetingDate | Date:'M/d/yyyy' }}
                </td>
                <td style=""border: 1px solid #c4c4c4; padding: 6px;"">
                    {{ GroupAttendance.Attendance }}
                </td>
                <td style=""border: 1px solid #c4c4c4; padding: 6px;"">
                    {{ GroupAttendance.Notes }}
                </td>
                <td style=""border: 1px solid #c4c4c4; padding: 6px;"">
                    {% if GroupAttendance.LeaderEmail != null and GroupAttendance.LeaderEmail != empty %}
                        <a href=""mailto:{{ GroupAttendance.LeaderEmail }}"">Email Leader</a>
                    {% else %}
                        &nbsp;
                    {% endif %}
                </td>
            </tr>
        {% endfor %}
    </tbody>
</table>

{{ 'Global' | Attribute:'EmailFooter' }}",
                SystemGuid.SystemCommunication.GROUP_ATTENDANCE_DIGEST);
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
