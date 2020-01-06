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
    public partial class AchievementTypeOptionalForeignKeys : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropIndex("dbo.StreakTypeAchievementType", new[] { "AchievementStartWorkflowTypeId" });
            DropIndex("dbo.StreakTypeAchievementType", new[] { "AchievementEndWorkflowTypeId" });
            DropIndex("dbo.StreakTypeAchievementType", new[] { "AchievementStepTypeId" });
            DropIndex("dbo.StreakTypeAchievementType", new[] { "AchievementStepStatusId" });

            AlterColumn("dbo.StreakTypeAchievementType", "AchievementStartWorkflowTypeId", c => c.Int());
            AlterColumn("dbo.StreakTypeAchievementType", "AchievementEndWorkflowTypeId", c => c.Int());
            AlterColumn("dbo.StreakTypeAchievementType", "AchievementStepTypeId", c => c.Int());
            AlterColumn("dbo.StreakTypeAchievementType", "AchievementStepStatusId", c => c.Int());

            CreateIndex("dbo.StreakTypeAchievementType", "AchievementStartWorkflowTypeId");
            CreateIndex("dbo.StreakTypeAchievementType", "AchievementEndWorkflowTypeId");
            CreateIndex("dbo.StreakTypeAchievementType", "AchievementStepTypeId");
            CreateIndex("dbo.StreakTypeAchievementType", "AchievementStepStatusId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.StreakTypeAchievementType", new[] { "AchievementStepStatusId" });
            DropIndex("dbo.StreakTypeAchievementType", new[] { "AchievementStepTypeId" });
            DropIndex("dbo.StreakTypeAchievementType", new[] { "AchievementEndWorkflowTypeId" });
            DropIndex("dbo.StreakTypeAchievementType", new[] { "AchievementStartWorkflowTypeId" });

            AlterColumn("dbo.StreakTypeAchievementType", "AchievementStepStatusId", c => c.Int(nullable: false));
            AlterColumn("dbo.StreakTypeAchievementType", "AchievementStepTypeId", c => c.Int(nullable: false));
            AlterColumn("dbo.StreakTypeAchievementType", "AchievementEndWorkflowTypeId", c => c.Int(nullable: false));
            AlterColumn("dbo.StreakTypeAchievementType", "AchievementStartWorkflowTypeId", c => c.Int(nullable: false));

            CreateIndex("dbo.StreakTypeAchievementType", "AchievementStepStatusId");
            CreateIndex("dbo.StreakTypeAchievementType", "AchievementStepTypeId");
            CreateIndex("dbo.StreakTypeAchievementType", "AchievementEndWorkflowTypeId");
            CreateIndex("dbo.StreakTypeAchievementType", "AchievementStartWorkflowTypeId");
        }
    }
}
