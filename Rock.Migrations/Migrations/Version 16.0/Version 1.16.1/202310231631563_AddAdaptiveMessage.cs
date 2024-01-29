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
    public partial class AddAdaptiveMessage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.AdaptiveMessageAdaptation",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 100 ),
                    Description = c.String(),
                    IsActive = c.Boolean( nullable: false ),
                    Order = c.Int( nullable: false ),
                    ViewSaturationCount = c.Int(),
                    ViewSaturationInDays = c.Int(),
                    AdaptiveMessageId = c.Int( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.AdaptiveMessage", t => t.AdaptiveMessageId, cascadeDelete: true )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.AdaptiveMessageId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.AdaptiveMessage",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 100 ),
                    Description = c.String(),
                    Key = c.String( maxLength: 200 ),
                    IsActive = c.Boolean( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.AdaptiveMessageAdaptationSegment",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    PersonalizationSegmentId = c.Int( nullable: false ),
                    AdaptiveMessageAdaptationId = c.Int( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.AdaptiveMessageAdaptation", t => t.AdaptiveMessageAdaptationId, cascadeDelete: true )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.PersonalizationSegment", t => t.PersonalizationSegmentId )
                .Index( t => t.PersonalizationSegmentId )
                .Index( t => t.AdaptiveMessageAdaptationId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.AdaptiveMessageCategory",
                c => new
                {
                    AdaptiveMessageId = c.Int( nullable: false ),
                    CategoryId = c.Int( nullable: false ),
                } )
                .PrimaryKey( t => new { t.AdaptiveMessageId, t.CategoryId } )
                .ForeignKey( "dbo.AdaptiveMessage", t => t.AdaptiveMessageId, cascadeDelete: true )
                .ForeignKey( "dbo.Category", t => t.CategoryId, cascadeDelete: true )
                .Index( t => t.AdaptiveMessageId )
                .Index( t => t.CategoryId );

            // Entity: Rock.Model.AdaptiveMessageAdaptation Attribute: Call To Action
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.AdaptiveMessageAdaptation", "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Call To Action", "Call To Action", @"", 2015, @"", "9E67B39B-C95C-464B-AC7B-CB191834EF85", "CallToAction" );
            // Entity: Rock.Model.AdaptiveMessageAdaptation Attribute: Summary Image
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.AdaptiveMessageAdaptation", "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D", "", "", "Summary Image", "Summary Image", @"", 2016, @"", "A927602E-DC72-4CE5-B9B6-D6F77086AB6D", "SummaryImage" );
            // Entity: Rock.Model.AdaptiveMessageAdaptation Attribute: Detail Image
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.AdaptiveMessageAdaptation", "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D", "", "", "Detail Image", "Detail Image", @"", 2017, @"", "2144A2A2-6381-426F-9B5A-BCAFFF96A5D3", "DetailImage" );
            // Entity: Rock.Model.AdaptiveMessageAdaptation Attribute: Summary
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.AdaptiveMessageAdaptation", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "", "", "Summary", "Summary", @"", 2018, @"", "611C3F9D-67BA-496E-9533-2D07B0AD9733", "Summary" );
            // Entity: Rock.Model.AdaptiveMessageAdaptation Attribute: Details
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.AdaptiveMessageAdaptation", "92C88D02-CE12-4217-80FB-19422B758437", "", "", "Details", "Details", @"", 2019, @"{}", "B55E8E55-1021-4A9E-8D05-EA9F52AA5C98", "Details" );
            // Entity: Rock.Model.AdaptiveMessageAdaptation Attribute: Mobile App Summary XAML
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.AdaptiveMessageAdaptation", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "", "", "Mobile App Summary XAML", "Mobile App Summary XAML", @"", 2020, @"", "8562A1F2-2942-4721-9585-18FC3E385F5B", "MobileApp" );
            Sql( @"UPDATE
	                [Attribute]
                   SET [IsSystem]=0
                   WHERE [Guid] IN ('9E67B39B-C95C-464B-AC7B-CB191834EF85','A927602E-DC72-4CE5-B9B6-D6F77086AB6D','2144A2A2-6381-426F-9B5A-BCAFFF96A5D3','611C3F9D-67BA-496E-9533-2D07B0AD9733','B55E8E55-1021-4A9E-8D05-EA9F52AA5C98','8562A1F2-2942-4721-9585-18FC3E385F5B')" );
           
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.AdaptiveMessageDetail", "Adaptive Message Detail", "Rock.Blocks.Cms.AdaptiveMessageDetail, Rock.Blocks, Version=1.17.0.3, Culture=neutral, PublicKeyToken=null", false, false, "D88CE6CF-C175-4C8F-BFF1-D90C590ABB3E" );
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.AdaptiveMessageList", "Adaptive Message List", "Rock.Blocks.Cms.AdaptiveMessageList, Rock.Blocks, Version=1.17.0.3, Culture=neutral, PublicKeyToken=null", false, false, "9C7E8E9D-2AF4-40E7-A4F9-307E114DB918" );
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.AdaptiveMessageAdaptationDetail", "Adaptive Message Adaptation Detail", "Rock.Blocks.Cms.AdaptiveMessageAdaptationDetail, Rock.Blocks, Version=1.17.0.3, Culture=neutral, PublicKeyToken=null", false, false, "005292C8-6AF7-4250-B29F-759047243BAF" );
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Adaptive Message Adaptation Detail", "Displays the details of a particular adaptive message adaptation.", "Rock.Blocks.Cms.AdaptiveMessageAdaptationDetail", "CMS", "113C4223-19B9-46F2-AAE8-AC646BC5A3C7" );
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Adaptive Message Detail", "Displays the details of a particular adaptive message.", "Rock.Blocks.Cms.AdaptiveMessageDetail", "CMS", "A81FE4E0-DF9F-4978-83A7-EB5459F37938" );
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Adaptive Message List", "Displays a list of adaptive messages.", "Rock.Blocks.Cms.AdaptiveMessageList", "CMS", "CBA57502-8C9A-4414-B0D4-DB0D57EF89BD" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CBA57502-8C9A-4414-B0D4-DB0D57EF89BD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the adaptive message details.", 0, @"", "F9E1AD87-CA8A-49DF-84AC-8F18895FCB87" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CBA57502-8C9A-4414-B0D4-DB0D57EF89BD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "F97029C0-77B9-430C-B8DE-1BE03DCA3C24" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CBA57502-8C9A-4414-B0D4-DB0D57EF89BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "0AC7BE6D-22FB-4A4F-9188-C1FAB8AC1EC1" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A81FE4E0-DF9F-4978-83A7-EB5459F37938", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Adaptive Message Adaptation Detail Page", "AdaptationDetailPage", "Adaptive Message Adaptation Detail Page", @"The page that will show the adaptive message adaptation details.", 0, @"", "E4F286BC-9338-4E17-ABFF-4578793B7A54" );
            

            Sql( @"DECLARE @ComponentEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid]='63D98F58-DA81-46AE-AE0C-662A7BFAA7D0')
                    DECLARE @InteractionEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid]='39753CCE-184A-4F14-AE80-08241DE8FC2E')
                    DECLARE @ChannelTypeMediumValueId INT = (SELECT [Id] FROM [DefinedValue] WHERE [Guid]='5919214F-9C59-4913-BE4E-0DFB6A05F528')

                    INSERT INTO InteractionChannel
                             ([Name], [ComponentEntityTypeId], [InteractionEntityTypeId], [ChannelTypeMediumValueId], [Guid], [IsActive])
                    VALUES 
		                    ('Adaptive Messages',@ComponentEntityTypeId,@InteractionEntityTypeId, @ChannelTypeMediumValueId,'6F467CB2-586B-4963-B73B-9ACC42916549',1)" );


        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "E4F286BC-9338-4E17-ABFF-4578793B7A54" );
            RockMigrationHelper.DeleteAttribute( "0AC7BE6D-22FB-4A4F-9188-C1FAB8AC1EC1" );
            RockMigrationHelper.DeleteAttribute( "F97029C0-77B9-430C-B8DE-1BE03DCA3C24" );
            RockMigrationHelper.DeleteAttribute( "F9E1AD87-CA8A-49DF-84AC-8F18895FCB87" );
            
            RockMigrationHelper.DeleteBlockType( "113C4223-19B9-46F2-AAE8-AC646BC5A3C7" );
            RockMigrationHelper.DeleteBlockType( "CBA57502-8C9A-4414-B0D4-DB0D57EF89BD" );
            RockMigrationHelper.DeleteBlockType( "A81FE4E0-DF9F-4978-83A7-EB5459F37938" );

            DropForeignKey( "dbo.AdaptiveMessageAdaptationSegment", "PersonalizationSegmentId", "dbo.PersonalizationSegment" );
            DropForeignKey( "dbo.AdaptiveMessageAdaptationSegment", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AdaptiveMessageAdaptationSegment", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AdaptiveMessageAdaptationSegment", "AdaptiveMessageAdaptationId", "dbo.AdaptiveMessageAdaptation" );
            DropForeignKey( "dbo.AdaptiveMessageAdaptation", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AdaptiveMessageAdaptation", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AdaptiveMessageAdaptation", "AdaptiveMessageId", "dbo.AdaptiveMessage" );
            DropForeignKey( "dbo.AdaptiveMessage", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AdaptiveMessage", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AdaptiveMessageCategory", "CategoryId", "dbo.Category" );
            DropForeignKey( "dbo.AdaptiveMessageCategory", "AdaptiveMessageId", "dbo.AdaptiveMessage" );
            DropIndex( "dbo.AdaptiveMessageCategory", new[] { "CategoryId" } );
            DropIndex( "dbo.AdaptiveMessageCategory", new[] { "AdaptiveMessageId" } );
            DropIndex( "dbo.AdaptiveMessageAdaptationSegment", new[] { "Guid" } );
            DropIndex( "dbo.AdaptiveMessageAdaptationSegment", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.AdaptiveMessageAdaptationSegment", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.AdaptiveMessageAdaptationSegment", new[] { "AdaptiveMessageAdaptationId" } );
            DropIndex( "dbo.AdaptiveMessageAdaptationSegment", new[] { "PersonalizationSegmentId" } );
            DropIndex( "dbo.AdaptiveMessage", new[] { "Guid" } );
            DropIndex( "dbo.AdaptiveMessage", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.AdaptiveMessage", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.AdaptiveMessageAdaptation", new[] { "Guid" } );
            DropIndex( "dbo.AdaptiveMessageAdaptation", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.AdaptiveMessageAdaptation", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.AdaptiveMessageAdaptation", new[] { "AdaptiveMessageId" } );
            DropTable( "dbo.AdaptiveMessageCategory" );
            DropTable( "dbo.AdaptiveMessageAdaptationSegment" );
            DropTable( "dbo.AdaptiveMessage" );
            DropTable( "dbo.AdaptiveMessageAdaptation" );
        }
    }
}