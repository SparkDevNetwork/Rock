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
    public partial class RenameContentLibraryToContentCollection : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // EF Code commented out to perform a cleaner rename.
            //RenameTable(name: "dbo.ContentLibrary", newName: "ContentCollection");
            //RenameTable(name: "dbo.ContentLibrarySource", newName: "ContentCollectionSource");
            //RenameColumn(table: "dbo.ContentCollectionSource", name: "ContentLibraryId", newName: "ContentCollectionId");
            //RenameIndex(table: "dbo.ContentCollectionSource", name: "IX_ContentLibraryId", newName: "IX_ContentCollectionId");
            //AddColumn("dbo.ContentCollection", "CollectionKey", c => c.String(maxLength: 100));
            //DropColumn("dbo.ContentCollection", "LibraryKey");

            Sql( @"
exec sp_rename '[dbo].[ContentLibrary]', 'ContentCollection'
exec sp_rename '[dbo].[ContentCollection].[LibraryKey]', 'CollectionKey'
exec sp_rename '[dbo].[PK_dbo.ContentLibrary]', 'PK_dbo.ContentCollection'
exec sp_rename '[dbo].[FK_dbo.ContentLibrary_dbo.PersonAlias_CreatedByPersonAliasId]', 'FK_dbo.ContentCollection_dbo.PersonAlias_CreatedByPersonAliasId'
exec sp_rename '[dbo].[FK_dbo.ContentLibrary_dbo.PersonAlias_ModifiedByPersonAliasId]', 'FK_dbo.ContentCollection_dbo.PersonAlias_ModifiedByPersonAliasId'

exec sp_rename '[dbo].[ContentLibrarySource]', 'ContentCollectionSource'
exec sp_rename '[dbo].[ContentCollectionSource].[ContentLibraryId]', 'ContentCollectionId'
exec sp_rename '[dbo].[PK_dbo.ContentLibrarySource]', 'PK_dbo.ContentCollectionSource'
exec sp_rename '[dbo].[FK_dbo.ContentLibrarySource_dbo.ContentLibrary_ContentLibraryId]', 'FK_dbo.ContentCollectionSource_dbo.ContentCollection_ContentCollectionId'
exec sp_rename '[dbo].[FK_dbo.ContentLibrarySource_dbo.EntityType_EntityTypeId]', 'FK_dbo.ContentCollectionSource_dbo.EntityType_EntityTypeId'
exec sp_rename '[dbo].[FK_dbo.ContentLibrarySource_dbo.PersonAlias_CreatedByPersonAliasId]', 'FK_dbo.ContentCollectionSource_dbo.PersonAlias_CreatedByPersonAliasId'
exec sp_rename '[dbo].[FK_dbo.ContentLibrarySource_dbo.PersonAlias_ModifiedByPersonAliasId]', 'FK_dbo.ContentCollectionSource_dbo.PersonAlias_ModifiedByPersonAliasId'" );

            // Update all entity types.
            // Migration helper updates by name instead of Guid.
            Sql( @"
UPDATE [EntityType]
SET [Name] = 'Rock.Model.ContentCollection',
    [FriendlyName] = 'Content Collection',
    [AssemblyName] = 'Rock.Model.ContentCollection, Rock, Version=1.14.0.12, Culture=neutral, PublicKeyToken=null'
WHERE [Guid] = 'AD7B9219-1B47-4164-9DD1-90F0AF588CB8'" );

            Sql( @"
UPDATE [EntityType]
SET [Name] = 'Rock.Model.ContentCollectionSource',
    [FriendlyName] = 'Content Collection Source',
    [AssemblyName] = 'Rock.Model.ContentCollectionSource, Rock, Version=1.14.0.12, Culture=neutral, PublicKeyToken=null'
WHERE [Guid] = '46BD0E73-14B3-499D-B8BE-C0EF6BDCD733'" );

            Sql( @"
UPDATE [EntityType]
SET [Name] = 'Rock.Blocks.Cms.ContentCollectionDetail',
    [FriendlyName] = 'Content Collection Detail',
    [AssemblyName] = 'Rock.Blocks.Cms.ContentCollectionDetail, Rock.Blocks, Version=1.14.0.14, Culture=neutral, PublicKeyToken=null'
WHERE [Guid] = '5C8A2E36-6CCC-42C7-8AAF-1C0B4A42B48B'" );

            Sql( @"
UPDATE [EntityType]
SET [Name] = 'Rock.Blocks.Cms.ContentCollectionView',
    [FriendlyName] = 'Content Collection View',
    [AssemblyName] = 'Rock.Blocks.Cms.ContentCollectionView, Rock.Blocks, Version=1.14.0.14, Culture=neutral, PublicKeyToken=null'
WHERE [Guid] = '16C3A9D7-DD61-4971-8FE0-EEE09AEF703F'" );

            // Update all block types.
            RockMigrationHelper.UpdateBlockTypeByGuid( "Content Collection List",
                "Lists all the content collection entities.",
                "~/Blocks/Cms/ContentCollectionList.ascx",
                "CMS",
                "F305FE35-2EFA-4653-AA1A-87AE990EAFEB" );

            RockMigrationHelper.UpdateMobileBlockType( "Content Collection Detail",
                "Displays the details of a particular content collection.",
                "Rock.Blocks.Cms.ContentCollectionDetail",
                "CMS",
                "D840AE7E-7226-4D84-AFA9-3F2C84947BDD" );

            RockMigrationHelper.UpdateMobileBlockType( "Content Collection View",
                "Displays the search results of a particular content collection.",
                "Rock.Blocks.Cms.ContentCollectionView",
                "CMS",
                "CC387575-3530-4CD6-97E0-1F449DCA1869" );

            // Update REST controllers.
            Sql( @"
UPDATE [RestController]
SET [Name] = 'ContentCollections',
    [ClassName] = 'Rock.Rest.Controllers.ContentCollectionsController'
WHERE [Guid] = '948CB210-35E9-4556-A851-AF44852B9284'" );

            Sql( @"
UPDATE [RestController]
SET [Name] = 'ContentCollectionSources',
    [ClassName] = 'Rock.Rest.Controllers.ContentCollectionSourcesController'
WHERE [Guid] = '73E391AA-8F89-4883-AEDC-7DA18C86EE98'" );

            // Update Pages.
            Sql( @"
UPDATE [Page]
SET [InternalName] = 'Content Collections',
    [PageTitle] = 'Content Collections',
    [BrowserTitle] = 'Content Collections'
WHERE [Guid] = '40875E7E-B912-43FF-892B-6161C21F130B'" );

            Sql( @"
UPDATE [Page]
SET [InternalName] = 'Content Collection Detail',
    [PageTitle] = 'Content Collection Detail',
    [BrowserTitle] = 'Content Collection Detail'
WHERE [Guid] = '9EB5FFB8-8BD6-4F64-9A27-7131D9AC76BF'" );

            // Update Page Routes.
            RockMigrationHelper.UpdatePageRoute( "C4C57059-A51A-4D29-A791-E340436BA249", "40875E7E-B912-43FF-892B-6161C21F130B", "admin/cms/content-collections" );
            RockMigrationHelper.UpdatePageRoute( "3287058E-E1E1-4445-9A00-85CC98509AF5", "9EB5FFB8-8BD6-4F64-9A27-7131D9AC76BF", "admin/cms/content-collections/{ContentCollectionId}" );

            // Update Blocks.
            Sql( @"
UPDATE [Block]
SET [Name] = 'Content Collection List'
WHERE [Guid] = '427F428F-909C-48D0-9CC7-DD72D22DA557'" );

            Sql( @"
UPDATE [Block]
SET [Name] = 'Content Collection Detail'
WHERE [Guid] = 'E2DC5066-E44D-48C1-B0B9-EBF98BF88BB6'" );

            // Update service job.
            Sql( @"
UPDATE [ServiceJob]
SET [Class] = 'Rock.Jobs.IndexContentCollections',
    [Name] = 'Index Content Collections',
    [Description] = 'A job that updates the content collections search index.'
WHERE [Guid] = '61F411F1-D77B-4FBD-B698-5EBA3A3AE14D'" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Update service job.
            Sql( @"
UPDATE [ServiceJob]
SET [Class] = 'Rock.Jobs.IndexContentLibraries'
WHERE [Guid] = '61F411F1-D77B-4FBD-B698-5EBA3A3AE14D'" );

            // Update Blocks.
            Sql( @"
UPDATE [Block]
SET [Name] = 'Content Library List'
WHERE [Guid] = '427F428F-909C-48D0-9CC7-DD72D22DA557'" );

            Sql( @"
UPDATE [Block]
SET [Name] = 'Content Library Detail'
WHERE [Guid] = 'E2DC5066-E44D-48C1-B0B9-EBF98BF88BB6'" );

            // Update Page Routes.
            RockMigrationHelper.UpdatePageRoute( "C4C57059-A51A-4D29-A791-E340436BA249", "40875E7E-B912-43FF-892B-6161C21F130B", "admin/cms/content-libraries" );
            RockMigrationHelper.UpdatePageRoute( "3287058E-E1E1-4445-9A00-85CC98509AF5", "9EB5FFB8-8BD6-4F64-9A27-7131D9AC76BF", "admin/cms/content-libraries/{ContentCollectionId}" );

            // Update Pages.
            Sql( @"
UPDATE [Page]
SET [InternalName] = 'Content Libraries',
    [PageTitle] = 'Content Libraries',
    [BrowserTitle] = 'Content Libraries'
WHERE [Guid] = '40875E7E-B912-43FF-892B-6161C21F130B'" );

            Sql( @"
UPDATE [Page]
SET [InternalName] = 'Content Library Detail',
    [PageTitle] = 'Content Library Detail',
    [BrowserTitle] = 'Content Library Detail'
WHERE [Guid] = '9EB5FFB8-8BD6-4F64-9A27-7131D9AC76BF'" );

            // Update REST controllers.
            Sql( @"UPDATE [RestController] SET [Name] = 'ContentLibraries', [ClassName] = 'Rock.Rest.Controllers.ContentLibrariesController' WHERE [Guid] = '948CB210-35E9-4556-A851-AF44852B9284'" );
            Sql( @"UPDATE [RestController] SET [Name] = 'ContentLibrarySources', [ClassName] = 'Rock.Rest.Controllers.ContentLibrarySourcesController' WHERE [Guid] = '73E391AA-8F89-4883-AEDC-7DA18C86EE98'" );

            // Update all block types.
            RockMigrationHelper.UpdateBlockTypeByGuid( "Content Library List",
                "Lists all the content library entities.",
                "~/Blocks/Cms/ContentCollectionList.ascx",
                "CMS",
                "F305FE35-2EFA-4653-AA1A-87AE990EAFEB" );

            RockMigrationHelper.UpdateMobileBlockType( "Content Library Detail",
                "Displays the details of a particular content library.",
                "Rock.Blocks.Cms.ContentCollectionDetail", // We have not restored entity type name yet, so this is correct.
                "CMS",
                "D840AE7E-7226-4D84-AFA9-3F2C84947BDD" );

            RockMigrationHelper.UpdateMobileBlockType( "Content Library View",
                "Displays the search results of a particular content library.",
                "Rock.Blocks.Cms.ContentCollectionView", // We have not restored entity type name yet, so this is correct.
                "CMS",
                "CC387575-3530-4CD6-97E0-1F449DCA1869" );

            // Migration helper updates by name instead of Guid.
            Sql( @"
UPDATE [EntityType]
SET [Name] = 'Rock.Model.ContentLibrary',
    [FriendlyName] = 'Content Library',
    [AssemblyName] = 'Rock.Model.ContentLibrary, Rock, Version=1.14.0.12, Culture=neutral, PublicKeyToken=null'
WHERE [Guid] = 'AD7B9219-1B47-4164-9DD1-90F0AF588CB8'" );

            Sql( @"
UPDATE [EntityType]
SET [Name] = 'Rock.Model.ContentLibrarySource',
    [FriendlyName] = 'Content Library Source',
    [AssemblyName] = 'Rock.Model.ContentLibrarySource, Rock, Version=1.14.0.12, Culture=neutral, PublicKeyToken=null'
WHERE [Guid] = '46BD0E73-14B3-499D-B8BE-C0EF6BDCD733'" );

            Sql( @"
UPDATE [EntityType]
SET [Name] = 'Rock.Blocks.Cms.ContentLibraryDetail',
    [FriendlyName] = 'Content Library Detail',
    [AssemblyName] = 'Rock.Blocks.Cms.ContentLibraryDetail, Rock.Blocks, Version=1.14.0.14, Culture=neutral, PublicKeyToken=null'
WHERE [Guid] = '5C8A2E36-6CCC-42C7-8AAF-1C0B4A42B48B'" );

            Sql( @"
UPDATE [EntityType]
SET [Name] = 'Rock.Blocks.Cms.ContentLibraryView',
    [FriendlyName] = 'Content Library View',
    [AssemblyName] = 'Rock.Blocks.Cms.ContentLibraryView, Rock.Blocks, Version=1.14.0.14, Culture=neutral, PublicKeyToken=null'
WHERE [Guid] = '16C3A9D7-DD61-4971-8FE0-EEE09AEF703F'" );

            // EF Code commented out to perform a cleaner rename.
            //AddColumn("dbo.ContentCollection", "LibraryKey", c => c.String(maxLength: 100));
            //DropColumn("dbo.ContentCollection", "CollectionKey");
            //RenameIndex(table: "dbo.ContentCollectionSource", name: "IX_ContentCollectionId", newName: "IX_ContentLibraryId");
            //RenameColumn(table: "dbo.ContentCollectionSource", name: "ContentCollectionId", newName: "ContentLibraryId");
            //RenameTable(name: "dbo.ContentCollectionSource", newName: "ContentLibrarySource");
            //RenameTable(name: "dbo.ContentCollection", newName: "ContentLibrary");

            Sql( @"
exec sp_rename '[dbo].[ContentCollection]', 'ContentLibrary'
exec sp_rename '[dbo].[ContentLibrary].[CollectionKey]', 'LibraryKey'
exec sp_rename '[dbo].[PK_dbo.ContentCollection]', 'PK_dbo.ContentLibrary'
exec sp_rename '[dbo].[FK_dbo.ContentCollection_dbo.PersonAlias_CreatedByPersonAliasId]', 'FK_dbo.ContentLibrary_dbo.PersonAlias_CreatedByPersonAliasId'
exec sp_rename '[dbo].[FK_dbo.ContentCollection_dbo.PersonAlias_ModifiedByPersonAliasId]', 'FK_dbo.ContentLibrary_dbo.PersonAlias_ModifiedByPersonAliasId'

exec sp_rename '[dbo].[ContentCollectionSource]', 'ContentLibrarySource'
exec sp_rename '[dbo].[ContentLibrarySource].[ContentCollectionId]', 'ContentLibraryId'
exec sp_rename '[dbo].[PK_dbo.ContentCollectionSource]', 'PK_dbo.ContentLibrarySource'
exec sp_rename '[dbo].[FK_dbo.ContentCollectionSource_dbo.ContentCollection_ContentCollectionId]', 'FK_dbo.ContentLibrarySource_dbo.ContentLibrary_ContentLibraryId'
exec sp_rename '[dbo].[FK_dbo.ContentCollectionSource_dbo.EntityType_EntityTypeId]', 'FK_dbo.ContentLibrarySource_dbo.EntityType_EntityTypeId'
exec sp_rename '[dbo].[FK_dbo.ContentCollectionSource_dbo.PersonAlias_CreatedByPersonAliasId]', 'FK_dbo.ContentLibrarySource_dbo.PersonAlias_CreatedByPersonAliasId'
exec sp_rename '[dbo].[FK_dbo.ContentCollectionSource_dbo.PersonAlias_ModifiedByPersonAliasId]', 'FK_dbo.ContentLibrarySource_dbo.PersonAlias_ModifiedByPersonAliasId'" );
        }
    }
}
