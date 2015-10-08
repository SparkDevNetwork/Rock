// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class BenevolencePages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Page: Benevolence Detail
            RockMigrationHelper.AddPage( "142627AE-6590-48E3-BFCA-3669260B8CF2", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Benevolence", "", "D893CCCC-368A-42CF-B36E-69991128F016", "" ); // Site:Rock RMS		
            RockMigrationHelper.AddPage( "D893CCCC-368A-42CF-B36E-69991128F016", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Benevolence Detail", "", "6DC7BAED-CA01-4703-B679-EC81143CDEDD", "" ); // Site:Rock RMS		
            RockMigrationHelper.AddBlock( "6DC7BAED-CA01-4703-B679-EC81143CDEDD", "", "34275D0E-BC7E-4A9C-913E-623D086159A1", "Benevolence Request Detail", "Main", "", "", 0, "596CE410-99BF-420F-A86E-CFFDF0BB45F3" );		
		
            // Page: Benevolence List		 
            RockMigrationHelper.AddBlock( "D893CCCC-368A-42CF-B36E-69991128F016", "", "3131C55A-8753-435F-85F3-DF777EFBD1C8", "Benevolence Request List", "Main", "", "", 0, "76519A99-2E29-4481-95B8-DCFF8E3225A1" );		
            RockMigrationHelper.AddBlockAttributeValue( "76519A99-2E29-4481-95B8-DCFF8E3225A1", "E2C90243-A79A-4DAD-9301-07F6DF095CDB", @"6dc7baed-ca01-4703-b679-ec81143cdedd" ); // Detail Page		
            RockMigrationHelper.AddBlockAttributeValue( "76519A99-2E29-4481-95B8-DCFF8E3225A1", "576E31E0-EE40-4A89-93AE-5CCF1F45D21F", @"26e7148c-2059-4f45-bcfe-32230a12f0dc" ); // Case Worker Group

            Sql(@"UPDATE [Page]
	            SET [IconCssClass] = 'fa fa-paste'
	            WHERE [Guid] IN  ('D893CCCC-368A-42CF-B36E-69991128F016', '6DC7BAED-CA01-4703-B679-EC81143CDEDD')");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "596CE410-99BF-420F-A86E-CFFDF0BB45F3" );
            RockMigrationHelper.DeletePage( "6DC7BAED-CA01-4703-B679-EC81143CDEDD" ); //  Page: Benevolence Detail

            RockMigrationHelper.DeleteBlock( "76519A99-2E29-4481-95B8-DCFF8E3225A1" );
            RockMigrationHelper.DeletePage( "D893CCCC-368A-42CF-B36E-69991128F016" ); //  Page: Benevolence
        }
    }
}
