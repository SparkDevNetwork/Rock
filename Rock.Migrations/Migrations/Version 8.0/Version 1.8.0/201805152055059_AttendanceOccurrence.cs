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
    public partial class AttendanceOccurrence : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.AttendanceOccurrence",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(),
                        LocationId = c.Int(),
                        ScheduleId = c.Int(),
                        OccurrenceDate = c.DateTime(nullable: false, storeType: "date"),
                        DidNotOccur = c.Boolean(),
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
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.Group", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.Location", t => t.LocationId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.Schedule", t => t.ScheduleId, cascadeDelete: true)
                .Index(t => t.GroupId)
                .Index(t => t.LocationId)
                .Index(t => t.ScheduleId)
                .Index(t => t.OccurrenceDate)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);

            Sql( @"
    ALTER TABLE dbo.AttendanceOccurrence ADD SundayDate AS (dbo.ufnUtility_GetSundayDate(OccurrenceDate)) persisted
    CREATE INDEX IX_SundayDate ON AttendanceOccurrence (SundayDate)
" );
            Sql( @"
    CREATE UNIQUE INDEX IX_GroupId_LocationID_ScheduleID_Date ON dbo.AttendanceOccurrence (GroupId, LocationId, ScheduleId, OccurrenceDate );
" );

            Sql( $@"
    SET IDENTITY_INSERT dbo.AttendanceOccurrence ON
" );

            Sql( $@"
    INSERT INTO [AttendanceOccurrence]
        ( [Id], [OccurrenceDate], [Guid] )
    VALUES
        ( 1, '1/1/1900', NEWID() ) 
" );

            Sql( $@"
    SET IDENTITY_INSERT dbo.AttendanceOccurrence OFF
" );

            AddColumn("dbo.Attendance", "OccurrenceId", c => c.Int(nullable: false, defaultValue: 1 ) );
            AddForeignKey( "dbo.Attendance", "OccurrenceId", "dbo.AttendanceOccurrence", "Id", cascadeDelete: false );

            // Job for Migrating Interaction Data (schedule for 9pm to avoid conflict with AppPoolRecycle)
            Sql( $@"
    INSERT INTO [dbo].[ServiceJob]
           ([IsSystem]
           ,[IsActive]
           ,[Name]
           ,[Description]
           ,[Class]
           ,[CronExpression]
           ,[NotificationStatus]
           ,[Guid])
     VALUES
        (0	
         ,1	
         ,'Move data from Attendance table to new Attendance Occurrence table'
         ,'Moves group/location/schedule/date information from the attendance table to a parent occurrence table. Once all data has been moved, it will drop those columns from the attendance table, and then the job will remove itself.'
         ,'Rock.Jobs.MigrateAttendanceOccurrenceData'
         ,'0 0 21 1/1 * ? *'
         ,3
         ,'{ SystemGuid.ServiceJob.MIGRATE_ATTENDANCE_OCCURRENCE }')" );

            Sql( MigrationSQL._201805152055059_AttendanceOccurrence_spAnalytics_ETL_Attendance );
            Sql( MigrationSQL._201805152055059_AttendanceOccurrence_spCheckin_AttendanceAnalyticsQuery_AttendeeDates );
            Sql( MigrationSQL._201805152055059_AttendanceOccurrence_spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates );
            Sql( MigrationSQL._201805152055059_AttendanceOccurrence_spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance );
            Sql( MigrationSQL._201805152055059_AttendanceOccurrence_spCheckin_AttendanceAnalyticsQuery_Attendees );
            Sql( MigrationSQL._201805152055059_AttendanceOccurrence_spCheckin_AttendanceAnalyticsQuery_NonAttendees );
            Sql( MigrationSQL._201805152055059_AttendanceOccurrence_spCheckin_BadgeAttendance );
            Sql( MigrationSQL._201805152055059_AttendanceOccurrence_spCheckin_WeeksAttendedInDuration );
            Sql( MigrationSQL._201805152055059_AttendanceOccurrence_spCrm_FamilyAnalyticsAttendance );
            Sql( MigrationSQL._201805152055059_AttendanceOccurrence_spCrm_FamilyAnalyticsEraDataset );
            Sql( MigrationSQL._201805152055059_AttendanceOccurrence_spCrm_FamilyAnalyticsUpdateVisitDates );
            Sql( MigrationSQL._201805152055059_AttendanceOccurrence_vCheckin_Attendance );
            Sql( MigrationSQL._201805152055059_AttendanceOccurrence_vCheckin_GroupTypeAttendance );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Attendance", "SundayDate", c => c.DateTime(nullable: false, storeType: "date"));
            AddColumn("dbo.Attendance", "DidNotOccur", c => c.Boolean());
            AddColumn("dbo.Attendance", "GroupId", c => c.Int());
            AddColumn("dbo.Attendance", "ScheduleId", c => c.Int());
            AddColumn("dbo.Attendance", "LocationId", c => c.Int());
            DropForeignKey("dbo.Attendance", "OccurrenceId", "dbo.AttendanceOccurrence");
            DropForeignKey("dbo.AttendanceOccurrence", "ScheduleId", "dbo.Schedule");
            DropForeignKey("dbo.AttendanceOccurrence", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.AttendanceOccurrence", "LocationId", "dbo.Location");
            DropForeignKey("dbo.AttendanceOccurrence", "GroupId", "dbo.Group");
            DropForeignKey("dbo.AttendanceOccurrence", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.AttendanceOccurrence", new[] { "Guid" });
            DropIndex("dbo.AttendanceOccurrence", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.AttendanceOccurrence", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.AttendanceOccurrence", new[] { "OccurrenceDate" });
            DropIndex("dbo.AttendanceOccurrence", new[] { "ScheduleId" });
            DropIndex("dbo.AttendanceOccurrence", new[] { "LocationId" });
            DropIndex("dbo.AttendanceOccurrence", new[] { "GroupId" });
            DropIndex("dbo.Attendance", new[] { "OccurrenceId" });
            DropColumn("dbo.Attendance", "OccurrenceId");
            DropTable("dbo.AttendanceOccurrence");
            CreateIndex("dbo.Attendance", "GroupId");
            CreateIndex("dbo.Attendance", "ScheduleId");
            CreateIndex("dbo.Attendance", "LocationId");
            AddForeignKey("dbo.Attendance", "ScheduleId", "dbo.Schedule", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Attendance", "LocationId", "dbo.Location", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Attendance", "GroupId", "dbo.Group", "Id", cascadeDelete: true);
        }
    }
}
