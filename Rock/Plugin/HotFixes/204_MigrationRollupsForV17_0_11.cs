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

using System.Collections.Generic;

using Rock.Model;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 204, "1.16.4" )]
    public class MigrationRollupsForV17_0_11 : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            UpdateProgramCompletionsJobUp();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            UpdateProgramCompletionsJobDown();
        }

        #region LMS Update Program Completions Job

        private void UpdateProgramCompletionsJobUp()
        {
            AddOrUpdateUpdateProgramCompletionsJob();
            IconCssClassesUp();
            AddShowOnlyEnrolledCoursesBlockAttribute();
            UpdateSendLearningNotificationSystemCommunicationBody();
        }

        private void UpdateProgramCompletionsJobDown()
        {
            DeleteUpdateProgramCompletionsJob();
            IconCssClassesDown();
            RemoveShowOnlyEnrolledCoursesBlockAttribute();
        }

        /// <summary>
        ///  Deletes the UpdateProgramCompletions Job based on Guid and Class.
        /// </summary>
        private void DeleteUpdateProgramCompletionsJob()
        {
            var jobClass = "Rock.Jobs.UpdateProgramCompletions";
            Sql( $"DELETE [ServiceJob] WHERE [Class] = '{jobClass}' AND [Guid] = '{SystemGuid.ServiceJob.UPDATE_PROGRAM_COMPLETIONS}'" );
        }

        /// <summary>
        /// Adds or Updates the UpdateProgramCompletions Job.
        /// </summary>
        private void AddOrUpdateUpdateProgramCompletionsJob()
        {
            var cronSchedule = "0 0 5 1/1 * ? *"; // 5am daily.
            var jobClass = "Rock.Jobs.UpdateProgramCompletions";
            var name = "Update Program Completions";
            var description = "A job that updates learning program completion records for programs that track completion status.";

            Sql( $@"
            IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = '{jobClass}' AND [Guid] = '{SystemGuid.ServiceJob.UPDATE_PROGRAM_COMPLETIONS}' )
            BEGIN
                INSERT INTO [ServiceJob] (
                    [IsSystem],
                    [IsActive],
                    [Name],
                    [Description],
                    [Class],
                    [CronExpression],
                    [NotificationStatus],
                    [Guid] )
                VALUES (
                    0,
                    1,
                    '{name}',
                    '{description}',
                    '{jobClass}',
                    '{cronSchedule}',
                    1,
                    '{SystemGuid.ServiceJob.UPDATE_PROGRAM_COMPLETIONS}' );
            END
            ELSE
            BEGIN
	            UPDATE	[ServiceJob]
	            SET
		              [IsSystem] = 1
		            , [IsActive] = 1
		            , [Name] = '{name}'
		            , [Description] = '{description}'
		            , [Class] = '{jobClass}'
		            , [CronExpression] = '{cronSchedule}'
		            , [NotificationStatus] = 1
	            WHERE
		              [Guid] = '{SystemGuid.ServiceJob.UPDATE_PROGRAM_COMPLETIONS}';
            END" );
        }

        private void IconCssClassesUp()
        {
            var learningProgramEntityTypeGuid = "AFD89885-6923-4FA1-B6FE-A1DF8D821BBC";
            var allChurchCategoryGuid = "824B5DD9-47A7-4CE4-A461-F4FDEC8343F3";
            var internalStaffCategoryGuid = "87E1BEC7-171F-4E7E-8EC7-4D0102DCDE70";
            var volunteeringCategoryGuid = "A94C5563-647D-4509-B213-890B6D2A8530";
            RockMigrationHelper.UpdateCategory( learningProgramEntityTypeGuid, "All Church", "fa fa-church", "All Church", allChurchCategoryGuid, 0 );
            RockMigrationHelper.UpdateCategory( learningProgramEntityTypeGuid, "Internal Staff", "fa fa-id-badge", "Internal Staff", internalStaffCategoryGuid, 1 );
            RockMigrationHelper.UpdateCategory( learningProgramEntityTypeGuid, "Volunteering", "fa fa-people-carry", "Volunteering", volunteeringCategoryGuid, 2 );
        }

        private void IconCssClassesDown()
        {
            var learningProgramEntityTypeGuid = "AFD89885-6923-4FA1-B6FE-A1DF8D821BBC";
            var allChurchCategoryGuid = "824B5DD9-47A7-4CE4-A461-F4FDEC8343F3";
            var internalStaffCategoryGuid = "87E1BEC7-171F-4E7E-8EC7-4D0102DCDE70";
            var volunteeringCategoryGuid = "A94C5563-647D-4509-B213-890B6D2A8530";
            RockMigrationHelper.UpdateCategory( learningProgramEntityTypeGuid, "All Church", "", "All Church", allChurchCategoryGuid, 0 );
            RockMigrationHelper.UpdateCategory( learningProgramEntityTypeGuid, "Internal Staff", "", "Internal Staff", internalStaffCategoryGuid, 1 );
            RockMigrationHelper.UpdateCategory( learningProgramEntityTypeGuid, "Volunteering", "", "Volunteering", volunteeringCategoryGuid, 2 );
        }

        private void AddShowOnlyEnrolledCoursesBlockAttribute()
        {
            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Show Only Enrolled Courses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Only Enrolled Courses", "Show Only Enrolled Courses", "Show Only Enrolled Courses", @"Filter to only those courses that the viewer is enrolled in.", 5, @"False", "7F5A60B0-1687-4527-8F4B-C1EE0917B0EC" );
        }

        private void RemoveShowOnlyEnrolledCoursesBlockAttribute()
        {
            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Show Only Enrolled Courses
            RockMigrationHelper.DeleteAttribute( "7F5A60B0-1687-4527-8F4B-C1EE0917B0EC" );
        }

        private void UpdateSendLearningNotificationSystemCommunicationBody()
        {
            Sql( @"
UPDATE s SET
	Body = '
{% assign currentDate = ''Now'' | Date:''MMMM d, yyyy'' %}
{{ ''Global'' | Attribute:''EmailHeader'' }}
<h1 style=""margin:0;"">
	Your Activities
</h1>

<p>
    Below are your newly available activities as of {{ currentDate }}.
</p>

{% for course in Courses %}
	<h2> {{course.ProgramName}}: {{course.CourseName}} - {{course.CourseCode}} </h2>
	
	<table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""table-layout: fixed;"">
		<tr>
			<th valign=""top"" style=""vertical-align:top;"" width=""50%"">
					Activity
			</th>
			<th valign=""top"" style=""vertical-align:top;"" width=""25%"">
					Available As Of
			</th>
			<th valign=""top"" style=""vertical-align:top;"" width=""25%"">
					Due
			</th>
		</tr>
		{% for activity in course.Activities %}
			<tr>
				<td>
					{{activity.ActivityName}}
				</td>
				<td>	
					{% if activity.AvailableDate == null %}
						Always
					{% else %}
						{{ activity.AvailableDate | HumanizeDateTime }}
					{% endif %}
				</td>
				<td>
					{% if activity.DueDate == null %}
						Optional
					{% else %}
						{{ activity.DueDate | HumanizeDateTime }}
					{% endif %}
				</td>		
			</tr>			
		{% endfor %}
	</table>
{% endfor %}

{{ ''Global'' | Attribute:''EmailFooter'' }}'
from systemcommunication s
WHERE [Guid] = 'D40A9C32-F179-4E5E-9B0D-CE208C5D1870'
" );
        }

        #endregion

    }
}
