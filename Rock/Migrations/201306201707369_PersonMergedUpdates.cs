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
    public partial class PersonMergedUpdates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey( "dbo.PersonAccount", "PersonId", "dbo.Person" );
            DropIndex( "dbo.PersonAccount", new[] { "PersonId" } );
            DropTable( "dbo.PersonAccount" );

            AddColumn( "dbo.PersonMerged", "PreviousPersonId", c => c.Int( nullable: false ) );
            CreateIndex( "dbo.PersonMerged", "PreviousPersonId", true );
            AddColumn( "dbo.PersonMerged", "PreviousPersonGuid", c => c.Guid( nullable: false ) );
            AddColumn( "dbo.PersonMerged", "NewPersonId", c => c.Int( nullable: false ) );
            AddColumn( "dbo.PersonMerged", "NewPersonGuid", c => c.Guid( nullable: false ) );
            DropColumn( "dbo.PersonMerged", "CurrentId" );
            DropColumn( "dbo.PersonMerged", "CurrentGuid" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CreateTable(
                "dbo.PersonAccount",
                c => new
                    {
                        Id = c.Int( nullable: false, identity: true ),
                        PersonId = c.Int(),
                        Account = c.String( maxLength: 50 ),
                        Guid = c.Guid( nullable: false ),
                    } )
                .PrimaryKey( t => t.Id );
            CreateIndex( "dbo.PersonAccount", "PersonId" );
            AddForeignKey( "dbo.PersonAccount", "PersonId", "dbo.Person", "Id" );

            AddColumn( "dbo.PersonMerged", "CurrentGuid", c => c.Guid( nullable: false ) );
            AddColumn( "dbo.PersonMerged", "CurrentId", c => c.Int( nullable: false ) );
            DropColumn( "dbo.PersonMerged", "NewPersonGuid" );
            DropColumn( "dbo.PersonMerged", "NewPersonId" );
            DropColumn( "dbo.PersonMerged", "PreviousPersonGuid" );
            DropIndex( "dbo.PersonMerged", new string[] { "PreviousPersonId" } );
            DropColumn( "dbo.PersonMerged", "PreviousPersonId" );
        }
    }
}
