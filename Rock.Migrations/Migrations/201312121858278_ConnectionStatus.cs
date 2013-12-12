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
    public partial class ConnectionStatus : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.Person", "PersonStatusValueId", "dbo.DefinedValue");
            DropIndex("dbo.Person", new[] { "PersonStatusValueId" });
            AddColumn("dbo.Person", "ConnectionStatusValueId", c => c.Int());
            CreateIndex("dbo.Person", "ConnectionStatusValueId");
            AddForeignKey("dbo.Person", "ConnectionStatusValueId", "dbo.DefinedValue", "Id");
            DropColumn("dbo.Person", "PersonStatusValueId");

            Sql( @"
    UPDATE [Attribute] SET [Key] = 'DefaultConnectionStatus' WHERE [key] = 'DefaultPersonStatus'

    -- Delete values that may have been created by framework before running migration
    DELETE [Attribute] WHERE [EntityTypeId] IN (
	    SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.ConnectionStatus'   
    )
    DELETE [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.ConnectionStatus'   
    UPDATE [EntityType] SET 
	    [Name] = 'Rock.PersonProfile.Badge.ConnectionStatus',
	    [AssemblyName] = 'Rock.PersonProfile.Badge.ConnectionStatus, Rock, Version=0.0.1.0, Culture=neutral, PublicKeyToken=null',
	    [FriendlyName] = 'Connection Status'
    WHERE [Name] = 'Rock.PersonProfile.Badge.PersonStatus'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    UPDATE [Attribute] SET [Key] = 'DefaultPersonStatus' WHERE [key] = 'DefaultConnectionStatus'

    -- Delete values that may have been created by framework before running migration
    DELETE [Attribute] WHERE [EntityTypeId] IN (
	    SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.PersonStatus'   
    )
    DELETE [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.PersonStatus'   
    UPDATE [EntityType] SET 
	    [Name] = 'Rock.PersonProfile.Badge.PersonStatus',
	    [AssemblyName] = 'Rock.PersonProfile.Badge.PersonStatus, Rock, Version=0.0.1.0, Culture=neutral, PublicKeyToken=null',
	    [FriendlyName] = 'Person Status'
    WHERE [Name] = 'Rock.PersonProfile.Badge.ConnectionStatus'
" ); 
            
            AddColumn( "dbo.Person", "PersonStatusValueId", c => c.Int() );
            DropForeignKey("dbo.Person", "ConnectionStatusValueId", "dbo.DefinedValue");
            DropIndex("dbo.Person", new[] { "ConnectionStatusValueId" });
            DropColumn("dbo.Person", "ConnectionStatusValueId");
            CreateIndex("dbo.Person", "PersonStatusValueId");
            AddForeignKey("dbo.Person", "PersonStatusValueId", "dbo.DefinedValue", "Id");
        }
    }
}
