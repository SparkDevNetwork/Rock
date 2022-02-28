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
    public partial class CodeGenerated_20211116 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
            // Attribute for BlockType
            //   BlockType: Calendar Lava
            //   Category: Event
            //   Attribute: Approval Status Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Approval Status Filter", "ApprovalStatusFilter", "Approval Status Filter", @"Allows filtering events by their approval status.", 22, @"1", "27F5FFB6-B9FF-4219-A737-2D74D37279F7" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Mobile > Connection
            //   Attribute: Header Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0015A574-C10A-4530-897C-F7B7C3D9393E", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Header Template", "HeaderTemplate", "Header Template", @"Lava template used to render the header above the connection request.", 0, @"", "5302982B-C725-4814-B6B5-8E2F44C2FE2E" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Mobile > Connection
            //   Attribute: Opportunity Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0015A574-C10A-4530-897C-F7B7C3D9393E", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Opportunity Template", "OpportunityTemplate", "Opportunity Template", @"The template used to render the connection opportunities.", 1, @"1FB8E236-DF34-4BA2-B5C6-CA8B542ABC7A", "CCF0ED34-5FBF-4042-A212-0078F38E4329" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Mobile > Connection
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0015A574-C10A-4530-897C-F7B7C3D9393E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page to link to when user taps on a connection opportunity. ConnectionOpportunityGuid is passed in the query string.", 2, @"", "FE3E8C9B-C127-4AE0-8550-481FE9734DEF" );

            // Attribute for BlockType
            //   BlockType: Connection Request Detail
            //   Category: Mobile > Connection
            //   Attribute: Header Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EF537CC9-5E53-4832-A473-0D5EA439C296", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Header Template", "HeaderTemplate", "Header Template", @"Lava template used to render the header above the connection request.", 0, @"", "FBC41A4A-379F-4113-BDC8-E7F9417C5B84" );

            // Attribute for BlockType
            //   BlockType: Connection Request Detail
            //   Category: Mobile > Connection
            //   Attribute: Activity Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EF537CC9-5E53-4832-A473-0D5EA439C296", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Activity Template", "ActivityTemplate", "Activity Template", @"The template used to render the activity history for the connection request.", 1, @"D19A6D1A-BB4F-45FB-92DE-17EB97479F40", "C6314C47-DEC3-4D42-A450-8B9CD4CC9E96" );

            // Attribute for BlockType
            //   BlockType: Connection Request Detail
            //   Category: Mobile > Connection
            //   Attribute: Person Profile Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EF537CC9-5E53-4832-A473-0D5EA439C296", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "Person Profile Page", @"Page to link to when user taps on the profile button. PersonGuid is passed in the query string.", 2, @"", "F23582A2-430F-481E-9B16-F6FA1CD5A215" );

            // Attribute for BlockType
            //   BlockType: Connection Request Detail
            //   Category: Mobile > Connection
            //   Attribute: Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EF537CC9-5E53-4832-A473-0D5EA439C296", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"Page to link to when user taps on the group. GroupGuid is passed in the query string.", 3, @"", "EAC4094A-DFAF-4217-8B30-FA0632F12169" );

            // Attribute for BlockType
            //   BlockType: Connection Request Detail
            //   Category: Mobile > Connection
            //   Attribute: Workflow Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EF537CC9-5E53-4832-A473-0D5EA439C296", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Page", "WorkflowPage", "Workflow Page", @"Page to link to when user launches a workflow that requires interaction. WorkflowTypeGuid is passed in the query string.", 4, @"", "A0AF6537-40B0-47FA-9790-567A7599A7A6" );

            // Attribute for BlockType
            //   BlockType: Connection Request List
            //   Category: Mobile > Connection
            //   Attribute: Header Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "612E9E13-434F-4E47-958D-37E1C3EEF304", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Header Template", "HeaderTemplate", "Header Template", @"Lava template used to render the header above the connection request.", 0, @"", "E690E353-AAB5-470D-BF6E-2F5E7DA2B004" );

            // Attribute for BlockType
            //   BlockType: Connection Request List
            //   Category: Mobile > Connection
            //   Attribute: Request Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "612E9E13-434F-4E47-958D-37E1C3EEF304", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Request Template", "RequestTemplate", "Request Template", @"The template used to render the connection requests.", 1, @"787BFAA8-FF61-49BA-80DD-67074DC362C2", "C3823686-F00B-42BC-9A34-8EF306CDF92D" );

            // Attribute for BlockType
            //   BlockType: Connection Request List
            //   Category: Mobile > Connection
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "612E9E13-434F-4E47-958D-37E1C3EEF304", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page to link to when user taps on a connection request. ConnectionRequestGuid is passed in the query string.", 2, @"", "E7522266-53CE-41A3-9D86-D112125C51C0" );

            // Attribute for BlockType
            //   BlockType: Connection Request List
            //   Category: Mobile > Connection
            //   Attribute: Max Requests to Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "612E9E13-434F-4E47-958D-37E1C3EEF304", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Requests to Show", "MaxRequstsToShow", "Max Requests to Show", @"The maximum number of requests to show in a single load, a Load More button will be visible if there are more requests to show.", 3, @"50", "2893DED4-91EB-45BF-978D-97EAA25BAB5F" );

            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Mobile > Connection
            //   Attribute: Header Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31E1FCCF-C4B1-4D84-992C-DEACAF3697CF", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Header Template", "HeaderTemplate", "Header Template", @"Lava template used to render the header above the connection request.", 0, @"", "C3B8B014-1D0D-4072-8191-3112A65B56EF" );

            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Mobile > Connection
            //   Attribute: Type Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31E1FCCF-C4B1-4D84-992C-DEACAF3697CF", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Type Template", "TypeTemplate", "Type Template", @"The template used to render the connection types.", 1, @"E0D00422-7895-4081-9C06-16DE9BF48E1A", "E246A9C0-DE64-48C1-80C1-CA91F39D4EF5" );

            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Mobile > Connection
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31E1FCCF-C4B1-4D84-992C-DEACAF3697CF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page to link to when user taps on a connection type. ConnectionTypeGuid is passed in the query string.", 2, @"", "DBDF4799-CD3E-4ECA-A89E-C9F0A693BFD7" );

            // Attribute for BlockType
            //   BlockType: Calendar Event List
            //   Category: Mobile > Events
            //   Attribute: Show Past Events
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A9149623-6A82-4F25-8F4D-0961557BE78C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Past Events", "ShowPastEvents", "Show Past Events", @"When enabled past events will be included on the calendar, otherwise only future events will be shown.", 5, @"True", "7D5D3732-9D04-4239-AAC5-211427AA42AA" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Entry
            //   Category: Mobile > Groups
            //   Attribute: Show Attendance Notes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "08AE409C-9E4C-42D1-A93C-A554A3EEA0C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Attendance Notes", "ShowAttendanceNotes", "Show Attendance Notes", @"Enables collecting notes about the attendance. This will automatically show the save button as well.", 5, @"False", "BD99B5D2-782D-4BF7-955F-40C554C49C95" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Entry
            //   Category: Mobile > Groups
            //   Attribute: Attendance Note Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "08AE409C-9E4C-42D1-A93C-A554A3EEA0C3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Attendance Note Label", "AttendanceNoteLabel", "Attendance Note Label", @"The label that will describe how the notes will be used.", 6, @"Notes", "4D695E6B-1233-423B-9FA1-A711E7CC4613" );

            // Attribute for BlockType
            //   BlockType: Group Member Edit
            //   Category: Mobile > Groups
            //   Attribute: Enable Delete
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "514B533A-8970-4628-A4C8-35388CD869BC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Delete", "EnableDelete", "Enable Delete", @"Will show or hide the delete button. This will either delete or archive the member depending on the group type configuration.", 6, @"True", "ECA79729-5893-408A-9A3B-B547770B9771" );

            // Attribute for BlockType
            //   BlockType: Group Member Edit
            //   Category: Mobile > Groups
            //   Attribute: Delete Navigation Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "514B533A-8970-4628-A4C8-35388CD869BC", "8AF3E49F-4FF1-47D8-BCD2-150201B7F1B8", "Delete Navigation Action", "DeleteNavigationAction", "Delete Navigation Action", @"The action to perform after the group member is deleted from the group.", 7, @"{""Type"": 1, ""PopCount"": 1}", "D0CA01F2-A768-4F7F-B252-11038FE3A85A" );
            RockMigrationHelper.UpdateFieldType("Mobile Navigation Action","","Rock","Rock.Field.Types.MobileNavigationActionFieldType","8AF3E49F-4FF1-47D8-BCD2-150201B7F1B8");

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
            // Attribute for BlockType
            //   BlockType: Group Member Edit
            //   Category: Mobile > Groups
            //   Attribute: Delete Navigation Action
            RockMigrationHelper.DeleteAttribute("D0CA01F2-A768-4F7F-B252-11038FE3A85A");

            // Attribute for BlockType
            //   BlockType: Group Member Edit
            //   Category: Mobile > Groups
            //   Attribute: Enable Delete
            RockMigrationHelper.DeleteAttribute("ECA79729-5893-408A-9A3B-B547770B9771");

            // Attribute for BlockType
            //   BlockType: Group Attendance Entry
            //   Category: Mobile > Groups
            //   Attribute: Attendance Note Label
            RockMigrationHelper.DeleteAttribute("4D695E6B-1233-423B-9FA1-A711E7CC4613");

            // Attribute for BlockType
            //   BlockType: Group Attendance Entry
            //   Category: Mobile > Groups
            //   Attribute: Show Attendance Notes
            RockMigrationHelper.DeleteAttribute("BD99B5D2-782D-4BF7-955F-40C554C49C95");

            // Attribute for BlockType
            //   BlockType: Calendar Event List
            //   Category: Mobile > Events
            //   Attribute: Show Past Events
            RockMigrationHelper.DeleteAttribute("7D5D3732-9D04-4239-AAC5-211427AA42AA");

            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Mobile > Connection
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute("DBDF4799-CD3E-4ECA-A89E-C9F0A693BFD7");

            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Mobile > Connection
            //   Attribute: Type Template
            RockMigrationHelper.DeleteAttribute("E246A9C0-DE64-48C1-80C1-CA91F39D4EF5");

            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Mobile > Connection
            //   Attribute: Header Template
            RockMigrationHelper.DeleteAttribute("C3B8B014-1D0D-4072-8191-3112A65B56EF");

            // Attribute for BlockType
            //   BlockType: Connection Request List
            //   Category: Mobile > Connection
            //   Attribute: Max Requests to Show
            RockMigrationHelper.DeleteAttribute("2893DED4-91EB-45BF-978D-97EAA25BAB5F");

            // Attribute for BlockType
            //   BlockType: Connection Request List
            //   Category: Mobile > Connection
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute("E7522266-53CE-41A3-9D86-D112125C51C0");

            // Attribute for BlockType
            //   BlockType: Connection Request List
            //   Category: Mobile > Connection
            //   Attribute: Request Template
            RockMigrationHelper.DeleteAttribute("C3823686-F00B-42BC-9A34-8EF306CDF92D");

            // Attribute for BlockType
            //   BlockType: Connection Request List
            //   Category: Mobile > Connection
            //   Attribute: Header Template
            RockMigrationHelper.DeleteAttribute("E690E353-AAB5-470D-BF6E-2F5E7DA2B004");

            // Attribute for BlockType
            //   BlockType: Connection Request Detail
            //   Category: Mobile > Connection
            //   Attribute: Workflow Page
            RockMigrationHelper.DeleteAttribute("A0AF6537-40B0-47FA-9790-567A7599A7A6");

            // Attribute for BlockType
            //   BlockType: Connection Request Detail
            //   Category: Mobile > Connection
            //   Attribute: Group Detail Page
            RockMigrationHelper.DeleteAttribute("EAC4094A-DFAF-4217-8B30-FA0632F12169");

            // Attribute for BlockType
            //   BlockType: Connection Request Detail
            //   Category: Mobile > Connection
            //   Attribute: Person Profile Page
            RockMigrationHelper.DeleteAttribute("F23582A2-430F-481E-9B16-F6FA1CD5A215");

            // Attribute for BlockType
            //   BlockType: Connection Request Detail
            //   Category: Mobile > Connection
            //   Attribute: Activity Template
            RockMigrationHelper.DeleteAttribute("C6314C47-DEC3-4D42-A450-8B9CD4CC9E96");

            // Attribute for BlockType
            //   BlockType: Connection Request Detail
            //   Category: Mobile > Connection
            //   Attribute: Header Template
            RockMigrationHelper.DeleteAttribute("FBC41A4A-379F-4113-BDC8-E7F9417C5B84");

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Mobile > Connection
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute("FE3E8C9B-C127-4AE0-8550-481FE9734DEF");

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Mobile > Connection
            //   Attribute: Opportunity Template
            RockMigrationHelper.DeleteAttribute("CCF0ED34-5FBF-4042-A212-0078F38E4329");

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Mobile > Connection
            //   Attribute: Header Template
            RockMigrationHelper.DeleteAttribute("5302982B-C725-4814-B6B5-8E2F44C2FE2E");

            // Attribute for BlockType
            //   BlockType: Calendar Lava
            //   Category: Event
            //   Attribute: Approval Status Filter
            RockMigrationHelper.DeleteAttribute("27F5FFB6-B9FF-4219-A737-2D74D37279F7");



        }
    }
}
