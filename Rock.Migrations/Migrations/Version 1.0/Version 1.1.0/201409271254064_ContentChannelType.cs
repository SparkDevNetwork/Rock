// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
    public partial class ContentChannelType : Rock.Migrations.RockMigration
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

    UPDATE [BlockType] SET [Path] = '~/Blocks/Cms/ContentChannelTypeDetail.ascx' WHERE [Path] = '~/Blocks/Cms/ContentTypeDetail.ascx'
    UPDATE [BlockType] SET [Path] = '~/Blocks/Cms/ContentChannelTypeList.ascx' WHERE [Path] = '~/Blocks/Cms/ContentTypeList.ascx'
    UPDATE [BlockType] SET [Path] = '~/Blocks/Cms/ContentChannelItemDetail.ascx' WHERE [Path] = '~/Blocks/Cms/ContentItemDetail.ascx'
    UPDATE [BlockType] SET [Path] = '~/Blocks/Cms/ContentChannelItemList.ascx' WHERE [Path] = '~/Blocks/Cms/ContentItemList.ascx'
    UPDATE [BlockType] SET [Path] = '~/Blocks/Cms/ContentChannelItemView.ascx' WHERE [Path] = '~/Blocks/Cms/MyContentItems.ascx'

" ); 
            RenameTable( name: "dbo.ContentType", newName: "ContentChannelType" );
            RenameTable(name: "dbo.ContentItem", newName: "ContentChannelItem");
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
            RenameColumn(table: "dbo.ContentChannel", name: "ContentTypeId", newName: "ContentChannelTypeId");
            RenameColumn(table: "dbo.ContentChannelItem", name: "ContentTypeId", newName: "ContentChannelTypeId");
            RenameIndex(table: "dbo.ContentChannel", name: "IX_ContentTypeId", newName: "IX_ContentChannelTypeId");
            RenameIndex(table: "dbo.ContentChannelItem", name: "IX_ContentTypeId", newName: "IX_ContentChannelTypeId");
            AlterColumn("dbo.ContentChannelType", "DateRangeType", c => c.Int(nullable: false));
            AlterColumn("dbo.ContentChannelItem", "Status", c => c.Int(nullable: false));
            DropTable("dbo.MarketingCampaignAd");
            DropTable("dbo.MarketingCampaign");
            DropTable("dbo.MarketingCampaignAudience");
            DropTable("dbo.MarketingCampaignCampus");
            DropTable("dbo.MarketingCampaignAdType");

            RockMigrationHelper.UpdateFieldType("Content Channel","","Rock","Rock.Field.Types.ContentChannelFieldType","D835A0EC-C8DB-483A-A37C-E8FB6E956C3D");

            RockMigrationHelper.AddPage("F7105BFE-B28C-41B6-9CE6-F1018D77DD8F","D65F783D-87A9-4CC9-8110-E83466A0EADB","Content","","117B547B-9D71-4EE9-8047-176676F5DC8C",""); // Site:Rock RMS
            RockMigrationHelper.AddPage("117B547B-9D71-4EE9-8047-176676F5DC8C","D65F783D-87A9-4CC9-8110-E83466A0EADB","Content Detail","","D18E837C-9E65-4A38-8647-DFF04A595D97",""); // Site:Rock RMS
            RockMigrationHelper.AddPage("EBAA5140-4B8F-44B8-B1E8-C73B654E4B22","5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD","Item Detail","","56F1DC05-3D7D-49B6-9A30-5CF271C687F4",""); // Site:External Website

            RockMigrationHelper.UpdateBlockType("Content Channel Item View","Block to display the content channels/items that user is authorized to view.","~/Blocks/Cms/ContentChannelItemView.ascx","CMS","0E023AE3-BF08-48E0-93F8-08C32EB5CAFA");
            RockMigrationHelper.UpdateBlockType("Content Channel Dynamic","Block to display dynamic content channel items.","~/Blocks/Cms/ContentChannelDynamic.ascx","CMS","143A2345-3E26-4ED0-A2FE-42AAF11B4C0F");

            // Add Block to Page: External Homepage, Site: External Website
            RockMigrationHelper.AddBlock("85F25819-E948-4960-9DDF-00F54D32444E","","143A2345-3E26-4ED0-A2FE-42AAF11B4C0F","Content","Feature","","",0,"095027CB-9114-4CD5-ABE8-1E8882422DCF"); 
            // Add Block to Page: Content, Site: Rock RMS
            RockMigrationHelper.AddBlock("117B547B-9D71-4EE9-8047-176676F5DC8C","","0E023AE3-BF08-48E0-93F8-08C32EB5CAFA","My Content Items","Main","","",0,"F79A7562-0277-4386-B511-2D4FFE2198E2"); 
            // Add Block to Page: Content Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock("D18E837C-9E65-4A38-8647-DFF04A595D97","","5B99687B-5FE9-4EE2-8679-5040CAEB9E2E","Content Item Detail","Main","","",0,"AA00DFCB-5C02-418A-A2DC-C0DEA910DA52"); 

            // Attrib for BlockType: Content:Count
            RockMigrationHelper.AddBlockTypeAttribute("143A2345-3E26-4ED0-A2FE-42AAF11B4C0F","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Count","Count","","The maximum number of items to display.",5,@"5","25A501FC-E269-40B8-9904-E20FA7A1ADB6");
            // Attrib for BlockType: Content:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute("143A2345-3E26-4ED0-A2FE-42AAF11B4C0F","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Detail Page","DetailPage","","The page to navigate to for details.",7,@"","2D7E6F55-B25E-4EA2-8F7E-F7E138E39E21");
            // Attrib for BlockType: Content:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute("143A2345-3E26-4ED0-A2FE-42AAF11B4C0F","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Debug","EnableDebug","","Enabling debug will display the fields of the first 5 items to help show you wants available for your liquid.",9,@"False","72F4232B-8D2A-4823-B9F1-ED68F182C1A4");
            // Attrib for BlockType: Content:Cache Duration
            RockMigrationHelper.AddBlockTypeAttribute("143A2345-3E26-4ED0-A2FE-42AAF11B4C0F","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Cache Duration","CacheDuration","","Number of seconds to cache the content.",6,@"3600","773BEFDD-EEBA-486C-98E6-AFD0D4156E22");
            // Attrib for BlockType: Content:Channel
            RockMigrationHelper.AddBlockTypeAttribute("143A2345-3E26-4ED0-A2FE-42AAF11B4C0F","D835A0EC-C8DB-483A-A37C-E8FB6E956C3D","Channel","Channel","","The channel to display items from.",0,@"","34EACB0F-DBC4-4F18-85C9-0D3EDFDF46BE");
            // Attrib for BlockType: Content:Status
            RockMigrationHelper.AddBlockTypeAttribute("143A2345-3E26-4ED0-A2FE-42AAF11B4C0F","BD0D9B57-2A41-4490-89FF-F01DAB7D4904","Status","Status","","Include items with the following status.",1,@"2","DA1DEE5D-BCEF-4AA4-A9D9-EFD4DD64462B");
            // Attrib for BlockType: Content:Order
            RockMigrationHelper.AddBlockTypeAttribute("143A2345-3E26-4ED0-A2FE-42AAF11B4C0F","9C204CD0-1233-41C5-818A-C5DA439445AA","Order","Order","","The specifics of how items should be ordered. This value is set through configuration and should not be modified here.",3,@"Priority^ASC|Expire^DESC|Start^DESC","07ED420E-749C-4938-ADFD-1DDEA6B63014");
            // Attrib for BlockType: Content:Template
            RockMigrationHelper.AddBlockTypeAttribute("143A2345-3E26-4ED0-A2FE-42AAF11B4C0F","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Template","Template","","The template to use when formatting the list of items.",8,@"
","8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8");
            // Attrib for BlockType: Content:Filters
            RockMigrationHelper.AddBlockTypeAttribute("143A2345-3E26-4ED0-A2FE-42AAF11B4C0F","C28C7BF3-A552-4D77-9408-DEDCF760CED0","Filters","Filters","","The filters to use when quering items.",2,@"","0FC5F418-FF53-4881-BB00-B67D23C5B4EC");

            // Attrib for BlockType: My Content Items:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute("0E023AE3-BF08-48E0-93F8-08C32EB5CAFA","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Detail Page","DetailPage","","Page used to view a content item.",0,@"","EB4EE5EE-BD15-4C98-87CD-EC3BC1429C43");
            
            // Attrib Value for Block:Content, Attribute:Count Page: External Homepage, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("095027CB-9114-4CD5-ABE8-1E8882422DCF","25A501FC-E269-40B8-9904-E20FA7A1ADB6",@"2");
            // Attrib Value for Block:Content, Attribute:Detail Page Page: External Homepage, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("095027CB-9114-4CD5-ABE8-1E8882422DCF","2D7E6F55-B25E-4EA2-8F7E-F7E138E39E21",@"56f1dc05-3d7d-49b6-9a30-5cf271c687f4");
            // Attrib Value for Block:Content, Attribute:Enable Debug Page: External Homepage, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("095027CB-9114-4CD5-ABE8-1E8882422DCF","72F4232B-8D2A-4823-B9F1-ED68F182C1A4",@"False");
            // Attrib Value for Block:Content, Attribute:Cache Duration Page: External Homepage, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("095027CB-9114-4CD5-ABE8-1E8882422DCF","773BEFDD-EEBA-486C-98E6-AFD0D4156E22",@"0");
            // Attrib Value for Block:Content, Attribute:Channel Page: External Homepage, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("095027CB-9114-4CD5-ABE8-1E8882422DCF","34EACB0F-DBC4-4F18-85C9-0D3EDFDF46BE",@"42e3e2cb-d689-473a-b2d6-f2c3439041fb");
            // Attrib Value for Block:Content, Attribute:Status Page: External Homepage, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("095027CB-9114-4CD5-ABE8-1E8882422DCF","DA1DEE5D-BCEF-4AA4-A9D9-EFD4DD64462B",@"2");
            // Attrib Value for Block:Content, Attribute:Order Page: External Homepage, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("095027CB-9114-4CD5-ABE8-1E8882422DCF","07ED420E-749C-4938-ADFD-1DDEA6B63014",@"Priority^ASC|Expire^DESC|Start^DESC");
            // Attrib Value for Block:Content, Attribute:Template Page: External Homepage, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("095027CB-9114-4CD5-ABE8-1E8882422DCF","8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8",@"<h3>Items</h3>
{% for item in Items %}
    <a href=""{{ PageLinks.DetailPage }}?Item={{ item.Id }}"">{{ item.Title }}</a><br/>
    {{ item.Content }}<br/><br/>
{% endfor %}

{% if Pagination.PreviousPage > 0 %}
    <a href=""/page/1?Page={{ Pagination.PreviousPage }}"">&lt; Prev</a>
{% endif %}

{% if Pagination.NextPage > 0  %}
    <a href=""/page/1?Page={{ Pagination.NextPage }}"">Next &gt;</a>
{% endif %}

");
            // Attrib Value for Block:Content, Attribute:Filters Page: External Homepage, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("095027CB-9114-4CD5-ABE8-1E8882422DCF","0FC5F418-FF53-4881-BB00-B67D23C5B4EC",@"");
            // Attrib Value for Block:My Content Items, Attribute:Detail Page Page: Content, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("F79A7562-0277-4386-B511-2D4FFE2198E2","EB4EE5EE-BD15-4C98-87CD-EC3BC1429C43",@"d18e837c-9e65-4a38-8647-dff04a595d97");

            // Add/Update PageContext for Page:Workflow Configuration, Entity: Rock.Model.WorkflowType, Parameter: WorkflowTypeId
            RockMigrationHelper.UpdatePageContext( "DCB18A76-6DFF-48A5-A66E-2CAA10D2CA1A", "Rock.Model.WorkflowType", "WorkflowTypeId", "E904932A-4551-4A5A-B6BF-EF60AD8E90E6");

        
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
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
            
            AlterColumn("dbo.ContentChannelItem", "Status", c => c.Byte(nullable: false));
            AlterColumn("dbo.ContentChannelType", "DateRangeType", c => c.Byte(nullable: false));
            RenameIndex(table: "dbo.ContentChannelItem", name: "IX_ContentChannelTypeId", newName: "IX_ContentTypeId");
            RenameIndex(table: "dbo.ContentChannel", name: "IX_ContentChannelTypeId", newName: "IX_ContentTypeId");
            RenameColumn(table: "dbo.ContentChannelItem", name: "ContentChannelTypeId", newName: "ContentTypeId");
            RenameColumn(table: "dbo.ContentChannel", name: "ContentChannelTypeId", newName: "ContentTypeId");
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
            RenameTable(name: "dbo.ContentChannelItem", newName: "ContentItem");
            RenameTable(name: "dbo.ContentChannelType", newName: "ContentType");
        }
    }
}
