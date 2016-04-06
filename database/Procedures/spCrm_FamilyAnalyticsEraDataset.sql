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
		For eRA we only consider adults for the critieria.
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
	DECLARE @EntryGivingDurationLongWeeks int = 52
	DECLARE @EntryGivingDurationShortWeeks int = 6
	DECLARE @EntryAttendanceDurationWeeks int = 16
	DECLARE @ExitGivingDurationWeeks int = 8
	DECLARE @ExitAttendanceDurationShortWeeks int = 4
	DECLARE @ExitAttendanceDurationLongWeeks int = 16

	-- configuration of the item counts in the durations
	DECLARE @EntryGiftCountDurationLong int = 4
	DECLARE @EntryGiftCountDurationShort int = 1
	DECLARE @EntryAttendanceCountDuration int = 8
	
	DECLARE @cACTIVE_RECORD_STATUS_VALUE_GUID uniqueidentifier = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
	DECLARE @cPERSON_RECORD_TYPE_VALUE_GUID uniqueidentifier = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
	DECLARE @cATTRIBUTE_IS_ERA_GUID uniqueidentifier = 'CE5739C5-2156-E2AB-48E5-1337C38B935E'
	DECLARE @cFAMILY_GROUPTYPE_GUID uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'
	DECLARE @cADULT_ROLE_GUID uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'

	-- --------- END CONFIGURATION --------------

	DECLARE @ActiveRecordStatusValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cACTIVE_RECORD_STATUS_VALUE_GUID)
	DECLARE @PersonRecordTypeValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cPERSON_RECORD_TYPE_VALUE_GUID)
	DECLARE @IsEraAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_IS_ERA_GUID)
	DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = @cFAMILY_GROUPTYPE_GUID)
	DECLARE @AdultRoleId int = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = @cADULT_ROLE_GUID)

	-- calculate dates for query
	DECLARE @SundayDateStart datetime = [dbo].[ufnUtility_GetPreviousSundayDate]()
	DECLARE @SundayEntryGivingDurationLong datetime = DATEADD(DAY,  (7 * @EntryGivingDurationLongWeeks * -1), @SundayDateStart)
	DECLARE @SundayEntryGivingDurationShort datetime = DATEADD(DAY,  (7 * @EntryGivingDurationShortWeeks * -1), @SundayDateStart)
	DECLARE @SundayEntryAttendanceDuration datetime = DATEADD(DAY,  (7 * @EntryAttendanceDurationWeeks * -1), @SundayDateStart)

	DECLARE @SundayExitGivingDuration datetime = DATEADD(DAY, (7 * @ExitGivingDurationWeeks * -1), @SundayDateStart)
	DECLARE @SundayExitAttendanceDurationShort datetime = DATEADD(DAY,  (7 * @ExitAttendanceDurationShortWeeks * -1), @SundayDateStart)
	DECLARE @SundayExitAttendanceDurationLong datetime = DATEADD(DAY,  (7 * @ExitAttendanceDurationLongWeeks * -1), @SundayDateStart)
	

	SELECT
		[FamilyId]
		, MAX([EntryGiftCountDurationShort]) AS [EntryGiftCountDurationShort]
		, MAX([EntryGiftCountDurationLong]) AS [EntryGiftCountDurationLong]
		, MAX([ExitGiftCountDuration]) AS [ExitGiftCountDuration]
		, MAX([EntryAttendanceCountDuration]) AS [EntryAttendanceCountDuration]
		, MAX([ExitAttendanceCountDurationShort]) AS [ExitAttendanceCountDurationShort]
		, MAX([ExitAttendanceCountDurationLong]) AS [ExitAttendanceCountDurationLong]
		, CAST(MAX([IsEra]) AS BIT) AS [IsEra]
	FROM (
		SELECT 
			p.[Id]
			, CASE WHEN (era.[Value] = 'true') THEN 1  ELSE 0 END AS [IsEra]
			, g.[Id] AS [FamilyId]
			, (SELECT COUNT(DISTINCT(ft.[Id])) 
					FROM [FinancialTransaction] ft
						INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
						INNER JOIN [Person] g1 ON g1.[Id] = pa.[PersonId]
						INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
						INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
					WHERE 
						g1.[GivingGroupId] = p.[GivingGroupId]
						AND fa.[IsTaxDeductible] = 'true'
						AND ft.TransactionDateTime >= @SundayEntryGivingDurationShort
						AND ft.TransactionDateTime <= @SundayDateStart) AS [EntryGiftCountDurationShort]
			, (SELECT COUNT(DISTINCT(ft.[Id])) 
					FROM [FinancialTransaction] ft
						INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
						INNER JOIN [Person] g2 ON g2.[Id] = pa.[PersonId]
						INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
						INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
					WHERE 
						g2.[GivingGroupId] = p.[GivingGroupId]
						AND fa.[IsTaxDeductible] = 'true'
						AND ft.TransactionDateTime >= @SundayExitGivingDuration
						AND ft.TransactionDateTime <= @SundayDateStart) AS [ExitGiftCountDuration]	
			, (SELECT COUNT(DISTINCT(ft.[Id])) 
					FROM [FinancialTransaction] ft
						INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
						INNER JOIN [Person] g2 ON g2.[Id] = pa.[PersonId]
						INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
						INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
					WHERE 
						g2.[GivingGroupId] = p.[GivingGroupId]
						AND fa.[IsTaxDeductible] = 'true'
						AND ft.TransactionDateTime >= @SundayEntryGivingDurationLong
						AND ft.TransactionDateTime <= @SundayDateStart) AS [EntryGiftCountDurationLong]	
			, (SELECT 
					COUNT(DISTINCT a.SundayDate )
				FROM
					[Attendance] a
					INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
				WHERE 
					[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
					AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](p.[Id]))
					AND a.[StartDateTime] <= @SundayDateStart AND a.[StartDateTime] >= @SundayExitAttendanceDurationShort) AS [ExitAttendanceCountDurationShort]
			, (SELECT 
					COUNT(DISTINCT a.SundayDate )
				FROM
					[Attendance] a
					INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
				WHERE 
					[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
					AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](p.[Id]))
					AND a.[StartDateTime] <= @SundayDateStart AND a.[StartDateTime] >= @SundayEntryAttendanceDuration) AS [EntryAttendanceCountDuration]
			, (SELECT 
					COUNT(DISTINCT a.SundayDate )
				FROM
					[Attendance] a
					INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
				WHERE 
					[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
					AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](p.[Id]))
					AND a.[StartDateTime] <= @SundayDateStart AND a.[StartDateTime] >= @SundayExitAttendanceDurationLong) AS [ExitAttendanceCountDurationLong]	
		FROM
			[Person] p
			INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id] AND gm.[GroupRoleId] = @AdultRoleId
			INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
			LEFT OUTER JOIN [AttributeValue] era ON era.[EntityId] = p.[Id] AND era.[AttributeId] = @IsEraAttributeId
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
		) AS t
		WHERE (
			([IsEra] = 1)
			OR (
				( [EntryGiftCountDurationLong] >= @EntryGiftCountDurationLong AND [EntryGiftCountDurationShort] >= @EntryGiftCountDurationShort )
				OR
				( [EntryAttendanceCountDuration] >= @EntryAttendanceCountDuration )
			)
		)
		GROUP BY [FamilyId]
	
END