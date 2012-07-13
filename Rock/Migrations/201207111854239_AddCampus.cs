namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddCampus : DbMigration
    {
        public override void Up()
        {
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
            
        }
        
        public override void Down()
        {
            DropTable("crmCampus");
        }
    }
}
