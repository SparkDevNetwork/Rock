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
    public partial class LocationTypes : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddDefinedType( "Location", "Location Type", "Location Types", "2e68d37c-fb7b-4aa5-9e09-3785d52156cb" );
            AddDefinedValue( "2e68d37c-fb7b-4aa5-9e09-3785d52156cb", "Home", "Home", "8C52E53C-2A66-435A-AE6E-5EE307D9A0DC" );
            AddDefinedValue( "2e68d37c-fb7b-4aa5-9e09-3785d52156cb", "Office", "Office", "E071472A-F805-4FC4-917A-D5E3C095C35C" );
            AddDefinedValue( "2e68d37c-fb7b-4aa5-9e09-3785d52156cb", "Business", "Business", "C89D123C-8645-4B96-8C71-6C87B5A96525" );
            AddDefinedValue( "2e68d37c-fb7b-4aa5-9e09-3785d52156cb", "Sports Field", "Sports Field", "F560DC25-E964-46C4-8CEF-0E67BB922163" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteDefinedValue( "F560DC25-E964-46C4-8CEF-0E67BB922163" );
            DeleteDefinedValue( "C89D123C-8645-4B96-8C71-6C87B5A96525" );
            DeleteDefinedValue( "E071472A-F805-4FC4-917A-D5E3C095C35C" );
            DeleteDefinedValue( "8C52E53C-2A66-435A-AE6E-5EE307D9A0DC" );
            DeleteDefinedType( "2e68d37c-fb7b-4aa5-9e09-3785d52156cb" );
        }
    }
}
