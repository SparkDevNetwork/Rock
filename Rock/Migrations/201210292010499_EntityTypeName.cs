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
    public partial class EntityTypeName : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.coreEntityType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 100),
                        FriendlyName = c.String(maxLength: 100),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo.coreEntityType", "Name", true );
            CreateIndex( "dbo.coreEntityType", "Guid", true );

            Sql( "UPDATE [CmsAuth] SET [EntityType] = 'Rock.' + [EntityType] WHERE [EntityType] <> 'Global'" );
            Sql( "UPDATE [CoreAudit] SET [EntityType] = 'Rock.' + [EntityType]" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( "UPDATE [CmsAuth] SET [EntityType] = REPLACE([EntityType], 'Rock.', '')" );
            Sql( "UPDATE [coreAudit] SET [EntityType] = REPLACE([EntityType], 'Rock.', '')" );

            DropTable( "dbo.coreEntityType" );
        }
    }
}
