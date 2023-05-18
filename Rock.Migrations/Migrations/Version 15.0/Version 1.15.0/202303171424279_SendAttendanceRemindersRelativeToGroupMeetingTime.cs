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
    public partial class SendAttendanceRemindersRelativeToGroupMeetingTime : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.GroupType", "AttendanceReminderSystemCommunicationId", c => c.Int());
            AddColumn("dbo.GroupType", "AttendanceReminderSendStartOffsetMinutes", c => c.Int());
            AddColumn("dbo.GroupType", "AttendanceReminderFollowupDays", c => c.String(maxLength: 100));
            AddColumn("dbo.AttendanceOccurrence", "AttendanceReminderLastSentDateTime", c => c.DateTime());
            CreateIndex("dbo.GroupType", "AttendanceReminderSystemCommunicationId");
            AddForeignKey("dbo.GroupType", "AttendanceReminderSystemCommunicationId", "dbo.SystemCommunication", "Id");

            UpdateJobs();
        }

        private void UpdateJobs()
        {
            Sql( @"
-- Reset SendAttendanceReminder values for all GroupTypes.
UPDATE	[GroupType]
SET		[SendAttendanceReminder] = 0;

-- Rename legacy ServiceJob.
UPDATE	[ServiceJob]
SET		[Name] = 'Send Attendance Reminders for Group Type'
WHERE	[Class] = 'Rock.Jobs.SendAttendanceReminder'
	AND	[Name] = 'Send Attendance Reminders';

-- Create new ServiceJob.
INSERT [dbo].[ServiceJob] (
	  [IsSystem]
	, [IsActive]
	, [Name]
	, [Description]
	, [Class]
	, [CronExpression]
	, [NotificationStatus]
	, [Guid]
	, [EnableHistory]
	, [HistoryCount]
	)
VALUES (
	  0
	, 1
	, N'Send Group Attendance Reminders'
	, N'Sends a reminder to group leaders about entering attendance for their group meeting.  This job is meant to run many times per day and will only send reminders to groups when the configured time has passed.  By default, this job runs every 15 minutes, and it will send reminders to any group whose time threshold has passed since the last time it ran.'
	, N'Rock.Jobs.SendGroupAttendanceReminders'
	, N'0 0/15 * 1/1 * ? *'
	, 3
	, N'A554B0EC-D439-4E3D-8462-517B69FDE5B1'
	, 0
	, 500
	)

" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.GroupType", "AttendanceReminderSystemCommunicationId", "dbo.SystemCommunication");
            DropIndex("dbo.GroupType", new[] { "AttendanceReminderSystemCommunicationId" });
            DropColumn("dbo.AttendanceOccurrence", "AttendanceReminderLastSentDateTime");
            DropColumn("dbo.GroupType", "AttendanceReminderFollowupDays");
            DropColumn("dbo.GroupType", "AttendanceReminderSendStartOffsetMinutes");
            DropColumn("dbo.GroupType", "AttendanceReminderSystemCommunicationId");
        }
    }
}
