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
    public partial class AddMissingTagConfiguration : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.Tag", "EntityTypeId", "dbo.EntityType");
            DropIndex("dbo.Tag", new[] { "EntityTypeId" });
            AddForeignKey("dbo.Tag", "EntityTypeId", "dbo.EntityType", "Id");
            CreateIndex("dbo.Tag", "EntityTypeId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.Tag", new[] { "EntityTypeId" });
            DropForeignKey("dbo.Tag", "EntityTypeId", "dbo.EntityType");
            CreateIndex("dbo.Tag", "EntityTypeId");
            AddForeignKey("dbo.Tag", "EntityTypeId", "dbo.EntityType", "Id", cascadeDelete: true);
        }
    }
}
