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
    public partial class CodeGenerated_20240718 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.WebFarm.WebFarmNodeLogList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.WebFarm.WebFarmNodeLogList", "Web Farm Node Log List", "Rock.Blocks.WebFarm.WebFarmNodeLogList, Rock.Blocks, Version=1.16.6.3, Culture=neutral, PublicKeyToken=null", false, false, "57E8356D-6E59-4F5B-8DB9-A274B7A0EFD8" );

            // Add/Update Obsidian Block Type
            //   Name:Web Farm Node Log List
            //   Category:WebFarm
            //   EntityType:Rock.Blocks.WebFarm.WebFarmNodeLogList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Web Farm Node Log List", "Displays a list of web farm node logs.", "Rock.Blocks.WebFarm.WebFarmNodeLogList", "WebFarm", "6C824483-6624-460B-9DD8-E127B25CA65D" );

            // Attribute for BlockType
            //   BlockType: Event Registration Wizard
            //   Category: Event
            //   Attribute: Require URL Slug
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B1C7E983-5000-4CBE-84DD-6B7D428635AC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require URL Slug", "RequireURLSlug ", "Require URL Slug", @"If set to ""Yes"", you will be required to input a URL Slug.", 17, @"False", "B1103800-B17E-4A24-B281-485297DE0C8A" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk
            //   Category: Check-in
            //   Attribute: Show Counts By Location
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A27FD0AA-67EE-44C3-9E5F-3289C6A210F3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Counts By Location", "ShowCountsByLocation", "Show Counts By Location", @"When displaying attendance counts on the admin login screen this will group the counts by location first instead of area first.", 1, @"False", "9E9CF3F4-1838-4F98-B62F-A8790DF53F0B" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk
            //   Category: Check-in
            //   Attribute: Promotions Content Channel
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A27FD0AA-67EE-44C3-9E5F-3289C6A210F3", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Promotions Content Channel", "PromotionsContentChannel", "Promotions Content Channel", @"The content channel to use for displaying promotions on the kiosk welcome screen.", 2, @"", "5AED205E-1477-4C29-B7F6-99DE33FCB8EB" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk
            //   Category: Check-in
            //   Attribute: REST Key
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A27FD0AA-67EE-44C3-9E5F-3289C6A210F3", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "REST Key", "RestKey", "REST Key", @"If your kiosk pages are configured for anonymous access then you must create a REST key with access to the check-in API endpoints and select it here.", 3, @"", "6B5B7141-7F40-4D58-AF48-EA89F2CDDC9D" );

            // Attribute for BlockType
            //   BlockType: Web Farm Node Log List
            //   Category: WebFarm
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C824483-6624-460B-9DD8-E127B25CA65D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "8238E099-2EFD-44A5-BE64-78C831CBF43B" );

            // Attribute for BlockType
            //   BlockType: Web Farm Node Log List
            //   Category: WebFarm
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C824483-6624-460B-9DD8-E127B25CA65D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "60E40A77-17FC-4E29-AB01-5D87F38EA9D0" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk
            //   Category: Check-in
            //   Attribute: REST Key
            RockMigrationHelper.DeleteAttribute( "6B5B7141-7F40-4D58-AF48-EA89F2CDDC9D" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk
            //   Category: Check-in
            //   Attribute: Promotions Content Channel
            RockMigrationHelper.DeleteAttribute( "5AED205E-1477-4C29-B7F6-99DE33FCB8EB" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk
            //   Category: Check-in
            //   Attribute: Show Counts By Location
            RockMigrationHelper.DeleteAttribute( "9E9CF3F4-1838-4F98-B62F-A8790DF53F0B" );

            // Attribute for BlockType
            //   BlockType: Web Farm Node Log List
            //   Category: WebFarm
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "60E40A77-17FC-4E29-AB01-5D87F38EA9D0" );

            // Attribute for BlockType
            //   BlockType: Web Farm Node Log List
            //   Category: WebFarm
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "8238E099-2EFD-44A5-BE64-78C831CBF43B" );

            // Attribute for BlockType
            //   BlockType: Event Registration Wizard
            //   Category: Event
            //   Attribute: Require URL Slug
            RockMigrationHelper.DeleteAttribute( "B1103800-B17E-4A24-B281-485297DE0C8A" );

            // Delete BlockType 
            //   Name: Web Farm Node Log List
            //   Category: WebFarm
            //   Path: -
            //   EntityType: Web Farm Node Log List
            RockMigrationHelper.DeleteBlockType( "6C824483-6624-460B-9DD8-E127B25CA65D" );
        }
    }
}
