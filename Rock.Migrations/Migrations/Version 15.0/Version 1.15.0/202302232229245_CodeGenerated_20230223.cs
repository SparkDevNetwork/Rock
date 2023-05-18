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
    public partial class CodeGenerated_20230223 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Utility.RealTimeDebugger
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Utility.RealTimeDebugger", "Real Time Debugger", "Rock.Blocks.Utility.RealTimeDebugger, Rock.Blocks, Version=1.15.0.12, Culture=neutral, PublicKeyToken=null", false, false, "D7D88034-6C6B-4626-A268-420BB8694BFA" );

            // Add/Update Obsidian Block Type
            //   Name:RealTime Debugger
            //   Category:Utility
            //   EntityType:Rock.Blocks.Utility.RealTimeDebugger
            RockMigrationHelper.UpdateMobileBlockType( "RealTime Debugger", "Provides a simple way to debug RealTime events.", "Rock.Blocks.Utility.RealTimeDebugger", "Utility", "E5FA4818-2E0C-4CC6-95F2-34DCC5B3D8C8" );

            // Add Block 
            //  Block Name: Membership
            //  Page Name: Extended Attributes V1
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "72C6E10C-DB12-4FD5-8F11-3CC92F9F6D66".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership", "SectionB1", @"", @"", 0, "5373DCE8-0E4D-49BB-95DE-B724BCE33474" );

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Reminders
            //   Attribute: Reminder Types Include
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC8DC018-C702-4A23-81BA-DF9DD6008CB6", "C66E6BF9-4A73-4429-ACAD-D94D5E3A89B7", "Reminder Types Include", "ReminderTypesInclude", "Reminder Types Include", @"Select any specific remindeder types to show in this block. Leave all unchecked to show all active reminder types ( except for excluded reminder types ).", 2, @"", "C47F2D19-2507-4973-A0FB-B337C836C7FF" );

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Reminders
            //   Attribute: Reminder Types Exclude
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC8DC018-C702-4A23-81BA-DF9DD6008CB6", "C66E6BF9-4A73-4429-ACAD-D94D5E3A89B7", "Reminder Types Exclude", "ReminderTypesExclude", "Reminder Types Exclude", @"Select group types to exclude from this block. Note that this setting is only effective if 'Reminder Types Include' has no specific group types selected.", 3, @"", "C345ABA3-0AF4-4BBB-AB06-4129D70FBC47" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Secondary Authentication Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "98F57599-2DC3-4022-BE33-14A22C3043E1", "Secondary Authentication Types", "SecondaryAuthenticationTypes", "Secondary Authentication Types", @"The active secondary authorization types that should be displayed as options for authentication.", 11, @"", "65712DDF-97AE-4955-A80A-E9BD4E0995C6" );

            // Add Block Attribute Value
            //   Block: Membership
            //   BlockType: Attribute Values
            //   Category: CRM > Person Detail
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS
            //   Attribute: Category
            /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue( "5373DCE8-0E4D-49BB-95DE-B724BCE33474", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", @"e919e722-f895-44a4-b86d-38db8fba1844" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Secondary Authentication Types
            RockMigrationHelper.DeleteAttribute( "65712DDF-97AE-4955-A80A-E9BD4E0995C6" );

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Reminders
            //   Attribute: Reminder Types Exclude
            RockMigrationHelper.DeleteAttribute( "C345ABA3-0AF4-4BBB-AB06-4129D70FBC47" );

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Reminders
            //   Attribute: Reminder Types Include
            RockMigrationHelper.DeleteAttribute( "C47F2D19-2507-4973-A0FB-B337C836C7FF" );

            // Remove Block
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS
            //  from Page: Extended Attributes V1, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "5373DCE8-0E4D-49BB-95DE-B724BCE33474" );

            // Delete BlockType 
            //   Name: RealTime Debugger
            //   Category: Utility
            //   Path: -
            //   EntityType: Real Time Debugger
            RockMigrationHelper.DeleteBlockType( "E5FA4818-2E0C-4CC6-95F2-34DCC5B3D8C8" );
        }
    }
}
