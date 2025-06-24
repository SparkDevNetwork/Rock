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
    public partial class CodeGenerated_20250624 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.BulkImport.BulkImportTool
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.BulkImport.BulkImportTool", "Bulk Import Tool", "Rock.Blocks.BulkImport.BulkImportTool, Rock.Blocks, Version=18.0.7.0, Culture=neutral, PublicKeyToken=null", false, false, "5B41F45E-2E09-4F97-8BEA-683AFFE0EB62" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.MediaElementList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.MediaElementList", "Media Element List", "Rock.Blocks.Cms.MediaElementList, Rock.Blocks, Version=18.0.7.0, Culture=neutral, PublicKeyToken=null", false, false, "9560305D-5ADA-4CE4-A67A-2FE2D606CFB8" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Group.GroupMemberList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Group.GroupMemberList", "Group Member List", "Rock.Blocks.Group.GroupMemberList, Rock.Blocks, Version=18.0.7.0, Culture=neutral, PublicKeyToken=null", false, false, "8CD71FCE-5F8A-46E0-A45A-504925002260" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Utility.StarkDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Utility.StarkDetail", "Stark Detail", "Rock.Blocks.Utility.StarkDetail, Rock.Blocks, Version=18.0.7.0, Culture=neutral, PublicKeyToken=null", false, false, "06B36FB8-34D9-4EE3-A24E-0E80BEDA313F" );

            // Add/Update Obsidian Block Type
            //   Name:Bulk Import
            //   Category:Bulk Import
            //   EntityType:Rock.Blocks.BulkImport.BulkImportTool
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Bulk Import", "Block to import Slingshot files into Rock using BulkImport", "Rock.Blocks.BulkImport.BulkImportTool", "Bulk Import", "66F5882F-163C-4616-9B39-2F063611DB22" );

            // Add/Update Obsidian Block Type
            //   Name:Media Element List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.MediaElementList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Media Element List", "Displays a list of media elements.", "Rock.Blocks.Cms.MediaElementList", "CMS", "A713CBD4-549E-4795-9468-828EE2F8C21D" );

            // Add/Update Obsidian Block Type
            //   Name:Group Member List
            //   Category:Obsidian > Group
            //   EntityType:Rock.Blocks.Group.GroupMemberList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Group Member List", "Lists the members of a group.", "Rock.Blocks.Group.GroupMemberList", "Obsidian > Group", "5959A986-A40B-45C6-A757-E66C67AE3BD9" );

            // Add/Update Obsidian Block Type
            //   Name:Stark Detail
            //   Category:Obsidian > Utility
            //   EntityType:Rock.Blocks.Utility.StarkDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Stark Detail", "An example block.", "Rock.Blocks.Utility.StarkDetail", "Obsidian > Utility", "B822D570-05B4-4DE1-8994-F9D918019ED9" );


            // Attribute for BlockType
            //   BlockType: Bulk Import
            //   Category: Bulk Import
            //   Attribute: Person Record Import Batch Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66F5882F-163C-4616-9B39-2F063611DB22", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Record Import Batch Size", "PersonRecordImportBatchSize", "Person Record Import Batch Size", @"If importing more than this many records, the import will be broken up into smaller batches to optimize memory use. If you run into memory utilization problems while importing a large number of records, consider decreasing this value. (A value less than 1 will result in the default of 25,000 records.)", 0, @"25000", "FA2FAD54-1A96-4DC0-8715-7F96D9FE880D" );

            // Attribute for BlockType
            //   BlockType: Bulk Import
            //   Category: Bulk Import
            //   Attribute: Financial Record Import Batch Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66F5882F-163C-4616-9B39-2F063611DB22", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Financial Record Import Batch Size", "FinancialRecordImportBatchSize", "Financial Record Import Batch Size", @"If importing more than this many records, the import will be broken up into smaller batches to optimize memory use. If you run into memory utilization problems while importing a large number of records, consider decreasing this value. (A value less than 1 will result in the default of 100,000 records.)", 1, @"100000", "199F5F94-363D-4A32-BEC2-8A5A7C70390E" );

            // Attribute for BlockType
            //   BlockType: Media Element List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A713CBD4-549E-4795-9468-828EE2F8C21D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the media element details.", 0, @"", "7194F0E3-BC6D-409C-A6AC-3B4D4176ABDD" );

            // Attribute for BlockType
            //   BlockType: Media Element List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A713CBD4-549E-4795-9468-828EE2F8C21D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "EEFF4FA6-A3D0-411B-B1F7-EF4A6EBEED0D" );

            // Attribute for BlockType
            //   BlockType: Media Element List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A713CBD4-549E-4795-9468-828EE2F8C21D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "2EEB994A-AF94-4779-AE2F-4A3F00B9B4A5" );

            // Attribute for BlockType
            //   BlockType: Stark Detail
            //   Category: Obsidian > Utility
            //   Attribute: Show Email Address
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B822D570-05B4-4DE1-8994-F9D918019ED9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Email Address", "ShowEmailAddress", "Show Email Address", @"Should the email address be shown?", 1, @"True", "7747E344-F8F0-449A-BE65-5F63A39FFD90" );

            // Attribute for BlockType
            //   BlockType: Stark Detail
            //   Category: Obsidian > Utility
            //   Attribute: Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B822D570-05B4-4DE1-8994-F9D918019ED9", "3D045CAE-EA72-4A04-B7BE-7FD1D6214217", "Email", "Email", "Email", @"The Email address to show.", 2, @"ted@rocksolidchurchdemo.com", "A9C39CAF-7FBC-4D8F-B16A-3405B9B9C0C0" );

            // Add Block Attribute Value
            //   Block: AI Provider List
            //   BlockType: AI Provider List
            //   Category: Core
            //   Block Location: Page=AI Providers, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "76338EFC-16E5-4CC8-922A-AB7A26013CC2", "6CD8F5D3-EAFD-4110-A230-B9F63EB802EB", @"" );

            // Add Block Attribute Value
            //   Block: AI Provider List
            //   BlockType: AI Provider List
            //   Category: Core
            //   Block Location: Page=AI Providers, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "76338EFC-16E5-4CC8-922A-AB7A26013CC2", "FE989BAE-45B1-4524-8020-4A3EC1752720", @"False" );

            // Add Block Attribute Value
            //   Block: Audit Information List
            //   BlockType: Audit List
            //   Category: Core
            //   Block Location: Page=Audit Information, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "9E03A8FF-545A-45A3-9A19-252AA94190D8", "8E418AB8-1F04-44BC-8438-2CDB4EF2AF79", @"" );

            // Add Block Attribute Value
            //   Block: Audit Information List
            //   BlockType: Audit List
            //   Category: Core
            //   Block Location: Page=Audit Information, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "9E03A8FF-545A-45A3-9A19-252AA94190D8", "D74B0439-2105-4165-A3A3-D466C2914463", @"False" );
            RockMigrationHelper.UpdateFieldType( "Device", "", "Rock", "Rock.Field.Types.DeviceFieldType", "D7F5D737-BDC9-4656-951E-08325D0543FD" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: Stark Detail
            //   Category: Obsidian > Utility
            //   Attribute: Email
            RockMigrationHelper.DeleteAttribute( "A9C39CAF-7FBC-4D8F-B16A-3405B9B9C0C0" );

            // Attribute for BlockType
            //   BlockType: Stark Detail
            //   Category: Obsidian > Utility
            //   Attribute: Show Email Address
            RockMigrationHelper.DeleteAttribute( "7747E344-F8F0-449A-BE65-5F63A39FFD90" );

            // Attribute for BlockType
            //   BlockType: Media Element List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "2EEB994A-AF94-4779-AE2F-4A3F00B9B4A5" );

            // Attribute for BlockType
            //   BlockType: Media Element List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "EEFF4FA6-A3D0-411B-B1F7-EF4A6EBEED0D" );

            // Attribute for BlockType
            //   BlockType: Media Element List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "7194F0E3-BC6D-409C-A6AC-3B4D4176ABDD" );

            // Attribute for BlockType
            //   BlockType: Bulk Import
            //   Category: Bulk Import
            //   Attribute: Financial Record Import Batch Size
            RockMigrationHelper.DeleteAttribute( "199F5F94-363D-4A32-BEC2-8A5A7C70390E" );

            // Attribute for BlockType
            //   BlockType: Bulk Import
            //   Category: Bulk Import
            //   Attribute: Person Record Import Batch Size
            RockMigrationHelper.DeleteAttribute( "FA2FAD54-1A96-4DC0-8715-7F96D9FE880D" );


            // Delete BlockType 
            //   Name: Stark Detail
            //   Category: Obsidian > Utility
            //   Path: -
            //   EntityType: Stark Detail
            RockMigrationHelper.DeleteBlockType( "B822D570-05B4-4DE1-8994-F9D918019ED9" );

            // Delete BlockType 
            //   Name: Group Member List
            //   Category: Obsidian > Group
            //   Path: -
            //   EntityType: Group Member List
            RockMigrationHelper.DeleteBlockType( "5959A986-A40B-45C6-A757-E66C67AE3BD9" );

            // Delete BlockType 
            //   Name: Media Element List
            //   Category: CMS
            //   Path: -
            //   EntityType: Media Element List
            RockMigrationHelper.DeleteBlockType( "A713CBD4-549E-4795-9468-828EE2F8C21D" );

            // Delete BlockType 
            //   Name: Bulk Import
            //   Category: Bulk Import
            //   Path: -
            //   EntityType: Bulk Import Tool
            RockMigrationHelper.DeleteBlockType( "66F5882F-163C-4616-9B39-2F063611DB22" );
        }
    }
}
