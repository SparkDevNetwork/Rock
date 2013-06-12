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
    public partial class AccountNumberSecuredGroupTypeDisplayOrder : RockMigration_5
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.GroupType", "DisplayOrder", c => c.Int( nullable: false ) );
            AddColumn( "dbo.FinancialTransaction", "AccountNumberSecured", c => c.String( maxLength: 40 ) );
            Sql( "alter table dbo.FinancialTransaction alter column AuthorizedPersonId int null" );
            AddColumn( "dbo.FinancialPersonBankAccount", "AccountNumberSecured", c => c.String( nullable: false, maxLength: 40 ) );
            CreateIndex( "dbo.FinancialPersonBankAccount", "AccountNumberSecured", true );
            DropColumn( "dbo.FinancialPersonBankAccount", "AccountNumber" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn( "dbo.FinancialPersonBankAccount", "AccountNumber", c => c.String( nullable: false, maxLength: 100 ) );
            DropIndex( "dbo.FinancialPersonBankAccount", new string[] { "AccountNumberSecured" } );
            DropColumn( "dbo.FinancialPersonBankAccount", "AccountNumberSecured" );
            DropIndex( "dbo.FinancialTransaction", new string[] { "AuthorizedPersonId" } );
            Sql( "alter table dbo.FinancialTransaction alter column AuthorizedPersonId int not null" );
            CreateIndex( "dbo.FinancialTransaction", new string[] { "AuthorizedPersonId" } );
            DropColumn( "dbo.FinancialTransaction", "AccountNumberSecured" );
            DropColumn( "dbo.GroupType", "DisplayOrder" );
        }
    }
}
