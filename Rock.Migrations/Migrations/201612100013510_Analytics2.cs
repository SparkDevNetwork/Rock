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
    public partial class Analytics2 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.AnalyticsSourceAttendance",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AttendanceDateKey = c.Int(nullable: false),
                        AttendanceTypeId = c.Int(),
                        DaysSinceLastAttendanceOfType = c.Int(),
                        IsFirstAttendanceOfType = c.Boolean(nullable: false),
                        Count = c.Int(nullable: false),
                        LocationId = c.Int(),
                        CampusId = c.Int(),
                        ScheduleId = c.Int(),
                        GroupId = c.Int(),
                        PersonAliasId = c.Int(),
                        DeviceId = c.Int(),
                        SearchTypeValueId = c.Int(),
                        SearchValue = c.String(),
                        SearchResultGroupId = c.Int(),
                        AttendanceCodeId = c.Int(),
                        QualifierValueId = c.Int(),
                        StartDateTime = c.DateTime(nullable: false),
                        EndDateTime = c.DateTime(),
                        RSVP = c.Int(nullable: false),
                        DidAttend = c.Boolean(),
                        DidNotOccur = c.Boolean(),
                        Processed = c.Boolean(),
                        Note = c.String(),
                        SundayDate = c.DateTime(nullable: false, storeType: "date"),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.AttendanceDateKey)
                .Index(t => t.AttendanceTypeId)
                .Index(t => t.LocationId)
                .Index(t => t.CampusId)
                .Index(t => t.ScheduleId)
                .Index(t => t.GroupId)
                .Index(t => t.DeviceId)
                .Index(t => t.SearchTypeValueId)
                .Index(t => t.StartDateTime)
                .Index(t => t.Guid, unique: true);
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.AnalyticsSourceAttendance", new[] { "Guid" });
            DropIndex("dbo.AnalyticsSourceAttendance", new[] { "StartDateTime" });
            DropIndex("dbo.AnalyticsSourceAttendance", new[] { "SearchTypeValueId" });
            DropIndex("dbo.AnalyticsSourceAttendance", new[] { "DeviceId" });
            DropIndex("dbo.AnalyticsSourceAttendance", new[] { "GroupId" });
            DropIndex("dbo.AnalyticsSourceAttendance", new[] { "ScheduleId" });
            DropIndex("dbo.AnalyticsSourceAttendance", new[] { "CampusId" });
            DropIndex("dbo.AnalyticsSourceAttendance", new[] { "LocationId" });
            DropIndex("dbo.AnalyticsSourceAttendance", new[] { "AttendanceTypeId" });
            DropIndex("dbo.AnalyticsSourceAttendance", new[] { "AttendanceDateKey" });
        }
    }
}
