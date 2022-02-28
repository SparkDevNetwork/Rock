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
    public partial class RemoveObsoleteV8Fields : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.SiteIconExtensions", "SiteId", "dbo.Site");
            DropForeignKey("dbo.SiteIconExtensions", "DefinedValueId", "dbo.DefinedValue");
            DropIndex("dbo.SiteIconExtensions", new[] { "SiteId" });
            DropIndex("dbo.SiteIconExtensions", new[] { "DefinedValueId" });
            DropColumn("dbo.Site", "IconCssWeight");
            DropColumn("dbo.Communication", "MediumDataJson");
            DropColumn("dbo.CommunicationTemplate", "MediumDataJson");
            DropColumn("dbo.History", "Summary");
            DropTable("dbo.SiteIconExtensions");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CreateTable(
                "dbo.SiteIconExtensions",
                c => new
                    {
                        SiteId = c.Int(nullable: false),
                        DefinedValueId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.SiteId, t.DefinedValueId });
            
            AddColumn("dbo.History", "Summary", c => c.String());
            AddColumn("dbo.CommunicationTemplate", "MediumDataJson", c => c.String());
            AddColumn("dbo.Communication", "MediumDataJson", c => c.String());
            AddColumn("dbo.Site", "IconCssWeight", c => c.Int(nullable: false));
            CreateIndex("dbo.SiteIconExtensions", "DefinedValueId");
            CreateIndex("dbo.SiteIconExtensions", "SiteId");
            AddForeignKey("dbo.SiteIconExtensions", "DefinedValueId", "dbo.DefinedValue", "Id", cascadeDelete: true);
            AddForeignKey("dbo.SiteIconExtensions", "SiteId", "dbo.Site", "Id", cascadeDelete: true);
        }
    }
}
