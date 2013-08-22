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
    public partial class GroupTypePurposeCheckinFilter : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddDefinedValue( "B23F1E45-BC26-4E82-BEB3-9B191FE5CCC3", "Check-in Filter", "A Group Type where the purpose is for check-in filter.", "6BCED84C-69AD-4F5A-9197-5C0F9C02DD34" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteDefinedValue( "6BCED84C-69AD-4F5A-9197-5C0F9C02DD34" );
        }
    }
}
