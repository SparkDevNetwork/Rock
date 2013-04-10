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
    public partial class EntityTypeAttributeValueByName : RockMigration_4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Workflow Tree
            AddBlockAttributeValue( "3DAAE6A6-E8AC-435F-A449-96E75D9E8ACA", "06D414F0-AA20-4D3C-B297-1530CCD64395", "Rock.Model.WorkflowType" );
            
            // Workflow Category Detail
            AddBlockAttributeValue( "DE94148B-7DF8-418D-A905-066740765AD4", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297", "Rock.Model.WorkflowType" );

            // Dataview Tree
            AddBlockAttributeValue( "6A9111AC-34E7-4103-A12A-9A89C2A14B57", "06D414F0-AA20-4D3C-B297-1530CCD64395", "Rock.Model.DataView" );

            // Dataview Category Detail
            AddBlockAttributeValue( "5BAD3495-6434-4663-A940-1DAC3AC0B643", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297", "Rock.Model.DataView" );

            // Report Category Tree
            AddBlockAttributeValue( "0F1F8343-A187-4653-9A4A-47D67CE86D71", "06D414F0-AA20-4D3C-B297-1530CCD64395", "Rock.Model.Report" );

            // Report Category Detail
            AddBlockAttributeValue( "76545653-16E1-4227-82D2-63295755D8D3", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297", "Rock.Model.Report" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Workflow Tree
            AddBlockAttributeValue( "3DAAE6A6-E8AC-435F-A449-96E75D9E8ACA", "06D414F0-AA20-4D3C-B297-1530CCD64395", "32" );

            // Workflow Category Detail
            AddBlockAttributeValue( "DE94148B-7DF8-418D-A905-066740765AD4", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297", "32" );

            // Dataview Tree
            AddBlockAttributeValue( "6A9111AC-34E7-4103-A12A-9A89C2A14B57", "06D414F0-AA20-4D3C-B297-1530CCD64395", "34" );

            // Dataview Category Detail
            AddBlockAttributeValue( "5BAD3495-6434-4663-A940-1DAC3AC0B643", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297", "34" );

            // Report Category Tree
            AddBlockAttributeValue( "0F1F8343-A187-4653-9A4A-47D67CE86D71", "06D414F0-AA20-4D3C-B297-1530CCD64395", "36" );

            // Report Category Detail
            AddBlockAttributeValue( "76545653-16E1-4227-82D2-63295755D8D3", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297", "36" );
        }
    }
}
