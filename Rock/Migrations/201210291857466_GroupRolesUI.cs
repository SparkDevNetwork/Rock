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
    public partial class GroupRolesUI : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlockType( "GroupRoles", "Allows for the configuration fof Group Roles", "~/Blocks/Crm/GroupRoles.ascx", "89315EBC-D4BD-41E6-B1F1-929D19E66608" );
            AddPage( "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F", "Group Roles", "Manage Group Roles", "BBD61BB9-7BE0-4F16-9615-91D38F3AE9C9" );
            AddBlock( "BBD61BB9-7BE0-4F16-9615-91D38F3AE9C9", "89315EBC-D4BD-41E6-B1F1-929D19E66608", "Group Roles", "Content", "1064932B-F0DB-4F39-B438-24703A14198B" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlock( "1064932B-F0DB-4F39-B438-24703A14198B" );
            DeletePage( "BBD61BB9-7BE0-4F16-9615-91D38F3AE9C9" );
            DeleteBlockType( "89315EBC-D4BD-41E6-B1F1-929D19E66608" );
        }
    }
}
