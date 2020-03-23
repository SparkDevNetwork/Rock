using Rock.Plugin;

namespace com.bemaservices.eSpace
{
    [MigrationNumber( 1, "1.7.4" )]
    public class InitialSetup : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //Create Quick Entry Page on Installed Plugins page
            RockMigrationHelper.AddPage( "5b6dbc42-8b03-4d15-8d92-aafa28fd8616", "d65f783d-87a9-4cc9-8110-e83466a0eadb", "eSPACE Quick Event", "", "1f44d17b-426a-4b08-9088-bb71c399ffe4", "fa fa-calendar-o" );
            RockMigrationHelper.UpdateBlockType( "Quick Create Event", "Form to create new events in eSPACE", "~/Plugins/com_bemaservices/eSpace/QuickCreate.ascx", "BEMA Services > eSPACE", "4c18ba20-fc73-40e1-8b4a-ebb6f67076b4" );

            //Add Block to new page
            RockMigrationHelper.AddBlock( "1f44d17b-426a-4b08-9088-bb71c399ffe4", "", "4c18ba20-fc73-40e1-8b4a-ebb6f67076b4", "Quick Create Event", "Main", "", "", 1, "0126eaee-660e-4e64-a34b-f6e30e0bc7c9" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "0126eaee-660e-4e64-a34b-f6e30e0bc7c9" );
            RockMigrationHelper.DeleteBlockType( "4c18ba20-fc73-40e1-8b4a-ebb6f67076b4" );
            RockMigrationHelper.DeletePage( "1f44d17b-426a-4b08-9088-bb71c399ffe4" );
            
        }
    }
}

