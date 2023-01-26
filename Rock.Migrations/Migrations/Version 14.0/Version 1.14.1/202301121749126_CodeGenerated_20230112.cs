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
    public partial class CodeGenerated_20230112 : Rock.Migrations.RockMigration
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
            RockMigrationHelper.AddBlock( true, "455744D7-D7A9-4631-90EA-FEC09F5C008C".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership","SectionB1",@"",@"",0,"C2DBE1F2-E5E5-45D9-BFE0-BC1B870C32D0"); 

            // Attribute for BlockType
            //   BlockType: Transaction Entry (V2)
            //   Category: Finance
            //   Attribute: Account Header Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Account Header Template", "AccountHeaderTemplate", "Account Header Template", @"The Lava Template to use as the amount input label for each account.", 6, @"{{ Account.PublicName }}", "CBDC8BE5-E822-488D-B9C1-8E821F68FA47" );

            // Attribute for BlockType
            //   BlockType: Live Experience
            //   Category: Mobile > Events
            //   Attribute: Keep Screen On
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "969EB376-281C-41D8-B7E9-A183DEA751DB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Keep Screen On", "KeepScreenOn", "Keep Screen On", @"Keeps the screen turned on when this page is being used by the application.", 2, @"False", "E7E9D38A-A4ED-4C08-87C8-C68DB32FE9EC" );

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Mobile > Events
            //   Attribute: Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C45BA1C6-CE7F-4C37-82BF-A86D28BB28FE", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 4, @"0C75B833-E710-45AE-B3B2-3FAC97A79BB2", "0A2BEEF0-01E6-434C-89C9-D4FB94C15594" );

            // Add Block Attribute Value
            //   Block: Membership
            //   BlockType: Attribute Values
            //   Category: CRM > Person Detail
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS
            //   Attribute: Category
            /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue("C2DBE1F2-E5E5-45D9-BFE0-BC1B870C32D0","EC43CF32-3BDF-4544-8B6A-CE9208DD7C81",@"e919e722-f895-44a4-b86d-38db8fba1844");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Transaction Entry (V2)
            //   Category: Finance
            //   Attribute: Account Header Template
            RockMigrationHelper.DeleteAttribute("CBDC8BE5-E822-488D-B9C1-8E821F68FA47");

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Mobile > Events
            //   Attribute: Template
            RockMigrationHelper.DeleteAttribute("0A2BEEF0-01E6-434C-89C9-D4FB94C15594");

            // Attribute for BlockType
            //   BlockType: Live Experience
            //   Category: Mobile > Events
            //   Attribute: Keep Screen On
            RockMigrationHelper.DeleteAttribute("E7E9D38A-A4ED-4C08-87C8-C68DB32FE9EC");

            // Remove Block
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS
            //  from Page: Extended Attributes V1, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("C2DBE1F2-E5E5-45D9-BFE0-BC1B870C32D0");
        }
    }
}
