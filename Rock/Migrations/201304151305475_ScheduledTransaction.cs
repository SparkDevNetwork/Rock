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
    public partial class ScheduledTransaction : RockMigration_5
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.FinancialScheduledTransaction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AuthorizedPersonId = c.Int(nullable: false),
                        TransactionFrequencyValueId = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(),
                        NumberOfPayments = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        GatewayId = c.Int(),
                        TransactionCode = c.String(maxLength: 50),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.AuthorizedPersonId)
                .ForeignKey("dbo.FinancialGateway", t => t.GatewayId)
                .ForeignKey("dbo.DefinedValue", t => t.TransactionFrequencyValueId)
                .Index(t => t.AuthorizedPersonId)
                .Index(t => t.GatewayId)
                .Index(t => t.TransactionFrequencyValueId);
            
            CreateIndex( "dbo.FinancialScheduledTransaction", "Guid", true );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.FinancialScheduledTransaction", new[] { "TransactionFrequencyValueId" });
            DropIndex("dbo.FinancialScheduledTransaction", new[] { "GatewayId" });
            DropIndex("dbo.FinancialScheduledTransaction", new[] { "AuthorizedPersonId" });
            DropForeignKey("dbo.FinancialScheduledTransaction", "TransactionFrequencyValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.FinancialScheduledTransaction", "GatewayId", "dbo.FinancialGateway");
            DropForeignKey("dbo.FinancialScheduledTransaction", "AuthorizedPersonId", "dbo.Person");
            DropTable("dbo.FinancialScheduledTransaction");
        }
    }
}
