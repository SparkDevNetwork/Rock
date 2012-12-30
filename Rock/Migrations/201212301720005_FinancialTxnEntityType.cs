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
    public partial class FinancialTxnEntityType : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.FinancialTransaction", "EntityTypeId", c => c.Int(nullable: false));
            AlterColumn("dbo.FinancialTransaction", "EntityId", c => c.Int(nullable: false));
            AddForeignKey("dbo.FinancialTransaction", "EntityTypeId", "dbo.EntityType", "Id");
            CreateIndex("dbo.FinancialTransaction", "EntityTypeId");
            DropColumn("dbo.FinancialTransaction", "Entity");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.FinancialTransaction", "Entity", c => c.String(maxLength: 50));
            DropIndex("dbo.FinancialTransaction", new[] { "EntityTypeId" });
            DropForeignKey("dbo.FinancialTransaction", "EntityTypeId", "dbo.EntityType");
            AlterColumn("dbo.FinancialTransaction", "EntityId", c => c.Int());
            DropColumn("dbo.FinancialTransaction", "EntityTypeId");
        }
    }
}
