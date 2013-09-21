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
    public partial class CurrencyType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( "DELETE FinancialPersonSavedAccount" );

            DropForeignKey("dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId", "dbo.EntityType");
            DropIndex("dbo.FinancialPersonSavedAccount", new[] { "GatewayEntityTypeId" });
            AddColumn("dbo.FinancialPersonSavedAccount", "FinancialTransactionId", c => c.Int(nullable: false));
            CreateIndex("dbo.FinancialPersonSavedAccount", "FinancialTransactionId");
            AddForeignKey("dbo.FinancialPersonSavedAccount", "FinancialTransactionId", "dbo.FinancialTransaction", "Id", cascadeDelete: true);
            DropColumn("dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId");
            DropColumn("dbo.FinancialPersonSavedAccount", "PaymentMethod");
            DropColumn("dbo.FinancialPersonSavedAccount", "TransactionCode");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.FinancialPersonSavedAccount", "TransactionCode", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.FinancialPersonSavedAccount", "PaymentMethod", c => c.Int(nullable: false));
            AddColumn("dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId", c => c.Int());
            DropForeignKey("dbo.FinancialPersonSavedAccount", "FinancialTransactionId", "dbo.FinancialTransaction");
            DropIndex("dbo.FinancialPersonSavedAccount", new[] { "FinancialTransactionId" });
            DropColumn("dbo.FinancialPersonSavedAccount", "FinancialTransactionId");
            CreateIndex("dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId");
            AddForeignKey("dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId", "dbo.EntityType", "Id");
        }
    }
}
