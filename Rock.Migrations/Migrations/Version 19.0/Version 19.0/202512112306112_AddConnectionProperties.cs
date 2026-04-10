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
    public partial class AddConnectionProperties : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.ConnectionTypeSource",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ConnectionTypeId = c.Int(nullable: false),
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
                .ForeignKey("dbo.ConnectionType", t => t.ConnectionTypeId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.ConnectionTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.ConnectionRequestStatusHistory",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ConnectionStatusId = c.Int(nullable: false),
                        StartDateTime = c.DateTime(nullable: false),
                        EndDateTime = c.DateTime(nullable: false),
                        CompletedByPersonAliasId = c.Int(),
                        WasCompletedOnTime = c.Boolean(nullable: false),
                        Note = c.String(),
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
                .ForeignKey("dbo.PersonAlias", t => t.CompletedByPersonAliasId)
                .ForeignKey("dbo.ConnectionStatus", t => t.ConnectionStatusId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.ConnectionStatusId)
                .Index(t => t.CompletedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.ConnectionActivityType", "PersonNoteCreationBehavior", c => c.Int());
            AddColumn("dbo.ConnectionActivityType", "PersonNoteTypeId", c => c.Int());
            AddColumn("dbo.ConnectionType", "SnippetCategoryId", c => c.Int());
            AddColumn("dbo.ConnectionType", "DueDateCalculationMode", c => c.Int(nullable: false));
            AddColumn("dbo.ConnectionType", "RequestDueDateOffestInDays", c => c.Int());
            AddColumn("dbo.ConnectionType", "RequestDueSoonOffsetInDays", c => c.Int());
            AddColumn("dbo.ConnectionType", "EnabledFeatures", c => c.Int(nullable: false));
            AddColumn("dbo.ConnectionType", "EnabledViews", c => c.Int(nullable: false));
            AddColumn("dbo.ConnectionType", "IsSequentialStatusEnforced", c => c.Boolean(nullable: false));
            AddColumn("dbo.ConnectionType", "AdditionalSettingsJson", c => c.String());
            AddColumn("dbo.ConnectionOpportunity", "RequestDueDateOffestInDays", c => c.Int());
            AddColumn("dbo.ConnectionOpportunity", "RequestDueSoonOffsetInDays", c => c.Int());
            AddColumn("dbo.ConnectionRequest", "ConnectionTypeSourceId", c => c.Int());
            AddColumn("dbo.ConnectionRequest", "CelebrationNote", c => c.String());
            AddColumn("dbo.ConnectionRequest", "DueDate", c => c.DateTime());
            AddColumn("dbo.ConnectionRequest", "DueSoonDate", c => c.DateTime());
            AddColumn("dbo.ConnectionRequest", "CelebrationText", c => c.String());
            AddColumn("dbo.ConnectionRequest", "ConnectedDateTime", c => c.DateTime());
            AddColumn("dbo.ConnectionRequest", "WasCompletedOnTime", c => c.Boolean(nullable: false));
            AddColumn("dbo.ConnectionStatus", "RequestStatusDueDateOffestInDays", c => c.Int());
            AddColumn("dbo.ConnectionStatus", "RequestStatusDueSoonOffsetInDays", c => c.Int());
            AddColumn("dbo.ConnectionStatus", "AutoFutureFollowUpPauseInDays", c => c.Int());
            AddColumn("dbo.ConnectionStatus", "IsNoteRequiredOnCompletion", c => c.Boolean(nullable: false));
            CreateIndex("dbo.ConnectionType", "SnippetCategoryId");
            CreateIndex("dbo.ConnectionRequest", "ConnectionTypeSourceId");
            AddForeignKey("dbo.ConnectionRequest", "ConnectionTypeSourceId", "dbo.ConnectionTypeSource", "Id");
            AddForeignKey("dbo.ConnectionType", "SnippetCategoryId", "dbo.Category", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.ConnectionRequestStatusHistory", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionRequestStatusHistory", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionRequestStatusHistory", "ConnectionStatusId", "dbo.ConnectionStatus");
            DropForeignKey("dbo.ConnectionRequestStatusHistory", "CompletedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionType", "SnippetCategoryId", "dbo.Category");
            DropForeignKey("dbo.ConnectionRequest", "ConnectionTypeSourceId", "dbo.ConnectionTypeSource");
            DropForeignKey("dbo.ConnectionTypeSource", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionTypeSource", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionTypeSource", "ConnectionTypeId", "dbo.ConnectionType");
            DropIndex("dbo.ConnectionRequestStatusHistory", new[] { "Guid" });
            DropIndex("dbo.ConnectionRequestStatusHistory", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ConnectionRequestStatusHistory", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ConnectionRequestStatusHistory", new[] { "CompletedByPersonAliasId" });
            DropIndex("dbo.ConnectionRequestStatusHistory", new[] { "ConnectionStatusId" });
            DropIndex("dbo.ConnectionTypeSource", new[] { "Guid" });
            DropIndex("dbo.ConnectionTypeSource", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ConnectionTypeSource", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ConnectionTypeSource", new[] { "ConnectionTypeId" });
            DropIndex("dbo.ConnectionRequest", new[] { "ConnectionTypeSourceId" });
            DropIndex("dbo.ConnectionType", new[] { "SnippetCategoryId" });
            DropColumn("dbo.ConnectionStatus", "IsNoteRequiredOnCompletion");
            DropColumn("dbo.ConnectionStatus", "AutoFutureFollowUpPauseInDays");
            DropColumn("dbo.ConnectionStatus", "RequestStatusDueSoonOffsetInDays");
            DropColumn("dbo.ConnectionStatus", "RequestStatusDueDateOffestInDays");
            DropColumn("dbo.ConnectionRequest", "WasCompletedOnTime");
            DropColumn("dbo.ConnectionRequest", "ConnectedDateTime");
            DropColumn("dbo.ConnectionRequest", "CelebrationText");
            DropColumn("dbo.ConnectionRequest", "DueSoonDate");
            DropColumn("dbo.ConnectionRequest", "DueDate");
            DropColumn("dbo.ConnectionRequest", "CelebrationNote");
            DropColumn("dbo.ConnectionRequest", "ConnectionTypeSourceId");
            DropColumn("dbo.ConnectionOpportunity", "RequestDueSoonOffsetInDays");
            DropColumn("dbo.ConnectionOpportunity", "RequestDueDateOffestInDays");
            DropColumn("dbo.ConnectionType", "AdditionalSettingsJson");
            DropColumn("dbo.ConnectionType", "IsSequentialStatusEnforced");
            DropColumn("dbo.ConnectionType", "EnabledViews");
            DropColumn("dbo.ConnectionType", "EnabledFeatures");
            DropColumn("dbo.ConnectionType", "RequestDueSoonOffsetInDays");
            DropColumn("dbo.ConnectionType", "RequestDueDateOffestInDays");
            DropColumn("dbo.ConnectionType", "DueDateCalculationMode");
            DropColumn("dbo.ConnectionType", "SnippetCategoryId");
            DropColumn("dbo.ConnectionActivityType", "PersonNoteTypeId");
            DropColumn("dbo.ConnectionActivityType", "PersonNoteCreationBehavior");
            DropTable("dbo.ConnectionRequestStatusHistory");
            DropTable("dbo.ConnectionTypeSource");
        }
    }
}
