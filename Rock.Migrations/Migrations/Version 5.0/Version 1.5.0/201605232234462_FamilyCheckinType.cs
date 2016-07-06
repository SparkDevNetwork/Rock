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
    public partial class FamilyCheckinType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.BinaryFile", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "BinaryFileTypeId", "1", "Print For Each", "When a family checks in, should this label be printed once per family, person, or location. Note: this only applys if check-in is configured to use Family check-in vs Individual check-in.", 1, "1", "733944B7-A0D5-41B4-94D4-DE007F72B6F0", "core_LabelType" );
            RockMigrationHelper.UpdateAttributeQualifier( "733944B7-A0D5-41B4-94D4-DE007F72B6F0", "fieldtype", "ddl", "D9313D93-A2AB-4FE9-B743-A376DEB3EE31" );
            RockMigrationHelper.UpdateAttributeQualifier( "733944B7-A0D5-41B4-94D4-DE007F72B6F0", "values", "0^Family,1^Person,2^Location", "E77DF4E6-A995-4C82-BBB7-DB57739D66F3" );

            RockMigrationHelper.AddPage( "CDF2C599-D341-42FD-B7DC-CD402EA96050", "66FA0143-F04C-4447-A67A-2A10A6BB1A2B", "Person Select (Family Check-in)", "", "D14154BA-2F2C-41C3-B380-F833252CBB13", "" ); // Site:Rock Check-in
            RockMigrationHelper.AddPage( "CDF2C599-D341-42FD-B7DC-CD402EA96050", "66FA0143-F04C-4447-A67A-2A10A6BB1A2B", "Time Select (Family Check-in)", "", "67BD09B0-0C6E-44E7-A8EB-0E71551F3E6B", "" ); // Site:Rock Check-in
            RockMigrationHelper.AddPage( "CDF2C599-D341-42FD-B7DC-CD402EA96050", "66FA0143-F04C-4447-A67A-2A10A6BB1A2B", "Save Attendance (Family Check-in)", "", "4AF7A0E1-E991-4AE5-A2B5-C440F67A2E6A", "" ); // Site:Rock Check-in

            RockMigrationHelper.AddPageRoute( "7B7207D0-B905-4836-800E-A24DDC6FE445", "checkin/{KioskId}/{CheckinConfigId}/{GroupTypeIds}" );
            RockMigrationHelper.AddPageRoute( "D14154BA-2F2C-41C3-B380-F833252CBB13", "checkin/people", "FC4747A0-E42B-4F5D-98A4-B94D393461AD" );// for Page:Person Select (Family Check-in)

            RockMigrationHelper.UpdateBlockType( "Person Select (Family Check-in)", "Lists people who match the selected family and provides option of selecting multiple.", "~/Blocks/CheckIn/MultiPersonSelect.ascx", "Check-in", "92DCF018-F551-4890-8BA1-511D97BF6B8A" );
            RockMigrationHelper.UpdateBlockType( "Process Only", "Provides a page for simply launching a check-in workflow action", "~/Blocks/CheckIn/ProcessOnly.ascx", "Check-in", "F7B86942-9BF2-4132-B5EB-C7310952ECFF" );

            // Add Block to Page: Person Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlock( "D14154BA-2F2C-41C3-B380-F833252CBB13", "", "92DCF018-F551-4890-8BA1-511D97BF6B8A", "Person Select", "Main", "", "", 0, "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5" );
            // Add Block to Page: Time Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlock( "67BD09B0-0C6E-44E7-A8EB-0E71551F3E6B", "", "D2348D51-B13A-4069-97AD-369D9615A711", "Time Select", "Main", "", "", 0, "558C15C1-47F7-4232-A069-89463B17924F" );
            // Add Block to Page: Save Attendance (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlock( "4AF7A0E1-E991-4AE5-A2B5-C440F67A2E6A", "", "F7B86942-9BF2-4132-B5EB-C7310952ECFF", "Process Only", "Main", "", "", 0, "887E090F-5468-44F0-BE84-9AE21D822987" );
            // Add Block to Page: Person Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlock( "D14154BA-2F2C-41C3-B380-F833252CBB13", "", "49FC4B38-741E-4B0B-B395-7C1929340D88", "Idle Redirect", "Main", "", "", 1, "619DEBA2-B84A-48CF-95A4-BEEA047E830C" );
            // Add Block to Page: Time Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlock( "67BD09B0-0C6E-44E7-A8EB-0E71551F3E6B", "", "49FC4B38-741E-4B0B-B395-7C1929340D88", "Idle Redirect", "Main", "", "", 1, "111609E5-EBCC-4B1A-992A-5B61A57327C1" );
            // Add Block to Page: Save Attendance (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlock( "4AF7A0E1-E991-4AE5-A2B5-C440F67A2E6A", "", "49FC4B38-741E-4B0B-B395-7C1929340D88", "Idle Redirect", "Main", "", "", 1, "ACA84721-3048-4953-A3DB-34DAA598357C" );

            // Attrib for BlockType: Administration:Enable Kiosk Match By Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "3B5FBE9A-2904-4220-92F3-47DD16E805C0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Kiosk Match By Name", "EnableReverseLookup", "", "Enable a kiosk match by computer name by doing reverseIP lookup to get computer name based on IP address", 8, @"False", "0E252443-86E1-4068-8B32-9943E0974C94" );

            // Attrib for BlockType: Family Select:Next Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page (Family Check-in)", "FamilyNextPage", "", "", 5, @"", "1C9DAB1C-C2B3-421D-BB53-554C28F33B27" );

            // Attrib for BlockType: Person Select (Family Check-in):Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "92DCF018-F551-4890-8BA1-511D97BF6B8A", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "", "The workflow type to activate for check-in", 0, @"", "C7ABDF19-09B1-4426-B409-CAA1BBB13A11" );
            // Attrib for BlockType: Person Select (Family Check-in):Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute( "92DCF018-F551-4890-8BA1-511D97BF6B8A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the workflow activity to run on selection.", 1, @"", "C1F2BA5A-5FA3-459E-A7C4-965BBCDF501D" );
            // Attrib for BlockType: Person Select (Family Check-in):Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "92DCF018-F551-4890-8BA1-511D97BF6B8A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 2, @"", "9E1DD40E-62F1-40BA-8BFD-2D2D4D8F00C6" );
            // Attrib for BlockType: Person Select (Family Check-in):Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "92DCF018-F551-4890-8BA1-511D97BF6B8A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 3, @"", "E9C55730-B5C4-4640-B8A0-86F10F9F1EE7" );
            // Attrib for BlockType: Person Select (Family Check-in):Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "92DCF018-F551-4890-8BA1-511D97BF6B8A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 4, @"", "DF2EC9E1-2499-4439-B926-A82CE950837D" );

            // Attrib for BlockType: Ability Level Select:Repeat Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Repeat Page (Family Check-in)", "FamilyRepeatPage", "", "The page to navigate back to if there are peopl or schedules already processed.", 3, @"", "B7A328EC-69EE-4472-8D5D-F6B5BEF271EE" );
            // Attrib for BlockType: Ability Level Select:Previous Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page (Family Check-in)", "FamilyPreviousPage", "", "The page to navigate back to if none of the people and schedules have been processed.", 4, @"", "3D24A4D2-90AF-4FDD-8CE2-7D1F9B76104B" );

            // Attrib for BlockType: Group Type Select:Select All and Skip
            RockMigrationHelper.UpdateBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Select All and Skip", "SelectAll", "", "Select this option if end-user should never see screen to select group types, all group types will automatically be selected and all the groups in all types will be available.", 5, @"False", "41AFF704-87A8-4282-80D0-B7C40983B549" );

            // Attrib for BlockType: Location Select:Repeat Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Repeat Page (Family Check-in)", "FamilyRepeatPage", "", "The page to navigate to if there are still more people or schedules to process.", 5, @"", "FDD6D619-028F-484C-A5FF-CB9B16D596CA" );
            // Attrib for BlockType: Location Select:Next Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page (Family Check-in)", "FamilyNextPage", "", "The page to navigate to if all people and schedules have been processed.", 6, @"", "BA3A2508-4AEF-44B6-895C-899D5AB2E378" );

            // Attrib for BlockType: Location Select:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7B86942-9BF2-4132-B5EB-C7310952ECFF", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "", "The workflow type to activate for check-in", 0, @"", "BA9AD11A-DB90-4BF6-ACDA-6FFB56C0358A" );
            // Attrib for BlockType: Process Only:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7B86942-9BF2-4132-B5EB-C7310952ECFF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the workflow activity to run on selection.", 1, @"", "21715BAA-59CE-41F7-8D7B-925C8DB4F3DD" );
            // Attrib for BlockType: Process Only:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7B86942-9BF2-4132-B5EB-C7310952ECFF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 2, @"", "44646AA4-0E73-4AE2-B456-B2F7E9C96BAE" );
            // Attrib for BlockType: Process Only:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7B86942-9BF2-4132-B5EB-C7310952ECFF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 3, @"", "0320C949-F06B-49FF-A9E1-F686CB14841C" );
            // Attrib for BlockType: Process Only:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7B86942-9BF2-4132-B5EB-C7310952ECFF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 4, @"", "BA7AB351-CF98-4846-90C2-62F5EE8D799C" );

            // Attrib for BlockType: Success:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute( "18911F1B-294E-48D6-9E6B-0F72BF6C9491", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the workflow activity to run on selection.", 1, @"", "185504E0-2E80-434D-8E95-EEA3D38D92F0" );

            // Attrib Value for Block:Family Select, Attribute:Next Page (Family Check-in) Page: Family Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "CD97D61E-7BCE-436B-ACDD-4383EB7490BA", "1C9DAB1C-C2B3-421D-BB53-554C28F33B27", @"d14154ba-2f2c-41c3-b380-f833252cbb13,fc4747a0-e42b-4f5d-98a4-b94d393461ad" );

            // Attrib Value for Block:Person Select, Attribute:Workflow Type Page: Person Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "C7ABDF19-09B1-4426-B409-CAA1BBB13A11", @"011e9f5a-60d4-4ff5-912a-290881e37eaf" );
            // Attrib Value for Block:Person Select, Attribute:Workflow Activity Page: Person Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "C1F2BA5A-5FA3-459E-A7C4-965BBCDF501D", @"Load Schedules" );
            // Attrib Value for Block:Person Select, Attribute:Home Page Page: Person Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "9E1DD40E-62F1-40BA-8BFD-2D2D4D8F00C6", @"432b615a-75ff-4b14-9c99-3e769f866950" );
            // Attrib Value for Block:Person Select, Attribute:Previous Page Page: Person Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "E9C55730-B5C4-4640-B8A0-86F10F9F1EE7", @"10c97379-f719-4acb-b8c6-651957b660a4" );
            // Attrib Value for Block:Person Select, Attribute:Next Page Page: Person Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "DF2EC9E1-2499-4439-B926-A82CE950837D", @"67bd09b0-0c6e-44e7-a8eb-0e71551f3e6b" );

            // Attrib Value for Block:Ability Level Select, Attribute:Repeat Page (Family Check-in) Page: Ability Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "C175A9ED-612E-4B25-BED4-CF713D922179", "B7A328EC-69EE-4472-8D5D-F6B5BEF271EE", @"043bb717-5799-446f-b8da-30e575110b0c" );
            // Attrib Value for Block:Ability Level Select, Attribute:Previous Page (Family Check-in) Page: Ability Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "C175A9ED-612E-4B25-BED4-CF713D922179", "3D24A4D2-90AF-4FDD-8CE2-7D1F9B76104B", @"d14154ba-2f2c-41c3-b380-f833252cbb13" );

            // Attrib Value for Block:Location Select, Attribute:Repeat Page (Family Check-in) Page: Location Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "FDD6D619-028F-484C-A5FF-CB9B16D596CA", @"a1cbdaa4-94dd-4156-8260-5a3781e39fd0" );
            // Attrib Value for Block:Location Select, Attribute:Next Page (Family Check-in) Page: Location Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "BA3A2508-4AEF-44B6-895C-899D5AB2E378", @"4af7a0e1-e991-4ae5-a2b5-c440f67a2e6a" );

            // Attrib Value for Block:Process Only, Attribute:Workflow Type Page: Save Attendance (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "887E090F-5468-44F0-BE84-9AE21D822987", "BA9AD11A-DB90-4BF6-ACDA-6FFB56C0358A", @"011e9f5a-60d4-4ff5-912a-290881e37eaf" );
            // Attrib Value for Block:Process Only, Attribute:Workflow Activity Page: Save Attendance (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "887E090F-5468-44F0-BE84-9AE21D822987", "21715BAA-59CE-41F7-8D7B-925C8DB4F3DD", @"Save Attendance" );
            // Attrib Value for Block:Process Only, Attribute:Home Page Page: Save Attendance (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "887E090F-5468-44F0-BE84-9AE21D822987", "44646AA4-0E73-4AE2-B456-B2F7E9C96BAE", @"432b615a-75ff-4b14-9c99-3e769f866950" );
            // Attrib Value for Block:Process Only, Attribute:Previous Page Page: Save Attendance (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "887E090F-5468-44F0-BE84-9AE21D822987", "0320C949-F06B-49FF-A9E1-F686CB14841C", @"043bb717-5799-446f-b8da-30e575110b0c" );
            // Attrib Value for Block:Process Only, Attribute:Next Page Page: Save Attendance (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "887E090F-5468-44F0-BE84-9AE21D822987", "BA7AB351-CF98-4846-90C2-62F5EE8D799C", @"e08230b8-35a4-40d6-a0bb-521418314da9" );

            // Attrib Value for Block:Time Select, Attribute:Workflow Type Page: Time Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "558C15C1-47F7-4232-A069-89463B17924F", "108E2E9E-DC18-4D5D-80FA-5D4A90FFCE65", @"011e9f5a-60d4-4ff5-912a-290881e37eaf" );
            // Attrib Value for Block:Time Select, Attribute:Workflow Activity Page: Time Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "558C15C1-47F7-4232-A069-89463B17924F", "12DF930E-6460-4A66-9326-E39BEAFC6F9D", @"Schedule Select" );
            // Attrib Value for Block:Time Select, Attribute:Home Page Page: Time Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "558C15C1-47F7-4232-A069-89463B17924F", "D5AFB471-3EE2-44D5-BC66-F4EFD26FD394", @"432b615a-75ff-4b14-9c99-3e769f866950" );
            // Attrib Value for Block:Time Select, Attribute:Previous Page Page: Time Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "558C15C1-47F7-4232-A069-89463B17924F", "DE808D50-0861-4E24-A483-F1C74C1FFDE8", @"d14154ba-2f2c-41c3-b380-f833252cbb13" );
            // Attrib Value for Block:Time Select, Attribute:Next Page Page: Time Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "558C15C1-47F7-4232-A069-89463B17924F", "840898DB-A9AB-45C9-9894-0A1E816EFC4C", @"a1cbdaa4-94dd-4156-8260-5a3781e39fd0" );

            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Person Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "619DEBA2-B84A-48CF-95A4-BEEA047E830C", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Person Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "619DEBA2-B84A-48CF-95A4-BEEA047E830C", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );

            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Time Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "111609E5-EBCC-4B1A-992A-5B61A57327C1", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Time Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "111609E5-EBCC-4B1A-992A-5B61A57327C1", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );

            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Save Attendance (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "ACA84721-3048-4953-A3DB-34DAA598357C", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Save Attendance (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "ACA84721-3048-4953-A3DB-34DAA598357C", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );

            //**********************
            //* New Action Types
            //**********************

            // Filter Locations By Schedule
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CheckIn.FilterLocationsBySchedule", "DC86310C-44CF-44F5-804E-5085A29F5AAE", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "DC86310C-44CF-44F5-804E-5085A29F5AAE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "5E590D32-9101-457D-9296-4FED6EA992F4" ); // Rock.Workflow.Action.CheckIn.FilterLocationsBySchedule:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "DC86310C-44CF-44F5-804E-5085A29F5AAE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Remove", "Remove", "Select 'Yes' if schedules should be be removed.  Select 'No' if they should just be marked as excluded.", 0, @"True", "4C6ACFAD-F94B-43F7-AA7C-FEF48EFAA79C" ); // Rock.Workflow.Action.CheckIn.FilterLocationsBySchedule:Remove
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "DC86310C-44CF-44F5-804E-5085A29F5AAE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "874AC9A3-CA6D-4C1F-8CCA-BCB7BFC74C19" ); // Rock.Workflow.Action.CheckIn.FilterLocationsBySchedule:Order

            // Filter Locations By Threshold
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CheckIn.FilterLocationsByThreshold", "EB9E5114-D86D-49CF-89A1-6EF52428AD2E", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EB9E5114-D86D-49CF-89A1-6EF52428AD2E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "62AACC4A-E976-43F5-8828-A8D2D1AC5D1B" ); // Rock.Workflow.Action.CheckIn.FilterLocationsByThreshold:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EB9E5114-D86D-49CF-89A1-6EF52428AD2E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Remove", "Remove", "Select 'Yes' if locations should be be removed.  Select 'No' if they should just be marked as excluded.", 0, @"True", "097966C4-8FE0-4464-A340-C3E78B0D3693" ); // Rock.Workflow.Action.CheckIn.FilterLocationsByThreshold:Remove
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EB9E5114-D86D-49CF-89A1-6EF52428AD2E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "3C6C2ABA-8705-4674-9BB5-F07AEBC38EDC" ); // Rock.Workflow.Action.CheckIn.FilterLocationsByThreshold:Order

            // PreSelect Recent Attendance
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CheckIn.PreSelectRecentAttendance", "23C1D3FA-4F16-4AF6-9528-04C7C52F5C2A", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "23C1D3FA-4F16-4AF6-9528-04C7C52F5C2A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "6AB918C5-5515-42A9-A54F-160E3C239710" ); // Rock.Workflow.Action.CheckIn.PreSelectRecentAttendance:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "23C1D3FA-4F16-4AF6-9528-04C7C52F5C2A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "B3A0641C-65B3-43AF-9BD5-AC62A72C5798" ); // Rock.Workflow.Action.CheckIn.PreSelectRecentAttendance:Order

            // Remove Empty Locations
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CheckIn.RemoveEmptyLocations", "51AE2690-ED00-423D-86AD-6E97054F04A9", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "51AE2690-ED00-423D-86AD-6E97054F04A9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "5E8DBE2C-DD06-43C1-A587-3F3DA4423964" ); // Rock.Workflow.Action.CheckIn.RemoveEmptyLocations:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "51AE2690-ED00-423D-86AD-6E97054F04A9", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "9CA7DD25-422D-4859-B8B4-C2A44293D485" ); // Rock.Workflow.Action.CheckIn.RemoveEmptyLocations:Order

            // Set Available Schedules
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CheckIn.SetAvailableSchedules", "0F16E0C5-825A-4058-8285-6370DAAC2C19", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "0F16E0C5-825A-4058-8285-6370DAAC2C19", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "DE072E28-C3B5-42A0-B12F-A0D8BE8F6975" ); // Rock.Workflow.Action.CheckIn.SetAvailableSchedules:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "0F16E0C5-825A-4058-8285-6370DAAC2C19", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "B6256274-8971-4DA2-9144-ED732B46EC5B" ); // Rock.Workflow.Action.CheckIn.SetAvailableSchedules:Order

            //**********************
            //* Workflow Config
            //**********************

            // Person Search
            RockMigrationHelper.AddActionTypeAttributeValue( "BB45E6E1-C39A-42A2-B988-490382DB7977", "0A6737BD-739A-4803-A3B9-5AD23AF70106", @"True" ); // Unattended Check-in:Person Search:Filter Groups by Age:Remove

            RockMigrationHelper.UpdateWorkflowActionType( "EB744DF1-E454-482C-B111-80A54EF8A674", "Filter Locations by Threshold", 99, "EB9E5114-D86D-49CF-89A1-6EF52428AD2E", true, false, "", "66EF6CB1-1A96-2F81-4534-3BCA5C33D4CD", 1, "False", "6A4E09F0-7AAF-441A-AAD7-BFEA7AF08A6A" ); // Unattended Check-in:Person Search:Filter Locations by Threshold
            RockMigrationHelper.AddActionTypeAttributeValue( "6A4E09F0-7AAF-441A-AAD7-BFEA7AF08A6A", "62AACC4A-E976-43F5-8828-A8D2D1AC5D1B", @"False" ); // Unattended Check-in:Person Search:Filter Locations by Threshold:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6A4E09F0-7AAF-441A-AAD7-BFEA7AF08A6A", "097966C4-8FE0-4464-A340-C3E78B0D3693", @"True" ); // Unattended Check-in:Person Search:Filter Locations by Threshold:Remove
            RockMigrationHelper.AddActionTypeAttributeValue( "6A4E09F0-7AAF-441A-AAD7-BFEA7AF08A6A", "3C6C2ABA-8705-4674-9BB5-F07AEBC38EDC", @"" ); // Unattended Check-in:Person Search:Filter Locations by Threshold:Order

            RockMigrationHelper.UpdateWorkflowActionType( "EB744DF1-E454-482C-B111-80A54EF8A674", "Set Available Schedules", 99, "0F16E0C5-825A-4058-8285-6370DAAC2C19", true, false, "", "", 1, "", "79CB608D-ED25-4526-A0F5-132D13642CDA" ); // Unattended Check-in:Person Search:Set Available Schedules
            RockMigrationHelper.AddActionTypeAttributeValue( "79CB608D-ED25-4526-A0F5-132D13642CDA", "B6256274-8971-4DA2-9144-ED732B46EC5B", @"" ); // Unattended Check-in:Person Search:Set Available Schedules:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "79CB608D-ED25-4526-A0F5-132D13642CDA", "DE072E28-C3B5-42A0-B12F-A0D8BE8F6975", @"False" ); // Unattended Check-in:Person Search:Set Available Schedules:Active

            RockMigrationHelper.UpdateWorkflowActionType( "EB744DF1-E454-482C-B111-80A54EF8A674", "Preselect Recent Attendees", 99, "23C1D3FA-4F16-4AF6-9528-04C7C52F5C2A", true, false, "", "", 1, "", "08D15C7A-4421-420A-BCA8-D6EE532E659F" ); // Unattended Check-in:Person Search:Preselect Recent Attendees
            RockMigrationHelper.AddActionTypeAttributeValue( "08D15C7A-4421-420A-BCA8-D6EE532E659F", "B3A0641C-65B3-43AF-9BD5-AC62A72C5798", @"" ); // Unattended Check-in:Person Search:Preselect Recent Attendees:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "08D15C7A-4421-420A-BCA8-D6EE532E659F", "6AB918C5-5515-42A9-A54F-160E3C239710", @"False" ); // Unattended Check-in:Person Search:Preselect Recent Attendees:Active

            // Ability Level Search
            RockMigrationHelper.UpdateWorkflowActionType( "0E2F5EBA-2204-4C2F-845A-92C25AB67474", "Set Available Schedules", 99, "0F16E0C5-825A-4058-8285-6370DAAC2C19", true, false, "", "", 1, "", "902931D2-6326-4A6A-967C-C9F65F8C1386" ); // Unattended Check-in:Ability Level Search:Set Available Schedules
            RockMigrationHelper.AddActionTypeAttributeValue( "902931D2-6326-4A6A-967C-C9F65F8C1386", "B6256274-8971-4DA2-9144-ED732B46EC5B", @"" ); // Unattended Check-in:Ability Level Search:Set Available Schedules:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "902931D2-6326-4A6A-967C-C9F65F8C1386", "DE072E28-C3B5-42A0-B12F-A0D8BE8F6975", @"False" ); // Unattended Check-in:Ability Level Search:Set Available Schedules:Active

            // Load Schedules 
            RockMigrationHelper.UpdateWorkflowActivityType( "011E9F5A-60D4-4FF5-912A-290881E37EAF", true, "Load Schedules", "Loads schedules for Family Check-In", false, 6, "118F4E64-BF2D-484C-BB7D-15CF25066173" ); // Unattended Check-in:Load Schedules

            RockMigrationHelper.UpdateWorkflowActionType( "118F4E64-BF2D-484C-BB7D-15CF25066173", "Load Schedules", 0, "24A7E196-B50B-4BD6-A347-07CFC5ABEF9E", true, false, "", "", 1, "", "992CDCED-02C8-4624-9C69-D7483713BA4A" ); // Unattended Check-in:Load Schedules:Load Schedules
            RockMigrationHelper.AddActionTypeAttributeValue( "992CDCED-02C8-4624-9C69-D7483713BA4A", "B222CAF2-DF12-433C-B5D4-A8DB95B60207", @"True" ); // Unattended Check-in:Load Schedules:Load Schedules:Load All
            RockMigrationHelper.AddActionTypeAttributeValue( "992CDCED-02C8-4624-9C69-D7483713BA4A", "19CB6600-7BEB-43BA-A17E-CA7E8466D93B", @"" ); // Unattended Check-in:Load Schedules:Load Schedules:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "992CDCED-02C8-4624-9C69-D7483713BA4A", "7BFA61E2-2FBB-4ADE-A6B0-38E8C0D1EB61", @"False" ); // Unattended Check-in:Load Schedules:Load Schedules:Active

            // Schedule Select 
            RockMigrationHelper.UpdateWorkflowActivityType( "011E9F5A-60D4-4FF5-912A-290881E37EAF", true, "Schedule Select", "Filters the locations, groups, and group types by the selected schedule(s) in Family Check-In", false, 7, "5D86DC3F-D56A-49D7-B6CC-5ED2B7E59A93" ); // Unattended Check-in:Schedule Select

            RockMigrationHelper.UpdateWorkflowActionType( "5D86DC3F-D56A-49D7-B6CC-5ED2B7E59A93", "Filter Locations By Schedule", 0, "DC86310C-44CF-44F5-804E-5085A29F5AAE", true, false, "", "", 1, "", "9C80AED6-23F7-485F-ACCA-DB3F722E6F16" ); // Unattended Check-in:Schedule Select:Filter Locations By Schedule
            RockMigrationHelper.AddActionTypeAttributeValue( "9C80AED6-23F7-485F-ACCA-DB3F722E6F16", "4C6ACFAD-F94B-43F7-AA7C-FEF48EFAA79C", @"False" ); // Unattended Check-in:Schedule Select:Filter Locations By Schedule:Remove
            RockMigrationHelper.AddActionTypeAttributeValue( "9C80AED6-23F7-485F-ACCA-DB3F722E6F16", "874AC9A3-CA6D-4C1F-8CCA-BCB7BFC74C19", @"" ); // Unattended Check-in:Schedule Select:Filter Locations By Schedule:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "9C80AED6-23F7-485F-ACCA-DB3F722E6F16", "5E590D32-9101-457D-9296-4FED6EA992F4", @"False" ); // Unattended Check-in:Schedule Select:Filter Locations By Schedule:Active

            RockMigrationHelper.UpdateWorkflowActionType( "5D86DC3F-D56A-49D7-B6CC-5ED2B7E59A93", "Remove Empty Locations", 1, "51AE2690-ED00-423D-86AD-6E97054F04A9", true, false, "", "", 1, "", "0E02BD35-17EA-430E-BA61-A227FC4FC535" ); // Unattended Check-in:Schedule Select:Remove Empty Locations
            RockMigrationHelper.AddActionTypeAttributeValue( "0E02BD35-17EA-430E-BA61-A227FC4FC535", "9CA7DD25-422D-4859-B8B4-C2A44293D485", @"" ); // Unattended Check-in:Schedule Select:Remove Empty Locations:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "0E02BD35-17EA-430E-BA61-A227FC4FC535", "5E8DBE2C-DD06-43C1-A587-3F3DA4423964", @"False" ); // Unattended Check-in:Schedule Select:Remove Empty Locations:Active

            RockMigrationHelper.UpdateWorkflowActionType( "5D86DC3F-D56A-49D7-B6CC-5ED2B7E59A93", "Remove Empty Groups", 2, "698115D4-7B5E-48F3-BBB0-C53A20193169", true, false, "", "", 1, "", "D335782D-3D07-497D-944D-6CC0414C06A8" ); // Unattended Check-in:Schedule Select:Remove Empty Groups
            RockMigrationHelper.AddActionTypeAttributeValue( "D335782D-3D07-497D-944D-6CC0414C06A8", "88D9EED1-BFDF-4D31-A25E-0C52EBBF55A3", @"" ); // Unattended Check-in:Schedule Select:Remove Empty Groups:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "D335782D-3D07-497D-944D-6CC0414C06A8", "2122F250-222A-45A5-8E76-EE9AA3633DC5", @"False" ); // Unattended Check-in:Schedule Select:Remove Empty Groups:Active

            RockMigrationHelper.UpdateWorkflowActionType( "5D86DC3F-D56A-49D7-B6CC-5ED2B7E59A93", "Remove Empty Group Types", 3, "E998B9A7-31C9-46F6-B91C-4E5C3F06C82F", true, false, "", "", 1, "", "F6CA869D-732B-4E98-A790-7314756CDF44" ); // Unattended Check-in:Schedule Select:Remove Empty Group Types
            RockMigrationHelper.AddActionTypeAttributeValue( "F6CA869D-732B-4E98-A790-7314756CDF44", "9B76E1C8-D562-4ED9-842B-11ADF06EB70B", @"" ); // Unattended Check-in:Schedule Select:Remove Empty Group Types:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "F6CA869D-732B-4E98-A790-7314756CDF44", "7461AAF6-E4DA-4052-BCA5-D25386CA65C5", @"False" ); // Unattended Check-in:Schedule Select:Remove Empty Group Types:Active

            RockMigrationHelper.UpdateWorkflowActionType( "5D86DC3F-D56A-49D7-B6CC-5ED2B7E59A93", "Set Available Schedules", 4, "0F16E0C5-825A-4058-8285-6370DAAC2C19", true, false, "", "", 1, "", "83D1D8BB-08FB-480C-AA6E-5C4BD527CA54" ); // Unattended Check-in:Schedule Select:Set Available Schedules
            RockMigrationHelper.AddActionTypeAttributeValue( "83D1D8BB-08FB-480C-AA6E-5C4BD527CA54", "B6256274-8971-4DA2-9144-ED732B46EC5B", @"" ); // Unattended Check-in:Schedule Select:Set Available Schedules:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "83D1D8BB-08FB-480C-AA6E-5C4BD527CA54", "DE072E28-C3B5-42A0-B12F-A0D8BE8F6975", @"False" ); // Unattended Check-in:Schedule Select:Set Available Schedules:Active

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.BinaryFile", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "BinaryFileTypeId", "1", "Print For Each", "When a family checks in, should this label be printed once per family, person, or location. Note: this only applys if check-in is configured to use Family check-in vs Individual check-in.", 1, "1", "733944B7-A0D5-41B4-94D4-DE007F72B6F0", "core_LabelType" );
            RockMigrationHelper.UpdateAttributeQualifier( "733944B7-A0D5-41B4-94D4-DE007F72B6F0", "fieldtype", "ddl", "D9313D93-A2AB-4FE9-B743-A376DEB3EE31" );
            RockMigrationHelper.UpdateAttributeQualifier( "733944B7-A0D5-41B4-94D4-DE007F72B6F0", "values", "0^Family,1^Person,2^Location", "E77DF4E6-A995-4C82-BBB7-DB57739D66F3" );

            RockMigrationHelper.AddDefinedValue( "E4D289A9-70FA-4381-913E-2A757AD11147", "Current Day/Date", "Displays The Current Day and Date", "23502286-D921-4455-BABC-D8D6CB8FFB3D", false );
            RockMigrationHelper.AddDefinedValue( "E4D289A9-70FA-4381-913E-2A757AD11147", "Codes And Ages (Even)", "Displays the codes and ages for the second, fourth, etc. person checked in using Family Check-in", "5B11A934-0398-429F-9A91-F727153392E7", false );
            RockMigrationHelper.AddDefinedValue( "E4D289A9-70FA-4381-913E-2A757AD11147", "Codes And Ages (Odd)", "Displays the codes and ages for the first, third, etc. person checked in using Family Check-in", "170207B6-9218-4E6E-8ADA-661521E80E5E", false );
            RockMigrationHelper.AddDefinedValue( "E4D289A9-70FA-4381-913E-2A757AD11147", "Name and Code (Even)", "Displays the name and security code for the second, fourth, etc. person who checked in during family check-in.", "3DCF76E8-866C-4EC9-B1FB-552691A8B440", false );
            RockMigrationHelper.AddDefinedValue( "E4D289A9-70FA-4381-913E-2A757AD11147", "Name and Code (Odd)", "Displays the name and security code for the first, third, etc. person who checked in during family check-in.", "1FAA4DAC-5240-486E-A23F-2A47D7F36F31", false );
            RockMigrationHelper.AddDefinedValue( "E4D289A9-70FA-4381-913E-2A757AD11147", "Person Locations and Times", "The locations and times that person checked into", "08882D9E-4D49-4D1E-94D2-7E5CF64A570D", false );
            RockMigrationHelper.AddDefinedValue( "E4D289A9-70FA-4381-913E-2A757AD11147", "Location and Times", "The name and times for a specific location that person checked into", "B407E2A5-A7DC-4C29-9B41-B88C99838BD1", false );

            RockMigrationHelper.AddDefinedValueAttributeValue( "23502286-D921-4455-BABC-D8D6CB8FFB3D", "51EB8583-55EA-4431-8B66-B5BD0F83D81E", @"{{ ''Now'' | Date:''ddd M/d'' }}" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5B11A934-0398-429F-9A91-F727153392E7", "51EB8583-55EA-4431-8B66-B5BD0F83D81E", @"{% for person in People %}{% assign remainder = forloop.index | Modulo:2 %}{% if remainder == 0 %}{{ person.SecurityCode }}-{{ person.Age }}yr\&{% endif %}{% endfor %}" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "170207B6-9218-4E6E-8ADA-661521E80E5E", "51EB8583-55EA-4431-8B66-B5BD0F83D81E", @"{% for person in People %}{% assign remainder = forloop.index | Modulo:2 %}{% if remainder > 0 %}{{ person.SecurityCode }}-{{ person.Age }}yr\&{% endif %}{% endfor %}" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3DCF76E8-866C-4EC9-B1FB-552691A8B440", "51EB8583-55EA-4431-8B66-B5BD0F83D81E", @"{% for person in People %}{% assign remainder = forloop.index | Modulo:2 %}{% if remainder == 0 %}{{ person.NickName }}-{{ person.SecurityCode }}\&{% endif %}{% endfor %}" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "1FAA4DAC-5240-486E-A23F-2A47D7F36F31", "51EB8583-55EA-4431-8B66-B5BD0F83D81E", @"{% for person in People %}{% assign remainder = forloop.index | Modulo:2 %}{% if remainder != 0 %}{{ person.NickName }}-{{ person.SecurityCode }}\&{% endif %}{% endfor %}" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "08882D9E-4D49-4D1E-94D2-7E5CF64A570D", "51EB8583-55EA-4431-8B66-B5BD0F83D81E", @"{% for group in GroupType.Groups %}{% for location in group.Locations %}{% if forloop.index > 1 %}; {% endif %}{{location.Name}}{% for schedule in location.Schedules %} {{schedule.Name}}{% endfor %}{% endfor %}{% endfor %}" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B407E2A5-A7DC-4C29-9B41-B88C99838BD1", "51EB8583-55EA-4431-8B66-B5BD0F83D81E", @"{{ Location.Name }}{% for schedule in Location.Schedules %} {{schedule.Name}}{% endfor %}" );

            Sql( MigrationSQL._201605232234462_FamilyCheckinType );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "733944B7-A0D5-41B4-94D4-DE007F72B6F0" );

            Sql( @"
    UPDATE [Attribute] SET
        [Name] = 'One Parent Label',
        [Key] = 'core_checkin_OneParentLabel'
    WHERE [Guid] = 'EC7FA927-95D0-44A8-8AB3-2D74A9FA2F26'
" );

            // Attrib for BlockType: Success:Workflow Activity
            RockMigrationHelper.DeleteAttribute( "185504E0-2E80-434D-8E95-EEA3D38D92F0" );
            // Attrib for BlockType: Location Select:Next Page
            RockMigrationHelper.DeleteAttribute( "BA7AB351-CF98-4846-90C2-62F5EE8D799C" );
            // Attrib for BlockType: Location Select:Previous Page
            RockMigrationHelper.DeleteAttribute( "0320C949-F06B-49FF-A9E1-F686CB14841C" );
            // Attrib for BlockType: Location Select:Home Page
            RockMigrationHelper.DeleteAttribute( "44646AA4-0E73-4AE2-B456-B2F7E9C96BAE" );
            // Attrib for BlockType: Location Select:Workflow Activity
            RockMigrationHelper.DeleteAttribute( "21715BAA-59CE-41F7-8D7B-925C8DB4F3DD" );
            // Attrib for BlockType: Location Select:Workflow Type
            RockMigrationHelper.DeleteAttribute( "BA9AD11A-DB90-4BF6-ACDA-6FFB56C0358A" );
            // Attrib for BlockType: Location Select:Next Page (Family Check-in)
            RockMigrationHelper.DeleteAttribute( "BA3A2508-4AEF-44B6-895C-899D5AB2E378" );
            // Attrib for BlockType: Location Select:Repeat Page (Family Check-in)
            RockMigrationHelper.DeleteAttribute( "FDD6D619-028F-484C-A5FF-CB9B16D596CA" );
            // Attrib for BlockType: Group Type Select:Select All and Skip
            RockMigrationHelper.DeleteAttribute( "41AFF704-87A8-4282-80D0-B7C40983B549" );
            // Attrib for BlockType: Ability Level Select:Previous Page (Family Check-in)
            RockMigrationHelper.DeleteAttribute( "3D24A4D2-90AF-4FDD-8CE2-7D1F9B76104B" );
            // Attrib for BlockType: Ability Level Select:Repeat Page (Family Check-in)
            RockMigrationHelper.DeleteAttribute( "B7A328EC-69EE-4472-8D5D-F6B5BEF271EE" );
            // Attrib for BlockType: Person Select (Family Check-in):Next Page
            RockMigrationHelper.DeleteAttribute( "DF2EC9E1-2499-4439-B926-A82CE950837D" );
            // Attrib for BlockType: Person Select (Family Check-in):Previous Page
            RockMigrationHelper.DeleteAttribute( "E9C55730-B5C4-4640-B8A0-86F10F9F1EE7" );
            // Attrib for BlockType: Person Select (Family Check-in):Home Page
            RockMigrationHelper.DeleteAttribute( "9E1DD40E-62F1-40BA-8BFD-2D2D4D8F00C6" );
            // Attrib for BlockType: Person Select (Family Check-in):Workflow Activity
            RockMigrationHelper.DeleteAttribute( "C1F2BA5A-5FA3-459E-A7C4-965BBCDF501D" );
            // Attrib for BlockType: Person Select (Family Check-in):Workflow Type
            RockMigrationHelper.DeleteAttribute( "C7ABDF19-09B1-4426-B409-CAA1BBB13A11" );
            // Attrib for BlockType: Family Select:Next Page (Family Check-in)
            RockMigrationHelper.DeleteAttribute( "1C9DAB1C-C2B3-421D-BB53-554C28F33B27" );
            // Attrib for BlockType: Administration:Enable Kiosk Match By Name
            RockMigrationHelper.DeleteAttribute( "0E252443-86E1-4068-8B32-9943E0974C94" );

            // Remove Block: Idle Redirect, from Page: Save Attendance (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.DeleteBlock( "ACA84721-3048-4953-A3DB-34DAA598357C" );
            // Remove Block: Idle Redirect, from Page: Time Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.DeleteBlock( "111609E5-EBCC-4B1A-992A-5B61A57327C1" );
            // Remove Block: Idle Redirect, from Page: Person Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.DeleteBlock( "619DEBA2-B84A-48CF-95A4-BEEA047E830C" );
            // Remove Block: Process Only, from Page: Save Attendance (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.DeleteBlock( "887E090F-5468-44F0-BE84-9AE21D822987" );
            // Remove Block: Time Select, from Page: Time Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.DeleteBlock( "558C15C1-47F7-4232-A069-89463B17924F" );
            // Remove Block: Person Select, from Page: Person Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.DeleteBlock( "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5" );

            RockMigrationHelper.DeleteBlockType( "F7B86942-9BF2-4132-B5EB-C7310952ECFF" ); // Process Only
            RockMigrationHelper.DeleteBlockType( "92DCF018-F551-4890-8BA1-511D97BF6B8A" ); // Person Select (Family Check-in)

            RockMigrationHelper.DeletePage( "4AF7A0E1-E991-4AE5-A2B5-C440F67A2E6A" ); //  Page: Save Attendance (Family Check-in), Layout: Checkin, Site: Rock Check-in
            RockMigrationHelper.DeletePage( "67BD09B0-0C6E-44E7-A8EB-0E71551F3E6B" ); //  Page: Time Select (Family Check-in), Layout: Checkin, Site: Rock Check-in
            RockMigrationHelper.DeletePage( "D14154BA-2F2C-41C3-B380-F833252CBB13" ); //  Page: Person Select (Family Check-in), Layout: Checkin, Site: Rock Check-in
        }
    }
}
