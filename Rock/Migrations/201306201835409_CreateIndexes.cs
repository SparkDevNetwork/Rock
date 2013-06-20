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
    public partial class CreateIndexes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateIndex( "CommunicationRecipient", new string[] { "CommunicationId", "PersonId" } );
            CreateIndex( "Note", new string[] { "NoteTypeId", "EntityId" } );
            CreateIndex( "TaggedItem", new string[] { "TagId", "EntityGuid" } );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex( "TaggedItem", new string[] { "TagId", "EntityGuid" } );
            DropIndex( "Note", new string[] { "NoteTypeId", "EntityId" } );
            DropIndex( "CommunicationRecipient", new string[] { "CommunicationId", "PersonId" } );
        }
    }
}
