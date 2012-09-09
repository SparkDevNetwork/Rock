namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTags : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.coreTag",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Entity = c.String(maxLength: 50),
                        EntityQualifierColumn = c.String(maxLength: 50),
                        EntityQualifierValue = c.String(maxLength: 200),
                        Name = c.String(nullable: false, maxLength: 100),
                        Order = c.Int(nullable: false),
                        OwnerId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.crmPerson", t => t.OwnerId)
                .ForeignKey("dbo.crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("dbo.crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.OwnerId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.coreTaggedItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        TagId = c.Int(nullable: false),
                        EntityId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.coreTag", t => t.TagId, cascadeDelete: true)
                .ForeignKey("dbo.crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("dbo.crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.TagId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.coreTaggedItem", new[] { "ModifiedByPersonId" });
            DropIndex("dbo.coreTaggedItem", new[] { "CreatedByPersonId" });
            DropIndex("dbo.coreTaggedItem", new[] { "TagId" });
            DropIndex("dbo.coreTag", new[] { "ModifiedByPersonId" });
            DropIndex("dbo.coreTag", new[] { "CreatedByPersonId" });
            DropIndex("dbo.coreTag", new[] { "OwnerId" });
            DropForeignKey("dbo.coreTaggedItem", "ModifiedByPersonId", "dbo.crmPerson");
            DropForeignKey("dbo.coreTaggedItem", "CreatedByPersonId", "dbo.crmPerson");
            DropForeignKey("dbo.coreTaggedItem", "TagId", "dbo.coreTag");
            DropForeignKey("dbo.coreTag", "ModifiedByPersonId", "dbo.crmPerson");
            DropForeignKey("dbo.coreTag", "CreatedByPersonId", "dbo.crmPerson");
            DropForeignKey("dbo.coreTag", "OwnerId", "dbo.crmPerson");
            DropTable("dbo.coreTaggedItem");
            DropTable("dbo.coreTag");
        }
    }
}
