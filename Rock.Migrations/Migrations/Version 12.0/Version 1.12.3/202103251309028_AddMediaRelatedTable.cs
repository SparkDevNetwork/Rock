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
    public partial class AddMediaRelatedTable : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.MediaAccount",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        LastRefreshDateTime = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                        ComponentEntityTypeId = c.Int(nullable: false),
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
                .ForeignKey("dbo.EntityType", t => t.ComponentEntityTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.ComponentEntityTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.MediaFolder",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MediaAccountId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 200),
                        Description = c.String(),
                        IsPublic = c.Boolean(),
                        SourceData = c.String(),
                        MetricData = c.String(),
                        SourceKey = c.String(maxLength: 60),
                        IsContentChannelSyncEnabled = c.Boolean(nullable: false),
                        ContentChannelId = c.Int(),
                        Status = c.Int(),
                        ContentChannelAttributeId = c.Int(),
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
                .ForeignKey("dbo.ContentChannel", t => t.ContentChannelId)
                .ForeignKey("dbo.Attribute", t => t.ContentChannelAttributeId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.MediaAccount", t => t.MediaAccountId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.MediaAccountId)
                .Index(t => t.ContentChannelId)
                .Index(t => t.ContentChannelAttributeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.MediaElement",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MediaFolderId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 200),
                        Description = c.String(),
                        Duration = c.Decimal(precision: 18, scale: 2),
                        SourceCreatedDateTime = c.DateTime(),
                        SourceModifiedDateTime = c.DateTime(),
                        SourceData = c.String(),
                        SourceMetric = c.String(),
                        SourceKey = c.String(maxLength: 60),
                        ThumbnailData = c.String(),
                        MediaElementData = c.String(),
                        DownloadData = c.String(),
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
                .ForeignKey("dbo.MediaFolder", t => t.MediaFolderId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.MediaFolderId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.MediaAccount", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MediaFolder", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MediaElement", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MediaElement", "MediaFolderId", "dbo.MediaFolder");
            DropForeignKey("dbo.MediaElement", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MediaFolder", "MediaAccountId", "dbo.MediaAccount");
            DropForeignKey("dbo.MediaFolder", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MediaFolder", "ContentChannelAttributeId", "dbo.Attribute");
            DropForeignKey("dbo.MediaFolder", "ContentChannelId", "dbo.ContentChannel");
            DropForeignKey("dbo.MediaAccount", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MediaAccount", "ComponentEntityTypeId", "dbo.EntityType");
            DropIndex("dbo.MediaElement", new[] { "Guid" });
            DropIndex("dbo.MediaElement", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.MediaElement", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.MediaElement", new[] { "MediaFolderId" });
            DropIndex("dbo.MediaFolder", new[] { "Guid" });
            DropIndex("dbo.MediaFolder", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.MediaFolder", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.MediaFolder", new[] { "ContentChannelAttributeId" });
            DropIndex("dbo.MediaFolder", new[] { "ContentChannelId" });
            DropIndex("dbo.MediaFolder", new[] { "MediaAccountId" });
            DropIndex("dbo.MediaAccount", new[] { "Guid" });
            DropIndex("dbo.MediaAccount", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.MediaAccount", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.MediaAccount", new[] { "ComponentEntityTypeId" });
            DropTable("dbo.MediaElement");
            DropTable("dbo.MediaFolder");
            DropTable("dbo.MediaAccount");
        }
    }
}
