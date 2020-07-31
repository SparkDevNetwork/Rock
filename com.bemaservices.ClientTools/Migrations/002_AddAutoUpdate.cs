using Rock;
using Rock.Plugin;

namespace com.bemaservices.ClientTools
{
    [MigrationNumber( 2, "1.9.4" )]
    public class AddAutoUpdate : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT, "", "", "BEMA Client Tools Version", "Currently installed BEMA Client Tools version number", 0, "0.0.0.0", com.bemaservices.ClientTools.SystemGuid.Attribute.BEMA_CLIENT_TOOLS_VERSION_ATTRIBUTE_GUID );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}

