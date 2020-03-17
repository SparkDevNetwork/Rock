using Rock.Plugin;
using com.bemaservices.Security.SSO.SystemGuids;

namespace com.bemaservices.Security.SSO.Migrations
{
    [MigrationNumber( 3, "1.8.0" )]
    public class SSO_AddBlock : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddBlock( true, Page.SSOLoginPage, "", BlockType.SSOAuthentication, "SSO Initiator", "Main", "", "", 0, Block.SSOLoginBlock );
        }

        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( Block.SSOLoginBlock );
        }
    }

}