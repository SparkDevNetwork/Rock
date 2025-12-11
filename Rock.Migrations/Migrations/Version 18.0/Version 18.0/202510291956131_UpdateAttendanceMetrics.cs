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
    public partial class UpdateAttendanceMetrics : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixMetrics();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        public const string AdultAttendanceMeasurementGuid = Rock.SystemGuid.DefinedValue.MEASUREMENT_TOTAL_ADULT_ATTENDANCE;
        public const string WeekendAttendanceMeasurementGuid = Rock.SystemGuid.DefinedValue.MEASUREMENT_TOTAL_WEEKEND_ATTENDANCE;
        public const string StudentAttendanceMeasurementGuid = Rock.SystemGuid.DefinedValue.MEASUREMENT_TOTAL_STUDENTS_ATTENDANCE;
        public const string ChildrenAttendanceMeasurementGuid = Rock.SystemGuid.DefinedValue.MEASUREMENT_TOTAL_CHILDRENS_ATTENDANCE;
        public const string VolunteerAttendanceMeasurementGuid = Rock.SystemGuid.DefinedValue.MEASUREMENT_TOTAL_VOLUNTEER_ATTENDANCE;

        public const string AdultAttendanceMetricGuid = "0D126800-2FDA-4B34-96FD-9BAE76F3A89A";
        public const string WeekendAttendanceMetricGuid = "89553EEE-91F3-4169-9D7C-04A17471E035";
        public const string VolunteerAttendanceMetricGuid = "4F965AE3-D455-4346-988F-2A2B5E236C0C";
        public const string ChildrenAttendanceMetricGuid = "1747F42D-791D-41D5-BFFC-49D6C31B9549";
        public const string StudentAttendanceMetricGuid = "310E5B92-E744-4E69-A832-9E395191A91C";

        public const string WeekendAttendanceMetricSourceSql = @"DECLARE @StartDate DATETIME = CONVERT(DATE, DATEADD(DAY, -7, dbo.RockGetDate()));
DECLARE @EndDate DATETIME = CONVERT(DATE, dbo.RockGetDate());

DECLARE @StartDateKey INT = CONVERT(INT, CONVERT(CHAR(8), @StartDate, 112));
DECLARE @EndDateKey   INT = CONVERT(INT, CONVERT(CHAR(8), @EndDate, 112));

DECLARE @ChildrenMeasurementId INT = (
    SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '9B16A979-48B1-4180-B44F-57FCD38A103A'
);
DECLARE @StudentMeasurementId INT = (
    SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '8EC797E4-7DCE-4A70-B1E8-9B21192476C3'
);
DECLARE @VolunteerMeasurementId INT = (
    SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '6A2621BF-E600-428A-94C2-CCB79645FA27'
);
DECLARE @AdultMeasurementId INT = (
    SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '66C649EF-7569-4CE2-8EA0-FB9851F9A598'
);

SELECT
    SUM(mv.[YValue]) AS [AttendanceCount],
    CampusPartition.[EntityId] AS [CampusId],
    SchedulePartition.[EntityId] AS [ScheduleId]
FROM [MetricValue] mv
INNER JOIN [Metric] m ON m.[Id] = mv.[MetricId]
LEFT JOIN [MetricValuePartition] CampusPartition
    ON CampusPartition.[MetricValueId] = mv.[Id]
    AND CampusPartition.[MetricPartitionId] = (
        SELECT [Id]
        FROM [MetricPartition]
        WHERE [MetricId] = m.[Id]
          AND [EntityTypeId] = (
              SELECT [Id] FROM [EntityType]
              WHERE [Guid] = '00096BED-9587-415E-8AD4-4E076AE8FBF0'
          )
    )
LEFT JOIN [MetricValuePartition] SchedulePartition
    ON SchedulePartition.[MetricValueId] = mv.[Id]
    AND SchedulePartition.[MetricPartitionId] = (
        SELECT [Id]
        FROM [MetricPartition]
        WHERE [MetricId] = m.[Id]
          AND [EntityTypeId] = (
              SELECT [Id] FROM [EntityType]
              WHERE [Guid] = '0B2C38A7-D79C-4F85-9757-F1B045D32C8A'
          )
    )
WHERE
    m.[MeasurementClassificationValueId] IN (
        @ChildrenMeasurementId,
        @StudentMeasurementId,
        @VolunteerMeasurementId,
        @AdultMeasurementId
    )
    AND mv.[MetricValueDateKey] BETWEEN @StartDateKey AND @EndDateKey
GROUP BY
    CampusPartition.[EntityId],
    SchedulePartition.[EntityId];";

        public const string VolunteerAttendanceMetricSourceSql = @"
DECLARE @STARTDATE DATETIME = DATEADD(DAY, -7, GETDATE())
DECLARE @ENDDATE DATETIME = GETDATE()
DECLARE @ServiceAreaDefinedValueId INT = (SELECT Id FROM dbo.[DefinedValue] WHERE [Guid] = '36A554CE-7815-41B9-A435-93F3D52A2828')

SELECT COUNT(1) as AttendanceCount, a.[CampusId], oa.[ScheduleId]
FROM [Attendance] a
INNER JOIN [AttendanceOccurrence] oa ON oa.Id = a.[OccurrenceId]
INNER JOIN [Group] g ON g.Id = oa.[GroupId]
INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
WHERE
   gt.[GroupTypePurposeValueId] = @ServiceAreaDefinedValueId
   AND a.[DidAttend] = 1 
   AND a.[StartDateTime] BETWEEN @STARTDATE AND @ENDDATE
GROUP BY a.[CampusId], oa.[ScheduleId]
";

        public const string ChildrenAttendanceMetricSourceSql = @"
-- Feel free to replace this with your own SQL or updated Check-in Areas below.
DECLARE @STARTDATE DATETIME = CONVERT(date, DATEADD(DAY, -7, dbo.RockGetDate()))
DECLARE @ENDDATE DATETIME = CONVERT(date, dbo.RockGetDate())

SELECT COUNT(1) as AttendanceCount, a.[CampusId], oa.[ScheduleId]
FROM [Attendance] a
INNER JOIN [AttendanceOccurrence] oa ON oa.Id = a.[OccurrenceId]
INNER JOIN [Group] g ON g.Id = oa.[GroupId]
INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
WHERE
    gt.[AttendanceCountsAsWeekendService] = 1
    AND a.[DidAttend] = 1 
    AND CONVERT(date, a.[StartDateTime]) BETWEEN @STARTDATE AND @ENDDATE
    AND gt.[Guid] IN (
          'CADB2D12-7836-44BC-8EEA-3C6AB22FD5E8' -- Nursery/Preschool Area
        , 'E3C8F7D6-5CEB-43BB-802F-66C3E734049E' -- Elementary Area
    )
GROUP BY a.[CampusId], oa.[ScheduleId]
";

        public const string StudentAttendanceMetricSourceSql = @"
-- Feel free to replace this with your own SQL or updated Check-in Areas below.
DECLARE @STARTDATE DATETIME = CONVERT(date, DATEADD(DAY, -7, dbo.RockGetDate()))
DECLARE @ENDDATE DATETIME = CONVERT(date, dbo.RockGetDate())

SELECT COUNT(1) as AttendanceCount, a.[CampusId], oa.[ScheduleId]
FROM [Attendance] a
INNER JOIN [AttendanceOccurrence] oa ON oa.Id = a.[OccurrenceId]
INNER JOIN [Group] g ON g.Id = oa.[GroupId]
INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
WHERE
    gt.[AttendanceCountsAsWeekendService] = 1
    AND a.[DidAttend] = 1 
    AND CONVERT(date, a.[StartDateTime]) BETWEEN @STARTDATE AND @ENDDATE
    AND gt.[Guid] IN (
          '7A17235B-69AD-439B-BAB0-1A0A472DB96F' -- Jr High Area
        , '9A88743B-F336-4404-B877-2A623689195D' -- High School Area
    )
GROUP BY a.[CampusId], oa.[ScheduleId]
";

        public const string WeeklyMetricsCategoryGuid = "64B29ADE-144D-4E84-96CC-A79398589733";

        public const string WeeklySchedule = @"
BEGIN:VCALENDAR
PRODID:-//github.com/SparkDevNetwork/Rock//NONSGML Rock//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:20240610T040100
DTSTAMP:20240610T144847
DTSTART:20240610T040000
RRULE:FREQ=WEEKLY;BYDAY=MO
SEQUENCE:0
UID:65398ce3-3a71-4261-a52d-ad28e49840c3
END:VEVENT
END:VCALENDAR";


        private void FixMetrics()
        {
            // Delete Metric Values for Metrics that have incorrect Source SQL (just Attendance Metrics for now). Update GROUP BY ALL to GROUP BY.
            Sql( @"
DECLARE @TotalChildrenMetricId INT;
DECLARE @TotalStudentMetricId INT;
DECLARE @TotalWeekendMetricId INT;
DECLARE @VolunteerMetricId INT;

-- Total Children's Attendance Metric

SELECT TOP 1 @TotalChildrenMetricId = [Id]
FROM [Metric]
WHERE [SourceSql] = '
-- Feel free to replace this with your own SQL or updated Check-in Areas below.
DECLARE @STARTDATE DATETIME = CONVERT(date, DATEADD(DAY, -7, dbo.RockGetDate()))
DECLARE @ENDDATE DATETIME = CONVERT(date, dbo.RockGetDate())

SELECT COUNT(1) as AttendanceCount, a.[CampusId], oa.[ScheduleId]
FROM [Attendance] a
INNER JOIN [AttendanceOccurrence] oa ON oa.Id = a.[OccurrenceId]
INNER JOIN [Group] g ON g.Id = oa.[GroupId]
INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
WHERE
    gt.[AttendanceCountsAsWeekendService] = 1
    AND a.[DidAttend] = 1 
    AND CONVERT(date, a.[StartDateTime]) BETWEEN @STARTDATE AND @ENDDATE
    AND gt.[Guid] IN (
          ''CADB2D12-7836-44BC-8EEA-3C6AB22FD5E8'' -- Nursery/Preschool Area
        , ''E3C8F7D6-5CEB-43BB-802F-66C3E734049E'' -- Elementary Area
    )
GROUP BY ALL a.[CampusId], oa.[ScheduleId]
'
AND [Guid] = '1747F42D-791D-41D5-BFFC-49D6C31B9549';

IF @TotalChildrenMetricId IS NOT NULL
BEGIN
    -- Replace GROUP BY ALL with GROUP BY in Source SQL
    UPDATE [Metric]
    SET [SourceSql] = REPLACE([SourceSql], 'GROUP BY ALL', 'GROUP BY')
    WHERE [SourceSql] LIKE '%GROUP BY ALL%'
        AND [Id] = @TotalChildrenMetricId;

    -- Delete dependent MetricValuePartition rows first
    DELETE FROM [MetricValuePartition]
    WHERE [MetricValueId] IN (
        SELECT [Id]
        FROM [MetricValue]
        WHERE [MetricId] = @TotalChildrenMetricId
    );

    -- Then delete MetricValue rows
    DELETE FROM [MetricValue]
    WHERE [MetricId] = @TotalChildrenMetricId;
END

-- Total Student's Attendance Metric

SELECT TOP 1 @TotalStudentMetricId = [Id]
FROM [Metric]
WHERE [SourceSql] = '
-- Feel free to replace this with your own SQL or updated Check-in Areas below.
DECLARE @STARTDATE DATETIME = CONVERT(date, DATEADD(DAY, -7, dbo.RockGetDate()))
DECLARE @ENDDATE DATETIME = CONVERT(date, dbo.RockGetDate())

SELECT COUNT(1) as AttendanceCount, a.[CampusId], oa.[ScheduleId]
FROM [Attendance] a
INNER JOIN [AttendanceOccurrence] oa ON oa.Id = a.[OccurrenceId]
INNER JOIN [Group] g ON g.Id = oa.[GroupId]
INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
WHERE
    gt.[AttendanceCountsAsWeekendService] = 1
    AND a.[DidAttend] = 1 
    AND CONVERT(date, a.[StartDateTime]) BETWEEN @STARTDATE AND @ENDDATE
    AND gt.[Guid] IN (
          ''7A17235B-69AD-439B-BAB0-1A0A472DB96F'' -- Jr High Area
        , ''9A88743B-F336-4404-B877-2A623689195D'' -- High School Area
    )
GROUP BY ALL a.[CampusId], oa.[ScheduleId]
'
AND [Guid] = '310E5B92-E744-4E69-A832-9E395191A91C';

IF @TotalStudentMetricId IS NOT NULL
BEGIN
    -- Replace GROUP BY ALL with GROUP BY in Source SQL
    UPDATE [Metric]
    SET [SourceSql] = REPLACE([SourceSql], 'GROUP BY ALL', 'GROUP BY')
    WHERE [SourceSql] LIKE '%GROUP BY ALL%'
        AND [Id] = @TotalStudentMetricId;

    -- Delete dependent MetricValuePartition rows first
    DELETE FROM [MetricValuePartition]
    WHERE [MetricValueId] IN (
        SELECT [Id]
        FROM [MetricValue]
        WHERE [MetricId] = @TotalStudentMetricId
    );

    -- Then delete MetricValue rows
    DELETE FROM [MetricValue]
    WHERE [MetricId] = @TotalStudentMetricId;
END

-- Total Weekend Attendance Metric

SELECT TOP 1 @TotalWeekendMetricId = [Id]
FROM [Metric]
WHERE [SourceSql] = 'DECLARE @STARTDATE DATETIME = CONVERT(date, DATEADD(DAY, -7, dbo.RockGetDate()))
DECLARE @ENDDATE DATETIME = CONVERT(date, dbo.RockGetDate())

SELECT COUNT(1) as AttendanceCount, a.[CampusId], oa.[ScheduleId]
FROM [Attendance] a
	INNER JOIN [AttendanceOccurrence] oa ON oa.Id = a.[OccurrenceId]
	INNER JOIN [Group] g ON g.Id = oa.[GroupId]
	INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
WHERE
	gt.[AttendanceCountsAsWeekendService] = 1
	AND a.[DidAttend] = 1
	AND CONVERT(date, a.[StartDateTime]) BETWEEN @STARTDATE AND @ENDDATE
GROUP BY ALL a.[CampusId], oa.[ScheduleId]'
AND [Guid] = '89553EEE-91F3-4169-9D7C-04A17471E035';

IF @TotalWeekendMetricId IS NOT NULL
BEGIN
    -- Replace GROUP BY ALL with GROUP BY in Source SQL
    UPDATE [Metric]
    SET [SourceSql] = REPLACE([SourceSql], 'GROUP BY ALL', 'GROUP BY')
    WHERE [SourceSql] LIKE '%GROUP BY ALL%'
        AND [Id] = @TotalWeekendMetricId;

    -- Delete dependent MetricValuePartition rows first
    DELETE FROM [MetricValuePartition]
    WHERE [MetricValueId] IN (
        SELECT [Id]
        FROM [MetricValue]
        WHERE [MetricId] = @TotalWeekendMetricId
    );

    -- Then delete MetricValue rows
    DELETE FROM [MetricValue]
    WHERE [MetricId] = @TotalWeekendMetricId;
END

-- Volunteer Attendance Metric

SELECT TOP 1 @VolunteerMetricId = [Id]
FROM [Metric]
WHERE [SourceSql] = 'DECLARE @STARTDATE DATETIME = CONVERT(date, DATEADD(DAY, -7, dbo.RockGetDate()))
DECLARE @ENDDATE DATETIME = CONVERT(date, dbo.RockGetDate())
DECLARE @ServiceAreaDefinedValueId INT = (SELECT Id FROM dbo.[DefinedValue] WHERE [Guid] = ''36A554CE-7815-41B9-A435-93F3D52A2828'')

SELECT COUNT(1) as AttendanceCount, a.[CampusId], oa.[ScheduleId]
FROM [Attendance] a
INNER JOIN [AttendanceOccurrence] oa ON oa.Id = a.[OccurrenceId]
INNER JOIN [Group] g ON g.Id = oa.[GroupId]
INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
WHERE
   gt.[GroupTypePurposeValueId] = @ServiceAreaDefinedValueId
   AND a.[DidAttend] = 1 
   AND CONVERT(date, a.[StartDateTime]) BETWEEN @STARTDATE AND @ENDDATE
GROUP BY ALL a.[CampusId], oa.[ScheduleId]'
AND [Guid] = '4F965AE3-D455-4346-988F-2A2B5E236C0C';

IF @VolunteerMetricId IS NOT NULL
BEGIN
    -- Replace GROUP BY ALL with GROUP BY in Source SQL
    UPDATE [Metric]
    SET [SourceSql] = REPLACE([SourceSql], 'GROUP BY ALL', 'GROUP BY')
    WHERE [SourceSql] LIKE '%GROUP BY ALL%'
        AND [Id] = @VolunteerMetricId;

    -- Delete dependent MetricValuePartition rows first
    DELETE FROM [MetricValuePartition]
    WHERE [MetricValueId] IN (
        SELECT [Id]
        FROM [MetricValue]
        WHERE [MetricId] = @VolunteerMetricId
    );

    -- Then delete MetricValue rows
    DELETE FROM [MetricValue]
    WHERE [MetricId] = @VolunteerMetricId;
END
" );

            // Add the Total Adult Attendance Measurement Classification
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "Total Adult Attendance",
                "This metric measures the total adult service attendance for the given week. This metric should be partitioned by Campus > Service (Schedule).",
                 AdultAttendanceMeasurementGuid );

            // Create the Weekly Metrics Category if it is missing.
            Sql( $@"
IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[Category]
    WHERE [Guid] = '{WeeklyMetricsCategoryGuid}'
)
BEGIN
    INSERT INTO [dbo].[Category]
    (
          [IsSystem]
        , [EntityTypeId]
        , [Name]
        , [IconCssClass]
        , [Description]
        , [Order]
        , [Guid]
        , [ParentCategoryId]
    )
    VALUES
    (
          1
        , (SELECT [Id] FROM [EntityType] WHERE [Guid] = '3D35C859-DF37-433F-A20A-0FFD0FCB9862')
        , 'Weekly Metrics'
        , 'icon-fw fa fa-calendar-week'
        , ''
        , 0
        , '{WeeklyMetricsCategoryGuid}'
        , NULL
    );
END" );

            // Create the Volunteer Attendance Metric if it is missing
            Sql( $@"
DECLARE @MetricId [int];

IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[Metric]
    WHERE [Guid] = '{VolunteerAttendanceMetricGuid}'
)
BEGIN
    DECLARE @ScheduleId INT;

    SET @ScheduleId = (
        SELECT [Id]
        FROM [dbo].[Schedule]
        WHERE [Guid] = 'C31DF106-D7C8-4B64-81E7-5C4AB20DBA7B'
    );

    IF @ScheduleId IS NULL
    BEGIN
        INSERT INTO [dbo].[Schedule] (
            [Name],
            [Description],
            [iCalendarContent],
            [CategoryId],
            [Guid],
            [IsActive],
            [IsPublic]
        )
        VALUES (
            'Weekly Metric Schedule',
            NULL,
            '{WeeklySchedule}',
            (SELECT [Id] FROM [dbo].[Category] WHERE [Guid] = '5A794741-5444-43F0-90D7-48E47276D426'),
            'C31DF106-D7C8-4B64-81E7-5C4AB20DBA7B',
            1,
            0
        );

        SET @ScheduleId = SCOPE_IDENTITY();
    END

    INSERT INTO [dbo].[Metric] (
        [IsSystem],
        [Title],
        [Description],
        [IsCumulative],
        [SourceValueTypeId],
        [SourceSql],
        [ScheduleId],
        [CreatedDateTime],
        [ModifiedDateTime],
        [Guid],
        [NumericDataType],
        [EnableAnalytics],
        [MeasurementClassificationValueId]
    )
    VALUES (
        0,
        'Total Volunteer Attendance',
        'This metric represents attendance records (total for the week) for any group(s) per campus of GroupTypes that have a Purpose of Serving Area.',
        0,
        (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '6A1E1A1B-A636-4E12-B90C-D7FD1BDAE764'),
        '{VolunteerAttendanceMetricSourceSql.Replace( "'", "''" )}',
        @ScheduleId,
        SYSDATETIME(),
        SYSDATETIME(),
        '{VolunteerAttendanceMetricGuid}',
        1,
        0,
        (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{VolunteerAttendanceMeasurementGuid}')
    );

    SET @MetricId = SCOPE_IDENTITY();

    INSERT INTO [dbo].[MetricCategory] (
        [MetricId],
        [CategoryId],
        [Order],
        [Guid]
    )
    VALUES (
        @MetricId,
        (SELECT [Id] FROM [Category] WHERE [Guid] = '{WeeklyMetricsCategoryGuid}'),
        0,
        NEWID()
    );

    INSERT INTO [dbo].[MetricPartition] (
        [MetricId],
        [Label],
        [EntityTypeId],
        [IsRequired],
        [Order],
        [CreatedDateTime],
        [ModifiedDateTime],
        [Guid]
    )
    VALUES 
    (
        @MetricId,
        'Campus',
        (SELECT [Id] FROM [EntityType] WHERE [Guid] = '00096BED-9587-415E-8AD4-4E076AE8FBF0'),
        1,
        0,
        SYSDATETIME(),
        SYSDATETIME(),
        NEWID()
    ),
    (
        @MetricId,
        'Schedule',
        (SELECT [Id] FROM [EntityType] WHERE [Guid] = '0B2C38A7-D79C-4F85-9757-F1B045D32C8A'),
        1,
        1,
        SYSDATETIME(),
        SYSDATETIME(),
        NEWID()
    )
END
" );

            // Create the Total Children's Attendance Metric if it is missing
            Sql( $@"
DECLARE @MetricId [int];

IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[Metric]
    WHERE [Guid] = '{ChildrenAttendanceMetricGuid}'
)
BEGIN
    DECLARE @ScheduleId INT;

    SET @ScheduleId = (
        SELECT [Id]
        FROM [dbo].[Schedule]
        WHERE [Guid] = 'C31DF106-D7C8-4B64-81E7-5C4AB20DBA7B'
    );

    IF @ScheduleId IS NULL
    BEGIN
        INSERT INTO [dbo].[Schedule] (
            [Name],
            [Description],
            [iCalendarContent],
            [CategoryId],
            [Guid],
            [IsActive],
            [IsPublic]
        )
        VALUES (
            'Weekly Metric Schedule',
            NULL,
            '{WeeklySchedule}',
            (SELECT [Id] FROM [dbo].[Category] WHERE [Guid] = '5A794741-5444-43F0-90D7-48E47276D426'),
            'C31DF106-D7C8-4B64-81E7-5C4AB20DBA7B',
            1,
            0
        );

        SET @ScheduleId = SCOPE_IDENTITY();
    END

    INSERT INTO [dbo].[Metric] (
        [IsSystem],
        [Title],
        [Description],
        [IsCumulative],
        [SourceValueTypeId],
        [SourceSql],
        [ScheduleId],
        [CreatedDateTime],
        [ModifiedDateTime],
        [Guid],
        [NumericDataType],
        [EnableAnalytics],
        [MeasurementClassificationValueId]
    )
    VALUES (
        0,
        'Total Children''s Attendance',
        'This metric represents attendance records (total for the week) for any group(s) per campus of GroupTypes that have the Weekend Service field checked and are part of the Children''s Check-in Areas (Nursery/Preschool Area and Elementary Area).',
        0,
        (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '6A1E1A1B-A636-4E12-B90C-D7FD1BDAE764'),
        '{ChildrenAttendanceMetricSourceSql.Replace( "'", "''" )}',
        @ScheduleId,
        SYSDATETIME(),
        SYSDATETIME(),
        '{ChildrenAttendanceMetricGuid}',
        1,
        0,
        (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{ChildrenAttendanceMeasurementGuid}')
    );

    SET @MetricId = SCOPE_IDENTITY();

    INSERT INTO [dbo].[MetricCategory] (
        [MetricId],
        [CategoryId],
        [Order],
        [Guid]
    )
    VALUES (
        @MetricId,
        (SELECT [Id] FROM [Category] WHERE [Guid] = '{WeeklyMetricsCategoryGuid}'),
        0,
        NEWID()
    );

    INSERT INTO [dbo].[MetricPartition] (
        [MetricId],
        [Label],
        [EntityTypeId],
        [IsRequired],
        [Order],
        [CreatedDateTime],
        [ModifiedDateTime],
        [Guid]
    )
    VALUES 
    (
        @MetricId,
        'Campus',
        (SELECT [Id] FROM [EntityType] WHERE [Guid] = '00096BED-9587-415E-8AD4-4E076AE8FBF0'),
        1,
        0,
        SYSDATETIME(),
        SYSDATETIME(),
        NEWID()
    ),
    (
        @MetricId,
        'Schedule',
        (SELECT [Id] FROM [EntityType] WHERE [Guid] = '0B2C38A7-D79C-4F85-9757-F1B045D32C8A'),
        1,
        1,
        SYSDATETIME(),
        SYSDATETIME(),
        NEWID()
    )
END
" );

            // Create the Total Student's Attendance Metric if it is missing
            Sql( $@"
DECLARE @MetricId [int];

IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[Metric]
    WHERE [Guid] = '{StudentAttendanceMetricGuid}'
)
BEGIN
    DECLARE @ScheduleId INT;

    SET @ScheduleId = (
        SELECT [Id]
        FROM [dbo].[Schedule]
        WHERE [Guid] = 'C31DF106-D7C8-4B64-81E7-5C4AB20DBA7B'
    );

    IF @ScheduleId IS NULL
    BEGIN
        INSERT INTO [dbo].[Schedule] (
            [Name],
            [Description],
            [iCalendarContent],
            [CategoryId],
            [Guid],
            [IsActive],
            [IsPublic]
        )
        VALUES (
            'Weekly Metric Schedule',
            NULL,
            '{WeeklySchedule}',
            (SELECT [Id] FROM [dbo].[Category] WHERE [Guid] = '5A794741-5444-43F0-90D7-48E47276D426'),
            'C31DF106-D7C8-4B64-81E7-5C4AB20DBA7B',
            1,
            0
        );

        SET @ScheduleId = SCOPE_IDENTITY();
    END

    INSERT INTO [dbo].[Metric] (
        [IsSystem],
        [Title],
        [Description],
        [IsCumulative],
        [SourceValueTypeId],
        [SourceSql],
        [ScheduleId],
        [CreatedDateTime],
        [ModifiedDateTime],
        [Guid],
        [NumericDataType],
        [EnableAnalytics],
        [MeasurementClassificationValueId]
    )
    VALUES (
        0,
        'Total Students Attendance',
        'This metric represents attendance records (total for the week) for any group(s) per campus of GroupTypes that have the Weekend Service field checked and are part of the Students Check-in Areas (Jr High Area and High School Area).',
        0,
        (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '6A1E1A1B-A636-4E12-B90C-D7FD1BDAE764'),
        '{StudentAttendanceMetricSourceSql.Replace( "'", "''" )}',
        @ScheduleId,
        SYSDATETIME(),
        SYSDATETIME(),
        '{StudentAttendanceMetricGuid}',
        1,
        0,
        (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{StudentAttendanceMeasurementGuid}')
    );

    SET @MetricId = SCOPE_IDENTITY();

    INSERT INTO [dbo].[MetricCategory] (
        [MetricId],
        [CategoryId],
        [Order],
        [Guid]
    )
    VALUES (
        @MetricId,
        (SELECT [Id] FROM [Category] WHERE [Guid] = '{WeeklyMetricsCategoryGuid}'),
        0,
        NEWID()
    );

    INSERT INTO [dbo].[MetricPartition] (
        [MetricId],
        [Label],
        [EntityTypeId],
        [IsRequired],
        [Order],
        [CreatedDateTime],
        [ModifiedDateTime],
        [Guid]
    )
    VALUES 
    (
        @MetricId,
        'Campus',
        (SELECT [Id] FROM [EntityType] WHERE [Guid] = '00096BED-9587-415E-8AD4-4E076AE8FBF0'),
        1,
        0,
        SYSDATETIME(),
        SYSDATETIME(),
        NEWID()
    ),
    (
        @MetricId,
        'Schedule',
        (SELECT [Id] FROM [EntityType] WHERE [Guid] = '0B2C38A7-D79C-4F85-9757-F1B045D32C8A'),
        1,
        1,
        SYSDATETIME(),
        SYSDATETIME(),
        NEWID()
    )
END
" );

            // Update the Adult Attendance Metric
            Sql( $@"
DECLARE @MetricId [int];

IF EXISTS (SELECT 1 FROM [Metric] WHERE [Guid] = '{AdultAttendanceMetricGuid}')
BEGIN
    UPDATE [Metric]
    SET 
        [Title] = CASE 
            WHEN [Title] = 'Adult Attendance' THEN 'Total Adult Attendance'
            ELSE [Title]
        END,
        [MeasurementClassificationValueId] = (
            SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{AdultAttendanceMeasurementGuid}'
        )
    WHERE [Guid] = '{AdultAttendanceMetricGuid}';
END
ELSE
BEGIN
    INSERT INTO [dbo].[Metric] (
        [IsSystem],
        [Title],
        [IsCumulative],
        [SourceValueTypeId],
        [YAxisLabel],
        [CreatedDateTime],
        [ModifiedDateTime],
        [Guid],
        [MeasurementClassificationValueId]
    )
    VALUES (
        0,
        'Total Adult Attendance',
        0,
        (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '1D6511D6-B15D-4DED-B3C4-459CD2A7EC0E'),
        'Attendance',
        SYSDATETIME(),
        SYSDATETIME(),
        '{AdultAttendanceMetricGuid}',
        (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{AdultAttendanceMeasurementGuid}')
    );

    SET @MetricId = SCOPE_IDENTITY();

    INSERT INTO [dbo].[MetricCategory] (
        [MetricId],
        [CategoryId],
        [Order],
        [Guid]
    )
    VALUES (
        @MetricId,
        (SELECT [Id] FROM [Category] WHERE [Guid] = '{WeeklyMetricsCategoryGuid}'),
        0,
        NEWID()
    );

    INSERT INTO [dbo].[MetricPartition] (
        [MetricId],
        [Label],
        [EntityTypeId],
        [IsRequired],
        [Order],
        [CreatedDateTime],
        [ModifiedDateTime],
        [Guid]
    )
    VALUES 
    (
        @MetricId,
        'Campus',
        (SELECT [Id] FROM [EntityType] WHERE [Guid] = '00096BED-9587-415E-8AD4-4E076AE8FBF0'),
        1,
        0,
        SYSDATETIME(),
        SYSDATETIME(),
        NEWID()
    ),
    (
        @MetricId,
        'Service',
        (SELECT [Id] FROM [EntityType] WHERE [Guid] = '0B2C38A7-D79C-4F85-9757-F1B045D32C8A'),
        1,
        1,
        SYSDATETIME(),
        SYSDATETIME(),
        NEWID()
    )
END
" );

            // Update Volunteer Attendance
            Sql( $@"
-- Update Volunteer Attendance Measurement Classification Title
IF EXISTS (SELECT 1 FROM [DefinedValue] WHERE [Value] = 'Volunteer Attendance' AND [Guid] = '{VolunteerAttendanceMeasurementGuid}')
BEGIN
    UPDATE [DefinedValue]
    SET 
        [Value] = 'Total Volunteer Attendance'
    WHERE [Value] = 'Volunteer Attendance' AND [Guid] = '{VolunteerAttendanceMeasurementGuid}';
END

-- Update Volunteer Attendance Metric Title
IF EXISTS (SELECT 1 FROM [Metric] WHERE [Title] = 'Volunteer Attendance' AND [MeasurementClassificationValueId] = (
    SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{VolunteerAttendanceMeasurementGuid}'
))
BEGIN
    UPDATE [Metric]
    SET 
        [Title] = 'Total Volunteer Attendance'
    WHERE [Title] = 'Volunteer Attendance' AND [MeasurementClassificationValueId] = (
        SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{VolunteerAttendanceMeasurementGuid}'
    );
END
" );

            // Update Total Weekend Attendance
            Sql( $@"
DECLARE @MetricId [int];

-- Update the Metric that is using the Weekend Attendance Measurement Classification.
IF EXISTS (SELECT 1 FROM [Metric] WHERE [MeasurementClassificationValueId] = (
    SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{WeekendAttendanceMeasurementGuid}'
))
BEGIN
    UPDATE [Metric]
    SET 
        [SourceSql] = '{WeekendAttendanceMetricSourceSql.Replace( "'", "''" )}'
    WHERE [MeasurementClassificationValueId] = (
        SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{WeekendAttendanceMeasurementGuid}'
    );
END
-- If no metric has the Weekend Measurement Classification and if the Weekend Attendance Metric exists, update it.
ELSE IF EXISTS (SELECT 1 FROM [Metric] WHERE [Guid] = '{WeekendAttendanceMetricGuid}')
BEGIN
    UPDATE [Metric]
        SET 
            [SourceSql] = '{WeekendAttendanceMetricSourceSql.Replace( "'", "''" )}',
            [MeasurementClassificationValueId] = (
                SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{WeekendAttendanceMeasurementGuid}'
            )
    WHERE [Guid] = '{WeekendAttendanceMetricGuid}';
END
-- If the Weekend Attendance Metric does not exist, create it.
ELSE
BEGIN
    DECLARE @ScheduleId INT;

    SET @ScheduleId = (
        SELECT [Id]
        FROM [dbo].[Schedule]
        WHERE [Guid] = 'C31DF106-D7C8-4B64-81E7-5C4AB20DBA7B'
    );

    IF @ScheduleId IS NULL
    BEGIN
        INSERT INTO [dbo].[Schedule] (
            [Name],
            [Description],
            [iCalendarContent],
            [CategoryId],
            [Guid],
            [IsActive],
            [IsPublic]
        )
        VALUES (
            'Weekly Metric Schedule',
            NULL,
            '{WeeklySchedule}',
            (SELECT [Id] FROM [dbo].[Category] WHERE [Guid] = '5A794741-5444-43F0-90D7-48E47276D426'),
            'C31DF106-D7C8-4B64-81E7-5C4AB20DBA7B',
            1,
            0
        );

        SET @ScheduleId = SCOPE_IDENTITY();
    END

    INSERT INTO [dbo].[Metric] (
        [IsSystem],
        [Title],
        [IsCumulative],
        [SourceValueTypeId],
        [SourceSql],
        [ScheduleId],
        [CreatedDateTime],
        [ModifiedDateTime],
        [Guid],
        [NumericDataType], -- NEW
        [EnableAnalytics], -- NEW
        [MeasurementClassificationValueId]
    )
    VALUES (
        0,
        'Total Weekend Attendance',
        0,
        (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '6A1E1A1B-A636-4E12-B90C-D7FD1BDAE764'),
        '{WeekendAttendanceMetricSourceSql.Replace( "'", "''" )}',
        @ScheduleId,
        SYSDATETIME(),
        SYSDATETIME(),
        '{WeekendAttendanceMetricGuid}',
        1,
        0,
        (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{WeekendAttendanceMeasurementGuid}')
    );

    SET @MetricId = SCOPE_IDENTITY();

    INSERT INTO [dbo].[MetricCategory] (
        [MetricId],
        [CategoryId],
        [Order],
        [Guid]
    )
    VALUES (
        @MetricId,
        (SELECT [Id] FROM [Category] WHERE [Guid] = '{WeeklyMetricsCategoryGuid}'),
        0,
        NEWID()
    );

    INSERT INTO [dbo].[MetricPartition] (
        [MetricId],
        [Label],
        [EntityTypeId],
        [IsRequired],
        [Order],
        [CreatedDateTime],
        [ModifiedDateTime],
        [Guid]
    )
    VALUES 
    (
        @MetricId,
        'Campus',
        (SELECT [Id] FROM [EntityType] WHERE [Guid] = '00096BED-9587-415E-8AD4-4E076AE8FBF0'),
        1,
        0,
        SYSDATETIME(),
        SYSDATETIME(),
        NEWID()
    ),
    (
        @MetricId,
        'Schedule',
        (SELECT [Id] FROM [EntityType] WHERE [Guid] = '0B2C38A7-D79C-4F85-9757-F1B045D32C8A'),
        1,
        1,
        SYSDATETIME(),
        SYSDATETIME(),
        NEWID()
    )
END
" );
        }

    }
}
