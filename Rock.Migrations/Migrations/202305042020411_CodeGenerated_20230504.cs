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
    public partial class CodeGenerated_20230504 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Group.Scheduling.GroupScheduler              
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Group.Scheduling.GroupScheduler", "Group Scheduler", "Rock.Blocks.Group.Scheduling.GroupScheduler, Rock.Blocks, Version=1.16.0.2, Culture=neutral, PublicKeyToken=null", false, false, "7ADCE833-A785-4A54-9805-7335809C5367");
            
            // Add/Update Obsidian Block Type              
            //   Name:Group Scheduler              
            //   Category:Group Scheduling              
            //   EntityType:Rock.Blocks.Group.Scheduling.GroupScheduler              
            RockMigrationHelper.UpdateMobileBlockType("Group Scheduler", "Allows group schedules for groups and locations to be managed by a scheduler.", "Rock.Blocks.Group.Scheduling.GroupScheduler", "Group Scheduling", "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0");
            
            // Add Block               
            //  Block Name: Membership              
            //  Page Name: Extended Attributes V1              
            //  Layout: -              
            //  Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "816A853D-04E7-4A12-BCE4-C3DE606F6128".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership","SectionB1",@"",@"",0,"A3047FDA-10EF-41F5-A054-B927E6DED57D"); 
            
            // Attribute for BlockType              
            //   BlockType: Group Tree View              
            //   Category: Groups              
            //   Attribute: Limit to Security Role Groups              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimitToSecurityRoleGroups", "Limit to Security Role Groups", @"", 5, @"False", "11D84CCF-6E5C-4FD8-BE05-2A55283ADA81" );
            
            // Attribute for BlockType              
            //   BlockType: Log In              
            //   Category: Mobile > Cms              
            //   Attribute: Enable Auth0 Login              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6006FE32-DC01-4B1C-A9B8-EE172451F4C5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Auth0 Login", "EnableAuth0Login", "Enable Auth0 Login", @"Whether or not to enable Auth0 as an authentication provider. This must be configured in `Security > Authentication Services` beforehand.", 6, @"False", "14525BC1-7CF3-4EAC-B889-0F80FC503535" );
            
            // Attribute for BlockType              
            //   BlockType: Log In              
            //   Category: Mobile > Cms              
            //   Attribute: Enable Database Login              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6006FE32-DC01-4B1C-A9B8-EE172451F4C5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Database Login", "EnableDatabaseLogin", "Enable Database Login", @"Whether or not to enable `Database` as an authentication provider.", 7, @"True", "661AE50E-9125-40CB-BFE8-A978DF17BC4A" );
            
            // Attribute for BlockType              
            //   BlockType: Log In              
            //   Category: Mobile > Cms              
            //   Attribute: Auth0 Login Button Text              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6006FE32-DC01-4B1C-A9B8-EE172451F4C5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Auth0 Login Button Text", "Auth0LoginButtonText", "Auth0 Login Button Text", @"The text of the Auth0 login button.", 8, @"Login With Auth0", "446FD78A-1855-4ED8-8EE9-23DDDB1F4B7B" );
            
            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Mobile > Core              
            //   Attribute: Group Notification Communication Template              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5B337D89-A298-4620-A0BE-078A41BC054B", "C3B37465-DCAF-4C8C-930C-9A9B5D066CA9", "Group Notification Communication Template", "GroupNotificationCommunicationTemplate", "Group Notification Communication Template", @"The template to use to send the communication.Note  will be passed as a merge field.", 5, @"", "17182F48-0A14-46C2-8E94-3102F6A2B79F" );
            
            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Mobile > Core              
            //   Attribute: Enable Group Notification              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5B337D89-A298-4620-A0BE-078A41BC054B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Notification", "EnableGroupNotification", "Enable Group Notification", @"If a Group is available through page context, this will send a communication to every person in a group (using their `CommunicationPreference`, and the `GroupNotificationCommunicationTemplate`), when a Note is added.", 4, @"False", "A79D5EFF-80DD-452B-9B00-079723D1D895" );
            
            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Mobile > Core              
            //   Attribute: Show Is Alert Toggle              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5B337D89-A298-4620-A0BE-078A41BC054B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Is Alert Toggle", "ShowIsAlert", "Show Is Alert Toggle", @"If enabled, a person will have the option of toggling whether their note is 'Alert' or not.", 9, @"False", "296F98E3-4E3B-4D0E-B82F-D261F573D72B" );
            
            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Mobile > Core              
            //   Attribute: Show Is Private Toggle              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5B337D89-A298-4620-A0BE-078A41BC054B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Is Private Toggle", "ShowIsPrivate", "Show Is Private Toggle", @"If enabled, a person will have the option of toggling whether their note is a 'Private' note or not.", 9, @"False", "976D7FCF-2EC8-4A91-870B-F1E20121FB37" );
            
            // Attribute for BlockType              
            //   BlockType: Group Scheduler              
            //   Category: Group Scheduling              
            //   Attribute: Roster Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Roster Page", "RosterPage", "Roster Page", @"Page used for viewing the group schedule roster.", 3, @"", "E93E9434-B4A5-485E-9C06-BDD91A559B43" );
            
            // Attribute for BlockType              
            //   BlockType: Group Scheduler              
            //   Category: Group Scheduling              
            //   Attribute: Enable Alternate Group Individual Selection              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Alternate Group Individual Selection", "EnableAlternateGroupIndividualSelection", "Enable Alternate Group Individual Selection", @"Determines if individuals may be selected from alternate groups.", 0, @"False", "D5059FAD-8CE9-4C68-9E69-66BE7E0202F7" );
            
            // Attribute for BlockType              
            //   BlockType: Group Scheduler              
            //   Category: Group Scheduling              
            //   Attribute: Enable Parent Group Individual Selection              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Parent Group Individual Selection", "EnableParentGroupIndividualSelection", "Enable Parent Group Individual Selection", @"Determines if individuals may be selected from parent groups.", 1, @"False", "DA8FAB6E-0699-463A-A613-FBEA96E89C3B" );
            
            // Attribute for BlockType              
            //   BlockType: Group Scheduler              
            //   Category: Group Scheduling              
            //   Attribute: Enable Data View Individual Selection              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Data View Individual Selection", "EnableDataViewIndividualSelection", "Enable Data View Individual Selection", @"Determines if individuals may be selected from data views.", 2, @"False", "9DF898C6-7353-44E8-8019-E30E46083C1C" );
            
            // Add Block Attribute Value              
            //   Block: Membership              
            //   BlockType: Attribute Values              
            //   Category: CRM > Person Detail              
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS              
            //   Attribute: Category              /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */              
            RockMigrationHelper.AddBlockAttributeValue("A3047FDA-10EF-41F5-A054-B927E6DED57D","EC43CF32-3BDF-4544-8B6A-CE9208DD7C81",@"e919e722-f895-44a4-b86d-38db8fba1844");
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Mobile > Core              
            //   Attribute: Show Is Private Toggle
            RockMigrationHelper.DeleteAttribute("976D7FCF-2EC8-4A91-870B-F1E20121FB37");
            
            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Mobile > Core              
            //   Attribute: Show Is Alert Toggle              
            RockMigrationHelper.DeleteAttribute("296F98E3-4E3B-4D0E-B82F-D261F573D72B");
            
            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Mobile > Core              
            //   Attribute: Group Notification Communication Template              
            RockMigrationHelper.DeleteAttribute("17182F48-0A14-46C2-8E94-3102F6A2B79F");
            
            // Attribute for BlockType              
            //   BlockType: Notes              
            //   Category: Mobile > Core              
            //   Attribute: Enable Group Notification              
            RockMigrationHelper.DeleteAttribute("A79D5EFF-80DD-452B-9B00-079723D1D895");
            
            // Attribute for BlockType              
            //   BlockType: Log In              
            //   Category: Mobile > Cms              
            //   Attribute: Auth0 Login Button Text              
            RockMigrationHelper.DeleteAttribute("446FD78A-1855-4ED8-8EE9-23DDDB1F4B7B");
            
            // Attribute for BlockType              
            //   BlockType: Log In              
            //   Category: Mobile > Cms              
            //   Attribute: Enable Database Login              
            RockMigrationHelper.DeleteAttribute("661AE50E-9125-40CB-BFE8-A978DF17BC4A");
            
            // Attribute for BlockType              
            //   BlockType: Log In              
            //   Category: Mobile > Cms              
            //   Attribute: Enable Auth0 Login              
            RockMigrationHelper.DeleteAttribute("14525BC1-7CF3-4EAC-B889-0F80FC503535");
            
            // Attribute for BlockType              
            //   BlockType: Group Scheduler              
            //   Category: Group Scheduling              
            //   Attribute: Roster Page              
            RockMigrationHelper.DeleteAttribute("E93E9434-B4A5-485E-9C06-BDD91A559B43");
            
            // Attribute for BlockType              
            //   BlockType: Group Scheduler              
            //   Category: Group Scheduling              
            //   Attribute: Enable Data View Individual Selection              
            RockMigrationHelper.DeleteAttribute("9DF898C6-7353-44E8-8019-E30E46083C1C");
            
            // Attribute for BlockType              
            //   BlockType: Group Scheduler              
            //   Category: Group Scheduling              
            //   Attribute: Enable Parent Group Individual Selection              
            RockMigrationHelper.DeleteAttribute("DA8FAB6E-0699-463A-A613-FBEA96E89C3B");
            
            // Attribute for BlockType              
            //   BlockType: Group Scheduler              
            //   Category: Group Scheduling              
            //   Attribute: Enable Alternate Group Individual Selection              
            RockMigrationHelper.DeleteAttribute("D5059FAD-8CE9-4C68-9E69-66BE7E0202F7");
            
            // Attribute for BlockType              
            //   BlockType: Group Tree View              
            //   Category: Groups              
            //   Attribute: Limit to Security Role Groups              
            RockMigrationHelper.DeleteAttribute("11D84CCF-6E5C-4FD8-BE05-2A55283ADA81");
            
            // Remove Block              
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS              
            //  from Page: Extended Attributes V1, Site: Rock RMS              RockMigrationHelper.DeleteBlock("A3047FDA-10EF-41F5-A054-B927E6DED57D");
            
            // Delete BlockType               
            //   Name: Group Scheduler              
            //   Category: Group Scheduling              
            //   Path: -              
            //   EntityType: Group Scheduler              
            RockMigrationHelper.DeleteBlockType("511D8E2E-4AF3-48D8-88EF-2AB311CD47E0");
        }
    }
}
