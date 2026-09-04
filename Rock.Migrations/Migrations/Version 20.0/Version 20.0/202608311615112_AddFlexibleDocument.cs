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
    public partial class AddFlexibleDocument : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.FlexibleDocumentModel",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Key = c.String(nullable: false, maxLength: 100),
                        Name = c.String(maxLength: 100),
                        Description = c.String(),
                        Documentation = c.String(),
                        IsSystem = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
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
                .Index(t => t.Key, unique: true)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.FlexibleDocument",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 100),
                        FlexibleDocumentModelId = c.Int(nullable: false),
                        CategoryId = c.Int(),
                        ContentJson = c.String(),
                        IndexedText1 = c.String(maxLength: 100),
                        IndexedText2 = c.String(maxLength: 100),
                        IndexedInteger1 = c.Int(),
                        IndexedDecimal1 = c.Decimal(precision: 18, scale: 4),
                        IndexedDate1 = c.DateTime(),
                        OwnerPersonAliasId = c.Int(),
                        Order = c.Int(),
                        ExpireDateTime = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
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
                .ForeignKey("dbo.FlexibleDocumentModel", t => t.FlexibleDocumentModelId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.OwnerPersonAliasId)
                .Index(t => t.FlexibleDocumentModelId)
                .Index(t => t.CategoryId)
                .Index(t => t.IndexedText1)
                .Index(t => t.IndexedText2)
                .Index(t => t.IndexedInteger1)
                .Index(t => t.IndexedDecimal1)
                .Index(t => t.IndexedDate1)
                .Index(t => t.OwnerPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);

            /*
                8/31/2026 - CLAUDE

                EF6 has no fluent API for check constraints, so the ISJSON guard on
                ContentJson is raw SQL. NULL passes on purpose: an empty document is
                legal; a malformed one is not.

                Reason: ContentJson must always hold well-formed JSON.
            */
            Sql( @"
ALTER TABLE [dbo].[FlexibleDocument] WITH CHECK
    ADD CONSTRAINT [CK_FlexibleDocument_ContentJson_IsJson]
    CHECK ([ContentJson] IS NULL OR ISJSON([ContentJson]) = 1)" );

            RegisterEntityTypes_Up();
        }

        /// <summary>
        /// Registers the EntityTypes this migration adds. Startup EntityType
        /// registration runs after migrations, so anything that needs the rows
        /// before the next application start must register them explicitly here.
        /// </summary>
        private void RegisterEntityTypes_Up()
        {
            RockMigrationHelper.UpdateEntityType(
                "Rock.Model.FlexibleDocumentModel",
                "Flexible Document Model",
                "Rock.Model.FlexibleDocumentModel, Rock, Version=20.0.7.0, Culture=neutral, PublicKeyToken=null",
                true,
                true,
                SystemGuid.EntityType.FLEXIBLE_DOCUMENT_MODEL );

            RockMigrationHelper.UpdateEntityType(
                "Rock.Model.FlexibleDocument",
                "Flexible Document",
                "Rock.Model.FlexibleDocument, Rock, Version=20.0.7.0, Culture=neutral, PublicKeyToken=null",
                true,
                true,
                SystemGuid.EntityType.FLEXIBLE_DOCUMENT );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // The EntityType rows are deliberately left in place: they are recreated
            // by startup registration and deleting them would orphan anything that
            // came to reference them. The ISJSON check constraint is removed with
            // its table below.
            DropForeignKey("dbo.FlexibleDocument", "OwnerPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FlexibleDocument", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FlexibleDocument", "FlexibleDocumentModelId", "dbo.FlexibleDocumentModel");
            DropForeignKey("dbo.FlexibleDocument", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FlexibleDocument", "CategoryId", "dbo.Category");
            DropForeignKey("dbo.FlexibleDocumentModel", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FlexibleDocumentModel", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.FlexibleDocument", new[] { "Guid" });
            DropIndex("dbo.FlexibleDocument", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.FlexibleDocument", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.FlexibleDocument", new[] { "OwnerPersonAliasId" });
            DropIndex("dbo.FlexibleDocument", new[] { "IndexedDate1" });
            DropIndex("dbo.FlexibleDocument", new[] { "IndexedDecimal1" });
            DropIndex("dbo.FlexibleDocument", new[] { "IndexedInteger1" });
            DropIndex("dbo.FlexibleDocument", new[] { "IndexedText2" });
            DropIndex("dbo.FlexibleDocument", new[] { "IndexedText1" });
            DropIndex("dbo.FlexibleDocument", new[] { "CategoryId" });
            DropIndex("dbo.FlexibleDocument", new[] { "FlexibleDocumentModelId" });
            DropIndex("dbo.FlexibleDocumentModel", new[] { "Guid" });
            DropIndex("dbo.FlexibleDocumentModel", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.FlexibleDocumentModel", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.FlexibleDocumentModel", new[] { "Key" });
            DropTable("dbo.FlexibleDocument");
            DropTable("dbo.FlexibleDocumentModel");
        }
    }
}
