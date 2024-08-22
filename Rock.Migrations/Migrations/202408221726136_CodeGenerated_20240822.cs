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
    public partial class CodeGenerated_20240822 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.TagReport
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.TagReport", "Tag Report", "Rock.Blocks.Core.TagReport, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "A9017995-C914-4EE4-B1BA-A852D162BDD7" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.StreakTypeExclusionList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.StreakTypeExclusionList", "Streak Type Exclusion List", "Rock.Blocks.Engagement.StreakTypeExclusionList, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "7740ECD4-1F20-4DE3-8289-4A4F0AFF0646" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.BenevolenceTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.BenevolenceTypeList", "Benevolence Type List", "Rock.Blocks.Finance.BenevolenceTypeList, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "76B2F803-5259-4FD3-A08D-18E546B2C45E" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.FundraisingList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FundraisingList", "Fundraising List", "Rock.Blocks.Finance.FundraisingList, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "FF4F82EC-2F12-4D60-90B9-6CCC7855B4B3" );

            // Add/Update Obsidian Block Type
            //   Name:Tag Report
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.TagReport
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Tag Report", "Block for viewing entities with a selected tag", "Rock.Blocks.Core.TagReport", "Core", "F140B415-9BB3-4492-844E-5A529517A484" );

            // Add/Update Obsidian Block Type
            //   Name:Streak Type Exclusion List
            //   Category:Streaks
            //   EntityType:Rock.Blocks.Engagement.StreakTypeExclusionList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Streak Type Exclusion List", "Lists all the exclusions for a streak type.", "Rock.Blocks.Engagement.StreakTypeExclusionList", "Streaks", "70A4FBE1-511B-457D-84A3-CF6D5B0E09AE" );

            // Add/Update Obsidian Block Type
            //   Name:Benevolence Type List
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.BenevolenceTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Benevolence Type List", "Block to display the benevolence types.", "Rock.Blocks.Finance.BenevolenceTypeList", "Finance", "F61C0FDF-E8A0-457A-B8AF-42CAC8A18718" );

            // Add/Update Obsidian Block Type
            //   Name:Fundraising List
            //   Category:Fundraising
            //   EntityType:Rock.Blocks.Finance.FundraisingList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Fundraising List", "Lists Fundraising Opportunities (Groups) that have the ShowPublic attribute set to true", "Rock.Blocks.Finance.FundraisingList", "Fundraising", "699ED6D1-E23A-4757-A0A2-83C5406B658A" );

            // Attribute for BlockType
            //   BlockType: Edit Person
            //   Category: CRM > Person Detail
            //   Attribute: Default Mobile SMS Checked
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0A15F28C-4828-4B38-AF66-58AC5BDE48E0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default Mobile SMS Checked", "DefaultMobileSMSChecked", "Default Mobile SMS Checked", @"Determines if the SMS checkbox should be automatically checked when a new mobile phone number is entered.", 6, @"True", "6D9CEF49-C562-4937-B4FD-30069222FD31" );

            // Attribute for BlockType
            //   BlockType: Streak Type Exclusion List
            //   Category: Streaks
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "70A4FBE1-511B-457D-84A3-CF6D5B0E09AE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the streak type exclusion details.", 0, @"", "76B7352A-2712-475B-8FDD-9FB22E561805" );

            // Attribute for BlockType
            //   BlockType: Streak Type Exclusion List
            //   Category: Streaks
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "70A4FBE1-511B-457D-84A3-CF6D5B0E09AE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "BC38AB00-ED3A-4333-9EBB-F0B88B460875" );

            // Attribute for BlockType
            //   BlockType: Streak Type Exclusion List
            //   Category: Streaks
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "70A4FBE1-511B-457D-84A3-CF6D5B0E09AE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "6E9CEB0B-7E2F-4C94-96DC-4B2FDBC1B166" );

            // Attribute for BlockType
            //   BlockType: Benevolence Type List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F61C0FDF-E8A0-457A-B8AF-42CAC8A18718", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the benevolence type details.", 0, @"", "DD448CC3-2B03-4FD8-81F1-7A6359B2A47E" );

            // Attribute for BlockType
            //   BlockType: Benevolence Type List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F61C0FDF-E8A0-457A-B8AF-42CAC8A18718", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "C924384F-0FE4-4740-8F4B-4B43B4EC8EC5" );

            // Attribute for BlockType
            //   BlockType: Benevolence Type List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F61C0FDF-E8A0-457A-B8AF-42CAC8A18718", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "925CA5B0-0551-4AC1-B19B-488A8D0528A5" );

            // Attribute for BlockType
            //   BlockType: Fundraising List
            //   Category: Fundraising
            //   Attribute: Fundraising Opportunity Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "699ED6D1-E23A-4757-A0A2-83C5406B658A", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Fundraising Opportunity Types", "FundraisingOpportunityTypes", "Fundraising Opportunity Types", @"Select which opportunity types are shown, or leave blank to show all", 1, @"", "79E427AD-AABF-42C3-9BA6-D479C0FBE3DB" );

            // Attribute for BlockType
            //   BlockType: Fundraising List
            //   Category: Fundraising
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "699ED6D1-E23A-4757-A0A2-83C5406B658A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the group details.", 2, @"", "EC3707C4-49A4-4991-9BE3-44C2016D88A0" );

            // Attribute for BlockType
            //   BlockType: Fundraising List
            //   Category: Fundraising
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "699ED6D1-E23A-4757-A0A2-83C5406B658A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "Lava Template", @"The lava template to use for the results", 3, @"{% include '~~/Assets/Lava/FundraisingList.lava' %}", "62F5499D-3E57-4B79-B1CE-20A6DBA84580" );

            // Attribute for BlockType
            //   BlockType: Fundraising List
            //   Category: Fundraising
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "699ED6D1-E23A-4757-A0A2-83C5406B658A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "F121EA23-E655-4C2F-9C70-EDDED8E9D367" );

            // Attribute for BlockType
            //   BlockType: Fundraising List
            //   Category: Fundraising
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "699ED6D1-E23A-4757-A0A2-83C5406B658A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "8F9E8C2A-FF77-4C8D-91BC-89D8AE12170E" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Fundraising List
            //   Category: Fundraising
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "8F9E8C2A-FF77-4C8D-91BC-89D8AE12170E" );

            // Attribute for BlockType
            //   BlockType: Fundraising List
            //   Category: Fundraising
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "F121EA23-E655-4C2F-9C70-EDDED8E9D367" );

            // Attribute for BlockType
            //   BlockType: Fundraising List
            //   Category: Fundraising
            //   Attribute: Lava Template
            RockMigrationHelper.DeleteAttribute( "62F5499D-3E57-4B79-B1CE-20A6DBA84580" );

            // Attribute for BlockType
            //   BlockType: Fundraising List
            //   Category: Fundraising
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "EC3707C4-49A4-4991-9BE3-44C2016D88A0" );

            // Attribute for BlockType
            //   BlockType: Fundraising List
            //   Category: Fundraising
            //   Attribute: Fundraising Opportunity Types
            RockMigrationHelper.DeleteAttribute( "79E427AD-AABF-42C3-9BA6-D479C0FBE3DB" );

            // Attribute for BlockType
            //   BlockType: Benevolence Type List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "925CA5B0-0551-4AC1-B19B-488A8D0528A5" );

            // Attribute for BlockType
            //   BlockType: Benevolence Type List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "C924384F-0FE4-4740-8F4B-4B43B4EC8EC5" );

            // Attribute for BlockType
            //   BlockType: Benevolence Type List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "DD448CC3-2B03-4FD8-81F1-7A6359B2A47E" );

            // Attribute for BlockType
            //   BlockType: Streak Type Exclusion List
            //   Category: Streaks
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "6E9CEB0B-7E2F-4C94-96DC-4B2FDBC1B166" );

            // Attribute for BlockType
            //   BlockType: Streak Type Exclusion List
            //   Category: Streaks
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "BC38AB00-ED3A-4333-9EBB-F0B88B460875" );

            // Attribute for BlockType
            //   BlockType: Streak Type Exclusion List
            //   Category: Streaks
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "76B7352A-2712-475B-8FDD-9FB22E561805" );

            // Attribute for BlockType
            //   BlockType: Edit Person
            //   Category: CRM > Person Detail
            //   Attribute: Default Mobile SMS Checked
            RockMigrationHelper.DeleteAttribute( "6D9CEF49-C562-4937-B4FD-30069222FD31" );

            // Delete BlockType 
            //   Name: Fundraising List
            //   Category: Fundraising
            //   Path: -
            //   EntityType: Fundraising List
            RockMigrationHelper.DeleteBlockType( "699ED6D1-E23A-4757-A0A2-83C5406B658A" );

            // Delete BlockType 
            //   Name: Benevolence Type List
            //   Category: Finance
            //   Path: -
            //   EntityType: Benevolence Type List
            RockMigrationHelper.DeleteBlockType( "F61C0FDF-E8A0-457A-B8AF-42CAC8A18718" );

            // Delete BlockType 
            //   Name: Streak Type Exclusion List
            //   Category: Streaks
            //   Path: -
            //   EntityType: Streak Type Exclusion List
            RockMigrationHelper.DeleteBlockType( "70A4FBE1-511B-457D-84A3-CF6D5B0E09AE" );

            // Delete BlockType 
            //   Name: Tag Report
            //   Category: Core
            //   Path: -
            //   EntityType: Tag Report
            RockMigrationHelper.DeleteBlockType( "F140B415-9BB3-4492-844E-5A529517A484" );
        }
    }
}
