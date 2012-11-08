namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class AddCampus : DbMigration
    {
        public override void Up()
        {
            RenameTable( name: "dbo.fiancialPersonAccountLookup", newName: "financialPersonAccountLookup" );
            CreateTable(
                "dbo.crmCampus",
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
                "dbo.crmPersonMerged",
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
            DropTable( "dbo.crmPersonTrail" );
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.crmPersonTrail",
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
            DropTable( "dbo.crmPersonMerged" );

            DropTable( "dbo.crmCampus" );
            RenameTable( name: "dbo.financialPersonAccountLookup", newName: "fiancialPersonAccountLookup" );
        }
    }
}
