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
    public partial class PersistedDataset : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.PersistedDataset",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccessKey = c.String(maxLength: 100),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        RefreshIntervalMinutes = c.Int(),
                        LastRefreshDateTime = c.DateTime(),
                        AllowManualRefresh = c.Boolean(nullable: false),
                        ResultData = c.String(),
                        ResultFormat = c.Int(nullable: false),
                        MemoryCacheDurationMS = c.Int(),
                        BuildScript = c.String(),
                        BuildScriptType = c.Int(nullable: false),
                        IsSystem = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false, defaultValue: true),
                        TimeToBuildMS = c.Double(),
                        EntityTypeId = c.Int(),
                        ExpireDateTime = c.DateTime(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .Index(t => t.AccessKey, unique: true)
                .Index(t => t.EntityTypeId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.PersistedDataset", "EntityTypeId", "dbo.EntityType");
            DropIndex("dbo.PersistedDataset", new[] { "Guid" });
            DropIndex("dbo.PersistedDataset", new[] { "EntityTypeId" });
            DropIndex("dbo.PersistedDataset", new[] { "AccessKey" });
            DropTable("dbo.PersistedDataset");
        }
    }
}
