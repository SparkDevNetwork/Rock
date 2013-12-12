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
    public partial class AccountNumberSecureNonUnique : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // drop the unique index
            DropIndex( "FinancialPersonBankAccount", new string[] { "AccountNumberSecured" } );

            // create the index, but as a non-unique
            CreateIndex( "FinancialPersonBankAccount", new string[] { "AccountNumberSecured" }, false );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // drop the index
            DropIndex( "FinancialPersonBankAccount", new string[] { "AccountNumberSecured" } );

            CreateIndex( "FinancialPersonBankAccount", new string[] { "AccountNumberSecured" }, true );
        }
    }
}
