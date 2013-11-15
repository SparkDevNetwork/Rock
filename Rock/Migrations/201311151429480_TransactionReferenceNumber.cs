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
    public partial class TransactionReferenceNumber : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameColumn(table: "dbo.FinancialPersonSavedAccount", name: "ReferenceId", newName: "FinancialTransactionId");
            AddColumn("dbo.FinancialPersonSavedAccount", "ReferenceNumber", c => c.String());
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.FinancialPersonSavedAccount", "ReferenceNumber");
            RenameColumn(table: "dbo.FinancialPersonSavedAccount", name: "FinancialTransactionId", newName: "ReferenceId");
        }
    }
}
