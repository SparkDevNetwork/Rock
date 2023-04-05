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
    public partial class AddNotificationMessageModels : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.NotificationMessageType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EntityTypeId = c.Int(nullable: false),
                        Key = c.String(nullable: false, maxLength: 200),
                        ComponentDataJson = c.String(),
                        IsDeletedOnRead = c.Boolean(nullable: false),
                        IsWebSupported = c.Boolean(nullable: false),
                        IsMobileApplicationSupported = c.Boolean(nullable: false),
                        IsTvApplicationSupported = c.Boolean(nullable: false),
                        RelatedWebSiteId = c.Int(),
                        RelatedMobileApplicationSiteId = c.Int(),
                        RelatedTvApplicationSiteId = c.Int(),
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
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.Site", t => t.RelatedMobileApplicationSiteId)
                .ForeignKey("dbo.Site", t => t.RelatedTvApplicationSiteId)
                .ForeignKey("dbo.Site", t => t.RelatedWebSiteId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.Key)
                .Index(t => t.RelatedWebSiteId)
                .Index(t => t.RelatedMobileApplicationSiteId)
                .Index(t => t.RelatedTvApplicationSiteId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);

            CreateTable(
                "dbo.NotificationMessage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NotificationMessageTypeId = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false, maxLength: 200),
                        Key = c.String(maxLength: 200),
                        MessageDateTime = c.DateTime(nullable: false),
                        ExpireDateTime = c.DateTime(nullable: false),
                        Count = c.Int(nullable: false),
                        PersonAliasId = c.Int(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                        ComponentDataJson = c.String(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NotificationMessageType", t => t.NotificationMessageTypeId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId)
                .Index(t => t.NotificationMessageTypeId)
                .Index(t => t.Key)
                .Index(t => t.PersonAliasId)
                .Index(t => t.Guid, unique: true);
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.NotificationMessage", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.NotificationMessage", "NotificationMessageTypeId", "dbo.NotificationMessageType");
            DropForeignKey("dbo.NotificationMessageType", "RelatedWebSiteId", "dbo.Site");
            DropForeignKey("dbo.NotificationMessageType", "RelatedTvApplicationSiteId", "dbo.Site");
            DropForeignKey("dbo.NotificationMessageType", "RelatedMobileApplicationSiteId", "dbo.Site");
            DropForeignKey("dbo.NotificationMessageType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.NotificationMessageType", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.NotificationMessageType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.NotificationMessage", new[] { "Guid" });
            DropIndex("dbo.NotificationMessage", new[] { "PersonAliasId" });
            DropIndex("dbo.NotificationMessage", new[] { "Key" });
            DropIndex("dbo.NotificationMessage", new[] { "NotificationMessageTypeId" });
            DropIndex("dbo.NotificationMessageType", new[] { "Guid" });
            DropIndex("dbo.NotificationMessageType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.NotificationMessageType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.NotificationMessageType", new[] { "RelatedTvApplicationSiteId" });
            DropIndex("dbo.NotificationMessageType", new[] { "RelatedMobileApplicationSiteId" });
            DropIndex("dbo.NotificationMessageType", new[] { "RelatedWebSiteId" });
            DropIndex("dbo.NotificationMessageType", new[] { "Key" });
            DropIndex("dbo.NotificationMessageType", new[] { "EntityTypeId" });
            DropTable("dbo.NotificationMessage");
            DropTable("dbo.NotificationMessageType");
        }
    }
}
