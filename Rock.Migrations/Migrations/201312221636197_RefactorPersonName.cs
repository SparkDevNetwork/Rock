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
    public partial class RefactorPersonName : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    UPDATE [Person] SET [NickName] = [GivenName] WHERE [NickName] IS NULL OR [NickName] = ''
" );
            DropIndex( "dbo.Person", "FirstLastName" );
            DropIndex( "dbo.Person", "LastFirstName" );

            DropColumn( "dbo.Person", "FirstName" );
            DropColumn( "dbo.Person", "FullName" );
            DropColumn( "dbo.Person", "FullNameLastFirst" );

            RenameColumn( "dbo.Person", "GivenName", "FirstName" );

            CreateIndex( "dbo.Person", new[] { "IsDeceased", "FirstName", "LastName" }, false, "FirstLastName" );
            CreateIndex( "dbo.Person", new[] { "IsDeceased", "LastName", "FirstName" }, false, "LastFirstName" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex( "dbo.Person", "FirstLastName" );
            DropIndex( "dbo.Person", "LastFirstName" );

            RenameColumn( "dbo.Person", "FirstName", "GivenName" );

            Sql( @" 
    ALTER TABLE [dbo].[Person] ADD
	    [FirstName] AS (isnull(nullif([NickName],''),[GivenName])),
	    [FullName]  AS ((isnull(isnull(nullif([NickName],''),[GivenName]),'')+' ')+isnull([LastName],'')),
	    [FullNameLastFirst] AS ((isnull([LastName],'')+', ')+isnull(isnull(nullif([NickName],''),[GivenName]),''))

    UPDATE [Person] SET [NickName] = NULL WHERE [NickName] IS NOT NULL AND [NickName] = [GivenName]
" );
            CreateIndex( "dbo.Person", new[] { "IsDeceased", "FirstName", "LastName" }, false, "FirstLastName" );
            CreateIndex( "dbo.Person", new[] { "IsDeceased", "LastName", "FirstName" }, false, "LastFirstName" );
        }
    }
}
