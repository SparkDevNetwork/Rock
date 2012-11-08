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
    public partial class RenameGroupBlockTypePath : RockMigration_2
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"update cmsBlockType set [path] = '~/Blocks/Crm/Groups.ascx' where [Guid] = '3D7FB6BE-6BBD-49F7-96B4-96310AF3048A'");
            Sql( @"update cmsBlockType set [path] = '~/Blocks/Crm/GroupTypes.ascx' where [Guid] = 'C443D72B-1A9E-41E7-8E70-4E9D39AE6AC3'");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"update cmsBlockType set [path] = '~/Blocks/Groups/Groups.ascx' where [Guid] = '3D7FB6BE-6BBD-49F7-96B4-96310AF3048A'" );
            Sql( @"update cmsBlockType set [path] = '~/Blocks/Groups/GroupTypes.ascx' where [Guid] = 'C443D72B-1A9E-41E7-8E70-4E9D39AE6AC3'" );
        }
    }
}
