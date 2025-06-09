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
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20250513 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ChopBlocksUp();
            UpdateMergeFieldAttributeDescriptionUp();
            AddProximityCheckInAttributeUp();
            UpdateLayoutBlockListNamingObsidianUp();
            ObsoleteAppleTVPageListBlockUp();
            JPH_ImproveLoginHistoryMigration_20250506_Up();
            ChopBlocksForV17Up();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddProximityCheckInAttributeDown();
        }

        #region KH: Register block attributes for chop job in v18.0 (18.0.6)

        private void ChopBlocksUp()
        {
            RegisterBlockAttributesForChop();
            ChopBlockTypesv18_0();
        }

        /// <summary>
        /// Ensure the Entity, BlockType and Block Setting Attribute records exist
        /// before the chop job runs. Any missing attributes would cause the job to fail.
        /// </summary>
        private void RegisterBlockAttributesForChop()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.LavaShortcodeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.LavaShortcodeList", "Lava Shortcode List", "Rock.Blocks.Cms.LavaShortcodeList, Rock.Blocks, Version=18.0.5.0, Culture=neutral, PublicKeyToken=null", false, false, "B02078CC-FA42-4249-ABE0-7E166C63D2B6" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.PageShortLinkClickList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.PageShortLinkClickList", "Page Short Link Click List", "Rock.Blocks.Cms.PageShortLinkClickList, Rock.Blocks, Version=18.0.5.0, Culture=neutral, PublicKeyToken=null", false, false, "AA860DC7-D590-4D0E-BBB3-16990F2CD680" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.PageShortLinkList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.PageShortLinkList", "Page Short Link List", "Rock.Blocks.Cms.PageShortLinkList, Rock.Blocks, Version=18.0.5.0, Culture=neutral, PublicKeyToken=null", false, false, "B9825E53-D074-4280-A1A3-E20771E34625" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.AchievementAttemptDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.AchievementAttemptDetail", "Achievement Attempt Detail", "Rock.Blocks.Engagement.AchievementAttemptDetail, Rock.Blocks, Version=18.0.5.0, Culture=neutral, PublicKeyToken=null", false, false, "A80564D5-701B-4F3A-8BA1-20BAA2304DA6" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.AchievementAttemptList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.AchievementAttemptList", "Achievement Attempt List", "Rock.Blocks.Engagement.AchievementAttemptList, Rock.Blocks, Version=18.0.5.0, Culture=neutral, PublicKeyToken=null", false, false, "039C87AE-0835-4844-AC9B-A66AE1D19530" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.AchievementTypeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.AchievementTypeDetail", "Achievement Type Detail", "Rock.Blocks.Engagement.AchievementTypeDetail, Rock.Blocks, Version=18.0.5.0, Culture=neutral, PublicKeyToken=null", false, false, "8B22D387-C8F3-41FF-99EF-EE4F088610A1" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.AchievementTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.AchievementTypeList", "Achievement Type List", "Rock.Blocks.Engagement.AchievementTypeList, Rock.Blocks, Version=18.0.5.0, Culture=neutral, PublicKeyToken=null", false, false, "E9E67424-1FD8-4A85-9E7B-C919117BDE1A" );

            // Add/Update Obsidian Block Type
            //   Name:Achievement Attempt Detail
            //   Category:Engagement
            //   EntityType:Rock.Blocks.Engagement.AchievementAttemptDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Achievement Attempt Detail", "Displays the details of a particular achievement attempt.", "Rock.Blocks.Engagement.AchievementAttemptDetail", "Engagement", "FBE75C18-7F71-4D23-A546-7A17CF944BA6" );

            // Add/Update Obsidian Block Type
            //   Name:Achievement Attempt List
            //   Category:Achievements
            //   EntityType:Rock.Blocks.Engagement.AchievementAttemptList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Achievement Attempt List", "Lists all the people that have made an attempt at earning an achievement.", "Rock.Blocks.Engagement.AchievementAttemptList", "Achievements", "B294C1B9-8368-422C-8054-9672C7F41477" );

            // Add/Update Obsidian Block Type
            //   Name:Achievement Type Detail
            //   Category:Achievements
            //   EntityType:Rock.Blocks.Engagement.AchievementTypeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Achievement Type Detail", "Displays the details of the given Achievement Type for editing.", "Rock.Blocks.Engagement.AchievementTypeDetail", "Achievements", "EDDFCAFF-70AA-4791-B051-6567B37518C4" );

            // Add/Update Obsidian Block Type
            //   Name:Achievement Type List
            //   Category:Streaks
            //   EntityType:Rock.Blocks.Engagement.AchievementTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Achievement Type List", "Shows a list of all achievement types.", "Rock.Blocks.Engagement.AchievementTypeList", "Streaks", "4ACFBF3F-3D49-4AE3-B468-529F79DA9898" );

            // Add/Update Obsidian Block Type
            //   Name:Lava Shortcode List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.LavaShortcodeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Lava Shortcode List", "Lists Lava Shortcode in the system.", "Rock.Blocks.Cms.LavaShortcodeList", "CMS", "09FD3746-48D1-4B94-AAA9-6896443AA43E" );

            // Add/Update Obsidian Block Type
            //   Name:Page Short Link Click List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.PageShortLinkClickList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Page Short Link Click List", "Lists clicks for a particular short link.", "Rock.Blocks.Cms.PageShortLinkClickList", "CMS", "E44CAC85-346F-41A4-884B-A6FB5FC64DE1" );

            // Add/Update Obsidian Block Type
            //   Name:Page Short Link List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.PageShortLinkList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Page Short Link List", "Displays a list of page short links.", "Rock.Blocks.Cms.PageShortLinkList", "CMS", "D25FF675-07C8-4E2D-A3FA-38BA3468B4AE" );

            // Attribute for BlockType
            //   BlockType: Achievement Attempt Detail
            //   Category: Engagement
            //   Attribute: Achievement Type Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FBE75C18-7F71-4D23-A546-7A17CF944BA6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Achievement Type Page", "AchievementPage", "Achievement Type Page", @"Page used for viewing the achievement type that this attempt is toward.", 2, @"", "4DC7AAC2-6F4E-4264-9142-166DE331F9F9" );

            // Attribute for BlockType
            //   BlockType: Achievement Attempt List
            //   Category: Achievements
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B294C1B9-8368-422C-8054-9672C7F41477", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "5E15CC7B-190E-4547-A4EE-20BA78219B96" );

            // Attribute for BlockType
            //   BlockType: Achievement Attempt List
            //   Category: Achievements
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B294C1B9-8368-422C-8054-9672C7F41477", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "FEE6722A-32E5-43B5-BBED-49D6EE6366C4" );

            // Attribute for BlockType
            //   BlockType: Achievement Attempt List
            //   Category: Achievements
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B294C1B9-8368-422C-8054-9672C7F41477", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the achievement attempt details.", 0, @"", "AC750FE6-33DF-4617-B9C1-38EEEEB781E2" );

            // Attribute for BlockType
            //   BlockType: Achievement Type List
            //   Category: Streaks
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4ACFBF3F-3D49-4AE3-B468-529F79DA9898", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "8DF9D395-BFE8-4CFB-8340-B4AB516BC49D" );

            // Attribute for BlockType
            //   BlockType: Achievement Type List
            //   Category: Streaks
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4ACFBF3F-3D49-4AE3-B468-529F79DA9898", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "B998A4BF-9A2A-4486-972A-C497FB3DE022" );

            // Attribute for BlockType
            //   BlockType: Achievement Type List
            //   Category: Streaks
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4ACFBF3F-3D49-4AE3-B468-529F79DA9898", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the achievement type details.", 0, @"", "59F4B591-3921-4AFB-BF51-5E429B2A72D7" );

            // Attribute for BlockType
            //   BlockType: Lava Shortcode List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "09FD3746-48D1-4B94-AAA9-6896443AA43E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "58055646-80F7-47CA-9DDB-6C1A0BDE39C1" );

            // Attribute for BlockType
            //   BlockType: Page Short Link Click List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E44CAC85-346F-41A4-884B-A6FB5FC64DE1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "AB7E8098-ED68-4C0F-BC65-06B116F5A2D3" );

            // Attribute for BlockType
            //   BlockType: Page Short Link Click List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E44CAC85-346F-41A4-884B-A6FB5FC64DE1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "6CB70101-5745-4DBB-9162-7D1C55E007CC" );

            // Attribute for BlockType
            //   BlockType: Page Short Link List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D25FF675-07C8-4E2D-A3FA-38BA3468B4AE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "54AB7012-8515-42EC-9440-854DAFF40E0C" );

            // Attribute for BlockType
            //   BlockType: Page Short Link List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D25FF675-07C8-4E2D-A3FA-38BA3468B4AE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "0F77028D-6D77-4E75-897E-642F34F342EE" );

            // Attribute for BlockType
            //   BlockType: Page Short Link List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D25FF675-07C8-4E2D-A3FA-38BA3468B4AE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the page short link details.", 0, @"", "04A2FCF6-DE8F-4973-A9F9-055A860E7AEB" );
        }

        private void ChopBlockTypesv18_0()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types 18.0 (18.0.6)",
                blockTypeReplacements: new Dictionary<string, string> {
                    // blocks chopped in v18.0
{ "1D7B8095-9E5B-4A9A-A519-69E1746140DD", "e44cac85-346f-41a4-884b-a6fb5fc64de1" }, // Page Short Link Click List ( CMS )
{ "4C4A46CD-1622-4642-A655-11585C5D3D31", "eddfcaff-70aa-4791-b051-6567b37518c4" }, // Achievement Type Detail ( Achievements )
{ "7E4663CD-2176-48D6-9CC2-2DBC9B880C23", "fbe75c18-7f71-4d23-a546-7a17cf944ba6" }, // Achievement Attempt Detail ( Engagement )
{ "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB", "b294c1b9-8368-422c-8054-9672c7f41477" }, // Achievement Attempt List ( Achievements )
{ "C26C7979-81C1-4A20-A167-35415CD7FED3", "09FD3746-48D1-4B94-AAA9-6896443AA43E" }, // Lava Shortcode List ( CMS )
{ "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD", "4acfbf3f-3d49-4ae3-b468-529f79da9898" }, // Achievement Type List ( Streaks )
{ "D6D87CCC-DB6D-4138-A4B5-30F0707A5300", "d25ff675-07c8-4e2d-a3fa-38ba3468b4ae" }, // Page Short Link List ( CMS )
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_180_CHOP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string> { } );
        }

        #endregion

        #region NL: UpdateMergeFieldAttributeDescription Plugin Migration #245

        private void UpdateMergeFieldAttributeDescriptionUp()
        {
            // We need to fix the descriptions of the following attributes due to being incorrect:
            Sql( @"
                UPDATE [Attribute]
                SET [Description] = 'The Lava syntax for accessing values from the check-in state object.'
                WHERE [Guid] = '51eb8583-55ea-4431-8b66-b5bd0f83d81e';
            " );

            Sql( @"
                UPDATE [Attribute] 
                SET [Description] = 'The Lava syntax to use for formatting addresses.'
                WHERE [Guid] = 'b6ef4138-c488-4043-a628-d35f91503843'
            " );
        }

        #endregion

        #region DH: AddProximityCheckInAttribute Plugin Migration #246

        private void AddProximityCheckInAttributeUp()
        {
            // Add new check-in configuration attribute.
            RockMigrationHelper.AddOrUpdateEntityAttributeByGuid( "Rock.Model.GroupType",
                SystemGuid.FieldType.BOOLEAN,
                "GroupTypePurposeValueId",
                "0",
                "Enable Proximity Check-in",
                "Enable Proximity Check-in",
                string.Empty,
                0,
                string.Empty,
                "0aadc315-6922-4b1c-a29f-5666df5ecadc",
                "core_EnableProximityCheckIn" );

            // Set the GroupTypePurposeValueId value.
            Sql( @"
DECLARE @CheckInTemplateValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '4a406cb0-495b-4795-b788-52bdfde00b01')

UPDATE [Attribute]
SET [EntityTypeQualifierValue] = @CheckInTemplateValueId
WHERE [Guid] = '0aadc315-6922-4b1c-a29f-5666df5ecadc'" );

            // Enable attendance for the adult service area.
            Sql( "UPDATE [GroupType] SET [TakesAttendance] = 1 WHERE [Guid] = '235bae2b-5760-4763-aadf-3938f34ba100'" );
        }

        private void AddProximityCheckInAttributeDown()
        {
            RockMigrationHelper.DeleteAttribute( "0aadc315-6922-4b1c-a29f-5666df5ecadc" );
        }

        #endregion

        #region NL UpdateLayoutBlockListNamingObsidian Plugin Migration #247

        private void UpdateLayoutBlockListNamingObsidianUp()
        {
            Sql( @"
UPDATE [EntityType]
SET 
    [Name] = 'Rock.Blocks.Cms.LayoutBlockList',
    [FriendlyName] = 'Layout Block List',
    [AssemblyName] = 'Rock.Blocks.Cms.LayoutBlockList, Rock.Blocks, Version=17.1, Culture=neutral, PublicKeyToken=null',
    [IsEntity] = 0,
    [IsSecured] = 0
WHERE [Guid] = '9CF1AA10-24E4-4530-A345-57DA4CFE9595';
" );

            // Add/Update Obsidian Block Type
            //  Name: Layout Block List
            //  Category: Obsidian > CMS
            //  EntityType:Rock.Blocks.Cms.LayoutBlockList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Layout Block List", "Displays a list of blocks.", "Rock.Blocks.Cms.LayoutBlockList", "CMS", "EA8BE085-D420-4D1B-A538-2C0D4D116E0A" );
        }

        #endregion

        #region NA: Obsolete the Apple TV Page List Block

        private void ObsoleteAppleTVPageListBlockUp()
        {
            Sql( @"UPDATE [BlockType] SET [Name] = 'Apple TV Page List (Obsolete)' WHERE [Guid] = 'a759218b-1c72-446c-8994-8559ba72941e'" );
        }

        #endregion

        #region JPH: Improve Login History Migration

        /// <summary>
        /// JPH: Add a post update job to prepare the new HistoryLogin table to record only true login events moving forward.
        /// </summary>
        private void JPH_ImproveLoginHistoryMigration_20250506_Up()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.1 - Migrate Login History",
                description: "This job will prepare the new HistoryLogin table to record only true login events moving forward.",
                jobType: "Rock.Jobs.PostV171MigrateLoginHistory",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_171_MIGRATE_LOGIN_HISTORY );
        }

        #endregion

        #region KH: Register block attributes for chop job in v17.1 (18.0.6)

        private void ChopBlocksForV17Up()
        {
            RegisterBlockAttributesForV17Chop();
            ChopBlockTypesv17_1();
        }

        /// <summary>
        /// Ensure the Entity, BlockType and Block Setting Attribute records exist
        /// before the chop job runs. Any missing attributes would cause the job to fail.
        /// </summary>
        private void RegisterBlockAttributesForV17Chop()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.LogSettings
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.LogSettings", "Log Settings", "Rock.Blocks.Cms.LogSettings, Rock.Blocks, Version=17.1.1.0, Culture=neutral, PublicKeyToken=null", false, false, "E5F272D4-E63F-46E7-9429-0D62CB458FD1" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.AssetStorageProviderList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.AssetStorageProviderList", "Asset Storage Provider List", "Rock.Blocks.Core.AssetStorageProviderList, Rock.Blocks, Version=17.1.1.0, Culture=neutral, PublicKeyToken=null", false, false, "172E0874-E30F-4FD1-A340-99A8134D9779" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.LogViewer
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.LogViewer", "Log Viewer", "Rock.Blocks.Core.LogViewer, Rock.Blocks, Version=17.1.1.0, Culture=neutral, PublicKeyToken=null", false, false, "DB6A13D0-964D-4839-9E32-BF1E522D176A" );

            // Add/Update Obsidian Block Type
            //   Name:Asset Storage Provider List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.AssetStorageProviderList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Asset Storage Provider List", "Displays a list of asset storage providers.", "Rock.Blocks.Core.AssetStorageProviderList", "Core", "2663E57E-ED73-49FE-BA16-69B4B829C488" );

            // Add/Update Obsidian Block Type
            //   Name:Log Settings
            //   Category:Administration
            //   EntityType:Rock.Blocks.Cms.LogSettings
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Log Settings", "Block to edit rock log settings.", "Rock.Blocks.Cms.LogSettings", "Administration", "FA01630C-18FB-472F-8BF1-013AF257DE3F" );

            // Add/Update Obsidian Block Type
            //   Name:Logs
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.LogViewer
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Logs", "Block to view system logs.", "Rock.Blocks.Core.LogViewer", "Core", "E35992D6-C175-4C35-9DA6-A9A7115E1FFD" );

            // Attribute for BlockType
            //   BlockType: Asset Storage Provider List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2663E57E-ED73-49FE-BA16-69B4B829C488", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "DFFA9B37-1332-4B51-A6A3-A5FED0BF8939" );

            // Attribute for BlockType
            //   BlockType: Asset Storage Provider List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2663E57E-ED73-49FE-BA16-69B4B829C488", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "8E2B6DEF-D02A-41FF-BFC3-CEFF5F5BBE69" );

            // Attribute for BlockType
            //   BlockType: Asset Storage Provider List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2663E57E-ED73-49FE-BA16-69B4B829C488", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the asset storage provider details.", 0, @"", "10FA745A-A70D-45F4-AFFB-EF3B6FB52345" );

            // Attribute for BlockType
            //   BlockType: Logs
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E35992D6-C175-4C35-9DA6-A9A7115E1FFD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "36BE6E4B-8843-44A4-90AC-675508664FF9" );

            // Attribute for BlockType
            //   BlockType: Logs
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E35992D6-C175-4C35-9DA6-A9A7115E1FFD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "18472384-59A7-4EBF-B507-D491285018D6" );
        }

        private void ChopBlockTypesv17_1()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types 17.1 (18.0.6)",
                blockTypeReplacements: new Dictionary<string, string> {
                    // blocks chopped in v17.1 (Pre-Alpha: 18.0.6)
{ "6059FC03-E398-4359-8632-909B63FFA550", "E35992D6-C175-4C35-9DA6-A9A7115E1FFD" }, // Logs ( Core )
{ "6ABC44FD-C4D7-4E30-8537-3A065B493453", "fa01630c-18fb-472f-8bf1-013af257de3f" }, // Log Settings ( Administration )
{ "7A8599B0-6B69-4E1F-9D12-CA9874E8E5D8", "2663e57e-ed73-49fe-ba16-69b4b829c488" }, // Asset Storage Provider List ( Core )
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
