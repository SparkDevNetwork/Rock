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
    public partial class ScheduleTypes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.GroupScheduleExclusion",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupTypeId = c.Int(nullable: false),
                        StartDate = c.DateTime(storeType: "date"),
                        EndDate = c.DateTime(storeType: "date"),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.GroupType", t => t.GroupTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.GroupTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.Attendance", "DidNotOccur", c => c.Boolean());
            AddColumn("dbo.Attendance", "Processed", c => c.Boolean());
            AddColumn("dbo.Schedule", "WeeklyDayOfWeek", c => c.Int());
            AddColumn("dbo.Schedule", "WeeklyTimeOfDay", c => c.Time(precision: 7));
            AddColumn("dbo.GroupType", "AllowedScheduleTypes", c => c.Int(nullable: false));
            DropColumn("dbo.Attendance", "Response");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Attendance", "Response", c => c.String(maxLength: 200));
            DropForeignKey("dbo.GroupScheduleExclusion", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.GroupScheduleExclusion", "GroupTypeId", "dbo.GroupType");
            DropForeignKey("dbo.GroupScheduleExclusion", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.GroupScheduleExclusion", new[] { "Guid" });
            DropIndex("dbo.GroupScheduleExclusion", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.GroupScheduleExclusion", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.GroupScheduleExclusion", new[] { "GroupTypeId" });
            DropColumn("dbo.GroupType", "AllowedScheduleTypes");
            DropColumn("dbo.Schedule", "WeeklyTimeOfDay");
            DropColumn("dbo.Schedule", "WeeklyDayOfWeek");
            DropColumn("dbo.Attendance", "Processed");
            DropColumn("dbo.Attendance", "DidNotOccur");
            DropTable("dbo.GroupScheduleExclusion");
        }
    }
}
