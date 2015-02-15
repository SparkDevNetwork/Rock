// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class GroupSchedule : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AlterColumn("dbo.Attendance", "DidAttend", c => c.Boolean());
            AddColumn("dbo.Attendance", "RSVP", c => c.Int(nullable: false));
            AddColumn("dbo.Attendance", "Response", c => c.String(maxLength: 200));

            AddColumn("dbo.Group", "ScheduleId", c => c.Int());
            CreateIndex("dbo.Group", "ScheduleId");
            AddForeignKey("dbo.Group", "ScheduleId", "dbo.Schedule", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Group", "ScheduleId", "dbo.Schedule");
            DropIndex("dbo.Group", new[] { "ScheduleId" });
            DropColumn("dbo.Group", "ScheduleId");

            DropColumn("dbo.Attendance", "Response");
            DropColumn("dbo.Attendance", "RSVP");
            AlterColumn("dbo.Attendance", "DidAttend", c => c.Boolean(nullable: false));
        }
    }
}
