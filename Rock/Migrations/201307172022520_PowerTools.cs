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
    public partial class PowerTools : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "550A898C-EDEA-48B5-9C58-B20EC13AF13B", "Power Tools", "Advanced configuration options", "Default", "7F1F4130-CB98-473B-9DE1-7A886D2283ED", "" );
            AddPage( "7F1F4130-CB98-473B-9DE1-7A886D2283ED", "Entity Attributes", "Manage attributes for any type of entity and qualifier", "Default", "23507C90-3F78-40D4-B847-6FE8941FCD32", "icon-pushpin" );

            AddBlock( "23507C90-3F78-40D4-B847-6FE8941FCD32", "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "Attributes", "", "Content", 0, "1B8BA918-FEE5-4B69-966C-3D79D555A761" );
            AddBlock( "7F1F4130-CB98-473B-9DE1-7A886D2283ED", "F49AD5F8-1E45-41E7-A88E-8CD285815BD9", "Page Xslt Transformation", "", "Content", 0, "EEDE1257-AC1F-4B7E-9177-BCF87F3B8C00" );
            
            // Attrib for BlockType: Attributes:Configure Type
            AddBlockTypeAttribute( "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Configure Type", "ConfigureType", "", "Only show attributes for type specified below", 0, "True", "D4132497-18BE-4D1F-8913-468E33DE63C4" );

            // Attrib Value for Block:Attributes, Attribute:Entity Qualifier Column, Page:Entity Attributes
            AddBlockAttributeValue( "1B8BA918-FEE5-4B69-966C-3D79D555A761", "ECD5B86C-2B48-4548-9FE9-7AC6F6FA8106", "" );
            // Attrib Value for Block:Attributes, Attribute:Entity Qualifier Value, Page:Entity Attributes
            AddBlockAttributeValue( "1B8BA918-FEE5-4B69-966C-3D79D555A761", "FCE1E87D-F816-4AD5-AE60-1E71942C547C", "" );
            // Attrib Value for Block:Attributes, Attribute:Entity, Page:Entity Attributes
            AddBlockAttributeValue( "1B8BA918-FEE5-4B69-966C-3D79D555A761", "5B33FE25-6BF0-4890-91C6-49FB1629221E", "" );
            // Attrib Value for Block:Attributes, Attribute:Entity Id, Page:Entity Attributes
            AddBlockAttributeValue( "1B8BA918-FEE5-4B69-966C-3D79D555A761", "CBB56D68-3727-42B9-BF13-0B2B593FB328", "0" );
            // Attrib Value for Block:Attributes, Attribute:Allow Setting of Values, Page:Entity Attributes
            AddBlockAttributeValue( "1B8BA918-FEE5-4B69-966C-3D79D555A761", "018C0016-C253-44E4-84DB-D166084C5CAD", "False" );
            // Attrib Value for Block:Attributes, Attribute:Configure Type, Page:Entity Attributes
            AddBlockAttributeValue( "1B8BA918-FEE5-4B69-966C-3D79D555A761", "D4132497-18BE-4D1F-8913-468E33DE63C4", "False" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Root Page, Page:Power Tools
            AddBlockAttributeValue( "EEDE1257-AC1F-4B7E-9177-BCF87F3B8C00", "DD516FA7-966E-4C80-8523-BEAC91C8EEDA", "00000000-0000-0000-0000-000000000000" );
            // Attrib Value for Block:Page Xslt Transformation, Attribute:XSLT File, Page:Power Tools
            AddBlockAttributeValue( "EEDE1257-AC1F-4B7E-9177-BCF87F3B8C00", "D8A029F8-83BE-454A-99D3-94D879EBF87C", "~/Assets/XSLT/PageListAsBlocks.xslt" );
            // Attrib Value for Block:Page Xslt Transformation, Attribute:Number of Levels, Page:Power Tools
            AddBlockAttributeValue( "EEDE1257-AC1F-4B7E-9177-BCF87F3B8C00", "9909E07F-0E68-43B8-A151-24D03C795093", "1" );
            // Attrib Value for Block:Page Xslt Transformation, Attribute:Include Current Parameters, Page:Power Tools
            AddBlockAttributeValue( "EEDE1257-AC1F-4B7E-9177-BCF87F3B8C00", "A0B1F15A-8735-48CA-AFF5-10ED7DD24EA7", "False" );
            // Attrib Value for Block:Page Xslt Transformation, Attribute:Include Current QueryString, Page:Power Tools
            AddBlockAttributeValue( "EEDE1257-AC1F-4B7E-9177-BCF87F3B8C00", "09D9AB6B-B2E6-4E9C-AAFE-25758BE2754B", "False" );

            UpdateFieldType( "Category Field Type", "", "Rock", "Rock.Field.Types.CategoryFieldType", "309460EF-0CC5-41C6-9161-B3837BA3D374" );

            MovePage( "03C49950-9C4C-4668-9C65-9A0DF43D1B33", "7F1F4130-CB98-473B-9DE1-7A886D2283ED" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            MovePage( "03C49950-9C4C-4668-9C65-9A0DF43D1B33", "BB0ACD18-24FB-42BA-B89A-2FFD80472F5B" );

            // Attrib for BlockType: Attributes:Configure Type
            DeleteAttribute( "D4132497-18BE-4D1F-8913-468E33DE63C4" );

            DeleteBlock( "EEDE1257-AC1F-4B7E-9177-BCF87F3B8C00" ); // Page Xslt Transformation
            DeleteBlock( "1B8BA918-FEE5-4B69-966C-3D79D555A761" ); // Attributes
            
            DeletePage( "7F1F4130-CB98-473B-9DE1-7A886D2283ED" ); // Power Tools
            DeletePage( "23507C90-3F78-40D4-B847-6FE8941FCD32" ); // Entity Attributes

        }
    }
}
