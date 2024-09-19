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
    public partial class CodeGenerated_20240919 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Core.MyNotes
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Core.MyNotes", "My Notes", "Rock.Blocks.Types.Mobile.Core.MyNotes, Rock, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "1CCC09C4-2994-4009-813F-2F4B86C13BFE" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Core.QuickNote
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Core.QuickNote", "Quick Note", "Rock.Blocks.Types.Mobile.Core.QuickNote, Rock, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "9AA328FB-8FBB-4C5D-A898-C9B355051ADD" );

            // Add/Update Mobile Block Type
            //   Name:My Notes
            //   Category:Mobile > Core
            //   EntityType:Rock.Blocks.Types.Mobile.Core.MyNotes
            RockMigrationHelper.AddOrUpdateEntityBlockType( "My Notes", "View notes created by you with the ability to manage associations and link unassociated notes.", "Rock.Blocks.Types.Mobile.Core.MyNotes", "Mobile > Core", "9BDE231C-B6A7-4753-BBB8-1531F6362387" );

            // Add/Update Mobile Block Type
            //   Name:Quick Note
            //   Category:Mobile > Core
            //   EntityType:Rock.Blocks.Types.Mobile.Core.QuickNote
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Quick Note", "Allows an individual to quickly add a note that is not tied to any specific entity.", "Rock.Blocks.Types.Mobile.Core.QuickNote", "Mobile > Core", "B7E187C8-6F74-4FBD-8853-5BAC48F5822C" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Administration.SystemConfiguration
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Administration.SystemConfiguration", "System Configuration", "Rock.Blocks.Administration.SystemConfiguration, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "7ECDCE1B-D63F-42AA-88B6-7C5585E1F33A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.StreakList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.StreakList", "Streak List", "Rock.Blocks.Engagement.StreakList, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "B7894CEB-837A-468E-92B1-53A1631C828E" );

            // Add/Update Obsidian Block Type
            //   Name:System Configuration
            //   Category:Administration
            //   EntityType:Rock.Blocks.Administration.SystemConfiguration
            RockMigrationHelper.AddOrUpdateEntityBlockType( "System Configuration", "Used for making configuration changes to configurable items in the web.config.", "Rock.Blocks.Administration.SystemConfiguration", "Administration", "3855B15B-C903-446A-AE5B-891AB52851CB" );

            // Add/Update Obsidian Block Type
            //   Name:Streak List
            //   Category:Streaks
            //   EntityType:Rock.Blocks.Engagement.StreakList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Streak List", "Lists all the people enrolled in a streak type.", "Rock.Blocks.Engagement.StreakList", "Streaks", "73EFC838-D5E3-4DBD-B5AF-C3C81D3E7DAF" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Prioritize Child Entry
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D6794F5-876B-47B9-9C9B-5C2C2CC81074", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Prioritize Child Entry", "PrioritizeChildEntry", "Prioritize Child Entry", @"Moves the Child panel above the Adult Information panel and starts with one child to be filled in.", 19, @"False", "09A07E82-3837-44FF-BF9B-D3408E70ECB4" );

            // Attribute for BlockType
            //   BlockType: Add Connection Request
            //   Category: Mobile > Connection
            //   Attribute: Opportunity Pre-Select Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1380115A-B3F0-49BC-A6BC-432A59DC27A2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Opportunity Pre-Select Template", "OpportunityPreSelectTemplate", "Opportunity Pre-Select Template", @"The Lava template to use when pre-selecting an opportunity. The types and their nested opportunities are stored in an 'Items' merge field.", 3, @"", "A49C0E52-B8DA-47EC-B7CD-0A4501B68D98" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk
            //   Category: Check-in
            //   Attribute: Idle Timeout
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A27FD0AA-67EE-44C3-9E5F-3289C6A210F3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Idle Timeout", "IdleTimeout", "Idle Timeout", @"The number of seconds that the kiosk can be idle without mouse or keyboard interaction before returning to the welcome screen.", 4, @"20", "23C35D06-7DB0-4BBF-A52D-79B2833E3525" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk
            //   Category: Check-in
            //   Attribute: Select All Schedules Automatically
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A27FD0AA-67EE-44C3-9E5F-3289C6A210F3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Select All Schedules Automatically", "SelectAllSchedulesAutomatically", "Select All Schedules Automatically", @"When enabled, the kiosk will automatically select all available schedules instead of asking the individual to make a selection.", 5, @"False", "B092C9FC-73A7-4D88-91E6-F7899E0E5840" );

            // Attribute for BlockType
            //   BlockType: Streak List
            //   Category: Streaks
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "73EFC838-D5E3-4DBD-B5AF-C3C81D3E7DAF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the streak details.", 0, @"", "E1482F4B-80B2-4B66-BF9E-2D2517F02F86" );

            // Attribute for BlockType
            //   BlockType: Streak List
            //   Category: Streaks
            //   Attribute: Person Profile Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "73EFC838-D5E3-4DBD-B5AF-C3C81D3E7DAF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "Person Profile Page", @"Page used for viewing a person's profile. If set, a view profile button will show for each enrollment.", 1, @"", "3C166023-9503-414E-8B0C-1CC36A8AC7FE" );

            // Attribute for BlockType
            //   BlockType: Streak List
            //   Category: Streaks
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "73EFC838-D5E3-4DBD-B5AF-C3C81D3E7DAF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "EFA519FD-6C22-4D37-B92E-D03F40D86D86" );

            // Attribute for BlockType
            //   BlockType: Streak List
            //   Category: Streaks
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "73EFC838-D5E3-4DBD-B5AF-C3C81D3E7DAF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "62A7B695-88E2-4AC8-85F9-6AEF4B9CBBC2" );

            // Attribute for BlockType
            //   BlockType: My Notes
            //   Category: Mobile > Core
            //   Attribute: Note Item Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9BDE231C-B6A7-4753-BBB8-1531F6362387", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Note Item Template", "NoteItemTemplate", "Note Item Template", @"The item template to use when rendering the notes.", 0, @"421F2759-B6B6-4C47-AA42-320B6DB9F0A7", "5F0EB28E-A441-4A50-9EE4-C13E9248F21A" );

            // Attribute for BlockType
            //   BlockType: My Notes
            //   Category: Mobile > Core
            //   Attribute: Enable Swipe for Options
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9BDE231C-B6A7-4753-BBB8-1531F6362387", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Swipe for Options", "EnableSwipeForOptions", "Enable Swipe for Options", @"When enabled, swipe actions will be available for each note.", 1, @"True", "56EDE86C-5257-415A-805B-35D0EF128035" );

            // Attribute for BlockType
            //   BlockType: My Notes
            //   Category: Mobile > Core
            //   Attribute: Person Note Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9BDE231C-B6A7-4753-BBB8-1531F6362387", "276CCA63-5670-48CA-8B5A-2AAC97E8EE5E", "Person Note Types", "PersonNoteTypes", "Person Note Types", @"The note types to allow selecting from when linking to a person.", 2, @"", "D8DD2DC7-C644-4B57-B2D7-D48CE00C1066" );

            // Attribute for BlockType
            //   BlockType: My Notes
            //   Category: Mobile > Core
            //   Attribute: Reminder Note Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9BDE231C-B6A7-4753-BBB8-1531F6362387", "E3FF88AC-13F6-4DF8-8371-FC0D7FD9A571", "Reminder Note Type", "ReminderNoteType", "Reminder Note Type", @"The note type to link when creating a reminder.", 3, @"", "3CFB3C92-1A59-4F30-B059-8BFDE87ACEB4" );

            // Attribute for BlockType
            //   BlockType: My Notes
            //   Category: Mobile > Core
            //   Attribute: Connection Note Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9BDE231C-B6A7-4753-BBB8-1531F6362387", "E3FF88AC-13F6-4DF8-8371-FC0D7FD9A571", "Connection Note Type", "ConnectionNoteType", "Connection Note Type", @"The note type to link when creating a connection.", 4, @"", "809DD5B3-40F3-47D7-9A60-4F003533C669" );

            // Attribute for BlockType
            //   BlockType: My Notes
            //   Category: Mobile > Core
            //   Attribute: Person Profile Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9BDE231C-B6A7-4753-BBB8-1531F6362387", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Detail Page", "PersonProfilePage", "Person Profile Detail Page", @"The page to link to view a person profile when a note is associated to a person.", 5, @"", "365AA793-A1D6-4767-975E-938BF5BD54DA" );

            // Attribute for BlockType
            //   BlockType: My Notes
            //   Category: Mobile > Core
            //   Attribute: Reminder Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9BDE231C-B6A7-4753-BBB8-1531F6362387", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Reminder Detail Page", "ReminderDetailPage", "Reminder Detail Page", @"The page to link to view a reminder when a note is associated to a reminder.", 6, @"", "2CA49AB1-8587-46A2-99D5-DED6E5040138" );

            // Attribute for BlockType
            //   BlockType: My Notes
            //   Category: Mobile > Core
            //   Attribute: Add Connection Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9BDE231C-B6A7-4753-BBB8-1531F6362387", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Connection Page", "AddConnectionPage", "Add Connection Page", @"The page to link to add a connection that will be associated with the note.", 7, @"", "493046BC-E3BD-49B9-98EE-215398AC9D5D" );

            // Attribute for BlockType
            //   BlockType: My Notes
            //   Category: Mobile > Core
            //   Attribute: Connection Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9BDE231C-B6A7-4753-BBB8-1531F6362387", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Connection Detail Page", "ConnectionDetailPage", "Connection Detail Page", @"The page to link to view a connection.", 8, @"", "70F2581C-9D91-4F21-A315-0890200BAC6E" );

            // Attribute for BlockType
            //   BlockType: My Notes
            //   Category: Mobile > Core
            //   Attribute: Group Notes by Date
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9BDE231C-B6A7-4753-BBB8-1531F6362387", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Group Notes by Date", "GroupNotesByDate", "Group Notes by Date", @"When enabled, notes will be grouped by date.", 9, @"True", "07C1824F-7A00-4768-A3CC-571877604425" );

            // Attribute for BlockType
            //   BlockType: Quick Note
            //   Category: Mobile > Core
            //   Attribute: Note Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B7E187C8-6F74-4FBD-8853-5BAC48F5822C", "E3FF88AC-13F6-4DF8-8371-FC0D7FD9A571", "Note Type", "NoteType", "Note Type", @"The note type associated with the Quick Note.", 0, @"A3F5982F-C4D0-4345-8021-EB38C4C9AA18", "1298F46D-B40F-4318-8584-9B6485E7E765" );

            // Attribute for BlockType
            //   BlockType: Quick Note
            //   Category: Mobile > Core
            //   Attribute: Placeholder Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B7E187C8-6F74-4FBD-8853-5BAC48F5822C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Placeholder Text", "PlaceholderText", "Placeholder Text", @"The text to display in the note text area when it is empty.", 1, @"Add a quick note...", "B64E7F40-4CED-459C-AEBA-AB0A3FCD1529" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Add Connection Request
            //   Category: Mobile > Connection
            //   Attribute: Opportunity Pre-Select Template
            RockMigrationHelper.DeleteAttribute( "A49C0E52-B8DA-47EC-B7CD-0A4501B68D98" );

            // Attribute for BlockType
            //   BlockType: Quick Note
            //   Category: Mobile > Core
            //   Attribute: Placeholder Text
            RockMigrationHelper.DeleteAttribute( "B64E7F40-4CED-459C-AEBA-AB0A3FCD1529" );

            // Attribute for BlockType
            //   BlockType: Quick Note
            //   Category: Mobile > Core
            //   Attribute: Note Type
            RockMigrationHelper.DeleteAttribute( "1298F46D-B40F-4318-8584-9B6485E7E765" );

            // Attribute for BlockType
            //   BlockType: My Notes
            //   Category: Mobile > Core
            //   Attribute: Group Notes by Date
            RockMigrationHelper.DeleteAttribute( "07C1824F-7A00-4768-A3CC-571877604425" );

            // Attribute for BlockType
            //   BlockType: My Notes
            //   Category: Mobile > Core
            //   Attribute: Connection Detail Page
            RockMigrationHelper.DeleteAttribute( "70F2581C-9D91-4F21-A315-0890200BAC6E" );

            // Attribute for BlockType
            //   BlockType: My Notes
            //   Category: Mobile > Core
            //   Attribute: Add Connection Page
            RockMigrationHelper.DeleteAttribute( "493046BC-E3BD-49B9-98EE-215398AC9D5D" );

            // Attribute for BlockType
            //   BlockType: My Notes
            //   Category: Mobile > Core
            //   Attribute: Reminder Detail Page
            RockMigrationHelper.DeleteAttribute( "2CA49AB1-8587-46A2-99D5-DED6E5040138" );

            // Attribute for BlockType
            //   BlockType: My Notes
            //   Category: Mobile > Core
            //   Attribute: Person Profile Detail Page
            RockMigrationHelper.DeleteAttribute( "365AA793-A1D6-4767-975E-938BF5BD54DA" );

            // Attribute for BlockType
            //   BlockType: My Notes
            //   Category: Mobile > Core
            //   Attribute: Connection Note Type
            RockMigrationHelper.DeleteAttribute( "809DD5B3-40F3-47D7-9A60-4F003533C669" );

            // Attribute for BlockType
            //   BlockType: My Notes
            //   Category: Mobile > Core
            //   Attribute: Reminder Note Type
            RockMigrationHelper.DeleteAttribute( "3CFB3C92-1A59-4F30-B059-8BFDE87ACEB4" );

            // Attribute for BlockType
            //   BlockType: My Notes
            //   Category: Mobile > Core
            //   Attribute: Person Note Types
            RockMigrationHelper.DeleteAttribute( "D8DD2DC7-C644-4B57-B2D7-D48CE00C1066" );

            // Attribute for BlockType
            //   BlockType: My Notes
            //   Category: Mobile > Core
            //   Attribute: Enable Swipe for Options
            RockMigrationHelper.DeleteAttribute( "56EDE86C-5257-415A-805B-35D0EF128035" );

            // Attribute for BlockType
            //   BlockType: My Notes
            //   Category: Mobile > Core
            //   Attribute: Note Item Template
            RockMigrationHelper.DeleteAttribute( "5F0EB28E-A441-4A50-9EE4-C13E9248F21A" );

            // Attribute for BlockType
            //   BlockType: Streak List
            //   Category: Streaks
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "62A7B695-88E2-4AC8-85F9-6AEF4B9CBBC2" );

            // Attribute for BlockType
            //   BlockType: Streak List
            //   Category: Streaks
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "EFA519FD-6C22-4D37-B92E-D03F40D86D86" );

            // Attribute for BlockType
            //   BlockType: Streak List
            //   Category: Streaks
            //   Attribute: Person Profile Page
            RockMigrationHelper.DeleteAttribute( "3C166023-9503-414E-8B0C-1CC36A8AC7FE" );

            // Attribute for BlockType
            //   BlockType: Streak List
            //   Category: Streaks
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "E1482F4B-80B2-4B66-BF9E-2D2517F02F86" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Prioritize Child Entry
            RockMigrationHelper.DeleteAttribute( "09A07E82-3837-44FF-BF9B-D3408E70ECB4" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk
            //   Category: Check-in
            //   Attribute: Select All Schedules Automatically
            RockMigrationHelper.DeleteAttribute( "B092C9FC-73A7-4D88-91E6-F7899E0E5840" );

            // Attribute for BlockType
            //   BlockType: Check-in Kiosk
            //   Category: Check-in
            //   Attribute: Idle Timeout
            RockMigrationHelper.DeleteAttribute( "23C35D06-7DB0-4BBF-A52D-79B2833E3525" );

            // Delete BlockType 
            //   Name: Quick Note
            //   Category: Mobile > Core
            //   Path: -
            //   EntityType: Quick Note
            RockMigrationHelper.DeleteBlockType( "B7E187C8-6F74-4FBD-8853-5BAC48F5822C" );

            // Delete BlockType 
            //   Name: My Notes
            //   Category: Mobile > Core
            //   Path: -
            //   EntityType: My Notes
            RockMigrationHelper.DeleteBlockType( "9BDE231C-B6A7-4753-BBB8-1531F6362387" );

            // Delete BlockType 
            //   Name: Streak List
            //   Category: Streaks
            //   Path: -
            //   EntityType: Streak List
            RockMigrationHelper.DeleteBlockType( "73EFC838-D5E3-4DBD-B5AF-C3C81D3E7DAF" );

            // Delete BlockType 
            //   Name: System Configuration
            //   Category: Administration
            //   Path: -
            //   EntityType: System Configuration
            RockMigrationHelper.DeleteBlockType( "3855B15B-C903-446A-AE5B-891AB52851CB" );
        }
    }
}
