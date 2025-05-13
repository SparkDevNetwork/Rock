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
    public partial class UpdateLmsBlockSettingFieldTypes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixLearningProgramConfigurationPageNameUp();
            FixLearningActivityAvailableSystemCommunicationUp();
            FixSendLearningNotificationsJobDescriptionAndClassUp();
            FixBlockSettingFieldTypesUp();
            FixBlockSettingDefaultValuesUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Don't try to restore any data when going down.
        }

        private void FixLearningProgramConfigurationPageNameUp()
        {
            Sql( $@"
UPDATE [dbo].[Page]
SET [InternalName] = 'Learning Program Configuration',
    [PageTitle] = 'Learning Program Configuration',
    [BrowserTitle] = 'Learning Program Configuration'
WHERE [Guid] = '0E5103B8-EF4A-46C9-8F76-313A259B0A3C' AND [PageTitle] = 'Courses'" );
        }

        private void FixLearningActivityAvailableSystemCommunicationUp()
        {
            // Fix body of the "Learning Activity Available" SystemCommunication record
            // d40a9c32-f179-4e5e-9b0d-ce208c5d1870 is Rock.SystemGuid.SystemCommunication.LEARNING_ACTIVITY_NOTIFICATIONS
            Sql( @"
DECLARE @activityCommunicationGuid UNIQUEIDENTIFIER = 'd40a9c32-f179-4e5e-9b0d-ce208c5d1870';
UPDATE [SystemCommunication]
    SET Body = '
{% assign currentDate = ''Now'' | Date:''MMMM d, yyyy'' %}
{{ ''Global'' | Attribute:''EmailHeader'' }}
<h1 style=""margin:0;"">
	Your Activities
</h1>

<p>
    Below are your available activities as of {{ currentDate }}.
</p>

{% for course in Courses %}
	{% assign availableActivitiesCount = course.Activities | Size %}
	{% if availableActivitiesCount == 0 %}{% continue %}{% endif %}
	<h2> {{ course.ProgramName }}: {{ course.CourseName }} {% if course.CourseCode and course.CourseCode != empty %}- {{ course.CourseCode }}{% endif %} </h2>

	{% assign orderedActivities = course.Activities | OrderBy:''Order'' %}
	{% for activity in orderedActivities %}
		<p class=""mb-4"">
			<strong>Activity:</strong>
			{{ activity.ActivityName }}
				{% if activity.AvailableDate and activity.AvailableDate != empty %}
					(available {{ activity.AvailableDate | Date: ''MMM dd'' }})
				{% endif %}
			<br />
			{% if activity.DueDate and activity.DueDate != empty %}
			    <strong>Due:</strong>
				{{ activity.DueDate | HumanizeDateTime }}
			{% endif %}
		</p>	
	{% endfor %}
{% endfor %}

{{ ''Global'' | Attribute:''EmailFooter'' }}
'
WHERE [Guid] = @activityCommunicationGuid;" );
        }

        /// <summary>
        /// Updates the name of the Send Learning Notification job
        /// (from Send Learning Activity Notifications).
        /// </summary>
        private void FixSendLearningNotificationsJobDescriptionAndClassUp()
        {
            Sql( $@"
UPDATE s SET
    [Class] = 'Rock.Jobs.SendLearningNotifications',
    [Description] = 'This job will send any unsent class announcements as well as an available activity digest emails for all their newly available activities within a learning program. The class announcements SystemCommunication is configured by the job setting and contains the Person and Announcement merge fields. The Available Activity Notification is configured by the learning program and contains ActivityCount and Courses (a list of CourseInfo) merge fields.'
FROM [dbo].[ServiceJob] s
WHERE s.[Guid] = '{SystemGuid.ServiceJob.SEND_LEARNING_ACTIVITY_NOTIFICATIONS}'
" );
        }

        private void FixBlockSettingFieldTypesUp()
        {
            // Public Learning Program List: Show Completion Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA1460D8-E895-4B23-8A8E-10EBBED3990F",
                SystemGuid.FieldType.BOOLEAN,
                "Show Completion Status",
                "ShowCompletionStatus",
                "Show Completion Status",
                @"Determines if the individual's completion status should be shown.",
                4,
                @"True",
                "D7B8A019-B32F-4D1E-BECE-40EB254AF5A7" );

            // Public Learning Course List: Show Completion Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D",
                SystemGuid.FieldType.BOOLEAN,
                "Show Completion Status",
                "ShowCompletionStatus",
                "Show Completion Status",
                @"Determines if the individual's completion status should be shown.",
                3,
                @"True",
                "C276C064-85DF-47AF-886B-672A0942496F" );

            // Public Learning Class Workspace: Show Grades
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "55F2E89B-DE57-4E24-AC6C-576956FB97C5",
                SystemGuid.FieldType.BOOLEAN,
                "Show Grades",
                "ShowGrades",
                "Show Grades",
                @"Determines if grades will be shown on the class overview page.",
                4,
                @"True",
                "6FE66C3C-E37B-440D-942C-88C008E844F5" );

            // Learning Class List: Show Location Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "340F6CC1-8C38-4579-9383-A6168680194A",
                SystemGuid.FieldType.BOOLEAN,
                "Show Location Column",
                "ShowLocationColumn",
                "Show Location Column",
                @"Determines if the Location column should be visible.",
                1,
                @"False",
                "4FBE92ED-3CF1-4BD4-A78E-B50C0080FA0B" );

            // Learning Class List: Show Schedule Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "340F6CC1-8C38-4579-9383-A6168680194A",
                SystemGuid.FieldType.BOOLEAN,
                "Show Schedule Column",
                "ShowScheduleColumn",
                "Show Schedule Column",
                @"Determines if the Schedule column should be visible.",
                2,
                @"False",
                "BECBD2AE-5D1E-4065-8CAA-FE7972CC5742" );

            // Learning Class List: Show Semester Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "340F6CC1-8C38-4579-9383-A6168680194A",
                SystemGuid.FieldType.BOOLEAN,
                "Show Semester Column",
                "ShowSemesterColumn",
                "Show Semester Column",
                @"Determines if the Semester column should be visible when the configuration is 'Academic Calendar'.",
                3,
                @"False",
                "28087DBE-A334-47BF-9CA5-76AFF5EE1A6F" );

        }

        private void FixBlockSettingDefaultValuesUp()
        {
            // Public Learning Program List: Program List Template
            RockMigrationHelper.DeleteBlockAttributeValue( "B15CC3F1-766B-4469-8F95-E31011A3279F", "63F97CF2-774C-480C-9933-A2BAA664DCE2" );

            // Public Learning Program List: Show Completion Status
            RockMigrationHelper.DeleteBlockAttributeValue( "B15CC3F1-766B-4469-8F95-E31011A3279F", "D7B8A019-B32F-4D1E-BECE-40EB254AF5A7" );

            // Public Learning Course List: Lava Template
            RockMigrationHelper.DeleteBlockAttributeValue( "7CE9134A-FF7B-4663-8B79-0CA6EED53A1A", "6ADA636C-F93A-4347-A2E6-3D9C1BDA51C0" );

            // Public Learning Course List: Course List Template
            RockMigrationHelper.DeleteBlockAttributeValue( "7CE9134A-FF7B-4663-8B79-0CA6EED53A1A", "C276C064-85DF-47AF-886B-672A0942496F" );

            // Public Learning Class Workspace: Show Grades
            RockMigrationHelper.DeleteBlockAttributeValue( "D46C2787-60BA-4776-BE6E-7F785A984922", "6FE66C3C-E37B-440D-942C-88C008E844F5" );

            // Learning Class List: Show Location Column
            RockMigrationHelper.AddBlockAttributeValue( "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0", "4FBE92ED-3CF1-4BD4-A78E-B50C0080FA0B", @"True" );

            // Learning Class List: Show Schedule Column
            RockMigrationHelper.AddBlockAttributeValue( "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0", "BECBD2AE-5D1E-4065-8CAA-FE7972CC5742", @"True" );

            // Learning Class List: Show Semester Column
            RockMigrationHelper.AddBlockAttributeValue( "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0", "28087DBE-A334-47BF-9CA5-76AFF5EE1A6F", @"True" );
        }
    }
}
