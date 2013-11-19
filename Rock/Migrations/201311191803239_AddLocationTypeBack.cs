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
    public partial class AddLocationTypeBack : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Location", "LocationTypeValueId", c => c.Int());
            AddDefinedType( "Location", "Location Type", "Is the type (Campus, Building, Room, etc.) of a named location ", "3285DCEF-FAA4-43B9-9338-983F4A384ABA" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Location", "LocationTypeValueId");
            DeleteDefinedType( "3285DCEF-FAA4-43B9-9338-983F4A384ABA" );
        }
    }
}
