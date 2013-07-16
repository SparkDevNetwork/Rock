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
    public partial class InheritedGroupType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.GroupType", "InheritedGroupTypeId", c => c.Int());
            AddForeignKey("dbo.GroupType", "InheritedGroupTypeId", "dbo.GroupType", "Id");
            CreateIndex("dbo.GroupType", "InheritedGroupTypeId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.GroupType", new[] { "InheritedGroupTypeId" });
            DropForeignKey("dbo.GroupType", "InheritedGroupTypeId", "dbo.GroupType");
            DropColumn("dbo.GroupType", "InheritedGroupTypeId");
        }
    }
}
