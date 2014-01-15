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
    public partial class ChangeLayoutOfSecurityRoleDetailsPage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"UPDATE [Page]
	                SET LayoutId = 13
	                WHERE [Guid] = '48AAD428-A9C9-4BBB-A80F-B85F28D31240'");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql(@"UPDATE [Page]
	                SET LayoutId = 12
	                WHERE [Guid] = '48AAD428-A9C9-4BBB-A80F-B85F28D31240'");
        }
    }
}
