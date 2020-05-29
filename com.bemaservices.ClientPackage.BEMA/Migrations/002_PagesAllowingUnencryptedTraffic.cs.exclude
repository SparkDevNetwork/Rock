using Rock.Plugin;

namespace com.bemaservices.ClientPackage.BEMA
{
    [MigrationNumber( 2, "1.9.4" )]
    public class PagesAllowingUnencryptedTraffic : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Page: Pages Allowing Unencrypted Traffic
            RockMigrationHelper.AddPage( "2571CBBD-7CCA-4B24-AAAB-107FD136298B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Pages Allowing Unencrypted Traffic", "This report displays a list of pages that allow unencrypted traffic to access them", "F745A701-B8B5-42F6-9813-922A77524781", "" ); // Site:Rock RMS
            // Add Block to Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F745A701-B8B5-42F6-9813-922A77524781", "", "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "Dynamic Data", "Main", "", "", 1, "43DD0895-6418-47EB-99FA-E037DBD5E728" );
            // Add Block to Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F745A701-B8B5-42F6-9813-922A77524781", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "HTML Content", "Main", "", "", 0, "CD84D302-D9D6-495B-857C-BD83054353B0" );
            // Attrib Value for Block:Dynamic Data, Attribute:Update Page Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "43DD0895-6418-47EB-99FA-E037DBD5E728", "230EDFE8-33CA-478D-8C9A-572323AF3466", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Query Params Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "43DD0895-6418-47EB-99FA-E037DBD5E728", "B0EC41B9-37C0-48FD-8E4E-37A8CA305012", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Columns Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "43DD0895-6418-47EB-99FA-E037DBD5E728", "90B0E6AF-B2F4-4397-953B-737A40D4023B", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Query Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "43DD0895-6418-47EB-99FA-E037DBD5E728", "71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356", @"{% include '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Assets/Sql/PagesAllowingUnencryptedTraffic.sql' %}" );
            // Attrib Value for Block:Dynamic Data, Attribute:Url Mask Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "43DD0895-6418-47EB-99FA-E037DBD5E728", "B9163A35-E09C-466D-8A2D-4ED81DF0114C", @"~/page/103?Page={PageId}" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Columns Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "43DD0895-6418-47EB-99FA-E037DBD5E728", "202A82BF-7772-481C-8419-600012607972", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Merge Fields Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "43DD0895-6418-47EB-99FA-E037DBD5E728", "8EB882CE-5BB1-4844-9C28-10190903EECD", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Formatted Output Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "43DD0895-6418-47EB-99FA-E037DBD5E728", "6A233402-446C-47E9-94A5-6A247C29BC21", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Person Report Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "43DD0895-6418-47EB-99FA-E037DBD5E728", "8104CE53-FDB3-4E9F-B8E7-FD9E06E7551C", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Communication Recipient Person Id Columns Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "43DD0895-6418-47EB-99FA-E037DBD5E728", "75DDB977-9E71-44E8-924B-27134659D3A4", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Excel Export Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "43DD0895-6418-47EB-99FA-E037DBD5E728", "E11B57E5-EC7D-4C42-9ADA-37594D71F145", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Communicate Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "43DD0895-6418-47EB-99FA-E037DBD5E728", "5B2C115A-C187-4AB3-93AE-7010644B39DA", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Merge Person Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "43DD0895-6418-47EB-99FA-E037DBD5E728", "8762ABE3-726E-4629-BD4D-3E42E1FBCC9E", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Bulk Update Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "43DD0895-6418-47EB-99FA-E037DBD5E728", "D01510AA-1B8D-467C-AFC6-F7554CB7CF78", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Stored Procedure Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "43DD0895-6418-47EB-99FA-E037DBD5E728", "A4439703-5432-489A-9C14-155903D6A43E", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Merge Template Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "43DD0895-6418-47EB-99FA-E037DBD5E728", "6697B0A2-C8FE-497A-B5B4-A9D459474338", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Paneled Grid Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "43DD0895-6418-47EB-99FA-E037DBD5E728", "5449CB61-2DFC-4B55-A697-38F1C2AF128B", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Grid Filter Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "43DD0895-6418-47EB-99FA-E037DBD5E728", "E582FD3C-9990-47D1-A57F-A3DB753B1D0C", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Timeout Page: Pages Allowing Unencrypted Traffic, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "43DD0895-6418-47EB-99FA-E037DBD5E728", "BEEE38DD-2791-4242-84B6-0495904143CC", @"30" );


            // Hide Page from view
            RockMigrationHelper.AddSecurityAuthForPage( "F745A701-B8B5-42F6-9813-922A77524781", 0, "View", false, "", 1, "45F690FD-F3AF-4D39-A609-79DDD40F3D62" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "CD84D302-D9D6-495B-857C-BD83054353B0" );
            RockMigrationHelper.DeleteBlock( "43DD0895-6418-47EB-99FA-E037DBD5E728" );
            RockMigrationHelper.DeletePage( "F745A701-B8B5-42F6-9813-922A77524781" ); //  Page: Pages Allowing Unencrypted Traffic
        }
    }
}

