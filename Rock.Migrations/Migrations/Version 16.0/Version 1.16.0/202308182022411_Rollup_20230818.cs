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
    public partial class Rollup_20230818 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RemoveNoDerivatesFromContentLibraryUp();
            RemoveDuplicateContentTopicsUp();
            AddNewNotificationMessagesPageUp();
            CleanupMigrationHistory();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RemoveNoDerivatesFromContentLibraryDown();
        }

        /// <summary>
        /// JMH: Remove "(No Derivatives)" from Content Library License Defined Values
        /// </summary>
        public void RemoveNoDerivatesFromContentLibraryUp()
        {
            // Remove "(No Derivatives)" from Content Library License values.
            RockMigrationHelper.UpdateDefinedValue( "83FB89B4-205A-41D6-A798-A81F12E6CDB0", "Author Attribution", "This license allows the licensed content to be downloaded and shared with others, as long as the text of content is not modified in any way, and the organization does not use the content for commercial purposes. Additionally, the organization must provide attribution to the individual author when displaying the content.", "9AED8DEE-F74D-4F38-AD45-2423170D31D2" );
            RockMigrationHelper.UpdateDefinedValue( "83FB89B4-205A-41D6-A798-A81F12E6CDB0", "Organization Attribution", "This license allows the licensed content to be downloaded and shared with others, as long as the content is not modified in any way, and the content is not used for commercial purposes. Additionally, the user must provide attribution to the organization that published the content when displaying the content.", "577F2BD5-BFDF-41B7-96A8-32C0F1E44905" );
        }

        /// <summary>
        /// JMH: Remove "(No Derivatives)" from Content Library License Defined Values
        /// </summary>
        public void RemoveNoDerivatesFromContentLibraryDown()
        {
            // Restore "(No Derivatives)" in Content Library License values.
            RockMigrationHelper.UpdateDefinedValue( "83FB89B4-205A-41D6-A798-A81F12E6CDB0", "Author Attribution (No Derivatives)", "This license allows the licensed content to be downloaded and shared with others, as long as the text of content is not modified in any way, and the organization does not use the content for commercial purposes. Additionally, the organization must provide attribution to the individual author when displaying the content.", "9AED8DEE-F74D-4F38-AD45-2423170D31D2" );
            RockMigrationHelper.UpdateDefinedValue( "83FB89B4-205A-41D6-A798-A81F12E6CDB0", "Organization Attribution (No Derivatives)", "This license allows the licensed content to be downloaded and shared with others, as long as the content is not modified in any way, and the content is not used for commercial purposes. Additionally, the user must provide attribution to the organization that published the content when displaying the content.", "577F2BD5-BFDF-41B7-96A8-32C0F1E44905" );
        }

        /// <summary>
        /// JMH: Remove duplicate Doubt and Sin ContentTopics
        /// </summary>
        public void RemoveDuplicateContentTopicsUp()
        {
            Sql( @"
                DECLARE @DoubtContentTopicIdToRemove AS INT
                DECLARE @DoubtContentTopicIdToKeep AS INT
                DECLARE @SinContentTopicIdToRemove AS INT
                DECLARE @SinContentTopicIdToKeep AS INT

                SELECT @DoubtContentTopicIdToRemove = [Id] FROM [ContentTopic] WHERE [Guid] = '0A1E7A47-2EFC-E8EE-0CFD-D38DB8BB60BA'
                SELECT @DoubtContentTopicIdToKeep = [Id] FROM [ContentTopic] WHERE [Guid] = '11A29EDA-38FF-B2F7-DEDC-D0170C1417B1'
                SELECT @SinContentTopicIdToRemove = [Id] FROM [ContentTopic] WHERE [Guid] = 'A93EDB0E-5761-0055-6DF7-34646BE579C2'
                SELECT @SinContentTopicIdToKeep = [Id] FROM [ContentTopic] WHERE [Guid] = 'A5386706-4D41-A8D9-33E6-9ED8A070A093'

                IF @DoubtContentTopicIdToRemove > 0
                BEGIN
	                -- Update items with the old Doubt topic ID to use the Doubt topic ID that will remain.
	                UPDATE [ContentChannelItem] SET [ContentLibraryContentTopicId] = @DoubtContentTopicIdToKeep WHERE [ContentLibraryContentTopicId] = @DoubtContentTopicIdToRemove

	                -- Remove the old Doubt topic.
	                DELETE [ContentTopic] WHERE [Id] = @DoubtContentTopicIdToRemove
                END

                IF @SinContentTopicIdToRemove > 0
                BEGIN
	                -- Update items with the old Doubt topic ID to use the Doubt topic ID that will remain.
	                UPDATE [ContentChannelItem] SET [ContentLibraryContentTopicId] = @SinContentTopicIdToKeep WHERE [ContentLibraryContentTopicId] = @SinContentTopicIdToRemove

	                -- Remove the old Doubt topic.
	                DELETE [ContentTopic] WHERE [Id] = @SinContentTopicIdToRemove
                END" );
        }

        /// <summary>
        /// DH: Add new Notification Messages page.
        /// </summary>
        public void AddNewNotificationMessagesPageUp()
        {
            // Add Page 
            //  Internal Name: View Notifications
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "AE1818D8-581C-4599-97B9-509EA450376A", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "View Notifications", "", "586A8827-5C22-4624-A5E3-1B1D6CD0E5B7", "" );

            // Add Block 
            //  Block Name: Notification Messages
            //  Page Name: View Notifications
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "586A8827-5C22-4624-A5E3-1B1D6CD0E5B7".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "2E4292B9-CD68-41E9-86BD-356ACCD54F36".AsGuid(), "Notification Messages", "Main", @"", @"", 0, "43EE91B4-6212-4B98-A53F-C3A6F7BB728E" );

            // Attribute for BlockType
            //   BlockType: Reminder Links
            //   Category: Core
            //   Attribute: View Notifications Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EC59B6D6-5CA1-4367-9109-CDDC92357D35", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "View Notifications Page", "ViewNotificationsPage", "View Notifications Page", @"The page where a person can view their notifications.", 0, @"586A8827-5C22-4624-A5E3-1B1D6CD0E5B7", "33F2F2D0-CA8C-4D30-88B8-64DCF2BA5864" );
        }

        /// <summary>
        /// DH: Add new Notification Messages page.
        /// </summary>
        public void AddNewNotificationMessagesPageDown()
        {
            // Remove Block
            //  Name: Notification Messages, from Page: View Notifications, Site: Rock RMS
            //  from Page: View Notifications, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "43EE91B4-6212-4B98-A53F-C3A6F7BB728E" );

            // Delete Page 
            //  Internal Name: View Notifications
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "586A8827-5C22-4624-A5E3-1B1D6CD0E5B7" );
        }

        /// <summary>
        /// Update the list of Apple device models
        /// </summary>
        public void RemoveUnneededAppleModels()
        {
            Sql( @"
                DECLARE @AppleDeviceDefinedTypeId INT = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = 'DAE31F78-7AB9-4ACE-9EE1-C1E6A734562C')

                DELETE FROM [DefinedValue]
                WHERE [DefinedTypeId] = @AppleDeviceDefinedTypeId
	                AND [Value] IN ('iPad14,6-A','iPad14,6-B')" );
        }

        /// <summary>
        /// Cleanups the migration history records except the last one.
        /// </summary>
        private void CleanupMigrationHistory()
        {
        Sql( @"
        UPDATE [dbo].[__MigrationHistory]
        SET [Model] = 0x
        WHERE MigrationId < (SELECT TOP 1 MigrationId FROM __MigrationHistory ORDER BY MigrationId DESC)" );
        }
    }
}
