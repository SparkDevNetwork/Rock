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
    public partial class CodeGenerated_20240425 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.PersonSignalList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.PersonSignalList", "Person Signal List", "Rock.Blocks.Core.PersonSignalList, Rock.Blocks, Version=1.16.5.1, Culture=neutral, PublicKeyToken=null", false, false, "DB2E3CE3-94BD-4D12-8ADD-598BF938E8E1" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Event.RegistrationInstanceFeeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Event.RegistrationInstanceFeeList", "Registration Instance Fee List", "Rock.Blocks.Event.RegistrationInstanceFeeList, Rock.Blocks, Version=1.16.5.1, Culture=neutral, PublicKeyToken=null", false, false, "10F4D211-FC60-40D5-B96B-6B9FCBDBEFAC" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Prayer.PrayerCommentList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Prayer.PrayerCommentList", "Prayer Comment List", "Rock.Blocks.Prayer.PrayerCommentList, Rock.Blocks, Version=1.16.5.1, Culture=neutral, PublicKeyToken=null", false, false, "B2F1B644-836D-46A6-86C9-8FBB26D96EA7" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Prayer.PrayerRequestList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Prayer.PrayerRequestList", "Prayer Request List", "Rock.Blocks.Prayer.PrayerRequestList, Rock.Blocks, Version=1.16.5.1, Culture=neutral, PublicKeyToken=null", false, false, "E8BE562A-BB24-47A9-B3DF-63CFB508F831" );

            // Add/Update Obsidian Block Type
            //   Name:Person Signal List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.PersonSignalList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Person Signal List", "Displays a list of person signals.", "Rock.Blocks.Core.PersonSignalList", "Core", "653052A0-CA1C-41B8-8340-4B13149C6E66" );

            // Add/Update Obsidian Block Type
            //   Name:Registration Instance - Fee List
            //   Category:Event
            //   EntityType:Rock.Blocks.Event.RegistrationInstanceFeeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Registration Instance - Fee List", "Displays the fees related to an event registration instance.", "Rock.Blocks.Event.RegistrationInstanceFeeList", "Event", "DBCFB477-0553-4BAE-BAC9-2AEC38E1DA37" );

            // Add/Update Obsidian Block Type
            //   Name:Prayer Comment List
            //   Category:Core
            //   EntityType:Rock.Blocks.Prayer.PrayerCommentList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Prayer Comment List", "Displays a list of prayer comments for the configured top-level group category.", "Rock.Blocks.Prayer.PrayerCommentList", "Core", "3F997DA7-AC42-41C9-97F1-2069BB9D9E5C" );

            // Add/Update Obsidian Block Type
            //   Name:Prayer Request List
            //   Category:Prayer
            //   EntityType:Rock.Blocks.Prayer.PrayerRequestList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Prayer Request List", "Displays a list of prayer requests.", "Rock.Blocks.Prayer.PrayerRequestList", "Prayer", "E860F577-F30D-4197-87F0-C3DC6132F537" );

            // Attribute for BlockType
            //   BlockType: Person Signal List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "653052A0-CA1C-41B8-8340-4B13149C6E66", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "00E16ED2-BC73-41C4-BA16-471725A23547" );

            // Attribute for BlockType
            //   BlockType: Person Signal List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "653052A0-CA1C-41B8-8340-4B13149C6E66", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F6CBDC1B-B5E6-4611-9A3B-F8229E3C27EA" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Fee List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DBCFB477-0553-4BAE-BAC9-2AEC38E1DA37", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "F858F67A-552A-4D76-BB8B-86C8A124BAB4" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Fee List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DBCFB477-0553-4BAE-BAC9-2AEC38E1DA37", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "A885C578-4EFF-40BE-9F34-ED183FEF6D97" );

            // Attribute for BlockType
            //   BlockType: Prayer Comment List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3F997DA7-AC42-41C9-97F1-2069BB9D9E5C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "5A1D49FC-BA17-45FD-A5E1-D1715294DB37" );

            // Attribute for BlockType
            //   BlockType: Prayer Comment List
            //   Category: Core
            //   Attribute: Category Selection
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3F997DA7-AC42-41C9-97F1-2069BB9D9E5C", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Category Selection", "PrayerRequestCategory", "Category Selection", @"A top level category. Only prayer requests comments under this category will be shown.", 1, @"", "370DB580-B342-48A5-8FED-4C92EF121974" );

            // Attribute for BlockType
            //   BlockType: Prayer Comment List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3F997DA7-AC42-41C9-97F1-2069BB9D9E5C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "19B8566A-34D0-4CA9-8868-928D17538CED" );

            // Attribute for BlockType
            //   BlockType: Prayer Comment List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3F997DA7-AC42-41C9-97F1-2069BB9D9E5C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "0FF4FDB9-C084-455D-8475-1B6B9EF8FF7A" );

            // Attribute for BlockType
            //   BlockType: Prayer Request List
            //   Category: Prayer
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E860F577-F30D-4197-87F0-C3DC6132F537", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the prayer request details.", 0, @"", "9ECF9D1D-47E3-4382-B83B-65353367EE3E" );

            // Attribute for BlockType
            //   BlockType: Prayer Request List
            //   Category: Prayer
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E860F577-F30D-4197-87F0-C3DC6132F537", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "97D35B90-EF8B-4B49-8440-49709A57BFF7" );

            // Attribute for BlockType
            //   BlockType: Prayer Request List
            //   Category: Prayer
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E860F577-F30D-4197-87F0-C3DC6132F537", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "74407C2D-E2C0-4068-B46D-2CAD7B4613DF" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Finance
            //   Attribute: Show Transaction Count Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1950524-E959-440F-9CF6-1A8B9B7527D8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Transaction Count Column", "ShowTransactionCountColumn", "Show Transaction Count Column", @"Should the transaction count column be displayed.", 3, @"False", "6E6F54E4-F0F9-4975-BFAE-2F0E55522546" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {            
            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Finance
            //   Attribute: Show Transaction Count Column
            RockMigrationHelper.DeleteAttribute( "6E6F54E4-F0F9-4975-BFAE-2F0E55522546" );

            // Attribute for BlockType
            //   BlockType: Prayer Request List
            //   Category: Prayer
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "74407C2D-E2C0-4068-B46D-2CAD7B4613DF" );

            // Attribute for BlockType
            //   BlockType: Prayer Request List
            //   Category: Prayer
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "97D35B90-EF8B-4B49-8440-49709A57BFF7" );

            // Attribute for BlockType
            //   BlockType: Prayer Request List
            //   Category: Prayer
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "9ECF9D1D-47E3-4382-B83B-65353367EE3E" );

            // Attribute for BlockType
            //   BlockType: Prayer Comment List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "0FF4FDB9-C084-455D-8475-1B6B9EF8FF7A" );

            // Attribute for BlockType
            //   BlockType: Prayer Comment List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "19B8566A-34D0-4CA9-8868-928D17538CED" );

            // Attribute for BlockType
            //   BlockType: Prayer Comment List
            //   Category: Core
            //   Attribute: Category Selection
            RockMigrationHelper.DeleteAttribute( "370DB580-B342-48A5-8FED-4C92EF121974" );

            // Attribute for BlockType
            //   BlockType: Prayer Comment List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "5A1D49FC-BA17-45FD-A5E1-D1715294DB37" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Fee List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "A885C578-4EFF-40BE-9F34-ED183FEF6D97" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Fee List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "F858F67A-552A-4D76-BB8B-86C8A124BAB4" );

            // Attribute for BlockType
            //   BlockType: Person Signal List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "F6CBDC1B-B5E6-4611-9A3B-F8229E3C27EA" );

            // Attribute for BlockType
            //   BlockType: Person Signal List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "00E16ED2-BC73-41C4-BA16-471725A23547" );

            // Delete BlockType 
            //   Name: Prayer Request List
            //   Category: Prayer
            //   Path: -
            //   EntityType: Prayer Request List
            RockMigrationHelper.DeleteBlockType( "E860F577-F30D-4197-87F0-C3DC6132F537" );

            // Delete BlockType 
            //   Name: Prayer Comment List
            //   Category: Core
            //   Path: -
            //   EntityType: Prayer Comment List
            RockMigrationHelper.DeleteBlockType( "3F997DA7-AC42-41C9-97F1-2069BB9D9E5C" );

            // Delete BlockType 
            //   Name: Registration Instance - Fee List
            //   Category: Event
            //   Path: -
            //   EntityType: Registration Instance Fee List
            RockMigrationHelper.DeleteBlockType( "DBCFB477-0553-4BAE-BAC9-2AEC38E1DA37" );

            // Delete BlockType 
            //   Name: Person Signal List
            //   Category: Core
            //   Path: -
            //   EntityType: Person Signal List
            RockMigrationHelper.DeleteBlockType( "653052A0-CA1C-41B8-8340-4B13149C6E66" );
        }
    }
}
