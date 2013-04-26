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
    public partial class TransactionRefund : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.FinancialTransactionRefund",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        RefundTransactionId = c.Int(nullable: false),
                        RefundReasonValueId = c.Int(),
                        RefundReasonSummary = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FinancialTransaction", t => t.Id)
                .ForeignKey("dbo.FinancialTransaction", t => t.RefundTransactionId)
                .ForeignKey("dbo.DefinedValue", t => t.RefundReasonValueId)
                .Index(t => t.Id)
                .Index(t => t.RefundTransactionId)
                .Index(t => t.RefundReasonValueId);
            
            AddColumn("dbo.FinancialGateway", "EntityTypeId", c => c.Int(nullable: false));
            AlterColumn("dbo.FinancialAccount", "Name", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.FinancialBatch", "Name", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.FinancialGateway", "Name", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.FinancialPersonBankAccount", "AccountNumber", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.FinancialPersonSavedAccount", "Name", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.FinancialPersonSavedAccount", "TransactionCode", c => c.String(nullable: false, maxLength: 50));
            AddForeignKey("dbo.FinancialGateway", "EntityTypeId", "dbo.EntityType", "Id");
            CreateIndex("dbo.FinancialGateway", "EntityTypeId");
            DropColumn("dbo.FinancialGateway", "ApiUrl");
            DropColumn("dbo.FinancialGateway", "ApiKey");
            DropColumn("dbo.FinancialGateway", "ApiSecret");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.FinancialGateway", "ApiSecret", c => c.String(maxLength: 100));
            AddColumn("dbo.FinancialGateway", "ApiKey", c => c.String(maxLength: 100));
            AddColumn("dbo.FinancialGateway", "ApiUrl", c => c.String(maxLength: 100));
            DropIndex("dbo.FinancialTransactionRefund", new[] { "RefundReasonValueId" });
            DropIndex("dbo.FinancialTransactionRefund", new[] { "RefundTransactionId" });
            DropIndex("dbo.FinancialTransactionRefund", new[] { "Id" });
            DropIndex("dbo.FinancialGateway", new[] { "EntityTypeId" });
            DropForeignKey("dbo.FinancialTransactionRefund", "RefundReasonValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.FinancialTransactionRefund", "RefundTransactionId", "dbo.FinancialTransaction");
            DropForeignKey("dbo.FinancialTransactionRefund", "Id", "dbo.FinancialTransaction");
            DropForeignKey("dbo.FinancialGateway", "EntityTypeId", "dbo.EntityType");
            AlterColumn("dbo.FinancialPersonSavedAccount", "TransactionCode", c => c.String(maxLength: 50));
            AlterColumn("dbo.FinancialPersonSavedAccount", "Name", c => c.String(maxLength: 50));
            AlterColumn("dbo.FinancialPersonBankAccount", "AccountNumber", c => c.String(maxLength: 100));
            AlterColumn("dbo.FinancialGateway", "Name", c => c.String(maxLength: 50));
            AlterColumn("dbo.FinancialBatch", "Name", c => c.String(maxLength: 50));
            AlterColumn("dbo.FinancialAccount", "Name", c => c.String(maxLength: 50));
            DropColumn("dbo.FinancialGateway", "EntityTypeId");
            DropTable("dbo.FinancialTransactionRefund");
        }
    }
}
