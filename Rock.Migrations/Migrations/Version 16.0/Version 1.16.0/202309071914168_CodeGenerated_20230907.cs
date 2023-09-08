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
    public partial class CodeGenerated_20230907 : Rock.Migrations.RockMigration
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
            RockMigrationHelper.AddBlock( true, "E9E67534-E12D-4696-A240-5BD3D86D1352".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership", "SectionB1", @"", @"", 0, "7B2870BE-C263-40F8-BAA4-9B49D4ECD2EB" );

            // Attribute for BlockType              
            //   BlockType: Group Tree View              
            //   Category: Groups              
            //   Attribute: Limit to Security Role Groups              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimitToSecurityRoleGroups", "Limit to Security Role Groups", @"", 5, @"False", "5F814906-54F1-4D1F-9343-C22E84BB5F60" );

            // Add Block Attribute Value              
            //   Block: Membership              
            //   BlockType: Attribute Values              
            //   Category: CRM > Person Detail              
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS              
            //   Attribute: Category              /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */              
            RockMigrationHelper.AddBlockAttributeValue( "7B2870BE-C263-40F8-BAA4-9B49D4ECD2EB", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", @"e919e722-f895-44a4-b86d-38db8fba1844" );

            RockMigrationHelper.UpdateFieldType( "Color Selector", "", "Rock", "Rock.Field.Types.ColorSelectorFieldType", "B07070F6-B66E-4B83-8DA6-12CF3400A1EC" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType              
            //   BlockType: Group Tree View              
            //   Category: Groups              
            //   Attribute: Limit to Security Role Groups              
            RockMigrationHelper.DeleteAttribute( "5F814906-54F1-4D1F-9343-C22E84BB5F60" );

            // Remove Block              
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS              
            //  from Page: Extended Attributes V1, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "7B2870BE-C263-40F8-BAA4-9B49D4ECD2EB" );
        }
    }
}
