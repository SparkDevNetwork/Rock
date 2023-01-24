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
    public partial class rollup_20230110 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.GroupLocationHistorical", "GroupLocationId", "dbo.GroupLocation");
            AddForeignKey("dbo.GroupLocationHistorical", "GroupLocationId", "dbo.GroupLocation", "Id", cascadeDelete: true);

            NewPagesBlocksAttributesUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.GroupLocationHistorical", "GroupLocationId", "dbo.GroupLocation");
            AddForeignKey("dbo.GroupLocationHistorical", "GroupLocationId", "dbo.GroupLocation", "Id");

            NewPagesBlocksAttributesDown();
        }

        private void NewPagesBlocksAttributesUp()
        {
            // Add Block 
            //  Block Name: Membership
            //  Page Name: Extended Attributes V1
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "51E28524-FC24-44B4-9122-7D7C4C6E67D1".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership","SectionB1",@"",@"",0,"3A73D635-C736-45F4-BE4C-C13F5054D679"); 

            // Attribute for BlockType
            //   BlockType: Time Select
            //   Category: Check-in
            //   Attribute: No Check-in Options Message
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Check-in Options Message", "NoCheckinOptionsMessage", "No Check-in Options Message", @"Message to display when there are not any schedule times after a person is selected. Use {0} for person's name", 6, @"Sorry, there are currently not any available times that {0} can check into.", "02815B66-E474-4FFB-87C6-16A9CE5ACAFD" );

            // Attribute for BlockType
            //   BlockType: Group Member Detail
            //   Category: Groups
            //   Attribute: Schedule List Format
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAE2E5C3-9279-4AB0-9682-F4D19519D678", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Schedule List Format", "ScheduleListFormat", "Schedule List Format", @"", 11, @"1", "86999250-9EBD-4532-93D3-BD1E8530B276" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4CCC45A5-4AB9-4A36-BF8D-A6E316790004", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "IncludedCampusTypes", "Campus Types", @"Set this to limit campuses by campus type.", 7, @"", "CD047DCF-8EF4-40EB-BDAB-09D8A8BB76C4" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4CCC45A5-4AB9-4A36-BF8D-A6E316790004", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "IncludedCampusStatuses", "Campus Statuses", @"Set this to limit campuses by campus status.", 8, @"", "C8F6AF26-360A-4BB3-B1C7-713B253952A6" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4CCC45A5-4AB9-4A36-BF8D-A6E316790004", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "AccountsToDisplay", "Accounts", @"The accounts to display. If Account Campus mapping logic is enabled and the account has a child account for the selected campus, the child account for that campus will be used.", 13, @"", "1F506E35-77C0-4AE2-BC6C-730E6A13011E" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Payment Comment Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4CCC45A5-4AB9-4A36-BF8D-A6E316790004", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Payment Comment Template", "PaymentCommentTemplate", "Payment Comment Template", @"The comment to include with the payment transaction when sending to Gateway. <span class='tip tip-lava'></span>", 14, @"", "F644AA6B-A828-4293-AEE4-55FDCA29D400" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Ask for Campus if Known
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4CCC45A5-4AB9-4A36-BF8D-A6E316790004", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Ask for Campus if Known", "AskForCampusIfKnown", "Ask for Campus if Known", @"If the campus for the person is already known, should the campus still be prompted for?", 5, @"True", "2E882AA2-1C01-4AE7-8604-81AA188C893F" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Include Inactive Campuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4CCC45A5-4AB9-4A36-BF8D-A6E316790004", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Inactive Campuses", "IncludeInactiveCampuses", "Include Inactive Campuses", @"Set this to true to include inactive campuses", 6, @"False", "1FCC5C05-3692-4642-833D-136789849BA3" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Enable Multi-Account
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4CCC45A5-4AB9-4A36-BF8D-A6E316790004", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Multi-Account", "EnableMultiAccount", "Enable Multi-Account", @"Should the person be able specify amounts for more than one account?", 9, @"True", "BF331D8A-E872-4E4A-8A44-57121D8B6E63" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Use Account Campus mapping logic.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4CCC45A5-4AB9-4A36-BF8D-A6E316790004", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Account Campus mapping logic.", "UseAccountCampusMappingLogic", "Use Account Campus mapping logic.", @"If enabled, the accounts will be determined as follows:
        <ul>
          <li>If the selected account is not associated with a campus, the Selected Account will be the first matching active child account that is associated with the selected campus.</li>
          <li>If the selected account is not associated with a campus, but there are no active child accounts for the selected campus, the parent account (the one the user sees) will be returned.</li>
          <li>If the selected account is associated with a campus, that account will be returned regardless of campus selection (and it won't use the child account logic)</li>
        <ul>", 14, @"False", "FADA569F-35B5-47B6-8FAC-3C8CB40CB07A" );

            // Add Block Attribute Value
            //   Block: Membership
            //   BlockType: Attribute Values
            //   Category: CRM > Person Detail
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS
            //   Attribute: Category
            /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue("3A73D635-C736-45F4-BE4C-C13F5054D679","EC43CF32-3BDF-4544-8B6A-CE9208DD7C81",@"e919e722-f895-44a4-b86d-38db8fba1844");
        }
    
        private void NewPagesBlocksAttributesDown()
        {
            // Attribute for BlockType
            //   BlockType: Group Member Detail
            //   Category: Groups
            //   Attribute: Schedule List Format
            RockMigrationHelper.DeleteAttribute("86999250-9EBD-4532-93D3-BD1E8530B276");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Payment Comment Template
            RockMigrationHelper.DeleteAttribute("F644AA6B-A828-4293-AEE4-55FDCA29D400");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Use Account Campus mapping logic.
            RockMigrationHelper.DeleteAttribute("FADA569F-35B5-47B6-8FAC-3C8CB40CB07A");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Accounts
            RockMigrationHelper.DeleteAttribute("1F506E35-77C0-4AE2-BC6C-730E6A13011E");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Enable Multi-Account
            RockMigrationHelper.DeleteAttribute("BF331D8A-E872-4E4A-8A44-57121D8B6E63");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Campus Statuses
            RockMigrationHelper.DeleteAttribute("C8F6AF26-360A-4BB3-B1C7-713B253952A6");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Campus Types
            RockMigrationHelper.DeleteAttribute("CD047DCF-8EF4-40EB-BDAB-09D8A8BB76C4");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Include Inactive Campuses
            RockMigrationHelper.DeleteAttribute("1FCC5C05-3692-4642-833D-136789849BA3");

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Ask for Campus if Known
            RockMigrationHelper.DeleteAttribute("2E882AA2-1C01-4AE7-8604-81AA188C893F");

            // Attribute for BlockType
            //   BlockType: Time Select
            //   Category: Check-in
            //   Attribute: No Check-in Options Message
            RockMigrationHelper.DeleteAttribute("02815B66-E474-4FFB-87C6-16A9CE5ACAFD");

            // Remove Block
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS
            //  from Page: Extended Attributes V1, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("3A73D635-C736-45F4-BE4C-C13F5054D679");
        }
    }
}
