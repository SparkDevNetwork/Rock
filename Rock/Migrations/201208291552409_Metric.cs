namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Metric : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "coreMetric",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Type = c.Boolean(nullable: false),
                        Category = c.String(maxLength: 100),
                        Title = c.String(nullable: false, maxLength: 100),
                        Subtitle = c.String(maxLength: 100),
                        Description = c.String(),
                        MinValue = c.Int(nullable: false),
                        MaxValue = c.Int(nullable: false),
                        CollectionFrequencyId = c.Int(nullable: false),
                        LastCollected = c.DateTime(),
                        Source = c.String(maxLength: 100),
                        SourceSQL = c.String(),
                        Order = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("coreDefinedValue", t => t.CollectionFrequencyId, cascadeDelete: true)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.CollectionFrequencyId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "coreMetricValue",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        MetricId = c.Int(nullable: false),
                        Value = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        xValue = c.Int(nullable: false),
                        isDateBased = c.Boolean(nullable: false),
                        Label = c.String(),
                        Order = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("coreMetric", t => t.MetricId)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.MetricId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
        }
        
        public override void Down()
        {
            DropIndex("coreMetricValue", new[] { "ModifiedByPersonId" });
            DropIndex("coreMetricValue", new[] { "CreatedByPersonId" });
            DropIndex("coreMetricValue", new[] { "MetricId" });
            DropIndex("coreMetric", new[] { "ModifiedByPersonId" });
            DropIndex("coreMetric", new[] { "CreatedByPersonId" });
            DropIndex("coreMetric", new[] { "CollectionFrequencyId" });
            DropForeignKey("coreMetricValue", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("coreMetricValue", "CreatedByPersonId", "crmPerson");
            DropForeignKey("coreMetricValue", "MetricId", "coreMetric");
            DropForeignKey("coreMetric", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("coreMetric", "CreatedByPersonId", "crmPerson");
            DropForeignKey("coreMetric", "CollectionFrequencyId", "coreDefinedValue");
            DropTable("coreMetricValue");
            DropTable("coreMetric");
        }
    }
}
