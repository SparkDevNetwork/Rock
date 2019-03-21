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
    [MigrationNumber( 13, "1.0.14" )]
    public class ReprintLabelFix : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.UpdateBlockType( "Reprint Label Client", "Used if the device prints from the client", "~/Plugins/com_bemadev/Checkin/ReprintLabelClient.ascx", "BEMA Services > Check-in", "FEBA6349-7C21-462D-B834-3505BFAE2A26" );
            // Add Block to Page: Reprint Label, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( true, "EF18BD24-CC1D-4602-906A-4030EE756024", "", "FEBA6349-7C21-462D-B834-3505BFAE2A26", "Reprint Label Client", "Main", "", "", 2, "29A557F4-F698-4CF0-92E1-03C790B7967D" );
            // Attrib for BlockType: Reprint Label Client:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "FEBA6349-7C21-462D-B834-3505BFAE2A26", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "", "The workflow type to activate for check-in", 0, @"", "2EDDBD15-0897-43A4-BD16-3CE33F761F74" );
            // Attrib for BlockType: Reprint Label Client:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute( "FEBA6349-7C21-462D-B834-3505BFAE2A26", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the workflow activity to run on selection.", 1, @"", "AE680D84-7950-45A3-95A0-2616177A2EC9" );
            // Attrib for BlockType: Reprint Label Client:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "FEBA6349-7C21-462D-B834-3505BFAE2A26", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 2, @"", "2FC73DFF-DEBF-4A55-8184-E499E8C15F67" );
            // Attrib for BlockType: Reprint Label Client:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "FEBA6349-7C21-462D-B834-3505BFAE2A26", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 3, @"", "2A81794C-C104-40D0-BBC6-CB199D106507" );
            // Attrib for BlockType: Reprint Label Client:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "FEBA6349-7C21-462D-B834-3505BFAE2A26", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 4, @"", "F48EA93B-54C7-43D7-9E19-6CA94FA8A649" );
            // Attrib for BlockType: Reprint Label Client:Multi-Person First Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute( "FEBA6349-7C21-462D-B834-3505BFAE2A26", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person First Page (Family Check-in)", "MultiPersonFirstPage", "", "The first page for each person during family check-in.", 5, @"", "36149BD5-51BB-419E-B9E1-68536CEDEFEA" );
            // Attrib for BlockType: Reprint Label Client:Multi-Person Last Page  (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute( "FEBA6349-7C21-462D-B834-3505BFAE2A26", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person Last Page  (Family Check-in)", "MultiPersonLastPage", "", "The last page for each person during family check-in.", 6, @"", "00EB5C99-7A3D-494E-9EBD-63EB3A11C9FC" );
            // Attrib for BlockType: Reprint Label Client:Multi-Person Done Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute( "FEBA6349-7C21-462D-B834-3505BFAE2A26", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person Done Page (Family Check-in)", "MultiPersonDonePage", "", "The page to navigate to once all people have checked in during family check-in.", 7, @"", "B33E44EA-D2BD-4E4D-960B-F18A6246697E" );
        }
        public override void Down()
        {
        }
    }
}
