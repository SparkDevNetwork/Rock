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
    public partial class AttributeEntityTypeId : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.coreAttribute", "EntityTypeId", c => c.Int() );

            Sql( @"
                ;WITH CTE
                AS
                (
                    SELECT DISTINCT [Entity]
                    FROM [coreAttribute] A
                    LEFT OUTER JOIN [coreEntityType] E
                        ON E.[Name] = A.[Entity]
                    WHERE E.[Id] IS NULL
                    AND A.[Entity] IS NOT NULL
                    AND A.[Entity] <> ''
                )
                INSERT INTO [coreEntityType] ([Name],[Guid])
                SELECT [Entity], NEWID()
                FROM CTE

                UPDATE A
                SET [EntityTypeId] = E.[Id]
                FROM [coreAttribute] A
                INNER JOIN [coreEntityType] E
                    ON E.[Name] = A.[Entity]
" );
            DropIndex( "dbo.coreAttribute", new[] { "Entity", "EntityQualifierColumn", "EntityQualifierValue", "Key" } );
            DropIndex( "dbo.coreAttribute", new[] { "Entity" } );
            DropColumn( "dbo.coreAttribute", "Entity" );

            AddForeignKey( "dbo.coreAttribute", "EntityTypeId", "dbo.coreEntityType", "Id" );
            CreateIndex( "dbo.coreAttribute", "EntityTypeId" );

            RenameColumn( table: "dbo.coreAttribute", name: "EntityQualifierColumn", newName: "EntityTypeQualifierColumn" );
            RenameColumn( table: "dbo.coreAttribute", name: "EntityQualifierValue", newName: "EntityTypeQualifierValue" );
            RenameColumn( table: "dbo.coreTag", name: "EntityQualifierColumn", newName: "EntityTypeQualifierColumn" );
            RenameColumn( table: "dbo.coreTag", name: "EntityQualifierValue", newName: "EntityTypeQualifierValue" );

            CreateIndex( "dbo.coreAttribute", new[] { "EntityTypeId", "EntityTypeQualifierColumn", "EntityTypeQualifierValue", "Key" } );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn( "dbo.coreAttribute", "Entity", c => c.String( maxLength: 50 ) );

            Sql( @"
                UPDATE A
                SET [Entity] = E.[Name]
                FROM [coreAttribute] A
                INNER JOIN [coreEntityType] E
                    ON E.[Id] = A.[EntityTypeId]
" );

            DropIndex( "dbo.coreAttribute", new[] { "EntityTypeId", "EntityTypeQualifierColumn", "EntityTypeQualifierValue", "Key" } );
            DropIndex( "dbo.coreAttribute", new[] { "EntityTypeId" } );
            DropForeignKey( "dbo.coreAttribute", "EntityTypeId", "dbo.coreEntityType" );
            DropColumn( "dbo.coreAttribute", "EntityTypeId" );

            CreateIndex( "dbo.coreAttribute", new[] { "Entity" } );

            RenameColumn( table: "dbo.coreAttribute", name: "EntityTypeQualifierColumn", newName: "EntityQualifierColumn" );
            RenameColumn( table: "dbo.coreAttribute", name: "EntityTypeQualifierValue", newName: "EntityQualifierValue" );
            RenameColumn( table: "dbo.coreTag", name: "EntityTypeQualifierColumn", newName: "EntityQualifierColumn" );
            RenameColumn( table: "dbo.coreTag", name: "EntityTypeQualifierValue", newName: "EntityQualifierValue" );

            CreateIndex( "dbo.coreAttribute", new[] { "Entity", "EntityQualifierColumn", "EntityQualifierValue", "Key" } );
        }
    }
}
