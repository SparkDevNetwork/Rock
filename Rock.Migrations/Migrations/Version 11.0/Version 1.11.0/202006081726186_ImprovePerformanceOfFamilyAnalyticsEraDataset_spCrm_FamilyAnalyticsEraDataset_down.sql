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
       )

    INSERT INTO @TempFinancialTransactionByDateAndGivingId
    SELECT COUNT(DISTINCT (ft.[Id])) [DistinctCount]
        ,g1.GivingId
        ,ft.TransactionDateTime [TransactionDateTime]
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

    SELECT [FamilyId]
        ,MAX([EntryGiftCountDurationShort]) AS [EntryGiftCountDurationShort]
        ,MAX([EntryGiftCountDurationLong]) AS [EntryGiftCountDurationLong]
        ,MAX([ExitGiftCountDuration]) AS [ExitGiftCountDuration]
        ,MAX([EntryAttendanceCountDuration]) AS [EntryAttendanceCountDuration]
        ,MAX([ExitAttendanceCountDurationShort]) AS [ExitAttendanceCountDurationShort]
        ,MAX([ExitAttendanceCountDurationLong]) AS [ExitAttendanceCountDurationLong]
        ,CAST(MAX([IsEra]) AS BIT) AS [IsEra]
    FROM (
        SELECT p.[Id]
            ,CASE 
                WHEN (era.[Value] = 'true')
                    THEN 1
                ELSE 0
                END AS [IsEra]
            ,g.[Id] AS [FamilyId]
            ,(
                SELECT ISNULL(SUM(ft.DistinctCount), 0)
                FROM @TempFinancialTransactionByDateAndGivingId ft
                WHERE ft.TransactionDateTime >= @SundayEntryGivingDurationShort
                    AND ft.TransactionDateTime <= @SundayDateStart
                    AND ft.GivingId = p.GivingId
                ) AS [EntryGiftCountDurationShort]
            ,(
                SELECT ISNULL(SUM(ft.DistinctCount), 0)
                FROM @TempFinancialTransactionByDateAndGivingId ft
                WHERE ft.TransactionDateTime >= @SundayExitGivingDuration
                    AND ft.TransactionDateTime <= @SundayDateStart
                    AND ft.GivingId = p.GivingId
                ) AS [ExitGiftCountDuration]
            ,(
                SELECT ISNULL(SUM(ft.DistinctCount), 0)
                FROM @TempFinancialTransactionByDateAndGivingId ft
                WHERE ft.TransactionDateTime >= @SundayEntryGivingDurationLong
                    AND ft.TransactionDateTime <= @SundayDateStart
                    AND ft.GivingId = p.GivingId
                ) AS [EntryGiftCountDurationLong]
            ,(
                SELECT COUNT(DISTINCT a.SundayDate)
                FROM @TempAttendanceBySundayDateAndFamily a
                WHERE a.FamilyId = g.Id
                    AND a.StartDateTime <= @SundayDateStart
                    AND a.StartDateTime >= @SundayExitAttendanceDurationShort
                ) AS [ExitAttendanceCountDurationShort]
            ,(
                SELECT COUNT(DISTINCT a.SundayDate)
                FROM @TempAttendanceBySundayDateAndFamily a
                WHERE a.FamilyId = g.Id
                    AND a.StartDateTime <= @SundayDateStart
                    AND a.StartDateTime >= @SundayEntryAttendanceDuration
                ) AS [EntryAttendanceCountDuration]
            ,(
                SELECT COUNT(DISTINCT a.SundayDate)
                FROM @TempAttendanceBySundayDateAndFamily a
                WHERE a.FamilyId = g.Id
                    AND a.StartDateTime <= @SundayDateStart
                    AND a.StartDateTime >= @SundayExitAttendanceDurationLong
                ) AS [ExitAttendanceCountDurationLong]
        FROM [Person] p
        INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
            AND gm.[GroupRoleId] = @AdultRoleId
        INNER JOIN [Group] g ON g.[Id] = gm.[GroupId]
            AND g.[GroupTypeId] = @FamilyGroupTypeId
        LEFT OUTER JOIN [AttributeValue] era ON era.[EntityId] = p.[Id]
            AND era.[AttributeId] = @IsEraAttributeId
        WHERE [RecordTypeValueId] = @PersonRecordTypeValueId -- person record type (not business)
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
END