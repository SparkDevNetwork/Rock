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
    public partial class BatchTransaction : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.FinancialBatch", "BatchType_Id", c => c.Int());
            AddForeignKey("dbo.FinancialBatch", "CampusId", "dbo.Campus", "Id");
            AddForeignKey("dbo.FinancialBatch", "BatchType_Id", "dbo.DefinedType", "Id");
            CreateIndex("dbo.FinancialBatch", "CampusId");
            CreateIndex("dbo.FinancialBatch", "BatchType_Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.FinancialBatch", new[] { "BatchType_Id" });
            DropIndex("dbo.FinancialBatch", new[] { "CampusId" });
            DropForeignKey("dbo.FinancialBatch", "BatchType_Id", "dbo.DefinedType");
            DropForeignKey("dbo.FinancialBatch", "CampusId", "dbo.Campus");
            DropColumn("dbo.FinancialBatch", "BatchType_Id");
        }
    }
}
