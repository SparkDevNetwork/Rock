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
    /// Creates the three tables that back the Knowledge Base feature
    /// (KnowledgeBase, KnowledgeBaseFolder, KnowledgeBaseDocument), then
    /// registers the six Obsidian block types and four admin pages under
    /// Admin Tools > General Settings. The folder list and document list
    /// blocks are placed on their parent detail pages as secondary blocks
    /// (auto-hidden when the detail block enters edit mode), so no
    /// separate list pages are needed for them.
    /// </summary>
    public partial class AddKnowledgeBaseEntities : Rock.Migrations.RockMigration
    {
        #region Constants

        // Layout and Site Guids used for all knowledge base admin pages.
        private const string FullWidthInternalLayoutGuid = "D65F783D-87A9-4CC9-8110-E83466A0EADB";
        private const string RockRmsInternalSiteGuid = "C2D29296-6A87-47A9-A753-EE4E9159C4C4";

        // Parent page: Admin Tools > General Settings.
        private const string GeneralSettingsPageGuid = "0B213645-FA4E-44A5-8E4C-B2D8EF054985";

        // FieldType used for LinkedPage attributes.
        private const string PageReferenceFieldTypeGuid = "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108";

        // Page Guids. Folder and document lists do not have their own pages —
        // they are embedded as secondary blocks on the parent detail pages.
        private const string KnowledgeBaseListPageGuid = "5A1F8B2C-3E4D-4F5A-9B6C-7D8E0F1A2B30";
        private const string KnowledgeBaseDetailPageGuid = "B7C9F5A1-3D2E-4F8B-9A6C-1E7D8B2A4F31";
        private const string KnowledgeBaseFolderDetailPageGuid = "D4A8B2C7-6E1F-4392-B87A-5C9D3E1F2A53";
        private const string KnowledgeBaseDocumentDetailPageGuid = "2A6E9C4F-1B8D-4E07-83F5-9D7C0E4B2A75";

        // Page Route Guids.
        private const string KnowledgeBaseListRouteGuid = "3B7F0D5A-2C9E-4F18-B406-AE8D1F5C3B86";
        private const string KnowledgeBaseDetailRouteGuid = "4C8A1E6B-3DAF-4029-C517-BF9E2A6D4C97";
        private const string KnowledgeBaseFolderDetailRouteGuid = "6EAC308D-5FC1-424B-E739-D1B04C8F6EB9";
        private const string KnowledgeBaseDocumentDetailRouteGuid = "80CE52AF-71E3-446D-0951-F3D26EA118DB";

        // Block Type Guids — must match the [Rock.SystemGuid.BlockTypeGuid]
        // attributes on the matching block classes in Rock.Blocks/AI/.
        private const string KnowledgeBaseListBlockTypeGuid = "D7E4F0A2-1C3B-4982-B5D6-7C8E9F0A1B23";
        private const string KnowledgeBaseDetailBlockTypeGuid = "8A3B6D1E-9C24-4F5A-B7C8-2E5D9F1A3B40";
        private const string KnowledgeBaseFolderListBlockTypeGuid = "F0A7B2C3-9D4E-4B81-B5A2-3C6D7E8F1A04";
        private const string KnowledgeBaseFolderDetailBlockTypeGuid = "4E7A0C1F-58B6-4D32-A917-6F8D2E5C3B49";
        private const string KnowledgeBaseDocumentListBlockTypeGuid = "2C5B8E0F-7A3D-49B2-86F4-1D9E3C8B5A47";
        private const string KnowledgeBaseDocumentDetailBlockTypeGuid = "8D3C6F9E-2A4B-4D81-B7E5-3F1A8C9D4B62";

        // Block Instance Guids (one block instance per page).
        private const string KnowledgeBaseListBlockGuid = "91DF63B0-82F4-4787-1A60-04E37FB229EC";
        private const string KnowledgeBaseDetailBlockGuid = "A2E074C1-9305-4898-2B71-15F480C33AFD";
        private const string KnowledgeBaseFolderListBlockGuid = "B3F185D2-A416-49A9-3C82-2605910440FE";
        private const string KnowledgeBaseFolderDetailBlockGuid = "C402960E-B527-4A0A-4D93-3716A2151501";
        private const string KnowledgeBaseDocumentListBlockGuid = "D513A71F-C638-4B1B-5EA4-4827B3262612";
        private const string KnowledgeBaseDocumentDetailBlockGuid = "E624B820-D749-4C2C-6FB5-5938C4373723";

        // Block Type Attribute Guids — one per LinkedPage attribute on the
        // three list blocks. The detail blocks navigate via GetParentPageUrl
        // and need no LinkedPage attribute.
        private const string KnowledgeBaseListDetailPageAttributeGuid = "F735C931-E85A-4D3D-70C6-6A49D5484834";
        private const string KnowledgeBaseFolderListDetailPageAttributeGuid = "08469A42-F96B-4E4E-81D7-7B5AE6595945";
        private const string KnowledgeBaseDocumentListDetailPageAttributeGuid = "1957AB53-007C-4F5F-92E8-8C6BF76A6A56";

        #endregion

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Schema first, then platform pieces. Block types must exist
            // before pages reference them via blocks, and pages must exist
            // before blocks land on them.
            CreateKnowledgeBaseTables_Up();
            AddKnowledgeBaseBlockTypes_Up();
            AddKnowledgeBasePages_Up();
            AddKnowledgeBaseBlocks_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Tear down platform pieces first (blocks → pages → block types),
            // then the schema. Reverses the Up ordering exactly.
            AddKnowledgeBaseBlocks_Down();
            AddKnowledgeBasePages_Down();
            AddKnowledgeBaseBlockTypes_Down();
            CreateKnowledgeBaseTables_Down();
        }

        #region Schema

        /// <summary>
        /// Creates the three knowledge base tables and their indexes /
        /// foreign keys.
        /// </summary>
        private void CreateKnowledgeBaseTables_Up()
        {
            // Parent table first so child FKs can reference it.
            CreateTable(
                "dbo.KnowledgeBase",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 250),
                        Description = c.String(),
                        ContextHint = c.String(),
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
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);

            CreateTable(
                "dbo.KnowledgeBaseFolder",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 250),
                        Description = c.String(),
                        ContextHint = c.String(),
                        KnowledgeBaseId = c.Int(nullable: false),
                        SourceEntityTypeId = c.Int(),
                        SourceKey = c.String(maxLength: 250),
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
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.KnowledgeBase", t => t.KnowledgeBaseId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.EntityType", t => t.SourceEntityTypeId)
                .Index(t => t.KnowledgeBaseId)
                .Index(t => t.SourceEntityTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);

            CreateTable(
                "dbo.KnowledgeBaseDocument",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        SourceName = c.String(maxLength: 100),
                        KnowledgeBaseFolderId = c.Int(nullable: false),
                        DocumentKey = c.String(nullable: false, maxLength: 250),
                        Content = c.String(),
                        Url = c.String(maxLength: 500),
                        BinaryFileId = c.Int(),
                        SourceKey = c.String(nullable: false, maxLength: 250),
                        IndexStatus = c.Int(nullable: false),
                        IndexDateTime = c.DateTime(),
                        IsIndexDirty = c.Boolean(nullable: false),
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
                .ForeignKey("dbo.BinaryFile", t => t.BinaryFileId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.KnowledgeBaseFolder", t => t.KnowledgeBaseFolderId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => new { t.KnowledgeBaseFolderId, t.SourceKey }, unique: true)
                .Index(t => t.DocumentKey)
                .Index(t => t.BinaryFileId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
        }

        /// <summary>
        /// Drops the three knowledge base tables, working from the leaf table
        /// back to the root so foreign key constraints can be released.
        /// </summary>
        private void CreateKnowledgeBaseTables_Down()
        {
            // Drop FKs on the child tables first, then their indexes, then the tables
            // themselves, working from the leaf table back to the root.
            DropForeignKey("dbo.KnowledgeBaseDocument", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.KnowledgeBaseDocument", "KnowledgeBaseFolderId", "dbo.KnowledgeBaseFolder");
            DropForeignKey("dbo.KnowledgeBaseDocument", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.KnowledgeBaseDocument", "BinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.KnowledgeBaseFolder", "SourceEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.KnowledgeBaseFolder", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.KnowledgeBaseFolder", "KnowledgeBaseId", "dbo.KnowledgeBase");
            DropForeignKey("dbo.KnowledgeBaseFolder", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.KnowledgeBase", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.KnowledgeBase", "CreatedByPersonAliasId", "dbo.PersonAlias");

            DropIndex("dbo.KnowledgeBaseDocument", new[] { "Guid" });
            DropIndex("dbo.KnowledgeBaseDocument", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.KnowledgeBaseDocument", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.KnowledgeBaseDocument", new[] { "BinaryFileId" });
            DropIndex("dbo.KnowledgeBaseDocument", new[] { "DocumentKey" });
            DropIndex("dbo.KnowledgeBaseDocument", new[] { "KnowledgeBaseFolderId", "SourceKey" });
            DropIndex("dbo.KnowledgeBaseFolder", new[] { "Guid" });
            DropIndex("dbo.KnowledgeBaseFolder", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.KnowledgeBaseFolder", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.KnowledgeBaseFolder", new[] { "SourceEntityTypeId" });
            DropIndex("dbo.KnowledgeBaseFolder", new[] { "KnowledgeBaseId" });
            DropIndex("dbo.KnowledgeBase", new[] { "Guid" });
            DropIndex("dbo.KnowledgeBase", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.KnowledgeBase", new[] { "CreatedByPersonAliasId" });

            DropTable("dbo.KnowledgeBaseDocument");
            DropTable("dbo.KnowledgeBaseFolder");
            DropTable("dbo.KnowledgeBase");
        }

        #endregion

        #region Block Types

        /// <summary>
        /// Registers all six knowledge base Obsidian block types.
        /// </summary>
        private void AddKnowledgeBaseBlockTypes_Up()
        {
            RockMigrationHelper.AddOrUpdateEntityBlockType(
                "Knowledge Base List",
                "Displays the knowledge bases the current person is authorized to view.",
                "Rock.Blocks.AI.KnowledgeBaseList",
                "AI",
                KnowledgeBaseListBlockTypeGuid );

            RockMigrationHelper.AddOrUpdateEntityBlockType(
                "Knowledge Base Detail",
                "Displays the details of a particular knowledge base.",
                "Rock.Blocks.AI.KnowledgeBaseDetail",
                "AI",
                KnowledgeBaseDetailBlockTypeGuid );

            RockMigrationHelper.AddOrUpdateEntityBlockType(
                "Knowledge Base Folder List",
                "Displays the folders that belong to a knowledge base.",
                "Rock.Blocks.AI.KnowledgeBaseFolderList",
                "AI",
                KnowledgeBaseFolderListBlockTypeGuid );

            RockMigrationHelper.AddOrUpdateEntityBlockType(
                "Knowledge Base Folder Detail",
                "Displays the details of a particular knowledge base folder.",
                "Rock.Blocks.AI.KnowledgeBaseFolderDetail",
                "AI",
                KnowledgeBaseFolderDetailBlockTypeGuid );

            RockMigrationHelper.AddOrUpdateEntityBlockType(
                "Knowledge Base Document List",
                "Displays the documents that belong to a knowledge base folder.",
                "Rock.Blocks.AI.KnowledgeBaseDocumentList",
                "AI",
                KnowledgeBaseDocumentListBlockTypeGuid );

            RockMigrationHelper.AddOrUpdateEntityBlockType(
                "Knowledge Base Document Detail",
                "Displays the details of a particular knowledge base document.",
                "Rock.Blocks.AI.KnowledgeBaseDocumentDetail",
                "AI",
                KnowledgeBaseDocumentDetailBlockTypeGuid );

            // LinkedPage attribute on the Knowledge Base List block type.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                KnowledgeBaseListBlockTypeGuid,
                PageReferenceFieldTypeGuid,
                "Detail Page",
                "DetailPage",
                "Detail Page",
                @"The page that will show the knowledge base details.",
                0,
                @"",
                KnowledgeBaseListDetailPageAttributeGuid );

            // LinkedPage attribute on the Knowledge Base Folder List block type.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                KnowledgeBaseFolderListBlockTypeGuid,
                PageReferenceFieldTypeGuid,
                "Detail Page",
                "DetailPage",
                "Detail Page",
                @"The page that will show the knowledge base folder details.",
                0,
                @"",
                KnowledgeBaseFolderListDetailPageAttributeGuid );

            // LinkedPage attribute on the Knowledge Base Document List block type.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                KnowledgeBaseDocumentListBlockTypeGuid,
                PageReferenceFieldTypeGuid,
                "Detail Page",
                "DetailPage",
                "Detail Page",
                @"The page that will show the knowledge base document details.",
                0,
                @"",
                KnowledgeBaseDocumentListDetailPageAttributeGuid );
        }

        /// <summary>
        /// Tears down the six knowledge base block types and their LinkedPage
        /// attributes.
        /// </summary>
        private void AddKnowledgeBaseBlockTypes_Down()
        {
            RockMigrationHelper.DeleteAttribute( KnowledgeBaseDocumentListDetailPageAttributeGuid );
            RockMigrationHelper.DeleteAttribute( KnowledgeBaseFolderListDetailPageAttributeGuid );
            RockMigrationHelper.DeleteAttribute( KnowledgeBaseListDetailPageAttributeGuid );

            RockMigrationHelper.DeleteBlockType( KnowledgeBaseDocumentDetailBlockTypeGuid );
            RockMigrationHelper.DeleteBlockType( KnowledgeBaseDocumentListBlockTypeGuid );
            RockMigrationHelper.DeleteBlockType( KnowledgeBaseFolderDetailBlockTypeGuid );
            RockMigrationHelper.DeleteBlockType( KnowledgeBaseFolderListBlockTypeGuid );
            RockMigrationHelper.DeleteBlockType( KnowledgeBaseDetailBlockTypeGuid );
            RockMigrationHelper.DeleteBlockType( KnowledgeBaseListBlockTypeGuid );
        }

        #endregion

        #region Pages

        /// <summary>
        /// Adds the four knowledge base admin pages and their routes under
        /// Admin Tools &gt; General Settings. Folder and document list blocks
        /// are placed on their parent detail pages (see <see cref="AddKnowledgeBaseBlocks_Up"/>)
        /// so no separate list pages are needed.
        /// </summary>
        private void AddKnowledgeBasePages_Up()
        {
            // Top-level: Knowledge Bases (list).
            RockMigrationHelper.AddPage( true, GeneralSettingsPageGuid, FullWidthInternalLayoutGuid, "Knowledge Bases", "", KnowledgeBaseListPageGuid, "ti ti-books" );
            RockMigrationHelper.AddOrUpdatePageRoute( KnowledgeBaseListPageGuid, "admin/general/knowledge-bases", KnowledgeBaseListRouteGuid );

            // Knowledge Base Detail. Hosts the folder list as a secondary block.
            RockMigrationHelper.AddPage( true, KnowledgeBaseListPageGuid, FullWidthInternalLayoutGuid, "Knowledge Base Detail", "", KnowledgeBaseDetailPageGuid, "ti ti-book-2" );
            RockMigrationHelper.AddOrUpdatePageRoute( KnowledgeBaseDetailPageGuid, "admin/general/knowledge-bases/{KnowledgeBaseId}", KnowledgeBaseDetailRouteGuid );

            // Knowledge Base Folder Detail. Hosts the document list as a secondary block.
            RockMigrationHelper.AddPage( true, KnowledgeBaseDetailPageGuid, FullWidthInternalLayoutGuid, "Knowledge Base Folder Detail", "", KnowledgeBaseFolderDetailPageGuid, "ti ti-folder" );
            RockMigrationHelper.AddOrUpdatePageRoute( KnowledgeBaseFolderDetailPageGuid, "admin/general/knowledge-base-folders/{KnowledgeBaseFolderId}", KnowledgeBaseFolderDetailRouteGuid );

            // Knowledge Base Document Detail.
            RockMigrationHelper.AddPage( true, KnowledgeBaseFolderDetailPageGuid, FullWidthInternalLayoutGuid, "Knowledge Base Document Detail", "", KnowledgeBaseDocumentDetailPageGuid, "ti ti-file-text" );
            RockMigrationHelper.AddOrUpdatePageRoute( KnowledgeBaseDocumentDetailPageGuid, "admin/general/knowledge-base-documents/{KnowledgeBaseDocumentId}", KnowledgeBaseDocumentDetailRouteGuid );
        }

        /// <summary>
        /// Removes the four knowledge base admin pages, working from the leaf
        /// page back to the root so parent FKs can be released.
        /// </summary>
        private void AddKnowledgeBasePages_Down()
        {
            RockMigrationHelper.DeletePage( KnowledgeBaseDocumentDetailPageGuid );
            RockMigrationHelper.DeletePage( KnowledgeBaseFolderDetailPageGuid );
            RockMigrationHelper.DeletePage( KnowledgeBaseDetailPageGuid );
            RockMigrationHelper.DeletePage( KnowledgeBaseListPageGuid );
        }

        #endregion

        #region Blocks

        /// <summary>
        /// Places the six knowledge base block instances and wires the
        /// LinkedPage attribute values that connect each list block to its
        /// detail page.
        ///
        /// Folder list and document list blocks are placed on their parent
        /// detail pages as secondary blocks (order 1, after the detail block
        /// at order 0). Their `[DefaultBlockRole(BlockRole.Secondary)]`
        /// attribute lets the parent detail block auto-hide them while in
        /// edit mode.
        /// </summary>
        private void AddKnowledgeBaseBlocks_Up()
        {
            // Primary blocks — one per page in the Main zone, order 0.
            RockMigrationHelper.AddBlock( true, KnowledgeBaseListPageGuid.AsGuid(), null, RockRmsInternalSiteGuid.AsGuid(), KnowledgeBaseListBlockTypeGuid.AsGuid(), "Knowledge Base List", "Main", @"", @"", 0, KnowledgeBaseListBlockGuid );
            RockMigrationHelper.AddBlock( true, KnowledgeBaseDetailPageGuid.AsGuid(), null, RockRmsInternalSiteGuid.AsGuid(), KnowledgeBaseDetailBlockTypeGuid.AsGuid(), "Knowledge Base Detail", "Main", @"", @"", 0, KnowledgeBaseDetailBlockGuid );
            RockMigrationHelper.AddBlock( true, KnowledgeBaseFolderDetailPageGuid.AsGuid(), null, RockRmsInternalSiteGuid.AsGuid(), KnowledgeBaseFolderDetailBlockTypeGuid.AsGuid(), "Knowledge Base Folder Detail", "Main", @"", @"", 0, KnowledgeBaseFolderDetailBlockGuid );
            RockMigrationHelper.AddBlock( true, KnowledgeBaseDocumentDetailPageGuid.AsGuid(), null, RockRmsInternalSiteGuid.AsGuid(), KnowledgeBaseDocumentDetailBlockTypeGuid.AsGuid(), "Knowledge Base Document Detail", "Main", @"", @"", 0, KnowledgeBaseDocumentDetailBlockGuid );

            // Secondary blocks — placed on the parent detail page below the
            // detail block (order 1) so the detail block can hide them when
            // entering edit mode.
            RockMigrationHelper.AddBlock( true, KnowledgeBaseDetailPageGuid.AsGuid(), null, RockRmsInternalSiteGuid.AsGuid(), KnowledgeBaseFolderListBlockTypeGuid.AsGuid(), "Knowledge Base Folder List", "Main", @"", @"", 1, KnowledgeBaseFolderListBlockGuid );
            RockMigrationHelper.AddBlock( true, KnowledgeBaseFolderDetailPageGuid.AsGuid(), null, RockRmsInternalSiteGuid.AsGuid(), KnowledgeBaseDocumentListBlockTypeGuid.AsGuid(), "Knowledge Base Document List", "Main", @"", @"", 1, KnowledgeBaseDocumentListBlockGuid );

            // List → Detail page wiring through the LinkedPage attribute.
            RockMigrationHelper.AddBlockAttributeValue( true, KnowledgeBaseListBlockGuid, KnowledgeBaseListDetailPageAttributeGuid, KnowledgeBaseDetailPageGuid.ToLower() );
            RockMigrationHelper.AddBlockAttributeValue( true, KnowledgeBaseFolderListBlockGuid, KnowledgeBaseFolderListDetailPageAttributeGuid, KnowledgeBaseFolderDetailPageGuid.ToLower() );
            RockMigrationHelper.AddBlockAttributeValue( true, KnowledgeBaseDocumentListBlockGuid, KnowledgeBaseDocumentListDetailPageAttributeGuid, KnowledgeBaseDocumentDetailPageGuid.ToLower() );
        }

        /// <summary>
        /// Removes the six knowledge base block instances.
        /// </summary>
        private void AddKnowledgeBaseBlocks_Down()
        {
            RockMigrationHelper.DeleteBlock( KnowledgeBaseDocumentDetailBlockGuid );
            RockMigrationHelper.DeleteBlock( KnowledgeBaseDocumentListBlockGuid );
            RockMigrationHelper.DeleteBlock( KnowledgeBaseFolderDetailBlockGuid );
            RockMigrationHelper.DeleteBlock( KnowledgeBaseFolderListBlockGuid );
            RockMigrationHelper.DeleteBlock( KnowledgeBaseDetailBlockGuid );
            RockMigrationHelper.DeleteBlock( KnowledgeBaseListBlockGuid );
        }

        #endregion
    }
}
