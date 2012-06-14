namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class PaymentGateway : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "financialGateway",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        Description = c.String(maxLength: 500),
                        ApiUrl = c.String(maxLength: 100),
                        ApiKey = c.String(maxLength: 100),
                        ApiSecret = c.String(maxLength: 100),
                        ModifiedDateTime = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            AddForeignKey("financialTransaction", "GatewayId", "financialGateway", "Id");
            CreateIndex("financialTransaction", "GatewayId");
        }
        
        public override void Down()
        {
            DropIndex("financialGateway", new[] { "ModifiedByPersonId" });
            DropIndex("financialGateway", new[] { "CreatedByPersonId" });
            DropIndex("financialTransaction", new[] { "GatewayId" });
            DropForeignKey("financialGateway", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("financialGateway", "CreatedByPersonId", "crmPerson");
            DropForeignKey("financialTransaction", "GatewayId", "financialGateway");
            DropTable("financialGateway");
        }
    }
}
