namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class Metric3 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.coreMetric", "CollectionFrequencyId", "dbo.coreDefinedValue");
            DropIndex("dbo.coreMetric", new[] { "CollectionFrequencyId" });
            AlterColumn("dbo.coreMetric", "CollectionFrequencyId", c => c.Int());
            AddForeignKey( "dbo.coreMetric", "CollectionFrequencyId", "dbo.coreDefinedValue", "Id");
            CreateIndex("dbo.coreMetric", "CollectionFrequencyId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.coreMetric", new[] { "CollectionFrequencyId" });
            DropForeignKey("dbo.coreMetric", "CollectionFrequencyId", "dbo.coreDefinedValue");
            AlterColumn("dbo.coreMetric", "CollectionFrequencyId", c => c.Int(nullable: false));
            CreateIndex("dbo.coreMetric", "CollectionFrequencyId");
            AddForeignKey( "dbo.coreMetric", "CollectionFrequencyId", "dbo.coreDefinedValue", "Id", cascadeDelete: true);
        }
    }
}
