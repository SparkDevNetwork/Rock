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
    public partial class AddHighlightColorAndAlternateImageBinaryFileIdToAchievementType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.AchievementType", "HighlightColor", c => c.String(maxLength: 50));
            AddColumn("dbo.AchievementType", "AlternateImageBinaryFileId", c => c.Int());
            CreateIndex("dbo.AchievementType", "AlternateImageBinaryFileId");
            AddForeignKey("dbo.AchievementType", "AlternateImageBinaryFileId", "dbo.BinaryFile", "Id");

            Sql(@"
                DECLARE @TrophyBinaryFileId INT = (SELECT TOP 1 [Id] FROM [BinaryFile] WHERE Guid = '9a1503bc-d965-4bd4-aea4-8039d4657201')
                DECLARE @MedalBinaryFileId INT = (SELECT TOP 1 [Id] FROM [BinaryFile] WHERE Guid = '80331f03-4f4b-46b3-b789-8d34c12b4f42')

                UPDATE AchievementType SET AlternateImageBinaryFileId = @TrophyBinaryFileId WHERE Guid = '21e6cc63-702b-4a5d-bc92-503b0f5caf5d'
                UPDATE AchievementType SET AlternateImageBinaryFileId = @MedalBinaryFileId WHERE Guid = '67ea551d-c3a6-4339-9f39-f6f4e4dab4ea'
            ");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.AchievementType", "AlternateImageBinaryFileId", "dbo.BinaryFile");
            DropIndex("dbo.AchievementType", new[] { "AlternateImageBinaryFileId" });
            DropColumn("dbo.AchievementType", "AlternateImageBinaryFileId");
            DropColumn("dbo.AchievementType", "HighlightColor");
        }
    }
}
