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
    public partial class AddPersonToNote : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Note", "CreatedByPersonId", c => c.Int());
            CreateIndex("dbo.Note", "CreatedByPersonId");
            AddForeignKey("dbo.Note", "CreatedByPersonId", "dbo.Person", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Note", "CreatedByPersonId", "dbo.Person");
            DropIndex("dbo.Note", new[] { "CreatedByPersonId" });
            DropColumn("dbo.Note", "CreatedByPersonId");
        }
    }
}
