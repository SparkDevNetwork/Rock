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
    public partial class RemoveEntityChange : RockMigration_3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.EntityChange", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.EntityChange", "CreatedByPersonId", "dbo.Person");
            DropIndex("dbo.EntityChange", new[] { "EntityTypeId" });
            DropIndex("dbo.EntityChange", new[] { "CreatedByPersonId" });
            DropTable("dbo.EntityChange");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CreateTable(
                "dbo.EntityChange",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ChangeSet = c.Guid(nullable: false),
                        ChangeType = c.String(nullable: false, maxLength: 10),
                        EntityTypeId = c.Int(nullable: false),
                        EntityId = c.Int(nullable: false),
                        Property = c.String(nullable: false, maxLength: 100),
                        OriginalValue = c.String(),
                        CurrentValue = c.String(),
                        CreatedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            // TableName: EntityChange
            // The given key was not present in the dictionary.
            // TableName: EntityChange
            // The given key was not present in the dictionary.
            // TableName: EntityChange
            // The given key was not present in the dictionary.
            // TableName: EntityChange
            // The given key was not present in the dictionary.
            // TableName: EntityChange
            // The given key was not present in the dictionary.
            // TableName: EntityChange
            // The given key was not present in the dictionary.
            // TableName: EntityChange
            // The given key was not present in the dictionary.
            // TableName: EntityChange
            // The given key was not present in the dictionary.
            // TableName: EntityChange
            // The given key was not present in the dictionary.
            // TableName: EntityChange
            // The given key was not present in the dictionary.
            // TableName: EntityChange
            // The given key was not present in the dictionary.
            CreateIndex("dbo.EntityChange", "CreatedByPersonId");
            CreateIndex("dbo.EntityChange", "EntityTypeId");
            AddForeignKey("dbo.EntityChange", "CreatedByPersonId", "dbo.Person", "Id", cascadeDelete: true);
            AddForeignKey("dbo.EntityChange", "EntityTypeId", "dbo.EntityType", "Id");
        }
    }
}
