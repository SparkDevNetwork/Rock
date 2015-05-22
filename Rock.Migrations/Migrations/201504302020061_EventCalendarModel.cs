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
    public partial class EventCalendarModel : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.CalendarItemAudience",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventItemId = c.Int(nullable: false),
                        DefinedValueId = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.DefinedValue", t => t.DefinedValueId)
                .ForeignKey("dbo.EventItem", t => t.EventItemId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.EventItemId)
                .Index(t => t.DefinedValueId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.EventItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Summary = c.String(),
                        Description = c.String(),
                        PhotoId = c.Int(),
                        DetailsUrl = c.String(),
                        IsActive = c.Boolean(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.CalendarItemSchedule",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventItemId = c.Int(nullable: false),
                        ScheduleId = c.Int(nullable: false),
                        ScheduleName = c.String(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.EventItem", t => t.EventItemId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.Schedule", t => t.ScheduleId)
                .Index(t => t.EventItemId)
                .Index(t => t.ScheduleId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.EventCalendarItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventCalendarId = c.Int(nullable: false),
                        EventItemId = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.EventCalendar", t => t.EventCalendarId)
                .ForeignKey("dbo.EventItem", t => t.EventItemId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.EventCalendarId)
                .Index(t => t.EventItemId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.EventCalendar",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        IconCssClass = c.String(),
                        IsActive = c.Boolean(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.EventItemCampus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventItemId = c.Int(nullable: false),
                        CampusId = c.Int(nullable: false),
                        Location = c.String(nullable: false),
                        ContactPersonAliasId = c.Int(nullable: false),
                        ContactPhone = c.String(nullable: false),
                        ContactEmail = c.String(nullable: false, maxLength: 75),
                        RegistrationUrl = c.String(),
                        CampusNote = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Campus", t => t.CampusId)
                .ForeignKey("dbo.PersonAlias", t => t.ContactPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.EventItem", t => t.EventItemId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.EventItemId)
                .Index(t => t.CampusId)
                .Index(t => t.ContactPersonAliasId)
                .Index(t => t.ContactEmail, name: "IX_Email")
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.CalendarItemAudience", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.CalendarItemAudience", "EventItemId", "dbo.EventItem");
            DropForeignKey("dbo.EventItem", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EventItemCampus", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EventItemCampus", "EventItemId", "dbo.EventItem");
            DropForeignKey("dbo.EventItemCampus", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EventItemCampus", "ContactPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EventItemCampus", "CampusId", "dbo.Campus");
            DropForeignKey("dbo.EventCalendarItem", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EventCalendarItem", "EventItemId", "dbo.EventItem");
            DropForeignKey("dbo.EventCalendarItem", "EventCalendarId", "dbo.EventCalendar");
            DropForeignKey("dbo.EventCalendar", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EventCalendar", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EventCalendarItem", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EventItem", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.CalendarItemSchedule", "ScheduleId", "dbo.Schedule");
            DropForeignKey("dbo.CalendarItemSchedule", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.CalendarItemSchedule", "EventItemId", "dbo.EventItem");
            DropForeignKey("dbo.CalendarItemSchedule", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.CalendarItemAudience", "DefinedValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.CalendarItemAudience", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.EventItemCampus", new[] { "ForeignId" });
            DropIndex("dbo.EventItemCampus", new[] { "Guid" });
            DropIndex("dbo.EventItemCampus", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.EventItemCampus", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.EventItemCampus", "IX_Email");
            DropIndex("dbo.EventItemCampus", new[] { "ContactPersonAliasId" });
            DropIndex("dbo.EventItemCampus", new[] { "CampusId" });
            DropIndex("dbo.EventItemCampus", new[] { "EventItemId" });
            DropIndex("dbo.EventCalendar", new[] { "ForeignId" });
            DropIndex("dbo.EventCalendar", new[] { "Guid" });
            DropIndex("dbo.EventCalendar", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.EventCalendar", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.EventCalendarItem", new[] { "ForeignId" });
            DropIndex("dbo.EventCalendarItem", new[] { "Guid" });
            DropIndex("dbo.EventCalendarItem", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.EventCalendarItem", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.EventCalendarItem", new[] { "EventItemId" });
            DropIndex("dbo.EventCalendarItem", new[] { "EventCalendarId" });
            DropIndex("dbo.CalendarItemSchedule", new[] { "ForeignId" });
            DropIndex("dbo.CalendarItemSchedule", new[] { "Guid" });
            DropIndex("dbo.CalendarItemSchedule", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.CalendarItemSchedule", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.CalendarItemSchedule", new[] { "ScheduleId" });
            DropIndex("dbo.CalendarItemSchedule", new[] { "EventItemId" });
            DropIndex("dbo.EventItem", new[] { "ForeignId" });
            DropIndex("dbo.EventItem", new[] { "Guid" });
            DropIndex("dbo.EventItem", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.EventItem", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.CalendarItemAudience", new[] { "ForeignId" });
            DropIndex("dbo.CalendarItemAudience", new[] { "Guid" });
            DropIndex("dbo.CalendarItemAudience", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.CalendarItemAudience", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.CalendarItemAudience", new[] { "DefinedValueId" });
            DropIndex("dbo.CalendarItemAudience", new[] { "EventItemId" });
            DropTable("dbo.EventItemCampus");
            DropTable("dbo.EventCalendar");
            DropTable("dbo.EventCalendarItem");
            DropTable("dbo.CalendarItemSchedule");
            DropTable("dbo.EventItem");
            DropTable("dbo.CalendarItemAudience");
        }
    }
}
