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
    public partial class StorageEntityTypes : Rock.Migrations.RockMigration
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
