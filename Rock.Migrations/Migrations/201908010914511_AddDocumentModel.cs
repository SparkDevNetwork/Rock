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
    public partial class AddDocumentModel : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.Document",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        DocumentTypeId = c.Int(nullable: false),
                        EntityId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                        BinaryFileId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BinaryFile", t => t.BinaryFileId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.DocumentType", t => t.DocumentTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.DocumentTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.BinaryFileId);
            
            CreateTable(
                "dbo.DocumentType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        EntityTypeId = c.Int(nullable: false),
                        EntityTypeQualifierColumn = c.String(maxLength: 50),
                        EntityTypeQualifierValue = c.String(maxLength: 200),
                        UserSelectable = c.Boolean(nullable: false),
                        IconCssClass = c.String(maxLength: 100),
                        Order = c.Int(nullable: false),
                        BinaryFileTypeId = c.Int(nullable: false),
                        DefaultDocumentNameTemplate = c.String(),
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
                .ForeignKey("dbo.BinaryFileType", t => t.BinaryFileTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.BinaryFileTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Document", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Document", "DocumentTypeId", "dbo.DocumentType");
            DropForeignKey("dbo.DocumentType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.DocumentType", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.DocumentType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.DocumentType", "BinaryFileTypeId", "dbo.BinaryFileType");
            DropForeignKey("dbo.Document", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Document", "BinaryFileId", "dbo.BinaryFile");
            DropIndex("dbo.DocumentType", new[] { "Guid" });
            DropIndex("dbo.DocumentType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.DocumentType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.DocumentType", new[] { "BinaryFileTypeId" });
            DropIndex("dbo.DocumentType", new[] { "EntityTypeId" });
            DropIndex("dbo.Document", new[] { "BinaryFileId" });
            DropIndex("dbo.Document", new[] { "Guid" });
            DropIndex("dbo.Document", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.Document", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.Document", new[] { "DocumentTypeId" });
            DropTable("dbo.DocumentType");
            DropTable("dbo.Document");
        }
    }
}
