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
    public partial class AuthEntityType : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.cmsAuth", "EntityTypeId", c => c.Int(nullable: true));

            Sql( @"
                ;WITH CTE
                AS
                (
                    SELECT DISTINCT [EntityType]
                    FROM [cmsAuth] A
                    LEFT OUTER JOIN [coreEntityType] E
                        ON E.[Name] = A.[EntityType]
                    WHERE E.[Id] IS NULL
                )
                INSERT INTO [coreEntityType] ([Name],[Guid])
                SELECT [EntityType], NEWID()
                FROM CTE

                UPDATE A
                SET [EntityTypeId] = E.[Id]
                FROM [cmsAuth] A
                INNER JOIN [coreEntityType] E
                    ON E.[Name] = A.[EntityType]

                ALTER TABLE [cmsAuth] ALTER COLUMN [EntityTypeId] int not null
" );

            DropIndex( "cmsAuth", new[] { "EntityType", "EntityId" } );
            AddForeignKey( "dbo.cmsAuth", "EntityTypeId", "dbo.coreEntityType", "Id" );
            CreateIndex("dbo.cmsAuth", "EntityTypeId");
            DropColumn("dbo.cmsAuth", "EntityType");
            CreateIndex( "dbo.cmsAuth", new[] { "EntityTypeId", "EntityId" } );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.cmsAuth", "EntityType", c => c.String(nullable: true, maxLength: 200));

            Sql( @"

                UPDATE A
                SET [EntityType] = E.[Name]
                FROM [cmsAuth] A
                INNER JOIN [coreEntityType] E
                    ON E.[Id] = A.[EntityTypeId]

                ALTER TABLE [cmsAuth] ALTER COLUMN [EntityType] varchar(200) not null
" );
            DropIndex( "dbo.cmsAuth", new[] { "EntityTypeId", "EntityId" } );
            DropIndex( "dbo.cmsAuth", new[] { "EntityTypeId" } );
            DropForeignKey("dbo.cmsAuth", "EntityTypeId", "dbo.coreEntityType");
            DropColumn("dbo.cmsAuth", "EntityTypeId");
            CreateIndex( "cmsAuth", new[] { "EntityType", "EntityId" } );
        }
    }
}
