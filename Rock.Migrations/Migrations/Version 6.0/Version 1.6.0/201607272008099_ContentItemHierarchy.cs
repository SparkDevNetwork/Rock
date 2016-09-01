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
    public partial class ContentItemHierarchy : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.ContentChannelItemAssociation",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ContentChannelItemId = c.Int(nullable: false),
                        ChildContentChannelItemId = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
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
                .ForeignKey("dbo.ContentChannelItem", t => t.ChildContentChannelItemId)
                .ForeignKey("dbo.ContentChannelItem", t => t.ContentChannelItemId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.ContentChannelItemId)
                .Index(t => t.ChildContentChannelItemId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId)
                .Index(t => t.ForeignGuid)
                .Index(t => t.ForeignKey);
            
            CreateTable(
                "dbo.ContentChannelAssociation",
                c => new
                    {
                        ContentChannelId = c.Int(nullable: false),
                        ChildContentChannelId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ContentChannelId, t.ChildContentChannelId })
                .ForeignKey("dbo.ContentChannel", t => t.ContentChannelId)
                .ForeignKey("dbo.ContentChannel", t => t.ChildContentChannelId)
                .Index(t => t.ContentChannelId)
                .Index(t => t.ChildContentChannelId);
            
            AddColumn("dbo.ContentChannelItem", "Order", c => c.Int(nullable: false));
            AddColumn("dbo.ContentChannel", "ItemsManuallyOrdered", c => c.Boolean(nullable: false));
            AddColumn("dbo.ContentChannel", "ChildItemsManuallyOrdered", c => c.Boolean(nullable: false));

            Sql( @"
CREATE UNIQUE NONCLUSTERED INDEX [IX_ContentChannelItemAssociation] ON [dbo].[ContentChannelItemAssociation]
(
	[ContentChannelItemId] ASC,
	[ChildContentChannelItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
" );

            Sql( @"
    UPDATE [ContentChannel] SET [ItemsManuallyOrdered] = 0, [ChildItemsManuallyOrdered] = 0
    UPDATE [ContentChannelItem] SET [Order] = 0
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.ContentChannelAssociation", "ChildContentChannelId", "dbo.ContentChannel");
            DropForeignKey("dbo.ContentChannelAssociation", "ContentChannelId", "dbo.ContentChannel");
            DropForeignKey("dbo.ContentChannelItemAssociation", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ContentChannelItemAssociation", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ContentChannelItemAssociation", "ContentChannelItemId", "dbo.ContentChannelItem");
            DropForeignKey("dbo.ContentChannelItemAssociation", "ChildContentChannelItemId", "dbo.ContentChannelItem");
            DropIndex("dbo.ContentChannelAssociation", new[] { "ChildContentChannelId" });
            DropIndex("dbo.ContentChannelAssociation", new[] { "ContentChannelId" });
            DropIndex("dbo.ContentChannelItemAssociation", new[] { "ForeignKey" });
            DropIndex("dbo.ContentChannelItemAssociation", new[] { "ForeignGuid" });
            DropIndex("dbo.ContentChannelItemAssociation", new[] { "ForeignId" });
            DropIndex("dbo.ContentChannelItemAssociation", new[] { "Guid" });
            DropIndex("dbo.ContentChannelItemAssociation", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ContentChannelItemAssociation", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ContentChannelItemAssociation", new[] { "ChildContentChannelItemId" });
            DropIndex("dbo.ContentChannelItemAssociation", new[] { "ContentChannelItemId" });
            DropColumn("dbo.ContentChannel", "ChildItemsManuallyOrdered");
            DropColumn("dbo.ContentChannel", "ItemsManuallyOrdered");
            DropColumn("dbo.ContentChannelItem", "Order");
            DropTable("dbo.ContentChannelAssociation");
            DropTable("dbo.ContentChannelItemAssociation");
        }
    }
}
