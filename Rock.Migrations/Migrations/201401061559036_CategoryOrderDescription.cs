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
    public partial class CategoryOrderDescription : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.Category", "Order", c => c.Int( nullable: true ) );
            AddColumn( "dbo.Category", "Description", c => c.String() );

            Sql( @"
    UPDATE [Category] SET [Order] = 0;
" );
            AlterColumn( "dbo.Category", "Order", c => c.Int( nullable: false ) );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.Category", "Description" );
            DropColumn( "dbo.Category", "Order" );
        }
    }
}
