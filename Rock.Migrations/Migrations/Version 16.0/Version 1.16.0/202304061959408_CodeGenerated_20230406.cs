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
    public partial class CodeGenerated_20230406 : Rock.Migrations.RockMigration
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
            RockMigrationHelper.AddBlock( true, "616A2217-0BCE-432E-8CC2-5B6B10D90869".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership", "SectionB1", @"", @"", 0, "ECDB533F-3DAC-43A3-9B25-734133B5C4F1" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Ethnicity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B215F5FA-410C-4674-8C47-43DC40AF9F67", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Ethnicity", "ShowEthnicity", "Show Ethnicity", @"When enabled the Ethnicity chart will be displayed.", 0, @"false", "866AE171-7C4D-405B-A3EC-37E52BBE447E" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Race
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B215F5FA-410C-4674-8C47-43DC40AF9F67", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Race", "ShowRace", "Show Race", @"When enabled the Race chart will be displayed.", 1, @"false", "31A4CDF6-D617-48FB-A234-017ACEFA08A8" );

            // Add Block Attribute Value
            //   Block: Membership
            //   BlockType: Attribute Values
            //   Category: CRM > Person Detail
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS
            //   Attribute: Category
            /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue( "ECDB533F-3DAC-43A3-9B25-734133B5C4F1", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", @"e919e722-f895-44a4-b86d-38db8fba1844" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Race
            RockMigrationHelper.DeleteAttribute( "31A4CDF6-D617-48FB-A234-017ACEFA08A8" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Ethnicity
            RockMigrationHelper.DeleteAttribute( "866AE171-7C4D-405B-A3EC-37E52BBE447E" );

            // Remove Block
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS
            //  from Page: Extended Attributes V1, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "ECDB533F-3DAC-43A3-9B25-734133B5C4F1" );
        }
    }
}
