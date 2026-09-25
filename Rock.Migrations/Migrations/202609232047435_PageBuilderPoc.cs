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
    /// Adds the module tables and sample module types, and the Page Builder layouts, block, the internal page that hosts it, and the sample page it edits.
    /// </summary>
    public partial class PageBuilderPoc : Rock.Migrations.RockMigration
    {
        private const string FullScreenLayoutGuid = "96B64C3E-D5CD-4853-8686-C5481AF45CA3";
        private const string PageBuilderLayoutGuid = "680D62E2-8018-4187-838F-7593CFB3F7CA";
        private const string PageBuilderPageGuid = "2540C899-5B4E-452B-865A-1E43A8BEAC2B";
        private const string PageBuilderPageRouteGuid = "B59DEF98-8C20-47EB-8E93-96300909ABE2";
        private const string PageBuilderSamplePageGuid = "83FC4394-ED09-461D-B865-68C1F5AFB34E";
        private const string PageBuilderEntityTypeGuid = "5968C4FC-2508-4457-B1D6-E3AC6711C9F9";
        private const string PageBuilderBlockTypeGuid = "89B56E5A-5A7D-44E6-85A0-49F101F814BE";
        private const string PageBuilderBlockGuid = "55C62D64-DCF5-4943-B0F2-74D9CC3CFEF2";

        private const string AccordionModuleTypeGuid = "25A85AE1-DD08-4E4E-86AA-739839F8C9A6";
        private const string BillboardModuleTypeGuid = "4FA807A9-7B74-401E-B683-3ED22F16AF66";
        private const string CardModuleTypeGuid = "02D0B6B3-BD8D-4D13-818C-FCF92CFDB564";
        private const string ContentModuleTypeGuid = "9B6A6E3A-D2C2-4595-A77B-3CE2EF1D18D2";
        private const string VideoModuleTypeGuid = "9C484786-EE1C-4634-B985-CD0AFB5D1526";

        private const string AccordionTitleAttributeGuid = "AF30ED15-684C-4C55-8258-11452E3F27A1";
        private const string BillboardTitleAttributeGuid = "F4D823F6-F94D-43BE-8F55-647E24B6CC17";
        private const string BillboardTextAttributeGuid = "5EF03A0E-C3D3-457A-A31A-1D91620BA3E5";
        private const string CardTitleAttributeGuid = "0B1DBB97-B635-4A5E-AF44-D3E389BFDA1F";
        private const string CardTextAttributeGuid = "C9704E9D-1BC6-45AC-AFE4-E7B7FC738259";
        private const string CardButtonTextAttributeGuid = "AA3FE8BF-53E7-4D0E-BFC4-F74B14613A17";
        private const string ContentTitleAttributeGuid = "0D5A4C08-867C-46BE-9186-653D9D9101D4";
        private const string ContentTextAttributeGuid = "DEC2A87E-09CF-4B46-A292-5B0D8AA999B4";
        private const string VideoTitleAttributeGuid = "F7F6137D-D14E-4CFC-9425-2149ECCCE3B9";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.ModuleInstance",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        ModuleTypeId = c.Int(nullable: false),
                        IsShareable = c.Boolean(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.ModuleType", t => t.ModuleTypeId)
                .Index(t => t.ModuleTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);

            CreateTable(
                "dbo.ModuleType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        IconCssClass = c.String(maxLength: 100),
                        CategoryId = c.Int(),
                        AreItemsSupported = c.Boolean(nullable: false),
                        ItemTerm = c.String(maxLength: 100),
                        IsPersonalizationEnabled = c.Boolean(nullable: false),
                        WebLavaTemplate = c.String(),
                        MobileLavaTemplate = c.String(),
                        AdditionalSettingsJson = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CategoryId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);

            RegisterModuleEntityTypes_Up();
            AddModuleTypes_Up();
            AddModuleInstanceAttributes_Up();
            AddPageBuilderLayout_Up();
            AddPageBuilderPages_Up();
            AddPageBuilderBlock_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddPageBuilderBlock_Down();
            AddPageBuilderPages_Down();
            AddPageBuilderLayout_Down();
            AddModuleInstanceAttributes_Down();

            DropForeignKey("dbo.ModuleInstance", "ModuleTypeId", "dbo.ModuleType");
            DropForeignKey("dbo.ModuleType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ModuleType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ModuleType", "CategoryId", "dbo.Category");
            DropForeignKey("dbo.ModuleInstance", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ModuleInstance", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.ModuleType", new[] { "Guid" });
            DropIndex("dbo.ModuleType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ModuleType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ModuleType", new[] { "CategoryId" });
            DropIndex("dbo.ModuleInstance", new[] { "Guid" });
            DropIndex("dbo.ModuleInstance", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ModuleInstance", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ModuleInstance", new[] { "ModuleTypeId" });
            DropTable("dbo.ModuleType");
            DropTable("dbo.ModuleInstance");
        }

        #region Private Methods

        /// <summary>
        /// Registers the ModuleType and ModuleInstance entity types.
        /// </summary>
        private void RegisterModuleEntityTypes_Up()
        {
            // Startup registers entity types after migrations run, and the attributes below need ModuleInstance's.
            RockMigrationHelper.UpdateEntityType(
                "Rock.Model.ModuleType",
                "Module Type",
                "Rock.Model.ModuleType, Rock, Version=21.0.1.0, Culture=neutral, PublicKeyToken=null",
                true,
                true,
                SystemGuid.EntityType.MODULE_TYPE );

            RockMigrationHelper.UpdateEntityType(
                "Rock.Model.ModuleInstance",
                "Module Instance",
                "Rock.Model.ModuleInstance, Rock, Version=21.0.1.0, Culture=neutral, PublicKeyToken=null",
                true,
                true,
                SystemGuid.EntityType.MODULE_INSTANCE );
        }

        /// <summary>
        /// Adds the sample module types the Page Builder offers.
        /// </summary>
        private void AddModuleTypes_Up()
        {
            AddModuleType( AccordionModuleTypeGuid, "Accordion", "ti ti-layout-navbar-collapse", @"<h3>{{ ModuleInstance | Attribute:'Title' }}</h3>
<details><summary>An accordion title goes here?</summary><p>Accordion content goes here.</p></details>
<details><summary>An accordion title goes here?</summary><p>Accordion content goes here.</p></details>
<details><summary>An accordion title goes here?</summary><p>Accordion content goes here.</p></details>" );

            AddModuleType( BillboardModuleTypeGuid, "Billboard", "ti ti-presentation",
                @"<div class=""jumbotron""><h1>{{ ModuleInstance | Attribute:'Title' }}</h1><div>{{ ModuleInstance | Attribute:'Text' }}</div></div>" );

            AddModuleType( CardModuleTypeGuid, "Card", "ti ti-id",
                @"<div class=""panel panel-default""><div class=""panel-body""><h4>{{ ModuleInstance | Attribute:'Title' }}</h4><div>{{ ModuleInstance | Attribute:'Text' }}</div><a class=""btn btn-default"" href=""#"">{{ ModuleInstance | Attribute:'ButtonText' }}</a></div></div>" );

            AddModuleType( ContentModuleTypeGuid, "Content", "ti ti-file-text",
                @"<h2>{{ ModuleInstance | Attribute:'Title' }}</h2><div>{{ ModuleInstance | Attribute:'Text' }}</div>" );

            AddModuleType( VideoModuleTypeGuid, "Video", "ti ti-player-play",
                @"<div class=""well text-center""><i class=""ti ti-player-play""></i> {{ ModuleInstance | Attribute:'Title' }}</div>" );
        }

        /// <summary>
        /// Adds a module type if it does not already exist.
        /// </summary>
        /// <param name="guid">The module type's unique identifier.</param>
        /// <param name="name">The module type's name.</param>
        /// <param name="iconCssClass">The module type's icon CSS class.</param>
        /// <param name="webLavaTemplate">The Lava template that renders the module type on the web.</param>
        private void AddModuleType( string guid, string name, string iconCssClass, string webLavaTemplate )
        {
            Sql( $@"
IF NOT EXISTS ( SELECT 1 FROM [ModuleType] WHERE [Guid] = '{guid}' )
BEGIN
    INSERT INTO [ModuleType] ( [Name], [IconCssClass], [AreItemsSupported], [IsPersonalizationEnabled], [WebLavaTemplate], [Guid] )
    VALUES ( N'{name}', N'{iconCssClass}', 0, 0, N'{webLavaTemplate.Replace( "'", "''" )}', '{guid}' );
END" );
        }

        /// <summary>
        /// Adds each sample module type's settings as ModuleInstance attributes qualified by that module type.
        /// </summary>
        private void AddModuleInstanceAttributes_Up()
        {
            AddModuleInstanceAttribute( AccordionModuleTypeGuid, SystemGuid.FieldType.TEXT, "Title", "Title", 0, "Accordion", AccordionTitleAttributeGuid );

            AddModuleInstanceAttribute( BillboardModuleTypeGuid, SystemGuid.FieldType.TEXT, "Title", "Title", 0, "Billboard", BillboardTitleAttributeGuid );
            AddModuleInstanceAttribute( BillboardModuleTypeGuid, SystemGuid.FieldType.HTML, "Text", "Text", 1, "<p>A large banner that introduces the page.</p>", BillboardTextAttributeGuid );

            AddModuleInstanceAttribute( CardModuleTypeGuid, SystemGuid.FieldType.TEXT, "Title", "Title", 0, "Card", CardTitleAttributeGuid );
            AddModuleInstanceAttribute( CardModuleTypeGuid, SystemGuid.FieldType.HTML, "Text", "Text", 1, "<p>A short summary with a link to more.</p>", CardTextAttributeGuid );
            AddModuleInstanceAttribute( CardModuleTypeGuid, SystemGuid.FieldType.TEXT, "Button Text", "ButtonText", 2, "View details", CardButtonTextAttributeGuid );

            AddModuleInstanceAttribute( ContentModuleTypeGuid, SystemGuid.FieldType.TEXT, "Title", "Title", 0, "Content", ContentTitleAttributeGuid );
            AddModuleInstanceAttribute( ContentModuleTypeGuid, SystemGuid.FieldType.HTML, "Text", "Text", 1, "<p>A block of formatted text.</p>", ContentTextAttributeGuid );

            AddModuleInstanceAttribute( VideoModuleTypeGuid, SystemGuid.FieldType.TEXT, "Title", "Title", 0, "Video", VideoTitleAttributeGuid );
        }

        /*
            09/23/26 - JMH

            A module type's settings are ModuleInstance attributes qualified by ModuleTypeId, which is
            how every instance of a type gets that type's settings. The qualifier value is the module
            type's Id, which is only known once its row exists, so the attribute is inserted with SQL
            that looks the Id up by Guid rather than with a migration helper that takes it as a literal.

            Reason: The qualifier value is an identity Id that has to be looked up at migration time.
        */
        private void AddModuleInstanceAttribute( string moduleTypeGuid, string fieldTypeGuid, string name, string key, int order, string defaultValue, string guid )
        {
            Sql( $@"
DECLARE @EntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '{SystemGuid.EntityType.MODULE_INSTANCE}' );
DECLARE @FieldTypeId INT = ( SELECT [Id] FROM [FieldType] WHERE [Guid] = '{fieldTypeGuid}' );
DECLARE @ModuleTypeId NVARCHAR(200) = CAST( ( SELECT [Id] FROM [ModuleType] WHERE [Guid] = '{moduleTypeGuid}' ) AS NVARCHAR(200) );

IF NOT EXISTS ( SELECT 1 FROM [Attribute] WHERE [Guid] = '{guid}' )
BEGIN
    INSERT INTO [Attribute] ( [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid] )
    VALUES ( 1, @FieldTypeId, @EntityTypeId, 'ModuleTypeId', @ModuleTypeId, '{key}', N'{name}', N'', {order}, 0, N'{defaultValue.Replace( "'", "''" )}', 0, 0, '{guid}' );
END" );
        }

        /// <summary>
        /// Removes the sample module types' ModuleInstance attributes, along with any values saved for them.
        /// </summary>
        private void AddModuleInstanceAttributes_Down()
        {
            RockMigrationHelper.DeleteAttribute( VideoTitleAttributeGuid );
            RockMigrationHelper.DeleteAttribute( ContentTextAttributeGuid );
            RockMigrationHelper.DeleteAttribute( ContentTitleAttributeGuid );
            RockMigrationHelper.DeleteAttribute( CardButtonTextAttributeGuid );
            RockMigrationHelper.DeleteAttribute( CardTextAttributeGuid );
            RockMigrationHelper.DeleteAttribute( CardTitleAttributeGuid );
            RockMigrationHelper.DeleteAttribute( BillboardTextAttributeGuid );
            RockMigrationHelper.DeleteAttribute( BillboardTitleAttributeGuid );
            RockMigrationHelper.DeleteAttribute( AccordionTitleAttributeGuid );
        }

        /// <summary>
        /// Adds the internal site layouts for the builder's full screen worksurface and for the page it edits.
        /// </summary>
        private void AddPageBuilderLayout_Up()
        {
            RockMigrationHelper.AddLayout( SystemGuid.Site.SITE_ROCK_INTERNAL, "FullScreen", "Full Screen", "Fills the browser window with the Main zone and no site navigation.", FullScreenLayoutGuid );
            RockMigrationHelper.AddLayout( SystemGuid.Site.SITE_ROCK_INTERNAL, "PageBuilder", "Page Builder", "", PageBuilderLayoutGuid );
        }

        /// <summary>
        /// Removes the Page Builder and Full Screen layouts.
        /// </summary>
        private void AddPageBuilderLayout_Down()
        {
            RockMigrationHelper.DeleteLayout( PageBuilderLayoutGuid );
            RockMigrationHelper.DeleteLayout( FullScreenLayoutGuid );
        }

        /// <summary>
        /// Adds the Page Builder page under CMS Configuration and the sample page it edits.
        /// </summary>
        private void AddPageBuilderPages_Up()
        {
            RockMigrationHelper.AddPage( true, SystemGuid.Page.CMS_CONFIGURATION, FullScreenLayoutGuid, "Page Builder", "", PageBuilderPageGuid, "ti ti-layout" );
            RockMigrationHelper.AddOrUpdatePageRoute( PageBuilderPageGuid, "admin/cms/page-builder", PageBuilderPageRouteGuid );

            RockMigrationHelper.AddPage( true, PageBuilderPageGuid, PageBuilderLayoutGuid, "Page Builder Sample", "", PageBuilderSamplePageGuid );

            // The sample page is only reached through the builder's iframe, so keep it out of navigation.
            Sql( $"UPDATE [Page] SET [DisplayInNavWhen] = {( int ) Rock.Model.DisplayInNavWhen.Never} WHERE [Guid] = '{PageBuilderSamplePageGuid}';" );
        }

        /// <summary>
        /// Removes the Page Builder sample page, route, and page.
        /// </summary>
        private void AddPageBuilderPages_Down()
        {
            RockMigrationHelper.DeletePage( PageBuilderSamplePageGuid );
            RockMigrationHelper.DeletePageRoute( PageBuilderPageRouteGuid );
            RockMigrationHelper.DeletePage( PageBuilderPageGuid );
        }

        /// <summary>
        /// Registers the Page Builder block type and places it on the Page Builder page.
        /// </summary>
        private void AddPageBuilderBlock_Up()
        {
            // Startup registers block entity types after migrations run, and AddOrUpdateEntityBlockType
            // does nothing without one, so the entity type is registered here first.
            RockMigrationHelper.UpdateEntityType(
                "Rock.Blocks.Cms.PageBuilder",
                "Page Builder",
                "Rock.Blocks.Cms.PageBuilder, Rock.Blocks, Version=21.0.1.0, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                PageBuilderEntityTypeGuid );

            RockMigrationHelper.AddOrUpdateEntityBlockType(
                "Page Builder",
                "Composes a page by dragging modules into its builder-enabled zones.",
                "Rock.Blocks.Cms.PageBuilder",
                "CMS",
                PageBuilderBlockTypeGuid );

            RockMigrationHelper.AddBlock( true, PageBuilderPageGuid, "", PageBuilderBlockTypeGuid, "Page Builder", "Main", "", "", 0, PageBuilderBlockGuid );
        }

        /// <summary>
        /// Removes the Page Builder block and block type.
        /// </summary>
        private void AddPageBuilderBlock_Down()
        {
            RockMigrationHelper.DeleteBlock( PageBuilderBlockGuid );
            RockMigrationHelper.DeleteBlockType( PageBuilderBlockTypeGuid );
        }

        #endregion
    }
}
