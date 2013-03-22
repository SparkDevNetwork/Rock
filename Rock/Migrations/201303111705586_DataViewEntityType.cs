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
    public partial class DataViewEntityType : RockMigration_4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropIndex( "dbo.DataView", new[] { "EntityTypeId" } );
            AlterColumn( "dbo.DataView", "EntityTypeId", c => c.Int( nullable: false ) );
            CreateIndex( "dbo.DataView", "EntityTypeId", false );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex( "dbo.DataView", new[] { "EntityTypeId" } );
            AlterColumn( "dbo.DataView", "EntityTypeId", c => c.Int() );
            CreateIndex( "dbo.DataView", "EntityTypeId", false );
        }
    }
}
