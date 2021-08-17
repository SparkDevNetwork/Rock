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
    public partial class ContributionStatementsDetail : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // Remove Block: Financial Statement Template Detail, from Page: Statement Template Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "43EAEEC3-F2AB-4A0D-A125-E9C7943F7157" );

            // Remove Block: Financial Statement Template List, from Page: Statement Templates, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A2022D1F-041C-4DA6-AD96-0C1FB3076703" );

            // Delete Page Statement Template Detail from Site:Rock RMS
            RockMigrationHelper.DeletePage( "DE55FD8A-D24C-4C01-9B7A-BD4E74E362BF" ); //  Page: Statement Template Detail, Layout: Full Width, Site: Rock RMS

            // Delete Page Statement Templates from Site:Rock RMS
            RockMigrationHelper.DeletePage( "B59F9E88-C6A1-463E-8FA0-DB381C617C89" ); //  Page: Statement Templates, Layout: Full Width, Site: Rock RMS

            // Add Page Contribution Statements to Site:Rock RMS
            RockMigrationHelper.AddPage( true, "18C9E5C3-3E28-4AA3-84F6-78CD4EA2DD3C", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Contribution Statements", "", "90723727-56EC-494D-9708-E188869D900C", "" );

            // Add Page Contribution Templates to Site:Rock RMS
            RockMigrationHelper.AddPage( true, "90723727-56EC-494D-9708-E188869D900C", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Contribution Templates", "", "D5269942-0B3B-4447-8EE9-F5DEB7657003", "fa fa-file-invoice-dollar" );

            // Add Page Contribution Template Detail to Site:Rock RMS
            RockMigrationHelper.AddPage( true, "D5269942-0B3B-4447-8EE9-F5DEB7657003", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Contribution Template Detail", "", "D4CB4CE6-FBF9-4FBD-B8C4-08BE022F97D7", "" );

            // Add/Update BlockType Rock Update Legacy
            RockMigrationHelper.UpdateBlockType( "Rock Update Legacy", "Handles checking for and performing upgrades to the Rock system.", "~/Blocks/Core/RockUpdateLegacy.ascx", "Core", "9E8E66FE-7AFC-4AE5-80E9-222C3ACA14DC" );

            // Add Block Contribution Statements Page Menu to Page: Contribution Statements, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "90723727-56EC-494D-9708-E188869D900C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Contribution Statements Page Menu", "Main", @"", @"", 0, "B40F695F-D402-45CA-BE51-B9542F2A5694" );

            // Add Block Financial Statement Template List to Page: Contribution Templates, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "D5269942-0B3B-4447-8EE9-F5DEB7657003".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "65057F07-85D5-4795-91A1-86D8F67A65DC".AsGuid(), "Financial Statement Template List", "Main", @"", @"", 0, "B581205E-866C-4EBA-A1CB-1577B378E476" );

            // Add Block Financial Statement Template Detail to Page: Contribution Template Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "D4CB4CE6-FBF9-4FBD-B8C4-08BE022F97D7".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "78DB9184-97CF-4FC4-BD71-8F3ABE4100BA".AsGuid(), "Financial Statement Template Detail", "Main", @"", @"", 0, "455DB8DE-2195-4B24-93B7-0815818ECA38" );

            // Attribute for BlockType: Attributes:Hide Columns on Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Hide Columns on Grid", "HideColumnsOnGrid", "Hide Columns on Grid", @"The grid columns that should be hidden from the user.", 6, @"", "D11D7EF0-48D2-49E6-9C6E-5C0ED86DD9AC" );

            // Add Block Attribute Value
            //   Block: Contribution Statements Page Menu
            //   BlockType: Page Menu
            //   Block Location: Page=Contribution Statements, Site=Rock RMS
            //   Attribute: Include Current Parameters
            //   Attribute Value: False
            RockMigrationHelper.AddBlockAttributeValue( "B40F695F-D402-45CA-BE51-B9542F2A5694", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );

            // Add Block Attribute Value
            //   Block: Contribution Statements Page Menu
            //   BlockType: Page Menu
            //   Block Location: Page=Contribution Statements, Site=Rock RMS
            //   Attribute: Template
            //   Attribute Value: {% include '~~/Assets/Lava/PageListAsBlocks.lava' %}
            RockMigrationHelper.AddBlockAttributeValue( "B40F695F-D402-45CA-BE51-B9542F2A5694", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" );

            // Add Block Attribute Value
            //   Block: Contribution Statements Page Menu
            //   BlockType: Page Menu
            //   Block Location: Page=Contribution Statements, Site=Rock RMS
            //   Attribute: Root Page
            //   Attribute Value: 90723727-56ec-494d-9708-e188869d900c
            RockMigrationHelper.AddBlockAttributeValue( "B40F695F-D402-45CA-BE51-B9542F2A5694", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"90723727-56ec-494d-9708-e188869d900c" );

            // Add Block Attribute Value
            //   Block: Contribution Statements Page Menu
            //   BlockType: Page Menu
            //   Block Location: Page=Contribution Statements, Site=Rock RMS
            //   Attribute: Number of Levels
            //   Attribute Value: 1
            RockMigrationHelper.AddBlockAttributeValue( "B40F695F-D402-45CA-BE51-B9542F2A5694", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );

            // Add Block Attribute Value
            //   Block: Contribution Statements Page Menu
            //   BlockType: Page Menu
            //   Block Location: Page=Contribution Statements, Site=Rock RMS
            //   Attribute: Include Current QueryString
            //   Attribute Value: False
            RockMigrationHelper.AddBlockAttributeValue( "B40F695F-D402-45CA-BE51-B9542F2A5694", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Add Block Attribute Value
            //   Block: Contribution Statements Page Menu
            //   BlockType: Page Menu
            //   Block Location: Page=Contribution Statements, Site=Rock RMS
            //   Attribute: Is Secondary Block
            //   Attribute Value: False
            RockMigrationHelper.AddBlockAttributeValue( "B40F695F-D402-45CA-BE51-B9542F2A5694", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Add Block Attribute Value
            //   Block: Financial Statement Template List
            //   BlockType: Financial Statement Template List
            //   Block Location: Page=Contribution Templates, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            //   Attribute Value: True
            RockMigrationHelper.AddBlockAttributeValue( "B581205E-866C-4EBA-A1CB-1577B378E476", "AFD65005-BD37-400F-8574-47CAC2EFFFF7", @"True" );

            // Add Block Attribute Value
            //   Block: Financial Statement Template List
            //   BlockType: Financial Statement Template List
            //   Block Location: Page=Contribution Templates, Site=Rock RMS
            //   Attribute: Detail Page
            //   Attribute Value: d4cb4ce6-fbf9-4fbd-b8c4-08be022f97d7
            RockMigrationHelper.AddBlockAttributeValue( "B581205E-866C-4EBA-A1CB-1577B378E476", "3D04EE40-9B9B-4848-B1D8-9831058028A2", @"d4cb4ce6-fbf9-4fbd-b8c4-08be022f97d7" );

            // Add Block Attribute Value
            //   Block: Financial Statement Template List
            //   BlockType: Financial Statement Template List
            //   Block Location: Page=Contribution Templates, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            //   Attribute Value: False
            RockMigrationHelper.AddBlockAttributeValue( "B581205E-866C-4EBA-A1CB-1577B378E476", "99C20A74-A560-4889-AAA0-B1544FE5140C", @"False" );
            RockMigrationHelper.UpdateFieldType( "Captcha", "", "Rock", "Rock.Field.Types.CaptchaFieldType", "22F43337-7177-4064-9D4B-841EAD671678" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Hide Columns on Grid Attribute for BlockType: Attributes
            RockMigrationHelper.DeleteAttribute( "D11D7EF0-48D2-49E6-9C6E-5C0ED86DD9AC" );

            // Remove Block: Financial Statement Template Detail, from Page: Contribution Template Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "455DB8DE-2195-4B24-93B7-0815818ECA38" );

            // Remove Block: Financial Statement Template List, from Page: Contribution Templates, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B581205E-866C-4EBA-A1CB-1577B378E476" );

            // Remove Block: Contribution Statements Page Menu, from Page: Contribution Statements, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B40F695F-D402-45CA-BE51-B9542F2A5694" );

            // Delete BlockType Rock Update Legacy
            RockMigrationHelper.DeleteBlockType( "9E8E66FE-7AFC-4AE5-80E9-222C3ACA14DC" ); // Rock Update Legacy

            // Delete Page Contribution Template Detail from Site:Rock RMS
            RockMigrationHelper.DeletePage( "D4CB4CE6-FBF9-4FBD-B8C4-08BE022F97D7" ); //  Page: Contribution Template Detail, Layout: Full Width, Site: Rock RMS

            // Delete Page Contribution Templates from Site:Rock RMS
            RockMigrationHelper.DeletePage( "D5269942-0B3B-4447-8EE9-F5DEB7657003" ); //  Page: Contribution Templates, Layout: Full Width, Site: Rock RMS

            // Delete Page Contribution Statements from Site:Rock RMS
            RockMigrationHelper.DeletePage( "90723727-56EC-494D-9708-E188869D900C" ); //  Page: Contribution Statements, Layout: Full Width, Site: Rock RMS
        }
    }
}
