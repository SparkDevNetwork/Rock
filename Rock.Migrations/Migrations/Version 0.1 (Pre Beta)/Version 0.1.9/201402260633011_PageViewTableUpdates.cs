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
    public partial class PageViewTableUpdates : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"DELETE FROM [PageView]" );
            
            DropForeignKey("dbo.PageView", "PageId", "dbo.Page");
            DropForeignKey("dbo.PageView", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PageView", "SiteId", "dbo.Site");
            DropIndex("dbo.PageView", new[] { "PageId" });
            DropIndex("dbo.PageView", new[] { "PersonAliasId" });
            DropIndex("dbo.PageView", new[] { "SiteId" });
            AddColumn("dbo.PageView", "Url", c => c.String(maxLength: 500));
            AlterColumn("dbo.PageView", "PageId", c => c.Int());
            AlterColumn("dbo.PageView", "SessionId", c => c.Guid());
            CreateIndex("dbo.PageView", "PageId");
            CreateIndex("dbo.PageView", "PersonAliasId");
            CreateIndex("dbo.PageView", "SiteId");
            AddForeignKey("dbo.PageView", "PageId", "dbo.Page", "Id");
            AddForeignKey("dbo.PageView", "PersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.PageView", "SiteId", "dbo.Site", "Id", cascadeDelete: true);
            DropColumn("dbo.PageView", "QueryString");

            CreateIndex( "PageView", new string[] { "PageId", "ClientType" }, false );
            CreateIndex( "PageView", new string[] { "SiteId", "ClientType" }, false );
            CreateIndex( "PageView", new string[] { "SessionId" }, false );
            CreateIndex( "PageView", new string[] { "IpAddress" }, false );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"DELETE FROM [PageView]" );

            DropIndex( "PageView", new string[] { "PageId", "ClientType" } );
            DropIndex( "PageView", new string[] { "SiteId", "ClientType" } );
            DropIndex( "PageView", new string[] { "SessionId" } );
            DropIndex( "PageView", new string[] { "IpAddress" } );

            AddColumn("dbo.PageView", "QueryString", c => c.String(maxLength: 500));
            DropForeignKey("dbo.PageView", "SiteId", "dbo.Site");
            DropForeignKey("dbo.PageView", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PageView", "PageId", "dbo.Page");
            DropIndex("dbo.PageView", new[] { "SiteId" });
            DropIndex("dbo.PageView", new[] { "PersonAliasId" });
            DropIndex("dbo.PageView", new[] { "PageId" });
            AlterColumn("dbo.PageView", "SessionId", c => c.String(maxLength: 25));
            AlterColumn("dbo.PageView", "PageId", c => c.Int(nullable: false));
            DropColumn("dbo.PageView", "Url");
            CreateIndex("dbo.PageView", "SiteId");
            CreateIndex("dbo.PageView", "PersonAliasId");
            CreateIndex("dbo.PageView", "PageId");
            AddForeignKey("dbo.PageView", "SiteId", "dbo.Site", "Id");
            AddForeignKey("dbo.PageView", "PersonAliasId", "dbo.PersonAlias", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PageView", "PageId", "dbo.Page", "Id");
        }
    }
}
