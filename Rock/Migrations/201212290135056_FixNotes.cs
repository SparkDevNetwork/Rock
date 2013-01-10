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
    public partial class FixNotes : RockMigration_2
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.Note", "NoteType_Id", "dbo.NoteType");
            DropIndex("dbo.Note", new[] { "NoteType_Id" });
            DropColumn("dbo.Note", "NoteType_Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Note", "NoteType_Id", c => c.Int());
            CreateIndex("dbo.Note", "NoteType_Id");
            AddForeignKey("dbo.Note", "NoteType_Id", "dbo.NoteType", "Id");
        }
    }
}
