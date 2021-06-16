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
    public partial class CodeGenerated_0525 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update EntityType Rock.Blocks.Types.Mobile.Groups.AddToGroup
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Types.Mobile.Groups.AddToGroup", "Add To Group", "Rock.Blocks.Types.Mobile.Groups.AddToGroup, Rock, Version=1.12.4.1, Culture=neutral, PublicKeyToken=null", false, false, Rock.SystemGuid.EntityType.MOBILE_GROUPS_GROUP_ADD_TO_GROUP );

            // Add/Update Mobile Block Type:Add To Group
            RockMigrationHelper.UpdateMobileBlockType("Add To Group", "Adds the current person to a group passed by query string parameter.", "Rock.Blocks.Types.Mobile.Groups.AddToGroup", "Mobile > Groups", "8A42E4FA-9FE1-493C-B6D8-7A766D96E912");

            // Attribute for BlockType: Add To Group:Save Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A42E4FA-9FE1-493C-B6D8-7A766D96E912", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Save Button Text", "SaveButtonText", "Save Button Text", @"The text to display in the save button.", 7, @"Save", "A7BC77F1-41AF-4360-B08C-3F0E5B2D047A" );

            // Attribute for BlockType: Add To Group:Family Label Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A42E4FA-9FE1-493C-B6D8-7A766D96E912", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Label Text", "FamilyLabelText", "Family Label Text", @"The label to display above other family members.", 8, @"Who Else Will Be Joining You?", "557ECA74-AE25-4182-8D84-FF8BC2103B1F" );

            // Attribute for BlockType: Add To Group:Show Family Members
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A42E4FA-9FE1-493C-B6D8-7A766D96E912", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Family Members", "ShowFamilyMembers", "Show Family Members", @"If the person is logged in then family members will also be shown as options to join group.", 0, @"True", "A17ACFC3-1DA2-4970-9CF9-E8096B33686A" );

            // Attribute for BlockType: Add To Group:Mobile Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A42E4FA-9FE1-493C-B6D8-7A766D96E912", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Mobile Phone", "MobilePhoneVisibility", "Mobile Phone", @"Determines if the Mobile Phone field should be hidden, optional or required.", 1, @"1", "A0B73F41-364C-41B7-9D3D-5401D457C2C3" );

            // Attribute for BlockType: Add To Group:Email Address
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A42E4FA-9FE1-493C-B6D8-7A766D96E912", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Email Address", "EmailAddressVisibility", "Email Address", @"Determines if the Email field should be hidden, optional or required.", 2, @"1", "2BC9774F-9CA1-4DA7-ADD5-A90F8588F814" );

            // Attribute for BlockType: Add To Group:Add As Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A42E4FA-9FE1-493C-B6D8-7A766D96E912", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Add As Status", "AddAsStatus", "Add As Status", @"The member status to add new members to the group with. Group and Role capacities will be checked if set to Active.", 4, @"1", "6A293ACD-7F6C-4F55-9451-0A8BF3ADF077" );

            // Attribute for BlockType: Add To Group:Default Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A42E4FA-9FE1-493C-B6D8-7A766D96E912", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Default Connection Status", "DefaultConnectionStatus", "Default Connection Status", @"The connection status to use for new individuals.", 5, @"368DD475-242C-49C4-A42C-7278BE690CC2", "39DF8349-D49A-4FDA-9D9A-81F421AE0168" );

            // Attribute for BlockType: Add To Group:Default Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A42E4FA-9FE1-493C-B6D8-7A766D96E912", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Default Record Status", "DefaultRecordStatus", "Default Record Status", @"The record status to use for new individuals.", 6, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "DA816A40-84DC-4DDD-BA23-572336C164FC" );

            // Attribute for BlockType: Add To Group:Group Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A42E4FA-9FE1-493C-B6D8-7A766D96E912", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Group Types", "GroupTypes", "Group Types", @"Determines which group types are allowed when adding people to a group from this block.", 3, @"8400497B-C52F-40AE-A529-3FCCB9587101", "331E7628-20CF-4979-8FB9-0F28DD91BD63" );

            // Attribute for BlockType: Add To Group:Workflow Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A42E4FA-9FE1-493C-B6D8-7A766D96E912", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "Workflow Type", @"The workflow to launch for each group member that was added. The Group Member object will be passed as the Entity. The primary person will be passed as the workflow initiator.", 10, @"", "4939D380-8947-4B75-9CF2-47490EA26B1A" );

            // Attribute for BlockType: Add To Group:Completed Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A42E4FA-9FE1-493C-B6D8-7A766D96E912", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completed Template", "CompletedTemplate", "Completed Template", @"The content to display after the user has been added to the group.", 9, @"<Label Text=""Thank you for your interest. You have been added to {{ Group.Name }}"" />", "CA828A3D-FD7B-42CD-951D-5A834B77517C" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
            // Workflow Type Attribute for BlockType: Add To Group
            RockMigrationHelper.DeleteAttribute("4939D380-8947-4B75-9CF2-47490EA26B1A");

            // Completed Template Attribute for BlockType: Add To Group
            RockMigrationHelper.DeleteAttribute("CA828A3D-FD7B-42CD-951D-5A834B77517C");

            // Family Label Text Attribute for BlockType: Add To Group
            RockMigrationHelper.DeleteAttribute("557ECA74-AE25-4182-8D84-FF8BC2103B1F");

            // Save Button Text Attribute for BlockType: Add To Group
            RockMigrationHelper.DeleteAttribute("A7BC77F1-41AF-4360-B08C-3F0E5B2D047A");

            // Default Record Status Attribute for BlockType: Add To Group
            RockMigrationHelper.DeleteAttribute("DA816A40-84DC-4DDD-BA23-572336C164FC");

            // Default Connection Status Attribute for BlockType: Add To Group
            RockMigrationHelper.DeleteAttribute("39DF8349-D49A-4FDA-9D9A-81F421AE0168");

            // Add As Status Attribute for BlockType: Add To Group
            RockMigrationHelper.DeleteAttribute("6A293ACD-7F6C-4F55-9451-0A8BF3ADF077");

            // Group Types Attribute for BlockType: Add To Group
            RockMigrationHelper.DeleteAttribute("331E7628-20CF-4979-8FB9-0F28DD91BD63");

            // Email Address Attribute for BlockType: Add To Group
            RockMigrationHelper.DeleteAttribute("2BC9774F-9CA1-4DA7-ADD5-A90F8588F814");

            // Mobile Phone Attribute for BlockType: Add To Group
            RockMigrationHelper.DeleteAttribute("A0B73F41-364C-41B7-9D3D-5401D457C2C3");

            // Show Family Members Attribute for BlockType: Add To Group
            RockMigrationHelper.DeleteAttribute("A17ACFC3-1DA2-4970-9CF9-E8096B33686A");

            // Delete BlockType Add To Group
            RockMigrationHelper.DeleteBlockType("8A42E4FA-9FE1-493C-B6D8-7A766D96E912"); // Add To Group

            // Delete the EntityType Add To Group
            RockMigrationHelper.DeleteEntityType( Rock.SystemGuid.EntityType.MOBILE_GROUPS_GROUP_ADD_TO_GROUP );
        }
    }
}
