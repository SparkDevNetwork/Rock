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
    public partial class DataViewResultsBlock : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            BlocksPagesUp();
        }

        /// <summary>
        /// Up Migration for DataViewResults block
        /// </summary>
        private void BlocksPagesUp()
        {
            // Add/Update BlockType Data View Results
            RockMigrationHelper.UpdateBlockType( "Data View Results", "Shows the details of the given data view.", "~/Blocks/Reporting/DataViewResults.ascx", "Reporting", "61CDA12E-A19F-4299-AF3E-4F7E2B8F5866" );

            // Add Block Data View Results to Page: Data Views, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "4011CB37-28AA-46C4-99D5-826F4A9CADF5".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "61CDA12E-A19F-4299-AF3E-4F7E2B8F5866".AsGuid(), "Data View Results", "Main", @"", @"", 2, "59B70442-91A8-4538-9508-14F596C3798E" );

            // update block order for pages with new blocks if the page,zone has multiple blocks

            // Update Order for Page: Data Views,  Zone: Main,  Block: Category Detail
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '5BAD3495-6434-4663-A940-1DAC3AC0B643'" );

            // Update Order for Page: Data Views,  Zone: Main,  Block: Data View Results
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '59B70442-91A8-4538-9508-14F596C3798E'" );

            // Update Order for Page: Data Views,  Zone: Main,  Block: Data Views
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '7868AF5C-6512-4F33-B127-93B159E08A56'" );

            // Attribute for BlockType: Data View Results:Enable Counting Data View Statistics
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "61CDA12E-A19F-4299-AF3E-4F7E2B8F5866", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Counting Data View Statistics", "EnableCountingDataViewStatistics", "Enable Counting Data View Statistics", @"Set this to false to prevent this block from counting data view statistics", 1, @"True", "A315410C-52B3-4F14-AB07-FEE8E663453F" );

            // Attribute for BlockType: Data View Results:Database Timeout
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "61CDA12E-A19F-4299-AF3E-4F7E2B8F5866", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Database Timeout", "DatabaseTimeoutSeconds", "Database Timeout", @"The number of seconds to wait before reporting a database timeout.", 0, @"180", "BDF7A3F9-2087-43E5-9009-F617CCC4CA28" );

            // Block Attribute Value for Data View Results ( Page: Data Views, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "59B70442-91A8-4538-9508-14F596C3798E", "BDF7A3F9-2087-43E5-9009-F617CCC4CA28", @"180" );

            // Block Attribute Value for Data View Results ( Page: Data Views, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "59B70442-91A8-4538-9508-14F596C3798E", "11BF128A-D822-4849-B64A-DA9BC34AA3F3", @"" );

            // Block Attribute Value for Data View Results ( Page: Data Views, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "59B70442-91A8-4538-9508-14F596C3798E", "2069CE9C-3777-44AC-A805-4999E74A9959", @"False" );

            // Block Attribute Value for Data View Results ( Page: Data Views, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "59B70442-91A8-4538-9508-14F596C3798E", "1AFD1572-CB6B-4386-BFD0-C1707C6E57C1", @"True" );

            // Block Attribute Value for Data View Results ( Page: Data Views, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "59B70442-91A8-4538-9508-14F596C3798E", "A315410C-52B3-4F14-AB07-FEE8E663453F", @"False" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            PagesBlocksDown ();
        }

        /// <summary>
        /// Down Migration for DataViewResults block
        /// </summary>
        private void PagesBlocksDown()
        {
            // Enable Counting Data View Statistics Attribute for BlockType: Data View Results
            RockMigrationHelper.DeleteAttribute( "A315410C-52B3-4F14-AB07-FEE8E663453F" );

            // core.EnableDefaultWorkflowLauncher Attribute for BlockType: Data View Results
            RockMigrationHelper.DeleteAttribute( "1AFD1572-CB6B-4386-BFD0-C1707C6E57C1" );

            // core.CustomActionsConfigs Attribute for BlockType: Data View Results
            RockMigrationHelper.DeleteAttribute( "A127D2A4-EA06-4D74-9BE5-B49DAF97AB0A" );

            // Database Timeout Attribute for BlockType: Data View Results
            RockMigrationHelper.DeleteAttribute( "BDF7A3F9-2087-43E5-9009-F617CCC4CA28" );

            // Remove Block: Data View Results, from Page: Data Views, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "59B70442-91A8-4538-9508-14F596C3798E" );

            // Delete BlockType Data View Results
            RockMigrationHelper.DeleteBlockType( "61CDA12E-A19F-4299-AF3E-4F7E2B8F5866" ); // Data View Results
        }
    }
}
