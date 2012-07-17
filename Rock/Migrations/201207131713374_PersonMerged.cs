namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class PersonMerged : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "fiancialPersonAccountLookup", newName: "financialPersonAccountLookup");
            CreateTable(
                "crmPersonMerged",
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
            
            CreateIndex( "crmPersonMerged", "CurrentId" );

            DropIndex( "crmPersonTrail", new[] { "CurrentId" } );
            DropTable( "crmPersonTrail" );
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

            RenameTable(name: "financialPersonAccountLookup", newName: "fiancialPersonAccountLookup");
        }
    }
}
