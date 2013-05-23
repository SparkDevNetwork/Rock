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
    public partial class AddIndexesForPersonProfile : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateIndex( "Person", new string[] { "IsDeceased", "LastName", "FirstName" }, false, "LastFirstName" );
            CreateIndex( "Person", new string[] { "IsDeceased", "FirstName", "LastName" }, false, "FirstLastName" );
            CreateIndex( "AttributeValue", new string[] { "EntityId", "AttributeId" }, false, "EntityAttribute" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex( "AttributeValue", "EntityAttribute" );
            DropIndex( "Person", "FirstLastName" );
            DropIndex( "Person", "LastFirstName" );
        }
    }
}
