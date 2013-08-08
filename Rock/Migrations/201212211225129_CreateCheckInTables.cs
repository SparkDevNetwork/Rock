//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    //using System.Data.Spatial;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class CreateCheckInTables : RockMigration_2
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
                        PrinterDeviceId = c.Int(),
                        PrintFrom = c.Int(nullable: false),
                        PrintToOverride = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Device", t => t.PrinterDeviceId)
                .ForeignKey("dbo.DefinedValue", t => t.DeviceTypeValueId)
                .Index(t => t.PrinterDeviceId)
                .Index(t => t.DeviceTypeValueId);
            
            CreateIndex( "dbo.Device", "Name", true );
            CreateIndex( "dbo.Device", "Guid", true );
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
            AddColumn("dbo.GroupLocation", "Id", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.GroupLocation", "Guid", c => c.Guid(nullable: false));
            CreateIndex( "dbo.GroupLocation", "Guid", true );

            RenameColumn( "dbo.Location", "Raw", "FullAddress" );
            RenameColumn( "dbo.Location", "ParcelId", "AssessorParcelId" );
            RenameColumn( "dbo.Location", "StandardizeAttempt", "StandardizeAttemptedDateTime" );
            RenameColumn( "dbo.Location", "StandardizeService", "StandardizeAttemptedServiceType" );
            RenameColumn( "dbo.Location", "StandardizeResult", "StandardizeAttemptedResult" );
            RenameColumn( "dbo.Location", "StandardizeDate", "StandardizedDateTime" );
            RenameColumn( "dbo.Location", "GeocodeAttempt", "GeocodeAttemptedDateTime" );
            RenameColumn( "dbo.Location", "GeocodeService", "GeocodeAttemptedServiceType" );
            RenameColumn( "dbo.Location", "GeocodeResult", "GeocodeAttemptedResult" );
            RenameColumn( "dbo.Location", "GeocodeDate", "GeocodedDateTime" );

            AddColumn( "dbo.Location", "ParentLocationId", c => c.Int() );
            AddColumn("dbo.Location", "Name", c => c.String(maxLength: 100));
            AddColumn("dbo.Location", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.Location", "LocationPoint", c => c.Geography());
            AddColumn("dbo.Location", "Perimeter", c => c.Geography());
            AddColumn("dbo.Location", "LocationTypeValueId", c => c.Int());
            AddColumn("dbo.Location", "PrinterDeviceId", c => c.Int());
            AddColumn("dbo.GroupType", "GroupTerm", c => c.String(maxLength: 100));
            AddColumn("dbo.GroupType", "GroupMemberTerm", c => c.String(maxLength: 100));
            AddColumn("dbo.GroupType", "AllowMultipleLocations", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupType", "ShowInGroupList", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupType", "IconSmallFileId", c => c.Int());
            AddColumn("dbo.GroupType", "IconLargeFileId", c => c.Int());
            AddColumn("dbo.GroupType", "IconCssClass", c => c.String());
            AddColumn("dbo.GroupType", "TakesAttendance", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupType", "AttendanceRule", c => c.Int(nullable: false));
            AddColumn("dbo.GroupType", "AttendancePrintTo", c => c.Int(nullable: false));
            AddColumn("dbo.GroupRole", "IsLeader", c => c.Boolean(nullable: false));

            DropPrimaryKey("dbo.GroupLocation", new[] { "GroupId", "LocationId" });
            AddPrimaryKey("dbo.GroupLocation", "Id");
            AddForeignKey("dbo.Location", "ParentLocationId", "dbo.Location", "Id");
            AddForeignKey("dbo.Location", "LocationTypeValueId", "dbo.DefinedValue", "Id");
            AddForeignKey("dbo.Location", "PrinterDeviceId", "dbo.Device", "Id");
            AddForeignKey("dbo.GroupType", "IconSmallFileId", "dbo.BinaryFile", "Id");
            AddForeignKey("dbo.GroupType", "IconLargeFileId", "dbo.BinaryFile", "Id");
            CreateIndex("dbo.Location", "ParentLocationId");
            CreateIndex("dbo.Location", "LocationTypeValueId");
            CreateIndex("dbo.Location", "PrinterDeviceId");
            CreateIndex("dbo.GroupType", "IconSmallFileId");
            CreateIndex("dbo.GroupType", "IconLargeFileId");

            Sql( @"
    UPDATE [dbo].[Location] 
    SET [LocationPoint] = geography::STPointFromText('POINT(' + CAST([Longitude] AS VARCHAR(20)) + ' ' + CAST([Latitude] AS VARCHAR(20)) + ')', 4326)
" );

            DropColumn("dbo.Location", "Latitude");
            DropColumn("dbo.Location", "Longitude");
         }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            AddColumn("dbo.Location", "Longitude", c => c.Double());
            AddColumn("dbo.Location", "Latitude", c => c.Double());

            Sql( @"
    UPDATE [dbo].[Location] 
    SET [Longitude] = [LocationPoint].Long, [Latitude] = [LocationPoint].Lat
" );

            RenameColumn( "dbo.Location", "FullAddress", "Raw" );
            RenameColumn( "dbo.Location", "AssessorParcelId", "ParcelId" );
            RenameColumn( "dbo.Location", "StandardizeAttemptedDateTime", "StandardizeAttempt" );
            RenameColumn( "dbo.Location", "StandardizeAttemptedServiceType", "StandardizeService" );
            RenameColumn( "dbo.Location", "StandardizeAttemptedResult", "StandardizeResult" );
            RenameColumn( "dbo.Location", "StandardizedDateTime", "StandardizeDate" );
            RenameColumn( "dbo.Location", "GeocodeAttemptedDateTime", "GeocodeAttempt" );
            RenameColumn( "dbo.Location", "GeocodeAttemptedServiceType", "GeocodeService" );
            RenameColumn( "dbo.Location", "GeocodeAttemptedResult", "GeocodeResult" );
            RenameColumn( "dbo.Location", "GeocodedDateTime", "GeocodeDate" );

            DropIndex("dbo.DeviceLocation", new[] { "LocationId" });
            DropIndex("dbo.DeviceLocation", new[] { "DeviceId" });
            DropIndex("dbo.GroupLocationSchedule", new[] { "ScheduleId" });
            DropIndex("dbo.GroupLocationSchedule", new[] { "GroupLocationId" });
            DropIndex("dbo.Device", new[] { "DeviceTypeValueId" });
            DropIndex("dbo.Device", new[] { "PrinterDeviceId" });
            DropIndex("dbo.GroupType", new[] { "IconLargeFileId" });
            DropIndex("dbo.GroupType", new[] { "IconSmallFileId" });
            DropIndex("dbo.Location", new[] { "PrinterDeviceId" });
            DropIndex("dbo.Location", new[] { "LocationTypeValueId" });
            DropIndex("dbo.Location", new[] { "ParentLocationId" });
            DropIndex("dbo.Attendance", new[] { "QualifierValueId" });
            DropIndex("dbo.Attendance", new[] { "PersonId" });
            DropIndex("dbo.Attendance", new[] { "GroupId" });
            DropIndex("dbo.Attendance", new[] { "ScheduleId" });
            DropIndex("dbo.Attendance", new[] { "LocationId" });
            DropForeignKey("dbo.DeviceLocation", "LocationId", "dbo.Location");
            DropForeignKey("dbo.DeviceLocation", "DeviceId", "dbo.Device");
            DropForeignKey("dbo.GroupLocationSchedule", "ScheduleId", "dbo.Schedule");
            DropForeignKey("dbo.GroupLocationSchedule", "GroupLocationId", "dbo.GroupLocation");
            DropForeignKey("dbo.Device", "DeviceTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.Device", "PrinterDeviceId", "dbo.Device");
            DropForeignKey("dbo.GroupType", "IconLargeFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.GroupType", "IconSmallFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.Location", "PrinterDeviceId", "dbo.Device");
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
            DropColumn("dbo.GroupType", "AttendancePrintTo");
            DropColumn("dbo.GroupType", "AttendanceRule");
            DropColumn("dbo.GroupType", "TakesAttendance");
            DropColumn("dbo.GroupType", "IconCssClass");
            DropColumn("dbo.GroupType", "IconLargeFileId");
            DropColumn("dbo.GroupType", "IconSmallFileId");
            DropColumn("dbo.GroupType", "ShowInGroupList");
            DropColumn("dbo.GroupType", "AllowMultipleLocations");
            DropColumn("dbo.GroupType", "GroupMemberTerm");
            DropColumn("dbo.GroupType", "GroupTerm");
            DropColumn("dbo.Location", "PrinterDeviceId");
            DropColumn("dbo.Location", "LocationTypeValueId");
            DropColumn("dbo.Location", "Perimeter");
            DropColumn("dbo.Location", "LocationPoint");
            DropColumn("dbo.Location", "IsActive");
            DropColumn("dbo.Location", "Name");
            DropColumn("dbo.Location", "ParentLocationId");
            DropColumn("dbo.GroupLocation", "Guid");
            DropColumn("dbo.GroupLocation", "Id");
            DropTable("dbo.DeviceLocation");
            DropTable("dbo.GroupLocationSchedule");
            DropTable("dbo.Device");
            DropTable("dbo.Schedule");
            DropTable("dbo.Attendance");
        }
    }
}
