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
    public partial class CodeGenerated_20221018 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Block 
            //  Block Name: Membership
            //  Page Name: Extended Attributes V1
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "1D1CDEBB-B6AC-4C66-AAF6-E90C78254E91".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership","SectionB1",@"",@"",0,"0AD3F245-95D4-4B87-9776-E4E1AD9B3503"); 

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"This setting filters the list of campuses by type that are displayed in the campus drop-down.", 3, @"", "F5A4EAD3-11E2-4B0E-A6A5-AAB278081E2E" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"This setting filters the list of campuses by statuses that are displayed in the campus drop-down.", 4, @"", "D37A6829-326F-49EE-9218-D1742F54E90A" );

            // Attribute for BlockType
            //   BlockType: Rock Solid Church Sample Data
            //   Category: Examples
            //   Attribute: Process Only Giving Data
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A42E0031-B2B9-403A-845B-9C968D7716A6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Process Only Giving Data", "ProcessOnlyGivingData", "Process Only Giving Data", @"If true, the only giving data will be loaded.", 4, @"False", "45BB2753-01CE-4779-B2D8-B1D8658695ED" );

            // Attribute for BlockType
            //   BlockType: Rock Solid Church Sample Data
            //   Category: Examples
            //   Attribute: Delete Data First
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A42E0031-B2B9-403A-845B-9C968D7716A6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Delete Data First", "DeleteDataFirst", "Delete Data First", @"If true, data will be deleted first.", 5, @"True", "4B164A9B-84D9-4266-A858-47A220A8231D" );

            // Add Block Attribute Value
            //   Block: Membership
            //   BlockType: Attribute Values
            //   Category: CRM > Person Detail
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS
            //   Attribute: Category
            /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue("0AD3F245-95D4-4B87-9776-E4E1AD9B3503","EC43CF32-3BDF-4544-8B6A-CE9208DD7C81",@"e919e722-f895-44a4-b86d-38db8fba1844");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Rock Solid Church Sample Data
            //   Category: Examples
            //   Attribute: Delete Data First
            RockMigrationHelper.DeleteAttribute("4B164A9B-84D9-4266-A858-47A220A8231D");

            // Attribute for BlockType
            //   BlockType: Rock Solid Church Sample Data
            //   Category: Examples
            //   Attribute: Process Only Giving Data
            RockMigrationHelper.DeleteAttribute("45BB2753-01CE-4779-B2D8-B1D8658695ED");

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Campus Statuses
            RockMigrationHelper.DeleteAttribute("D37A6829-326F-49EE-9218-D1742F54E90A");

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Campus Types
            RockMigrationHelper.DeleteAttribute("F5A4EAD3-11E2-4B0E-A6A5-AAB278081E2E");

            // Remove Block
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS
            //  from Page: Extended Attributes V1, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("0AD3F245-95D4-4B87-9776-E4E1AD9B3503");
        }
    }
}
