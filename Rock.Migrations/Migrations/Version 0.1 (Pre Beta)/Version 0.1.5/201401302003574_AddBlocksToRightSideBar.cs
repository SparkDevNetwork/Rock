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
    public partial class AddBlocksToRightSideBar : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Site:External Website -- this layout exists, it just has no blocks on it.
            //AddLayout( "F3F82256-2D66-432B-9D67-3552CD2F4C2B", "RightSidebar", "Right Sidebar", "", "ADC7866C-D0B3-4B97-9AB2-EACB36EA24ED" );

            // Add Block to Layout: Right Sidebar, Site: External Website
            AddBlock("","ADC7866C-D0B3-4B97-9AB2-EACB36EA24ED","CACB9D1A-A820-4587-986A-D66A69EE9948","Page Menu","Navigation","","",0,"D4651935-E652-469D-8EBD-69FF3E684BA0"); 

            // Add Block to Layout: Right Sidebar, Site: External Website
            AddBlock("","ADC7866C-D0B3-4B97-9AB2-EACB36EA24ED","19B61D65-37E3-459F-A44F-DEF0089118A3","Footer Text","Footer","<div class=\"footer-message\">","</div>",0,"A490588D-8943-42A4-9DE5-B67BAB88C5FA"); 

            // Add Block to Layout: Right Sidebar, Site: External Website
            AddBlock("","ADC7866C-D0B3-4B97-9AB2-EACB36EA24ED","19B61D65-37E3-459F-A44F-DEF0089118A3","Footer Address","Footer","<div class=\"footer-address\">","</div>",0,"790557BF-E776-48AE-ABBC-E37CF18FF3FA"); 

            // Add Block to Layout: Right Sidebar, Site: External Website
            AddBlock("","ADC7866C-D0B3-4B97-9AB2-EACB36EA24ED","04712F3D-9667-4901-A49D-4507573EF7AD","Login Status","Login","","",0,"ED8662A3-44A9-4181-8D0B-55087E1062D1"); 

            // Add Block to Layout: Right Sidebar, Site: External Website
            AddBlock("","ADC7866C-D0B3-4B97-9AB2-EACB36EA24ED","19B61D65-37E3-459F-A44F-DEF0089118A3","Header Text","Header","","",0,"FE2ADC7E-6D1C-4C3C-8D33-C6328A0D4EA3");

            // New Attributes for the HTML Content block

            // Attrib for BlockType: HTML Content:Image Root Folder
            AddBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image Root Folder", "ImageRootFolder", "", "The folder to use as the root when browsing or uploading images.", 2, @"~/Content", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E" );

            // Attrib for BlockType: HTML Content:User Specific Folders
            AddBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User Specific Folders", "UserSpecificFolders", "", "Should the root folders be specific to current user?", 3, @"False", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE" );

            // Attrib for BlockType: HTML Content:Document Root Folder
            AddBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Root Folder", "DocumentRootFolder", "", "The folder to use as the root when browsing or uploading documents.", 1, @"~/Content", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );

            // Add specific block attribute values for the external site's RightSidebar layout.
            AddAllBlockAttributeValues();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Header Text, from Layout: Right Sidebar, Site: External Website
            DeleteBlock( "FE2ADC7E-6D1C-4C3C-8D33-C6328A0D4EA3" );
            // Remove Block: Login Status, from Layout: Right Sidebar, Site: External Website
            DeleteBlock( "ED8662A3-44A9-4181-8D0B-55087E1062D1" );
            // Remove Block: Footer Address, from Layout: Right Sidebar, Site: External Website
            DeleteBlock( "790557BF-E776-48AE-ABBC-E37CF18FF3FA" );
            // Remove Block: Footer Text, from Layout: Right Sidebar, Site: External Website
            DeleteBlock( "A490588D-8943-42A4-9DE5-B67BAB88C5FA" );
            // Remove Block: Page Menu, from Layout: Right Sidebar, Site: External Website
            DeleteBlock( "D4651935-E652-469D-8EBD-69FF3E684BA0" );
        }

        private void AddAllBlockAttributeValues()
        {
            // Attrib Value for Page/BlockPage??/Footer Address:Support Versions (FieldType: Boolean)
            AddBlockAttributeValue( "790557BF-E776-48AE-ABBC-E37CF18FF3FA", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", "False" );

            // Attrib Value for Page/BlockPage??/Footer Address:Require Approval (FieldType: Boolean)
            AddBlockAttributeValue( "790557BF-E776-48AE-ABBC-E37CF18FF3FA", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", "False" );

            // Attrib Value for Page/BlockPage??/Footer Address:Context Name (FieldType: Text)
            AddBlockAttributeValue( "790557BF-E776-48AE-ABBC-E37CF18FF3FA", "466993F7-D838-447A-97E7-8BBDA6A57289", "FooterAddress" );

            // Attrib Value for Page/BlockPage??/Footer Address:Context Parameter (FieldType: Text)
            AddBlockAttributeValue( "790557BF-E776-48AE-ABBC-E37CF18FF3FA", "3FFC512D-A576-4289-B648-905FD7A64ABB", "" );

            // Attrib Value for Page/BlockPage??/Footer Address:Cache Duration (FieldType: Integer)
            AddBlockAttributeValue( "790557BF-E776-48AE-ABBC-E37CF18FF3FA", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", "3600" );

            // Attrib Value for Page/BlockPage??/Footer Address:Use Code Editor (FieldType: Boolean)
            AddBlockAttributeValue( "790557BF-E776-48AE-ABBC-E37CF18FF3FA", "0673E015-F8DD-4A52-B380-C758011331B2", "False" );

            // Attrib Value for Page/BlockPage??/Header Text:Support Versions (FieldType: Boolean)
            AddBlockAttributeValue( "FE2ADC7E-6D1C-4C3C-8D33-C6328A0D4EA3", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", "False" );

            // Attrib Value for Page/BlockPage??/Header Text:Require Approval (FieldType: Boolean)
            AddBlockAttributeValue( "FE2ADC7E-6D1C-4C3C-8D33-C6328A0D4EA3", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", "False" );

            // Attrib Value for Page/BlockPage??/Header Text:Context Name (FieldType: Text)
            AddBlockAttributeValue( "FE2ADC7E-6D1C-4C3C-8D33-C6328A0D4EA3", "466993F7-D838-447A-97E7-8BBDA6A57289", "ExternalSiteHeaderText" );

            // Attrib Value for Page/BlockPage??/Header Text:Context Parameter (FieldType: Text)
            AddBlockAttributeValue( "FE2ADC7E-6D1C-4C3C-8D33-C6328A0D4EA3", "3FFC512D-A576-4289-B648-905FD7A64ABB", "" );

            // Attrib Value for Page/BlockPage??/Header Text:Cache Duration (FieldType: Integer)
            AddBlockAttributeValue( "FE2ADC7E-6D1C-4C3C-8D33-C6328A0D4EA3", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", "3600" );

            // Attrib Value for Page/BlockPage??/Header Text:Use Code Editor (FieldType: Boolean)
            AddBlockAttributeValue( "FE2ADC7E-6D1C-4C3C-8D33-C6328A0D4EA3", "0673E015-F8DD-4A52-B380-C758011331B2", "False" );

            // Attrib Value for Page/BlockPage??/Login Status:My Account Page (FieldType: Page Reference)
            AddBlockAttributeValue( "ED8662A3-44A9-4181-8D0B-55087E1062D1", "2A790D3A-3DB2-49C6-A218-87CD629397FA", "" );

            // Attrib Value for Page/BlockPage??/Footer Text:Support Versions (FieldType: Boolean)
            AddBlockAttributeValue( "A490588D-8943-42A4-9DE5-B67BAB88C5FA", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", "False" );

            // Attrib Value for Page/BlockPage??/Footer Text:Require Approval (FieldType: Boolean)
            AddBlockAttributeValue( "A490588D-8943-42A4-9DE5-B67BAB88C5FA", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", "False" );

            // Attrib Value for Page/BlockPage??/Footer Text:Context Name (FieldType: Text)
            AddBlockAttributeValue( "A490588D-8943-42A4-9DE5-B67BAB88C5FA", "466993F7-D838-447A-97E7-8BBDA6A57289", "ExternalSiteFooterText" );

            // Attrib Value for Page/BlockPage??/Footer Text:Context Parameter (FieldType: Text)
            AddBlockAttributeValue( "A490588D-8943-42A4-9DE5-B67BAB88C5FA", "3FFC512D-A576-4289-B648-905FD7A64ABB", "" );

            // Attrib Value for Page/BlockPage??/Footer Text:Cache Duration (FieldType: Integer)
            AddBlockAttributeValue( "A490588D-8943-42A4-9DE5-B67BAB88C5FA", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", "3600" );

            // Attrib Value for Page/BlockPage??/Page Menu:Template (FieldType: Code Editor)
            AddBlockAttributeValue( "D4651935-E652-469D-8EBD-69FF3E684BA0", "1322186A-862A-4CF1-B349-28ECB67229BA", @"
{% include 'PageNav' %}
" );

            // Attrib Value for Page/BlockPage??/Page Menu:Root Page (FieldType: Page Reference)
            AddBlockAttributeValue( "D4651935-E652-469D-8EBD-69FF3E684BA0", "41F1C42E-2395-4063-BD4F-031DF8D5B231", "85f25819-e948-4960-9ddf-00f54d32444e" );

            // Attrib Value for Page/BlockPage??/Page Menu:Number of Levels (FieldType: Text)
            AddBlockAttributeValue( "D4651935-E652-469D-8EBD-69FF3E684BA0", "6C952052-BC79-41BA-8B88-AB8EA3E99648", "1" );

            // Attrib Value for Page/BlockPage??/Page Menu:Is Secondary Block (FieldType: Boolean)
            AddBlockAttributeValue( "D4651935-E652-469D-8EBD-69FF3E684BA0", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", "False" );

            // Attrib Value for Page/BlockPage??/Page Menu:Include Current QueryString (FieldType: Boolean)
            AddBlockAttributeValue( "D4651935-E652-469D-8EBD-69FF3E684BA0", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", "True" );

            // Attrib Value for Page/BlockPage??/Page Menu:Include Current Parameters (FieldType: Boolean)
            AddBlockAttributeValue( "D4651935-E652-469D-8EBD-69FF3E684BA0", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", "True" );

            // Attrib Value for Page/BlockPage??/Page Menu:Enable Debug (FieldType: Boolean)
            AddBlockAttributeValue( "D4651935-E652-469D-8EBD-69FF3E684BA0", "2EF904CD-976E-4489-8C18-9BA43885ACD9", "False" );

            // Attrib Value for Page/BlockPage??/Page Menu:CSS File (FieldType: Text)
            AddBlockAttributeValue( "D4651935-E652-469D-8EBD-69FF3E684BA0", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", "" );
        }
    }
}
