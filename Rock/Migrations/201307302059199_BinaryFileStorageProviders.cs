//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class BinaryFileStorageProviders : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.BinaryFile", "StorageEntityTypeId", c => c.Int());
            AddColumn("dbo.BinaryFileType", "StorageEntityTypeId", c => c.Int());
            AddForeignKey("dbo.BinaryFile", "StorageEntityTypeId", "dbo.EntityType", "Id");
            AddForeignKey("dbo.BinaryFileType", "StorageEntityTypeId", "dbo.EntityType", "Id");
            CreateIndex("dbo.BinaryFile", "StorageEntityTypeId");
            CreateIndex("dbo.BinaryFileType", "StorageEntityTypeId");

            UpdateEntityType( "Rock.BinaryFile.Storage.Database", "Database",
                "Rock.BinaryFile.Storage.Database, Rock, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                false, true, "0AA42802-04FD-4AEC-B011-FEB127FC85CD" );
            UpdateEntityType( "Rock.BinaryFile.Storage.FileSystem", "FileSystem",
                "Rock.BinaryFile.Storage.FileSystem, Rock, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                false, true, "A97B6002-454E-4890-B529-B99F8F2F376A" );

            AddEntityAttribute( "Rock.BinaryFile.Storage.Database", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should service be used?", 0, "False", "DD6006C1-27D4-41EF-914F-5BED787B9428" );
            AddEntityAttribute( "Rock.BinaryFile.Storage.FileSystem", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should service be used?", 0, "False", "58BB64FB-C3E6-4042-B767-08023F5066AE" );
            AddEntityAttribute( "Rock.BinaryFile.Storage.FileSystem", "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "RootPath", "", "Root path where the files will be stored on disk.", 1, "", "7F09424A-454F-4F25-9FDD-4888894A7992" );
            AddAttributeValue( "DD6006C1-27D4-41EF-914F-5BED787B9428", 0, "True", "34BDA3D9-AF8B-4D57-8218-F1578E7A4C2A" );
            AddAttributeValue( "58BB64FB-C3E6-4042-B767-08023F5066AE", 0, "True", "6C6EF3AE-2E12-4C9C-A223-A409354E055D" );
            AddAttributeValue( "7F09424A-454F-4F25-9FDD-4888894A7992", 0, "~/Assets/Uploads", "04C9E6AF-F109-41DA-8353-C4E01FAF6963" );

            AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "File Storage Providers", "", "Default", "FEA8D6FC-B26F-48D5-BE69-6BCEF7CDC4E5", "icon-hdd" );
            AddBlock( "FEA8D6FC-B26F-48D5-BE69-6BCEF7CDC4E5", "21F5F466-59BC-40B2-8D73-7314D936C3CB", "File Storage Provider Configuration", "", "Content", 0, "8966CAFE-D8FC-4703-8960-17CB5807A3B8" );
            AddBlockAttributeValue( "8966CAFE-D8FC-4703-8960-17CB5807A3B8", "259AF14D-0214-4BE4-A7BF-40423EA07C99", "Rock.BinaryFile.StorageContainer, Rock" );

            // By default, set all system BinaryFileTypes to use Database file storage provider
            Sql( @"
declare @ProviderId int
select @ProviderId = (select Id from EntityType where [Guid] = '0AA42802-04FD-4AEC-B011-FEB127FC85CD')
update BinaryFileType
set StorageEntityTypeId = @ProviderId
where IsSystem = 1" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlockAttributeValue( "8966CAFE-D8FC-4703-8960-17CB5807A3B8", "259AF14D-0214-4BE4-A7BF-40423EA07C99" );
            DeleteBlock( "8966CAFE-D8FC-4703-8960-17CB5807A3B8" ); // File Storage Provider Configuration
            DeletePage( "FEA8D6FC-B26F-48D5-BE69-6BCEF7CDC4E5" ); // File Storage Providers

            DeleteAttribute( "DD6006C1-27D4-41EF-914F-5BED787B9428" );
            DeleteAttribute( "58BB64FB-C3E6-4042-B767-08023F5066AE" );
            DeleteAttribute( "7F09424A-454F-4F25-9FDD-4888894A7992" );

            DeleteEntityType( "0AA42802-04FD-4AEC-B011-FEB127FC85CDA" );
            DeleteEntityType( "A97B6002-454E-4890-B529-B99F8F2F376A" );

            DropIndex("dbo.BinaryFileType", new[] { "StorageEntityTypeId" });
            DropIndex("dbo.BinaryFile", new[] { "StorageEntityTypeId" });
            DropForeignKey("dbo.BinaryFileType", "StorageEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.BinaryFile", "StorageEntityTypeId", "dbo.EntityType");
            DropColumn("dbo.BinaryFileType", "StorageEntityTypeId");
            DropColumn("dbo.BinaryFile", "StorageEntityTypeId");
        }
    }
}
