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
    public partial class PersonDetailPages : Rock.Migrations.RockMigration2
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage("53CF4CBE-85F9-4A50-87D7-0D72A3FB2892","D65F783D-87A9-4CC9-8110-E83466A0EADB","Add Scheduled Transaction","","B1CA86DC-9890-4D26-8EBD-488044E1B3DD",""); // Site:Rock Internal
            AddPage("53CF4CBE-85F9-4A50-87D7-0D72A3FB2892","D65F783D-87A9-4CC9-8110-E83466A0EADB","Edit Scheduled Transaction","","D360B64F-1267-4518-95CD-99CD5AB87D88",""); // Site:Rock Internal

            AddBlockType("Giving Profile Detail","Edit an existing scheduled transaction.","~/Blocks/Finance/GivingProfileDetail.ascx","5171C4E5-7698-453E-9CC8-088D362296DE");
            AddBlockType( "Stark", "Template block for developers to use to start a new block.", "~/Blocks/Utility/Stark.ascx", "EF4DE627-DD03-4EBA-990B-F2F4CC754548" );

            // Add Block to Page: Groups, Site: Rock Internal
            AddBlock("183B7B7E-105A-4C9A-A4BC-06CD26B7FE6D","","3D7FB6BE-6BBD-49F7-96B4-96310AF3048A","Groups","SectionC1",@"<h4>Groups</h4>
<p>This person is a member of the following groups</p>",@"",0,"1CBE10C7-5E64-4385-BEE3-81DCA43DC47F"); 
            
            // Add Block to Page: Contributions, Site: Rock Internal
            AddBlock("53CF4CBE-85F9-4A50-87D7-0D72A3FB2892","","694FF260-8C6F-4A59-93C9-CF3793FE30E6","Finance - Giving Profile List","SectionC1",@"<h4>Scheduled Transactions</h4>",@"",0,"B33DF8C4-29B2-4DC5-B182-61FC255B01C0"); 


            // Add Block to Page: Add Scheduled Transaction, Site: Rock Internal
            AddBlock("B1CA86DC-9890-4D26-8EBD-488044E1B3DD","","74EE3481-3E5A-4971-A02E-D463ABB45591","Finance - Add Transaction","Feature",@"",@"",0,"8ADB1C1F-299B-461A-8469-0FF4E2C98216"); 
            // Add Block to Page: Edit Scheduled Transaction, Site: Rock Internal
            AddBlock("D360B64F-1267-4518-95CD-99CD5AB87D88","","5171C4E5-7698-453E-9CC8-088D362296DE","Giving Profile Detail","Feature",@"",@"",0,"CC4AC47D-1EA8-406F-94D5-50D19DC6B87A"); 
            
            // Add Block to Page: Security, Site: Rock Internal
            AddBlock( "0E56F56E-FB32-4827-A69A-B90D43CB47F5", "", "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "Group List", "SectionC1", @"<h4>Security Groups</h4>", @"<div class='pull-right'><a href='~/SecurityRoles' class='btn btn-link'>Manage Security Roles</a></div>", 0, "68D34EC2-0A10-4344-89E3-E6DF99951FDB" );
            // Add Block to Page: Security, Site: Rock Internal
            AddBlock( "0E56F56E-FB32-4827-A69A-B90D43CB47F5", "", "CE06640D-C1BA-4ACE-AF03-8D733FD3247C", "User Logins", "SectionC1", @"<br/>
<br/>
<h4>User Accounts</h4>", @"", 1, "CD99F432-DFB4-4AA2-8B79-83B469448F98" ); 

            // Attrib for BlockType: Group List:Entity Type
            AddBlockTypeAttribute("3D7FB6BE-6BBD-49F7-96B4-96310AF3048A","3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB","Entity Type","ContextEntityType","","The type of entity that will provide context for this block",0,@"","A32C7EFE-2E5F-4E99-9867-DD562407B72E");
            // Attrib for BlockType: Group List:Display Filter
            AddBlockTypeAttribute("3D7FB6BE-6BBD-49F7-96B4-96310AF3048A","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Display Filter","DisplayFilter","","Should filter be displayed to allow filtering by group type?",6,@"False","7E0EDF09-9374-4AC4-8591-30C08D7F1E1F");
            // Attrib for BlockType: Group List:Include Group Types
            AddBlockTypeAttribute("3D7FB6BE-6BBD-49F7-96B4-96310AF3048A","F725B854-A15E-46AE-9D4C-0608D4154F1E","Include Group Types","IncludeGroupTypes","","The group types to display in the list.  If none are selected, all group types will be included.",1,@"","5164FF88-A53B-4982-BE50-D56F1FE13FC6");
            // Attrib for BlockType: Group List:Exclude Group Types
            AddBlockTypeAttribute("3D7FB6BE-6BBD-49F7-96B4-96310AF3048A","F725B854-A15E-46AE-9D4C-0608D4154F1E","Exclude Group Types","ExcludeGroupTypes","","The group types to exclude from the list (only valid if including all groups).",3,@"","0901CBFE-1980-4A1C-8AF0-4A8BD0FC46E9");
            // Attrib for BlockType: Group List:Display Group Type Column
            AddBlockTypeAttribute("3D7FB6BE-6BBD-49F7-96B4-96310AF3048A","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Display Group Type Column","DisplayGroupTypeColumn","","Should the Group Type column be displayed?",4,@"True","951D268A-B2A8-42A2-B1C1-3B854070DDF9");
            // Attrib for BlockType: Group List:Display Description Column
            AddBlockTypeAttribute("3D7FB6BE-6BBD-49F7-96B4-96310AF3048A","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Display Description Column","DisplayDescriptionColumn","","Should the Description column be displayed?",5,@"True","A0E1B2A4-9D86-4F57-B608-FC7CC498EAC3");
            // Attrib for BlockType: Group Detail:Map HTML
            AddBlockTypeAttribute("582BEEA1-5B27-444D-BC0A-F60CEB053981","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Map HTML","MapHTML","","The HTML to use for displaying group location maps. Liquid syntax is used to render data from the following data structure: points[type, latitude, longitude], polygons[type, polygon_wkt, google_encoded_polygon]",3,@"
    {% for point in points %}
        <div class='group-location-map'>
            <h4>{{ point.type }}</h4>
            <img src='http://maps.googleapis.com/maps/api/staticmap?sensor=false&size=350x200&zoom=13&format=png&style=feature:all|saturation:0|hue:0xe7ecf0&style=feature:road|saturation:-70&style=feature:transit|visibility:off&style=feature:poi|visibility:off&style=feature:water|visibility:simplified|saturation:-60&markers=color:0x779cb1|{{ point.latitude }},{{ point.longitude }}&visual_refresh=true'/>
        </div>
    {% endfor %}
    {% for polygon in polygons %}
        <div class='group-location-map'>
            <h4>{{ polygon.type }}</h4>
            <img src='http://maps.googleapis.com/maps/api/staticmap?sensor=false&size=350x200&format=png&style=feature:all|saturation:0|hue:0xe7ecf0&style=feature:road|saturation:-70&style=feature:transit|visibility:off&style=feature:poi|visibility:off&style=feature:water|visibility:simplified|saturation:-60&visual_refresh=true&path=fillcolor:0x779cb155|color:0xFFFFFF00|enc:{{ polygon.google_encoded_polygon }}'/>
        </div>
    {% endfor %}
","0D459868-02FD-4AB7-9A9C-92ACFCBB0FDC");

            // Attrib for BlockType: Finance - Giving Profile List:Edit Page
            AddBlockTypeAttribute("694FF260-8C6F-4A59-93C9-CF3793FE30E6","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Edit Page","EditPage","","",0,@"","78622434-4B4E-42E4-B044-21AEDD315186");
            // Attrib for BlockType: Finance - Giving Profile List:Add Page
            AddBlockTypeAttribute("694FF260-8C6F-4A59-93C9-CF3793FE30E6","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Add Page","AddPage","","",0,@"","9BCE3FD8-9014-4120-9DCC-06C4936284BA");
            
            // Attrib for BlockType: Finance - Add Transaction:Confirm Account
            AddBlockTypeAttribute("74EE3481-3E5A-4971-A02E-D463ABB45591","CE7CA048-551C-4F68-8C0A-FCDCBEB5B956","Confirm Account","ConfirmAccountTemplate","","Confirm Account Email Template",17,@"17aaceef-15ca-4c30-9a3a-11e6cf7e6411","95F2EFCF-6A68-4857-90AB-CC3A467EDF9A");
            
            // Attrib for BlockType: Giving Profile Detail:Success Header
            AddBlockTypeAttribute("5171C4E5-7698-453E-9CC8-088D362296DE","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Success Header","SuccessHeader","","The text (HTML) to display at the top of the success section.",15,@"
<p>
Thank you for your generous contribution.  Your support is helping {{ OrganizationName }} actively 
achieve our mission.  We are so grateful for your commitment. 
</p>
","7F14DD3C-C761-4AE1-A9E5-021EBF794547");
            // Attrib for BlockType: Giving Profile Detail:Add Account Text
            AddBlockTypeAttribute("5171C4E5-7698-453E-9CC8-088D362296DE","9C204CD0-1233-41C5-818A-C5DA439445AA","Add Account Text","AddAccountText","","The button text to display for adding an additional account",8,@"Add Another Account","77113EE3-27B0-4BE5-9218-7F7EB7DDD193");
            // Attrib for BlockType: Giving Profile Detail:Impersonation
            AddBlockTypeAttribute("5171C4E5-7698-453E-9CC8-088D362296DE","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Impersonation","Impersonation","","Should the current user be able to view and edit other people's transactions?  IMPORTANT: This should only be enabled on an internal page that is secured to trusted users",10,@"False","11649364-1508-484A-AA1E-751E5C4F9CD6");
            // Attrib for BlockType: Giving Profile Detail:Confirmation Footer
            AddBlockTypeAttribute("5171C4E5-7698-453E-9CC8-088D362296DE","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Confirmation Footer","ConfirmationFooter","","The text (HTML) to display at the bottom of the confirmation section.",14,@"
<div class='alert alert-info'>
By clicking the 'finish' button below I agree to allow {{ OrganizationName }} to debit the amount above from my account. I acknowledge that I may 
update the transaction information at any time by returning to this website. Please call the Finance Office if you have any additional questions. 
</div>
","E96D2D98-BB52-45E5-88D2-FECC345636D4");
            // Attrib for BlockType: Giving Profile Detail:Success Footer
            AddBlockTypeAttribute("5171C4E5-7698-453E-9CC8-088D362296DE","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Success Footer","SuccessFooter","","The text (HTML) to display at the bottom of the success section.",16,@"
","8F9A8359-2EAF-4390-BFD3-E8361E069F08");
            // Attrib for BlockType: Giving Profile Detail:Confirmation Header
            AddBlockTypeAttribute("5171C4E5-7698-453E-9CC8-088D362296DE","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Confirmation Header","ConfirmationHeader","","The text (HTML) to display at the top of the confirmation section.",13,@"
<p>
Please confirm the information below. Once you have confirmed that the information is accurate click the 'Finish' button to complete your transaction. 
</p>
","42F035FE-A1E8-4856-86A6-BA009BC6F33B");
            // Attrib for BlockType: Giving Profile Detail:Credit Card Gateway
            AddBlockTypeAttribute("5171C4E5-7698-453E-9CC8-088D362296DE","A7486B0E-4CA2-4E00-A987-5544C7DABA76","Credit Card Gateway","CCGateway","","The payment gateway to use for Credit Card transactions",0,@"","8177CE07-AA1B-43F4-ABCE-23E63DA8EBC8");
            // Attrib for BlockType: Giving Profile Detail:ACH Card Gateway
            AddBlockTypeAttribute("5171C4E5-7698-453E-9CC8-088D362296DE","A7486B0E-4CA2-4E00-A987-5544C7DABA76","ACH Card Gateway","ACHGateway","","The payment gateway to use for ACH (bank account) transactions",1,@"","FC9DF232-D7B1-4CA9-B348-D139276783BB");
            // Attrib for BlockType: Giving Profile Detail:Accounts
            AddBlockTypeAttribute("5171C4E5-7698-453E-9CC8-088D362296DE","17033CDD-EF97-4413-A483-7B85A787A87F","Accounts","Accounts","","The accounts to display.  By default all active accounts with a Public Name will be displayed",6,@"","32B27FF4-0EDC-4709-B714-41084F8FE99C");
            // Attrib for BlockType: Giving Profile Detail:Additional Accounts
            AddBlockTypeAttribute("5171C4E5-7698-453E-9CC8-088D362296DE","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Additional Accounts","AdditionalAccounts","","Should users be allowed to select additional accounts?  If so, any active account with a Public Name value will be available",7,@"True","DA811408-9985-4D1B-B92C-8C8FCA026B3D");

            // Attrib Value for Block:Groups, Attribute:Detail Page Page: Groups, Site: Rock Internal
            AddBlockAttributeValue( "1CBE10C7-5E64-4385-BEE3-81DCA43DC47F", "8E57EC42-ABEE-4D35-B7FA-D8513880E8E4", @"4e237286-b715-4109-a578-c1445ec02707" );
            // Attrib Value for Block:Groups, Attribute:Entity Type Page: Groups, Site: Rock Internal
            AddBlockAttributeValue( "1CBE10C7-5E64-4385-BEE3-81DCA43DC47F", "A32C7EFE-2E5F-4E99-9867-DD562407B72E", @"72657ed8-d16e-492e-ac12-144c5e7567e7" );
            // Attrib Value for Block:Groups, Attribute:Include Group Types Page: Groups, Site: Rock Internal
            AddBlockAttributeValue( "1CBE10C7-5E64-4385-BEE3-81DCA43DC47F", "5164FF88-A53B-4982-BE50-D56F1FE13FC6", @"" );
            // Attrib Value for Block:Groups, Attribute:Limit to Security Role Groups Page: Groups, Site: Rock Internal
            AddBlockAttributeValue( "1CBE10C7-5E64-4385-BEE3-81DCA43DC47F", "1DAD66E3-8859-487E-8200-483C98DE2E07", @"False" );
            // Attrib Value for Block:Groups, Attribute:Exclude Group Types Page: Groups, Site: Rock Internal
            AddBlockAttributeValue( "1CBE10C7-5E64-4385-BEE3-81DCA43DC47F", "0901CBFE-1980-4A1C-8AF0-4A8BD0FC46E9", @"aece949f-704c-483e-a4fb-93d5e4720c4c" );
            // Attrib Value for Block:Groups, Attribute:Display Group Type Column Page: Groups, Site: Rock Internal
            AddBlockAttributeValue( "1CBE10C7-5E64-4385-BEE3-81DCA43DC47F", "951D268A-B2A8-42A2-B1C1-3B854070DDF9", @"True" );
            // Attrib Value for Block:Groups, Attribute:Display Description Column Page: Groups, Site: Rock Internal
            AddBlockAttributeValue( "1CBE10C7-5E64-4385-BEE3-81DCA43DC47F", "A0E1B2A4-9D86-4F57-B608-FC7CC498EAC3", @"True" );
            // Attrib Value for Block:Groups, Attribute:Display Filter Page: Groups, Site: Rock Internal
            AddBlockAttributeValue( "1CBE10C7-5E64-4385-BEE3-81DCA43DC47F", "7E0EDF09-9374-4AC4-8591-30C08D7F1E1F", @"True" );
 
            AddBlockAttributeValue("B33DF8C4-29B2-4DC5-B182-61FC255B01C0","9BCE3FD8-9014-4120-9DCC-06C4936284BA",@"b1ca86dc-9890-4d26-8ebd-488044e1b3dd");
            // Attrib Value for Block:Finance - Giving Profile List, Attribute:Edit Page Page: Contributions, Site: Rock Internal
            AddBlockAttributeValue("B33DF8C4-29B2-4DC5-B182-61FC255B01C0","78622434-4B4E-42E4-B044-21AEDD315186",@"d360b64f-1267-4518-95cd-99cd5ab87d88");
            // Attrib Value for Block:Finance - Add Transaction, Attribute:Credit Card Gateway Page: Add Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("8ADB1C1F-299B-461A-8469-0FF4E2C98216","3D478949-1F85-4E81-A403-22BBA96B8F69",@"d4a40c4a-336f-49a6-9f44-88f149726126");
            // Attrib Value for Block:Finance - Add Transaction, Attribute:ACH Card Gateway Page: Add Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("8ADB1C1F-299B-461A-8469-0FF4E2C98216","D6429E78-E8F0-4EF2-9D18-DFDDE4ECC6A7",@"d4a40c4a-336f-49a6-9f44-88f149726126");
            // Attrib Value for Block:Finance - Add Transaction, Attribute:Batch Name Prefix Page: Add Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("8ADB1C1F-299B-461A-8469-0FF4E2C98216","245BDD4E-E8FF-4039-8C0B-C7AC1C185D1D",@"Online Giving");
            // Attrib Value for Block:Finance - Add Transaction, Attribute:Source Page: Add Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("8ADB1C1F-299B-461A-8469-0FF4E2C98216","5C54E6E7-1C21-4959-98EA-FB1C2D0A0D61",@"");
            // Attrib Value for Block:Finance - Add Transaction, Attribute:Address Type Page: Add Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("8ADB1C1F-299B-461A-8469-0FF4E2C98216","DBF313AB-0488-4BF7-A11D-1998D7A3476D",@"8c52e53c-2a66-435a-ae6e-5ee307d9a0dc");
            // Attrib Value for Block:Finance - Add Transaction, Attribute:Layout Style Page: Add Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("8ADB1C1F-299B-461A-8469-0FF4E2C98216","23B31F2F-9366-446D-9D3E-4CA68A6876D1",@"Vertical");
            // Attrib Value for Block:Finance - Add Transaction, Attribute:Accounts Page: Add Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("8ADB1C1F-299B-461A-8469-0FF4E2C98216","DAB27F0A-D0C0-4275-93F4-DEF227F6B1A2",@"4410306f-3fb5-4a57-9a80-09a3f9d40d0c,67c6181c-1d8c-44d7-b262-b81e746f06d8");
            // Attrib Value for Block:Finance - Add Transaction, Attribute:Additional Accounts Page: Add Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("8ADB1C1F-299B-461A-8469-0FF4E2C98216","EAF2435D-FACE-40F2-832D-CDB5A4D51BF3",@"True");
            // Attrib Value for Block:Finance - Add Transaction, Attribute:Add Account Text Page: Add Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("8ADB1C1F-299B-461A-8469-0FF4E2C98216","1133170C-8E4C-4020-B795-F799F893D70D",@"Add Another Account");
            // Attrib Value for Block:Finance - Add Transaction, Attribute:Scheduled Transactions Page: Add Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("8ADB1C1F-299B-461A-8469-0FF4E2C98216","63CA1F26-6942-48F4-9A15-F0A2D40E3FAB",@"True");
            // Attrib Value for Block:Finance - Add Transaction, Attribute:Impersonation Page: Add Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("8ADB1C1F-299B-461A-8469-0FF4E2C98216","FE64D72C-EA41-4C4E-9F0C-48048EEAB8A1",@"True");
            // Attrib Value for Block:Finance - Add Transaction, Attribute:Prompt for Phone Page: Add Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("8ADB1C1F-299B-461A-8469-0FF4E2C98216","8A572D6B-5CC1-4357-BFD5-8D887433A0AB",@"False");
            // Attrib Value for Block:Finance - Add Transaction, Attribute:Prompt for Email Page: Add Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("8ADB1C1F-299B-461A-8469-0FF4E2C98216","8B67B723-1C71-44EF-81F0-F4225CE7039B",@"False");
            // Attrib Value for Block:Finance - Add Transaction, Attribute:Confirmation Header Page: Add Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("8ADB1C1F-299B-461A-8469-0FF4E2C98216","FA6300AD-9268-47FD-BBBE-BB6A415B0002",@"<p>
Please confirm the information below. Once you have confirmed that the information is 
accurate click the 'Finish' button to complete the transaction. 
</p>
");
            // Attrib Value for Block:Finance - Add Transaction, Attribute:Confirmation Footer Page: Add Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("8ADB1C1F-299B-461A-8469-0FF4E2C98216","B1F9196D-B51D-4ECD-A7BE-89F34431D736",@"<div class='alert alert-info'>
Clicking the 'finish' button will submit the transaction for processing.  
</div>
");
            // Attrib Value for Block:Finance - Add Transaction, Attribute:Success Header Page: Add Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("8ADB1C1F-299B-461A-8469-0FF4E2C98216","1597A542-E6EB-4E29-A435-E5C23785251E",@"<p>
Thank you.  The transaction has been submitted successfully. 
</p>
");
            // Attrib Value for Block:Finance - Add Transaction, Attribute:Success Footer Page: Add Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("8ADB1C1F-299B-461A-8469-0FF4E2C98216","188C6D55-CC08-4019-AA5F-706251509696",@"
");
            // Attrib Value for Block:Finance - Add Transaction, Attribute:Confirm Account Page: Add Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("8ADB1C1F-299B-461A-8469-0FF4E2C98216","95F2EFCF-6A68-4857-90AB-CC3A467EDF9A",@"17aaceef-15ca-4c30-9a3a-11e6cf7e6411");
            // Attrib Value for Block:Giving Profile Detail, Attribute:Credit Card Gateway Page: Edit Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("CC4AC47D-1EA8-406F-94D5-50D19DC6B87A","8177CE07-AA1B-43F4-ABCE-23E63DA8EBC8",@"d4a40c4a-336f-49a6-9f44-88f149726126");
            // Attrib Value for Block:Giving Profile Detail, Attribute:ACH Card Gateway Page: Edit Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("CC4AC47D-1EA8-406F-94D5-50D19DC6B87A","FC9DF232-D7B1-4CA9-B348-D139276783BB",@"d4a40c4a-336f-49a6-9f44-88f149726126");
            // Attrib Value for Block:Giving Profile Detail, Attribute:Accounts Page: Edit Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("CC4AC47D-1EA8-406F-94D5-50D19DC6B87A","32B27FF4-0EDC-4709-B714-41084F8FE99C",@"4410306f-3fb5-4a57-9a80-09a3f9d40d0c,67c6181c-1d8c-44d7-b262-b81e746f06d8");
            // Attrib Value for Block:Giving Profile Detail, Attribute:Additional Accounts Page: Edit Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("CC4AC47D-1EA8-406F-94D5-50D19DC6B87A","DA811408-9985-4D1B-B92C-8C8FCA026B3D",@"True");
            // Attrib Value for Block:Giving Profile Detail, Attribute:Add Account Text Page: Edit Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("CC4AC47D-1EA8-406F-94D5-50D19DC6B87A","77113EE3-27B0-4BE5-9218-7F7EB7DDD193",@"Add Another Account");
            // Attrib Value for Block:Giving Profile Detail, Attribute:Impersonation Page: Edit Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("CC4AC47D-1EA8-406F-94D5-50D19DC6B87A","11649364-1508-484A-AA1E-751E5C4F9CD6",@"True");
            // Attrib Value for Block:Giving Profile Detail, Attribute:Confirmation Header Page: Edit Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("CC4AC47D-1EA8-406F-94D5-50D19DC6B87A","42F035FE-A1E8-4856-86A6-BA009BC6F33B",@"<p>
Please confirm the information below. Once you have confirmed that the information is 
accurate click the 'Finish' button to complete the transaction. 
</p>
");
            // Attrib Value for Block:Giving Profile Detail, Attribute:Confirmation Footer Page: Edit Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("CC4AC47D-1EA8-406F-94D5-50D19DC6B87A","E96D2D98-BB52-45E5-88D2-FECC345636D4",@"<div class='alert alert-info'>
Clicking the 'finish' button below will submit the transaction for processing.
</div>
");
            // Attrib Value for Block:Giving Profile Detail, Attribute:Success Header Page: Edit Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("CC4AC47D-1EA8-406F-94D5-50D19DC6B87A","7F14DD3C-C761-4AE1-A9E5-021EBF794547",@"<p>
The transaction has been updated.
</p>
");
            // Attrib Value for Block:Giving Profile Detail, Attribute:Success Footer Page: Edit Scheduled Transaction, Site: Rock Internal
            AddBlockAttributeValue("CC4AC47D-1EA8-406F-94D5-50D19DC6B87A","8F9A8359-2EAF-4390-BFD3-E8361E069F08",@"
");

            // Attrib Value for Block:Group List, Attribute:Detail Page Page: Security, Site: Rock Internal
            AddBlockAttributeValue("68D34EC2-0A10-4344-89E3-E6DF99951FDB","8E57EC42-ABEE-4D35-B7FA-D8513880E8E4",@"d9678fef-c086-4232-972c-5dbac14bfee6");
            // Attrib Value for Block:Group List, Attribute:Entity Type Page: Security, Site: Rock Internal
            AddBlockAttributeValue("68D34EC2-0A10-4344-89E3-E6DF99951FDB","A32C7EFE-2E5F-4E99-9867-DD562407B72E",@"72657ed8-d16e-492e-ac12-144c5e7567e7");
            // Attrib Value for Block:Group List, Attribute:Include Group Types Page: Security, Site: Rock Internal
            AddBlockAttributeValue("68D34EC2-0A10-4344-89E3-E6DF99951FDB","5164FF88-A53B-4982-BE50-D56F1FE13FC6",@"");
            // Attrib Value for Block:Group List, Attribute:Limit to Security Role Groups Page: Security, Site: Rock Internal
            AddBlockAttributeValue("68D34EC2-0A10-4344-89E3-E6DF99951FDB","1DAD66E3-8859-487E-8200-483C98DE2E07",@"True");
            // Attrib Value for Block:Group List, Attribute:Exclude Group Types Page: Security, Site: Rock Internal
            AddBlockAttributeValue("68D34EC2-0A10-4344-89E3-E6DF99951FDB","0901CBFE-1980-4A1C-8AF0-4A8BD0FC46E9",@"");
            // Attrib Value for Block:Group List, Attribute:Display Group Type Column Page: Security, Site: Rock Internal
            AddBlockAttributeValue("68D34EC2-0A10-4344-89E3-E6DF99951FDB","951D268A-B2A8-42A2-B1C1-3B854070DDF9",@"True");
            // Attrib Value for Block:Group List, Attribute:Display Description Column Page: Security, Site: Rock Internal
            AddBlockAttributeValue("68D34EC2-0A10-4344-89E3-E6DF99951FDB","A0E1B2A4-9D86-4F57-B608-FC7CC498EAC3",@"True");
            // Attrib Value for Block:Group List, Attribute:Display Filter Page: Security, Site: Rock Internal
            AddBlockAttributeValue("68D34EC2-0A10-4344-89E3-E6DF99951FDB","7E0EDF09-9374-4AC4-8591-30C08D7F1E1F",@"False");

            Sql( @"
    UPDATE [Block] SET
        [Order] = 1,
        [PreHtml] = '<h4>Past Transactions</h4>'
    WHERE [Guid] = '9382B285-3EF6-47F7-94BB-A47C498196A3'

    UPDATE [Page] SET 
	    [Order] = 4,
	    [Title] = 'Security',
	    [Name] = 'Security'
    WHERE [Guid] = '0E56F56E-FB32-4827-A69A-B90D43CB47F5'

    UPDATE [Page] SET 
	    [Order] = 3
    WHERE [Guid] = '53CF4CBE-85F9-4A50-87D7-0D72A3FB2892'
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Giving Profile Detail:Additional Accounts
            DeleteAttribute( "DA811408-9985-4D1B-B92C-8C8FCA026B3D" );
            // Attrib for BlockType: Giving Profile Detail:Accounts
            DeleteAttribute( "32B27FF4-0EDC-4709-B714-41084F8FE99C" );
            // Attrib for BlockType: Giving Profile Detail:ACH Card Gateway
            DeleteAttribute( "FC9DF232-D7B1-4CA9-B348-D139276783BB" );
            // Attrib for BlockType: Giving Profile Detail:Credit Card Gateway
            DeleteAttribute( "8177CE07-AA1B-43F4-ABCE-23E63DA8EBC8" );
            // Attrib for BlockType: Giving Profile Detail:Confirmation Header
            DeleteAttribute( "42F035FE-A1E8-4856-86A6-BA009BC6F33B" );
            // Attrib for BlockType: Giving Profile Detail:Success Footer
            DeleteAttribute( "8F9A8359-2EAF-4390-BFD3-E8361E069F08" );
            // Attrib for BlockType: Giving Profile Detail:Confirmation Footer
            DeleteAttribute( "E96D2D98-BB52-45E5-88D2-FECC345636D4" );
            // Attrib for BlockType: Giving Profile Detail:Impersonation
            DeleteAttribute( "11649364-1508-484A-AA1E-751E5C4F9CD6" );
            // Attrib for BlockType: Giving Profile Detail:Add Account Text
            DeleteAttribute( "77113EE3-27B0-4BE5-9218-7F7EB7DDD193" );
            // Attrib for BlockType: Giving Profile Detail:Success Header
            DeleteAttribute( "7F14DD3C-C761-4AE1-A9E5-021EBF794547" );
            // Attrib for BlockType: Finance - Add Transaction:Confirm Account
            DeleteAttribute( "95F2EFCF-6A68-4857-90AB-CC3A467EDF9A" );
            // Attrib for BlockType: Finance - Giving Profile List:Add Page
            DeleteAttribute( "9BCE3FD8-9014-4120-9DCC-06C4936284BA" );
            // Attrib for BlockType: Finance - Giving Profile List:Edit Page
            DeleteAttribute( "78622434-4B4E-42E4-B044-21AEDD315186" );
            // Attrib for BlockType: Group Detail:Map HTML
            DeleteAttribute( "0D459868-02FD-4AB7-9A9C-92ACFCBB0FDC" );
            // Attrib for BlockType: Group List:Display Description Column
            DeleteAttribute( "A0E1B2A4-9D86-4F57-B608-FC7CC498EAC3" );
            // Attrib for BlockType: Group List:Display Group Type Column
            DeleteAttribute( "951D268A-B2A8-42A2-B1C1-3B854070DDF9" );
            // Attrib for BlockType: Group List:Exclude Group Types
            DeleteAttribute( "0901CBFE-1980-4A1C-8AF0-4A8BD0FC46E9" );
            // Attrib for BlockType: Group List:Include Group Types
            DeleteAttribute( "5164FF88-A53B-4982-BE50-D56F1FE13FC6" );
            // Attrib for BlockType: Group List:Display Filter
            DeleteAttribute( "7E0EDF09-9374-4AC4-8591-30C08D7F1E1F" );
            // Attrib for BlockType: Group List:Entity Type
            DeleteAttribute( "A32C7EFE-2E5F-4E99-9867-DD562407B72E" );
            // Remove Block: User Logins, from Page: Security, Site: Rock Internal
            DeleteBlock( "CD99F432-DFB4-4AA2-8B79-83B469448F98" );
            // Remove Block: Group List, from Page: Security, Site: Rock Internal
            DeleteBlock( "68D34EC2-0A10-4344-89E3-E6DF99951FDB" );
            // Remove Block: Giving Profile Detail, from Page: Edit Scheduled Transaction, Site: Rock Internal
            DeleteBlock( "CC4AC47D-1EA8-406F-94D5-50D19DC6B87A" );
            // Remove Block: Finance - Add Transaction, from Page: Add Scheduled Transaction, Site: Rock Internal
            DeleteBlock( "8ADB1C1F-299B-461A-8469-0FF4E2C98216" );
            // Remove Block: Finance - Giving Profile List, from Page: Contributions, Site: Rock Internal
            DeleteBlock( "B33DF8C4-29B2-4DC5-B182-61FC255B01C0" );
            // Remove Block: Groups, from Page: Groups, Site: Rock Internal
            DeleteBlock( "1CBE10C7-5E64-4385-BEE3-81DCA43DC47F" );
            DeleteBlockType( "5171C4E5-7698-453E-9CC8-088D362296DE" ); // Giving Profile Detail
            DeletePage( "D360B64F-1267-4518-95CD-99CD5AB87D88" ); // Page: Edit Scheduled TransactionLayout: Full Width, Site: Rock Internal
            DeletePage( "B1CA86DC-9890-4D26-8EBD-488044E1B3DD" ); // Page: Add Scheduled TransactionLayout: Full Width, Site: Rock Internal

        }
    }
}
