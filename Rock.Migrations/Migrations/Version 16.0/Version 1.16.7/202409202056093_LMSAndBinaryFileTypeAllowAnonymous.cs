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

    /// <summary>
    ///
    /// </summary>
    public partial class LMSAndBinaryFileTypeAllowAnonymous : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.BinaryFileType", "AllowAnonymous", c => c.Boolean(nullable: false, defaultValue: false ));
            AddColumn("dbo.LearningActivityCompletion", "GradedByPersonAliasId", c => c.Int());
            AddColumn("dbo.LearningActivity", "DueDateCriteria", c => c.Int(nullable: false));
            AddColumn("dbo.LearningActivity", "AvailabilityCriteria", c => c.Int(nullable: false));
            AddColumn("dbo.LearningProgram", "DefaultLearningGradingSystemId", c => c.Int());
            CreateIndex("dbo.LearningActivityCompletion", "GradedByPersonAliasId");
            CreateIndex("dbo.LearningProgram", "DefaultLearningGradingSystemId");
            AddForeignKey("dbo.LearningActivityCompletion", "GradedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.LearningProgram", "DefaultLearningGradingSystemId", "dbo.LearningGradingSystem", "Id");
            DropColumn("dbo.LearningActivity", "DueDateCalculationMethod");
            DropColumn("dbo.LearningActivity", "AvailableDateCalculationMethod");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.LearningActivity", "AvailableDateCalculationMethod", c => c.Int(nullable: false));
            AddColumn("dbo.LearningActivity", "DueDateCalculationMethod", c => c.Int(nullable: false));
            DropForeignKey("dbo.LearningProgram", "DefaultLearningGradingSystemId", "dbo.LearningGradingSystem");
            DropForeignKey("dbo.LearningActivityCompletion", "GradedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.LearningProgram", new[] { "DefaultLearningGradingSystemId" });
            DropIndex("dbo.LearningActivityCompletion", new[] { "GradedByPersonAliasId" });
            DropColumn("dbo.LearningProgram", "DefaultLearningGradingSystemId");
            DropColumn("dbo.LearningActivity", "AvailabilityCriteria");
            DropColumn("dbo.LearningActivity", "DueDateCriteria");
            DropColumn("dbo.LearningActivityCompletion", "GradedByPersonAliasId");
            DropColumn("dbo.BinaryFileType", "AllowAnonymous");
        }
    }
}
