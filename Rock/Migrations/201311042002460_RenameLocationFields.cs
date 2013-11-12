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
    public partial class RenameLocationFields : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey( "dbo.Location", "LocationTypeValueId", "dbo.DefinedValue" );
            DropIndex( "dbo.Location", new[] { "LocationTypeValueId" } );
            DropColumn( "dbo.Location", "LocationTypeValueId" );

            RenameColumn( "dbo.Location", "IsLocation", "IsNamedLocation" );
            RenameColumn( "dbo.GroupLocation", "IsMailing", "IsMailingLocation" );
            RenameColumn( "dbo.GroupLocation", "IsLocation", "IsMappedLocation" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RenameColumn( "dbo.Location", "IsNamedLocation", "IsLocation" );
            RenameColumn( "dbo.GroupLocation", "IsMailingLocation", "IsMailing" );
            RenameColumn( "dbo.GroupLocation", "IsMappedLocation", "IsLocation" );

            AddColumn( "dbo.Location", "LocationTypeValueId", c => c.Int() );
            CreateIndex( "dbo.Location", "LocationTypeValueId" );
            AddForeignKey( "dbo.Location", "LocationTypeValueId", "dbo.DefinedValue", "Id" );
        }
    }
}
