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
    public partial class GlobalAttributeDefaultsForOrg : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddGlobalAttribute( "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Organization Address", "The primary mailing address for the organization.", 1, "555 E Main St, Kansas, USA", "E132C358-F28E-45BD-B357-6A2F8B24743A" );
            UpdateAttribute( "410bf494-0714-4e60-afbd-ad65899a12be", "[DefaultValue]", "'Rock Solid Church'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "E132C358-F28E-45BD-B357-6A2F8B24743A" );
            UpdateAttribute( "410bf494-0714-4e60-afbd-ad65899a12be", "[DefaultValue]", "'Our Organization Name'" );
        }
    }
}
