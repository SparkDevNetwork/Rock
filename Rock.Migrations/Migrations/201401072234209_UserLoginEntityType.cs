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
    public partial class UserLoginEntityType : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.UserLogin", "EntityTypeId", c => c.Int( nullable: true ) );

            Sql( @"
    UPDATE L 
    SET [EntityTypeId] = E.[Id]
    FROM [UserLogin] L
    INNER JOIN [EntityType] E ON E.[Name] = L.[ServiceName]
" );
            AlterColumn( "dbo.UserLogin", "EntityTypeId", c => c.Int( nullable: false ) );
            CreateIndex( "dbo.UserLogin", "EntityTypeId" );
            AddForeignKey( "dbo.UserLogin", "EntityTypeId", "dbo.EntityType", "Id" );
            DropColumn( "dbo.UserLogin", "ServiceName" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.UserLogin", "ServiceName", c => c.String(nullable: false, maxLength: 200));
            DropForeignKey("dbo.UserLogin", "EntityTypeId", "dbo.EntityType");
            DropIndex("dbo.UserLogin", new[] { "EntityTypeId" });

            Sql( @"
    UPDATE L 
    SET [ServiceName] = E.[Name]
    FROM [UserLogin] L
    INNER JOIN [EntityType] E ON E.[ID] = L.[EntityTypeId]
" );
            DropColumn("dbo.UserLogin", "EntityTypeId");
        }
    }
}
