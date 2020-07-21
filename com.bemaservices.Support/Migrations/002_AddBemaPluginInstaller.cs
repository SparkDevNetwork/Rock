using Rock.Plugin;

namespace com.bemaservices.Support
{
    [MigrationNumber( 2, "1.7.4" )]
    public class AddBemaPluginInstaller : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT, "", "", "BEMA Code Package Version", "Currently installed BEMA Code version number", 0, "0.0.0.0", com.bemaservices.Support.SystemGuid.Attribute.BEMA_CLIENT_PACKAGE_VERSION_ATTRIBUTE_GUID );

            // Page: Internal Homepage
            RockMigrationHelper.UpdateBlockType( "BEMA Plugin Installer", "Allows a client to download the latest copy of their BEMA Code.", "~/Plugins/com_bemaservices/Support/BemaPluginInstaller.ascx", "BEMA Services > Support", "82360F6E-7D0B-4EAC-9371-A55ACE0F512C" );
            RockMigrationHelper.AddBlock( true, "20F97A93-7949-4C2A-8A5E-C756FE8585CA", "", "82360F6E-7D0B-4EAC-9371-A55ACE0F512C", "BEMA Plugin Installer", "Sidebar1", "", "", 0, "18A61600-6147-47DE-B543-472E7953905E" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "82360F6E-7D0B-4EAC-9371-A55ACE0F512C", "C0D0D7E2-C3B0-4004-ABEA-4BBFAD10D5D2", "Server Url", "ServerUrl", "", "The Url location to check for the latest BEMA packages.", 0, @"", "1B8097C0-8409-4166-89A8-970E0B1FFD27" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "1B8097C0-8409-4166-89A8-970E0B1FFD27" );
            RockMigrationHelper.DeleteBlock( "18A61600-6147-47DE-B543-472E7953905E" );
            RockMigrationHelper.DeleteBlockType( "82360F6E-7D0B-4EAC-9371-A55ACE0F512C" );
            RockMigrationHelper.DeleteAttribute( com.bemaservices.Support.SystemGuid.Attribute.BEMA_CLIENT_PACKAGE_VERSION_ATTRIBUTE_GUID );
        }
    }
}

