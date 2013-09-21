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
    public partial class BatchDates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.FinancialBatch", "BatchStartDateTime", c => c.DateTime());
            AddColumn("dbo.FinancialBatch", "BatchEndDateTime", c => c.DateTime());
            DropColumn("dbo.FinancialBatch", "BatchDate");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.FinancialBatch", "BatchDate", c => c.DateTime(storeType: "date"));
            DropColumn("dbo.FinancialBatch", "BatchEndDateTime");
            DropColumn("dbo.FinancialBatch", "BatchStartDateTime");
        }
    }
}
