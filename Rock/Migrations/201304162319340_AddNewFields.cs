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
    public partial class AddNewFields : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.Schedule", "IsShared", c => c.Boolean( nullable: true ) );
            AddColumn( "dbo.FinancialScheduledTransaction", "CardReminderDate", c => c.DateTime() );
            AddColumn( "dbo.FinancialScheduledTransaction", "LastRemindedDate", c => c.DateTime() );

            Sql( @"
    UPDATE [Schedule] SET [IsShared] = 0
" );

            AlterColumn( "dbo.Schedule", "IsShared", c => c.Boolean( nullable: false ) );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.FinancialScheduledTransaction", "LastRemindedDate");
            DropColumn("dbo.FinancialScheduledTransaction", "CardReminderDate");
            DropColumn("dbo.Schedule", "IsShared");
        }
    }
}
