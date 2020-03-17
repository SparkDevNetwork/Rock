using Rock.Plugin;

namespace com.bemaservices.Support
{
    [MigrationNumber( 1, "1.7.4" )]
    public class InitialSetup : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Add Support Request Block Type
            RockMigrationHelper.AddBlockType( "Support Request List", "Allows BEMA clients to View and Update Support Requests.", "~/Plugins/com_bemaservices/Support/SupportRequestList.ascx", "BEMA Services > Support", "7F3661E7-0357-4980-8863-4DF6F0D1D58B" );

            // Add Global Attributes
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT, "", "", "BEMA Support API Key", "Key used to integrate with BEMA Support.", 0, "", "B27C3626-668B-4118-9282-20362080D921", "BEMASupportAPIKey" );
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT, "", "", "BEMA Support API URL", "URL used to connect to BEMA Support.", 0, "https://rockadmin.bemaservices.com", "E47688B5-12CA-4EF1-91E4-297E4E12E3DD", "BEMASupportAPIURL" );

            // Add Support Page (Under Installed Plugins)
            RockMigrationHelper.AddPage( "5b6dbc42-8b03-4d15-8d92-aafa28fd8616", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "BEMA Support", "", "5504607F-82BE-4F38-B726-D96DD3574517", "fa fa-building-o", "" );

            // Add Support Block
            RockMigrationHelper.AddBlock( "5504607F-82BE-4F38-B726-D96DD3574517", "", "7F3661E7-0357-4980-8863-4DF6F0D1D58B", "BEMA Support Requests", "Main", "", "", 0, "DE330BD4-FB65-4FF5-8408-18A00470DB4D" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "DE330BD4-FB65-4FF5-8408-18A00470DB4D" );
            RockMigrationHelper.DeletePage( "5504607F-82BE-4F38-B726-D96DD3574517" );
            RockMigrationHelper.DeleteAttribute( "E47688B5-12CA-4EF1-91E4-297E4E12E3DD" );
            RockMigrationHelper.DeleteAttribute( "B27C3626-668B-4118-9282-20362080D921" );
            RockMigrationHelper.DeleteBlockType( "7F3661E7-0357-4980-8863-4DF6F0D1D58B" );
        }
    }
}

