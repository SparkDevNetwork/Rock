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
    public partial class AchievementTypeWorkflowTypes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameColumn( table: "dbo.StreakTypeAchievementType", name: "AchievementEndWorkflowTypeId", newName: "AchievementFailureWorkflowTypeId" );
            RenameIndex( table: "dbo.StreakTypeAchievementType", name: "IX_AchievementEndWorkflowTypeId", newName: "IX_AchievementFailureWorkflowTypeId" );
            AddColumn( "dbo.StreakTypeAchievementType", "AchievementSuccessWorkflowTypeId", c => c.Int() );
            CreateIndex( "dbo.StreakTypeAchievementType", "AchievementSuccessWorkflowTypeId" );
            AddForeignKey( "dbo.StreakTypeAchievementType", "AchievementSuccessWorkflowTypeId", "dbo.WorkflowType", "Id" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.StreakTypeAchievementType", "AchievementSuccessWorkflowTypeId", "dbo.WorkflowType" );
            DropIndex( "dbo.StreakTypeAchievementType", new[] { "AchievementSuccessWorkflowTypeId" } );
            DropColumn( "dbo.StreakTypeAchievementType", "AchievementSuccessWorkflowTypeId" );
            RenameIndex( table: "dbo.StreakTypeAchievementType", name: "IX_AchievementFailureWorkflowTypeId", newName: "IX_AchievementEndWorkflowTypeId" );
            RenameColumn( table: "dbo.StreakTypeAchievementType", name: "AchievementFailureWorkflowTypeId", newName: "AchievementEndWorkflowTypeId" );
        }
    }
}
