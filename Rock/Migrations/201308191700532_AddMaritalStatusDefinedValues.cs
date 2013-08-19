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
    public partial class AddMaritalStatusDefinedValues : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS is b4b92c3f-a935-40e1-a00b-ba484ead613b
            // PERSON_MARITAL_STATUS_MARRIED = "5FE5A540-7D9F-433E-B47E-4229D1472248" (this is a System Value since we'll have logic for it)
            AddDefinedValue( "b4b92c3f-a935-40e1-a00b-ba484ead613b", "Married", "", "5FE5A540-7D9F-433E-B47E-4229D1472248", true );

            // Single not a System value, since we don't need to do logic on it, and they could delete it and replace it with other NotMarried Defined Values)
            AddDefinedValue( "b4b92c3f-a935-40e1-a00b-ba484ead613b", "Single", "", "F19FC180-FE8F-4B72-A59C-8013E3B0EB0D", false );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteDefinedValue( "F19FC180-FE8F-4B72-A59C-8013E3B0EB0D" );
            DeleteDefinedValue( "5FE5A540-7D9F-433E-B47E-4229D1472248" );
        }
    }
}
