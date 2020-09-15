using Rock.Plugin;

namespace com.bemaservices.PastoralCare.Migrations
{
    [MigrationNumber( 6, "1.10.2" )]
    public class FixSharedAttributeIssue : Migration
    {
        public override void Up()
        {

            // Attrib Value for Block:Attributes, Attribute:Entity Page: Shared Care Item Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "437C2FD9-AC09-4F37-B4E5-364B48E26F5C", "5B33FE25-6BF0-4890-91C6-49FB1629221E", @"72352815-30f3-46fb-86c0-69ac284d9ed2" );

        }
        public override void Down()
        {
          
        }
    }
}
