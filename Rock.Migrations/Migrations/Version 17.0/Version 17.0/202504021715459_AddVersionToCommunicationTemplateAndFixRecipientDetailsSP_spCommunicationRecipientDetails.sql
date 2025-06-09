/*
<doc>
	<summary>
        This procedure takes a communication list (group) or communication and returns all of the
		recipients along with the data needed to determine if the individual can recieve an email,
		sms or push notification.
	</summary>

	<returns>
		All of the fields needed to drive the communication wizard.
	</returns>
	<param name='ListId' datatype='int'>The group id of the communication list or the communication id.</param>
	<param name='ListType' datatype='int'>1 = Communication List Id, 2 = Communication Id</param>
	<param name='MatchType' datatype='int'>1 = OR compare or 2 = AND compare</param>
	<param name='PersonalizationSegmentList' datatype='varchar(max)'>A comma separated string of personalization segment ids.</param>
	<remarks>	
	</remarks>
	<code>
        EXEC [dbo].[spCommunicationRecipientDetails] 1097001, 1, 1, '11,12'

		Sample Segments 3 = clients, 5= females, 2 = staff
	</code>
</doc>
*/
ALTER PROCEDURE [dbo].[spCommunicationRecipientDetails]
	@ListId int			-- this can be either a communication list or communication
	, @ListType int		-- 1 = Communication List, 2 = Communication
	, @MatchType int	-- 1 = OR, 2 = AND
	, @PersonalizationSegmentList nvarchar(max)   
	WITH RECOMPILE

AS

BEGIN
	DECLARE @StartTime DATETIME2, @RecipientLoadTime DATETIME2, @RecipientFilterTime DATETIME2, @ReturnResultsTime DATETIME2;
	
	SET @StartTime = SYSDATETIME();

	/* Get defined value ids for filtering */
    DECLARE @MobilePersonDeviceTypeValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '5A8F264F-3BE6-4F15-912A-3CE93A98E8F6')
	DECLARE @MobilePhoneTypeValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '407E7E45-7B2E-4FCD-9605-ECB1339F2453')


	/* Define temp tables for filtering */
	CREATE TABLE #RecipientList (
		[PersonId] INT PRIMARY KEY,
		[GroupCommunicationPreference] INT NULL
	);

	CREATE TABLE #PersonSegmentList (
		[PersonId] INT,
		[PersonalizationSegmentId] INT,
		PRIMARY KEY ([PersonId], [PersonalizationSegmentId])
	);

	/* Load the recipient list and filter by segment if needed */
	IF @ListType = 1 
		BEGIN
			/* Communication List Logic - This uses segments for filtering */
			INSERT INTO #RecipientList ([PersonId], [GroupCommunicationPreference])
				SELECT DISTINCT [PersonId], gm.[CommunicationPreference] FROM [GroupMember] gm 
					INNER JOIN [Person] p ON p.[Id] = gm.[PersonId]
				WHERE gm.[GroupId] = @ListId
					AND p.[IsDeceased] = 0
					AND gm.[GroupMemberStatus] = 1

			SET @RecipientLoadTime = SYSDATETIME();

			;WITH [PersonalizationSegments] AS (
				SELECT value AS [PersonalizationSegmentId] FROM STRING_SPLIT(@PersonalizationSegmentList, ',')  -- Convert to table
			)

			/* Load the person segment table */
			INSERT INTO #PersonSegmentList
			SELECT DISTINCT p.[Id], pap.[PersonalizationEntityId] FROM [Person] p
				INNER JOIN [PersonAlias] pa ON pa.[PersonId] = p.[Id]
					INNER JOIN [PersonAliasPersonalization] pap ON pap.[PersonAliasId] = pa.[Id] 
						AND pap.[PersonalizationType] = 0
					INNER JOIN [PersonalizationSegments] ps ON ps.[PersonalizationSegmentId] = pap.[PersonalizationEntityId]
					WHERE
						p.[Id] IN (SELECT [PersonId] FROM #RecipientList)

			/* Get Count of Provided Segments */
			DECLARE @SegmentCount int = (SELECT COUNT(*) FROM STRING_SPLIT(@PersonalizationSegmentList, ',') Split WHERE Split.[value] IS NOT NULL AND Split.[value] <> '')

            IF @SegmentCount > 0
                BEGIN
			        /* Filter out based on segments using AND/OR logic */
			        IF @MatchType = 1
				        BEGIN
					        DELETE FROM #RecipientList
					        WHERE [PersonId] NOT IN (SELECT [PersonId] FROM #PersonSegmentList)
				        END
			        ELSE 
				        BEGIN
					        DELETE FROM #RecipientList
					        WHERE [PersonId] NOT IN (SELECT [PersonId] FROM #PersonSegmentList GROUP BY [PersonId] HAVING COUNT(DISTINCT [PersonalizationSegmentId]) = @SegmentCount)
				        END
                END

			SET @RecipientFilterTime = SYSDATETIME();
		END
	ELSE
		BEGIN
			INSERT INTO #RecipientList ([PersonId], [GroupCommunicationPreference])
				SELECT pa.[PersonId], NULL FROM [CommunicationRecipient] cr 
					INNER JOIN [PersonAlias] pa ON pa.[Id] = cr.[PersonAliasId]
					INNER JOIN [Person] p ON p.[Id] = pa.[PersonId]
				WHERE cr.[CommunicationId] = @ListId
					AND p.[IsDeceased] = 0

			SET @RecipientLoadTime = SYSDATETIME();
			SET @RecipientFilterTime = SYSDATETIME();
		END

	/* Return the final result set with required data */
	SELECT 
		p.[Id]
		, CASE WHEN p.[NickName] IS NOT NULL THEN p.[NickName] ELSE p.[FirstName] END AS [NickName]
		, [LastName]
		, p.[PhotoId]
		, CAST(CASE WHEN (p.[Email] IS NULL OR p.[Email] = '' OR p.[IsEmailActive] = 0 OR p.[EmailPreference] = 2) THEN 0 ELSE 1 END AS BIT) AS [IsEmailEnabled]
		, p.[Email]
		, p.[IsEmailActive]
		, p.[EmailNote] -- possibly not needed
		, p.[ConnectionStatusValueId]
		, p.[CommunicationPreference]
		, rl.[GroupCommunicationPreference]
		, CAST(CASE WHEN (pn.[NumberFormatted] IS NULL OR pn.[IsMessagingEnabled] = 0 OR pn.[IsMessagingOptedOut] = 1) THEN 0 ELSE 1 END AS BIT) AS [IsSmsEnabled]
		, pn.[NumberFormatted] AS [MobilePhoneNumber]
		, CAST(CASE WHEN pn.[IsMessagingEnabled] IS NULL THEN 0 ELSE pn.[IsMessagingEnabled] END AS BIT) AS [IsMessagingEnabled]
		, CAST(CASE WHEN pn.[IsMessagingOptedOut] IS NULL THEN 0 ELSE pn.[IsMessagingOptedOut] END AS BIT) AS [IsMessagingOptedOut]
		, CAST(CASE WHEN (pd.[DeviceRegistrationId] IS NULL OR pd.[NotificationsEnabled] = 0) THEN 0 ELSE 1 END AS BIT) AS [IsPushEnabled]
		, pd.[DeviceRegistrationId]
		, CAST(CASE WHEN pd.[NotificationsEnabled] IS NULL THEN 0 ELSE pd.[NotificationsEnabled] END AS BIT) AS [NotificationsEnabled]
		, CAST(CASE WHEN (p.[Email] IS NULL OR p.[Email] = '' OR p.[IsEmailActive] = 0 OR p.[EmailPreference] = 2 OR p.[EmailPreference] = 1) THEN 0 ELSE 1 END AS BIT) AS [IsBulkEmailEnabled]
        , p.[EmailPreference] -- needed for disabled messaging
        , p.[RecordTypeValueId] -- needed for nameless check and full name
        , p.[SuffixValueId] -- needed for full name
        , p.[PrimaryAliasGuid]
        , p.[PrimaryAliasId]
        , p.[AgeClassification] -- needed for photo
        , p.[Age] -- needed for photo
        , p.[Gender] -- needed for photo
        
	FROM [Person] p
		INNER JOIN #RecipientList rl ON rl.[PersonId] = p.[Id]
		OUTER APPLY ( -- outer apply to ensure we only get one
			SELECT TOP 1 pn.[NumberFormatted], pn.[IsMessagingEnabled], pn.[IsMessagingOptedOut] 
			FROM [PhoneNumber] pn 
			WHERE pn.[PersonId] = p.[Id] AND pn.[NumberTypeValueId] = @MobilePhoneTypeValueId
			ORDER BY pn.[IsMessagingEnabled], pn.[IsMessagingOptedOut]
		) pn
		OUTER APPLY ( -- outer apply to ensure we only get one
			SELECT TOP 1 pd.[DeviceRegistrationId], pd.[NotificationsEnabled]
			FROM [PersonalDevice] pd 
			WHERE pd.[PersonAliasId] IN (SELECT [Id] FROM [PersonAlias] pa WHERE pa.[PersonId] = p.[Id])
			AND pd.[IsActive] = 1 AND pd.[PersonalDeviceTypeValueId] = @MobilePersonDeviceTypeValueId 
			ORDER BY [NotificationsEnabled]
		) pd
	
	WHERE p.[PrimaryAliasGuid] IS NOT NULL AND p.[PrimaryAliasId] IS NOT NULL

	SET @ReturnResultsTime = SYSDATETIME();

	PRINT 'Recipient Load: ' + CONVERT(NVARCHAR, DATEDIFF(MILLISECOND, @StartTime, @RecipientLoadTime));
	PRINT 'Recipient Filter: ' + CONVERT(NVARCHAR, DATEDIFF(MILLISECOND, @RecipientLoadTime, @RecipientFilterTime));
	PRINT 'Recipient List: ' + CONVERT(NVARCHAR, DATEDIFF(MILLISECOND, @RecipientFilterTime, @ReturnResultsTime));
END