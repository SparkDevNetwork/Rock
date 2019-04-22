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
    public partial class AssetManagerInHtmlEditor : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( true, "E7BD353C-91A6-4C15-A6C8-F44D0B16D16E","2E169330-D7D7-4ECA-B417-72C64BE150F0","HtmlEditor RockAssetManager Plugin Frame","","DEB88EA2-D0CE-47B2-9EB3-FDDDAC2C3389",""); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute("DEB88EA2-D0CE-47B2-9EB3-FDDDAC2C3389","htmleditorplugins/RockAssetManager","7B268B31-F67C-4187-AB99-0B13EB36D4BA");// for Page:HtmlEditor RockAssetManager Plugin Frame
            RockMigrationHelper.UpdateBlockType("HtmlEditor AssetManager","Block to be used as part of the RockAssetManager HtmlEditor Plugin","~/Blocks/Utility/HtmlEditorAssetManager.ascx","Utility","72FB6069-7C55-4B5C-B386-A1668C481BEF");
            // Add Block to Page: HtmlEditor RockAssetManager Plugin Frame Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "DEB88EA2-D0CE-47B2-9EB3-FDDDAC2C3389".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"72FB6069-7C55-4B5C-B386-A1668C481BEF".AsGuid(), "HtmlEditor AssetManager","Main",@"",@"",0,"C5839BE7-EB9B-4776-B786-D7128BDBF5A8"); 
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: HtmlEditor AssetManager, from Page: HtmlEditor RockAssetManager Plugin Frame, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("C5839BE7-EB9B-4776-B786-D7128BDBF5A8");
            RockMigrationHelper.DeleteBlockType("72FB6069-7C55-4B5C-B386-A1668C481BEF"); // HtmlEditor AssetManager
            RockMigrationHelper.DeletePage("DEB88EA2-D0CE-47B2-9EB3-FDDDAC2C3389"); //  Page: HtmlEditor RockAssetManager Plugin Frame, Layout: Blank, Site: Rock RMS
        }
    }
}
