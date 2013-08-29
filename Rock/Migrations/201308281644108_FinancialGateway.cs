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
    public partial class FinancialGateway : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.FinancialGateway", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.FinancialTransaction", "GatewayId", "dbo.FinancialGateway");
            DropForeignKey("dbo.FinancialPersonSavedAccount", "GatewayId", "dbo.FinancialGateway");
            DropForeignKey("dbo.FinancialScheduledTransaction", "GatewayId", "dbo.FinancialGateway");
            DropIndex("dbo.FinancialGateway", new[] { "EntityTypeId" });
            DropIndex("dbo.FinancialTransaction", new[] { "GatewayId" });
            DropIndex("dbo.FinancialPersonSavedAccount", new[] { "GatewayId" });
            DropIndex("dbo.FinancialScheduledTransaction", new[] { "GatewayId" });
            AddColumn("dbo.FinancialTransaction", "GatewayEntityTypeId", c => c.Int());
            AddColumn("dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId", c => c.Int());
            AddColumn("dbo.FinancialScheduledTransaction", "GatewayEntityTypeId", c => c.Int());
            CreateIndex("dbo.FinancialTransaction", "GatewayEntityTypeId");
            CreateIndex("dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId");
            CreateIndex("dbo.FinancialScheduledTransaction", "GatewayEntityTypeId");
            AddForeignKey("dbo.FinancialTransaction", "GatewayEntityTypeId", "dbo.EntityType", "Id");
            AddForeignKey("dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId", "dbo.EntityType", "Id");
            AddForeignKey("dbo.FinancialScheduledTransaction", "GatewayEntityTypeId", "dbo.EntityType", "Id");
            DropColumn("dbo.FinancialTransaction", "GatewayId");
            DropColumn("dbo.FinancialPersonSavedAccount", "GatewayId");
            DropColumn("dbo.FinancialScheduledTransaction", "GatewayId");
            DropTable("dbo.FinancialGateway");

            // Delete Payment Type defined type (not needed)
            DeleteDefinedType( "23E80D98-017E-47B9-BAF3-AC442A1EC3EE" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddDefinedType( "Financial", "Payment Type", "The type of payment associated with a transaction", "23E80D98-017E-47B9-BAF3-AC442A1EC3EE" );
            AddDefinedValue( "23E80D98-017E-47B9-BAF3-AC442A1EC3EE", "Credit Card", "Credit Card payment type", "09412338-AAAA-4644-BA2A-4CADBE653468" );
            AddDefinedValue( "23E80D98-017E-47B9-BAF3-AC442A1EC3EE", "Checking/ACH", "Checking/ACH payment type", "FFAD975C-7504-418F-8959-30BD0C62CD30" );

            CreateTable(
                "dbo.FinancialGateway",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(),
                        EntityTypeId = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.FinancialScheduledTransaction", "GatewayId", c => c.Int());
            AddColumn("dbo.FinancialPersonSavedAccount", "GatewayId", c => c.Int());
            AddColumn("dbo.FinancialTransaction", "GatewayId", c => c.Int());
            DropForeignKey("dbo.FinancialScheduledTransaction", "GatewayEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.FinancialTransaction", "GatewayEntityTypeId", "dbo.EntityType");
            DropIndex("dbo.FinancialScheduledTransaction", new[] { "GatewayEntityTypeId" });
            DropIndex("dbo.FinancialPersonSavedAccount", new[] { "GatewayEntityTypeId" });
            DropIndex("dbo.FinancialTransaction", new[] { "GatewayEntityTypeId" });
            DropColumn("dbo.FinancialScheduledTransaction", "GatewayEntityTypeId");
            DropColumn("dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId");
            DropColumn("dbo.FinancialTransaction", "GatewayEntityTypeId");
            CreateIndex("dbo.FinancialScheduledTransaction", "GatewayId");
            CreateIndex("dbo.FinancialPersonSavedAccount", "GatewayId");
            CreateIndex("dbo.FinancialTransaction", "GatewayId");
            CreateIndex("dbo.FinancialGateway", "EntityTypeId");
            AddForeignKey("dbo.FinancialScheduledTransaction", "GatewayId", "dbo.FinancialGateway", "Id");
            AddForeignKey("dbo.FinancialPersonSavedAccount", "GatewayId", "dbo.FinancialGateway", "Id");
            AddForeignKey("dbo.FinancialTransaction", "GatewayId", "dbo.FinancialGateway", "Id");
            AddForeignKey("dbo.FinancialGateway", "EntityTypeId", "dbo.EntityType", "Id");
        }
    }
}
