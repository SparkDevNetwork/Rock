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
    public partial class RangeAttributeFieldTypes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateFieldType( "Date Range", "Date Range Field Type", "Rock", "Rock.Field.Types.DateRangeFieldType", "9C7D431C-875C-4792-9E76-93F3A32BB850" );
            UpdateFieldType( "Decimal Range", "Decimal Range Field Type", "Rock", "Rock.Field.Types.DecimalRangeFieldType", "758D9648-573E-4800-B5AF-7CC29F4BE170" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
