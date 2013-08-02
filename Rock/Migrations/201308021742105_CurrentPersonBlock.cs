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
    public partial class CurrentPersonBlock : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlockType( "Cms - Current Person", "", "~/Blocks/Cms/CurrentPerson.ascx", "F7193487-1234-49D7-9CEC-7F5F452B7E3F" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlockType( "F7193487-1234-49D7-9CEC-7F5F452B7E3F" ); // Cms - Current Person
        }
    }
}
