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
    public partial class CustomSundayDate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attendance.SundayDate might still exist due to the Rock.Migrations.AttendanceOccurrence migration not dropping the column, and waiting for the Rock.Jobs.MigrateAttendanceOccurrenceData to do it
            // However, it could have safely dropped the column in the migration since Rock.Jobs.MigrateAttendanceOccurrenceData doesn't need it for anything
            // So, just in case, drop the SundayDate column and index if they still exist
            Sql( @"IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_SundayDate' AND object_id = OBJECT_ID('Attendance'))
BEGIN
    DROP INDEX [IX_SundayDate] ON [dbo].[Attendance]
END" );

            Sql( @"
if (exists (SELECT * FROM information_schema.COLUMNS WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'SundayDate')) 
begin
     ALTER TABLE [Attendance] DROP COLUMN [SundayDate]
end
" );

            Sql( MigrationSQL._201910281713564_CustomSundayDate_ufnUtility_GetSundayDate );
            Sql( MigrationSQL._201910281713564_CustomSundayDate_ufnUtility_GetSundayDateRange );
            Sql( MigrationSQL._201910281713564_CustomSundayDate_spCheckin_WeeksAttendedInDuration );
            Sql( MigrationSQL._201910281713564_CustomSundayDate_spCheckin_AttendanceAnalyticsQuery_NonAttendees );
            Sql( MigrationSQL._201910281713564_CustomSundayDate_spCheckin_AttendanceAnalyticsQuery_Attendees );
            Sql( MigrationSQL._201910281713564_CustomSundayDate_spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance );
            Sql( MigrationSQL._201910281713564_CustomSundayDate_spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates );
            Sql( MigrationSQL._201910281713564_CustomSundayDate_spCheckin_AttendanceAnalyticsQuery_AttendeeDates );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
