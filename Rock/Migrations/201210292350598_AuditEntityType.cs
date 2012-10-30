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
    public partial class AuditEntityType : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Update coreAudit to use EntityTypeId foreign key instead of EntityType text field
            AddColumn("dbo.coreAudit", "EntityTypeId", c => c.Int(nullable: true));

            Sql( @"
                ;WITH CTE
                AS
                (
                    SELECT DISTINCT [EntityType]
                    FROM [coreAudit] A
                    LEFT OUTER JOIN [coreEntityType] E
                        ON E.[Name] = A.[EntityType]
                    WHERE E.[Id] IS NULL
                )
                INSERT INTO [coreEntityType] ([Name],[Guid])
                SELECT [EntityType], NEWID()
                FROM CTE

                UPDATE A
                SET [EntityTypeId] = E.[Id]
                FROM [coreAudit] A
                INNER JOIN [coreEntityType] E
                    ON E.[Name] = A.[EntityType]

                ALTER TABLE [coreAudit] ALTER COLUMN [EntityTypeId] int not null
" );

            AddForeignKey( "dbo.coreAudit", "EntityTypeId", "dbo.coreEntityType", "Id" );
            CreateIndex("dbo.coreAudit", "EntityTypeId");
            DropColumn("dbo.coreAudit", "EntityType");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Update coreAudit to use EntityType text field instead of EntityTypeId foreign key
            AddColumn( "dbo.coreAudit", "EntityType", c => c.String( nullable: true, maxLength: 100 ) );

            Sql( @"

                UPDATE A
                SET [EntityType] = E.[Name]
                FROM [coreAudit] A
                INNER JOIN [coreEntityType] E
                    ON E.[Id] = A.[EntityTypeId]

                ALTER TABLE [coreAudit] ALTER COLUMN [EntityType] varchar(100) not null
" );

            DropIndex( "dbo.coreAudit", new[] { "EntityTypeId" } );
            DropForeignKey("dbo.coreAudit", "EntityTypeId", "dbo.coreEntityType");
            DropColumn("dbo.coreAudit", "EntityTypeId");
        }
    }
}
