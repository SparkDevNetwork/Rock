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
    public partial class ChangeXsltFoAdminPageBlock : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    -- Update location of xslt file
    UPDATE [coreAttributeValue] SET [Value] = '~/Assets/XSLT/PageListAsBlocks.xslt' WHERE [Guid] = '26AF0210-F562-40FE-A171-62295D87836D'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    -- Update location of xslt file
    UPDATE [coreAttributeValue] SET [Value] = '~/Assets/XSLT/AdminPageList.xslt' WHERE [Guid] = '26AF0210-F562-40FE-A171-62295D87836D'
" );
        }
    }
}
