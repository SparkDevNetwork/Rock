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
            // Convert Defined Value field types to use Guid instead of id
            Sql( @"
    UPDATE AV	
	    SET [Value] = CAST(DV.[Guid] AS varchar(100))
    FROM [Attribute] A
    INNER JOIN [FieldType] F 
	    ON F.[Id] = A.[FieldTypeId]
	    AND F.[Class] = 'Rock.Field.Types.DefinedValueFieldType'
    INNER JOIN [AttributeValue] AV
	    ON AV.[AttributeId] = A.[Id]
    INNER JOIN [DefinedValue] DV
	    ON DV.[Id] = CAST(AV.[Value] AS int)

    -- Update existing attribute value that had previusly used ID
    UPDATE AV	
	    SET [Value] = '283999EC-7346-42E3-B807-BCE9B2BABB49'
    FROM [attribute] A
    INNER JOIN [AttributeValue] AV
    ON AV.[AttributeId] = A.[Id]
    WHERE A.[Guid] = '88021EF1-853C-4FF7-859B-FDDB4C5E9DDC'
" );

            UpdateFieldType( "Categories Field Type", "", "Rock", "Rock.Field.Types.CategoriesFieldType", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1" );

            AddPage( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "New Family", "", "Default", "6A11A13D-05AB-4982-A4C2-67A8B1950C74", "" );
            AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "Attribute Categories", "Categorize attributes based on their associated entity type", "Default", "220D72F5-B589-4378-9852-BBB6F145AD7F", "icon-folder-close" );
            AddPageRoute( "6A11A13D-05AB-4982-A4C2-67A8B1950C74", "NewFamily" );

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

            // Attrib Value for Add Family:Category
            AddBlockAttributeValue( "613536BE-86BC-4755-B815-807C236B92E6", "CF150193-1568-42D8-8E28-5BFCD4954C6F", "" );
            // Attrib Value for Add Family:Require Gender
            AddBlockAttributeValue( "613536BE-86BC-4755-B815-807C236B92E6", "9BBA1555-9809-4A30-9B79-D7501CE1A864", "True" );
            // Attrib Value for Add Family:Require Grade
            AddBlockAttributeValue( "613536BE-86BC-4755-B815-807C236B92E6", "090099D5-D8C4-4C72-B1B0-ACEBA231E68F", "True" );
            // Attrib Value for Add Family:Location Type
            AddBlockAttributeValue( "613536BE-86BC-4755-B815-807C236B92E6", "687E6973-0259-4DB4-B4EB-73708D98EFCD", "8C52E53C-2A66-435A-AE6E-5EE307D9A0DC" );

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

            // Convert Defined Value field types to use Id instead of Guid
            Sql( @"
    UPDATE AV	
	    SET [Value] = CAST(DV.[Id] AS varchar)
    FROM [Attribute] A
    INNER JOIN [FieldType] F 
	    ON F.[Id] = A.[FieldTypeId]
	    AND F.[Class] = 'Rock.Field.Types.DefinedValueFieldType'
    INNER JOIN [AttributeValue] AV
	    ON AV.[AttributeId] = A.[Id]
    INNER JOIN [DefinedValue] DV
	    ON CAST(DV.[Guid] AS varchar(100)) = AV.[Value]
" );
        }
    }
}
