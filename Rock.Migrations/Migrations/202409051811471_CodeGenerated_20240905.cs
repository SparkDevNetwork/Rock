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
    public partial class CodeGenerated_20240905 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Administration.PageProperties
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Administration.PageProperties", "Page Properties", "Rock.Blocks.Administration.PageProperties, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "D256A348-E8DC-4886-A055-EAE44E71CE92" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.StreakMapEditor
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.StreakMapEditor", "Streak Map Editor", "Rock.Blocks.Engagement.StreakMapEditor, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "4935B24C-851A-4480-A907-EAEB90D594D2" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Security.SecurityChangeAuditList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Security.SecurityChangeAuditList", "Security Change Audit List", "Rock.Blocks.Security.SecurityChangeAuditList, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "5A2E4F3C-9915-4B67-8FFE-87056D2E68DF" );

            // Add/Update Obsidian Block Type
            //   Name:Page Properties
            //   Category:Administration
            //   EntityType:Rock.Blocks.Administration.PageProperties
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Page Properties", "Displays the page properties.", "Rock.Blocks.Administration.PageProperties", "Administration", "4C2E12B8-DCD5-4EA6-A853-A02A5B121D13" );

            // Add/Update Obsidian Block Type
            //   Name:Streak Map Editor
            //   Category:Engagement
            //   EntityType:Rock.Blocks.Engagement.StreakMapEditor
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Streak Map Editor", "Allows editing a streak occurrence, engagement, or exclusion map.", "Rock.Blocks.Engagement.StreakMapEditor", "Engagement", "B5616E10-0551-41BB-BD14-3ABA33E0040B" );

            // Add/Update Obsidian Block Type
            //   Name:Security Change Audit List
            //   Category:Security
            //   EntityType:Rock.Blocks.Security.SecurityChangeAuditList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Security Change Audit List", "Block for Security Change Audit List.", "Rock.Blocks.Security.SecurityChangeAuditList", "Security", "CFE6F48B-ED85-4FA8-B068-EFE116B32284" );

            // Attribute for BlockType
            //   BlockType: Business Detail
            //   Category: Finance
            //   Attribute: Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "729E1953-4CFF-46F0-8715-9D7892BADB4E", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "Entity Type", @"The type of entity that will provide context for this block", 0, @"", "833EBE1C-2BE4-4989-B347-C787C683023C" );

            // Attribute for BlockType
            //   BlockType: Page Properties
            //   Category: Administration
            //   Attribute: Enable Full Edit Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4C2E12B8-DCD5-4EA6-A853-A02A5B121D13", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Full Edit Mode", "EnableFullEditMode", "Enable Full Edit Mode", @"Have the block initially show a readonly summary view, in a panel, with Edit and Delete buttons. Also include Save and Cancel buttons.", 1, @"False", "66A3CAE5-D73E-446C-A31F-69B9DA35738C" );

            // Attribute for BlockType
            //   BlockType: Page Properties
            //   Category: Administration
            //   Attribute: Median Time to Serve Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4C2E12B8-DCD5-4EA6-A853-A02A5B121D13", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Median Time to Serve Detail Page", "MedianTimeDetailPage", "Median Time to Serve Detail Page", @"The page that shows details about the median time to serve was calculated.", 2, @"E556D6C5-E2DB-4041-81AB-4F582008155C", "F1564508-AB3C-49E3-8A0B-3168E0047C30" );

            // Attribute for BlockType
            //   BlockType: Streak Map Editor
            //   Category: Engagement
            //   Attribute: Show Streak Enrollment Exclusion Map
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B5616E10-0551-41BB-BD14-3ABA33E0040B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Streak Enrollment Exclusion Map", "IsEngagementExclusion", "Show Streak Enrollment Exclusion Map", @"If this map editor is placed in the context of a streak enrollment, should it show the person exclusion map for that streak enrollment?", 0, @"False", "331547F0-A162-48C3-A703-A3C25F0B5D8C" );

            // Attribute for BlockType
            //   BlockType: Security Change Audit List
            //   Category: Security
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CFE6F48B-ED85-4FA8-B068-EFE116B32284", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "88B7ED40-401C-4BCB-90FA-94EEE4BBC6C4" );

            // Attribute for BlockType
            //   BlockType: Security Change Audit List
            //   Category: Security
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CFE6F48B-ED85-4FA8-B068-EFE116B32284", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "56D7689D-3BDF-435B-9605-2F61BFCA07B1" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Business Detail
            //   Category: Finance
            //   Attribute: Entity Type
            RockMigrationHelper.DeleteAttribute( "833EBE1C-2BE4-4989-B347-C787C683023C" );

            // Attribute for BlockType
            //   BlockType: Security Change Audit List
            //   Category: Security
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "56D7689D-3BDF-435B-9605-2F61BFCA07B1" );

            // Attribute for BlockType
            //   BlockType: Security Change Audit List
            //   Category: Security
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "88B7ED40-401C-4BCB-90FA-94EEE4BBC6C4" );

            // Attribute for BlockType
            //   BlockType: Streak Map Editor
            //   Category: Engagement
            //   Attribute: Show Streak Enrollment Exclusion Map
            RockMigrationHelper.DeleteAttribute( "331547F0-A162-48C3-A703-A3C25F0B5D8C" );

            // Attribute for BlockType
            //   BlockType: Page Properties
            //   Category: Administration
            //   Attribute: Median Time to Serve Detail Page
            RockMigrationHelper.DeleteAttribute( "F1564508-AB3C-49E3-8A0B-3168E0047C30" );

            // Attribute for BlockType
            //   BlockType: Page Properties
            //   Category: Administration
            //   Attribute: Enable Full Edit Mode
            RockMigrationHelper.DeleteAttribute( "66A3CAE5-D73E-446C-A31F-69B9DA35738C" );

            // Delete BlockType 
            //   Name: Security Change Audit List
            //   Category: Security
            //   Path: -
            //   EntityType: Security Change Audit List
            RockMigrationHelper.DeleteBlockType( "CFE6F48B-ED85-4FA8-B068-EFE116B32284" );

            // Delete BlockType 
            //   Name: Streak Map Editor
            //   Category: Engagement
            //   Path: -
            //   EntityType: Streak Map Editor
            RockMigrationHelper.DeleteBlockType( "B5616E10-0551-41BB-BD14-3ABA33E0040B" );

            // Delete BlockType 
            //   Name: Page Properties
            //   Category: Administration
            //   Path: -
            //   EntityType: Page Properties
            RockMigrationHelper.DeleteBlockType( "4C2E12B8-DCD5-4EA6-A853-A02A5B121D13" );
        }
    }
}
