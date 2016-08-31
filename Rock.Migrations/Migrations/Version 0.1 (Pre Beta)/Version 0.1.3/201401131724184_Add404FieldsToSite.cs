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
    public partial class Add404FieldsToSite : Rock.Migrations.RockMigration2
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Site", "PageNotFoundPageId", c => c.Int());
            AddColumn("dbo.Site", "PageNotFoundPageRouteId", c => c.Int());
            CreateIndex("dbo.Site", "PageNotFoundPageId");
            CreateIndex("dbo.Site", "PageNotFoundPageRouteId");
            AddForeignKey("dbo.Site", "PageNotFoundPageId", "dbo.Page", "Id");
            AddForeignKey("dbo.Site", "PageNotFoundPageRouteId", "dbo.PageRoute", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Site", "PageNotFoundPageRouteId", "dbo.PageRoute");
            DropForeignKey("dbo.Site", "PageNotFoundPageId", "dbo.Page");
            DropIndex("dbo.Site", new[] { "PageNotFoundPageRouteId" });
            DropIndex("dbo.Site", new[] { "PageNotFoundPageId" });
            DropColumn("dbo.Site", "PageNotFoundPageRouteId");
            DropColumn("dbo.Site", "PageNotFoundPageId");
        }
    }
}
