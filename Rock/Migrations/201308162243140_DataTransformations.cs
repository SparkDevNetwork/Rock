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
    public partial class DataTransformations : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "Data Transformations", "Custom plugins that can be used to transform report data created using data filters", "Default", "9C569E6B-F745-40E4-B91B-A518CD6C2922", "icon-random" );
            AddBlock( "9C569E6B-F745-40E4-B91B-A518CD6C2922", "21F5F466-59BC-40B2-8D73-7314D936C3CB", "Data Transformations", "", "Content", 0, "F87CE2AA-8B89-4D2D-9EF4-3C2CC8D48440" );
            AddBlockAttributeValue( "F87CE2AA-8B89-4D2D-9EF4-3C2CC8D48440", "259AF14D-0214-4BE4-A7BF-40423EA07C99", "Rock.Reporting.DataTransformContainer, Rock" );

            UpdateEntityType("Rock.Reporting.DataTransform.Person.ParentTransform", "887FB3C0-E71A-4041-8DB5-C255EA0D8637");
            AddEntityAttribute( "Rock.Reporting.DataTransform.Person.ParentTransform", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should service be used?", 0, "True", "E02689B4-CB79-40FB-B52B-6CC772B5CBB5" );
            AddEntityAttribute( "Rock.Reporting.DataTransform.Person.ParentTransform", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "", "", "Order", "", "The order that this service should be used (priority)", 0, "0", "37F2A7B2-9426-4C37-BB63-36781F88F0C4" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "E02689B4-CB79-40FB-B52B-6CC772B5CBB5" );
            DeleteAttribute( "37F2A7B2-9426-4C37-BB63-36781F88F0C4" );

            DeleteBlockAttributeValue( "F87CE2AA-8B89-4D2D-9EF4-3C2CC8D48440", "259AF14D-0214-4BE4-A7BF-40423EA07C99" );
            DeleteBlock( "F87CE2AA-8B89-4D2D-9EF4-3C2CC8D48440" );
            DeletePage( "9C569E6B-F745-40E4-B91B-A518CD6C2922" );

        }
    }
}
