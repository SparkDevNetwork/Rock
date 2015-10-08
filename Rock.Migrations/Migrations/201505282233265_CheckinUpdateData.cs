// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class CheckinUpdateData : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            RockMigrationHelper.AddPage( "7A3CF259-1090-403C-83B7-2DB3A53DEE26", "0CB60906-6B74-44FD-AB25-026050EF70EB", "Check-in Details", "", "832BF701-7D3F-4775-A8A1-C823E6A56A84", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "832BF701-7D3F-4775-A8A1-C823E6A56A84", "0CB60906-6B74-44FD-AB25-026050EF70EB", "Attendance Details", "", "9C692660-80DE-4F15-8770-1A57EE1BF201", "" ); // Site:Rock RMS

            RockMigrationHelper.UpdateBlockType( "Check-in Group List", "Lists checkin areas and their groups based off a parent checkin configuration group type.", "~/Blocks/CheckIn/CheckinGroupList.ascx", "Check-in", "67E83A02-6D23-4B90-A861-F81FF78B56C7" );

            // Add Block to Page: Check-in Details, Site: Rock RMS
            RockMigrationHelper.AddBlock( "832BF701-7D3F-4775-A8A1-C823E6A56A84", "", "67E83A02-6D23-4B90-A861-F81FF78B56C7", "Checkin Group List", "Sidebar1", "", "", 0, "0A59256A-2969-4BBF-916A-B3CDF00C16A4" );

            // Add Block to Page: Check-in Details, Site: Rock RMS
            RockMigrationHelper.AddBlock( "832BF701-7D3F-4775-A8A1-C823E6A56A84", "", "5C547728-38C2-420A-8602-3CDAAC369247", "Group Attendance List", "Main", "", "", 0, "845CD5A6-A6B6-4159-9A16-2B198F83A5EF" );

            // Add Block to Page: Attendance Details, Site: Rock RMS
            RockMigrationHelper.AddBlock( "9C692660-80DE-4F15-8770-1A57EE1BF201", "", "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "Group Attendance Detail", "Main", "", "", 0, "A99521FB-1BD2-498C-B6F7-140472A1D9DF" );

            // Add Block to Page: Attendance Details, Site: Rock RMS
            RockMigrationHelper.AddBlock( "9C692660-80DE-4F15-8770-1A57EE1BF201", "", "67E83A02-6D23-4B90-A861-F81FF78B56C7", "Checkin Group List", "Sidebar1", "", "", 0, "FCBCB471-716C-46B7-9C88-55B4B584A85C" );

            // Attrib for BlockType: Attendance Analysis:Show Group Ancestry
            RockMigrationHelper.AddBlockTypeAttribute( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Ancestry", "ShowGroupAncestry", "", "By default the group ancestry path is shown.  Unselect this to show only the group name.", 0, @"True", "80488EAE-EDFE-4C91-B26D-6EEC46EB291E" );

            // Attrib for BlockType: Attendance Analysis:Check-in Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Check-in Detail Page", "Check-inDetailPage", "", "Page that shows the user details for the check-in data.", 0, @"", "733358C0-EB7E-462C-997A-AEFCAC9A7EF8" );

            // Attrib for BlockType: Check-in Group List:Group Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "67E83A02-6D23-4B90-A861-F81FF78B56C7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "", "Link to the group details page", 0, @"", "7A442398-18C9-46D8-9678-E4C4794B7AAE" );

            // Attrib for BlockType: Check-in Group List:Check-in Type
            RockMigrationHelper.AddBlockTypeAttribute( "67E83A02-6D23-4B90-A861-F81FF78B56C7", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Check-in Type", "GroupTypeTemplate", "", "", 0, @"", "CC4CF626-2F18-4BBB-A443-18E7AF97E165" );

            // Attrib for BlockType: Group Attendance Detail:Allow Adding Person
            RockMigrationHelper.AddBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Adding Person", "AllowAddingPerson", "", "Should block support adding new attendee ( Requires that person has rights to search for new person )?", 1, @"False", "92F47ABB-C051-4D87-9266-C8DAE77736D4" );

            // Attrib Value for Block:Attendance Reporting, Attribute:Check-in Detail Page Page: Attendance Analysis, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3EF007F1-6B46-4BCD-A435-345C03EBCA17", "733358C0-EB7E-462C-997A-AEFCAC9A7EF8", @"832bf701-7d3f-4775-a8a1-c823e6a56a84" );

            // Attrib Value for Block:Checkin Group List, Attribute:Group Detail Page Page: Check-in Details, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0A59256A-2969-4BBF-916A-B3CDF00C16A4", "7A442398-18C9-46D8-9678-E4C4794B7AAE", @"832bf701-7d3f-4775-a8a1-c823e6a56a84" );

            // Attrib Value for Block:Group Attendance List, Attribute:Allow Add Page: Check-in Details, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "845CD5A6-A6B6-4159-9A16-2B198F83A5EF", "B978EBAD-333D-4E8B-8C68-19FABB87984B", @"True" );

            // Attrib Value for Block:Group Attendance List, Attribute:Detail Page Page: Check-in Details, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "845CD5A6-A6B6-4159-9A16-2B198F83A5EF", "15299237-7F47-404D-BEFF-460F7818D3D7", @"9c692660-80de-4f15-8770-1a57ee1bf201" );

            // Attrib Value for Block:Group Attendance Detail, Attribute:Allow Adding Person Page: Attendance Details, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A99521FB-1BD2-498C-B6F7-140472A1D9DF", "92F47ABB-C051-4D87-9266-C8DAE77736D4", @"True" );

            // Attrib Value for Block:Group Attendance Detail, Attribute:Allow Add Page: Attendance Details, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A99521FB-1BD2-498C-B6F7-140472A1D9DF", "D24A540E-3E6B-4790-AB77-6661F8DA292E", @"True" );

            // Attrib Value for Block:Checkin Group List, Attribute:Group Detail Page Page: Attendance Details, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FCBCB471-716C-46B7-9C88-55B4B584A85C", "7A442398-18C9-46D8-9678-E4C4794B7AAE", @"832bf701-7d3f-4775-a8a1-c823e6a56a84" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Group Attendance Detail:Allow Adding Person
            RockMigrationHelper.DeleteAttribute( "92F47ABB-C051-4D87-9266-C8DAE77736D4" );
            // Attrib for BlockType: Check-in Group List:Check-in Type
            RockMigrationHelper.DeleteAttribute( "CC4CF626-2F18-4BBB-A443-18E7AF97E165" );
            // Attrib for BlockType: Check-in Group List:Group Detail Page
            RockMigrationHelper.DeleteAttribute( "7A442398-18C9-46D8-9678-E4C4794B7AAE" );
            // Attrib for BlockType: Attendance Analysis:Check-in Detail Page
            RockMigrationHelper.DeleteAttribute( "733358C0-EB7E-462C-997A-AEFCAC9A7EF8" );
            // Attrib for BlockType: Attendance Analysis:Show Group Ancestry
            RockMigrationHelper.DeleteAttribute( "80488EAE-EDFE-4C91-B26D-6EEC46EB291E" );
            // Remove Block: Checkin Group List, from Page: Attendance Details, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "FCBCB471-716C-46B7-9C88-55B4B584A85C" );
            // Remove Block: Group Attendance Detail, from Page: Attendance Details, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A99521FB-1BD2-498C-B6F7-140472A1D9DF" );
            // Remove Block: Group Attendance List, from Page: Check-in Details, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "845CD5A6-A6B6-4159-9A16-2B198F83A5EF" );
            // Remove Block: Checkin Group List, from Page: Check-in Details, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "0A59256A-2969-4BBF-916A-B3CDF00C16A4" );

            RockMigrationHelper.DeletePage( "9C692660-80DE-4F15-8770-1A57EE1BF201" ); //  Page: Attendance Details, Layout: Left Sidebar, Site: Rock RMS
            RockMigrationHelper.DeletePage( "832BF701-7D3F-4775-A8A1-C823E6A56A84" ); //  Page: Check-in Details, Layout: Left Sidebar, Site: Rock RMS
        }
    }
}
