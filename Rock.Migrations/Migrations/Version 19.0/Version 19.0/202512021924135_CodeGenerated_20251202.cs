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
    public partial class CodeGenerated_20251202 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType
            //   BlockType: Group Tree View
            //   Category: Groups
            //   Attribute: Search Results Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Search Results Page", "SearchResultsPage", "Search Results Page", @"If set, this is the page where search results will be shown when using the quick find feature. The selected page must include a Group Search block, and that block should be configured to link back to the appropriate Group Detail page.", 11, @"", "A7B95CB2-7222-4A60-B1DB-ADE8FE4CD9AD" );

            // Attribute for BlockType
            //   BlockType: Financial Batch Detail
            //   Category: Mobile > Finance
            //   Attribute: Currency Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EF68BE-522E-49E0-9BE5-C971433765DE", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Currency Types", "CurrencyTypes", "Currency Types", @"The currency type to show in the scan settings page.", 8, @"", "087A2A62-4B00-492E-8244-030A77F7AD00" );

            // Attribute for BlockType
            //   BlockType: Financial Batch Detail
            //   Category: Mobile > Finance
            //   Attribute: Transaction Source
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EF68BE-522E-49E0-9BE5-C971433765DE", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Transaction Source", "TransactionSource", "Transaction Source", @"The Transaction Source to show in the scan settings page.", 9, @"", "FEC02440-FD56-4341-ABB5-B89EEB0F924A" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Financial Batch Detail
            //   Category: Mobile > Finance
            //   Attribute: Transaction Source
            RockMigrationHelper.DeleteAttribute( "FEC02440-FD56-4341-ABB5-B89EEB0F924A" );

            // Attribute for BlockType
            //   BlockType: Financial Batch Detail
            //   Category: Mobile > Finance
            //   Attribute: Currency Types
            RockMigrationHelper.DeleteAttribute( "087A2A62-4B00-492E-8244-030A77F7AD00" );

            // Attribute for BlockType
            //   BlockType: Group Tree View
            //   Category: Groups
            //   Attribute: Search Results Page
            RockMigrationHelper.DeleteAttribute( "A7B95CB2-7222-4A60-B1DB-ADE8FE4CD9AD" );
        }
    }
}
