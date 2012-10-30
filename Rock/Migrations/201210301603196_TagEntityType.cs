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
    public partial class TagEntityType : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Update coreTag to use EntityTypeId foreign key instead of Entity text field
            AddColumn( "dbo.coreTag", "EntityTypeId", c => c.Int( nullable: false ) );

            Sql( @"
                ;WITH CTE
                AS
                (
                    SELECT DISTINCT [Entity]
                    FROM [coreTag] A
                    LEFT OUTER JOIN [coreEntityType] E
                        ON E.[Name] = A.[Entity]
                    WHERE E.[Id] IS NULL
                )
                INSERT INTO [coreEntityType] ([Name],[Guid])
                SELECT [Entity], NEWID()
                FROM CTE

                UPDATE A
                SET [EntityTypeId] = E.[Id]
                FROM [coreTag] A
                INNER JOIN [coreEntityType] E
                    ON E.[Name] = A.[Entity]

                ALTER TABLE [coreTag] ALTER COLUMN [EntityTypeId] int not null
" );

            AddForeignKey( "dbo.coreTag", "EntityTypeId", "dbo.coreEntityType", "Id", cascadeDelete: true );
            CreateIndex("dbo.coreTag", "EntityTypeId");
            DropColumn("dbo.coreTag", "Entity");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Update coreTag to use Entity text field instead of EntityTypeId foreign key
            AddColumn( "dbo.coreTag", "Entity", c => c.String( maxLength: 50 ) );

            Sql( @"

                UPDATE A
                SET [Entity] = E.[Name]
                FROM [coreTag] A
                INNER JOIN [coreEntityType] E
                    ON E.[Id] = A.[EntityTypeId]

                ALTER TABLE [coreTag] ALTER COLUMN [Entity] varchar(50) not null
" );
            DropIndex( "dbo.coreTag", new[] { "EntityTypeId" } );
            DropForeignKey("dbo.coreTag", "EntityTypeId", "dbo.coreEntityType");
            DropColumn("dbo.coreTag", "EntityTypeId");
        }
    }
}
