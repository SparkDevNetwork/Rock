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
    public partial class MarketingCampaignTables : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.cmsMarketingCampaign",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 100),
                        ContactPersonId = c.Int(),
                        ContactEmail = c.String(maxLength: 254),
                        ContactPhoneNumber = c.String(maxLength: 20),
                        ContactFullName = c.String(maxLength: 152),
                        EventGroupId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.crmPerson", t => t.ContactPersonId)
                .ForeignKey("dbo.crmGroup", t => t.EventGroupId)
                .Index(t => t.ContactPersonId)
                .Index(t => t.EventGroupId);
            
            CreateIndex( "dbo.cmsMarketingCampaign", "Guid", true );
            CreateTable(
                "dbo.cmsMarketingCampaignAd",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MarketingCampaignId = c.Int(nullable: false),
                        MarketingCampaignAdTypeId = c.Int(nullable: false),
                        Priority = c.Int(nullable: false),
                        MarketingCampaignAdStatus = c.Byte(nullable: false),
                        MarketingCampaignStatusPersonId = c.Int(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        Url = c.String(maxLength: 2000),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.cmsMarketingCampaign", t => t.MarketingCampaignId, cascadeDelete: true)
                .ForeignKey("dbo.cmsMarketingCampaignAdType", t => t.MarketingCampaignAdTypeId)
                .Index(t => t.MarketingCampaignId)
                .Index(t => t.MarketingCampaignAdTypeId);
            
            CreateIndex( "dbo.cmsMarketingCampaignAd", "Guid", true );
            CreateTable(
                "dbo.cmsMarketingCampaignAdType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Name = c.String(maxLength: 100),
                        DateRangeType = c.Byte(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo.cmsMarketingCampaignAdType", "Guid", true );
            CreateTable(
                "dbo.cmsMarketingCampaignAudience",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MarketingCampaignId = c.Int(nullable: false),
                        AudienceTypeValueId = c.Int(nullable: false),
                        IsPrimary = c.Boolean(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.cmsMarketingCampaign", t => t.MarketingCampaignId, cascadeDelete: true)
                .ForeignKey("dbo.coreDefinedValue", t => t.AudienceTypeValueId, cascadeDelete: true)
                .Index(t => t.MarketingCampaignId)
                .Index(t => t.AudienceTypeValueId);
            
            CreateIndex( "dbo.cmsMarketingCampaignAudience", "Guid", true );
            CreateTable(
                "dbo.cmsMarketingCampaignCampus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MarketingCampaignId = c.Int(nullable: false),
                        CampusId = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.cmsMarketingCampaign", t => t.MarketingCampaignId, cascadeDelete: true)
                .ForeignKey("dbo.crmCampus", t => t.CampusId, cascadeDelete: true)
                .Index(t => t.MarketingCampaignId)
                .Index(t => t.CampusId);
            
            CreateIndex( "dbo.cmsMarketingCampaignCampus", "Guid", true );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.cmsMarketingCampaignCampus", new[] { "CampusId" });
            DropIndex("dbo.cmsMarketingCampaignCampus", new[] { "MarketingCampaignId" });
            DropIndex("dbo.cmsMarketingCampaignAudience", new[] { "AudienceTypeValueId" });
            DropIndex("dbo.cmsMarketingCampaignAudience", new[] { "MarketingCampaignId" });
            DropIndex("dbo.cmsMarketingCampaignAd", new[] { "MarketingCampaignAdTypeId" });
            DropIndex("dbo.cmsMarketingCampaignAd", new[] { "MarketingCampaignId" });
            DropIndex("dbo.cmsMarketingCampaign", new[] { "EventGroupId" });
            DropIndex("dbo.cmsMarketingCampaign", new[] { "ContactPersonId" });
            DropForeignKey("dbo.cmsMarketingCampaignCampus", "CampusId", "dbo.crmCampus");
            DropForeignKey("dbo.cmsMarketingCampaignCampus", "MarketingCampaignId", "dbo.cmsMarketingCampaign");
            DropForeignKey("dbo.cmsMarketingCampaignAudience", "AudienceTypeValueId", "dbo.coreDefinedValue");
            DropForeignKey("dbo.cmsMarketingCampaignAudience", "MarketingCampaignId", "dbo.cmsMarketingCampaign");
            DropForeignKey("dbo.cmsMarketingCampaignAd", "MarketingCampaignAdTypeId", "dbo.cmsMarketingCampaignAdType");
            DropForeignKey("dbo.cmsMarketingCampaignAd", "MarketingCampaignId", "dbo.cmsMarketingCampaign");
            DropForeignKey("dbo.cmsMarketingCampaign", "EventGroupId", "dbo.crmGroup");
            DropForeignKey("dbo.cmsMarketingCampaign", "ContactPersonId", "dbo.crmPerson");
            DropTable("dbo.cmsMarketingCampaignCampus");
            DropTable("dbo.cmsMarketingCampaignAudience");
            DropTable("dbo.cmsMarketingCampaignAdType");
            DropTable("dbo.cmsMarketingCampaignAd");
            DropTable("dbo.cmsMarketingCampaign");
        }
    }
}
