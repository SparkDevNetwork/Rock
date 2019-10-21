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
    public partial class AddMobileAppPagesAndBlocks : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            SqlChangesUp();
            CmsUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CmsDown();
            SqlChangesDown();
        }

        /// <summary>
        /// Adds the EntityTypeId column to the BlockType and marks the Path column
        /// as not required.
        /// </summary>
        private void SqlChangesUp()
        {
            AddColumn( "dbo.BlockType", "EntityTypeId", c => c.Int() );
            AlterColumn( "dbo.BlockType", "Path", c => c.String( maxLength: 260 ) );
            CreateIndex( "dbo.BlockType", "EntityTypeId" );
            AddForeignKey( "dbo.BlockType", "EntityTypeId", "dbo.EntityType", "Id" );
        }

        /// <summary>
        /// Removes the EntityTypeId column from the BlockType table.
        /// </summary>
        private void SqlChangesDown()
        {
            DropForeignKey( "dbo.BlockType", "EntityTypeId", "dbo.EntityType" );
            DropIndex( "dbo.BlockType", new[] { "EntityTypeId" } );
            AlterColumn( "dbo.BlockType", "Path", c => c.String( nullable: false, maxLength: 260 ) );
            DropColumn( "dbo.BlockType", "EntityTypeId" );
        }

        private void CmsUp()
        {
            RockMigrationHelper.AddPage( true, "B4A24AB7-9369-4055-883F-4F4892C39AE3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Mobile Applications", "", "784259EC-46B7-4DE3-AC37-E8BFDB0B90A6", "fa fa-mobile" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "784259EC-46B7-4DE3-AC37-E8BFDB0B90A6", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Application Detail", "", "A4B0BCBB-721D-439C-8566-24F604DD4A1C", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "A4B0BCBB-721D-439C-8566-24F604DD4A1C", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Layouts", "", "5583A55D-7398-48E9-971F-6A1EF8158943", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "A4B0BCBB-721D-439C-8566-24F604DD4A1C", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Pages", "", "37E21200-DF91-4426-89CC-7D067237A037", "" ); // Site:Rock RMS

            // Hide the page from navigation.
            Sql( "UPDATE [Page] SET DisplayInNavWhen = 2 WHERE [Guid] = '784259EC-46B7-4DE3-AC37-E8BFDB0B90A6'" );

            RockMigrationHelper.UpdateBlockType( "Mobile Application Detail", "Edits and configures the settings of a mobile application.", "~/Blocks/Mobile/MobileApplicationDetail.ascx", "Mobile", "1D001ED9-F711-4820-BED0-92150D069BA2" );
            RockMigrationHelper.UpdateBlockType( "Mobile Layout Detail", "Edits and configures the settings of a mobile layout.", "~/Blocks/Mobile/MobileLayoutDetail.ascx", "Mobile", "74B6C64A-9617-4745-9928-ABAC7948A95D" );
            RockMigrationHelper.UpdateBlockType( "Mobile Page Detail", "Edits and configures the settings of a mobile page.", "~/Blocks/Mobile/MobilePageDetail.ascx", "Mobile", "E3C4547A-E29B-4CBA-9610-6C19D939183B" );

            // Add Block to Page: Mobile Applications Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "784259EC-46B7-4DE3-AC37-E8BFDB0B90A6".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "441D5A71-C250-4FF5-90C3-DEEAD3AC028D".AsGuid(), "Site List", "Main", @"", @"", 0, "BD30CBF7-3296-43B6-A98E-6EF6F2F12E51" );
            // Add Block to Page: Application Detail Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "A4B0BCBB-721D-439C-8566-24F604DD4A1C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "1D001ED9-F711-4820-BED0-92150D069BA2".AsGuid(), "Mobile Application Detail", "Main", @"", @"", 0, "48EDB434-8F21-4FC7-8599-9825F7A6103D" );
            // Add Block to Page: Layouts Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "5583A55D-7398-48E9-971F-6A1EF8158943".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "74B6C64A-9617-4745-9928-ABAC7948A95D".AsGuid(), "Mobile Layout Detail", "Main", @"", @"", 0, "C65112A2-D12C-4138-A5EF-A6B89902240B" );
            // Add Block to Page: Pages Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "37E21200-DF91-4426-89CC-7D067237A037".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "E3C4547A-E29B-4CBA-9610-6C19D939183B".AsGuid(), "Mobile Page Detail", "Main", @"", @"", 0, "3EC828F7-9243-429A-947E-AEC800AEEDB0" );

            // Attrib for BlockType: Mobile Application Detail:Layout Detail
            RockMigrationHelper.UpdateBlockTypeAttribute( "1D001ED9-F711-4820-BED0-92150D069BA2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Layout Detail", "LayoutDetail", "", @"", 0, @"", "D98E9C63-1C5D-47F1-9365-E04121A7D10E" );
            // Attrib for BlockType: Mobile Application Detail:Page Detail
            RockMigrationHelper.UpdateBlockTypeAttribute( "1D001ED9-F711-4820-BED0-92150D069BA2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Page Detail", "PageDetail", "", @"", 0, @"", "993CB65A-9812-46BE-A1ED-32F244D722CE" );

            // Attrib Value for Block:Site List, Attribute:Detail Page Page: Mobile Applications, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BD30CBF7-3296-43B6-A98E-6EF6F2F12E51", "BE7EEE0F-9B2B-4CF5-8714-29166025B3DD", @"a4b0bcbb-721d-439c-8566-24f604dd4a1c" );
            // Attrib Value for Block:Site List, Attribute:Site Type Page: Mobile Applications, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BD30CBF7-3296-43B6-A98E-6EF6F2F12E51", "786B9AA2-EA35-4C96-BA33-7A6F9945A10E", @"1" );

            // Attrib Value for Block:Mobile Application Detail, Attribute:Layout Detail Page: Application Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "48EDB434-8F21-4FC7-8599-9825F7A6103D", "D98E9C63-1C5D-47F1-9365-E04121A7D10E", @"5583a55d-7398-48e9-971f-6a1ef8158943" );
            // Attrib Value for Block:Mobile Application Detail, Attribute:Page Detail Page: Application Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "48EDB434-8F21-4FC7-8599-9825F7A6103D", "993CB65A-9812-46BE-A1ED-32F244D722CE", @"37e21200-df91-4426-89cc-7d067237a037" );
        }

        private void CmsDown()
        {
            // Attrib for BlockType: Mobile Application Detail:Page Detail
            RockMigrationHelper.DeleteAttribute( "993CB65A-9812-46BE-A1ED-32F244D722CE" );
            // Attrib for BlockType: Mobile Application Detail:Layout Detail
            RockMigrationHelper.DeleteAttribute( "D98E9C63-1C5D-47F1-9365-E04121A7D10E" );

            // Remove Block: Mobile Page Detail, from Page: Pages, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3EC828F7-9243-429A-947E-AEC800AEEDB0" );
            // Remove Block: Mobile Layout Detail, from Page: Layouts, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "C65112A2-D12C-4138-A5EF-A6B89902240B" );
            // Remove Block: Mobile Application Detail, from Page: Application Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "48EDB434-8F21-4FC7-8599-9825F7A6103D" );
            // Remove Block: Site List, from Page: Mobile Applications, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "BD30CBF7-3296-43B6-A98E-6EF6F2F12E51" );

            RockMigrationHelper.DeleteBlockType( "E3C4547A-E29B-4CBA-9610-6C19D939183B" ); // Mobile Page Detail
            RockMigrationHelper.DeleteBlockType( "74B6C64A-9617-4745-9928-ABAC7948A95D" ); // Mobile Layout Detail
            RockMigrationHelper.DeleteBlockType( "1D001ED9-F711-4820-BED0-92150D069BA2" ); // Mobile Application Detail

            RockMigrationHelper.DeletePage( "37E21200-DF91-4426-89CC-7D067237A037" ); //  Page: Pages, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "5583A55D-7398-48E9-971F-6A1EF8158943" ); //  Page: Layouts, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "A4B0BCBB-721D-439C-8566-24F604DD4A1C" ); //  Page: Application Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "784259EC-46B7-4DE3-AC37-E8BFDB0B90A6" ); //  Page: Mobile Applications, Layout: Full Width, Site: Rock RMS
        }
    }
}
