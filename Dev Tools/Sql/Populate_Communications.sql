SET NOCOUNT OFF

----------------------------------------------
-- README
-- Adds 250,000 sample communications
----------------------------------------------
DECLARE @recipientPersonAliasId INT
	,@senderPersonAliasId INT
	,@communicationCounter INT = 0
	,@maxCommunicationCount INT = 250000
	,@maxPersonAliasIdForCommunications INT = (
		SELECT max(Id)
		FROM (
			SELECT TOP 4000 Id
			FROM PersonAlias
			ORDER BY Id
			) x
		)
	,/* limit to first 4000 persons in the database */
	@communicationDateTime DATETIME
	,@futureSendDateTime DATETIME
	,@communicationSubject NVARCHAR(max)
	,@communicationId INT
	,@mediumId INT
	,@yearsBack INT = 4
	,@isbulk BIT
	,@communicationStatus INT = 0
DECLARE @daysBack INT = @yearsBack * 366

BEGIN
	--begin transaction
	/*
 delete [CommunicationRecipient]
 delete [Communication]
*/
	SET @communicationDateTime = DATEADD(DAY, - @daysBack, SYSDATETIME())

	WHILE @communicationCounter < @maxCommunicationCount
	BEGIN
		SET @communicationSubject = 'Random Subject ' + convert(NVARCHAR(max), rand());
		SET @recipientPersonAliasId = (
				SELECT TOP 1 Id
				FROM PersonAlias
				WHERE Id <= rand() * @maxPersonAliasIdForCommunications
				ORDER BY Id DESC
				);

		WHILE @recipientPersonAliasId IS NULL
		BEGIN
			-- Try again just in case we didn't get anybody
			SET @recipientPersonAliasId = (
					SELECT TOP 1 Id
					FROM PersonAlias
					WHERE Id <= rand() * @maxPersonAliasIdForCommunications
					ORDER BY Id DESC
					);
		END

		SET @senderPersonAliasId = (
				SELECT TOP 1 Id
				FROM PersonAlias
				WHERE Id <= rand() * @maxPersonAliasIdForCommunications
				ORDER BY Id DESC
				);

		WHILE @senderPersonAliasId IS NULL
		BEGIN
			-- Try again just in case we didn't get anybody
			SET @senderPersonAliasId = (
					SELECT TOP 1 Id
					FROM PersonAlias
					WHERE Id <= rand() * @maxPersonAliasIdForCommunications
					ORDER BY Id DESC
					);
		END

		SET @futureSendDateTime = DATEADD(DAY, round(50 * rand(), 0) - 1, @communicationDateTime)

		IF (@futureSendDateTime = @communicationDateTime)
		BEGIN
			SET @futureSendDateTime = NULL
		END

		SET @communicationStatus = ROUND(5 * RAND(), 0)

		SELECT TOP 1 @mediumId = id
		FROM EntityType
		WHERE Name LIKE 'Rock.Communication.Medium%'
		ORDER BY newid()

		-- generate a bulk about 1/20th of the time
		SELECT @isbulk = CASE CAST(ROUND(RAND() * 20, 0) AS BIT)
				WHEN 1
					THEN 0
				ELSE 1
				END

		INSERT INTO [dbo].[Communication] (
			[Subject]
			,[FutureSendDateTime]
			,[Status]
			,[ReviewedDateTime]
			,[ReviewerNote]
			,[AdditionalMergeFieldsJson]
			,[Guid]
			,[CreatedDateTime]
			,[ModifiedDateTime]
			,[CreatedByPersonAliasId]
			,[ModifiedByPersonAliasId]
			,[ForeignId]
			,[IsBulkCommunication]
			,[SenderPersonAliasId]
			,[ReviewerPersonAliasId]
			,[MediumDataJson]
			)
		VALUES (
			@communicationSubject
			,@futureSendDateTime
			,@communicationStatus
			,DATEADD(DAY, round(50 * rand(), 0), @communicationDateTime)
			,'some note'
			,''
			,newid()
			,@communicationDateTime
			,@communicationDateTime
			,NULL
			,NULL
			,NULL
			,@isbulk
			,@senderPersonAliasId
			,NULL
			,NULL
			)

		SET @communicationId = SCOPE_IDENTITY()

		IF (@isbulk = 0)
		BEGIN
			INSERT INTO [dbo].[CommunicationRecipient] (
				[CommunicationId]
				,[Status]
				,[StatusNote]
				,[AdditionalMergeValuesJson]
				,[Guid]
				,[CreatedDateTime]
				,[ModifiedDateTime]
				,[CreatedByPersonAliasId]
				,[ModifiedByPersonAliasId]
				,[OpenedDateTime]
				,[OpenedClient]
				,[ForeignId]
				,[TransportEntityTypeName]
				,[UniqueMessageId]
				,[ResponseCode]
				,[PersonAliasId]
				)
			VALUES (
				@communicationId
				,ROUND(5 * RAND(), 0)
				,NULL
				,NULL
				,newid()
				,@communicationDateTime
				,@communicationDateTime
				,NULL
				,NULL
				,DATEADD(day, 5, @communicationDateTime)
				,NULL
				,NULL
				,NULL
				,NULL
				,NULL
				,@recipientPersonAliasId
				)
		END
		ELSE
		BEGIN
			INSERT INTO [dbo].[CommunicationRecipient] (
				[CommunicationId]
				,[Status]
				,[StatusNote]
				,[AdditionalMergeValuesJson]
				,[Guid]
				,[CreatedDateTime]
				,[ModifiedDateTime]
				,[CreatedByPersonAliasId]
				,[ModifiedByPersonAliasId]
				,[OpenedDateTime]
				,[OpenedClient]
				,[ForeignId]
				,[TransportEntityTypeName]
				,[UniqueMessageId]
				,[ResponseCode]
				,[PersonAliasId]
				)
			SELECT TOP 40 @communicationId
				,ROUND(5 * RAND(), 0)
				,NULL
				,NULL
				,newid()
				,@communicationDateTime
				,@communicationDateTime
				,NULL
				,NULL
				,DATEADD(day, 5, @communicationDateTime)
				,NULL
				,NULL
				,NULL
				,NULL
				,NULL
				,Id
			FROM PersonAlias
			ORDER BY newid()
		END

		DECLARE @messageKey NVARCHAR(11) = (
				SELECT TOP 1 CONCAT (
						isnull(CountryCode, 1)
						,Number
						)
				FROM PhoneNumber
				WHERE IsMessagingEnabled = 1
				ORDER BY newid()
				)

		INSERT INTO [dbo].[CommunicationResponse] (
			[MessageKey]
			,[FromPersonAliasId]
			,[ToPersonAliasId]
			,[IsRead]
			,[RelatedSmsFromDefinedValueId]
			,[RelatedTransportEntityTypeId]
			,[RelatedMediumEntityTypeId]
			,[Response]
			,[CreatedDateTime]
			,[Guid]
			)
		VALUES (
			@messageKey
			,null -- @recipientPersonAliasId
			,@senderPersonAliasId
			,0
			,780
			,163
			,38
			,CONCAT (
				'Some Message'
				,rand()
				)
			,DATEADD(DAY, - round(50 * rand(), 0), getdate())
			,NEWID()
			)

		SET @communicationCounter += 1;
		SET @communicationDateTime = DATEADD(ss, (86000 * @daysBack / @maxCommunicationCount), @communicationDateTime);
	END
			--commit transaction
END;