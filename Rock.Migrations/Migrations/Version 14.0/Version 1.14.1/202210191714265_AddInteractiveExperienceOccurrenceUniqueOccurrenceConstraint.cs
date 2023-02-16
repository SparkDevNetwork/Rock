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
    public partial class AddInteractiveExperienceOccurrenceUniqueOccurrenceConstraint : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropIndex("dbo.InteractiveExperienceOccurrence", new[] { "InteractiveExperienceScheduleId" });
            DropIndex("dbo.InteractiveExperienceOccurrence", new[] { "CampusId" });
            DropIndex("dbo.InteractiveExperienceOccurrence", new[] { "OccurrenceDateTime" });
            AlterColumn("dbo.InteractiveExperienceOccurrence", "OccurrenceDateTime", c => c.DateTime(nullable: false));
            CreateIndex("dbo.InteractiveExperienceOccurrence", new[] { "InteractiveExperienceScheduleId", "CampusId", "OccurrenceDateTime" }, unique: true, name: "IX_InteractiveExperienceScheduleIdCampusIdOccurrenceDateTime");
            CreateIndex("dbo.InteractiveExperienceOccurrence", "OccurrenceDateTime");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.InteractiveExperienceOccurrence", new[] { "OccurrenceDateTime" });
            DropIndex("dbo.InteractiveExperienceOccurrence", "IX_InteractiveExperienceScheduleIdCampusIdOccurrenceDateTime");
            AlterColumn("dbo.InteractiveExperienceOccurrence", "OccurrenceDateTime", c => c.DateTime(nullable: false, storeType: "date"));
            CreateIndex("dbo.InteractiveExperienceOccurrence", "OccurrenceDateTime");
            CreateIndex("dbo.InteractiveExperienceOccurrence", "CampusId");
            CreateIndex("dbo.InteractiveExperienceOccurrence", "InteractiveExperienceScheduleId");
        }
    }
}
