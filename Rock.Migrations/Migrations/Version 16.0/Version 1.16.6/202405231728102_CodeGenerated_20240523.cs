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
    public partial class CodeGenerated_20240523 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.SystemCommunicationList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.SystemCommunicationList", "System Communication List", "Rock.Blocks.Communication.SystemCommunicationList, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "6452B97C-2777-44CE-8DCA-72F32D07E500" );

            // Add/Update Obsidian Block Type
            //   Name:System Communication List
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.SystemCommunicationList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "System Communication List", "Lists the system communications that can be configured for use by the system and other automated (non-user) tasks.", "Rock.Blocks.Communication.SystemCommunicationList", "Communication", "411A5AD2-D667-4283-B58D-8A8614B07B0F" );

            // Attribute for BlockType
            //   BlockType: Rapid Attendance Entry
            //   Category: Check-in
            //   Attribute: Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"This setting filters the list of campuses by type that are displayed in the campus drop-down.", 7, @"", "0A9FA714-33B5-47F5-A299-6F8A24B73538" );

            // Attribute for BlockType
            //   BlockType: Rapid Attendance Entry
            //   Category: Check-in
            //   Attribute: Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"This setting filters the list of campuses by statuses that are displayed in the campus drop-down.", 8, @"", "EEBDC3B9-0F4B-4BA6-A39D-592C795E8866" );

            // Attribute for BlockType
            //   BlockType: Attendance Analytics
            //   Category: Check-in
            //   Attribute: Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"This setting filters the list of campuses by type that are displayed in the campus drop-down.", 11, @"", "F54C2DF4-E231-4013-9E3D-EA70F866EF62" );

            // Attribute for BlockType
            //   BlockType: Attendance Analytics
            //   Category: Check-in
            //   Attribute: Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"This setting filters the list of campuses by statuses that are displayed in the campus drop-down.", 12, @"", "1114DAB5-32E8-41F7-BDE4-73EE820B710C" );

            // Attribute for BlockType
            //   BlockType: Public Profile Edit
            //   Category: CMS
            //   Attribute: Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"This setting filters the list of campuses by type that are displayed in the campus drop-down.", 26, @"", "6A6C87EE-7C80-4931-BA8A-7FCBF547E54C" );

            // Attribute for BlockType
            //   BlockType: Public Profile Edit
            //   Category: CMS
            //   Attribute: Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"This setting filters the list of campuses by statuses that are displayed in the campus drop-down.", 27, @"", "87AB0CEF-7D72-4959-9C62-1A4872E89D63" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Search
            //   Category: Connection
            //   Attribute: Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C0D58DEE-D266-4AA8-8750-414A3CC26C07", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"This setting filters the list of campuses by type that are displayed in the campus drop-down.", 6, @"", "8E7D085A-2FFA-4914-A8B1-5A7127EFBDF1" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Search
            //   Category: Connection
            //   Attribute: Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C0D58DEE-D266-4AA8-8750-414A3CC26C07", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"This setting filters the list of campuses by statuses that are displayed in the campus drop-down.", 7, @"", "7D27C9F2-0E67-471E-9903-A59F56C0CB19" );

            // Attribute for BlockType
            //   BlockType: Group Map
            //   Category: Groups
            //   Attribute: Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "967F0D2B-DB76-486A-B034-D22B9D9240D3", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"This setting filters the list of campuses by type that are displayed in the campus drop-down.", 7, @"", "13D361FC-B9D2-4B20-BA3B-F56C737EE35A" );

            // Attribute for BlockType
            //   BlockType: Group Map
            //   Category: Groups
            //   Attribute: Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "967F0D2B-DB76-486A-B034-D22B9D9240D3", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"This setting filters the list of campuses by statuses that are displayed in the campus drop-down.", 8, @"", "13551EB2-9314-4931-BA7F-8BAF70DCD8D2" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Group
            //   Attribute: Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "308DBA32-F656-418E-A019-9D18235027C1", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"This setting filters the list of campuses by type that are displayed in the campus drop-down.", 5, @"", "4D328133-AA80-4AAB-9B99-E92DA4BB4A0E" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Group
            //   Attribute: Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "308DBA32-F656-418E-A019-9D18235027C1", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"This setting filters the list of campuses by statuses that are displayed in the campus drop-down.", 6, @"", "C172F624-192B-4062-B708-B098EB319556" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Security
            //   Attribute: Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"This setting filters the list of campuses by type that are displayed in the campus drop-down.", 24, @"", "D6AF4EBF-B117-4951-ABBB-BE36B4571E33" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Security
            //   Attribute: Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"This setting filters the list of campuses by statuses that are displayed in the campus drop-down.", 25, @"", "93AE5C78-5F04-4C4F-90B3-279D8C0A559A" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Attribute: Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"This setting filters the list of campuses by type that are displayed in the campus drop-down.", 14, @"", "E31A1228-140A-49B3-8260-F65488AFFDC1" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Attribute: Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"This setting filters the list of campuses by statuses that are displayed in the campus drop-down.", 15, @"", "B7985AF5-2393-4FBD-8F91-64F018931FBB" );

            // Attribute for BlockType
            //   BlockType: System Communication List
            //   Category: Communication
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "411A5AD2-D667-4283-B58D-8A8614B07B0F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the system communication details.", 0, @"", "5337FAD1-86B0-41FC-A0A0-0F4E23D3F3D8" );

            // Attribute for BlockType
            //   BlockType: System Communication List
            //   Category: Communication
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "411A5AD2-D667-4283-B58D-8A8614B07B0F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "4335565C-DB20-4F08-8514-38CF280375F3" );

            // Attribute for BlockType
            //   BlockType: System Communication List
            //   Category: Communication
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "411A5AD2-D667-4283-B58D-8A8614B07B0F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "7AF09A06-C2B7-4BDE-90D8-B4F518F6AFEA" );

            // Attribute for BlockType
            //   BlockType: Connection Request Detail
            //   Category: Mobile > Connection
            //   Attribute: Show Transfer Option
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EF537CC9-5E53-4832-A473-0D5EA439C296", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Transfer Option", "ShowTransferOption", "Show Transfer Option", @"Indicates whether or not the 'Transfer' option should be shown.", 6, @"True", "2104E0CD-33AE-47F5-B67F-03A5C278965E" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Connection Request Detail
            //   Category: Mobile > Connection
            //   Attribute: Show Transfer Option
            RockMigrationHelper.DeleteAttribute( "2104E0CD-33AE-47F5-B67F-03A5C278965E" );

            // Attribute for BlockType
            //   BlockType: System Communication List
            //   Category: Communication
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "7AF09A06-C2B7-4BDE-90D8-B4F518F6AFEA" );

            // Attribute for BlockType
            //   BlockType: System Communication List
            //   Category: Communication
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "4335565C-DB20-4F08-8514-38CF280375F3" );

            // Attribute for BlockType
            //   BlockType: System Communication List
            //   Category: Communication
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "5337FAD1-86B0-41FC-A0A0-0F4E23D3F3D8" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Attribute: Campus Statuses
            RockMigrationHelper.DeleteAttribute( "B7985AF5-2393-4FBD-8F91-64F018931FBB" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Attribute: Campus Types
            RockMigrationHelper.DeleteAttribute( "E31A1228-140A-49B3-8260-F65488AFFDC1" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Security
            //   Attribute: Campus Statuses
            RockMigrationHelper.DeleteAttribute( "93AE5C78-5F04-4C4F-90B3-279D8C0A559A" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Security
            //   Attribute: Campus Types
            RockMigrationHelper.DeleteAttribute( "D6AF4EBF-B117-4951-ABBB-BE36B4571E33" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Group
            //   Attribute: Campus Statuses
            RockMigrationHelper.DeleteAttribute( "C172F624-192B-4062-B708-B098EB319556" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Group
            //   Attribute: Campus Types
            RockMigrationHelper.DeleteAttribute( "4D328133-AA80-4AAB-9B99-E92DA4BB4A0E" );

            // Attribute for BlockType
            //   BlockType: Group Map
            //   Category: Groups
            //   Attribute: Campus Statuses
            RockMigrationHelper.DeleteAttribute( "13551EB2-9314-4931-BA7F-8BAF70DCD8D2" );

            // Attribute for BlockType
            //   BlockType: Group Map
            //   Category: Groups
            //   Attribute: Campus Types
            RockMigrationHelper.DeleteAttribute( "13D361FC-B9D2-4B20-BA3B-F56C737EE35A" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Search
            //   Category: Connection
            //   Attribute: Campus Statuses
            RockMigrationHelper.DeleteAttribute( "7D27C9F2-0E67-471E-9903-A59F56C0CB19" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Search
            //   Category: Connection
            //   Attribute: Campus Types
            RockMigrationHelper.DeleteAttribute( "8E7D085A-2FFA-4914-A8B1-5A7127EFBDF1" );

            // Attribute for BlockType
            //   BlockType: Public Profile Edit
            //   Category: CMS
            //   Attribute: Campus Statuses
            RockMigrationHelper.DeleteAttribute( "87AB0CEF-7D72-4959-9C62-1A4872E89D63" );

            // Attribute for BlockType
            //   BlockType: Public Profile Edit
            //   Category: CMS
            //   Attribute: Campus Types
            RockMigrationHelper.DeleteAttribute( "6A6C87EE-7C80-4931-BA8A-7FCBF547E54C" );

            // Attribute for BlockType
            //   BlockType: Attendance Analytics
            //   Category: Check-in
            //   Attribute: Campus Statuses
            RockMigrationHelper.DeleteAttribute( "1114DAB5-32E8-41F7-BDE4-73EE820B710C" );

            // Attribute for BlockType
            //   BlockType: Attendance Analytics
            //   Category: Check-in
            //   Attribute: Campus Types
            RockMigrationHelper.DeleteAttribute( "F54C2DF4-E231-4013-9E3D-EA70F866EF62" );

            // Attribute for BlockType
            //   BlockType: Rapid Attendance Entry
            //   Category: Check-in
            //   Attribute: Campus Statuses
            RockMigrationHelper.DeleteAttribute( "EEBDC3B9-0F4B-4BA6-A39D-592C795E8866" );

            // Attribute for BlockType
            //   BlockType: Rapid Attendance Entry
            //   Category: Check-in
            //   Attribute: Campus Types
            RockMigrationHelper.DeleteAttribute( "0A9FA714-33B5-47F5-A299-6F8A24B73538" );

            // Delete BlockType 
            //   Name: System Communication List
            //   Category: Communication
            //   Path: -
            //   EntityType: System Communication List
            RockMigrationHelper.DeleteBlockType( "411A5AD2-D667-4283-B58D-8A8614B07B0F" );
        }
    }
}
