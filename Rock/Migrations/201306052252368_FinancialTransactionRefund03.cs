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
    public partial class FinancialTransactionRefund03 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.FinancialTransactionRefund", "RefundTransactionId", "dbo.FinancialTransaction");
            DropIndex("dbo.FinancialTransactionRefund", new[] { "RefundTransactionId" });
            AlterColumn("dbo.FinancialTransactionRefund", "Id", c => c.Int(nullable: false));
            DropColumn("dbo.FinancialTransactionRefund", "RefundTransactionId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.FinancialTransactionRefund", "RefundTransactionId", c => c.Int(nullable: false));
            AlterColumn("dbo.FinancialTransactionRefund", "Id", c => c.Int(nullable: false, identity: true));
            CreateIndex("dbo.FinancialTransactionRefund", "RefundTransactionId");
            AddForeignKey("dbo.FinancialTransactionRefund", "RefundTransactionId", "dbo.FinancialTransaction", "Id");
        }
    }
}
