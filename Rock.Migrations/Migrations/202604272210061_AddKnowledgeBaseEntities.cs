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
    /// Creates the three tables that back the Knowledge Base feature: KnowledgeBase,
    /// KnowledgeBaseFolder, and KnowledgeBaseDocument.
    /// </summary>
    public partial class AddKnowledgeBaseEntities : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
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
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
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
    }
}
