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
    public partial class Achievements : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.StreakTypeAchievementType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StreakTypeId = c.Int(nullable: false),
                        AchievementEntityTypeId = c.Int(nullable: false),
                        AchievementStartWorkflowTypeId = c.Int(nullable: false),
                        AchievementEndWorkflowTypeId = c.Int(nullable: false),
                        AchievementStepTypeId = c.Int(nullable: false),
                        AchievementStepStatusId = c.Int(nullable: false),
                        BadgeLavaTemplate = c.String(),
                        ResultsLavaTemplate = c.String(),
                        AchievementIconCssClass = c.String(maxLength: 100),
                        MaxAccomplishmentsAllowed = c.Int(nullable: false),
                        AllowOverAchievement = c.Boolean(nullable: false),
                        CategoryId = c.Int(),
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
                .ForeignKey("dbo.WorkflowType", t => t.AchievementEndWorkflowTypeId)
                .ForeignKey("dbo.EntityType", t => t.AchievementEntityTypeId, cascadeDelete: true)
                .ForeignKey("dbo.WorkflowType", t => t.AchievementStartWorkflowTypeId)
                .ForeignKey("dbo.StepStatus", t => t.AchievementStepStatusId)
                .ForeignKey("dbo.StepType", t => t.AchievementStepTypeId)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.StreakType", t => t.StreakTypeId, cascadeDelete: true)
                .Index(t => t.StreakTypeId)
                .Index(t => t.AchievementEntityTypeId)
                .Index(t => t.AchievementStartWorkflowTypeId)
                .Index(t => t.AchievementEndWorkflowTypeId)
                .Index(t => t.AchievementStepTypeId)
                .Index(t => t.AchievementStepStatusId)
                .Index(t => t.CategoryId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.StreakAchievementAttempt",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StreakId = c.Int(nullable: false),
                        StreakTypeAchievementTypeId = c.Int(nullable: false),
                        Progress = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsClosed = c.Boolean(nullable: false),
                        IsSuccessful = c.Boolean(nullable: false),
                        AchievementAttemptStartDateTime = c.DateTime(nullable: false),
                        AchievementAttemptEndDateTime = c.DateTime(),
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
                .ForeignKey("dbo.Streak", t => t.StreakId)
                .ForeignKey("dbo.StreakTypeAchievementType", t => t.StreakTypeAchievementTypeId, cascadeDelete: true)
                .Index(t => t.StreakId)
                .Index(t => t.StreakTypeAchievementTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.StreakTypeAchievementType", "StreakTypeId", "dbo.StreakType");
            DropForeignKey("dbo.StreakAchievementAttempt", "StreakTypeAchievementTypeId", "dbo.StreakTypeAchievementType");
            DropForeignKey("dbo.StreakAchievementAttempt", "StreakId", "dbo.Streak");
            DropForeignKey("dbo.StreakAchievementAttempt", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.StreakAchievementAttempt", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.StreakTypeAchievementType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.StreakTypeAchievementType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.StreakTypeAchievementType", "CategoryId", "dbo.Category");
            DropForeignKey("dbo.StreakTypeAchievementType", "AchievementStepTypeId", "dbo.StepType");
            DropForeignKey("dbo.StreakTypeAchievementType", "AchievementStepStatusId", "dbo.StepStatus");
            DropForeignKey("dbo.StreakTypeAchievementType", "AchievementStartWorkflowTypeId", "dbo.WorkflowType");
            DropForeignKey("dbo.StreakTypeAchievementType", "AchievementEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.StreakTypeAchievementType", "AchievementEndWorkflowTypeId", "dbo.WorkflowType");
            DropIndex("dbo.StreakAchievementAttempt", new[] { "Guid" });
            DropIndex("dbo.StreakAchievementAttempt", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.StreakAchievementAttempt", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.StreakAchievementAttempt", new[] { "StreakTypeAchievementTypeId" });
            DropIndex("dbo.StreakAchievementAttempt", new[] { "StreakId" });
            DropIndex("dbo.StreakTypeAchievementType", new[] { "Guid" });
            DropIndex("dbo.StreakTypeAchievementType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.StreakTypeAchievementType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.StreakTypeAchievementType", new[] { "CategoryId" });
            DropIndex("dbo.StreakTypeAchievementType", new[] { "AchievementStepStatusId" });
            DropIndex("dbo.StreakTypeAchievementType", new[] { "AchievementStepTypeId" });
            DropIndex("dbo.StreakTypeAchievementType", new[] { "AchievementEndWorkflowTypeId" });
            DropIndex("dbo.StreakTypeAchievementType", new[] { "AchievementStartWorkflowTypeId" });
            DropIndex("dbo.StreakTypeAchievementType", new[] { "AchievementEntityTypeId" });
            DropIndex("dbo.StreakTypeAchievementType", new[] { "StreakTypeId" });
            DropTable("dbo.StreakAchievementAttempt");
            DropTable("dbo.StreakTypeAchievementType");
        }
    }
}
