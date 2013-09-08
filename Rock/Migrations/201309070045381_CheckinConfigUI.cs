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
    public partial class CheckinConfigUI : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Delete and Add the blocktype to update the name
            DeleteBlockType( "2506B048-F62C-4945-B09A-1E053F66C592" );
            AddBlockType("Administration - Check-in Configuration","","~/Blocks/Administration/CheckinConfiguration.ascx","2506B048-F62C-4945-B09A-1E053F66C592");

            // Add Block to Page: Configuration
            AddBlock("4AB679AF-C8CC-427C-A615-0BF9F52E8E3E","2506B048-F62C-4945-B09A-1E053F66C592","Check-in Configuration","","Content",0,"5F2EA21F-CB8A-4A6B-9E33-A3D8570DC716");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlock( "5F2EA21F-CB8A-4A6B-9E33-A3D8570DC716" );
            DeleteBlockType( "2506B048-F62C-4945-B09A-1E053F66C592" );
        }
    }
}
