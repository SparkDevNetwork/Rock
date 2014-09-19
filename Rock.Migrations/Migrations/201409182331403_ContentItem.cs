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
    public partial class ContentItem : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.ContentChannel",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ContentTypeId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        IconCssClass = c.String(maxLength: 100),
                        RequiresApproval = c.Boolean(nullable: false),
                        EnableRss = c.Boolean(nullable: false),
                        ChannelUrl = c.String(),
                        ItemUrl = c.String(),
                        TimeToLive = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ContentType", t => t.ContentTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.ContentTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.ContentType",
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.ContentItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ContentChannelId = c.Int(nullable: false),
                        ContentTypeId = c.Int(nullable: false),
                        Title = c.String(maxLength: 200),
                        Content = c.String(),
                        Priority = c.Int(nullable: false),
                        Status = c.Byte(nullable: false),
                        ApprovedByPersonAliasId = c.Int(),
                        ApprovedDateTime = c.DateTime(),
                        StartDateTime = c.DateTime(nullable: false),
                        ExpireDateTime = c.DateTime(),
                        Permalink = c.String(maxLength: 200),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.ApprovedByPersonAliasId)
                .ForeignKey("dbo.ContentChannel", t => t.ContentChannelId)
                .ForeignKey("dbo.ContentType", t => t.ContentTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.ContentChannelId)
                .Index(t => t.ContentTypeId)
                .Index(t => t.ApprovedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AlterColumn("dbo.WorkflowAction", "FormAction", c => c.String(maxLength: 200));
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.ContentChannel", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ContentItem", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ContentItem", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ContentItem", "ContentTypeId", "dbo.ContentType");
            DropForeignKey("dbo.ContentItem", "ContentChannelId", "dbo.ContentChannel");
            DropForeignKey("dbo.ContentItem", "ApprovedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ContentChannel", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ContentChannel", "ContentTypeId", "dbo.ContentType");
            DropForeignKey("dbo.ContentType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ContentType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.ContentItem", new[] { "Guid" });
            DropIndex("dbo.ContentItem", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ContentItem", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ContentItem", new[] { "ApprovedByPersonAliasId" });
            DropIndex("dbo.ContentItem", new[] { "ContentTypeId" });
            DropIndex("dbo.ContentItem", new[] { "ContentChannelId" });
            DropIndex("dbo.ContentType", new[] { "Guid" });
            DropIndex("dbo.ContentType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ContentType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ContentChannel", new[] { "Guid" });
            DropIndex("dbo.ContentChannel", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ContentChannel", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ContentChannel", new[] { "ContentTypeId" });
            AlterColumn("dbo.WorkflowAction", "FormAction", c => c.String(maxLength: 20));
            DropTable("dbo.ContentItem");
            DropTable("dbo.ContentType");
            DropTable("dbo.ContentChannel");
        }
    }
}
