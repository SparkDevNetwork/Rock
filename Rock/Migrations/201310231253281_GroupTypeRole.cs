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
    public partial class GroupTypeRole : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( "UPDATE [GroupRole] SET [SortOrder] = 0 WHERE [SortOrder] IS NULL" );
            AlterColumn( "dbo.GroupRole", "SortOrder", c => c.Int( nullable: false ) );
            RenameColumn( "dbo.GroupRole", "SortOrder", "Order" );
            RenameTable( "GroupRole", "GroupTypeRole" );

            Sql( @"
    DECLARE @EntityTypeId int
    SET @EntityTypeId = (SELECT [Id] FROM EntityType WHERE [Name] = 'Rock.Model.GroupTypeRole')

    DELETE Audit
    WHERE [EntityTypeId] = @EntityTypeId

    DELETE EntityType
    WHERE [Id] = @EntityTypeId

    UPDATE EntityType SET
	    [Name] = 'Rock.Model.GroupTypeRole',
	    [AssemblyName] = 'Rock.Model.GroupTypeRole, Rock, Version=0.0.1.0, Culture=neutral, PublicKeyToken=null',
	    [FriendlyName] = 'Group Type Role',
	    [IsEntity] = 1,
	    [IsSecured] = 0
    WHERE [Name] = 'Rock.Model.GroupRole'
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn( "dbo.GroupTypeRole", "Order", c => c.Int( nullable: true ) );
            RenameColumn( "dbo.GroupTypeRole", "Order", "SortOrder" );
            RenameTable( "GroupTypeRole", "GroupRole" );

            Sql( @"
    DECLARE @EntityTypeId int
    SET @EntityTypeId = (SELECT [Id] FROM EntityType WHERE [Name] = 'Rock.Model.GroupRole')

    DELETE Audit
    WHERE [EntityTypeId] = @EntityTypeId

    DELETE EntityType
    WHERE [Id] = @EntityTypeId

    UPDATE EntityType SET
	    [Name] = 'Rock.Model.GroupRole',
	    [AssemblyName] = 'Rock.Model.GroupRole, Rock, Version=0.0.1.0, Culture=neutral, PublicKeyToken=null',
	    [FriendlyName] = 'Group Role',
	    [IsEntity] = 1,
	    [IsSecured] = 0
    WHERE [Name] = 'Rock.Model.GroupTypeRole'
" );
        }
    }
}
