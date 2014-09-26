// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class RemoveAds : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DELETE [Attribute]
    WHERE [EntityTypeQualifierColumn] = 'BlockTypeId'
    AND [EntityTypeQualifierValue] IN (
	    SELECT CAST ( [Id] AS varchar) 
	    FROM [BlockType] 
	    WHERE [Path] LIKE '%MarketingCampaign%' 
	    OR [Path] LIKE '%AdList%'
	    OR [Path] LIKE '%AdDetail%'
    )

    DELETE [BlockType] 
    WHERE [Path] LIKE '%MarketingCampaign%' 
    OR [Path] LIKE '%AdList%'
    OR [Path] LIKE '%AdDetail%'

    DELETE [PageView]
    WHERE [PageId] IN (
	    SELECT [Id] 
	    FROM [Page]
	    WHERE [Guid] IN (
		    '74345663-5BCA-493C-A2FB-80DC9CC8E70C',
		    'E6F5F06B-65EE-4949-AA56-1FE4E2933C63',
		    '36826974-C613-48F2-877E-460C4EC90CCE',
		    '64521E8E-3BA7-409C-A18F-4ACAAC6758CE',
		    '738006C2-692C-4402-AD32-4F729A98F8BF',
		    '5EB07686-D032-41A5-95C0-FD36F939FA52',
		    '78D470E9-221B-4EBD-9FF6-995B45FB9CD5',
		    '5B4A4DF6-17BB-4C99-B5B7-6DC9C896BC8E'
	    )
    )

    DELETE [Page]
    WHERE [Guid] IN (
	    '74345663-5BCA-493C-A2FB-80DC9CC8E70C',
	    'E6F5F06B-65EE-4949-AA56-1FE4E2933C63',
	    '36826974-C613-48F2-877E-460C4EC90CCE',
	    '64521E8E-3BA7-409C-A18F-4ACAAC6758CE',
	    '738006C2-692C-4402-AD32-4F729A98F8BF',
	    '5EB07686-D032-41A5-95C0-FD36F939FA52',
	    '78D470E9-221B-4EBD-9FF6-995B45FB9CD5',
	    '5B4A4DF6-17BB-4C99-B5B7-6DC9C896BC8E'
    )

" );
            DropForeignKey("dbo.MarketingCampaignAd", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MarketingCampaign", "ContactPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MarketingCampaign", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MarketingCampaign", "EventGroupId", "dbo.Group");
            DropForeignKey("dbo.MarketingCampaignAudience", "AudienceTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.MarketingCampaignAudience", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MarketingCampaignAudience", "MarketingCampaignId", "dbo.MarketingCampaign");
            DropForeignKey("dbo.MarketingCampaignAudience", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MarketingCampaignCampus", "CampusId", "dbo.Campus");
            DropForeignKey("dbo.MarketingCampaignCampus", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MarketingCampaignCampus", "MarketingCampaignId", "dbo.MarketingCampaign");
            DropForeignKey("dbo.MarketingCampaignCampus", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MarketingCampaign", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MarketingCampaignAd", "MarketingCampaignId", "dbo.MarketingCampaign");
            DropForeignKey("dbo.MarketingCampaignAdType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MarketingCampaignAdType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MarketingCampaignAd", "MarketingCampaignAdTypeId", "dbo.MarketingCampaignAdType");
            DropForeignKey("dbo.MarketingCampaignAd", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.MarketingCampaignAd", new[] { "MarketingCampaignId" });
            DropIndex("dbo.MarketingCampaignAd", new[] { "MarketingCampaignAdTypeId" });
            DropIndex("dbo.MarketingCampaignAd", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.MarketingCampaignAd", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.MarketingCampaignAd", new[] { "Guid" });
            DropIndex("dbo.MarketingCampaign", new[] { "ContactPersonAliasId" });
            DropIndex("dbo.MarketingCampaign", new[] { "EventGroupId" });
            DropIndex("dbo.MarketingCampaign", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.MarketingCampaign", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.MarketingCampaign", new[] { "Guid" });
            DropIndex("dbo.MarketingCampaignAudience", new[] { "MarketingCampaignId" });
            DropIndex("dbo.MarketingCampaignAudience", new[] { "AudienceTypeValueId" });
            DropIndex("dbo.MarketingCampaignAudience", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.MarketingCampaignAudience", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.MarketingCampaignAudience", new[] { "Guid" });
            DropIndex("dbo.MarketingCampaignCampus", new[] { "MarketingCampaignId" });
            DropIndex("dbo.MarketingCampaignCampus", new[] { "CampusId" });
            DropIndex("dbo.MarketingCampaignCampus", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.MarketingCampaignCampus", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.MarketingCampaignCampus", new[] { "Guid" });
            DropIndex("dbo.MarketingCampaignAdType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.MarketingCampaignAdType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.MarketingCampaignAdType", new[] { "Guid" });
            DropTable("dbo.MarketingCampaignAd");
            DropTable("dbo.MarketingCampaign");
            DropTable("dbo.MarketingCampaignAudience");
            DropTable("dbo.MarketingCampaignCampus");
            DropTable("dbo.MarketingCampaignAdType");

            AlterColumn("dbo.ContentType", "DateRangeType", c => c.Int(nullable: false));
            AlterColumn("dbo.ContentItem", "Status", c => c.Int(nullable: false));
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn( "dbo.ContentItem", "Status", c => c.Byte( nullable: false ) );
            AlterColumn( "dbo.ContentType", "DateRangeType", c => c.Byte( nullable: false ) );

            CreateTable(
                "dbo.MarketingCampaignAdType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        DateRangeType = c.Byte(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MarketingCampaignCampus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MarketingCampaignId = c.Int(nullable: false),
                        CampusId = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MarketingCampaignAudience",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MarketingCampaignId = c.Int(nullable: false),
                        AudienceTypeValueId = c.Int(nullable: false),
                        IsPrimary = c.Boolean(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MarketingCampaign",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 100),
                        ContactPersonAliasId = c.Int(),
                        ContactEmail = c.String(maxLength: 254),
                        ContactPhoneNumber = c.String(maxLength: 20),
                        ContactFullName = c.String(maxLength: 152),
                        EventGroupId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MarketingCampaignAd",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MarketingCampaignId = c.Int(nullable: false),
                        MarketingCampaignAdTypeId = c.Int(nullable: false),
                        Priority = c.Int(nullable: false),
                        MarketingCampaignAdStatus = c.Byte(nullable: false),
                        MarketingCampaignStatusPersonId = c.Int(),
                        StartDate = c.DateTime(nullable: false, storeType: "date"),
                        EndDate = c.DateTime(nullable: false, storeType: "date"),
                        Url = c.String(maxLength: 2000),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.MarketingCampaignAdType", "Guid", unique: true);
            CreateIndex("dbo.MarketingCampaignAdType", "ModifiedByPersonAliasId");
            CreateIndex("dbo.MarketingCampaignAdType", "CreatedByPersonAliasId");
            CreateIndex("dbo.MarketingCampaignCampus", "Guid", unique: true);
            CreateIndex("dbo.MarketingCampaignCampus", "ModifiedByPersonAliasId");
            CreateIndex("dbo.MarketingCampaignCampus", "CreatedByPersonAliasId");
            CreateIndex("dbo.MarketingCampaignCampus", "CampusId");
            CreateIndex("dbo.MarketingCampaignCampus", "MarketingCampaignId");
            CreateIndex("dbo.MarketingCampaignAudience", "Guid", unique: true);
            CreateIndex("dbo.MarketingCampaignAudience", "ModifiedByPersonAliasId");
            CreateIndex("dbo.MarketingCampaignAudience", "CreatedByPersonAliasId");
            CreateIndex("dbo.MarketingCampaignAudience", "AudienceTypeValueId");
            CreateIndex("dbo.MarketingCampaignAudience", "MarketingCampaignId");
            CreateIndex("dbo.MarketingCampaign", "Guid", unique: true);
            CreateIndex("dbo.MarketingCampaign", "ModifiedByPersonAliasId");
            CreateIndex("dbo.MarketingCampaign", "CreatedByPersonAliasId");
            CreateIndex("dbo.MarketingCampaign", "EventGroupId");
            CreateIndex("dbo.MarketingCampaign", "ContactPersonAliasId");
            CreateIndex("dbo.MarketingCampaignAd", "Guid", unique: true);
            CreateIndex("dbo.MarketingCampaignAd", "ModifiedByPersonAliasId");
            CreateIndex("dbo.MarketingCampaignAd", "CreatedByPersonAliasId");
            CreateIndex("dbo.MarketingCampaignAd", "MarketingCampaignAdTypeId");
            CreateIndex("dbo.MarketingCampaignAd", "MarketingCampaignId");
            AddForeignKey("dbo.MarketingCampaignAd", "ModifiedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.MarketingCampaignAd", "MarketingCampaignAdTypeId", "dbo.MarketingCampaignAdType", "Id");
            AddForeignKey("dbo.MarketingCampaignAdType", "ModifiedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.MarketingCampaignAdType", "CreatedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.MarketingCampaignAd", "MarketingCampaignId", "dbo.MarketingCampaign", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MarketingCampaign", "ModifiedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.MarketingCampaignCampus", "ModifiedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.MarketingCampaignCampus", "MarketingCampaignId", "dbo.MarketingCampaign", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MarketingCampaignCampus", "CreatedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.MarketingCampaignCampus", "CampusId", "dbo.Campus", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MarketingCampaignAudience", "ModifiedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.MarketingCampaignAudience", "MarketingCampaignId", "dbo.MarketingCampaign", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MarketingCampaignAudience", "CreatedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.MarketingCampaignAudience", "AudienceTypeValueId", "dbo.DefinedValue", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MarketingCampaign", "EventGroupId", "dbo.Group", "Id");
            AddForeignKey("dbo.MarketingCampaign", "CreatedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.MarketingCampaign", "ContactPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.MarketingCampaignAd", "CreatedByPersonAliasId", "dbo.PersonAlias", "Id");
        }
    }
}
