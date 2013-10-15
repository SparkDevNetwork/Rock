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
    public partial class GivingGroup : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Person", "GivingGroupId", c => c.Int());
            CreateIndex("dbo.Person", "GivingGroupId");
            AddForeignKey("dbo.Person", "GivingGroupId", "dbo.Group", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Person", "GivingGroupId", "dbo.Group");
            DropIndex("dbo.Person", new[] { "GivingGroupId" });
            DropColumn("dbo.Person", "GivingGroupId");
        }
    }
}
