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
    public partial class AddRestKeyAdmin : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add a defined value for the Rest User record type
            RockMigrationHelper.AddDefinedValue_pre20140819( "26BE73A6-A9C5-4E94-AE00-3AFDCF8C9275", "RestUser", "Rest User Record", "E2261A84-831D-4234-9BE0-4D628BBE751E", true );

            // Add the rest key list page
            RockMigrationHelper.AddPage( "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Rest Keys", "A list of REST API Keys", "881AB1C2-4E00-4A73-80CC-9886B3717A20", "fa fa-key" );

            // Add the rest key list block type
            RockMigrationHelper.AddBlockType( "Rest Key List", "Lists the REST API Keys", "~/Blocks/Security/RestKeyList.ascx", "Security", "7FAA46B5-C70D-4A77-9BBF-80DA4431AF13" );

            // Add the rest key list block
            RockMigrationHelper.AddBlock( "881AB1C2-4E00-4A73-80CC-9886B3717A20", "", "7FAA46B5-C70D-4A77-9BBF-80DA4431AF13", "Rest Key List", "Main", "", "", 0, "A8AEA50D-DF92-46D1-8A42-512C0EC5179E" );

            // Add the rest key detail page
            RockMigrationHelper.AddPage( "881AB1C2-4E00-4A73-80CC-9886B3717A20", "195BCD57-1C10-4969-886F-7324B6287B75", "Rest Key Detail", "Detailed information about this REST API Key", "594692AA-5647-4F9A-9488-AADB990FDE56", "fa fa-key" );

            // Add the rest key detail block type
            RockMigrationHelper.AddBlockType( "Rest Key Detail", "Used for viewing and editing REST API key information", "~/Blocks/Security/RestKeyDetail.ascx", "Security", "C3989D26-69C4-4E80-B069-60A157D467BB" );

            // Add the rest key detail block
            RockMigrationHelper.AddBlock( "594692AA-5647-4F9A-9488-AADB990FDE56", "", "C3989D26-69C4-4E80-B069-60A157D467BB", "Rest Key Detail", "Main", "", "", 0, "8457C5EE-C6A4-4DEA-9025-5788A0587C96" );

            // Create the Detail Page attribute on the Rest Key List page
            RockMigrationHelper.AddBlockTypeAttribute( "7FAA46B5-C70D-4A77-9BBF-80DA4431AF13", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, "", "8ACB614A-BF2E-44B2-9F37-F7A113D7C9CA" );

            // Set "Detail Page" attribute on Rest Key List page to the Rest Key Detail page
            RockMigrationHelper.AddBlockAttributeValue( "A8AEA50D-DF92-46D1-8A42-512C0EC5179E", "8ACB614A-BF2E-44B2-9F37-F7A113D7C9CA", "594692AA-5647-4F9A-9488-AADB990FDE56" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlockAttributeValue( "A8AEA50D-DF92-46D1-8A42-512C0EC5179E", "8ACB614A-BF2E-44B2-9F37-F7A113D7C9CA" );
            RockMigrationHelper.DeleteAttribute( "8ACB614A-BF2E-44B2-9F37-F7A113D7C9CA" );
            RockMigrationHelper.DeleteBlock( "8457C5EE-C6A4-4DEA-9025-5788A0587C96" );
            RockMigrationHelper.DeleteBlockType( "C3989D26-69C4-4E80-B069-60A157D467BB" );
            RockMigrationHelper.DeletePage( "594692AA-5647-4F9A-9488-AADB990FDE56" );
            RockMigrationHelper.DeleteBlock( "A8AEA50D-DF92-46D1-8A42-512C0EC5179E" );
            RockMigrationHelper.DeleteBlockType( "7FAA46B5-C70D-4A77-9BBF-80DA4431AF13" );
            RockMigrationHelper.DeletePage( "881AB1C2-4E00-4A73-80CC-9886B3717A20" );
            RockMigrationHelper.DeleteDefinedValue( "E2261A84-831D-4234-9BE0-4D628BBE751E" );
        }
    }
}
