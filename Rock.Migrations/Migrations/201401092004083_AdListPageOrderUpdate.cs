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
    public partial class AdListPageOrderUpdate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
                Sql(@"UPDATE [PAGE] SET [Order] = 4 WHERE [Guid] = '1A3437C8-D4CB-4329-A366-8D6A4CBF79BF'");
                Sql(@"UPDATE [PAGE] SET [Order] = 5 WHERE [Guid] = 'CADB44F2-2453-4DB5-AB11-DADA5162AB79'");
                Sql(@"UPDATE [PAGE] SET [Order] = 6 WHERE [Guid] = '2A22D08D-73A8-4AAF-AC7E-220E8B2E7857'");
                Sql(@"UPDATE [PAGE] SET [Order] = 3 WHERE [Guid] = '78D470E9-221B-4EBD-9FF6-995B45FB9CD5'");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
