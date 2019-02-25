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
    [MigrationNumber( 6, "1.0.14" )]
    public class AddRosterPrintToCheckIn : Migration
    {
        public override void Up()
        {
            //Adds the Current Roster with Codes page link to a button in the manager portion of check-in
            //This migration sets this new button block up and adds it to the default check-in welcome page

            RockMigrationHelper.UpdateBlockType( "Roster Button", "Displays a button to print rosters for location", "~/Plugins/com_bemadev/Checkin/RosterButton.ascx", "com_bemadev > Check-in", "31308E57-85D5-4E93-BFFB-4066EB6FF90D" );
            // Add Block to Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlock( true, "432B615A-75FF-4B14-9C99-3E769F866950","","31308E57-85D5-4E93-BFFB-4066EB6FF90D","Roster Button","Main","","",2,"EF05B416-FD31-4C4E-AE60-AA90C9B0D9DD");

            // Attrib for BlockType: Roster Button:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("31308E57-85D5-4E93-BFFB-4066EB6FF90D","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","01F5204F-4806-4E42-B3CB-F17422476A52");
            // Attrib for BlockType: Roster Button:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("31308E57-85D5-4E93-BFFB-4066EB6FF90D","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","3378C278-6E4A-46B5-A72B-30D23D9059DE");

            // Attrib for BlockType: Roster Button:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("31308E57-85D5-4E93-BFFB-4066EB6FF90D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","68C1E145-01A8-4842-99D3-E91E5DBC2E8A");
            // Attrib for BlockType: Roster Button:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("31308E57-85D5-4E93-BFFB-4066EB6FF90D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","0AA8150B-579F-456D-8BC6-0A16421226D7");

            // Attrib for BlockType: Roster Button:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("31308E57-85D5-4E93-BFFB-4066EB6FF90D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","E785339B-3A10-42A0-9184-235DFEF33615");
            // Attrib for BlockType: Roster Button:Multi-Person First Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("31308E57-85D5-4E93-BFFB-4066EB6FF90D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person First Page (Family Check-in)","MultiPersonFirstPage","","The first page for each person during family check-in.",5,@"","059BB4A2-8F81-4F5D-847E-87FD0D4C4E33");

            // Attrib for BlockType: Roster Button:Multi-Person Last Page  (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("31308E57-85D5-4E93-BFFB-4066EB6FF90D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Last Page  (Family Check-in)","MultiPersonLastPage","","The last page for each person during family check-in.",6,@"","DC541F93-7E99-4C9F-BCB6-7A41BC4BEA25");
            // Attrib for BlockType: Roster Button:Multi-Person Done Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("31308E57-85D5-4E93-BFFB-4066EB6FF90D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Done Page (Family Check-in)","MultiPersonDonePage","","The page to navigate to once all people have checked in during family check-in.",7,@"","1D0AD768-EF42-4C82-937B-CDCCCAA9183C");

            // Attrib Value for Block:Roster Button, Attribute:Workflow Type Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue("EF05B416-FD31-4C4E-AE60-AA90C9B0D9DD","01F5204F-4806-4E42-B3CB-F17422476A52",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d"); 
        }
        public override void Down()
        {

        }
    }
}
