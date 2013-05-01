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
    public partial class DbCalculatedPersonNameFields : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // these are db computed columns
            Sql( @"alter table dbo.Person add FirstName as IsNull(nullif(NickName, ''), GivenName)" );
            Sql( @"alter table dbo.Person add FullName as IsNull(IsNull(nullif(NickName, ''), GivenName), '') + ' ' + IsNull(LastName, '')" );
            Sql( @"alter table dbo.Person add FullNameLastFirst as IsNull(LastName, '') + ', ' + IsNull(IsNull(nullif(NickName, ''), GivenName), '')" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.Person", "FullNameLastFirst" );
            DropColumn( "dbo.Person", "FirstName" );
            DropColumn( "dbo.Person", "FullName" );
        }
    }
}
