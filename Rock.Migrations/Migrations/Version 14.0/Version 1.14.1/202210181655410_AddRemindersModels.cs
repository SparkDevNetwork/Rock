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
    public partial class AddRemindersModels : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.ReminderType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        NotificationType = c.Int(nullable: false),
                        NotificationWorkflowTypeId = c.Int(),
                        ShouldShowNote = c.Boolean(nullable: false),
                        Order = c.Int(nullable: false),
                        EntityTypeId = c.Int(nullable: false),
                        ShouldAutoCompleteWhenNotified = c.Boolean(nullable: false),
                        HighlightColor = c.String(maxLength: 50),
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
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.Reminder",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ReminderTypeId = c.Int(nullable: false),
                        PersonAliasId = c.Int(nullable: false),
                        EntityId = c.Int(nullable: false),
                        Note = c.String(),
                        ReminderDate = c.DateTime(nullable: false),
                        IsComplete = c.Boolean(nullable: false),
                        RenewPeriodDays = c.Int(),
                        RenewMaxCount = c.Int(),
                        RenewCurrentCount = c.Int(),
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
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId, cascadeDelete: true)
                .ForeignKey("dbo.ReminderType", t => t.ReminderTypeId, cascadeDelete: true)
                .Index(t => t.ReminderTypeId)
                .Index(t => t.PersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.Person", "ReminderCount", c => c.Int());
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Reminder", "ReminderTypeId", "dbo.ReminderType");
            DropForeignKey("dbo.Reminder", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Reminder", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Reminder", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ReminderType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ReminderType", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.ReminderType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.Reminder", new[] { "Guid" });
            DropIndex("dbo.Reminder", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.Reminder", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.Reminder", new[] { "PersonAliasId" });
            DropIndex("dbo.Reminder", new[] { "ReminderTypeId" });
            DropIndex("dbo.ReminderType", new[] { "Guid" });
            DropIndex("dbo.ReminderType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ReminderType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ReminderType", new[] { "EntityTypeId" });
            DropColumn("dbo.Person", "ReminderCount");
            DropTable("dbo.Reminder");
            DropTable("dbo.ReminderType");
        }
    }
}
