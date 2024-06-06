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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 199, "1.16.4" )]
    public class MigrationRollupsForV17_0_7: Migration
    {
        // KH: Create Administrative Settings Page/Page Search Block and move various Admin Tool Settings Pages under the new Administrative Settings Page
        private const string AdministrativeSettingsPageGuid = "A7E36E7A-EFBD-4912-B46E-BB61A74B86FF";

        //KA: D: V17 Migration to add Tithing overview metrics
        const string TITHING_OVERVIEW_CATEGORY = "914E7A39-EA2D-469B-95B5-B6518DBE5F52";
        const string TITHING_OVERVIEW_SCHEDULE_GUID = "5E51EA9E-8475-4955-875E-45F44270A462";
        const string TITHING_OVERVIEW_BY_CAMPUS_METRIC_GUID = "F4951A42-9F71-4CB1-A46E-2A7ED84CD923";
        const string TITHING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID = "2B798177-E8F4-46DB-A1D7-308D63CA519A";
        const string GIVING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID = "B5BFAB51-9B46-4E7E-992E-B0119E4D25EC";

        //KA: Create data-migration to add new page with new Tithing Overview block
        const string TITHING_OVERVIEW_PAGE = "72BA5DD9-8685-4182-833D-22BB1E0F9A36";
        const string TITHING_OVERVIEW_BLOCK_ENTITY_TYPE = "1E44B061-7767-487D-A98F-16912E8C7DE7";
        const string TITHING_OVERVIEW_BLOCK_TYPE = "DB756565-8A35-42E2-BC79-8D11F57E4004";
        const string TITHING_OVERVIEW_BLOCK = "E6956ECC-08DC-4EF0-9F9A-67F8BD2F5F91";

        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            CreateAdministrativeSettingsPage();
            MoveSettingsPages();
            TithingOverviewMetricsUp();
            TithingOverviewPageAndBlockUp();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            TithingOverviewMetricsDown();
            TithingOverviewPageAndBlockDown();
        }

        private void CreateAdministrativeSettingsPage()
        {
            // Add Page 
            //  Internal Name: Administrative Settings
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "550A898C-EDEA-48B5-9C58-B20EC13AF13B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Administrative Settings", "", AdministrativeSettingsPageGuid, "" );

            // Add Page Route
            //   Page:Administrative Settings
            //   Route:admin/settings
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPageRoute( AdministrativeSettingsPageGuid, "admin/settings", "A000D38F-D19C-4F99-B498-227E3509A5C7" );
#pragma warning restore CS0618 // Type or member is obsolete

            // Update page order
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 0 WHERE [Guid] = '{AdministrativeSettingsPageGuid}'" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.PageSearch
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.PageSearch", "Page Search", "Rock.Blocks.Cms.PageSearch, Rock.Blocks, Version=1.17.0.18, Culture=neutral, PublicKeyToken=null", false, false, "85BA51A4-41CF-4F60-9EAE-1D8B1E73C736" );

            // Add/Update Obsidian Block Type
            //   Name:Rock.Blocks.Cms.PageSearch
            //   Category:
            //   EntityType:Rock.Blocks.Cms.PageSearch
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Page Search", "Displays a search page to find child pages", "Rock.Blocks.Cms.PageSearch", "CMS", "A279A88E-D4E0-4867-A108-2AA743B3CFD0" );

            // Add Block 
            //  Block Name: Page Search
            //  Page Name: Administrative Settings
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, AdministrativeSettingsPageGuid.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "A279A88E-D4E0-4867-A108-2AA743B3CFD0".AsGuid(), "Page Search", "Main", @"", @"", 0, "2B8A3D46-8E5F-44E2-AC84-2554DCA502EC" );
        }

        private void MoveSettingsPages()
        {
            // General
            var generalSettingsPageGuid = "0B213645-FA4E-44A5-8E4C-B2D8EF054985";

            Sql( $@"UPDATE [Page]
SET InternalName = 'General',
PageTitle = 'General',
BrowserTitle = 'General'
WHERE [Page].[Guid] = '{generalSettingsPageGuid}'" );

            RockMigrationHelper.MovePage( generalSettingsPageGuid, AdministrativeSettingsPageGuid );

            // Security
            RockMigrationHelper.MovePage( "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F", AdministrativeSettingsPageGuid );

            // Communications
            RockMigrationHelper.MovePage( "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", AdministrativeSettingsPageGuid );

            // CMS
            // Move all the pages back to the CMS Configuration Page
            var cmsConfigurationPageGuid = "B4A24AB7-9369-4055-883F-4F4892C39AE3";

            Sql( $@"UPDATE [Page]
SET InternalName = 'CMS',
PageTitle = 'CMS',
BrowserTitle = 'CMS'
WHERE [Guid] = '{cmsConfigurationPageGuid}'" );

            RockMigrationHelper.MovePage( cmsConfigurationPageGuid, AdministrativeSettingsPageGuid );

            Sql( $@"
DECLARE @newParentPageId INT = (SELECT [Id] FROM [Page] WHERE [Guid] = '{cmsConfigurationPageGuid}')

UPDATE [dbo].[Page] SET [ParentPageId] = @newParentPageId WHERE [ParentPageId] IN (
   SELECT [Id] FROM [dbo].[Page] WHERE [Guid] IN (
   'CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89'  -- Website Configuration Section 
   ,'889D7F7F-EB0F-40CD-9E80-E58A00EE69F7' -- Content Channels Section
   ,'B892DF6D-4789-4AC3-9E6C-2BFE0D9E30E4' -- Personalization Section
   ,'04FE297E-D45E-44EC-B521-181423F05A1C' -- Content Platform Section
   ,'82726ACD-3480-4514-A920-FE920A71C046' -- Digital Media Applications Section
   )
)" );

            // Check In
            RockMigrationHelper.MovePage( "66C5DD58-094C-4FF9-9AFB-44801FCFCC2D", AdministrativeSettingsPageGuid );

            // Power Tools
            RockMigrationHelper.MovePage( "7F1F4130-CB98-473B-9DE1-7A886D2283ED", AdministrativeSettingsPageGuid );

            // System Settings
            var systemSettingsPageGuid = "C831428A-6ACD-4D49-9B2D-046D399E3123";

            Sql( $@"UPDATE [dbo].[Page]
SET [InternalName] = 'System',
[PageTitle] = 'System',
[BrowserTitle] = 'System'
WHERE [Page].[Guid] = '{systemSettingsPageGuid}'" );

            RockMigrationHelper.MovePage( systemSettingsPageGuid, AdministrativeSettingsPageGuid );
        }

        /// <summary>
        /// KA: Migration to add Tithing overview metrics
        /// </summary>
        private void TithingOverviewMetricsUp()
        {
            AddSchedule();

            RockMigrationHelper.UpdateCategory( SystemGuid.EntityType.METRICCATEGORY, "Tithing Overview", "icon-fw fa fa-chart-bar", "A few metrics to show high-level tithing statistics.", TITHING_OVERVIEW_CATEGORY );

            AddTithingOverviewByCampusMetrics();
            AddTithingHouseholdsByCampusMetrics();
            AddGivingHouseholdsOverviewByCampusMetrics();
        }

        /// <summary>
        /// KA: Migration to add Tithing overview metrics
        /// </summary>
        private void TithingOverviewMetricsDown()
        {
            DeleteMetric( TITHING_OVERVIEW_BY_CAMPUS_METRIC_GUID );
            DeleteMetric( TITHING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID );
            DeleteMetric( GIVING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID );

            RockMigrationHelper.DeleteCategory( TITHING_OVERVIEW_CATEGORY );

            Sql( $"DELETE FROM Schedule WHERE [Guid] = '{TITHING_OVERVIEW_SCHEDULE_GUID}'" );
        }

        private void AddSchedule()
        {
            Sql( $@"
DECLARE @ScheduleId int = ( 
    SELECT [Id] 
    FROM [dbo].[Schedule] 
    WHERE Guid = '{TITHING_OVERVIEW_SCHEDULE_GUID}'),

@MetricsCategoryId int = ( 
    SELECT [Id] 
    FROM [dbo].[Category] 
    WHERE Guid = '{SystemGuid.Category.SCHEDULE_METRICS}'
)
IF @ScheduleId IS NULL
BEGIN
	INSERT [dbo].[Schedule] ([Name], [Description], [iCalendarContent], [CategoryId], [Guid], [IsActive])
	VALUES (N'Tithing Overview', NULL, N'BEGIN:VCALENDAR
PRODID:-//github.com/SparkDevNetwork/Rock//NONSGML Rock//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:20240409T040200
DTSTAMP:20240409T040100
DTSTART:20240409T040000
RRULE:FREQ=WEEKLY;BYDAY=TU
SEQUENCE:0
UID:407d07c9-36a8-430e-bbea-8b5062e9b5a6
END:VEVENT
END:VCALENDAR',
@MetricsCategoryId, N'{TITHING_OVERVIEW_SCHEDULE_GUID}', 1)
END" );
        }

        private void AddTithingOverviewByCampusMetrics()
        {
            const string sql = @"DECLARE @StartDate int = FORMAT( DATEADD( d, -365, GETDATE()), 'yyyyMMdd' )
DECLARE @EndDate int = FORMAT( GETDATE(), 'yyyyMMdd' )
-- Only Include Person Type Records
DECLARE @PersonRecordTypeId INT = ( SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E' )

;WITH CTE AS (
    SELECT
    [GivingLeaderId]
    , [PrimaryCampusId]
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
	GROUP BY [GivingLeaderId], [PrimaryCampusId], [PrimaryFamilyId]
)
SELECT 
    CAST(COUNT(CASE WHEN [GivingAmount] > [FamiliesMedianTithe] THEN 1 END) AS FLOAT) / COUNT(*) * 100 AS PercentageAboveMedianTithe,
    [PrimaryCampusId],
    COUNT(*) AS TotalFamilies,
    COUNT(CASE WHEN [GivingAmount] > [FamiliesMedianTithe] THEN 1 END) AS FamiliesAboveMedianTithe
FROM 
    CTE
-- Only include families that have a postal code and/or we have a [FamiliesMedianIncome] value
WHERE ( [PostalCode] IS NOT NULL AND [PostalCode] != '') and [FamiliesMedianTithe] is NOT NULL
GROUP BY [PrimaryCampusId];";
            AddSqlSourcedMetric( TITHING_OVERVIEW_BY_CAMPUS_METRIC_GUID, "Tithing Overview By Campus (percent)", TITHING_OVERVIEW_CATEGORY, sql, "This a breakdown of the percentage of families above the median tithe for each campus. Only families with a recognized postal code are included in this metric value." );
        }

        private void AddTithingHouseholdsByCampusMetrics()
        {
            const string sql = @"DECLARE @StartDate int = FORMAT( DATEADD( d, -365, GETDATE()), 'yyyyMMdd' )
DECLARE @EndDate int = FORMAT( GETDATE(), 'yyyyMMdd' )
DECLARE @PersonRecordTypeId INT = ( SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E' )

;WITH CTE AS (
    SELECT
    [GivingLeaderId]
    , [PrimaryCampusId]
    , [PrimaryFamilyId]
    , SUM(ftd.[Amount]) AS [GivingAmount]
    , (SELECT TOP 1 
            LEFT([PostalCode], 5)
        FROM [dbo].[Location] l
        INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMailingLocation] = 1
      ) AS [PostalCode]
    , (SELECT [FamiliesMedianIncome] * .1 FROM [dbo].[AnalyticsSourcePostalCode] WHERE [PostalCode] = (SELECT TOP 1 LEFT([PostalCode], 5) FROM [dbo].[Location] l INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMailingLocation] = 1) ) AS [FamiliesMedianTithe]
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
AND p.[RecordTypeValueId] = @PersonRecordTypeId
	GROUP BY [GivingLeaderId], [PrimaryCampusId], [PrimaryFamilyId]
)
SELECT
    SUM(CASE WHEN [GivingAmount] >= [FamiliesMedianTithe] THEN 1 ELSE 0 END) AS [TotalTithingHouseholds]
    , PrimaryCampusId AS [CampusId]
FROM CTE
WHERE 
   ( PostalCode IS NOT NULL AND PostalCode != '') and FamiliesMedianTithe is NOT NULL
GROUP BY PrimaryCampusId;";
            AddSqlSourcedMetric( TITHING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID, "Tithing Households Per Campus", TITHING_OVERVIEW_CATEGORY, sql, "This is the percent of households that are at/above the tithe. The tithe value is one tenth of the median family income for a family's location/postal code as determined by the AnalyticsSourcePostalCode table. Only families with a recognized postal code are included in this metric value." );
        }

        private void AddGivingHouseholdsOverviewByCampusMetrics()
        {
            const string sql = @"DECLARE @StartDate int = FORMAT( DATEADD( d, -365, GETDATE()), 'yyyyMMdd' )
DECLARE @EndDate int = FORMAT( GETDATE(), 'yyyyMMdd' )
DECLARE @PersonRecordTypeId INT = ( SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E' )

;WITH CTE AS (
    SELECT
    [GivingLeaderId]
    , [PrimaryCampusId]
    , [PrimaryFamilyId]
    , SUM(ftd.[Amount]) AS [GivingAmount]
    , (SELECT TOP 1 
            LEFT([PostalCode], 5)
        FROM [dbo].[Location] l
        INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMailingLocation] = 1
      ) AS [PostalCode]
    , (SELECT [FamiliesMedianIncome] * .1 FROM [dbo].[AnalyticsSourcePostalCode] WHERE [PostalCode] = (SELECT TOP 1 LEFT([PostalCode], 5) FROM [dbo].[Location] l INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMailingLocation] = 1) ) AS [FamiliesMedianTithe]
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
AND p.[RecordTypeValueId] = @PersonRecordTypeId
	GROUP BY [GivingLeaderId], [PrimaryCampusId], [PrimaryFamilyId]
)
SELECT
    SUM(CASE WHEN [GivingAmount] >= 0 THEN 1 ELSE 0 END) AS [TotalGivingHouseholds]
    , PrimaryCampusId AS [CampusId]
FROM CTE
WHERE ( PostalCode IS NOT NULL AND PostalCode != '') and FamiliesMedianTithe is NOT NULL
GROUP BY PrimaryCampusId;";
            AddSqlSourcedMetric( GIVING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID, "Giving Households Per Campus", TITHING_OVERVIEW_CATEGORY, sql, "This is the percent of households per campus that are giving. Only families with a recognized postal code are included in this metric value." );
        }

        private void AddSqlSourcedMetric( string guid, string title, string categoryGuid, string sourceSql, string description = null )
        {
            var formattedTitle = title?.Replace( "'", "''" ) ?? throw new ArgumentNullException( nameof( title ) );
            var createMetricAndMetricCategorySql = $@"DECLARE @MetricId [int] = (SELECT [Id] FROM [dbo].[Metric] WHERE ([Guid] = '{guid}'))
    , @SourceValueTypeId [int] = (SELECT [Id] FROM [dbo].[DefinedValue] WHERE ([Guid] = '{SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_SQL}'))
    , @MetricCategoryId [int] = (SELECT [Id] FROM [dbo].[Category] WHERE ([Guid] = '{categoryGuid}'))
    , @Description [varchar] (max) = {( string.IsNullOrWhiteSpace( description ) ? "NULL" : $"'{description.Replace( "'", "''" )}'" )};

IF (@MetricId IS NULL AND @SourceValueTypeId IS NOT NULL AND @MetricCategoryId IS NOT NULL)
BEGIN
    DECLARE @Now [datetime] = GETDATE();
    INSERT INTO [dbo].[Metric]
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
    )
    VALUES
    (
        0
        , '{formattedTitle}'
        , @Description
        , 0
        , @SourceValueTypeId
        , '{sourceSql.Replace( "'", "''" )}'
        , (SELECT [Id] FROM Schedule WHERE Guid = '{TITHING_OVERVIEW_SCHEDULE_GUID}')
        , @Now
        , @Now
        , '{guid}'
        , 1
        , 0
    );
    SET @MetricId = SCOPE_IDENTITY();
    INSERT INTO [dbo].[MetricCategory]
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
    );
    INSERT INTO [dbo].[MetricPartition]
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
        , 'Campus'
        , (SELECT Id FROM [dbo].[EntityType] WHERE GUID = '{SystemGuid.EntityType.CAMPUS}')
        , 1
        , 0
        , @Now
        , @Now
        , NEWID()
    );
END
";

            Sql( createMetricAndMetricCategorySql );
        }

        private void DeleteMetric( string guid )
        {
            Sql( $@"DECLARE @MetricId [int] = (SELECT [Id] FROM [Metric] WHERE ([Guid] = '{guid}'));
IF (@MetricId IS NOT NULL)
BEGIN
    DELETE FROM [dbo].[MetricPartition] WHERE ([MetricId] = @MetricId);
    DELETE FROM [dbo].[MetricCategory] WHERE ([MetricId] = @MetricId);
    DELETE FROM [dbo].[Metric] WHERE ([Id] = @MetricId);
END" );
        }

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        private void TithingOverviewPageAndBlockUp()
        {
            // Add the Tithing Overview Page
            RockMigrationHelper.AddPage( true, "8D5917F1-4E0E-4F18-8815-62EFBF808995", SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Tithing Overview", "Shows high-level statistics of the tithing overview.", TITHING_OVERVIEW_PAGE, "UpdateAgeBracketValues" );

            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Blocks.Reporting.TithingOverview", TITHING_OVERVIEW_BLOCK_ENTITY_TYPE, true, true );

            // Add/Update the Tithing Overview block type
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Tithing Overview", "Shows high-level statistics of the tithing overview.", "Rock.Blocks.Reporting.TithingOverview", "Reporting", TITHING_OVERVIEW_BLOCK_TYPE );

            // Add Tithing Overview block to the Tithing Overview Page
            RockMigrationHelper.AddBlock( true, TITHING_OVERVIEW_PAGE, null, TITHING_OVERVIEW_BLOCK_TYPE, "Tithing Overview", "Main", "", "", 0, TITHING_OVERVIEW_BLOCK );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        private void TithingOverviewPageAndBlockDown()
        {
            // Delete the Tithing Overview Block
            RockMigrationHelper.DeleteBlock( TITHING_OVERVIEW_BLOCK );

            // Delete the Tithing Overview Page
            RockMigrationHelper.DeletePage( TITHING_OVERVIEW_PAGE );
        }
    }
}
