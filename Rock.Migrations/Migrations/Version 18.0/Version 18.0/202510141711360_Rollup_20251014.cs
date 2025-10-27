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
    public partial class Rollup_20251014 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateStreakMapEditorBlockTypeCategoryUp();
            JMH_UpdateCommunicationEntryWizardSimpleCommunicationPageSetting_Up();
            JPH_UpdateCommunicationBlocksLinkedPages_20251010_Up();
            UpdateAppleDeviceListUp();
            CleanupMigrationHistory();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        #region KH: Update Streak Map Editor Block Type Category

        private void UpdateStreakMapEditorBlockTypeCategoryUp()
        {
            Sql( @"UPDATE [BlockType] SET [Category] = 'Streaks' WHERE [Guid] = 'B5616E10-0551-41BB-BD14-3ABA33E0040B'" );
        }

        #endregion

        #region JMH: Update Communication Entry Wizard Simple Communication Page Setting

        /// <summary>
        /// JMH - Copy the "SimpleCommunicationPage" setting from the WebForms Communication Entry Wizard block
        /// on the "New Communication (Legacy)" page to the Obsidian block on the "New Communication" page.
        /// </summary>
        private void JMH_UpdateCommunicationEntryWizardSimpleCommunicationPageSetting_Up()
        {
            var obsidianCommunicationEntryBlockGuid = SqlScalar( $@"DECLARE @ObsidianCommunicationPageId AS INT = (SELECT [Id] FROM [Page] WHERE [Guid] = '{SystemGuid.Page.NEW_COMMUNICATION_OBSIDIAN}');
DECLARE @ObsidianCommunicationEntryWizardBlockTypeId AS INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '9FFC7A4F-2061-4F30-AF79-D68C85EE9F27');

SELECT TOP 1 [Guid]
FROM [Block] B
WHERE B.[BlockTypeId] = @ObsidianCommunicationEntryWizardBlockTypeId
    AND B.[PageId] = @ObsidianCommunicationPageId;" ).ToStringSafe().AsGuidOrNull();

            var obsidianSimpleCommunicationPageAttributeGuid = SqlScalar( $@"DECLARE @ObsidianCommunicationPageId AS INT = (SELECT [Id] FROM [Page] WHERE [Guid] = '{SystemGuid.Page.NEW_COMMUNICATION_OBSIDIAN}');
DECLARE @ObsidianCommunicationEntryWizardBlockTypeId AS INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '9FFC7A4F-2061-4F30-AF79-D68C85EE9F27');
DECLARE @ObsidianCommunicationEntryWizardBlockId AS INT = (SELECT TOP 1 [Id] FROM [Block] B WHERE B.[BlockTypeId] = @ObsidianCommunicationEntryWizardBlockTypeId AND B.[PageId] = @ObsidianCommunicationPageId);

SELECT TOP 1 [Guid]
FROM [Attribute]
WHERE [Key] = 'SimpleCommunicationPage'
    AND [EntityTypeQualifierColumn] = 'BlockTypeId'
    AND [EntityTypeQualifierValue] = @ObsidianCommunicationEntryWizardBlockTypeId;" ).ToStringSafe().AsGuidOrNull();

            var obsidianBlockAttributeValue = SqlScalar( $@"DECLARE @ObsidianCommunicationPageId AS INT = (SELECT [Id] FROM [Page] WHERE [Guid] = '{SystemGuid.Page.NEW_COMMUNICATION_OBSIDIAN}');
DECLARE @ObsidianCommunicationEntryWizardBlockTypeId AS INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '9FFC7A4F-2061-4F30-AF79-D68C85EE9F27');
DECLARE @ObsidianCommunicationEntryWizardBlockId AS INT = (SELECT TOP 1 [Id] FROM [Block] B WHERE B.[BlockTypeId] = @ObsidianCommunicationEntryWizardBlockTypeId AND B.[PageId] = @ObsidianCommunicationPageId);
DECLARE @ObsidianAttributeId AS INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Key] = 'SimpleCommunicationPage' AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = @ObsidianCommunicationEntryWizardBlockTypeId);

SELECT TOP 1 [Value]
FROM [AttributeValue]
WHERE [AttributeId] = @ObsidianAttributeId
      AND [EntityId] = @ObsidianCommunicationEntryWizardBlockId;" ).ToStringSafe();

            var webFormsBlockAttributeValue = SqlScalar( $@"DECLARE @WebFormsCommunicationPageId AS INT = (SELECT [Id] FROM [Page] WHERE [Guid] = '{SystemGuid.Page.NEW_COMMUNICATION}');
DECLARE @WebFormsCommunicationEntryWizardBlockTypeId AS INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{SystemGuid.BlockType.COMMUNICATION_ENTRY_WIZARD}');
DECLARE @WebFormsCommunicationEntryWizardBlockId AS INT = (SELECT TOP 1 [Id] FROM [Block] B WHERE B.[BlockTypeId] = @WebFormsCommunicationEntryWizardBlockTypeId AND B.[PageId] = @WebFormsCommunicationPageId);
DECLARE @WebFormsAttributeId AS INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Key] = 'SimpleCommunicationPage' AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = @WebFormsCommunicationEntryWizardBlockTypeId);

SELECT TOP 1 [Value]
FROM [AttributeValue]
WHERE [AttributeId] = @WebFormsAttributeId
    AND [EntityId] = @WebFormsCommunicationEntryWizardBlockId;" ).ToStringSafe();

            if ( obsidianCommunicationEntryBlockGuid.HasValue
                    && obsidianSimpleCommunicationPageAttributeGuid.HasValue
                    && webFormsBlockAttributeValue.IsNotNullOrWhiteSpace()
                    && obsidianBlockAttributeValue.IsNullOrWhiteSpace() )
            {
                // Only set the Obsidian Communication Entry Wizard "SimpleCommunicationPage" setting
                // if it isn't set and if the WebForms equivalent has a setting value.
                RockMigrationHelper.AddBlockAttributeValue(
                    obsidianCommunicationEntryBlockGuid.Value.ToString(),
                    obsidianSimpleCommunicationPageAttributeGuid.Value.ToString(),
                    webFormsBlockAttributeValue );
            }
        }

        #endregion

        #region JPH: Update Communication Blocks' Linked Pages

        /// <summary>
        /// JPH - Update recently added communication blocks' linked page block settings.
        /// </summary>
        private void JPH_UpdateCommunicationBlocksLinkedPages_20251010_Up()
        {
            #region Update Communication List Block Detail Page

            // Find the block instance.
            var communicationListBlockGuid = SqlScalar( $@"
DECLARE @CommunicationHistoryPageId INT = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '{Rock.SystemGuid.Page.COMMUNICATION_HISTORY}');
DECLARE @CommunicationListBlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = 'c3544f53-8e2d-43d6-b165-8fefc541a4eb');

SELECT TOP 1 [Guid]
FROM [Block]
WHERE [PageId] = @CommunicationHistoryPageId
    AND [BlockTypeId] = @CommunicationListBlockTypeId;" )
                .ToStringSafe()
                .AsGuidOrNull();

            // Only perform the update if the block instance was found.
            if ( communicationListBlockGuid.HasValue )
            {
                // Overwrite the block setting to link to the latest "New Communication" page.
                RockMigrationHelper.AddBlockAttributeValue(
                    skipIfAlreadyExists: false,
                    blockGuid: communicationListBlockGuid.Value.ToString(),
                    attributeGuid: "5209A318-9C53-43E4-9511-AAC595FC3684",
                    value: "9f7ae226-cc95-4e6a-b333-c0294a2024bc,79c0c1a7-41b6-4b40-954d-660a4b39b8ce"
                );
            }

            #endregion Update Communication List Block Detail Page

            #region Update Email Preference Entry Block My Account Page

            // Because this block will have been chopped in a Rock update job, we'll need a follow-up job to update the
            // block setting AFTER this chop has taken place.
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v18.0 - Update Email Preference Entry Block Manage My Account Page",
                description: "This job will update the Manage My Account Page block setting for the recently-chopped Email Preference Entry block.",
                jobType: "Rock.Jobs.PostV18UpdateEmailPreferenceEntryBlockManageMyAccountPage",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_180_UPDATE_EMAIL_PREFERENCE_ENTRY_BLOCK_MANAGE_MY_ACCOUNT_PAGE );

            #endregion Update Email Preference Entry Block My Account Page
        }

        #endregion

        #region KH: Update Apple Device List

        private void UpdateAppleDeviceListUp()
        {
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone18,1", "iPhone 17 Pro", "63B74479-F333-4301-ACE5-A3DC650E0786", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone18,2", "iPhone 17 Pro Max", "8F3A7E55-9885-4267-8F6D-EB47B2C20A2A", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone18,3", "iPhone 17", "B9992759-3B0E-4D60-9439-F3E3FD580C0B", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone18,4", "iPhone Air", "012071D4-E069-4CB9-BCE6-6220335A04C5", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,12", "Apple Watch Ultra 3 49mm case", "A9613744-23CE-4AC6-8D0F-042CE5D54C67", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,13", "Apple Watch SE 3 40mm case", "236C2306-D507-43E3-97B9-8BAFC1578E78", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,14", "Apple Watch SE 3 44mm case", "BB560C44-ED54-4BA5-9208-D1DB8006C5BB", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,15", "Apple Watch SE 3 40mm case (GPS+Cellular)", "348E8B3C-2A63-4BDD-8D9E-60EF08A7740F", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,16", "Apple Watch SE 3 44mm case (GPS+Cellular)", "28D10D56-59F5-4B3B-AC96-4831FE1C9E17", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,17", "Apple Watch Series 11 42mm case", "56F6DDC8-B893-4D0A-8490-95B08DF09F23", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,18", "Apple Watch Series 11 46mm case", "2A73DE79-DEB5-4823-B89C-5186720A87A8", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,19", "Apple Watch Series 11 42mm case (GPS+Celllular)", "15F1A1C4-85BA-4C20-8A67-D8E84C092A4D", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,20", "Apple Watch Series 11 46mm case (GPS+Celllular)", "E36CD124-459E-4BA7-B620-31B9BA898B64", true );

            Sql( @"
                DECLARE @AppleDeviceDefinedTypeId INT = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = 'DAE31F78-7AB9-4ACE-9EE1-C1E6A734562C')

                UPDATE [PersonalDevice] SET [Model] = dv.[Description]
                FROM [PersonalDevice] pd
                JOIN [DefinedValue] dv ON pd.[Model] = dv.[Value]
                WHERE pd.[Manufacturer] = 'Apple'
                  AND pd.[Model] like '%,%'
                  AND dv.DefinedTypeId = @AppleDeviceDefinedTypeId" );
        }

        #endregion

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
