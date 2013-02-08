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
    public partial class FixMissingLoginStatusTwoColumnLeft : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlock( "", "04712F3D-9667-4901-A49D-4507573EF7AD", "Login Status", "TwoColumnLeft", "zHeader", 0, "05719CF7-283B-45A2-AE12-A2815D14E7FD" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlock( "05719CF7-283B-45A2-AE12-A2815D14E7FD" );
        }
    }
}
