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
    DECLARE @AttributeId int
    SET @AttributeId = (SELECT [id] FROM [coreAttribute] WHERE guid = 'D8A029F8-83BE-454A-99D3-94D879EBF87C')
    UPDATE [coreAttributeValue] 
        SET [Value] = '~/Assets/XSLT/PageListAsBlocks.xslt' 
    WHERE [AttributeId] = @AttributeId 
    AND [Value] = '~/Assets/XSLT/AdminPageList.xslt'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    -- Update location of xslt file
    DECLARE @AttributeId int
    SET @AttributeId = (SELECT [id] FROM [coreAttribute] WHERE guid = 'D8A029F8-83BE-454A-99D3-94D879EBF87C')
    UPDATE [coreAttributeValue] 
        SET [Value] = '~/Assets/XSLT/AdminPageList.xslt' 
    WHERE [AttributeId] = @AttributeId 
    AND [Value] = '~/Assets/XSLT/PageListAsBlocks.xslt'
" );
        }
    }
}
