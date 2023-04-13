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
    public partial class AddInteractiveExperienceOccurrence : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Any existing data would cause SQL errors, and there shouldn't be any data
            // yet anyway. Just make sure nobody has manually inserted rows.
            Sql( "TRUNCATE TABLE dbo.InteractiveExperienceAnswer" );

            DropForeignKey("dbo.InteractiveExperienceAnswer", "CampusId", "dbo.Campus");
            DropForeignKey("dbo.InteractiveExperienceAnswer", "InteractiveExperienceScheduleId", "dbo.InteractiveExperienceSchedule");

            DropIndex("dbo.InteractiveExperienceAnswer", new[] { "InteractiveExperienceScheduleId" });
            DropIndex("dbo.InteractiveExperienceAnswer", new[] { "CampusId" });

            CreateTable(
                "dbo.InteractiveExperienceOccurrence",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InteractiveExperienceScheduleId = c.Int(nullable: false),
                        CampusId = c.Int(),
                        OccurrenceDateTime = c.DateTime(nullable: false, storeType: "date"),
                        OccurrenceDateKey = c.Int(nullable: false),
                        CurrentlyShownActionId = c.Int(),
                        StateJson = c.String(),
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
                .ForeignKey("dbo.Campus", t => t.CampusId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.InteractiveExperienceAction", t => t.CurrentlyShownActionId)
                .ForeignKey("dbo.InteractiveExperienceSchedule", t => t.InteractiveExperienceScheduleId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.InteractiveExperienceScheduleId)
                .Index(t => t.CampusId)
                .Index(t => t.OccurrenceDateTime)
                .Index(t => t.CurrentlyShownActionId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.InteractiveExperienceAnswer", "InteractiveExperienceOccurrenceId", c => c.Int(nullable: false));
            CreateIndex("dbo.InteractiveExperienceAnswer", "InteractiveExperienceOccurrenceId");
            AddForeignKey("dbo.InteractiveExperienceAnswer", "InteractiveExperienceOccurrenceId", "dbo.InteractiveExperienceOccurrence", "Id");

            DropColumn("dbo.InteractiveExperienceAnswer", "InteractiveExperienceScheduleId");
            DropColumn("dbo.InteractiveExperienceAnswer", "CampusId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.InteractiveExperienceAnswer", "CampusId", c => c.Int());
            AddColumn("dbo.InteractiveExperienceAnswer", "InteractiveExperienceScheduleId", c => c.Int(nullable: false));
            DropForeignKey("dbo.InteractiveExperienceAnswer", "InteractiveExperienceOccurrenceId", "dbo.InteractiveExperienceOccurrence");
            DropForeignKey("dbo.InteractiveExperienceOccurrence", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractiveExperienceOccurrence", "InteractiveExperienceScheduleId", "dbo.InteractiveExperienceSchedule");
            DropForeignKey("dbo.InteractiveExperienceOccurrence", "CurrentlyShownActionId", "dbo.InteractiveExperienceAction");
            DropForeignKey("dbo.InteractiveExperienceOccurrence", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractiveExperienceOccurrence", "CampusId", "dbo.Campus");
            DropIndex("dbo.InteractiveExperienceAnswer", new[] { "InteractiveExperienceOccurrenceId" });
            DropIndex("dbo.InteractiveExperienceOccurrence", new[] { "Guid" });
            DropIndex("dbo.InteractiveExperienceOccurrence", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.InteractiveExperienceOccurrence", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.InteractiveExperienceOccurrence", new[] { "CurrentlyShownActionId" });
            DropIndex("dbo.InteractiveExperienceOccurrence", new[] { "OccurrenceDateTime" });
            DropIndex("dbo.InteractiveExperienceOccurrence", new[] { "CampusId" });
            DropIndex("dbo.InteractiveExperienceOccurrence", new[] { "InteractiveExperienceScheduleId" });
            DropColumn("dbo.InteractiveExperienceAnswer", "InteractiveExperienceOccurrenceId");
            DropTable("dbo.InteractiveExperienceOccurrence");
            CreateIndex("dbo.InteractiveExperienceAnswer", "CampusId");
            CreateIndex("dbo.InteractiveExperienceAnswer", "InteractiveExperienceScheduleId");
            AddForeignKey("dbo.InteractiveExperienceAnswer", "InteractiveExperienceScheduleId", "dbo.InteractiveExperienceSchedule", "Id");
            AddForeignKey("dbo.InteractiveExperienceAnswer", "CampusId", "dbo.Campus", "Id");
        }
    }
}
