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
    public partial class FinancialBatchPerson : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( "delete from dbo.FinancialBatch" );
            AddColumn("dbo.FinancialBatch", "CreatedByPersonId", c => c.Int(nullable: false));
            AddForeignKey("dbo.FinancialBatch", "CreatedByPersonId", "dbo.Person", "Id");
            CreateIndex("dbo.FinancialBatch", "CreatedByPersonId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.FinancialBatch", new[] { "CreatedByPersonId" });
            DropForeignKey("dbo.FinancialBatch", "CreatedByPersonId", "dbo.Person");
            DropColumn("dbo.FinancialBatch", "CreatedByPersonId");
        }
    }
}
