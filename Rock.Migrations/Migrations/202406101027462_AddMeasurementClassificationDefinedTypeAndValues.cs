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
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Text;

    /// <summary>
    /// Migration to add Measurement Classification Defined Type and new Weekly metrics.
    /// </summary>
    public partial class AddMeasurementClassificationDefinedTypeAndValues : Rock.Migrations.RockMigration
    {
        private const string AllowMultipleValues = "218CC5B7-AF55-49DF-8959-93459485BA3B";
        private const string WeeklyMetricsCategory = "64B29ADE-144D-4E84-96CC-A79398589733";

        // Defined Value Guids
        private const string TotalWeekendAttendanceValue = "B24ACB41-8B75-41DC-9B47-F289D8C9F04F";
        private const string VolunteerAttendanceValue = "6A2621BF-E600-428A-94C2-CCB79645FA27";
        private const string PrayerRequestsValue = "0428144F-F28D-4DFB-9C16-568751AE2B8E";
        private const string PrayersValue = "03B8B301-58B9-4DE3-87FB-C77A588EE258";
        private const string ActiveFamiliesValue = "26F7B2F5-4D3F-4D55-AA2E-23FAC4D40F7B";
        private const string BaptismsValue = "C1E5B1E5-3DB1-460E-9761-CF07CE105632";
        private const string GivingValue = "152F3B1C-8797-4ACA-9948-C1F9A000EA8B";
        private const string eRAWeeklyWinsValue = "3DB1FB2D-2A55-4F24-9FF6-6D7021574132";
        private const string eRAWeeklyLossesValue = "30489505-50BE-4A1B-8AD2-2D19853F820A";

        private const string ActiveFamiliesMetricPartitionGuid = "2B962606-6162-4C86-A46A-760EC9FF3486";

        private const string WeeklyScheduleGuid = "C31DF106-D7C8-4B64-81E7-5C4AB20DBA7B";
        private const string MonthlyScheduleGuid = "599A64CF-41CD-476C-AC0A-52BA9E71D354";

        // Metric Guids 
        private const string TotalWeekendAttendanceMetric = "89553EEE-91F3-4169-9D7C-04A17471E035";
        private const string VolunteerAttendanceMetric = "4F965AE3-D455-4346-988F-2A2B5E236C0C";
        private const string PrayerRequestsMetric = "2B5ECA35-47D8-4690-A8AD-72488485F2B4";
        private const string PrayersMetric = "685B7912-CB17-473B-90C1-2804F221931C";
        private const string ActiveFamiliesMetric = "491061B7-1834-44DA-8EA1-BB73B2D52AD3";
        private const string BaptismsMetric = "8B63D9D5-A82D-49D4-9AED-2EDBCF60FDEE";
        private const string GivingMetric = "43338E8A-622A-4195-B153-285E570B229D";
        private const string eRAWeeklyWinsMetric = "D05D685A-9A88-4375-A563-70BB44FBD237";
        private const string eRAWeeklyLossesMetric = "16A3FF64-31F0-4CFF-B5F4-83EEB69E0C25";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddDefinedTypeAndAttribute();
            AddDefinedValues();
            AddMetricSchedules();
            AddMetrics();
        }

        private void AddMetricSchedules()
        {
            // Add Weekly Schedule.
            AddSchedule( WeeklyScheduleGuid, "Weekly Metric Schedule", @"
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
END:VCALENDAR" );

            // Add Monthly Schedule.
            AddSchedule( MonthlyScheduleGuid, "Monthly Metric Schedule", @"BEGIN:VCALENDAR
PRODID:-//github.com/SparkDevNetwork/Rock//NONSGML Rock//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:20240601T040100
DTSTAMP:20240610T144835
DTSTART:20240601T040000
RRULE:FREQ=MONTHLY;BYMONTHDAY=1
SEQUENCE:0
UID:ce40496e-b079-44b9-abb7-dc94d8a631f3
END:VEVENT
END:VCALENDAR" );
        }

        private void AddSchedule( string guid, string name, string schedule )
        {
            Sql( $@"
DECLARE @ScheduleId int = ( 
    SELECT [Id] 
    FROM dbo.[Schedule] 
    WHERE Guid = '{guid}'),

@MetricsCategoryId int = ( 
    SELECT [Id] 
    FROM dbo.[Category] 
    WHERE Guid = '5A794741-5444-43F0-90D7-48E47276D426'
)
IF @ScheduleId IS NULL
BEGIN
	INSERT [dbo].[Schedule] ([Name], [Description], [iCalendarContent], [CategoryId], [Guid], [IsActive], [IsPublic])
	VALUES ('{name}', NULL, '{schedule}',@MetricsCategoryId, '{guid}', 1, 0)
END" );
        }

        private void AddDefinedTypeAndAttribute()
        {
            // Add Measurement Classification Defined Type.
            RockMigrationHelper.AddDefinedType( "Metric",
                "Measurement Classification",
                "The values for this defined type will clarify what the metrics are measuring, enabling the system to utilize these metrics for analytics. The description of the defined values should outline any expected configuration for the metric, such as partitions, schedules, etc.",
                SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "Choose the purpose of this metric based on what you''re measuring." );

            // Add Allow Multiple Metrics attribute to the Classification Defined Type.
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                SystemGuid.FieldType.BOOLEAN,
                "Allow Multiple Metrics",
                "AllowMultipleMetrics",
                "This setting determines whether multiple metrics can share the same classification.",
                0,
                "False",
                AllowMultipleValues );
        }

        private void AddDefinedValues()
        {
            // Add Total Weekend Attendance Defined Value.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "Total Weekend Attendance",
                "This metric measures the total weekend attendance for the organization, partitioned by Campus > Schedule.",
                TotalWeekendAttendanceValue );

            // Add Volunteer Attendance Defined Value.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "Volunteer Attendance",
                "This metric measures the number of volunteers that served for the given week. This metric should be partitioned by Campus > Schedule.",
                VolunteerAttendanceValue );

            // Add Prayer Requests Defined Value.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "Prayer Requests",
                "This metric measures the number of active prayer requests for the given week. This metric should be partitioned by Campus.",
                PrayerRequestsValue );

            // Add Prayers Defined Value.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "Prayers",
                "This metrics measures the number of prayers for the given week. This metric should be partitioned by Campus.",
                PrayersValue );

            // Add Active Families Defined Value.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "Active Families",
                "This metric represents the number of active families in the given week. This metric should be partitioned by Campus.",
                ActiveFamiliesValue );

            // Add Baptisms Defined Value.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "Baptisms",
                "The metric that represents the number of baptisms in a given month. This metric should be partitioned by Campus.",
                BaptismsValue );

            // Add Giving Defined Value.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "Giving",
                "This metric represents weekly giving to the tithe. It's up to each organization to define the financial accounts that make up this metric. This metric should be partitioned by Campus of the financial account.",
                GivingValue );

            // Add eRA Weekly Wins Defined Value.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "eRA Weekly Wins",
                "The metric that tracks eRA wins by week. This metric should be partitioned by Campus.",
                eRAWeeklyWinsValue );

            // Add eRA Weekly Losses Defined Value.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "eRA Weekly Losses",
                "The metric that tracks eRA losses by week. This metric should be partitioned by Campus.",
                eRAWeeklyLossesValue );
        }

        private void AddMetrics()
        {
            // Update Active Families Metric
            Sql( $@"
UPDATE [dbo].[Metric]
SET [SourceSql] = 'DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = ''790e3215-3b10-442b-af69-616c0dcb998e'')
SELECT COUNT( DISTINCT(g.[Id])) as ActiveFamilies, p.[PrimaryCampusId]
FROM [Person] p
    INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
    INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
WHERE ([RecordTypeValueId] = 1) AND ([RecordStatusValueId] = 3)
GROUP BY p.[PrimaryCampusId]',
[ScheduleId] = (SELECT [Id] FROM Schedule WHERE Guid = '{WeeklyScheduleGuid}'),
[MeasurementClassificationValueId] = (SELECT [Id] FROM dbo.[DefinedValue] WHERE [Guid] = '{ActiveFamiliesValue}')
WHERE [Guid] = '{ActiveFamiliesMetric}'
" );

            // Add Campus Partition to Active Families Metric
            Sql( $@"
DECLARE @ActiveFamiliesMetricId int = (SELECT TOP 1 [Id] FROM dbo.[Metric] WHERE [Guid] = '{ActiveFamiliesMetric}')
DECLARE @CampusEntityTypeId int = (SELECT TOP 1 [Id] FROM dbo.[EntityType] WHERE [Guid] = '{SystemGuid.EntityType.CAMPUS}')
IF NOT EXISTS (SELECT [Id] FROM dbo.[MetricPartition] WHERE [Guid] = 'F879279D-3484-4F58-A16D-F64BDB277358')
BEGIN
	INSERT INTO dbo.[MetricPartition]
		([MetricId], [Label],[EntityTypeId],[IsRequired],[Guid],[Order])
	VALUES 
		(@ActiveFamiliesMetricId, 'Campus', @CampusEntityTypeId, 1, 'F879279D-3484-4F58-A16D-F64BDB277358',0)
END
ELSE
BEGIN
 UPDATE dbo.[MetricPartition]
    SET [Label] = 'Campus',
     [EntityTypeId] = @CampusEntityTypeId
     WHERE [Guid] = 'F879279D-3484-4F58-A16D-F64BDB277358'
END
" );

            // Add Total Weekend Attendance Metric
            AddMetric( TotalWeekendAttendanceMetric,
                "Total Weekend Attendance",
                WeeklyMetricsCategory,
                @"
DECLARE @STARTDATE DATETIME = DATEADD(DAY, -7, GETDATE())
DECLARE @ENDDATE DATETIME = GETDATE() 

SELECT COUNT(1) as AttendanceCount, a.[CampusId], oa.[ScheduleId]
FROM [Attendance] a
INNER JOIN [AttendanceOccurrence] oa ON oa.Id = a.[OccurrenceId]
INNER JOIN [Group] g ON g.Id = oa.[GroupId]
INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
WHERE
   gt.[AttendanceCountsAsWeekendService] = 1
   AND a.[DidAttend] = 1 
   AND a.[StartDateTime] BETWEEN @STARTDATE AND @ENDDATE
GROUP BY ALL a.[CampusId], oa.[ScheduleId]
",
                new List<PartitionDetails>() { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ), new PartitionDetails( "Schedule", SystemGuid.EntityType.SCHEDULE ) },
                WeeklyScheduleGuid,
                "This metric represents attendance records (total for the week) for any group(s) per campus of GroupTypes that have the Weekend Service field checked.",
                TotalWeekendAttendanceValue );

            // Add Volunteer Attendance Metric
            AddMetric( VolunteerAttendanceMetric,
                "Volunteer Attendance",
                WeeklyMetricsCategory,
                @"
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
GROUP BY ALL a.[CampusId], oa.[ScheduleId]
",
                new List<PartitionDetails>() { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ), new PartitionDetails( "Schedule", SystemGuid.EntityType.SCHEDULE ) },
                WeeklyScheduleGuid,
                "This metric represents attendance records (total for the week) for any group(s) per campus of GroupTypes that have a Purpose of Serving Area.",
                VolunteerAttendanceValue );

            // Add Prayer Requests Metric
            AddMetric( PrayerRequestsMetric,
                "Prayer Requests",
                WeeklyMetricsCategory,
                @"
DECLARE @STARTDATE DATETIME = DATEADD(DAY, -7, GETDATE())
DECLARE @ENDDATE DATETIME = GETDATE()

SELECT COUNT(1) as PrayerRequests, p.[PrimaryCampusId]
FROM dbo.[PrayerRequest] pr
INNER JOIN [PersonAlias] pa ON pa.[Id] = pr.[RequestedByPersonAliasId]
INNER JOIN dbo.[Person] p ON p.[Id] = pa.[PersonId]
WHERE
   pr.[IsActive] = 1
   AND pr.[CreatedDateTime] BETWEEN @STARTDATE AND @ENDDATE
GROUP BY ALL p.[PrimaryCampusId]
",
                new List<PartitionDetails>() { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) },
                WeeklyScheduleGuid,
                "This metric represents the number of PrayerRequests created during the given week per campus.",
                PrayerRequestsValue );

            // Add Prayers Metric
            AddMetric( PrayersMetric,
                "Prayers",
                WeeklyMetricsCategory,
                @"
DECLARE @STARTDATE DATETIME = DATEADD(DAY, -7, GETDATE())
DECLARE @ENDDATE DATETIME = GETDATE() 

SELECT  COUNT(*) as Prayers, p.[PrimaryCampusId]
FROM dbo.[Interaction] i 
INNER JOIN [InteractionComponent] ic ON ic.[Id] = i.[InteractionComponentId]
INNER JOIN [InteractionChannel] ichan ON ichan.[Id] = ic.[InteractionChannelId]
INNER JOIN [PrayerRequest] pr ON pr.[Id] = ic.[EntityId]
INNER JOIN [PersonAlias] pa ON pa.[Id] = i.[PersonAliasId]
INNER JOIN [Person] p ON p.[Id] = pa.[PersonId]
WHERE 
   ichan.[Guid] = '3D49FB99-94D1-4F63-B1A2-30D4FEDE11E9'
   AND i.[Operation] = 'Prayed'
   AND i.[InteractionDateTime] BETWEEN @STARTDATE AND @ENDDATE
GROUP BY ALL p.[PrimaryCampusId]
",
                new List<PartitionDetails>() { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) },
                WeeklyScheduleGuid,
                "This metric uses prayer request Interaction records to determine the number of prayers offered (see Prayer Session page) during the given week per campus.",
                PrayersValue );

            // Add Baptisms Metric
            AddMetric( BaptismsMetric,
                "Baptisms",
                WeeklyMetricsCategory,
                $@"
DECLARE @STARTDATE DATETIME= DATEADD(mm, DATEDIFF(mm, 0, GETDATE()), 0)
DECLARE @ENDDATE DATETIME = DATEADD(DAY, -1, DATEADD(mm, 1, @STARTDATE));
DECLARE @BaptismDateAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = 'D42763FA-28E9-4A55-A25A-48998D7D7FEF')

SELECT COUNT(*) as Baptisms, p.PrimaryCampusId FROM Person p
JOIN dbo.[AttributeValue] av
ON p.[Id] = av.[EntityId]
WHERE av.[AttributeId] = @BaptismDateAttributeId
AND av.[ValueAsDateTime] >= @STARTDATE
AND av.[ValueAsDateTime] < @ENDDATE
GROUP BY ALL p.[PrimaryCampusId]
",
                new List<PartitionDetails>() { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) },
                MonthlyScheduleGuid,
                "This metric uses the system/core \"BaptismDate\" attribute and values to get a total number of people who were baptized within the given month per campus.",
                BaptismsValue );

            // Add Giving Metric
            AddMetric( GivingMetric,
                "Giving",
                WeeklyMetricsCategory,
                @"
-- =====================================================================================================
-- Description: This metric represents weekly giving to the tithe and should be partitioned by Campus.
-- =====================================================================================================
-- You can edit this to match the financial accounts that are considered part of the 'tithe', but please
-- do not change the remainder of this script:
 
DECLARE @Accounts VARCHAR(100) = '1';   -- Comma separated accounts to extract giving information from, their child accounts will be included.
 
-------------------------------------------------------------------------------------------------------
DECLARE @STARTDATE int = FORMAT( DATEADD(DAY, -7, GETDATE()), 'yyyyMMdd' )
DECLARE @ENDDATE int = FORMAT( GETDATE(), 'yyyyMMdd' )
DECLARE @PersonRecordTypeId INT = ( SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E' )
DECLARE @AccountsWithChildren TABLE (Id INT);
-- Recursively get accounts and their children.
WITH AccountHierarchy AS (
    SELECT [Id]
    FROM dbo.[FinancialAccount] fa
    WHERE [Id] IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@Accounts, ','))
    UNION ALL
    SELECT e.[Id]
    FROM dbo.[FinancialAccount] e
    INNER JOIN AccountHierarchy ah ON e.[ParentAccountId] = ah.[Id]
)
INSERT INTO @AccountsWithChildren SELECT * FROM AccountHierarchy;

;WITH CTE AS (
    SELECT
	fa.[Name] AS AccountName
    , fa.[CampusId] AS AccountCampusId
    , [PrimaryFamilyId]
    , SUM(ftd.[Amount]) AS [GivingAmount]
	FROM
		[Person] p
		INNER JOIN [dbo].[PersonAlias] pa ON pa.[PersonId] = p.[Id]
		INNER JOIN [dbo].[FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
		INNER JOIN [dbo].[FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
		INNER JOIN [dbo].[FinancialAccount] fa ON fa.[Id] = ftd.[AccountId] 
		INNER JOIN [dbo].[AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
	WHERE
		fa.[IsTaxDeductible] = 1
		AND asd.[DateKey] > = @STARTDATE AND asd.[DateKey] <= @ENDDATE
		AND fa.[Id] IN (SELECT * FROM @AccountsWithChildren)
AND p.[RecordTypeValueId] = @PersonRecordTypeId
	GROUP BY fa.[CampusId], fa.[Name], [PrimaryFamilyId]
)
SELECT
    [GivingAmount] AS [GivingAmount]
    , [AccountCampusId] AS [CampusId]
	, [AccountName]
FROM CTE
GROUP BY [AccountCampusId], [AccountName], [GivingAmount];
",
                new List<PartitionDetails>() { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) },
                WeeklyScheduleGuid,
                "This metric represents weekly giving to the tithe per campus of the financial account.",
                GivingValue );

            // Add Weekly eRA Wins Metric
            AddMetric( eRAWeeklyWinsMetric,
                "Weekly eRA Wins",
                WeeklyMetricsCategory,
                @"
DECLARE @StartDate DATETIME = DATEADD(DAY, -7, GETDATE())
DECLARE @EndDate DATETIME = GETDATE()
DECLARE @EraStartDateAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = 'A106610C-A7A1-469E-4097-9DE6400FDFC2')
DECLARE @IsEraAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = 'CE5739C5-2156-E2AB-48E5-1337C38B935E')

SELECT COUNT(*) as eraWins, p.[PrimaryCampusId]
FROM dbo.[Person] p
JOIN dbo.[AttributeValue] av ON p.Id = av.[EntityId]
WHERE av.[AttributeId] = @EraStartDateAttributeId AND av.[ValueAsDateTime] BETWEEN @StartDate AND @EndDate
AND EXISTS (
    SELECT 1
    FROM dbo.[AttributeValue] av2
    WHERE av2.[EntityId] = p.Id
    AND av2.[AttributeId] = @IsEraAttributeId
    AND av2.ValueAsBoolean = 1
)
GROUP BY ALL p.[PrimaryCampusId];
",
                new List<PartitionDetails>() { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) },
                WeeklyScheduleGuid,
                "This metric tracks the number of individuals who attained eRA status within the current week per campus.",
                eRAWeeklyWinsValue );

            // Add Weekly eRA Losses Metric
            AddMetric( eRAWeeklyLossesMetric,
                "Weekly eRA Losses",
                WeeklyMetricsCategory,
                @"
DECLARE @StartDate DATETIME = DATEADD(DAY, -7, GETDATE())
DECLARE @EndDate DATETIME = GETDATE()
DECLARE @EraStartDateAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = '4711D67E-7526-9582-4A8E-1CD7BBE1B3A2')
DECLARE @IsEraAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = 'CE5739C5-2156-E2AB-48E5-1337C38B935E')

SELECT COUNT(*) as eraWins, p.[PrimaryCampusId]
FROM dbo.[Person] p
JOIN dbo.[AttributeValue] av ON p.Id = av.[EntityId]
WHERE av.[AttributeId] = @EraStartDateAttributeId AND av.[ValueAsDateTime] BETWEEN @StartDate AND @EndDate
AND EXISTS (
    SELECT 1
    FROM dbo.[AttributeValue] av2
    WHERE av2.[EntityId] = p.Id
    AND av2.[AttributeId] = @IsEraAttributeId
    AND av2.ValueAsBoolean = 0
)
GROUP BY ALL p.[PrimaryCampusId];
",
                new List<PartitionDetails>() { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) },
                WeeklyScheduleGuid,
                "This metric monitors the number of individuals who exited eRA status within the current week per campus.",
                eRAWeeklyLossesValue );
        }

        private void AddMetric( string guid, string title, string categoryGuid, string sourceSql, List<PartitionDetails> partitions, string scheduleGuid, string description, string measurementClassificationValueGuid )
        {
            var formattedTitle = title?.Replace( "'", "''" ) ?? throw new ArgumentNullException( nameof( title ) );
            var createMetricAndMetricCategorySql = $@"DECLARE @MetricId [int] = (SELECT [Id] FROM dbo.[Metric] WHERE [Guid] = '{guid}')
    , @SourceValueTypeId [int] = (SELECT [Id] FROM dbo.[DefinedValue] WHERE [Guid] = '{SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_SQL}')
    , @MetricCategoryId [int] = (SELECT [Id] FROM dbo.[Category] WHERE [Guid] = '{categoryGuid}')
    , @Description [varchar] (max) = {( string.IsNullOrWhiteSpace( description ) ? "NULL" : $"'{description.Replace( "'", "''" )}'" )}
    , @MeasurementClassificationId [int] = (SELECT [Id] FROM dbo.[DefinedValue] WHERE [Guid] = '{measurementClassificationValueGuid}');

IF (@MetricId IS NULL AND @SourceValueTypeId IS NOT NULL AND @MetricCategoryId IS NOT NULL)
BEGIN
    DECLARE @Now [datetime] = GETDATE();
    INSERT INTO dbo.[Metric]
    (
        [IsSystem]
        , [Title]
        , [Description]
        , [IsCumulative]
        , [SourceValueTypeId]
        , [SourceSql]
        , [ScheduleId]
        , [CreatedDateTime]
        , [ModifiedDateTime]
        , [Guid]
        , [NumericDataType]
        , [EnableAnalytics]
        , [MeasurementClassificationValueId]
    )
    VALUES
    (
        0
        , '{formattedTitle}'
        , @Description
        , 0
        , @SourceValueTypeId
        , '{sourceSql.Replace( "'", "''" )}'
        , (SELECT [Id] FROM Schedule WHERE Guid = '{scheduleGuid}')
        , @Now
        , @Now
        , '{guid}'
        , 1
        , 0
        , @MeasurementClassificationId
    );
    SET @MetricId = SCOPE_IDENTITY();
    INSERT INTO dbo.[MetricCategory]
    (
        [MetricId]
        , [CategoryId]
        , [Order]
        , [Guid]
    )
    VALUES
    (
        @MetricId
        , @MetricCategoryId
        , 0
        , NEWID()
    );";
            var sqlBuilder = new StringBuilder( createMetricAndMetricCategorySql );

            if ( partitions == null || partitions.Count == 0 )
            {
                sqlBuilder.Append( @"INSERT INTO dbo.[MetricPartition]
    (
        [MetricId]
        , [IsRequired]
        , [Order]
        , [Guid]
    )
    VALUES
    (
        @MetricId
        , 1
        , 0
        , NEWID()
    );" );
            }
            else
            {
                foreach ( var partitionDetail in partitions )
                {
                    var createMetricPartitionSql = $@"INSERT INTO dbo.[MetricPartition]
    (
        [MetricId]
        , [Label]
        , [EntityTypeId]
        , [IsRequired]
        , [Order]
        , [CreatedDateTime]
        , [ModifiedDateTime]
        , [Guid]
    )
    VALUES
    (
        @MetricId
        , '{partitionDetail.Label}'
        , (SELECT Id FROM dbo.[EntityType] WHERE [GUID] = '{partitionDetail.EntityTypeGuid}')
        , 1
        , {partitions.IndexOf( partitionDetail )}
        , @Now
        , @Now
        , NEWID()
    );";
                    sqlBuilder.Append( createMetricPartitionSql );
                }
            }

            sqlBuilder.AppendLine( "END" );

            Sql( sqlBuilder.ToString() );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteMetrics();
            DeleteMetricSchedules();
            DeleteDefinedValues();
            DeleteDefinedTypeAndAttribute();
        }

        private void DeleteMetricSchedules()
        {
            Sql( $"DELETE FROM dbo.[Schedule] WHERE [Guid] = '{WeeklyScheduleGuid}'" );
            Sql( $"DELETE FROM dbo.[Schedule] WHERE [Guid] = '{MonthlyScheduleGuid}'" );
        }

        private void DeleteDefinedTypeAndAttribute()
        {
            RockMigrationHelper.DeleteDefinedType( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION );
            RockMigrationHelper.DeleteAttribute( AllowMultipleValues );
        }

        private void DeleteDefinedValues()
        {
            RockMigrationHelper.DeleteDefinedValue( TotalWeekendAttendanceValue );
            RockMigrationHelper.DeleteDefinedValue( VolunteerAttendanceValue );
            RockMigrationHelper.DeleteDefinedValue( PrayerRequestsValue );
            RockMigrationHelper.DeleteDefinedValue( PrayersValue );
            RockMigrationHelper.DeleteDefinedValue( ActiveFamiliesValue );
            RockMigrationHelper.DeleteDefinedValue( BaptismsValue );
            RockMigrationHelper.DeleteDefinedValue( GivingValue );
            RockMigrationHelper.DeleteDefinedValue( eRAWeeklyWinsValue );
            RockMigrationHelper.DeleteDefinedValue( eRAWeeklyLossesValue );
        }

        private void DeleteMetrics()
        {
            Sql( $"DELETE FROM dbo.[MetricPartition] WHERE [Guid] = '{ActiveFamiliesMetricPartitionGuid}'" );

            Sql( @"
UPDATE [dbo].[Metric]
SET [SourceSql] = 'DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = ''790e3215-3b10-442b-af69-616c0dcb998e'')
SELECT COUNT( DISTINCT(g.[Id])) 
FROM [Person] p
    INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
    INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
WHERE ([RecordTypeValueId] = 1) AND ([RecordStatusValueId] = 3)',
[ScheduleId] = (SELECT Id FROM Schedule WHERE [Guid] = '717d75f1-644f-45a4-b25e-64652a270ad9'),
[MeasurementClassificationValueId] = NULL
WHERE [Guid] = '491061B7-1834-44DA-8EA1-BB73B2D52AD3'
" );

            Sql( $"DELETE FROM dbo.[Metric] WHERE [Guid] = '{TotalWeekendAttendanceMetric}'" );
            Sql( $"DELETE FROM dbo.[Metric] WHERE [Guid] = '{VolunteerAttendanceMetric}'" );
            Sql( $"DELETE FROM dbo.[Metric] WHERE [Guid] = '{PrayerRequestsMetric}'" );
            Sql( $"DELETE FROM dbo.[Metric] WHERE [Guid] = '{PrayersMetric}'" );
            Sql( $"DELETE FROM dbo.[Metric] WHERE [Guid] = '{BaptismsMetric}'" );
            Sql( $"DELETE FROM dbo.[Metric] WHERE [Guid] = '{GivingMetric}'" );
            Sql( $"DELETE FROM dbo.[Metric] WHERE [Guid] = '{eRAWeeklyWinsMetric}'" );
            Sql( $"DELETE FROM dbo.[Metric] WHERE [Guid] = '{eRAWeeklyLossesMetric}'" );
        }

        private sealed class PartitionDetails
        {
            public PartitionDetails( string label, string entityTypeGuid )
            {
                Label = label;
                EntityTypeGuid = entityTypeGuid;
            }

            /// <summary>
            /// Gets or sets the label.
            /// </summary>
            /// <value>
            /// The label.
            /// </value>
            public string Label { get; set; }

            /// <summary>
            /// Gets or sets the entity type unique identifier.
            /// </summary>
            /// <value>
            /// The entity type unique identifier.
            /// </value>
            public string EntityTypeGuid { get; set; }
        }
    }
}
