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
    public partial class AddContentLibrarySystemPages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Page
            //  Internal Name: Content Libraries
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "B4A24AB7-9369-4055-883F-4F4892C39AE3","D65F783D-87A9-4CC9-8110-E83466A0EADB","Content Libraries","","40875E7E-B912-43FF-892B-6161C21F130B","fa fa-book-reader");

            // Add Page
            //  Internal Name: Content Library Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "40875E7E-B912-43FF-892B-6161C21F130B","D65F783D-87A9-4CC9-8110-E83466A0EADB","Content Library Detail","","9EB5FFB8-8BD6-4F64-9A27-7131D9AC76BF","");

#pragma warning disable CS0618 // Type or member is obsolete
            // Add Page Route
            //   Page:Content Libraries
            //   Route:admin/cms/content-libraries
            RockMigrationHelper.AddPageRoute("40875E7E-B912-43FF-892B-6161C21F130B","admin/cms/content-libraries","C4C57059-A51A-4D29-A791-E340436BA249");

            // Add Page Route
            //   Page:Content Library Detail
            //   Route:admin/cms/content-libraries/{ContentLibraryId}
            RockMigrationHelper.AddPageRoute( "9EB5FFB8-8BD6-4F64-9A27-7131D9AC76BF","admin/cms/content-libraries/{ContentLibraryId}","3287058E-E1E1-4445-9A00-85CC98509AF5" );
#pragma warning restore CS0618 // Type or member is obsolete

            // Add/Update BlockType
            //   Name: Content Library List
            //   Category: CMS
            //   Path: ~/Blocks/Cms/ContentLibraryList.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType("Content Library List","Lists all the content library entities.","~/Blocks/Cms/ContentLibraryList.ascx","CMS","F305FE35-2EFA-4653-AA1A-87AE990EAFEB");

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.ContentLibraryDetail
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Cms.ContentLibraryDetail", "Content Library Detail", "Rock.Blocks.Cms.ContentLibraryDetail, Rock.Blocks, Version=1.14.0.14, Culture=neutral, PublicKeyToken=null", false, false, "5C8A2E36-6CCC-42C7-8AAF-1C0B4A42B48B");

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.ContentLibraryView
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Cms.ContentLibraryView", "Content Library View", "Rock.Blocks.Cms.ContentLibraryView, Rock.Blocks, Version=1.14.0.14, Culture=neutral, PublicKeyToken=null", false, false, "16C3A9D7-DD61-4971-8FE0-EEE09AEF703F");

            // Add/Update Obsidian Block Type
            //   Name:Content Library Detail
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.ContentLibraryDetail
            RockMigrationHelper.UpdateMobileBlockType("Content Library Detail", "Displays the details of a particular content library.", "Rock.Blocks.Cms.ContentLibraryDetail", "CMS", "D840AE7E-7226-4D84-AFA9-3F2C84947BDD");

            // Add/Update Obsidian Block Type
            //   Name:Content Library View
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.ContentLibraryView
            RockMigrationHelper.UpdateMobileBlockType("Content Library View", "Displays the search results of a particular content library.", "Rock.Blocks.Cms.ContentLibraryView", "CMS", "CC387575-3530-4CD6-97E0-1F449DCA1869");

            // Add Block
            //  Block Name: Content Library List
            //  Page Name: Content Libraries
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "40875E7E-B912-43FF-892B-6161C21F130B".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"F305FE35-2EFA-4653-AA1A-87AE990EAFEB".AsGuid(), "Content Library List","Main",@"",@"",0,"427F428F-909C-48D0-9CC7-DD72D22DA557");

            // Add Block
            //  Block Name: Content Library Detail
            //  Page Name: Content Library Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "9EB5FFB8-8BD6-4F64-9A27-7131D9AC76BF".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"D840AE7E-7226-4D84-AFA9-3F2C84947BDD".AsGuid(), "Content Library Detail","Main",@"",@"",0,"E2DC5066-E44D-48C1-B0B9-EBF98BF88BB6");

            // Attribute for BlockType
            //   BlockType: Content Library List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F305FE35-2EFA-4653-AA1A-87AE990EAFEB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "F3280993-7E2F-4D57-A32D-6AFF30FC2D67" );

            // Add Block Attribute Value
            //   Block: Content Library List
            //   BlockType: Content Library List
            //   Category: CMS
            //   Block Location: Page=Content Libraries, Site=Rock RMS
            //   Attribute: Detail Page
            //   Attribute Value: 9eb5ffb8-8bd6-4f64-9a27-7131d9ac76bf
            RockMigrationHelper.AddBlockAttributeValue("427F428F-909C-48D0-9CC7-DD72D22DA557","F3280993-7E2F-4D57-A32D-6AFF30FC2D67",@"9eb5ffb8-8bd6-4f64-9a27-7131d9ac76bf");

            // Add the nightly job to re-index all libraries.
            AddIndexContentLibrariesJob();
        }

        private void AddIndexContentLibrariesJob()
        {
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.IndexContentLibraries'
                    AND [Guid] = '61F411F1-D77B-4FBD-B698-5EBA3A3AE14D'
            )
            BEGIN
                INSERT INTO [ServiceJob] (
                      [IsSystem]
                    , [IsActive]
                    , [Name]
                    , [Description]
                    , [Class]
                    , [CronExpression]
                    , [NotificationStatus]
                    , [Guid]
                ) VALUES (
                      0
                    , 1
                    , 'Index Content Libraries'
                    , 'This job will update the content library search indexes to match the current data.'
                    , 'Rock.Jobs.IndexContentLibraries'
                    , '0 45 4 1/1 * ? *'
                    , 1
                    , '61F411F1-D77B-4FBD-B698-5EBA3A3AE14D'
                );
            END" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Delete the nightly update index job.
            Sql( "DELETE FROM [ServiceJob] WHERE [Guid] = '61F411F1-D77B-4FBD-B698-5EBA3A3AE14D'" );

            // Attribute for BlockType
            //   BlockType: Content Library List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute("F3280993-7E2F-4D57-A32D-6AFF30FC2D67");

            // Remove Block
            //  Name: Content Library Detail, from Page: Content Library Detail, Site: Rock RMS
            //  from Page: Content Library Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("E2DC5066-E44D-48C1-B0B9-EBF98BF88BB6");

            // Remove Block
            //  Name: Content Library List, from Page: Content Libraries, Site: Rock RMS
            //  from Page: Content Libraries, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("427F428F-909C-48D0-9CC7-DD72D22DA557");

            // Delete BlockType
            //   Name: Content Library List
            //   Category: CMS
            //   Path: ~/Blocks/Cms/ContentLibraryList.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType("F305FE35-2EFA-4653-AA1A-87AE990EAFEB");

            // Delete BlockType
            //   Name: Content Library View
            //   Category: CMS
            //   Path: -
            //   EntityType: Content Library View
            RockMigrationHelper.DeleteBlockType("CC387575-3530-4CD6-97E0-1F449DCA1869");

            // Delete BlockType
            //   Name: Content Library Detail
            //   Category: CMS
            //   Path: -
            //   EntityType: Content Library Detail
            RockMigrationHelper.DeleteBlockType("D840AE7E-7226-4D84-AFA9-3F2C84947BDD");

            // Delete Page
            //  Internal Name: Content Library Detail
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage("9EB5FFB8-8BD6-4F64-9A27-7131D9AC76BF"); 

            // Delete Page
            //  Internal Name: Content Libraries
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage("40875E7E-B912-43FF-892B-6161C21F130B"); 
        }
    }
}
