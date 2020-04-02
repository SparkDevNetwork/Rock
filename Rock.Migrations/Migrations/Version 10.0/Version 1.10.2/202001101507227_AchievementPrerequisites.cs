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
    public partial class AchievementPrerequisites : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.StreakTypeAchievementTypePrerequisite",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StreakTypeAchievementTypeId = c.Int(nullable: false),
                        PrerequisiteStreakTypeAchievementTypeId = c.Int(nullable: false),
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
                .ForeignKey("dbo.StreakTypeAchievementType", t => t.PrerequisiteStreakTypeAchievementTypeId)
                .ForeignKey("dbo.StreakTypeAchievementType", t => t.StreakTypeAchievementTypeId, cascadeDelete: true)
                .Index(t => t.StreakTypeAchievementTypeId)
                .Index(t => t.PrerequisiteStreakTypeAchievementTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.StreakTypeAchievementTypePrerequisite", "StreakTypeAchievementTypeId", "dbo.StreakTypeAchievementType");
            DropForeignKey("dbo.StreakTypeAchievementTypePrerequisite", "PrerequisiteStreakTypeAchievementTypeId", "dbo.StreakTypeAchievementType");
            DropForeignKey("dbo.StreakTypeAchievementTypePrerequisite", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.StreakTypeAchievementTypePrerequisite", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.StreakTypeAchievementTypePrerequisite", new[] { "Guid" });
            DropIndex("dbo.StreakTypeAchievementTypePrerequisite", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.StreakTypeAchievementTypePrerequisite", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.StreakTypeAchievementTypePrerequisite", new[] { "PrerequisiteStreakTypeAchievementTypeId" });
            DropIndex("dbo.StreakTypeAchievementTypePrerequisite", new[] { "StreakTypeAchievementTypeId" });
            DropTable("dbo.StreakTypeAchievementTypePrerequisite");
        }
    }
}
