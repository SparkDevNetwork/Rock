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
    public partial class CodeGenerated_20211019 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
           // Add/Update BlockType 
           //   Name: Security Change Audit List
           //   Category: Security
           //   Path: ~/Blocks/Security/SecurityChangeAuditList.ascx
           //   EntityType: -
            RockMigrationHelper.UpdateBlockType("Security Change Audit List","Block for Security Change Audit List.","~/Blocks/Security/SecurityChangeAuditList.ascx","Security","9F577C39-19FB-4C33-804B-35023284B856");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Location Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAC6671E-4D6F-4428-A6FA-69B8BEADF55C", "48624B0B-6A58-45B8-9E47-B67B67898D25", "Location Types", "LocationTypes", "Location Types", @"The location types available to pick from when searching by location.", 1, @"8C52E53C-2A66-435A-AE6E-5EE307D9A0DC", "7564A0E0-A592-4910-AF8C-0428794DBF0F" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Hide Overcapacity Groups
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAC6671E-4D6F-4428-A6FA-69B8BEADF55C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Overcapacity Groups", "HideOvercapacityGroups", "Hide Overcapacity Groups", @"Hides groups that have already reached their capacity limit.", 2, @"True", "87724BAC-1E57-409D-8885-5318267BE5AF" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Show Results on Initial Page Load
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAC6671E-4D6F-4428-A6FA-69B8BEADF55C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Results on Initial Page Load", "ShowResultsOnInitialPageLoad", "Show Results on Initial Page Load", @"Bypasses the filter and shows results immediately. Can also be set in query string with LoadResults=true.", 3, @"False", "11B3600F-48D8-4064-A6A9-53B696E1C8FB" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Search Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAC6671E-4D6F-4428-A6FA-69B8BEADF55C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Search Header", "SearchHeader", "Search Header", @"The XAML content to display above the search filters.", 4, @"", "EFDD73BB-F31D-43FF-AA05-E93DE3600E17" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAC6671E-4D6F-4428-A6FA-69B8BEADF55C", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The Lava template to use to render the results.", 5, @"CC117DBB-5C3C-4A32-8ABA-88A7493C7F70", "92F92A63-7577-43CD-999F-DC51CA728659" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Max Results
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAC6671E-4D6F-4428-A6FA-69B8BEADF55C", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Results", "MaxResults", "Max Results", @"The maximum number of results to show on the page.", 6, @"25", "0F1D09D7-FA63-4B89-85E9-D4FF914D4C56" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Show Location Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAC6671E-4D6F-4428-A6FA-69B8BEADF55C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Location Filter", "ShowLocationFilter", "Show Location Filter", @"Shows the location search filter and enables ordering results by distance.", 7, @"True", "52567D6D-FE95-455E-BA3C-646E2491389F" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Show Campus Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAC6671E-4D6F-4428-A6FA-69B8BEADF55C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus Filter", "ShowCampusFilter", "Show Campus Filter", @"Shows the campus search filter and enables filtering by campus.", 8, @"True", "6EF61B27-9F15-4F85-BE3D-999CFF1C0491" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAC6671E-4D6F-4428-A6FA-69B8BEADF55C", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"Specifies which campus types will be shown in the campus filter.", 9, @"", "24AB2372-8B62-4A8B-8EEE-09F0259D18D5" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAC6671E-4D6F-4428-A6FA-69B8BEADF55C", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"Specifies which campus statuses will be shown in the campus filter.", 10, @"", "ECFE9764-2BF2-4795-97D7-0F8AEF616FEB" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Show Day of Week Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAC6671E-4D6F-4428-A6FA-69B8BEADF55C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Day of Week Filter", "ShowDayOfWeekFilter", "Show Day of Week Filter", @"Shows the day of week filter and enables filtering to groups that meet on the selected day.", 11, @"True", "A041F92C-E9EA-4D1F-AED8-A39CA1CD04BE" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Show Time Period Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAC6671E-4D6F-4428-A6FA-69B8BEADF55C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Time Period Filter", "ShowTimePeriodFilter", "Show Time Period Filter", @"Shows a filter that enables filtering based on morning, afternoon and evening.", 12, @"True", "B3DBED1D-92A7-442D-9F47-C6DC2BB4CF7C" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Campus Context Enabled
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAC6671E-4D6F-4428-A6FA-69B8BEADF55C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Context Enabled", "CampusContextEnabled", "Campus Context Enabled", @"Automatically sets the campus filter to the current campus context.", 13, @"False", "29253485-695E-418F-881A-C27E942B634E" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAC6671E-4D6F-4428-A6FA-69B8BEADF55C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to link to when selecting a group.", 14, @"", "F321149D-4471-4069-9DCA-683A8AE4F199" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Results Transition
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAC6671E-4D6F-4428-A6FA-69B8BEADF55C", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Results Transition", "ResultsTransition", "Results Transition", @"The transition to use when going from filters to results and back.", 15, @"1", "2344DDD9-8C2E-4C92-B995-EDED842927E1" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Group Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAC6671E-4D6F-4428-A6FA-69B8BEADF55C", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Group Types", "GroupTypes", "Group Types", @"Specifies which group types are included in search results.", 0, @"", "9123F1E9-DC6C-421D-95A2-AF98D1F159FC" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Group Types Location Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAC6671E-4D6F-4428-A6FA-69B8BEADF55C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Types Location Type", "GroupTypesLocationType", "Group Types Location Type", @"The type of location each group type can use for distance calculations.", 0, @"", "B9E4AC96-6547-4E74-8FD4-28F3004F98B1" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Attribute Filters
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BAC6671E-4D6F-4428-A6FA-69B8BEADF55C", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Attribute Filters", "AttributeFilters", "Attribute Filters", @"", 0, @"", "E100682A-E5B7-4BF6-A39B-15EC6D01D6CB" );

            // Attribute for BlockType
            //   BlockType: Group Registration
            //   Category: Mobile > Groups
            //   Attribute: Group Member Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A42E4FA-9FE1-493C-B6D8-7A766D96E912", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Group Member Status", "GroupMemberStatus", "Group Member Status", @"The group member status to use when adding person to the group.", 0, @"2", "FC8CA904-3E5D-4797-8C95-E032E1E6134E" );

            // Attribute for BlockType
            //   BlockType: Group Registration
            //   Category: Mobile > Groups
            //   Attribute: Registration Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A42E4FA-9FE1-493C-B6D8-7A766D96E912", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Registration Workflow", "RegistrationWorkflow", "Registration Workflow", @"An optional workflow to start for each individual being added to the group. The GroupMember will be set as the workflow Entity. The current/primary person will be passed as the workflow initiator.", 2, @"", "7E6C881C-46AD-4692-BCA1-B9C416F5A63A" );

            // Attribute for BlockType
            //   BlockType: Group Registration
            //   Category: Mobile > Groups
            //   Attribute: Family Options
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A42E4FA-9FE1-493C-B6D8-7A766D96E912", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Family Options", "FamilyOptions", "Family Options", @"Provides additional inputs to register additional members of the family.", 3, @"0", "C5DFE24D-62EC-46E5-A23F-2789EA95A450" );

            // Attribute for BlockType
            //   BlockType: Group Registration
            //   Category: Mobile > Groups
            //   Attribute: Group
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A42E4FA-9FE1-493C-B6D8-7A766D96E912", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Group", "Group", "Group", @"An optional group to add person to. If omitted, the group's Guid should be passed via the Query String (GroupGuid=).", 6, @"", "3E1413C0-6FC1-4963-92C9-5AD8D9C6FC33" );

            // Attribute for BlockType
            //   BlockType: Group Registration
            //   Category: Mobile > Groups
            //   Attribute: Result Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A42E4FA-9FE1-493C-B6D8-7A766D96E912", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Result Page", "ResultPage", "Result Page", @"An optional page to redirect user to after they have been registered for the group. GroupGuid will be passed in the query string.", 9, @"", "466B5D46-E718-45BE-AC54-13F48E50479A" );

            // Attribute for BlockType
            //   BlockType: Group Registration
            //   Category: Mobile > Groups
            //   Attribute: Prevent Overcapacity Registrations
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A42E4FA-9FE1-493C-B6D8-7A766D96E912", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Prevent Overcapacity Registrations", "PreventOvercapacityRegistrations", "Prevent Overcapacity Registrations", @"When set to true, user cannot register for groups that are at capacity or whose default GroupTypeRole are at capacity. If only one spot is available, no family members can be registered.", 11, @"False", "3190D83F-6EAE-4B77-B2C1-0B0FAD5EC846" );

            // Attribute for BlockType
            //   BlockType: Group Registration
            //   Category: Mobile > Groups
            //   Attribute: Autofill Form
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A42E4FA-9FE1-493C-B6D8-7A766D96E912", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Autofill Form", "AutofillForm", "Autofill Form", @"If set to false then the form will not load the context of the logged in user.", 12, @"True", "E75E6654-0A4F-4833-8051-A6E96A36A784" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
            // Attribute for BlockType
            //   BlockType: Group Registration
            //   Category: Mobile > Groups
            //   Attribute: Autofill Form
            RockMigrationHelper.DeleteAttribute("E75E6654-0A4F-4833-8051-A6E96A36A784");

            // Attribute for BlockType
            //   BlockType: Group Registration
            //   Category: Mobile > Groups
            //   Attribute: Prevent Overcapacity Registrations
            RockMigrationHelper.DeleteAttribute("3190D83F-6EAE-4B77-B2C1-0B0FAD5EC846");

            // Attribute for BlockType
            //   BlockType: Group Registration
            //   Category: Mobile > Groups
            //   Attribute: Result Page
            RockMigrationHelper.DeleteAttribute("466B5D46-E718-45BE-AC54-13F48E50479A");

            // Attribute for BlockType
            //   BlockType: Group Registration
            //   Category: Mobile > Groups
            //   Attribute: Group
            RockMigrationHelper.DeleteAttribute("3E1413C0-6FC1-4963-92C9-5AD8D9C6FC33");

            // Attribute for BlockType
            //   BlockType: Group Registration
            //   Category: Mobile > Groups
            //   Attribute: Family Options
            RockMigrationHelper.DeleteAttribute("C5DFE24D-62EC-46E5-A23F-2789EA95A450");

            // Attribute for BlockType
            //   BlockType: Group Registration
            //   Category: Mobile > Groups
            //   Attribute: Registration Workflow
            RockMigrationHelper.DeleteAttribute("7E6C881C-46AD-4692-BCA1-B9C416F5A63A");

            // Attribute for BlockType
            //   BlockType: Group Registration
            //   Category: Mobile > Groups
            //   Attribute: Group Member Status
            RockMigrationHelper.DeleteAttribute("FC8CA904-3E5D-4797-8C95-E032E1E6134E");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Attribute Filters
            RockMigrationHelper.DeleteAttribute("E100682A-E5B7-4BF6-A39B-15EC6D01D6CB");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Group Types Location Type
            RockMigrationHelper.DeleteAttribute("B9E4AC96-6547-4E74-8FD4-28F3004F98B1");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Group Types
            RockMigrationHelper.DeleteAttribute("9123F1E9-DC6C-421D-95A2-AF98D1F159FC");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Results Transition
            RockMigrationHelper.DeleteAttribute("2344DDD9-8C2E-4C92-B995-EDED842927E1");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute("F321149D-4471-4069-9DCA-683A8AE4F199");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Campus Context Enabled
            RockMigrationHelper.DeleteAttribute("29253485-695E-418F-881A-C27E942B634E");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Show Time Period Filter
            RockMigrationHelper.DeleteAttribute("B3DBED1D-92A7-442D-9F47-C6DC2BB4CF7C");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Show Day of Week Filter
            RockMigrationHelper.DeleteAttribute("A041F92C-E9EA-4D1F-AED8-A39CA1CD04BE");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Campus Statuses
            RockMigrationHelper.DeleteAttribute("ECFE9764-2BF2-4795-97D7-0F8AEF616FEB");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Campus Types
            RockMigrationHelper.DeleteAttribute("24AB2372-8B62-4A8B-8EEE-09F0259D18D5");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Show Campus Filter
            RockMigrationHelper.DeleteAttribute("6EF61B27-9F15-4F85-BE3D-999CFF1C0491");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Show Location Filter
            RockMigrationHelper.DeleteAttribute("52567D6D-FE95-455E-BA3C-646E2491389F");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Max Results
            RockMigrationHelper.DeleteAttribute("0F1D09D7-FA63-4B89-85E9-D4FF914D4C56");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Template
            RockMigrationHelper.DeleteAttribute("92F92A63-7577-43CD-999F-DC51CA728659");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Search Header
            RockMigrationHelper.DeleteAttribute("EFDD73BB-F31D-43FF-AA05-E93DE3600E17");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Show Results on Initial Page Load
            RockMigrationHelper.DeleteAttribute("11B3600F-48D8-4064-A6A9-53B696E1C8FB");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Hide Overcapacity Groups
            RockMigrationHelper.DeleteAttribute("87724BAC-1E57-409D-8885-5318267BE5AF");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Mobile > Groups
            //   Attribute: Location Types
            RockMigrationHelper.DeleteAttribute("7564A0E0-A592-4910-AF8C-0428794DBF0F");

            // Delete BlockType 
            //   Name: Security Change Audit List
            //   Category: Security
            //   Path: ~/Blocks/Security/SecurityChangeAuditList.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType("9F577C39-19FB-4C33-804B-35023284B856");

            // Delete BlockType 
            //   Name: Field Type Gallery
            //   Category: Obsidian > Example
            //   Path: -
            //   EntityType: Field Type Gallery
            RockMigrationHelper.DeleteBlockType("50B7B326-8212-44E6-8CF6-515B1FF75A19");

            // Delete BlockType 
            //   Name: Control Gallery
            //   Category: Obsidian > Example
            //   Path: -
            //   EntityType: Control Gallery
            RockMigrationHelper.DeleteBlockType("6FAB07FF-D4C6-412B-B13F-7B881ECBFAD0");

            // Delete BlockType 
            //   Name: Widget List
            //   Category: Rock Solid Church Demo > Page Debug
            //   Path: -
            //   EntityType: Widgets List
            RockMigrationHelper.DeleteBlockType("430EAAFC-A574-4693-9608-78764B4C9B4F");

            // Delete BlockType 
            //   Name: Context Group
            //   Category: Rock Solid Church Demo > Page Debug
            //   Path: -
            //   EntityType: Context Group
            RockMigrationHelper.DeleteBlockType("439D277C-1FB9-4B12-B53A-0E88777E045B");

            // Delete BlockType 
            //   Name: Context Entities
            //   Category: Rock Solid Church Demo > Page Debug
            //   Path: -
            //   EntityType: Context Entities
            RockMigrationHelper.DeleteBlockType("529FCBFC-099C-4DB0-82CC-F739D5D19836");
        }
    }
}
