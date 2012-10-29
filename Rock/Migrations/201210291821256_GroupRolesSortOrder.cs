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
    public partial class GroupRolesSortOrder : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropIndex( "dbo.crmGroupRole", new string[] { "Order" } );
            RenameColumn( "dbo.crmGroupRole", "Order", "SortOrder" );
            CreateIndex( "dbo.crmGroupRole", new string[] { "SortOrder" } );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex( "dbo.crmGroupRole", new string[] { "SortOrder" } );
            RenameColumn( "dbo.crmGroupRole", "SortOrder", "Order" );
            CreateIndex( "dbo.crmGroupRole", new string[] { "Order" } );
        }
    }
}
