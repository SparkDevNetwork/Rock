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
    public partial class GroupLocationPersonId : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.GroupLocation", "GroupMemberPersonId", c => c.Int());
            CreateIndex("dbo.GroupLocation", "GroupMemberPersonId");
            AddForeignKey("dbo.GroupLocation", "GroupMemberPersonId", "dbo.Person", "Id", cascadeDelete: true);
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.GroupLocation", "GroupMemberPersonId", "dbo.Person");
            DropIndex("dbo.GroupLocation", new[] { "GroupMemberPersonId" });
            DropColumn("dbo.GroupLocation", "GroupMemberPersonId");
        }
    }
}
