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
    public partial class LocationUpdates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameColumn( "dbo.Location", "LocationPoint", "GeoPoint" );
            RenameColumn( "dbo.Location", "Perimeter", "GeoFence" );
            AddColumn( "dbo.GroupType", "LocationType", c => c.Int( nullable: false ) );
            AddColumn("dbo.Device", "LocationId", c => c.Int());
            AddForeignKey("dbo.Device", "LocationId", "dbo.Location", "Id");
            CreateIndex("dbo.Device", "LocationId");
            DropColumn("dbo.Device", "GeoPoint");
            DropColumn("dbo.Device", "GeoFence");

            Sql( @"
    DECLARE @DefaultOrgLocationId INT
    SET @DefaultOrgLocationId = (SELECT [Id] FROM [Location] WHERE [Guid] = '17E57EA8-D942-485B-8B61-233589AAC631')
    IF (@DefaultOrgLocationId IS NULL)
    BEGIN
	    INSERT INTO [Location] ([FullAddress],[Street1],[City],[State],[Country],[Zip],[StandardizedDateTime],[GeocodedDateTime],[GeoPoint],[Name],[Guid])
		    VALUES('3120 W Cholla St Phoenix, AZ 85029', '3120 W Cholla St', 'Phoenix', 'AZ', 'US', '85029-4113', '1/1/2014', '1/1/2014', 
			    GEOGRAPHY::STPointFromText('POINT(' + STR(-112.126459,15,7) + ' ' + STR(33.590795,15,7) + ')', 4326),
			    'Organization Address', '17E57EA8-D942-485B-8B61-233589AAC631')
	    SET @DefaultOrgLocationId = SCOPE_IDENTITY()
    END
" );
            UpdateFieldType( "Location Field Type", "", "Rock", "Rock.Field.Types.LocationFieldType", "B0B9EFE3-F09F-4604-AD1B-76B298A85D83" );
            AddGlobalAttribute( "B0B9EFE3-F09F-4604-AD1B-76B298A85D83", "", "", "Organization Address", "The primary mailing address for the organization.", 1, "17E57EA8-D942-485B-8B61-233589AAC631", "E132C358-F28E-45BD-B357-6A2F8B24743A" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Device", "GeoFence", c => c.Geography());
            AddColumn("dbo.Device", "GeoPoint", c => c.Geography());
            DropIndex("dbo.Device", new[] { "LocationId" });
            DropForeignKey("dbo.Device", "LocationId", "dbo.Location");
            DropColumn("dbo.Device", "LocationId");
            DropColumn("dbo.GroupType", "LocationType");

            RenameColumn( "dbo.Location", "GeoPoint", "LocationPoint" );
            RenameColumn( "dbo.Location", "GeoFence", "Perimeter" );
        }
    }
}
