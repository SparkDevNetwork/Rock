namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class AddCampus : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "fiancialPersonAccountLookup", newName: "financialPersonAccountLookup");
            CreateTable(
                "crmCampus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        System = c.Boolean(nullable: false),
                        Name = c.String(maxLength: 100),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "crmPersonMerged",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: false),
                        CurrentId = c.Int(nullable: false),
                        CurrentGuid = c.Guid(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            CreateIndex( "crmPersonMerged", "CurrentId" );

            DropIndex( "crmPersonTrail", new[] { "CurrentId" } );
            DropTable("crmPersonTrail");
        }
        
        public override void Down()
        {
            CreateTable(
                "crmPersonTrail",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CurrentId = c.Int(nullable: false),
                        CurrentGuid = c.Guid(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            CreateIndex( "crmPersonTrail", "CurrentId" );

            DropIndex( "crmPersonMerged", new[] { "CurrentId" } );
            DropTable( "crmPersonMerged" );

            DropTable( "crmCampus" );
            RenameTable(name: "financialPersonAccountLookup", newName: "fiancialPersonAccountLookup");
        }
    }
}
