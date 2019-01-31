// <copyright>
// Copyright by Central Christian Church
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
    [MigrationNumber( 3, "1.0.14" )]
    public class OverrideButtonAdd : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.UpdateBlockType( "Override", "Override button for changing classrooms without a PIN", "~/Plugins/com_bemadev/Checkin/Override.ascx", "com_bemadev > Check-in", "6B3F9708-16A0-4D01-B1D0-02FAD523B00F" );
            // Add Block to Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlock( true, "432B615A-75FF-4B14-9C99-3E769F866950","","6B3F9708-16A0-4D01-B1D0-02FAD523B00F","Override","Main","","",1,"FB5C56AF-366A-4988-80B0-AE507744C657");

            // Attrib for BlockType: Override:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("6B3F9708-16A0-4D01-B1D0-02FAD523B00F","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","61CA26C0-BC7B-4C99-BBCF-557A5C451808");
            // Attrib for BlockType: Override:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("6B3F9708-16A0-4D01-B1D0-02FAD523B00F","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","7F9D96B4-B667-454F-93DB-8806841E88BC");
            // Attrib for BlockType: Override:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("6B3F9708-16A0-4D01-B1D0-02FAD523B00F","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","4E018CB8-8D5D-45EB-B257-EC5D36377CA7");
            // Attrib for BlockType: Override:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("6B3F9708-16A0-4D01-B1D0-02FAD523B00F","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","16F1CDF4-E11D-4611-866F-163AB00D4D69");
            // Attrib for BlockType: Override:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("6B3F9708-16A0-4D01-B1D0-02FAD523B00F","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","7E4CF473-A9C3-451D-AD69-19A2D834C0A0");

            // Attrib Value for Block:Override, Attribute:Workflow Type Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue("FB5C56AF-366A-4988-80B0-AE507744C657","61CA26C0-BC7B-4C99-BBCF-557A5C451808",@"a0bbc045-00e5-4485-88cb-69a73ac7c78d");
            // Attrib Value for Block:Override, Attribute:Workflow Activity Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue("FB5C56AF-366A-4988-80B0-AE507744C657","7F9D96B4-B667-454F-93DB-8806841E88BC",@"");
            // Attrib Value for Block:Override, Attribute:Home Page Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue("FB5C56AF-366A-4988-80B0-AE507744C657","4E018CB8-8D5D-45EB-B257-EC5D36377CA7",@"432b615a-75ff-4b14-9c99-3e769f866950");
            // Attrib Value for Block:Override, Attribute:Previous Page Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue("FB5C56AF-366A-4988-80B0-AE507744C657","16F1CDF4-E11D-4611-866F-163AB00D4D69",@"7b7207d0-b905-4836-800e-a24ddc6fe445");
            // Attrib Value for Block:Override, Attribute:Next Page Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue("FB5C56AF-366A-4988-80B0-AE507744C657","7E4CF473-A9C3-451D-AD69-19A2D834C0A0",@"d47858c0-0e6e-46dc-ae99-8ec84ba5f45f");
        }
        public override void Down()
        {
           
        }
    }
}
