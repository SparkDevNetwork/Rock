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
    public partial class DataViewPersisted : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.DataViewPersistedValue",
                c => new
                    {
                        DataViewId = c.Int(nullable: false),
                        EntityId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.DataViewId, t.EntityId })
                .ForeignKey("dbo.DataView", t => t.DataViewId, cascadeDelete: true)
                .Index(t => t.DataViewId);
            
            AddColumn("dbo.DataView", "PersistedScheduleIntervalMinutes", c => c.Int());
            AddColumn("dbo.DataView", "PersistedLastRefreshDateTime", c => c.DateTime());
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.DataViewPersistedValue", "DataViewId", "dbo.DataView");
            DropIndex("dbo.DataViewPersistedValue", new[] { "DataViewId" });
            DropColumn("dbo.DataView", "PersistedLastRefreshDateTime");
            DropColumn("dbo.DataView", "PersistedScheduleIntervalMinutes");
            DropTable("dbo.DataViewPersistedValue");
        }
    }
}
