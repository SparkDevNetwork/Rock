using Rock.Plugin;

namespace com.bemaservices.ClientPackage.BEMA
{
    [MigrationNumber( 1, "1.9.4" )]
    public class InitialSetup : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Page: BEMA Reports
            RockMigrationHelper.AddPage("BB0ACD18-24FB-42BA-B89A-2FFD80472F5B","D65F783D-87A9-4CC9-8110-E83466A0EADB","BEMA Reports","","2571CBBD-7CCA-4B24-AAAB-107FD136298B",""); // Site:Rock RMS
            // Add Block to Page: BEMA Reports, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "2571CBBD-7CCA-4B24-AAAB-107FD136298B","","CACB9D1A-A820-4587-986A-D66A69EE9948","Page Menu","Main","","",0,"97DBA921-3028-49FB-966E-3D0C78EE25FB");   
            // Attrib Value for Block:Page Menu, Attribute:Include Current Parameters Page: BEMA Reports, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("97DBA921-3028-49FB-966E-3D0C78EE25FB","EEE71DDE-C6BC-489B-BAA5-1753E322F183",@"False");  
            // Attrib Value for Block:Page Menu, Attribute:Template Page: BEMA Reports, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("97DBA921-3028-49FB-966E-3D0C78EE25FB","1322186A-862A-4CF1-B349-28ECB67229BA",@"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}");  
            // Attrib Value for Block:Page Menu, Attribute:Root Page Page: BEMA Reports, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("97DBA921-3028-49FB-966E-3D0C78EE25FB","41F1C42E-2395-4063-BD4F-031DF8D5B231",@"2571cbbd-7cca-4b24-aaab-107fd136298b");  
            // Attrib Value for Block:Page Menu, Attribute:Number of Levels Page: BEMA Reports, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("97DBA921-3028-49FB-966E-3D0C78EE25FB","6C952052-BC79-41BA-8B88-AB8EA3E99648",@"1");  
            // Attrib Value for Block:Page Menu, Attribute:Include Current QueryString Page: BEMA Reports, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("97DBA921-3028-49FB-966E-3D0C78EE25FB","E4CF237D-1D12-4C93-AFD7-78EB296C4B69",@"False");  
            // Attrib Value for Block:Page Menu, Attribute:Is Secondary Block Page: BEMA Reports, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("97DBA921-3028-49FB-966E-3D0C78EE25FB","C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2",@"False");

            // Page: Configure BEMA Reports
            RockMigrationHelper.AddPage( "550A898C-EDEA-48B5-9C58-B20EC13AF13B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Configure BEMA Reports", "", "6C3F5510-0B1D-4E35-971B-5285472A542C", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "BEMA Report Configuration", "Renders a page menu based on a root page and lava template.", "~/Plugins/com_bemaservices/CustomBlocks/BEMA/Reporting/BemaReportConfiguration.ascx", "BEMA Services > Reporting", "DC982F6F-56C8-40C9-B60E-08D463842042" );
            // Add Block to Page: Configure BEMA Reports, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6C3F5510-0B1D-4E35-971B-5285472A542C", "", "DC982F6F-56C8-40C9-B60E-08D463842042", "BEMA Report Configuration", "Main", "", "", 0, "F099C0A7-7175-4A9D-9005-39CD53CBED88" );
            // Attrib for BlockType: BEMA Report Configuration:Root Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "DC982F6F-56C8-40C9-B60E-08D463842042", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Root Page", "RootPage", "", "The root page to use for the page collection. Defaults to the current page instance if not set.", 0, @"", "9E0DF4EE-8627-4A22-9049-777E9BC9E25B" );
            // Attrib Value for Block:BEMA Report Configuration, Attribute:Root Page Page: Configure BEMA Reports, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F099C0A7-7175-4A9D-9005-39CD53CBED88", "9E0DF4EE-8627-4A22-9049-777E9BC9E25B", @"2571cbbd-7cca-4b24-aaab-107fd136298b" );


             // Hide Pages from view
            RockMigrationHelper.AddSecurityAuthForPage( "2571CBBD-7CCA-4B24-AAAB-107FD136298B", 0, "View", false, "", 1, "957DF695-849C-4C1C-BEAF-3F2DB7EBEA70" );
            RockMigrationHelper.AddSecurityAuthForPage( "6C3F5510-0B1D-4E35-971B-5285472A542C", 0, "View", false, "", 1, "C693B273-0658-4413-A225-9DAFDF5CCD19" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "9E0DF4EE-8627-4A22-9049-777E9BC9E25B" );
            RockMigrationHelper.DeleteBlock( "F099C0A7-7175-4A9D-9005-39CD53CBED88" );
            RockMigrationHelper.DeleteBlockType( "DC982F6F-56C8-40C9-B60E-08D463842042" );
            RockMigrationHelper.DeletePage( "6C3F5510-0B1D-4E35-971B-5285472A542C" ); //  Page: Configure BEMA Reports

            RockMigrationHelper.DeleteBlock( "97DBA921-3028-49FB-966E-3D0C78EE25FB" );
            RockMigrationHelper.DeletePage( "2571CBBD-7CCA-4B24-AAAB-107FD136298B" ); //  Page: BEMA Reports
        }
    }
}

