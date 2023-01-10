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
    public partial class AddCampusIdToInteractiveExperienceAnswer : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.InteractiveExperienceSchedule", "ScheduleSettingsJson", c => c.String());
            AddColumn("dbo.InteractiveExperienceAnswer", "CampusId", c => c.Int());
            AddColumn("dbo.InteractiveExperienceAnswer", "ResponseDataJson", c => c.String());

            CreateIndex("dbo.InteractiveExperienceAnswer", "InteractionSessionId");
            CreateIndex("dbo.InteractiveExperienceAnswer", "CampusId");

            AddForeignKey("dbo.InteractiveExperienceAnswer", "CampusId", "dbo.Campus", "Id");

            // Instead of the scaffolded foreign key, we want ours to 
            // set NULL on delete.
            //AddForeignKey("dbo.InteractiveExperienceAnswer", "InteractionSessionId", "dbo.InteractionSession", "Id");
            Sql( @"ALTER TABLE [dbo].[InteractiveExperienceAnswer]
ADD CONSTRAINT [FK_dbo.InteractiveExperienceAnswer_dbo.InteractionSession_InteractionSessionId] FOREIGN KEY ([InteractionSessionId])
REFERENCES [dbo].[InteractionSession] ([Id])
ON DELETE SET NULL" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.InteractiveExperienceAnswer", "InteractionSessionId", "dbo.InteractionSession");
            DropForeignKey("dbo.InteractiveExperienceAnswer", "CampusId", "dbo.Campus");
            DropIndex("dbo.InteractiveExperienceAnswer", new[] { "CampusId" });
            DropIndex("dbo.InteractiveExperienceAnswer", new[] { "InteractionSessionId" });
            DropColumn("dbo.InteractiveExperienceAnswer", "ResponseDataJson");
            DropColumn("dbo.InteractiveExperienceAnswer", "CampusId");
            DropColumn("dbo.InteractiveExperienceSchedule", "ScheduleSettingsJson");
        }
    }
}
