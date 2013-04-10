namespace com.mychurch.MyFirstRockLibrary.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreatePotluckTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo._com_mychurch_PotluckDinner",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Guid = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        StartDateTime = c.DateTime(nullable: false),
                        EndDateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            // make sure to create index for Guid for new tables
            CreateIndex( "dbo._com_mychurch_PotluckDinner", "Guid", true );
            
            CreateTable(
                "dbo._com_mychurch_PotluckDish",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Guid = c.Guid(nullable: false),
                        PotluckDinnerId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Instructions = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._com_mychurch_PotluckDinner", t => t.PotluckDinnerId, cascadeDelete: true)
                .Index(t => t.PotluckDinnerId);

            // make sure to create index for Guid for new tables
            CreateIndex( "dbo._com_mychurch_PotluckDish", "Guid", true );
        }
        
        public override void Down()
        {
            DropForeignKey("dbo._com_mychurch_PotluckDish", "PotluckDinnerId", "dbo._com_mychurch_PotluckDinner");
            DropIndex("dbo._com_mychurch_PotluckDish", new[] { "PotluckDinnerId" });
            DropTable("dbo._com_mychurch_PotluckDish");
            DropTable("dbo._com_mychurch_PotluckDinner");
        }
    }
}
