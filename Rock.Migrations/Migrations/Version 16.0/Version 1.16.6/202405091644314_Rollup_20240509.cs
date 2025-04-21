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

    /// <summary>
    /// Rollup of migrations for plugins performed in branches w/o the migration token
    /// and for tasks in the migration rollup project.
    /// </summary>
    public partial class Rollup_20240509 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateAppleDeviceListPlugin_189();
            MailgunCopyApiKeyToHttpWebhhokSigningKey_192();
            MigrationRollupsForV15_4_1_193();
            MigrationRollupsForV16_5_1_Up_196();

            InteractionCreateDateIndexUp();
            UpdateBuildScript();
            MarkCheckInCategoryIsSystem();
            MigratePreviousBusinessLocationFromGroupLocationHistoricalToGroupLocation();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            InteractionCreateDateIndexDown();

            ConsolidateGenderSettingsOnPublicProfileEditBlock_Down_196();
        }

        /// <summary>
        /// KA: Migration to add Interaction CreatedDateTime Index Post Update Job
        /// </summary>
        private void InteractionCreateDateIndexUp()
        {
            Sql( $@"
        IF NOT EXISTS (
            SELECT 1
            FROM [ServiceJob]
            WHERE [Class] = 'Rock.Jobs.PostV166DataMigrationsAddInteractionCreatedDateTimeIndex'
                            AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_166_ADD_INTERACTION_CREATED_DATE_TIME_INDEX}'
        )
        BEGIN
            INSERT INTO [ServiceJob] (
                [IsSystem]
                ,[IsActive]
                ,[Name]
                ,[Description]
                ,[Class]
                ,[CronExpression]
                ,[NotificationStatus]
                ,[Guid]
            ) VALUES (
                1
                ,1
                ,'Rock Update Helper v16.6 - Interaction CreatedDateTime Index'
                ,'This job will add an index to the CreatedDateTime property on the Interactions table.'
                ,'Rock.Jobs.PostV166DataMigrationsAddInteractionCreatedDateTimeIndex'
                ,'0 0 21 1/1 * ? *'
                ,1
                ,'{SystemGuid.ServiceJob.DATA_MIGRATIONS_166_ADD_INTERACTION_CREATED_DATE_TIME_INDEX}'
            );
        END" );
        }

        /// <summary>
        /// KA: Migration to add Interaction CreatedDateTime Index Post Update Job
        /// </summary>
        private void InteractionCreateDateIndexDown()
        {
            Sql( $@"
            DELETE [ServiceJob]
            WHERE [Class] = 'Rock.Jobs.PostV166DataMigrationsAddInteractionCreatedDateTimeIndex' 
            AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_166_ADD_INTERACTION_CREATED_DATE_TIME_INDEX}'" );
        }

        /// <summary>
        /// JR: Data Migration to Add Updated Performant Build Script 
        /// </summary>
        private void UpdateBuildScript()
        {
            string newBuildScript = @"//- Retrieve the base URL for linking photos from a global attribute 
{% assign publicApplicationRoot = 'Global' | Attribute:'PublicApplicationRoot' %}

{% sql %}
DECLARE @NumberOfDays INT = 365;
DECLARE @NumberOfMonths INT = 13;
DECLARE @ServingAreaDefinedValueGuid UNIQUEIDENTIFIER = '36a554ce-7815-41b9-a435-93f3d52a2828';
DECLARE @ActiveRecordStatusValueId INT = (SELECT Id FROM DefinedValue WHERE Guid = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E');
DECLARE @ConnectionStatusDefinedTypeId INT = (SELECT Id FROM DefinedType WHERE [Guid] = '2e6540ea-63f0-40fe-be50-f2a84735e600');
DECLARE @StartDateKey INT = (SELECT TOP 1 [DateKey] FROM [AnalyticsSourceDate] WHERE [Date] = CAST(DATEADD(DAY, -@NumberOfDays, GETDATE()) AS DATE));
DECLARE @CurrentMonth DATE = DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0);
DECLARE @StartingDateKeyForGiving INT = (SELECT [DateKey] FROM [AnalyticsSourceDate] WHERE [Date] = DATEADD(MONTH, -@NumberOfMonths, @CurrentMonth));

;WITH CTE_Giving AS (
    SELECT
        p.[GivingId],
        asd.[CalendarYear],
        asd.[CalendarMonth],
        POWER(2, asd.[CalendarMonth] - 1) AS DonationMonthBitmask,
        SUM(ftd.[Amount]) AS TotalAmount
    FROM
        [Person] p
        INNER JOIN [PersonAlias] pa ON pa.[PersonId] = p.[Id]
        INNER JOIN [FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
        INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
        INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.[AccountId]
        INNER JOIN [AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
    WHERE
        fa.[IsTaxDeductible] = 1 AND ft.[TransactionDateKey] >= @StartingDateKeyForGiving
    GROUP BY
        p.[GivingId], asd.[CalendarYear], asd.[CalendarMonth]
    HAVING
        SUM(ftd.[Amount]) > 0
),
CTE_GivingAggregated AS (
    SELECT
        GD.[GivingId],
        GD.[CalendarYear],
        SUM(GD.[DonationMonthBitmask]) AS DonationMonthBitmask
    FROM
        CTE_Giving GD
    GROUP BY
        GD.[GivingId], GD.[CalendarYear]
),
CTE_CampusShortCode AS (
    SELECT
        g.[Id] AS GroupId,
        CASE WHEN c.[ShortCode] IS NOT NULL AND c.[ShortCode] != '' THEN c.[ShortCode] ELSE c.[Name] END AS CampusShortCode
    FROM
        [Group] g
        LEFT JOIN [Campus] c ON c.[Id] = g.[CampusId]
),
CTE_GivingResult AS (
    SELECT
        GDA.[GivingId],
        STUFF((
            SELECT '|' + CAST(GDA2.[CalendarYear] AS VARCHAR(4)) + '-' + RIGHT('000' + CAST(GDA2.[DonationMonthBitmask] AS VARCHAR(11)), 11)
            FROM CTE_GivingAggregated GDA2
            WHERE GDA2.[GivingId] = GDA.[GivingId]
            FOR XML PATH('')
        ), 1, 1, '') AS DonationMonthYearBitmask
    FROM
        CTE_GivingAggregated GDA
    GROUP BY
        GDA.[GivingId]
)
SELECT DISTINCT
    p.[Id] AS PersonId,
    CONCAT(CAST(p.[Id] AS NVARCHAR(12)), '-', CAST(g.[Id] AS NVARCHAR(12))) AS PersonGroupKey,
    p.[LastName],
    p.[NickName],
    p.[PhotoId],
    p.[GivingId],
    g.[Id] AS GroupId,
    g.[Name] AS GroupName,
    csc.CampusShortCode,
    MAX(ao.[OccurrenceDate]) AS LastAttendanceDate,
    dvcs.[Value] AS ConnectionStatus,
    CAST(CASE WHEN p.[RecordStatusValueId] = @ActiveRecordStatusValueId AND gm.[IsArchived] = 0 THEN 1 ELSE 0 END AS BIT) AS IsActive,
    GR.DonationMonthYearBitmask
FROM
    [Person] p
    INNER JOIN [PersonAlias] pa ON pa.[PersonId] = p.[Id]
    INNER JOIN [Attendance] a ON a.[PersonAliasId] = pa.[Id]
    INNER JOIN [AttendanceOccurrence] ao ON ao.[Id] = a.[OccurrenceId]
    INNER JOIN [Group] g ON g.[Id] = ao.[GroupId]
    INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
    INNER JOIN [DefinedValue] dvp ON dvp.[Id] = gt.[GroupTypePurposeValueId] AND dvp.[Guid] = @ServingAreaDefinedValueGuid
    LEFT JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id] AND gm.[GroupId] = g.[Id]
    LEFT JOIN CTE_CampusShortCode csc ON csc.GroupId = g.[Id]
    LEFT JOIN [DefinedValue] dvcs ON dvcs.[Id] = p.[ConnectionStatusValueId] AND dvcs.[DefinedTypeId] = @ConnectionStatusDefinedTypeId
    LEFT JOIN CTE_GivingResult GR ON p.[GivingId] = GR.GivingId
WHERE
    ao.[OccurrenceDateKey] >= @StartDateKey AND a.[DidAttend] = 1
GROUP BY
    p.[Id], p.[LastName], p.[NickName], p.[PhotoId], p.[GivingId], g.[Id], g.[Name], csc.CampusShortCode, dvcs.[Value], p.[RecordStatusValueId], gm.[IsArchived], GR.DonationMonthYearBitmask;

{% endsql %}
{
    ""PeopleData"": [
    {% for result in results %}
        {% if forloop.first != true %},{% endif %}
        {
            ""PersonGroupKey"": {{ result.PersonGroupKey | ToJSON }},
            ""PersonId"": {{ result.PersonId }},
            ""LastName"": {{ result.LastName | ToJSON }},
            ""NickName"": {{ result.NickName | ToJSON }},
            ""PhotoUrl"": ""{% if result.PhotoId %}{{ publicApplicationRoot }}GetAvatar.ashx?PhotoId={{ result.PhotoId }}{% else %}{{ publicApplicationRoot }}GetAvatar.ashx?AgeClassification=Adult&Gender=Male&RecordTypeId=1&Text={{ result.NickName | Slice: 0, 1 | UrlEncode }}{{ result.LastName | Slice: 0, 1 | UrlEncode }}{% endif %}"",
            ""GivingId"": {{ result.GivingId | ToJSON }},
            ""LastAttendanceDate"": ""{{ result.LastAttendanceDate | Date: 'yyyy-MM-dd' }}"",
            ""GroupId"": {{ result.GroupId }},
            ""GroupName"": {{ result.GroupName | ToJSON }},
            ""CampusShortCode"": {{ result.CampusShortCode | ToJSON }},
            ""ConnectionStatus"": {{ result.ConnectionStatus | ToJSON }},
            ""IsActive"": {{ result.IsActive }},
            ""DonationMonthYearBitmask"": {% if result.DonationMonthYearBitmask != null %}{{ result.DonationMonthYearBitmask | ToJSON}}{% else %}null{% endif %}
        }
    {% endfor %}
    ]
}
";

            Sql( $@"UPDATE [PersistedDataset]
           SET [BuildScript] = '{newBuildScript.Replace( "'", "''" )}'
           WHERE [Guid] = '10539E72-B5D3-48E2-B9C6-DB43AFDAD55F'" );

            Sql( $@"UPDATE [PersistedDataset]
               SET [ResultData] = null
               WHERE [Guid] = '10539E72-B5D3-48E2-B9C6-DB43AFDAD55F'" );
        }

        /// <summary>
        /// NA: Make "Check-in" Category (for GroupType) a System category
        /// </summary>
        private void MarkCheckInCategoryIsSystem()
        {
            Sql( @"
-- Make this a System category so no one can delete it 
UPDATE [dbo].[Category] SET [IsSystem] = 1 WHERE [Guid] = 'c8e0fd8d-3032-4acd-9db9-ff70b11d6bcc'" );
        }

        /// <summary>
        /// SK: Migration Previous Business Location from GroupLocationHistorical to GroupLocation
        /// </summary>
        private void MigratePreviousBusinessLocationFromGroupLocationHistoricalToGroupLocation()
        {
            Sql( @"
DECLARE @businessRecordTypeValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid]='BF64ADD3-E70A-44CE-9C4B-E76BBED37550')
DECLARE @previousGroupLocationTypeValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid]='853D98F1-6E08-4321-861B-520B4106CFE0')

SELECT glh.GroupId, @previousGroupLocationTypeValueId as GroupLocationTypeValueId, glh.LocationId, NEWID() as [Guid], 1 as IsMailingLocation, glh.[Id]
INTO #GLH
FROM  [Group] AS g INNER JOIN
         Person AS p ON p.PrimaryFamilyId = g.Id INNER JOIN
         GroupLocation AS gl ON g.Id = gl.GroupId INNER JOIN
         GroupLocationHistorical AS glh ON gl.Id = glh.GroupLocationId AND glh.LocationId <> gl.LocationId
WHERE P.[RecordTypeValueId] = @businessRecordTypeValueId

INSERT INTO GroupLocation
			(GroupId, GroupLocationTypeValueId, LocationId, [Guid], IsMailingLocation, IsMappedLocation)
SELECT GroupId, GroupLocationTypeValueId, LocationId, [Guid], IsMailingLocation, 0
FROM #GLH
WHERE NOT EXISTS (SELECT 1 FROM [GroupLocation] gl WHERE gl.GroupId = #GLH.GroupId AND gl.LocationId = #GLH.LocationId) 

DELETE FROM [GroupLocationHistorical] WHERE [Id] IN (SELECT [Id] FROM [#GLH])
" );
        }

        #region Plugin Migrations moved to EF migration

        /// <summary>
        /// Plugin migration from 189_UpdateAppleDeviceList
        /// </summary>
        private void UpdateAppleDeviceListPlugin_189()
        {
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone15,4", "iPhone 15", "d97cdd93-276c-487e-9a91-7fb8023856cc", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone15,5", "iPhone 15 Plus", "a8d06af7-9baf-4ec8-91c2-0e0eca171b5a", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone16,1", "iPhone 15 Pro", "e4f9fa97-d51f-47ba-9795-79320e025817", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone16,2", "iPhone 15 Pro Max", "91e5aa06-6b34-4d08-91a7-7c75a891041d", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,1", "Apple Watch Series 9 41mm case (GPS)", "f5468959-14a5-46c8-bcfd-201ad56a7008", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,2", "Apple Watch Series 9 45mm case (GPS)", "c5ef18a9-cd62-446f-bede-9f28b0907d41", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,3", "Apple Watch Series 9 41mm case (GPS+Cellular)", "7b902398-25f3-4250-95b1-1c5469a81330", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,4", "Apple Watch Series 9 45mm case (GPS+Cellular)", "f9c8ca02-db1b-4525-bace-7a1afbec335c", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,5", "Apple Watch Ultra 2", "baef68c7-274a-4ba3-ac4a-8d781ea57b42", true );

            // Remove invalid values. No need to update Device Modles since the description is correct.
            RockMigrationHelper.DeleteDefinedValue( "F902AB78-90EB-46E8-839D-8082F537AF22" ); //iPad14,6-A
            RockMigrationHelper.DeleteDefinedValue( "8417E05B-C79A-4CCE-B781-F38908E509C9" ); //iPad14,6-B

            Sql( @"
                DECLARE @AppleDeviceDefinedTypeId INT = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = 'DAE31F78-7AB9-4ACE-9EE1-C1E6A734562C')

                UPDATE [PersonalDevice] SET [Model] = dv.[Description]
                FROM [PersonalDevice] pd
                JOIN [DefinedValue] dv ON pd.[Model] = dv.[Value]
                WHERE pd.[Manufacturer] = 'Apple'
                  AND pd.[Model] like '%,%'
                  AND dv.DefinedTypeId = @AppleDeviceDefinedTypeId" );
        }

        /// <summary>
        /// Plugin migration from 192_MailgunCopyApiKeyToHttpWebhookSigningKey
        /// </summary>
        private void MailgunCopyApiKeyToHttpWebhhokSigningKey_192()
        {
            Sql( @"
DECLARE @EntityTypeId [int] = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Communication.Transport.MailgunHttp');
DECLARE @FieldTypeId [int] = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA'); -- Text

-- Attempt to find the existing APIKey Attribute and AttributeValue.
DECLARE @ApiKeyAttrId [int] = (
    SELECT TOP 1 [Id]
    FROM [Attribute]
    WHERE [EntityTypeId] = @EntityTypeId
        AND [FieldTypeId] = @FieldTypeId
        AND [Key] = 'APIKey'
);

DECLARE @ApiKeyAttrValue [nvarchar](max) = (
    SELECT TOP 1 [Value]
    FROM [AttributeValue]
    WHERE [AttributeId] = @ApiKeyAttrId
);

-- Only attempt to set the HTTPWebhookSigningKey value if the APIKey value is actually set.
IF @ApiKeyAttrValue IS NOT NULL AND @ApiKeyAttrValue <> ''
BEGIN
    -- Ensure the HTTPWebhookSigningKey Attribute exists; create it if not.
    DECLARE @WebhookKeyAttrKey [nvarchar](21) = 'HTTPWebhookSigningKey';
    DECLARE @WebhookKeyAttrId [int] = (
        SELECT [Id]
        FROM [Attribute]
        WHERE [EntityTypeId] = @EntityTypeId
            AND [FieldTypeId] = @FieldTypeId
            AND [Key] = @WebhookKeyAttrKey
    );

    DECLARE @Now [datetime] = (SELECT GETDATE());

    IF @WebhookKeyAttrId IS NULL
    BEGIN
        INSERT INTO [Attribute]
        (
            [IsSystem]
            , [FieldTypeId]
            , [EntityTypeId]
            , [EntityTypeQualifierColumn]
            , [EntityTypeQualifierValue]
            , [Key]
            , [Name]
            , [Description]
            , [Order]
            , [IsGridColumn]
            , [DefaultValue]
            , [IsMultiValue]
            , [IsRequired]
            , [Guid]
            , [CreatedDateTime]
            , [ModifiedDateTime]
            , [IconCssClass]
            , [AbbreviatedName]
            , [IsDefaultPersistedValueDirty]
        )
        VALUES
        (
            0
            , @FieldTypeId
            , @EntityTypeId
            , ''
            , ''
            , @WebhookKeyAttrKey
            , 'HTTP Webhook Signing Key'
            , 'The HTTP Webhook Signing Key provided by Mailgun. Newly-created Mailgun accounts will have separate API and Webhook keys.'
            , 4
            , 0
            , ''
            , 0
            , 1
            , NEWID()
            , @Now
            , @Now
            , ''
            , 'HTTP Webhook Signing Key'
            , 0
        );

        SET @WebhookKeyAttrId = (SELECT @@IDENTITY);
    END

    -- If the HTTPWebhookSigningKey happens to already have a value, don't overwrite it.
    DECLARE @WebhookKeyAttrValueId [int] = (
        SELECT TOP 1 [Id]
        FROM [AttributeValue]
        WHERE [AttributeId] = @WebhookKeyAttrId
    );

    DECLARE @WebhookKeyAttrValue [nvarchar](max) = (SELECT [Value] FROM [AttributeValue] WHERE [Id] = @WebhookKeyAttrValueId);

    IF @WebhookKeyAttrValueId IS NULL
    BEGIN
        -- AttributeValue didn't already exist; create it.
        INSERT INTO [AttributeValue]
        (
            [IsSystem]
            , [AttributeId]
            , [EntityId]
            , [Value]
            , [Guid]
            , [CreatedDateTime]
            , [ModifiedDateTime]
        )
        VALUES
        (
            0
            , @WebhookKeyAttrId
            , 0
            , @ApiKeyAttrValue
            , NEWID()
            , @Now
            , @Now
        );
    END
    ELSE IF @WebhookKeyAttrValue IS NULL OR @WebhookKeyAttrValue = ''
    BEGIN
        -- AttributeValue already existed without a value; update it.
        UPDATE [AttributeValue]
        SET [Value] = @ApiKeyAttrValue
            , [ModifiedDateTime]= @Now
        WHERE [Id] = @WebhookKeyAttrValueId;
    END
END" );
        }

        private void MigrationRollupsForV15_4_1_193()
        {
            // add ServiceJob: Rock Update Helper v15.4 - Update AgeBracket Values
            Sql( $@"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.PostV154DataMigrationsUpdateAgeBracketValues' AND [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_154_UPDATE_AGE_BRACKET_VALUES}' )
            BEGIN
               INSERT INTO [ServiceJob] (
                  [IsSystem]
                  ,[IsActive]
                  ,[Name]
                  ,[Description]
                  ,[Class]
                  ,[CronExpression]
                  ,[NotificationStatus]
                  ,[Guid] )
               VALUES ( 
                  1
                  ,1
                  ,'Rock Update Helper v15.4 - Update AgeBracket Values'
                  ,'This job will update the AgeBracket values to reflect the new values after splitting the 0 - 12 bracket.'
                  ,'Rock.Jobs.PostV154DataMigrationsUpdateAgeBracketValues'
                  ,'0 0 21 1/1 * ? *'
                  ,1
                  ,'{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_154_UPDATE_AGE_BRACKET_VALUES}'
                  );
            END" );

            UpdateMetricsUp();
        }

        private void UpdateMetricsUp()
        {
            Sql( $@"
UPDATE Metric SET
	[Title] = '0-5',
	[Description] = 'Active people between the ages of 0 and 5',
	SourceSql = 'DECLARE @ActiveRecordStatusValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
    )
    ,@PersonRecordTypeValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
    )     
    ,@Today Date = GetDate()   
	SELECT COUNT(1), P.[PrimaryCampusId]  
    FROM [Person] P  
    WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
	    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
	    AND P.[IsDeceased] = 0
	    AND P.[Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
	    AND P.[AgeBracket] = {Rock.Enums.Crm.AgeBracket.ZeroToFive.ConvertToInt()}    
	GROUP BY ALL P.[PrimaryCampusId] ORDER BY P.[PrimaryCampusId]'
	WHERE [Guid] = 'EEDEE264-F49D-46B9-815D-C5DBB5DCC9CE'

	DECLARE @MetricId [int] = (SELECT [Id] FROM [Metric] WHERE ([Guid] = '909FA7E8-F991-44EC-B2BF-07BEE9A29558'))
    , @SourceValueTypeId [int] = (SELECT [Id] FROM [DefinedValue] WHERE ([Guid] = '6A1E1A1B-A636-4E12-B90C-D7FD1BDAE764'))
    , @MetricCategoryId [int] = (SELECT [Id] FROM [Category] WHERE ([Guid] = '10716347-44BA-44F1-8E82-21C23445EB76'))
    , @Description [varchar] (max) = 'Active people between the ages of 6 and 12';

IF (@MetricId IS NULL AND @SourceValueTypeId IS NOT NULL AND @MetricCategoryId IS NOT NULL)
BEGIN
    DECLARE @Now [datetime] = GETDATE();
    -- Create 06-12 Metric
    INSERT INTO [Metric]
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
        , '06-12'
        , @Description
        , 0
        , @SourceValueTypeId
        , 'DECLARE @ActiveRecordStatusValueId INT = (
		    SELECT TOP 1 Id
            FROM DefinedValue
            WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
	    )
	    ,@PersonRecordTypeValueId INT = (
	    	SELECT TOP 1 Id
            FROM DefinedValue
            WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
	    )     
	    ,@Today Date = GetDate()      
	    SELECT COUNT(1), P.[PrimaryCampusId]
	    FROM [Person] P  
	    WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
	    	AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
	    	AND P.[IsDeceased] = 0
	    	AND P.[Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
	    	AND P.[AgeBracket] = {Rock.Enums.Crm.AgeBracket.SixToTwelve.ConvertToInt()}    
	    GROUP BY ALL P.[PrimaryCampusId] ORDER BY P.[PrimaryCampusId]'
        , (SELECT [Id] FROM Schedule WHERE Guid = '20AFE3A5-86DC-4232-9FAE-9271D7A251FB')
        , @Now
        , @Now
        , '909FA7E8-F991-44EC-B2BF-07BEE9A29558'
        , 1
        , 0
    );
    SET @MetricId = SCOPE_IDENTITY();
    -- Create Metric Category
    INSERT INTO [MetricCategory]
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
    -- Create Partitions for newly created 06-12 Metric
    INSERT INTO [MetricPartition]
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
        , 'CAMPUS'
        , (SELECT Id FROM [EntityType] WHERE GUID = '{Rock.SystemGuid.EntityType.CAMPUS}')
        , 1
        , 0
        , @Now
        , @Now
        , NEWID()
    );
END
" );
        }

        /// <summary>
        /// 
        /// </summary>
        private void MigrationRollupsForV16_5_1_Up_196()
        {
            ConsolidateGenderSettingsOnPublicProfileEditBlockUp();
            // Commented thie method out to run in a later EF migration - AddOrUpdateResultPageLocationAndAddOrUpdateVolGenPersistedDataset.cs
            // AddVolunteerGenerosityAnalysisFeature();
            DropHistoryGuidIndexUp();
        }

        const string PublicProfileEditBlockTypeGuid = "841D1670-8BFD-4913-8409-FB47EB7A2AB9";
        const string GenderBlockTypeAttributeGuid = "DD636ABE-3E5B-442F-9548-9F85DF768FFF";
        const string GenderAttributeValueGuid = "6FE7E960-0DC2-4089-B346-1CD047EBE6F3";
        const string MyAccountPageGuid = "C0854F84-2E8B-479C-A3FB-6B47BE89B795";

        /// <summary>
        /// KA: MigrationToConsolidateGenderSettingsOnPublicProfileEditBlock
        /// </summary>
        public void ConsolidateGenderSettingsOnPublicProfileEditBlockUp()
        {

        RockMigrationHelper.AddOrUpdateBlockTypeAttribute( PublicProfileEditBlockTypeGuid, Rock.SystemGuid.FieldType.SINGLE_SELECT, "Gender", "Gender", "Gender", "How should Gender be displayed?", 26, "Required", GenderBlockTypeAttributeGuid );

            string qry = $@"
DECLARE @BlockEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')
DECLARE @BlockTypeId INT = (SELECT [Id] FROM [BlockType] WHERE Guid = '{PublicProfileEditBlockTypeGuid}')
DECLARE @BlockId INT = (SELECT [Id] FROM [Block] WHERE BlockTypeId = @BlockTypeId AND PageId = (SELECT Id FROM Page WHERE Guid = '{MyAccountPageGuid}'))
DECLARE @GenderAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [KEY] = 'Gender' AND [EntityTypeId] = @BlockEntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = @BlockTypeId)
DECLARE @RequireGenderAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [KEY] = 'RequireGender' AND [EntityTypeId] = @BlockEntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = @BlockTypeId)
DECLARE @ShowGenderAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [KEY] = 'ShowGender' AND [EntityTypeId] = @BlockEntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = @BlockTypeId)
DECLARE @RequireGender VARCHAR(50) = (SELECT [Value] FROM [AttributeValue] WHERE [EntityId] = @BlockId AND [AttributeId] = @RequireGenderAttributeId)
DECLARE @ShowGender VARCHAR(50) = (SELECT [Value] FROM [AttributeValue] WHERE [EntityId] = @BlockId AND [AttributeId] = @ShowGenderAttributeId)
DECLARE @TheValue VARCHAR(50) = CASE
	WHEN @RequireGender = 'True' AND @ShowGender = 'True' THEN 'Required'
	WHEN @RequireGender = 'True' AND @ShowGender = 'False' THEN 'Required'
	WHEN @RequireGender = 'False' AND @ShowGender = 'False' THEN 'Hide'
	WHEN @RequireGender = 'False' AND @ShowGender = 'True' THEN 'Optional'
	ELSE 'Required'
END

IF(@BlockId IS NULL)
BEGIN
    RETURN;
END

IF EXISTS (SELECT 1 FROM [AttributeValue] WHERE [EntityId] = @BlockId AND [AttributeId] = @GenderAttributeId)  
BEGIN  
	UPDATE [AttributeValue]   
	SET [Value] = @TheValue,  
	[Guid] = '{GenderAttributeValueGuid}'
	WHERE [EntityId] = @BlockId AND [AttributeId] = @GenderAttributeId;  
END  
ELSE  
BEGIN  
	INSERT INTO [AttributeValue] (
	    [IsSystem],
	    [AttributeId],
	    [EntityId],
	    [Value],
	    [Guid])
	VALUES(
	    1,
	    @GenderAttributeId,
	    @BlockId,
	    @TheValue,
	    '{GenderAttributeValueGuid}') 
END
";
            Sql( qry );
        }

        /// <summary>
        /// KA: MigrationToConsolidateGenderSettingsOnPublicProfileEditBlock
        /// </summary>
        public void ConsolidateGenderSettingsOnPublicProfileEditBlock_Down_196()
        {
            RockMigrationHelper.DeleteBlockAttribute( GenderBlockTypeAttributeGuid );
        }

        /// <summary>
        /// DL: Drop History.Guid index
        /// Drop index on [History].[Guid] column.
        /// </summary>
        private void DropHistoryGuidIndexUp()
        {
            Sql( @"
-- Drop index on [History].[Guid] column
IF EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'IX_Guid' AND object_id = OBJECT_ID('History')) 
BEGIN
	DROP INDEX [IX_Guid] ON [History]
END
      " );
        }

        #endregion
    }
}
