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

    /// <summary>
    ///
    /// </summary>
    public partial class CodeGenerated_20230518 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            // EntityType:Rock.Blocks.Example.ObsidianGalleryList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Example.ObsidianGalleryList", "Obsidian Gallery List", "Rock.Blocks.Example.ObsidianGalleryList, Rock.Blocks, Version=1.16.0.3, Culture=neutral, PublicKeyToken=null", false, false, "4315FD92-F9F1-4038-ABDC-A2D661B9DEDF" );

            // Add/Update Obsidian Block Type
            // Name:Obsidian Gallery List
            // Category:Example
            // EntityType:Rock.Blocks.Example.ObsidianGalleryList
            RockMigrationHelper.UpdateMobileBlockType( "Obsidian Gallery List", "Demonstrates the various parts of the Obsidian List blocks.", "Rock.Blocks.Example.ObsidianGalleryList", "Example", "121EEC5E-F8AA-4CD8-A61D-9C99C269280E" );

            // Add Block
            // Block Name: Membership
            // Page Name: Extended Attributes V1
            // Layout: -
            // Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "7842F3ED-67A1-4CD2-ABD8-CFC723E43370".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership", "SectionB1", @"", @"", 0, "8C970528-42A5-4AF5-92DE-677AFAF694CC" );

            // Attribute for BlockType
            // BlockType: Group Tree View
            // Category: Groups
            // Attribute: Limit to Security Role Groups
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimitToSecurityRoleGroups", "Limit to Security Role Groups", @"", 5, @"False", "F529A620-4C41-4F72-9031-16F849FB59EA" );

            // Attribute for BlockType
            // BlockType: Utility Payment Entry
            // Category: Finance
            // Attribute: Disable Captcha Support
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4CCC45A5-4AB9-4A36-BF8D-A6E316790004", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Captcha Support", "DisableCaptchaSupport", "Disable Captcha Support", @"If set to 'Yes' the CAPTCHA verification step will not be performed.", 29, @"False", "29ADDCA8-1C21-4C20-86E1-93523D36BD14" );

            // Attribute for BlockType
            // BlockType: Obsidian Gallery List
            // Category: Example
            // Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "121EEC5E-F8AA-4CD8-A61D-9C99C269280E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "5A3E1B4F-A495-46F0-A227-CF509F7A499D" );

            // Attribute for BlockType
            // BlockType: Obsidian Gallery List
            // Category: Example
            // Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "121EEC5E-F8AA-4CD8-A61D-9C99C269280E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "FEF8FD62-A9A4-400A-A26E-E25EC4BAFD99" );

            // Add Block Attribute Value
            // Block: Membership
            // BlockType: Attribute Values
            // Category: CRM > Person Detail
            // Block Location: Page=Extended Attributes V1, Site=Rock RMS
            // Attribute: Category
            /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue( "8C970528-42A5-4AF5-92DE-677AFAF694CC", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", @"e919e722-f895-44a4-b86d-38db8fba1844" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
            // Attribute for BlockType              
            //   BlockType: Obsidian Gallery List              
            //   Category: Example              
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("FEF8FD62-A9A4-400A-A26E-E25EC4BAFD99");
            
            // Attribute for BlockType              
            //   BlockType: Obsidian Gallery List              
            //   Category: Example              
            //   Attribute: core.CustomActionsConfigs             
            RockMigrationHelper.DeleteAttribute("5A3E1B4F-A495-46F0-A227-CF509F7A499D");
            
            // Attribute for BlockType              
            //   BlockType: Group Tree View              
            //   Category: Groups              
            //   Attribute: Limit to Security Role Groups             
            RockMigrationHelper.DeleteAttribute("F529A620-4C41-4F72-9031-16F849FB59EA");
            
            // Attribute for BlockType              
            //   BlockType: Utility Payment Entry              
            //   Category: Finance              
            //   Attribute: Disable Captcha Support             
            RockMigrationHelper.DeleteAttribute("29ADDCA8-1C21-4C20-86E1-93523D36BD14");
            
            // Remove Block              
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS              
            //  from Page: Extended Attributes V1, Site: Rock RMS             
            RockMigrationHelper.DeleteBlock("8C970528-42A5-4AF5-92DE-677AFAF694CC");
            
            // Delete BlockType               
            //   Name: Obsidian Gallery List              
            //   Category: Example              
            //   Path: -              
            //   EntityType: Obsidian Gallery List             
            RockMigrationHelper.DeleteBlockType("121EEC5E-F8AA-4CD8-A61D-9C99C269280E");
        }
    }
}
