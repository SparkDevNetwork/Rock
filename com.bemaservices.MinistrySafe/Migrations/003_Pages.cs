using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.bemaservices.MinistrySafe.Migrations
{
    [MigrationNumber( 3, "1.8.5" )]
    public partial class Pages : Migration
    {
        public override void Up()
        {          
            // Page: Ministry Safe
            RockMigrationHelper.AddPage("C831428A-6ACD-4D49-9B2D-046D399E3123","D65F783D-87A9-4CC9-8110-E83466A0EADB","Ministry Safe","","5C7EA1BE-FC79-4821-8FA3-759F8C65C87B","fa fa-shield"); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "User List", "Lists all the Ministry Safe Users.", "~/Plugins/com_bemaservices/Security/MinistrySafe/UserList.ascx", "BEMA Services > Ministry Safe", "D8BF3D63-C063-4A13-A22C-4AD7C3B331B3" );
            // Add Block to Page: Ministry Safe, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "5C7EA1BE-FC79-4821-8FA3-759F8C65C87B","","D8BF3D63-C063-4A13-A22C-4AD7C3B331B3","User List","Main","","",0,"67D69BD8-E164-4E6C-B038-2609C9330760");   
            // Attrib for BlockType: User List:Workflow Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute("D8BF3D63-C063-4A13-A22C-4AD7C3B331B3","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Workflow Detail Page","WorkflowDetailPage","","The page to view details about the Ministry Safe workflow",0, @"ba547eed-5537-49cf-bd4e-c583d760788c", "1AED9A04-2926-49CF-A986-C3EA98C4357C");  
            // Attrib for BlockType: User List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute("D8BF3D63-C063-4A13-A22C-4AD7C3B331B3","9C204CD0-1233-41C5-818A-C5DA439445AA","core.CustomGridColumnsConfig","core.CustomGridColumnsConfig","","",0,@"","350C7846-2103-47D8-A902-564E6FB86E04");  
            // Attrib for BlockType: User List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute("D8BF3D63-C063-4A13-A22C-4AD7C3B331B3","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","core.CustomGridEnableStickyHeaders","core.CustomGridEnableStickyHeaders","","",0,@"False","EEC0A72A-0334-4240-BFA4-A6472AC1B85B");

            // Add Badge
            RockMigrationHelper.AddBlockAttributeValue( "F3E6CC14-C540-4FFC-A5A9-48AD9CC0A61B", "F5AB231E-3836-4D52-BD03-BF79773C237A", "9e9b9faf-c7b8-40aa-b0c9-24177058943b", true );

            // Add Workflow to Action Dropdown
            RockMigrationHelper.AddBlockAttributeValue( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", "7197A0FB-B330-43C4-8E62-F3C14F649813", "5876314a-fc4f-4a07-8ca0-a02de26e55be", true );

            // Add Ministry Safe block to Person Profile Extended Attributes Page
            // Add Block to Page: Extended Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "1C737278-4CBA-404B-B6B3-E3F0E05AB5FE","","D70A59DC-16BE-43BE-9880-59598FA7A94C","Ministry Safe","SectionB2","","",3,"980731DB-3271-420E-A258-CECC9E7DFE77");
            // Attrib Value for Block:Ministry Safe, Attribute:Category Page: Extended Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("980731DB-3271-420E-A258-CECC9E7DFE77","EC43CF32-3BDF-4544-8B6A-CE9208DD7C81",@"cb481ab7-e0f9-4a3e-b846-0f5e5c94c038");  
            // Attrib Value for Block:Ministry Safe, Attribute:Attribute Order Page: Extended Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("980731DB-3271-420E-A258-CECC9E7DFE77","235C6D48-E1D1-410C-8006-1EA412BC12EF",@"");  

        }
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "EEC0A72A-0334-4240-BFA4-A6472AC1B85B" );
            RockMigrationHelper.DeleteAttribute( "350C7846-2103-47D8-A902-564E6FB86E04" );
            RockMigrationHelper.DeleteAttribute( "1AED9A04-2926-49CF-A986-C3EA98C4357C" );
            RockMigrationHelper.DeleteBlock( "67D69BD8-E164-4E6C-B038-2609C9330760" );
            RockMigrationHelper.DeleteBlockType( "D8BF3D63-C063-4A13-A22C-4AD7C3B331B3" );
            RockMigrationHelper.DeletePage( "5C7EA1BE-FC79-4821-8FA3-759F8C65C87B" ); //  Page: Ministry Safe           
        }
    }
}
