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

using Rock.SystemGuid;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 199, "1.16.4" )]
    public class LmsData: Migration
    {
        /// <summary>
        /// Adds the SendLearningActivityNotifications job.
        /// </summary>
        public override void Up()
        {
            AddSendLearningActivityNotificationsJob();
        }

        private void AddSendLearningActivityNotificationsJob()
        {
            var cronSchedule = "0 0 7 1/1 * ? *"; // 7am daily.
            var jobClass = "Rock.Jobs.SendLearningActivityNotifications";
            var name = "Send Learning Activity Notifications";
            var description = "A job that sends notifications to students for newly available activities.";

            Sql( $@"
            IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = '{jobClass}' AND [Guid] = '{SystemGuid.ServiceJob.SEND_LEARNING_ACTIVITY_NOTIFICATIONS}' )
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
                    '{SystemGuid.ServiceJob.SEND_LEARNING_ACTIVITY_NOTIFICATIONS}' );
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
		              [Guid] = '{SystemGuid.ServiceJob.SEND_LEARNING_ACTIVITY_NOTIFICATIONS}';
            END" );
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
        }
    }
}
