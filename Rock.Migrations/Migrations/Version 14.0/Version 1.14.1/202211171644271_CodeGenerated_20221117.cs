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
    public partial class CodeGenerated_20221117 : Rock.Migrations.RockMigration
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
            RockMigrationHelper.AddBlock( true, "9CD6EE22-8F7C-4CD6-B860-1EF8BCF04513".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership","SectionB1",@"",@"",0,"0D8120A2-AF66-4425-8908-7F932063F1E8"); 

            // Attribute for BlockType
            //   BlockType: Group Scheduler
            //   Category: Group Scheduling
            //   Attribute: Disallow Group Selection If Specified
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "37D43C21-1A4D-4B13-9555-EF0B7304EB8A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disallow Group Selection If Specified", "DisallowGroupSelectionIfSpecified", "Disallow Group Selection If Specified", @"When enabled it will hide the group picker if there is a GroupId in the query string.", 1, @"False", "DB6D9229-30DD-44B1-9076-C5E605737854" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Schedule List Format
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Schedule List Format", "ScheduleListFormat", "Schedule List Format", @"", 17, @"1", "FEA1002E-2D8D-4263-AB7D-EB14115D9609" );

            // Add Block Attribute Value
            //   Block: Membership
            //   BlockType: Attribute Values
            //   Category: CRM > Person Detail
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS
            //   Attribute: Category
            /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue("0D8120A2-AF66-4425-8908-7F932063F1E8","EC43CF32-3BDF-4544-8B6A-CE9208DD7C81",@"e919e722-f895-44a4-b86d-38db8fba1844");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Schedule List Format
            RockMigrationHelper.DeleteAttribute("FEA1002E-2D8D-4263-AB7D-EB14115D9609");

            // Attribute for BlockType
            //   BlockType: Group Scheduler
            //   Category: Group Scheduling
            //   Attribute: Disallow Group Selection If Specified
            RockMigrationHelper.DeleteAttribute("DB6D9229-30DD-44B1-9076-C5E605737854");

            // Remove Block
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS
            //  from Page: Extended Attributes V1, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("0D8120A2-AF66-4425-8908-7F932063F1E8");
        }
    }
}
