//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class NewFamilyPage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "New Family", "", "Default", "6A11A13D-05AB-4982-A4C2-67A8B1950C74", "" );

//TODO: this should have a diff parent page.
            AddPage( "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F", "Attribute Categories", "", "Default", "220D72F5-B589-4378-9852-BBB6F145AD7F", "" );

            AddBlockType( "Administration - Attribute Categories", "", "~/Blocks/Administration/AttributeCategories.ascx", "1FC50941-A883-47A2-ABE9-13528BCC4D1B" );
            AddBlockType( "CRM - Person Detail - Add Family", "", "~/Blocks/CRM/PersonDetail/AddFamily.ascx", "DE156975-597A-4C55-A649-FE46712F91C3" );
            AddBlockType( "CRM - Person Detail - Attribute Values", "", "~/Blocks/CRM/PersonDetail/AttributeValues.ascx", "D70A59DC-16BE-43BE-9880-59598FA7A94C" );

            AddBlock( "6A11A13D-05AB-4982-A4C2-67A8B1950C74", "DE156975-597A-4C55-A649-FE46712F91C3", "Add Family", "", "Content", 0, "613536BE-86BC-4755-B815-807C236B92E6" );
            AddBlock( "220D72F5-B589-4378-9852-BBB6F145AD7F", "1FC50941-A883-47A2-ABE9-13528BCC4D1B", "Attribute Categories", "", "Content", 0, "F4778FD0-D3DC-4732-9482-5022300D25EF" );

            // Attrib for BlockType: CRM - Person Detail - Add Family:Category
            AddBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Category", "Category", "", "The Attribute Categories to display attributes from", 0, "", "CF150193-1568-42D8-8E28-5BFCD4954C6F" );
            // Attrib for BlockType: CRM - Person Detail - Add Family:Require Grade
            AddBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Grade", "RequireGrade", "", "Should Grade by required", 0, "False", "090099D5-D8C4-4C72-B1B0-ACEBA231E68F" );
            // Attrib for BlockType: CRM - Person Detail - Add Family:Require Gender
            AddBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Gender", "RequireGender", "", "Should Gender be required", 0, "False", "9BBA1555-9809-4A30-9B79-D7501CE1A864" );
            // Attrib for BlockType: CRM - Person Detail - Add Family:Location Type
            AddBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Location Type", "LocationType", "", "The type of location that address should use", 0, "", "687E6973-0259-4DB4-B4EB-73708D98EFCD" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: CRM - Person Detail - Add Family:Location Type
            DeleteAttribute( "687E6973-0259-4DB4-B4EB-73708D98EFCD" );
            // Attrib for BlockType: CRM - Person Detail - Add Family:Require Gender
            DeleteAttribute( "9BBA1555-9809-4A30-9B79-D7501CE1A864" );
            // Attrib for BlockType: CRM - Person Detail - Add Family:Require Grade
            DeleteAttribute( "090099D5-D8C4-4C72-B1B0-ACEBA231E68F" );
            // Attrib for BlockType: CRM - Person Detail - Add Family:Category
            DeleteAttribute( "CF150193-1568-42D8-8E28-5BFCD4954C6F" );

            DeleteBlock( "F4778FD0-D3DC-4732-9482-5022300D25EF" ); // Attribute Categories
            DeleteBlock( "613536BE-86BC-4755-B815-807C236B92E6" ); // Add Family

            DeleteBlockType( "D70A59DC-16BE-43BE-9880-59598FA7A94C" ); // CRM - Person Detail - Attribute Values
            DeleteBlockType( "DE156975-597A-4C55-A649-FE46712F91C3" ); // CRM - Person Detail - Add Family
            DeleteBlockType( "1FC50941-A883-47A2-ABE9-13528BCC4D1B" ); // Administration - Attribute Categories

            DeletePage( "220D72F5-B589-4378-9852-BBB6F145AD7F" ); // Attribute Categories
            DeletePage( "6A11A13D-05AB-4982-A4C2-67A8B1950C74" ); // New Family
        }
    }
}
