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
    public partial class FixMyAccountRoute : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DELETE [PageRoute] WHERE [Route] = 'MyAccount';

    DECLARE @BlockTypeID int
    SET @BlockTypeID = (SELECT [Id] FROM [BlockType] WHERE [Path] = '~/Blocks/Security/LoginStatus.ascx')

    DECLARE @RockSiteId int
    SET @RockSiteId = (SELECT [Id] FROM [Site] WHERE [Guid] = 'C2D29296-6A87-47A9-A753-EE4E9159C4C4')

    DECLARE @StarkSiteId int
    SET @StarkSiteId = (SELECT [Id] FROM [Site] WHERE [Guid] = 'F3F82256-2D66-432B-9D67-3552CD2F4C2B')

    DECLARE @AttributeId int
    SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeID as varchar))

    DELETE [AttributeValue]
    WHERE [AttributeId] = @AttributeId
    AND [EntityId] IN (
	    SELECT B.[Id] 
	    FROM [Block] B
	    INNER JOIN [Layout] L 
		    ON L.[Id] = B.[LayoutId] 
		    AND ( L.[SiteId] = @StarkSiteId OR L.[SiteId] = @RockSiteId)
	    WHERE [BlockTypeId] = @BlockTypeID
    )

    INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid])
    SELECT 1, @AttributeId, B.[Id], 0, '290C53DC-0960-484C-B314-8301882A454C', NEWID()
    FROM [Block] B
    INNER JOIN [Layout] L 
	    ON L.[Id] = B.[LayoutId] 
	    AND L.[SiteId] = @RockSiteId
    WHERE [BlockTypeId] = @BlockTypeID
" );

            AddPageRoute( "C0854F84-2E8B-479C-A3FB-6B47BE89B795", "MyAccount" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
