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
using System.Linq;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for the SQL Query to complete. Leave blank to use the default for this job (3600). Note, it could take several minutes, so you might want to set it at 3600 (60 minutes) or higher", false, 60 * 60, "General", 1, "CommandTimeout" )]
    public class MigrateAttendanceOccurrenceData : IJob
    {
        private int _commandTimeout = 0;

        private int _remainingAttendanceRecords = 0;
        private int _occurrenceRecordsAdded = 0;
        private int _attendanceRecordsUpdated = 0;

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>

        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            _commandTimeout = dataMap.GetString( "CommandTimeout" ).AsIntegerOrNull() ?? 3600;

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = _commandTimeout;
                _remainingAttendanceRecords = rockContext.Database.SqlQuery<int>( $@"
    SELECT COUNT(*) FROM [Attendance] WHERE [OccurrenceId] = 1
" ).First();

                if ( _remainingAttendanceRecords == 0 )
                {
                    // drop the indexes and columns
                    rockContext.Database.ExecuteSqlCommand( @"
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_OccurrenceId' AND object_id = OBJECT_ID('Attendance'))
BEGIN
    CREATE INDEX [IX_OccurrenceId] ON [dbo].[Attendance]([OccurrenceId])
END

IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_GroupId' AND object_id = OBJECT_ID('Attendance'))
BEGIN
    DROP INDEX [IX_GroupId] ON [dbo].[Attendance]
END

IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_GroupId_StartDateTime_DidAttend' AND object_id = OBJECT_ID('Attendance'))
BEGIN
    DROP INDEX [IX_GroupId_StartDateTime_DidAttend] ON [dbo].[Attendance]
END

IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_PersonAliasId' AND object_id = OBJECT_ID('Attendance'))
BEGIN
    DROP INDEX [IX_PersonAliasId] ON [dbo].[Attendance]
END

IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_StartDateTime_DidAttend' AND object_id = OBJECT_ID('Attendance'))
BEGIN
    DROP INDEX [IX_StartDateTime_DidAttend] ON [dbo].[Attendance]
END

IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_SundayDate' AND object_id = OBJECT_ID('Attendance'))
BEGIN
    DROP INDEX [IX_SundayDate] ON [dbo].[Attendance]
END

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_PersonAliasId' AND object_id = OBJECT_ID('Attendance'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_PersonAliasId] ON [dbo].[Attendance]
    (
	    [PersonAliasId] ASC
    )
    INCLUDE ( 	[Id],
	    [StartDateTime],
	    [DidAttend],
	    [CampusId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_StartDateTime_DidAttend' AND object_id = OBJECT_ID('Attendance'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_StartDateTime_DidAttend] ON [dbo].[Attendance]
    (
	    [StartDateTime] ASC,
	    [DidAttend] ASC
    )
    INCLUDE ( 	[Id],
	    [CampusId],
	    [PersonAliasId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

ALTER TABLE [dbo].[Attendance] 
    DROP CONSTRAINT [FK_dbo.Attendance_dbo.Group_GroupId], [FK_dbo.Attendance_dbo.Location_LocationId], [FK_dbo.Attendance_dbo.Schedule_ScheduleId]

ALTER TABLE [dbo].[Attendance] 
    DROP COLUMN [LocationId], [ScheduleId], [GroupId], [DidNotOccur], [SundayDate]

IF NOT EXISTS ( SELECT [Id] FROM [Attendance] WHERE [OccurrenceId] = 1 )
BEGIN
    DELETE [AttendanceOccurrence] WHERE [Id] = 1
END 
" );
                    
                    // delete job if there are no unlined attendance records
                    var jobId = context.GetJobId();
                    var jobService = new ServiceJobService( rockContext );
                    var job = jobService.Get( jobId );
                    if ( job != null )
                    {
                        jobService.Delete( job );
                        rockContext.SaveChanges();
                        return;
                    }
                }
            }

            MigrateAttendanceData( context );

            context.UpdateLastStatusMessage( $@"Attendance Records Read: {_attendanceRecordsUpdated}, 
Occurrence Records Inserted: { _occurrenceRecordsAdded}, 
Attendance Records Updated: { _attendanceRecordsUpdated}
" );
        }

        /// <summary>
        /// Migrates the page views data.
        /// </summary>
        /// <param name="context">The context.</param>
        private void MigrateAttendanceData( IJobExecutionContext context )
        {
            using ( var rockContext = new RockContext() )
            {
                var sqlCreateOccurrenceRecords = $@"
INSERT INTO [AttendanceOccurrence] (
     [GroupId]
    ,[LocationId]
    ,[ScheduleId]
    ,[OccurrenceDate]
    ,[DidNotOccur]    
    ,[Guid]
)
SELECT 
    [GroupId],
    [LocationId],
    [ScheduleId],
    [OccurrenceDate],
    [DidNotOccur],
    NEWID()
FROM (
SELECT DISTINCT 
        A.[GroupId],
        A.[LocationId],
        A.[ScheduleId],
        A.[OccurrenceDate],
        A.[DidNotOccur]
    FROM (
		SELECT 
        A.[GroupId],
        A.[LocationId],
        A.[ScheduleId],
        CAST(A.[StartDateTime] AS DATE) AS [OccurrenceDate],
		max(cast(a.DidNotOccur as int) ) [DidNotOccur]
    FROM [Attendance] a
	GROUP BY A.[GroupId],
        A.[LocationId],
        A.[ScheduleId],
        CAST(A.[StartDateTime] AS DATE)
	) A
        LEFT OUTER JOIN [AttendanceOccurrence] O
            ON ((O.[GroupId] = A.[GroupId]) or (o.GroupId is null and a.GroupId is null))
            AND ((O.[LocationId] = A.[LocationId]) or (o.LocationId is null and a.LocationId is null))
            AND ((O.[ScheduleId] = A.[ScheduleId]) or (o.ScheduleId is null and a.ScheduleId is null))
            AND O.[OccurrenceDate] = A.[OccurrenceDate]
    WHERE O.[Id] IS NULL
) x
";

                var sqlUpdateAttendanceRecords = @"
UPDATE A 
    SET [OccurrenceId] = O.[Id]
FROM 
    [Attendance] A
    INNER JOIN [AttendanceOccurrence] O
        ON ((O.[GroupId] = A.[GroupId]) or (o.GroupId is null and a.GroupId is null))
        AND ((O.[LocationId] = A.[LocationId]) or (o.LocationId is null and a.LocationId is null))
        AND ((O.[ScheduleId] = A.[ScheduleId]) or (o.ScheduleId is null and a.ScheduleId is null))
        AND O.[OccurrenceDate] = CAST(A.[StartDateTime] AS DATE)
WHERE 
    A.[OccurrenceId] = 1
";

                rockContext.Database.CommandTimeout = _commandTimeout;
                _occurrenceRecordsAdded += rockContext.Database.ExecuteSqlCommand( sqlCreateOccurrenceRecords );
                _attendanceRecordsUpdated = rockContext.Database.ExecuteSqlCommand( sqlUpdateAttendanceRecords );

            }
        }

    }
}
