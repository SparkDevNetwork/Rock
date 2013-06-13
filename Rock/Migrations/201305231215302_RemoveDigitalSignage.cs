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
    public partial class RemoveDigitalSignage : RockMigration_5
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DeleteDefinedValue( "01B585B1-389D-4C86-A9DA-267A8564699D" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddDefinedValue( "0368B637-327A-4F5E-80C2-832079E482EE", "Digital Signage", "Computer used to display promoted content", "01B585B1-389D-4C86-A9DA-267A8564699D", true );
        }
    }
}
