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
    public partial class AddGroupFields : RockMigration_2
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameTable(name: "dbo.groupsGroupTypeAssociation", newName: "crmGroupTypeAssociation");
            CreateTable(
                "dbo.crmGroupTypeLocationType",
                c => new
                    {
                        GroupTypeId = c.Int(nullable: false),
                        LocationTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.GroupTypeId, t.LocationTypeId })
                .ForeignKey("dbo.crmGroupType", t => t.GroupTypeId, cascadeDelete: true)
                .ForeignKey("dbo.coreDefinedValue", t => t.LocationTypeId, cascadeDelete: true)
                .Index(t => t.GroupTypeId)
                .Index(t => t.LocationTypeId);
            
            AddColumn("dbo.crmGroup", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.crmGroupRole", "MaxCount", c => c.Int());
            AddColumn("dbo.crmGroupRole", "MinCount", c => c.Int());
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.crmGroupTypeLocationType", new[] { "LocationTypeId" });
            DropIndex("dbo.crmGroupTypeLocationType", new[] { "GroupTypeId" });
            DropForeignKey("dbo.crmGroupTypeLocationType", "LocationTypeId", "dbo.coreDefinedValue");
            DropForeignKey("dbo.crmGroupTypeLocationType", "GroupTypeId", "dbo.crmGroupType");
            DropColumn("dbo.crmGroupRole", "MinCount");
            DropColumn("dbo.crmGroupRole", "MaxCount");
            DropColumn("dbo.crmGroup", "IsActive");
            DropTable("dbo.crmGroupTypeLocationType");
            RenameTable(name: "dbo.crmGroupTypeAssociation", newName: "groupsGroupTypeAssociation");
        }
    }
}
