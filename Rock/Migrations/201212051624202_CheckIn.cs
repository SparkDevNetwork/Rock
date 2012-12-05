//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Data.Spatial;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class CheckIn : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.Attendance",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LocationId = c.Int(),
                        ScheduleId = c.Int(),
                        GroupId = c.Int(),
                        PersonId = c.Int(),
                        QualifierValueId = c.Int(),
                        StartDateTime = c.DateTime(nullable: false),
                        EndDateTime = c.DateTime(),
                        DidAttend = c.Boolean(nullable: false),
                        SecurityCode = c.String(maxLength: 10),
                        Note = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Location", t => t.LocationId, cascadeDelete: true)
                .ForeignKey("dbo.Schedule", t => t.ScheduleId, cascadeDelete: true)
                .ForeignKey("dbo.Group", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.Person", t => t.PersonId, cascadeDelete: true)
                .ForeignKey("dbo.DefinedValue", t => t.QualifierValueId)
                .Index(t => t.LocationId)
                .Index(t => t.ScheduleId)
                .Index(t => t.GroupId)
                .Index(t => t.PersonId)
                .Index(t => t.QualifierValueId);
            
            CreateIndex( "dbo.Attendance", "Guid", true );
            CreateTable(
                "dbo.Device",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(),
                        GeoPoint = c.Geography(),
                        GeoFence = c.Geography(),
                        DeviceTypeValueId = c.Int(nullable: false),
                        IPAddress = c.String(maxLength: 45),
                        PrinterId = c.Int(),
                        PrintFrom = c.Int(nullable: false),
                        PrintToOverride = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Device", t => t.PrinterId)
                .ForeignKey("dbo.DefinedValue", t => t.DeviceTypeValueId)
                .Index(t => t.PrinterId)
                .Index(t => t.DeviceTypeValueId);
            
            CreateIndex( "dbo.Device", "Name", true );
            CreateIndex( "dbo.Device", "Guid", true );
            CreateTable(
                "dbo.Schedule",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Frequency = c.Int(nullable: false),
                        FrequencyQualifier = c.String(maxLength: 100),
                        StartTime = c.DateTime(nullable: false),
                        EndTime = c.DateTime(nullable: false),
                        CheckInStartTime = c.DateTime(),
                        CheckInEndTime = c.DateTime(),
                        EffectiveStartDate = c.DateTimeOffset(),
                        EffectiveEndDate = c.DateTimeOffset(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo.Schedule", "Name", true );
            CreateIndex( "dbo.Schedule", "Guid", true );
            CreateTable(
                "dbo.DeviceLocation",
                c => new
                    {
                        DeviceId = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.DeviceId, t.LocationId })
                .ForeignKey("dbo.Device", t => t.DeviceId, cascadeDelete: true)
                .ForeignKey("dbo.Location", t => t.LocationId, cascadeDelete: true)
                .Index(t => t.DeviceId)
                .Index(t => t.LocationId);
            
            // TableName: DeviceLocation
            // The given key was not present in the dictionary.
            // TableName: DeviceLocation
            // The given key was not present in the dictionary.
            CreateTable(
                "dbo.GroupLocationSchedule",
                c => new
                    {
                        GroupLocationId = c.Int(nullable: false),
                        ScheduleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.GroupLocationId, t.ScheduleId })
                .ForeignKey("dbo.GroupLocation", t => t.GroupLocationId, cascadeDelete: true)
                .ForeignKey("dbo.Schedule", t => t.ScheduleId, cascadeDelete: true)
                .Index(t => t.GroupLocationId)
                .Index(t => t.ScheduleId);
            
            // TableName: GroupLocationSchedule
            // The given key was not present in the dictionary.
            // TableName: GroupLocationSchedule
            // The given key was not present in the dictionary.
            AddColumn("dbo.GroupLocation", "Id", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.GroupLocation", "Guid", c => c.Guid(nullable: false));
            CreateIndex( "dbo.GroupLocation", "Guid", true );
            AddColumn("dbo.Location", "ParentLocationId", c => c.Int());
            AddColumn("dbo.Location", "Name", c => c.String(maxLength: 100));
            AddColumn("dbo.Location", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.Location", "LocationPoint", c => c.Geography());
            AddColumn("dbo.Location", "Perimeter", c => c.Geography());
            AddColumn("dbo.Location", "LocationTypeValueId", c => c.Int());
            AddColumn("dbo.Location", "FullAddress", c => c.String(maxLength: 400));
            AddColumn("dbo.Location", "AssessorParcelId", c => c.String(maxLength: 50));
            AddColumn("dbo.Location", "StandardizeAttemptedDateTime", c => c.DateTime());
            AddColumn("dbo.Location", "StandardizeAttemptedServiceType", c => c.String(maxLength: 50));
            AddColumn("dbo.Location", "StandardizeAttemptedResult", c => c.String(maxLength: 50));
            AddColumn("dbo.Location", "StandardizedDateTime", c => c.DateTime());
            AddColumn("dbo.Location", "GeocodeAttemptedDateTime", c => c.DateTime());
            AddColumn("dbo.Location", "GeocodeAttemptedServiceType", c => c.String(maxLength: 50));
            AddColumn("dbo.Location", "GeocodeAttemptedResult", c => c.String(maxLength: 50));
            AddColumn("dbo.Location", "GeocodedDateTime", c => c.DateTime());
            AddColumn("dbo.Location", "AttendancePrinterId", c => c.Int());
            AddColumn("dbo.GroupType", "GroupTerm", c => c.String(maxLength: 100));
            AddColumn("dbo.GroupType", "GroupMemberTerm", c => c.String(maxLength: 100));
            AddColumn("dbo.GroupType", "AllowMultipleLocations", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupType", "SmallIconFileId", c => c.Int());
            AddColumn("dbo.GroupType", "LargeIconFileId", c => c.Int());
            AddColumn("dbo.GroupType", "TakesAttendance", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupType", "AttendanceRule", c => c.Int(nullable: false));
            AddColumn("dbo.GroupType", "AttendancePrintTo", c => c.Int(nullable: false));
            AddColumn("dbo.GroupType", "SmallIcon_Id", c => c.Int());
            AddColumn("dbo.GroupType", "LargeIcon_Id", c => c.Int());
            AddColumn("dbo.GroupRole", "IsLeader", c => c.Boolean(nullable: false));
            DropPrimaryKey("dbo.GroupLocation", new[] { "GroupId", "LocationId" });
            AddPrimaryKey("dbo.GroupLocation", "Id");
            AddForeignKey("dbo.Location", "ParentLocationId", "dbo.Location", "Id");
            AddForeignKey("dbo.Location", "LocationTypeValueId", "dbo.DefinedValue", "Id");
            AddForeignKey("dbo.Location", "AttendancePrinterId", "dbo.Device", "Id");
            AddForeignKey("dbo.GroupType", "SmallIcon_Id", "dbo.BinaryFile", "Id");
            AddForeignKey("dbo.GroupType", "LargeIcon_Id", "dbo.BinaryFile", "Id");
            CreateIndex("dbo.Location", "ParentLocationId");
            CreateIndex("dbo.Location", "LocationTypeValueId");
            CreateIndex("dbo.Location", "AttendancePrinterId");
            CreateIndex("dbo.GroupType", "SmallIcon_Id");
            CreateIndex("dbo.GroupType", "LargeIcon_Id");
            DropColumn("dbo.Location", "Raw");
            DropColumn("dbo.Location", "Latitude");
            DropColumn("dbo.Location", "Longitude");
            DropColumn("dbo.Location", "ParcelId");
            DropColumn("dbo.Location", "StandardizeAttempt");
            DropColumn("dbo.Location", "StandardizeService");
            DropColumn("dbo.Location", "StandardizeResult");
            DropColumn("dbo.Location", "StandardizeDate");
            DropColumn("dbo.Location", "GeocodeAttempt");
            DropColumn("dbo.Location", "GeocodeService");
            DropColumn("dbo.Location", "GeocodeResult");
            DropColumn("dbo.Location", "GeocodeDate");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Location", "GeocodeDate", c => c.DateTime());
            AddColumn("dbo.Location", "GeocodeResult", c => c.String(maxLength: 50));
            AddColumn("dbo.Location", "GeocodeService", c => c.String(maxLength: 50));
            AddColumn("dbo.Location", "GeocodeAttempt", c => c.DateTime());
            AddColumn("dbo.Location", "StandardizeDate", c => c.DateTime());
            AddColumn("dbo.Location", "StandardizeResult", c => c.String(maxLength: 50));
            AddColumn("dbo.Location", "StandardizeService", c => c.String(maxLength: 50));
            AddColumn("dbo.Location", "StandardizeAttempt", c => c.DateTime());
            AddColumn("dbo.Location", "ParcelId", c => c.String(maxLength: 50));
            AddColumn("dbo.Location", "Longitude", c => c.Double());
            AddColumn("dbo.Location", "Latitude", c => c.Double());
            AddColumn("dbo.Location", "Raw", c => c.String(maxLength: 400));
            DropIndex("dbo.GroupLocationSchedule", new[] { "ScheduleId" });
            DropIndex("dbo.GroupLocationSchedule", new[] { "GroupLocationId" });
            DropIndex("dbo.DeviceLocation", new[] { "LocationId" });
            DropIndex("dbo.DeviceLocation", new[] { "DeviceId" });
            DropIndex("dbo.GroupType", new[] { "LargeIcon_Id" });
            DropIndex("dbo.GroupType", new[] { "SmallIcon_Id" });
            DropIndex("dbo.Device", new[] { "DeviceTypeValueId" });
            DropIndex("dbo.Device", new[] { "PrinterId" });
            DropIndex("dbo.Location", new[] { "AttendancePrinterId" });
            DropIndex("dbo.Location", new[] { "LocationTypeValueId" });
            DropIndex("dbo.Location", new[] { "ParentLocationId" });
            DropIndex("dbo.Attendance", new[] { "QualifierValueId" });
            DropIndex("dbo.Attendance", new[] { "PersonId" });
            DropIndex("dbo.Attendance", new[] { "GroupId" });
            DropIndex("dbo.Attendance", new[] { "ScheduleId" });
            DropIndex("dbo.Attendance", new[] { "LocationId" });
            DropForeignKey("dbo.GroupLocationSchedule", "ScheduleId", "dbo.Schedule");
            DropForeignKey("dbo.GroupLocationSchedule", "GroupLocationId", "dbo.GroupLocation");
            DropForeignKey("dbo.DeviceLocation", "LocationId", "dbo.Location");
            DropForeignKey("dbo.DeviceLocation", "DeviceId", "dbo.Device");
            DropForeignKey("dbo.GroupType", "LargeIcon_Id", "dbo.BinaryFile");
            DropForeignKey("dbo.GroupType", "SmallIcon_Id", "dbo.BinaryFile");
            DropForeignKey("dbo.Device", "DeviceTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.Device", "PrinterId", "dbo.Device");
            DropForeignKey("dbo.Location", "AttendancePrinterId", "dbo.Device");
            DropForeignKey("dbo.Location", "LocationTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.Location", "ParentLocationId", "dbo.Location");
            DropForeignKey("dbo.Attendance", "QualifierValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.Attendance", "PersonId", "dbo.Person");
            DropForeignKey("dbo.Attendance", "GroupId", "dbo.Group");
            DropForeignKey("dbo.Attendance", "ScheduleId", "dbo.Schedule");
            DropForeignKey("dbo.Attendance", "LocationId", "dbo.Location");
            DropPrimaryKey("dbo.GroupLocation", new[] { "Id" });
            AddPrimaryKey("dbo.GroupLocation", new[] { "GroupId", "LocationId" });
            DropColumn("dbo.GroupRole", "IsLeader");
            DropColumn("dbo.GroupType", "LargeIcon_Id");
            DropColumn("dbo.GroupType", "SmallIcon_Id");
            DropColumn("dbo.GroupType", "AttendancePrintTo");
            DropColumn("dbo.GroupType", "AttendanceRule");
            DropColumn("dbo.GroupType", "TakesAttendance");
            DropColumn("dbo.GroupType", "LargeIconFileId");
            DropColumn("dbo.GroupType", "SmallIconFileId");
            DropColumn("dbo.GroupType", "AllowMultipleLocations");
            DropColumn("dbo.GroupType", "GroupMemberTerm");
            DropColumn("dbo.GroupType", "GroupTerm");
            DropColumn("dbo.Location", "AttendancePrinterId");
            DropColumn("dbo.Location", "GeocodedDateTime");
            DropColumn("dbo.Location", "GeocodeAttemptedResult");
            DropColumn("dbo.Location", "GeocodeAttemptedServiceType");
            DropColumn("dbo.Location", "GeocodeAttemptedDateTime");
            DropColumn("dbo.Location", "StandardizedDateTime");
            DropColumn("dbo.Location", "StandardizeAttemptedResult");
            DropColumn("dbo.Location", "StandardizeAttemptedServiceType");
            DropColumn("dbo.Location", "StandardizeAttemptedDateTime");
            DropColumn("dbo.Location", "AssessorParcelId");
            DropColumn("dbo.Location", "FullAddress");
            DropColumn("dbo.Location", "LocationTypeValueId");
            DropColumn("dbo.Location", "Perimeter");
            DropColumn("dbo.Location", "LocationPoint");
            DropColumn("dbo.Location", "IsActive");
            DropColumn("dbo.Location", "Name");
            DropColumn("dbo.Location", "ParentLocationId");
            DropColumn("dbo.GroupLocation", "Guid");
            DropColumn("dbo.GroupLocation", "Id");
            DropTable("dbo.GroupLocationSchedule");
            DropTable("dbo.DeviceLocation");
            DropTable("dbo.Schedule");
            DropTable("dbo.Device");
            DropTable("dbo.Attendance");
        }
    }
}
