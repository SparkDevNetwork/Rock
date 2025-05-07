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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;

using Rock.Model;
using Rock.Store;

namespace Rock.Migrations
{
    /// <summary>
    ///
    /// </summary>
    public partial class TempAddLavaApplications : Rock.Migrations.RockMigration
    {
        private const string HelixCategoryGuid = "5874AE45-B5EE-4D10-A274-26B5D69E6283";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // This migration is a bit weird. We have to convert from a plugin
            // that had multiple migrations. So we need to account for the database
            // being potentially in any state along the way up. Meaning, each Up
            // step needs to check if it actually needs to run. The Down() tasks
            // can just run without checks.
            PluginMigration1Up();
            PluginMigration2Up();
            PluginMigration3Up();
            PluginMigration4Up();
            PluginMigration5Up();
            PluginMigration6Up();
            PluginMigration7Up();
            PluginMigration8Up();
            PluginMigration9Up();
            PluginMigration10Up();

            ShortcodesUp();

            MakeGuidIndexesUniqueUp();

            MovePagesUp();

            UpdateRestControllerUp();

            PluginCleanupUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            PluginCleanupDown();

            UpdateRestControllerDown();

            MovePagesDown();

            MakeGuidIndexesUniqueDown();

            ShortcodesDown();

            PluginMigration10Down();
            PluginMigration9Down();
            PluginMigration8Down();
            PluginMigration7Down();
            PluginMigration6Down();
            PluginMigration5Down();
            PluginMigration4Down();
            PluginMigration3Down();
            PluginMigration2Down();
            PluginMigration1Down();
        }

        private void PluginMigration1Up()
        {
            Sql( @"
IF NOT EXISTS (SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'LavaApplication')
BEGIN
    CREATE TABLE [dbo].[LavaApplication] (
        [Id] [int] NOT NULL IDENTITY,
        [Name] [nvarchar](100) NOT NULL,
        [Description] [nvarchar](max),
        [IsSystem] [bit] NOT NULL,
        [IsActive] [bit] NOT NULL,
        [SecurityMode] [int] NOT NULL,
        [Slug] [nvarchar](200),
        [AdditionalSettingsJson] [nvarchar](max),
        [ConfigurationRiggingJson] [nvarchar](max),
        [CreatedDateTime] [datetime],
        [ModifiedDateTime] [datetime],
        [CreatedByPersonAliasId] [int],
        [ModifiedByPersonAliasId] [int],
        [Guid] [uniqueidentifier] NOT NULL,
        [ForeignId] [int],
        [ForeignGuid] [uniqueidentifier],
        [ForeignKey] [nvarchar](100),
        CONSTRAINT [PK_dbo.LavaApplication] PRIMARY KEY ([Id])
    )
    CREATE INDEX [IX_CreatedByPersonAliasId] ON [dbo].[LavaApplication]([CreatedByPersonAliasId])
    CREATE INDEX [IX_ModifiedByPersonAliasId] ON [dbo].[LavaApplication]([ModifiedByPersonAliasId])
    CREATE INDEX [IX_Guid] ON [dbo].[LavaApplication]([Guid])
    CREATE TABLE [dbo].[LavaEndpoint] (
        [Id] [int] NOT NULL IDENTITY,
        [Name] [nvarchar](100) NOT NULL,
        [Description] [nvarchar](max),
        [LavaApplicationId] [int] NOT NULL,
        [Slug] [nvarchar](200),
        [IsSystem] [bit] NOT NULL,
        [EnabledLavaCommands] [nvarchar](500),
        [IsActive] [bit] NOT NULL,
        [AdditionalSettingsJson] [nvarchar](max),
        [HttpMethod] [int] NOT NULL,
        [CodeTemplate] [nvarchar](max),
        [CreatedDateTime] [datetime],
        [ModifiedDateTime] [datetime],
        [CreatedByPersonAliasId] [int],
        [ModifiedByPersonAliasId] [int],
        [Guid] [uniqueidentifier] NOT NULL,
        [ForeignId] [int],
        [ForeignGuid] [uniqueidentifier],
        [ForeignKey] [nvarchar](100),
        CONSTRAINT [PK_dbo.LavaEndpoint] PRIMARY KEY ([Id])
    )
    CREATE INDEX [IX_LavaApplicationId] ON [dbo].[LavaEndpoint]([LavaApplicationId])
    CREATE INDEX [IX_CreatedByPersonAliasId] ON [dbo].[LavaEndpoint]([CreatedByPersonAliasId])
    CREATE INDEX [IX_ModifiedByPersonAliasId] ON [dbo].[LavaEndpoint]([ModifiedByPersonAliasId])
    CREATE INDEX [IX_Guid] ON [dbo].[LavaEndpoint]([Guid])
    ALTER TABLE [dbo].[LavaApplication] ADD CONSTRAINT [FK_dbo.LavaApplication_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY ([CreatedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
    ALTER TABLE [dbo].[LavaApplication] ADD CONSTRAINT [FK_dbo.LavaApplication_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY ([ModifiedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
    ALTER TABLE [dbo].[LavaEndpoint] ADD CONSTRAINT [FK_dbo.LavaEndpoint_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY ([CreatedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
    ALTER TABLE [dbo].[LavaEndpoint] ADD CONSTRAINT [FK_dbo.LavaEndpoint_dbo.LavaApplication_LavaApplicationId] FOREIGN KEY ([LavaApplicationId]) REFERENCES [dbo].[LavaApplication] ([Id]) ON DELETE CASCADE
    ALTER TABLE [dbo].[LavaEndpoint] ADD CONSTRAINT [FK_dbo.LavaEndpoint_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY ([ModifiedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
END
" );
            //CreateTable(
            //    "dbo.LavaApplication",
            //    c => new
            //    {
            //        Id = c.Int( nullable: false, identity: true ),
            //        Name = c.String( nullable: false, maxLength: 100 ),
            //        Description = c.String(),
            //        IsSystem = c.Boolean( nullable: false ),
            //        IsActive = c.Boolean( nullable: false ),
            //        SecurityMode = c.Int( nullable: false ),
            //        Slug = c.String( maxLength: 200 ),
            //        AdditionalSettingsJson = c.String(),
            //        ConfigurationRiggingJson = c.String(),
            //        CreatedDateTime = c.DateTime(),
            //        ModifiedDateTime = c.DateTime(),
            //        CreatedByPersonAliasId = c.Int(),
            //        ModifiedByPersonAliasId = c.Int(),
            //        Guid = c.Guid( nullable: false ),
            //        ForeignId = c.Int(),
            //        ForeignGuid = c.Guid(),
            //        ForeignKey = c.String( maxLength: 100 ),
            //    } )
            //    .PrimaryKey( t => t.Id )
            //    .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
            //    .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
            //    .Index( t => t.CreatedByPersonAliasId )
            //    .Index( t => t.ModifiedByPersonAliasId )
            //    .Index( t => t.Guid, unique: false ); // Unique=false to match plugin, will be fixed later.

            //CreateTable(
            //    "dbo.LavaEndpoint",
            //    c => new
            //    {
            //        Id = c.Int( nullable: false, identity: true ),
            //        Name = c.String( nullable: false, maxLength: 100 ),
            //        Description = c.String(),
            //        LavaApplicationId = c.Int( nullable: false ),
            //        Slug = c.String( maxLength: 200 ),
            //        IsSystem = c.Boolean( nullable: false ),
            //        EnabledLavaCommands = c.String( maxLength: 500 ),
            //        IsActive = c.Boolean( nullable: false ),
            //        AdditionalSettingsJson = c.String(),
            //        HttpMethod = c.Int( nullable: false ),
            //        CodeTemplate = c.String(),
            //        CreatedDateTime = c.DateTime(),
            //        ModifiedDateTime = c.DateTime(),
            //        CreatedByPersonAliasId = c.Int(),
            //        ModifiedByPersonAliasId = c.Int(),
            //        Guid = c.Guid( nullable: false ),
            //        ForeignId = c.Int(),
            //        ForeignGuid = c.Guid(),
            //        ForeignKey = c.String( maxLength: 100 ),
            //    } )
            //    .PrimaryKey( t => t.Id )
            //    .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
            //    .ForeignKey( "dbo.LavaApplication", t => t.LavaApplicationId, cascadeDelete: true )
            //    .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
            //    .Index( t => t.LavaApplicationId )
            //    .Index( t => t.CreatedByPersonAliasId )
            //    .Index( t => t.ModifiedByPersonAliasId )
            //    .Index( t => t.Guid, unique: false ); // Unique=false to match plugin, will be fixed later.
        }

        private void PluginMigration1Down()
        {
            DropForeignKey( "dbo.LavaApplication", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.LavaEndpoint", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.LavaEndpoint", "LavaApplicationId", "dbo.LavaApplication" );
            DropForeignKey( "dbo.LavaEndpoint", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.LavaApplication", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.LavaEndpoint", new[] { "Guid" } );
            DropIndex( "dbo.LavaEndpoint", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.LavaEndpoint", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.LavaEndpoint", new[] { "LavaApplicationId" } );
            DropIndex( "dbo.LavaApplication", new[] { "Guid" } );
            DropIndex( "dbo.LavaApplication", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.LavaApplication", new[] { "CreatedByPersonAliasId" } );
            DropTable( "dbo.LavaEndpoint" );
            DropTable( "dbo.LavaApplication" );
        }

        private void PluginMigration2Up()
        {
            Sql( @"
IF NOT EXISTS (SELECT * FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'LavaEndpoint' AND [COLUMN_NAME] = N'CacheControlHeaderSettings')
BEGIN
    ALTER TABLE [dbo].[LavaEndpoint] ADD [CacheControlHeaderSettings] [nvarchar](500)
    ALTER TABLE [dbo].[LavaEndpoint] ADD [RateLimitRequestPerPeriod] [int]
    ALTER TABLE [dbo].[LavaEndpoint] ADD [RateLimitPeriodDurationSeconds] [int]
END
" );
            //AddColumn( "dbo.LavaEndpoint", "CacheControlHeaderSettings", c => c.String( maxLength: 500 ) );
            //AddColumn( "dbo.LavaEndpoint", "RateLimitRequestPerPeriod", c => c.Int( nullable: true ) );
            //AddColumn( "dbo.LavaEndpoint", "RateLimitPeriodDurationSeconds", c => c.Int( nullable: true ) );
        }

        private void PluginMigration2Down()
        {
            DropColumn( "dbo.LavaEndpoint", "CacheControlHeaderSettings" );
            DropColumn( "dbo.LavaEndpoint", "RateLimitRequestPerPeriod" );
            DropColumn( "dbo.LavaEndpoint", "RateLimitPeriodDurationSeconds" );
        }

        private void PluginMigration3Up()
        {
            // --------------------------------------------------------------
            // ENTITY TYPES
            // --------------------------------------------------------------

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.LavaEndpointList
            RockMigrationHelper.RenameEntityType( "B643984C-03C3-46E2-AA41-5E658DE79921", "Rock.Blocks.Cms.LavaEndpointList", "Lava Endpoint List", "Rock.Blocks.Cms.LavaEndpointList, Rock.Blocks, Version=18.0.0.0, Culture=neutral, PublicKeyToken=null", false, false );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.LavaEndpointDetail
            RockMigrationHelper.RenameEntityType( "45F8578C-C2D6-478D-B20E-D2FA274AB96F", "Rock.Blocks.Cms.LavaEndpointDetail", "Lava Endpoint Detail", "Rock.Blocks.Cms.LavaEndpointDetail, Rock.Blocks, Version=18.0.0.0, Culture=neutral, PublicKeyToken=null", false, false );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.LavaApplicationList
            RockMigrationHelper.RenameEntityType( "B1E023F7-DAF1-4F90-9002-3134ECC5FD6E", "Rock.Blocks.Cms.LavaApplicationList", "Lava Application List", "Rock.Blocks.Cms.LavaApplicationList, Rock.Blocks, Version=18.0.0.0, Culture=neutral, PublicKeyToken=null", false, false );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.LavaApplicationDetail
            RockMigrationHelper.RenameEntityType( "F326D043-C955-4CBA-BCE5-ACCE565371FA", "Rock.Blocks.Cms.LavaApplicationDetail", "Lava Application Detail", "Rock.Blocks.Cms.LavaApplicationDetail, Rock.Blocks, Version=18.0.0.0, Culture=neutral, PublicKeyToken=null", false, false );

            // --------------------------------------------------------------
            // BLOCK TYPES
            // --------------------------------------------------------------

            // Add/Update BlockType 
            //   Name: Lava Endpoint List
            //   Category: CMS
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Lava Endpoint List", "Displays a list of endpoints in a Lava application.", "Rock.Blocks.Cms.LavaEndpointList", "CMS", "3ba03384-027c-4ee8-b44e-5643d583686d" );

            // Add/Update BlockType 
            //   Name: Lava Endpoint Detail
            //   Category: CMS
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Lava Endpoint Detail", "Displays the details of a particular Lava endpoint.", "Rock.Blocks.Cms.LavaEndpointDetail", "CMS", "5466ea16-dac2-490b-9161-92bca6cdfc1a" );

            // Add/Update BlockType 
            //   Name: Lava Application List
            //   Category: CMS
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Lava Application List", "Displays a list of Lava applications.", "Rock.Blocks.Cms.LavaApplicationList", "CMS", "a59ed0dd-cd86-4a0a-a7b7-95d7966d90a5" );

            // Add/Update BlockType 
            //   Name: Lava Application Detail
            //   Category: CMS
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Lava Application Detail", "Displays the details of a particular Lava application.", "Rock.Blocks.Cms.LavaApplicationDetail", "CMS", "f57c354b-9475-4cfb-93d9-cbb9e2e33270" );

            // --------------------------------------------------------------
            // PAGES
            // --------------------------------------------------------------

            // Add Page 
            //  Internal Name: Lava Applications
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "5B6DBC42-8B03-4D15-8D92-AAFA28FD8616", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Lava Applications", "", "A4F654D5-3A85-4626-940F-2AED69A12821", "fa fa-mountain" );

            // Add Page 
            //  Internal Name: Lava Application Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "A4F654D5-3A85-4626-940F-2AED69A12821", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Lava Application Detail", "", "8187988F-38ED-4765-92B3-A9817868CEAA", "" );

            // Add Page 
            //  Internal Name: Lava Endpoint Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "8187988F-38ED-4765-92B3-A9817868CEAA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Lava Endpoint Detail", "", "31A3C218-F0AB-49CD-85C7-E7E78FDAA4EC", "" );

            // --------------------------------------------------------------
            // PAGE ROUTES
            // --------------------------------------------------------------

            // Add Page Route
            //   Page:Lava Applications
            //   Route:cms/lava-applications
            RockMigrationHelper.AddOrUpdatePageRoute( "A4F654D5-3A85-4626-940F-2AED69A12821", "cms/lava-applications", "88DB3032-F842-489F-AC61-A6572416ED8F" );

            // Add Page Route
            //   Page:Lava Application Detail
            //   Route:cms/lava-applications/{LavaApplicationId}
            RockMigrationHelper.AddOrUpdatePageRoute( "8187988F-38ED-4765-92B3-A9817868CEAA", "cms/lava-applications/{LavaApplicationId}", "B91B0858-1600-47BB-AB99-D9BFD839366C" );

            // Add Page Route
            //   Page:Lava Endpoint Detail
            //   Route:cms/lava-applications/{LavaApplicationId}/endpoints/{LavaEndpointId}
            RockMigrationHelper.AddOrUpdatePageRoute( "31A3C218-F0AB-49CD-85C7-E7E78FDAA4EC", "cms/lava-applications/{LavaApplicationId}/endpoints/{LavaEndpointId}", "03AB3C02-E41D-475D-9AAC-C8FE2DFAA69F" );

            // --------------------------------------------------------------
            // PAGE BLOCKS
            // --------------------------------------------------------------

            // Add Block 
            //  Block Name: Lava Application List
            //  Page Name: Lava Applications
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "A4F654D5-3A85-4626-940F-2AED69A12821".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "A59ED0DD-CD86-4A0A-A7B7-95D7966D90A5".AsGuid(), "Lava Application List", "Main", @"", @"", 0, "F72CB267-DAEB-4577-ADEC-6F1350F16C33" );

            // Add Block 
            //  Block Name: Lava Endpoint List
            //  Page Name: Lava Application Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "8187988F-38ED-4765-92B3-A9817868CEAA".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "3BA03384-027C-4EE8-B44E-5643D583686D".AsGuid(), "Lava Endpoint List", "Main", @"", @"", 1, "CC7C2511-C165-42D0-BE3D-1111449A9A56" );

            // Add Block 
            //  Block Name: Lava Application Detail
            //  Page Name: Lava Application Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "8187988F-38ED-4765-92B3-A9817868CEAA".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "F57C354B-9475-4CFB-93D9-CBB9E2E33270".AsGuid(), "Lava Application Detail", "Main", @"", @"", 0, "B3EEBCFE-333B-4288-A7E8-FBBD5EA03270" );

            // Add Block 
            //  Block Name: Lava Endpoint Detail
            //  Page Name: Lava Endpoint Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "31A3C218-F0AB-49CD-85C7-E7E78FDAA4EC".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "5466EA16-DAC2-490B-9161-92BCA6CDFC1A".AsGuid(), "Lava Endpoint Detail", "Main", @"", @"", 0, "8D566AE7-3116-4577-B767-249CCFA3DA9F" );

            // --------------------------------------------------------------
            // BLOCK ORDER
            // --------------------------------------------------------------

            // update block order for pages with new blocks if the page,zone has multiple blocks

            // Update Order for Page: Lava Application Detail,  Zone: Main,  Block: Lava Application Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'B3EEBCFE-333B-4288-A7E8-FBBD5EA03270'" );

            // Update Order for Page: Lava Application Detail,  Zone: Main,  Block: Lava Endpoint List
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'CC7C2511-C165-42D0-BE3D-1111449A9A56'" );

            // --------------------------------------------------------------
            // BLOCK ATTRIBUTES
            // --------------------------------------------------------------

            // Attribute for BlockType
            //   BlockType: Lava Application List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A59ED0DD-CD86-4A0A-A7B7-95D7966D90A5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "54352BC0-35D6-43DC-A27C-9521395B5517" );

            // Attribute for BlockType
            //   BlockType: Lava Application List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A59ED0DD-CD86-4A0A-A7B7-95D7966D90A5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "85301760-7A0E-415A-8702-2E3476096828" );

            // Attribute for BlockType
            //   BlockType: Lava Application List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A59ED0DD-CD86-4A0A-A7B7-95D7966D90A5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the lava application details.", 0, @"", "523FCBB7-F260-4218-A5A0-F8E996BB5AC0" );

            // Attribute for BlockType
            //   BlockType: Lava Endpoint List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3BA03384-027C-4EE8-B44E-5643D583686D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the lava endpoint details.", 0, @"", "6DF78136-324D-46EE-9018-C7099CFA36ED" );

            // Attribute for BlockType
            //   BlockType: Lava Endpoint List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3BA03384-027C-4EE8-B44E-5643D583686D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "9C4E20ED-5C1E-4DB0-AE1F-6AF377804F2D" );

            // Attribute for BlockType
            //   BlockType: Lava Endpoint List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3BA03384-027C-4EE8-B44E-5643D583686D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "AB163FE5-CA58-40EF-841D-CC327466CF6B" );

            // --------------------------------------------------------------
            // BLOCK ATTRIBUTE VALUES
            // --------------------------------------------------------------

            // Add Block Attribute Value
            //   Block: Lava Application List
            //   BlockType: Lava Application List
            //   Category: CMS
            //   Block Location: Page=Lava Applications, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 8187988f-38ed-4765-92b3-a9817868ceaa,b91b0858-1600-47bb-ab99-d9bfd839366c */
            RockMigrationHelper.AddBlockAttributeValue( "F72CB267-DAEB-4577-ADEC-6F1350F16C33", "523FCBB7-F260-4218-A5A0-F8E996BB5AC0", @"8187988f-38ed-4765-92b3-a9817868ceaa,b91b0858-1600-47bb-ab99-d9bfd839366c" );

            // Add Block Attribute Value
            //   Block: Lava Application List
            //   BlockType: Lava Application List
            //   Category: CMS
            //   Block Location: Page=Lava Applications, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "F72CB267-DAEB-4577-ADEC-6F1350F16C33", "74E6E05D-6EEC-42FA-9706-92CC64913BE7", @"False" );

            // Add Block Attribute Value
            //   Block: Lava Application List
            //   BlockType: Lava Application List
            //   Category: CMS
            //   Block Location: Page=Lava Applications, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "F72CB267-DAEB-4577-ADEC-6F1350F16C33", "54352BC0-35D6-43DC-A27C-9521395B5517", @"True" );

            // Add Block Attribute Value
            //   Block: Lava Endpoint List
            //   BlockType: Lava Endpoint List
            //   Category: CMS
            //   Block Location: Page=Lava Application Detail, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 31a3c218-f0ab-49cd-85c7-e7e78fdaa4ec,03ab3c02-e41d-475d-9aac-c8fe2dfaa69f */
            RockMigrationHelper.AddBlockAttributeValue( "CC7C2511-C165-42D0-BE3D-1111449A9A56", "6DF78136-324D-46EE-9018-C7099CFA36ED", @"31a3c218-f0ab-49cd-85c7-e7e78fdaa4ec,03ab3c02-e41d-475d-9aac-c8fe2dfaa69f" );

            // Add Block Attribute Value
            //   Block: Lava Endpoint List
            //   BlockType: Lava Endpoint List
            //   Category: CMS
            //   Block Location: Page=Lava Application Detail, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "CC7C2511-C165-42D0-BE3D-1111449A9A56", "7567C775-E791-4287-8D47-C4FCC896C16C", @"False" );

            // Add Block Attribute Value
            //   Block: Lava Endpoint List
            //   BlockType: Lava Endpoint List
            //   Category: CMS
            //   Block Location: Page=Lava Application Detail, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "CC7C2511-C165-42D0-BE3D-1111449A9A56", "AB163FE5-CA58-40EF-841D-CC327466CF6B", @"True" );
        }

        private void PluginMigration3Down()
        {
            // Attribute for BlockType
            //   BlockType: Lava Endpoint List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "AB163FE5-CA58-40EF-841D-CC327466CF6B" );

            // Attribute for BlockType
            //   BlockType: Lava Endpoint List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "9C4E20ED-5C1E-4DB0-AE1F-6AF377804F2D" );

            // Attribute for BlockType
            //   BlockType: Lava Endpoint List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "6DF78136-324D-46EE-9018-C7099CFA36ED" );

            // Attribute for BlockType
            //   BlockType: Lava Application List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "54352BC0-35D6-43DC-A27C-9521395B5517" );

            // Attribute for BlockType
            //   BlockType: Lava Application List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "85301760-7A0E-415A-8702-2E3476096828" );

            // Attribute for BlockType
            //   BlockType: Lava Application List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "523FCBB7-F260-4218-A5A0-F8E996BB5AC0" );

            // Remove Block
            //  Name: Lava Endpoint Detail, from Page: Lava Endpoint Detail, Site: Rock RMS
            //  from Page: Lava Endpoint Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "8D566AE7-3116-4577-B767-249CCFA3DA9F" );

            // Remove Block
            //  Name: Lava Endpoint List, from Page: Lava Application Detail, Site: Rock RMS
            //  from Page: Lava Application Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "CC7C2511-C165-42D0-BE3D-1111449A9A56" );

            // Remove Block
            //  Name: Lava Application Detail, from Page: Lava Application Detail, Site: Rock RMS
            //  from Page: Lava Application Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B3EEBCFE-333B-4288-A7E8-FBBD5EA03270" );

            // Remove Block
            //  Name: Lava Application List, from Page: Lava Applications, Site: Rock RMS
            //  from Page: Lava Applications, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F72CB267-DAEB-4577-ADEC-6F1350F16C33" );

            // Delete BlockType 
            //   Name: Lava Endpoint List
            //   Category: CMS
            //   Path: -
            //   EntityType: Lava Endpoint List
            RockMigrationHelper.DeleteBlockType( "3BA03384-027C-4EE8-B44E-5643D583686D" );

            // Delete BlockType 
            //   Name: Lava Endpoint Detail
            //   Category: CMS
            //   Path: -
            //   EntityType: Lava Endpoint Detail
            RockMigrationHelper.DeleteBlockType( "5466EA16-DAC2-490B-9161-92BCA6CDFC1A" );

            // Delete BlockType 
            //   Name: Lava Application List
            //   Category: CMS
            //   Path: -
            //   EntityType: Lava Application List
            RockMigrationHelper.DeleteBlockType( "A59ED0DD-CD86-4A0A-A7B7-95D7966D90A5" );

            // Delete BlockType 
            //   Name: Lava Application Detail
            //   Category: CMS
            //   Path: -
            //   EntityType: Lava Application Detail
            RockMigrationHelper.DeleteBlockType( "F57C354B-9475-4CFB-93D9-CBB9E2E33270" );

            // Delete Page 
            //  Internal Name: Lava Endpoint Detail
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "31A3C218-F0AB-49CD-85C7-E7E78FDAA4EC" );

            // Delete Page 
            //  Internal Name: Lava Application Detail
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "8187988F-38ED-4765-92B3-A9817868CEAA" );

            // Delete Page 
            //  Internal Name: Lava Applications
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "A4F654D5-3A85-4626-940F-2AED69A12821" );
        }

        private void PluginMigration4Up()
        {
            // We don't need to check if this step has already run because
            // it is safe to just always set the value.
            Sql( @"
UPDATE [Page]
    SET [BreadCrumbDisplayName] = 0
    WHERE [Guid] IN ('8187988f-38ed-4765-92b3-a9817868ceaa','31a3c218-f0ab-49cd-85c7-e7e78fdaa4ec')
" );
        }

        private void PluginMigration4Down()
        {
        }

        private void PluginMigration5Up()
        {
            Sql( @"
IF NOT EXISTS (SELECT * FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'LavaEndpoint' AND [COLUMN_NAME] = N'SecurityMode')
BEGIN
    -- Drop the randomly named default constraint.
    DECLARE @var0 nvarchar(128)
    SELECT @var0 = name
    FROM sys.default_constraints
    WHERE parent_object_id = object_id(N'dbo.LavaApplication')
    AND col_name(parent_object_id, parent_column_id) = 'SecurityMode';
    IF @var0 IS NOT NULL
        EXECUTE('ALTER TABLE [dbo].[LavaApplication] DROP CONSTRAINT [' + @var0 + ']')

    ALTER TABLE LavaEndpoint ADD [SecurityMode] int NOT NULL DEFAULT(0)
    ALTER TABLE LavaApplication DROP COLUMN [SecurityMode]
END
" );
        }

        private void PluginMigration5Down()
        {
            Sql( @"
-- Drop the randomly named default constraint.
DECLARE @var0 nvarchar(128)
SELECT @var0 = name
FROM sys.default_constraints
WHERE parent_object_id = object_id(N'dbo.LavaEndpoint')
AND col_name(parent_object_id, parent_column_id) = 'SecurityMode';
IF @var0 IS NOT NULL
    EXECUTE('ALTER TABLE [dbo].[LavaEndpoint] DROP CONSTRAINT [' + @var0 + ']')

ALTER TABLE LavaApplication ADD [SecurityMode] int NOT NULL DEFAULT(0)
ALTER TABLE LavaEndpoint DROP COLUMN [SecurityMode]
" );
        }

        private void PluginMigration6Up()
        {
            Sql( @"
IF NOT EXISTS (SELECT [Guid] FROM [Group] WHERE [Guid] = 'F88EC798-2DCC-119C-4459-CF0F304CC036')
BEGIN
    DECLARE @groupTypeId int
    SET @groupTypeId = (SELECT [Id] FROM [GroupType] WHERE [Guid] = 'AECE949F-704C-483E-A4FB-93D5E4720C4C')

    INSERT INTO [dbo].[Group]
               ([IsSystem]
               ,[ParentGroupId]
               ,[GroupTypeId]
               ,[CampusId]
               ,[Name]
               ,[Description]
               ,[IsSecurityRole]
               ,[IsActive]
               ,[Guid]
               ,[Order])
         VALUES
               (1
               ,null
               ,@groupTypeId
               ,null
               ,'RSR - Lava Application Developers'
               ,'Security role for individuals who will be developing Lava Applications'
               ,1
               ,1
               ,'F88EC798-2DCC-119C-4459-CF0F304CC036'
               ,0)
END
" );
            //RockMigrationHelper.AddSecurityRoleGroup( "RSR - Lava Application Developers", "Security role for individuals who will be developing Lava Applications", SystemGuid.Group.GROUP_LAVA_APPLICATION_DEVELOPERS );
        }

        private void PluginMigration6Down()
        {
            Sql( @"
DECLARE @GroupId INT = (SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = 'F88EC798-2DCC-119C-4459-CF0F304CC036')
IF @GroupId IS NOT NULL
BEGIN
    DELETE FROM [AuthAuditLog] WHERE [GroupId] = @GroupId
END" );
            RockMigrationHelper.DeleteSecurityRoleGroup( SystemGuid.Group.GROUP_LAVA_APPLICATION_DEVELOPERS );
        }

        private void PluginMigration7Up()
        {
            RockMigrationHelper.UpdateCategory( "7574A473-3326-4973-8DF6-C7BF5F64EB36", "Helix", "", "Category for Helix related shortcodes.", HelixCategoryGuid );

            // The rest of this plugin migration has been moved to ShortcodesUp()
        }

        private void PluginMigration7Down()
        {
            // The rest of this plugin migration has been moved to ShortcodesDown()

            RockMigrationHelper.DeleteCategory( HelixCategoryGuid );
        }

        private void PluginMigration8Up()
        {
            // This has been moved to ShortcodesUp()
        }

        private void PluginMigration8Down()
        {
            // This has been moved to ShortcodesDown()
        }

        private void PluginMigration9Up()
        {
            // We don't need to check if this step has already run because
            // it is safe to just always delete the constraint even if it
            // is already correct.
            Sql( @"
-- Drop existing constraint
ALTER TABLE [dbo].[LavaEndpoint]
    DROP CONSTRAINT [FK_dbo.LavaEndpoint_dbo.LavaApplication_LavaApplicationId]

-- Add it back with a cascade delete
ALTER TABLE [dbo].[LavaEndpoint]  WITH CHECK ADD  CONSTRAINT [FK_dbo.LavaEndpoint_dbo.LavaApplication_LavaApplicationId] FOREIGN KEY([LavaApplicationId])
REFERENCES [dbo].[LavaApplication] ([Id]) ON DELETE CASCADE
" );
        }

        private void PluginMigration9Down()
        {
            // Leave the cascade delete in place.
        }

        private void PluginMigration10Up()
        {
            // This has been moved to ShortcodesUp()
        }

        private void PluginMigration10Down()
        {
            // This has been moved to ShortcodesDown()
        }

        private void ShortcodesUp()
        {
            RockMigrationHelper.AddOrUpdateLavaShortcode( "Campus Picker", CampusPickerTagName, CampusPickerDescription, CampusPickerDocumentation, CampusPickerMarkup, CampusPickerParameters, CampusPickerTagType, HelixCategoryGuid, CampusPickerGuid );
            RockMigrationHelper.AddOrUpdateLavaShortcode( "Checkbox List", CheckboxListTagName, CheckboxListDescription, CheckboxListDocumentation, CheckboxListMarkup, CheckboxListParameters, CheckboxListTagType, HelixCategoryGuid, CheckboxListGuid );
            RockMigrationHelper.AddOrUpdateLavaShortcode( "Currency", CurrencyTagName, CurrencyDescription, CurrencyDocumentation, CurrencyMarkup, CurrencyParameters, CurrencyTagType, HelixCategoryGuid, CurrencyGuid );
            RockMigrationHelper.AddOrUpdateLavaShortcode( "Date Picker", DatePickerTagName, DatePickerDescription, DatePickerDocumentation, DatePickerMarkup, DatePickerParameters, DatePickerTagType, HelixCategoryGuid, DatePickerGuid );
            RockMigrationHelper.AddOrUpdateLavaShortcode( "Date Range Picker", DateRangePickerTagName, DateRangePickerDescription, DateRangePickerDocumentation, DateRangePickerMarkup, DateRangePickerParameters, DateRangePickerTagType, HelixCategoryGuid, DateRangePickerGuid );
            RockMigrationHelper.AddOrUpdateLavaShortcode( "Defined Value Picker", DefinedValuePickerTagName, DefinedValuePickerDescription, DefinedValuePickerDocumentation, DefinedValuePickerMarkup, DefinedValuePickerParameters, DefinedValuePickerTagType, HelixCategoryGuid, DefinedValuePickerGuid );
            RockMigrationHelper.AddOrUpdateLavaShortcode( "Dropdown", DropdownTagName, DropdownDescription, DropdownDocumentation, DropdownMarkup, DropdownParameters, DropdownTagType, HelixCategoryGuid, DropdownGuid );
            RockMigrationHelper.AddOrUpdateLavaShortcode( "Memo", MemoTagName, MemoDescription, MemoDocumentation, MemoMarkup, MemoParameters, MemoTagType, HelixCategoryGuid, MemoGuid );
            RockMigrationHelper.AddOrUpdateLavaShortcode( "Radio Button List", RadioButtonListTagName, RadioButtonListDescription, RadioButtonListDocumentation, RadioButtonListMarkup, RadioButtonListParameters, RadioButtonListTagType, HelixCategoryGuid, RadioButtonListGuid );
            RockMigrationHelper.AddOrUpdateLavaShortcode( "Range Slider", RangeSliderTagName, RangeSliderDescription, RangeSliderDocumentation, RangeSliderMarkup, RangeSliderParameters, RangeSliderTagType, HelixCategoryGuid, RangeSliderGuid );
            RockMigrationHelper.AddOrUpdateLavaShortcode( "Rock Control", RockControlTagName, RockControlDescription, RockControlDocumentation, RockControlMarkup, RockControlParameters, RockControlTagType, HelixCategoryGuid, RockControlGuid );
            RockMigrationHelper.AddOrUpdateLavaShortcode( "Text Box", TextBoxTagName, TextBoxDescription, TextBoxDocumentation, TextBoxMarkup, TextBoxParameters, TextBoxTagType, HelixCategoryGuid, TextBoxGuid );
        }

        private void ShortcodesDown()
        {
            RockMigrationHelper.DeleteLavaShortcode( TextBoxGuid );
            RockMigrationHelper.DeleteLavaShortcode( RockControlGuid );
            RockMigrationHelper.DeleteLavaShortcode( RangeSliderGuid );
            RockMigrationHelper.DeleteLavaShortcode( RadioButtonListGuid );
            RockMigrationHelper.DeleteLavaShortcode( MemoGuid );
            RockMigrationHelper.DeleteLavaShortcode( DropdownGuid );
            RockMigrationHelper.DeleteLavaShortcode( DefinedValuePickerGuid );
            RockMigrationHelper.DeleteLavaShortcode( DateRangePickerGuid );
            RockMigrationHelper.DeleteLavaShortcode( DatePickerGuid );
            RockMigrationHelper.DeleteLavaShortcode( CurrencyGuid );
            RockMigrationHelper.DeleteLavaShortcode( CheckboxListGuid );
            RockMigrationHelper.DeleteLavaShortcode( CampusPickerGuid );
        }

        private void MakeGuidIndexesUniqueUp()
        {
            Sql( @"
-- Generate new unique identifiers for any duplicate values.
UPDATE [LavaApplication]
SET [Guid] = NEWID()
WHERE [Guid] IN (SELECT [Guid] FROM [LavaApplication] GROUP BY [Guid] HAVING COUNT(*) > 1)

UPDATE [LavaEndpoint]
SET [Guid] = NEWID()
WHERE [Guid] IN (SELECT [Guid] FROM [LavaEndpoint] GROUP BY [Guid] HAVING COUNT(*) > 1)
" );

            DropIndex( "dbo.LavaApplication", "IX_Guid" );
            DropIndex( "dbo.LavaEndpoint", "IX_Guid" );

            CreateIndex( "dbo.LavaApplication", "Guid", unique: true );
            CreateIndex( "dbo.LavaEndpoint", "Guid", unique: true );
        }

        private void MakeGuidIndexesUniqueDown()
        {
            DropIndex( "dbo.LavaApplication", "IX_Guid" );
            DropIndex( "dbo.LavaEndpoint", "IX_Guid" );

            CreateIndex( "dbo.LavaApplication", "Guid", unique: false );
            CreateIndex( "dbo.LavaEndpoint", "Guid", unique: false );
        }

        private void MovePagesUp()
        {
            // Move the page tree to the Settings -> CMS page.
            RockMigrationHelper.MovePage( "A4F654D5-3A85-4626-940F-2AED69A12821", "b4a24ab7-9369-4055-883f-4f4892c39ae3" );
        }

        private void MovePagesDown()
        {
            // Move the page tree back to Installed Plugins.
            RockMigrationHelper.MovePage( "A4F654D5-3A85-4626-940F-2AED69A12821", "5B6DBC42-8B03-4D15-8D92-AAFA28FD8616" );
        }

        private void UpdateRestControllerUp()
        {
            // Update the name and class, if the controller doesn't exist
            // that is fine because it will be created later during Rock
            // startup.
            Sql( @"
UPDATE [RestController]
SET [Name] = 'LavaApp',
    [ClassName] = 'Rock.Rest.v2.LavaAppController'
WHERE [Guid] = '8af769e9-972c-4f40-8344-89ff4b07fcbd'
" );
        }

        private void UpdateRestControllerDown()
        {
            // Intentionally blank
        }

        private void PluginCleanupUp()
        {
            // Delete PluginMigration values.
            Sql( "DELETE FROM [PluginMigration] WHERE [PluginAssemblyName] = 'tech.triumph.Lava.Helix'" );

            // Delete the things that won't cause a restart first.
            try
            {
                // Delete the ~/Plugin files.
                var path = HostingEnvironment.MapPath( "~/Plugins/tech_triumph/LavaHelix" );
                
                if ( Directory.Exists( path ) )
                {
                    Directory.Delete( path, true );
                }

                // Remove the plugin from the installed plugins list.
                var packageFile = HostingEnvironment.MapPath( "~/App_Data/InstalledStorePackages.json" );

                if ( File.Exists( packageFile ) )
                {
                    var json = File.ReadAllText( packageFile );
                    var installedPackages = json.FromJsonOrNull<List<InstalledPackage>>();

                    if ( installedPackages != null )
                    {
                        // PackageId 208 == Helix Plugin
                        installedPackages = installedPackages.Where( p => p.PackageId != 209 ).ToList();
                        File.WriteAllText( packageFile, installedPackages.ToJson() );
                    }
                }
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( $"Error during Helix cleanup: {ex.Message}" );
            }

            // Delete old files from ~/Bin directory. tech.triumph.Lava.Helix.dll, tech.triumph.Lava.Helix.pdb
            try
            {
                var path = HostingEnvironment.MapPath( "~/Bin/tech.triumph.Lava.Helix.dll" );
                if ( File.Exists( path ) )
                {
                    File.Delete( path );
                }

                path = HostingEnvironment.MapPath( "~/Bin/tech.triumph.Lava.Helix.pdb" );
                if ( File.Exists( path ) )
                {
                    File.Delete( path );
                }
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( $"Error during Helix cleanup: {ex.Message}" );
            }
        }

        private void PluginCleanupDown()
        {
            // Intentionally blank, don't try to restore any files.
        }

        #region ShortCode Strings

        #region Campus Picker

        private const string CampusPickerTagName = "campuspicker";
        private const string CampusPickerGuid = "E787B188-2E0F-479E-A855-0E4ABA75C91B";
        private const string CampusPickerMarkup = @"//- Prep configuration settings
{% assign sc-statusList = campusstatuses | Split:',' %}
{% assign sc-typeList = campustypes | Split:',',true %}
{% assign sc-selectableList = selectablecampuses | Split:',' %}
{% assign sc-currentValues = value | Split:',' %} //- Note we're supporting the possibility that there could be multiple values.
{% assign includeinactive = includeinactive | AsBoolean %}
{% assign allowmultiple = allowmultiple | AsBoolean %}

//- Get source data
{% assign sc-campuses = 'All' | FromCache:'Campus'  %}

//- Filtering works by selecting all campuses that should not be shown
//- and then removing them from the list. This means that configuration
//- filters are AND not OR (which matches the C# logic).

//- Filter by type
{% if sc-typeList != empty %}

    {% for campus in sc-campuses reversed %}
        {% assign campusTypeId = campus.CampusTypeValueId | ToString %}
        {% assign isConfiguredType = sc-typeList | Contains:campusTypeId %}
        {% if isConfiguredType == false %}
            {% assign sc-campuses = sc-campuses | RemoveFromArray:campus %}
        {% endif %}
    {% endfor %}

{% endif %}

//- Filter by status
{% if sc-statusList != empty %}

    {% for campus in sc-campuses reversed %}
        {% assign campusStatusId = campus.CampusStatusValueId | ToString %}
        {% assign isConfiguredStatus = sc-statusList | Contains:campusStatusId %}
        {% if isConfiguredStatus == false %}
            {% assign sc-campuses = sc-campuses | RemoveFromArray:campus %}
        {% endif %}
    {% endfor %}

{% endif %}

//- Filter by selected
{% if sc-selectableList != empty %}

    {% for campus in sc-campuses reversed %}
        {% assign campusId = campus.Id | ToString %}
        {% assign isSelected = sc-statusList | Contains:campusId %}
        {% if isSelected == false %}
            {% assign sc-campuses = sc-campuses | RemoveFromArray:campus %}
        {% endif %}
    {% endfor %}

{% endif %}

//- Remove inactive campuses
{% if includeinactive == false %}
    {% assign sc-campuses = sc-campuses | Where:'IsActive',true %}
{% endif %}

//- Ensure current values are still in the list, the value can be either a campus id or guid
{% assign allCampuses = 'All' | FromCache:'Campus'  %}
{% for currentValue in sc-currentValues %}
    {% for campus in allCampuses %}
        {% assign campusId = campus.Id | ToString %}
        {% assign campusGuid = campus.Guid | ToString %}
        {% if campusId == currentValue or campusGuid == currentValue %}
            //- Ensure the campus list has this campus, if not add it
            {% assign isInCampusList = sc-campuses | Contains:campus %}

            {% if isInCampusList == false %}
                {% assign sc-campuses = sc-campuses | AddToArray:campus %}
            {% endif %}
        {% endif %}
    {% endfor %}
{% endfor %}

//- Sort Campuses
{% assign sc-campuses = sc-campuses | OrderBy:'Order' %}

//- Control formatting
{% if allowmultiple %}
    {[ checkboxlist label:'{{ label }}' showlabel:'{{ showlabel }}' name:'{{ name }}' isrequired:'{{ isrequired }}' value:'{{ value }}' columns:'4' controltype:'campus-picker' id:'{{ id }}' validationmessage:'{{ validationmessage }}' additionalattributes:'{{ additionalattributes}}' ]}

        {% for campus in sc-campuses %}
            [[ item value:'{% if valuefield == 'id' %}{{ campus.Id }}{% else %}{{ campus.Guid }}{% endif %}' text:'{{ campus.Name }}' ]][[ enditem]]
        {% endfor %}

    {[ endcheckboxlist ]}
{% else %}
    {[ dropdown label:'{{ label }}' showlabel:'{{ showlabel }}' name:'{{ name }}' longlistenabled:'{{ longlistenabled }}' value:'{{ value }}' controltype:'campus-picker' isrequired:'{{ isrequired }}' id:'{{ id }}' validationmessage:'{{ validationmessage }}' additionalattributes:'{{ additionalattributes}}' ]}

        {% for campus in sc-campuses %}
            [[ item value:'{% if valuefield == 'id' %}{{ campus.Id }}{% else %}{{ campus.Guid }}{% endif %}' text:'{{ campus.Name }}' ]][[ enditem]]
        {% endfor %}

    {[ enddropdown ]}
{% endif %}";
        private const string CampusPickerParameters = @"value^|includeinactive^false|campustypes^|campusstatuses^|selectablecampuses^|name^campus|isrequired^false|label^Campus|validationmessage^Please select a campus.|longlistenabled^false|valuefield^id|allowmultiple^false|additionalattributes^|showlabel^true";
        private const string CampusPickerDocumentation = @"<p>This control allows you to select a campus.</p>

<h5>Example Usage</h5>
<pre>{[ campuspicker label:'Primary Campus' value:'1,2' types:'768' statuses:'765' selectablecampuses:'1' ]}</pre>

<h5>Parameters</h5>
<p>Below are the parameters for the campus picker shortcode.</p>
<ul>
    <li><strong>label</strong> - The label to display above the control.</li><li><b>showlabel </b>(true) - Whether to display label.</li>
    <li><strong>name</strong> (campus) - The name for the campus picker control.</li>
    <li><strong>value</strong> - The ID or Guid of the currently selected campus(es).</li>
    <li><strong>valuefield</strong> (id) - Specifies whether the picker's value should correspond to the campus' <code>id</code> or <code>guid</code>.</li>
    <li><strong>includeinactive</strong> (false) - Determines if inactive campuses should be displayed.</li>
    <li><strong>campustypes</strong> - Filters the campus list by type (comma separated list of defined value ids).</li>
    <li><strong>campusstatuses</strong> - Filters the campus list by status (comma separated list of defined value ids).</li>
    <li><strong>selectablecampuses</strong> -  List of specific campuses to display (comma separated list of campus ids).</li>
    <li><strong>longlistenabled</strong> (false) -  Enhances the functionality to include a search feature, facilitating swift and efficient selection of the preferred item from the list.</li>
    <li><strong>allowmultiple</strong> (false) -  Determines if the selection of multiple values is allowed.</li><li><b>isrequired </b>(false) - Establishes whether making a selection is necessary.
</li><li><b>validationmessage </b>(Please provide a campus.) - Message to display when the value is not valid.</li>
    <li><strong>additionalattributes</strong> -  Additional attributes to include on the input control.</li>
</ul>

<p>
    The above settings enable a wide range of filtering options for the list. Regardless of the filter configurations, the
    current value will consistently be shown.
</p>";
        private const string CampusPickerDescription = "Displays a campus picker.";
        private const int CampusPickerTagType = ( int ) TagType.Inline;

        #endregion

        #region Checkbox List

        private const string CheckboxListTagName = "checkboxlist";
        private const string CheckboxListGuid = "D052824F-E514-47D9-953E-2C9B55FF72D0";
        private const string CheckboxListMarkup = @"{% assign sc-values =  value | Split:',',true %}

{[ rockcontrol id:'rc-{{ '' | UniqueIdentifier }}' label:'{{ label }}' showlabel:'{{ showlabel }}' controltype:'{{ controltype }}' isrequired:'{{ isrequired }}' validationmessage:'{{ validationmessage }}' ]}

    <div class=""controls js-rockcheckboxlist rock-check-box-list rockcheckboxlist rockcheckboxlist-horizontal in-columns in-columns-{{ columns }} cbl-{{ name }} {% if isrequired %}required{% endif %}"">
        {% for item in items %}

            {% assign itemId = id | Append:'_' | Append:forloop.index0 %}
            {% assign isValueSelected = sc-values | Contains:item.value %}
            <label class=""checkbox-inline"" for=""{{ itemId }}"">
                <input id=""{{ itemId }}"" type=""checkbox"" name=""{{ name }}"" value=""{{ item.value }}"" {% if isValueSelected %}checked{% endif %} {{ additionalattributes }}>
                <span class=""label-text"">{{ item.text }}</span>
            </label>

        {% endfor %}
    </div>

{[ endrockcontrol ]}";
        private const string CheckboxListParameters = @"label^Checkbox List|value^|name^checkbox|isrequired^false|type^rock-check-box-list|validationmessage^Please select at least one item.|columns^1|additionalattributes^|showlabel^true";
        private const string CheckboxListDocumentation = @"<p>This control displays a checkbox list.</p>
<h5>Example Usage</h5>
<pre>{[ checkboxlist label:'Favorite Colors' name:'favorite-colors' isrequired:'true' value:'2' columns:'4' ]}
    [[ item value:'1' text:'Red' ]][[ enditem]]
    [[ item value:'2' text:'Green' ]][[ enditem]]
    [[ item value:'3' text:'Blue' ]][[ enditem]]
    [[ item value:'4' text:'Orange' ]][[ enditem]]
    [[ item value:'5' text:'Yellow' ]][[ enditem]]
{[ endcheckboxlist ]}</pre>
<h5>Parameters</h5>
<p>Below are the parameters for the checkbox list shortcode.</p>
<ul>
<li><strong>label</strong> - The label to display above the control.</li>
<li><strong>showlabel </strong>(true) - Whether to display label.</li>
<li><strong>name </strong>(checkbox) - The name for the Checkbox list picker control.</li>
<li><strong>value</strong> - The currently selected values.</li>
<li><strong>controltype</strong> (rock-check-box-list) - The type of control. This is appended to the root form-group.</li>
<li><strong>columns</strong> - The number of colums to align the checkboxes to.</li>
<li><strong>isrequired </strong>(false) - Establishes whether making a selection is necessary.</li>
<li><strong>validationmessage </strong>(Please select a value.) - Message to display when the value is not valid.</li>
<li><strong>additionalattributes</strong> - Additional attributes that you want to add to the input controls (not each checkbox control will get these attributes).</li>
</ul>";
        private const string CheckboxListDescription = "Displays a checkbox list.";
        private const int CheckboxListTagType = ( int ) TagType.Block;

        #endregion

        #region Currency

        private const string CurrencyTagName = "currency";
        private const string CurrencyGuid = "1307452F-446E-485B-A949-7CDD6DF75931";
        private const string CurrencyMarkup = @"{% assign isrequired = isrequired | AsBoolean %}

{[ rockcontrol id:'rc-{{ '' | UniqueIdentifier }}' showlabel:'{{ showlabel }}' label:'{{ label }}' controltype:'currency-box' isrequired:'{{ isrequired }}' validationmessage:'{{ validationmessage }}' ]}

    <div class=""control-wrapper"">
        <div class=""input-group "">
            <span class=""input-group-addon"">{{ 'Global' | Attribute:'CurrencySymbol' }}</span>
            <input type=""number"" class=""form-control"" inputmode=""decimal"" step=""0.01"" value=""{{ value }}"" {% if isrequired == true %}required{% endif %} {{ additionalattributes }}>
        </div>
    </div>

{[ endrockcontrol ]}";
        private const string CurrencyParameters = @"label^Currency|isrequired^false|value^|validationmessage^Please specify an amount.|showlabel^true|additionalattributes ^";
        private const string CurrencyDocumentation = @"<p>This control displays a currency input.</p>

<h5>Example Usage</h5>
<pre>{[ currency label:'Currency' value:'100.00' isrequired:'true' ]}</pre>

<h5>Parameter List</h5>
<p>This control supports the following parameters:</p>
<ul>
    <li><strong>label</strong> (Currency) - The label to display above the control.</li><li><b>showlabel </b>(true) - Whether to display the label.</li>
    <li><strong>value</strong> - The value to initially set the control to.</li><li><b>isrequired </b>(false) - Establishes whether making a selection is necessary.</li>
    <li><strong>validationmessage</strong> (Please specify an amount.) - Message to display when the value is not valid.</li>
    <li><strong>additionalattributes</strong> -  Additional attributes to include on the input control.</li>
</ul>";
        private const string CurrencyDescription = "Displays a currency control.";
        private const int CurrencyTagType = ( int ) TagType.Inline;

        #endregion

        #region Date Picker

        private const string DatePickerTagName = "datepicker";
        private const string DatePickerGuid = "43B4AC7D-A8DE-4F99-9812-84C055D34799";
        private const string DatePickerMarkup = @"{% capture id %}rc-{{ '' | UniqueIdentifier }}{% endcapture %}

{[ rockcontrol id:'{{ id }}' label:'{{ label }}' showlabel:'{{ showlabel }}' controltype:'date-picker' isrequired:'{{ isrequired }}' validationmessage:'{{ validationmessage }}' ]}

    {% assign isrequired = isrequired | AsBoolean %}
    {% assign showlabel = showlabel | AsBoolean %}

    <div class=""input-group input-width-md js-date-picker date"">
        <input name=""{{ name }}"" type=""text"" id=""{{ id }}"" class=""form-control"" value=""{{ value }}"" {% if isrequired == true %}required{% endif %} {{ additionalattributes }}> <span class=""input-group-addon""><i class=""fa fa-calendar""></i></span>
    </div>


    <script>
        Rock.controls.datePicker.initialize(
            {
                id: '{{ id }}',
                startView: 0,
                showOnFocus: true,
                format: 'mm/dd/yyyy',
                todayHighlight: true,
                forceParse: true,
                postbackScript: '',
            });
    </script>

{[ endrockcontrol ]}
";
        private const string DatePickerParameters = @"label^Date|value^|isrequired^false|name^date|validationmessage^Please provide a date.|additionalattributes^|showlabel^true";
        private const string DatePickerDocumentation = @"<p>This control displays a date input.</p>
<h5>Example Usage</h5>
<pre>{[ datepicker label:'My Date' name:'date' isrequired:'true' value:'{{ 'Now' | Date:'M/d/yyyy' }}' ]}</pre>
<h5>Parameter List</h5>
<p>This control supports the following parameters:</p>
<ul>
<li><strong>label</strong> (Date) - The label to display above the control.</li>
<li><strong>showlabel&nbsp;</strong>(true) - Whether to display a label.</li>
<li><strong>name</strong> (date) - The name for the date picker control.</li>
<li><strong>value</strong> - The date to initially set the control to.</li>
<li><strong>isrequired </strong>(false) - Establishes whether making a selection is necessary.</li>
<li><strong>validationmessage </strong>(Please provide a date.) - Message to display when the value is not valid.</li>
<li><strong>additionalattributes</strong> - Additional attributes to include on the input control.</li>
</ul>";
        private const string DatePickerDescription = "Displays a date picker.";
        private const int DatePickerTagType = ( int ) TagType.Inline;

        #endregion

        #region Date Range Picker

        private const string DateRangePickerTagName = "daterangepicker";
        private const string DateRangePickerGuid = "5ABBC6EA-8AD4-4FD4-AF9A-0D782A32C956";
        private const string DateRangePickerMarkup = @"{% capture id %}rc-{{ '' | UniqueIdentifier }}{% endcapture %}

{[ rockcontrol id:'{{ id }}' label:'{{ label }}' showlabel:'{{ showlabel }}' controltype:'date-range-picker' isrequired:'{{ isrequired }}' validationmessage:'{{ validationmessage }}' ]}

    {% assign isrequired = isrequired | AsBoolean %}
    
    <div id=""{{ id }}"" data-required=""false"" data-itemlabel=""Default Value"" class=""js-daterangepicker picker-daterange"">
      <div class=""form-control-group"">
      
            <div id=""{{ id | Append:'_lower'}}"" class=""input-group input-group-lower js-lower input-width-md js-date-picker date"">
                <input name=""{{ name | Append:'_lower'}}"" type=""text"" class=""form-control"" value=""{{ value }}"" {% if isrequired == true %}required{% endif %}>
                <span class=""input-group-addon"">
                    <i class=""fa fa-calendar""></i>
                </span>
            </div>
            
            <span name=""{{ id }}"" class=""validation-error help-inline"" style=""display:none;""></span>
            <div class=""input-group form-control-static""> to </div>
            
            <div id= ""{{id | Append:'_upper' }}"" class=""input-group input-group-upper js-upper input-width-md js-date-picker date"">
                <input name=""{{ name | Append:'_upper'}}"" type=""text"" class=""form-control"" value=""{{ value }}"">
                <span class=""input-group-addon"">
                    <i class=""fa fa-calendar""></i>
                </span>
            </div>
            
            <span name=""{{ id }}"" class=""validation-error help-inline"" style=""display:none;""></span>
            
        </div>
      <span name=""{{ id }}"" class=""validation-error help-inline"" style=""display:none;"">Default Value is required.</span>
    </div>

    <script>

        
             Rock.controls.datePicker.initialize(
                {
                    id: ""{{ id | Append:'_lower'}}"",
                });
                Rock.controls.datePicker.initialize(
                {
                    id: ""{{ id | Append:'_upper'}}"",
                });

    </script>

{[ endrockcontrol ]}
";
        private const string DateRangePickerParameters = @"label^Date Range|isrequired^false|value^|validationmessage^Please provide a valid Date.|showlabel^true|name^daterange";
        private const string DateRangePickerDocumentation = @"<p>This control displays a two input the Start Date and End Date.</p>
<h5>Example Usage</h5>
<pre>{[ daterangepicker label:'Date Range' isrequired:'true' value:'{{ 'Now' | Date:'M/d/yyyy' }}']}</pre>
<h5>Parameter List</h5>
<p>This control supports the following parameters:</p>
<ul>
<li><strong>label</strong> (Date) - The label to display above the control.</li>
<li><strong>showlabel </strong>(true) - Whether to display the label.</li>
<li><strong>value</strong> - The date to initially set the control to.</li>
<li><strong>isrequired </strong>(false) - Establishes whether making a selection is necessary.</li>
<li><strong>validationmessage </strong>(Please provide a valid Date.) - Message to display when the value is not valid.</li>
</ul>";
        private const string DateRangePickerDescription = "Display a Date Range picker.";
        private const int DateRangePickerTagType = ( int ) TagType.Inline;

        #endregion

        #region Defined Value Picker

        private const string DefinedValuePickerTagName = "definedvaluepicker";
        private const string DefinedValuePickerGuid = "E2FC377F-EDCE-4FD3-B734-D06939E65210";
        private const string DefinedValuePickerMarkup = @"//- Prep configuration settings
{% assign sc-definedtype = definedtypeid | FromCache:'DefinedType' %}
{% assign allowmultiple = allowmultiple | AsBoolean %}
{% assign includeinactive = includeinactive | AsBoolean %}
{% assign displaydescriptions = displaydescriptions | AsBoolean %}

{% assign sc-definedValues = sc-definedtype.DefinedValues | OrderBy:'Order' %}

{% if allowmultiple %}
    {[ checkboxlist label:'{{ label }}' showlabel:'{{ showlabel }}' name:'{{ name }}' isrequired:'{{ isrequired }}' value:'{{ value }}' columns:'4' controltype:'defined-values-picker' id:'{{ id }}' validationmessage:'{{ validationmessage }}' additionalattributes:'{{ additionalattributes}}' ]}

        {% for definedvalue in sc-definedValues %}
            {% if definedvalue.IsActive == true or includeinactive == true  %}
                [[ item value:'{% if valuefield == 'id' %}{{ definedvalue.Id }}{% else %}{{ definedvalue.Guid }}{% endif %}' text:'{% if displaydescriptions == false %}{{ definedvalue.Value }}{% else %} {{ definedvalue.Description }}{% endif %}' ]][[ enditem]]
            {% endif %}
        {% endfor %}

    {[ endcheckboxlist ]}
{% else %}
    {[ dropdown label:'{{ label }}' showlabel:'{{ showlabel }}' name:'{{ name }}' longlistenabled:'{{ longlistenabled }}' value:'{{ value }}' controltype:'defined-value-picker' isrequired:'{{ isrequired }}' id:'{{ id }}' validationmessage:'{{ validationmessage }}' additionalattributes:'{{ additionalattributes}}' ]}

        {% for definedvalue in sc-definedValues %}
            {% if definedvalue.IsActive == true or includeinactive == true  %}
                [[ item value:'{% if valuefield == 'id' %}{{ definedvalue.Id }}{% else %}{{ definedvalue.Guid }}{% endif %}' text:'{% if displaydescriptions == false %}{{ definedvalue.Value }}{% else %} {{ definedvalue.Description }}{% endif %}' ]][[ enditem]]
            {% endif %}
        {% endfor %}

    {[ enddropdown ]}
{% endif %}";
        private const string DefinedValuePickerParameters = @"value^|includeinactive^false|name^definedvalue|isrequired^false|label^Defined Value|validationmessage^Please select a value.|longlistenabled^false|valuefield^id|allowmultiple^false|additionalattributes^|definedtypeid^1|displaydescriptions^false|showlabel^true";
        private const string DefinedValuePickerDocumentation = @"<p>This control allows you to select a value.</p>

<h5>Example Usage</h5>
<pre>{[ definedvaluepicker label:'Connection Type' value:'1,2' definedtype:'768' ]}</pre>

<h5>Parameters</h5>
<p>Below are the parameters for the defined value picker shortcode.</p>
<ul>
    <li><strong>label</strong> - The label to display above the control.</li><li><b>showlabel </b>(true) - Whether to display label.</li>
    <li><strong>name</strong> (definedvalue) - The name for the defined value picker control.</li>
    <li><strong>value</strong> - The ID or Guid of the currently selected value.</li>
    <li><strong>valuefield</strong> (id) - Specifies whether the picker's value should correspond to the defined value's <code>id</code> or <code>guid</code>.</li>
    <li><strong>longlistenabled</strong> (false) -  Enhances the functionality to include a search feature, facilitating swift and efficient selection of the preferred item from the list.<br></li>
    <li><strong>allowmultiple</strong> (false) -  Determines if the selection of multiple values is allowed.</li><li><b>isrequired </b>(false) - Establishes whether making a selection is necessary.</li><li><b>validationmessage </b>(Please select a value.) - Message to display when the value is not valid.</li>
    <li><strong>additionalattributes</strong> -  Additional attributes to include on the input control.</li>
</ul>

<p>
    The above settings enable a wide range of filtering options for the list. Regardless of the filter configurations, the
    current value will consistently be shown.
</p>";
        private const string DefinedValuePickerDescription = "Displays a defined value picker control.";
        private const int DefinedValuePickerTagType = ( int ) TagType.Inline;

        #endregion

        #region Dropdown

        private const string DropdownTagName = "dropdown";
        private const string DropdownGuid = "7BA8EA13-BC09-4075-B0AF-F88AB1203953";
        private const string DropdownMarkup = @"{% assign isrequired = isrequired | AsBoolean %}
{% capture id %}rc-{{ '' | UniqueIdentifier }}{% endcapture %}

{[ rockcontrol id:'{{ id }}' label:'{{ label }}' showlabel:'{{ showlabel }}' controltype:'{{ controltype }}' isrequired:'{{ isrequired }}' validationmessage:'{{ validationmessage }}' ]}
    <select name=""{{ name }}"" id=""{{ id }}"" class=""form-control {% if longlistenabled == 'true' %}chosen-select chosen-select-absolute {% endif %}"" {% if isrequired == true %}required{% endif %} {{ additionalattributes }}>

        {% for item in items %}
            <option value=""{{ item.value }}"" {% if item.value == value %}selected=""selected""{% endif %}>{{ item.text }}</option>
        {% endfor %}

    </select>
{[ endrockcontrol ]}

{% if longlistenabled == 'true'%}
    <script> 
    (function() { 
        var $chosenDropDowns = $('#{{ id }}');
        if ($chosenDropDowns.length) {
            if (document.activeElement && !document.activeElement.nodeType) {
                $('body').trigger('focus');
            }
        
            $chosenDropDowns.chosen({
                width: '100%',
                allow_single_deselect: true,
                placeholder_text_multiple: ' ',
                placeholder_text_single: ' '
            });
        
            $chosenDropDowns.on('chosen:showing_dropdown chosen:hiding_dropdown', function (evt, params) {
                // update the outer modal
                Rock.dialogs.updateModalScrollBar(this);
            });
        
            var $chosenDropDownsAbsolute = $chosenDropDowns.filter('.chosen-select-absolute');
            if ($chosenDropDownsAbsolute.length) {
                $chosenDropDownsAbsolute.on('chosen:showing_dropdown', function (evt, params) {
                    $(this).next('.chosen-container').find('.chosen-drop').css('position', 'relative');
                });
                $chosenDropDownsAbsolute.on('chosen:hiding_dropdown', function (evt, params) {
                    $(this).next('.chosen-container').find('.chosen-drop').css('position', 'absolute');
                });
            }
        }
    })();
    </script>
{% endif %}

";
        private const string DropdownParameters = @"labe^|value^|name^dropdown|^isrequired^false|longlistenabled^false|controltype^rock-drop-down-list|validationmessage^Please select an item.|showlabel^true|additionalattributes^";
        private const string DropdownDocumentation = @"
<p>This control displays a dropdown list with the option to enable support for longer lists.</p>

<h5>Example Usage</h5>
<pre>{[ dropdown label:'Favorite Number' name:'favorite-number' longlistenabled:'true' isrequired:'true' value:'2' ]}
    [[ item value:'1' text:'One' ]][[ enditem]]
    [[ item value:'2' text:'Two' ]][[ enditem]]
    [[ item value:'3' text:'Three' ]][[ enditem]]
{[ enddropdown ]}</pre>


<h5>Parameters</h5>
<p>Below are the parameters for the dropdown shortcode.</p>
<ul>
    <li><strong>label</strong> - The label to display above the control.</li><li><b>showlabel </b>(true) - Whether to display label.</li>
    <li><strong>name</strong> (dropdown) - The name for the drop down control.</li>
    <li><strong>value</strong> - The ID or Guid of the currently selected value.</li>
    <li><strong>longlistenabled</strong> (false) -  Enhances the functionality to include a search feature, facilitating swift and efficient selection of the preferred item from the list.</li>
    <li><strong>controltype</strong> (rock-drop-down-list) -  The type of control. This is appended to the root form-group.</li><li><b>isrequired </b>(false) - Establishes whether making a selection is necessary.
</li><li><b>validationmessage </b>(Please select an item.) - Message to display when the value is not valid.</li>
    <li><strong>additionalattributes</strong> -  Additional attributes to include on the input control.</li>
</ul>

<p>
    The above settings enable a wide range of filtering options for the list. Regardless of the filter configurations, the
    current value will consistently be shown.
</p>";
        private const string DropdownDescription = "Display a dropdown list control.";
        private const int DropdownTagType = ( int ) TagType.Block;

        #endregion

        #region Memo

        private const string MemoTagName = "memo";
        private const string MemoGuid = "94E4E2D0-531C-430D-9353-3A230177D4DD";
        private const string MemoMarkup = @"{[ rockcontrol id:'rc-{{ '' | UniqueIdentifier }}' label:'{{ label }}' showlabel:'{{ showlabel }}' controltype:'rock-text-box' isrequired:'{{ isrequired }}' validationmessage:'{{ validationmessage }}' ]}

    <textarea name=""{{ name }}"" maxlength=""{{ maxlength }}"" rows=""{{ rows }}"" id=""{{ id }}"" class=""form-control"" {{ additionalattributes }} {% if isrequired == true %}required{% endif %}>{{ value }}</textarea>

{[ endrockcontrol ]}
";
        private const string MemoParameters = @"rows^3|label^memo|additionalattributes^|validationmessage^Please insert text|isrequired^false|name^memo|maxlength^|showlabel^true|value^";
        private const string MemoDocumentation = @"<p>This control displays a memo</p>
<h5>Example Usage</h5>
<pre>{[ memo label:'memo' maxlength:'200' name:'textarea' row:'3' value:'Hello Ted!' isrequired:'true' ]}</pre>
<h5>Parameter List</h5>
<p>This control supports the following parameters:</p>
<ul>
<li><strong>label</strong> (memo) - The label to display above the control.</li>
<li><strong>showlabel </strong>(true) - Whether to display the label.</li>
<li><strong>rows</strong> (3) - The number of rows.</li>
<li><strong>maxlength </strong>- The max amount of characters allow</li>
<li><strong>name</strong> (memo) - The name for the memo control.</li>
<li><strong>value </strong>- The text to initially set the control to.</li>
<li><strong>isrequired</strong> (false) - Establishes whether giving a value is necessary.</li>
<li><strong>validationmessage&nbsp;</strong>(Please insert text) - Message to display when the value is not valid.</li>
<li><strong>additionalattributes </strong>- Additional attributes to include on the input control.</li>
</ul>";
        private const string MemoDescription = "Displays a memo.";
        private const int MemoTagType = ( int ) TagType.Inline;

        #endregion

        #region Radio Button List

        private const string RadioButtonListTagName = "radiobuttonlist";
        private const string RadioButtonListGuid = "B1EA9AC3-DC51-444C-953A-565AD38A31DD";
        private const string RadioButtonListMarkup = @"{[ rockcontrol id:'rc-{{ '' | UniqueIdentifier }}' label:'{{ label }}' showlabel:'{{ showlabel }}' type:'{{ type }}' isrequired:'{{ isrequired }}' validationmessage:'{{ validationmessage }}' ]}

    <div class=""controls js-rockradiobuttonlist rockradiobuttonlist  rockradiobuttonlist-horizontal in-columns in-columns-{{ columns }} {% if isrequired %}required{% endif %}"">
        {% for item in items %}

            {% assign itemId = id | Append:'_' | Append:forloop.index0 %}
            {% assign isValueSelected = sc-values | Contains:item.value %}
            <label class=""radio-inline"" for=""{{ itemId }}"">
                <input id=""{{ itemId }}"" type=""radio"" name=""{{ name }}"" value=""{{ item.value }}"" {% if value == item.value %}checked=""checked""{% endif %} {{ additionalattributes }}>
                <span class=""label-text"">{{ item.text }}</span></label>
        {% endfor %}
    </div>

{[ endrockcontrol ]}";
        private const string RadioButtonListParameters = @"label^Radio Button List|value^|name^radiobuttonlist|isrequired^false|type^rock-radio-button-list|validationmessage^Please select at least one item.|columns^1|additionalattributes^|showlabel^true";
        private const string RadioButtonListDocumentation = @"<p>This control displays a radio button list.</p>

<h5>Example Usage</h5>
<pre>{[ radiobuttonlist label:'Favorite Color' name:'favorite-color' isrequired:'true' value:'2' columns:'4' ]}
    [[ item value:'1' text:'Red' ]][[ enditem]]
    [[ item value:'2' text:'Green' ]][[ enditem]]
    [[ item value:'3' text:'Blue' ]][[ enditem]]
    [[ item value:'4' text:'Orange' ]][[ enditem]]
    [[ item value:'5' text:'Yellow' ]][[ enditem]]
{[ endradiobuttonlist ]}</pre>

<h5>Parameters</h5>
<p>Below are the parameters for the radio button list shortcode.</p>
<ul>
    <li><strong>label</strong> - The label to display above the control.</li><li><b>showlabel </b>(true) - Whether to display label.</li>
    <li><strong>name</strong> (radiobuttonlist) - The name for the radio button list control.</li>
    <li><strong>value</strong> - The currently selected values.</li>
    <li><strong>type</strong> (rock-check-box-list) -  The type of control. This is appended to the root form-group.</li>
    <li><strong>columns</strong> -  The number of colums to align the checkboxes to.</li><li><b>isrequired </b>(false) - Establishes whether making a selection is necessary.
</li><li><b>validationmessage </b>(Please select at least one item.) - Message to display when the value is not valid.</li>
    <li><strong>additionalattributes</strong> -  Additional attributes to include on the input control.</li>
</ul>";
        private const string RadioButtonListDescription = "Displays a radio button list control.";
        private const int RadioButtonListTagType = ( int ) TagType.Block;

        #endregion

        #region Range Slider

        private const string RangeSliderTagName = "rangeslider";
        private const string RangeSliderGuid = "2D6B04C4-A54A-4F3F-98E4-657F8AC882D9";
        private const string RangeSliderMarkup = @"{[ rockcontrol id:'rc-{{ '' | UniqueIdentifier }}' label:'{{ label }}' showlabel:'{{ showlabel }}' controltype:'range-slider' isrequired:'{{ isrequired }} validationmessage:'{{ validationmessage }}']}
    <input type=""text"" id='{{ id }}' />
    <script>
        Rock.controls.rangeSlider.initialize(
            {
                controlId: '{{ id }}',
                min: '{{ min }}',
                max: '{{ max }}',
                step: '{{ step }}',
                from: '{{ value }}',
            });
    </script>
{[ endrockcontrol ]}";
        private const string RangeSliderParameters = @"max^100|min^0|isrequired^false|step ^1|label^Range Slider|value^0|validationmessage^Please select a value.|showlabel^true";
        private const string RangeSliderDocumentation = @"<p>This control displays a range slider</p>

<h5>Example Usage</h5>
<pre>{[ rangeslider label:'Range Slider' min:'0' max:'100' step:'.1' value:'10' isrequired:'false']}</pre>

<h5>Parameter List</h5>
<p>This control supports the following parameters:</p>
<ul>
    <li><strong>label</strong> (Range Slider) - The label to display above the control.</li><li><b>showlabel </b>(true) - Whether to display label.</li>
    <li><strong>min</strong> (0) - The minimum amount on slider</li>
    <li><strong>max</strong> (100) - The maximum amount on slider</li>
    <li><strong>step</strong> (1) - The amount of increment at a time</li>
    <li><strong>value</strong>(0) - The amount to initially set the control to.</li><li><b>isrequired </b>(false) - Require the range slider to have a value</li>
    <li><strong>validationmessage </strong>(Please select a value.) - Message to display when the value is not valid.</li>

</ul>";
        private const string RangeSliderDescription = "Displays a range slider control.";
        private const int RangeSliderTagType = ( int ) TagType.Inline;

        #endregion

        #region Rock Control

        private const string RockControlTagName = "rockcontrol";
        private const string RockControlGuid = "4A671223-7165-4710-9FA3-904AC77300F8";
        private const string RockControlMarkup = @"{% assign isrequired = isrequired | AsBoolean %}
{% assign showlabel = showlabel | AsBoolean %}

<div class=""form-group {{ controltype }} {% if isrequired %}required{% endif %}"">
    {% if showlabel %}
        <label class=""control-label"" for=""{{ id }}"">{{ label }}</label>
    {% endif %}
    <div class=""control-wrapper"">
        {{ blockContent }}
    </div>
    {% if isrequired %}
        <span id=""rfv-{{ id }}"" class=""validation-error help-inline"" style=""display:none;"">{{ validationmessage }}</span>
    {% endif %}
</div>";
        private const string RockControlParameters = @"label^Control Label|isrequired^false|validationmessage^Please enter a value.|id^|showlabel^true|controltype^";
        private const string RockControlDocumentation = @"<h4>Example Usage</h4>

<pre>{[ rockcontrol label:'My Control' type:'data-text-box' isrequired:'true' validationmessage:'Please enter a value.' ]}
{[ endrockcontrol ]}
</pre>

<h5>Parameters</h5>
<p>
    The following parameters are available for configuration.
</p>
<ul>
    <li><strong>id</strong> - This is the identifier that will be used for the control. If one is not provided a unique one will be created for you with the pattern of <code>rc-{guid}</code>.</li>
    <li><strong>label</strong> (Campus) - The label to display above the control.</li><li><b>showlabel (</b>true<b>) - </b>Whether or not to display a label.</li>
    <li><strong>controltype</strong> - The type of control. This is appended to the root <code>form-group</code>.</li>
    <li><strong>isrequired</strong> (false) - Establishes whether making a selection is necessary.</li>
    <li><strong>validationmessage</strong> - Message to display when the value is not valid.</li>
</ul>


<h5>Notes on Usage</h5>
<ul>
    <li>This shortcodes provided the<code>id</code> Lava merge field available to the rendering of the inner content.</li>
</ul>";
        private const string RockControlDescription = "Base control shortcode for Helix controls.";
        private const int RockControlTagType = ( int ) TagType.Block;

        #endregion

        #region Text Box

        private const string TextBoxTagName = "textbox";
        private const string TextBoxGuid = "BE829889-A775-4170-9A88-591BB82C86DD";
        private const string TextBoxMarkup = @"{% assign isrequired = isrequired | AsBoolean %}

{[ rockcontrol id:'rc-{{ '' | UniqueIdentifier }}' label:'{{ label }}' showlabel:'{{ showlabel }}' controltype:'rock-text-box' isrequired:'{{ isrequired }}' validationmessage:'{{ validationmessage }}' ]}

    {% if preaddon != empty or postaddon != empty %}<div class=""input-group"">{% endif %}

    {% if preaddon != empty %}
        <span class=""input-group-addon"">{{ preaddon }}</span>
    {% endif %}

    <input name=""{{ name }}"" type=""{{ type}}"" id=""{{ id }}"" class=""form-control {% if size != empty %}input-{{ size }}{% endif %} {% if width != empty %}input-width-{{ width }}{% endif %}"" value=""{{ value }}"" {% if isrequired == true %}required{% endif %} {{ additionalattributes }}>

    {% if postaddon != empty %}
        <span class=""input-group-addon"">{{ postaddon }}</span>
    {% endif %}

    {% if preaddon != empty or postaddon != empty %}</div>{% endif %}

{[ endrockcontrol ]}";
        private const string TextBoxParameters = @"label^Text Area|value^|isrequired^false|name^text|validationmessage^Please enter a value.|width^|preaddon^|postaddon^|size^|type^text|showlabel^true|additionalattributes^";
        private const string TextBoxDocumentation = @"<p>This control displays a textbox.</p>

<h5>Example Usage</h5>
<pre>{[ textbox name:'lastname' label:'Last Name' value:'Decker' ]}</pre>

<h5>Parameter List</h5>
<p>This control supports the following parameters:</p>
<ul>
    <li><strong>label</strong> (Text Area) - The label to display above the control.</li><li><b>showlabel</b>&nbsp;(true) - Whether to display label.</li>
    <li><strong>name</strong> (text) - The name for the text area control.</li>
    <li><strong>type</strong> (text) - The HTML input type (date, datetime-local, email, month, number, password, tel, time, url, week).</li>
    <li><strong>value</strong> - The text to initially set the control to.</li>
    <li><strong>size</strong> - Sets the vertical size of the input. Valid values: xs, sm, md, lg, xl, xxl.</li>
    <li><strong>width</strong> - Adjusts the width of the input. By default the width will grow to the size of the container. Valid values: xs, sm, md, lg.</li>
    <li><strong>preeaddon</strong> - Places an add-on component before the input to provide extra context or functionality (like an icon or text). To place an icon here use the pattern &lt;i class=""fa fa-envelope""&gt;&lt;/i&gt;.</li>
    <li><strong>postaddon</strong> - Places an add-on component after the input to provide extra context or functionality (like an icon or text). To place an icon here use the pattern &lt;i class=""fa fa-envelope""&gt;&lt;/i&gt;.</li><li><b>isrequired </b>(false) - Establishes whether making a selection is necessary.
</li><li><b>validationmessage </b>(Please enter a value.) - Message to display when the value is not valid.</li>
    <li><strong>additionalattributes</strong> -  Additional attributes to include on the input control.</li>
</ul>";
        private const string TextBoxDescription = "Displays a text box control.";
        private const int TextBoxTagType = ( int ) TagType.Inline;

        #endregion

        #endregion
    }
}
