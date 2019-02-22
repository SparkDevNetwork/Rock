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
    [MigrationNumber( 10, "1.0.14" )]
    public class ReprintLabels : Migration
    {
        public override void Up()
        {
            // Page: Reprint Label
            RockMigrationHelper.AddPage("CDF2C599-D341-42FD-B7DC-CD402EA96050","8305704F-928D-4379-967A-253E576E0923","Reprint Label","","EF18BD24-CC1D-4602-906A-4030EE756024",""); // Site:Rock Check-in Manager
            RockMigrationHelper.UpdateBlockType( "Reprint Labels", "Used to quickly reprint a child's label", "~/Plugins/com_bemadev/Checkin/ReprintLabel.ascx", "BEMA Services > Check-in", "03321528-2AD6-4F8D-8097-F923DD94532A" );
            // Add Block to Page: Reprint Label, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( true, "EF18BD24-CC1D-4602-906A-4030EE756024","","03321528-2AD6-4F8D-8097-F923DD94532A","Reprint Labels","Main","","",1,"5C14D50D-F10B-4B59-A7F7-B1B6F27A2EAB");   
            // Attrib for BlockType: Reprint Labels:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("03321528-2AD6-4F8D-8097-F923DD94532A","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","B0CC25E3-7F21-4B05-A0FF-133FE0B2DB84");  
            // Attrib for BlockType: Reprint Labels:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("03321528-2AD6-4F8D-8097-F923DD94532A","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","A5BF7D1B-143A-4509-8DA1-2688BC2F381D");  
            // Attrib for BlockType: Reprint Labels:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("03321528-2AD6-4F8D-8097-F923DD94532A","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","26652D54-5C30-4E29-8759-3192A011A8C8");  
            // Attrib for BlockType: Reprint Labels:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("03321528-2AD6-4F8D-8097-F923DD94532A","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","782BB17E-4519-4958-A0AA-201C61922FE8");  
            // Attrib for BlockType: Reprint Labels:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("03321528-2AD6-4F8D-8097-F923DD94532A","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","03186D78-3B7C-49DB-AFB6-E605F8E2705D");  
            // Attrib for BlockType: Reprint Labels:Multi-Person First Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("03321528-2AD6-4F8D-8097-F923DD94532A","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person First Page (Family Check-in)","MultiPersonFirstPage","","The first page for each person during family check-in.",5,@"","6BB5F8B7-E58F-4DEF-A70F-8C57A7EB383B");  
            // Attrib for BlockType: Reprint Labels:Multi-Person Last Page  (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("03321528-2AD6-4F8D-8097-F923DD94532A","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Last Page  (Family Check-in)","MultiPersonLastPage","","The last page for each person during family check-in.",6,@"","F48A7930-8E72-4D2B-85E6-183B1A10B37C");  
            // Attrib for BlockType: Reprint Labels:Multi-Person Done Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("03321528-2AD6-4F8D-8097-F923DD94532A","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Done Page (Family Check-in)","MultiPersonDonePage","","The page to navigate to once all people have checked in during family check-in.",7,@"","558B57E0-66B4-4C07-9881-10E6F81C6A9D");  
            // Attrib Value for Block:Reprint Labels, Attribute:Workflow Type Page: Reprint Label, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue("5C14D50D-F10B-4B59-A7F7-B1B6F27A2EAB","B0CC25E3-7F21-4B05-A0FF-133FE0B2DB84",@"");  
            // Attrib Value for Block:Reprint Labels, Attribute:Workflow Activity Page: Reprint Label, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue("5C14D50D-F10B-4B59-A7F7-B1B6F27A2EAB","A5BF7D1B-143A-4509-8DA1-2688BC2F381D",@"");  
            // Attrib Value for Block:Reprint Labels, Attribute:Home Page Page: Reprint Label, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue("5C14D50D-F10B-4B59-A7F7-B1B6F27A2EAB","26652D54-5C30-4E29-8759-3192A011A8C8",@"432b615a-75ff-4b14-9c99-3e769f866950");  
            // Attrib Value for Block:Reprint Labels, Attribute:Previous Page Page: Reprint Label, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue("5C14D50D-F10B-4B59-A7F7-B1B6F27A2EAB","782BB17E-4519-4958-A0AA-201C61922FE8",@"");  
            // Attrib Value for Block:Reprint Labels, Attribute:Next Page Page: Reprint Label, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue("5C14D50D-F10B-4B59-A7F7-B1B6F27A2EAB","03186D78-3B7C-49DB-AFB6-E605F8E2705D",@"");  
            // Attrib Value for Block:Reprint Labels, Attribute:Multi-Person First Page (Family Check-in) Page: Reprint Label, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue("5C14D50D-F10B-4B59-A7F7-B1B6F27A2EAB","6BB5F8B7-E58F-4DEF-A70F-8C57A7EB383B",@"");  
            // Attrib Value for Block:Reprint Labels, Attribute:Multi-Person Last Page  (Family Check-in) Page: Reprint Label, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue("5C14D50D-F10B-4B59-A7F7-B1B6F27A2EAB","F48A7930-8E72-4D2B-85E6-183B1A10B37C",@"");  
            // Attrib Value for Block:Reprint Labels, Attribute:Multi-Person Done Page (Family Check-in) Page: Reprint Label, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue("5C14D50D-F10B-4B59-A7F7-B1B6F27A2EAB","558B57E0-66B4-4C07-9881-10E6F81C6A9D",@"");  

            RockMigrationHelper.UpdateBlockType( "Reprint Label Button", "Displays a button to print rosters for location", "~/Plugins/com_bemadev/Checkin/ReprintLabelButton.ascx", "BEMA Services > Check-in", "6781F30C-6F34-4E45-93A5-42DD7CD2132D" );
            // Add Block to Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlock( true, "432B615A-75FF-4B14-9C99-3E769F866950","","6781F30C-6F34-4E45-93A5-42DD7CD2132D","Reprint Label Button","Main","","",3,"D7210688-21E5-407A-AACD-A9D7E7544D24");
            // Attrib for BlockType: Reprint Label Button:Reprint Label Page
            RockMigrationHelper.UpdateBlockTypeAttribute("6781F30C-6F34-4E45-93A5-42DD7CD2132D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Reprint Label Page","ReprintLabelPage","","",0,@"","FF39512E-D345-416A-AA83-69A83355B01A");
            // Attrib for BlockType: Reprint Label Button:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute("6781F30C-6F34-4E45-93A5-42DD7CD2132D","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","","The workflow type to activate for check-in",0,@"","EB9C9C0B-B811-43D6-9CE8-23925074983A");
            // Attrib for BlockType: Reprint Label Button:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute("6781F30C-6F34-4E45-93A5-42DD7CD2132D","9C204CD0-1233-41C5-818A-C5DA439445AA","Workflow Activity","WorkflowActivity","","The name of the workflow activity to run on selection.",1,@"","CA33C5AE-963B-430C-83A4-1D8C2C849320");
            // Attrib for BlockType: Reprint Label Button:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute("6781F30C-6F34-4E45-93A5-42DD7CD2132D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Home Page","HomePage","","",2,@"","71976F89-A374-4156-BBB2-4C3CF192E555");
            // Attrib for BlockType: Reprint Label Button:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute("6781F30C-6F34-4E45-93A5-42DD7CD2132D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Previous Page","PreviousPage","","",3,@"","2AEAF491-1241-47B5-8639-E3AEBDB26F90");
            // Attrib for BlockType: Reprint Label Button:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute("6781F30C-6F34-4E45-93A5-42DD7CD2132D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Next Page","NextPage","","",4,@"","69481D3C-079C-4283-9A30-CB4E78BB9CFF");
            // Attrib for BlockType: Reprint Label Button:Multi-Person First Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("6781F30C-6F34-4E45-93A5-42DD7CD2132D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person First Page (Family Check-in)","MultiPersonFirstPage","","The first page for each person during family check-in.",5,@"","565214B3-80B8-4C4F-B7FF-C3649F06BBCC");
            // Attrib for BlockType: Reprint Label Button:Multi-Person Last Page  (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("6781F30C-6F34-4E45-93A5-42DD7CD2132D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Last Page  (Family Check-in)","MultiPersonLastPage","","The last page for each person during family check-in.",6,@"","5E85FA42-F467-49DB-B61F-D85108BC7FED");
            // Attrib for BlockType: Reprint Label Button:Multi-Person Done Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute("6781F30C-6F34-4E45-93A5-42DD7CD2132D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Multi-Person Done Page (Family Check-in)","MultiPersonDonePage","","The page to navigate to once all people have checked in during family check-in.",7,@"","56DAEDFB-0A8A-4533-B06C-D888D409D7D6");
            // Attrib Value for Block:Reprint Label Button, Attribute:Reprint Label Page Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue("D7210688-21E5-407A-AACD-A9D7E7544D24","FF39512E-D345-416A-AA83-69A83355B01A",@"ef18bd24-cc1d-4602-906a-4030ee756024");
            // Attrib Value for Block:Reprint Label Button, Attribute:Workflow Type Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue("D7210688-21E5-407A-AACD-A9D7E7544D24","EB9C9C0B-B811-43D6-9CE8-23925074983A",@"");
            // Attrib Value for Block:Reprint Label Button, Attribute:Workflow Activity Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue("D7210688-21E5-407A-AACD-A9D7E7544D24","CA33C5AE-963B-430C-83A4-1D8C2C849320",@"");
            // Attrib Value for Block:Reprint Label Button, Attribute:Home Page Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue("D7210688-21E5-407A-AACD-A9D7E7544D24","71976F89-A374-4156-BBB2-4C3CF192E555",@"");
            // Attrib Value for Block:Reprint Label Button, Attribute:Previous Page Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue("D7210688-21E5-407A-AACD-A9D7E7544D24","2AEAF491-1241-47B5-8639-E3AEBDB26F90",@"");
            // Attrib Value for Block:Reprint Label Button, Attribute:Next Page Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue("D7210688-21E5-407A-AACD-A9D7E7544D24","69481D3C-079C-4283-9A30-CB4E78BB9CFF",@"");
            // Attrib Value for Block:Reprint Label Button, Attribute:Multi-Person First Page (Family Check-in) Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue("D7210688-21E5-407A-AACD-A9D7E7544D24","565214B3-80B8-4C4F-B7FF-C3649F06BBCC",@"");
            // Attrib Value for Block:Reprint Label Button, Attribute:Multi-Person Last Page  (Family Check-in) Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue("D7210688-21E5-407A-AACD-A9D7E7544D24","5E85FA42-F467-49DB-B61F-D85108BC7FED",@"");
            // Attrib Value for Block:Reprint Label Button, Attribute:Multi-Person Done Page (Family Check-in) Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue("D7210688-21E5-407A-AACD-A9D7E7544D24","56DAEDFB-0A8A-4533-B06C-D888D409D7D6",@"");  
        }
        public override void Down()
        {

        }
    }
}
