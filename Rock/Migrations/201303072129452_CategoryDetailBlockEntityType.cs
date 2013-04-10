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
    public partial class CategoryDetailBlockEntityType : RockMigration_4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib Value for Category Detail:Entity Type
            AddBlockAttributeValue( "DE94148B-7DF8-418D-A905-066740765AD4", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297", "32" );

            // Attrib Value for Category Detail:Entity Type
            AddBlockAttributeValue( "5BAD3495-6434-4663-A940-1DAC3AC0B643", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297", "34" );

            // Attrib Value for Category Detail:Entity Type
            AddBlockAttributeValue( "76545653-16E1-4227-82D2-63295755D8D3", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297", "79" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib Value for Category Detail:Entity Type
            DeleteBlockAttributeValue( "DE94148B-7DF8-418D-A905-066740765AD4", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297");

            // Attrib Value for Category Detail:Entity Type
            DeleteBlockAttributeValue( "5BAD3495-6434-4663-A940-1DAC3AC0B643", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297");

            // Attrib Value for Category Detail:Entity Type
            DeleteBlockAttributeValue( "76545653-16E1-4227-82D2-63295755D8D3", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297");
        }
    }
}
