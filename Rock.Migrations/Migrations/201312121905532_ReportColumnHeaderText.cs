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
    public partial class ReportColumnHeaderText : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.ReportField", "ColumnHeaderText", c => c.String() );

            // Fix up Category for Report Category Tree and Report Category Detail
            Sql( @"update AttributeValue set Value = 'f1f22d3e-fefa-4c84-9ffa-9e8ace60fce7' where [Guid] = 'CF04A81D-A95F-47E4-BE65-04A641607FA2'" );
            Sql( @"update AttributeValue set Value = 'f1f22d3e-fefa-4c84-9ffa-9e8ace60fce7' where [Guid] = '603B229C-047A-4995-BF7F-2F93450204FB'" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.ReportField", "ColumnHeaderText" );
        }
    }
}
