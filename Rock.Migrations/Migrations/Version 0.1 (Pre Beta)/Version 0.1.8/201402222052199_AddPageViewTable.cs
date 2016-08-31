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
    public partial class AddPageViewTable : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.PageView",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PageId = c.Int(nullable: false),
                        SiteId = c.Int(),
                        PersonAliasId = c.Int(),
                        DateTimeViewed = c.DateTime(nullable: false),
                        UserAgent = c.String(maxLength: 500),
                        ClientType = c.String(maxLength: 25),
                        QueryString = c.String(maxLength: 500),
                        SessionId = c.String(maxLength: 25),
                        IpAddress = c.String(maxLength: 45),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Page", t => t.PageId)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId, cascadeDelete: true)
                .ForeignKey("dbo.Site", t => t.SiteId)
                .Index(t => t.PageId)
                .Index(t => t.PersonAliasId)
                .Index(t => t.SiteId);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.PageView", "SiteId", "dbo.Site");
            DropForeignKey("dbo.PageView", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PageView", "PageId", "dbo.Page");
            DropIndex("dbo.PageView", new[] { "SiteId" });
            DropIndex("dbo.PageView", new[] { "PersonAliasId" });
            DropIndex("dbo.PageView", new[] { "PageId" });
            DropTable("dbo.PageView");
        }
    }
}
