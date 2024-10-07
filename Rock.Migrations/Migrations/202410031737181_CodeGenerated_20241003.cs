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
    public partial class CodeGenerated_20241003 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.ThemeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.ThemeDetail", "Theme Detail", "Rock.Blocks.Cms.ThemeDetail, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "D4BFE2A3-B5FA-45CB-9C53-A6BEA98ECDDA" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.LogViewer
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.LogViewer", "Log Viewer", "Rock.Blocks.Core.LogViewer, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "DB6A13D0-964D-4839-9E32-BF1E522D176A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.ScheduleCategoryExclusionList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.ScheduleCategoryExclusionList", "Schedule Category Exclusion List", "Rock.Blocks.Core.ScheduleCategoryExclusionList, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "C08129E7-D22A-4213-8703-0F0C1511EBDD" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Event.RegistrationInstanceList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Event.RegistrationInstanceList", "Registration Instance List", "Rock.Blocks.Event.RegistrationInstanceList, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "F9BDE297-09CD-456B-BFE3-31FE9EB28D5B" );

            // Add/Update Obsidian Block Entity Type				 
            //   EntityType:Rock.Blocks.Tv.RokuApplicationDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Tv.RokuApplicationDetail", "Roku Application Detail", "Rock.Blocks.Tv.RokuApplicationDetail, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "89843E83-ADDB-4140-AA54-926ADCCD5558" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Tv.RokuPageDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Tv.RokuPageDetail", "Roku Page Detail", "Rock.Blocks.Tv.RokuPageDetail, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "DDD1ACC4-7FC4-42C8-B66D-64346C026FD1" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Tv.TvApplicationList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Tv.TvApplicationList", "Tv Application List", "Rock.Blocks.Tv.TvApplicationList, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "869B2D70-4AE6-40A0-8899-A3EB9EDFB3B3" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Tv.TvPageList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Tv.TvPageList", "Tv Page List", "Rock.Blocks.Tv.TvPageList, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "BFE024A8-BDF2-4F11-8266-8AE4F4EA483B" );

            // Add/Update Obsidian Block Type
            //   Name:Theme Detail
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.ThemeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Theme Detail", "Displays the details of a particular theme.", "Rock.Blocks.Cms.ThemeDetail", "CMS", "4BD81377-E3C2-48C8-8BBE-20D2BE915446" );

            // Add/Update Obsidian Block Type
            //   Name:Logs
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.LogViewer
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Logs", "Block to view system logs.", "Rock.Blocks.Core.LogViewer", "Core", "E35992D6-C175-4C35-9DA6-A9A7115E1FFD" );

            // Add/Update Obsidian Block Type
            //   Name:Schedule Category Exclusion List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.ScheduleCategoryExclusionList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Schedule Category Exclusion List", "List of dates that schedules are not active for an entire category.", "Rock.Blocks.Core.ScheduleCategoryExclusionList", "Core", "6BC7DA76-1A19-4685-B50A-DFD7EAA5CE33" );

            // Add/Update Obsidian Block Type
            //   Name:Registration Instance List
            //   Category:Event
            //   EntityType:Rock.Blocks.Event.RegistrationInstanceList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Registration Instance List", "Lists all the instances of the given registration template.", "Rock.Blocks.Event.RegistrationInstanceList", "Event", "3A56FE6A-F216-4EF3-9059-ACC1F5906428" );

            // Add/Update Obsidian Block Type
            //   Name:Roku Application Detail
            //   Category:TV > TV Apps
            //   EntityType:Rock.Blocks.Tv.RokuApplicationDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Roku Application Detail", "Displays the details of a Roku application.", "Rock.Blocks.Tv.RokuApplicationDetail", "TV > TV Apps", "261903DF-8632-456B-8272-4E4FFF07147A" );

            // Add/Update Obsidian Block Type
            //   Name:Roku TV Page Detail
            //   Category:TV > TV Apps
            //   EntityType:Rock.Blocks.Tv.RokuPageDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Roku TV Page Detail", "Displays the details of a particular page.", "Rock.Blocks.Tv.RokuPageDetail", "TV > TV Apps", "97C8A25D-8CB3-4662-8371-A37CC28B6F36" );

            // Add/Update Obsidian Block Type
            //   Name:TV Application List
            //   Category:TV > TV Apps
            //   EntityType:Rock.Blocks.Tv.TvApplicationList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "TV Application List", "Displays a list of TV applications.", "Rock.Blocks.Tv.TvApplicationList", "TV > TV Apps", "5DA60F71-DD30-4333-9863-1CCFCE241CDF" );

            // Add/Update Obsidian Block Type
            //   Name:TV Page List
            //   Category:TV > TV Apps
            //   EntityType:Rock.Blocks.Tv.TvPageList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "TV Page List", "Displays a list of pages.", "Rock.Blocks.Tv.TvPageList", "TV > TV Apps", "11616362-6F7F-4B98-BC2A-DFD18AB983D9" );

            // Attribute for BlockType						  
            //   BlockType: Theme List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FD99E0AA-E1CB-4049-A6F6-9C5F2A34F694", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page to use for editing next-gen themes.", 0, @"", "6A6BD7AC-7A7F-4A79-B39D-1A579D807124" );

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

            // Attribute for BlockType
            //   BlockType: Schedule Category Exclusion List
            //   Category: Core
            //   Attribute: Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6BC7DA76-1A19-4685-B50A-DFD7EAA5CE33", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Category", "Category", "Category", @"Optional Category to use (if not specified, query will be determined by query string).", 0, @"", "0E7C9636-EFD8-4291-9E2E-E04956C78D78" );

            // Attribute for BlockType
            //   BlockType: Schedule Category Exclusion List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6BC7DA76-1A19-4685-B50A-DFD7EAA5CE33", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "E0598054-8D1D-44CD-A0C4-55FCD45887B9" );

            // Attribute for BlockType
            //   BlockType: Schedule Category Exclusion List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6BC7DA76-1A19-4685-B50A-DFD7EAA5CE33", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "3F1FB398-7962-41C9-87BA-F9994FE5E854" );

            // Attribute for BlockType
            //   BlockType: Registration Instance List
            //   Category: Event
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3A56FE6A-F216-4EF3-9059-ACC1F5906428", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the registration instance details.", 0, @"", "E40FDAC1-0B56-4E96-B675-372FC995DB60" );

            // Attribute for BlockType
            //   BlockType: Registration Instance List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3A56FE6A-F216-4EF3-9059-ACC1F5906428", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "D0AB742E-D996-4645-9E3B-4AC8FDB85C09" );

            // Attribute for BlockType
            //   BlockType: Registration Instance List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3A56FE6A-F216-4EF3-9059-ACC1F5906428", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "40130CB8-92E0-401C-95F8-7EE151F7FC0B" );

            // Attribute for BlockType
            //   BlockType: TV Application List
            //   Category: TV > TV Apps
            //   Attribute: Apple TV Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5DA60F71-DD30-4333-9863-1CCFCE241CDF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Apple TV Detail Page", "AppleTvDetailPage", "Apple TV Detail Page", @"The page that will show the site details for an Apple TV application.", 0, @"3D874455-7FE1-407B-A817-B0F82A51CEB8", "2F113D7E-11A6-4CA0-AB23-6FD9D60C662C" );

            // Attribute for BlockType
            //   BlockType: TV Application List
            //   Category: TV > TV Apps
            //   Attribute: Roku Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5DA60F71-DD30-4333-9863-1CCFCE241CDF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Roku Detail Page", "RokuDetailPage", "Roku Detail Page", @"The page that will show the site details for a Roku application.", 1, @"867EC436-7F72-4108-81B6-ADBCFFC3918A", "76439C14-1682-47AB-A1F1-E7D29872568B" );

            // Attribute for BlockType
            //   BlockType: TV Application List
            //   Category: TV > TV Apps
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5DA60F71-DD30-4333-9863-1CCFCE241CDF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "E0C002DD-4E19-472C-8492-AE2CBB05A16A" );

            // Attribute for BlockType
            //   BlockType: TV Application List
            //   Category: TV > TV Apps
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5DA60F71-DD30-4333-9863-1CCFCE241CDF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "B99102F5-5227-413F-AEBA-4E6E4778AF15" );

            // Attribute for BlockType
            //   BlockType: TV Page List
            //   Category: TV > TV Apps
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "11616362-6F7F-4B98-BC2A-DFD18AB983D9", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the page details.", 0, @"", "66296944-62EA-4E9F-9FF3-2F4570257AF3" );

            // Attribute for BlockType
            //   BlockType: TV Page List
            //   Category: TV > TV Apps
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "11616362-6F7F-4B98-BC2A-DFD18AB983D9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "9563986E-8F31-4873-975E-EB9A04A041AB" );

            // Attribute for BlockType
            //   BlockType: TV Page List
            //   Category: TV > TV Apps
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "11616362-6F7F-4B98-BC2A-DFD18AB983D9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "50E535B1-B2A2-4B24-B6E0-21EA5C21EAB4" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: TV Page List
            //   Category: TV > TV Apps
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "50E535B1-B2A2-4B24-B6E0-21EA5C21EAB4" );

            // Attribute for BlockType
            //   BlockType: TV Page List
            //   Category: TV > TV Apps
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "9563986E-8F31-4873-975E-EB9A04A041AB" );

            // Attribute for BlockType
            //   BlockType: TV Page List
            //   Category: TV > TV Apps
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "66296944-62EA-4E9F-9FF3-2F4570257AF3" );

            // Attribute for BlockType
            //   BlockType: TV Application List
            //   Category: TV > TV Apps
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "B99102F5-5227-413F-AEBA-4E6E4778AF15" );

            // Attribute for BlockType
            //   BlockType: TV Application List
            //   Category: TV > TV Apps
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "E0C002DD-4E19-472C-8492-AE2CBB05A16A" );

            // Attribute for BlockType
            //   BlockType: TV Application List
            //   Category: TV > TV Apps
            //   Attribute: Roku Detail Page
            RockMigrationHelper.DeleteAttribute( "76439C14-1682-47AB-A1F1-E7D29872568B" );

            // Attribute for BlockType
            //   BlockType: TV Application List
            //   Category: TV > TV Apps
            //   Attribute: Apple TV Detail Page
            RockMigrationHelper.DeleteAttribute( "2F113D7E-11A6-4CA0-AB23-6FD9D60C662C" );

            // Attribute for BlockType
            //   BlockType: Registration Instance List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "40130CB8-92E0-401C-95F8-7EE151F7FC0B" );

            // Attribute for BlockType
            //   BlockType: Registration Instance List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "D0AB742E-D996-4645-9E3B-4AC8FDB85C09" );

            // Attribute for BlockType
            //   BlockType: Registration Instance List
            //   Category: Event
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "E40FDAC1-0B56-4E96-B675-372FC995DB60" );

            // Attribute for BlockType
            //   BlockType: Schedule Category Exclusion List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "3F1FB398-7962-41C9-87BA-F9994FE5E854" );

            // Attribute for BlockType
            //   BlockType: Schedule Category Exclusion List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "E0598054-8D1D-44CD-A0C4-55FCD45887B9" );

            // Attribute for BlockType
            //   BlockType: Schedule Category Exclusion List
            //   Category: Core
            //   Attribute: Category
            RockMigrationHelper.DeleteAttribute( "0E7C9636-EFD8-4291-9E2E-E04956C78D78" );

            // Attribute for BlockType
            //   BlockType: Logs
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "18472384-59A7-4EBF-B507-D491285018D6" );

            // Attribute for BlockType
            //   BlockType: Logs
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "36BE6E4B-8843-44A4-90AC-675508664FF9" );

            // Attribute for BlockType
            //   BlockType: Theme List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "6A6BD7AC-7A7F-4A79-B39D-1A579D807124" );

            // Delete BlockType 
            //   Name: TV Page List
            //   Category: TV > TV Apps
            //   Path: -
            //   EntityType: Tv Page List
            RockMigrationHelper.DeleteBlockType( "11616362-6F7F-4B98-BC2A-DFD18AB983D9" );

            // Delete BlockType 
            //   Name: TV Application List
            //   Category: TV > TV Apps
            //   Path: -
            //   EntityType: Tv Application List
            RockMigrationHelper.DeleteBlockType( "5DA60F71-DD30-4333-9863-1CCFCE241CDF" );

            // Delete BlockType 
            //   Name: Roku TV Page Detail
            //   Category: TV > TV Apps
            //   Path: -
            //   EntityType: Roku Page Detail
            RockMigrationHelper.DeleteBlockType( "97C8A25D-8CB3-4662-8371-A37CC28B6F36" );

            // Delete BlockType 
            //   Name: Roku Application Detail
            //   Category: TV > TV Apps
            //   Path: -
            //   EntityType: Roku Application Detail
            RockMigrationHelper.DeleteBlockType( "261903DF-8632-456B-8272-4E4FFF07147A" );

            // Delete BlockType 
            //   Name: Registration Instance List
            //   Category: Event
            //   Path: -
            //   EntityType: Registration Instance List
            RockMigrationHelper.DeleteBlockType( "3A56FE6A-F216-4EF3-9059-ACC1F5906428" );

            // Delete BlockType 
            //   Name: Schedule Category Exclusion List
            //   Category: Core
            //   Path: -
            //   EntityType: Schedule Category Exclusion List
            RockMigrationHelper.DeleteBlockType( "6BC7DA76-1A19-4685-B50A-DFD7EAA5CE33" );

            // Delete BlockType 
            //   Name: Logs
            //   Category: Core
            //   Path: -
            //   EntityType: Log Viewer
            RockMigrationHelper.DeleteBlockType( "E35992D6-C175-4C35-9DA6-A9A7115E1FFD" );

            // Delete BlockType 
            //   Name: Theme Detail
            //   Category: CMS
            //   Path: -
            //   EntityType: Theme Detail
            RockMigrationHelper.DeleteBlockType( "4BD81377-E3C2-48C8-8BBE-20D2BE915446" );
        }
    }
}
