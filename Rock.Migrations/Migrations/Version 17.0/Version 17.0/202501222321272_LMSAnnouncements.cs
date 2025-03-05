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
    /// <summary>
    ///
    /// </summary>
    public partial class LMSAnnouncements : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AlterColumn( "dbo.LearningClassAnnouncement", "DetailsUrl", c => c.String( maxLength: 200 ) );
            AlterColumn( "dbo.LearningGradingSystemScale", "Name", c => c.String( maxLength: 100 ) );
            AddSendAnnouncementsSystemCommunicationUp();
            FixActivityAvailableSystemCommunicationEntityTypeId();
            UpdateSendLearningNotificationsJobName();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn( "dbo.LearningGradingSystemScale", "Name", c => c.String( nullable: false, maxLength: 100 ) );
            AlterColumn( "dbo.LearningClassAnnouncement", "DetailsUrl", c => c.String() );
            AddSendAnnouncementsSystemCommunicationDown();
        }

        private void AddSendAnnouncementsSystemCommunicationUp()
        {
            var body = @"{{ 'Global' | Attribute:'EmailHeader' }}

Hello {{Person.NickName}},

{{Announcement.Description}}

{{ 'Global' | Attribute:'EmailFooter' }}";

            RockMigrationHelper.UpdateSystemCommunication(
                "Learning Management", // category
                "Learning Class Announcement", // title
                "", // from
                "", // fromName
                "", // to
                "", // cc
                "", // bcc
                "{{Announcement.Title}}", // subject
                body, // body
                Rock.SystemGuid.SystemCommunication.LEARNING_ANNOUNCEMENT_NOTIFICATIONS );
        }

        private void AddSendAnnouncementsSystemCommunicationDown()
        {
            RockMigrationHelper.DeleteSystemCommunication( Rock.SystemGuid.SystemCommunication.LEARNING_ANNOUNCEMENT_NOTIFICATIONS );
        }

        /// <summary>
        /// The Send Learning Notifications job was initially created using a category whose EntityTypeId
        /// was not SystemCommunicationId - this prevents it from showing on the SystemCommunication List block.
        /// </summary>
        private void FixActivityAvailableSystemCommunicationEntityTypeId()
        {
            Sql( @"
DECLARE @activityCommunicationGuid UNIQUEIDENTIFIER = 'd40a9c32-f179-4e5e-9b0d-ce208c5d1870';
DECLARE @SystemCommunicationEntity int = (
                    SELECT TOP 1 [Id]
                    FROM [EntityType]
                    WHERE [Name] = 'Rock.Model.SystemCommunication' )
DECLARE @lmsSystemCommunicationCategoryId INT = (SELECT TOP 1 [Id]
					FROM [Category]
					WHERE [Name] = 'Learning Management'
						AND [EntityTypeId] = @SystemCommunicationEntity);
UPDATE SystemCommunication SET
    CategoryId = @lmsSystemCommunicationCategoryId
WHERE [Guid] = @activityCommunicationGuid;" );
        }

        /// <summary>
        /// Updates the name of the Send Learning Notification job
        /// (from Send Learning Activity Notifications).
        /// </summary>
        private void UpdateSendLearningNotificationsJobName()
        {
            Sql( $@"
UPDATE s SET
	[Name] = 'Send Learning Notifications'
FROM [dbo].[ServiceJob] s
WHERE s.[Guid] = '{SystemGuid.ServiceJob.SEND_LEARNING_ACTIVITY_NOTIFICATIONS}'
" );
        }
    }
}
