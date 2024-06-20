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

using System;
using System.Collections.Generic;
using System.Text;

using Rock.Model;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 202, "1.16.4" )]
    public class MigrationRollupsForV17_0_9 : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            NewWeeklyMetricsAndMeasurementClassificationDefinedValuesUp();
            AddLocationHistoryCategoryUp();
            UpdateTithingMetricsUp();
            AddLmsDataUp();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            NewWeeklyMetricsAndMeasurementClassificationDefinedValuesDown();
            AddLocationHistoryCategoryDown();
            UpdateTithingMetricsDown();
            AddLmsDataDown();
        }

        #region KA: Migration to add New Weekly Metrics and MeasurementClassification Defined Values

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

        private void NewWeeklyMetricsAndMeasurementClassificationDefinedValuesUp()
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

SELECT COUNT(*) as eraLosses, p.[PrimaryCampusId]
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
        private void NewWeeklyMetricsAndMeasurementClassificationDefinedValuesDown()
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

        #endregion

        #region KA: Add Location History Category Migration

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        private void AddLocationHistoryCategoryUp()
        {
            // Add Location History Category.
            RockMigrationHelper.UpdateCategory( "546D5F43-1184-47C9-8265-2D7BF4E1BCA5", "Location", "", "", Rock.SystemGuid.Category.HISTORY_LOCATION );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        private void AddLocationHistoryCategoryDown()
        {
            RockMigrationHelper.DeleteCategory( Rock.SystemGuid.Category.HISTORY_LOCATION );
        }

        #endregion

        #region KA: D: v17 Migration to add color attribute to Campus

            private const string TITHING_OVERVIEW_PAGE = "72BA5DD9-8685-4182-833D-22BB1E0F9A36";
            private const string TITHING_OVERVIEW_SCHEDULE_GUID = "5E51EA9E-8475-4955-875E-45F44270A462";
            private const string TITHING_OVERVIEW_BY_CAMPUS_METRIC_GUID = "F4951A42-9F71-4CB1-A46E-2A7ED84CD923";
            private const string GIVING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID = "B5BFAB51-9B46-4E7E-992E-B0119E4D25EC";
            private const string TITHING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID = "2B798177-E8F4-46DB-A1D7-308D63CA519A";

            /// <summary>
            /// Operations to be performed during the upgrade process.
            /// </summary>
            private void UpdateTithingMetricsUp()
            {
                // Update Tithing overview page icon and hide description.
                Sql( $@"
UPDATE [dbo].[PAGE]
SET [PageDisplayDescription] = 0,
[IconCssClass] = 'fa fa-chart-bar'
WHERE [Guid] = '{TITHING_OVERVIEW_PAGE}' 
" );

                // Add Campus Color Attribute.
                RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.Campus",
                    SystemGuid.FieldType.COLOR,
                    string.Empty,
                    string.Empty,
                    "Color",
                    "This color will be shown in certain places where multiple campuses are being visualized.",
                    0,
                    string.Empty,
                    "B63C8C1A-58DC-4DDF-AA5D-C71F1E2D74B6",
                    "core_CampusColor" );

                // Update Schedule, Set [IsPublic] to false and update execution time from 4am to 5am.
                Sql( $@"
UPDATE [dbo].[Schedule]
SET [IsPublic] = 0,
[iCalendarContent] = 'BEGIN:VCALENDAR
PRODID:-//github.com/SparkDevNetwork/Rock//NONSGML Rock//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:20240409T050200
DTSTAMP:20240529T183514
DTSTART:20240409T050000
RRULE:FREQ=WEEKLY;BYDAY=TU
SEQUENCE:0
UID:1b23207e-e482-4de1-af49-7e445d084739
END:VEVENT
END:VCALENDAR'
WHERE [Guid] = '{TITHING_OVERVIEW_SCHEDULE_GUID}'
" );

                // Update Tithing Overview by Campus description and SourceSql.
                Sql( $@"
UPDATE [dbo].[Metric]
SET [Description] = 'This a breakdown of the percentage of households at/above the median tithe for each campus. Only households with a recognized postal code are included in this metric value.',
[SourceSql] = 'DECLARE @StartDate int = FORMAT( DATEADD( d, -365, GETDATE()), ''yyyyMMdd'' )
DECLARE @EndDate int = FORMAT( GETDATE(), ''yyyyMMdd'' )
-- Only Include Person Type Records
DECLARE @PersonRecordTypeId INT = ( SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E'' )

;WITH CTE AS (
    SELECT
    [PrimaryCampusId]
    , [PrimaryFamilyId]
    , SUM(ftd.[Amount]) AS [GivingAmount]
    , (SELECT TOP 1 
            LEFT([PostalCode], 5)
        FROM [dbo].[Location] l
        INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMappedLocation] = 1
      ) AS [PostalCode]
    , (SELECT [FamiliesMedianIncome] * .1 FROM [dbo].[AnalyticsSourcePostalCode] WHERE [PostalCode] = (SELECT TOP 1 LEFT([PostalCode], 5) FROM [dbo].[Location] l INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMappedLocation] = 1) ) AS [FamiliesMedianTithe]
	FROM
		[Person] p
		INNER JOIN [dbo].[PersonAlias] pa ON pa.[PersonId] = p.[Id]
		INNER JOIN [dbo].[FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
		INNER JOIN [dbo].[FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
		INNER JOIN [dbo].[FinancialAccount] fa ON fa.[Id] = ftd.[AccountId] 
		INNER JOIN [dbo].[AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
	WHERE
		fa.[IsTaxDeductible] = 1
		AND asd.[DateKey] > = @StartDate AND asd.[DateKey] <= @EndDate
-- Only include person type records.
AND p.[RecordTypeValueId] = @PersonRecordTypeId
	GROUP BY [PrimaryCampusId], [PrimaryFamilyId]
)
SELECT 
    CAST(COUNT(CASE WHEN [GivingAmount] > [FamiliesMedianTithe] THEN 1 END) AS FLOAT) / COUNT(*) * 100 AS PercentageAboveMedianTithe,
    [PrimaryCampusId],
    COUNT(*) AS TotalFamilies,
    COUNT(CASE WHEN [GivingAmount] > [FamiliesMedianTithe] THEN 1 END) AS FamiliesAboveMedianTithe
FROM 
    CTE
-- Only include families that have a postal code and/or we have a [FamiliesMedianIncome] value
WHERE ( [PostalCode] IS NOT NULL AND [PostalCode] != '''') and [FamiliesMedianTithe] is NOT NULL
GROUP BY [PrimaryCampusId];'
WHERE [Guid] = '{TITHING_OVERVIEW_BY_CAMPUS_METRIC_GUID}'
" );

                // Update Tithing households by Campus description.
                Sql( $@"
UPDATE [dbo].[Metric]
SET [Description] = 'This is the percent of households that are at/above the tithe. The tithe value is one tenth of the median family income for a household''s location/postal code as determined by the AnalyticsSourcePostalCode table. Only households with a recognized postal code are included in this metric value'
WHERE [Guid] = '{TITHING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID}'
" );

                // Update Giving households by Campus description.
                Sql( $@"
UPDATE [dbo].[Metric]
SET [Description] = 'This is the percent of households per campus that are giving. Only households with a recognized postal code are included in this metric value.'
WHERE [Guid] = '{GIVING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID}'
" );
            }

            /// <summary>
            /// Operations to be performed during the downgrade process.
            /// </summary>
            private  void UpdateTithingMetricsDown()
            {
                // Delete Campus Color Attribute
                RockMigrationHelper.DeleteAttribute( "B63C8C1A-58DC-4DDF-AA5D-C71F1E2D74B6" );
            }

        #endregion

        #region LMS Add Data

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        private void AddLmsDataUp()
        {
            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Model.LearningProgram", Rock.SystemGuid.EntityType.LEARNING_PROGRAM, true, false );
            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Model.LearningActivityCompletion", Rock.SystemGuid.EntityType.LEARNING_ACTIVITY_COMPLETION, true, false );
            LmsEntityTypesPagesBlocksUp();
            AddSeedData();
            AddOrUpdateSendLearningActivityNotificationsJob();

            // Hide the Learning Navigation from the menu by default.
            Sql( $"UPDATE [Page] SET [DisplayInNavWhen] = {( int ) DisplayInNavWhen.Never} WHERE [Guid] = '84DBEC51-EE0B-41C2-94B3-F361C4B98879';" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        private void AddLmsDataDown()
        {
            DeleteSendLearningActivityNotificationsJob();
            LmsEntityTypesPagesBlocksDown();

            RemoveSeedData();
            RockMigrationHelper.DeleteEntityType( Rock.SystemGuid.EntityType.LEARNING_ACTIVITY_COMPLETION );
        }

        /// <summary>
        /// Adds the LMS seed data to the database.
        /// </summary>
        private void AddSeedData()
        {
            Sql( @"
DECLARE @now DATETIMEOFFSET = SYSDATETIMEOFFSET();

/* Grading Systems */
-- Create Learning Grading Systems using well-known Identity (and GUID) values.
SET IDENTITY_INSERT [dbo].[LearningGradingSystem] ON;

INSERT [dbo].[LearningGradingSystem] (
	[Id]
	, [Name]
	, [Description]
	, [IsActive]
	, [CreatedDateTime]
	, [ModifiedDateTime]
	, [Guid]
)
SELECT Id, [Name], [Description], 1 IsActive, @now Created, @now Modified, [Guid]
FROM (
	SELECT 1 Id, 'Pass/Fail' [Name], 'The Pass/Fail grading system evaluates students simply as ""Pass"" if they meet the course requirements, or ""Fail"" if they do not.' [Description], '99D9914B-7CCB-4E32-BD2D-541ACD7A1B22' [Guid]
	UNION SELECT 2 Id, 'Letter Grade' [Name], 'The Letter Grade system assigns grades ranging from A (excellent) to F (failing), reflecting a student''s performance in a course. ' [Description], '1EBBA9F7-E3DF-4930-9677-639D0915CAA3' [Guid]
) seed
WHERE NOT EXISTS (
	SELECT 1
	FROM [dbo].[LearningGradingSystem] ex
	WHERE ex.Id = seed.Id
)

SET IDENTITY_INSERT [dbo].[LearningGradingSystem] OFF;


/* Grading System Scales */
-- Create Learning Grading System Scales using well-known Identity values.
SET IDENTITY_INSERT [dbo].[LearningGradingSystemScale] ON;

INSERT [dbo].[LearningGradingSystemScale] (
	[Id]
	, [Name]
	, [Description]
	, [ThresholdPercentage]
	, [IsPassing]
	, [Order]
	, [LearningGradingSystemId]
	, [CreatedDateTime]
	, [ModifiedDateTime]
	, [Guid]
)
SELECT Id, [Name], [Description], [Threshold], [IsPassing], [Order], [GradingSystemId], @now Created, @now Modified, [Guid]
FROM (
	SELECT 1 Id, 'Pass' [Name], 'Passes.' [Description], 70 [Threshold], 1 IsPassing, 1 [Order], 1 [GradingSystemId], 'C07A3227-7188-4D61-AC02-FF6AB8380AAD' [Guid]
	UNION SELECT 2 Id, 'Fail' [Name], 'Doesn''t Pass.' [Description], 0 [Threshold], 0 IsPassing, 2 [Order], 1 [GradingSystemId], 'BD209F2D-22E0-41A9-B425-ED42D515E13B' [Guid]
	UNION SELECT 3 Id, 'A' [Name], 'Passes with an ""A"".' [Description], 93 [Threshold], 1 IsPassing, 3 [Order], 2 [GradingSystemId], 'F96BDDD2-EA0F-4C35-90BB-0B7D9FAABD26' [Guid]
	UNION SELECT 4 Id, 'B' [Name], 'Passes with a ""B"".' [Description], 83 [Threshold], 1 IsPassing, 4 [Order], 2 [GradingSystemId], 'E8128844-04B0-4772-AB59-55F17645AB7A' [Guid]
	UNION SELECT 5 Id, 'C' [Name], 'Passes with a ""C"".' [Description], 73 [Threshold], 1 IsPassing, 5 [Order], 2 [GradingSystemId], 'A99DC539-D363-416F-BDA8-00163D186919' [Guid]
	UNION SELECT 6 Id, 'D' [Name], 'Fails with a ""D"".' [Description], 63 [Threshold], 0 IsPassing, 6 [Order], 2 [GradingSystemId], '6E6A61C3-3305-491D-80C6-1C3723468460' [Guid]
	UNION SELECT 7 Id, 'F' [Name], 'Fais with an ""F"".' [Description], 0 [Threshold], 0 IsPassing, 7 [Order], 2 [GradingSystemId], '2F7885F5-4DFB-4057-92D7-2684B4542BF7' [Guid]
) seed
WHERE NOT EXISTS (
	SELECT 1
	FROM [dbo].[LearningGradingSystemScale] ex
	WHERE ex.Id = seed.Id
)

SET IDENTITY_INSERT [dbo].[LearningGradingSystemScale] OFF;

/* Group Type */
-- Create an LMS Group Type using a well-known Guid.
DECLARE @lmsGroupTypeGuid uniqueidentifier = '4BBC41E2-0A37-4289-B7A7-756B9FE8F775';
DECLARE @mustBelongToCheckIn INT = 2;
DECLARE @attendancePrintToDefault INT = 0;
DECLARE @groupLocationPicker INT = 2;

INSERT [GroupType] ( [IsSystem], [Name], [GroupTerm], [GroupMemberTerm], [AllowMultipleLocations], [ShowInGroupList], [ShowInNavigation], [TakesAttendance], [AttendanceRule], [AttendancePrintTo], [Order], [LocationSelectionMode], [Guid] )
SELECT [IsSystem], [Name], [GroupTerm], [GroupMemberTerm], [AllowMultipleLocations], [ShowInGroupList], [ShowInNavigation], [TakesAttendance], [AttendanceRule], [AttendancePrintTo], [Order], [LocationSelectionMode], [Guid]
FROM (
	SELECT 1 [IsSystem], 'LMS Class' [Name], 'Class' [GroupTerm], 'Participant' [GroupMemberTerm], 1 [AllowMultipleLocations], 1 [ShowInGroupList], 0 [ShowInNavigation], 1 [TakesAttendance], @mustBelongToCheckIn [AttendanceRule], @attendancePrintToDefault [AttendancePrintTo], 1 [Order], @groupLocationPicker [LocationSelectionMode], @lmsGroupTypeGuid [Guid]
) seed
WHERE NOT EXISTS (
	SELECT 1
	FROM GroupType
	WHERE [Guid] = seed.[Guid]
)

/* Group Type Roles */
-- Get the LMS Group Type based on the well-known Guid used above.
DECLARE @lmsGroupType INT = (SELECT TOP 1 Id FROM GroupType WHERE [Guid] = @lmsGroupTypeGuid);

INSERT [GroupTypeRole] ( [IsSystem], [GroupTypeId], [Name], [Order], [IsLeader], [Guid], [Description] )
SELECT [IsSystem], [GroupTypeId], [Name], [Order], [IsLeader], [Guid], [Description]
FROM (
	SELECT 1 [IsSystem], @lmsGroupType [GroupTypeId], 'Facilitator' [Name], 2 [Order], 1 [IsLeader], '80F802CE-2F59-4AB1-ABD8-CFD7A009A00A' [Guid], 'Indicates the person is a facilitator/teacher/administrator of the class' [Description]
	UNION SELECT 1 [IsSystem], @lmsGroupType [GroupTypeId], 'Student' [Name], 1 [Order], 0 [IsLeader], 'FA3ACAC2-0377-484C-B888-974CA3BF2FF2' [Guid], 'Indicates the person is enrolled as a student in the class' [Description]
) seed
WHERE NOT EXISTS (
	SELECT 1
	FROM GroupTypeRole ex
	WHERE ex.[Guid] = seed.[Guid]
)

/* Category */
DECLARE @entityTypeId INT = (SELECT TOP 1 Id FROM [dbo].[EntityType] WHERE [Name] = 'Rock.Model.LearningActivityCompletion');
DECLARE @categoryGuid NVARCHAR(200) = '6d0d5e3a-944c-4de9-a436-8b9bf37b4879';

INSERT [Category] ( [IsSystem], [EntityTypeId], [Name], [IconCssClass], [Description], [Order], [Guid] )
SELECT [IsSystem], [EntityTypeId], [Name], [IconCssClass], [Description], [Order], [Guid]
FROM (
	SELECT 1 [IsSystem], @entityTypeId [EntityTypeId], 'Learning Management' [Name], 'fa fa-university' [IconCssClass], 'System Category for Learning Management' [Description], 0 [Order], @categoryGuid [Guid]
) [seed]
WHERE NOT EXISTS (
	SELECT 1
	FROM [Category] [ex]
	WHERE [ex].[Guid] = [seed].[Guid]
)

/* System Communication */
DECLARE @categoryId INT = (SELECT TOP 1 [Id] FROM [dbo].[Category] WHERE [Guid] = @categoryGuid);
DECLARE @activityCommunicationGuid UNIQUEIDENTIFIER = 'd40a9c32-f179-4e5e-9b0d-ce208c5d1870';

DECLARE @activityNotificatonCommunicationSubject NVARCHAR(200) = 'New {%if ActivityCount == 1 %}Activity{% else %}Activities{%endif%} Available'
DECLARE @activityNotificatonCommunicationBody NVARCHAR(MAX) = '
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd""><html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
    <meta name=""viewport"" content=""width=device-width"">
    <style type=""text/css"">
        .grid {
            display: grid;
            grid-template-columns: 33%;
        }

        .grid.header > div {
            margin-bottom: 4px;
            grid-row: 1;
            font-weight: bold;
        }

        .column-1 {
            grid-column: 1;
        }
        
        .column-2 {
            grid-column: 2;
        }
        
        .column-3 {
            grid-column: 3;
        }
    </style>
</head>
<body>
    <h2 style=""display: flex; justify-content: center;"">
        <span>
        {%if ActivityCount == 1 %}
            You have a new activity available!
        {% else %}
            You have new activities available!
        {%endif%}
        </span>
    </h2>
    <div>
        {% for course in Courses %}
            <h3 style=""display: flex; justify-content: center;"">
                <span>
                {{course.ProgramName}}: {{course.CourseName}} - {{course.CourseCode}}
                </span>
            </h3>
            <div class=""grid header"">
                <div>
                    Activity
                </div>

                <div>
                    Available as of
                </div>

                <div>
                    Due
                </div>
            </div>
            <div class=""grid"">
                {% for activity in course.Activities %}
                    <div class=""column-1"">
                        {{activity.ActivityName}}
                    </div>

                    <div class=""column-2"">
                        {% if activity.AvailableDate == null %}
                            Always
                        {% else %}
                            <dd>{{ activity.AvailableDate | HumanizeDateTime }}</dd>
                        {% endif %}
                    </div>

                    <div class=""column-3"">
                        {% if activity.DueDate == null %}
                            Optional
                        {% else %}
                            {{ activity.DueDate | HumanizeDateTime }}
                        {% endif %}
                    </div>				
                {% endfor %}
            </div>
        {% endfor %}
    </div>
</body>
';

INSERT [SystemCommunication] ( [IsSystem], [IsActive], [Title], [Subject], [Body], [Guid], [CategoryId] )
SELECT [IsSystem], [IsActive], [Title], [Subject], [Body], [Guid], [CategoryId]
FROM (
	SELECT 1 [IsSystem], 1 [IsActive], 'Learning Activity Available' [Title], @activityNotificatonCommunicationSubject [Subject], @activityNotificatonCommunicationBody [Body], @activityCommunicationGuid [Guid],  @categoryId [CategoryId]
) [seed]
WHERE NOT EXISTS (
	SELECT 1
	FROM [SystemCommunication] [ex]
	WHERE [ex].[Guid] = [seed].[Guid]
)

/* Binary File Type */
DECLARE @storageEntityTypeId INT = (
	SELECT TOP 1 [e].[Id]
	FROM [dbo].[EntityType] [e]
	WHERE [e].[Name] = 'Rock.Storage.Provider.Database'
);

INSERT [BinaryFileType] ( [IsSystem], [Name], [CacheToServerFileSystem], [Description], [IconCssClass], [StorageEntityTypeId], [CacheControlHeaderSettings], [Guid], [RequiresViewSecurity] )
SELECT [IsSystem], [Name], [CacheToServerFileSystem], [Description], [IconCssClass], [StorageEntityTypeId], [CacheControlHeaderSettings], [Guid], [RequiresViewSecurity]
FROM (
	SELECT 
		1 [IsSystem], 
		'Learning Management' [Name], 
		1 [CacheToServerFileSystem], 
		'File related to the Learning Management System (LMS).' [Description], 
		'fa fa-university' [IconCssClass], 
		@storageEntityTypeId [StorageEntityTypeId],
		'{
			""RockCacheablityType"": 3,
			""MaxAge"": null,
			""MaxSharedAge"": null
		}' [CacheControlHeaderSettings], 
		'4f55987b-5279-4d10-8c38-f320046b4bbb' [Guid],
        1 [RequiresViewSecurity]
) [seed]
WHERE NOT EXISTS (
	SELECT 1
	FROM [BinaryFileType] [ex]
	WHERE [ex].[Guid] = [seed].[Guid]
)
" );
        }

        /// <summary>
        /// Removes the LMS Seed data from the database.
        /// </summary>
        private void RemoveSeedData()
        {
            var activityCommunicationGuid = "d40a9c32-f179-4e5e-9b0d-ce208c5d1870";
            var lmsBinaryFileTypeGuid = "4f55987b-5279-4d10-8c38-f320046b4bbb";
            var activityCompletionCategoryGuid = "6d0d5e3a-944c-4de9-a436-8b9bf37b4879";
            var lmsGroupTypeGuid = "4BBC41E2-0A37-4289-B7A7-756B9FE8F775";

            Sql( $"DELETE [BinaryFileType] WHERE [Guid] = '{lmsBinaryFileTypeGuid}';" );
            Sql( $"DELETE [SystemCommunication] WHERE [Guid] = '{activityCommunicationGuid}'" );
            Sql( $"DELETE [Category] WHERE [Guid] = '{activityCompletionCategoryGuid}'" );
            Sql( $"DELETE gm FROM [GroupMember] gm JOIN [GroupType] gt ON gm.[GroupTypeId] = gt.[Id] WHERE gt.[Guid] = '{lmsGroupTypeGuid}'" );
            Sql( $"DELETE gtr FROM [GroupTypeRole] gtr JOIN [GroupType] gt ON gtr.[GroupTypeId] = gt.[Id] WHERE gt.[Guid] = '{lmsGroupTypeGuid}'" );
            Sql( $"DELETE g FROM [Group] g JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId] WHERE gt.[Guid] = '{lmsGroupTypeGuid}'" );
            Sql( $"DELETE [GroupType] WHERE [Guid] = '{lmsGroupTypeGuid}'" );
        }

        /// <summary>
        ///  Deletes the SendLearningActivityNotifications Job based on Guid and Class.
        /// </summary>
        private void DeleteSendLearningActivityNotificationsJob()
        {
            var jobClass = "Rock.Jobs.SendLearningActivityNotifications";
            Sql( $"DELETE [ServiceJob] WHERE [Class] = '{jobClass}' AND [Guid] = '{SystemGuid.ServiceJob.SEND_LEARNING_ACTIVITY_NOTIFICATIONS}'" );
        }

        /// <summary>
        /// Adds or Updates the SendLearningActivityNotifications Job.
        /// </summary>
        private void AddOrUpdateSendLearningActivityNotificationsJob()
        {
            var cronSchedule = "0 0 7 1/1 * ? *"; // 7am daily.
            var jobClass = "Rock.Jobs.SendLearningActivityNotifications";
            var name = "Send Learning Activity Notifications";
            var description = "A job that sends notifications to students for newly available activities.";

            Sql( $@"
            IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = '{jobClass}' AND [Guid] = '{SystemGuid.ServiceJob.SEND_LEARNING_ACTIVITY_NOTIFICATIONS}' )
            BEGIN
                INSERT INTO [ServiceJob] (
                    [IsSystem],
                    [IsActive],
                    [Name],
                    [Description],
                    [Class],
                    [CronExpression],
                    [NotificationStatus],
                    [Guid] )
                VALUES (
                    0,
                    1,
                    '{name}',
                    '{description}',
                    '{jobClass}',
                    '{cronSchedule}',
                    1,
                    '{SystemGuid.ServiceJob.SEND_LEARNING_ACTIVITY_NOTIFICATIONS}' );
            END
            ELSE
            BEGIN
	            UPDATE	[ServiceJob]
	            SET
		              [IsSystem] = 1
		            , [IsActive] = 1
		            , [Name] = '{name}'
		            , [Description] = '{description}'
		            , [Class] = '{jobClass}'
		            , [CronExpression] = '{cronSchedule}'
		            , [NotificationStatus] = 1
	            WHERE
		              [Guid] = '{SystemGuid.ServiceJob.SEND_LEARNING_ACTIVITY_NOTIFICATIONS}';
            END" );
        }

        /// <summary>
        /// Results of CodeGen_PagesBlocksAttributesMigration.sql for LMS (Up).
        /// </summary>
        private void LmsEntityTypesPagesBlocksUp()
        {
            // Add Page 
            //  Internal Name: Learning
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "48242949-944A-4651-B6CC-60194EDE08A0", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Learning", "", "84DBEC51-EE0B-41C2-94B3-F361C4B98879", "" );

            // Add Page 
            //  Internal Name: Program
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "84DBEC51-EE0B-41C2-94B3-F361C4B98879", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Program", "", "7888CAF4-AF5D-44BA-AB9E-80138361F69D", "" );

            // Add Page 
            //  Internal Name: Courses
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "7888CAF4-AF5D-44BA-AB9E-80138361F69D", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Courses", "", "870318D8-5381-4B3C-BE4A-04E57125B590", "" );

            // Add Page 
            //  Internal Name: Completions
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "7888CAF4-AF5D-44BA-AB9E-80138361F69D", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Completions", "", "532BC5A9-40B3-42AF-9AD3-740FC0B3EB41", "" );

            // Add Page 
            //  Internal Name: Semesters
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "7888CAF4-AF5D-44BA-AB9E-80138361F69D", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Semesters", "", "F7073393-D3A7-4C2E-8001-A73F9E169D60", "" );

            // Add Page 
            //  Internal Name: Grading Systems
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Grading Systems", "", "F76C9FC6-CDE0-4122-8D05-7862D683A3CA", "fa fa-graduation-cap" );

            // Add Page 
            //  Internal Name: Grading System
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "F76C9FC6-CDE0-4122-8D05-7862D683A3CA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Grading System", "", "6163CCFD-CB15-452E-99F2-229A5E5B22F0", "" );

            // Add Page 
            //  Internal Name: Course
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "870318D8-5381-4B3C-BE4A-04E57125B590", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Course", "", "A57D990E-6F34-45CF-ABAA-08C40E8D4844", "" );

            // Add Page 
            //  Internal Name: Grading Scale
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "6163CCFD-CB15-452E-99F2-229A5E5B22F0", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Grading Scale", "", "AE85B3FC-C951-497F-8C43-9D0A1E467A50", "" );

            // Add Page 
            //  Internal Name: Class
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "A57D990E-6F34-45CF-ABAA-08C40E8D4844", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Class", "", "23D5076E-C062-4987-9985-B3A4792BF3CE", "" );

            // Add Page 
            //  Internal Name: Activity
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "23D5076E-C062-4987-9985-B3A4792BF3CE", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Activity", "", "D72DCBC4-C57F-4028-B503-1954925EDC7D", "" );

            // Add Page 
            //  Internal Name: Participant
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "23D5076E-C062-4987-9985-B3A4792BF3CE", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Participant", "", "72C75C91-18F8-48D0-B0CF-1FBD82EB50FC", "" );

            // Add Page 
            //  Internal Name: Semester
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "F7073393-D3A7-4C2E-8001-A73F9E169D60", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Semester", "", "36FFA805-B283-443E-990D-87040339D16F", "" );

            // Add Page 
            //  Internal Name: Completion
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "D72DCBC4-C57F-4028-B503-1954925EDC7D", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Completion", "", "E0F2E4F1-ED10-49F6-B053-AC6807994204", "" );

            // Add Page 
            //  Internal Name: Content Page
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "23D5076E-C062-4987-9985-B3A4792BF3CE", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Content Page", "", "E6A89360-B7B4-48A8-B799-39A27EAB6F36", "" );

            // Add Page 
            //  Internal Name: Announcement
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "23D5076E-C062-4987-9985-B3A4792BF3CE", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Announcement", "", "7C8134A4-524A-4C3D-BA4C-875FEE672850", "" );

            // Add Page Route
            //   Page:Learning
            //   Route:learning
            RockMigrationHelper.AddOrUpdatePageRoute( "84DBEC51-EE0B-41C2-94B3-F361C4B98879", "learning", "A3A52449-4B51-45CE-B91D-3AEEB42C1577" );

            // Add Page Route
            //   Page:Program
            //   Route:learning/{LearningProgramId}
            RockMigrationHelper.AddOrUpdatePageRoute( "7888CAF4-AF5D-44BA-AB9E-80138361F69D", "learning/{LearningProgramId}", "84F3567D-6FA8-4B78-8224-51B6BF995558" );

            // Add Page Route
            //   Page:Courses
            //   Route:learning/{LearningProgramId}/courses
            RockMigrationHelper.AddOrUpdatePageRoute( "870318D8-5381-4B3C-BE4A-04E57125B590", "learning/{LearningProgramId}/courses", "D82F6906-AEF2-440A-B2A5-3DA95419CE14" );

            // Add Page Route
            //   Page:Completions
            //   Route:learning/{LearningProgramId}/completion
            RockMigrationHelper.AddOrUpdatePageRoute( "532BC5A9-40B3-42AF-9AD3-740FC0B3EB41", "learning/{LearningProgramId}/completion", "218FB50F-A231-4029-887D-0F921918ECB1" );

            // Add Page Route
            //   Page:Semesters
            //   Route:learning/{LearningProgramId}/semester
            RockMigrationHelper.AddOrUpdatePageRoute( "F7073393-D3A7-4C2E-8001-A73F9E169D60", "learning/{LearningProgramId}/semester", "E95CC988-2D64-4299-9BA2-E6928B191A33" );

            // Add Page Route
            //   Page:Grading Systems
            //   Route:admin/system/grading-system
            RockMigrationHelper.AddOrUpdatePageRoute( "F76C9FC6-CDE0-4122-8D05-7862D683A3CA", "admin/system/grading-system", "E895CCC1-119A-48B3-80E5-DA7B04BDAB51" );

            // Add Page Route
            //   Page:Grading System
            //   Route:admin/system/grading-system/{LearningGradingSystemId}
            RockMigrationHelper.AddOrUpdatePageRoute( "6163CCFD-CB15-452E-99F2-229A5E5B22F0", "admin/system/grading-system/{LearningGradingSystemId}", "B6D9D035-FB6B-45D4-B1EA-66E1E1B5C846" );

            // Add Page Route
            //   Page:Course
            //   Route:learning/{LearningProgramId}/courses/{LearningCourseId}
            RockMigrationHelper.AddOrUpdatePageRoute( "A57D990E-6F34-45CF-ABAA-08C40E8D4844", "learning/{LearningProgramId}/courses/{LearningCourseId}", "C77EBCB8-F174-4F2D-8113-48D9B0D489EA" );

            // Add Page Route
            //   Page:Grading Scale
            //   Route:admin/system/grading-system/{LearningGradingSystemId}/scale/{LearningGradingSystemScaleId}
            RockMigrationHelper.AddOrUpdatePageRoute( "AE85B3FC-C951-497F-8C43-9D0A1E467A50", "admin/system/grading-system/{LearningGradingSystemId}/scale/{LearningGradingSystemScaleId}", "D4C428AD-300D-4A07-AEC7-94E9518A4A7D" );

            // Add Page Route
            //   Page:Class
            //   Route:learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}
            RockMigrationHelper.AddOrUpdatePageRoute( "23D5076E-C062-4987-9985-B3A4792BF3CE", "learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}", "5FCE29A7-4530-4CCE-9891-C95242923EFE" );

            // Add Page Route
            //   Page:Activity
            //   Route:learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/activities/{LearningActivityId}
            RockMigrationHelper.AddOrUpdatePageRoute( "D72DCBC4-C57F-4028-B503-1954925EDC7D", "learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/activities/{LearningActivityId}", "E2581432-C9D8-4324-97E2-BCFE6BBD0F57" );

            // Add Page Route
            //   Page:Participant
            //   Route:learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/participants/{LearningParticipantId}
            RockMigrationHelper.AddOrUpdatePageRoute( "72C75C91-18F8-48D0-B0CF-1FBD82EB50FC", "learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/participants/{LearningParticipantId}", "827AF9A8-BF1A-4008-B4C3-3D07076ACB84" );

            // Add Page Route
            //   Page:Semester
            //   Route:learning/{LearningProgramId}/semester/{LearningSemesterId}
            RockMigrationHelper.AddOrUpdatePageRoute( "36FFA805-B283-443E-990D-87040339D16F", "learning/{LearningProgramId}/semester/{LearningSemesterId}", "D796B863-964F-4A10-A880-9D398376851A" );

            // Add Page Route
            //   Page:Completion
            //   Route:learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/activities/{LearningActivityId}/completions/{LearningActivityCompletionId}
            RockMigrationHelper.AddOrUpdatePageRoute( "E0F2E4F1-ED10-49F6-B053-AC6807994204", "learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/activities/{LearningActivityId}/completions/{LearningActivityCompletionId}", "8C40AE8D-60C6-49DE-B7DE-BE46D8A64AA6" );

            // Add Page Route
            //   Page:Content Page
            //   Route:learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/content-pages/{LearningClassContentPageId}
            RockMigrationHelper.AddOrUpdatePageRoute( "E6A89360-B7B4-48A8-B799-39A27EAB6F36", "learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/content-pages/{LearningClassContentPageId}", "4F35DC5D-FBB5-4B10-91B4-BC1C6A7009E8" );

            // Add Page Route
            //   Page:Announcement
            //   Route:learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/announcement/{LearningClassAnnouncementId}
            RockMigrationHelper.AddOrUpdatePageRoute( "7C8134A4-524A-4C3D-BA4C-875FEE672850", "learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/announcement/{LearningClassAnnouncementId}", "864A8615-AC20-4B3A-9D5F-087E92859AD1" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningClassList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningClassList", "Learning Class List", "Rock.Blocks.Lms.LearningClassList, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "AB72D147-D4CA-4FF5-AB49-696319CB9844" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningCourseDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningCourseDetail", "Learning Course Detail", "Rock.Blocks.Lms.LearningCourseDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "CB48C60A-E518-42E8-AA52-6A549A1A4152" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningCourseList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningCourseList", "Learning Course List", "Rock.Blocks.Lms.LearningCourseList, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "E882D582-BC31-4B68-945B-D0D44A2CE5BC" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningParticipantDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningParticipantDetail", "Learning Participant Detail", "Rock.Blocks.Lms.LearningParticipantDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "7F3752EF-7A4A-4F96-BD5C-E6609F0BFAC6" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningProgramCompletionList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningProgramCompletionList", "Learning Program Completion List", "Rock.Blocks.Lms.LearningProgramCompletionList, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "A4E78CB9-B53C-4188-BB9A-4435C051916D" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningProgramDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningProgramDetail", "Learning Program Detail", "Rock.Blocks.Lms.LearningProgramDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "41351A30-3B4F-44DA-B413-49D7C997FBB5" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningProgramList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningProgramList", "Learning Program List", "Rock.Blocks.Lms.LearningProgramList, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "ADEA7D88-0C99-4B0C-9784-6E97074C2742" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningSemesterList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningSemesterList", "Learning Semester List", "Rock.Blocks.Lms.LearningSemesterList, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "928978C0-9695-454D-9E17-33F12F278F78" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningGradingSystemList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningGradingSystemList", "Learning Grading System List", "Rock.Blocks.Lms.LearningGradingSystemList, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "10D43EAD-972C-4D6D-B5CB-A2D463247369" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningGradingSystemDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningGradingSystemDetail", "Learning Grading System Detail", "Rock.Blocks.Lms.LearningGradingSystemDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "C174C0EF-9085-4ED6-B21D-019E2AC04B12" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningGradingSystemScaleList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningGradingSystemScaleList", "Learning Grading System Scale List", "Rock.Blocks.Lms.LearningGradingSystemScaleList, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "FB5A07B0-CA85-460E-8700-2E57AE5194C8" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningGradingSystemScaleDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningGradingSystemScaleDetail", "Learning Grading System Scale Detail", "Rock.Blocks.Lms.LearningGradingSystemScaleDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "B14CB1A6-B60B-45B0-8F7D-457A869A25F2" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningActivityCompletionList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningActivityCompletionList", "Learning Activity Completion List", "Rock.Blocks.Lms.LearningActivityCompletionList, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "152FEA00-5721-4CB2-897F-1B6829F4B7C4" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningSemesterDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningSemesterDetail", "Learning Semester Detail", "Rock.Blocks.Lms.LearningSemesterDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "78BCF0D7-B5AC-4429-8055-B436652083A7" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningActivityDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningActivityDetail", "Learning Activity Detail", "Rock.Blocks.Lms.LearningActivityDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "FE13BFEF-6266-4667-B51F-01AF8E6C5B89" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningClassDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningClassDetail", "Learning Class Detail", "Rock.Blocks.Lms.LearningClassDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "08B8DA88-BE2E-4237-883D-B9A2DB5F6260" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningActivityCompletionDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningActivityCompletionDetail", "Learning Activity Completion Detail", "Rock.Blocks.Lms.LearningActivityCompletionDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "19474EB0-EEDA-4FCB-B1EA-A35E23E6F691" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.PublicLearningProgramList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.PublicLearningProgramList", "Public Learning Program List", "Rock.Blocks.Lms.PublicLearningProgramList, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "59D82730-E4A7-4AAF-BB1E-BEC4B7AA8624" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.PublicLearningCourseList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.PublicLearningCourseList", "Public Learning Course List", "Rock.Blocks.Lms.PublicLearningCourseList, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "4356FEBE-5EFD-421A-BFC4-05942B6BD910" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.PublicLearningCourseDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.PublicLearningCourseDetail", "Public Learning Course Detail", "Rock.Blocks.Lms.PublicLearningCourseDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "C5D5A151-038E-4295-A03C-63196883F68E" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.PublicLearningClassWorkspace
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.PublicLearningClassWorkspace", "Public Learning Class Workspace", "Rock.Blocks.Lms.PublicLearningClassWorkspace, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "1BF70976-85AC-43D3-B98A-0B87A2FFD9B6" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningClassAnnouncementDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningClassAnnouncementDetail", "Learning Class Announcement Detail", "Rock.Blocks.Lms.LearningClassAnnouncementDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "08429949-4774-41F7-8840-2D8DEFFF14AB" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningClassContentPageDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningClassContentPageDetail", "Learning Class Content Page Detail", "Rock.Blocks.Lms.LearningClassContentPageDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "0220D970-1865-4B79-9E07-3CE9452E2B82" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Activity Completion Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningActivityCompletionDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Activity Completion Detail", "Displays the details of a particular learning activity completion.", "Rock.Blocks.Lms.LearningActivityCompletionDetail", "LMS", "4569F28D-1EFB-4B95-A506-0D9043C24775" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Activity Completion List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningActivityCompletionList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Activity Completion List", "Displays a list of learning activity completions.", "Rock.Blocks.Lms.LearningActivityCompletionList", "LMS", "EF1A5CDD-6769-4FFC-B826-55C194B01897" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Activity Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningActivityDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Activity Detail", "Displays the details of a particular learning activity.", "Rock.Blocks.Lms.LearningActivityDetail", "LMS", "4B18BF0D-D91B-4934-AC2D-A7188B15B893" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Activity List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningActivityList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Activity List", "Displays a list of learning activities.", "Rock.Blocks.Lms.LearningActivityList", "LMS", "5CEB6EC7-69F5-43B6-A74F-144A57F9B465" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Class Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningClassDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Class Detail", "Displays the details of a particular learning class.", "Rock.Blocks.Lms.LearningClassDetail", "LMS", "D5369F8D-11AA-482B-AE08-2B3C519D8D87" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Class List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningClassList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Class List", "Displays a list of learning classes.", "Rock.Blocks.Lms.LearningClassList", "LMS", "340F6CC1-8C38-4579-9383-A6168680194A" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Course Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningCourseDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Course Detail", "Displays the details of a particular learning course.", "Rock.Blocks.Lms.LearningCourseDetail", "LMS", "94C4CB0B-5617-4F46-B902-6E6DD4A447B8" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Course List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningCourseList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Course List", "Displays a list of learning courses.", "Rock.Blocks.Lms.LearningCourseList", "LMS", "D0AFDF98-4AFC-4E4F-A6E2-07CA4E7358E8" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Grading System Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningGradingSystemDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Grading System Detail", "Displays the details of a particular learning grading system.", "Rock.Blocks.Lms.LearningGradingSystemDetail", "LMS", "A4182806-95B0-49AE-97B5-246A834156E3" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Grading System List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningGradingSystemList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Grading System List", "Displays a list of learning grading systems.", "Rock.Blocks.Lms.LearningGradingSystemList", "LMS", "F003DAAC-B9FE-4007-B218-983084E1126B" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Grading System Scale Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningGradingSystemScaleDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Grading System Scale Detail", "Displays the details of a particular learning grading system scale.", "Rock.Blocks.Lms.LearningGradingSystemScaleDetail", "LMS", "34D7A280-8D4D-4FE5-BB45-810732F76341" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Grading System Scale List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningGradingSystemScaleList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Grading System Scale List", "Displays a list of learning grading system scales.", "Rock.Blocks.Lms.LearningGradingSystemScaleList", "LMS", "27390ED3-57B2-42EF-A212-F8B29851F9BA" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Participant Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningParticipantDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Participant Detail", "Displays the details of a particular learning participant.", "Rock.Blocks.Lms.LearningParticipantDetail", "LMS", "F1179439-31A1-4897-AB2E-B991D60455AA" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Program Completion List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningProgramCompletionList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Program Completion List", "Displays a list of learning program completions.", "Rock.Blocks.Lms.LearningProgramCompletionList", "LMS", "CE703EB6-028F-47FC-A09A-AD8F72C12CBC" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Program Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningProgramDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Program Detail", "Displays the details of a particular learning program.", "Rock.Blocks.Lms.LearningProgramDetail", "LMS", "796C87E7-678F-4A38-8C04-A401A4F7AC21" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Program List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningProgramList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Program List", "Displays a list of learning programs.", "Rock.Blocks.Lms.LearningProgramList", "LMS", "7B1DB013-A552-455F-A1D0-7B17488D0D5C" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Semester Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningSemesterDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Semester Detail", "Displays the details of a particular learning semester.", "Rock.Blocks.Lms.LearningSemesterDetail", "LMS", "97B2E57F-3A03-490D-834F-CD3640C7FF1E" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Semester List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningSemesterList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Semester List", "Displays a list of learning semesters.", "Rock.Blocks.Lms.LearningSemesterList", "LMS", "C89C7F15-FB8A-43D4-9AFB-5E40E397F246" );

            // Add/Update Obsidian Block Type
            //   Name:Public Learning Class Workspace
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.PublicLearningClassWorkspace
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Public Learning Class Workspace", "The main block for interacting with enrolled classes.", "Rock.Blocks.Lms.PublicLearningClassWorkspace", "LMS", "55F2E89B-DE57-4E24-AC6C-576956FB97C5" );

            // Add/Update Obsidian Block Type
            //   Name:Public Learning Course Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.PublicLearningCourseDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Public Learning Course Detail", "Displays the details of a particular public learning course.", "Rock.Blocks.Lms.PublicLearningCourseDetail", "LMS", "B0DCE130-0C91-4AA0-8161-57E8FA523392" );

            // Add/Update Obsidian Block Type
            //   Name:Public Learning Course List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.PublicLearningCourseList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Public Learning Course List", "Displays a list of public learning courses.", "Rock.Blocks.Lms.PublicLearningCourseList", "LMS", "5D6BA94F-342A-4EC1-B024-FC5046FFE14D" );

            // Add/Update Obsidian Block Type
            //   Name:Public Learning Program List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.PublicLearningProgramList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Public Learning Program List", "Displays a list of public learning programs.", "Rock.Blocks.Lms.PublicLearningProgramList", "LMS", "DA1460D8-E895-4B23-8A8E-10EBBED3990F" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Class Announcement Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningClassAnnouncementDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Class Announcement Detail", "Displays the details of a particular learning class announcement.", "Rock.Blocks.Lms.LearningClassAnnouncementDetail", "LMS", "53C12A53-773E-4398-8627-DD44C1421675" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Class Content Page Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningClassContentPageDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Class Content Page Detail", "Displays the details of a particular learning class content page.", "Rock.Blocks.Lms.LearningClassContentPageDetail", "LMS", "92E0DB96-0FFF-41F2-982D-F750AB23EF5B" );

            // Add Block 
            //  Block Name: Learning Program List
            //  Page Name: Learning
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "84DBEC51-EE0B-41C2-94B3-F361C4B98879".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "7B1DB013-A552-455F-A1D0-7B17488D0D5C".AsGuid(), "Learning Program List", "Main", @"", @"", 1, "B7D8BC2F-CEDD-4E22-8D46-69A759DCB59C" );

            // Add Block 
            //  Block Name: Current Classes
            //  Page Name: Program
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "7888CAF4-AF5D-44BA-AB9E-80138361F69D".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "340F6CC1-8C38-4579-9383-A6168680194A".AsGuid(), "Current Classes", "Main", @"", @"", 2, "53A87507-D270-4340-A93F-AD3AB023E0B1" );

            // Add Block 
            //  Block Name: Program
            //  Page Name: Program
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "7888CAF4-AF5D-44BA-AB9E-80138361F69D".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "796C87E7-678F-4A38-8C04-A401A4F7AC21".AsGuid(), "Program", "Main", @"", @"", 1, "D4ED6CD7-C28E-4ABB-90B3-39499BE41E1B" );

            // Add Block 
            //  Block Name: Learning Course List
            //  Page Name: Courses
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "870318D8-5381-4B3C-BE4A-04E57125B590".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D0AFDF98-4AFC-4E4F-A6E2-07CA4E7358E8".AsGuid(), "Learning Course List", "Main", @"", @"", 0, "60979468-1F45-467B-A600-B1A230BF6CB9" );

            // Add Block 
            //  Block Name: Program Completions
            //  Page Name: Completions
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "532BC5A9-40B3-42AF-9AD3-740FC0B3EB41".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CE703EB6-028F-47FC-A09A-AD8F72C12CBC".AsGuid(), "Program Completions", "Main", @"", @"", 0, "ADCFF541-8908-4F78-9B96-2C3E37D5C4AB" );

            // Add Block 
            //  Block Name: Semesters
            //  Page Name: Semesters
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F7073393-D3A7-4C2E-8001-A73F9E169D60".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "C89C7F15-FB8A-43D4-9AFB-5E40E397F246".AsGuid(), "Semesters", "Main", @"", @"", 0, "E58406FD-E17B-4480-95EB-ADCFD956CA17" );

            // Add Block 
            //  Block Name: Learning Grading System List
            //  Page Name: Grading Systems
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F76C9FC6-CDE0-4122-8D05-7862D683A3CA".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "F003DAAC-B9FE-4007-B218-983084E1126B".AsGuid(), "Learning Grading System List", "Main", @"", @"", 0, "AF6C3E7F-C7EB-40ED-BCE2-038FABC6A868" );

            // Add Block 
            //  Block Name: Learning Grading System Detail
            //  Page Name: Grading System
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6163CCFD-CB15-452E-99F2-229A5E5B22F0".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "A4182806-95B0-49AE-97B5-246A834156E3".AsGuid(), "Learning Grading System Detail", "Main", @"", @"", 0, "88233998-6298-40F8-B433-931486D30B2D" );

            // Add Block 
            //  Block Name: Grading Scales
            //  Page Name: Grading System
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6163CCFD-CB15-452E-99F2-229A5E5B22F0".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "27390ED3-57B2-42EF-A212-F8B29851F9BA".AsGuid(), "Grading Scales", "Main", @"", @"", 1, "FD56EBFE-8BC5-42B7-8847-941B58AD14B6" );

            // Add Block 
            //  Block Name: Classes
            //  Page Name: Course
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "A57D990E-6F34-45CF-ABAA-08C40E8D4844".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "340F6CC1-8C38-4579-9383-A6168680194A".AsGuid(), "Classes", "Main", @"", @"", 1, "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0" );

            // Add Block 
            //  Block Name: Learning Course Detail
            //  Page Name: Course
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "A57D990E-6F34-45CF-ABAA-08C40E8D4844".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "94C4CB0B-5617-4F46-B902-6E6DD4A447B8".AsGuid(), "Learning Course Detail", "Main", @"", @"", 0, "D85084D3-E298-4307-9AA2-C1570C4A3FA4" );

            // Add Block 
            //  Block Name: Grading Scale
            //  Page Name: Grading Scale
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "AE85B3FC-C951-497F-8C43-9D0A1E467A50".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "34D7A280-8D4D-4FE5-BB45-810732F76341".AsGuid(), "Grading Scale", "Main", @"", @"", 0, "5FBFF087-3366-4620-8C5C-6A12F8BC6BD2" );

            // Add Block 
            //  Block Name: Class
            //  Page Name: Class
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "23D5076E-C062-4987-9985-B3A4792BF3CE".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D5369F8D-11AA-482B-AE08-2B3C519D8D87".AsGuid(), "Class", "Main", @"", @"", 0, "C67D2164-33E5-46C0-94EF-DF387EF8477D" );

            // Add Block 
            //  Block Name: Learning Activity Detail
            //  Page Name: Activity
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "D72DCBC4-C57F-4028-B503-1954925EDC7D".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "4B18BF0D-D91B-4934-AC2D-A7188B15B893".AsGuid(), "Learning Activity Detail", "Main", @"", @"", 0, "8BDFF6BF-E043-4EB5-BDD9-C1813AE83295" );

            // Add Block 
            //  Block Name: Activity List
            //  Page Name: Activity
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "D72DCBC4-C57F-4028-B503-1954925EDC7D".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "EF1A5CDD-6769-4FFC-B826-55C194B01897".AsGuid(), "Activity List", "Main", @"", @"", 1, "7CE84856-A789-4D74-AB6C-DA05A5C82479" );

            // Add Block 
            //  Block Name: Participant
            //  Page Name: Participant
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "72C75C91-18F8-48D0-B0CF-1FBD82EB50FC".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "F1179439-31A1-4897-AB2E-B991D60455AA".AsGuid(), "Participant", "Main", @"", @"", 0, "8C05CAC8-AB57-46F2-8E0C-21293BE8F464" );

            // Add Block 
            //  Block Name: Semester
            //  Page Name: Semester
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "36FFA805-B283-443E-990D-87040339D16F".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "97B2E57F-3A03-490D-834F-CD3640C7FF1E".AsGuid(), "Semester", "Main", @"", @"", 0, "2DF1C8E3-B65E-47F0-8B82-86AA5A3DA21A" );

            // Add Block 
            //  Block Name: Activity Completion Detail
            //  Page Name: Completion
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "E0F2E4F1-ED10-49F6-B053-AC6807994204".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "4569F28D-1EFB-4B95-A506-0D9043C24775".AsGuid(), "Activity Completion Detail", "Main", @"", @"", 0, "43D3D01A-101E-494E-AF4D-BBEE9595DE0A" );

            // Add Block 
            //  Block Name: Class Content Page
            //  Page Name: Class Content Page
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "E6A89360-B7B4-48A8-B799-39A27EAB6F36".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "92E0DB96-0FFF-41F2-982D-F750AB23EF5B".AsGuid(), "Class Content Page", "Main", @"", @"", 0, "9788FDD8-A587-48CA-BE0B-D5A937CCEE70" );

            // Add Block 
            //  Block Name: Class Announcement Detail
            //  Page Name: Announcement
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "7C8134A4-524A-4C3D-BA4C-875FEE672850".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "53C12A53-773E-4398-8627-DD44C1421675".AsGuid(), "Class Announcement Detail", "Main", @"", @"", 0, "3EAEE2BB-E223-4C47-9C9D-230A412D6D2A" );

            // update block order for pages with new blocks if the page,zone has multiple blocks

            // Update Order for Page: Activity,  Zone: Main,  Block: Activity List
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '7CE84856-A789-4D74-AB6C-DA05A5C82479'" );

            // Update Order for Page: Activity,  Zone: Main,  Block: Learning Activity Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '8BDFF6BF-E043-4EB5-BDD9-C1813AE83295'" );

            // Update Order for Page: Course,  Zone: Main,  Block: Classes
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '0AE21CCB-BE0C-4565-8EC9-33A61C503DC0'" );

            // Update Order for Page: Course,  Zone: Main,  Block: Learning Course Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'D85084D3-E298-4307-9AA2-C1570C4A3FA4'" );

            // Update Order for Page: Grading System,  Zone: Main,  Block: Grading Scales
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'FD56EBFE-8BC5-42B7-8847-941B58AD14B6'" );

            // Update Order for Page: Grading System,  Zone: Main,  Block: Learning Grading System Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '88233998-6298-40F8-B433-931486D30B2D'" );

            // Update Order for Page: Learning,  Zone: Main,  Block: Learning Program List
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'B7D8BC2F-CEDD-4E22-8D46-69A759DCB59C'" );

            // Update Order for Page: Program,  Zone: Main,  Block: Current Classes
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '53A87507-D270-4340-A93F-AD3AB023E0B1'" );

            // Update Order for Page: Program,  Zone: Main,  Block: Program
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'D4ED6CD7-C28E-4ABB-90B3-39499BE41E1B'" );

            // Attribute for BlockType
            //   BlockType: Learning Activity List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5CEB6EC7-69F5-43B6-A74F-144A57F9B465", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B4130B6F-E3DE-4161-A28D-B7F6F160CB38" );

            // Attribute for BlockType
            //   BlockType: Learning Activity List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5CEB6EC7-69F5-43B6-A74F-144A57F9B465", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "8783E0C3-6B14-4252-A912-A13C4D1E89B0" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: Show Location Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "340F6CC1-8C38-4579-9383-A6168680194A", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Show Location Column", "ShowLocationColumn", "Show Location Column", @"Select 'Show' to show the 'Location'.", 1, @"No", "4FBE92ED-3CF1-4BD4-A78E-B50C0080FA0B" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: Show Schedule Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "340F6CC1-8C38-4579-9383-A6168680194A", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Show Schedule Column", "ShowScheduleColumn", "Show Schedule Column", @"Select 'Show' to show the 'Schedule' column.", 2, @"No", "BECBD2AE-5D1E-4065-8CAA-FE7972CC5742" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: Show Semester Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "340F6CC1-8C38-4579-9383-A6168680194A", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Show Semester Column", "ShowSemesterColumn", "Show Semester Column", @"Select 'Show' to show the 'Semester' column when the configuration is 'Academic Calendar'.", 3, @"No", "28087DBE-A334-47BF-9CA5-76AFF5EE1A6F" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: Display Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "340F6CC1-8C38-4579-9383-A6168680194A", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Mode", "DisplayMode", "Display Mode", @"Select 'Show only Acadmemic Calendar Mode' to show the block only when the configuration mode is 'Academic Calendar'.", 4, @"AcademicCalendarOnly", "DA69DB3C-139E-4DC6-BBA5-DC03C0ACBD6A" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "340F6CC1-8C38-4579-9383-A6168680194A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the learning class details.", 5, @"", "0C995F98-F483-4814-B3A1-6FACCD2D686F" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "340F6CC1-8C38-4579-9383-A6168680194A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "FFEF4113-FBD9-49B6-BCBD-4844CFF5FA3B" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "340F6CC1-8C38-4579-9383-A6168680194A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "4778450C-5B2A-40AD-9FC0-B3ED8AAE2F5C" );

            // Attribute for BlockType
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Attribute: Activity Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "94C4CB0B-5617-4F46-B902-6E6DD4A447B8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Activity Detail Page", "ActivityDetailPage", "Activity Detail Page", @"The page that will be navigated to when clicking an activity row.", 1, @"", "30C34DC5-08B4-4826-9EA1-5008B0864805" );

            // Attribute for BlockType
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Attribute: Participant Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "94C4CB0B-5617-4F46-B902-6E6DD4A447B8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Participant Detail Page", "ParticipantDetailPage", "Participant Detail Page", @"The page that will be navigated to when clicking a student or facilitator row.", 2, @"72C75C91-18F8-48D0-B0CF-1FBD82EB50FC", "5F4ADFD7-1CA0-4A3F-8ADA-1B0744119CB0" );

            // Attribute for BlockType
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D0AFDF98-4AFC-4E4F-A6E2-07CA4E7358E8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the learning course details.", 0, @"", "67E7F552-C0F2-4852-B817-E216795D1E30" );

            // Attribute for BlockType
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D0AFDF98-4AFC-4E4F-A6E2-07CA4E7358E8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "16763698-7847-43BF-B2F2-68FBB4408FF8" );

            // Attribute for BlockType
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D0AFDF98-4AFC-4E4F-A6E2-07CA4E7358E8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "223232AD-21C4-4024-ADE2-B9180B165728" );

            // Attribute for BlockType
            //   BlockType: Learning Program Completion List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CE703EB6-028F-47FC-A09A-AD8F72C12CBC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the learning program completion details.", 0, @"", "206A8316-1203-4661-A9E7-A4032C930075" );

            // Attribute for BlockType
            //   BlockType: Learning Program Completion List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CE703EB6-028F-47FC-A09A-AD8F72C12CBC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "C16D7666-6155-4D64-B515-BEB96C934E4C" );

            // Attribute for BlockType
            //   BlockType: Learning Program Completion List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CE703EB6-028F-47FC-A09A-AD8F72C12CBC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "C7138043-593C-4807-A0F1-E31B1A6F297F" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "796C87E7-678F-4A38-8C04-A401A4F7AC21", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Category", "Category", "Category", @"Optional category for the Program.", 1, @"", "BEABF6B9-F85E-44DD-9B70-D49209AD84A8" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Display Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "796C87E7-678F-4A38-8C04-A401A4F7AC21", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Mode", "DisplayMode", "Display Mode", @"Select 'Summary' to show only attributes that are 'Show on Grid'. Select 'Full' to show all attributes.", 2, @"Summary", "18731EB1-888B-4AB2-B1C1-759493B2E639" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Courses Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "796C87E7-678F-4A38-8C04-A401A4F7AC21", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Courses Page", "CoursesPage", "Courses Page", @"The page that will show the courses for the learning program.", 4, @"", "8C648BE3-357F-42D1-9402-500A290F9FD5" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Completions Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "796C87E7-678F-4A38-8C04-A401A4F7AC21", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Completions Page", "CompletionsPage", "Completions Page", @"The page that will show the course completions for the learning program.", 5, @"", "94676A26-01A8-4D53-B382-70DDF17AEEDA" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Semesters Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "796C87E7-678F-4A38-8C04-A401A4F7AC21", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Semesters Page", "SemestersPage", "Semesters Page", @"The page that will show the semesters for the learning program.", 6, @"", "80BFAC80-B689-4B66-898E-69DA756DE093" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Show KPIs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "796C87E7-678F-4A38-8C04-A401A4F7AC21", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show KPIs", "ShowKPIs", "Show KPIs", @"Determines if the KPIs are visible.", 0, @"Yes", "6AE52A7E-EFA3-4685-B331-A2D3058438D3" );

            // Attribute for BlockType
            //   BlockType: Learning Program List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7B1DB013-A552-455F-A1D0-7B17488D0D5C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the learning program details.", 0, @"", "CE5E2633-0E73-4A30-AB96-DE6A0BB40AD6" );

            // Attribute for BlockType
            //   BlockType: Learning Program List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7B1DB013-A552-455F-A1D0-7B17488D0D5C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B870E6CC-2952-400C-ADAA-B51BF3147E6D" );

            // Attribute for BlockType
            //   BlockType: Learning Program List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7B1DB013-A552-455F-A1D0-7B17488D0D5C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F676DCD1-F5DA-440A-80C9-29930A0B7E21" );

            // Attribute for BlockType
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C89C7F15-FB8A-43D4-9AFB-5E40E397F246", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the learning semester details.", 0, @"", "EEA24DC4-4CFC-4B34-9348-917839DDBBA2" );

            // Attribute for BlockType
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C89C7F15-FB8A-43D4-9AFB-5E40E397F246", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "FCA17D14-C775-4651-A41F-C5EF673B446F" );

            // Attribute for BlockType
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C89C7F15-FB8A-43D4-9AFB-5E40E397F246", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "57106CC3-5D67-40BA-809B-2EA9826478D8" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F003DAAC-B9FE-4007-B218-983084E1126B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the learning grading system details.", 0, @"", "877C67A0-91A2-45C2-94AC-D2FB4951A0C8" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F003DAAC-B9FE-4007-B218-983084E1126B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "56A23C5C-8BE5-419A-9945-9044CBB514E9" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F003DAAC-B9FE-4007-B218-983084E1126B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F07A92F7-2B8B-4ADA-AE64-C8E3D7734A65" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System Scale List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "27390ED3-57B2-42EF-A212-F8B29851F9BA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the learning grading system scale details.", 0, @"", "B930784E-C501-482C-8007-C9C3380BFDF4" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System Scale List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "27390ED3-57B2-42EF-A212-F8B29851F9BA", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "9EF3D35C-C7C0-4AF2-8F1C-4682FDB79FDB" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System Scale List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "27390ED3-57B2-42EF-A212-F8B29851F9BA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "AAB4897B-5FED-4093-BAC0-C2CA043CC285" );

            // Attribute for BlockType
            //   BlockType: Learning Activity Completion List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EF1A5CDD-6769-4FFC-B826-55C194B01897", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the learning activity completion details.", 0, @"", "7733ED7C-A8C4-4501-8BD1-6B66F54DF13B" );

            // Attribute for BlockType
            //   BlockType: Learning Activity Completion List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EF1A5CDD-6769-4FFC-B826-55C194B01897", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "BABBE946-AB0E-405B-9787-5A09D9D055DF" );

            // Attribute for BlockType
            //   BlockType: Learning Activity Completion List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EF1A5CDD-6769-4FFC-B826-55C194B01897", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "2DB21AA3-F122-4D3A-97A5-3BD3F0020D13" );

            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Attendance Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D5369F8D-11AA-482B-AE08-2B3C519D8D87", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Attendance Page", "AttendancePage", "Attendance Page", @"The page that to be used for taking attendance for the class.", 2, @"", "B417E2A7-BBA1-453F-9933-3BE439CD2063" );

            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Activity Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D5369F8D-11AA-482B-AE08-2B3C519D8D87", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Activity Detail Page", "ActivityDetailPage", "Activity Detail Page", @"The page that will be navigated to when clicking an activity row.", 1, @"", "44565F75-B4CA-4438-A9B6-BEB943813559" );

            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Participant Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D5369F8D-11AA-482B-AE08-2B3C519D8D87", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Participant Detail Page", "ParticipantDetailPage", "Participant Detail Page", @"The page that to be used for taking attendance for the class.", 3, @"", "1B237D13-A86A-42CC-8E91-96CBCCAC6866" );

            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Content Page Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D5369F8D-11AA-482B-AE08-2B3C519D8D87", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Content Page Detail Page", "ContentPageDetailPage", "Content Page Detail Page", @"The page that will be navigated to when clicking a content page row.", 4, @"", "AAF9675D-BAF0-4F25-8C63-4F864F4369C0" );

            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Announcement Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D5369F8D-11AA-482B-AE08-2B3C519D8D87", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Announcement Detail Page", "AnnouncementDetailPage", "Announcement Detail Page", @"The page that will be navigated to when clicking a content page row.", 5, @"", "B570F05B-95DE-4F22-95FF-DDDCCE943E29" );

            // Attribute for BlockType
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Attribute: Participant Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "94C4CB0B-5617-4F46-B902-6E6DD4A447B8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Participant Detail Page", "ParticipantDetailPage", "Participant Detail Page", @"The page that will be navigated to when clicking a student or facilitator row.", 2, @"", "75CF60D8-19F7-45DB-BB6C-CEF859F2327D" );

            // Attribute for BlockType
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Attribute: Content Page Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "94C4CB0B-5617-4F46-B902-6E6DD4A447B8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Content Page Detail Page", "ContentPageDetailPage", "Content Page Detail Page", @"The page that will be navigated to when clicking a content page row.", 3, @"", "8FFDFA67-E20E-4BEC-91D3-7DDF7975291C" );

            // Attribute for BlockType
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Attribute: Announcement Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "94C4CB0B-5617-4F46-B902-6E6DD4A447B8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Announcement Detail Page", "AnnouncementDetailPage", "Announcement Detail Page", @"The page that will be navigated to when clicking a content page row.", 4, @"", "45F3C2D4-8C84-4CE2-BAB3-ACF93DC8E940" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA1460D8-E895-4B23-8A8E-10EBBED3990F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the courses for the program.", 0, @"", "0317E0AD-9FE9-409B-9A14-C3D30D303B23" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Program List Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA1460D8-E895-4B23-8A8E-10EBBED3990F", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Program List Template", "ProgramListTemplate", "Program List Template", @"The lava template showing the program list. Merge fields include: Programs, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
//- Styles
{% stylesheet %}
    .page-container {
        display: flex;
        flex-direction: column;
    }
    
    .page-header-section {
        height: 280px; 
        align-items: center; 
        border-radius: 12px; 
        background-image: url('/GetImage.ashx?guid=4812baaf-a173-472c-a9a7-8ceb83c06f53'); 
        background-size: cover;
    }
    
    .programs-list-header-section {
        margin-top: 100px;   
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        bottom: -80%;
        -webkit-transform: translateY(-30%);
        transform: translateY(-30%);
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        margin-bottom: 12px;
        padding-bottom: 12px;
    }
    
    .program-item-container {
        background-color: white; 
        border-radius: 12px;
        margin: 8px;
        display: flex;
        flex-direction: column;
        justify-content: space-between;
    }
{% endstylesheet %}
<div class=""page-container"" >
	<div class=""page-header-section"">
		<div class=""header-block text-center"">
			<div class=""h1 text-bold"">
				Growth Through Learning
			</div>
			<div class=""page-sub-header"">
				We believe that spiritual growth is deeply intertwined with continuous learning. 
				""Growth Through Learning"" is our commitment to nurture your faith journey, providing resources and opportunies to deepen your understanding of God's Word and his purpose for your life.
			</div>
		</div>
	</div>
	
	<div class=""programs-list-header-section center-block text-center mb-4"">
		<span class=""program-list-header h5"">
			Programs Available
		</span>

		<div class=""program-list-sub-header text-muted"">
			The following types of classes are available.
		</div>
	</div>
	
	<div class=""program-list-container d-flex flex-fill"">
		{% for program in Programs %}
		<div class=""program-item-container"">
		
			{% if program.ImageFileGuid and program.ImageFileGuid != '' %}
			<div class=""program-image program-item-header"" >
				<img style=""border-radius: 12px 12px 0 0;"" src=""/GetImage.ashx?guid={{program.ImageFileGuid}}&maxwidth=300&maxheight=150&mode=crop"" />
			</div>
			{% endif %}
			
			<div class=""program-item-middle p-3"">
			
				<h4 class=""program-name"">
					{{ program.Entity.PublicName }}
				</h4>
				<div class=""program-category"">
					<span class=""badge badge-info"">{{ program.Category }}</span>
				</div>
				<div class=""program-summary text-muted"">
					{{ program.Entity.Summary }} 
				</div>
			</div>
			
			<div class=""program-item-footer d-flex justify-content-between mt-4 p-3"">
				<a class=""btn btn-default"" href=""{{ program.CoursesLink }}"">Learn More</a>
				
				{% if program.CompletionStatus == 'Completed' %}
					<span class=""badge badge-success p-2"" style=""line-height: normal;"">Completed</span>
				{% elseif program.CompletionStatus == 'Pending' %}
					<span class=""badge badge-info p-2"" style=""line-height: normal;"">Enrolled</span>
				{% endif %}
			</div>
		</div>
		{% endfor %}
	</div>
</div>", "63F97CF2-774C-480C-9933-A2BAA664DCE2" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the course details.", 0, @"", "5A8A251D-56B3-4E3B-8BF9-1D333C016B74" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Course List Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Course List Template", "CourseListTemplate", "Course List Template", @"The lava template showing the courses list. Merge fields include: Program, Courses, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
//- Variables
{% assign imageFileNameLength = Program.ImageBinaryFile.Guid | Size %}

//- Styles
{% stylesheet %}
    .page-container {
        display: flex;
        flex-direction: column;
        margin-bottom: 12px;
    }
    
    .page-header-section {
        {% if imageFileNameLength > 0 %}
            height: 280px; 
        {% endif %}
        align-items: center; 
        border-radius: 12px; 
        background-image: url('/GetImage.ashx?guid={{Program.ImageBinaryFile.Guid}}'); 
        background-size: cover;
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        {% if imageFileNameLength > 0 %}
            bottom: -85%;
            -webkit-transform: translateY(-30%);
            transform: translateY(-30%);
        {% endif %}
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        padding-bottom: 12px;
        margin-bottom: 12px;
    }
    
    .course-item-container {
        max-width: 300px;
        background-color: white; 
        border-radius: 12px;
        margin: 8px;
        display: flex;
        flex-direction: column;
        justify-content: space-between;
    }
{% endstylesheet %}
<div class=""page-container"">
	<div class=""page-header-section mb-5"">
		<div class=""header-block text-center"">
			<h2>
				{{ Program.Name }}
			</h2>
			<div class=""page-sub-header"">
				{{ Program.Summary }}
			</div>
		</div>
	</div>
	
	<div class=""course-list-header-section center-block text-center mb-4"">
		<span class=""course-list-header h5"">
			Courses
		</span>
	</div>
	
	<div class=""course-list-container d-flex flex-fill"">
		{% for course in Courses %}
		<div class=""course-item-container"">
			<div class=""course-item-middle p-3"">
			
				<h4 class=""course-name"">
					{{ course.Entity.PublicName }}
				</h4>
				<div class=""course-category d-flex justify-content-between mb-2"">
				    {% if course.Category and course.Category <> '' %}
					    <span class=""badge badge-info"">{{ course.Category }}</span>
					{% else %}
					    <span> </span>
				    {% endif %}
				    {% if course.Entity.Credits %}
				        <span class=""badge"" style=""background-color: #ddedf2; color: #546a71;"">Credits: {{ course.Entity.Credits }}</span>
				    {% endif %}
				</div>
				<div class=""course-summary text-muted"">
					{{ course.Entity.Summary }} 
				</div>
			</div>
		
			<div class=""course-item-footer d-flex flex-column mt-4 p-3"">
                <div class=""course-next-session text-muted mb-3"">
                    <div class=""text-bold"">Next Session Starts</div>
                    <ul><li>{{ course.NextSemester.StartDate | Date:'MMMM dd, yyyy' }}</li></ul>
                </div>
                
                <div class=""d-flex justify-content-between"">
    				<a class=""btn btn-default"" href=""{{ course.CourseDetailsLink }}"">Learn More</a>
    				
    				{% if course.LearningCompletionStatus == 'Pass' %}
    					<span class=""badge badge-success p-2"" style=""line-height: normal;"">Completed</span>
    				{% elseif course.LearningCompletionStatus == 'Incomplete' %}
    					<span class=""badge badge-info p-2"" style=""line-height: normal;"">Enrolled</span>
    				{% elseif course.UnmetPrerequisites != empty %}
                        <a class=""text-muted"" href=""{{ course.PrerequisiteEnrollmentLink }}"">Prerequisites Not Met</a>
                    {% else %}
                        <a class=""text-bold ml-3"" href=""{{ course.CourseEnrollmentLink }}"">Enroll</a>
    				{% endif %}
			    </div>
			</div>
		</div>
		{% endfor %}
	</div>
</div>", "BB215F63-EF8A-4196-A4F1-A4A822B4BD60" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Enrollment Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Enrollment Page", "CourseEnrollmentPage", "Detail Page", @"The page that will enroll the student in the course.", 0, @"", "FF294CFA-6D54-4CBC-B8EF-D45893677D58" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Class Workspace Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0DCE130-0C91-4AA0-8161-57E8FA523392", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Class Workspace Page", "DetailPage", "Class Workspace Page", @"The page that will show the class workspace.", 0, @"", "8787C9F3-1CF9-4790-B65E-90303F446536" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Enrollment Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0DCE130-0C91-4AA0-8161-57E8FA523392", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Enrollment Page", "CourseEnrollmentPage", "Enrollment Page", @"The page that will enroll the student in the course.", 0, @"", "96644CEF-4FC7-4986-B591-D6675AA38C2C" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Course Detail Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0DCE130-0C91-4AA0-8161-57E8FA523392", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Course Detail Template", "CourseListTemplate", "Course Detail Template", @"The lava template showing the course detail. Merge fields include: Course, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
//- Variable Assignments
{% assign requirementTypes = Course.CourseRequirements | Distinct:'RequirementType' %}
{% assign prerequisitesText = Course.CourseRequirements | Where:'RequirementType','Prerequisite' | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
{% assign facilitatorCount = Course.Facilitators | Size %}
{% assign facilitators = Course.Facilitators | Join:', ' | ReplaceLast:',',' and' | Default:'TBD' %}
{% assign imageFileNameLength = Course.ImageFileGuid | Size %}

//- Styles
{% stylesheet %}
    .page-container {
        display: flex;
        flex-direction: column;
        margin-bottom: 12px;
    }
    
    .page-header-section {
        {% if imageFileNameLength > 0 %}
            height: 280px;
            background-image: url('/GetImage.ashx?guid={{Course.ImageFileGuid}}'); 
            background-size: cover;
        {% endif %}
        align-items: center; 
        border-radius: 12px; 
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        {% if imageFileNameLength > 0 %}
            bottom: -85%;
            -webkit-transform: translateY(-30%);
            transform: translateY(-30%);
        {% endif %}
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        padding-bottom: 12px;
        margin-bottom: 12px;
    }
    
    .page-main-content {
        margin-top: 30px;   
    }
    
    .course-detail-container {
        background-color: white; 
        border-radius: 12px;
        padding: 12px;
        display: flex;
        flex-direction: column;
    }
    
    .course-status-sidebar-container {
        padding: 12px; 
        margin-left: 12px;
        background-color: white; 
        border-radius: 12px;
        width: 300px;
    }
{% endstylesheet %}
<div class=""page-container"">
	<div class=""page-header-section mb-5"">
		<div class=""header-block text-center"">
			<h2>
				{{ Course.Entity.PublicName }}
			</h2>
			<div class=""page-sub-header"">
				{{ Course.Entity.Summary }}
			</div>
		</div>
	</div>
	
	<div class=""page-main-content d-flex"">
		<div class=""course-detail-container text-muted"">
			<div class=""description-header h4"">Course Description</div>
			
			<div class=""course-item-pair-container course-code"">
				<span class=""text-bold"">Course Code: </span>
				<span>{{Course.Entity.CourseCode}}</span>
			</div>
			
			<div class=""course-item-pair-container credits"">
				<span class=""text-bold"">Credits: </span>
				<span>{{Course.Entity.Credits}}</span>
			</div>
			
			<div class=""course-item-pair-container prerequisites"">
				<span class=""text-bold"">Prerequisites: </span>
				
				<span>
					{{ prerequisitesText }}
				</span>
			</div>
			
			<div class=""course-item-pair-container description"">
				<span>{{Course.DescriptionAsHtml}}</span>
			</div>
		</div>
		
		
		<div class=""course-side-panel d-flex flex-column"">
			<div class=""course-status-sidebar-container"">
				
			{% case Course.LearningCompletionStatus %}
			{% when 'Incomplete' %} 
				<div class=""sidebar-header text-bold"">Currently Enrolled</div>
				<div class=""sidebar-value text-muted"">You are currently enrolled in this course.</div>
					
				<div class=""side-bar-action mt-3"">
					<a class=""btn btn-info"" href=""{{ Course.ClassWorkspaceLink }}"">View Course</a>
				</div>
				
                {% for requirementType in requirementTypes %}
                	{% assign requirementsText = Course.CourseRequirements | Where:'RequirementType',requirementType | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                				<div class=""sidebar-header text-bold"">{{ requirementType | Pluralize }}</div>
                				<div class=""sidebar-value text-muted"">{{ requirementsText }}</div>
                {% endfor %}
			{% when 'Passed' %} 
				<div class=""sidebar-header text-bold"">History</div>
				<div class=""sidebar-value text-muted"">You completed this class on {{ Course.MostRecentParticipation.LearningCompletionDateTime | Date: 'MMMM dd, yyyy' }}</div>
				
				<div class=""side-bar-action mt-3"">
					<a href=""{{ Course.ClassWorkspaceLink }}"">View Class Work</a>
				</div>
				
                {% for requirementType in requirementTypes %}
                	{% assign requirementsText = Course.CourseRequirements | Where:'RequirementType',requirementType | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                				<div class=""sidebar-header text-bold"">{{ requirementType | Pluralize }}</div>
                				<div class=""sidebar-value text-muted"">{{ requirementsText }}</div>
                {% endfor %}
			{% else %} 
                {% for requirementType in requirementTypes %}
                	{% assign requirementsText = Course.CourseRequirements | Where:'RequirementType',requirementType | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                				<div class=""sidebar-header text-bold"">{{ requirementType | Pluralize }}</div>
                				<div class=""sidebar-value text-muted"">{{ requirementsText }}</div>
                {% endfor %}
				
				<div class=""sidebar-upcoming-schedule h4"">Upcoming Schedule</div>
				
				<div class=""side-bar-header-value-pair text-muted"">
					<div class=""sidebar-header text-bold"">Next Session Semester: </div>
					<div class=""sidebar-value"">{{ Course.NextSemester.Name }}</div>
				</div>
				
				<div class=""side-bar-header-value-pair text-muted"">
					<div class=""sidebar-header text-bold"">{{ 'Instructor' | PluralizeForQuantity:facilitatorCount }}:</div>
					<div class=""sidebar-value"">{{ facilitators }}</div>
				</div>
				<div class=""side-bar-action mt-3"">
					<a class=""btn btn-info"" href=""{{ Course.CourseEnrollmentLink }}"">Enroll</a>
				</div>
			{% endcase %}
			</div>
		</div>
	</div>
</div>", "0935CE10-BD61-4C85-9D6C-0269DEC2EEAC" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Attribute: Facilitator Portal Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "55F2E89B-DE57-4E24-AC6C-576956FB97C5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Facilitator Portal Page", "FacilitatorPortalPage", "Facilitator Portal Page", @"The page that will be navigated to when clicking facilitator portal link.", 1, @"", "72DFE773-DA2F-45A8-976A-6C19FD0AFE28" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Attribute: Header Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "55F2E89B-DE57-4E24-AC6C-576956FB97C5", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Header Template", "HeaderTemplate", "Header Template", @"The lava template showing the program list. Merge fields include: Course, Activities, Facilitators and other Common Merge Fields. <span class='tip tip-lava'></span>", 2, @"
{% assign imageFileNameLength = Course.ImageFileGuid | Size %}

//- Styles
{% stylesheet %}
    .header-container {
        display: flex;
        flex-direction: column;
        margin-bottom: 12px;
    }
    
    .page-header-section {
        {% if imageFileNameLength > 0 %}
            height: 280px;
            background-image: url('/GetImage.ashx?guid={{Course.ImageFileGuid}}'); 
            background-size: cover;
        {% endif %}
        align-items: center; 
        border-radius: 12px; 
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        {% if imageFileNameLength > 0 %}
            bottom: -85%;
            -webkit-transform: translateY(-30%);
            transform: translateY(-30%);
        {% endif %}
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        padding-bottom: 12px;
        margin-bottom: 12px;
    }
{% endstylesheet %}
<div class=""header-container"">
	<div class=""page-header-section mb-5"">
		<div class=""header-block text-center"">
			<h2>
				{{ Course.PublicName }}
			</h2>
			<div class=""page-sub-header"">
				{{ Course.Summary }}
			</div>
		</div>
	</div>
</div>
", "F7F1BE33-AF44-4F73-9C55-0F8FF97E1B69" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Attribute: The Number of Notifications to Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "55F2E89B-DE57-4E24-AC6C-576956FB97C5", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "The Number of Notifications to Show", "NumberOfNotificationsToShow", "The Number of Notifications to Show", @"The number of notifications to show on the class overview page", 3, @"3", "3B8378C9-80DD-4A50-9197-CA9E1EC88507" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Attribute: Show Grades
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "55F2E89B-DE57-4E24-AC6C-576956FB97C5", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Show Grades", "ShowGrades", "Show Grades", @"Select 'Show' to show grades on the class overview page; 'Hide' to not show any grades.", 4, @"Show", "6FE66C3C-E37B-440D-942C-88C008E844F5" );

            // Add Block Attribute Value
            //   Block: Learning Program List
            //   BlockType: Learning Program List
            //   Category: LMS
            //   Block Location: Page=Learning, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 7888caf4-af5d-44ba-ab9e-80138361f69d */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "B7D8BC2F-CEDD-4E22-8D46-69A759DCB59C", "CE5E2633-0E73-4A30-AB96-DE6A0BB40AD6", @"7888caf4-af5d-44ba-ab9e-80138361f69d" );

            // Add Block Attribute Value
            //   Block: Learning Program List
            //   BlockType: Learning Program List
            //   Category: LMS
            //   Block Location: Page=Learning, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "B7D8BC2F-CEDD-4E22-8D46-69A759DCB59C", "71BC04BF-E3D4-41C6-96EA-25B69CE7C466", @"False" );

            // Add Block Attribute Value
            //   Block: Learning Program List
            //   BlockType: Learning Program List
            //   Category: LMS
            //   Block Location: Page=Learning, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "B7D8BC2F-CEDD-4E22-8D46-69A759DCB59C", "F676DCD1-F5DA-440A-80C9-29930A0B7E21", @"True" );

            // Add Block Attribute Value
            //   Block: Program
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Display Mode
            /*   Attribute Value: Full */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D4ED6CD7-C28E-4ABB-90B3-39499BE41E1B", "18731EB1-888B-4AB2-B1C1-759493B2E639", @"Full" );

            // Add Block Attribute Value
            //   Block: Program
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Courses Page
            /*   Attribute Value: 870318d8-5381-4b3c-be4a-04e57125b590 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D4ED6CD7-C28E-4ABB-90B3-39499BE41E1B", "8C648BE3-357F-42D1-9402-500A290F9FD5", @"870318d8-5381-4b3c-be4a-04e57125b590" );

            // Add Block Attribute Value
            //   Block: Program
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Completions Page
            /*   Attribute Value: 532bc5a9-40b3-42af-9ad3-740fc0b3eb41 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D4ED6CD7-C28E-4ABB-90B3-39499BE41E1B", "94676A26-01A8-4D53-B382-70DDF17AEEDA", @"532bc5a9-40b3-42af-9ad3-740fc0b3eb41" );

            // Add Block Attribute Value
            //   Block: Program
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Semesters Page
            /*   Attribute Value: f7073393-d3a7-4c2e-8001-a73f9e169d60 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D4ED6CD7-C28E-4ABB-90B3-39499BE41E1B", "80BFAC80-B689-4B66-898E-69DA756DE093", @"f7073393-d3a7-4c2e-8001-a73f9e169d60" );

            // Add Block Attribute Value
            //   Block: Program
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Show KPIs
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D4ED6CD7-C28E-4ABB-90B3-39499BE41E1B", "6AE52A7E-EFA3-4685-B331-A2D3058438D3", @"False" );

            // Add Block Attribute Value
            //   Block: Learning Grading System List
            //   BlockType: Learning Grading System List
            //   Category: LMS
            //   Block Location: Page=Grading Systems, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 6163ccfd-cb15-452e-99f2-229a5e5b22f0 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "AF6C3E7F-C7EB-40ED-BCE2-038FABC6A868", "877C67A0-91A2-45C2-94AC-D2FB4951A0C8", @"6163ccfd-cb15-452e-99f2-229a5e5b22f0" );

            // Add Block Attribute Value
            //   Block: Learning Grading System List
            //   BlockType: Learning Grading System List
            //   Category: LMS
            //   Block Location: Page=Grading Systems, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "AF6C3E7F-C7EB-40ED-BCE2-038FABC6A868", "B80C9816-EA60-4EB2-9920-DCE896A6FCC8", @"False" );

            // Add Block Attribute Value
            //   Block: Learning Grading System List
            //   BlockType: Learning Grading System List
            //   Category: LMS
            //   Block Location: Page=Grading Systems, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "AF6C3E7F-C7EB-40ED-BCE2-038FABC6A868", "F07A92F7-2B8B-4ADA-AE64-C8E3D7734A65", @"True" );

            // Add Block Attribute Value
            //   Block: Learning Course List
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: a57d990e-6f34-45cf-abaa-08c40e8d4844,85f70ecd-9425-4ba5-8a60-c2f6891b1265 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "60979468-1F45-467B-A600-B1A230BF6CB9", "67E7F552-C0F2-4852-B817-E216795D1E30", @"a57d990e-6f34-45cf-abaa-08c40e8d4844,85f70ecd-9425-4ba5-8a60-c2f6891b1265" );

            // Add Block Attribute Value
            //   Block: Learning Course List
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "60979468-1F45-467B-A600-B1A230BF6CB9", "148C0E92-ACA0-45DE-84EB-C8CB76D57747", @"False" );

            // Add Block Attribute Value
            //   Block: Learning Course List
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "60979468-1F45-467B-A600-B1A230BF6CB9", "223232AD-21C4-4024-ADE2-B9180B165728", @"True" );

            // Add Block Attribute Value
            //   Block: Learning Course Detail
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Block Location: Page=Course, Site=Rock RMS
            //   Attribute: Activity Detail Page
            /*   Attribute Value: d72dcbc4-c57f-4028-b503-1954925edc7d */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D85084D3-E298-4307-9AA2-C1570C4A3FA4", "30C34DC5-08B4-4826-9EA1-5008B0864805", @"d72dcbc4-c57f-4028-b503-1954925edc7d" );

            // Add Block Attribute Value
            //   Block: Learning Course Detail
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Block Location: Page=Course, Site=Rock RMS
            //   Attribute: Participant Detail Page
            /*   Attribute Value: 72c75c91-18f8-48d0-b0cf-1fbd82eb50fc,827af9a8-bf1a-4008-b4c3-3d07076acb84 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D85084D3-E298-4307-9AA2-C1570C4A3FA4", "75CF60D8-19F7-45DB-BB6C-CEF859F2327D", @"72c75c91-18f8-48d0-b0cf-1fbd82eb50fc,827af9a8-bf1a-4008-b4c3-3d07076acb84" );

            // Add Block Attribute Value
            //   Block: Grading Scales
            //   BlockType: Learning Grading System Scale List
            //   Category: LMS
            //   Block Location: Page=Grading System, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: ae85b3fc-c951-497f-8c43-9d0a1e467a50,4a7b8b2e-9820-4932-b849-35624a27e47b */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "FD56EBFE-8BC5-42B7-8847-941B58AD14B6", "B930784E-C501-482C-8007-C9C3380BFDF4", @"ae85b3fc-c951-497f-8c43-9d0a1e467a50,4a7b8b2e-9820-4932-b849-35624a27e47b" );

            // Add Block Attribute Value
            //   Block: Grading Scales
            //   BlockType: Learning Grading System Scale List
            //   Category: LMS
            //   Block Location: Page=Grading System, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "FD56EBFE-8BC5-42B7-8847-941B58AD14B6", "27B7F5D0-39FF-4913-8525-E9F571F8C1BB", @"False" );

            // Add Block Attribute Value
            //   Block: Grading Scales
            //   BlockType: Learning Grading System Scale List
            //   Category: LMS
            //   Block Location: Page=Grading System, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "FD56EBFE-8BC5-42B7-8847-941B58AD14B6", "AAB4897B-5FED-4093-BAC0-C2CA043CC285", @"True" );

            // Add Block Attribute Value
            //   Block: Activity List
            //   BlockType: Learning Activity Completion List
            //   Category: LMS
            //   Block Location: Page=Activity, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: e0f2e4f1-ed10-49f6-b053-ac6807994204,8c40ae8d-60c6-49de-b7de-be46d8a64aa6 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "7CE84856-A789-4D74-AB6C-DA05A5C82479", "7733ED7C-A8C4-4501-8BD1-6B66F54DF13B", @"e0f2e4f1-ed10-49f6-b053-ac6807994204,8c40ae8d-60c6-49de-b7de-be46d8a64aa6" );

            // Add Block Attribute Value
            //   Block: Activity List
            //   BlockType: Learning Activity Completion List
            //   Category: LMS
            //   Block Location: Page=Activity, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "7CE84856-A789-4D74-AB6C-DA05A5C82479", "F353856F-95FA-4708-9B2C-8E99C09B4399", @"False" );

            // Add Block Attribute Value
            //   Block: Activity List
            //   BlockType: Learning Activity Completion List
            //   Category: LMS
            //   Block Location: Page=Activity, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "7CE84856-A789-4D74-AB6C-DA05A5C82479", "2DB21AA3-F122-4D3A-97A5-3BD3F0020D13", @"True" );

            // Add Block Attribute Value
            //   Block: Semesters
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 36ffa805-b283-443e-990d-87040339d16f,bfbdea91-9519-4946-9836-073efe02551f */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "E58406FD-E17B-4480-95EB-ADCFD956CA17", "EEA24DC4-4CFC-4B34-9348-917839DDBBA2", @"36ffa805-b283-443e-990d-87040339d16f,bfbdea91-9519-4946-9836-073efe02551f" );

            // Add Block Attribute Value
            //   Block: Semesters
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "E58406FD-E17B-4480-95EB-ADCFD956CA17", "A0627D65-2057-42C0-8730-65C87F05F86A", @"False" );

            // Add Block Attribute Value
            //   Block: Semesters
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "E58406FD-E17B-4480-95EB-ADCFD956CA17", "57106CC3-5D67-40BA-809B-2EA9826478D8", @"True" );

            // Add Block Attribute Value
            //   Block: Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Course, Site=Rock RMS
            //   Attribute: Show Schedule Column
            /*   Attribute Value: Yes */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0", "BECBD2AE-5D1E-4065-8CAA-FE7972CC5742", @"Yes" );

            // Add Block Attribute Value
            //   Block: Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Course, Site=Rock RMS
            //   Attribute: Show Semester Column
            /*   Attribute Value: Yes */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0", "28087DBE-A334-47BF-9CA5-76AFF5EE1A6F", @"Yes" );

            // Add Block Attribute Value
            //   Block: Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Course, Site=Rock RMS
            //   Attribute: Display Mode
            /*   Attribute Value: Always */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0", "DA69DB3C-139E-4DC6-BBA5-DC03C0ACBD6A", @"Always" );

            // Add Block Attribute Value
            //   Block: Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Course, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 23d5076e-c062-4987-9985-b3a4792bf3ce,5fce29a7-4530-4cce-9891-c95242923efe */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0", "0C995F98-F483-4814-B3A1-6FACCD2D686F", @"23d5076e-c062-4987-9985-b3a4792bf3ce,5fce29a7-4530-4cce-9891-c95242923efe" );

            // Add Block Attribute Value
            //   Block: Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Course, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0", "8D30B478-C616-40A3-A159-7320D22390B6", @"False" );

            // Add Block Attribute Value
            //   Block: Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Course, Site=Rock RMS
            //   Attribute: Show Location Column
            /*   Attribute Value: Yes */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0", "4FBE92ED-3CF1-4BD4-A78E-B50C0080FA0B", @"Yes" );

            // Add Block Attribute Value
            //   Block: Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Course, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0", "4778450C-5B2A-40AD-9FC0-B3ED8AAE2F5C", @"True" );

            // Add Block Attribute Value
            //   Block: Class
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Block Location: Page=Class, Site=Rock RMS
            //   Attribute: Activity Detail Page
            /*   Attribute Value: d72dcbc4-c57f-4028-b503-1954925edc7d */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "C67D2164-33E5-46C0-94EF-DF387EF8477D", "44565F75-B4CA-4438-A9B6-BEB943813559", @"d72dcbc4-c57f-4028-b503-1954925edc7d" );

            // Add Block Attribute Value
            //   Block: Class
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Block Location: Page=Class, Site=Rock RMS
            //   Attribute: Participant Detail Page
            /*   Attribute Value: 72c75c91-18f8-48d0-b0cf-1fbd82eb50fc,827af9a8-bf1a-4008-b4c3-3d07076acb84 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "C67D2164-33E5-46C0-94EF-DF387EF8477D", "1B237D13-A86A-42CC-8E91-96CBCCAC6866", @"72c75c91-18f8-48d0-b0cf-1fbd82eb50fc,827af9a8-bf1a-4008-b4c3-3d07076acb84" );

            // Add Block Attribute Value
            //   Block: Class
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Block Location: Page=Class, Site=Rock RMS
            //   Attribute: Attendance Page
            /*   Attribute Value: 78b79290-3234-4d8c-96d3-1901901ba1dd */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "C67D2164-33E5-46C0-94EF-DF387EF8477D", "B417E2A7-BBA1-453F-9933-3BE439CD2063", @"78b79290-3234-4d8c-96d3-1901901ba1dd" );

            // Add Block Attribute Value
            //   Block: Class
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Block Location: Page=Class, Site=Rock RMS
            //   Attribute: Content Page Detail Page
            /*   Attribute Value: e6a89360-b7b4-48a8-b799-39a27eab6f36 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "C67D2164-33E5-46C0-94EF-DF387EF8477D", "AAF9675D-BAF0-4F25-8C63-4F864F4369C0", @"e6a89360-b7b4-48a8-b799-39a27eab6f36" );

            // Add Block Attribute Value
            //   Block: Class
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Block Location: Page=Class, Site=Rock RMS
            //   Attribute: Announcement Detail Page
            /*   Attribute Value: 7c8134a4-524a-4c3d-ba4c-875fee672850 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "C67D2164-33E5-46C0-94EF-DF387EF8477D", "B570F05B-95DE-4F22-95FF-DDDCCE943E29", @"7c8134a4-524a-4c3d-ba4c-875fee672850" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "53A87507-D270-4340-A93F-AD3AB023E0B1", "8D30B478-C616-40A3-A159-7320D22390B6", @"False" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "53A87507-D270-4340-A93F-AD3AB023E0B1", "4778450C-5B2A-40AD-9FC0-B3ED8AAE2F5C", @"True" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 23d5076e-c062-4987-9985-b3a4792bf3ce,5fce29a7-4530-4cce-9891-c95242923efe */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "53A87507-D270-4340-A93F-AD3AB023E0B1", "0C995F98-F483-4814-B3A1-6FACCD2D686F", @"23d5076e-c062-4987-9985-b3a4792bf3ce,5fce29a7-4530-4cce-9891-c95242923efe" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Show Semester Column
            /*   Attribute Value: Yes */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "53A87507-D270-4340-A93F-AD3AB023E0B1", "28087DBE-A334-47BF-9CA5-76AFF5EE1A6F", @"Yes" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Display Mode
            /*   Attribute Value: Always */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "53A87507-D270-4340-A93F-AD3AB023E0B1", "DA69DB3C-139E-4DC6-BBA5-DC03C0ACBD6A", @"Always" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Show Location Column
            /*   Attribute Value: No */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "53A87507-D270-4340-A93F-AD3AB023E0B1", "4FBE92ED-3CF1-4BD4-A78E-B50C0080FA0B", @"No" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Show Schedule Column
            /*   Attribute Value: No */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "53A87507-D270-4340-A93F-AD3AB023E0B1", "BECBD2AE-5D1E-4065-8CAA-FE7972CC5742", @"No" );
        }

        /// <summary>
        /// Results of CodeGen_PagesBlocksAttributesMigration.sql for LMS (Down).
        /// </summary>
        private void LmsEntityTypesPagesBlocksDown()
        {
            // Attribute for BlockType
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Attribute: Show Grades
            RockMigrationHelper.DeleteAttribute( "6FE66C3C-E37B-440D-942C-88C008E844F5" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Attribute: The Number of Notifications to Show
            RockMigrationHelper.DeleteAttribute( "3B8378C9-80DD-4A50-9197-CA9E1EC88507" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Attribute: Header Template
            RockMigrationHelper.DeleteAttribute( "F7F1BE33-AF44-4F73-9C55-0F8FF97E1B69" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Course Detail Template
            RockMigrationHelper.DeleteAttribute( "0935CE10-BD61-4C85-9D6C-0269DEC2EEAC" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Enrollment Page
            RockMigrationHelper.DeleteAttribute( "96644CEF-4FC7-4986-B591-D6675AA38C2C" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Class Workspace Page
            RockMigrationHelper.DeleteAttribute( "8787C9F3-1CF9-4790-B65E-90303F446536" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Enrollment Page
            RockMigrationHelper.DeleteAttribute( "FF294CFA-6D54-4CBC-B8EF-D45893677D58" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Course List Template
            RockMigrationHelper.DeleteAttribute( "BB215F63-EF8A-4196-A4F1-A4A822B4BD60" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "5A8A251D-56B3-4E3B-8BF9-1D333C016B74" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Program List Template
            RockMigrationHelper.DeleteAttribute( "63F97CF2-774C-480C-9933-A2BAA664DCE2" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "0317E0AD-9FE9-409B-9A14-C3D30D303B23" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Show KPIs
            RockMigrationHelper.DeleteAttribute( "6AE52A7E-EFA3-4685-B331-A2D3058438D3" );

            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Participant Detail Page
            RockMigrationHelper.DeleteAttribute( "C247F6BF-C7D1-428E-B42F-65D1F8610CED" );

            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Activity Detail Page
            RockMigrationHelper.DeleteAttribute( "44565F75-B4CA-4438-A9B6-BEB943813559" );

            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Attendance Page
            RockMigrationHelper.DeleteAttribute( "B417E2A7-BBA1-453F-9933-3BE439CD2063" );

            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Announcement Detail Page
            RockMigrationHelper.DeleteAttribute( "B570F05B-95DE-4F22-95FF-DDDCCE943E29" );

            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Content Page Detail Page
            RockMigrationHelper.DeleteAttribute( "AAF9675D-BAF0-4F25-8C63-4F864F4369C0" );

            // Attribute for BlockType
            //   BlockType: Learning Activity Completion List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "2DB21AA3-F122-4D3A-97A5-3BD3F0020D13" );

            // Attribute for BlockType
            //   BlockType: Learning Activity Completion List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "BABBE946-AB0E-405B-9787-5A09D9D055DF" );

            // Attribute for BlockType
            //   BlockType: Learning Activity Completion List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "7733ED7C-A8C4-4501-8BD1-6B66F54DF13B" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System Scale List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "AAB4897B-5FED-4093-BAC0-C2CA043CC285" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System Scale List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "9EF3D35C-C7C0-4AF2-8F1C-4682FDB79FDB" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System Scale List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "B930784E-C501-482C-8007-C9C3380BFDF4" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "F07A92F7-2B8B-4ADA-AE64-C8E3D7734A65" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "56A23C5C-8BE5-419A-9945-9044CBB514E9" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "877C67A0-91A2-45C2-94AC-D2FB4951A0C8" );

            // Attribute for BlockType
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "57106CC3-5D67-40BA-809B-2EA9826478D8" );

            // Attribute for BlockType
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "FCA17D14-C775-4651-A41F-C5EF673B446F" );

            // Attribute for BlockType
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "EEA24DC4-4CFC-4B34-9348-917839DDBBA2" );

            // Attribute for BlockType
            //   BlockType: Learning Program List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "F676DCD1-F5DA-440A-80C9-29930A0B7E21" );

            // Attribute for BlockType
            //   BlockType: Learning Program List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "B870E6CC-2952-400C-ADAA-B51BF3147E6D" );

            // Attribute for BlockType
            //   BlockType: Learning Program List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "CE5E2633-0E73-4A30-AB96-DE6A0BB40AD6" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Semesters Page
            RockMigrationHelper.DeleteAttribute( "80BFAC80-B689-4B66-898E-69DA756DE093" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Completions Page
            RockMigrationHelper.DeleteAttribute( "94676A26-01A8-4D53-B382-70DDF17AEEDA" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Courses Page
            RockMigrationHelper.DeleteAttribute( "8C648BE3-357F-42D1-9402-500A290F9FD5" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Display Mode
            RockMigrationHelper.DeleteAttribute( "18731EB1-888B-4AB2-B1C1-759493B2E639" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Category
            RockMigrationHelper.DeleteAttribute( "BEABF6B9-F85E-44DD-9B70-D49209AD84A8" );

            // Attribute for BlockType
            //   BlockType: Learning Program Completion List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "C7138043-593C-4807-A0F1-E31B1A6F297F" );

            // Attribute for BlockType
            //   BlockType: Learning Program Completion List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "C16D7666-6155-4D64-B515-BEB96C934E4C" );

            // Attribute for BlockType
            //   BlockType: Learning Program Completion List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "206A8316-1203-4661-A9E7-A4032C930075" );

            // Attribute for BlockType
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "223232AD-21C4-4024-ADE2-B9180B165728" );

            // Attribute for BlockType
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "16763698-7847-43BF-B2F2-68FBB4408FF8" );

            // Attribute for BlockType
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "67E7F552-C0F2-4852-B817-E216795D1E30" );

            // Attribute for BlockType
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Attribute: Announcement Detail Page
            RockMigrationHelper.DeleteAttribute( "45F3C2D4-8C84-4CE2-BAB3-ACF93DC8E940" );

            // Attribute for BlockType
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Attribute: Content Page Detail Page
            RockMigrationHelper.DeleteAttribute( "8FFDFA67-E20E-4BEC-91D3-7DDF7975291C" );

            // Attribute for BlockType
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Attribute: Participant Detail Page
            RockMigrationHelper.DeleteAttribute( "75CF60D8-19F7-45DB-BB6C-CEF859F2327D" );

            // Attribute for BlockType
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Attribute: Activity Detail Page
            RockMigrationHelper.DeleteAttribute( "30C34DC5-08B4-4826-9EA1-5008B0864805" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "4778450C-5B2A-40AD-9FC0-B3ED8AAE2F5C" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "FFEF4113-FBD9-49B6-BCBD-4844CFF5FA3B" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "0C995F98-F483-4814-B3A1-6FACCD2D686F" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: Display Mode
            RockMigrationHelper.DeleteAttribute( "DA69DB3C-139E-4DC6-BBA5-DC03C0ACBD6A" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: Show Semester Column
            RockMigrationHelper.DeleteAttribute( "28087DBE-A334-47BF-9CA5-76AFF5EE1A6F" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: Show Schedule Column
            RockMigrationHelper.DeleteAttribute( "BECBD2AE-5D1E-4065-8CAA-FE7972CC5742" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: Show Location Column
            RockMigrationHelper.DeleteAttribute( "4FBE92ED-3CF1-4BD4-A78E-B50C0080FA0B" );

            // Attribute for BlockType
            //   BlockType: Learning Activity List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "8783E0C3-6B14-4252-A912-A13C4D1E89B0" );

            // Attribute for BlockType
            //   BlockType: Learning Activity List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "B4130B6F-E3DE-4161-A28D-B7F6F160CB38" );

            // Remove Block
            //  Name: Current Classes, from Page: Program, Site: Rock RMS
            //  from Page: Program, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "53A87507-D270-4340-A93F-AD3AB023E0B1" );

            // Remove Block
            //  Name: Activity Completion Detail, from Page: Completion, Site: Rock RMS
            //  from Page: Completion, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "43D3D01A-101E-494E-AF4D-BBEE9595DE0A" );

            // Remove Block
            //  Name: Class, from Page: Class, Site: Rock RMS
            //  from Page: Class, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "C67D2164-33E5-46C0-94EF-DF387EF8477D" );

            // Remove Block
            //  Name: Classes, from Page: Course, Site: Rock RMS
            //  from Page: Course, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0" );

            // Remove Block
            //  Name: Learning Activity Detail, from Page: Activity, Site: Rock RMS
            //  from Page: Activity, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "8BDFF6BF-E043-4EB5-BDD9-C1813AE83295" );

            // Remove Block
            //  Name: Program Completions, from Page: Completions, Site: Rock RMS
            //  from Page: Completions, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "ADCFF541-8908-4F78-9B96-2C3E37D5C4AB" );

            // Remove Block
            //  Name: Semester, from Page: Semester, Site: Rock RMS
            //  from Page: Semester, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "2DF1C8E3-B65E-47F0-8B82-86AA5A3DA21A" );

            // Remove Block
            //  Name: Participant, from Page: Participant, Site: Rock RMS
            //  from Page: Participant, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "8C05CAC8-AB57-46F2-8E0C-21293BE8F464" );

            // Remove Block
            //  Name: Semesters, from Page: Semesters, Site: Rock RMS
            //  from Page: Semesters, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "E58406FD-E17B-4480-95EB-ADCFD956CA17" );

            // Remove Block
            //  Name: Activity List, from Page: Activity, Site: Rock RMS
            //  from Page: Activity, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "7CE84856-A789-4D74-AB6C-DA05A5C82479" );

            // Remove Block
            //  Name: Grading Scale, from Page: Grading Scale, Site: Rock RMS
            //  from Page: Grading Scale, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "5FBFF087-3366-4620-8C5C-6A12F8BC6BD2" );

            // Remove Block
            //  Name: Grading Scales, from Page: Grading System, Site: Rock RMS
            //  from Page: Grading System, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "FD56EBFE-8BC5-42B7-8847-941B58AD14B6" );

            // Remove Block
            //  Name: Learning Grading System Detail, from Page: Grading System, Site: Rock RMS
            //  from Page: Grading System, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "88233998-6298-40F8-B433-931486D30B2D" );

            // Remove Block
            //  Name: Learning Course Detail, from Page: Course, Site: Rock RMS
            //  from Page: Course, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "D85084D3-E298-4307-9AA2-C1570C4A3FA4" );

            // Remove Block
            //  Name: Learning Course List, from Page: Courses, Site: Rock RMS
            //  from Page: Courses, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "60979468-1F45-467B-A600-B1A230BF6CB9" );

            // Remove Block
            //  Name: Learning Grading System List, from Page: Grading Systems, Site: Rock RMS
            //  from Page: Grading Systems, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "AF6C3E7F-C7EB-40ED-BCE2-038FABC6A868" );

            // Remove Block
            //  Name: Program, from Page: Program, Site: Rock RMS
            //  from Page: Program, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "D4ED6CD7-C28E-4ABB-90B3-39499BE41E1B" );

            // Remove Block
            //  Name: Learning Program List, from Page: Learning, Site: Rock RMS
            //  from Page: Learning, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B7D8BC2F-CEDD-4E22-8D46-69A759DCB59C" );

            // Remove Block
            //  Name: Class Announcement Detail, from Page: Announcement, Site: Rock RMS
            //  from Page: Announcement, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3EAEE2BB-E223-4C47-9C9D-230A412D6D2A" );

            // Remove Block
            //  Name: Content Page, from Page: Content Page, Site: Rock RMS
            //  from Page: Content Page, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "9788FDD8-A587-48CA-BE0B-D5A937CCEE70" );

            // Delete BlockType 
            //   Name: Public Learning Class Workspace
            //   Category: LMS
            //   Path: -
            //   EntityType: Public Learning Class Workspace
            RockMigrationHelper.DeleteBlockType( "55F2E89B-DE57-4E24-AC6C-576956FB97C5" );

            // Delete BlockType 
            //   Name: Public Learning Course Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Public Learning Course Detail
            RockMigrationHelper.DeleteBlockType( "B0DCE130-0C91-4AA0-8161-57E8FA523392" );

            // Delete BlockType 
            //   Name: Public Learning Course List
            //   Category: LMS
            //   Path: -
            //   EntityType: Public Learning Course List
            RockMigrationHelper.DeleteBlockType( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D" );

            // Delete BlockType 
            //   Name: Public Learning Program List
            //   Category: LMS
            //   Path: -
            //   EntityType: Public Learning Program List
            RockMigrationHelper.DeleteBlockType( "DA1460D8-E895-4B23-8A8E-10EBBED3990F" );

            // Delete BlockType 
            //   Name: Learning Activity Completion Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Activity Completion Detail
            RockMigrationHelper.DeleteBlockType( "4569F28D-1EFB-4B95-A506-0D9043C24775" );

            // Delete BlockType 
            //   Name: Learning Class Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Class Detail
            RockMigrationHelper.DeleteBlockType( "D5369F8D-11AA-482B-AE08-2B3C519D8D87" );

            // Delete BlockType 
            //   Name: Learning Activity Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Activity Detail
            RockMigrationHelper.DeleteBlockType( "4B18BF0D-D91B-4934-AC2D-A7188B15B893" );

            // Delete BlockType 
            //   Name: Learning Semester Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Semester Detail
            RockMigrationHelper.DeleteBlockType( "97B2E57F-3A03-490D-834F-CD3640C7FF1E" );

            // Delete BlockType 
            //   Name: Learning Activity Completion List
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Activity Completion List
            RockMigrationHelper.DeleteBlockType( "EF1A5CDD-6769-4FFC-B826-55C194B01897" );

            // Delete BlockType 
            //   Name: Learning Grading System Scale Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Grading System Scale Detail
            RockMigrationHelper.DeleteBlockType( "34D7A280-8D4D-4FE5-BB45-810732F76341" );

            // Delete BlockType 
            //   Name: Learning Grading System Scale List
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Grading System Scale List
            RockMigrationHelper.DeleteBlockType( "27390ED3-57B2-42EF-A212-F8B29851F9BA" );

            // Delete BlockType 
            //   Name: Learning Grading System Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Grading System Detail
            RockMigrationHelper.DeleteBlockType( "A4182806-95B0-49AE-97B5-246A834156E3" );

            // Delete BlockType 
            //   Name: Learning Grading System List
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Grading System List
            RockMigrationHelper.DeleteBlockType( "F003DAAC-B9FE-4007-B218-983084E1126B" );

            // Delete BlockType 
            //   Name: Learning Semester List
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Semester List
            RockMigrationHelper.DeleteBlockType( "C89C7F15-FB8A-43D4-9AFB-5E40E397F246" );

            // Delete BlockType 
            //   Name: Learning Program List
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Program List
            RockMigrationHelper.DeleteBlockType( "7B1DB013-A552-455F-A1D0-7B17488D0D5C" );

            // Delete BlockType 
            //   Name: Learning Program Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Program Detail
            RockMigrationHelper.DeleteBlockType( "796C87E7-678F-4A38-8C04-A401A4F7AC21" );

            // Delete BlockType 
            //   Name: Learning Program Completion List
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Program Completion List
            RockMigrationHelper.DeleteBlockType( "CE703EB6-028F-47FC-A09A-AD8F72C12CBC" );

            // Delete BlockType 
            //   Name: Learning Participant Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Participant Detail
            RockMigrationHelper.DeleteBlockType( "F1179439-31A1-4897-AB2E-B991D60455AA" );

            // Delete BlockType 
            //   Name: Learning Course List
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Course List
            RockMigrationHelper.DeleteBlockType( "D0AFDF98-4AFC-4E4F-A6E2-07CA4E7358E8" );

            // Delete BlockType 
            //   Name: Learning Course Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Course Detail
            RockMigrationHelper.DeleteBlockType( "94C4CB0B-5617-4F46-B902-6E6DD4A447B8" );

            // Delete BlockType 
            //   Name: Learning Class List
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Class List
            RockMigrationHelper.DeleteBlockType( "340F6CC1-8C38-4579-9383-A6168680194A" );

            // Delete BlockType 
            //   Name: Learning Activity List
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Activity List
            RockMigrationHelper.DeleteBlockType( "5CEB6EC7-69F5-43B6-A74F-144A57F9B465" );

            // Delete BlockType 
            //   Name: Learning Class Content Page Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Class Content Page Detail
            RockMigrationHelper.DeleteBlockType( "92E0DB96-0FFF-41F2-982D-F750AB23EF5B" );

            // Delete BlockType 
            //   Name: Learning Class Announcement Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Class Announcement Detail
            RockMigrationHelper.DeleteBlockType( "53C12A53-773E-4398-8627-DD44C1421675" );

            // Delete Page 
            //  Internal Name: Completion
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "E0F2E4F1-ED10-49F6-B053-AC6807994204" );

            // Delete Page 
            //  Internal Name: Semester
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "36FFA805-B283-443E-990D-87040339D16F" );

            // Delete Page 
            //  Internal Name: Participant
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "72C75C91-18F8-48D0-B0CF-1FBD82EB50FC" );

            // Delete Page 
            //  Internal Name: Activity
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "D72DCBC4-C57F-4028-B503-1954925EDC7D" );

            // Delete Page 
            //  Internal Name: Class
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "23D5076E-C062-4987-9985-B3A4792BF3CE" );

            // Delete Page 
            //  Internal Name: Grading Scale
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "AE85B3FC-C951-497F-8C43-9D0A1E467A50" );

            // Delete Page 
            //  Internal Name: Course
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "A57D990E-6F34-45CF-ABAA-08C40E8D4844" );

            // Delete Page 
            //  Internal Name: Grading System
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "6163CCFD-CB15-452E-99F2-229A5E5B22F0" );

            // Delete Page 
            //  Internal Name: Grading Systems
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "F76C9FC6-CDE0-4122-8D05-7862D683A3CA" );

            // Delete Page 
            //  Internal Name: Semesters
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "F7073393-D3A7-4C2E-8001-A73F9E169D60" );

            // Delete Page 
            //  Internal Name: Completions
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "532BC5A9-40B3-42AF-9AD3-740FC0B3EB41" );

            // Delete Page 
            //  Internal Name: Courses
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "870318D8-5381-4B3C-BE4A-04E57125B590" );

            // Delete Page 
            //  Internal Name: Program
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "7888CAF4-AF5D-44BA-AB9E-80138361F69D" );

            // Delete Page 
            //  Internal Name: Learning
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "84DBEC51-EE0B-41C2-94B3-F361C4B98879" );

            // Delete Page 
            //  Internal Name: Privacy
            //  Site: External Website
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "964A4B4B-BF7F-4520-8620-05A6DEEAB5C8" );

            // Delete Page 
            //  Internal Name: Terms
            //  Site: External Website
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "DF471E9C-EEFC-4493-B6C0-C8D94BC248EB" );
        }

        #endregion
    }
}
