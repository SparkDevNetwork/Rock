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
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsDateTime], [IsPersistedValueDirty] )
	SELECT [PersonId], [AttributeId], [FirstContributionDate], [IsSystem], [Guid], [CreateDate], [FirstContributionDate], 1  FROM 
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
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsDateTime], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [FirstContributionDate], [IsSystem], [Guid], [CreateDate], [FirstContributionDate], 1 FROM 
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
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsDateTime], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [LastContributionDate], [IsSystem], [Guid], [CreateDate], [LastContributionDate], 1 FROM 
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
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsDateTime], [IsPersistedValueDirty] )
	SELECT [PersonId], [AttributeId], [LastContributionDate], [IsSystem], [Guid], [CreateDate], [LastContributionDate], 1 FROM 
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
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsNumeric], [IsPersistedValueDirty] )
	SELECT [PersonId], [AttributeId], [GiftCountDurationShort], [IsSystem], [Guid], [CreateDate], [GiftCountDurationShort], 1
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
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsNumeric], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [GiftCountDurationShort], [IsSystem], [Guid], [CreateDate], [GiftCountDurationShort], 1
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
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsNumeric], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [GiftCountDurationLong], [IsSystem], [Guid], [CreateDate], [GiftCountDurationLong], 1
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
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsNumeric], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [GiftCountDurationLong], [IsSystem], [Guid], [CreateDate], [GiftCountDurationLong], 1
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
	
END