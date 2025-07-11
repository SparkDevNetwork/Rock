﻿// <copyright>
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

    using Rock.Plugin.HotFixes;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20250603 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Update_spCrm_FamilyAnalyticsAttendance_Up();
            AddDuplicateCommunicationRecipientHandlingUp();
            ReduceDefaultSizeRockLoggerMaxFileSizeAndNumberOfLogFilesUp();
            UpdateAppleDeviceListUp();
            UpdateStatusTemplateAttributeUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Update_spCrm_FamilyAnalyticsAttendance_Down();
        }

        #region SC: Fix_spCrm_FamilyAnalyticsAttendance Plugin Migration #249

        private void Update_spCrm_FamilyAnalyticsAttendance_Up()
        {
            Sql( @"
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
	DECLARE @TimesAttendedAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_TIMES_ATTENDED_IN_DURATION);

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
" );
        }

        private void Update_spCrm_FamilyAnalyticsAttendance_Down()
        {
            Sql( @"ALTER PROCEDURE [dbo].[spCrm_FamilyAnalyticsAttendance]

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
	DECLARE @TimesAttendedAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_TIMES_ATTENDED_IN_DURATION);
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
" );
        }

        #endregion

        #region JMH: Add Duplicate Communication Recipient Handling - up. Plugin Migration #250

        /// <summary>
        /// JMH: Add Duplicate Communication Recipient Handling - up.
        /// </summary>
        private void AddDuplicateCommunicationRecipientHandlingUp()
        {
            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            Sql( HotFixMigrationResource._250_AddDuplicateCommunicationRecipientHandling_spCommunicationRecipientDetails );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
        }

        #endregion

        #region NA: Updates the RockLogger MaxFileSize and NumberOfLogFiles from 20 to 5 - up. Plugin Migration #251

        /// <summary>
        /// NA: Updates the RockLogger MaxFileSize and NumberOfLogFiles from 20 to 5 - up
        /// </summary>
        private void ReduceDefaultSizeRockLoggerMaxFileSizeAndNumberOfLogFilesUp()
        {
            Sql( @"UPDATE [Attribute]
    SET [DefaultValue] = REPLACE(
            REPLACE([DefaultValue], '""MaxFileSize"":20,', '""MaxFileSize"":5,'),
            '""NumberOfLogFiles"":20,', '""NumberOfLogFiles"":5,'
            )
    ,IsDefaultPersistedValueDirty = 1
    WHERE [Key] = 'core_LoggingConfig';" );
        }

        /// <summary>
        /// Update the Apple Device List to include new devices.
        /// </summary>
        private void UpdateAppleDeviceListUp()
        {
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone17,5", "iPhone 16e", "099C90E2-0771-413D-8762-B473EE6DFDB9", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad15,8", "iPad 11th Gen (WiFi+Cellular)", "27D7717D-9483-4EFB-BBF9-1CD26D4C5F68", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad15,7", "iPad 11th Gen (WiFi)", "A55A657E-CA61-415D-8BA6-27C48A51A1D7", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad15,6", "iPad Air 13-inch 7th Gen (WiFi+Cellular)", "7B731688-93B7-4DE0-AF86-A0621FDC586B", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad15,5", "iPad Air 13-inch 7th Gen (WiFi)", "E2940051-B693-4B20-B230-0AAB0F2CD8D0", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad15,4", "iPad Air 11-inch 7th Gen (WiFi+Cellular)", "2575B3EB-C8CD-448B-80F3-15C2BDD11069", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad15,3", "iPad Air 11-inch 7th Gen (WiFi)", "E4EA4216-12E3-49DF-AFB4-F6B2A26E311E", true );

            Sql( @"
                DECLARE @AppleDeviceDefinedTypeId INT = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = 'DAE31F78-7AB9-4ACE-9EE1-C1E6A734562C')

                UPDATE [PersonalDevice] SET [Model] = dv.[Description]
                FROM [PersonalDevice] pd
                JOIN [DefinedValue] dv ON pd.[Model] = dv.[Value]
                WHERE pd.[Manufacturer] = 'Apple'
                  AND pd.[Model] like '%,%'
                  AND dv.DefinedTypeId = @AppleDeviceDefinedTypeId" );
        }

        #endregion

        #region KH: Update the Default Value for the Status Template Attribute. Plugin Migration #252

        private void UpdateStatusTemplateAttributeUp()
        {
            var targetAttributeColumn = RockMigrationHelper.NormalizeColumnCRLF( "DefaultValue" );
            var targetAttributeValueColumn = RockMigrationHelper.NormalizeColumnCRLF( "Value" );

            string oldValue = @"<span class='badge badge-danger badge-circle js-legend-badge'>{{ IdleTooltip }}</span>";

            string newValue = @"<span class='badge badge-danger badge-circle js-legend-badge' data-toggle='tooltip' data-html='true' title='{{IdleTooltipList}}'>{{ IdleTooltip }}</span>";

            oldValue = oldValue.Replace( "'", "''" );
            newValue = newValue.Replace( "'", "''" );

            Sql( $@"
DECLARE @BlockEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65')
DECLARE @BlockTypeId INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '23438CBC-105B-4ADB-8B9A-D5DDDCDD7643')

UPDATE [Attribute]
SET [DefaultValue] = REPLACE({targetAttributeColumn}, '{oldValue}', '{newValue}')
, [DefaultPersistedTextValue] = NULL
, [DefaultPersistedHtmlValue] = NULL
, [DefaultPersistedCondensedTextValue] = NULL
, [DefaultPersistedCondensedHtmlValue] = NULL
, [IsDefaultPersistedValueDirty] = 1
WHERE [EntityTypeId] = @BlockEntityTypeId
    AND [EntityTypeQualifierColumn] = 'BlockTypeId'
    AND [EntityTypeQualifierValue] = @BlockTypeId
    AND [Key] = 'StatusTemplate';

UPDATE [AttributeValue]
SET [Value] = REPLACE({targetAttributeValueColumn}, '{oldValue}', '{newValue}')
, [PersistedTextValue] = NULL
, [PersistedHtmlValue] = NULL
, [PersistedCondensedTextValue] = NULL
, [PersistedCondensedHtmlValue] = NULL
, [IsPersistedValueDirty] = 1
WHERE [AttributeId] IN (
    SELECT [Id] FROM [Attribute]
    WHERE [EntityTypeId] = @BlockEntityTypeId
        AND [EntityTypeQualifierColumn] = 'BlockTypeId'
        AND [EntityTypeQualifierValue] = @BlockTypeId
        AND [Key] = 'StatusTemplate'
)
AND {targetAttributeValueColumn} LIKE '%{oldValue}%';"
            );
        }

        #endregion
    }
}
