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
    public partial class AddConnectionStatusAutomation : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.ConnectionStatusAutomation",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AutomationName = c.String(nullable: false, maxLength: 50),
                        SourceStatusId = c.Int(nullable: false),
                        DestinationStatusId = c.Int(nullable: false),
                        DataViewId = c.Int(),
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
                .ForeignKey("dbo.DataView", t => t.DataViewId)
                .ForeignKey("dbo.ConnectionStatus", t => t.DestinationStatusId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.ConnectionStatus", t => t.SourceStatusId)
                .Index(t => t.SourceStatusId)
                .Index(t => t.DestinationStatusId)
                .Index(t => t.DataViewId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.ConnectionStatusAutomation", "SourceStatusId", "dbo.ConnectionStatus");
            DropForeignKey("dbo.ConnectionStatusAutomation", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionStatusAutomation", "DestinationStatusId", "dbo.ConnectionStatus");
            DropForeignKey("dbo.ConnectionStatusAutomation", "DataViewId", "dbo.DataView");
            DropForeignKey("dbo.ConnectionStatusAutomation", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.ConnectionStatusAutomation", new[] { "Guid" });
            DropIndex("dbo.ConnectionStatusAutomation", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ConnectionStatusAutomation", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ConnectionStatusAutomation", new[] { "DataViewId" });
            DropIndex("dbo.ConnectionStatusAutomation", new[] { "DestinationStatusId" });
            DropIndex("dbo.ConnectionStatusAutomation", new[] { "SourceStatusId" });
            DropTable("dbo.ConnectionStatusAutomation");
        }
    }
}
