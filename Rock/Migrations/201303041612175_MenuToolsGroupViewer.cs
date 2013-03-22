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
    public partial class MenuToolsGroupViewer : RockMigration_4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
-- up F7105BFE-B28C-41B6-9CE6-F1018D77DD8F - Tools/Website and Communications
update [Page] set [Order] = 7, ParentPageId = (select [Id] from [Page] where [Guid] = 'F7105BFE-B28C-41B6-9CE6-F1018D77DD8F') where [Guid] = '4E237286-B715-4109-A578-C1445EC02707' 
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
-- down 91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F - Person Settings
update [Page] set [Order] = 0, ParentPageId = (select [Id] from [Page] where [Guid] = '91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F') where [Guid] = '4E237286-B715-4109-A578-C1445EC02707' 
" );
        }
    }
}
