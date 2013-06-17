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
    public partial class FinancialPersonAccount : RockMigration_5
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.FinancialPersonBankAccount",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonId = c.Int(nullable: false),
                        AccountNumber = c.String(maxLength: 100),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId, cascadeDelete: true)
                .Index(t => t.PersonId);
            
            CreateIndex( "dbo.FinancialPersonBankAccount", "Guid", true );
            CreateTable(
                "dbo.FinancialPersonSavedAccount",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonId = c.Int(nullable: false),
                        GatewayId = c.Int(),
                        Name = c.String(maxLength: 50),
                        PaymentMethod = c.Int(nullable: false),
                        MaskedAccountNumber = c.String(maxLength: 100),
                        TransactionCode = c.String(maxLength: 50),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId, cascadeDelete: true)
                .ForeignKey("dbo.FinancialGateway", t => t.GatewayId)
                .Index(t => t.PersonId)
                .Index(t => t.GatewayId);
            
            CreateIndex( "dbo.FinancialPersonSavedAccount", "Guid", true );
            CreateTable(
                "dbo.FinancialScheduledTransactionDetail",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ScheduledTransactionId = c.Int(nullable: false),
                        AccountId = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Summary = c.String(maxLength: 500),
                        EntityTypeId = c.Int(),
                        EntityId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FinancialScheduledTransaction", t => t.ScheduledTransactionId, cascadeDelete: true)
                .ForeignKey("dbo.FinancialAccount", t => t.AccountId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .Index(t => t.ScheduledTransactionId)
                .Index(t => t.AccountId)
                .Index(t => t.EntityTypeId);
            
            CreateIndex( "dbo.FinancialScheduledTransactionDetail", "Guid", true );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.FinancialScheduledTransactionDetail", new[] { "EntityTypeId" });
            DropIndex("dbo.FinancialScheduledTransactionDetail", new[] { "AccountId" });
            DropIndex("dbo.FinancialScheduledTransactionDetail", new[] { "ScheduledTransactionId" });
            DropIndex("dbo.FinancialPersonSavedAccount", new[] { "GatewayId" });
            DropIndex("dbo.FinancialPersonSavedAccount", new[] { "PersonId" });
            DropIndex("dbo.FinancialPersonBankAccount", new[] { "PersonId" });
            DropForeignKey("dbo.FinancialScheduledTransactionDetail", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.FinancialScheduledTransactionDetail", "AccountId", "dbo.FinancialAccount");
            DropForeignKey("dbo.FinancialScheduledTransactionDetail", "ScheduledTransactionId", "dbo.FinancialScheduledTransaction");
            DropForeignKey("dbo.FinancialPersonSavedAccount", "GatewayId", "dbo.FinancialGateway");
            DropForeignKey("dbo.FinancialPersonSavedAccount", "PersonId", "dbo.Person");
            DropForeignKey("dbo.FinancialPersonBankAccount", "PersonId", "dbo.Person");
            DropTable("dbo.FinancialScheduledTransactionDetail");
            DropTable("dbo.FinancialPersonSavedAccount");
            DropTable("dbo.FinancialPersonBankAccount");
        }
    }
}
