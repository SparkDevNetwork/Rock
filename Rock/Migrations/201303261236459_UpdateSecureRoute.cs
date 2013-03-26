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
    public partial class UpdateSecureRoute : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
            Sql( @"
    UPDATE [PageRoute] SET [Route] = 'Secure/{EntityTypeId}/{EntityId}' 
	    WHERE [Route] = 'Secure/{EntityType}/{EntityId}'
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
            Sql( @"
    UPDATE [PageRoute] SET [Route] = 'Secure/{EntityType}/{EntityId}' 
	    WHERE [Route] = 'Secure/{EntityTypeId}/{EntityId}'
" );

        }
    }
}
