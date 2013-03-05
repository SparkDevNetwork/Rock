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
    public partial class FinancialBatchNewFields : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
            AddColumn("dbo.FinancialBatch", "BatchDateStart", c => c.DateTime());
            AddColumn("dbo.FinancialBatch", "BatchDateEnd", c => c.DateTime());
            AddColumn("dbo.FinancialBatch", "BatchTypeValueId", c => c.Int(nullable: false));
            AddColumn("dbo.FinancialBatch", "ControlAmount", c => c.Single(nullable: false));
           
            DropColumn("dbo.FinancialBatch", "BatchDate");
           
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
           
            AddColumn("dbo.FinancialBatch", "BatchDate", c => c.DateTime());
            
            DropColumn("dbo.FinancialBatch", "ControlAmount");
            DropColumn("dbo.FinancialBatch", "BatchTypeValueId");
            DropColumn("dbo.FinancialBatch", "BatchDateEnd");
            DropColumn("dbo.FinancialBatch", "BatchDateStart");
        }
    }
}
