using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Plugin;
using Rock.Web.Cache;
namespace com.centralaz.Baptism.Migrations
{
    [MigrationNumber( 5, "1.0.14" )]
    public class NewBaptismLocationListBlock : Migration
    {
        public override void Up()
        {
            // Remove Block: Baptism Locations, from Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B38DDC5A-F3D8-4E84-920F-C6465CF76667" );


            // Page: Baptisms
            RockMigrationHelper.UpdateBlockType( "Baptism Location List", "Lists all baptism locations.", "~/Plugins/com_centralaz/Baptism/BaptismLocationList.ascx", "com_centralaz > Baptism", "DBC3F9A4-817C-443B-BED9-A2A3687558E0" );
            RockMigrationHelper.AddBlock( "F08B7F8F-5EC6-4CEC-8C20-5AD5EC5A3800", "", "DBC3F9A4-817C-443B-BED9-A2A3687558E0", "Baptism Locations", "Main", "", "", 0, "B38DDC5A-F3D8-4E84-920F-C6465CF76667" );

            RockMigrationHelper.AddBlockTypeAttribute( "DBC3F9A4-817C-443B-BED9-A2A3687558E0", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "339797A0-3DA7-49C5-B166-76019D1E9CC1" );

            RockMigrationHelper.AddBlockTypeAttribute( "DBC3F9A4-817C-443B-BED9-A2A3687558E0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Member Count Column", "DisplayMemberCountColumn", "", "Should the Member Count column be displayed? Does not affect lists with a person context.", 7, @"True", "DC2EDBFD-15BD-4A1C-B380-F9361844A295" );

            RockMigrationHelper.AddBlockAttributeValue( "B38DDC5A-F3D8-4E84-920F-C6465CF76667", "339797A0-3DA7-49C5-B166-76019D1E9CC1", @"b248d7e3-ad38-4e83-9e3c-3cc6d3814ab4" ); // Detail Page

            RockMigrationHelper.AddBlockAttributeValue( "B38DDC5A-F3D8-4E84-920F-C6465CF76667", "DC2EDBFD-15BD-4A1C-B380-F9361844A295", @"True" ); // Display Member Count Column

        }

        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "DC2EDBFD-15BD-4A1C-B380-F9361844A295" );
            RockMigrationHelper.DeleteAttribute( "339797A0-3DA7-49C5-B166-76019D1E9CC1" );
            RockMigrationHelper.DeleteBlock( "B38DDC5A-F3D8-4E84-920F-C6465CF76667" );
            RockMigrationHelper.DeleteBlockType( "DBC3F9A4-817C-443B-BED9-A2A3687558E0" );


            // Add Block to Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.AddBlock( "F08B7F8F-5EC6-4CEC-8C20-5AD5EC5A3800", "", "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "Baptism Locations", "Main", "", "", 0, "B38DDC5A-F3D8-4E84-920F-C6465CF76667" );

            // Attrib Value for Block:Baptism Locations, Attribute:Display Filter Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B38DDC5A-F3D8-4E84-920F-C6465CF76667", "7E0EDF09-9374-4AC4-8591-30C08D7F1E1F", @"False" );

            // Attrib Value for Block:Baptism Locations, Attribute:Include Group Types Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B38DDC5A-F3D8-4E84-920F-C6465CF76667", "5164FF88-A53B-4982-BE50-D56F1FE13FC6", @"32f8592c-ae11-44a7-a053-de43789811d9" );

            // Attrib Value for Block:Baptism Locations, Attribute:Exclude Group Types Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B38DDC5A-F3D8-4E84-920F-C6465CF76667", "0901CBFE-1980-4A1C-8AF0-4A8BD0FC46E9", @"" );

            // Attrib Value for Block:Baptism Locations, Attribute:Display Group Type Column Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B38DDC5A-F3D8-4E84-920F-C6465CF76667", "951D268A-B2A8-42A2-B1C1-3B854070DDF9", @"False" );

            // Attrib Value for Block:Baptism Locations, Attribute:Display Description Column Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B38DDC5A-F3D8-4E84-920F-C6465CF76667", "A0E1B2A4-9D86-4F57-B608-FC7CC498EAC3", @"False" );

            // Attrib Value for Block:Baptism Locations, Attribute:Display System Column Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B38DDC5A-F3D8-4E84-920F-C6465CF76667", "766A4BFA-D2D1-4744-B30D-637A7E3B9D8F", @"False" );

            // Attrib Value for Block:Baptism Locations, Attribute:Display Active Status Column Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B38DDC5A-F3D8-4E84-920F-C6465CF76667", "FCB5F8B3-9C0E-46A8-974A-15353447FCD7", @"False" );

            // Attrib Value for Block:Baptism Locations, Attribute:Limit to Security Role Groups Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B38DDC5A-F3D8-4E84-920F-C6465CF76667", "1DAD66E3-8859-487E-8200-483C98DE2E07", @"False" );

            // Attrib Value for Block:Baptism Locations, Attribute:Detail Page Page: Baptisms, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B38DDC5A-F3D8-4E84-920F-C6465CF76667", "8E57EC42-ABEE-4D35-B7FA-D8513880E8E4", @"b248d7e3-ad38-4e83-9e3c-3cc6d3814ab4" );
        }
    }
}