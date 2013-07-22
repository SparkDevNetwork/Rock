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
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.BinaryFileType", new[] { "StorageEntityTypeId" });
            DropIndex("dbo.BinaryFile", new[] { "StorageEntityTypeId" });
            DropForeignKey("dbo.BinaryFileType", "StorageEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.BinaryFile", "StorageEntityTypeId", "dbo.EntityType");
            DropColumn("dbo.BinaryFileType", "StorageEntityTypeId");
            DropColumn("dbo.BinaryFile", "StorageEntityTypeId");
        }
    }
}
