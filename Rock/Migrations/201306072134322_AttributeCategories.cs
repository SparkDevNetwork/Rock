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
    public partial class AttributeCategories : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.AttributeCategory",
                c => new
                    {
                        AttributeId = c.Int(nullable: false),
                        CategoryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.AttributeId, t.CategoryId })
                .ForeignKey("dbo.Attribute", t => t.AttributeId, cascadeDelete: true)
                .ForeignKey("dbo.Category", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.AttributeId)
                .Index(t => t.CategoryId);

            UpdateEntityType( "Rock.Model.Attribute", "Attribute", "Rock.Model.Attribute, Rock, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", true, true, "5997C8D3-8840-4591-99A5-552919F90CBD" );

            Sql ( @"
	DECLARE @AttributeEntityId INT
	SET @AttributeEntityId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Attribute')

	;WITH CTE
	AS
	(
		SELECT DISTINCT 
			CAST(EntityTypeId as VARCHAR) AS EntityTypeId,
			Category
		FROM Attribute
		WHERE [Category] IS NOT NULL
		AND [Category] <> ''
	)
    
    INSERT INTO [Category] ( IsSystem, EntityTypeId, EntityTypeQualifierColumn, EntityTypeQualifierValue, Name, Guid)
    SELECT 
		0,
		@AttributeEntityId,
	    'EntityTypeId',
		EntityTypeId,
		Category,
	    NEWID()
    FROM CTE

	INSERT INTO AttributeCategory
	SELECT 
		A.Id,
		C.Id
    FROM Attribute A
	INNER JOIN Category C
		ON C.EntityTypeId = @AttributeEntityId
		AND C.EntityTypeQualifierColumn = 'EntityTypeId'
 	    AND (
		    (C.EntityTypeQualifierValue IS NULL AND A.EntityTypeId IS NULL) OR
		    (C.EntityTypeQualifierValue = CAST(A.EntityTypeId AS VARCHAR))
	    )
	    AND C.Name = A.Category
    WHERE A.[Category] IS NOT NULL
    AND A.[Category] <> ''
" );

            DropColumn("dbo.Attribute", "Category");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Attribute", "Category", c => c.String(maxLength: 100));

            Sql( @"
	DECLARE @AttributeEntityId INT
	SET @AttributeEntityId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Attribute')

    UPDATE A
    SET Category = (
        SELECT TOP 1 C.Name
        FROM AttributeCategory AC
        INNER JOIN Category C ON C.Id = AC.CategoryId
        WHERE AC.AttributeId = A.Id
        ORDER BY C.Name
    )
    FROM Attribute A
    WHERE A.Id IN (SELECT DISTINCT AttributeId FROM AttributeCategory)
" );

            DropIndex( "dbo.AttributeCategory", new[] { "CategoryId" } );
            DropIndex("dbo.AttributeCategory", new[] { "AttributeId" });
            DropForeignKey("dbo.AttributeCategory", "CategoryId", "dbo.Category");
            DropForeignKey("dbo.AttributeCategory", "AttributeId", "dbo.Attribute");
            DropTable("dbo.AttributeCategory");
        }
    }
}
