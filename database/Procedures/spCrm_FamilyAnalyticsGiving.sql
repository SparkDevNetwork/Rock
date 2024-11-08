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
	DECLARE @Now DATETIME = dbo.RockGetDate()

	-- first gift (people w/Giving Group)
	DECLARE @FirstGaveAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_FIRST_GAVE);
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
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
		SELECT [PersonId], [FirstContributionDate], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #firstGiftWithGroup
		FROM 
			(SELECT 
	    		[PersonId]
	    		, (SELECT MIN(ft.TransactionDateTime)
	    					FROM [FinancialTransaction] ft
	    						INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
	    						INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
	    						INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
	    						INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
	    					WHERE 
	    						gp.[GivingGroupId] = i.[GivingGroupId]
	    						AND ft.TransactionTypeValueId = @ContributionTypeId) AS [FirstContributionDate]
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @FirstGaveAttributeId
	    WHERE g.[FirstContributionDate] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[FirstContributionDate],
		av.[ValueAsDateTime] = f.[FirstContributionDate],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #firstGiftWithGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[FirstContributionDate]

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
		, @FirstGaveAttributeId
		, f.[FirstContributionDate]
		, 0
		, newid()
		, @Now
		, f.[FirstContributionDate]
		, 1
	FROM #firstGiftWithGroup f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #firstGiftWithGroup

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
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [FirstContributionDate], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #firstGiftWithoutGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
	    		, (SELECT MIN(ft.TransactionDateTime)
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[Id] = i.[PersonId] -- match by person id
							AND ft.TransactionTypeValueId = @ContributionTypeId) AS [FirstContributionDate]
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @FirstGaveAttributeId
	    WHERE g.[FirstContributionDate] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[FirstContributionDate],
		av.[ValueAsDateTime] = f.[FirstContributionDate],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #firstGiftWithoutGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[FirstContributionDate]

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
		, @FirstGaveAttributeId
		, f.[FirstContributionDate]
		, 0
		, newid()
		, @Now
		, f.[FirstContributionDate]
		, 1
	FROM #firstGiftWithoutGroup f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #firstGiftWithoutGroup
	
	-- last gift (people w/Giving Group)
	DECLARE @LastGaveAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_LAST_GAVE);
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
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [LastContributionDate], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #lastGiftWithGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
	    		, (SELECT MAX(ft.TransactionDateTime)
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[GivingGroupId] = i.[GivingGroupId]
							AND ft.TransactionTypeValueId = @ContributionTypeId) AS [LastContributionDate]
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @LastGaveAttributeId
	    WHERE g.[LastContributionDate] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[LastContributionDate],
		av.[ValueAsDateTime] = f.[LastContributionDate],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #lastGiftWithGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[LastContributionDate]

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
		, @LastGaveAttributeId
		, f.[LastContributionDate]
		, 0
		, newid()
		, @Now
		, f.[LastContributionDate]
		, 1
	FROM #lastGiftWithGroup f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #lastGiftWithGroup

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
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [LastContributionDate], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #lastGiftWithoutGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
	    		, (SELECT MAX(ft.TransactionDateTime)
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[Id] = i.[PersonId]
							AND ft.TransactionTypeValueId = @ContributionTypeId) AS [LastContributionDate]
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @LastGaveAttributeId
	    WHERE g.[LastContributionDate] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[LastContributionDate],
		av.[ValueAsDateTime] = f.[LastContributionDate],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #lastGiftWithoutGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[LastContributionDate]

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
		, @LastGaveAttributeId
		, f.[LastContributionDate]
		, 0
		, newid()
		, @Now
		, f.[LastContributionDate]
		, 1
	FROM #lastGiftWithoutGroup f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #lastGiftWithoutGroup

	-- number of gifts short duration (people w/Giving Group)
	DECLARE @GiftCountShortAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_GIFT_COUNT_SHORT);
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
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [GiftCountDurationShort], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #giftCountShortWithGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
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
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @GiftCountShortAttributeId
	    WHERE g.[GiftCountDurationShort] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[GiftCountDurationShort],
		av.[ValueAsNumeric] = f.[GiftCountDurationShort],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #giftCountShortWithGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[GiftCountDurationShort]

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
		, @GiftCountShortAttributeId
		, f.[GiftCountDurationShort]
		, 0
		, newid()
		, @Now
		, f.[GiftCountDurationShort]
		, 1
	FROM #giftCountShortWithGroup f
	WHERE f.[AttributeValueId] IS NULL

    -- Remove the temp table.
	DROP TABLE #giftCountShortWithGroup

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
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [GiftCountDurationShort], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #giftCountShortWithoutGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
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
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @GiftCountShortAttributeId
	    WHERE g.[GiftCountDurationShort] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[GiftCountDurationShort],
		av.[ValueAsNumeric] = f.[GiftCountDurationShort],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #giftCountShortWithoutGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[GiftCountDurationShort]

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
		, @GiftCountShortAttributeId
		, f.[GiftCountDurationShort]
		, 0
		, newid()
		, @Now
		, f.[GiftCountDurationShort]
		, 1
	FROM #giftCountShortWithoutGroup f
	WHERE f.[AttributeValueId] IS NULL

    -- Remove the temp table.
	DROP TABLE #giftCountShortWithoutGroup

	-- number of gifts long duration (people w/Giving Group)
	DECLARE @GiftCountLongAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_GIFT_COUNT_LONG);
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
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [GiftCountDurationLong], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #giftCountLongWithGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
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
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @GiftCountLongAttributeId
	    WHERE g.[GiftCountDurationLong] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[GiftCountDurationLong],
		av.[ValueAsNumeric] = f.[GiftCountDurationLong],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #giftCountLongWithGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[GiftCountDurationLong]

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
		, @GiftCountLongAttributeId
		, f.[GiftCountDurationLong]
		, 0
		, newid()
		, @Now
		, f.[GiftCountDurationLong]
		, 1
	FROM #giftCountLongWithGroup f
	WHERE f.[AttributeValueId] IS NULL

    -- Remove the temp table.
	DROP TABLE #giftCountLongWithGroup
	
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
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [GiftCountDurationLong], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #giftCountLongWithoutGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
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
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @GiftCountLongAttributeId
	    WHERE g.[GiftCountDurationLong] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[GiftCountDurationLong],
		av.[ValueAsNumeric] = f.[GiftCountDurationLong],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #giftCountLongWithoutGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[GiftCountDurationLong]

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
		, @GiftCountLongAttributeId
		, f.[GiftCountDurationLong]
		, 0
		, newid()
		, @Now
		, f.[GiftCountDurationLong]
		, 1
	FROM #giftCountLongWithoutGroup f
	WHERE f.[AttributeValueId] IS NULL

    -- Remove the temp table.
	DROP TABLE #giftCountLongWithoutGroup
END