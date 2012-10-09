namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class GroupLocationType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.groupGroupLocation", "LocationTypeId", c => c.Int());
            AlterColumn("dbo.crmLocation", "Latitude", c => c.Double());
            AlterColumn("dbo.crmLocation", "Longitude", c => c.Double());
            AddForeignKey("dbo.groupGroupLocation", "LocationTypeId", "dbo.coreDefinedValue", "Id");
            CreateIndex("dbo.groupGroupLocation", "LocationTypeId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.groupGroupLocation", new[] { "LocationTypeId" });
            DropForeignKey("dbo.groupGroupLocation", "LocationTypeId", "dbo.coreDefinedValue");
            AlterColumn("dbo.crmLocation", "Longitude", c => c.Double(nullable: false));
            AlterColumn("dbo.crmLocation", "Latitude", c => c.Double(nullable: false));
            AlterColumn("dbo.groupGroupLocation", "LocationTypeId", c => c.Int(nullable: false));
        }
    }
}
