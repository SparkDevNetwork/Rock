using Rock.Plugin;

namespace com.bemaservices.PastoralCare.Migrations
{
    [MigrationNumber( 5, "1.9.4" )]
    public class AddInactiveColumn : Migration
    {
        public override void Up()
        {
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem] ADD [IsActive] BIT
                    " );

            // Page: Care Contact Attributes
            RockMigrationHelper.AddPage("FC1531F6-5A3C-4F05-8E92-B2B66688B492","D65F783D-87A9-4CC9-8110-E83466A0EADB","Shared Care Contact Attributes","","7BB3E53B-5C42-448F-A5D2-3BBD32A218EB",""); // Site:Rock RMS
            // Add Block to Page: Care Contact Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "7BB3E53B-5C42-448F-A5D2-3BBD32A218EB","","E5EA2F6D-43A2-48E0-B59C-4409B78AC830","Attributes","Main","","",0,"A6278C8D-A594-425A-84C6-980CDDBCA07F");   
            // Attrib Value for Block:Attributes, Attribute:Entity Qualifier Column Page: Care Contact Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("A6278C8D-A594-425A-84C6-980CDDBCA07F","ECD5B86C-2B48-4548-9FE9-7AC6F6FA8106",@"");  
            // Attrib Value for Block:Attributes, Attribute:Entity Qualifier Value Page: Care Contact Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("A6278C8D-A594-425A-84C6-980CDDBCA07F","FCE1E87D-F816-4AD5-AE60-1E71942C547C",@"");  
            // Attrib Value for Block:Attributes, Attribute:Entity Page: Care Contact Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("A6278C8D-A594-425A-84C6-980CDDBCA07F","5B33FE25-6BF0-4890-91C6-49FB1629221E",@"95719340-44e1-4a2d-bb40-8dad1e67d83c");  
            // Attrib Value for Block:Attributes, Attribute:Entity Id Page: Care Contact Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("A6278C8D-A594-425A-84C6-980CDDBCA07F","CBB56D68-3727-42B9-BF13-0B2B593FB328",@"0");  
            // Attrib Value for Block:Attributes, Attribute:Allow Setting of Values Page: Care Contact Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("A6278C8D-A594-425A-84C6-980CDDBCA07F","018C0016-C253-44E4-84DB-D166084C5CAD",@"False");  
            // Attrib Value for Block:Attributes, Attribute:Enable Show In Grid Page: Care Contact Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("A6278C8D-A594-425A-84C6-980CDDBCA07F","920FE120-AD75-4D5C-BFE0-FA5745B1118B",@"True");  
            // Attrib Value for Block:Attributes, Attribute:Category Filter Page: Care Contact Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("A6278C8D-A594-425A-84C6-980CDDBCA07F","0C2BCD33-05CC-4B03-9F57-C686B8911E64",@"");  
            // Attrib Value for Block:Attributes, Attribute:core.CustomGridColumnsConfig Page: Care Contact Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("A6278C8D-A594-425A-84C6-980CDDBCA07F","11F74455-F71D-45C7-806B-0DB463D34DAB",@"");  
            // Attrib Value for Block:Attributes, Attribute:core.CustomGridEnableStickyHeaders Page: Care Contact Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("A6278C8D-A594-425A-84C6-980CDDBCA07F","BFBE7D47-DCDD-468C-A330-74CC6E7AAFCC",@"False");

            // Page: Care Types
            RockMigrationHelper.UpdateBlockType("Care Type List","Block to display the care types.","~/Plugins/com_bemaservices/PastoralCare/CareTypeList.ascx","BEMA Services > Pastoral Care","252EF3E6-876A-40BA-9F7A-0EEEC9A50200");
            // Attrib for BlockType: Care Type List:Shared Care Contact Attribute Page
            RockMigrationHelper.UpdateBlockTypeAttribute("252EF3E6-876A-40BA-9F7A-0EEEC9A50200","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Shared Care Contact Attribute Page","SharedCareContactAttributePage","","Page used to view universal care contact attributes.",0,@"","116564E5-DA12-4D0A-8DB4-42A8758D5C22");
            // Attrib for BlockType: Care Type List:Allow Shared Care Item Attributes
            RockMigrationHelper.UpdateBlockTypeAttribute("252EF3E6-876A-40BA-9F7A-0EEEC9A50200","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Allow Shared Care Item Attributes","AllowSharedCareItemAttributes","","Displays a link to a page for care item attributes shared across care types",0,@"False","5DB27B1A-2E7D-4626-91B4-2087E9D622B2");
            // Attrib for BlockType: Care Type List:Shared Care Item Attribute Page
            RockMigrationHelper.UpdateBlockTypeAttribute("252EF3E6-876A-40BA-9F7A-0EEEC9A50200","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Shared Care Item Attribute Page","SharedCareItemAttributePage","","Page used to view universal care item attributes.",0,@"","F7AC513C-6C6F-4072-967B-E0B12F663719");
            // Attrib for BlockType: Care Type List:Allow Shared Care Contact Attributes
            RockMigrationHelper.UpdateBlockTypeAttribute("252EF3E6-876A-40BA-9F7A-0EEEC9A50200","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Allow Shared Care Contact Attributes","AllowSharedCareContactAttributes","","Displays a link to a page for care contact attributes shared across care types",0,@"False","99E11ECA-A2C3-44A9-9C61-BCC5946BA007");
            // Attrib Value for Block:Care Type List, Attribute:Shared Care Contact Attribute Page Page: Care Types, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("7FD75626-52D5-4206-A1ED-265233EB19EE","116564E5-DA12-4D0A-8DB4-42A8758D5C22",@"7bb3e53b-5c42-448f-a5d2-3bbd32a218eb");
            // Attrib Value for Block:Care Type List, Attribute:Allow Shared Care Item Attributes Page: Care Types, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("7FD75626-52D5-4206-A1ED-265233EB19EE","5DB27B1A-2E7D-4626-91B4-2087E9D622B2",@"True");
            // Attrib Value for Block:Care Type List, Attribute:Shared Care Item Attribute Page Page: Care Types, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("7FD75626-52D5-4206-A1ED-265233EB19EE","F7AC513C-6C6F-4072-967B-E0B12F663719",@"b70d4b19-74cf-4c21-be7d-ea0bd33ecf65");
            // Attrib Value for Block:Care Type List, Attribute:Allow Shared Care Contact Attributes Page: Care Types, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("7FD75626-52D5-4206-A1ED-265233EB19EE","99E11ECA-A2C3-44A9-9C61-BCC5946BA007",@"True");

            // Page: Universal Care Item Attributes
            Sql( @"Update Page
                    Set [InternalName] = 'Shared Care Item Attributes',
                        [PageTitle] = 'Shared Care Item Attributes',
                        [BrowserTitle] = 'Shared Care Item Attributes'
                    Where [Guid] = 'B70D4B19-74CF-4C21-BE7D-EA0BD33ECF65'" );
        }
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock("A6278C8D-A594-425A-84C6-980CDDBCA07F");
            RockMigrationHelper.DeletePage("7BB3E53B-5C42-448F-A5D2-3BBD32A218EB"); //  Page: Care Contact Attributes
        }
    }
}
