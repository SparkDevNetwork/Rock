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
    using System.Collections.Generic;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20250429 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ChatBackendSyncAdditionsUp();
            UpdateFileAssetManagerDefaultValuesUp();
            AddDefaultAllowedGroupTypesToMobileGroupRegistrationUp();
            ChopBlocksUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            ChatBackendSyncAdditionsDown();
            UpdateFileAssetManagerDefaultValuesDown();
        }

        #region JPH: ChatBackendSyncAdditions Plugin Migration #241

        private void ChatBackendSyncAdditionsUp()
        {
            RockMigrationHelper.AddGroupTypeGroupAttribute(
                groupTypeGuid: SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL,
                fieldTypeGuid: SystemGuid.FieldType.IMAGE,
                name: "Avatar Image",
                description: "The avatar image to use for this chat channel in the external chat system.",
                order: 0,
                defaultValue: "",
                guid: "CB6178C6-4A32-4008-B56E-9D548FD8303B"
            );
        }

        private void ChatBackendSyncAdditionsDown()
        {
            RockMigrationHelper.DeleteAttribute( "CB6178C6-4A32-4008-B56E-9D548FD8303B" );
        }

        #endregion

        #region NL: UpdateFileAssetManagerDefaultValues Plugin Migration #242

        private void UpdateFileAssetManagerDefaultValuesUp()
        {
            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Enable Asset Storage Providers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Asset Storage Providers", "EnableAssetProviders", "Enable Asset Storage Providers", @"Set this to true to enable showing folders and files from your configured asset storage providers.", 0, @"False", "7750F7BB-DC53-41C6-987B-5FD2B02674C2" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Enable File Manager
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable File Manager", "EnableFileManager", "Enable File Manager", @"Set this to true to enable showing folders and files your server's local file system.", 1, @"True", "FCBB90A6-965F-4237-9B0F-4384E3FFC991" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Use Static Height
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Static Height", "IsStaticHeight", "Use Static Height", @"Set this to true to be able to set a CSS height value dictating how tall the block will be. Otherwise, it will grow with the content.", 2, @"True", "5D1524EA-E1F0-472B-AE24-4D30DA6672F8" );
        }

        private void UpdateFileAssetManagerDefaultValuesDown()
        {
            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Enable Asset Storage Providers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Asset Storage Providers", "EnableAssetProviders", "Enable Asset Storage Providers", @"Set this to true to enable showing folders and files from your configured asset storage providers.", 0, @"True", "7750F7BB-DC53-41C6-987B-5FD2B02674C2" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Enable File Manager
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable File Manager", "EnableFileManager", "Enable File Manager", @"Set this to true to enable showing folders and files your server's local file system.", 1, @"False", "FCBB90A6-965F-4237-9B0F-4384E3FFC991" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Use Static Height
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Static Height", "IsStaticHeight", "Use Static Height", @"Set this to true to be able to set a CSS height value dictating how tall the block will be. Otherwise, it will grow with the content.", 2, @"False", "5D1524EA-E1F0-472B-AE24-4D30DA6672F8" );
        }

        #endregion

        #region DH: AddDefaultAllowedGroupTypesToMobileGroupRegistration Plugin Migration #243

        private void AddDefaultAllowedGroupTypesToMobileGroupRegistrationUp()
        {
            // In v17.1 we fixed the mobile Group Registration block to have a
            // default value of Small Groups for the Allowed Group Types attribute.
            // Because we don't want to break existing instances of the block, we
            // need to enable all group types for existing blocks that don't have
            // a value for Allowed Group Types set.
            Sql( @"
DECLARE @BlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '8A42E4FA-9FE1-493C-B6D8-7A766D96E912')
DECLARE @BlockEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')
DECLARE @AttributeId INT = (SELECT TOP 1 [Id] FROM [Attribute]
    WHERE [EntityTypeId] = @BlockEntityTypeId
      AND [EntityTypeQualifierColumn] = 'BlockTypeId'
      AND [EntityTypeQualifierValue] = @BlockTypeId
      AND [Key] = 'GroupTypes')

-- Get all Group Type Guids as a comma separated list to store in the Attribute Values.
DECLARE @GroupTypeGuids NVARCHAR(max) = TRIM(',' FROM (SELECT CAST([Guid] AS NVARCHAR(MAX)) + ',' FROM [GroupType] FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'))

IF @BlockTypeId IS NOT NULL AND @AttributeId IS NOT NULL
BEGIN
    -- Update existing values that are blank.
    UPDATE [AttributeValue]
    SET [Value] = @GroupTypeGuids
        , [IsPersistedValueDirty] = 1
    WHERE [AttributeId] = @AttributeId
    AND [EntityId] IN (SELECT [Id] FROM [Block] WHERE [BlockTypeId] = @BlockTypeId)
    AND [Value] = ''

    -- Insert new values for blocks without a current value.
    INSERT INTO [AttributeValue]
        ([IsSystem], [AttributeId], [EntityId], [Value], [Guid], [IsPersistedValueDirty])
        SELECT
            0,
            @AttributeId,
            [B].[Id],
            @GroupTypeGuids,
            NEWID(),
            1
        FROM [Block] AS [B]
        WHERE [B].[BlockTypeId] = @BlockTypeId
        AND [B].[Id] NOT IN (SELECT [EntityId] FROM [AttributeValue] WHERE [AttributeId] = @AttributeId)
END
" );
        }

        #endregion

        #region KH: Register block attributes for chop job in v17.1 (18.0.5). Plugin Migration #244

        private void ChopBlocksUp()
        {
            RegisterBlockAttributesForChop();
            //ChopBlockTypesv17_1();
        }

        /// <summary>
        /// Ensure the Entity, BlockType and Block Setting Attribute records exist
        /// before the chop job runs. Any missing attributes would cause the job to fail.
        /// </summary>
        private void RegisterBlockAttributesForChop()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.MediaElementDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.MediaElementDetail", "Media Element Detail", "Rock.Blocks.Cms.MediaElementDetail, Rock.Blocks, Version=17.1.1.0, Culture=neutral, PublicKeyToken=null", false, false, "6A7052F9-94DF-4244-BBF0-DB688C3ACBBC" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.CommunicationTemplateDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.CommunicationTemplateDetail", "Communication Template Detail", "Rock.Blocks.Communication.CommunicationTemplateDetail, Rock.Blocks, Version=17.1.1.0, Culture=neutral, PublicKeyToken=null", false, false, "017EEC30-BDDA-4159-8249-2852AF4ADCF2" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.PersonFollowingList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.PersonFollowingList", "Person Following List", "Rock.Blocks.Core.PersonFollowingList, Rock.Blocks, Version=17.1.1.0, Culture=neutral, PublicKeyToken=null", false, false, "030B944D-66B5-4EDB-AA38-10081E2ACFB6" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.ScheduledJobList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.ScheduledJobList", "Scheduled Job List", "Rock.Blocks.Core.ScheduledJobList, Rock.Blocks, Version=17.1.1.0, Culture=neutral, PublicKeyToken=null", false, false, "D72E22CA-040D-4DE9-B2E0-438BA70BA91A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.ServiceJobDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.ServiceJobDetail", "Service Job Detail", "Rock.Blocks.Core.ServiceJobDetail, Rock.Blocks, Version=17.1.1.0, Culture=neutral, PublicKeyToken=null", false, false, "B50B6B68-D327-4A73-B2A8-57EF9E151182" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Tv.AppleTvPageList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Tv.AppleTvPageList", "Apple Tv Page List", "Rock.Blocks.Tv.AppleTvPageList, Rock.Blocks, Version=17.1.1.0, Culture=neutral, PublicKeyToken=null", false, false, "4E89A96E-88A2-4CA4-A86B-B9FFDCACF49F" );

            // Add/Update Obsidian Block Type
            //   Name:Apple TV Page List
            //   Category:TV > TV Apps
            //   EntityType:Rock.Blocks.Tv.AppleTvPageList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Apple TV Page List", "Lists pages for TV apps (Apple or other).", "Rock.Blocks.Tv.AppleTvPageList", "TV > TV Apps", "A759218B-1C72-446C-8994-8559BA72941E" );

            // Add/Update Obsidian Block Type
            //   Name:Communication Template Detail
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.CommunicationTemplateDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Communication Template Detail", "Used for editing a communication template that can be selected when creating a new communication, SMS, etc. to people.", "Rock.Blocks.Communication.CommunicationTemplateDetail", "Communication", "FBAB4EB2-B180-4A76-9B5B-C75E2255F691" );

            // Add/Update Obsidian Block Type
            //   Name:Media Element Detail
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.MediaElementDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Media Element Detail", "Displays the details of a particular media element.", "Rock.Blocks.Cms.MediaElementDetail", "CMS", "D481AE29-A6AA-49F4-9DBB-D3FDF0995CA3" );

            // Add/Update Obsidian Block Type
            //   Name:Person Following List
            //   Category:Follow
            //   EntityType:Rock.Blocks.Core.PersonFollowingList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Person Following List", "Block for displaying people that current person follows.", "Rock.Blocks.Core.PersonFollowingList", "Follow", "18FA879F-1466-413B-8623-834D728F677B" );

            // Add/Update Obsidian Block Type
            //   Name:Scheduled Job Detail
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.ServiceJobDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Scheduled Job Detail", "Displays the details of a particular service job.", "Rock.Blocks.Core.ServiceJobDetail", "Core", "762F09EA-0A11-4BC7-9A68-13F0E44217C1" );

            // Add/Update Obsidian Block Type
            //   Name:Scheduled Job List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.ScheduledJobList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Scheduled Job List", "Lists all scheduled jobs.", "Rock.Blocks.Core.ScheduledJobList", "Core", "9B90F2D1-0C7B-4F08-A808-8BA4C9A70A20" );

            // Attribute for BlockType
            //   BlockType: Apple TV Page List
            //   Category: TV > TV Apps
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A759218B-1C72-446C-8994-8559BA72941E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "887084AE-416A-4389-916A-99466BDEBEAC" );

            // Attribute for BlockType
            //   BlockType: Apple TV Page List
            //   Category: TV > TV Apps
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A759218B-1C72-446C-8994-8559BA72941E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "84D1AB87-B286-47F4-8851-6D25A65EACF2" );

            Sql( "DELETE FROM [dbo].[Attribute] WHERE [Guid] = 'D400AC7B-A227-4768-A8B2-7B403A0CAF17' AND [Key] = 'DetailPage'" );

            // Attribute for BlockType
            //   BlockType: Apple TV Page List
            //   Category: TV > TV Apps
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A759218B-1C72-446C-8994-8559BA72941E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "PageDetail", "Detail Page", @"The page that will show the page details.", 0, @"", "D400AC7B-A227-4768-A8B2-7B403A0CAF17" );

            // Attribute for BlockType
            //   BlockType: Communication Template Detail
            //   Category: Communication
            //   Attribute: Attachment Binary File Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FBAB4EB2-B180-4A76-9B5B-C75E2255F691", "09EC7F0D-3505-4090-B010-ABA68CB9B904", "Attachment Binary File Type", "AttachmentBinaryFileType", "Attachment Binary File Type", @"The FileType to use for files that are attached to an sms or email communication", 1, @"10FD7FE8-7187-45CC-A1E7-D9F71BD90E6C", "78CC588D-D5DA-4FAD-AEDF-E37719454320" );

            // Attribute for BlockType
            //   BlockType: Communication Template Detail
            //   Category: Communication
            //   Attribute: Personal Templates View
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FBAB4EB2-B180-4A76-9B5B-C75E2255F691", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Personal Templates View", "PersonalTemplatesView", "Personal Templates View", @"Is this block being used to display personal templates (only templates that current user is allowed to edit)?", 0, @"False", "655D88DE-04B8-49F6-B994-7F16E4B88E4E" );

            // Attribute for BlockType
            //   BlockType: Person Following List
            //   Category: Follow
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18FA879F-1466-413B-8623-834D728F677B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "60AAFCAC-553B-430D-9F33-1B5BBF214F3D" );

            // Attribute for BlockType
            //   BlockType: Person Following List
            //   Category: Follow
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18FA879F-1466-413B-8623-834D728F677B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "FF99A423-17FC-4430-A968-7FF8AE755D41" );

            // Attribute for BlockType
            //   BlockType: Scheduled Job List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9B90F2D1-0C7B-4F08-A808-8BA4C9A70A20", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "C6B47E32-CA90-4F7B-83D1-E163BE89AB02" );

            // Attribute for BlockType
            //   BlockType: Scheduled Job List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9B90F2D1-0C7B-4F08-A808-8BA4C9A70A20", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "DA72C7D5-B0CC-4147-828A-5DF79F88D621" );

            // Attribute for BlockType
            //   BlockType: Scheduled Job List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9B90F2D1-0C7B-4F08-A808-8BA4C9A70A20", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the service job details.", 0, @"", "DF2A7B20-54DA-44DA-A3BE-695CCCCAD6B6" );

            // Attribute for BlockType
            //   BlockType: Scheduled Job List
            //   Category: Core
            //   Attribute: History Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9B90F2D1-0C7B-4F08-A808-8BA4C9A70A20", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "History Page", "HistoryPage", "History Page", @"The page to display group history.", 0, @"", "B45EC31E-A1DF-41EE-B2C3-201A8A3C9BF9" );
        }

        private void ChopBlockTypesv17_1()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types 17.1 (18.0.5)",
                blockTypeReplacements: new Dictionary<string, string> {
                    // blocks chopped in v17.1 (Pre-Alpha: 18.0.5)
{ "6D3F924E-BDD0-4C78-981E-B698351E75AD", "9b90f2d1-0c7b-4f08-a808-8ba4c9a70a20" }, // Scheduled Job List ( Core )
{ "7BD1B79C-BF27-42C6-8359-F80EC7FEE397", "a759218b-1c72-446c-8994-8559ba72941e" }, // Apple TV Page List ( TV > TV Apps )
{ "881DC0D1-FF98-4A5E-827F-49DD5CD0BD32", "d481ae29-a6aa-49f4-9dbb-d3fdf0995ca3" }, // Media Element Detail ( CMS )
{ "BD548744-DC6D-4870-9FED-BB9EA24E709B", "18fa879f-1466-413b-8623-834d728f677b" }, // Person Following List ( Follow )
{ "BFDCA2E2-DAA1-4FA6-B33C-C53C7CF23C5D", "FBAB4EB2-B180-4A76-9B5B-C75E2255F691" }, // Communication Template Detail ( Communication )
{ "C5EC90C9-26C4-493A-84AC-4B5DEF9EA472", "762f09ea-0a11-4bc7-9a68-13f0e44217c1" }, // Scheduled Job Detail ( Core )
                    // blocks chopped in v17.1 (Pre-Alpha: 18.0.4)
{ "00A86827-1E0C-4F47-8A6F-82581FA75CED", "1fde6d4f-390a-4ff6-ad42-668ec8cc62c4" }, // Assessment Type List ( CRM )
{ "069554B7-983E-4653-9A28-BA39659C6D63", "47f619c2-f66d-45ec-adbb-22ca23b4f3ad" }, // Attribute Matrix Template List ( Core )
{ "32183AD6-01CB-4533-858B-1BDA5120AAD5", "7686a42f-a2c4-4c15-9331-8b364f24bd0f" }, // Device List ( Core )
{ "32E89BAE-C085-40B3-B872-B62E25A62BDB", "0f99866a-7fab-462d-96eb-9f9534322c57" }, // Gateway List ( Finance )
{ "50ADE904-BB5C-40F9-A97D-ED8FF530B5A6", "cfbb4daf-1aeb-4095-8098-e3a82e30fa7e" }, // Persisted Dataset List ( CMS )
{ "65057F07-85D5-4795-91A1-86D8F67A65DC", "2eaf9e5a-f47d-4c58-9aa4-2d340547a35f" }, // Financial Statement Template List ( Finance )
{ "A580027F-56DB-43B0-AAD6-7C2B8A952012", "29227fc7-8f24-44b1-a0fb-e6a8694f1c3b" }, // Content Channel Type List ( CMS )
{ "C4FBF612-C1F6-428B-97FD-8AB0B8EA31FC", "40b6af94-5ffc-4ee3-add9-c76818992274" }, // Rest Key List ( Security )
{ "C6DFE5AE-8C4C-49AD-8EC9-11CE03146F53", "0acf764f-5f60-4985-9d10-029cb042da0d" }, // Tag List ( Core )
{ "D8CCD577-2200-44C5-9073-FD16F174D364", "559978d5-a392-4bd1-8e04-055c2833f347" }, // Badge List ( CRM )
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_171_CHOP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string> { } );
        }

        #endregion
    }
}
