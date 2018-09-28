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
    public partial class PagesForHttpModules : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Web.HttpModules.ResponseHeaders", "Response Headers", "Rock.Web.HttpModules.ResponseHeaders, Rock, Version=1.8.0.11, Culture=neutral, PublicKeyToken=null", false, false, "EDE69F48-5E05-4260-B360-DA37DFD1AB83" );
            
            // Page: HTTP Modules              
            RockMigrationHelper.AddPage( true, "B4A24AB7-9369-4055-883F-4F4892C39AE3","D65F783D-87A9-4CC9-8110-E83466A0EADB","HTTP Modules","","39F928A5-1374-4380-B807-EADF145F18A1","fa fa-code"); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            RockMigrationHelper.UpdateBlockType( "Components", "Block to administrate MEF plugins.", "~/Blocks/Core/Components.ascx", "Core", "21F5F466-59BC-40B2-8D73-7314D936C3CB" );
            // Add Block to Page: HTTP Modules, Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "39F928A5-1374-4380-B807-EADF145F18A1","","21F5F466-59BC-40B2-8D73-7314D936C3CB","HTTP Module Components","Main","","",1,"77ACB436-ED40-42D9-84C0-8B049DB7012D");
            // Add Block to Page: HTTP Modules, Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "39F928A5-1374-4380-B807-EADF145F18A1","","19B61D65-37E3-459F-A44F-DEF0089118A3","Notes","Main","","",0,"22013002-2D9E-43B4-B807-714FB8F3D3EF");
            // Attrib for BlockType: HTML Content:Enabled Lava Commands              
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D","Enabled Lava Commands","EnabledLavaCommands","","The Lava commands that should be enabled for this HTML block.",0,@"","7146AC24-9250-4FC4-9DF2-9803B9A84299");
            // Attrib for BlockType: HTML Content:Entity Type              
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB","Entity Type","ContextEntityType","","The type of entity that will provide context for this block",0,@"","6783D47D-92F9-4F48-93C0-16111D675A0F");
            // Attrib for BlockType: Components:core.CustomGridColumnsConfig              
            RockMigrationHelper.UpdateBlockTypeAttribute("21F5F466-59BC-40B2-8D73-7314D936C3CB","9C204CD0-1233-41C5-818A-C5DA439445AA","core.CustomGridColumnsConfig","core.CustomGridColumnsConfig","","",0,@"","11507A9C-6ECB-4C7C-87AA-017512EE23D3");
            // Attrib for BlockType: Components:Component Container              
            RockMigrationHelper.UpdateBlockTypeAttribute("21F5F466-59BC-40B2-8D73-7314D936C3CB","9C204CD0-1233-41C5-818A-C5DA439445AA","Component Container","ComponentContainer","","The Rock Extension Managed Component Container to manage. For example: 'Rock.Search.SearchContainer, Rock'",1,@"","259AF14D-0214-4BE4-A7BF-40423EA07C99");
            // Attrib for BlockType: HTML Content:Use Code Editor              
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Use Code Editor","UseCodeEditor","","Use the code editor instead of the WYSIWYG editor",1,@"True","0673E015-F8DD-4A52-B380-C758011331B2");
            // Attrib for BlockType: HTML Content:Document Root Folder              
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","9C204CD0-1233-41C5-818A-C5DA439445AA","Document Root Folder","DocumentRootFolder","","The folder to use as the root when browsing or uploading documents.",2,@"~/Content","3BDB8AED-32C5-4879-B1CB-8FC7C8336534");
            // Attrib for BlockType: Components:Support Ordering              
            RockMigrationHelper.UpdateBlockTypeAttribute("21F5F466-59BC-40B2-8D73-7314D936C3CB","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Support Ordering","SupportOrdering","","Should user be allowed to re-order list of components?",2,@"True","A4889D7B-87AA-419D-846C-3E618E79D875");
            // Attrib for BlockType: Components:Support Security              
            RockMigrationHelper.UpdateBlockTypeAttribute("21F5F466-59BC-40B2-8D73-7314D936C3CB","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Support Security","SupportSecurity","","Should the user be allowed to configure security for the components?",3,@"True","A8F1D1B8-0709-497C-9DCB-44826F26AE7A");
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
            // Attrib for BlockType: HTML Content:Is Secondary Block              
            RockMigrationHelper.UpdateBlockTypeAttribute("19B61D65-37E3-459F-A44F-DEF0089118A3","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Is Secondary Block","IsSecondaryBlock","","Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.",12,@"False","04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4");
            // Attrib Value for Block:HTTP Module Components, Attribute:Component Container Page: HTTP Modules, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("77ACB436-ED40-42D9-84C0-8B049DB7012D","259AF14D-0214-4BE4-A7BF-40423EA07C99",@"Rock.Web.HttpModules.HttpModuleContainer, Rock");
            // Attrib Value for Block:HTTP Module Components, Attribute:Support Ordering Page: HTTP Modules, Site: Rock RMS             
            RockMigrationHelper.AddBlockAttributeValue("77ACB436-ED40-42D9-84C0-8B049DB7012D","A4889D7B-87AA-419D-846C-3E618E79D875",@"True");
            // Attrib Value for Block:HTTP Module Components, Attribute:core.CustomGridColumnsConfig Page: HTTP Modules, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("77ACB436-ED40-42D9-84C0-8B049DB7012D","11507A9C-6ECB-4C7C-87AA-017512EE23D3",@"");
            // Attrib Value for Block:HTTP Module Components, Attribute:Support Security Page: HTTP Modules, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("77ACB436-ED40-42D9-84C0-8B049DB7012D","A8F1D1B8-0709-497C-9DCB-44826F26AE7A",@"False");  
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "A8F1D1B8-0709-497C-9DCB-44826F26AE7A" );
            RockMigrationHelper.DeleteAttribute( "11507A9C-6ECB-4C7C-87AA-017512EE23D3" );
            RockMigrationHelper.DeleteAttribute( "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );
            RockMigrationHelper.DeleteAttribute( "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            RockMigrationHelper.DeleteAttribute( "6783D47D-92F9-4F48-93C0-16111D675A0F" );
            RockMigrationHelper.DeleteAttribute( "A4889D7B-87AA-419D-846C-3E618E79D875" );
            RockMigrationHelper.DeleteAttribute( "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );
            RockMigrationHelper.DeleteAttribute( "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE" );
            RockMigrationHelper.DeleteAttribute( "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E" );
            RockMigrationHelper.DeleteAttribute( "0673E015-F8DD-4A52-B380-C758011331B2" );
            RockMigrationHelper.DeleteAttribute( "466993F7-D838-447A-97E7-8BBDA6A57289" );
            RockMigrationHelper.DeleteAttribute( "3FFC512D-A576-4289-B648-905FD7A64ABB" );
            RockMigrationHelper.DeleteAttribute( "7C1CE199-86CF-4EAE-8AB3-848416A72C58" );
            RockMigrationHelper.DeleteAttribute( "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A" );
            RockMigrationHelper.DeleteAttribute( "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4" );
            RockMigrationHelper.DeleteAttribute( "259AF14D-0214-4BE4-A7BF-40423EA07C99" );
            RockMigrationHelper.DeleteBlock( "22013002-2D9E-43B4-B807-714FB8F3D3EF" );
            RockMigrationHelper.DeleteBlock( "77ACB436-ED40-42D9-84C0-8B049DB7012D" );
            RockMigrationHelper.DeleteBlockType( "21F5F466-59BC-40B2-8D73-7314D936C3CB" );
            RockMigrationHelper.DeleteBlockType( "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            RockMigrationHelper.DeletePage( "39F928A5-1374-4380-B807-EADF145F18A1" ); //  Page: HTTP Modules
        }
    }
}
