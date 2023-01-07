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
    public partial class Rollup_20220929 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddSystemSettingPersonalizationSegmentAffinityDuration();
            Update_spCrm_FamilyAnalyticsEraDataset();
            Update_spCrm_FamilyAnalyticsGiving();
            FixMediaInteractionChannelEntityTypeIdUp();
            UpdateStatementGeneratorDownloadLinkUp();
            ShowAppleTVAppsPage();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            GroupRequirementChangesForFundraisingOpportunitiesDown();
            UpdateStatementGeneratorDownloadLinkDown();
        }

        /// <summary>
        /// ED: Add System Setting PersonalizationSegmentAffinityDuration
        /// </summary>
        private void AddSystemSettingPersonalizationSegmentAffinityDuration()
        {
            Sql( $@"
                DECLARE @FieldTypeId INT = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{Rock.SystemGuid.FieldType.TEXT}')
                DECLARE @AttributeId INT = (
	                SELECT [Id]
                    FROM [Attribute]
                    WHERE [EntityTypeId] IS NULL
		                AND [EntityTypeQualifierColumn] = '{Rock.Model.Attribute.SYSTEM_SETTING_QUALIFIER}'
		                AND [Key] = '')

                IF @AttributeId IS NULL
                BEGIN
                    INSERT INTO [Attribute] (
                        [IsSystem]
		                , [FieldTypeId]
		                , [EntityTypeId]
		                , [EntityTypeQualifierColumn]
		                , [EntityTypeQualifierValue]
		                , [Order]
		                , [IsGridColumn]
		                , [IsMultiValue]
		                , [IsRequired]
		                , [Key]
		                , [Name]
		                , [DefaultValue]
		                , [Guid])
                    VALUES(
                          1
		                , @FieldTypeId
		                , NULL
		                , '{Rock.Model.Attribute.SYSTEM_SETTING_QUALIFIER}'
		                , ''
		                , 0
		                , 0
		                , 0
		                , 0
		                , 'core_PersonalizationSegmentCookieAffinityDurationMinutes'
		                , 'Personalization Segment Cookie Affinity Duration (Minutes)'
		                , '5'
		                , '239CD168-0373-49A6-93EB-FC494228F11E')
                END" );
        }

        /// <summary>
        /// KA:Migration to Update spCrm_FamilyAnalyticsEraDataset.sql to use SundayDate when comparing Transaction date
        /// </summary>
        private void Update_spCrm_FamilyAnalyticsEraDataset()
        {
            Sql( $@"
                /*
                <doc>
	                <summary>
 		                This stored procedure returns a data set used by the Rock eRA job to add/remove
		                people from being an eRA. It should not be modified as it will be updated in the
		                future to meet additional requirements.
		                The goal of the query is to return both those that meet the eRA requirements as well
		                as those that are marked as already being an eRA and the criteria to ensure that
		                they still should be an era.
	                </summary>
	
	                <remarks>
		                For eRA we only consider adults for the criteria.
	                </remarks>
	                <code>
		                EXEC [dbo].[spCrm_FamilyAnalyticsEraDataset] 
	                </code>
                </doc>
                */
                ALTER PROCEDURE [dbo].[spCrm_FamilyAnalyticsEraDataset]
                AS
                BEGIN
                    -- configuration of the duration in weeks
                    DECLARE @EntryGivingDurationLongWeeks INT = 52
                    DECLARE @EntryGivingDurationShortWeeks INT = 6
                    DECLARE @EntryAttendanceDurationWeeks INT = 16
                    DECLARE @ExitGivingDurationWeeks INT = 8
                    DECLARE @ExitAttendanceDurationShortWeeks INT = 4
                    DECLARE @ExitAttendanceDurationLongWeeks INT = 16
                    -- configuration of the item counts in the durations
                    DECLARE @EntryGiftCountDurationLong INT = 4
                    DECLARE @EntryGiftCountDurationShort INT = 1
                    DECLARE @EntryAttendanceCountDuration INT = 8
                    DECLARE @cACTIVE_RECORD_STATUS_VALUE_GUID UNIQUEIDENTIFIER = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
                    DECLARE @cPERSON_RECORD_TYPE_VALUE_GUID UNIQUEIDENTIFIER = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
                    DECLARE @cATTRIBUTE_IS_ERA_GUID UNIQUEIDENTIFIER = 'CE5739C5-2156-E2AB-48E5-1337C38B935E'
                    DECLARE @cFAMILY_GROUPTYPE_GUID UNIQUEIDENTIFIER = '790E3215-3B10-442B-AF69-616C0DCB998E'
                    DECLARE @cADULT_ROLE_GUID UNIQUEIDENTIFIER = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'
                    DECLARE @cTRANSACTION_TYPE_CONTRIBUTION UNIQUEIDENTIFIER = '2D607262-52D6-4724-910D-5C6E8FB89ACC';
	                DECLARE @cATTRIBUTE_ERA_START_DATE_GUID UNIQUEIDENTIFIER = 'A106610C-A7A1-469E-4097-9DE6400FDFC2';
                    -- --------- END CONFIGURATION --------------
                    DECLARE @PersonRecordTypeValueId INT = (
                            SELECT TOP 1 [Id]
                            FROM [DefinedValue]
                            WHERE [Guid] = @cPERSON_RECORD_TYPE_VALUE_GUID
                            )
                    DECLARE @IsEraAttributeId INT = (
                            SELECT TOP 1 [Id]
                            FROM [Attribute]
                            WHERE [Guid] = @cATTRIBUTE_IS_ERA_GUID
                            )
	                DECLARE @EraStartDateAttributeId INT = (
                            SELECT TOP 1 [Id]
                            FROM [Attribute]
                            WHERE [Guid] = @cATTRIBUTE_ERA_START_DATE_GUID
                            )
                    DECLARE @FamilyGroupTypeId INT = (
                            SELECT TOP 1 [Id]
                            FROM [GroupType]
                            WHERE [Guid] = @cFAMILY_GROUPTYPE_GUID
                            )
                    DECLARE @AdultRoleId INT = (
                            SELECT TOP 1 [Id]
                            FROM [GroupTypeRole]
                            WHERE [Guid] = @cADULT_ROLE_GUID
                            )
                    DECLARE @ContributionType INT = (
                            SELECT TOP 1 [Id]
                            FROM [DefinedValue]
                            WHERE [Guid] = @cTRANSACTION_TYPE_CONTRIBUTION
                            )
                    -- calculate dates for query
                    DECLARE @SundayDateStart DATETIME = [dbo].[ufnUtility_GetPreviousSundayDate]()
                    DECLARE @SundayEntryGivingDurationLong DATETIME = DATEADD(DAY, (7 * @EntryGivingDurationLongWeeks * - 1), @SundayDateStart)
                    DECLARE @SundayEntryGivingDurationShort DATETIME = DATEADD(DAY, (7 * @EntryGivingDurationShortWeeks * - 1), @SundayDateStart)
                    DECLARE @SundayEntryAttendanceDuration DATETIME = DATEADD(DAY, (7 * @EntryAttendanceDurationWeeks * - 1), @SundayDateStart)
                    DECLARE @SundayExitGivingDuration DATETIME = DATEADD(DAY, (7 * @ExitGivingDurationWeeks * - 1), @SundayDateStart)
                    DECLARE @SundayExitAttendanceDurationShort DATETIME = DATEADD(DAY, (7 * @ExitAttendanceDurationShortWeeks * - 1), @SundayDateStart)
                    DECLARE @SundayExitAttendanceDurationLong DATETIME = DATEADD(DAY, (7 * @ExitAttendanceDurationLongWeeks * - 1), @SundayDateStart)
                    DECLARE @TempFinancialTransactionByDateAndGivingId TABLE (
                 DistinctCount INT
                        ,GivingId NVARCHAR(50)
                        ,TransactionDateTime DATETIME
                        ,SundayDate Date
                        )
                    INSERT INTO @TempFinancialTransactionByDateAndGivingId
                    SELECT COUNT(DISTINCT (ft.[Id])) [DistinctCount]
                        ,g1.GivingId
                        ,ft.TransactionDateTime [TransactionDateTime]
                        ,SundayDate Date
                    FROM [FinancialTransaction] ft
                    INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
                    INNER JOIN [Person] g1 ON g1.[Id] = pa.[PersonId]
                    INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
                    INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
                    WHERE ft.TransactionTypeValueId = @ContributionType
                        AND fa.[IsTaxDeductible] = 1
                        AND ft.TransactionDateTime >= @SundayEntryGivingDurationLong
                    GROUP BY g1.GivingId
                        ,ft.TransactionDateTime
                        ,SundayDate
                    DECLARE @TempAttendanceBySundayDateAndFamily TABLE (
                        SundayDate DATE
                        ,StartDateTime DATETIME
                        ,FamilyId INT
                        )
                    INSERT INTO @TempAttendanceBySundayDateAndFamily
                    SELECT O.SundayDate
                        ,a.StartDateTime
                        ,fg.Id [FamilyId]
                    FROM [Attendance] a
                    INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
                    INNER JOIN [Group] ag ON ag.[Id] = O.[GroupId]
                    INNER JOIN [GroupType] agt ON agt.[Id] = ag.[GroupTypeId]
                        AND agt.[AttendanceCountsAsWeekendService] = 1
                    INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
                    INNER JOIN [GroupMember] fgm ON fgm.[PersonId] = pa.[PersonId]
                    INNER JOIN [Group] fg ON fg.[Id] = fgm.[GroupId]
                        AND fg.[GroupTypeId] = @FamilyGroupTypeId
                    WHERE a.[DidAttend] = 1
                        AND a.StartDateTime >= @SundayExitAttendanceDurationLong
                    GROUP BY O.SundayDate
                        ,StartDateTime
                        ,fg.Id
	                DECLARE @TempEntryGiftCountDurationShort TABLE (
			                GivingId NVARCHAR(50) INDEX IX1 CLUSTERED
			                , DistinctCount INT
		                    )
	                INSERT INTO @TempEntryGiftCountDurationShort
	                SELECT GivingId, SUM(ft.DistinctCount)
	                FROM @TempFinancialTransactionByDateAndGivingId ft
	                WHERE ft.TransactionDateTime >= @SundayEntryGivingDurationShort
		                AND ft.SundayDate <= @SundayDateStart
	                GROUP BY GivingId
	                DECLARE @TempExitGiftCountDuration TABLE (
			                GivingId NVARCHAR(50) INDEX IX2 CLUSTERED
			                , DistinctCount INT
		                    )
	                INSERT INTO @TempExitGiftCountDuration
	                SELECT GivingId, ISNULL(SUM(ft.DistinctCount), 0)
	                FROM @TempFinancialTransactionByDateAndGivingId ft
	                WHERE ft.TransactionDateTime >= @SundayExitGivingDuration
		                AND ft.SundayDate <= @SundayDateStart
	                GROUP BY GivingId
	                DECLARE @TempEntryGiftCountDurationLong TABLE (
			                GivingId NVARCHAR(50) INDEX IX3 CLUSTERED
			                , DistinctCount INT
		                    )
	                INSERT INTO @TempEntryGiftCountDurationLong
	                SELECT GivingId, ISNULL(SUM(ft.DistinctCount), 0) AS [EntryGiftCountDurationLong]
					                FROM @TempFinancialTransactionByDateAndGivingId ft
					                WHERE ft.TransactionDateTime >= @SundayEntryGivingDurationLong
						                AND ft.SundayDate <= @SundayDateStart
	                GROUP BY GivingId
	                DECLARE @TempExitAttendanceCountDurationShort TABLE (
			                FamilyId INT INDEX IX4 CLUSTERED
			                , DistinctCount INT
		                    )
	                INSERT INTO @TempExitAttendanceCountDurationShort
	                SELECT FamilyId, COUNT(DISTINCT a.SundayDate) AS [ExitAttendanceCountDurationShort]
					                FROM @TempAttendanceBySundayDateAndFamily a
					                WHERE a.StartDateTime <= @SundayDateStart
						                AND a.StartDateTime >= @SundayExitAttendanceDurationShort
	                GROUP BY FamilyId
	                DECLARE @TempEntryAttendanceCountDuration TABLE (
			                FamilyId INT INDEX IX5 CLUSTERED
			                , DistinctCount INT
		                    )
	                INSERT INTO @TempEntryAttendanceCountDuration
	                SELECT FamilyId, COUNT(DISTINCT a.SundayDate) AS [EntryAttendanceCountDuration]
					                FROM @TempAttendanceBySundayDateAndFamily a
					                WHERE a.StartDateTime <= @SundayDateStart
						                AND a.StartDateTime >= @SundayEntryAttendanceDuration
	                GROUP BY FamilyId
	                DECLARE @TempExitAttendanceCountDurationLong TABLE (
			                FamilyId INT INDEX IX6 CLUSTERED
			                , DistinctCount INT
		                    )
	                INSERT INTO @TempExitAttendanceCountDurationLong
	                SELECT FamilyId, COUNT(DISTINCT a.SundayDate) AS [ExitAttendanceCountDurationLong]
					                FROM @TempAttendanceBySundayDateAndFamily a
					                WHERE a.StartDateTime <= @SundayDateStart
						                AND a.StartDateTime >= @SundayExitAttendanceDurationLong
	                GROUP BY FamilyId
	                DECLARE @TempPersonFamilyGroupIds TABLE (
			                Id INT,
			                GivingId NVARCHAR(50),
			                [IsEra] INT,
			                FamilyId INT,
			                INDEX IXR NONCLUSTERED(GivingId,[IsEra],FamilyId)
		                    )
	                INSERT INTO @TempPersonFamilyGroupIds
	                SELECT p.[Id]
		                , p.[GivingId]
		                ,CASE 
			                WHEN (era.[Value] = 'true')
				                THEN 1
			                ELSE 0
			                END AS [IsEra]
		                ,g.[Id] AS [FamilyId]
	                FROM [Person] p
	                INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
		                AND gm.[GroupRoleId] = @AdultRoleId
	                INNER JOIN [Group] g ON g.[Id] = gm.[GroupId]
		                AND g.[GroupTypeId] = @FamilyGroupTypeId
	                LEFT OUTER JOIN [AttributeValue] era ON era.[EntityId] = p.[Id]
		                AND era.[AttributeId] = @IsEraAttributeId
	                WHERE [RecordTypeValueId] = @PersonRecordTypeValueId -- person record type (not business)
	                DECLARE @TempIsEraFamilyMembers TABLE (
		                PersonId INT,
		                [IsEra] INT,
		                FamilyId INT,
		                INDEX IXR NONCLUSTERED([IsEra],FamilyId)
	                    )
	                DECLARE @CreatedDateTime DATETIME = GETDATE()
	                DECLARE @StartDateValue NVARCHAR(MAX) = CONVERT(nvarchar(MAX), GETDATE(), 127)
	                INSERT INTO @TempIsEraFamilyMembers
	                SELECT p.Id,
		                Min(tgpgids.IsEra),
		                Min(p.PrimaryFamilyId)
		                FROM [Person] p
		                LEFT JOIN @TempPersonFamilyGroupIds tgpgids ON p.PrimaryFamilyId = tgpgids.FamilyId
		                WHERE tgpgids.IsEra = 1
		                GROUP BY  p.Id
    
                    -- Insert missing IsEra Attribute --
	                MERGE [AttributeValue] AS TARGET
	                USING @TempIsEraFamilyMembers AS SOURCE
	                ON (TARGET.EntityId = SOURCE.PersonId AND TARGET.AttributeId = @IsEraAttributeId)
	                WHEN NOT MATCHED BY TARGET
	                THEN INSERT (EntityId, AttributeId, Value, IsSystem, Guid, CreatedDateTime) VALUES (SOURCE.PersonId, @IsEraAttributeId, 'True', 0, NEWID(), @CreatedDateTime)
	                WHEN MATCHED AND [Value] != 'True' THEN UPDATE SET Value = 'True';
                    -- Insert missing EraStartDate --
	                MERGE [AttributeValue] AS TARGET
	                USING @TempIsEraFamilyMembers AS SOURCE
	                ON (TARGET.EntityId = SOURCE.PersonId AND TARGET.AttributeId = @EraStartDateAttributeId)
	                WHEN NOT MATCHED BY TARGET
	                THEN INSERT (EntityId, AttributeId, Value, IsSystem, Guid, CreatedDateTime) VALUES (SOURCE.PersonId, @EraStartDateAttributeId, @StartDateValue, 0, NEWID(), @CreatedDateTime)
                    WHEN MATCHED AND ([Value] is null or [Value] = '') THEN UPDATE SET Value = @StartDateValue;
	                SELECT [FamilyId]
                        ,MAX([EntryGiftCountDurationShort]) AS [EntryGiftCountDurationShort]
                        ,MAX([EntryGiftCountDurationLong]) AS [EntryGiftCountDurationLong]
                        ,MAX([ExitGiftCountDuration]) AS [ExitGiftCountDuration]
                        ,MAX([EntryAttendanceCountDuration]) AS [EntryAttendanceCountDuration]
                        ,MAX([ExitAttendanceCountDurationShort]) AS [ExitAttendanceCountDurationShort]
                        ,MAX([ExitAttendanceCountDurationLong]) AS [ExitAttendanceCountDurationLong]
                        ,CAST(MAX([IsEra]) AS BIT) AS [IsEra]
                    FROM (
                        SELECT [Id]
                            ,[IsEra]
                            ,tpfgids.[FamilyId]
                            ,ISNULL(egcds.DistinctCount, 0) AS [EntryGiftCountDurationShort]
                            ,ISNULL(egcd.DistinctCount, 0) AS [ExitGiftCountDuration]
                            ,ISNULL(egcdl.DistinctCount, 0) AS [EntryGiftCountDurationLong]
                            ,ISNULL(eacds.DistinctCount, 0) AS [ExitAttendanceCountDurationShort]
                            ,ISNULL(eacd.DistinctCount, 0) AS [EntryAttendanceCountDuration]
                            ,ISNULL(eacdl.DistinctCount, 0) AS [ExitAttendanceCountDurationLong]
                        FROM @TempPersonFamilyGroupIds tpfgids
		                LEFT JOIN @TempEntryGiftCountDurationShort egcds ON egcds.GivingId = tpfgids.GivingId
		                LEFT JOIN @TempExitGiftCountDuration egcd ON egcd.GivingId = tpfgids.GivingId
		                LEFT JOIN @TempEntryGiftCountDurationLong egcdl ON egcdl.GivingId = tpfgids.GivingId
		                LEFT JOIN @TempExitAttendanceCountDurationShort eacds ON eacds.FamilyId = tpfgids.FamilyId
		                LEFT JOIN @TempEntryAttendanceCountDuration eacd ON eacd.FamilyId = tpfgids.FamilyId
		                LEFT JOIN @TempExitAttendanceCountDurationLong eacdl ON eacdl.FamilyId = tpfgids.FamilyId
                        ) AS t
                    WHERE (
                            ([IsEra] = 1)
                            OR (
                                (
                                    [EntryGiftCountDurationLong] >= @EntryGiftCountDurationLong
                                    AND [EntryGiftCountDurationShort] >= @EntryGiftCountDurationShort
                                    )
                                OR ([EntryAttendanceCountDuration] >= @EntryAttendanceCountDuration)
                                )
                            )
                    GROUP BY [FamilyId]
                END" );
        }

        /// <summary>
        /// KA:Migration to Update spCrm_FamilyAnalyticsGiving.sql to use SundayDate when comparing Transaction date
        /// </summary>
        private void Update_spCrm_FamilyAnalyticsGiving()
        {
            Sql( $@"
                /*
                    <doc>
	                    <summary>
 		                    This stored procedure updates several attributes related to a person's
		                    giving.
	                    </summary>
	
	                    <remarks>	
		                    For eRA we only consider adults for the critieria.
	                    </remarks>
	                    <code>
		                    EXEC [dbo].[spCrm_FamilyAnalyticsGiving] 
	                    </code>
                    </doc>
                    */
                    ALTER PROCEDURE [dbo].[spCrm_FamilyAnalyticsGiving]
	
                    AS
                    BEGIN
	
	                    -- configuration of the duration in weeks
	                    DECLARE @GivingDurationLongWeeks int = 52
	                    DECLARE @GivingDurationShortWeeks int = 6
	
	                    DECLARE @cACTIVE_RECORD_STATUS_VALUE_GUID uniqueidentifier = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
	                    DECLARE @cPERSON_RECORD_TYPE_VALUE_GUID uniqueidentifier = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
	                    DECLARE @cFAMILY_GROUPTYPE_GUID uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'
	                    DECLARE @cADULT_ROLE_GUID uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'	
	                    DECLARE @cCONTRIBUTION_TYPE_VALUE_GUID uniqueidentifier = '2D607262-52D6-4724-910D-5C6E8FB89ACC'
	                    DECLARE @cATTRIBUTE_FIRST_GAVE uniqueidentifier  = 'EE5EC76A-D4B9-56B5-4B48-29627D945F10'
	                    DECLARE @cATTRIBUTE_LAST_GAVE uniqueidentifier  = '02F64263-E290-399E-4487-FC236F4DE81F'
	                    DECLARE @cATTRIBUTE_GIFT_COUNT_SHORT uniqueidentifier  = 'AC11EF53-AE55-79A0-4CAD-43721750E988'
	                    DECLARE @cATTRIBUTE_GIFT_COUNT_LONG uniqueidentifier  = '57700E8F-ED11-D787-415A-04DDF411BB10'
	                    -- --------- END CONFIGURATION --------------
	                    DECLARE @ActiveRecordStatusValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cACTIVE_RECORD_STATUS_VALUE_GUID)
	                    DECLARE @PersonRecordTypeValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cPERSON_RECORD_TYPE_VALUE_GUID)
	                    DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = @cFAMILY_GROUPTYPE_GUID)
	                    DECLARE @AdultRoleId int = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = @cADULT_ROLE_GUID)
	                    DECLARE @ContributionTypeId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cCONTRIBUTION_TYPE_VALUE_GUID)
	
	                    -- calculate dates for queries
	                    DECLARE @SundayDateStart datetime = [dbo].[ufnUtility_GetPreviousSundayDate]()
	                    DECLARE @SundayGivingDurationLong datetime = DATEADD(DAY,  (7 * @GivingDurationLongWeeks * -1), @SundayDateStart)
	                    DECLARE @SundayGivingDurationShort datetime = DATEADD(DAY,  (7 * @GivingDurationShortWeeks * -1), @SundayDateStart);

	                    -- first gift (people w/Giving Group)
	                    DECLARE @FirstGaveAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_FIRST_GAVE)
	                    DELETE FROM [AttributeValue] WHERE [AttributeId] = @FirstGaveAttributeId;
	                    WITH
	                      cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	                      AS
	                      (
		                    SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		                    FROM [Person] p
		                    CROSS APPLY
			                    (
			                    SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			                    FROM    
				                    [GroupMember] gm 
				                    INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				                    INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			                    WHERE 
				                    gm.[PersonId] = p.[Id] 
				                    AND p.[GivingGroupId] IS NOT NULL
				
			                    ) fr
		                    WHERE
			                    [RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			                    AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	                      )
	                    INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime])
	                    SELECT * FROM 
		                    (SELECT 
			                    [PersonId]
			                    , @FirstGaveAttributeId AS [AttributeId]
			                    , (SELECT MIN(ft.TransactionDateTime)
						                    FROM [FinancialTransaction] ft
							                    INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							                    INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							                    INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							                    INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						                    WHERE 
							                    gp.[GivingGroupId] = i.[GivingGroupId]
							                    AND ft.TransactionTypeValueId = @ContributionTypeId) AS [FirstContributionDate]
			                    , 0 AS [IsSystem]
			                    , newid() AS [Guid]
			                    , getdate() AS [CreateDate]
		                    FROM cteIndividual i
		                    WHERE [FamilyRole] = 'Adult') AS g
	                    WHERE g.[FirstContributionDate] IS NOT NULL
	                    -- first gift (people WITHOUT Giving Group)
	                    ;WITH
	                      cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	                      AS
	                      (
		                    SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		                    FROM [Person] p
		                    CROSS APPLY
			                    (
			                    SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			                    FROM    
				                    [GroupMember] gm 
				                    INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				                    INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			                    WHERE 
				                    gm.[PersonId] = p.[Id] 
				                    AND p.[GivingGroupId] IS NULL
				
			                    ) fr
		                    WHERE
			                    [RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			                    AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	                      )
	                    INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime])
	                    SELECT * FROM 
		                    (SELECT 
			                    [PersonId]
			                    , @FirstGaveAttributeId AS [AttributeId]
			                    , (SELECT MIN(ft.TransactionDateTime)
						                    FROM [FinancialTransaction] ft
							                    INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							                    INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							                    INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							                    INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						                    WHERE 
							                    gp.[Id] = i.[PersonId] -- match by person id
							                    AND ft.TransactionTypeValueId = @ContributionTypeId) AS [FirstContributionDate]
			                    , 0 AS [IsSystem]
			                    , newid() AS [Guid]
			                    , getdate() AS [CreateDate]
		                    FROM cteIndividual i
		                    WHERE [FamilyRole] = 'Adult') AS g
	                    WHERE g.[FirstContributionDate] IS NOT NULL
	
	                    -- last gift (people w/Giving Group)
	                    DECLARE @LastGaveAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_LAST_GAVE)
	                    DELETE FROM [AttributeValue] WHERE [AttributeId] = @LastGaveAttributeId;
	                    WITH
	                      cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	                      AS
	                      (
		                    SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		                    FROM [Person] p
		                    CROSS APPLY
			                    (
			                    SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			                    FROM    
				                    [GroupMember] gm 
				                    INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				                    INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			                    WHERE 
				                    gm.[PersonId] = p.[Id] 
				                    AND p.[GivingGroupId] IS NOT NULL
				
			                    ) fr
		                    WHERE
			                    [RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			                    AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	                      )
	                    INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime])
	                    SELECT * FROM 
		                    (SELECT 
			                    [PersonId]
			                    , @LastGaveAttributeId AS [AttributeId]
			                    , (SELECT MAX(ft.TransactionDateTime)
						                    FROM [FinancialTransaction] ft
							                    INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							                    INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							                    INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							                    INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						                    WHERE 
							                    gp.[GivingGroupId] = i.[GivingGroupId]
							                    AND ft.TransactionTypeValueId = @ContributionTypeId) AS [LastContributionDate]
			                    , 0 AS [IsSystem]
			                    , newid() AS [Guid]
			                    , getdate() AS [CreateDate]
		                    FROM cteIndividual i
		                    WHERE [FamilyRole] = 'Adult') AS g
	                    WHERE g.[LastContributionDate] IS NOT NULL
	                    -- last gift (people WITHOUT Giving Group)
	                    ;WITH
	                      cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	                      AS
	                      (
		                    SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		                    FROM [Person] p
		                    CROSS APPLY
			                    (
			                    SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			                    FROM    
				                    [GroupMember] gm 
				                    INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				                    INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			                    WHERE 
				                    gm.[PersonId] = p.[Id] -- match by person id
				                    AND p.[GivingGroupId] IS NULL
				
			                    ) fr
		                    WHERE
			                    [RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			                    AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	                      )
	                    INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime])
	                    SELECT * FROM 
		                    (SELECT 
			                    [PersonId]
			                    , @LastGaveAttributeId AS [AttributeId]
			                    , (SELECT MAX(ft.TransactionDateTime)
						                    FROM [FinancialTransaction] ft
							                    INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							                    INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							                    INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							                    INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						                    WHERE 
							                    gp.[Id] = i.[PersonId]
							                    AND ft.TransactionTypeValueId = @ContributionTypeId) AS [LastContributionDate]
			                    , 0 AS [IsSystem]
			                    , newid() AS [Guid]
			                    , getdate() AS [CreateDate]
		                    FROM cteIndividual i
		                    WHERE [FamilyRole] = 'Adult') AS g
	                    WHERE g.[LastContributionDate] IS NOT NULL
	                    -- number of gifts short duration (people w/Giving Group)
	                    DECLARE @GiftCountShortAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_GIFT_COUNT_SHORT)
	                    DELETE FROM [AttributeValue] WHERE [AttributeId] = @GiftCountShortAttributeId;
	                    WITH
	                      cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	                      AS
	                      (
		                    SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		                    FROM [Person] p
		                    CROSS APPLY
			                    (
			                    SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			                    FROM    
				                    [GroupMember] gm 
				                    INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				                    INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			                    WHERE 
				                    gm.[PersonId] = p.[Id] 
				                    AND p.[GivingGroupId] IS NOT NULL
				
			                    ) fr
		                    WHERE
			                    [RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			                    AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	                      )
	                    INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsNumeric])
	                    SELECT [PersonId], [AttributeId], [GiftCountDurationShort], [IsSystem], [Guid], [CreateDate], [GiftCountDurationShort]
                        FROM 
		                    (SELECT 
			                    [PersonId]
			                    , @GiftCountShortAttributeId AS [AttributeId]
			                    , (SELECT COUNT(DISTINCT(ft.[Id])) 
						                    FROM [FinancialTransaction] ft
							                    INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							                    INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							                    INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							                    INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						                    WHERE 
							                    gp.[GivingGroupId] = i.[GivingGroupId]
							                    AND ft.TransactionTypeValueId = @ContributionTypeId
							                    AND ft.TransactionDateTime >= @SundayGivingDurationShort
							                    AND ft.SundayDate <= @SundayDateStart) AS [GiftCountDurationShort]
			                    , 0 AS [IsSystem]
			                    , newid() AS [Guid]
			                    , getdate() AS [CreateDate]
		                    FROM cteIndividual i
		                    WHERE [FamilyRole] = 'Adult') AS g
	                    WHERE g.[GiftCountDurationShort] IS NOT NULL
	                    -- number of gifts short duration (people WITHOUT Giving Group)
	                    ;WITH
	                      cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	                      AS
	                      (
		                    SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		                    FROM [Person] p
		                    CROSS APPLY
			                    (
			                    SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			                    FROM    
				                    [GroupMember] gm 
				                    INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				                    INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			                    WHERE 
				                    gm.[PersonId] = p.[Id] 
				                    AND p.[GivingGroupId] IS NULL
				
			                    ) fr
		                    WHERE
			                    [RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			                    AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	                      )
	                    INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsNumeric])
	                    SELECT [PersonId], [AttributeId], [GiftCountDurationShort], [IsSystem], [Guid], [CreateDate], [GiftCountDurationShort]
                        FROM 
		                    (SELECT 
			                    [PersonId]
			                    , @GiftCountShortAttributeId AS [AttributeId]
			                    , (SELECT COUNT(DISTINCT(ft.[Id])) 
						                    FROM [FinancialTransaction] ft
							                    INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							                    INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							                    INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							                    INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						                    WHERE 
							                    gp.[Id] = i.[PersonId] -- match by person id
							                    AND ft.TransactionTypeValueId = @ContributionTypeId
							                    AND ft.TransactionDateTime >= @SundayGivingDurationShort
							                    AND ft.SundayDate <= @SundayDateStart) AS [GiftCountDurationShort]
			                    , 0 AS [IsSystem]
			                    , newid() AS [Guid]
			                    , getdate() AS [CreateDate]
		                    FROM cteIndividual i
		                    WHERE [FamilyRole] = 'Adult') AS g
	                    WHERE g.[GiftCountDurationShort] IS NOT NULL
	                    -- number of gifts long duration (people w/Giving Group)
	                    DECLARE @GiftCountLongAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_GIFT_COUNT_LONG)
	                    DELETE FROM [AttributeValue] WHERE [AttributeId] = @GiftCountLongAttributeId;
	                    WITH
	                      cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	                      AS
	                      (
		                    SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		                    FROM [Person] p
		                    CROSS APPLY
			                    (
			                    SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			                    FROM    
				                    [GroupMember] gm 
				                    INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				                    INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			                    WHERE 
				                    gm.[PersonId] = p.[Id] 
				                    AND p.[GivingGroupId] IS NOT NULL
				
			                    ) fr
		                    WHERE
			                    [RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			                    AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	                      )
	                    INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsNumeric])
	                    SELECT [PersonId], [AttributeId], [GiftCountDurationLong], [IsSystem], [Guid], [CreateDate], [GiftCountDurationLong]
                        FROM 
		                    (SELECT 
			                    [PersonId]
			                    , @GiftCountLongAttributeId AS [AttributeId]
			                    , (SELECT COUNT(DISTINCT(ft.[Id])) 
						                    FROM [FinancialTransaction] ft
							                    INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							                    INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							                    INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							                    INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						                    WHERE 
							                    gp.[GivingGroupId] = i.[GivingGroupId]
							                    AND ft.TransactionTypeValueId = @ContributionTypeId
							                    AND ft.TransactionDateTime >= @SundayGivingDurationLong
							                    AND ft.SundayDate <= @SundayDateStart) AS [GiftCountDurationLong]
			                    , 0 AS [IsSystem]
			                    , newid() AS [Guid]
			                    , getdate() AS [CreateDate]
		                    FROM cteIndividual i
		                    WHERE [FamilyRole] = 'Adult') AS g
	                    WHERE g.[GiftCountDurationLong] IS NOT NULL
	
	                    -- number of gifts long duration (people WITHOUT Giving Group)
	                    ;WITH
	                      cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	                      AS
	                      (
		                    SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		                    FROM [Person] p
		                    CROSS APPLY
			                    (
			                    SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			                    FROM    
				                    [GroupMember] gm 
				                    INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				                    INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			                    WHERE 
				                    gm.[PersonId] = p.[Id] 
				                    AND p.[GivingGroupId] IS NULL
				
			                    ) fr
		                    WHERE
			                    [RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			                    AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	                      )
	                    INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsNumeric])
	                    SELECT [PersonId], [AttributeId], [GiftCountDurationLong], [IsSystem], [Guid], [CreateDate], [GiftCountDurationLong]
                        FROM 
		                    (SELECT 
			                    [PersonId]
			                    , @GiftCountLongAttributeId AS [AttributeId]
			                    , (SELECT COUNT(DISTINCT(ft.[Id])) 
						                    FROM [FinancialTransaction] ft
							                    INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							                    INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							                    INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							                    INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						                    WHERE 
							                    gp.[Id] = i.[PersonId] -- match by person id
							                    AND ft.TransactionTypeValueId = @ContributionTypeId
							                    AND ft.TransactionDateTime >= @SundayGivingDurationLong
							                    AND ft.SundayDate <= @SundayDateStart) AS [GiftCountDurationLong]
			                    , 0 AS [IsSystem]
			                    , newid() AS [Guid]
			                    , getdate() AS [CreateDate]
		                    FROM cteIndividual i
		                    WHERE [FamilyRole] = 'Adult') AS g
	                    WHERE g.[GiftCountDurationLong] IS NOT NULL
	
                    END" );
        }

        /// <summary>
        /// CR: (Migration) Group Type Participation Attribute, Fundraising Entry Header, and GroupMemberReq ManuallyCompleted Data Update
        /// </summary>
        public void GroupRequirementChangesForFundraisingOpportunitiesUp()
        {
            // Get the GroupTypeId for Fundraising Opportunity.
            var groupReq_GroupTypeIdSql = $"SELECT TOP 1 [Id] FROM GroupType WHERE [Guid] = '{SystemGuid.GroupType.GROUPTYPE_FUNDRAISINGOPPORTUNITY}'";
            var groupReq_GroupTypeId = SqlScalar( groupReq_GroupTypeIdSql ).ToIntSafe();
            if ( groupReq_GroupTypeId > 0 )
            {
                RockMigrationHelper.AddOrUpdateEntityAttribute(
                    "Rock.Model.Group", SystemGuid.FieldType.SINGLE_SELECT, "GroupTypeId",
                    groupReq_GroupTypeId.ToString(),
                    "Participation Type",
                    "Participation Type",
                    @"The type of participation in this group.", 18, @"1", SystemGuid.Attribute.PARTICIPATION_TYPE, "ParticipationType" );
                // Qualifier for attribute: ParticipationType
                RockMigrationHelper.UpdateAttributeQualifier( SystemGuid.Attribute.PARTICIPATION_TYPE, "values", @"1^Individual,2^Family", "4B50E240-74D5-4953-B9B3-99DC4EEB5C84" );
                // Qualifier for attribute: ParticipationType
                RockMigrationHelper.UpdateAttributeQualifier( SystemGuid.Attribute.PARTICIPATION_TYPE, "fieldtype", @"ddl", "93A56DAE-2672-43F4-8AC9-C144B0AB84B3" );
                // Qualifier for attribute: ParticipationType
                RockMigrationHelper.UpdateAttributeQualifier( SystemGuid.Attribute.PARTICIPATION_TYPE, "repeatColumns", @"", "04D5573C-4671-45B7-A591-13EC6EA0FF99" );
            }

            // Update the Fundraising Entry Transaction Header
            string oldAssignParticipantReference;
            string newAssignParticipantReference;
            string oldParticipantReference;
            string newParticipantReference;

            // This is for the Participant Lava.
            oldAssignParticipantReference = @"{% assign groupMember = TransactionEntity %}
{% assign fundraisingGoal = groupMember | Attribute:''IndividualFundraisingGoal'',''RawValue'' %}
{% if fundraisingGoal == '''' %}
  {% assign fundraisingGoal = groupMember.Group | Attribute:''IndividualFundraisingGoal'',''RawValue'' %}
{% endif %}
{% comment %}
-- convert fundraisingGoal to a numeric by using Plus
{% endcomment %}
{% assign fundraisingGoal = fundraisingGoal | Plus:0.00 %}
{% assign amountRemaining = fundraisingGoal | Minus:TransactionEntityTransactionsTotal %}";

            newAssignParticipantReference = @"{% assign groupMember = TransactionEntity %}
{% assign fundraisingGoal = FundraisingGoal %}
{% assign amountRaised = AmountRaised %}
{% assign participationType = PageParameter[''ParticipationMode''] %}
{% comment %}
-- convert fundraisingGoal to a numeric by using Plus
{% endcomment %}
{% assign fundraisingGoal = fundraisingGoal | Plus:0.00 %}
{% assign amountRemaining = fundraisingGoal | Minus:AmountRaised %}";

            Sql( $@"
                    UPDATE [AttributeValue]
                    SET [Value] = REPLACE([Value],'{oldAssignParticipantReference}','{newAssignParticipantReference}')
                    WHERE [Id] IN (SELECT av.[Id]
                            FROM [dbo].[AttributeValue] av
                            INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
                            WHERE a.[Guid] = '{SystemGuid.Attribute.FUNDRAISING_TRANSACTION_HEADER}')
                " );

            oldParticipantReference = "{{ groupMember.Person.FullName }}";
            newParticipantReference = @"
                 {% if participationType == ''2'' %}
                   {{ groupMember.Person.PrimaryFamily.Name }}
                 {% else %}
                   {{ groupMember.Person.FullName }}
                 {% endif  %}
                    ";
            Sql( $@"
                    UPDATE [AttributeValue]
                    SET [Value] = REPLACE([Value],'{oldParticipantReference}','{newParticipantReference}')
                    WHERE [Id] IN (SELECT av.[Id]
                            FROM [dbo].[AttributeValue] av
                            INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
                            WHERE a.[Guid] = '{SystemGuid.Attribute.FUNDRAISING_TRANSACTION_HEADER}')
                " );

            // This updates all group member requirements that have
            // 1. requirements met (completed) per RequirementMetDateTime
            // and 2. the group member requirement's requirement type has a RequirementCheckType value of 2.
            // The Rock.Model.RequirementCheckType.Manual enum value is '2'.
            // There is no "down" for this change, as it is populating a newly added property to the Group Member Requirement model.
            Sql( $@"UPDATE gmr
                    SET gmr.[WasManuallyCompleted] = 1, gmr.[ManuallyCompletedDateTime] = gmr.[RequirementMetDateTime]
                        FROM [dbo].[GroupRequirementType] grt INNER JOIN [dbo].[GroupRequirement] gr ON grt.[Id] = gr.[GroupRequirementTypeId] 
                        INNER JOIN [dbo].[GroupMemberRequirement] gmr ON gr.[Id] = gmr.[GroupRequirementId]
                        WHERE (gmr.[RequirementMetDateTime] IS NOT NULL) AND (grt.[RequirementCheckType] = 2)" );
        }

        /// <summary>
        /// CR: (Migration) Group Type Participation Attribute, Fundraising Entry Header, and GroupMemberReq ManuallyCompleted Data Update 
        /// </summary>
        public void GroupRequirementChangesForFundraisingOpportunitiesDown()
        {
            string oldAssignParticipantReference;
            string newAssignParticipantReference;
            string oldParticipantReference;
            string newParticipantReference;
            // Reverse the changes that were made.
            oldAssignParticipantReference = @"{% assign groupMember = TransactionEntity %}
{% assign fundraisingGoal = groupMember | Attribute:''IndividualFundraisingGoal'',''RawValue'' %}
{% if fundraisingGoal == '''' %}
  {% assign fundraisingGoal = groupMember.Group | Attribute:''IndividualFundraisingGoal'',''RawValue'' %}
{% endif %}
{% comment %}
-- convert fundraisingGoal to a numeric by using Plus
{% endcomment %}
{% assign fundraisingGoal = fundraisingGoal | Plus:0.00 %}
{% assign amountRemaining = fundraisingGoal | Minus:TransactionEntityTransactionsTotal %}";
            newAssignParticipantReference = @"{% assign groupMember = TransactionEntity %}
{% assign fundraisingGoal = FundraisingGoal %}
{% assign amountRaised = AmountRaised %}
{% assign participationType = PageParameter[''ParticipationMode''] %}
{% comment %}
-- convert fundraisingGoal to a numeric by using Plus
{% endcomment %}
{% assign fundraisingGoal = fundraisingGoal | Plus:0.00 %}
{% assign amountRemaining = fundraisingGoal | Minus:AmountRaised %}";

            Sql( $@"
                    UPDATE [AttributeValue]
                    SET [Value] = REPLACE([Value],'{newAssignParticipantReference }','{oldAssignParticipantReference}')
                    WHERE [Id] IN (SELECT av.[Id]
                            FROM [dbo].[AttributeValue] av
                            INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
                            WHERE a.[Guid] = '{SystemGuid.Attribute.FUNDRAISING_TRANSACTION_HEADER}')
                " );

            oldParticipantReference = "{{ groupMember.Person.FullName }}";
            newParticipantReference = @"
                 {% if participationType == ''2'' %}
                   {{ groupMember.Person.PrimaryFamily.Name }}
                 {% else %}
                   {{ groupMember.Person.FullName }}
                 {% endif  %}
                    ";
            Sql( $@"
                    UPDATE [AttributeValue]
                    SET [Value] = REPLACE([Value],'{newParticipantReference}','{oldParticipantReference}')
                    WHERE [Id] IN (SELECT av.[Id]
                            FROM [dbo].[AttributeValue] av
                            INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
                            WHERE a.[Guid] = '{SystemGuid.Attribute.FUNDRAISING_TRANSACTION_HEADER}')
                " );
            // Rock.Model.Group: Participation Type.
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.PARTICIPATION_TYPE );
        }

        /// <summary>
        /// DH: Fix MediaInteractionChannelEntityTypeId
        /// </summary>
        public void FixMediaInteractionChannelEntityTypeIdUp()
        {
            // Ensure the entity type identifier exists for use in the query below.
            RockMigrationHelper.UpdateEntityType(
                "Rock.Model.MediaElement",
                "Media Element",
                "Rock.Model.MediaElement, Rock, Version=1.14.1.1, Culture=neutral, PublicKeyToken=null",
                true,
                true,
                "F4506B5D-F22C-4D3F-8205-FE48A9B7584B" );
            Sql( @"
                DECLARE @ChannelGuid UNIQUEIDENTIFIER = 'D5B9BDAF-6E52-40D5-8E74-4E23973DF159'
                DECLARE @PageEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Page')
                DECLARE @MediaElementEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.MediaElement')
                IF NOT EXISTS( SELECT * FROM [InteractionChannel] WHERE [Guid] = @ChannelGuid )
                BEGIN
                    DECLARE @ChannelTypeMediumValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '5919214F-9C59-4913-BE4E-0DFB6A05F528')
                    INSERT INTO [InteractionChannel]
	                    ([Name], [ComponentEntityTypeId], [InteractionEntityTypeId], [ChannelTypeMediumValueId], [Guid], [UsesSession], [IsActive])
	                    VALUES ('Media Events', @MediaElementEntityTypeId, @PageEntityTypeId, @ChannelTypeMediumValueId, @ChannelGuid, 0, 1)
                END
                ELSE
                BEGIN
                    UPDATE [InteractionChannel]
                    SET [ComponentEntityTypeId] = @MediaElementEntityTypeId,
                        [InteractionEntityTypeId] = @PageEntityTypeId
                    WHERE [Guid] = @ChannelGuid
                END" );
        }

        /// <summary>
        /// GJ: Add gridlinecolor property to chart shortcode
        /// </summary>
        public void AddChartGridlinecolorProperty()
        {
            Sql( MigrationSQL._202209291606147_Rollup_20220929_AddChartGridlinecolorProperty );
        }
    
        /// <summary>
        /// MP: Update Statement Generator Installer Link
        /// Statement Generator Download Location - Updates the statement generator download link up.
        /// </summary>
        private void UpdateStatementGeneratorDownloadLinkUp()
        {
	        Sql( @"
		        DECLARE @statementGeneratorDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = '54E1EBCC-5A5A-4B26-9CCB-36E7CEB49C3C')
		        DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')
		        UPDATE [AttributeValue]
		        SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.14.1/statementgenerator.msi'
		        WHERE AttributeId = @downloadUrlAttributeId and EntityId = @statementGeneratorDefinedValueId" );
        }

        /// <summary>
        /// MP: Update Statement Generator Installer Link
        /// Statement Generator Download Location - Updates the statement generator download link down.
        /// </summary>
        private void UpdateStatementGeneratorDownloadLinkDown()
        {
	        Sql( @"
		        DECLARE @statementGeneratorDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = '54E1EBCC-5A5A-4B26-9CCB-36E7CEB49C3C')
		        DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')
		        UPDATE [AttributeValue]
		        SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.13.5/statementgenerator.msi'
		        WHERE AttributeId = @downloadUrlAttributeId and EntityId = @statementGeneratorDefinedValueId" );
        }
    
        /// <summary>
        /// NA: Show the Apple TV Apps Page
        /// </summary>
        private void ShowAppleTVAppsPage()
        {
            Sql( @"
                UPDATE [Page]
                SET [DisplayInNavWhen] = 0
                WHERE [Guid] = 'C8B81EBE-E98F-43EF-9E39-0491685145E2'" );
        }
    }
}
