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
    public partial class GoogleAPIKeyGlobalAttribute : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddGlobalAttribute( "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Google API Key", "Google API key for browser apps (with referers) as described here: https://developers.google.com/maps/documentation/javascript/tutorial#api_key", 0, "", "d8b02008-b672-4414-95b1-616d62676056" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "d8b02008-b672-4414-95b1-616d62676056" );
        }
    }
}
