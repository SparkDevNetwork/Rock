/*
<doc>
	<summary>
 		This stored procedure updates several attributes related to a person's
		attendance.
	</summary>
	
	<remarks>	
		For eRA we only consider adults for the critieria.
	</remarks>
	<code>
		EXEC [dbo].[spCrm_FamilyAnalyticsAttendance] 
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCrm_FamilyAnalyticsAttendance]

AS
BEGIN
	
	-- configuration of the duration in weeks
	DECLARE @EntryAttendanceDurationWeeks int = 16
		
	DECLARE @cACTIVE_RECORD_STATUS_VALUE_GUID uniqueidentifier = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
	DECLARE @cPERSON_RECORD_TYPE_VALUE_GUID uniqueidentifier = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
	DECLARE @cATTRIBUTE_IS_ERA_GUID uniqueidentifier = 'CE5739C5-2156-E2AB-48E5-1337C38B935E'
	DECLARE @cFAMILY_GROUPTYPE_GUID uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'
	DECLARE @cADULT_ROLE_GUID uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'
	DECLARE @cCHILD_ROLE_GUID uniqueidentifier = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9'

	DECLARE @cATTRIBUTE_FIRST_ATTENDED uniqueidentifier  = 'AB12B3B0-55B8-D6A5-4C1F-DB9CCB2C4342'
	DECLARE @cATTRIBUTE_LAST_ATTENDED uniqueidentifier  = '5F4C6462-018E-D19C-4AB0-9843CB21C57E'
	DECLARE @cATTRIBUTE_TIMES_ATTENDED_IN_DURATION uniqueidentifier  = '45A1E978-DC5B-CFA1-4AF4-EA098A24C914'
	DECLARE @Now DATETIME = dbo.RockGetDate()

	-- --------- END CONFIGURATION --------------

	DECLARE @ActiveRecordStatusValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cACTIVE_RECORD_STATUS_VALUE_GUID)
	DECLARE @PersonRecordTypeValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cPERSON_RECORD_TYPE_VALUE_GUID)
	DECLARE @IsEraAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_IS_ERA_GUID)
	DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = @cFAMILY_GROUPTYPE_GUID)
	DECLARE @AdultRoleId int = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = @cADULT_ROLE_GUID)
	DECLARE @ChildRoleId int = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = @cCHILD_ROLE_GUID)

	-- calculate dates for query
	DECLARE @SundayDateStart datetime = [dbo].[ufnUtility_GetPreviousSundayDate]()
	DECLARE @SundayEntryAttendanceDuration datetime = DATEADD(DAY,  (7 * @EntryAttendanceDurationWeeks * -1), @SundayDateStart)
	

	-- first checkin
	DECLARE @FirstAttendedAttributeId int = (SELECT TOP 1 [Id] FROM dbo.[Attribute] WHERE [Guid] = @cATTRIBUTE_FIRST_ATTENDED);

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
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	  )
	SELECT [PersonId], [FirstAttendedDate], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
	INTO #firstAttended
	FROM 
		(SELECT 
			i.[PersonId]
			, CASE WHEN [FamilyRole] = 'Adult' THEN 
					(SELECT 
						MIN(A.[StartDateTime] )
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](i.[PersonId])))
				ELSE
					(SELECT 
						MIN(a.[StartDateTime] )
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] = i.[PersonId])
			  END AS [FirstAttendedDate]
		FROM cteIndividual i ) AS a
	LEFT JOIN [AttributeValue] av ON av.[EntityId] = a.[PersonId]
	AND av.[AttributeId] = @FirstAttendedAttributeId
	WHERE a.[FirstAttendedDate] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[FirstAttendedDate],
		av.[ValueAsDateTime] = f.[FirstAttendedDate],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #firstAttended f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[FirstAttendedDate]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsDateTime]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @FirstAttendedAttributeId
		, f.[FirstAttendedDate]
		, 0
		, newid()
		, @Now
		, f.[FirstAttendedDate]
		, 1
	FROM #firstAttended f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #firstAttended

	-- last checkin
	DECLARE @LastAttendedAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_LAST_ATTENDED);

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
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	  )
	SELECT [PersonId], [LastAttendedDate], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
	INTO #lastAttended
	FROM 
		(SELECT 
			i.[PersonId]
			, CASE WHEN [FamilyRole] = 'Adult' THEN 
					(SELECT 
						MAX(a.StartDateTime )
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](i.[PersonId])))
				ELSE
					(SELECT 
						MAX(a.StartDateTime )
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] = i.[PersonId])
			  END AS [LastAttendedDate]
		FROM cteIndividual i ) AS a
	LEFT JOIN dbo.[AttributeValue] av ON av.[EntityId] = a.[PersonId] 
	AND av.[AttributeId] = @LastAttendedAttributeId
	WHERE a.[LastAttendedDate] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[LastAttendedDate],
		av.[ValueAsDateTime] = f.[LastAttendedDate],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #lastAttended f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[LastAttendedDate]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsDateTime]
        , [IsPersistedValueDirty])
	SELECT f.PersonId
		, @LastAttendedAttributeId
		, f.[LastAttendedDate]
		, 0
		, newid()
		, @Now
		, f.[LastAttendedDate]
		, 1
	FROM #lastAttended f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #lastAttended

	-- times checkedin
	DECLARE @TimesAttendedAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_TIMES_ATTENDED_IN_DURATION)
	DELETE FROM [AttributeValue] WHERE [AttributeId] = @TimesAttendedAttributeId;

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
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	  )
	SELECT [PersonId], [CheckinCount], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
	INTO #checkInCount
	FROM 
		(SELECT 
			i.[PersonId]
			, CASE WHEN [FamilyRole] = 'Adult' THEN 
					(SELECT 
						COUNT(*)
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
						AND CAST( a.[StartDateTime] AS DATE ) <= @SundayDateStart AND a.[StartDateTime] >= @SundayEntryAttendanceDuration
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](i.[PersonId])))
				ELSE
					(SELECT 
						COUNT(*)
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
						AND CAST( a.[StartDateTime] AS DATE ) <= @SundayDateStart AND a.[StartDateTime] >= @SundayEntryAttendanceDuration
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] = i.[PersonId])
			  END AS [CheckinCount]
		FROM cteIndividual i ) AS a
	LEFT JOIN dbo.[AttributeValue] av ON av.[EntityId] = a.[PersonId]
	AND av.[AttributeId] = @TimesAttendedAttributeId
	WHERE a.[CheckinCount] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[CheckinCount],
		av.[ValueAsNumeric] = f.[CheckinCount],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #checkInCount f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[CheckinCount]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsNumeric]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @TimesAttendedAttributeId
		, f.[CheckinCount]
		, 0
		, newid()
		, @Now
		, f.[CheckinCount]
		, 1
	FROM #checkInCount f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #checkInCount
	
END