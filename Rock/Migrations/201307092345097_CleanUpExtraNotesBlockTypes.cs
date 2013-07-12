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
    public partial class CleanUpExtraNotesBlockTypes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // there were two Blocks with a path of '~/Blocks/Core/Notes.ascx', the one with '2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3' is being used, the other isn't
            DeleteBlockType( "599D274D-55C7-4DE6-BB2D-B334D4BD51BC" );
            Sql( @"update [BlockType] set [Description] = 'Context aware block for adding notes to an entity' where [Guid] = '2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // intentionally blank
        }
    }
}
