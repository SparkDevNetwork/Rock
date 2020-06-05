// <copyright>
// Copyright by BEMA Information Technologies
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

using Rock.Plugin;

namespace com.bemaservices.ClientPackage.BEMA
{
    [MigrationNumber( 8, "1.9.4" )]
    public class LargeDonations : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Page: Large Donations              
            RockMigrationHelper.AddPage("2571CBBD-7CCA-4B24-AAAB-107FD136298B","D65F783D-87A9-4CC9-8110-E83466A0EADB","Large Donations","A report on the givers of large donations and their families.","754EFBFA-54B2-4558-9669-8121D92D88F0",""); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType("HTML Content","Adds an editable HTML fragment to the page.","~/Blocks/Cms/HtmlContentDetail.ascx","CMS","19B61D65-37E3-459F-A44F-DEF0089118A3");
            RockMigrationHelper.UpdateBlockType("Dynamic Data","Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure.","~/Blocks/Reporting/DynamicData.ascx","Reporting","E31E02E9-73F6-4B3E-98BA-E0E4F86CA126");
            // Add Block to Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "754EFBFA-54B2-4558-9669-8121D92D88F0","","19B61D65-37E3-459F-A44F-DEF0089118A3","HTML","Main","","",0,"CEB3D683-9A8D-49D7-900D-BAE844887C1D");   
            // Add Block to Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "754EFBFA-54B2-4558-9669-8121D92D88F0","","E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","Dynamic Data","Main","","",1,"D540BECB-1139-4138-B17F-218ECE6CCAEF");   
            // Attrib for BlockType: Dynamic Data:Update Page              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Update Page","UpdatePage","","If True, provides fields for updating the parent page's Name and Description",0,@"True","230EDFE8-33CA-478D-8C9A-572323AF3466");  
            // Attrib for BlockType: Dynamic Data:Query Params              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","9C204CD0-1233-41C5-818A-C5DA439445AA","Query Params","QueryParams","","Parameters to pass to query",0,@"","B0EC41B9-37C0-48FD-8E4E-37A8CA305012");  
            // Attrib for BlockType: Dynamic Data:Columns              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","9C204CD0-1233-41C5-818A-C5DA439445AA","Columns","Columns","","The columns to hide or show",0,@"","90B0E6AF-B2F4-4397-953B-737A40D4023B");  
            // Attrib for BlockType: Dynamic Data:Query             
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Query","Query","","The query to execute. Note that if you are providing SQL you can add items from the query string using Lava like {{ QueryParmName }}.",0,@"","71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356");  
            // Attrib for BlockType: Dynamic Data:Url Mask              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","9C204CD0-1233-41C5-818A-C5DA439445AA","Url Mask","UrlMask","","The Url to redirect to when a row is clicked",0,@"","B9163A35-E09C-466D-8A2D-4ED81DF0114C");  
            // Attrib for BlockType: Dynamic Data:Show Columns              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Show Columns","ShowColumns","","Should the 'Columns' specified below be the only ones shown (vs. the only ones hidden)",0,@"False","202A82BF-7772-481C-8419-600012607972");  
            // Attrib for BlockType: Dynamic Data:Merge Fields              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","9C204CD0-1233-41C5-818A-C5DA439445AA","Merge Fields","MergeFields","","Any fields to make available as merge fields for any new communications",0,@"","8EB882CE-5BB1-4844-9C28-10190903EECD");  
            // Attrib for BlockType: Dynamic Data:Formatted Output              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Formatted Output","FormattedOutput","","Optional formatting to apply to the returned results.  If left blank, a grid will be displayed. Example: {% for row in rows %} {{ row.FirstName }}<br/> {% endfor %}",0,@"","6A233402-446C-47E9-94A5-6A247C29BC21");  
            // Attrib for BlockType: Dynamic Data:Person Report              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Person Report","PersonReport","","Is this report a list of people?",0,@"False","8104CE53-FDB3-4E9F-B8E7-FD9E06E7551C");  
            // Attrib for BlockType: Dynamic Data:Communication Recipient Person Id Columns              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","9C204CD0-1233-41C5-818A-C5DA439445AA","Communication Recipient Person Id Columns","CommunicationRecipientPersonIdColumns","","Columns that contain a communication recipient person id.",0,@"","75DDB977-9E71-44E8-924B-27134659D3A4");  
            // Attrib for BlockType: Dynamic Data:Show Excel Export              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Show Excel Export","ShowExcelExport","","Show Export to Excel button in grid footer?",0,@"True","E11B57E5-EC7D-4C42-9ADA-37594D71F145");  
            // Attrib for BlockType: Dynamic Data:Show Communicate              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Show Communicate","ShowCommunicate","","Show Communicate button in grid footer?",0,@"True","5B2C115A-C187-4AB3-93AE-7010644B39DA");  
            // Attrib for BlockType: Dynamic Data:Show Merge Person              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Show Merge Person","ShowMergePerson","","Show Merge Person button in grid footer?",0,@"True","8762ABE3-726E-4629-BD4D-3E42E1FBCC9E");  
            // Attrib for BlockType: Dynamic Data:Show Bulk Update              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Show Bulk Update","ShowBulkUpdate","","Show Bulk Update button in grid footer?",0,@"True","D01510AA-1B8D-467C-AFC6-F7554CB7CF78");  
            // Attrib for BlockType: Dynamic Data:Stored Procedure              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Stored Procedure","StoredProcedure","","Is the query a stored procedure?",0,@"False","A4439703-5432-489A-9C14-155903D6A43E");  
            // Attrib for BlockType: Dynamic Data:Show Merge Template              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Show Merge Template","ShowMergeTemplate","","Show Export to Merge Template button in grid footer?",0,@"True","6697B0A2-C8FE-497A-B5B4-A9D459474338");  
            // Attrib for BlockType: Dynamic Data:Paneled Grid              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Paneled Grid","PaneledGrid","","Add the 'grid-panel' class to the grid to allow it to fit nicely in a block.",0,@"False","5449CB61-2DFC-4B55-A697-38F1C2AF128B");  
            // Attrib for BlockType: Dynamic Data:Show Grid Filter              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Show Grid Filter","ShowGridFilter","","Show filtering controls that are dynamically generated to match the columns of the dynamic data.",0,@"True","E582FD3C-9990-47D1-A57F-A3DB753B1D0C");  
            // Attrib for BlockType: Dynamic Data:Timeout              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Timeout","Timeout","","The amount of time in xxx to allow the query to run before timing out.",0,@"30","BEEE38DD-2791-4242-84B6-0495904143CC");  
            // Attrib for BlockType: Dynamic Data:Page Title Lava              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Page Title Lava","PageTitleLava","","Optional Lava for setting the page title. If nothing is provided then the page's title will be used.",0,@"","3F4BA170-F5C5-405E-976F-0AFBB8855FE8");  
            // Attrib for BlockType: HTML Content:Enabled Lava Commands              
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D","Enabled Lava Commands","EnabledLavaCommands","","The Lava commands that should be enabled for this HTML block.",0,@"","7146AC24-9250-4FC4-9DF2-9803B9A84299");  
            // Attrib for BlockType: HTML Content:Entity Type              
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB","Entity Type","ContextEntityType","","The type of entity that will provide context for this block",0,@"","6783D47D-92F9-4F48-93C0-16111D675A0F");  
            // Attrib for BlockType: Dynamic Data:Encrypted Fields              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","9C204CD0-1233-41C5-818A-C5DA439445AA","Encrypted Fields","EncryptedFields","","Any fields that need to be decrypted before displaying their value",0,@"","AF7714D4-D825-419A-B136-FF8293396635");  
            // Attrib for BlockType: HTML Content:Start in Code Editor mode              
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Start in Code Editor mode","UseCodeEditor","","Start the editor in code editor mode instead of WYSIWYG editor mode.",1,@"True","0673E015-F8DD-4A52-B380-C758011331B2");  
            // Attrib for BlockType: Dynamic Data:Enabled Lava Commands              
            RockMigrationHelper.UpdateBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D","Enabled Lava Commands","EnabledLavaCommands","","The Lava commands that should be enabled for this dynamic data block.",1,@"","824634D6-7F75-465B-A2D2-BA3CE1662CAC");  
            // Attrib for BlockType: HTML Content:Document Root Folder              
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","9C204CD0-1233-41C5-818A-C5DA439445AA","Document Root Folder","DocumentRootFolder","","The folder to use as the root when browsing or uploading documents.",2,@"~/Content","3BDB8AED-32C5-4879-B1CB-8FC7C8336534");  
            // Attrib for BlockType: HTML Content:Image Root Folder              
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","9C204CD0-1233-41C5-818A-C5DA439445AA","Image Root Folder","ImageRootFolder","","The folder to use as the root when browsing or uploading images.",3,@"~/Content","26F3AFC6-C05B-44A4-8593-AFE1D9969B0E");  
            // Attrib for BlockType: HTML Content:User Specific Folders              
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","User Specific Folders","UserSpecificFolders","","Should the root folders be specific to current user?",4,@"False","9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE");  
            // Attrib for BlockType: HTML Content:Cache Duration              
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Cache Duration","CacheDuration","","Number of seconds to cache the content.",5,@"0","4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4");  
            // Attrib for BlockType: HTML Content:Context Parameter              
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","9C204CD0-1233-41C5-818A-C5DA439445AA","Context Parameter","ContextParameter","","Query string parameter to use for 'personalizing' content based on unique values.",6,@"","3FFC512D-A576-4289-B648-905FD7A64ABB");  
            // Attrib for BlockType: HTML Content:Context Name              
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","9C204CD0-1233-41C5-818A-C5DA439445AA","Context Name","ContextName","","Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share html values.",7,@"","466993F7-D838-447A-97E7-8BBDA6A57289");  
            // Attrib for BlockType: HTML Content:Enable Versioning              
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Versioning","SupportVersions","","If checked, previous versions of the content will be preserved. Versioning is required if you want to require approval.",8,@"False","7C1CE199-86CF-4EAE-8AB3-848416A72C58");  
            // Attrib for BlockType: HTML Content:Require Approval              
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Require Approval","RequireApproval","","Require that content be approved?",9,@"False","EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A");  
            // Attrib for BlockType: HTML Content:Cache Tags              
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","BD0D9B57-2A41-4490-89FF-F01DAB7D4904","Cache Tags","CacheTags","","Cached tags are used to link cached content so that it can be expired as a group",10,@"","522C18A9-C727-42A5-A0BA-13C673E8C4B6");  
            // Attrib for BlockType: HTML Content:Is Secondary Block              
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Is Secondary Block","IsSecondaryBlock","","Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.",11,@"False","04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4");  
            // Attrib Value for Block:Dynamic Data, Attribute:Update Page Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("D540BECB-1139-4138-B17F-218ECE6CCAEF","230EDFE8-33CA-478D-8C9A-572323AF3466",@"True");  
            // Attrib Value for Block:Dynamic Data, Attribute:Query Params Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("D540BECB-1139-4138-B17F-218ECE6CCAEF","B0EC41B9-37C0-48FD-8E4E-37A8CA305012",@"");  
            // Attrib Value for Block:Dynamic Data, Attribute:Columns Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("D540BECB-1139-4138-B17F-218ECE6CCAEF","90B0E6AF-B2F4-4397-953B-737A40D4023B",@"");  
            // Attrib Value for Block:Dynamic Data, Attribute:Query Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("D540BECB-1139-4138-B17F-218ECE6CCAEF","71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356",@"{% assign amount = '' %}  {% include '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Assets/Sql/LargeDonations.sql' %}");  
            // Attrib Value for Block:Dynamic Data, Attribute:Url Mask Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("D540BECB-1139-4138-B17F-218ECE6CCAEF","B9163A35-E09C-466D-8A2D-4ED81DF0114C",@"");  
            // Attrib Value for Block:Dynamic Data, Attribute:Show Columns Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("D540BECB-1139-4138-B17F-218ECE6CCAEF","202A82BF-7772-481C-8419-600012607972",@"False");  
            // Attrib Value for Block:Dynamic Data, Attribute:Merge Fields Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("D540BECB-1139-4138-B17F-218ECE6CCAEF","8EB882CE-5BB1-4844-9C28-10190903EECD",@"");  
            // Attrib Value for Block:Dynamic Data, Attribute:Formatted Output Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("D540BECB-1139-4138-B17F-218ECE6CCAEF","6A233402-446C-47E9-94A5-6A247C29BC21",@"");  
            // Attrib Value for Block:Dynamic Data, Attribute:Person Report Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("D540BECB-1139-4138-B17F-218ECE6CCAEF","8104CE53-FDB3-4E9F-B8E7-FD9E06E7551C",@"False");  
            // Attrib Value for Block:Dynamic Data, Attribute:Communication Recipient Person Id Columns Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("D540BECB-1139-4138-B17F-218ECE6CCAEF","75DDB977-9E71-44E8-924B-27134659D3A4",@"");  
            // Attrib Value for Block:Dynamic Data, Attribute:Show Excel Export Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("D540BECB-1139-4138-B17F-218ECE6CCAEF","E11B57E5-EC7D-4C42-9ADA-37594D71F145",@"True");  
            // Attrib Value for Block:Dynamic Data, Attribute:Show Communicate Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("D540BECB-1139-4138-B17F-218ECE6CCAEF","5B2C115A-C187-4AB3-93AE-7010644B39DA",@"False");  
            // Attrib Value for Block:Dynamic Data, Attribute:Show Merge Person Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("D540BECB-1139-4138-B17F-218ECE6CCAEF","8762ABE3-726E-4629-BD4D-3E42E1FBCC9E",@"False");  
            // Attrib Value for Block:Dynamic Data, Attribute:Show Bulk Update Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("D540BECB-1139-4138-B17F-218ECE6CCAEF","D01510AA-1B8D-467C-AFC6-F7554CB7CF78",@"False");  
            // Attrib Value for Block:Dynamic Data, Attribute:Stored Procedure Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("D540BECB-1139-4138-B17F-218ECE6CCAEF","A4439703-5432-489A-9C14-155903D6A43E",@"False");  
            // Attrib Value for Block:Dynamic Data, Attribute:Show Merge Template Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("D540BECB-1139-4138-B17F-218ECE6CCAEF","6697B0A2-C8FE-497A-B5B4-A9D459474338",@"True");  
            // Attrib Value for Block:Dynamic Data, Attribute:Paneled Grid Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("D540BECB-1139-4138-B17F-218ECE6CCAEF","5449CB61-2DFC-4B55-A697-38F1C2AF128B",@"False");  
            // Attrib Value for Block:Dynamic Data, Attribute:Show Grid Filter Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("D540BECB-1139-4138-B17F-218ECE6CCAEF","E582FD3C-9990-47D1-A57F-A3DB753B1D0C",@"True");  
            // Attrib Value for Block:Dynamic Data, Attribute:Timeout Page: Large Donations, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("D540BECB-1139-4138-B17F-218ECE6CCAEF","BEEE38DD-2791-4242-84B6-0495904143CC",@"30");  

            RockMigrationHelper.UpdateHtmlContentBlock( "CEB3D683-9A8D-49D7-900D-BAE844887C1D", @"<h4>The report works with a Page Parameter of DateRange or Amount. You can also assign a minimum amount for the donations if you edit the report. The default minimum is $5000.</h4>"
            , "d50c9802-5f45-4092-b4e5-7981f091bbe7" );

            // Hide Page from view
            RockMigrationHelper.AddSecurityAuthForPage( "754EFBFA-54B2-4558-9669-8121D92D88F0", 0, "View", false, "", 1, "7c1e372c-48c4-48bc-a787-9b1a2754f924" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
                        RockMigrationHelper.DeleteAttribute("824634D6-7F75-465B-A2D2-BA3CE1662CAC");
            RockMigrationHelper.DeleteAttribute("522C18A9-C727-42A5-A0BA-13C673E8C4B6");
            RockMigrationHelper.DeleteAttribute("AF7714D4-D825-419A-B136-FF8293396635");
            RockMigrationHelper.DeleteAttribute("3F4BA170-F5C5-405E-976F-0AFBB8855FE8");
            RockMigrationHelper.DeleteAttribute("BEEE38DD-2791-4242-84B6-0495904143CC");
            RockMigrationHelper.DeleteAttribute("E582FD3C-9990-47D1-A57F-A3DB753B1D0C");
            RockMigrationHelper.DeleteAttribute("5449CB61-2DFC-4B55-A697-38F1C2AF128B");
            RockMigrationHelper.DeleteAttribute("6697B0A2-C8FE-497A-B5B4-A9D459474338");
            RockMigrationHelper.DeleteAttribute("A4439703-5432-489A-9C14-155903D6A43E");
            RockMigrationHelper.DeleteAttribute("D01510AA-1B8D-467C-AFC6-F7554CB7CF78");
            RockMigrationHelper.DeleteAttribute("8762ABE3-726E-4629-BD4D-3E42E1FBCC9E");
            RockMigrationHelper.DeleteAttribute("5B2C115A-C187-4AB3-93AE-7010644B39DA");
            RockMigrationHelper.DeleteAttribute("E11B57E5-EC7D-4C42-9ADA-37594D71F145");
            RockMigrationHelper.DeleteAttribute("75DDB977-9E71-44E8-924B-27134659D3A4");
            RockMigrationHelper.DeleteAttribute("04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4");
            RockMigrationHelper.DeleteAttribute("7146AC24-9250-4FC4-9DF2-9803B9A84299");
            RockMigrationHelper.DeleteAttribute("8104CE53-FDB3-4E9F-B8E7-FD9E06E7551C");
            RockMigrationHelper.DeleteAttribute("6A233402-446C-47E9-94A5-6A247C29BC21");
            RockMigrationHelper.DeleteAttribute("8EB882CE-5BB1-4844-9C28-10190903EECD");
            RockMigrationHelper.DeleteAttribute("6783D47D-92F9-4F48-93C0-16111D675A0F");
            RockMigrationHelper.DeleteAttribute("3BDB8AED-32C5-4879-B1CB-8FC7C8336534");
            RockMigrationHelper.DeleteAttribute("9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE");
            RockMigrationHelper.DeleteAttribute("26F3AFC6-C05B-44A4-8593-AFE1D9969B0E");
            RockMigrationHelper.DeleteAttribute("0673E015-F8DD-4A52-B380-C758011331B2");
            RockMigrationHelper.DeleteAttribute("202A82BF-7772-481C-8419-600012607972");
            RockMigrationHelper.DeleteAttribute("B9163A35-E09C-466D-8A2D-4ED81DF0114C");
            RockMigrationHelper.DeleteAttribute("71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356");
            RockMigrationHelper.DeleteAttribute("90B0E6AF-B2F4-4397-953B-737A40D4023B");
            RockMigrationHelper.DeleteAttribute("B0EC41B9-37C0-48FD-8E4E-37A8CA305012");
            RockMigrationHelper.DeleteAttribute("230EDFE8-33CA-478D-8C9A-572323AF3466");
            RockMigrationHelper.DeleteAttribute("466993F7-D838-447A-97E7-8BBDA6A57289");
            RockMigrationHelper.DeleteAttribute("3FFC512D-A576-4289-B648-905FD7A64ABB");
            RockMigrationHelper.DeleteAttribute("7C1CE199-86CF-4EAE-8AB3-848416A72C58");
            RockMigrationHelper.DeleteAttribute("EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A");
            RockMigrationHelper.DeleteAttribute("4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4");
            RockMigrationHelper.DeleteBlock("D540BECB-1139-4138-B17F-218ECE6CCAEF");
            RockMigrationHelper.DeleteBlock("CEB3D683-9A8D-49D7-900D-BAE844887C1D");
            RockMigrationHelper.DeleteBlockType("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126");
            RockMigrationHelper.DeleteBlockType("19B61D65-37E3-459F-A44F-DEF0089118A3");
            RockMigrationHelper.DeletePage("754EFBFA-54B2-4558-9669-8121D92D88F0"); //  Page: Large Donations
        }
    }
}

