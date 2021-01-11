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
    public partial class BusAndFarmModels : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.WebFarmNodeLog",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Severity = c.Int(nullable: false),
                        WebFarmNodeId = c.Int(nullable: false),
                        WriterWebFarmNodeId = c.Int(nullable: false),
                        EventType = c.String(nullable: false, maxLength: 50),
                        EventDateTime = c.DateTime(nullable: false),
                        Message = c.String(),
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
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.WebFarmNode", t => t.WebFarmNodeId, cascadeDelete: true)
                .ForeignKey("dbo.WebFarmNode", t => t.WriterWebFarmNodeId)
                .Index(t => t.WebFarmNodeId)
                .Index(t => t.WriterWebFarmNodeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.WebFarmNode",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NodeName = c.String(nullable: false, maxLength: 250),
                        AddedDateTime = c.DateTime(nullable: false),
                        LastRestartDateTime = c.DateTime(nullable: false),
                        StoppedDateTime = c.DateTime(),
                        JobsAllowed = c.Boolean(nullable: false),
                        IsCurrentJobRunner = c.Boolean(nullable: false),
                        LastSeenDateTime = c.DateTime(nullable: false),
                        IsLeader = c.Boolean(nullable: false),
                        CurrentLeadershipPollingIntervalSeconds = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ConfiguredLeadershipPollingIntervalSeconds = c.Int(),
                        IsActive = c.Boolean(nullable: false),
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
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.NodeName, unique: true)
                .Index(t => t.CurrentLeadershipPollingIntervalSeconds, unique: true)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.EntityType", "IsMessageBusEventPublishEnabled", c => c.Boolean(nullable: false));
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.WebFarmNodeLog", "WriterWebFarmNodeId", "dbo.WebFarmNode");
            DropForeignKey("dbo.WebFarmNodeLog", "WebFarmNodeId", "dbo.WebFarmNode");
            DropForeignKey("dbo.WebFarmNode", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.WebFarmNode", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.WebFarmNodeLog", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.WebFarmNodeLog", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.WebFarmNode", new[] { "Guid" });
            DropIndex("dbo.WebFarmNode", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.WebFarmNode", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.WebFarmNode", new[] { "CurrentLeadershipPollingIntervalSeconds" });
            DropIndex("dbo.WebFarmNode", new[] { "NodeName" });
            DropIndex("dbo.WebFarmNodeLog", new[] { "Guid" });
            DropIndex("dbo.WebFarmNodeLog", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.WebFarmNodeLog", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.WebFarmNodeLog", new[] { "WriterWebFarmNodeId" });
            DropIndex("dbo.WebFarmNodeLog", new[] { "WebFarmNodeId" });
            DropColumn("dbo.EntityType", "IsMessageBusEventPublishEnabled");
            DropTable("dbo.WebFarmNode");
            DropTable("dbo.WebFarmNodeLog");
        }
    }
}
