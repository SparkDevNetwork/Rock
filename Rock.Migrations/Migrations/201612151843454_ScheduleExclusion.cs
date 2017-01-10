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
    public partial class ScheduleExclusion : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.ScheduleCategoryExclusion",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CategoryId = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 50),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
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
                .ForeignKey("dbo.Category", t => t.CategoryId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CategoryId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);

            // Page: Schedules
            RockMigrationHelper.UpdateBlockType( "Schedule Category Exclusion List", "List of dates that schedules are not active for an entire category.", "~/Blocks/Core/ScheduleCategoryExclusionList.ascx", "Core", "ACF84335-34A1-4DD6-B242-20119B8D0967" );
            RockMigrationHelper.AddBlock( "AFFFB245-A0EB-4002-B736-A2D52DD692CF", "", "ACF84335-34A1-4DD6-B242-20119B8D0967", "Schedule Category Exclusion List", "Main", "", "", 1, "286DC221-C04A-4405-8B69-EC3D83740CD0" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "286DC221-C04A-4405-8B69-EC3D83740CD0" );
            RockMigrationHelper.DeleteBlockType( "ACF84335-34A1-4DD6-B242-20119B8D0967" );

            DropForeignKey("dbo.ScheduleCategoryExclusion", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ScheduleCategoryExclusion", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ScheduleCategoryExclusion", "CategoryId", "dbo.Category");
            DropIndex("dbo.ScheduleCategoryExclusion", new[] { "Guid" });
            DropIndex("dbo.ScheduleCategoryExclusion", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ScheduleCategoryExclusion", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ScheduleCategoryExclusion", new[] { "CategoryId" });
            DropTable("dbo.ScheduleCategoryExclusion");
        }
    }
}
