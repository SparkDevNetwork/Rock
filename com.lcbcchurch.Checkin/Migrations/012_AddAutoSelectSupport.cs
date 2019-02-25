// <copyright>
// Copyright by LCBC Church
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Plugin;
namespace com.lcbcchurch.Checkin.Migrations
{
    [MigrationNumber( 12, "1.0.14" )]
    public class AddAuytoSelectSupport : Migration
    {
        public override void Up()
        {
            // Attrib Value for Block:Person Select, Attribute:Auto Select Next Page Page: Person Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "4302646B-F6CD-492D-8850-96B9CA1CEA59", @"f533d1fd-5904-4232-bb2b-432567208fd4" );

            // Attrib for BlockType: Select Select:Auto Select First Page
            RockMigrationHelper.UpdateBlockTypeAttribute("BF35D025-2BE8-4E10-B5F2-94F5E808326E","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Auto Select First Page","FamilyAutoSelectFirstPage","","The first page for each person during family check-in.",13,@"","8FE6374B-7E9E-49C7-A633-4B9B601F12DD");  
            // Attrib for BlockType: Select Select:Auto Select Done Page
            RockMigrationHelper.UpdateBlockTypeAttribute("BF35D025-2BE8-4E10-B5F2-94F5E808326E","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Auto Select Done Page","FamilyAutoSelectDonePage","","The page to navigate to once all people have checked in during family check-in.",14,@"","2ED7931E-CD8C-4F6D-B055-B38941277F2A");  
            // Attrib Value for Block:Select Select, Attribute:Auto Select First Page Page: Pager Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue("96041DBC-A35D-4E7D-8A28-6D4E1E19A775","8FE6374B-7E9E-49C7-A633-4B9B601F12DD",@"f533d1fd-5904-4232-bb2b-432567208fd4");  
            // Attrib Value for Block:Select Select, Attribute:Auto Select Done Page Page: Pager Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue("96041DBC-A35D-4E7D-8A28-6D4E1E19A775","2ED7931E-CD8C-4F6D-B055-B38941277F2A",@"4af7a0e1-e991-4ae5-a2b5-c440f67a2e6a");

            // Attrib for BlockType: Item Tag Select:Auto Select Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("09113783-54B7-4C9F-B626-218B0BA75646","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Auto Select Previous Page","FamilyAutoSelectPreviousPage","","The page to navigate back to if none of the people and schedules have been processed.",13,@"","377B732F-F1A8-498E-88A4-E2AC44595E31");  
            // Attrib for BlockType: Item Tag Select:Auto Select Last Page
            RockMigrationHelper.UpdateBlockTypeAttribute("09113783-54B7-4C9F-B626-218B0BA75646","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Auto Select Last Page","FamilyAutoSelectLastPage","","The last page for each person during family check-in.",14,@"","A17972F1-F8DB-444F-9281-9F1AAF1ADBBA");  
            // Attrib Value for Block:Item Tag Select, Attribute:Auto Select Previous Page Page: Item Tags, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue("503BD55F-ED26-4021-9317-999A414B75C8","377B732F-F1A8-498E-88A4-E2AC44595E31",@"d14154ba-2f2c-41c3-b380-f833252cbb13");  
            // Attrib Value for Block:Item Tag Select, Attribute:Auto Select Last Page Page: Item Tags, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue("503BD55F-ED26-4021-9317-999A414B75C8","A17972F1-F8DB-444F-9281-9F1AAF1ADBBA",@"eb789391-f355-4815-b151-0775bec4e8b6");  
        }
        public override void Down()
        {
        }
    }
}
