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
    public partial class ContextAwareAttribute : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
                UPDATE A SET
	                 [Key] = 'ContextEntityType'
	                ,[Name] = 'Context Entity Type'
	                ,[Description] = 'Context Entity Type'
                FROM [cmsBlock] B
                INNER JOIN [cmsBlockType] BT
	                ON BT.[Id] = B.[BlockTypeId]
                INNER JOIN [coreAttribute] A
	                ON A.[Entity] = 'Rock.Cms.Block'
	                AND A.[EntityQualifierColumn] = 'BlockTypeId'
	                AND A.[EntityQualifierValue] = CAST(BT.[Id] AS varchar)
	                AND A.[Key] = 'Entity'
                WHERE BT.[Path] in ('~/Blocks/Core/AttributeCategoryView.ascx','~/Blocks/Core/ContextAttributeValues.ascx','~/Blocks/Core/Tags.ascx')
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
                UPDATE A SET
	                 [Key] = 'Entity'
	                ,[Name] = 'Entity'
	                ,[Description] = 'Context Entity Type'
                FROM [cmsBlock] B
                INNER JOIN [cmsBlockType] BT
	                ON BT.[Id] = B.[BlockTypeId]
                INNER JOIN [coreAttribute] A
	                ON A.[Entity] = 'Rock.Cms.Block'
	                AND A.[EntityQualifierColumn] = 'BlockTypeId'
	                AND A.[EntityQualifierValue] = CAST(BT.[Id] AS varchar)
	                AND A.[Key] = 'Entity'
                WHERE BT.[Path] in ('~/Blocks/Core/AttributeCategoryView.ascx','~/Blocks/Core/ContextAttributeValues.ascx','~/Blocks/Core/Tags.ascx')
" );
        }
    }
}
