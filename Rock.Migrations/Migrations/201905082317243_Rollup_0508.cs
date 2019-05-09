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
    public partial class Rollup_0508 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            AddNewWelcomeBlockSettingsUp();
            UpdateSampleDataBlockSetting();
            FixLavaChartShortcode();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddNewWelcomeBlockSettingsDown();
            CodeGenMigrationsDown();
        }

        /// <summary>
        /// Add Page, Block, and Attribute migrations created by script.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            // Attrib for BlockType: Welcome:Allow Opening and Closing Rooms
            RockMigrationHelper.UpdateBlockTypeAttribute("E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Allow Opening and Closing Rooms","AllowOpeningAndClosingRooms","",@"Determines if opening and closing rooms should be allowed. If not allowed, the locations only show counts and the open/close toggles are not shown.",20,@"True","3F4C48A2-C311-425E-82E9-9B8935CE0550");
            // Attrib for BlockType: Scheduled Transaction Edit:Workflow Trigger
            RockMigrationHelper.UpdateBlockTypeAttribute("5171C4E5-7698-453E-9CC8-088D362296DE","1D50399B-0D6E-480B-A71A-E7BD37DD83F0","Workflow Trigger","WorkflowType","",@"Workflow types to trigger when an edit is submitted for a schedule.",13,@"","1570CFD4-1E4D-499F-848D-A7843A23D3CD");
        }

        /// <summary>
        /// Remove Page, Block, and Attribute migrations created by script.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Scheduled Transaction Edit:Workflow Trigger
            RockMigrationHelper.DeleteAttribute("1570CFD4-1E4D-499F-848D-A7843A23D3CD");
            // Attrib for BlockType: Welcome:Allow Opening and Closing Rooms
            RockMigrationHelper.DeleteAttribute("3F4C48A2-C311-425E-82E9-9B8935CE0550");
        }

        /// <summary>
        /// NA: Add new Welcome block settings
        /// </summary>
        private void AddNewWelcomeBlockSettingsUp()
        {
            // Attrib for BlockType: Welcome:Allow Opening and Closing Rooms
            RockMigrationHelper.UpdateBlockTypeAttribute("E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Allow Opening and Closing Rooms","AllowOpeningAndClosingRooms","",@"Determines if opening and closing rooms should be allowed. If not allowed, the locations only show counts and the open/close toggles are not shown.",20,@"True","59BF23CC-8FFE-4492-B88C-325164A9FCB2");

            // Attrib for BlockType: Welcome:Allow Label Reprinting
            RockMigrationHelper.UpdateBlockTypeAttribute("E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Allow Label Reprinting","AllowLabelReprinting","",@" Determines if reprinting labels should be allowed.",21,@"False","563FF060-D7FD-4704-A4AA-8F4B6D4F75CE");
        }

        /// <summary>
        /// NA: Add new Welcome block settings
        /// </summary>
        private void AddNewWelcomeBlockSettingsDown()
        {
            // Attrib for BlockType: Welcome:Allow Opening and Closing Rooms
            RockMigrationHelper.DeleteAttribute("59BF23CC-8FFE-4492-B88C-325164A9FCB2");

            // Attrib for BlockType: Welcome:Allow Label Reprinting
            RockMigrationHelper.DeleteAttribute("563FF060-D7FD-4704-A4AA-8F4B6D4F75CE");
        }

        /// <summary>
        /// NA: SampleData block setting for v9 DISC converted data-points and new ShowOnWaitlist feature
        /// </summary>
        private void UpdateSampleDataBlockSetting()
        {
            RockMigrationHelper.AddBlockAttributeValue( "34CA1FA0-F8F1-449F-9788-B5E6315DC058", "5E26439E-4E98-45B1-B19B-D5B2F3405963", @"http://storage.rockrms.com/sampledata/sampledata_1_6_0.xml" );
        }

        /// <summary>
        /// SK: Adds linearhorizontal0to100 and fixes several issues with broken functionality of Rock's "Chart" shortcode.
        /// </summary>
        private void FixLavaChartShortcode()
        {
            Sql( MigrationSQL._201905082317243_Rollup_0508_FixLavaChartShortcode );
        }
    }
}
