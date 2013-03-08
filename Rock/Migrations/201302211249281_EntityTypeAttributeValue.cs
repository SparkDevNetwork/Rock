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
    public partial class EntityTypeAttributeValue : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
DECLARE @AttributeId int
SET @AttributeId = (SELECT [ID] FROM [Attribute] WHERE [Key] = 'EntityTypeName')
IF @AttributeId IS NOT NULL
BEGIN
    UPDATE [Attribute] SET [Key] = 'EntityType' WHERE [Id] = @AttributeId
    UPDATE AV 
	    SET [Value] = CAST(ET.[Id] AS varchar)
    FROM [AttributeValue] AV
    INNER JOIN [EntityType] ET
	    ON ET.[Name] = AV.[Value]
    WHERE AV.[AttributeId] = @AttributeId
END
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
DECLARE @AttributeId int
SET @AttributeId = (SELECT [ID] FROM [Attribute] WHERE [Key] = 'EntityType')
IF @AttributeId IS NOT NULL
BEGIN
    UPDATE [Attribute] SET [Key] = 'EntityTypeName' WHERE [Id] = @AttributeId
    UPDATE AV 
	    SET [Value] = ET.[Name]
    FROM [AttributeValue] AV
    INNER JOIN [EntityType] ET
	    ON CAST(ET.[Id] AS varchar) = AV.[Value]
    WHERE AV.[AttributeId] = @AttributeId
END
" );
        }
    }
}
